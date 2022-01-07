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
                var array = (Array)e.Data.GetData(DataFormats.FileDrop);
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

        private void TextBox1_DragEnter(object sender, DragEventArgs e) => e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;

        private void Button1_Click(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show("No file found!", "Error!");
            }
        }

        private readonly List<Action> _proc = new(); 


        private void btn_file_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select your file";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = ofd.FileName;
            }
        }

        private void btn_select_Click(object sender, EventArgs e)
        {
            string selectAll = "Select all";
            string unselectAll = "Unselect all";
            if (btn_select.Text == selectAll)
            {
                foreach (CheckBox checkBox in panel1.Controls)
                {
                    checkBox.Checked = true;
                }
                btn_select.Text = unselectAll;
            }
            else
            {
                foreach (CheckBox checkbox in panel1.Controls)
                {
                    checkbox.Checked = false;
                }
                btn_select.Text = selectAll;
            }
        }

        //////////////////////////////////////
        ///   Method for adjusting list
        /////////////////////////////////////

        private void adjustProcess(CheckBox checkBox, Action action, string item)
        {
            if (checkBox.Checked)
            {
                _proc.Add(action);
                listBox1.Items.Add(item);
            }
            else
            {
                _proc.Remove(action);
                listBox1.Items.Remove(item);
            }
        }

        //////////////////////////////////////
        ///       String encryption
        /////////////////////////////////////

        private void cB_StringEncryption_CheckedChanged(object sender, EventArgs e)
        {
            adjustProcess(cB_StringEncryption, ProcessStringEncrypt, "-> String Encrypt");
        }

        private void ProcessStringEncrypt()
        {
            StringEncPhase.Execute(Md);
        }

        //////////////////////////////////////
        ///    String Online Decryption
        /////////////////////////////////////

        private void cB_StringOnlineDecryption_CheckedChanged(object sender, EventArgs e)
        {
            adjustProcess(cB_StringOnlineDecryption, ProcessOnlineStringDecryption, "-> OnlineStrDecrypt");
        }

        private void ProcessOnlineStringDecryption()
        {
            OnlinePhase.Execute(Md);
        }

        //////////////////////////////////////
        ///         Control Flow
        /////////////////////////////////////

        private void cB_ControlFlow_CheckedChanged(object sender, EventArgs e)
        {
            adjustProcess(cB_ControlFlow, ProcessControlFlow, "-> ControlFlow");
        }

        private void ProcessControlFlow()
        {
            ControlFlowObfuscation.Execute(Md);
        }

        //////////////////////////////////////
        ///          IntConfusion
        /////////////////////////////////////

        private void cB_IntConfusion_CheckedChanged(object sender, EventArgs e)
        {
            adjustProcess(cB_IntConfusion, ProcessIntConfusion, "-> IntConfusion");
        }

        private void ProcessIntConfusion()
        {
            AddIntPhase.Execute2(Md);
        }

        //////////////////////////////////////
        ///           Arithmetic
        /////////////////////////////////////
        private void cB_Arithmetic_CheckedChanged(object sender, EventArgs e)
        {
            adjustProcess(cB_Arithmetic, ProcessArithmetic, "-> Arithmetic");
        }

        private void ProcessArithmetic()
        {
            Arithmetic.Execute(Md);
        }

        //////////////////////////////////////
        ///         Local to Field
        /////////////////////////////////////

        private void cB_Local2Field_CheckedChanged(object sender, EventArgs e)
        {
            adjustProcess(cB_Local2Field, ProcessLocalToField, "-> L2F");
        }

        private void ProcessLocalToField()
        {
            L2F.Execute(Md);
        }

        //////////////////////////////////////
        ///        Local to Field V2
        /////////////////////////////////////

        private void cB_Local2FieldV2_CheckedChanged(object sender, EventArgs e)
        {
            adjustProcess(cB_Local2FieldV2, ProcessLocalToFieldV2, "-> L2FV2");
        }

        private void ProcessLocalToFieldV2()
        {
            L2FV2.Execute(Md);
        }

        //////////////////////////////////////
        ///           Proxy Meth
        /////////////////////////////////////

        private void cB_ProxyMeth_CheckedChanged(object sender, EventArgs e)
        {
            adjustProcess(cB_ProxyMeth, ProcessProxyMeth, "-> ProxyMeth");
        }

        private void ProcessProxyMeth()
        {
            ProxyMeth.Execute(Md);
        }

        //////////////////////////////////////
        ///           Proxy Int
        /////////////////////////////////////

        private void cB_ProxyInt_CheckedChanged(object sender, EventArgs e)
        {
            adjustProcess(cB_ProxyInt, ProcessProxyInt, "-> ProxyInt");
        }

        private void ProcessProxyInt()
        {
            ProxyInt.Execute(Md);
        }

        //////////////////////////////////////
        ///         Proxy Strings
        /////////////////////////////////////

        private void cB_ProxyStrings_CheckedChanged(object sender, EventArgs e)
        {
            adjustProcess(cB_ProxyStrings, ProcessProxyString, "-> ProxyString");
        }

        private void ProcessProxyString()
        {
            ProxyString.Execute(Md);
        }

        //////////////////////////////////////
        ///             ReName
        /////////////////////////////////////

        private void cB_Renamer_CheckedChanged(object sender, EventArgs e)
        {
            adjustProcess(cB_Renamer, ProcessRenamer, "-> Renamer");
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

        //////////////////////////////////////
        ///          JumpCFlow
        /////////////////////////////////////

        private void cB_JumpCflow_CheckedChanged(object sender, EventArgs e)
        {
            adjustProcess(cB_JumpCflow, ProcessJumpCflow, "-> JumpCflow");
        }

        private void ProcessJumpCflow()
        {
            JumpCFlow.Execute(Md);
        }

        //////////////////////////////////////
        ///           AntiDebug
        /////////////////////////////////////

        private void cB_AntiDebug_CheckedChanged(object sender, EventArgs e)
        {
            adjustProcess(cB_AntiDebug, ProcessAntiDebug, "-> Anti Debug");
        }

        private void ProcessAntiDebug()
        {
            AntiDebug.Execute(Md);
        }

        //////////////////////////////////////
        ///             Calli
        /////////////////////////////////////

        private void cB_Calli_CheckedChanged(object sender, EventArgs e)
        {
            adjustProcess(cB_Calli, ProcessCalli, "-> Calli");
        }

        private void ProcessCalli()
        {
            Calli.Execute(Md);
        }

        //////////////////////////////////////
        ///          Invalid MD
        /////////////////////////////////////

        private void cB_InvalidMD_CheckedChanged(object sender, EventArgs e)
        {
            adjustProcess(cB_InvalidMD, ProcessInvalidMd, "-> InvalidMD");
        }

        private void ProcessInvalidMd()
        {
            InvalidMDPhase.Execute(Md.Assembly);
        }


        //////////////////////////////////////
        ///           Anti De4Dot
        /////////////////////////////////////

        private void cB_AntiD4D_CheckedChanged(object sender, EventArgs e)
        {
            adjustProcess(cB_AntiD4D, ProcessAntiDe4dot, "-> AntiDe4dot");
        }

        private void ProcessAntiDe4dot()
        {
            AntiDe4dot.Execute(Md.Assembly);
        }

        //////////////////////////////////////
        ///       StackUnfConfusion
        /////////////////////////////////////
        
        private void cB_StackUnfConfusion_CheckedChanged(object sender, EventArgs e)
        {
            adjustProcess(cB_StackUnfConfusion, ProcessStackUnfConfusion, "-> StackUnfConfusion");
        }

        private void ProcessStackUnfConfusion()
        {
            StackUnfConfusion.Execute(Md);
        }

        //////////////////////////////////////
        ///         AntiManything
        /////////////////////////////////////

        private void cB_AntiManything_CheckedChanged(object sender, EventArgs e)
        {
            adjustProcess(cB_AntiManything, ProcessAntimanything, "-> Anti manything");
        }

        private void ProcessAntimanything()
        {
            Antimanything.Execute(Md);
        }

        //////////////////////////////////////
        ///          Anti Tamper
        /////////////////////////////////////

        private void cB_AntiTamper_CheckedChanged(object sender, EventArgs e)
        {
            adjustProcess(cB_AntiTamper, ProcessAntiTamper, "-> Anti Tamper");
        }

        private void ProcessAntiTamper()
        {
            AntiTamper.Execute(Md);
        }

        //////////////////////////////////////
        ///           Anti Dump
        /////////////////////////////////////

        private void cB_AntiDump_CheckedChanged(object sender, EventArgs e)
        {
            adjustProcess(cB_AntiDump, ProcessAntiDump, "-> Anti Dump");
        }

        private void ProcessAntiDump()
        {
            AntiDump.Execute(Md);
        }
    }
}