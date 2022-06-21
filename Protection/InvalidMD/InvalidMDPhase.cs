using dnlib.DotNet;
using dnlib.DotNet.Emit;
using MindLated.Protection.Renamer;
using System;

namespace MindLated.Protection.InvalidMD
{
    internal class InvalidMDPhase
    {
        public static void Execute(AssemblyDef asm)
        {
            var module = asm.ManifestModule;
            module.Mvid = null;
            module.Name = RenamerPhase.GenerateString(RenamerPhase.RenameMode.Normal);
            asm.ManifestModule.Import(new FieldDefUser(RenamerPhase.GenerateString(RenamerPhase.RenameMode.Normal)));
            foreach (var typeDef in module.Types)
            {
                TypeDef typeDef2 = new TypeDefUser(RenamerPhase.GenerateString(RenamerPhase.RenameMode.Normal));
                typeDef2.Methods.Add(new MethodDefUser());
                typeDef2.NestedTypes.Add(new TypeDefUser(RenamerPhase.GenerateString(RenamerPhase.RenameMode.Normal)));
                MethodDef item = new MethodDefUser();
                typeDef2.Methods.Add(item);
                typeDef.NestedTypes.Add(typeDef2);
                typeDef.Events.Add(new EventDefUser());
                foreach (var meth in typeDef.Methods)
                {
                    if (meth.Body == null) continue;
                    meth.Body.SimplifyBranches();
                    if (string.Compare(meth.ReturnType.FullName, "System.Void", StringComparison.Ordinal) != 0 || !meth.HasBody ||
                        meth.Body.Instructions.Count == 0) continue;
                    var typeSig = asm.ManifestModule.Import(typeof(int)).ToTypeSig();
                    var local = new Local(typeSig);
                    var typeSig2 = asm.ManifestModule.Import(typeof(bool)).ToTypeSig();
                    var local2 = new Local(typeSig2);
                    meth.Body.Variables.Add(local);
                    meth.Body.Variables.Add(local2);
                    var operand = meth.Body.Instructions[^1];
                    var instruction = new Instruction(OpCodes.Ret);
                    var instruction2 = new Instruction(OpCodes.Ldc_I4_1);
                    meth.Body.Instructions.Insert(0, new Instruction(OpCodes.Ldc_I4_0));
                    meth.Body.Instructions.Insert(1, new Instruction(OpCodes.Stloc, local));
                    meth.Body.Instructions.Insert(2, new Instruction(OpCodes.Br, instruction2));
                    var instruction3 = new Instruction(OpCodes.Ldloc, local);
                    meth.Body.Instructions.Insert(3, instruction3);
                    meth.Body.Instructions.Insert(4, new Instruction(OpCodes.Ldc_I4_0));
                    meth.Body.Instructions.Insert(5, new Instruction(OpCodes.Ceq));
                    meth.Body.Instructions.Insert(6, new Instruction(OpCodes.Ldc_I4_1));
                    meth.Body.Instructions.Insert(7, new Instruction(OpCodes.Ceq));
                    meth.Body.Instructions.Insert(8, new Instruction(OpCodes.Stloc, local2));
                    meth.Body.Instructions.Insert(9, new Instruction(OpCodes.Ldloc, local2));
                    meth.Body.Instructions.Insert(10, new Instruction(OpCodes.Brtrue, meth.Body.Instructions[10]));
                    meth.Body.Instructions.Insert(11, new Instruction(OpCodes.Ret));
                    meth.Body.Instructions.Insert(12, new Instruction(OpCodes.Calli));
                    meth.Body.Instructions.Insert(13, new Instruction(OpCodes.Sizeof, operand));
                    meth.Body.Instructions.Insert(meth.Body.Instructions.Count, instruction2);
                    meth.Body.Instructions.Insert(meth.Body.Instructions.Count, new Instruction(OpCodes.Stloc, local2));
                    meth.Body.Instructions.Insert(meth.Body.Instructions.Count, new Instruction(OpCodes.Br, instruction3));
                    meth.Body.Instructions.Insert(meth.Body.Instructions.Count, instruction);
                    var exceptionHandler = new ExceptionHandler(ExceptionHandlerType.Finally)
                    {
                        HandlerStart = meth.Body.Instructions[10],
                        HandlerEnd = meth.Body.Instructions[11],
                        TryEnd = meth.Body.Instructions[14],
                        TryStart = meth.Body.Instructions[12]
                    };
                    if (!meth.Body.HasExceptionHandlers)
                    {
                        meth.Body.ExceptionHandlers.Add(exceptionHandler);
                    }
                    meth.Body.OptimizeBranches();
                    meth.Body.OptimizeMacros();
                }
            }
            TypeDef typeDef3 = new TypeDefUser(RenamerPhase.GenerateString(RenamerPhase.RenameMode.Normal));
            FieldDef item2 = new FieldDefUser(RenamerPhase.GenerateString(RenamerPhase.RenameMode.Normal), new FieldSig(module.Import(typeof(MindLatedPng)).ToTypeSig()));
            typeDef3.Fields.Add(item2);
            typeDef3.BaseType = module.Import(typeof(MindLatedPng));
            module.Types.Add(typeDef3);
            TypeDef typeDef4 = new TypeDefUser(RenamerPhase.GenerateString(RenamerPhase.RenameMode.Normal))
            {
                IsInterface = true,
                IsSealed = true
            };
            module.Types.Add(typeDef4);
            module.TablesHeaderVersion = 257;
        }
    }
}