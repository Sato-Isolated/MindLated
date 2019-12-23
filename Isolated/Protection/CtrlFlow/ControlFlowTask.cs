using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Isolated.Services;
using Isolated.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Isolated.Protection.CtrlFlow
{
    public static class ControlFlowTask
    {
        private static ModuleDefMD loadedMod;
        private static Dictionary<MethodDef, Tuple<int[], int[]>> obfMethods;
        private static RandomGen rand = new RandomGen();

        public static void Execute(ModuleDefMD loadedMod)
        {
            obfMethods = CreateMethods(loadedMod);
            foreach (TypeDef type in loadedMod.Types)
            {
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody || !method.Body.HasInstructions || method.IsNative)
                        continue;
                    MethodDef methodd = method as MethodDef;
                    var body = method.Body;
                    body.SimplifyBranches();
                    body.MaxStack++;
                    List<Instruction> instructions = body.Instructions.ToList();
                    List<HBlock> Hblocks = new List<HBlock>();
                    List<HBlock> obfHBlocks = new List<HBlock>();
                    /*if (body.HasExceptionHandlers)
                    {
                        foreach (ExceptionHandler eh in body.ExceptionHandlers)
                        {
                            ExceptionHandlerType HType = eh.HandlerType;
                            List<Instruction> HInstr = new List<Instruction>();
                            int HTStart = Array.IndexOf(instructions.ToArray(), eh.TryStart);
                            int HTEnd = Array.IndexOf(instructions.ToArray(), eh.TryEnd);
                            if (eh.TryEnd == null) HTEnd = instructions.Count - 1;
                            for (int i = HTStart; i < HTEnd; i++)
                                HInstr.Add(instructions[i]);
                            Hblocks.Add(new HBlock() { instructions = HInstr });
                            HInstr.Clear();
                            int HCStart = Array.IndexOf(instructions.ToArray(), eh.HandlerStart);
                            int HCEnd = Array.IndexOf(instructions.ToArray(), eh.HandlerEnd);
                            if (eh.HandlerEnd == null) HCEnd = instructions.Count - 1;
                            for (int i = HCStart; i < HCEnd; i++)
                                HInstr.Add(instructions[i]);
                            Hblocks.Add(new HBlock() { instructions = HInstr });
                        }
                        foreach (HBlock Hblock in Hblocks)
                            obfHBlocks.Add(ObfuscateHBlock(Hblock, true));
                    }
                    else*/
                    obfHBlocks.Add(ObfuscateHBlock(new HBlock() { instructions = instructions }, false));
                    body.Instructions.Clear();
                    foreach (HBlock hBlock in obfHBlocks)
                    {
                        foreach (Instruction instr in hBlock.instructions)
                            body.Instructions.Add(instr);
                    }
                    body.UpdateInstructionOffsets();
                    body.SimplifyBranches();
                }
            }
        }

        public static HBlock ObfuscateHBlock(HBlock HB, bool isHBlock)
        {
            List<BBlock> bBlocks = new List<BBlock>();
            List<Instruction> instructions = HB.instructions;
            Instruction firstBr = Instruction.Create(OpCodes.Br, instructions[0]);
            BBlock mainBlock = new BBlock() { instructions = new List<Instruction>(), fakeBranches = new List<Instruction>(), branchOrRet = new List<Instruction>() };
            int stack = 0;
            int push, pop;
            for (int i = 0; i < instructions.Count; i++)
            {
                Instruction instr = instructions[i];
                instr.CalculateStackUsage(out push, out pop);
                stack += (push - pop);
                if (instr.OpCode == OpCodes.Ret)
                {
                    mainBlock.branchOrRet.Add(instr);
                    bBlocks.Add((BBlock)mainBlock.Clone());
                    mainBlock.Clear();
                }
                else
                    if (stack == 0 && instr.OpCode.OpCodeType != OpCodeType.Prefix)
                {
                    MethodDef obfMethod = obfMethods.Keys.ToArray()[rand.Next(0, 4)];
                    mainBlock.instructions.Add(instr);
                    if (rand.Next(0, 2) == 0)
                    {
                        mainBlock.branchOrRet.Add(Instruction.CreateLdcI4(obfMethods[obfMethod].Item2[rand.Next(0, 4)]));
                        mainBlock.branchOrRet.Add(Instruction.Create(OpCodes.Call, obfMethod));
                        mainBlock.branchOrRet.Add(Instruction.Create(OpCodes.Brfalse, instructions[i + 1]));
                    }
                    else
                    {
                        mainBlock.branchOrRet.Add(Instruction.CreateLdcI4(obfMethods[obfMethod].Item1[rand.Next(0, 4)]));
                        mainBlock.branchOrRet.Add(Instruction.Create(OpCodes.Call, obfMethod));
                        mainBlock.branchOrRet.Add(Instruction.Create(OpCodes.Brtrue, instructions[i + 1]));
                    }
                    bBlocks.Add((BBlock)mainBlock.Clone());
                    mainBlock.Clear();
                }
                else
                    mainBlock.instructions.Add(instr);
            }
            /*if (instructions.Count != bBlocks.Sum(a => a.instructions.Count) + 1)
            {
                throw new Exception("Did you delete any instruction?");
            }*/
            int[] position;
            bBlocks = GeneralUtils.Shuffle<BBlock>(bBlocks, out position);
            int index = Array.IndexOf(position, position.Length - 1);
            BBlock lastB = bBlocks[position.Length - 1];
            BBlock tempB;
            tempB = bBlocks[index];
            bBlocks[index] = lastB;
            bBlocks[position.Length - 1] = tempB;
            if (isHBlock)
            {
                int index2 = Array.IndexOf(position, 0);
                BBlock firstB = bBlocks[0];
                BBlock tempB2;
                tempB2 = bBlocks[index2];
                bBlocks[index2] = firstB;
                bBlocks[0] = tempB2;
            }
            foreach (BBlock block in bBlocks)
            {
                if (block.branchOrRet[0].OpCode != OpCodes.Ret)
                {
                    MethodDef obfMethod = obfMethods.Keys.ToArray()[rand.Next(0, 4)];
                    int rr = rand.Next(0, bBlocks.Count);
                    while (bBlocks[rr].instructions.Count == 0)
                        rr = rand.Next(0, bBlocks.Count);
                    if (rand.Next(0, 2) == 0)
                    {
                        block.fakeBranches.Add(Instruction.CreateLdcI4(obfMethods[obfMethod].Item1[rand.Next(0, 4)]));
                        block.fakeBranches.Add(Instruction.Create(OpCodes.Call, obfMethod));
                        block.fakeBranches.Add(Instruction.Create(OpCodes.Brfalse, bBlocks[rr].instructions[0]));
                    }
                    else
                    {
                        block.fakeBranches.Add(Instruction.CreateLdcI4(obfMethods[obfMethod].Item2[rand.Next(0, 4)]));
                        block.fakeBranches.Add(Instruction.Create(OpCodes.Call, obfMethod));
                        block.fakeBranches.Add(Instruction.Create(OpCodes.Brtrue, bBlocks[rr].instructions[0]));
                    }
                }
            }
            List<Instruction> bInstrs = new List<Instruction>();
            foreach (BBlock B in bBlocks)
            {
                bInstrs.AddRange(B.instructions);
                if (rand.Next(0, 2) == 0)
                {
                    if (B.branchOrRet.Count != 0) bInstrs.AddRange(B.branchOrRet);
                    if (B.fakeBranches.Count != 0) bInstrs.AddRange(B.fakeBranches);
                }
                else
                {
                    if (B.fakeBranches.Count != 0) bInstrs.AddRange(B.fakeBranches);
                    if (B.branchOrRet.Count != 0) bInstrs.AddRange(B.branchOrRet);
                }
                if (B.afterInstr != null)
                    bInstrs.Add(B.afterInstr);
            }
            if (!isHBlock)
                bInstrs.Insert(0, firstBr);
            return new HBlock() { instructions = bInstrs };
        }

        public static Dictionary<MethodDef, Tuple<int[], int[]>> CreateMethods(ModuleDef loadedMod)
        {
            DynamicCode code = new DynamicCode(3);
            int[] modules = new int[4];
            for (int i = 0; i < modules.Length; i++)
                modules[i] = rand.Next(2, 25);
            Instruction[,] methods = new Instruction[4, 10];
            for (int i = 0; i < 4; i++)
            {
                Instruction[] methodBody = code.Create();
                for (int y = 0; y < methodBody.Length; y++)
                    methods[i, y] = methodBody[y];
            }

            List<Tuple<Instruction[], Tuple<int, Tuple<int[], int[]>>>> InstrToInt =
                           new List<Tuple<Instruction[], Tuple<int, Tuple<int[], int[]>>>>();

            for (int i = 0; i < 4; i++)
            {
                List<Instruction> instr = new List<Instruction>();
                int[] numbersTrue = new int[5];
                int[] numbersFalse = new int[5];
                for (int y = 0; y < 10; y++)
                    instr.Add(methods[i, y]);
                for (int y = 0; y < 5; y++)
                    numbersTrue[y] = code.RandomNumberInModule(instr.ToArray(), modules[i], true);
                for (int y = 0; y < 5; y++)
                    numbersFalse[y] = code.RandomNumberInModule(instr.ToArray(), modules[i], false);
                InstrToInt.Add(Tuple.Create(instr.ToArray(), Tuple.Create(modules[i], Tuple.Create(numbersTrue, numbersFalse))));
            }
            Dictionary<MethodDef, Tuple<int[], int[]>> final = new Dictionary<MethodDef, Tuple<int[], int[]>>();
            MethodAttributes methFlags = MethodAttributes.Public | MethodAttributes.Static
                | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
            MethodImplAttributes methImplFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
            for (int i = 0; i < 4; i++)
            {
                MethodDef methodDefs1 = new MethodDefUser(
                                     "",
                                     MethodSig.CreateStatic(loadedMod.CorLibTypes.Boolean, loadedMod.CorLibTypes.Int32),
                                     methImplFlags, methFlags);
                RenameTask.Rename(methodDefs1);
                methodDefs1.Body = new CilBody();
                methodDefs1.ParamDefs.Add(new ParamDefUser("lol", 0));
                List<Instruction> preInstr = new List<Instruction>(InstrToInt[i].Item1);
                int module = InstrToInt[i].Item2.Item1;
                //preInstr.RemoveAt(preInstr.Count - 1);
                preInstr.Insert(preInstr.Count - 1, Instruction.CreateLdcI4(module));
                preInstr.Insert(preInstr.Count - 1, OpCodes.Rem.ToInstruction());
                preInstr.Insert(preInstr.Count - 1, Instruction.CreateLdcI4(0));
                preInstr.Insert(preInstr.Count - 1, Instruction.Create(OpCodes.Ceq));
                //preInstr.Insert(preInstr.Count - 1, Instruction.Create(OpCodes.Ret));
                foreach (var item in preInstr)
                    methodDefs1.Body.Instructions.Add(item);
                final.Add(methodDefs1, InstrToInt[i].Item2.Item2);
            }

            TypeDef type1 = new TypeDefUser("", "", loadedMod.CorLibTypes.Object.TypeDefOrRef);
            RenameTask.Rename(type1);
            type1.Attributes = dnlib.DotNet.TypeAttributes.Public | dnlib.DotNet.TypeAttributes.AutoLayout |
            dnlib.DotNet.TypeAttributes.Class | dnlib.DotNet.TypeAttributes.AnsiClass;
            loadedMod.Types.Add(type1);
            foreach (var item in final)
                type1.Methods.Add(item.Key);
            return final;
        }
    }

    public class HBlock : ICloneable
    {
        public List<Instruction> instructions;

        public void Clear()
        {
            instructions = new List<Instruction>();
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class BBlock : ICloneable
    {
        public List<Instruction> instructions;
        public List<Instruction> branchOrRet;
        public Instruction afterInstr;
        public List<Instruction> fakeBranches;

        public void Clear()
        {
            instructions = new List<Instruction>();
            branchOrRet = new List<Instruction>();
            afterInstr = null;
            fakeBranches = new List<Instruction>();
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}