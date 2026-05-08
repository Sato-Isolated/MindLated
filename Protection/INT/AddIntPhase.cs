using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;

namespace MindLated.Protection.INT
{
    public static class AddIntPhase
    {
        private static readonly Random Random = new();

        public static void Execute2(ModuleDef module)
        {
            foreach (var type in module.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody) continue;

                    var body = method.Body;
                    var instructions = body.Instructions;
                    for (var i = 0; i < instructions.Count; i++)
                    {
                        var instr = instructions[i];
                        if (!instr.IsLdcI4()) continue;

                        var originalValue = instr.GetLdcI4Value();
                        i += ApplyObfuscationPattern(body, i, module, originalValue);
                    }

                    body.SimplifyBranches();
                }
            }
        }

        private static int ApplyObfuscationPattern(CilBody body, int index, ModuleDef module, int originalValue)
        {
            return Random.Next(5) switch
            {
                0 => InsertXorNoise(body, index, originalValue),
                1 => InsertAddSubLocalNoise(body, index, module, originalValue),
                2 => InsertDoubleNegBranchNoise(body, index, originalValue),
                3 => InsertSizeofNoise(body, index, module, originalValue),
                _ => InsertOrAndNoise(body, index, originalValue)
            };
        }

        private static int InsertXorNoise(CilBody body, int index, int originalValue)
        {
            var instructions = body.Instructions;
            var noise = GetNonZeroRandom();
            var target = Instruction.Create(OpCodes.Nop);

            instructions[index].OpCode = OpCodes.Ldc_I4;
            instructions[index].Operand = originalValue ^ noise;

            instructions.Insert(index + 1, Instruction.Create(OpCodes.Ldc_I4, noise));
            instructions.Insert(index + 2, Instruction.Create(OpCodes.Xor));
            instructions.Insert(index + 3, Instruction.Create(OpCodes.Ldc_I4_0));
            instructions.Insert(index + 4, Instruction.Create(OpCodes.Brfalse_S, target));
            instructions.Insert(index + 5, Instruction.Create(OpCodes.Nop));
            instructions.Insert(index + 6, target);

            return 6;
        }

        private static int InsertAddSubLocalNoise(CilBody body, int index, ModuleDef module, int originalValue)
        {
            var instructions = body.Instructions;
            var offset = GetRandomSmallInt();
            var local = AddLocal(body, module);
            var target = Instruction.Create(OpCodes.Nop);

            instructions[index].OpCode = OpCodes.Ldc_I4;
            instructions[index].Operand = originalValue + offset;

            instructions.Insert(index + 1, OpCodes.Stloc.ToInstruction(local));
            instructions.Insert(index + 2, OpCodes.Ldloc.ToInstruction(local));
            instructions.Insert(index + 3, Instruction.Create(OpCodes.Ldc_I4, offset));
            instructions.Insert(index + 4, Instruction.Create(OpCodes.Sub));
            instructions.Insert(index + 5, Instruction.Create(OpCodes.Ldc_I4, GetRandomInt()));
            instructions.Insert(index + 6, Instruction.Create(OpCodes.Ldc_I4, GetRandomInt()));
            instructions.Insert(index + 7, Instruction.Create(OpCodes.Ceq));
            instructions.Insert(index + 8, Instruction.Create(OpCodes.Brtrue_S, target));
            instructions.Insert(index + 9, Instruction.Create(OpCodes.Nop));
            instructions.Insert(index + 10, target);

            return 10;
        }

        private static int InsertDoubleNegBranchNoise(CilBody body, int index, int originalValue)
        {
            var instructions = body.Instructions;
            var target = Instruction.Create(OpCodes.Nop);

            instructions[index].OpCode = OpCodes.Ldc_I4;
            instructions[index].Operand = originalValue;

            instructions.Insert(index + 1, OpCodes.Neg.ToInstruction());
            instructions.Insert(index + 2, OpCodes.Neg.ToInstruction());
            instructions.Insert(index + 3, Instruction.Create(OpCodes.Ldc_I4, GetRandomInt()));
            instructions.Insert(index + 4, Instruction.Create(OpCodes.Ldc_I4, GetRandomInt()));
            instructions.Insert(index + 5, Instruction.Create(OpCodes.Ceq));
            instructions.Insert(index + 6, Instruction.Create(OpCodes.Brtrue_S, target));
            instructions.Insert(index + 7, Instruction.Create(OpCodes.Nop));
            instructions.Insert(index + 8, target);

            return 8;
        }

        private static int InsertSizeofNoise(CilBody body, int index, ModuleDef module, int originalValue)
        {
            var instructions = body.Instructions;
            var target = Instruction.Create(OpCodes.Nop);

            instructions[index].OpCode = OpCodes.Ldc_I4;
            instructions[index].Operand = originalValue;

            instructions.Insert(index + 1, Instruction.Create(OpCodes.Sizeof, module.Import(typeof(bool))));
            instructions.Insert(index + 2, Instruction.Create(OpCodes.Conv_I4));
            instructions.Insert(index + 3, Instruction.Create(OpCodes.Add));
            instructions.Insert(index + 4, Instruction.Create(OpCodes.Sizeof, module.Import(typeof(bool))));
            instructions.Insert(index + 5, Instruction.Create(OpCodes.Conv_I4));
            instructions.Insert(index + 6, Instruction.Create(OpCodes.Sub));
            instructions.Insert(index + 7, Instruction.Create(OpCodes.Ldc_I4_0));
            instructions.Insert(index + 8, Instruction.Create(OpCodes.Brfalse_S, target));
            instructions.Insert(index + 9, Instruction.Create(OpCodes.Nop));
            instructions.Insert(index + 10, target);

            return 10;
        }

        private static int InsertOrAndNoise(CilBody body, int index, int originalValue)
        {
            var instructions = body.Instructions;
            var target = Instruction.Create(OpCodes.Nop);

            instructions[index].OpCode = OpCodes.Ldc_I4;
            instructions[index].Operand = originalValue;

            instructions.Insert(index + 1, Instruction.Create(OpCodes.Ldc_I4_0));
            instructions.Insert(index + 2, Instruction.Create(OpCodes.Or));
            instructions.Insert(index + 3, Instruction.Create(OpCodes.Ldc_I4_M1));
            instructions.Insert(index + 4, Instruction.Create(OpCodes.And));
            instructions.Insert(index + 5, Instruction.Create(OpCodes.Ldc_I4, GetRandomInt()));
            instructions.Insert(index + 6, Instruction.Create(OpCodes.Ldc_I4, GetRandomInt()));
            instructions.Insert(index + 7, Instruction.Create(OpCodes.Ceq));
            instructions.Insert(index + 8, Instruction.Create(OpCodes.Brtrue_S, target));
            instructions.Insert(index + 9, Instruction.Create(OpCodes.Nop));
            instructions.Insert(index + 10, target);

            return 10;
        }

        private static Local AddLocal(CilBody body, ModuleDef module)
        {
            var local = new Local(module.CorLibTypes.Int32);
            body.Variables.Add(local);
            return local;
        }

        private static int GetNonZeroRandom()
        {
            lock (Random)
            {
                var value = Random.Next(int.MinValue, int.MaxValue);
                return value != 0 ? value : 1;
            }
        }

        private static int GetRandomInt()
        {
            lock (Random)
            {
                return Random.Next();
            }
        }

        private static int GetRandomSmallInt()
        {
            lock (Random)
            {
                return Random.Next(1, 32);
            }
        }
    }
}
