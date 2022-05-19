using System.ComponentModel;
using Telerik.WinControls.Themes;
using Telerik.WinControls.UI;

namespace AOVGEN
{
    partial class SetCabName
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
            this.radTextBox1 = new Telerik.WinControls.UI.RadTextBox();
            this.radButton1 = new Telerik.WinControls.UI.RadButton();
            this.radLabel1 = new Telerik.WinControls.UI.RadLabel();
            this.radLabel2 = new Telerik.WinControls.UI.RadLabel();
            this.radTextBox2 = new Telerik.WinControls.UI.RadTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.radTextBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radButton1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radTextBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // radTextBox1
            // 
            this.radTextBox1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.radTextBox1.Location = new System.Drawing.Point(151, 21);
            this.radTextBox1.Name = "radTextBox1";
            this.radTextBox1.Size = new System.Drawing.Size(93, 27);
            this.radTextBox1.TabIndex = 0;
            this.radTextBox1.ThemeName = "Office2007Black";
            // 
            // radButton1
            // 
            this.radButton1.Location = new System.Drawing.Point(81, 108);
            this.radButton1.Name = "radButton1";
            this.radButton1.Size = new System.Drawing.Size(98, 36);
            this.radButton1.TabIndex = 1;
            this.radButton1.Text = "Применить";
            this.radButton1.ThemeName = "Office2007Black";
            this.radButton1.Click += new System.EventHandler(this.RadButton1_Click);
            // 
            // radLabel1
            // 
            this.radLabel1.Location = new System.Drawing.Point(12, 28);
            this.radLabel1.Name = "radLabel1";
            this.radLabel1.Size = new System.Drawing.Size(133, 18);
            this.radLabel1.TabIndex = 2;
            this.radLabel1.Text = "Буквенное обозначение";
            // 
            // radLabel2
            // 
            this.radLabel2.Location = new System.Drawing.Point(12, 63);
            this.radLabel2.Name = "radLabel2";
            this.radLabel2.Size = new System.Drawing.Size(87, 18);
            this.radLabel2.TabIndex = 3;
            this.radLabel2.Text = "Начало отсчета";
            // 
            // radTextBox2
            // 
            this.radTextBox2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.radTextBox2.Location = new System.Drawing.Point(151, 56);
            this.radTextBox2.Name = "radTextBox2";
            this.radTextBox2.Size = new System.Drawing.Size(93, 27);
            this.radTextBox2.TabIndex = 4;
            this.radTextBox2.Text = "1";
            this.radTextBox2.ThemeName = "Office2007Black";
            // 
            // SetCabName
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(261, 169);
            this.Controls.Add(this.radTextBox2);
            this.Controls.Add(this.radLabel2);
            this.Controls.Add(this.radLabel1);
            this.Controls.Add(this.radButton1);
            this.Controls.Add(this.radTextBox1);
            this.Name = "SetCabName";
            // 
            // 
            // 
            this.RootElement.ApplyShapeToControl = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SetCabName";
            this.ThemeName = "Office2007Black";
            this.Load += new System.EventHandler(this.SetCabName_Load);
            ((System.ComponentModel.ISupportInitialize)(this.radTextBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radButton1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radTextBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Office2007BlackTheme office2007BlackTheme1;
        private RadTextBox radTextBox1;
        private RadButton radButton1;
        private RadLabel radLabel1;
        private RadLabel radLabel2;
        private RadTextBox radTextBox2;
    }
}
