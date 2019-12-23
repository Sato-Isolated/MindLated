using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Isolated.Protection.CtrlFlow
{
    internal static class obfuscatorHelper
    {
        public static MethodDef importMethod(string name)
        {
            MethodDef methodDef = null;
            foreach (TypeDef typeDef in obfuscatorHelper.me.ManifestModule.Types)
            {
                if (methodDef != null)
                {
                    break;
                }
                foreach (MethodDef methodDef2 in typeDef.Methods)
                {
                    if (methodDef2.Name == name)
                    {
                        methodDef = methodDef2;
                        break;
                    }
                }
            }
            if (methodDef == null)
            {
                throw new Exception("no such method");
            }
            methodDef.DeclaringType = null;
            return methodDef;
        }

        public static string generateName(int length = -1)
        {
            return controlflow.generate(length);
        }

        public static TypeDef importType(string name)
        {
            TypeDef typeDef = null;
            foreach (TypeDef typeDef2 in obfuscatorHelper.me.ManifestModule.Types)
            {
                if (typeDef2.Name == name)
                {
                    typeDef = typeDef2;
                    break;
                }
            }
            if (typeDef == null)
            {
                throw new Exception("no such type");
            }
            obfuscatorHelper.me.ManifestModule.Types.Remove(typeDef);
            return typeDef;
        }

        public static MethodDef cloneMethod(MethodDef baseMethod)
        {
            MethodDef methodDef = new MethodDefUser();
            methodDef.Name = baseMethod.Name;
            new List<TypeSig>();
            foreach (Parameter parameter in ((IEnumerable<Parameter>)baseMethod.Parameters))
            {
            }
            MethodSig methodSig = baseMethod.MethodSig;
            methodDef.MethodSig = methodSig;
            methodDef.Attributes = baseMethod.Attributes;
            methodDef.Body = baseMethod.Body;
            methodDef.CodeType = baseMethod.CodeType;
            return methodDef;
        }

        public static TypeDef cloneType(TypeDef baseTyp, string name)
        {
            if (baseTyp == null)
            {
                throw new Exception();
            }
            TypeDef typeDef = new TypeDefUser(name);
            foreach (MethodDef baseMethod in baseTyp.Methods)
            {
                typeDef.Methods.Add(obfuscatorHelper.cloneMethod(baseMethod));
            }
            typeDef.BaseType = baseTyp.BaseType;
            return typeDef;
        }

        public static byte[] encryptData(byte[] data, byte password)
        {
            char c = (char)password;
            obfuscatorHelper.getKey(c.ToString());
            ICryptoTransform cryptoTransform = obfuscatorHelper._algorithm.CreateEncryptor();
            return cryptoTransform.TransformFinalBlock(data, 0, data.Length);
        }

        private static void getKey(string password)
        {
            byte[] array = new byte[8];
            byte[] bytes = Encoding.ASCII.GetBytes(password);
            int num = Math.Min(bytes.Length, array.Length);
            for (int i = 0; i < num; i++)
            {
                array[i] = bytes[i];
            }
            Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, array);
            obfuscatorHelper._algorithm.Key = rfc2898DeriveBytes.GetBytes(obfuscatorHelper._algorithm.KeySize / 8);
            obfuscatorHelper._algorithm.IV = rfc2898DeriveBytes.GetBytes(obfuscatorHelper._algorithm.BlockSize / 8);
        }

        public static int SearchBytePattern(byte[] pattern, byte[] bytes)
        {
            int num = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                if (pattern[0] == bytes[i] && bytes.Length - i >= pattern.Length)
                {
                    bool flag = true;
                    int num2 = 1;
                    while (num2 < pattern.Length && flag)
                    {
                        if (bytes[i + num2] != pattern[num2])
                        {
                            flag = false;
                        }
                        num2++;
                    }
                    if (flag)
                    {
                        num++;
                        i += pattern.Length - 1;
                    }
                }
            }
            return num;
        }

        public static int[] Locate(this byte[] self, byte[] candidate)
        {
            if (obfuscatorHelper.IsEmptyLocate(self, candidate))
            {
                return obfuscatorHelper.Empty;
            }
            List<int> list = new List<int>();
            for (int i = 0; i < self.Length; i++)
            {
                if (obfuscatorHelper.IsMatch(self, i, candidate))
                {
                    list.Add(i);
                }
            }
            return list.ToArray();
        }

        private static bool IsMatch(byte[] array, int position, byte[] candidate)
        {
            if (candidate.Length > array.Length - position)
            {
                return false;
            }
            for (int i = 0; i < candidate.Length; i++)
            {
                if (array[position + i] != candidate[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsEmptyLocate(byte[] array, byte[] candidate)
        {
            return array == null || candidate == null || array.Length == 0 || candidate.Length == 0 || candidate.Length > array.Length;
        }

        public static Random randomizer = new Random();

        private static AssemblyDef me = AssemblyDef.Load(Assembly.GetExecutingAssembly().Location);

        public static TypeDef moduleType = null;

        private static SymmetricAlgorithm _algorithm = new RijndaelManaged();

        private static readonly int[] Empty = new int[0];
    }
}