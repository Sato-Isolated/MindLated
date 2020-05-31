using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;
using Isolated.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Isolated.Services.Utils.ModuleWriterListener;

namespace Isolated.Protection.Fake
{
    internal class FakeNative
    {
        private readonly Random R = new Random();

        public void Execute()
        {
            Utils.listener.OnWriterEvent += OnWriterEvent;
        }

        public static string GetRandomString()
        {
            string randomFileName = Path.GetRandomFileName();
            return randomFileName.Replace(".", "");
        }

        private void OnWriterEvent(object sender, ModuleWriterListenerEventArgs e)
        {
            ModuleWriterBase moduleWriterBase = (ModuleWriterBase)sender;
            if (e.WriterEvent == ModuleWriterEvent.MDEndCreateTables)
            {
                PESection pESection = new PESection("Isolated", 1073741888);
                moduleWriterBase.Sections.Add(pESection);
                pESection.Add(new ByteArrayChunk(new byte[123]), 4);
                pESection.Add(new ByteArrayChunk(new byte[10]), 4);
                string text = ".Isolated";
                string s = null;
                for (int i = 0; i < 80; i++)
                {
                    text += GetRandomString();
                }
                for (int j = 0; j < 80; j++)
                {
                    byte[] bytes = Encoding.ASCII.GetBytes(text);
                    s = EncodeString(bytes, asciiCharset);
                }
                byte[] bytes2 = Encoding.ASCII.GetBytes(s);
                moduleWriterBase.TheOptions.MetaDataOptions.OtherHeapsEnd.Add(new RawHeap("#Isolator", bytes2));
                pESection.Add(new ByteArrayChunk(bytes2), 4);
                var writer = (ModuleWriterBase)sender;
                uint signature = (uint)(moduleWriterBase.MetaData.TablesHeap.TypeSpecTable.Rows + 1);
                List<uint> list = (from row in moduleWriterBase.MetaData.TablesHeap.TypeDefTable
                                   select row.Namespace).Distinct().ToList();
                List<uint> list2 = (from row in moduleWriterBase.MetaData.TablesHeap.MethodTable
                                    select row.Name).Distinct().ToList();
                uint num2 = Convert.ToUInt32(R.Next(15, 3546));
                using (List<uint>.Enumerator enumerator = list.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        uint current = enumerator.Current;
                        if (current != 0u)
                        {
                            foreach (uint current2 in list2)
                            {
                                if (current2 != 0u)
                                {
                                    moduleWriterBase.MetaData.TablesHeap.TypeSpecTable.Add(new RawTypeSpecRow(signature));
                                    moduleWriterBase.MetaData.TablesHeap.ModuleTable.Add(new RawModuleRow(65535, 0, 4294967295, 4294967295, 4294967295));
                                    moduleWriterBase.MetaData.TablesHeap.ParamTable.Add(new RawParamRow(254, 254, moduleWriterBase.MetaData.TablesHeap.ENCMapTable.Add(new RawENCMapRow((UInt32)R.Next(2147483647)))));
                                    moduleWriterBase.MetaData.TablesHeap.FieldTable.Add(new RawFieldRow((ushort)(num2 * 4 + 77), 31 + num2 / 2 * 3, (UInt32)R.Next(2147483647)));
                                    moduleWriterBase.MetaData.TablesHeap.MemberRefTable.Add(new RawMemberRefRow(num2 + 18, num2 * 4 + 77, 31 + num2 / 2 * 3));
                                    moduleWriterBase.MetaData.TablesHeap.TypeSpecTable.Add(new RawTypeSpecRow(3391 + num2 / 2 * 3));
                                    moduleWriterBase.MetaData.TablesHeap.PropertyTable.Add(new RawPropertyRow((ushort)(num2 + 44 - 1332), num2 / 2 + 2, (UInt32)R.Next(2147483647)));
                                    moduleWriterBase.MetaData.TablesHeap.TypeSpecTable.Add(new RawTypeSpecRow(3391 + num2 / 2 * 3));
                                    moduleWriterBase.MetaData.TablesHeap.PropertyPtrTable.Add(new RawPropertyPtrRow((UInt32)R.Next(2147483647)));
                                    moduleWriterBase.MetaData.TablesHeap.AssemblyRefTable.Add(new RawAssemblyRefRow(55, 44, 66, 500, (UInt32)R.Next(2147483647), (UInt32)R.Next(2147483647), moduleWriterBase.MetaData.TablesHeap.ENCMapTable.Add(new RawENCMapRow((UInt32)R.Next(2147483647))), (UInt32)R.Next(2147483647), (UInt32)R.Next(2147483647)));
                                    moduleWriterBase.MetaData.TablesHeap.ENCLogTable.Add(new RawENCLogRow((UInt32)R.Next(2147483647), moduleWriterBase.MetaData.TablesHeap.ENCMapTable.Add(new RawENCMapRow((UInt32)R.Next(2147483647)))));
                                    moduleWriterBase.MetaData.TablesHeap.ENCLogTable.Add(new RawENCLogRow((UInt32)R.Next(2147483647), (uint)(moduleWriterBase.MetaData.TablesHeap.ENCMapTable.Rows - 1)));
                                    moduleWriterBase.MetaData.TablesHeap.ImplMapTable.Add(new RawImplMapRow(18, num2 * 4 + 77, 31 + num2 / 2 * 3, num2 * 4 + 77));
                                }
                            }
                        }
                    }
                }
            }
            if (e.WriterEvent == ModuleWriterEvent.MDOnAllTablesSorted)
            {
                moduleWriterBase.MetaData.TablesHeap.DeclSecurityTable.Add(new RawDeclSecurityRow(32767, 4294934527u, 4294934527u));
            }
        }

        public static string EncodeString(byte[] buff, char[] charset)
        {
            int current = buff[0];
            StringBuilder ret = new StringBuilder();
            for (int i = 1; i < buff.Length; i++)
            {
                for (current = (current << 8) + buff[i]; current >= charset.Length; current /= charset.Length)
                {
                    ret.Append(charset[current % charset.Length]);
                }
            }
            if (current != 0)
            {
                ret.Append(charset[current % charset.Length]);
            }
            return ret.ToString();
        }

        private static readonly char[] asciiCharset = (from ord in Enumerable.Range(32, 95)
                                                       select (char)ord).Except(new char[]
        {
                '.'
        }).ToArray();
    }

    internal class RawHeap : HeapBase
    {
        public override string Name
        {
            get
            {
                return name;
            }
        }

        public RawHeap(string name, byte[] content)
        {
            this.name = name;
            this.content = content;
        }

        public override uint GetRawLength()
        {
            return (uint)content.Length;
        }

        protected override void WriteToImpl(BinaryWriter writer)
        {
            writer.Write(content);
        }

        private readonly byte[] content;

        private readonly string name;
    }
}