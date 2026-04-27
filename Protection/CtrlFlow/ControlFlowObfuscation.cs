using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MindLated.Protection.CtrlFlow
{
    internal class ControlFlowObfuscation
    {
        private enum StateEncodingMode
        {
            Xor,
            Add
        }

        private enum PayloadPatternKind
        {
            ArithmeticBranch,
            ContradictoryState,
            BoolConfusion,
            FakeSwitch,
            NestedPredicate
        }

        private enum PayloadConstantContext
        {
            NormalInt,
            BooleanLike,
            EnumLike
        }

        private enum CflowTransformationMode
        {
            Full,
            Safe,
            EHSafe,
            Fragile,
            Skip
        }

        private sealed class MethodCflowAnalysis
        {
            public required bool HasTraceAnalysis { get; init; }

            public required bool HasExceptionHandlers { get; init; }

            public required bool HasTransformableExceptionRegions { get; init; }

            public required bool HasNestedExceptionRegions { get; init; }

            public required bool HasSwitchInstructions { get; init; }

            public required bool HasBackwardBranches { get; init; }

            public required bool HasMultipleSources { get; init; }

            public required bool HasUnknownIncomingSources { get; init; }

            public required bool HasFragileInstructionSequences { get; init; }

            public required bool HasBoolEnumHeavyLiteralConsumers { get; init; }

            public required bool IsUiOrEventMethod { get; init; }

            public required bool IsEntryPoint { get; init; }

            public required bool RequiresEhSafeMode { get; init; }

            public required bool CanParseNormalizedBody { get; init; }

            public required bool SupportsSpillRewrite { get; init; }

            public required int InstructionCount { get; init; }

            public required int ExceptionRegionCount { get; init; }

            public required int TransformableExceptionRegionCount { get; init; }

            public required int UnsupportedExceptionRegionCount { get; init; }

            public required int LoweredSwitchCount { get; init; }

            public required int BlockCount { get; init; }

            public required int MaxObservedStack { get; init; }

            public required int MultiSourceOffsetCount { get; init; }

            public required int BoolEnumLiteralSiteCount { get; init; }

            public required IReadOnlyList<string> Reasons { get; init; }
        }

        private sealed class ExceptionRegionCompatibility
        {
            public required bool HasNestedExceptionRegions { get; init; }

            public required int RegionCount { get; init; }

            public required int TransformableRegionCount { get; init; }

            public required int UnsupportedRegionCount { get; init; }

            public required int MaxRegionBlockCount { get; init; }
        }

        private sealed class MethodTransformationDecision
        {
            public required MethodCflowAnalysis Analysis { get; init; }

            public required CflowTransformationMode Mode { get; init; }

            public required string Diagnostic { get; init; }
        }

        private sealed class StackTraceAnalysis
        {
            public StackTraceAnalysis(MethodDef meth)
            {
                RefCount = new Dictionary<uint, int>();
                BranchReferences = new Dictionary<uint, List<Instruction>>();
                BeforeStack = new Dictionary<uint, int>();
                AfterStack = new Dictionary<uint, int>();
                SwitchTargetOffsets = new HashSet<uint>();

                var body = meth.Body;
                var hasReturnValue = meth.MethodSig?.RetType.RemovePinnedAndModifiers().ElementType != ElementType.Void;

                body.UpdateInstructionOffsets();

                foreach (var handler in body.ExceptionHandlers)
                {
                    BeforeStack[handler.TryStart.Offset] = 0;
                    BeforeStack[handler.HandlerStart.Offset] = handler.HandlerType != ExceptionHandlerType.Finally ? 1 : 0;
                    if (handler.FilterStart is not null)
                        BeforeStack[handler.FilterStart.Offset] = 1;
                }

                var currentStack = 0;
                for (var index = 0; index < body.Instructions.Count; index++)
                {
                    var instruction = body.Instructions[index];

                    if (BeforeStack.TryGetValue(instruction.Offset, out var seededStack))
                        currentStack = seededStack;

                    BeforeStack[instruction.Offset] = currentStack;
                    instruction.UpdateStack(ref currentStack, hasReturnValue);
                    AfterStack[instruction.Offset] = currentStack;
                    if (currentStack > MaxObservedStack)
                        MaxObservedStack = currentStack;

                    switch (instruction.OpCode.FlowControl)
                    {
                        case FlowControl.Branch:
                            RegisterBranchTarget((Instruction)instruction.Operand!, instruction, currentStack, isSwitchTarget: false);
                            currentStack = 0;
                            continue;

                        case FlowControl.Cond_Branch:
                            if (instruction.OpCode.Code == Code.Switch)
                            {
                                foreach (var target in (Instruction[])instruction.Operand!)
                                    RegisterBranchTarget(target, instruction, currentStack, isSwitchTarget: true);
                            }
                            else
                            {
                                RegisterBranchTarget((Instruction)instruction.Operand!, instruction, currentStack, isSwitchTarget: false);
                            }

                            break;

                        case FlowControl.Call:
                            if (instruction.OpCode.Code == Code.Jmp)
                                currentStack = 0;
                            break;

                        case FlowControl.Return:
                        case FlowControl.Throw:
                            continue;
                    }

                    if (index + 1 < body.Instructions.Count)
                        Increment(RefCount, body.Instructions[index + 1].Offset);
                }
            }

            public Dictionary<uint, int> RefCount { get; }

            public Dictionary<uint, List<Instruction>> BranchReferences { get; }

            public Dictionary<uint, int> BeforeStack { get; }

            public Dictionary<uint, int> AfterStack { get; }

            public HashSet<uint> SwitchTargetOffsets { get; }

            public int MaxObservedStack { get; private set; }

            public int MultiSourceOffsetCount
                => RefCount.Values.Count(static count => count > 1);

            public bool HasUnknownIncomingSources
                => SwitchTargetOffsets.Count > 0 || MultiSourceOffsetCount > 0;

            public bool HasMultipleSources(uint offset)
                => RefCount.TryGetValue(offset, out var count) && count > 1;

            private static void Increment(IDictionary<uint, int> counts, uint key)
            {
                counts[key] = counts.TryGetValue(key, out var current) ? current + 1 : 1;
            }

            private void RegisterBranchTarget(Instruction target, Instruction source, int currentStack, bool isSwitchTarget)
            {
                if (!BeforeStack.ContainsKey(target.Offset))
                    BeforeStack[target.Offset] = currentStack;

                Increment(RefCount, target.Offset);
                if (!BranchReferences.TryGetValue(target.Offset, out var sources))
                {
                    sources = new List<Instruction>();
                    BranchReferences[target.Offset] = sources;
                }

                sources.Add(source);
                if (isSwitchTarget)
                    SwitchTargetOffsets.Add(target.Offset);
            }
        }

        private sealed class DecoyRoute
        {
            public required int EntryState { get; init; }

            public required int GuardState { get; init; }

            public required int PredicateSeed { get; init; }

            public required int DispatchSignature { get; init; }
        }

        private sealed class DispatchRoute
        {
            public required int DispatchSignature { get; init; }

            public required int PredicateSeed { get; init; }

            public required int GuardState { get; init; }

            public required Instruction TargetLabel { get; init; }
        }

        private enum ExceptionRegionKind
        {
            Try,
            Handler,
            Filter
        }

        private sealed class ExceptionRegion
        {
            public required ExceptionRegionKind Kind { get; init; }

            public required Instruction Start { get; init; }

            public required Instruction End { get; init; }

            public required IReadOnlyList<StackSlotProfile> InitialStack { get; init; }
        }

        private static readonly Random Rnd = new();
        private static readonly HashSet<uint> FlattenedMethods = new();
        private static readonly List<string> LastDiagnostics = new();
        private static readonly HashSet<string> MethodEmergencySkipOverrides = new(StringComparer.Ordinal);

        public static void Execute(ModuleDefMD md)
        {
            FlattenedMethods.Clear();
            LastDiagnostics.Clear();
            foreach (var type in md.Types)
            {
                if (type == md.GlobalType) continue;
                foreach (var meth in type.Methods)
                {
                    if (!CanTransformMethod(meth))
                        continue;

                    var decision = AnalyzeMethodDecision(meth);
                    LastDiagnostics.Add(decision.Diagnostic);
                    Debug.WriteLine(decision.Diagnostic);

                    if (decision.Mode == CflowTransformationMode.Skip)
                        continue;

                    if (TryExecuteMethod(meth, decision.Analysis, decision.Mode))
                        FlattenedMethods.Add(meth.MDToken.Raw);
                }
            }
        }

        internal static IReadOnlyList<string> AnalyzeModule(ModuleDefMD md)
        {
            var diagnostics = new List<string>();
            foreach (var type in md.Types)
            {
                if (type == md.GlobalType)
                    continue;

                foreach (var meth in type.Methods)
                {
                    if (!CanTransformMethod(meth))
                        continue;

                    diagnostics.Add(AnalyzeMethodDecision(meth).Diagnostic);
                }
            }

            return diagnostics;
        }

        internal static IReadOnlyList<string> GetLastDiagnostics()
            => LastDiagnostics.ToArray();

        private static bool IsMethodEnabledForDebug(MethodDef meth)
        {
            var key = GetMethodDebugKey(meth);
            return !MethodEmergencySkipOverrides.Contains(key);
        }

        internal static string GetMethodDebugKey(MethodDef meth)
        {
            var parameterList = meth.MethodSig is null || meth.MethodSig.Params.Count == 0
                ? string.Empty
                : string.Join(",", meth.MethodSig.Params.Select(static parameter => parameter.FullName));

            return $"{meth.DeclaringType.FullName}::{meth.Name}({parameterList})";
        }

        internal static bool WasTransformed(MethodDef meth)
            => FlattenedMethods.Contains(meth.MDToken.Raw);

        private static bool CanTransformMethod(MethodDef meth)
            => meth.HasBody &&
               meth.Body.HasInstructions &&
               !meth.IsConstructor &&
               !meth.IsGetter &&
               !meth.IsSetter &&
               !meth.IsAddOn &&
               !meth.IsRemoveOn &&
               !meth.IsAbstract &&
               !meth.HasGenericParameters;

        private static MethodTransformationDecision AnalyzeMethodDecision(MethodDef meth)
        {
            var analysis = AnalyzeMethodCompatibility(meth);
            var mode = SelectTransformationMode(analysis);
            var debugOverrideDisabled = !IsMethodEnabledForDebug(meth);
            if (debugOverrideDisabled)
                mode = CflowTransformationMode.Skip;

            return new MethodTransformationDecision
            {
                Analysis = analysis,
                Mode = mode,
                Diagnostic = BuildDecisionDiagnostic(meth, analysis, mode, debugOverrideDisabled)
            };
        }

        private static MethodCflowAnalysis AnalyzeMethodCompatibility(MethodDef meth)
        {
            var body = meth.Body;
            body.UpdateInstructionOffsets();

            var instructions = body.Instructions;
            var reasons = new List<string>();
            var hasSwitchInstructions = instructions.Any(static instruction => instruction.OpCode.Code == Code.Switch);
            var loweredSwitchCount = instructions.Count(static instruction => instruction.OpCode.Code == Code.Switch);
            var hasBackwardBranches = HasBackwardBranch(instructions);
            var hasFragileInstructionSequences = HasFragileInstructionSequences(instructions);
            var isUiOrEventMethod = IsLikelyUiOrEventMethod(meth);
            var isEntryPoint = meth.Module.EntryPoint == meth;
            var boolEnumLiteralSiteCount = CountSensitiveLiteralSites(meth, instructions);
            var hasBoolEnumHeavyLiteralConsumers = boolEnumLiteralSiteCount >= 2;
            var requiresEhSafeMode = false;
            var hasTransformableExceptionRegions = false;
            var hasNestedExceptionRegions = false;
            var exceptionRegionCount = 0;
            var transformableExceptionRegionCount = 0;
            var unsupportedExceptionRegionCount = 0;

            if (body.HasExceptionHandlers)
                reasons.Add("eh");
            if (hasSwitchInstructions)
                reasons.Add("switch");
            if (hasBackwardBranches)
                reasons.Add("loop");
            if (hasFragileInstructionSequences)
                reasons.Add("fragile-seq");
            if (isUiOrEventMethod)
                reasons.Add("ui-event");
            if (isEntryPoint)
                reasons.Add("entry-point");
            if (hasBoolEnumHeavyLiteralConsumers)
                reasons.Add("bool-enum-heavy");

            requiresEhSafeMode = isEntryPoint ||
                                 isUiOrEventMethod ||
                                 hasBoolEnumHeavyLiteralConsumers;

            var hasTraceAnalysis = TryCreateStackTraceAnalysis(meth, out var traceAnalysis);
            if (!hasTraceAnalysis)
            {
                reasons.Add("trace-fail");
            }
            else
            {
                if (traceAnalysis!.MultiSourceOffsetCount > 0)
                    reasons.Add("multi-source");
                if (traceAnalysis.HasUnknownIncomingSources)
                    reasons.Add("unknown-incoming");
                if (traceAnalysis.MaxObservedStack >= 16)
                    reasons.Add("deep-stack");
            }

            var canParseNormalizedBody = false;
            var supportsSpillRewrite = false;
            var blockCount = 0;
            if (body.HasExceptionHandlers)
            {
                var regionCompatibility = AnalyzeExceptionRegions(meth);
                hasTransformableExceptionRegions = regionCompatibility.TransformableRegionCount > 0;
                hasNestedExceptionRegions = regionCompatibility.HasNestedExceptionRegions;
                exceptionRegionCount = regionCompatibility.RegionCount;
                transformableExceptionRegionCount = regionCompatibility.TransformableRegionCount;
                unsupportedExceptionRegionCount = regionCompatibility.UnsupportedRegionCount;
                blockCount = regionCompatibility.MaxRegionBlockCount;

                if (hasNestedExceptionRegions)
                    reasons.Add("eh-nested");

                requiresEhSafeMode |= hasNestedExceptionRegions;

                reasons.Add(exceptionRegionCount == 0
                    ? "eh-region:none"
                    : $"eh-region:{transformableExceptionRegionCount}/{exceptionRegionCount}");
            }
            else if (!hasSwitchInstructions)
            {
                canParseNormalizedBody = BlockParser.TryParseMethod(meth, out var parsedBlocks);
                if (canParseNormalizedBody)
                {
                    blockCount = parsedBlocks.Count;
                    supportsSpillRewrite = SupportsDispatcherRewrite(parsedBlocks);
                    if (blockCount < 3)
                        reasons.Add("tiny");
                    if (!supportsSpillRewrite)
                        reasons.Add("spill-unsupported");
                }
                else
                {
                    reasons.Add("unparsed");
                }
            }

            return new MethodCflowAnalysis
            {
                HasTraceAnalysis = hasTraceAnalysis,
                HasExceptionHandlers = body.HasExceptionHandlers,
                HasTransformableExceptionRegions = hasTransformableExceptionRegions,
                HasNestedExceptionRegions = hasNestedExceptionRegions,
                HasSwitchInstructions = hasSwitchInstructions,
                HasBackwardBranches = hasBackwardBranches,
                HasMultipleSources = traceAnalysis?.MultiSourceOffsetCount > 0,
                HasUnknownIncomingSources = traceAnalysis?.HasUnknownIncomingSources ?? false,
                HasFragileInstructionSequences = hasFragileInstructionSequences,
                HasBoolEnumHeavyLiteralConsumers = hasBoolEnumHeavyLiteralConsumers,
                IsUiOrEventMethod = isUiOrEventMethod,
                IsEntryPoint = isEntryPoint,
                RequiresEhSafeMode = requiresEhSafeMode,
                CanParseNormalizedBody = canParseNormalizedBody,
                SupportsSpillRewrite = supportsSpillRewrite,
                InstructionCount = instructions.Count,
                ExceptionRegionCount = exceptionRegionCount,
                TransformableExceptionRegionCount = transformableExceptionRegionCount,
                UnsupportedExceptionRegionCount = unsupportedExceptionRegionCount,
                LoweredSwitchCount = loweredSwitchCount,
                BlockCount = blockCount,
                MaxObservedStack = Math.Max(body.MaxStack, traceAnalysis?.MaxObservedStack ?? 0),
                MultiSourceOffsetCount = traceAnalysis?.MultiSourceOffsetCount ?? 0,
                BoolEnumLiteralSiteCount = boolEnumLiteralSiteCount,
                Reasons = reasons.Distinct(StringComparer.Ordinal).ToArray()
            };
        }

        private static bool TryCreateStackTraceAnalysis(MethodDef meth, out StackTraceAnalysis? traceAnalysis)
        {
            try
            {
                traceAnalysis = new StackTraceAnalysis(meth);
                return true;
            }
            catch
            {
                traceAnalysis = null;
                return false;
            }
        }

        private static CflowTransformationMode SelectTransformationMode(MethodCflowAnalysis analysis)
        {
            if (!analysis.HasTraceAnalysis)
                return CflowTransformationMode.Skip;

            if (analysis.InstructionCount < 4)
                return CflowTransformationMode.Skip;

            if (analysis.HasExceptionHandlers)
            {
                if (!analysis.HasTransformableExceptionRegions)
                    return CflowTransformationMode.Skip;

                if (analysis.RequiresEhSafeMode)
                    return CflowTransformationMode.EHSafe;

                if (analysis.HasFragileInstructionSequences)
                    return CflowTransformationMode.Fragile;

                return CflowTransformationMode.Safe;
            }

            if (!analysis.HasSwitchInstructions)
            {
                if (!analysis.CanParseNormalizedBody || !analysis.SupportsSpillRewrite)
                    return CflowTransformationMode.Skip;
            }

            if (analysis.RequiresEhSafeMode)
                return CflowTransformationMode.EHSafe;

            if (analysis.HasFragileInstructionSequences)
                return CflowTransformationMode.Fragile;

            if (analysis.IsEntryPoint ||
                analysis.HasSwitchInstructions ||
                analysis.HasUnknownIncomingSources ||
                analysis.IsUiOrEventMethod)
            {
                return CflowTransformationMode.Safe;
            }

            return CflowTransformationMode.Full;
        }

        private static string BuildDecisionDiagnostic(MethodDef meth,
            MethodCflowAnalysis analysis,
            CflowTransformationMode mode,
            bool debugOverrideDisabled)
        {
            var reasons = new List<string>();
            if (debugOverrideDisabled)
                reasons.Add("debug-override");

            reasons.AddRange(analysis.Reasons);

            var diagnostic = $"CFlow {mode.ToString().ToLowerInvariant()}: {GetMethodDebugKey(meth)}";
            if (reasons.Count > 0)
                diagnostic += $" reason={string.Join(",", reasons.Distinct(StringComparer.Ordinal))}";

            return diagnostic;
        }

        private static bool HasBackwardBranch(IList<Instruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.Operand is Instruction target && target.Offset <= instruction.Offset)
                    return true;

                if (instruction.Operand is IList<Instruction> targets && targets.Any(target => target.Offset <= instruction.Offset))
                    return true;
            }

            return false;
        }

        private static bool HasFragileInstructionSequences(IList<Instruction> instructions)
        {
            for (var index = 0; index < instructions.Count; index++)
            {
                if (instructions[index].OpCode.OpCodeType == OpCodeType.Prefix)
                    return true;

                if (HasInstructionSequence(instructions, index, Code.Dup, Code.Ldvirtftn, Code.Newobj) ||
                    HasInstructionSequence(instructions, index, Code.Ldftn, Code.Newobj) ||
                    HasInstructionSequence(instructions, index, Code.Ldc_I4, Code.Newarr, Code.Dup, Code.Ldtoken, Code.Call))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasInstructionSequence(IList<Instruction> instructions, int offset, params Code[] codes)
        {
            if (offset + codes.Length > instructions.Count)
                return false;

            for (var index = 0; index < codes.Length; index++)
            {
                if (instructions[offset + index].OpCode.Code != codes[index])
                    return false;
            }

            return true;
        }

        private static bool IsLikelyUiOrEventMethod(MethodDef meth)
        {
            var methodName = meth.Name.String;
            if (string.Equals(methodName, "InitializeComponent", StringComparison.Ordinal) ||
                methodName.EndsWith("_Load", StringComparison.Ordinal) ||
                methodName.Contains("DoWork", StringComparison.Ordinal) ||
                methodName.Contains("RunWorker", StringComparison.Ordinal))
            {
                return true;
            }

            return meth.MethodSig?.Params.Any(static parameter =>
                parameter.FullName.Contains("System.EventArgs", StringComparison.Ordinal) ||
                parameter.FullName.Contains("EventArgs", StringComparison.Ordinal)) == true;
        }

        private static int CountSensitiveLiteralSites(MethodDef meth, IList<Instruction> instructions)
        {
            var count = 0;
            for (var index = 0; index < instructions.Count; index++)
            {
                if (!instructions[index].IsLdcI4())
                    continue;

                if (ClassifyPayloadConstantContext(meth.Module, meth, instructions, index) != PayloadConstantContext.NormalInt)
                    count++;
            }

            return count;
        }

        private static bool TryExecuteMethod(MethodDef meth, MethodCflowAnalysis analysis, CflowTransformationMode mode)
        {
            var body = meth.Body;
            var originalInstructions = body.Instructions.ToList();
            var originalVariables = body.Variables.ToList();
            var originalInitLocals = body.InitLocals;

            try
            {
                NormalizeMethodBody(meth);

                var transformed = body.HasExceptionHandlers
                    ? TryExecuteExceptionRegions(meth, mode)
                    : TryExecuteWholeMethod(meth, mode);

                if (!transformed)
                    return RestoreOriginalBody();

                ValidateMethod(meth, analysis);
                return true;
            }
            catch
            {
                return RestoreOriginalBody();
            }

            bool RestoreOriginalBody()
            {
                body.Instructions.Clear();
                foreach (var instruction in originalInstructions)
                    body.Instructions.Add(instruction);

                body.Variables.Clear();
                foreach (var variable in originalVariables)
                    body.Variables.Add(variable);

                body.InitLocals = originalInitLocals;
                return false;
            }
        }

        private static void NormalizeMethodBody(MethodDef meth)
        {
            var body = meth.Body;
            body.SimplifyBranches();
            body.SimplifyMacros(meth.Parameters);
        }

        private static bool TryExecuteWholeMethod(MethodDef meth, CflowTransformationMode mode)
        {
            LowerSwitchInstructions(meth, 0, meth.Body.Instructions.Count);

            if (!TryRewriteInstructionSlice(meth,
                meth.Body.Instructions.ToList(),
                Array.Empty<StackSlotProfile>(),
                mode,
                out var rewrittenInstructions,
                out var localsToAdd))
            {
                return false;
            }

            meth.Body.Instructions.Clear();
            foreach (var instruction in rewrittenInstructions)
                meth.Body.Instructions.Add(instruction);

            foreach (var local in localsToAdd)
                meth.Body.Variables.Add(local);

            meth.Body.InitLocals = true;
            return true;
        }

        private static bool TryExecuteExceptionRegions(MethodDef meth, CflowTransformationMode mode)
        {
            var transformedAnyRegion = false;
            foreach (var region in EnumerateExceptionRegions(meth))
            {
                if (!CanRewriteExceptionRegion(region, mode))
                    continue;

                if (!TryRewriteExceptionRegion(meth, region, mode))
                    continue;

                transformedAnyRegion = true;
            }

            if (transformedAnyRegion)
                meth.Body.InitLocals = true;

            return transformedAnyRegion;
        }

        private static List<Block> Randomize(List<Block> input)
        {
            var ret = new List<Block>();
            foreach (var group in input)
                ret.Insert(Rnd.Next(0, ret.Count), group);

            for (var index = 0; index < ret.Count; index++)
                ret[index].PhysicalOrder = index;

            return ret;
        }

        private static IEnumerable<ExceptionRegion> EnumerateExceptionRegions(MethodDef meth)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var handler in meth.Body.ExceptionHandlers)
            {
                if (TryCreateRegion(meth, ExceptionRegionKind.Try, handler.TryStart, handler.TryEnd, Array.Empty<StackSlotProfile>(), seen, out var tryRegion))
                    yield return tryRegion;

                if (handler.FilterStart is not null)
                {
                    IReadOnlyList<StackSlotProfile> filterSeed = new[]
                    {
                        new StackSlotProfile(StackSlotKind.Object, meth.Module.CorLibTypes.Object)
                    };
                    if (TryCreateRegion(meth, ExceptionRegionKind.Filter, handler.FilterStart, handler.HandlerStart, filterSeed, seen, out var filterRegion))
                        yield return filterRegion;
                }

                var handlerSeed = CreateHandlerInitialStack(meth, handler);
                if (TryCreateRegion(meth, ExceptionRegionKind.Handler, handler.HandlerStart, handler.HandlerEnd, handlerSeed, seen, out var handlerRegion))
                    yield return handlerRegion;
            }
        }

        private static ExceptionRegionCompatibility AnalyzeExceptionRegions(MethodDef meth)
        {
            var transformableRegionCount = 0;
            var unsupportedRegionCount = 0;
            var maxRegionBlockCount = 0;

            foreach (var region in EnumerateExceptionRegions(meth))
            {
                if (!CanRewriteExceptionRegion(region, CflowTransformationMode.EHSafe))
                    continue;

                if (!TryGetRegionSlice(meth, region, out var slice))
                    continue;

                if (!BlockParser.TryParseInstructions(meth, slice, region.InitialStack, out var blocks) ||
                    !SupportsDispatcherRewrite(blocks))
                {
                    unsupportedRegionCount++;
                    continue;
                }

                transformableRegionCount++;
                if (blocks.Count > maxRegionBlockCount)
                    maxRegionBlockCount = blocks.Count;
            }

            return new ExceptionRegionCompatibility
            {
                HasNestedExceptionRegions = HasNestedExceptionRegions(meth),
                RegionCount = transformableRegionCount + unsupportedRegionCount,
                TransformableRegionCount = transformableRegionCount,
                UnsupportedRegionCount = unsupportedRegionCount,
                MaxRegionBlockCount = maxRegionBlockCount
            };
        }

        private static bool CanRewriteExceptionRegion(ExceptionRegion region, CflowTransformationMode mode)
            => region.Kind == ExceptionRegionKind.Try;

        private static bool TryCreateRegion(MethodDef meth,
            ExceptionRegionKind kind,
            Instruction? start,
            Instruction? end,
            IReadOnlyList<StackSlotProfile> initialStack,
            ISet<string> seen,
            out ExceptionRegion region)
        {
            region = null!;
            if (start is null || end is null ||
                !TryGetRegionBounds(meth, start, end, out var startIndex, out var endIndex))
                return false;

            if (HasNestedEhBoundaryInside(meth, startIndex, endIndex))
                return false;

            var key = BuildExceptionRegionKey(kind, start, end);
            if (!seen.Add(key))
                return false;

            region = new ExceptionRegion
            {
                Kind = kind,
                Start = start,
                End = end,
                InitialStack = initialStack
            };
            return true;
        }

        private static bool TryGetRegionBounds(MethodDef meth,
            Instruction start,
            Instruction end,
            out int startIndex,
            out int endIndex)
        {
            var instructions = meth.Body.Instructions;
            startIndex = instructions.IndexOf(start);
            endIndex = instructions.IndexOf(end);
            return startIndex >= 0 && endIndex > startIndex;
        }

        private static bool TryGetRegionSlice(MethodDef meth, ExceptionRegion region, out List<Instruction> slice)
        {
            slice = new List<Instruction>();
            if (!TryGetRegionBounds(meth, region.Start, region.End, out var startIndex, out var endIndex))
                return false;

            slice = meth.Body.Instructions.Skip(startIndex).Take(endIndex - startIndex).ToList();
            return slice.Count > 0;
        }

        private static string BuildExceptionRegionKey(ExceptionRegionKind kind, Instruction start, Instruction end)
            => $"{kind}:{start.Offset}:{end.Offset}";

        private static IReadOnlyList<StackSlotProfile> CreateHandlerInitialStack(MethodDef meth, ExceptionHandler handler)
        {
            if (handler.HandlerType == ExceptionHandlerType.Catch)
            {
                var catchType = handler.CatchType?.ToTypeSig() ?? meth.Module.CorLibTypes.Object;
                return new[] { new StackSlotProfile(StackSlotKind.Object, catchType) };
            }

            return Array.Empty<StackSlotProfile>();
        }

        private static bool HasNestedExceptionRegions(MethodDef meth)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var handler in meth.Body.ExceptionHandlers)
            {
                if (IsNestedExceptionRegion(meth, ExceptionRegionKind.Try, handler.TryStart, handler.TryEnd, seen) ||
                    IsNestedExceptionRegion(meth, ExceptionRegionKind.Handler, handler.HandlerStart, handler.HandlerEnd, seen) ||
                    IsNestedExceptionRegion(meth, ExceptionRegionKind.Filter, handler.FilterStart, handler.HandlerStart, seen))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsNestedExceptionRegion(MethodDef meth,
            ExceptionRegionKind kind,
            Instruction? start,
            Instruction? end,
            ISet<string> seen)
        {
            if (start is null || end is null)
                return false;

            var key = BuildExceptionRegionKey(kind, start, end);
            if (!seen.Add(key) || !TryGetRegionBounds(meth, start, end, out var startIndex, out var endIndex))
                return false;

            return HasNestedEhBoundaryInside(meth, startIndex, endIndex);
        }

        private static bool HasNestedEhBoundaryInside(MethodDef meth, int startIndex, int endIndex)
        {
            foreach (var handler in meth.Body.ExceptionHandlers)
            {
                if (IsBoundaryInside(meth, handler.TryStart, startIndex, endIndex) ||
                    IsBoundaryInside(meth, handler.HandlerStart, startIndex, endIndex) ||
                    IsBoundaryInside(meth, handler.FilterStart, startIndex, endIndex))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsBoundaryInside(MethodDef meth, Instruction? boundary, int startIndex, int endIndex)
        {
            if (boundary is null)
                return false;

            var boundaryIndex = meth.Body.Instructions.IndexOf(boundary);
            return boundaryIndex > startIndex && boundaryIndex < endIndex;
        }

        private static bool TryRewriteExceptionRegion(MethodDef meth, ExceptionRegion region, CflowTransformationMode mode)
        {
            var body = meth.Body;
            var instructionSnapshot = body.Instructions.ToList();
            var variableSnapshot = body.Variables.ToList();
            var initLocalsSnapshot = body.InitLocals;

            var startIndex = body.Instructions.IndexOf(region.Start);
            var endIndex = body.Instructions.IndexOf(region.End);
            if (startIndex < 0 || endIndex <= startIndex)
                return false;

            LowerSwitchInstructions(meth, startIndex, endIndex);

            startIndex = body.Instructions.IndexOf(body.Instructions[startIndex]);
            endIndex = body.Instructions.IndexOf(region.End);
            var slice = body.Instructions.Skip(startIndex).Take(endIndex - startIndex).ToList();

            if (!TryRewriteInstructionSlice(meth,
                slice,
                region.InitialStack,
                mode,
                out var rewrittenInstructions,
                out var localsToAdd))
            {
                RestoreSnapshot();
                return false;
            }

            ReplaceInstructionRange(body, startIndex, endIndex - startIndex, rewrittenInstructions);
            UpdateExceptionRegionStart(meth, region, rewrittenInstructions[0]);
            foreach (var local in localsToAdd)
                body.Variables.Add(local);

            return true;

            void RestoreSnapshot()
            {
                body.Instructions.Clear();
                foreach (var instruction in instructionSnapshot)
                    body.Instructions.Add(instruction);

                body.Variables.Clear();
                foreach (var variable in variableSnapshot)
                    body.Variables.Add(variable);

                body.InitLocals = initLocalsSnapshot;
            }
        }

        private static void ReplaceInstructionRange(CilBody body,
            int startIndex,
            int length,
            IReadOnlyList<Instruction> replacement)
        {
            var rewritten = new List<Instruction>(body.Instructions.Count - length + replacement.Count);
            for (var index = 0; index < startIndex; index++)
                rewritten.Add(body.Instructions[index]);

            rewritten.AddRange(replacement);

            for (var index = startIndex + length; index < body.Instructions.Count; index++)
                rewritten.Add(body.Instructions[index]);

            body.Instructions.Clear();
            foreach (var instruction in rewritten)
                body.Instructions.Add(instruction);
        }

        private static void UpdateExceptionRegionStart(MethodDef meth, ExceptionRegion region, Instruction newStart)
        {
            foreach (var handler in meth.Body.ExceptionHandlers)
            {
                if (region.Kind == ExceptionRegionKind.Try && handler.TryStart == region.Start && handler.TryEnd == region.End)
                    handler.TryStart = newStart;

                if (region.Kind == ExceptionRegionKind.Handler && handler.HandlerStart == region.Start && handler.HandlerEnd == region.End)
                    handler.HandlerStart = newStart;

                if (region.Kind == ExceptionRegionKind.Filter && handler.FilterStart == region.Start && handler.HandlerStart == region.End)
                    handler.FilterStart = newStart;
            }
        }

        private static bool TryRewriteInstructionSlice(MethodDef meth,
            IReadOnlyList<Instruction> instructions,
            IReadOnlyList<StackSlotProfile> initialStack,
            CflowTransformationMode mode,
            out List<Instruction> rewrittenInstructions,
            out IReadOnlyList<Local> localsToAdd)
        {
            rewrittenInstructions = new List<Instruction>();
            localsToAdd = Array.Empty<Local>();

            if (!BlockParser.TryParseInstructions(meth, instructions, initialStack, out var blocks))
                return false;

            if (!SupportsDispatcherRewrite(blocks))
                return false;

            var stateLocal = new Local(meth.Module.CorLibTypes.Int32);
            var guardLocal = new Local(meth.Module.CorLibTypes.Int32);
            var scratchLocal = new Local(meth.Module.CorLibTypes.Int32);
            var selectorLocal = new Local(meth.Module.CorLibTypes.Int32);
            var payloadScratchLocal = new Local(meth.Module.CorLibTypes.Int32);
            var payloadAuxLocal = new Local(meth.Module.CorLibTypes.Int32);
            var payloadFlagLocal = new Local(meth.Module.CorLibTypes.Boolean);
            var stackSpillLocals = CreateStackSpillLocals(blocks);
            var stateKey = NextNonZeroInt();
            var guardKey = NextNonZeroInt();
            var relationKey = NextNonZeroInt();
            var compareKey = NextNonZeroInt();
            var compareSalt = NextNonZeroInt();
            var selectorBias = Rnd.Next(7, 31);
            var stateMode = Rnd.Next(0, 2) == 0 ? StateEncodingMode.Xor : StateEncodingMode.Add;
            var guardMode = Rnd.Next(0, 2) == 0 ? StateEncodingMode.Xor : StateEncodingMode.Add;

            AssignStates(blocks, relationKey, compareKey, compareSalt);
            var shuffledBlocks = mode == CflowTransformationMode.Fragile
                || mode == CflowTransformationMode.EHSafe
                ? blocks
                : Randomize(blocks);
            var decoyRoutes = mode == CflowTransformationMode.Full
                ? BuildDecoyRoutes(blocks, relationKey, compareKey, compareSalt)
                : Array.Empty<DecoyRoute>();
            rewrittenInstructions = BuildFlattenedInstructions(blocks,
                shuffledBlocks,
                decoyRoutes,
                mode,
                meth,
                meth.Module,
                stackSpillLocals,
                stateLocal,
                guardLocal,
                scratchLocal,
                selectorLocal,
                payloadScratchLocal,
                payloadAuxLocal,
                payloadFlagLocal,
                stateKey,
                guardKey,
                relationKey,
                compareKey,
                compareSalt,
                selectorBias,
                stateMode,
                guardMode);

            localsToAdd = new[] { stateLocal, guardLocal, scratchLocal, selectorLocal, payloadScratchLocal, payloadAuxLocal, payloadFlagLocal }
                .Concat(stackSpillLocals.Values.SelectMany(x => x))
                .ToList();
            return true;
        }

        private static void LowerSwitchInstructions(MethodDef meth, int startIndex, int endIndex)
        {
            var instructions = meth.Body.Instructions;
            Local? selectorLocal = null;
            for (var index = startIndex; index < endIndex; index++)
            {
                var instruction = instructions[index];
                if (instruction.OpCode.Code != Code.Switch)
                    continue;

                if (instruction.Operand is not IList<Instruction> targets)
                    throw new InvalidOperationException("Unsupported switch operand.");

                if (index + 1 >= instructions.Count)
                    throw new InvalidOperationException("Switch without fallthrough target.");

                selectorLocal ??= new Local(meth.Module.CorLibTypes.Int32);
                if (!meth.Body.Variables.Contains(selectorLocal))
                    meth.Body.Variables.Add(selectorLocal);

                var defaultTarget = instructions[index + 1];
                instructions[index] = Instruction.Create(OpCodes.Stloc, selectorLocal);

                var insertionIndex = index + 1;
                for (var caseIndex = 0; caseIndex < targets.Count; caseIndex++)
                {
                    instructions.Insert(insertionIndex++, Instruction.Create(OpCodes.Ldloc, selectorLocal));
                    instructions.Insert(insertionIndex++, Instruction.Create(OpCodes.Ldc_I4, caseIndex));
                    instructions.Insert(insertionIndex++, Instruction.Create(OpCodes.Beq, targets[caseIndex]));
                }

                instructions.Insert(insertionIndex, Instruction.Create(OpCodes.Br, defaultTarget));
                endIndex += targets.Count * 3 + 1;
                index = insertionIndex;
            }
        }

        private static bool SupportsDispatcherRewrite(IEnumerable<Block> blocks)
            => blocks.All(block =>
                block.EntryStackProfile.All(CanSpillSlot) &&
                (IsTerminalExitKind(block.ExitKind) ||
                 block.ExitStackProfile.All(CanSpillSlot)));

        private static bool IsTerminalExitKind(BlockExitKind exitKind)
            => exitKind == BlockExitKind.Return ||
               exitKind == BlockExitKind.Throw ||
               exitKind == BlockExitKind.Leave ||
               exitKind == BlockExitKind.Endfinally ||
               exitKind == BlockExitKind.Endfilter ||
               exitKind == BlockExitKind.Rethrow;

        private static bool CanSpillSlot(StackSlotProfile slot)
            => slot.Kind != StackSlotKind.Unknown;

        private static IReadOnlyDictionary<int, IReadOnlyList<Local>> CreateStackSpillLocals(IEnumerable<Block> blocks)
            => blocks.ToDictionary(
                block => block.Id,
                block => (IReadOnlyList<Local>)block.EntryStackProfile
                    .Select(slot => new Local(slot.SpillType))
                    .ToList());

        private static void AssignStates(IEnumerable<Block> blocks, int relationKey, int compareKey, int compareSalt)
        {
            var usedStates = new HashSet<int>();
            foreach (var block in blocks)
            {
                var nextState = NextUniqueState(usedStates);
                block.EntryState = nextState;
                block.PredicateSeed = NextNonZeroInt();
                block.GuardState = ComputeGuardState(nextState, relationKey, block.PredicateSeed);
                block.DispatchSignature = ComputeDispatchSignature(nextState, compareKey, compareSalt);
            }
        }

        private static int ComputeGuardState(int rawState, int relationKey, int predicateSeed)
            => unchecked((rawState ^ relationKey) + predicateSeed);

        private static int ComputeDispatchSignature(int rawState, int compareKey, int compareSalt)
            => unchecked((rawState + compareSalt) ^ compareKey);

        private static int NextUniqueState(ISet<int> usedStates)
        {
            var candidate = NextNonZeroInt();
            while (!usedStates.Add(candidate))
                candidate = NextNonZeroInt();

            return candidate;
        }

        private static int NextNonZeroInt()
        {
            var high = Rnd.Next(short.MinValue, short.MaxValue + 1);
            var low = Rnd.Next(short.MinValue, short.MaxValue + 1);
            var candidate = (high << 16) ^ (low & 0xFFFF);
            return candidate == 0 ? 0x13572468 : candidate;
        }

        private static IReadOnlyList<DecoyRoute> BuildDecoyRoutes(IReadOnlyCollection<Block> blocks,
            int relationKey,
            int compareKey,
            int compareSalt)
        {
            var usedStates = new HashSet<int>(blocks.Select(block => block.EntryState));
            var decoys = new List<DecoyRoute>();
            var count = Math.Min(3, Math.Max(1, blocks.Count / 2));
            for (var index = 0; index < count; index++)
            {
                var state = NextUniqueState(usedStates);
                var predicateSeed = NextNonZeroInt();
                decoys.Add(new DecoyRoute
                {
                    EntryState = state,
                    PredicateSeed = predicateSeed,
                    GuardState = ComputeGuardState(state, relationKey, predicateSeed),
                    DispatchSignature = ComputeDispatchSignature(state, compareKey, compareSalt)
                });
            }

            return decoys;
        }

        private static List<Instruction> BuildFlattenedInstructions(IReadOnlyList<Block> blocks,
            IReadOnlyList<Block> shuffledBlocks,
            IReadOnlyList<DecoyRoute> decoyRoutes,
            CflowTransformationMode mode,
            MethodDef meth,
            ModuleDef module,
            IReadOnlyDictionary<int, IReadOnlyList<Local>> stackSpillLocals,
            Local stateLocal,
            Local guardLocal,
            Local scratchLocal,
            Local selectorLocal,
            Local payloadScratchLocal,
            Local payloadAuxLocal,
            Local payloadFlagLocal,
            int stateKey,
            int guardKey,
            int relationKey,
            int compareKey,
            int compareSalt,
            int selectorBias,
            StateEncodingMode stateMode,
            StateEncodingMode guardMode)
        {
            if (mode == CflowTransformationMode.EHSafe)
            {
                return BuildEhSafeInstructions(blocks,
                    meth,
                    module,
                    stackSpillLocals,
                    stateLocal,
                    guardLocal,
                    payloadScratchLocal,
                    payloadAuxLocal,
                    payloadFlagLocal,
                    stateKey,
                    guardKey,
                    stateMode,
                    guardMode);
            }

            var instructions = new List<Instruction>();
            var dispatcher = Instruction.Create(OpCodes.Nop);
            var switchDispatch = Instruction.Create(OpCodes.Nop);
            var blockLabels = blocks.ToDictionary(block => block.Id, _ => Instruction.Create(OpCodes.Nop));
            var stateMap = blocks.ToDictionary(block => block.Id, block => block.EntryState);
            var blockMap = blocks.ToDictionary(block => block.Id, block => block);
            var entryBlock = blocks.Single(block => block.IsEntry);
            var decoyLabels = decoyRoutes.ToDictionary(route => route, _ => Instruction.Create(OpCodes.Nop));
            var routes = new List<DispatchRoute>();

            routes.AddRange(decoyRoutes.Select(route => new DispatchRoute
            {
                DispatchSignature = route.DispatchSignature,
                PredicateSeed = route.PredicateSeed,
                GuardState = route.GuardState,
                TargetLabel = decoyLabels[route]
            }));
            routes.AddRange(shuffledBlocks.Select(block => new DispatchRoute
            {
                DispatchSignature = block.DispatchSignature,
                PredicateSeed = block.PredicateSeed,
                GuardState = block.GuardState,
                TargetLabel = blockLabels[block.Id]
            }));

            var extraJunkCount = mode == CflowTransformationMode.Full
                ? Math.Max(3, routes.Count / 2)
                : 1;
            var totalCaseCount = routes.Count + extraJunkCount;
            var caseSlots = Enumerable.Range(0, totalCaseCount).OrderBy(_ => Rnd.Next()).ToArray();
            var routeCaseMap = new Dictionary<Instruction, int>();
            for (var index = 0; index < routes.Count; index++)
                routeCaseMap[routes[index].TargetLabel] = caseSlots[index];

            var defaultCaseIndex = caseSlots[routes.Count];
            var junkLabels = Enumerable.Range(0, extraJunkCount)
                .Select(_ => Instruction.Create(OpCodes.Nop))
                .ToArray();
            var switchTargets = new Instruction[totalCaseCount];
            for (var index = 0; index < switchTargets.Length; index++)
                switchTargets[index] = junkLabels[index % junkLabels.Length];

            foreach (var route in routes)
                switchTargets[routeCaseMap[route.TargetLabel]] = route.TargetLabel;

            instructions.AddRange(SpillEvaluationStack(stackSpillLocals[entryBlock.Id]));
            instructions.AddRange(StoreRoute(entryBlock.EntryState,
                entryBlock.GuardState,
                stateLocal,
                guardLocal,
                stateKey,
                guardKey,
                stateMode,
                guardMode));
            instructions.Add(Instruction.Create(OpCodes.Br, dispatcher));
            instructions.Add(dispatcher);

            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, defaultCaseIndex + selectorBias));
            instructions.Add(Instruction.Create(OpCodes.Stloc, selectorLocal));

            foreach (var route in routes)
            {
                instructions.AddRange(BuildDispatchCheck(route.DispatchSignature,
                    route.PredicateSeed,
                    route.GuardState,
                    stateLocal,
                    guardLocal,
                    scratchLocal,
                    selectorLocal,
                    stateKey,
                    guardKey,
                    relationKey,
                    compareKey,
                    compareSalt,
                    routeCaseMap[route.TargetLabel] + selectorBias,
                    switchDispatch,
                    stateMode,
                    guardMode));
            }

            instructions.Add(Instruction.Create(OpCodes.Br, switchDispatch));
            instructions.Add(switchDispatch);
            instructions.Add(Instruction.Create(OpCodes.Ldloc, selectorLocal));
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, selectorBias));
            instructions.Add(Instruction.Create(OpCodes.Sub));
            instructions.Add(Instruction.Create(OpCodes.Switch, switchTargets));
            instructions.Add(Instruction.Create(OpCodes.Br, dispatcher));

            foreach (var block in shuffledBlocks)
            {
                instructions.Add(blockLabels[block.Id]);
                instructions.AddRange(RestoreEvaluationStack(stackSpillLocals[block.Id]));
                instructions.AddRange(BuildBlockInstructions(block,
                    blockMap,
                    stateMap,
                    mode,
                    meth,
                    module,
                    stackSpillLocals,
                    stateLocal,
                    guardLocal,
                    dispatcher,
                    payloadScratchLocal,
                    payloadAuxLocal,
                    payloadFlagLocal,
                    stateKey,
                    guardKey,
                    stateMode,
                    guardMode));
            }

            foreach (var decoy in decoyRoutes)
            {
                instructions.Add(decoyLabels[decoy]);
                instructions.AddRange(StoreRoute(entryBlock.EntryState,
                    entryBlock.GuardState,
                    stateLocal,
                    guardLocal,
                    stateKey,
                    guardKey,
                    stateMode,
                    guardMode));
                instructions.Add(Instruction.Create(OpCodes.Br, dispatcher));
            }

            for (var index = 0; index < junkLabels.Length; index++)
            {
                var junkLabel = junkLabels[index];
                var targetRoute = decoyRoutes.Count > 0 && index % 2 == 0
                    ? decoyRoutes[index % decoyRoutes.Count]
                    : null;

                instructions.Add(junkLabel);
                if (targetRoute is not null)
                {
                    instructions.AddRange(StoreRoute(targetRoute.EntryState,
                        targetRoute.GuardState,
                        stateLocal,
                        guardLocal,
                        stateKey,
                        guardKey,
                        stateMode,
                        guardMode));
                }
                else
                {
                    instructions.AddRange(StoreRoute(entryBlock.EntryState,
                        entryBlock.GuardState,
                        stateLocal,
                        guardLocal,
                        stateKey,
                        guardKey,
                        stateMode,
                        guardMode));
                }

                instructions.Add(Instruction.Create(OpCodes.Br, dispatcher));
            }

            return instructions;
        }

        private static List<Instruction> BuildEhSafeInstructions(IReadOnlyList<Block> blocks,
            MethodDef meth,
            ModuleDef module,
            IReadOnlyDictionary<int, IReadOnlyList<Local>> stackSpillLocals,
            Local stateLocal,
            Local guardLocal,
            Local payloadScratchLocal,
            Local payloadAuxLocal,
            Local payloadFlagLocal,
            int stateKey,
            int guardKey,
            StateEncodingMode stateMode,
            StateEncodingMode guardMode)
        {
            var instructions = new List<Instruction>();
            var dispatcher = Instruction.Create(OpCodes.Nop);
            var blockLabels = blocks.ToDictionary(block => block.Id, _ => Instruction.Create(OpCodes.Nop));
            var stateMap = blocks.ToDictionary(block => block.Id, block => block.EntryState);
            var blockMap = blocks.ToDictionary(block => block.Id, block => block);
            var entryBlock = blocks.Single(block => block.IsEntry);

            instructions.AddRange(SpillEvaluationStack(stackSpillLocals[entryBlock.Id]));
            instructions.AddRange(StoreRoute(entryBlock.EntryState,
                entryBlock.GuardState,
                stateLocal,
                guardLocal,
                stateKey,
                guardKey,
                stateMode,
                guardMode));
            instructions.Add(Instruction.Create(OpCodes.Br, dispatcher));
            instructions.Add(dispatcher);

            foreach (var block in blocks)
            {
                var nextCheck = Instruction.Create(OpCodes.Nop);
                instructions.Add(Instruction.Create(OpCodes.Ldloc, stateLocal));
                instructions.AddRange(DecodeState(stateKey, stateMode, NextNonZeroInt()));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, block.EntryState));
                instructions.Add(Instruction.Create(OpCodes.Bne_Un, nextCheck));
                instructions.Add(Instruction.Create(OpCodes.Br, blockLabels[block.Id]));
                instructions.Add(nextCheck);
            }

            instructions.Add(Instruction.Create(OpCodes.Br, blockLabels[entryBlock.Id]));

            foreach (var block in blocks)
            {
                instructions.Add(blockLabels[block.Id]);
                instructions.AddRange(RestoreEvaluationStack(stackSpillLocals[block.Id]));
                instructions.AddRange(BuildBlockInstructions(block,
                    blockMap,
                    stateMap,
                    CflowTransformationMode.EHSafe,
                    meth,
                    module,
                    stackSpillLocals,
                    stateLocal,
                    guardLocal,
                    dispatcher,
                    payloadScratchLocal,
                    payloadAuxLocal,
                    payloadFlagLocal,
                    stateKey,
                    guardKey,
                    stateMode,
                    guardMode));
            }

            return instructions;
        }

        private static IEnumerable<Instruction> BuildDispatchCheck(int dispatchSignature,
            int predicateSeed,
            int guardState,
            Local stateLocal,
            Local guardLocal,
            Local scratchLocal,
            Local selectorLocal,
            int stateKey,
            int guardKey,
            int relationKey,
            int compareKey,
            int compareSalt,
            int selectorValue,
            Instruction switchDispatch,
            StateEncodingMode stateMode,
            StateEncodingMode guardMode)
        {
            var nextCheck = Instruction.Create(OpCodes.Nop);

            yield return Instruction.Create(OpCodes.Ldloc, stateLocal);
            foreach (var instruction in DecodeState(stateKey, stateMode, NextNonZeroInt()))
                yield return instruction;
            yield return Instruction.Create(OpCodes.Ldc_I4, compareSalt);
            yield return Instruction.Create(OpCodes.Add);
            yield return Instruction.Create(OpCodes.Ldc_I4, compareKey);
            yield return Instruction.Create(OpCodes.Xor);
            yield return Instruction.Create(OpCodes.Ldc_I4, dispatchSignature);
            yield return Instruction.Create(OpCodes.Ceq);
            yield return Instruction.Create(OpCodes.Brfalse, nextCheck);

            yield return Instruction.Create(OpCodes.Ldloc, stateLocal);
            foreach (var instruction in DecodeState(stateKey, stateMode, NextNonZeroInt()))
                yield return instruction;
            yield return Instruction.Create(OpCodes.Ldc_I4, relationKey);
            yield return Instruction.Create(OpCodes.Xor);
            yield return Instruction.Create(OpCodes.Ldc_I4, predicateSeed);
            yield return Instruction.Create(OpCodes.Add);
            yield return Instruction.Create(OpCodes.Ldloc, guardLocal);
            foreach (var instruction in DecodeState(guardKey, guardMode, NextNonZeroInt()))
                yield return instruction;
            yield return Instruction.Create(OpCodes.Stloc, scratchLocal);
            yield return Instruction.Create(OpCodes.Ldloc, scratchLocal);
            yield return Instruction.Create(OpCodes.Ceq);
            yield return Instruction.Create(OpCodes.Brfalse, nextCheck);
            yield return Instruction.Create(OpCodes.Ldloc, scratchLocal);
            yield return Instruction.Create(OpCodes.Ldc_I4, guardState);
            yield return Instruction.Create(OpCodes.Ceq);
            yield return Instruction.Create(OpCodes.Brfalse, nextCheck);
            yield return Instruction.Create(OpCodes.Ldc_I4, selectorValue);
            yield return Instruction.Create(OpCodes.Stloc, selectorLocal);
            yield return Instruction.Create(OpCodes.Br, switchDispatch);
            yield return nextCheck;
        }

        private static IEnumerable<Instruction> BuildBlockInstructions(Block block,
            IReadOnlyDictionary<int, Block> blockMap,
            IReadOnlyDictionary<int, int> stateMap,
            CflowTransformationMode mode,
            MethodDef meth,
            ModuleDef module,
            IReadOnlyDictionary<int, IReadOnlyList<Local>> stackSpillLocals,
            Local stateLocal,
            Local guardLocal,
            Instruction dispatcher,
            Local payloadScratchLocal,
            Local payloadAuxLocal,
            Local payloadFlagLocal,
            int stateKey,
            int guardKey,
            StateEncodingMode stateMode,
            StateEncodingMode guardMode)
        {
            switch (block.ExitKind)
            {
                case BlockExitKind.Fallthrough:
                    foreach (var instruction in ObfuscatePayloadInstructions(module,
                        mode,
                        meth,
                        block.Instructions,
                        payloadScratchLocal,
                        payloadAuxLocal,
                        payloadFlagLocal))
                        yield return instruction;

                    foreach (var instruction in SpillEvaluationStack(stackSpillLocals[block.PrimaryTargetId!.Value]))
                        yield return instruction;
                    foreach (var instruction in StoreRoute(stateMap[block.PrimaryTargetId!.Value],
                        blockMap[block.PrimaryTargetId.Value].GuardState,
                        stateLocal,
                        guardLocal,
                        stateKey,
                        guardKey,
                        stateMode,
                        guardMode))
                        yield return instruction;
                    foreach (var instruction in BuildDispatcherBranch(dispatcher, mode))
                        yield return instruction;
                    yield break;

                case BlockExitKind.UnconditionalBranch:
                    foreach (var instruction in ObfuscatePayloadInstructions(module,
                        mode,
                        meth,
                        block.Instructions.Take(block.Instructions.Count - 1),
                        payloadScratchLocal,
                        payloadAuxLocal,
                        payloadFlagLocal))
                        yield return instruction;

                    foreach (var instruction in SpillEvaluationStack(stackSpillLocals[block.PrimaryTargetId!.Value]))
                        yield return instruction;
                    foreach (var instruction in StoreRoute(stateMap[block.PrimaryTargetId!.Value],
                        blockMap[block.PrimaryTargetId.Value].GuardState,
                        stateLocal,
                        guardLocal,
                        stateKey,
                        guardKey,
                        stateMode,
                        guardMode))
                        yield return instruction;
                    foreach (var instruction in BuildDispatcherBranch(dispatcher, mode))
                        yield return instruction;
                    yield break;

                case BlockExitKind.ConditionalBranch:
                    foreach (var instruction in ObfuscatePayloadInstructions(module,
                        mode,
                        meth,
                        block.Instructions.Take(block.Instructions.Count - 1),
                        payloadScratchLocal,
                        payloadAuxLocal,
                        payloadFlagLocal))
                        yield return instruction;

                    var branchTarget = Instruction.Create(OpCodes.Nop);
                    yield return Instruction.Create(block.Terminator!.OpCode, branchTarget);

                    foreach (var instruction in SpillEvaluationStack(stackSpillLocals[block.SecondaryTargetId!.Value]))
                        yield return instruction;
                    foreach (var instruction in StoreRoute(stateMap[block.SecondaryTargetId!.Value],
                        blockMap[block.SecondaryTargetId.Value].GuardState,
                        stateLocal,
                        guardLocal,
                        stateKey,
                        guardKey,
                        stateMode,
                        guardMode))
                        yield return instruction;
                    foreach (var instruction in BuildDispatcherBranch(dispatcher, mode))
                        yield return instruction;
                    yield return branchTarget;

                    foreach (var instruction in SpillEvaluationStack(stackSpillLocals[block.PrimaryTargetId!.Value]))
                        yield return instruction;
                    foreach (var instruction in StoreRoute(stateMap[block.PrimaryTargetId!.Value],
                        blockMap[block.PrimaryTargetId.Value].GuardState,
                        stateLocal,
                        guardLocal,
                        stateKey,
                        guardKey,
                        stateMode,
                        guardMode))
                        yield return instruction;
                    foreach (var instruction in BuildDispatcherBranch(dispatcher, mode))
                        yield return instruction;
                    yield break;

                case BlockExitKind.Return:
                case BlockExitKind.Throw:
                case BlockExitKind.Leave:
                case BlockExitKind.Endfinally:
                case BlockExitKind.Endfilter:
                case BlockExitKind.Rethrow:
                    foreach (var instruction in ObfuscatePayloadInstructions(module,
                        mode,
                        meth,
                        block.Instructions,
                        payloadScratchLocal,
                        payloadAuxLocal,
                        payloadFlagLocal))
                        yield return instruction;
                    yield break;

                default:
                    throw new InvalidOperationException($"Unsupported block exit kind: {block.ExitKind}");
            }
        }

        private static IEnumerable<Instruction> StoreRoute(int state,
            int guard,
            Local stateLocal,
            Local guardLocal,
            int stateKey,
            int guardKey,
            StateEncodingMode stateMode,
            StateEncodingMode guardMode)
        {
            foreach (var instruction in StoreState(state, stateLocal, stateKey, stateMode))
                yield return instruction;

            foreach (var instruction in StoreState(guard, guardLocal, guardKey, guardMode))
                yield return instruction;
        }

        private static IEnumerable<Instruction> ObfuscatePayloadInstructions(ModuleDef module,
            CflowTransformationMode mode,
            MethodDef meth,
            IEnumerable<Instruction> instructions,
            Local payloadScratchLocal,
            Local payloadAuxLocal,
            Local payloadFlagLocal)
        {
            var instructionList = instructions.ToList();
            PayloadPatternKind? lastPattern = null;
            var siteIndex = 0;

            for (var index = 0; index < instructionList.Count; index++)
            {
                var instruction = instructionList[index];
                if (!instruction.IsLdcI4())
                {
                    yield return instruction;
                    continue;
                }

                if (mode == CflowTransformationMode.Fragile || mode == CflowTransformationMode.EHSafe)
                {
                    foreach (var rewrittenInstruction in BuildConservativeSensitivePayload(instruction.GetLdcI4Value()))
                        yield return rewrittenInstruction;

                    continue;
                }

                var context = ClassifyPayloadConstantContext(module, meth, instructionList, index);
                if (context != PayloadConstantContext.NormalInt)
                {
                    foreach (var rewrittenInstruction in BuildConservativeSensitivePayload(instruction.GetLdcI4Value()))
                        yield return rewrittenInstruction;

                    continue;
                }

                var pattern = ChoosePayloadPattern(lastPattern, siteIndex++, mode);
                lastPattern = pattern;

                foreach (var rewrittenInstruction in BuildOpaqueIntPayload(module,
                    instruction.GetLdcI4Value(),
                    payloadScratchLocal,
                    payloadAuxLocal,
                    payloadFlagLocal,
                    pattern))
                {
                    yield return rewrittenInstruction;
                }
            }
        }

        private static PayloadPatternKind ChoosePayloadPattern(PayloadPatternKind? lastPattern,
            int siteIndex,
            CflowTransformationMode mode)
        {
            if (mode == CflowTransformationMode.Safe)
            {
                if (siteIndex % 3 == 2)
                    return PayloadPatternKind.NestedPredicate;

                return siteIndex % 2 == 0 ? PayloadPatternKind.ArithmeticBranch : PayloadPatternKind.ContradictoryState;
            }

            PayloadPatternKind pattern;
            var attempts = 0;
            do
            {
                var roll = Rnd.Next(0, 100);
                pattern = roll switch
                {
                    < 24 => PayloadPatternKind.ArithmeticBranch,
                    < 48 => PayloadPatternKind.ContradictoryState,
                    < 68 => PayloadPatternKind.BoolConfusion,
                    < 84 => PayloadPatternKind.FakeSwitch,
                    _ => PayloadPatternKind.NestedPredicate
                };

                if (siteIndex % 5 == 4 && pattern == PayloadPatternKind.ArithmeticBranch)
                    pattern = PayloadPatternKind.NestedPredicate;

                attempts++;
            }
            while (lastPattern.HasValue && pattern == lastPattern.Value && attempts < 4);

            return pattern;
        }

        private static IEnumerable<Instruction> BuildOpaqueIntPayload(ModuleDef module,
            int value,
            Local payloadScratchLocal,
            Local payloadAuxLocal,
            Local payloadFlagLocal,
            PayloadPatternKind pattern)
            => pattern switch
            {
                PayloadPatternKind.ArithmeticBranch => BuildArithmeticBranchPayload(module, value, payloadScratchLocal),
                PayloadPatternKind.ContradictoryState => BuildContradictoryStatePayload(module, value, payloadScratchLocal, payloadAuxLocal, payloadFlagLocal),
                PayloadPatternKind.BoolConfusion => BuildBoolConfusionPayload(value, payloadScratchLocal, payloadAuxLocal, payloadFlagLocal),
                PayloadPatternKind.FakeSwitch => BuildFakeSwitchPayload(value, payloadScratchLocal, payloadAuxLocal),
                PayloadPatternKind.NestedPredicate => BuildNestedPredicatePayload(module, value, payloadScratchLocal, payloadAuxLocal),
                _ => throw new InvalidOperationException($"Unsupported payload pattern: {pattern}")
            };

        private static PayloadConstantContext ClassifyPayloadConstantContext(ModuleDef module,
            MethodDef meth,
            IList<Instruction> instructions,
            int index)
        {
            if (index + 1 >= instructions.Count)
                return PayloadConstantContext.NormalInt;

            return ClassifyPayloadConstantContext(module, meth, instructions[index + 1]);
        }

        private static PayloadConstantContext ClassifyPayloadConstantContext(ModuleDef module,
            MethodDef meth,
            Instruction consumer)
            => consumer.OpCode.Code switch
            {
                Code.Brtrue => PayloadConstantContext.BooleanLike,
                Code.Brtrue_S => PayloadConstantContext.BooleanLike,
                Code.Brfalse => PayloadConstantContext.BooleanLike,
                Code.Brfalse_S => PayloadConstantContext.BooleanLike,
                Code.Ceq => PayloadConstantContext.BooleanLike,
                Code.Switch => PayloadConstantContext.EnumLike,
                Code.Beq => PayloadConstantContext.BooleanLike,
                Code.Beq_S => PayloadConstantContext.BooleanLike,
                Code.Bne_Un => PayloadConstantContext.BooleanLike,
                Code.Bne_Un_S => PayloadConstantContext.BooleanLike,
                Code.Bge => PayloadConstantContext.BooleanLike,
                Code.Bge_S => PayloadConstantContext.BooleanLike,
                Code.Bge_Un => PayloadConstantContext.BooleanLike,
                Code.Bge_Un_S => PayloadConstantContext.BooleanLike,
                Code.Bgt => PayloadConstantContext.BooleanLike,
                Code.Bgt_S => PayloadConstantContext.BooleanLike,
                Code.Bgt_Un => PayloadConstantContext.BooleanLike,
                Code.Bgt_Un_S => PayloadConstantContext.BooleanLike,
                Code.Ble => PayloadConstantContext.BooleanLike,
                Code.Ble_S => PayloadConstantContext.BooleanLike,
                Code.Ble_Un => PayloadConstantContext.BooleanLike,
                Code.Ble_Un_S => PayloadConstantContext.BooleanLike,
                Code.Blt => PayloadConstantContext.BooleanLike,
                Code.Blt_S => PayloadConstantContext.BooleanLike,
                Code.Blt_Un => PayloadConstantContext.BooleanLike,
                Code.Blt_Un_S => PayloadConstantContext.BooleanLike,
                Code.Call => ClassifyPayloadType(module, GetCallLikeConsumedType(consumer.Operand as IMethod)),
                Code.Callvirt => ClassifyPayloadType(module, GetCallLikeConsumedType(consumer.Operand as IMethod)),
                Code.Newobj => ClassifyPayloadType(module, GetCallLikeConsumedType(consumer.Operand as IMethod)),
                Code.Stloc => ClassifyPayloadType(module, GetLocalValueType(consumer.Operand as Local)),
                Code.Stloc_S => ClassifyPayloadType(module, GetLocalValueType(consumer.Operand as Local)),
                Code.Starg => ClassifyPayloadType(module, GetParameterValueType(consumer.Operand as Parameter)),
                Code.Starg_S => ClassifyPayloadType(module, GetParameterValueType(consumer.Operand as Parameter)),
                Code.Stfld => ClassifyPayloadType(module, GetFieldValueType(consumer.Operand as IField)),
                Code.Stsfld => ClassifyPayloadType(module, GetFieldValueType(consumer.Operand as IField)),
                Code.Ret => ClassifyPayloadType(module, meth.MethodSig?.RetType ?? meth.ReturnType),
                _ => PayloadConstantContext.NormalInt
            };

        private static PayloadConstantContext ClassifyPayloadType(ModuleDef module, TypeSig? type)
        {
            if (IsBooleanLikeTypeSig(type))
                return PayloadConstantContext.BooleanLike;

            if (IsEnumLikeTypeSig(module, type))
                return PayloadConstantContext.EnumLike;

            return PayloadConstantContext.NormalInt;
        }

        private static TypeSig? GetCallLikeConsumedType(IMethod? method)
        {
            var signature = method?.MethodSig;
            if (signature is null || signature.Params.Count == 0)
                return null;

            return signature.Params[signature.Params.Count - 1];
        }

        private static TypeSig? GetFieldValueType(IField? field)
            => field?.FieldSig?.Type;

        private static TypeSig? GetLocalValueType(Local? local)
            => local?.Type;

        private static TypeSig? GetParameterValueType(Parameter? parameter)
            => parameter?.Type;

        private static bool IsBooleanLikeTypeSig(TypeSig? type)
        {
            if (type is null)
                return false;

            return type.RemovePinnedAndModifiers().ElementType == ElementType.Boolean;
        }

        private static bool IsEnumLikeTypeSig(ModuleDef module, TypeSig? type)
        {
            if (module is null || type is null)
                return false;

            var next = type.RemovePinnedAndModifiers();
            if (next.ElementType != ElementType.ValueType)
                return false;

            if (next is not TypeDefOrRefSig typeDefOrRefSig)
                return false;

            var typeDef = typeDefOrRefSig.TypeDefOrRef.ResolveTypeDef();
            return string.Equals(typeDef?.BaseType?.FullName, "System.Enum", StringComparison.Ordinal);
        }

        private static IEnumerable<Instruction> BuildConservativeSensitivePayload(int value)
        {
            var mask = NextNonZeroInt();

            yield return Instruction.Create(OpCodes.Ldc_I4, value ^ mask);
            yield return Instruction.Create(OpCodes.Ldc_I4, mask);
            yield return Instruction.Create(OpCodes.Xor);
        }

        private static IEnumerable<Instruction> BuildOpaqueIntFour()
        {
            yield return Instruction.Create(OpCodes.Ldc_I4_6);
            yield return Instruction.Create(OpCodes.Ldc_I4_2);
            yield return Instruction.Create(OpCodes.Xor);
        }

        private static IEnumerable<Instruction> BuildArithmeticBranchPayload(ModuleDef module,
            int value,
            Local payloadScratchLocal)
        {
            var opaqueSeed = NextNonZeroInt();
            var opaqueMask = NextNonZeroInt();
            var opaqueValue = opaqueSeed ^ opaqueMask;
            var continueLabel = Instruction.Create(OpCodes.Nop);

            yield return Instruction.Create(OpCodes.Ldc_I4, value);
            foreach (var instruction in BuildOpaqueIntFour())
                yield return instruction;
            yield return Instruction.Create(OpCodes.Sub);
            yield return Instruction.Create(OpCodes.Ldc_I4, opaqueValue);
            yield return Instruction.Create(OpCodes.Ldc_I4, opaqueMask);
            yield return Instruction.Create(OpCodes.Xor);
            yield return Instruction.Create(OpCodes.Ldc_I4, opaqueSeed);
            yield return Instruction.Create(OpCodes.Bne_Un, continueLabel);
            yield return Instruction.Create(OpCodes.Ldc_I4, 2);
            yield return Instruction.Create(OpCodes.Stloc, payloadScratchLocal);
            foreach (var instruction in BuildOpaqueIntFour())
                yield return instruction;
            yield return Instruction.Create(OpCodes.Add);
            yield return continueLabel;
        }

        private static IEnumerable<Instruction> BuildContradictoryStatePayload(ModuleDef module,
            int value,
            Local payloadScratchLocal,
            Local payloadAuxLocal,
            Local payloadFlagLocal)
        {
            var bias = Rnd.Next(3, 19);
            var skipLabel = Instruction.Create(OpCodes.Nop);

            yield return Instruction.Create(OpCodes.Ldc_I4, unchecked(value + bias));
            yield return Instruction.Create(OpCodes.Stloc, payloadScratchLocal);
            yield return Instruction.Create(OpCodes.Ldloc, payloadScratchLocal);
            yield return Instruction.Create(OpCodes.Ldc_I4, bias);
            yield return Instruction.Create(OpCodes.Sub);
            yield return Instruction.Create(OpCodes.Stloc, payloadAuxLocal);
            yield return Instruction.Create(OpCodes.Ldloc, payloadScratchLocal);
            yield return Instruction.Create(OpCodes.Ldc_I4, unchecked(value + bias));
            yield return Instruction.Create(OpCodes.Ceq);
            yield return Instruction.Create(OpCodes.Stloc, payloadFlagLocal);
            yield return Instruction.Create(OpCodes.Ldloc, payloadFlagLocal);
            yield return Instruction.Create(OpCodes.Brfalse, skipLabel);
            yield return Instruction.Create(OpCodes.Ldloc, payloadAuxLocal);
            foreach (var instruction in BuildOpaqueIntFour())
                yield return instruction;
            yield return Instruction.Create(OpCodes.Add);
            yield return Instruction.Create(OpCodes.Stloc, payloadAuxLocal);
            yield return Instruction.Create(OpCodes.Ldloc, payloadAuxLocal);
            foreach (var instruction in BuildOpaqueIntFour())
                yield return instruction;
            yield return Instruction.Create(OpCodes.Sub);
            yield return Instruction.Create(OpCodes.Stloc, payloadAuxLocal);
            yield return skipLabel;
            yield return Instruction.Create(OpCodes.Ldloc, payloadAuxLocal);
        }

        private static IEnumerable<Instruction> BuildBoolConfusionPayload(int value,
            Local payloadScratchLocal,
            Local payloadAuxLocal,
            Local payloadFlagLocal)
        {
            var mask = NextNonZeroInt();
            var encodedValue = value ^ mask;
            var falseLabel = Instruction.Create(OpCodes.Nop);
            var endLabel = Instruction.Create(OpCodes.Nop);

            yield return Instruction.Create(OpCodes.Ldc_I4, encodedValue);
            yield return Instruction.Create(OpCodes.Stloc, payloadScratchLocal);
            yield return Instruction.Create(OpCodes.Ldloc, payloadScratchLocal);
            yield return Instruction.Create(OpCodes.Ldc_I4, mask);
            yield return Instruction.Create(OpCodes.Xor);
            yield return Instruction.Create(OpCodes.Stloc, payloadAuxLocal);
            yield return Instruction.Create(OpCodes.Ldloc, payloadAuxLocal);
            yield return Instruction.Create(OpCodes.Ldc_I4, value);
            yield return Instruction.Create(OpCodes.Ceq);
            yield return Instruction.Create(OpCodes.Ldc_I4_0);
            yield return Instruction.Create(OpCodes.Ceq);
            yield return Instruction.Create(OpCodes.Ldc_I4_0);
            yield return Instruction.Create(OpCodes.Ceq);
            yield return Instruction.Create(OpCodes.Stloc, payloadFlagLocal);
            yield return Instruction.Create(OpCodes.Ldloc, payloadFlagLocal);
            yield return Instruction.Create(OpCodes.Brfalse, falseLabel);
            yield return Instruction.Create(OpCodes.Ldloc, payloadAuxLocal);
            yield return Instruction.Create(OpCodes.Stloc, payloadScratchLocal);
            yield return Instruction.Create(OpCodes.Br, endLabel);
            yield return falseLabel;
            yield return Instruction.Create(OpCodes.Ldc_I4, value);
            yield return Instruction.Create(OpCodes.Stloc, payloadScratchLocal);
            yield return endLabel;
            yield return Instruction.Create(OpCodes.Ldloc, payloadScratchLocal);
        }

        private static IEnumerable<Instruction> BuildFakeSwitchPayload(int value,
            Local payloadScratchLocal,
            Local payloadAuxLocal)
        {
            var mask = NextNonZeroInt();
            var delta = Rnd.Next(2, 13);
            var firstPath = Instruction.Create(OpCodes.Nop);
            var secondPath = Instruction.Create(OpCodes.Nop);
            var deadPath = Instruction.Create(OpCodes.Nop);
            var endLabel = Instruction.Create(OpCodes.Nop);

            yield return Instruction.Create(OpCodes.Ldc_I4, unchecked(value + delta));
            yield return Instruction.Create(OpCodes.Stloc, payloadScratchLocal);
            yield return Instruction.Create(OpCodes.Ldloc, payloadScratchLocal);
            yield return Instruction.Create(OpCodes.Ldc_I4, unchecked(value + delta));
            yield return Instruction.Create(OpCodes.Beq, firstPath);
            yield return Instruction.Create(OpCodes.Br, deadPath);

            yield return firstPath;
            yield return Instruction.Create(OpCodes.Ldloc, payloadScratchLocal);
            yield return Instruction.Create(OpCodes.Ldc_I4, delta);
            yield return Instruction.Create(OpCodes.Sub);
            yield return Instruction.Create(OpCodes.Stloc, payloadAuxLocal);
            yield return Instruction.Create(OpCodes.Ldloc, payloadAuxLocal);
            yield return Instruction.Create(OpCodes.Ldc_I4, value);
            yield return Instruction.Create(OpCodes.Beq, secondPath);
            yield return Instruction.Create(OpCodes.Ldc_I4, value);
            yield return Instruction.Create(OpCodes.Stloc, payloadScratchLocal);
            yield return Instruction.Create(OpCodes.Br, endLabel);

            yield return secondPath;
            yield return Instruction.Create(OpCodes.Ldloc, payloadAuxLocal);
            yield return Instruction.Create(OpCodes.Stloc, payloadScratchLocal);
            yield return Instruction.Create(OpCodes.Br, endLabel);

            yield return deadPath;
            yield return Instruction.Create(OpCodes.Ldc_I4, value ^ mask);
            yield return Instruction.Create(OpCodes.Ldc_I4, mask);
            yield return Instruction.Create(OpCodes.Xor);
            yield return Instruction.Create(OpCodes.Stloc, payloadScratchLocal);
            yield return Instruction.Create(OpCodes.Br, endLabel);

            yield return endLabel;
            yield return Instruction.Create(OpCodes.Ldloc, payloadScratchLocal);
        }

        private static IEnumerable<Instruction> BuildNestedPredicatePayload(ModuleDef module,
            int value,
            Local payloadScratchLocal,
            Local payloadAuxLocal)
        {
            var bias = Rnd.Next(4, 15);
            var outerFalse = Instruction.Create(OpCodes.Nop);
            var innerFalse = Instruction.Create(OpCodes.Nop);
            var endLabel = Instruction.Create(OpCodes.Nop);

            yield return Instruction.Create(OpCodes.Ldc_I4, unchecked(value + bias));
            yield return Instruction.Create(OpCodes.Stloc, payloadScratchLocal);
            yield return Instruction.Create(OpCodes.Ldloc, payloadScratchLocal);
            yield return Instruction.Create(OpCodes.Ldc_I4, bias);
            yield return Instruction.Create(OpCodes.Sub);
            yield return Instruction.Create(OpCodes.Stloc, payloadAuxLocal);
            yield return Instruction.Create(OpCodes.Ldloc, payloadAuxLocal);
            yield return Instruction.Create(OpCodes.Ldc_I4, value);
            yield return Instruction.Create(OpCodes.Ceq);
            yield return Instruction.Create(OpCodes.Brfalse, outerFalse);
            yield return Instruction.Create(OpCodes.Ldloc, payloadScratchLocal);
            yield return Instruction.Create(OpCodes.Ldc_I4, unchecked(value + bias));
            yield return Instruction.Create(OpCodes.Ceq);
            yield return Instruction.Create(OpCodes.Brfalse, innerFalse);
            yield return Instruction.Create(OpCodes.Ldloc, payloadAuxLocal);
            yield return Instruction.Create(OpCodes.Stloc, payloadScratchLocal);
            yield return Instruction.Create(OpCodes.Br, endLabel);
            yield return innerFalse;
            yield return Instruction.Create(OpCodes.Ldloc, payloadAuxLocal);
            foreach (var instruction in BuildOpaqueIntFour())
                yield return instruction;
            yield return Instruction.Create(OpCodes.Add);
            yield return Instruction.Create(OpCodes.Stloc, payloadScratchLocal);
            yield return Instruction.Create(OpCodes.Ldloc, payloadScratchLocal);
            foreach (var instruction in BuildOpaqueIntFour())
                yield return instruction;
            yield return Instruction.Create(OpCodes.Sub);
            yield return Instruction.Create(OpCodes.Stloc, payloadScratchLocal);
            yield return Instruction.Create(OpCodes.Br, endLabel);
            yield return outerFalse;
            yield return Instruction.Create(OpCodes.Ldc_I4, value);
            yield return Instruction.Create(OpCodes.Stloc, payloadScratchLocal);
            yield return endLabel;
            yield return Instruction.Create(OpCodes.Ldloc, payloadScratchLocal);
        }

        private static IEnumerable<Instruction> SpillEvaluationStack(IReadOnlyList<Local> spillLocals)
        {
            for (var index = spillLocals.Count - 1; index >= 0; index--)
                yield return Instruction.Create(OpCodes.Stloc, spillLocals[index]);
        }

        private static IEnumerable<Instruction> RestoreEvaluationStack(IReadOnlyList<Local> spillLocals)
        {
            for (var index = 0; index < spillLocals.Count; index++)
                yield return Instruction.Create(OpCodes.Ldloc, spillLocals[index]);
        }

        private static IEnumerable<Instruction> BuildDispatcherBranch(Instruction dispatcher, CflowTransformationMode mode)
        {
            if (mode == CflowTransformationMode.EHSafe)
            {
                yield return Instruction.Create(OpCodes.Br, dispatcher);
                yield break;
            }

            if (mode == CflowTransformationMode.Safe || mode == CflowTransformationMode.Fragile)
            {
                var directBranch = Instruction.Create(OpCodes.Nop);
                yield return Instruction.Create(OpCodes.Ldc_I4_1);
                yield return Instruction.Create(OpCodes.Brfalse, directBranch);
                yield return Instruction.Create(OpCodes.Br, dispatcher);
                yield return directBranch;
                yield return Instruction.Create(OpCodes.Br, dispatcher);
                yield break;
            }

            foreach (var instruction in BuildSyntheticSwitchBranch(dispatcher))
                yield return instruction;
        }

        private static IEnumerable<Instruction> BuildSyntheticSwitchBranch(Instruction dispatcher)
        {
            var caseCount = 4;
            var bias = Rnd.Next(5, 17);
            var realCase = Rnd.Next(0, caseCount);
            var finalTarget = Instruction.Create(OpCodes.Nop);
            var defaultLabel = Instruction.Create(OpCodes.Nop);
            var caseLabels = Enumerable.Range(0, caseCount)
                .Select(_ => Instruction.Create(OpCodes.Nop))
                .ToArray();

            yield return Instruction.Create(OpCodes.Ldc_I4, realCase + bias);
            yield return Instruction.Create(OpCodes.Ldc_I4, bias);
            yield return Instruction.Create(OpCodes.Sub);

            for (var index = 0; index < caseLabels.Length; index++)
            {
                yield return Instruction.Create(OpCodes.Dup);
                yield return Instruction.Create(OpCodes.Ldc_I4, index);
                yield return Instruction.Create(OpCodes.Beq, caseLabels[index]);
            }

            yield return Instruction.Create(OpCodes.Br, defaultLabel);

            for (var index = 0; index < caseLabels.Length; index++)
            {
                yield return caseLabels[index];
                yield return Instruction.Create(OpCodes.Pop);

                if (index != realCase)
                {
                    yield return Instruction.Create(OpCodes.Ldc_I4, index ^ bias);
                    yield return Instruction.Create(OpCodes.Pop);
                }

                yield return Instruction.Create(OpCodes.Br, finalTarget);
            }

            yield return defaultLabel;
            yield return Instruction.Create(OpCodes.Pop);
            yield return Instruction.Create(OpCodes.Br, finalTarget);

            yield return finalTarget;
            yield return Instruction.Create(OpCodes.Br, dispatcher);
        }

        private static IEnumerable<Instruction> StoreState(int rawState,
            Local stateLocal,
            int key,
            StateEncodingMode mode)
        {
            foreach (var instruction in LoadEncodedState(rawState, key, mode, NextNonZeroInt()))
                yield return instruction;

            yield return Instruction.Create(OpCodes.Stloc, stateLocal);
        }

        private static IEnumerable<Instruction> LoadEncodedState(int rawState,
            int key,
            StateEncodingMode mode,
            int noise)
        {
            yield return Instruction.Create(OpCodes.Ldc_I4, rawState);
            yield return Instruction.Create(OpCodes.Ldc_I4, noise);
            yield return Instruction.Create(OpCodes.Add);
            yield return Instruction.Create(OpCodes.Ldc_I4, noise);
            yield return Instruction.Create(OpCodes.Sub);
            yield return Instruction.Create(OpCodes.Ldc_I4, key);
            yield return Instruction.Create(mode == StateEncodingMode.Xor ? OpCodes.Xor : OpCodes.Add);
        }

        private static IEnumerable<Instruction> DecodeState(int key, StateEncodingMode mode, int noise)
        {
            yield return Instruction.Create(OpCodes.Ldc_I4, noise);
            yield return Instruction.Create(OpCodes.Xor);
            yield return Instruction.Create(OpCodes.Ldc_I4, noise);
            yield return Instruction.Create(OpCodes.Xor);
            yield return Instruction.Create(OpCodes.Ldc_I4, key);
            yield return Instruction.Create(mode == StateEncodingMode.Xor ? OpCodes.Xor : OpCodes.Sub);
        }

        private static void ValidateMethod(MethodDef meth, MethodCflowAnalysis analysis)
        {
            var body = meth.Body;
            body.UpdateInstructionOffsets();
            body.OptimizeBranches();
            body.OptimizeMacros();
            body.UpdateInstructionOffsets();
            var instructionCount = body.Instructions.Count;
            var conservativeMinStack = (ushort)Math.Min(ushort.MaxValue,
                Math.Max(Math.Max(8, instructionCount + 8), analysis.MaxObservedStack + 8));
            body.MaxStack = conservativeMinStack;
            body.KeepOldMaxStack = true;
        }
    }
}