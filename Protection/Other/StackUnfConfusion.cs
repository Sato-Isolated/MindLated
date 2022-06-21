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
                foreach (var meth in type.Methods)
                {
                    if (meth is { HasBody: false })
                    {
                        break;
                    }

                    var body = meth?.Body;
                    var target = body?.Instructions[0];
                    var item = Instruction.Create(OpCodes.Br_S, target);
                    var instruction3 = Instruction.Create(OpCodes.Pop);
                    var random = new Random();
                    var instruction4 = random.Next(0, 5) switch
                    {
                        0 => Instruction.Create(OpCodes.Ldnull),
                        1 => Instruction.Create(OpCodes.Ldc_I4_0),
                        2 => Instruction.Create(OpCodes.Ldstr, "Isolator"),
                        3 => Instruction.Create(OpCodes.Ldc_I8, (uint)random.Next()),
                        _ => Instruction.Create(OpCodes.Ldc_I8, (long)random.Next())
                    };

                    body?.Instructions.Insert(0, instruction4);
                    body?.Instructions.Insert(1, instruction3);
                    body?.Instructions.Insert(2, item);
                    if (body != null)
                    {
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
}