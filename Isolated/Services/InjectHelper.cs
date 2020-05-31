using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;
using System.Linq;

namespace Isolated.Services
{
    public static class InjectHelper
    {
        private static TypeDefUser Clone(TypeDef origin)
        {
            var ret = new TypeDefUser(origin.Namespace, origin.Name)
            {
                Attributes = origin.Attributes
            };

            if (origin.ClassLayout != null)
                ret.ClassLayout = new ClassLayoutUser(origin.ClassLayout.PackingSize, origin.ClassSize);

            foreach (GenericParam genericParam in origin.GenericParameters)
                ret.GenericParameters.Add(new GenericParamUser(genericParam.Number, genericParam.Flags, "-"));

            return ret;
        }

        private static MethodDefUser Clone(MethodDef origin)
        {
            var ret = new MethodDefUser(origin.Name, null, origin.ImplAttributes, origin.Attributes);

            foreach (GenericParam genericParam in origin.GenericParameters)
                ret.GenericParameters.Add(new GenericParamUser(genericParam.Number, genericParam.Flags, "-"));

            return ret;
        }

        private static FieldDefUser Clone(FieldDef origin)
        {
            var ret = new FieldDefUser(origin.Name, null, origin.Attributes);
            return ret;
        }

        private static TypeDef PopulateContext(TypeDef typeDef, InjectContext ctx)
        {
            TypeDef ret;
            if (!ctx.Map.TryGetValue(typeDef, out IDnlibDef existing))
            {
                ret = Clone(typeDef);
                ctx.Map[typeDef] = ret;
            }
            else
                ret = (TypeDef)existing;

            foreach (TypeDef nestedType in typeDef.NestedTypes)
                ret.NestedTypes.Add(PopulateContext(nestedType, ctx));

            foreach (MethodDef method in typeDef.Methods)
                ret.Methods.Add((MethodDef)(ctx.Map[method] = Clone(method)));

            foreach (FieldDef field in typeDef.Fields)
                ret.Fields.Add((FieldDef)(ctx.Map[field] = Clone(field)));

            return ret;
        }

        private static void CopyTypeDef(TypeDef typeDef, InjectContext ctx)
        {
            var newTypeDef = (TypeDef)ctx.Map[typeDef];

            newTypeDef.BaseType = (ITypeDefOrRef)ctx.Importer.Import(typeDef.BaseType);

            foreach (InterfaceImpl iface in typeDef.Interfaces)
                newTypeDef.Interfaces.Add(new InterfaceImplUser((ITypeDefOrRef)ctx.Importer.Import(iface.Interface)));
        }

        private static void CopyMethodDef(MethodDef methodDef, InjectContext ctx)
        {
            var newMethodDef = (MethodDef)ctx.Map[methodDef];

            newMethodDef.Signature = ctx.Importer.Import(methodDef.Signature);
            newMethodDef.Parameters.UpdateParameterTypes();

            if (methodDef.ImplMap != null)
                newMethodDef.ImplMap = new ImplMapUser(new ModuleRefUser(ctx.TargetModule, methodDef.ImplMap.Module.Name), methodDef.ImplMap.Name, methodDef.ImplMap.Attributes);

            foreach (CustomAttribute ca in methodDef.CustomAttributes)
                newMethodDef.CustomAttributes.Add(new CustomAttribute((ICustomAttributeType)ctx.Importer.Import(ca.Constructor)));

            if (methodDef.HasBody)
            {
                newMethodDef.Body = new CilBody(methodDef.Body.InitLocals, new List<Instruction>(), new List<ExceptionHandler>(), new List<Local>());
                newMethodDef.Body.MaxStack = methodDef.Body.MaxStack;

                var bodyMap = new Dictionary<object, object>();

                foreach (Local local in methodDef.Body.Variables)
                {
                    var newLocal = new Local(ctx.Importer.Import(local.Type));
                    newMethodDef.Body.Variables.Add(newLocal);
                    newLocal.Name = local.Name;
                    newLocal.PdbAttributes = local.PdbAttributes;

                    bodyMap[local] = newLocal;
                }

                foreach (Instruction instr in methodDef.Body.Instructions)
                {
                    var newInstr = new Instruction(instr.OpCode, instr.Operand)
                    {
                        SequencePoint = instr.SequencePoint
                    };

                    if (newInstr.Operand is IType type)
                        newInstr.Operand = ctx.Importer.Import(type);
                    else if (newInstr.Operand is IMethod method)
                        newInstr.Operand = ctx.Importer.Import(method);
                    else if (newInstr.Operand is IField field)
                        newInstr.Operand = ctx.Importer.Import(field);

                    newMethodDef.Body.Instructions.Add(newInstr);
                    bodyMap[instr] = newInstr;
                }

                foreach (Instruction instr in newMethodDef.Body.Instructions)
                {
                    if (instr.Operand != null && bodyMap.ContainsKey(instr.Operand))
                        instr.Operand = bodyMap[instr.Operand];
                    else if (instr.Operand is Instruction[] v)
                        instr.Operand = v.Select(target => (Instruction)bodyMap[target]).ToArray();
                }

                foreach (ExceptionHandler eh in methodDef.Body.ExceptionHandlers)
                    newMethodDef.Body.ExceptionHandlers.Add(new ExceptionHandler(eh.HandlerType)
                    {
                        CatchType = eh.CatchType == null ? null : (ITypeDefOrRef)ctx.Importer.Import(eh.CatchType),
                        TryStart = (Instruction)bodyMap[eh.TryStart],
                        TryEnd = (Instruction)bodyMap[eh.TryEnd],
                        HandlerStart = (Instruction)bodyMap[eh.HandlerStart],
                        HandlerEnd = (Instruction)bodyMap[eh.HandlerEnd],
                        FilterStart = eh.FilterStart == null ? null : (Instruction)bodyMap[eh.FilterStart]
                    });

                newMethodDef.Body.SimplifyMacros(newMethodDef.Parameters);
            }
        }

        private static void CopyFieldDef(FieldDef fieldDef, InjectContext ctx)
        {
            var newFieldDef = (FieldDef)ctx.Map[fieldDef];

            newFieldDef.Signature = ctx.Importer.Import(fieldDef.Signature);
        }

        private static void Copy(TypeDef typeDef, InjectContext ctx, bool copySelf)
        {
            if (copySelf)
                CopyTypeDef(typeDef, ctx);

            foreach (TypeDef nestedType in typeDef.NestedTypes)
                Copy(nestedType, ctx, true);

            foreach (MethodDef method in typeDef.Methods)
                CopyMethodDef(method, ctx);

            foreach (FieldDef field in typeDef.Fields)
                CopyFieldDef(field, ctx);
        }

        public static TypeDef Inject(TypeDef typeDef, ModuleDef target)
        {
            var ctx = new InjectContext(typeDef.Module, target);
            PopulateContext(typeDef, ctx);
            Copy(typeDef, ctx, true);
            return (TypeDef)ctx.Map[typeDef];
        }

        public static MethodDef Inject(MethodDef methodDef, ModuleDef target)
        {
            var ctx = new InjectContext(methodDef.Module, target);
            ctx.Map[methodDef] = Clone(methodDef);
            CopyMethodDef(methodDef, ctx);
            return (MethodDef)ctx.Map[methodDef];
        }

        public static IEnumerable<IDnlibDef> Inject(TypeDef typeDef, TypeDef newType, ModuleDef target)
        {
            var ctx = new InjectContext(typeDef.Module, target);
            ctx.Map[typeDef] = newType;
            PopulateContext(typeDef, ctx);
            Copy(typeDef, ctx, false);
            return ctx.Map.Values.Except(new[] { newType });
        }

        private class InjectContext : ImportResolver
        {
            public readonly Dictionary<IDnlibDef, IDnlibDef> Map = new Dictionary<IDnlibDef, IDnlibDef>();

            public readonly ModuleDef OriginModule;

            public readonly ModuleDef TargetModule;

            private readonly Importer importer;

            public InjectContext(ModuleDef module, ModuleDef target)
            {
                OriginModule = module;
                TargetModule = target;
                importer = new Importer(target, ImporterOptions.TryToUseTypeDefs)
                {
                    Resolver = this
                };
            }

            public Importer Importer
            {
                get { return importer; }
            }

            public override TypeDef Resolve(TypeDef typeDef)
            {
                if (Map.ContainsKey(typeDef))
                    return (TypeDef)Map[typeDef];
                return null;
            }

            public override MethodDef Resolve(MethodDef methodDef)
            {
                if (Map.ContainsKey(methodDef))
                    return (MethodDef)Map[methodDef];
                return null;
            }

            public override FieldDef Resolve(FieldDef fieldDef)
            {
                if (Map.ContainsKey(fieldDef))
                    return (FieldDef)Map[fieldDef];
                return null;
            }
        }
    }
}