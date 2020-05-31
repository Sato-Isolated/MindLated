using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Linq;

namespace Isolated.Protection.CtrlFlow
{
    public static class JumpCFlow
    {
        public static bool Checking(MethodDef method)
        {
            for (int i = 1; i < method.Body.Instructions.Count - 1; i++)
            {
                if (method.Body.Instructions[i].IsLdcI4() && !method.Body.Instructions[i - 1].IsBr())
                {
                    return true;
                }
                else
                {
                    continue;
                }
            }
            return false;
        }

        public static void Execute(ModuleDefMD module)
        {
            foreach (TypeDef type in module.Types)
            {
                foreach (MethodDef method in type.Methods.ToArray())
                {
                    if (method.HasBody && method.Body.HasInstructions && !method.Body.HasExceptionHandlers)
                    {
                        for (int i = 0; i < method.Body.Instructions.Count - 2; i++)
                        {
                            Instruction inst = method.Body.Instructions[i + 1];
                            method.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Ldstr, "Isolated.jpg"));
                            method.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Br_S, inst));
                            i += 2;
                        }
                    }
                }
            }
        }
    }
}