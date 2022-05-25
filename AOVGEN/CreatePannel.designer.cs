using System.ComponentModel;
using System.Windows.Forms;
using Bunifu.Framework.UI;
using Telerik.WinControls.Themes;
using Telerik.WinControls.UI;

namespace AOVGEN
{
    partial class CreatePannel
    {
        private BunifuImageButton bunifuImageButton1;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        private Office2007BlackTheme office2007BlackTheme1;

        private Panel panel1;

        private Panel panel2;

        private Panel panel3;

        private RadButton radButton1;

        private RadDropDownList radDropDownList2;

        private RadDropDownList radDropDownList3;

        private RadLabel radLabel1;

        private RadLabel radLabel4;

        private RadLabel radLabel5;

        private RadLabel radLabel6;

        private RadLabel radLabel7;

        private RadTextBox radTextBox1;

        private RadToggleSwitch radToggleSwitch1;

        private RadToggleSwitch radToggleSwitch2;

        private Timer timer1;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreatePannel));
            Telerik.WinControls.UI.RadListDataItem radListDataItem1 = new Telerik.WinControls.UI.RadListDataItem();
            Telerik.WinControls.UI.RadListDataItem radListDataItem2 = new Telerik.WinControls.UI.RadListDataItem();
            Telerik.WinControls.UI.RadListDataItem radListDataItem3 = new Telerik.WinControls.UI.RadListDataItem();
            Telerik.WinControls.UI.RadListDataItem radListDataItem4 = new Telerik.WinControls.UI.RadListDataItem();
            Telerik.WinControls.UI.RadListDataItem radListDataItem5 = new Telerik.WinControls.UI.RadListDataItem();
            Telerik.WinControls.UI.RadListDataItem radListDataItem6 = new Telerik.WinControls.UI.RadListDataItem();
            Telerik.WinControls.UI.RadListDataItem radListDataItem7 = new Telerik.WinControls.UI.RadListDataItem();
            this.office2007BlackTheme1 = new Telerik.WinControls.Themes.Office2007BlackTheme();
            this.panel1 = new System.Windows.Forms.Panel();
            this.bunifuImageButton1 = new Bunifu.Framework.UI.BunifuImageButton();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.radButton1 = new Telerik.WinControls.UI.RadButton();
            this.radTextBox1 = new Telerik.WinControls.UI.RadTextBox();
            this.radLabel1 = new Telerik.WinControls.UI.RadLabel();
            this.radDropDownList3 = new Telerik.WinControls.UI.RadDropDownList();
            this.radLabel4 = new Telerik.WinControls.UI.RadLabel();
            this.radLabel5 = new Telerik.WinControls.UI.RadLabel();
            this.radToggleSwitch1 = new Telerik.WinControls.UI.RadToggleSwitch();
            this.radToggleSwitch2 = new Telerik.WinControls.UI.RadToggleSwitch();
            this.radLabel6 = new Telerik.WinControls.UI.RadLabel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.radDropDownList2 = new Telerik.WinControls.UI.RadDropDownList();
            this.radLabel7 = new Telerik.WinControls.UI.RadLabel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bunifuImageButton1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radButton1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radTextBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radDropDownList3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radToggleSwitch1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radToggleSwitch2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radDropDownList2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel7)).BeginInit();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Crimson;
            this.panel1.Controls.Add(this.bunifuImageButton1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(265, 37);
            this.panel1.TabIndex = 0;
            // 
            // bunifuImageButton1
            // 
            this.bunifuImageButton1.Image = ((System.Drawing.Image)(resources.GetObject("bunifuImageButton1.Image")));
            this.bunifuImageButton1.ImageActive = null;
            this.bunifuImageButton1.Location = new System.Drawing.Point(231, 5);
            this.bunifuImageButton1.Name = "bunifuImageButton1";
            this.bunifuImageButton1.Size = new System.Drawing.Size(26, 26);
            this.bunifuImageButton1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.bunifuImageButton1.TabIndex = 4;
            this.bunifuImageButton1.TabStop = false;
            this.bunifuImageButton1.Zoom = 10;
            this.bunifuImageButton1.Click += new System.EventHandler(this.bunifuImageButton1_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // radButton1
            // 
            this.radButton1.Location = new System.Drawing.Point(76, 251);
            this.radButton1.Name = "radButton1";
            this.radButton1.Size = new System.Drawing.Size(110, 35);
            this.radButton1.TabIndex = 1;
            this.radButton1.Text = "Применить";
            this.radButton1.ThemeName = "Office2007Black";
            this.radButton1.Click += new System.EventHandler(this.radButton1_Click);
            // 
            // radTextBox1
            // 
            this.radTextBox1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.radTextBox1.ForeColor = System.Drawing.Color.Silver;
            this.radTextBox1.Location = new System.Drawing.Point(102, 13);
            this.radTextBox1.Name = "radTextBox1";
            this.radTextBox1.Size = new System.Drawing.Size(125, 27);
            this.radTextBox1.TabIndex = 2;
            this.radTextBox1.Text = "Имя шкафа";
            this.radTextBox1.ThemeName = "Office2007Black";
            this.radTextBox1.Click += new System.EventHandler(this.radTextBox1_Click);
            // 
            // radLabel1
            // 
            this.radLabel1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.radLabel1.Location = new System.Drawing.Point(3, 17);
            this.radLabel1.Name = "radLabel1";
            this.radLabel1.Size = new System.Drawing.Size(77, 21);
            this.radLabel1.TabIndex = 5;
            this.radLabel1.Text = "Имя шкафа";
            // 
            // radDropDownList3
            // 
            this.radDropDownList3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.radDropDownList3.ForeColor = System.Drawing.Color.Silver;
            radListDataItem1.ForeColor = System.Drawing.Color.Red;
            radListDataItem1.Text = "1";
            radListDataItem2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            radListDataItem2.Text = "2";
            radListDataItem3.ForeColor = System.Drawing.Color.Green;
            radListDataItem3.Text = "3";
            this.radDropDownList3.Items.Add(radListDataItem1);
            this.radDropDownList3.Items.Add(radListDataItem2);
            this.radDropDownList3.Items.Add(radListDataItem3);
            this.radDropDownList3.Location = new System.Drawing.Point(102, 52);
            this.radDropDownList3.Name = "radDropDownList3";
            this.radDropDownList3.Size = new System.Drawing.Size(125, 27);
            this.radDropDownList3.TabIndex = 10;
            this.radDropDownList3.Text = "Категория";
            this.radDropDownList3.ThemeName = "Office2007Black";
            this.radDropDownList3.Leave += new System.EventHandler(this.radDropDownList3_Leave);
            this.radDropDownList3.MouseEnter += new System.EventHandler(this.radDropDownList3_MouseEnter);
            // 
            // radLabel4
            // 
            this.radLabel4.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.radLabel4.Location = new System.Drawing.Point(3, 56);
            this.radLabel4.Name = "radLabel4";
            this.radLabel4.Size = new System.Drawing.Size(69, 21);
            this.radLabel4.TabIndex = 9;
            this.radLabel4.Text = "Категория";
            // 
            // radLabel5
            // 
            this.radLabel5.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.radLabel5.Location = new System.Drawing.Point(3, 93);
            this.radLabel5.Name = "radLabel5";
            this.radLabel5.Size = new System.Drawing.Size(162, 21);
            this.radLabel5.TabIndex = 10;
            this.radLabel5.Text = "Отключение при пожаре";
            // 
            // radToggleSwitch1
            // 
            this.radToggleSwitch1.Location = new System.Drawing.Point(177, 93);
            this.radToggleSwitch1.Name = "radToggleSwitch1";
            this.radToggleSwitch1.OffText = "НЕТ";
            this.radToggleSwitch1.OnText = "ДА";
            this.radToggleSwitch1.Size = new System.Drawing.Size(50, 20);
            this.radToggleSwitch1.TabIndex = 12;
            this.radToggleSwitch1.ThemeName = "Office2007Black";
            this.radToggleSwitch1.ThumbTickness = 19;
            ((Telerik.WinControls.UI.RadToggleSwitchElement)(this.radToggleSwitch1.GetChildAt(0))).ThumbTickness = 19;
            ((Telerik.WinControls.UI.RadToggleSwitchElement)(this.radToggleSwitch1.GetChildAt(0))).ThumbOffset = 31;
            ((Telerik.WinControls.UI.ToggleSwitchPartElement)(this.radToggleSwitch1.GetChildAt(0).GetChildAt(0))).BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(121)))), ((int)(((byte)(255)))), ((int)(((byte)(116)))));
            ((Telerik.WinControls.UI.ToggleSwitchPartElement)(this.radToggleSwitch1.GetChildAt(0).GetChildAt(0))).BackColor3 = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(255)))), ((int)(((byte)(73)))));
            ((Telerik.WinControls.UI.ToggleSwitchPartElement)(this.radToggleSwitch1.GetChildAt(0).GetChildAt(0))).BackColor4 = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(254)))), ((int)(((byte)(120)))));
            ((Telerik.WinControls.UI.ToggleSwitchPartElement)(this.radToggleSwitch1.GetChildAt(0).GetChildAt(0))).Text = "ДА";
            ((Telerik.WinControls.UI.ToggleSwitchPartElement)(this.radToggleSwitch1.GetChildAt(0).GetChildAt(0))).BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(255)))), ((int)(((byte)(162)))));
            // 
            // radToggleSwitch2
            // 
            this.radToggleSwitch2.Location = new System.Drawing.Point(177, 120);
            this.radToggleSwitch2.Name = "radToggleSwitch2";
            this.radToggleSwitch2.OffText = "НЕТ";
            this.radToggleSwitch2.OnText = "ДА";
            this.radToggleSwitch2.Size = new System.Drawing.Size(50, 20);
            this.radToggleSwitch2.TabIndex = 14;
            this.radToggleSwitch2.ThemeName = "Office2007Black";
            this.radToggleSwitch2.ThumbTickness = 19;
            this.radToggleSwitch2.Value = false;
            this.radToggleSwitch2.ValueChanged += new System.EventHandler(this.radToggleSwitch2_ValueChanged);
            // 
            // radLabel6
            // 
            this.radLabel6.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.radLabel6.Location = new System.Drawing.Point(3, 120);
            this.radLabel6.Name = "radLabel6";
            this.radLabel6.Size = new System.Drawing.Size(116, 21);
            this.radLabel6.TabIndex = 13;
            this.radLabel6.Text = "Диспетчеризация";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Gray;
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 296);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(265, 22);
            this.panel2.TabIndex = 15;
            // 
            // radDropDownList2
            // 
            this.radDropDownList2.Enabled = false;
            this.radDropDownList2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.radDropDownList2.ForeColor = System.Drawing.Color.Black;
            radListDataItem4.ForeColor = System.Drawing.Color.Black;
            radListDataItem4.Text = "ModBus_RTU";
            radListDataItem5.ForeColor = System.Drawing.Color.Black;
            radListDataItem5.Text = "ModBus_TCP";
            radListDataItem6.ForeColor = System.Drawing.Color.Black;
            radListDataItem6.Text = "Bacnet_IP";
            radListDataItem7.Text = "LON";
            this.radDropDownList2.Items.Add(radListDataItem4);
            this.radDropDownList2.Items.Add(radListDataItem5);
            this.radDropDownList2.Items.Add(radListDataItem6);
            this.radDropDownList2.Items.Add(radListDataItem7);
            this.radDropDownList2.Location = new System.Drawing.Point(102, 147);
            this.radDropDownList2.Name = "radDropDownList2";
            this.radDropDownList2.Size = new System.Drawing.Size(125, 27);
            this.radDropDownList2.TabIndex = 16;
            this.radDropDownList2.Text = "Протокол";
            this.radDropDownList2.ThemeName = "Office2007Black";
            this.radDropDownList2.MouseEnter += new System.EventHandler(this.radDropDownList2_MouseEnter);
            // 
            // radLabel7
            // 
            this.radLabel7.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.radLabel7.Location = new System.Drawing.Point(3, 151);
            this.radLabel7.Name = "radLabel7";
            this.radLabel7.Size = new System.Drawing.Size(67, 21);
            this.radLabel7.TabIndex = 14;
            this.radLabel7.Text = "Протокол";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.radLabel4);
            this.panel3.Controls.Add(this.radLabel7);
            this.panel3.Controls.Add(this.radLabel1);
            this.panel3.Controls.Add(this.radTextBox1);
            this.panel3.Controls.Add(this.radDropDownList3);
            this.panel3.Controls.Add(this.radDropDownList2);
            this.panel3.Controls.Add(this.radLabel5);
            this.panel3.Controls.Add(this.radLabel6);
            this.panel3.Controls.Add(this.radToggleSwitch2);
            this.panel3.Controls.Add(this.radToggleSwitch1);
            this.panel3.Location = new System.Drawing.Point(12, 43);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(239, 202);
            this.panel3.TabIndex = 17;
            // 
            // CreatePannel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(265, 318);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.radButton1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Location = new System.Drawing.Point(0, 260);
            this.Name = "CreatePannel";
            // 
            // 
            // 
            this.RootElement.ApplyShapeToControl = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "CreatePannel";
            this.ThemeName = "Office2007Black";
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bunifuImageButton1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radButton1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radTextBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radDropDownList3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radToggleSwitch1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radToggleSwitch2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radDropDownList2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel7)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
    }
}
