using dnlib.DotNet;
using dnlib.DotNet.Writer;
using Isolated.Protection.Arithmetic;
using Isolated.Protection.Calli;
using Isolated.Protection.CtrlFlow;
using Isolated.Protection.Fake;
using Isolated.Protection.INT;
using Isolated.Protection.InvalidMD;
using Isolated.Protection.LocalF;
using Isolated.Protection.Other;
using Isolated.Protection.Proxy;
using Isolated.Protection.Renamer;
using Isolated.Protection.String;
using Isolated.Services;
using System;
using System.IO;
using System.Windows.Forms;

namespace Isolated
{
    public partial class Form1 : Form
    {
        public static MethodDef init;

        public static MethodDef init2;

        public static Random rand = new Random();

        public string DirectoryName = "";

        private readonly FakeNative FFakeNative = new FakeNative();

        public Form1() => InitializeComponent();

        private void Button1_Click(object sender, EventArgs e)
        {
            ModuleDefMD module = ModuleDefMD.Load(textBox1.Text);

            if (checkBox1.Checked)
            { StringEncPhase.Execute(module); }

            if (checkBox2.Checked)
            { OnlinePhase.Execute(module); }

            if (checkBox7.Checked)
            { ControlFlowObfuscation.Execute(module); }

            if (checkBox8.Checked)
            { AddIntPhase.Execute(module); }

            if (checkBox17.Checked)
            { Arithmetic.Execute(module); }

            if (checkBox13.Checked)
            { L2F.Execute(module); }

            if (checkBox15.Checked)
            { L2FV2.Execute(module); }

            if (checkBox14.Checked)
            { Calli.Execute(module); }

            if (checkBox3.Checked)
            { ProxyString.Execute(module); }

            if (checkBox4.Checked)
            { ProxyINT.Execute(module); }

            if (checkBox12.Checked)
            { ProxyMeth.Execute(module); }

            if (checkBox10.Checked)
            { RenamerPhase.Execute(module); }

            if (checkBox11.Checked)
            { JumpCFlow.Execute(module); }
       
            if (checkBox5.Checked)
            { Anti_Debug.Execute(module); }

            if (checkBox6.Checked)
            { Anti_Tamper.Execute(module); }

            if (checkBox9.Checked)
            { InvalidMDPhase.Execute(module.Assembly); }

            if (checkBox16.Checked)
            { FFakeNative.Execute(); }

            string text2 = Path.GetDirectoryName(textBox1.Text);
            if (!text2.EndsWith("\\"))
            { text2 += "\\"; }

            string path = text2 + Path.GetFileNameWithoutExtension(textBox1.Text) + "_protected" +
                          Path.GetExtension(textBox1.Text);

            module.Write(path, new ModuleWriterOptions(module)
            {
                Listener = Utils.listener,
                PEHeadersOptions = { NumberOfRvaAndSizes = 13 },
                Logger = DummyLogger.NoThrowInstance
            });

            if (checkBox6.Checked)
            { Anti_Tamper.Md5(path); }
        }

        private void TextBox1_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                Array array = (Array)e.Data.GetData(DataFormats.FileDrop);
                if (array != null)
                {
                    string text = array.GetValue(0).ToString();
                    int num = text.LastIndexOf(".", StringComparison.Ordinal);
                    if (num != -1)
                    {
                        string text2 = text.Substring(num);
                        text2 = text2.ToLower();
                        if (text2 == ".exe" || text2 == ".dll")
                        {
                            Activate();
                            textBox1.Text = text;
                            int num2 = text.LastIndexOf("\\", StringComparison.Ordinal);
                            if (num2 != -1)
                            { DirectoryName = text.Remove(num2, text.Length - num2); }
                            if (DirectoryName.Length == 2)
                            { DirectoryName += "\\"; }
                        }
                    }
                }
            }
            catch { }
        }

        private void TextBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            { e.Effect = DragDropEffects.Copy; }
            else
            { e.Effect = DragDropEffects.None; }
        }
    }
}