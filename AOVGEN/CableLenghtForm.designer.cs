using System.ComponentModel;
using System.Windows.Forms;
using Telerik.WinControls.Themes;
using Telerik.WinControls.UI;

namespace AOVGEN
{
    partial class CableLenghtForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.office2007BlackTheme1 = new Telerik.WinControls.Themes.Office2007BlackTheme();
            this.radButton1 = new Telerik.WinControls.UI.RadButton();
            this.radCheckBox2 = new Telerik.WinControls.UI.RadCheckBox();
            this.radCheckBox3 = new Telerik.WinControls.UI.RadCheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.radTextBox1 = new Telerik.WinControls.UI.RadTextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.radTextBox2 = new Telerik.WinControls.UI.RadTextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.radCheckBox1 = new Telerik.WinControls.UI.RadCheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.radCheckBox4 = new Telerik.WinControls.UI.RadCheckBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.radTextBox3 = new Telerik.WinControls.UI.RadTextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.radCheckBox5 = new Telerik.WinControls.UI.RadCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.radButton1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radCheckBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radCheckBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radTextBox1)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radTextBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radCheckBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radCheckBox4)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radTextBox3)).BeginInit();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radCheckBox5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // radButton1
            // 
            this.radButton1.Location = new System.Drawing.Point(80, 273);
            this.radButton1.Name = "radButton1";
            this.radButton1.Size = new System.Drawing.Size(98, 35);
            this.radButton1.TabIndex = 0;
            this.radButton1.Text = "Применить";
            this.radButton1.ThemeName = "Office2007Black";
            this.radButton1.Click += new System.EventHandler(this.RadButton1_Click);
            // 
            // radCheckBox2
            // 
            this.radCheckBox2.DisplayStyle = Telerik.WinControls.DisplayStyle.None;
            this.radCheckBox2.Location = new System.Drawing.Point(217, 52);
            this.radCheckBox2.Name = "radCheckBox2";
            this.radCheckBox2.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.radCheckBox2.Size = new System.Drawing.Size(15, 15);
            this.radCheckBox2.TabIndex = 2;
            this.radCheckBox2.Text = "Считать длины из файла обмена";
            this.radCheckBox2.ToggleStateChanged += new Telerik.WinControls.UI.StateChangedEventHandler(this.RadCheckBox2_ToggleStateChanged);
            // 
            // radCheckBox3
            // 
            this.radCheckBox3.DisplayStyle = Telerik.WinControls.DisplayStyle.None;
            this.radCheckBox3.Location = new System.Drawing.Point(217, 76);
            this.radCheckBox3.Name = "radCheckBox3";
            this.radCheckBox3.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.radCheckBox3.Size = new System.Drawing.Size(15, 15);
            this.radCheckBox3.TabIndex = 3;
            this.radCheckBox3.Text = "Считать длины из файла обмена";
            this.radCheckBox3.ToggleStateChanged += new Telerik.WinControls.UI.StateChangedEventHandler(this.RadCheckBox3_ToggleStateChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(186, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Считать длины с блоков чертежа";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(187, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Считать длины после трансляции";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(142, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Сгенерировать случайно";
            // 
            // radTextBox1
            // 
            this.radTextBox1.Location = new System.Drawing.Point(9, 7);
            this.radTextBox1.Name = "radTextBox1";
            this.radTextBox1.Size = new System.Drawing.Size(100, 20);
            this.radTextBox1.TabIndex = 7;
            this.radTextBox1.Text = "20";
            this.radTextBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.radTextBox1.ThemeName = "Office2007Black";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.radTextBox2);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.radTextBox1);
            this.panel1.Location = new System.Drawing.Point(12, 92);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(229, 37);
            this.panel1.TabIndex = 8;
            this.panel1.Visible = false;
            // 
            // radTextBox2
            // 
            this.radTextBox2.Location = new System.Drawing.Point(139, 7);
            this.radTextBox2.Name = "radTextBox2";
            this.radTextBox2.Size = new System.Drawing.Size(62, 20);
            this.radTextBox2.TabIndex = 8;
            this.radTextBox2.Text = "5";
            this.radTextBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.radTextBox2.ThemeName = "Office2007Black";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.Location = new System.Drawing.Point(115, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(15, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "+";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // radCheckBox1
            // 
            this.radCheckBox1.DisplayStyle = Telerik.WinControls.DisplayStyle.None;
            this.radCheckBox1.Location = new System.Drawing.Point(217, 30);
            this.radCheckBox1.Name = "radCheckBox1";
            this.radCheckBox1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.radCheckBox1.Size = new System.Drawing.Size(15, 15);
            this.radCheckBox1.TabIndex = 3;
            this.radCheckBox1.Text = "Считать длины из файла обмена";
            this.radCheckBox1.ToggleStateChanged += new Telerik.WinControls.UI.StateChangedEventHandler(this.RadCheckBox1_ToggleStateChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label5.Location = new System.Drawing.Point(2, 5);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(152, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Заполнять силовые кабели";
            // 
            // radCheckBox4
            // 
            this.radCheckBox4.DisplayStyle = Telerik.WinControls.DisplayStyle.None;
            this.radCheckBox4.Location = new System.Drawing.Point(217, 143);
            this.radCheckBox4.Name = "radCheckBox4";
            this.radCheckBox4.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.radCheckBox4.Size = new System.Drawing.Size(15, 15);
            this.radCheckBox4.TabIndex = 9;
            this.radCheckBox4.Text = "Считать длины из файла обмена";
            this.radCheckBox4.ToggleStateChanged += new Telerik.WinControls.UI.StateChangedEventHandler(this.RadCheckBox4_ToggleStateChanged);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.panel2.Controls.Add(this.panel4);
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Location = new System.Drawing.Point(13, 138);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(228, 110);
            this.panel2.TabIndex = 11;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.label6);
            this.panel3.Controls.Add(this.radTextBox3);
            this.panel3.Location = new System.Drawing.Point(8, 63);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(211, 35);
            this.panel3.TabIndex = 11;
            this.panel3.Visible = false;
            // 
            // radTextBox3
            // 
            this.radTextBox3.Location = new System.Drawing.Point(95, 8);
            this.radTextBox3.Name = "radTextBox3";
            this.radTextBox3.Size = new System.Drawing.Size(62, 20);
            this.radTextBox3.TabIndex = 9;
            this.radTextBox3.Text = "0";
            this.radTextBox3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.radTextBox3.ThemeName = "Office2007Black";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label6.Location = new System.Drawing.Point(45, 10);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "Длина";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.radCheckBox5);
            this.panel4.Controls.Add(this.label7);
            this.panel4.Location = new System.Drawing.Point(8, 29);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(217, 28);
            this.panel4.TabIndex = 12;
            this.panel4.Visible = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label7.Location = new System.Drawing.Point(5, 8);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(115, 13);
            this.label7.TabIndex = 11;
            this.label7.Text = "Взять из генератора";
            // 
            // radCheckBox5
            // 
            this.radCheckBox5.DisplayStyle = Telerik.WinControls.DisplayStyle.None;
            this.radCheckBox5.Location = new System.Drawing.Point(196, 6);
            this.radCheckBox5.Name = "radCheckBox5";
            this.radCheckBox5.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.radCheckBox5.Size = new System.Drawing.Size(15, 15);
            this.radCheckBox5.TabIndex = 12;
            this.radCheckBox5.Text = "Считать длины из файла обмена";
            this.radCheckBox5.ToggleStateChanged += new Telerik.WinControls.UI.StateChangedEventHandler(this.RadCheckBox5_ToggleStateChanged);
            // 
            // CableLenghtForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(253, 325);
            this.Controls.Add(this.radCheckBox4);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.radCheckBox1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.radCheckBox3);
            this.Controls.Add(this.radCheckBox2);
            this.Controls.Add(this.radButton1);
            this.Name = "CableLenghtForm";
            // 
            // 
            // 
            this.RootElement.ApplyShapeToControl = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "CableLenghtForm";
            this.ThemeName = "Office2007Black";
            ((System.ComponentModel.ISupportInitialize)(this.radButton1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radCheckBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radCheckBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radTextBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radTextBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radCheckBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radCheckBox4)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radTextBox3)).EndInit();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radCheckBox5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Office2007BlackTheme office2007BlackTheme1;
        private RadButton radButton1;
        private RadCheckBox radCheckBox2;
        private RadCheckBox radCheckBox3;
        private Label label1;
        private Label label2;
        private Label label3;
        private RadTextBox radTextBox1;
        private Panel panel1;
        private RadTextBox radTextBox2;
        private Label label4;
        private OpenFileDialog openFileDialog1;
        private RadCheckBox radCheckBox1;
        private Label label5;
        private RadCheckBox radCheckBox4;
        private Panel panel2;
        private Panel panel3;
        private Label label6;
        private RadTextBox radTextBox3;
        private Panel panel4;
        private RadCheckBox radCheckBox5;
        private Label label7;
    }
}
