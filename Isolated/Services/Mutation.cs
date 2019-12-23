using dnlib.DotNet;
using System;
using System.Collections.Generic;

namespace Isolated.Services
{
    public class Mutation
    {
        public static int Key1, Key2;
        private static Dictionary<string, int> dic = new Dictionary<string, int> { { "Key1", Key1 }, { "Key2", Key2 } };

        public static MethodDef InjectKey(MethodDef method, string key, int value)
        {
            var instr = method.Body.Instructions;
            int intKey;
            if (dic.TryGetValue(key, out intKey))
            {
                for (int i = 0; i < instr.Count; i++)
                {
                }
            }
            throw new NotImplementedException();
        }
    }
}