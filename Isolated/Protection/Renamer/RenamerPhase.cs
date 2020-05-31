using dnlib.DotNet;
using System;
using System.Collections.Generic;

namespace Isolated.Protection.Renamer
{
    public class RenamerPhase
    {
        private static readonly Dictionary<TypeDef, bool> typeRename = new Dictionary<TypeDef, bool>();
        private static readonly List<string> typeNewName = new List<string>();
        private static readonly Dictionary<MethodDef, bool> methodRename = new Dictionary<MethodDef, bool>();
        private static readonly List<string> methodNewName = new List<string>();
        private static readonly Dictionary<FieldDef, bool> fieldRename = new Dictionary<FieldDef, bool>();
        private static readonly List<string> fieldNewName = new List<string>();
        public static bool IsObfuscationActive = true;

        public static void Rename(TypeDef type, bool canRename = true)
        {
            if (typeRename.ContainsKey(type))
                typeRename[type] = canRename;
            else
                typeRename.Add(type, canRename);
        }

        public static void Rename(MethodDef method, bool canRename = true)
        {
            if (methodRename.ContainsKey(method))
                methodRename[method] = canRename;
            else
                methodRename.Add(method, canRename);
        }

        public static void Rename(FieldDef field, bool canRename = true)
        {
            if (fieldRename.ContainsKey(field))
                fieldRename[field] = canRename;
            else
                fieldRename.Add(field, canRename);
        }

        public static void Execute(ModuleDefMD module)
        {
            if (IsObfuscationActive)
            {
                string namespaceNewName = GenerateString();
                foreach (TypeDef type in module.Types)
                {
                    if (typeRename.TryGetValue(type, out bool canRenameType))
                    {
                        if (canRenameType)
                            InternalRename(type);
                    }
                    else
                        InternalRename(type);
                    type.Namespace = namespaceNewName;
                    foreach (MethodDef method in type.Methods)
                    {
                        if (methodRename.TryGetValue(method, out bool canRenameMethod))
                        {
                            if (canRenameMethod && !method.IsConstructor && !method.IsSpecialName)
                                InternalRename(method);
                        }
                        else if (!method.IsConstructor && !method.IsSpecialName)
                            InternalRename(method);
                    }
                    methodNewName.Clear();
                    foreach (FieldDef field in type.Fields)
                    {
                        if (fieldRename.TryGetValue(field, out bool canRenameField))
                        {
                            if (canRenameField)
                                InternalRename(field);
                        }
                        else
                            InternalRename(field);
                    }
                    fieldNewName.Clear();
                }
            }
            else
            {
                foreach (var typeItem in typeRename)
                {
                    if (typeItem.Value)
                        InternalRename(typeItem.Key);
                }
                foreach (var methodItem in methodRename)
                {
                    if (methodItem.Value)
                        InternalRename(methodItem.Key);
                }
                foreach (var fieldItem in fieldRename)
                {
                    if (fieldItem.Value)
                        InternalRename(fieldItem.Key);
                }
            }
        }

        private static void InternalRename(TypeDef type)
        {
            string randString = GenerateString();
            while (typeNewName.Contains(randString))
                randString = GenerateString();
            typeNewName.Add(randString);
            type.Name = randString;
        }

        private static void InternalRename(MethodDef method)
        {
            string randString = GenerateString();
            while (methodNewName.Contains(randString))
                randString = GenerateString();
            methodNewName.Add(randString);
            method.Name = randString;
        }

        private static void InternalRename(FieldDef field)
        {
            string randString = GenerateString();
            while (fieldNewName.Contains(randString))
                randString = GenerateString();
            fieldNewName.Add(randString);
            field.Name = randString;
        }

        public static Random random = new Random();

        public static string GenerateString()
        {
            string s = "";
            for (int i = 0; i < 3; i++)
                s += (char)random.Next(8000, 8500);
            return s;
        }
    }
}