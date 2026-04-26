using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MindLated.Protection.CtrlFlow
{
    public class BlockParser
    {
        private static readonly HashSet<Code> SupportedUnconditionalBranchCodes = new()
        {
            Code.Br,
            Code.Br_S
        };

        private static readonly HashSet<Code> SupportedConditionalBranchCodes = new()
        {
            Code.Brfalse,
            Code.Brfalse_S,
            Code.Brtrue,
            Code.Brtrue_S,
            Code.Beq,
            Code.Beq_S,
            Code.Bge,
            Code.Bge_S,
            Code.Bge_Un,
            Code.Bge_Un_S,
            Code.Bgt,
            Code.Bgt_S,
            Code.Bgt_Un,
            Code.Bgt_Un_S,
            Code.Ble,
            Code.Ble_S,
            Code.Ble_Un,
            Code.Ble_Un_S,
            Code.Blt,
            Code.Blt_S,
            Code.Blt_Un,
            Code.Blt_Un_S,
            Code.Bne_Un,
            Code.Bne_Un_S
        };

        private static readonly HashSet<Code> TerminalCodes = new()
        {
            Code.Ret,
            Code.Throw,
            Code.Leave,
            Code.Leave_S,
            Code.Endfilter,
            Code.Endfinally,
            Code.Rethrow
        };

        private static readonly HashSet<Code> UnsupportedCodes = new()
        {
            Code.Switch,
            Code.Jmp,
            Code.Localloc,
            Code.Calli
        };

        public static bool TryParseMethod(MethodDef meth, out List<Block> blocks)
        {
            if (!CanTransformBody(meth))
            {
                blocks = new List<Block>();
                return false;
            }

            return TryParseInstructions(meth, meth.Body.Instructions.ToList(), Array.Empty<StackSlotProfile>(), out blocks);
        }

        internal static bool TryParseInstructions(MethodDef meth,
            IReadOnlyList<Instruction> instructions,
            IReadOnlyList<StackSlotProfile> initialEntryStack,
            out List<Block> blocks)
        {
            blocks = new List<Block>();
            if (instructions.Count == 0)
                return false;

            var leaders = new HashSet<Instruction> { instructions[0] };
            for (var index = 0; index < instructions.Count; index++)
            {
                var instruction = instructions[index];
                if (!IsSupportedInstruction(instruction))
                    return false;

                if (instruction.Operand is Instruction target)
                {
                    leaders.Add(target);
                }
                else if (instruction.Operand is IList<Instruction> targets)
                {
                    foreach (var switchTarget in targets)
                        leaders.Add(switchTarget);
                }

                if (StartsNewBlockAfter(instruction) && index + 1 < instructions.Count)
                    leaders.Add(instructions[index + 1]);
            }

            Block? currentBlock = null;
            foreach (var instruction in instructions)
            {
                if (leaders.Contains(instruction))
                {
                    if (currentBlock is { Instructions.Count: > 0 })
                        blocks.Add(currentBlock);

                    currentBlock = new Block { Id = blocks.Count };
                }

                currentBlock ??= new Block { Id = blocks.Count };
                currentBlock.Instructions.Add(instruction);
            }

            if (currentBlock is { Instructions.Count: > 0 })
                blocks.Add(currentBlock);

            var blockMap = blocks.ToDictionary(block => block.Instructions[0], block => block.Id);
            for (var index = 0; index < blocks.Count; index++)
            {
                var block = blocks[index];
                block.IsEntry = index == 0;
                block.Terminator = block.Instructions[^1];

                if (!TryResolveExit(blocks, blockMap, index, block))
                {
                    blocks.Clear();
                    return false;
                }
            }

            if (!TryComputeStackProfiles(meth, blocks, initialEntryStack))
            {
                blocks.Clear();
                return false;
            }

            return true;
        }

        private static bool CanTransformBody(MethodDef meth)
            => meth.HasBody &&
               meth.Body.HasInstructions &&
               !meth.Body.HasExceptionHandlers;

        private static bool IsSupportedInstruction(Instruction instruction)
            => instruction.OpCode.OpCodeType != OpCodeType.Prefix &&
               !UnsupportedCodes.Contains(instruction.OpCode.Code);

        private static bool StartsNewBlockAfter(Instruction instruction)
            => IsSupportedUnconditionalBranch(instruction) ||
               IsSupportedConditionalBranch(instruction) ||
               IsTerminalInstruction(instruction);

        private static bool TryResolveExit(IReadOnlyList<Block> blocks,
            IReadOnlyDictionary<Instruction, int> blockMap,
            int blockIndex,
            Block block)
        {
            var terminator = block.Terminator!;
            if (IsSupportedUnconditionalBranch(terminator))
            {
                if (terminator.Operand is not Instruction target || !blockMap.TryGetValue(target, out var targetId))
                    return false;

                block.ExitKind = BlockExitKind.UnconditionalBranch;
                block.PrimaryTargetId = targetId;
                return true;
            }

            if (IsSupportedConditionalBranch(terminator))
            {
                if (terminator.Operand is not Instruction target || !blockMap.TryGetValue(target, out var trueTargetId))
                    return false;

                if (blockIndex + 1 >= blocks.Count)
                    return false;

                block.ExitKind = BlockExitKind.ConditionalBranch;
                block.PrimaryTargetId = trueTargetId;
                block.SecondaryTargetId = blocks[blockIndex + 1].Id;
                return true;
            }

            if (terminator.OpCode.Code == Code.Ret)
            {
                block.ExitKind = BlockExitKind.Return;
                return true;
            }

            if (terminator.OpCode.Code == Code.Throw)
            {
                block.ExitKind = BlockExitKind.Throw;
                return true;
            }

            if (terminator.OpCode.Code is Code.Leave or Code.Leave_S)
            {
                block.ExitKind = BlockExitKind.Leave;
                return true;
            }

            if (terminator.OpCode.Code == Code.Endfinally)
            {
                block.ExitKind = BlockExitKind.Endfinally;
                return true;
            }

            if (terminator.OpCode.Code == Code.Endfilter)
            {
                block.ExitKind = BlockExitKind.Endfilter;
                return true;
            }

            if (terminator.OpCode.Code == Code.Rethrow)
            {
                block.ExitKind = BlockExitKind.Rethrow;
                return true;
            }

            if (blockIndex + 1 >= blocks.Count)
                return false;

            block.ExitKind = BlockExitKind.Fallthrough;
            block.PrimaryTargetId = blocks[blockIndex + 1].Id;
            return true;
        }

        private static bool IsSupportedUnconditionalBranch(Instruction instruction)
            => SupportedUnconditionalBranchCodes.Contains(instruction.OpCode.Code);

        private static bool IsSupportedConditionalBranch(Instruction instruction)
            => SupportedConditionalBranchCodes.Contains(instruction.OpCode.Code);

        private static bool IsTerminalInstruction(Instruction instruction)
            => TerminalCodes.Contains(instruction.OpCode.Code);

        private static bool TryComputeStackProfiles(MethodDef method,
            IReadOnlyList<Block> blocks,
            IReadOnlyList<StackSlotProfile> initialEntryStack)
        {
            var blockById = blocks.ToDictionary(block => block.Id);
            var blockIndexById = blocks
                .Select((block, index) => new { block.Id, Index = index })
                .ToDictionary(x => x.Id, x => x.Index);
            var predecessors = blocks.ToDictionary(block => block.Id, _ => new List<int>());
            foreach (var block in blocks)
            {
                if (block.PrimaryTargetId is int primaryTargetId)
                    predecessors[primaryTargetId].Add(block.Id);
                if (block.SecondaryTargetId is int secondaryTargetId)
                    predecessors[secondaryTargetId].Add(block.Id);
            }

            var entryStates = blocks.ToDictionary(block => block.Id, _ => (List<StackSlotProfile>?)null);
            var exitStates = blocks.ToDictionary(block => block.Id, _ => (List<StackSlotProfile>?)null);
            var pending = new Queue<int>();
            var queued = new HashSet<int>();

            if (!SeedEntryState(blocks[0].Id, CloneProfile(initialEntryStack)))
                return false;

            for (var index = 1; index < blocks.Count; index++)
            {
                var block = blocks[index];
                var previousBlock = blocks[index - 1];
                var hasEarlierIncomingEdge = predecessors[block.Id]
                    .Any(predecessorId => blockIndexById[predecessorId] < index);

                if (!hasEarlierIncomingEdge &&
                    (previousBlock.ExitKind == BlockExitKind.UnconditionalBranch ||
                     previousBlock.ExitKind == BlockExitKind.Return ||
                     previousBlock.ExitKind == BlockExitKind.Throw ||
                     previousBlock.ExitKind == BlockExitKind.Leave ||
                     previousBlock.ExitKind == BlockExitKind.Endfinally ||
                     previousBlock.ExitKind == BlockExitKind.Endfilter ||
                     previousBlock.ExitKind == BlockExitKind.Rethrow))
                {
                    if (!SeedEntryState(block.Id, new List<StackSlotProfile>()))
                        return false;
                }
            }

            while (pending.Count > 0)
            {
                var blockId = pending.Dequeue();
                queued.Remove(blockId);

                var block = blockById[blockId];
                var currentState = CloneProfile(entryStates[blockId]!);
                foreach (var instruction in block.Instructions)
                {
                    if (!TrySimulateInstruction(method, instruction, currentState))
                        return false;
                }

                exitStates[blockId] = CloneProfile(currentState);

                foreach (var successorId in GetSuccessors(block))
                {
                    if (!TryMergeEntryState(successorId, currentState))
                        return false;
                }
            }

            foreach (var block in blocks)
            {
                if (entryStates[block.Id] is null || exitStates[block.Id] is null)
                    return false;

                block.EntryStackDepth = entryStates[block.Id]!.Count;
                block.ExitStackDepth = exitStates[block.Id]!.Count;
                block.EntryStackProfile = CloneProfile(entryStates[block.Id]!);
                block.ExitStackProfile = CloneProfile(exitStates[block.Id]!);
            }

            return true;

            bool SeedEntryState(int blockId, List<StackSlotProfile> state)
            {
                if (entryStates[blockId] is not null)
                    return TryMergeProfiles(entryStates[blockId]!, state, out _);

                entryStates[blockId] = CloneProfile(state);

                if (queued.Add(blockId))
                    pending.Enqueue(blockId);

                return true;
            }

            bool TryMergeEntryState(int blockId, List<StackSlotProfile> state)
            {
                if (entryStates[blockId] is null)
                {
                    entryStates[blockId] = CloneProfile(state);
                    if (queued.Add(blockId))
                        pending.Enqueue(blockId);
                    return true;
                }

                if (!TryMergeProfiles(entryStates[blockId]!, state, out var changed))
                    return false;

                if (changed && queued.Add(blockId))
                    pending.Enqueue(blockId);

                return true;
            }
        }

        private static List<StackSlotProfile> CloneProfile(IReadOnlyList<StackSlotProfile> profile)
            => profile.Select(slot => new StackSlotProfile(slot.Kind, slot.SpillType, slot.IsNullReference)).ToList();

        private static bool TryMergeProfiles(List<StackSlotProfile> left, IReadOnlyList<StackSlotProfile> right, out bool changed)
        {
            changed = false;

            if (left.Count != right.Count)
                return false;

            for (var index = 0; index < left.Count; index++)
            {
                if (!TryMergeSlot(left[index], right[index], out var merged))
                    return false;

                if (!AreSameSlot(left[index], merged))
                {
                    left[index] = merged;
                    changed = true;
                }
            }

            return true;
        }

        private static bool TryMergeSlot(StackSlotProfile left, StackSlotProfile right, out StackSlotProfile merged)
        {
            merged = left;
            if (left.Kind != right.Kind)
                return false;

            if (AreEquivalentTypes(left.SpillType, right.SpillType))
            {
                if (left.IsNullReference && !right.IsNullReference)
                    merged = right;
                return true;
            }

            if (left.Kind == StackSlotKind.Object)
            {
                if (left.IsNullReference && !right.IsNullReference)
                {
                    merged = right;
                    return true;
                }

                if (right.IsNullReference && !left.IsNullReference)
                    return true;
            }

            return false;
        }

        private static bool AreSameSlot(StackSlotProfile left, StackSlotProfile right)
            => left.Kind == right.Kind &&
               left.IsNullReference == right.IsNullReference &&
               AreEquivalentTypes(left.SpillType, right.SpillType);

        private static bool AreEquivalentTypes(TypeSig left, TypeSig right)
            => left.FullName == right.FullName;

        private static bool TrySimulateInstruction(MethodDef method, Instruction instruction, List<StackSlotProfile> state)
        {
            if (UsesManualStackSimulation(instruction.OpCode.Code))
                return TryApplyPreciseStackTransition(method, instruction, state);

            instruction.CalculateStackUsage(out var pushes, out var pops);
            if (pushes < 0 || pops < 0 || state.Count < pops)
                return false;

            if (!TryApplyPreciseStackTransition(method, instruction, state))
            {
                state.RemoveRange(state.Count - pops, pops);
                for (var index = 0; index < pushes; index++)
                    state.Add(CreateUnknownSlot(method.Module));
            }

            return true;
        }

        private static bool UsesManualStackSimulation(Code code)
            => code == Code.Ret ||
               code == Code.Throw ||
               code == Code.Leave ||
               code == Code.Leave_S ||
               code == Code.Endfinally ||
               code == Code.Endfilter ||
               code == Code.Rethrow;

        private static bool TryApplyPreciseStackTransition(MethodDef method, Instruction instruction, List<StackSlotProfile> state)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Nop:
                case Code.Break:
                case Code.Br:
                case Code.Br_S:
                case Code.Ret:
                case Code.Throw:
                case Code.Leave:
                case Code.Leave_S:
                case Code.Endfinally:
                case Code.Rethrow:
                    return true;

                case Code.Endfilter:
                    state.RemoveAt(state.Count - 1);
                    return true;

                case Code.Pop:
                    state.RemoveAt(state.Count - 1);
                    return true;

                case Code.Dup:
                    state.Add(state[^1]);
                    return true;

                case Code.Ldnull:
                    state.Add(new StackSlotProfile(StackSlotKind.Object, method.Module.CorLibTypes.Object, isNullReference: true));
                    return true;

                case Code.Ldstr:
                    state.Add(new StackSlotProfile(StackSlotKind.Object, method.Module.CorLibTypes.String));
                    return true;

                case Code.Ldc_I4_M1:
                case Code.Ldc_I4_0:
                case Code.Ldc_I4_1:
                case Code.Ldc_I4_2:
                case Code.Ldc_I4_3:
                case Code.Ldc_I4_4:
                case Code.Ldc_I4_5:
                case Code.Ldc_I4_6:
                case Code.Ldc_I4_7:
                case Code.Ldc_I4_8:
                case Code.Ldc_I4:
                case Code.Ldc_I4_S:
                    state.Add(CreateInt32Slot(method.Module));
                    return true;

                case Code.Ldc_I8:
                    state.Add(CreateInt64Slot(method.Module));
                    return true;

                case Code.Ldc_R4:
                case Code.Ldc_R8:
                    state.Add(CreateFloatSlot(method.Module));
                    return true;

                case Code.Ldloc:
                case Code.Ldloc_S:
                case Code.Ldloc_0:
                case Code.Ldloc_1:
                case Code.Ldloc_2:
                case Code.Ldloc_3:
                    state.Add(GetLocalStackProfile(method, instruction));
                    return true;

                case Code.Ldloca:
                case Code.Ldloca_S:
                    state.Add(CreateManagedPointerSlot(GetReferencedLocal(method, instruction).Type));
                    return true;

                case Code.Stloc:
                case Code.Stloc_S:
                case Code.Stloc_0:
                case Code.Stloc_1:
                case Code.Stloc_2:
                case Code.Stloc_3:
                    state.RemoveAt(state.Count - 1);
                    return true;

                case Code.Ldarg:
                case Code.Ldarg_S:
                case Code.Ldarg_0:
                case Code.Ldarg_1:
                case Code.Ldarg_2:
                case Code.Ldarg_3:
                    state.Add(GetArgumentStackProfile(method, instruction));
                    return true;

                case Code.Ldarga:
                case Code.Ldarga_S:
                    state.Add(CreateManagedPointerSlot(GetReferencedParameter(method, instruction).Type));
                    return true;

                case Code.Starg:
                case Code.Starg_S:
                    state.RemoveAt(state.Count - 1);
                    return true;

                case Code.Add:
                case Code.Add_Ovf:
                case Code.Add_Ovf_Un:
                case Code.Sub:
                case Code.Sub_Ovf:
                case Code.Sub_Ovf_Un:
                case Code.Mul:
                case Code.Mul_Ovf:
                case Code.Mul_Ovf_Un:
                case Code.Div:
                case Code.Div_Un:
                case Code.Rem:
                case Code.Rem_Un:
                case Code.And:
                case Code.Or:
                case Code.Xor:
                    return ApplyBinaryResult(method.Module, state);

                case Code.Shl:
                case Code.Shr:
                case Code.Shr_Un:
                    return ApplyShiftResult(state);

                case Code.Neg:
                case Code.Not:
                    return true;

                case Code.Ceq:
                case Code.Cgt:
                case Code.Cgt_Un:
                case Code.Clt:
                case Code.Clt_Un:
                    state.RemoveRange(state.Count - 2, 2);
                    state.Add(CreateInt32Slot(method.Module));
                    return true;

                case Code.Brfalse:
                case Code.Brfalse_S:
                case Code.Brtrue:
                case Code.Brtrue_S:
                    state.RemoveAt(state.Count - 1);
                    return true;

                case Code.Beq:
                case Code.Beq_S:
                case Code.Bge:
                case Code.Bge_S:
                case Code.Bge_Un:
                case Code.Bge_Un_S:
                case Code.Bgt:
                case Code.Bgt_S:
                case Code.Bgt_Un:
                case Code.Bgt_Un_S:
                case Code.Ble:
                case Code.Ble_S:
                case Code.Ble_Un:
                case Code.Ble_Un_S:
                case Code.Blt:
                case Code.Blt_S:
                case Code.Blt_Un:
                case Code.Blt_Un_S:
                case Code.Bne_Un:
                case Code.Bne_Un_S:
                    state.RemoveRange(state.Count - 2, 2);
                    return true;

                case Code.Box:
                    state.RemoveAt(state.Count - 1);
                    state.Add(CreateObjectSlot(method.Module));
                    return true;

                case Code.Unbox:
                case Code.Unbox_Any:
                    state.RemoveAt(state.Count - 1);
                    state.Add(CreateStackSlot(((ITypeDefOrRef)instruction.Operand).ToTypeSig(), method.Module));
                    return true;

                case Code.Newobj:
                    return ApplyNewobjTransition(method.Module, (IMethod)instruction.Operand, state);

                case Code.Call:
                    return ApplyCallLikeTransition(method.Module, ((IMethod)instruction.Operand).MethodSig, state, HasImplicitThis((IMethod)instruction.Operand), forceObjectResult: false);

                case Code.Callvirt:
                    return ApplyCallLikeTransition(method.Module, ((IMethod)instruction.Operand).MethodSig, state, true, forceObjectResult: false);

                default:
                    return false;
            }
        }

        private static bool ApplyBinaryResult(ModuleDef module, List<StackSlotProfile> state)
        {
            var right = state[^1];
            var left = state[^2];
            state.RemoveRange(state.Count - 2, 2);
            state.Add(MergeArithmeticKinds(module, left, right));
            return true;
        }

        private static bool ApplyShiftResult(List<StackSlotProfile> state)
        {
            var valueKind = state[^2];
            state.RemoveRange(state.Count - 2, 2);
            state.Add(valueKind);
            return true;
        }

        private static bool ApplyNewobjTransition(ModuleDef module, IMethod method, List<StackSlotProfile> state)
        {
            var signature = method.MethodSig;
            if (signature is null)
                return false;

            var popCount = signature.Params.Count;
            if (state.Count < popCount)
                return false;

            state.RemoveRange(state.Count - popCount, popCount);
            if (method.DeclaringType is null)
                return false;

            state.Add(CreateStackSlot(method.DeclaringType.ToTypeSig(), module));
            return true;
        }

        private static bool ApplyCallLikeTransition(ModuleDef module, MethodSig? signature, List<StackSlotProfile> state, bool hasInstance, bool forceObjectResult)
        {
            if (signature is null)
                return false;

            var popCount = signature.Params.Count + (hasInstance ? 1 : 0);
            if (state.Count < popCount)
                return false;

            state.RemoveRange(state.Count - popCount, popCount);
            if (signature.RetType is null || signature.RetType.ElementType == ElementType.Void)
                return true;

            state.Add(forceObjectResult ? CreateObjectSlot(module) : CreateStackSlot(signature.RetType, module));
            return true;
        }

        private static StackSlotProfile MergeArithmeticKinds(ModuleDef module, StackSlotProfile left, StackSlotProfile right)
        {
            if (left.Kind == right.Kind)
            {
                return left.Kind switch
                {
                    StackSlotKind.Int32 => CreateInt32Slot(module),
                    StackSlotKind.Int64 => CreateInt64Slot(module),
                    StackSlotKind.NativeInt => CreateNativeIntSlot(module),
                    StackSlotKind.Float => CreateFloatSlot(module),
                    _ => left
                };
            }

            if ((left.Kind == StackSlotKind.Int32 && right.Kind == StackSlotKind.NativeInt) ||
                (left.Kind == StackSlotKind.NativeInt && right.Kind == StackSlotKind.Int32))
            {
                return CreateNativeIntSlot(module);
            }

            if (left.Kind == StackSlotKind.Float || right.Kind == StackSlotKind.Float)
                return CreateFloatSlot(module);

            if (left.Kind == StackSlotKind.Int64 || right.Kind == StackSlotKind.Int64)
                return CreateInt64Slot(module);

            return CreateUnknownSlot(module);
        }

        private static Local GetReferencedLocal(MethodDef method, Instruction instruction)
        {
            return instruction.OpCode.Code switch
            {
                Code.Ldloc_0 or Code.Stloc_0 => method.Body.Variables[0],
                Code.Ldloc_1 or Code.Stloc_1 => method.Body.Variables[1],
                Code.Ldloc_2 or Code.Stloc_2 => method.Body.Variables[2],
                Code.Ldloc_3 or Code.Stloc_3 => method.Body.Variables[3],
                _ => (Local)instruction.Operand
            };
        }

        private static StackSlotProfile GetLocalStackProfile(MethodDef method, Instruction instruction)
            => CreateStackSlot(GetReferencedLocal(method, instruction).Type, method.Module);

        private static Parameter GetReferencedParameter(MethodDef method, Instruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Ldarg_0:
                    return method.Parameters[0];
                case Code.Ldarg_1:
                    return method.Parameters[1];
                case Code.Ldarg_2:
                    return method.Parameters[2];
                case Code.Ldarg_3:
                    return method.Parameters[3];
                default:
                    return (Parameter)instruction.Operand;
            }
        }

        private static StackSlotProfile GetArgumentStackProfile(MethodDef method, Instruction instruction)
            => CreateStackSlot(GetReferencedParameter(method, instruction).Type, method.Module);

        private static bool HasImplicitThis(IMethod method)
            => method.MethodSig?.HasThis == true;

        private static StackSlotProfile CreateStackSlot(TypeSig? type, ModuleDef module)
        {
            if (type is null)
                return CreateUnknownSlot(module);

            var next = type.RemovePinnedAndModifiers();
            return MapTypeToStackKind(next) switch
            {
                StackSlotKind.Int32 => CreateInt32Slot(module),
                StackSlotKind.Int64 => CreateInt64Slot(module),
                StackSlotKind.NativeInt => CreateNativeIntSlot(module),
                StackSlotKind.Float => CreateFloatSlot(module),
                StackSlotKind.ManagedPointer => new StackSlotProfile(StackSlotKind.ManagedPointer, next),
                StackSlotKind.ValueType => new StackSlotProfile(StackSlotKind.ValueType, next),
                StackSlotKind.Object => new StackSlotProfile(StackSlotKind.Object, NormalizeReferenceType(next, module)),
                _ => CreateUnknownSlot(module)
            };
        }

        private static TypeSig NormalizeReferenceType(TypeSig type, ModuleDef module)
            => type.ElementType switch
            {
                ElementType.String => module.CorLibTypes.String,
                ElementType.Object => module.CorLibTypes.Object,
                _ => type
            };

        private static StackSlotProfile CreateManagedPointerSlot(TypeSig targetType)
            => new(StackSlotKind.ManagedPointer, new ByRefSig(targetType));

        private static StackSlotProfile CreateInt32Slot(ModuleDef module)
            => new(StackSlotKind.Int32, module.CorLibTypes.Int32);

        private static StackSlotProfile CreateInt64Slot(ModuleDef module)
            => new(StackSlotKind.Int64, module.CorLibTypes.Int64);

        private static StackSlotProfile CreateNativeIntSlot(ModuleDef module)
            => new(StackSlotKind.NativeInt, module.ImportAsTypeSig(typeof(IntPtr)));

        private static StackSlotProfile CreateFloatSlot(ModuleDef module)
            => new(StackSlotKind.Float, module.CorLibTypes.Double);

        private static StackSlotProfile CreateObjectSlot(ModuleDef module)
            => new(StackSlotKind.Object, module.CorLibTypes.Object);

        private static StackSlotProfile CreateUnknownSlot(ModuleDef module)
            => new(StackSlotKind.Unknown, module.CorLibTypes.Object);

        private static StackSlotKind MapTypeToStackKind(TypeSig? type)
        {
            if (type is null)
                return StackSlotKind.Unknown;

            var next = type.RemovePinnedAndModifiers();
            switch (next.ElementType)
            {
                case ElementType.Boolean:
                case ElementType.Char:
                case ElementType.I1:
                case ElementType.U1:
                case ElementType.I2:
                case ElementType.U2:
                case ElementType.I4:
                case ElementType.U4:
                    return StackSlotKind.Int32;

                case ElementType.I8:
                case ElementType.U8:
                    return StackSlotKind.Int64;

                case ElementType.I:
                case ElementType.U:
                    return StackSlotKind.NativeInt;

                case ElementType.R4:
                case ElementType.R8:
                    return StackSlotKind.Float;

                case ElementType.ByRef:
                case ElementType.Ptr:
                case ElementType.FnPtr:
                    return StackSlotKind.ManagedPointer;

                case ElementType.ValueType:
                case ElementType.TypedByRef:
                    return StackSlotKind.ValueType;

                case ElementType.String:
                case ElementType.Object:
                case ElementType.Class:
                case ElementType.Array:
                case ElementType.SZArray:
                case ElementType.Var:
                case ElementType.MVar:
                    return StackSlotKind.Object;

                default:
                    return StackSlotKind.Unknown;
            }
        }

        private static IEnumerable<int> GetSuccessors(Block block)
        {
            if (block.PrimaryTargetId is int primaryTargetId)
                yield return primaryTargetId;
            if (block.SecondaryTargetId is int secondaryTargetId)
                yield return secondaryTargetId;
        }
    }
}