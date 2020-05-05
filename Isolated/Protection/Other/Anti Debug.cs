using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Isolated.Services;
using System.Collections.Generic;
using System.Linq;

namespace Isolated.Protection.Other
{
    public static class Anti_Debug
    {
        public static void Execute(ModuleDef module)
        {
            ModuleDefMD typeModule = ModuleDefMD.Load(typeof(AntiDebugSafe).Module);
            MethodDef cctor = module.GlobalType.FindOrCreateStaticConstructor();
            TypeDef typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(AntiDebugSafe).MetadataToken));
            IEnumerable<IDnlibDef> members = InjectHelper.Inject(typeDef, module.GlobalType, module);
            var init = (MethodDef)members.Single(method => method.Name == "Initialize");
            cctor.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, init));
            foreach (MethodDef md in module.GlobalType.Methods)
            {
                if (md.Name == ".ctor")
                {
                    module.GlobalType.Remove(md);
                    break;
                }
            }
        }
    }
}