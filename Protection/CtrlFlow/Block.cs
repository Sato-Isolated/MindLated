using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;

namespace MindLated.Protection.CtrlFlow
{
    public enum StackSlotKind
    {
        Int32,
        Int64,
        NativeInt,
        Float,
        Object,
        ManagedPointer,
        ValueType,
        Unknown
    }

    public enum BlockExitKind
    {
        Fallthrough,
        UnconditionalBranch,
        ConditionalBranch,
        Return,
        Throw,
        Leave,
        Endfinally,
        Endfilter,
        Rethrow
    }

    public sealed class StackSlotProfile
    {
        public StackSlotProfile(StackSlotKind kind, TypeSig spillType, bool isNullReference = false)
        {
            Kind = kind;
            SpillType = spillType;
            IsNullReference = isNullReference;
        }

        public StackSlotKind Kind { get; }

        public TypeSig SpillType { get; }

        public bool IsNullReference { get; }
    }

    public class Block
    {
        public Block()
        {
            Instructions = new List<Instruction>();
        }

        public List<Instruction> Instructions { get; set; }

        public int Id { get; set; }

        public int EntryState { get; set; }

        public int EntryStackDepth { get; set; }

        public int ExitStackDepth { get; set; }

        public IReadOnlyList<StackSlotProfile> EntryStackProfile { get; set; } = new List<StackSlotProfile>();

        public IReadOnlyList<StackSlotProfile> ExitStackProfile { get; set; } = new List<StackSlotProfile>();

        public int GuardState { get; set; }

        public int PredicateSeed { get; set; }

        public int DispatchSignature { get; set; }

        public int PhysicalOrder { get; set; }

        public bool IsEntry { get; set; }

        public Instruction? Terminator { get; set; }

        public BlockExitKind ExitKind { get; set; }

        public int? PrimaryTargetId { get; set; }

        public int? SecondaryTargetId { get; set; }
    }
}