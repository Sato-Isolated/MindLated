using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Linq;

namespace MindLated.Protection.Other
{
    internal class Calli
    {
        internal readonly record struct CalliReport(int TransformedCalls, int SkippedCalls);

        public static CalliReport Execute(ModuleDef module)
        {
            var transformedCalls = 0;
            var skippedCalls = 0;

            foreach (var type in module.Types.ToArray())
            {
                foreach (var meth in type.Methods.ToArray())
                {
                    if (ShouldSkipMethod(meth)) continue;

                    for (var i = 0; i < meth.Body.Instructions.Count - 1; i++)
                    {
                        if (!TryGetSupportedTarget(meth.Body.Instructions[i], out var target))
                        {
                            skippedCalls++;
                            continue;
                        }

                        meth.Body.Instructions.Insert(i, Instruction.Create(OpCodes.Ldftn, target));
                        i++;
                        meth.Body.Instructions[i].OpCode = OpCodes.Calli;
                        meth.Body.Instructions[i].Operand = target.MethodSig;
                        transformedCalls++;
                    }
                }
            }

            return new CalliReport(transformedCalls, skippedCalls);
        }

        private static bool ShouldSkipMethod(MethodDef method)
        {
            if (!method.HasBody || !method.Body.HasInstructions)
                return true;

            if (method.FullName.Contains("My.", StringComparison.Ordinal) ||
                method.FullName.Contains(".My", StringComparison.Ordinal) ||
                method.FullName.Contains("Costura", StringComparison.Ordinal))
                return true;

            return method.IsConstructor || method.DeclaringType.IsGlobalModuleType;
        }

        private static bool TryGetSupportedTarget(Instruction instruction, out IMethodDefOrRef target)
        {
            target = null!;
            if (instruction.OpCode != OpCodes.Call)
                return false;

            if (instruction.Operand is not IMethodDefOrRef method)
                return false;

            if (method is MethodSpec)
                return false;

            if (method.Name.StartsWith("get_", StringComparison.Ordinal) ||
                method.Name.StartsWith("set_", StringComparison.Ordinal) ||
                method.Name == ".ctor" ||
                method.Name == ".cctor" ||
                method.FullName.Contains("ISupportInitialize", StringComparison.Ordinal) ||
                method.FullName.Contains("System.Object", StringComparison.Ordinal))
                return false;

            var signature = method.MethodSig;
            if (signature == null || signature.HasThis || signature.GenParamCount > 0)
                return false;

            if (HasUnsupportedType(signature.RetType) || signature.Params.Any(HasUnsupportedType))
                return false;

            target = method;
            return true;
        }

        private static bool HasUnsupportedType(TypeSig typeSig)
        {
            if (typeSig == null)
                return false;

            if (typeSig.FullName.Contains("!", StringComparison.Ordinal))
                return true;

            return typeSig.ElementType == ElementType.Ptr ||
                   typeSig.ElementType == ElementType.ByRef ||
                   typeSig.ElementType == ElementType.FnPtr ||
                   typeSig.ElementType == ElementType.Var ||
                   typeSig.ElementType == ElementType.MVar;
        }
    }
}