using System.ComponentModel;
using Telerik.WinControls.UI;

namespace AOVGEN
{
    partial class SpreadSheetForm
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
            Telerik.Windows.Documents.Spreadsheet.Model.Workbook workbook1 = new Telerik.Windows.Documents.Spreadsheet.Model.Workbook();
            this.radSpreadsheetRibbonBar1 = new Telerik.WinControls.UI.RadSpreadsheetRibbonBar();
            this.radRibbonFormBehavior1 = new Telerik.WinControls.UI.RadRibbonFormBehavior();
            this.radSpreadsheet1 = new Telerik.WinControls.UI.RadSpreadsheet();
            ((System.ComponentModel.ISupportInitialize)(this.radSpreadsheetRibbonBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radSpreadsheet1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // radSpreadsheetRibbonBar1
            // 
            this.radSpreadsheetRibbonBar1.ApplicationMenuStyle = Telerik.WinControls.UI.ApplicationMenuStyle.BackstageView;
            this.radSpreadsheetRibbonBar1.AssociatedSpreadsheet = this.radSpreadsheet1;
            // 
            // 
            // 
            this.radSpreadsheetRibbonBar1.ExitButton.Text = "Exit";
            this.radSpreadsheetRibbonBar1.LocalizationSettings.LayoutModeText = "Simplified Layout";
            this.radSpreadsheetRibbonBar1.Location = new System.Drawing.Point(0, 0);
            this.radSpreadsheetRibbonBar1.Name = "radSpreadsheetRibbonBar1";
            // 
            // 
            // 
            this.radSpreadsheetRibbonBar1.OptionsButton.Text = "Options";
            this.radSpreadsheetRibbonBar1.ShowLayoutModeButton = true;
            this.radSpreadsheetRibbonBar1.Size = new System.Drawing.Size(1381, 161);
            this.radSpreadsheetRibbonBar1.TabIndex = 0;
            this.radSpreadsheetRibbonBar1.Text = "radSpreadsheetRibbonBar1";
            this.radSpreadsheetRibbonBar1.ThemeName = "Office2007Black";
            // 
            // radRibbonFormBehavior1
            // 
            this.radRibbonFormBehavior1.Form = this;
            // 
            // radSpreadsheet1
            // 
            this.radSpreadsheet1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radSpreadsheet1.Location = new System.Drawing.Point(0, 161);
            this.radSpreadsheet1.Name = "radSpreadsheet1";
            this.radSpreadsheet1.Size = new System.Drawing.Size(1381, 629);
            this.radSpreadsheet1.TabIndex = 0;
            this.radSpreadsheet1.ThemeName = "Office2007Black";
            workbook1.ActiveTabIndex = -1;
            workbook1.Name = "Book1";
            workbook1.WorkbookContentChangedInterval = System.TimeSpan.Parse("00:00:00.0300000");
            this.radSpreadsheet1.Workbook = workbook1;
            // 
            // SpreadSheetForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1381, 790);
            this.Controls.Add(this.radSpreadsheet1);
            this.Controls.Add(this.radSpreadsheetRibbonBar1);
            this.FormBehavior = this.radRibbonFormBehavior1;
            this.IconScaling = Telerik.WinControls.Enumerations.ImageScaling.None;
            this.Name = "SpreadSheetForm";
            // 
            // 
            // 
            this.RootElement.ApplyShapeToControl = true;
            this.Text = "radSpreadsheetRibbonBar1";
            this.ThemeName = "Office2007Black";
            this.Load += new System.EventHandler(this.SpreadSheetForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.radSpreadsheetRibbonBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radSpreadsheet1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private RadSpreadsheetRibbonBar radSpreadsheetRibbonBar1;
        private RadRibbonFormBehavior radRibbonFormBehavior1;
        private RadSpreadsheet radSpreadsheet1;
    }
}
