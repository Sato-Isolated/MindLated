using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;

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
                var namespaceNewName = GenerateString();
                foreach (var type in module.Types)
                {
                    if (typeRename.TryGetValue(type, out var canRenameType))
                    {
                        if (canRenameType)
                            InternalRename(type);
                    }
                    else
                        InternalRename(type);
                    type.Namespace = namespaceNewName;
                    foreach (var method in type.Methods)
                    {
                        if (methodRename.TryGetValue(method, out var canRenameMethod))
                        {
                            if (canRenameMethod && !method.IsConstructor && !method.IsSpecialName)
                                InternalRename(method);
                        }
                        else if (!method.IsConstructor && !method.IsSpecialName)
                            InternalRename(method);
                    }
                    methodNewName.Clear();
                    foreach (var field in type.Fields)
                    {
                        if (fieldRename.TryGetValue(field, out var canRenameField))
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
                foreach (var typeItem in typeRename.Where(typeItem => typeItem.Value))
                {
                    InternalRename(typeItem.Key);
                }
                foreach (var methodItem in methodRename.Where(methodItem => methodItem.Value))
                {
                    InternalRename(methodItem.Key);
                }
                foreach (var fieldItem in fieldRename.Where(fieldItem => fieldItem.Value))
                {
                    InternalRename(fieldItem.Key);
                }
            }
        }

        private static void InternalRename(TypeDef type)
        {
            var randString = GenerateString();
            while (typeNewName.Contains(randString))
                randString = GenerateString();
            typeNewName.Add(randString);
            type.Name = randString;
        }

        private static void InternalRename(MethodDef method)
        {
            var randString = GenerateString();
            while (methodNewName.Contains(randString))
                randString = GenerateString();
            methodNewName.Add(randString);
            method.Name = randString;
        }

        private static void InternalRename(FieldDef field)
        {
            var randString = GenerateString();
            while (fieldNewName.Contains(randString))
                randString = GenerateString();
            fieldNewName.Add(randString);
            field.Name = randString;
        }

        public static Random random = new Random();

        private static string RandomString(int length, string chars)
        {
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GenerateString()
        {
            var ascii = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return RandomString(random.Next(1, 7), ascii);
        }
    }
}