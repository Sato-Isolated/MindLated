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
            var typeModule = ModuleDefMD.Load(typeof(AntiDebugSafe).Module);
            var cctor = module.GlobalType.FindOrCreateStaticConstructor();
            var typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(AntiDebugSafe).MetadataToken));
            var members = InjectHelper.Inject(typeDef, module.GlobalType, module);
            var init = (MethodDef)members.Single(method => method.Name == "Initialize");
            cctor.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, init));
            foreach (var md in module.GlobalType.Methods)
            {
                if (md.Name != ".ctor") continue;
                module.GlobalType.Remove(md);
                break;
            }
        }
    }
}