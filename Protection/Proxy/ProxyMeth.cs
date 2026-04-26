using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MindLated.Protection.Proxy
{
    public static class ProxyMeth
    {
        private const int MaximumSwitchCaseCount = 4;
        private const int MinimumDistinctTargets = 2;
        private static readonly Random Rand = new();

        private sealed class DispatcherInfo
        {
            public DispatcherInfo(MethodDef dispatcher, IReadOnlyDictionary<string, int> selectors)
            {
                Dispatcher = dispatcher;
                Selectors = selectors;
            }

            public MethodDef Dispatcher { get; }

            public IReadOnlyDictionary<string, int> Selectors { get; }
        }

        private static bool CanProxy(IMethod target)
        {
            if (target is not IMethodDefOrRef method)
                return false;

            var signature = method.MethodSig;
            if (signature == null)
                return false;

            if (method is MethodSpec)
                return false;

            if (method.Name.StartsWith("ProxyMeth_", StringComparison.Ordinal) ||
                method.Name.StartsWith("get_", StringComparison.Ordinal) ||
                method.Name.StartsWith("set_", StringComparison.Ordinal) ||
                method.Name == ".ctor" ||
                method.Name == ".cctor")
                return false;

            if (signature.HasThis || signature.GenParamCount > 0)
                return false;

            return true;
        }

        private static string GetSignatureKey(IMethodDefOrRef method)
        {
            var signature = method.MethodSig;
            var parameters = string.Join("|", signature.Params.Select(param => param.FullName));
            return $"{signature.RetType.FullName}::{parameters}";
        }

        private static string GetTargetKey(IMethodDefOrRef method)
            => $"{method.DeclaringType.FullName}::{method.Name}::{GetSignatureKey(method)}";

        private static Dictionary<string, List<IMethodDefOrRef>> BuildSignaturePools(ModuleDef module)
        {
            var pools = new Dictionary<string, List<IMethodDefOrRef>>(StringComparer.Ordinal);
            var seenTargets = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

            foreach (var type in module.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;

                foreach (var method in type.Methods)
                {
                    if (!method.HasBody || !method.Body.HasInstructions) continue;

                    foreach (var instruction in method.Body.Instructions)
                    {
                        if (instruction.OpCode != OpCodes.Call) continue;
                        if (instruction.Operand is not IMethodDefOrRef target) continue;
                        if (!CanProxy(target)) continue;

                        var signatureKey = GetSignatureKey(target);
                        var targetKey = GetTargetKey(target);

                        if (!pools.TryGetValue(signatureKey, out var candidates))
                        {
                            candidates = new List<IMethodDefOrRef>();
                            pools[signatureKey] = candidates;
                            seenTargets[signatureKey] = new HashSet<string>(StringComparer.Ordinal);
                        }

                        if (seenTargets[signatureKey].Add(targetKey))
                            candidates.Add(target);
                    }
                }
            }

            return pools;
        }

        private static List<IMethodDefOrRef> SelectTargetsForDispatcher(List<IMethodDefOrRef> pool, IMethodDefOrRef original)
        {
            var originalKey = GetTargetKey(original);
            var selected = new List<IMethodDefOrRef> { original };

            foreach (var candidate in pool)
            {
                if (selected.Count >= MaximumSwitchCaseCount)
                    break;

                if (GetTargetKey(candidate) == originalKey)
                    continue;

                selected.Add(candidate);
            }

            if (selected.Count < MinimumDistinctTargets)
                return new List<IMethodDefOrRef>();

            var originalIndex = Rand.Next(0, selected.Count);
            (selected[0], selected[originalIndex]) = (selected[originalIndex], selected[0]);
            return selected;
        }

        private static MethodDef CreateHelper(ModuleDef module, IMethodDefOrRef original)
        {
            var signature = original.MethodSig;
            var methodImplFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
            var methodFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig;
            var helper = new MethodDefUser($"ProxyMeth_Helper_{Rand.Next(0, int.MaxValue):X8}",
                MethodSig.CreateStatic(signature.RetType, signature.Params.ToArray()),
                methodImplFlags, methodFlags)
            {
                Body = new CilBody()
            };

            for (var index = 0; index < signature.Params.Count; index++)
                helper.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, helper.Parameters[index]));

            helper.Body.Instructions.Add(Instruction.Create(OpCodes.Call, original));
            helper.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            helper.Body.UpdateInstructionOffsets();
            helper.Body.OptimizeMacros();
            module.GlobalType.Methods.Add(helper);
            return helper;
        }

        private static DispatcherInfo CreateDispatcher(ModuleDef module, List<IMethodDefOrRef> targets,
            Dictionary<string, MethodDef> helpers)
        {
            var signature = targets[0].MethodSig;
            var methodImplFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
            var methodFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig;
            var dispatcher = new MethodDefUser($"ProxyMeth_Dispatch_{Rand.Next(0, int.MaxValue):X8}",
                MethodSig.CreateStatic(signature.RetType,
                    signature.Params.Concat(new[] { module.CorLibTypes.Int32 }).ToArray()),
                methodImplFlags, methodFlags)
            {
                Body = new CilBody()
            };

            var selectorParameter = dispatcher.Parameters[signature.Params.Count];
            var defaultTarget = Instruction.Create(OpCodes.Nop);
            var caseTargets = new Instruction[targets.Count];
            var selectors = new Dictionary<string, int>(StringComparer.Ordinal);

            for (var index = 0; index < targets.Count; index++)
            {
                caseTargets[index] = Instruction.Create(OpCodes.Nop);
                var targetKey = GetTargetKey(targets[index]);
                if (!selectors.ContainsKey(targetKey))
                    selectors[targetKey] = index;
            }

            dispatcher.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, selectorParameter));
            dispatcher.Body.Instructions.Add(Instruction.Create(OpCodes.Switch, caseTargets));
            dispatcher.Body.Instructions.Add(Instruction.Create(OpCodes.Br, defaultTarget));

            for (var index = 0; index < targets.Count; index++)
            {
                dispatcher.Body.Instructions.Add(caseTargets[index]);
                var helper = helpers[GetTargetKey(targets[index])];
                for (var paramIndex = 0; paramIndex < signature.Params.Count; paramIndex++)
                    dispatcher.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, dispatcher.Parameters[paramIndex]));

                dispatcher.Body.Instructions.Add(Instruction.Create(OpCodes.Call, helper));
                dispatcher.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            }

            dispatcher.Body.Instructions.Add(defaultTarget);
            for (var paramIndex = 0; paramIndex < signature.Params.Count; paramIndex++)
                dispatcher.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, dispatcher.Parameters[paramIndex]));

            dispatcher.Body.Instructions.Add(Instruction.Create(OpCodes.Call, helpers[GetTargetKey(targets[0])]));
            dispatcher.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            dispatcher.Body.UpdateInstructionOffsets();
            dispatcher.Body.OptimizeMacros();
            module.GlobalType.Methods.Add(dispatcher);
            return new DispatcherInfo(dispatcher, selectors);
        }

        public static void Execute(ModuleDef module)
        {
            var signaturePools = BuildSignaturePools(module);
            var helpers = new Dictionary<string, MethodDef>(StringComparer.Ordinal);
            var dispatchers = new Dictionary<string, DispatcherInfo>(StringComparer.Ordinal);
            var singleTargetProxies = new Dictionary<string, MethodDef>(StringComparer.Ordinal);

            foreach (var type in module.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;
                foreach (var meth in type.Methods.ToArray())
                {
                    if (!meth.HasBody || !meth.Body.HasInstructions) continue;
                    var instr = meth.Body.Instructions;
                    for (var i = 0; i < instr.Count; i++)
                    {
                        if (instr[i].OpCode != OpCodes.Call) continue;
                        if (instr[i].Operand is not IMethodDefOrRef original) continue;
                        if (!CanProxy(original)) continue;

                        var signatureKey = GetSignatureKey(original);
                        if (!signaturePools.TryGetValue(signatureKey, out var pool) || pool.Count == 0)
                            continue;

                        var originalKey = GetTargetKey(original);
                        var selectedTargets = SelectTargetsForDispatcher(pool, original);
                        if (selectedTargets.Count < MinimumDistinctTargets)
                        {
                            if (!singleTargetProxies.TryGetValue(originalKey, out var directProxy))
                            {
                                directProxy = CreateHelper(module, original);
                                singleTargetProxies[originalKey] = directProxy;
                            }

                            instr[i].Operand = directProxy;
                            continue;
                        }

                        foreach (var target in selectedTargets)
                        {
                            var targetKey = GetTargetKey(target);
                            if (!helpers.ContainsKey(targetKey))
                                helpers[targetKey] = CreateHelper(module, target);
                        }

                        var dispatcherKey = string.Join(";;", selectedTargets.Select(GetTargetKey));
                        if (!dispatchers.TryGetValue(dispatcherKey, out var dispatcherInfo))
                        {
                            dispatcherInfo = CreateDispatcher(module, selectedTargets, helpers);
                            dispatchers[dispatcherKey] = dispatcherInfo;
                        }

                        var selector = dispatcherInfo.Selectors[originalKey];
                        instr.Insert(i, Instruction.CreateLdcI4(selector));
                        i++;
                        instr[i].Operand = dispatcherInfo.Dispatcher;
                    }
                }
            }
        }
    }
}