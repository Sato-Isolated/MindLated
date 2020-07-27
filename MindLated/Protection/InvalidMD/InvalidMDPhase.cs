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
            var manifestModule = asm.ManifestModule;
            manifestModule.Mvid = null;
            manifestModule.Name = RenamerPhase.GenerateString(RenamerPhase.RenameMode.Normal);
            asm.ManifestModule.Import(new FieldDefUser(RenamerPhase.GenerateString(RenamerPhase.RenameMode.Normal)));
            foreach (var typeDef in manifestModule.Types)
            {
                TypeDef typeDef2 = new TypeDefUser(RenamerPhase.GenerateString(RenamerPhase.RenameMode.Normal));
                typeDef2.Methods.Add(new MethodDefUser());
                typeDef2.NestedTypes.Add(new TypeDefUser(RenamerPhase.GenerateString(RenamerPhase.RenameMode.Normal)));
                MethodDef item = new MethodDefUser();
                typeDef2.Methods.Add(item);
                typeDef.NestedTypes.Add(typeDef2);
                typeDef.Events.Add(new EventDefUser());
                foreach (var methodDef2 in typeDef.Methods)
                {
                    if (methodDef2.Body == null) continue;
                    methodDef2.Body.SimplifyBranches();
                    if (string.Compare(methodDef2.ReturnType.FullName, "System.Void", StringComparison.Ordinal) != 0 || !methodDef2.HasBody ||
                        methodDef2.Body.Instructions.Count == 0) continue;
                    var typeSig = asm.ManifestModule.Import(typeof(int)).ToTypeSig();
                    var local = new Local(typeSig);
                    var typeSig2 = asm.ManifestModule.Import(typeof(bool)).ToTypeSig();
                    var local2 = new Local(typeSig2);
                    methodDef2.Body.Variables.Add(local);
                    methodDef2.Body.Variables.Add(local2);
                    var operand = methodDef2.Body.Instructions[methodDef2.Body.Instructions.Count - 1];
                    var instruction = new Instruction(OpCodes.Ret);
                    var instruction2 = new Instruction(OpCodes.Ldc_I4_1);
                    methodDef2.Body.Instructions.Insert(0, new Instruction(OpCodes.Ldc_I4_0));
                    methodDef2.Body.Instructions.Insert(1, new Instruction(OpCodes.Stloc, local));
                    methodDef2.Body.Instructions.Insert(2, new Instruction(OpCodes.Br, instruction2));
                    var instruction3 = new Instruction(OpCodes.Ldloc, local);
                    methodDef2.Body.Instructions.Insert(3, instruction3);
                    methodDef2.Body.Instructions.Insert(4, new Instruction(OpCodes.Ldc_I4_0));
                    methodDef2.Body.Instructions.Insert(5, new Instruction(OpCodes.Ceq));
                    methodDef2.Body.Instructions.Insert(6, new Instruction(OpCodes.Ldc_I4_1));
                    methodDef2.Body.Instructions.Insert(7, new Instruction(OpCodes.Ceq));
                    methodDef2.Body.Instructions.Insert(8, new Instruction(OpCodes.Stloc, local2));
                    methodDef2.Body.Instructions.Insert(9, new Instruction(OpCodes.Ldloc, local2));
                    methodDef2.Body.Instructions.Insert(10, new Instruction(OpCodes.Brtrue, methodDef2.Body.Instructions[10]));
                    methodDef2.Body.Instructions.Insert(11, new Instruction(OpCodes.Ret));
                    methodDef2.Body.Instructions.Insert(12, new Instruction(OpCodes.Calli));
                    methodDef2.Body.Instructions.Insert(13, new Instruction(OpCodes.Sizeof, operand));
                    methodDef2.Body.Instructions.Insert(methodDef2.Body.Instructions.Count, instruction2);
                    methodDef2.Body.Instructions.Insert(methodDef2.Body.Instructions.Count, new Instruction(OpCodes.Stloc, local2));
                    methodDef2.Body.Instructions.Insert(methodDef2.Body.Instructions.Count, new Instruction(OpCodes.Br, instruction3));
                    methodDef2.Body.Instructions.Insert(methodDef2.Body.Instructions.Count, instruction);
                    var exceptionHandler = new ExceptionHandler(ExceptionHandlerType.Finally)
                    {
                        HandlerStart = methodDef2.Body.Instructions[10],
                        HandlerEnd = methodDef2.Body.Instructions[11],
                        TryEnd = methodDef2.Body.Instructions[14],
                        TryStart = methodDef2.Body.Instructions[12]
                    };
                    if (!methodDef2.Body.HasExceptionHandlers)
                    {
                        methodDef2.Body.ExceptionHandlers.Add(exceptionHandler);
                    }
                    methodDef2.Body.OptimizeBranches();
                    methodDef2.Body.OptimizeMacros();
                }
            }
            TypeDef typeDef3 = new TypeDefUser(RenamerPhase.GenerateString(RenamerPhase.RenameMode.Normal));
            FieldDef item2 = new FieldDefUser(RenamerPhase.GenerateString(RenamerPhase.RenameMode.Normal), new FieldSig(manifestModule.Import(typeof(MindLated_png)).ToTypeSig()));
            typeDef3.Fields.Add(item2);
            typeDef3.BaseType = manifestModule.Import(typeof(MindLated_png));
            manifestModule.Types.Add(typeDef3);
            TypeDef typeDef4 = new TypeDefUser(RenamerPhase.GenerateString(RenamerPhase.RenameMode.Normal))
            {
                IsInterface = true,
                IsSealed = true
            };
            manifestModule.Types.Add(typeDef4);
            manifestModule.TablesHeaderVersion = 257;
        }
    }
}