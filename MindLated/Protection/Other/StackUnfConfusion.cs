using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;

namespace MindLated.Protection.Other
{
    internal class StackUnfConfusion
    {
        public static void Execute(ModuleDef mod)
        {
            foreach (var type in mod.Types)
            {
                foreach (var dnlibDef in type.Methods)
                {
                    var def = dnlibDef;
                    if (def != null && !def.HasBody)
                    {
                        break;
                    }

                    var body = def.Body;
                    var target = body.Instructions[0];
                    var item = Instruction.Create(OpCodes.Br_S, target);
                    var instruction3 = Instruction.Create(OpCodes.Pop);
                    var random = new Random();
                    Instruction instruction4;
                    switch (random.Next(0, 5))
                    {
                        case 0:
                            instruction4 = Instruction.Create(OpCodes.Ldnull);
                            break;

                        case 1:
                            instruction4 = Instruction.Create(OpCodes.Ldc_I4_0);
                            break;

                        case 2:
                            instruction4 = Instruction.Create(OpCodes.Ldstr, "Isolator");
                            break;

                        case 3:
                            instruction4 = Instruction.Create(OpCodes.Ldc_I8, (uint)random.Next());
                            break;

                        default:
                            instruction4 = Instruction.Create(OpCodes.Ldc_I8, (long)random.Next());
                            break;
                    }

                    body.Instructions.Insert(0, instruction4);
                    body.Instructions.Insert(1, instruction3);
                    body.Instructions.Insert(2, item);
                    foreach (var handler in body.ExceptionHandlers)
                    {
                        if (handler.TryStart == target)
                        {
                            handler.TryStart = item;
                        }
                        else if (handler.HandlerStart == target)
                        {
                            handler.HandlerStart = item;
                        }
                        else if (handler.FilterStart == target)
                        {
                            handler.FilterStart = item;
                        }
                    }
                }
            }
        }
    }
}