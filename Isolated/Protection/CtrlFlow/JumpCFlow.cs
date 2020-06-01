using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Isolated.Protection.CtrlFlow
{
    public static class JumpCFlow
    {
        public static bool Checking(MethodDef method)
        {
            for (var i = 1; i < method.Body.Instructions.Count - 1; i++)
            {
                if (method.Body.Instructions[i].IsLdcI4() && !method.Body.Instructions[i - 1].IsBr())
                {
                    return true;
                }
            }
            return false;
        }

        public static void Execute(ModuleDefMD module)
        {
            foreach (var type in module.Types)
            {
                foreach (var method in type.Methods.ToArray())
                {
                    if (!method.HasBody || !method.Body.HasInstructions || method.Body.HasExceptionHandlers) continue;
                    for (var i = 0; i < method.Body.Instructions.Count - 2; i++)
                    {
                        var inst = method.Body.Instructions[i + 1];
                        method.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Ldstr, "Isolated.jpg"));
                        method.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Br_S, inst));
                        i += 2;
                    }
                }
            }
        }
    }
}