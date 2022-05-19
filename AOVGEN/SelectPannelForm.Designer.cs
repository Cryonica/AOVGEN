using System.ComponentModel;
using Telerik.WinControls.Themes;
using Telerik.WinControls.UI;

namespace AOVGEN
{
    partial class SelectPannelForm
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
            this.radCheckedListBox1 = new Telerik.WinControls.UI.RadCheckedListBox();
            this.radButton1 = new Telerik.WinControls.UI.RadButton();
            this.radButton2 = new Telerik.WinControls.UI.RadButton();
            this.radButton3 = new Telerik.WinControls.UI.RadButton();
            ((System.ComponentModel.ISupportInitialize)(this.radCheckedListBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radButton1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radButton2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radButton3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // radCheckedListBox1
            // 
            this.radCheckedListBox1.AllowArbitraryItemHeight = true;
            this.radCheckedListBox1.Location = new System.Drawing.Point(12, 12);
            this.radCheckedListBox1.Name = "radCheckedListBox1";
            this.radCheckedListBox1.Size = new System.Drawing.Size(269, 291);
            this.radCheckedListBox1.TabIndex = 0;
            this.radCheckedListBox1.ThemeName = "Office2007Black";
            this.radCheckedListBox1.ItemCheckedChanged += new Telerik.WinControls.UI.ListViewItemEventHandler(this.radCheckedListBox1_ItemCheckedChanged);
            // 
            // radButton1
            // 
            this.radButton1.Location = new System.Drawing.Point(21, 309);
            this.radButton1.Name = "radButton1";
            this.radButton1.Size = new System.Drawing.Size(104, 24);
            this.radButton1.TabIndex = 1;
            this.radButton1.Text = "Выбрать всё";
            this.radButton1.ThemeName = "Office2007Black";
            this.radButton1.Click += new System.EventHandler(this.radButton1_Click);
            // 
            // radButton2
            // 
            this.radButton2.Location = new System.Drawing.Point(159, 309);
            this.radButton2.Name = "radButton2";
            this.radButton2.Size = new System.Drawing.Size(104, 24);
            this.radButton2.TabIndex = 3;
            this.radButton2.Text = "Отменить всё";
            this.radButton2.ThemeName = "Office2007Black";
            this.radButton2.Click += new System.EventHandler(this.radButton2_Click);
            // 
            // radButton3
            // 
            this.radButton3.Location = new System.Drawing.Point(85, 340);
            this.radButton3.Name = "radButton3";
            this.radButton3.Size = new System.Drawing.Size(104, 24);
            this.radButton3.TabIndex = 4;
            this.radButton3.Text = "Применить";
            this.radButton3.ThemeName = "Office2007Black";
            this.radButton3.Click += new System.EventHandler(this.radButton3_Click);
            // 
            // SelectPannelForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(294, 376);
            this.Controls.Add(this.radButton3);
            this.Controls.Add(this.radButton2);
            this.Controls.Add(this.radButton1);
            this.Controls.Add(this.radCheckedListBox1);
            this.Name = "SelectPannelForm";
            // 
            // 
            // 
            this.RootElement.ApplyShapeToControl = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SelectPannelForm";
            this.ThemeName = "Office2007Black";
            this.Load += new System.EventHandler(this.SelectPannelForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.radCheckedListBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radButton1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radButton2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radButton3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Office2007BlackTheme office2007BlackTheme1;
        private RadCheckedListBox radCheckedListBox1;
        private RadButton radButton1;
        private RadButton radButton2;
        private RadButton radButton3;
    }
}
