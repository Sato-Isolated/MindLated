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
        private ModuleDefMD Md { get; set; } = null!;
        private string _directoryName = string.Empty;

        public Form1()
        {
            InitializeComponent();
        }

        private static void AppendMsg(RichTextBox rtb, Color color, string text, bool autoTime)
        {
            rtb.BeginInvoke(new ThreadStart(() =>
            {
                lock (rtb)
                {
                    rtb.Focus();
                    if (rtb.TextLength > 100000)
                    {
                        rtb.Clear();
                    }

                    using var temp = new RichTextBox();
                    temp.SelectionColor = color;
                    if (autoTime)
                        temp.AppendText(DateTime.Now.ToString("HH:mm:ss"));
                    temp.AppendText(text);
                    rtb.Select(rtb.Rtf.Length, 0);
                    rtb.SelectedRtf = temp.Rtf;
                }
            }));
        }

        private void TextBox1_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                var array = (Array)e.Data?.GetData(DataFormats.FileDrop)!;
                var text = array.GetValue(0)!.ToString();
                var num = text!.LastIndexOf(".", StringComparison.Ordinal);
                if (num == -1)
                    return;
                var text2 = text[num..];
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

        private void TextBox1_DragEnter(object sender, DragEventArgs e) => e.Effect = e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;

        private void Button1_Click(object sender, EventArgs e)
        {
            Md = ModuleDefMD.Load(textBox1.Text);
            foreach (Action func in _proc)
            {
                func();
            }
            var text2 = Path.GetDirectoryName(textBox1.Text);
            if (text2 != null && !text2.EndsWith("\\"))
                text2 += "\\";
            var path = $"{text2}{Path.GetFileNameWithoutExtension(textBox1.Text)}_protected{Path.GetExtension(textBox1.Text)}";

            var opts = new ModuleWriterOptions(Md)
            {
                Logger = DummyLogger.NoThrowInstance
            };
            Md.Write(path, opts);

            AppendMsg(richTextBox1, Color.Red, $"Save: {path}", true);
        }

        private readonly List<Action> _proc = new();

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                _proc.Add(ProcessStringEncrypt);
                listBox1.Items.Add("-> String Encrypt");
            }
            else
            {
                _proc.Remove(ProcessStringEncrypt);
                listBox1.Items.Remove("-> String Encrypt");
            }
        }

        private void ProcessStringEncrypt()
        {
            StringEncPhase.Execute(Md);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                _proc.Add(ProcessOnlineStringDecryption);
                listBox1.Items.Add("-> OnlineStrDecrypt");
            }
            else
            {
                _proc.Remove(ProcessOnlineStringDecryption);
                listBox1.Items.Remove("-> OnlineStrDecrypt");
            }
        }

        private void ProcessOnlineStringDecryption()
        {
            OnlinePhase.Execute(Md);
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                _proc.Add(ProcessControlFlow);
                listBox1.Items.Add("-> ControlFlow");
            }
            else
            {
                _proc.Remove(ProcessControlFlow);
                listBox1.Items.Remove("-> ControlFlow");
            }
        }

        private void ProcessControlFlow()
        {
            ControlFlowObfuscation.Execute(Md);
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                _proc.Add(ProcessIntConfusion);
                listBox1.Items.Add("-> IntConfusion");
            }
            else
            {
                _proc.Remove(ProcessIntConfusion);
                listBox1.Items.Remove("-> IntConfusion");
            }
        }

        private void ProcessIntConfusion()
        {
            AddIntPhase.Execute2(Md);
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked)
            {
                _proc.Add(ProcessArithmetic);
                listBox1.Items.Add("-> Arithmetic");
            }
            else
            {
                _proc.Remove(ProcessArithmetic);
                listBox1.Items.Remove("-> Arithmetic");
            }
        }

        private void ProcessArithmetic()
        {
            Arithmetic.Execute(Md);
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox6.Checked)
            {
                _proc.Add(ProcessLocalToField);
                listBox1.Items.Add("-> L2F");
            }
            else
            {
                _proc.Remove(ProcessLocalToField);
                listBox1.Items.Remove("-> L2F");
            }
        }

        private void ProcessLocalToField()
        {
            L2F.Execute(Md);
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox7.Checked)
            {
                _proc.Add(ProcessLocalToFieldV2);
                listBox1.Items.Add("-> L2FV2");
            }
            else
            {
                _proc.Remove(ProcessLocalToFieldV2);
                listBox1.Items.Remove("-> L2FV2");
            }
        }

        private void ProcessLocalToFieldV2()
        {
            L2FV2.Execute(Md);
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox8.Checked)
            {
                _proc.Add(ProcessCalli);
                listBox1.Items.Add("-> Calli");
            }
            else
            {
                _proc.Remove(ProcessCalli);
                listBox1.Items.Remove("-> Calli");
            }
        }

        private void ProcessCalli()
        {
            Calli.Execute(Md);
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox9.Checked)
            {
                _proc.Add(ProcessProxyMeth);
                listBox1.Items.Add("-> ProxyMeth");
            }
            else
            {
                _proc.Remove(ProcessProxyMeth);
                listBox1.Items.Remove("-> ProxyMeth");
            }
        }

        private void ProcessProxyMeth()
        {
            ProxyMeth.Execute(Md);
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox10.Checked)
            {
                _proc.Add(ProcessProxyInt);
                listBox1.Items.Add("-> ProxyInt");
            }
            else
            {
                _proc.Remove(ProcessProxyInt);
                listBox1.Items.Remove("-> ProxyInt");
            }
        }

        private void ProcessProxyInt()
        {
            ProxyInt.Execute(Md);
        }

        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox11.Checked)
            {
                _proc.Add(ProcessProxyString);
                listBox1.Items.Add("-> ProxyString");
            }
            else
            {
                _proc.Remove(ProcessProxyString);
                listBox1.Items.Remove("-> ProxyString");
            }
        }

        private void ProcessProxyString()
        {
            ProxyString.Execute(Md);
        }

        private void checkBox12_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox12.Checked)
            {
                _proc.Add(ProcessRenamer);
                listBox1.Items.Add("-> Renamer");
            }
            else
            {
                _proc.Remove(ProcessRenamer);
                listBox1.Items.Remove("-> Renamer");
            }
        }

        private void ProcessRenamer()
        {
            RenamerPhase.ExecuteClassRenaming(Md);
            RenamerPhase.ExecuteFieldRenaming(Md);
            RenamerPhase.ExecuteMethodRenaming(Md);
            RenamerPhase.ExecuteModuleRenaming(Md);
            RenamerPhase.ExecuteNamespaceRenaming(Md);
            RenamerPhase.ExecutePropertiesRenaming(Md);
        }

        private void checkBox13_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox13.Checked)
            {
                _proc.Add(ProcessJumpCflow);
                listBox1.Items.Add("-> JumpCflow");
            }
            else
            {
                _proc.Remove(ProcessJumpCflow);
                listBox1.Items.Remove("-> JumpCflow");
            }
        }

        private void ProcessJumpCflow()
        {
            JumpCFlow.Execute(Md);
        }

        private void checkBox14_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox14.Checked)
            {
                _proc.Add(ProcessAntiDebug);
                listBox1.Items.Add("-> Anti Debug");
            }
            else
            {
                _proc.Remove(ProcessAntiDebug);
                listBox1.Items.Remove("-> Anti Debug");
            }
        }

        private void ProcessAntiDebug()
        {
            AntiDebug.Execute(Md);
        }

        private void checkBox15_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox15.Checked)
            {
                _proc.Add(ProcessAntiDump);
                listBox1.Items.Add("-> Anti Dump");
            }
            else
            {
                _proc.Remove(ProcessAntiDump);
                listBox1.Items.Remove("-> Anti Dump");
            }
        }

        private void ProcessAntiDump()
        {
            AntiDump.Execute(Md);
        }

        private void checkBox16_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox16.Checked)
            {
                _proc.Add(ProcessAntiTamper);
                listBox1.Items.Add("-> Anti Tamper");
            }
            else
            {
                _proc.Remove(ProcessAntiTamper);
                listBox1.Items.Remove("-> Anti Tamper");
            }
        }

        private void ProcessAntiTamper()
        {
            AntiTamper.Execute(Md);
        }

        private void checkBox17_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox17.Checked)
            {
                _proc.Add(ProcessInvalidMd);
                listBox1.Items.Add("-> InvalidMD");
            }
            else
            {
                _proc.Remove(ProcessInvalidMd);
                listBox1.Items.Remove("-> InvalidMD");
            }
        }

        private void ProcessInvalidMd()
        {
            InvalidMDPhase.Execute(Md.Assembly);
        }

        private void checkBox18_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox18.Checked)
            {
                _proc.Add(ProcessAntiDe4dot);
                listBox1.Items.Add("-> AntiDe4dot");
            }
            else
            {
                _proc.Remove(ProcessAntiDe4dot);
                listBox1.Items.Remove("-> AntiDe4dot");
            }
        }

        private void ProcessAntiDe4dot()
        {
            AntiDe4dot.Execute(Md.Assembly);
        }

        private void checkBox19_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox19.Checked)
            {
                _proc.Add(ProcessStackUnfConfusion);
                listBox1.Items.Add("-> StackUnfConfusion");
            }
            else
            {
                _proc.Remove(ProcessStackUnfConfusion);
                listBox1.Items.Remove("-> StackUnfConfusion");
            }
        }

        private void ProcessStackUnfConfusion()
        {
            StackUnfConfusion.Execute(Md);
        }

        private void checkBox20_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox20.Checked)
            {
                _proc.Add(ProcessAntimanything);
                listBox1.Items.Add("-> Anti manything");
            }
            else
            {
                _proc.Remove(ProcessAntimanything);
                listBox1.Items.Remove("-> Anti manything");
            }
        }

        private void ProcessAntimanything()
        {
            Antimanything.Execute(Md);
        }
    }
}