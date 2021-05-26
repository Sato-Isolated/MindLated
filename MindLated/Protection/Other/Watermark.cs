using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace MindLated.Protection.Other
{
    internal class Watermark
    {
        public static void Execute(ModuleDefMD md)
        {
            foreach (ModuleDefMD module in md.Assembly.Modules)
            {
                TypeRef attrRef = module.CorLibTypes.GetTypeRef("System", "Attribute");
                var attrType = new TypeDefUser("", "MindLated", attrRef);
                module.Types.Add(attrType);

                var ctor = new MethodDefUser(
                    ".ctor",
                    MethodSig.CreateInstance(module.CorLibTypes.Void, module.CorLibTypes.String),
                    MethodImplAttributes.Managed,
                    MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName)
                {
                    Body = new CilBody()
                };
                ctor.Body.MaxStack = 1;
                ctor.Body.Instructions.Add(OpCodes.Ldarg_0.ToInstruction());
                ctor.Body.Instructions.Add(OpCodes.Call.ToInstruction(new MemberRefUser(module, ".ctor", MethodSig.CreateInstance(module.CorLibTypes.Void), attrRef)));
                ctor.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
                attrType.Methods.Add(ctor);
            }
        }
    }
}