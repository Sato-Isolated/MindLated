using dnlib.DotNet;
using dnlib.DotNet.Writer;
using MindLated.Protection.Anti;
using MindLated.Protection.Arithmetic;
using MindLated.Protection.CtrlFlow;
using MindLated.Protection.INT;
using MindLated.Protection.InvalidMD;
using MindLated.Protection.LocalF;
using MindLated.Protection.Other;
using MindLated.Protection.Proxy;
using MindLated.Protection.Renamer;
using MindLated.Protection.String;
using MindLated.Protection.StringOnline;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace MindLated
{
    public partial class Form1 : Form
    {
        public static MethodDef? Init;
        public static MethodDef? Init2;
        private ModuleDefMD? md { get; set; }
        private string _directoryName = string.Empty;

        public Form1() => InitializeComponent();

        public void AppendMsg(RichTextBox richTextBox1, Color color, string text, bool AutoTime)
        {
            richTextBox1.BeginInvoke(new ThreadStart(() =>
            {
                lock (richTextBox1)
                {
                    richTextBox1.Focus();
                    if (richTextBox1.TextLength > 100000)
                    {
                        richTextBox1.Clear();
                    }
                    using (var temp = new RichTextBox())
                    {
                        temp.SelectionColor = color;
                        if (AutoTime)
                            temp.AppendText(DateTime.Now.ToString("HH:mm:ss"));
                        temp.AppendText(text);
                        richTextBox1.Select(richTextBox1.Rtf.Length, 0);
                        richTextBox1.SelectedRtf = temp.Rtf;
                    }
                }
            }));
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                var array = (Array)e.Data.GetData(DataFormats.FileDrop);
                if (array == null)
                    return;
                var text = array.GetValue(0).ToString();
                var num = text.LastIndexOf(".", StringComparison.Ordinal);
                if (num == -1)
                    return;
                var text2 = text.Substring(num);
                text2 = text2.ToLower();
                if (string.Compare(text2, ".exe", StringComparison.Ordinal) != 0 && string.Compare(text2, ".dll", StringComparison.Ordinal) != 0)
                {
                    return;
                }

                Activate();
                textBox1.Text = text;
                var num2 = text.LastIndexOf("\\", StringComparison.Ordinal);
                if (num2 != -1)
                {
                    _directoryName = text.Remove(num2, text.Length - num2);
                }

                if (_directoryName.Length == 2)
                {
                    _directoryName += "\\";
                }
            }
            catch
            {
                /* ignored */
            }
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e) => e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;

        private void button1_Click(object sender, EventArgs e)
        {
        
            md = ModuleDefMD.Load(textBox1.Text);
            foreach (Action func in Proc)
            {
                func();
            }
            var text2 = Path.GetDirectoryName(textBox1.Text);
            if (text2 != null && !text2.EndsWith("\\"))
                text2 += "\\";
            var path = $"{text2}{Path.GetFileNameWithoutExtension(textBox1.Text)}_protected{Path.GetExtension(textBox1.Text)}";

            var opts = new ModuleWriterOptions(md);
            opts.Logger = DummyLogger.NoThrowInstance;
            md.Write(path, opts);

            AppendMsg(richTextBox1, Color.Red, $"Save: {path}", true);
        }

        public List<Action> Proc = new();
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                Proc.Add(ProcessStringEncrypt);
                listBox1.Items.Add("-> String Encrypt");
            }
            else
            {
                Proc.Remove(ProcessStringEncrypt);
                listBox1.Items.Remove("-> String Encrypt");
            }
        }

        private void ProcessStringEncrypt()
        {
            StringEncPhase.Execute(md);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                Proc.Add(ProcessOnlineStringDecryption);
                listBox1.Items.Add("-> OnlineStrDecrypt");
            }
            else
            {
                Proc.Remove(ProcessOnlineStringDecryption);
                listBox1.Items.Remove("-> OnlineStrDecrypt");
            }
        }
        private void ProcessOnlineStringDecryption()
        {
            OnlinePhase.Execute(md);
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                Proc.Add(ProcessControlFlow);
                listBox1.Items.Add("-> ControlFlow");
            }
            else
            {
                Proc.Remove(ProcessControlFlow);
                listBox1.Items.Remove("-> ControlFlow");
            }
        }
        private void ProcessControlFlow()
        {
            ControlFlowObfuscation.Execute(md);
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                Proc.Add(ProcessIntConfusion);
                listBox1.Items.Add("-> IntConfusion");
            }
            else
            {
                Proc.Remove(ProcessIntConfusion);
                listBox1.Items.Remove("-> IntConfusion");
            }
        }
        private void ProcessIntConfusion()
        {
            AddIntPhase.Execute2(md);
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked)
            {
                Proc.Add(ProcessArithmetic);
                listBox1.Items.Add("-> Arithmetic");
            }
            else
            {
                Proc.Remove(ProcessArithmetic);
                listBox1.Items.Remove("-> Arithmetic");
            }
        }
        private void ProcessArithmetic()
        {
            Arithmetic.Execute(md);
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox6.Checked)
            {
                Proc.Add(ProcessLocalToField);
                listBox1.Items.Add("-> L2F");
            }
            else
            {
                Proc.Remove(ProcessLocalToField);
                listBox1.Items.Remove("-> L2F");
            }
        }
        private void ProcessLocalToField()
        {
            L2F.Execute(md);
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox7.Checked)
            {
                Proc.Add(ProcessLocalToFieldV2);
                listBox1.Items.Add("-> L2FV2");
            }
            else
            {
                Proc.Remove(ProcessLocalToFieldV2);
                listBox1.Items.Remove("-> L2FV2");
            }
        }
        private void ProcessLocalToFieldV2()
        {
            L2FV2.Execute(md);
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox8.Checked)
            {
                Proc.Add(ProcessCalli);
                listBox1.Items.Add("-> Calli");
            }
            else
            {
                Proc.Remove(ProcessCalli);
                listBox1.Items.Remove("-> Calli");
            }
        }
        private void ProcessCalli()
        {
            Calli.Execute(md);
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox9.Checked)
            {
                Proc.Add(ProcessProxyMeth);
                listBox1.Items.Add("-> ProxyMeth");
            }
            else
            {
                Proc.Remove(ProcessProxyMeth);
                listBox1.Items.Remove("-> ProxyMeth");
            }
        }

        private void ProcessProxyMeth()
        {
            ProxyMeth.Execute(md);
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox10.Checked)
            {
                Proc.Add(ProcessProxyInt);
                listBox1.Items.Add("-> ProxyInt");
            }
            else
            {
                Proc.Remove(ProcessProxyInt);
                listBox1.Items.Remove("-> ProxyInt");
            }
        }

        private void ProcessProxyInt()
        {
            ProxyInt.Execute(md);
        }

        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox11.Checked)
            {
                Proc.Add(ProcessProxyString);
                listBox1.Items.Add("-> ProxyString");
            }
            else
            {
                Proc.Remove(ProcessProxyString);
                listBox1.Items.Remove("-> ProxyString");
            }
        }
        private void ProcessProxyString()
        {
            ProxyString.Execute(md);
        }

        private void checkBox12_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox12.Checked)
            {
                Proc.Add(ProcessRenamer);
                listBox1.Items.Add("-> Renamer");
            }
            else
            {
                Proc.Remove(ProcessRenamer);
                listBox1.Items.Remove("-> Renamer");
            }
        }
        private void ProcessRenamer()
        {
            RenamerPhase.Execute(md);
        }

        private void checkBox13_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox13.Checked)
            {
                Proc.Add(ProcessJumpCflow);
                listBox1.Items.Add("-> JumpCflow");
            }
            else
            {
                Proc.Remove(ProcessJumpCflow);
                listBox1.Items.Remove("-> JumpCflow");
            }
        }
        private void ProcessJumpCflow()
        {
            RenamerPhase.Execute(md);
        }

        private void checkBox14_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox14.Checked)
            {
                Proc.Add(ProcessAntiDebug);
                listBox1.Items.Add("-> Anti Debug");
            }
            else
            {
                Proc.Remove(ProcessAntiDebug);
                listBox1.Items.Remove("-> Anti Debug");
            }
        }
        private void ProcessAntiDebug()
        {
            AntiDebug.Execute(md);
        }

        private void checkBox15_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox15.Checked)
            {
                Proc.Add(ProcessAntiDump);
                listBox1.Items.Add("-> Anti Dump");
            }
            else
            {
                Proc.Remove(ProcessAntiDump);
                listBox1.Items.Remove("-> Anti Dump");
            }
        }
        private void ProcessAntiDump()
        {
            AntiDump.Execute(md);
        }

        private void checkBox16_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox16.Checked)
            {
                Proc.Add(ProcessAntiTamper);
                listBox1.Items.Add("-> Anti Tamper");
            }
            else
            {
                Proc.Remove(ProcessAntiTamper);
                listBox1.Items.Remove("-> Anti Tamper");
            }
        }
        private void ProcessAntiTamper()
        {
            AntiTamper.Execute(md);
        }

        private void checkBox17_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox17.Checked)
            {
                Proc.Add(ProcessInvalidMD);
                listBox1.Items.Add("-> InvalidMD");
            }
            else
            {
                Proc.Remove(ProcessInvalidMD);
                listBox1.Items.Remove("-> InvalidMD");
            }
        }

        private void ProcessInvalidMD()
        {
            InvalidMDPhase.Execute(md.Assembly);
        }

        private void checkBox18_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox18.Checked)
            {
                Proc.Add(ProcessAntiDe4dot);
                listBox1.Items.Add("-> AntiDe4dot");
            }
            else
            {
                Proc.Remove(ProcessAntiDe4dot);
                listBox1.Items.Remove("-> AntiDe4dot");
            }
        }

        private void ProcessAntiDe4dot()
        {
            AntiDe4dot.Execute(md.Assembly);
        }

        private void checkBox19_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox19.Checked)
            {
                Proc.Add(ProcessStackUnfConfusion);
                listBox1.Items.Add("-> StackUnfConfusion");
            }
            else
            {
                Proc.Remove(ProcessStackUnfConfusion);
                listBox1.Items.Remove("-> StackUnfConfusion");
            }
        }
        private void ProcessStackUnfConfusion()
        {
            StackUnfConfusion.Execute(md);
        }

        private void checkBox20_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox20.Checked)
            {
                Proc.Add(ProcessAntimanything);
                listBox1.Items.Add("-> Anti manything");
            }
            else
            {
                Proc.Remove(ProcessAntimanything);
                listBox1.Items.Remove("-> Anti manything");
            }
        }

        private void ProcessAntimanything()
        {
            Antimanything.Execute(md);
        }
    }
}
