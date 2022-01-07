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
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.checkBox6 = new System.Windows.Forms.CheckBox();
            this.checkBox7 = new System.Windows.Forms.CheckBox();
            this.checkBox8 = new System.Windows.Forms.CheckBox();
            this.checkBox9 = new System.Windows.Forms.CheckBox();
            this.checkBox10 = new System.Windows.Forms.CheckBox();
            this.checkBox11 = new System.Windows.Forms.CheckBox();
            this.checkBox12 = new System.Windows.Forms.CheckBox();
            this.checkBox13 = new System.Windows.Forms.CheckBox();
            this.checkBox14 = new System.Windows.Forms.CheckBox();
            this.checkBox15 = new System.Windows.Forms.CheckBox();
            this.checkBox16 = new System.Windows.Forms.CheckBox();
            this.checkBox17 = new System.Windows.Forms.CheckBox();
            this.checkBox18 = new System.Windows.Forms.CheckBox();
            this.checkBox19 = new System.Windows.Forms.CheckBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBox20 = new System.Windows.Forms.CheckBox();
            this.btn_file = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btn_select = new System.Windows.Forms.Button();
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
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.ForeColor = System.Drawing.SystemColors.Control;
            this.checkBox1.Location = new System.Drawing.Point(10, 3);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(117, 19);
            this.checkBox1.TabIndex = 2;
            this.checkBox1.Text = "String Encryption";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.CheckBox1_CheckedChanged);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.ForeColor = System.Drawing.SystemColors.Control;
            this.checkBox2.Location = new System.Drawing.Point(10, 28);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(156, 19);
            this.checkBox2.TabIndex = 3;
            this.checkBox2.Text = "String Online Decryption";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.ForeColor = System.Drawing.SystemColors.Control;
            this.checkBox3.Location = new System.Drawing.Point(10, 53);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(94, 19);
            this.checkBox3.TabIndex = 4;
            this.checkBox3.Text = "Control Flow";
            this.checkBox3.UseVisualStyleBackColor = true;
            this.checkBox3.CheckedChanged += new System.EventHandler(this.checkBox3_CheckedChanged);
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.ForeColor = System.Drawing.SystemColors.Control;
            this.checkBox4.Location = new System.Drawing.Point(10, 78);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(98, 19);
            this.checkBox4.TabIndex = 5;
            this.checkBox4.Text = "Int Confusion";
            this.checkBox4.UseVisualStyleBackColor = true;
            this.checkBox4.CheckedChanged += new System.EventHandler(this.checkBox4_CheckedChanged);
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.ForeColor = System.Drawing.SystemColors.Control;
            this.checkBox5.Location = new System.Drawing.Point(10, 102);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(82, 19);
            this.checkBox5.TabIndex = 6;
            this.checkBox5.Text = "Arithmetic";
            this.checkBox5.UseVisualStyleBackColor = true;
            this.checkBox5.CheckedChanged += new System.EventHandler(this.checkBox5_CheckedChanged);
            // 
            // checkBox6
            // 
            this.checkBox6.AutoSize = true;
            this.checkBox6.ForeColor = System.Drawing.SystemColors.Control;
            this.checkBox6.Location = new System.Drawing.Point(10, 128);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(97, 19);
            this.checkBox6.TabIndex = 7;
            this.checkBox6.Text = "Local To Field";
            this.checkBox6.UseVisualStyleBackColor = true;
            this.checkBox6.CheckedChanged += new System.EventHandler(this.checkBox6_CheckedChanged);
            // 
            // checkBox7
            // 
            this.checkBox7.AutoSize = true;
            this.checkBox7.ForeColor = System.Drawing.SystemColors.Control;
            this.checkBox7.Location = new System.Drawing.Point(10, 153);
            this.checkBox7.Name = "checkBox7";
            this.checkBox7.Size = new System.Drawing.Size(113, 19);
            this.checkBox7.TabIndex = 8;
            this.checkBox7.Text = "Local To Field V2";
            this.checkBox7.UseVisualStyleBackColor = true;
            this.checkBox7.CheckedChanged += new System.EventHandler(this.checkBox7_CheckedChanged);
            // 
            // checkBox8
            // 
            this.checkBox8.AutoSize = true;
            this.checkBox8.ForeColor = System.Drawing.SystemColors.Control;
            this.checkBox8.Location = new System.Drawing.Point(172, 153);
            this.checkBox8.Name = "checkBox8";
            this.checkBox8.Size = new System.Drawing.Size(49, 19);
            this.checkBox8.TabIndex = 9;
            this.checkBox8.Text = "Calli";
            this.checkBox8.UseVisualStyleBackColor = true;
            this.checkBox8.CheckedChanged += new System.EventHandler(this.checkBox8_CheckedChanged);
            // 
            // checkBox9
            // 
            this.checkBox9.AutoSize = true;
            this.checkBox9.ForeColor = System.Drawing.SystemColors.Control;
            this.checkBox9.Location = new System.Drawing.Point(172, 3);
            this.checkBox9.Name = "checkBox9";
            this.checkBox9.Size = new System.Drawing.Size(87, 19);
            this.checkBox9.TabIndex = 10;
            this.checkBox9.Text = "Proxy Meth";
            this.checkBox9.UseVisualStyleBackColor = true;
            this.checkBox9.CheckedChanged += new System.EventHandler(this.checkBox9_CheckedChanged);
            // 
            // checkBox10
            // 
            this.checkBox10.AutoSize = true;
            this.checkBox10.ForeColor = System.Drawing.SystemColors.Control;
            this.checkBox10.Location = new System.Drawing.Point(172, 28);
            this.checkBox10.Name = "checkBox10";
            this.checkBox10.Size = new System.Drawing.Size(73, 19);
            this.checkBox10.TabIndex = 11;
            this.checkBox10.Text = "Proxy Int";
            this.checkBox10.UseVisualStyleBackColor = true;
            this.checkBox10.CheckedChanged += new System.EventHandler(this.checkBox10_CheckedChanged);
            // 
            // checkBox11
            // 
            this.checkBox11.AutoSize = true;
            this.checkBox11.ForeColor = System.Drawing.SystemColors.Control;
            this.checkBox11.Location = new System.Drawing.Point(172, 53);
            this.checkBox11.Name = "checkBox11";
            this.checkBox11.Size = new System.Drawing.Size(95, 19);
            this.checkBox11.TabIndex = 12;
            this.checkBox11.Text = "Proxy Strings";
            this.checkBox11.UseVisualStyleBackColor = true;
            this.checkBox11.CheckedChanged += new System.EventHandler(this.checkBox11_CheckedChanged);
            // 
            // checkBox12
            // 
            this.checkBox12.AutoSize = true;
            this.checkBox12.ForeColor = System.Drawing.SystemColors.Control;
            this.checkBox12.Location = new System.Drawing.Point(172, 77);
            this.checkBox12.Name = "checkBox12";
            this.checkBox12.Size = new System.Drawing.Size(73, 19);
            this.checkBox12.TabIndex = 13;
            this.checkBox12.Text = "Renamer";
            this.checkBox12.UseVisualStyleBackColor = true;
            this.checkBox12.CheckedChanged += new System.EventHandler(this.checkBox12_CheckedChanged);
            // 
            // checkBox13
            // 
            this.checkBox13.AutoSize = true;
            this.checkBox13.ForeColor = System.Drawing.SystemColors.Control;
            this.checkBox13.Location = new System.Drawing.Point(172, 102);
            this.checkBox13.Name = "checkBox13";
            this.checkBox13.Size = new System.Drawing.Size(86, 19);
            this.checkBox13.TabIndex = 14;
            this.checkBox13.Text = "JumpCflow";
            this.checkBox13.UseVisualStyleBackColor = true;
            this.checkBox13.CheckedChanged += new System.EventHandler(this.checkBox13_CheckedChanged);
            // 
            // checkBox14
            // 
            this.checkBox14.AutoSize = true;
            this.checkBox14.ForeColor = System.Drawing.SystemColors.Control;
            this.checkBox14.Location = new System.Drawing.Point(172, 127);
            this.checkBox14.Name = "checkBox14";
            this.checkBox14.Size = new System.Drawing.Size(86, 19);
            this.checkBox14.TabIndex = 15;
            this.checkBox14.Text = "Anti Debug";
            this.checkBox14.UseVisualStyleBackColor = true;
            this.checkBox14.CheckedChanged += new System.EventHandler(this.checkBox14_CheckedChanged);
            // 
            // checkBox15
            // 
            this.checkBox15.AutoSize = true;
            this.checkBox15.ForeColor = System.Drawing.SystemColors.Control;
            this.checkBox15.Location = new System.Drawing.Point(267, 128);
            this.checkBox15.Name = "checkBox15";
            this.checkBox15.Size = new System.Drawing.Size(84, 19);
            this.checkBox15.TabIndex = 16;
            this.checkBox15.Text = "Anti Dump";
            this.checkBox15.UseVisualStyleBackColor = true;
            this.checkBox15.CheckedChanged += new System.EventHandler(this.checkBox15_CheckedChanged);
            // 
            // checkBox16
            // 
            this.checkBox16.AutoSize = true;
            this.checkBox16.ForeColor = System.Drawing.SystemColors.Control;
            this.checkBox16.Location = new System.Drawing.Point(267, 103);
            this.checkBox16.Name = "checkBox16";
            this.checkBox16.Size = new System.Drawing.Size(90, 19);
            this.checkBox16.TabIndex = 17;
            this.checkBox16.Text = "Anti Tamper";
            this.checkBox16.UseVisualStyleBackColor = true;
            this.checkBox16.CheckedChanged += new System.EventHandler(this.checkBox16_CheckedChanged);
            // 
            // checkBox17
            // 
            this.checkBox17.AutoSize = true;
            this.checkBox17.ForeColor = System.Drawing.SystemColors.Control;
            this.checkBox17.Location = new System.Drawing.Point(267, 3);
            this.checkBox17.Name = "checkBox17";
            this.checkBox17.Size = new System.Drawing.Size(83, 19);
            this.checkBox17.TabIndex = 18;
            this.checkBox17.Text = "Invalid MD";
            this.checkBox17.UseVisualStyleBackColor = true;
            this.checkBox17.CheckedChanged += new System.EventHandler(this.checkBox17_CheckedChanged);
            // 
            // checkBox18
            // 
            this.checkBox18.AutoSize = true;
            this.checkBox18.ForeColor = System.Drawing.SystemColors.Control;
            this.checkBox18.Location = new System.Drawing.Point(267, 28);
            this.checkBox18.Name = "checkBox18";
            this.checkBox18.Size = new System.Drawing.Size(88, 19);
            this.checkBox18.TabIndex = 19;
            this.checkBox18.Text = "Anti de4dot";
            this.checkBox18.UseVisualStyleBackColor = true;
            this.checkBox18.CheckedChanged += new System.EventHandler(this.checkBox18_CheckedChanged);
            // 
            // checkBox19
            // 
            this.checkBox19.AutoSize = true;
            this.checkBox19.ForeColor = System.Drawing.SystemColors.Control;
            this.checkBox19.Location = new System.Drawing.Point(267, 53);
            this.checkBox19.Name = "checkBox19";
            this.checkBox19.Size = new System.Drawing.Size(128, 19);
            this.checkBox19.TabIndex = 20;
            this.checkBox19.Text = "StackUnfConfusion";
            this.checkBox19.UseVisualStyleBackColor = true;
            this.checkBox19.CheckedChanged += new System.EventHandler(this.checkBox19_CheckedChanged);
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
            // checkBox20
            // 
            this.checkBox20.AutoSize = true;
            this.checkBox20.ForeColor = System.Drawing.SystemColors.Control;
            this.checkBox20.Location = new System.Drawing.Point(267, 78);
            this.checkBox20.Name = "checkBox20";
            this.checkBox20.Size = new System.Drawing.Size(109, 19);
            this.checkBox20.TabIndex = 23;
            this.checkBox20.Text = "Anti Manything";
            this.checkBox20.UseVisualStyleBackColor = true;
            this.checkBox20.CheckedChanged += new System.EventHandler(this.checkBox20_CheckedChanged);
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
            this.panel1.Controls.Add(this.checkBox3);
            this.panel1.Controls.Add(this.checkBox20);
            this.panel1.Controls.Add(this.checkBox1);
            this.panel1.Controls.Add(this.checkBox2);
            this.panel1.Controls.Add(this.checkBox4);
            this.panel1.Controls.Add(this.checkBox19);
            this.panel1.Controls.Add(this.checkBox5);
            this.panel1.Controls.Add(this.checkBox18);
            this.panel1.Controls.Add(this.checkBox6);
            this.panel1.Controls.Add(this.checkBox17);
            this.panel1.Controls.Add(this.checkBox7);
            this.panel1.Controls.Add(this.checkBox16);
            this.panel1.Controls.Add(this.checkBox8);
            this.panel1.Controls.Add(this.checkBox15);
            this.panel1.Controls.Add(this.checkBox9);
            this.panel1.Controls.Add(this.checkBox14);
            this.panel1.Controls.Add(this.checkBox10);
            this.panel1.Controls.Add(this.checkBox13);
            this.panel1.Controls.Add(this.checkBox11);
            this.panel1.Controls.Add(this.checkBox12);
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.ClientSize = new System.Drawing.Size(578, 376);
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
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.CheckBox checkBox5;
        private System.Windows.Forms.CheckBox checkBox6;
        private System.Windows.Forms.CheckBox checkBox7;
        private System.Windows.Forms.CheckBox checkBox8;
        private System.Windows.Forms.CheckBox checkBox9;
        private System.Windows.Forms.CheckBox checkBox10;
        private System.Windows.Forms.CheckBox checkBox11;
        private System.Windows.Forms.CheckBox checkBox12;
        private System.Windows.Forms.CheckBox checkBox13;
        private System.Windows.Forms.CheckBox checkBox14;
        private System.Windows.Forms.CheckBox checkBox15;
        private System.Windows.Forms.CheckBox checkBox16;
        private System.Windows.Forms.CheckBox checkBox17;
        private System.Windows.Forms.CheckBox checkBox18;
        private System.Windows.Forms.CheckBox checkBox19;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox checkBox20;
        private System.Windows.Forms.Button btn_file;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btn_select;
    }
}
