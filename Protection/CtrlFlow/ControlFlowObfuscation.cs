using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
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

        public static void Execute(ModuleDefMD md)
        {
            FlattenedMethods.Clear();
            foreach (var type in md.Types)
            {
                if (type == md.GlobalType) continue;
                foreach (var meth in type.Methods)
                {
                    if (!CanTransformMethod(meth))
                        continue;

                    if (TryExecuteMethod(meth))
                        FlattenedMethods.Add(meth.MDToken.Raw);
                }
            }
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

        private static bool TryExecuteMethod(MethodDef meth)
        {
            var body = meth.Body;
            var originalInstructions = body.Instructions.ToList();
            var originalVariables = body.Variables.ToList();
            var originalInitLocals = body.InitLocals;

            try
            {
                body.SimplifyBranches();
                body.SimplifyMacros(meth.Parameters);

                var transformed = body.HasExceptionHandlers
                    ? TryExecuteExceptionRegions(meth)
                    : TryExecuteWholeMethod(meth);

                if (!transformed)
                    return RestoreOriginalBody();

                ValidateMethod(meth);
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

        private static bool TryExecuteWholeMethod(MethodDef meth)
        {
            LowerSwitchInstructions(meth, 0, meth.Body.Instructions.Count);

            if (!TryRewriteInstructionSlice(meth,
                meth.Body.Instructions.ToList(),
                Array.Empty<StackSlotProfile>(),
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

        private static bool TryExecuteExceptionRegions(MethodDef meth)
        {
            var transformedAnyRegion = false;
            foreach (var region in EnumerateExceptionRegions(meth))
            {
                if (!TryRewriteExceptionRegion(meth, region))
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

        private static bool TryCreateRegion(MethodDef meth,
            ExceptionRegionKind kind,
            Instruction? start,
            Instruction? end,
            IReadOnlyList<StackSlotProfile> initialStack,
            ISet<string> seen,
            out ExceptionRegion region)
        {
            region = null!;
            if (start is null || end is null)
                return false;

            var instructions = meth.Body.Instructions;
            var startIndex = instructions.IndexOf(start);
            var endIndex = instructions.IndexOf(end);
            if (startIndex < 0 || endIndex <= startIndex)
                return false;

            if (HasNestedEhBoundaryInside(meth, startIndex, endIndex))
                return false;

            var key = $"{kind}:{start.Offset}:{end.Offset}";
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

        private static IReadOnlyList<StackSlotProfile> CreateHandlerInitialStack(MethodDef meth, ExceptionHandler handler)
        {
            if (handler.HandlerType == ExceptionHandlerType.Catch)
            {
                var catchType = handler.CatchType?.ToTypeSig() ?? meth.Module.CorLibTypes.Object;
                return new[] { new StackSlotProfile(StackSlotKind.Object, catchType) };
            }

            return Array.Empty<StackSlotProfile>();
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

        private static bool TryRewriteExceptionRegion(MethodDef meth, ExceptionRegion region)
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
            var shuffledBlocks = Randomize(blocks);
            var decoyRoutes = BuildDecoyRoutes(blocks, relationKey, compareKey, compareSalt);
            rewrittenInstructions = BuildFlattenedInstructions(blocks,
                shuffledBlocks,
                decoyRoutes,
                meth.Module,
                stackSpillLocals,
                stateLocal,
                guardLocal,
                scratchLocal,
                selectorLocal,
                payloadScratchLocal,
                stateKey,
                guardKey,
                relationKey,
                compareKey,
                compareSalt,
                selectorBias,
                stateMode,
                guardMode);

            localsToAdd = new[] { stateLocal, guardLocal, scratchLocal, selectorLocal, payloadScratchLocal }
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
            ModuleDef module,
            IReadOnlyDictionary<int, IReadOnlyList<Local>> stackSpillLocals,
            Local stateLocal,
            Local guardLocal,
            Local scratchLocal,
            Local selectorLocal,
            Local payloadScratchLocal,
            int stateKey,
            int guardKey,
            int relationKey,
            int compareKey,
            int compareSalt,
            int selectorBias,
            StateEncodingMode stateMode,
            StateEncodingMode guardMode)
        {
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

            var extraJunkCount = Math.Max(3, routes.Count / 2);
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
                    module,
                    stackSpillLocals,
                    stateLocal,
                    guardLocal,
                    dispatcher,
                    payloadScratchLocal,
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
            ModuleDef module,
            IReadOnlyDictionary<int, IReadOnlyList<Local>> stackSpillLocals,
            Local stateLocal,
            Local guardLocal,
            Instruction dispatcher,
            Local payloadScratchLocal,
            int stateKey,
            int guardKey,
            StateEncodingMode stateMode,
            StateEncodingMode guardMode)
        {
            switch (block.ExitKind)
            {
                case BlockExitKind.Fallthrough:
                    foreach (var instruction in ObfuscatePayloadInstructions(module,
                        block.Instructions,
                        payloadScratchLocal))
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
                    foreach (var instruction in BuildSyntheticSwitchBranch(dispatcher))
                        yield return instruction;
                    yield break;

                case BlockExitKind.UnconditionalBranch:
                    foreach (var instruction in ObfuscatePayloadInstructions(module,
                        block.Instructions.Take(block.Instructions.Count - 1),
                        payloadScratchLocal))
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
                    foreach (var instruction in BuildSyntheticSwitchBranch(dispatcher))
                        yield return instruction;
                    yield break;

                case BlockExitKind.ConditionalBranch:
                    foreach (var instruction in ObfuscatePayloadInstructions(module,
                        block.Instructions.Take(block.Instructions.Count - 1),
                        payloadScratchLocal))
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
                    foreach (var instruction in BuildSyntheticSwitchBranch(dispatcher))
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
                    foreach (var instruction in BuildSyntheticSwitchBranch(dispatcher))
                        yield return instruction;
                    yield break;

                case BlockExitKind.Return:
                case BlockExitKind.Throw:
                case BlockExitKind.Leave:
                case BlockExitKind.Endfinally:
                case BlockExitKind.Endfilter:
                case BlockExitKind.Rethrow:
                    foreach (var instruction in ObfuscatePayloadInstructions(module,
                        block.Instructions,
                        payloadScratchLocal))
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
            IEnumerable<Instruction> instructions,
            Local payloadScratchLocal)
        {
            foreach (var instruction in instructions)
            {
                if (!instruction.IsLdcI4())
                {
                    yield return instruction;
                    continue;
                }

                foreach (var rewrittenInstruction in BuildOpaqueIntPayload(module,
                    instruction.GetLdcI4Value(),
                    payloadScratchLocal))
                {
                    yield return rewrittenInstruction;
                }
            }
        }

        private static IEnumerable<Instruction> BuildOpaqueIntPayload(ModuleDef module,
            int value,
            Local payloadScratchLocal)
        {
            var opaqueSeed = NextNonZeroInt();
            var opaqueMask = NextNonZeroInt();
            var opaqueValue = opaqueSeed ^ opaqueMask;
            var continueLabel = Instruction.Create(OpCodes.Nop);

            yield return Instruction.Create(OpCodes.Ldc_I4, unchecked(value - sizeof(float)));
            yield return Instruction.Create(OpCodes.Ldc_I4, opaqueValue);
            yield return Instruction.Create(OpCodes.Ldc_I4, opaqueMask);
            yield return Instruction.Create(OpCodes.Xor);
            yield return Instruction.Create(OpCodes.Ldc_I4, opaqueSeed);
            yield return Instruction.Create(OpCodes.Bne_Un, continueLabel);
            yield return Instruction.Create(OpCodes.Ldc_I4, 2);
            yield return Instruction.Create(OpCodes.Stloc, payloadScratchLocal);
            yield return Instruction.Create(OpCodes.Sizeof, module.Import(typeof(float)));
            yield return Instruction.Create(OpCodes.Add);
            yield return continueLabel;
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

        private static IEnumerable<Instruction> BuildSyntheticSwitchBranch(Instruction dispatcher)
        {
            var caseCount = 4;
            var bias = Rnd.Next(5, 17);
            var realCase = Rnd.Next(0, caseCount);
            var finalTarget = Instruction.Create(OpCodes.Nop);
            var junkLabels = Enumerable.Range(0, caseCount)
                .Select(_ => Instruction.Create(OpCodes.Nop))
                .ToArray();
            var switchTargets = junkLabels.ToArray();
            switchTargets[realCase] = finalTarget;

            yield return Instruction.Create(OpCodes.Ldc_I4, realCase + bias);
            yield return Instruction.Create(OpCodes.Ldc_I4, bias);
            yield return Instruction.Create(OpCodes.Sub);
            yield return Instruction.Create(OpCodes.Switch, switchTargets);
            yield return Instruction.Create(OpCodes.Br, finalTarget);

            for (var index = 0; index < junkLabels.Length; index++)
            {
                if (index == realCase)
                    continue;

                yield return junkLabels[index];
                yield return Instruction.Create(OpCodes.Ldc_I4, index ^ bias);
                yield return Instruction.Create(OpCodes.Pop);
                yield return Instruction.Create(OpCodes.Br, finalTarget);
            }

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

        private static void ValidateMethod(MethodDef meth)
        {
            var body = meth.Body;
            body.UpdateInstructionOffsets();
            body.OptimizeBranches();
            body.OptimizeMacros();
            body.UpdateInstructionOffsets();
        }
    }
}