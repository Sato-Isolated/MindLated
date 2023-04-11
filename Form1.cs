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

namespace MindLated;

public partial class Form1 : Form
{
    public static MethodDef? Init;
    public static MethodDef? Init2;
    private readonly List<Action> _func = new();
    private string _directoryName = string.Empty;

    public Form1() => InitializeComponent();

    private ModuleDefMD Md { get; set; } = null!;

    private static void AppendMsg(RichTextBox rtb, Color color, string text, bool autoTime)
    {
        rtb.BeginInvoke(new ThreadStart(() =>
        {
            lock (rtb)
            {
                rtb.Focus();
                if (rtb.TextLength > 100000) rtb.Clear();

                using var temp = new RichTextBox();
                temp.SelectionColor = color;
                if (autoTime)
                    temp.AppendText(DateTime.Now.ToString("HH:mm:ss") + " ");
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
            textBox1.Clear();
            var array = (Array)e.Data?.GetData(DataFormats.FileDrop)!;
            var text = array.GetValue(0)!.ToString();
            var num = text!.LastIndexOf(".", StringComparison.Ordinal);
            if (num == -1)
                return;
            var text2 = text[num..];
            text2 = text2.ToLower();
            if (string.Compare(text2, ".exe", StringComparison.Ordinal) != 0 &&
                string.Compare(text2, ".dll", StringComparison.Ordinal) != 0) return;

            Activate();
            textBox1.Text = text;
            var num2 = text.LastIndexOf("\\", StringComparison.Ordinal);
            if (num2 != -1) _directoryName = text.Remove(num2, text.Length - num2);

            if (_directoryName.Length == 2) _directoryName += "\\";
        }
        catch
        {
            /* ignored */
        }
    }

    private void TextBox1_DragEnter(object sender, DragEventArgs e)
    {
        e.Effect = e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }

    private void Button1_Click(object sender, EventArgs e)
    {
        Md = ModuleDefMD.Load(textBox1.Text);
        foreach (var func in _func) func();
        var text2 = Path.GetDirectoryName(textBox1.Text);
        if (text2 != null && !text2.EndsWith("\\"))
            text2 += "\\";
        var path =
            $"{text2}{Path.GetFileNameWithoutExtension(textBox1.Text)}_protected{Path.GetExtension(textBox1.Text)}";

        var opts = new ModuleWriterOptions(Md)
        {
            Logger = DummyLogger.NoThrowInstance
        };
        Md.Write(path, opts);

        AppendMsg(richTextBox1, Color.Red, $"Save: {path}", true);
    }

    private void CheckProcess(CheckBox check, Action action, string str)
    {
        if (check.Checked)
        {
            _func.Add(action);
            listBox1.Items.Add(str);
        }
        else
        {
            _func.Remove(action);
            listBox1.Items.Remove(str);
        }
    }

    private void RunProtection(Protection protect)
    {
        switch (protect)
        {
            case Protection.Calli:
                Calli.Execute(Md);
                break;
            case Protection.ControlFlow:
                ControlFlowObfuscation.Execute(Md);
                break;
            case Protection.InvalidMd:
                InvalidMDPhase.Execute(Md.Assembly);
                break;
            case Protection.StringProtect:
                StringEncPhase.Execute(Md);
                break;
            case Protection.OnlineString:
                OnlinePhase.Execute(Md);
                break;
            case Protection.LocalToField:
                L2F.Execute(Md);
                break;
            case Protection.LocalToFieldV2:
                L2FV2.Execute(Md);
                break;
            case Protection.Arithmetic:
                Arithmetic.Execute(Md);
                break;
            case Protection.IntConfusion:
                AddIntPhase.Execute2(Md);
                break;
            case Protection.ProxyString:
                ProxyString.Execute(Md);
                break;
            case Protection.ProxyInt:
                ProxyInt.Execute(Md);
                break;
            case Protection.AntiDebug:
                AntiDebug.Execute(Md);
                break;
            case Protection.AntiDump:
                AntiDump.Execute(Md);
                break;
            case Protection.AntiTamper:
                AntiTamper.Execute(Md);
                break;
            case Protection.AntiDe4dot:
                AntiDe4dot.Execute(Md.Assembly);
                break;
            case Protection.AntiManyThing:
                Antimanything.Execute(Md);
                break;
            case Protection.ProxyMeth:
                ProxyMeth.Execute(Md);
                break;
            case Protection.Watermark:
                Watermark.Execute(Md);
                break;
            case Protection.Renamer:
                RenamerPhase.ExecuteNamespaceRenaming(Md);
                RenamerPhase.ExecuteModuleRenaming(Md);
                RenamerPhase.ExecuteClassRenaming(Md);
                RenamerPhase.ExecutePropertiesRenaming(Md);
                RenamerPhase.ExecuteFieldRenaming(Md);
                RenamerPhase.ExecuteMethodRenaming(Md);
                break;
            case Protection.StackUnf:
                StackUnfConfusion.Execute(Md);
                break;
            case Protection.JumpCflow:
                JumpCFlow.Execute(Md);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        => CheckProcess(checkBox1, () => { RunProtection(Protection.StringProtect); }, "-> String Encrypt");

    private void checkBox2_CheckedChanged(object sender, EventArgs e)
        => CheckProcess(checkBox2, () => { RunProtection(Protection.OnlineString); }, "-> OnlineStrDecrypt");

    private void checkBox3_CheckedChanged(object sender, EventArgs e)
        => CheckProcess(checkBox3, () => { RunProtection(Protection.ControlFlow); }, "-> ControlFlow");

    private void checkBox4_CheckedChanged(object sender, EventArgs e)
        => CheckProcess(checkBox4, () => { RunProtection(Protection.IntConfusion); }, "-> IntConfusion");

    private void checkBox5_CheckedChanged(object sender, EventArgs e)
        => CheckProcess(checkBox5, () => { RunProtection(Protection.Arithmetic); }, "-> Arithmetic");

    private void checkBox6_CheckedChanged(object sender, EventArgs e)
        => CheckProcess(checkBox6, () => { RunProtection(Protection.LocalToField); }, "-> L2F");

    private void checkBox7_CheckedChanged(object sender, EventArgs e)
        => CheckProcess(checkBox7, () => { RunProtection(Protection.LocalToFieldV2); }, "-> L2F");

    private void checkBox8_CheckedChanged(object sender, EventArgs e)
        => CheckProcess(checkBox8, () => { RunProtection(Protection.Calli); }, "-> Calli");

    private void checkBox9_CheckedChanged(object sender, EventArgs e)
        => CheckProcess(checkBox9, () => { RunProtection(Protection.ProxyMeth); }, "-> ProxyMeth");

    private void checkBox10_CheckedChanged(object sender, EventArgs e)
        => CheckProcess(checkBox10, () => { RunProtection(Protection.ProxyInt); }, "-> ProxyInt");

    private void checkBox11_CheckedChanged(object sender, EventArgs e)
        => CheckProcess(checkBox11, () => { RunProtection(Protection.ProxyString); }, "-> ProxyString");

    private void checkBox12_CheckedChanged(object sender, EventArgs e)
        => CheckProcess(checkBox12, () => { RunProtection(Protection.Renamer); }, "-> Renamer");

    private void checkBox13_CheckedChanged(object sender, EventArgs e)
        => CheckProcess(checkBox13, () => { RunProtection(Protection.JumpCflow); }, "-> JumpCflow");

    private void checkBox14_CheckedChanged(object sender, EventArgs e)
        => CheckProcess(checkBox14, () => { RunProtection(Protection.AntiDebug); }, "-> Anti Debug");

    private void checkBox15_CheckedChanged(object sender, EventArgs e)
        => CheckProcess(checkBox15, () => { RunProtection(Protection.AntiDump); }, "-> Anti Dump");

    private void checkBox16_CheckedChanged(object sender, EventArgs e)
        => CheckProcess(checkBox16, () => { RunProtection(Protection.AntiTamper); }, "-> Anti Tamper");

    private void checkBox17_CheckedChanged(object sender, EventArgs e)
        => CheckProcess(checkBox17, () => { RunProtection(Protection.InvalidMd); }, "-> InvalidMD");

    private void checkBox18_CheckedChanged(object sender, EventArgs e)
        => CheckProcess(checkBox18, () => { RunProtection(Protection.AntiDe4dot); }, "-> AntiDe4dot");

    private void checkBox19_CheckedChanged(object sender, EventArgs e)
        => CheckProcess(checkBox19, () => { RunProtection(Protection.StackUnf); }, "-> StackUnfConfusion");

    private void checkBox20_CheckedChanged(object sender, EventArgs e)
        => CheckProcess(checkBox20, () => { RunProtection(Protection.AntiManyThing); }, "-> Anti manything");

    private enum Protection
    {
        Calli,
        ControlFlow,
        InvalidMd,
        LocalToField,
        LocalToFieldV2,
        Arithmetic,
        IntConfusion,
        StackUnf,
        ProxyString,
        ProxyMeth,
        StringProtect,
        OnlineString,
        AntiDebug,
        AntiDump,
        AntiTamper,
        AntiDe4dot,
        AntiManyThing,
        Watermark,
        Renamer,
        JumpCflow,
        ProxyInt
    }
}