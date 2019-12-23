using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Isolated.Protection.CtrlFlow;

namespace Isolated.Protection.InvalidMD
{
    internal class InvalidMDPhase
    {
        public static void process(AssemblyDef asm)
        {
            ModuleDef manifestModule = asm.ManifestModule;
            manifestModule.Mvid = null;
            manifestModule.Name = "";
            asm.ManifestModule.Import(new FieldDefUser(""));
            foreach (TypeDef typeDef in manifestModule.Types)
            {
                TypeDef typeDef2 = new TypeDefUser("");
                typeDef2.Methods.Add(new MethodDefUser());
                typeDef2.NestedTypes.Add(new TypeDefUser(""));
                MethodDef item = new MethodDefUser();
                typeDef2.Methods.Add(item);
                typeDef.NestedTypes.Add(typeDef2);
                typeDef.Events.Add(new EventDefUser());
                MethodDef methodDef = new MethodDefUser();
                methodDef.MethodSig = new MethodSig();
                foreach (MethodDef methodDef2 in typeDef.Methods)
                {
                    if (methodDef2.Body != null)
                    {
                        methodDef2.Body.SimplifyBranches();
                        if (!(methodDef2.ReturnType.FullName != "System.Void") && methodDef2.HasBody && methodDef2.Body.Instructions.Count != 0)
                        {
                            TypeSig typeSig = asm.ManifestModule.Import(typeof(int)).ToTypeSig();
                            Local local = new Local(typeSig);
                            TypeSig typeSig2 = asm.ManifestModule.Import(typeof(bool)).ToTypeSig();
                            Local local2 = new Local(typeSig2);
                            methodDef2.Body.Variables.Add(local);
                            methodDef2.Body.Variables.Add(local2);
                            Instruction operand = methodDef2.Body.Instructions[methodDef2.Body.Instructions.Count - 1];
                            Instruction instruction = new Instruction(OpCodes.Ret);
                            Instruction instruction2 = new Instruction(OpCodes.Ldc_I4_1);
                            methodDef2.Body.Instructions.Insert(0, new Instruction(OpCodes.Ldc_I4_0));
                            methodDef2.Body.Instructions.Insert(1, new Instruction(OpCodes.Stloc, local));
                            methodDef2.Body.Instructions.Insert(2, new Instruction(OpCodes.Br, instruction2));
                            Instruction instruction3 = new Instruction(OpCodes.Ldloc, local);
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
                            ExceptionHandler exceptionHandler = new ExceptionHandler(ExceptionHandlerType.Finally);
                            exceptionHandler.HandlerStart = methodDef2.Body.Instructions[10];
                            exceptionHandler.HandlerEnd = methodDef2.Body.Instructions[11];
                            exceptionHandler.TryEnd = methodDef2.Body.Instructions[14];
                            exceptionHandler.TryStart = methodDef2.Body.Instructions[12];
                            if (!methodDef2.Body.HasExceptionHandlers)
                            {
                                methodDef2.Body.ExceptionHandlers.Add(exceptionHandler);
                            }
                            operand = new Instruction(OpCodes.Br, instruction);
                            methodDef2.Body.OptimizeBranches();
                            methodDef2.Body.OptimizeMacros();
                        }
                    }
                }
            }
            TypeDef typeDef3 = new TypeDefUser(controlflow.generate(-1));
            FieldDef item2 = new FieldDefUser(controlflow.generate(-1), new FieldSig(manifestModule.Import(typeof(Isolated_png)).ToTypeSig()));
            typeDef3.Fields.Add(item2);
            typeDef3.BaseType = manifestModule.Import(typeof(Isolated_png));
            manifestModule.Types.Add(typeDef3);
            TypeDef typeDef4 = new TypeDefUser("");
            typeDef4.IsInterface = true;
            typeDef4.IsSealed = true;
            manifestModule.Types.Add(typeDef4);
            manifestModule.TablesHeaderVersion = new ushort?(257);
        }
    }
}