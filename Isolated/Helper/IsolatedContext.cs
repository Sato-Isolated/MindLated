using dnlib.DotNet;

namespace Isolated.Helper
{
    public class IsolatedContext
    {
        public AssemblyDef Assembly;
        public ModuleDef ManifestModule;
        public TypeDef GlobalType;
        public Importer Imp;
        public MethodDef cctor;

        public IsolatedContext(AssemblyDef asm)
        {
            this.Assembly = asm;
            this.ManifestModule = asm.ManifestModule;
            this.GlobalType = this.ManifestModule.GlobalType;
            this.Imp = new Importer(ManifestModule);
            this.cctor = this.GlobalType.FindOrCreateStaticConstructor();
        }
    }
}