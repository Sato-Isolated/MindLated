namespace Isolated.Protection.Arithmetic
{
    public abstract class IArithmetic
    {
        public abstract string Name { get; }
        public abstract string Description { get; }

        public abstract void Init();
    }
}