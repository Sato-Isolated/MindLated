namespace MindLated
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.cB_StringEncryption = new System.Windows.Forms.CheckBox();
            this.cB_StringOnlineDecryption = new System.Windows.Forms.CheckBox();
            this.cB_ControlFlow = new System.Windows.Forms.CheckBox();
            this.cB_IntConfusion = new System.Windows.Forms.CheckBox();
            this.cB_Arithmetic = new System.Windows.Forms.CheckBox();
            this.cB_Local2Field = new System.Windows.Forms.CheckBox();
            this.cB_Local2FieldV2 = new System.Windows.Forms.CheckBox();
            this.cB_Calli = new System.Windows.Forms.CheckBox();
            this.cB_ProxyMeth = new System.Windows.Forms.CheckBox();
            this.cB_ProxyInt = new System.Windows.Forms.CheckBox();
            this.cB_ProxyStrings = new System.Windows.Forms.CheckBox();
            this.cB_Renamer = new System.Windows.Forms.CheckBox();
            this.cB_JumpCflow = new System.Windows.Forms.CheckBox();
            this.cB_AntiDebug = new System.Windows.Forms.CheckBox();
            this.cB_AntiDump = new System.Windows.Forms.CheckBox();
            this.cB_AntiTamper = new System.Windows.Forms.CheckBox();
            this.cB_InvalidMD = new System.Windows.Forms.CheckBox();
            this.cB_AntiD4D = new System.Windows.Forms.CheckBox();
            this.cB_StackUnfConfusion = new System.Windows.Forms.CheckBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.cB_AntiManything = new System.Windows.Forms.CheckBox();
            this.btn_file = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btn_select = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.AllowDrop = true;
            this.textBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(55)))));
            this.textBox1.ForeColor = System.Drawing.Color.White;
            this.textBox1.Location = new System.Drawing.Point(9, 9);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(376, 23);
            this.textBox1.TabIndex = 0;
            this.textBox1.DragDrop += new System.Windows.Forms.DragEventHandler(this.TextBox1_DragDrop);
            this.textBox1.DragEnter += new System.Windows.Forms.DragEventHandler(this.TextBox1_DragEnter);
            // 
            // listBox1
            // 
            this.listBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.listBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listBox1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.listBox1.ForeColor = System.Drawing.Color.White;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 17;
            this.listBox1.Items.AddRange(new object[] {
            "Selected Obfuscation"});
            this.listBox1.Location = new System.Drawing.Point(425, 9);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(144, 359);
            this.listBox1.TabIndex = 1;
            // 
            // cB_StringEncryption
            // 
            this.cB_StringEncryption.AutoSize = true;
            this.cB_StringEncryption.ForeColor = System.Drawing.SystemColors.Control;
            this.cB_StringEncryption.Location = new System.Drawing.Point(10, 3);
            this.cB_StringEncryption.Name = "cB_StringEncryption";
            this.cB_StringEncryption.Size = new System.Drawing.Size(117, 19);
            this.cB_StringEncryption.TabIndex = 2;
            this.cB_StringEncryption.Text = "String Encryption";
            this.cB_StringEncryption.UseVisualStyleBackColor = true;
            this.cB_StringEncryption.CheckedChanged += new System.EventHandler(this.cB_StringEncryption_CheckedChanged);
            // 
            // cB_StringOnlineDecryption
            // 
            this.cB_StringOnlineDecryption.AutoSize = true;
            this.cB_StringOnlineDecryption.ForeColor = System.Drawing.SystemColors.Control;
            this.cB_StringOnlineDecryption.Location = new System.Drawing.Point(10, 28);
            this.cB_StringOnlineDecryption.Name = "cB_StringOnlineDecryption";
            this.cB_StringOnlineDecryption.Size = new System.Drawing.Size(156, 19);
            this.cB_StringOnlineDecryption.TabIndex = 3;
            this.cB_StringOnlineDecryption.Text = "String Online Decryption";
            this.cB_StringOnlineDecryption.UseVisualStyleBackColor = true;
            this.cB_StringOnlineDecryption.CheckedChanged += new System.EventHandler(this.cB_StringOnlineDecryption_CheckedChanged);
            // 
            // cB_ControlFlow
            // 
            this.cB_ControlFlow.AutoSize = true;
            this.cB_ControlFlow.ForeColor = System.Drawing.SystemColors.Control;
            this.cB_ControlFlow.Location = new System.Drawing.Point(10, 53);
            this.cB_ControlFlow.Name = "cB_ControlFlow";
            this.cB_ControlFlow.Size = new System.Drawing.Size(94, 19);
            this.cB_ControlFlow.TabIndex = 4;
            this.cB_ControlFlow.Text = "Control Flow";
            this.cB_ControlFlow.UseVisualStyleBackColor = true;
            this.cB_ControlFlow.CheckedChanged += new System.EventHandler(this.cB_ControlFlow_CheckedChanged);
            // 
            // cB_IntConfusion
            // 
            this.cB_IntConfusion.AutoSize = true;
            this.cB_IntConfusion.ForeColor = System.Drawing.SystemColors.Control;
            this.cB_IntConfusion.Location = new System.Drawing.Point(10, 78);
            this.cB_IntConfusion.Name = "cB_IntConfusion";
            this.cB_IntConfusion.Size = new System.Drawing.Size(98, 19);
            this.cB_IntConfusion.TabIndex = 5;
            this.cB_IntConfusion.Text = "Int Confusion";
            this.cB_IntConfusion.UseVisualStyleBackColor = true;
            this.cB_IntConfusion.CheckedChanged += new System.EventHandler(this.cB_IntConfusion_CheckedChanged);
            // 
            // cB_Arithmetic
            // 
            this.cB_Arithmetic.AutoSize = true;
            this.cB_Arithmetic.ForeColor = System.Drawing.SystemColors.Control;
            this.cB_Arithmetic.Location = new System.Drawing.Point(10, 102);
            this.cB_Arithmetic.Name = "cB_Arithmetic";
            this.cB_Arithmetic.Size = new System.Drawing.Size(82, 19);
            this.cB_Arithmetic.TabIndex = 6;
            this.cB_Arithmetic.Text = "Arithmetic";
            this.cB_Arithmetic.UseVisualStyleBackColor = true;
            this.cB_Arithmetic.CheckedChanged += new System.EventHandler(this.cB_Arithmetic_CheckedChanged);
            // 
            // cB_Local2Field
            // 
            this.cB_Local2Field.AutoSize = true;
            this.cB_Local2Field.ForeColor = System.Drawing.SystemColors.Control;
            this.cB_Local2Field.Location = new System.Drawing.Point(10, 128);
            this.cB_Local2Field.Name = "cB_Local2Field";
            this.cB_Local2Field.Size = new System.Drawing.Size(97, 19);
            this.cB_Local2Field.TabIndex = 7;
            this.cB_Local2Field.Text = "Local To Field";
            this.cB_Local2Field.UseVisualStyleBackColor = true;
            this.cB_Local2Field.CheckedChanged += new System.EventHandler(this.cB_Local2Field_CheckedChanged);
            // 
            // cB_Local2FieldV2
            // 
            this.cB_Local2FieldV2.AutoSize = true;
            this.cB_Local2FieldV2.ForeColor = System.Drawing.SystemColors.Control;
            this.cB_Local2FieldV2.Location = new System.Drawing.Point(10, 153);
            this.cB_Local2FieldV2.Name = "cB_Local2FieldV2";
            this.cB_Local2FieldV2.Size = new System.Drawing.Size(113, 19);
            this.cB_Local2FieldV2.TabIndex = 8;
            this.cB_Local2FieldV2.Text = "Local To Field V2";
            this.cB_Local2FieldV2.UseVisualStyleBackColor = true;
            this.cB_Local2FieldV2.CheckedChanged += new System.EventHandler(this.cB_Local2FieldV2_CheckedChanged);
            // 
            // cB_Calli
            // 
            this.cB_Calli.AutoSize = true;
            this.cB_Calli.ForeColor = System.Drawing.SystemColors.Control;
            this.cB_Calli.Location = new System.Drawing.Point(172, 153);
            this.cB_Calli.Name = "cB_Calli";
            this.cB_Calli.Size = new System.Drawing.Size(49, 19);
            this.cB_Calli.TabIndex = 9;
            this.cB_Calli.Text = "Calli";
            this.cB_Calli.UseVisualStyleBackColor = true;
            this.cB_Calli.CheckedChanged += new System.EventHandler(this.cB_Calli_CheckedChanged);
            // 
            // cB_ProxyMeth
            // 
            this.cB_ProxyMeth.AutoSize = true;
            this.cB_ProxyMeth.ForeColor = System.Drawing.SystemColors.Control;
            this.cB_ProxyMeth.Location = new System.Drawing.Point(172, 3);
            this.cB_ProxyMeth.Name = "cB_ProxyMeth";
            this.cB_ProxyMeth.Size = new System.Drawing.Size(87, 19);
            this.cB_ProxyMeth.TabIndex = 10;
            this.cB_ProxyMeth.Text = "Proxy Meth";
            this.cB_ProxyMeth.UseVisualStyleBackColor = true;
            this.cB_ProxyMeth.CheckedChanged += new System.EventHandler(this.cB_ProxyMeth_CheckedChanged);
            // 
            // cB_ProxyInt
            // 
            this.cB_ProxyInt.AutoSize = true;
            this.cB_ProxyInt.ForeColor = System.Drawing.SystemColors.Control;
            this.cB_ProxyInt.Location = new System.Drawing.Point(172, 28);
            this.cB_ProxyInt.Name = "cB_ProxyInt";
            this.cB_ProxyInt.Size = new System.Drawing.Size(73, 19);
            this.cB_ProxyInt.TabIndex = 11;
            this.cB_ProxyInt.Text = "Proxy Int";
            this.cB_ProxyInt.UseVisualStyleBackColor = true;
            this.cB_ProxyInt.CheckedChanged += new System.EventHandler(this.cB_ProxyInt_CheckedChanged);
            // 
            // cB_ProxyStrings
            // 
            this.cB_ProxyStrings.AutoSize = true;
            this.cB_ProxyStrings.ForeColor = System.Drawing.SystemColors.Control;
            this.cB_ProxyStrings.Location = new System.Drawing.Point(172, 53);
            this.cB_ProxyStrings.Name = "cB_ProxyStrings";
            this.cB_ProxyStrings.Size = new System.Drawing.Size(95, 19);
            this.cB_ProxyStrings.TabIndex = 12;
            this.cB_ProxyStrings.Text = "Proxy Strings";
            this.cB_ProxyStrings.UseVisualStyleBackColor = true;
            this.cB_ProxyStrings.CheckedChanged += new System.EventHandler(this.cB_ProxyStrings_CheckedChanged);
            // 
            // cB_Renamer
            // 
            this.cB_Renamer.AutoSize = true;
            this.cB_Renamer.ForeColor = System.Drawing.SystemColors.Control;
            this.cB_Renamer.Location = new System.Drawing.Point(172, 77);
            this.cB_Renamer.Name = "cB_Renamer";
            this.cB_Renamer.Size = new System.Drawing.Size(73, 19);
            this.cB_Renamer.TabIndex = 13;
            this.cB_Renamer.Text = "Renamer";
            this.cB_Renamer.UseVisualStyleBackColor = true;
            this.cB_Renamer.CheckedChanged += new System.EventHandler(this.cB_Renamer_CheckedChanged);
            // 
            // cB_JumpCflow
            // 
            this.cB_JumpCflow.AutoSize = true;
            this.cB_JumpCflow.ForeColor = System.Drawing.SystemColors.Control;
            this.cB_JumpCflow.Location = new System.Drawing.Point(172, 102);
            this.cB_JumpCflow.Name = "cB_JumpCflow";
            this.cB_JumpCflow.Size = new System.Drawing.Size(86, 19);
            this.cB_JumpCflow.TabIndex = 14;
            this.cB_JumpCflow.Text = "JumpCflow";
            this.cB_JumpCflow.UseVisualStyleBackColor = true;
            this.cB_JumpCflow.CheckedChanged += new System.EventHandler(this.cB_JumpCflow_CheckedChanged);
            // 
            // cB_AntiDebug
            // 
            this.cB_AntiDebug.AutoSize = true;
            this.cB_AntiDebug.ForeColor = System.Drawing.SystemColors.Control;
            this.cB_AntiDebug.Location = new System.Drawing.Point(172, 127);
            this.cB_AntiDebug.Name = "cB_AntiDebug";
            this.cB_AntiDebug.Size = new System.Drawing.Size(86, 19);
            this.cB_AntiDebug.TabIndex = 15;
            this.cB_AntiDebug.Text = "Anti Debug";
            this.cB_AntiDebug.UseVisualStyleBackColor = true;
            this.cB_AntiDebug.CheckedChanged += new System.EventHandler(this.cB_AntiDebug_CheckedChanged);
            // 
            // cB_AntiDump
            // 
            this.cB_AntiDump.AutoSize = true;
            this.cB_AntiDump.ForeColor = System.Drawing.SystemColors.Control;
            this.cB_AntiDump.Location = new System.Drawing.Point(267, 128);
            this.cB_AntiDump.Name = "cB_AntiDump";
            this.cB_AntiDump.Size = new System.Drawing.Size(84, 19);
            this.cB_AntiDump.TabIndex = 16;
            this.cB_AntiDump.Text = "Anti Dump";
            this.cB_AntiDump.UseVisualStyleBackColor = true;
            this.cB_AntiDump.CheckedChanged += new System.EventHandler(this.cB_AntiDump_CheckedChanged);
            // 
            // cB_AntiTamper
            // 
            this.cB_AntiTamper.AutoSize = true;
            this.cB_AntiTamper.ForeColor = System.Drawing.SystemColors.Control;
            this.cB_AntiTamper.Location = new System.Drawing.Point(267, 103);
            this.cB_AntiTamper.Name = "cB_AntiTamper";
            this.cB_AntiTamper.Size = new System.Drawing.Size(90, 19);
            this.cB_AntiTamper.TabIndex = 17;
            this.cB_AntiTamper.Text = "Anti Tamper";
            this.cB_AntiTamper.UseVisualStyleBackColor = true;
            this.cB_AntiTamper.CheckedChanged += new System.EventHandler(this.cB_AntiTamper_CheckedChanged);
            // 
            // cB_InvalidMD
            // 
            this.cB_InvalidMD.AutoSize = true;
            this.cB_InvalidMD.ForeColor = System.Drawing.SystemColors.Control;
            this.cB_InvalidMD.Location = new System.Drawing.Point(267, 3);
            this.cB_InvalidMD.Name = "cB_InvalidMD";
            this.cB_InvalidMD.Size = new System.Drawing.Size(83, 19);
            this.cB_InvalidMD.TabIndex = 18;
            this.cB_InvalidMD.Text = "Invalid MD";
            this.cB_InvalidMD.UseVisualStyleBackColor = true;
            this.cB_InvalidMD.CheckedChanged += new System.EventHandler(this.cB_InvalidMD_CheckedChanged);
            // 
            // cB_AntiD4D
            // 
            this.cB_AntiD4D.AutoSize = true;
            this.cB_AntiD4D.ForeColor = System.Drawing.SystemColors.Control;
            this.cB_AntiD4D.Location = new System.Drawing.Point(267, 28);
            this.cB_AntiD4D.Name = "cB_AntiD4D";
            this.cB_AntiD4D.Size = new System.Drawing.Size(88, 19);
            this.cB_AntiD4D.TabIndex = 19;
            this.cB_AntiD4D.Text = "Anti de4dot";
            this.cB_AntiD4D.UseVisualStyleBackColor = true;
            this.cB_AntiD4D.CheckedChanged += new System.EventHandler(this.cB_AntiD4D_CheckedChanged);
            // 
            // cB_StackUnfConfusion
            // 
            this.cB_StackUnfConfusion.AutoSize = true;
            this.cB_StackUnfConfusion.ForeColor = System.Drawing.SystemColors.Control;
            this.cB_StackUnfConfusion.Location = new System.Drawing.Point(267, 53);
            this.cB_StackUnfConfusion.Name = "cB_StackUnfConfusion";
            this.cB_StackUnfConfusion.Size = new System.Drawing.Size(128, 19);
            this.cB_StackUnfConfusion.TabIndex = 20;
            this.cB_StackUnfConfusion.Text = "StackUnfConfusion";
            this.cB_StackUnfConfusion.UseVisualStyleBackColor = true;
            this.cB_StackUnfConfusion.CheckedChanged += new System.EventHandler(this.cB_StackUnfConfusion_CheckedChanged);
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBox1.ForeColor = System.Drawing.Color.White;
            this.richTextBox1.Location = new System.Drawing.Point(9, 250);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(410, 118);
            this.richTextBox1.TabIndex = 21;
            this.richTextBox1.Text = "";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(55)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Location = new System.Drawing.Point(182, 221);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(237, 23);
            this.button1.TabIndex = 22;
            this.button1.Text = "Obfuscate";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // cB_AntiManything
            // 
            this.cB_AntiManything.AutoSize = true;
            this.cB_AntiManything.ForeColor = System.Drawing.SystemColors.Control;
            this.cB_AntiManything.Location = new System.Drawing.Point(267, 78);
            this.cB_AntiManything.Name = "cB_AntiManything";
            this.cB_AntiManything.Size = new System.Drawing.Size(109, 19);
            this.cB_AntiManything.TabIndex = 23;
            this.cB_AntiManything.Text = "Anti Manything";
            this.cB_AntiManything.UseVisualStyleBackColor = true;
            this.cB_AntiManything.CheckedChanged += new System.EventHandler(this.cB_AntiManything_CheckedChanged);
            // 
            // btn_file
            // 
            this.btn_file.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(55)))));
            this.btn_file.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_file.ForeColor = System.Drawing.Color.White;
            this.btn_file.Location = new System.Drawing.Point(391, 9);
            this.btn_file.Name = "btn_file";
            this.btn_file.Size = new System.Drawing.Size(28, 23);
            this.btn_file.TabIndex = 24;
            this.btn_file.Text = "...";
            this.btn_file.UseVisualStyleBackColor = false;
            this.btn_file.Click += new System.EventHandler(this.btn_file_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.cB_ControlFlow);
            this.panel1.Controls.Add(this.cB_AntiManything);
            this.panel1.Controls.Add(this.cB_StringEncryption);
            this.panel1.Controls.Add(this.cB_StringOnlineDecryption);
            this.panel1.Controls.Add(this.cB_IntConfusion);
            this.panel1.Controls.Add(this.cB_StackUnfConfusion);
            this.panel1.Controls.Add(this.cB_Arithmetic);
            this.panel1.Controls.Add(this.cB_AntiD4D);
            this.panel1.Controls.Add(this.cB_Local2Field);
            this.panel1.Controls.Add(this.cB_InvalidMD);
            this.panel1.Controls.Add(this.cB_Local2FieldV2);
            this.panel1.Controls.Add(this.cB_AntiTamper);
            this.panel1.Controls.Add(this.cB_Calli);
            this.panel1.Controls.Add(this.cB_AntiDump);
            this.panel1.Controls.Add(this.cB_ProxyMeth);
            this.panel1.Controls.Add(this.cB_AntiDebug);
            this.panel1.Controls.Add(this.cB_ProxyInt);
            this.panel1.Controls.Add(this.cB_JumpCflow);
            this.panel1.Controls.Add(this.cB_ProxyStrings);
            this.panel1.Controls.Add(this.cB_Renamer);
            this.panel1.Location = new System.Drawing.Point(9, 38);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(410, 177);
            this.panel1.TabIndex = 25;
            // 
            // btn_select
            // 
            this.btn_select.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(55)))));
            this.btn_select.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_select.ForeColor = System.Drawing.Color.White;
            this.btn_select.Location = new System.Drawing.Point(9, 221);
            this.btn_select.Name = "btn_select";
            this.btn_select.Size = new System.Drawing.Size(167, 23);
            this.btn_select.TabIndex = 26;
            this.btn_select.Text = "Select all";
            this.btn_select.UseVisualStyleBackColor = false;
            this.btn_select.Click += new System.EventHandler(this.btn_select_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(9, 392);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(560, 17);
            this.progressBar1.TabIndex = 27;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(6, 371);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 19);
            this.label1.TabIndex = 28;
            this.label1.Text = "Status:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(53, 371);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 19);
            this.label2.TabIndex = 29;
            this.label2.Text = "Idle";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.ClientSize = new System.Drawing.Size(578, 416);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btn_select);
            this.Controls.Add(this.btn_file);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MindLated | Custom Mod by KorolBlack";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.CheckBox cB_StringEncryption;
        private System.Windows.Forms.CheckBox cB_StringOnlineDecryption;
        private System.Windows.Forms.CheckBox cB_ControlFlow;
        private System.Windows.Forms.CheckBox cB_IntConfusion;
        private System.Windows.Forms.CheckBox cB_Arithmetic;
        private System.Windows.Forms.CheckBox cB_Local2Field;
        private System.Windows.Forms.CheckBox cB_Local2FieldV2;
        private System.Windows.Forms.CheckBox cB_Calli;
        private System.Windows.Forms.CheckBox cB_ProxyMeth;
        private System.Windows.Forms.CheckBox cB_ProxyInt;
        private System.Windows.Forms.CheckBox cB_ProxyStrings;
        private System.Windows.Forms.CheckBox cB_Renamer;
        private System.Windows.Forms.CheckBox cB_JumpCflow;
        private System.Windows.Forms.CheckBox cB_AntiDebug;
        private System.Windows.Forms.CheckBox cB_AntiDump;
        private System.Windows.Forms.CheckBox cB_AntiTamper;
        private System.Windows.Forms.CheckBox cB_InvalidMD;
        private System.Windows.Forms.CheckBox cB_AntiD4D;
        private System.Windows.Forms.CheckBox cB_StackUnfConfusion;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox cB_AntiManything;
        private System.Windows.Forms.Button btn_file;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btn_select;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}
