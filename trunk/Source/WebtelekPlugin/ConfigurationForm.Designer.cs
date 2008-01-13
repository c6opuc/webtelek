namespace MediaPortal.GUI.WebTelek
{
    partial class ConfigurationForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigurationForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.VersionCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.OSDDelay = new System.Windows.Forms.NumericUpDown();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.label12 = new System.Windows.Forms.Label();
            this.NetDelay = new System.Windows.Forms.NumericUpDown();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.EPGNotifyCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.EPGLoadCheckBox = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.EPGdays = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.CustomButton = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OSDDelay)).BeginInit();
            this.groupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NetDelay)).BeginInit();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EPGdays)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.groupBox8);
            this.groupBox1.Controls.Add(this.groupBox6);
            this.groupBox1.Controls.Add(this.groupBox7);
            this.groupBox1.Controls.Add(this.groupBox5);
            this.groupBox1.Controls.Add(this.groupBox4);
            this.groupBox1.Controls.Add(this.CustomButton);
            this.groupBox1.Controls.Add(this.button4);
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.button3);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(12, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(328, 307);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Настройки";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.VersionCheckBox);
            this.groupBox8.Location = new System.Drawing.Point(196, 151);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(120, 40);
            this.groupBox8.TabIndex = 37;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Новые версии";
            this.toolTip.SetToolTip(this.groupBox8, "Отметьте галочку если Вы \r\nхотите получать уведомления \r\nо новых версиях WebTelek" +
                    "+ плагина.");
            // 
            // VersionCheckBox
            // 
            this.VersionCheckBox.AutoSize = true;
            this.VersionCheckBox.Location = new System.Drawing.Point(9, 17);
            this.VersionCheckBox.Name = "VersionCheckBox";
            this.VersionCheckBox.Size = new System.Drawing.Size(97, 17);
            this.VersionCheckBox.TabIndex = 0;
            this.VersionCheckBox.Text = "вкл. проверку";
            this.toolTip.SetToolTip(this.VersionCheckBox, "Отметьте галочку если Вы \r\nхотите получать уведомления \r\nо новых версиях WebTelek" +
                    "+ плагина.");
            this.VersionCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.label10);
            this.groupBox6.Controls.Add(this.OSDDelay);
            this.groupBox6.Location = new System.Drawing.Point(196, 232);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(120, 40);
            this.groupBox6.TabIndex = 13;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "OSD таймаут";
            this.toolTip.SetToolTip(this.groupBox6, resources.GetString("groupBox6.ToolTip"));
            this.groupBox6.Enter += new System.EventHandler(this.groupBox6_Enter);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(59, 18);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(28, 13);
            this.label10.TabIndex = 35;
            this.label10.Text = "сек.";
            this.toolTip.SetToolTip(this.label10, resources.GetString("label10.ToolTip"));
            // 
            // OSDDelay
            // 
            this.OSDDelay.Location = new System.Drawing.Point(9, 14);
            this.OSDDelay.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.OSDDelay.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.OSDDelay.Name = "OSDDelay";
            this.OSDDelay.Size = new System.Drawing.Size(44, 20);
            this.OSDDelay.TabIndex = 34;
            this.OSDDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip.SetToolTip(this.OSDDelay, resources.GetString("OSDDelay.ToolTip"));
            this.OSDDelay.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.OSDDelay.ValueChanged += new System.EventHandler(this.OSDDelay_ValueChanged);
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.label12);
            this.groupBox7.Controls.Add(this.NetDelay);
            this.groupBox7.Location = new System.Drawing.Point(196, 192);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(120, 40);
            this.groupBox7.TabIndex = 15;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Сетевой таймаут";
            this.toolTip.SetToolTip(this.groupBox7, "Этот параметр определяет сколько секунд плагин \r\nбудет пытаться соединиться с сер" +
                    "вером WebTelek+.\r\nМеняйте этот параметр только когда это необходимо и \r\nтолько е" +
                    "слы Вы знаете что Вы делаете.");
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(59, 17);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(28, 13);
            this.label12.TabIndex = 36;
            this.label12.Text = "сек.";
            this.toolTip.SetToolTip(this.label12, "Этот параметр определяет сколько секунд плагин \r\nбудет пытаться соединиться с сер" +
                    "вером WebTelek+.\r\nМеняйте этот параметр только когда это необходимо и \r\nтолько е" +
                    "слы Вы знаете что Вы делаете.");
            // 
            // NetDelay
            // 
            this.NetDelay.Location = new System.Drawing.Point(9, 13);
            this.NetDelay.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.NetDelay.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.NetDelay.Name = "NetDelay";
            this.NetDelay.Size = new System.Drawing.Size(44, 20);
            this.NetDelay.TabIndex = 0;
            this.NetDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip.SetToolTip(this.NetDelay, "Этот параметр определяет сколько секунд плагин \r\nбудет пытаться соединиться с сер" +
                    "вером WebTelek+.\r\nМеняйте этот параметр только когда это необходимо и \r\nтолько е" +
                    "слы Вы знаете что Вы делаете.");
            this.NetDelay.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.EPGNotifyCheckBox);
            this.groupBox5.Location = new System.Drawing.Point(17, 228);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(173, 44);
            this.groupBox5.TabIndex = 12;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "TV Notify";
            this.toolTip.SetToolTip(this.groupBox5, resources.GetString("groupBox5.ToolTip"));
            // 
            // EPGNotifyCheckBox
            // 
            this.EPGNotifyCheckBox.AutoSize = true;
            this.EPGNotifyCheckBox.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.EPGNotifyCheckBox.Location = new System.Drawing.Point(6, 14);
            this.EPGNotifyCheckBox.Name = "EPGNotifyCheckBox";
            this.EPGNotifyCheckBox.Size = new System.Drawing.Size(155, 17);
            this.EPGNotifyCheckBox.TabIndex = 30;
            this.EPGNotifyCheckBox.Text = "Напоминатель программ";
            this.EPGNotifyCheckBox.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip.SetToolTip(this.EPGNotifyCheckBox, resources.GetString("EPGNotifyCheckBox.ToolTip"));
            this.EPGNotifyCheckBox.UseVisualStyleBackColor = true;
            this.EPGNotifyCheckBox.CheckedChanged += new System.EventHandler(this.EPGNotifyCheckBox_CheckedChanged_1);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.EPGLoadCheckBox);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.EPGdays);
            this.groupBox4.Controls.Add(this.label9);
            this.groupBox4.Location = new System.Drawing.Point(17, 151);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(173, 71);
            this.groupBox4.TabIndex = 11;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "EPG";
            this.toolTip.SetToolTip(this.groupBox4, resources.GetString("groupBox4.ToolTip"));
            // 
            // EPGLoadCheckBox
            // 
            this.EPGLoadCheckBox.AutoSize = true;
            this.EPGLoadCheckBox.Location = new System.Drawing.Point(12, 17);
            this.EPGLoadCheckBox.Name = "EPGLoadCheckBox";
            this.EPGLoadCheckBox.Size = new System.Drawing.Size(139, 17);
            this.EPGLoadCheckBox.TabIndex = 30;
            this.EPGLoadCheckBox.Text = "Загружать программу";
            this.toolTip.SetToolTip(this.EPGLoadCheckBox, resources.GetString("EPGLoadCheckBox.ToolTip"));
            this.EPGLoadCheckBox.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 37);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(84, 13);
            this.label7.TabIndex = 27;
            this.label7.Text = "Программа на ";
            this.toolTip.SetToolTip(this.label7, resources.GetString("label7.ToolTip"));
            // 
            // EPGdays
            // 
            this.EPGdays.Location = new System.Drawing.Point(93, 35);
            this.EPGdays.Maximum = new decimal(new int[] {
            7,
            0,
            0,
            0});
            this.EPGdays.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.EPGdays.Name = "EPGdays";
            this.EPGdays.Size = new System.Drawing.Size(42, 20);
            this.EPGdays.TabIndex = 28;
            this.EPGdays.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip.SetToolTip(this.EPGdays, resources.GetString("EPGdays.ToolTip"));
            this.EPGdays.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(139, 38);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(22, 13);
            this.label9.TabIndex = 29;
            this.label9.Text = "дн.";
            this.toolTip.SetToolTip(this.label9, resources.GetString("label9.ToolTip"));
            // 
            // CustomButton
            // 
            this.CustomButton.Location = new System.Drawing.Point(216, 278);
            this.CustomButton.Name = "CustomButton";
            this.CustomButton.Size = new System.Drawing.Size(100, 23);
            this.CustomButton.TabIndex = 10;
            this.CustomButton.Text = "Другие каналы";
            this.toolTip.SetToolTip(this.CustomButton, "Здесь Вы сможете добавить  Ваши любимые видео- и \r\nтелеканалы найденые на простор" +
                    "ах Интернета. \r\nК примеру, Вы сможете добавить бесплатные каналы, \r\nопубликованы" +
                    "е на www.webtelek.com.");
            this.CustomButton.UseVisualStyleBackColor = true;
            this.CustomButton.Click += new System.EventHandler(this.CustomButton_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(98, 278);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 4;
            this.button4.Text = "Cancel";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.comboBox2);
            this.groupBox3.Controls.Add(this.comboBox1);
            this.groupBox3.Controls.Add(this.textBox2);
            this.groupBox3.Controls.Add(this.textBox1);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Location = new System.Drawing.Point(135, 19);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(181, 126);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Enter += new System.EventHandler(this.groupBox3_Enter);
            // 
            // comboBox2
            // 
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(6, 89);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(169, 21);
            this.comboBox2.TabIndex = 6;
            this.comboBox2.SelectedIndexChanged += new System.EventHandler(this.comboBox2_SelectedIndexChanged);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(6, 62);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(169, 21);
            this.comboBox1.TabIndex = 5;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(6, 36);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(169, 20);
            this.textBox2.TabIndex = 4;
            this.textBox2.UseSystemPasswordChar = true;
            this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(6, 10);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(169, 20);
            this.textBox1.TabIndex = 3;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(26, 113);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(0, 13);
            this.label11.TabIndex = 2;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(17, 278);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 3;
            this.button3.Text = "OK";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Location = new System.Drawing.Point(17, 19);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(112, 126);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 97);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(91, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Временная зона";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 70);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "3она вещания";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Пароль";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "E-mail адрес";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(26, 113);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(0, 13);
            this.label8.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(26, 113);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(0, 13);
            this.label3.TabIndex = 2;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 206);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(0, 13);
            this.label6.TabIndex = 2;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(220, 323);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(125, 13);
            this.linkLabel1.TabIndex = 3;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "© 6opuc (Borys Saulyak)";
            this.linkLabel1.VisitedLinkColor = System.Drawing.Color.Blue;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 10000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // ConfigurationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(357, 342);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(365, 370);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(365, 370);
            this.Name = "ConfigurationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Настройки WEBTELEK+";
            this.Load += new System.EventHandler(this.ConfigurationForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OSDDelay)).EndInit();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NetDelay)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EPGdays)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Button CustomButton;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown EPGdays;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown OSDDelay;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox EPGNotifyCheckBox;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.NumericUpDown NetDelay;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.CheckBox VersionCheckBox;
        private System.Windows.Forms.CheckBox EPGLoadCheckBox;
        private System.Windows.Forms.ToolTip toolTip;
    }
}