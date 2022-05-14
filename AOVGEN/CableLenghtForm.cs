using System;
using System.Windows.Forms;


namespace AOVGEN
{
    public partial class CableLenghtForm : Telerik.WinControls.UI.RadForm
    {
        internal double Lenght { get; set; }
        internal double Addition { get; set; }
        internal string Filename { get; set; }
        internal bool ReadTranslationFie { get; set; }
        internal bool SetRandom { get; set; }
        internal bool ReadAcadFile { get; set; }
        internal bool AllowPowerCables { get; set; }
        internal double PowerCableLenght { get; set; }
        internal bool UseGenPowerCables { get; set; }

        public CableLenghtForm()
        {
            InitializeComponent();
        }

        
        private void RadCheckBox2_ToggleStateChanged(object sender, Telerik.WinControls.UI.StateChangedEventArgs args)
        {
            if (radCheckBox2.Checked)
            {
                radCheckBox1.Checked = false;
                radCheckBox3.Checked = false;
                this.ReadTranslationFie = true;
            }
            else
            {
                this.ReadTranslationFie = false;
            }
        }

        private void RadCheckBox3_ToggleStateChanged(object sender, Telerik.WinControls.UI.StateChangedEventArgs args)
        {
            if (radCheckBox3.Checked)
            {
               
                radCheckBox2.Checked = false;
                panel1.Visible = true;
                radCheckBox5.Checked = true;
            }
            else
            {
                panel1.Visible = false;
                radCheckBox5.Checked = false;
            }
        }

        private void RadButton1_Click(object sender, EventArgs e)
        {
            if (radCheckBox3.Checked)
            {
                double.TryParse(this.radTextBox1.Text, out double lenght);
                double.TryParse(this.radTextBox2.Text, out double addition);
                Addition = addition;
                Lenght = lenght;
                this.SetRandom = true;
            }
            if (radCheckBox4.Checked) AllowPowerCables = true;
            if (radCheckBox5.Checked) UseGenPowerCables = true;
            double.TryParse(this.radTextBox3.Text, out double tmp);
            PowerCableLenght = tmp;
            
            this.DialogResult = DialogResult.OK;
            this.Close();
            
        }
        private void RadCheckBox1_ToggleStateChanged(object sender, Telerik.WinControls.UI.StateChangedEventArgs args)
        {
            if (radCheckBox1.Checked)
            {
                radCheckBox2.Checked = false;
                radCheckBox3.Checked = false;
                this.ReadAcadFile = true;
            }
            else
            {
                this.ReadAcadFile = false;
            }

            
        }

        private void RadCheckBox4_ToggleStateChanged(object sender, Telerik.WinControls.UI.StateChangedEventArgs args)
        {
            if (radCheckBox4.Checked)
            {
                panel4.Visible = true;
                if (!radCheckBox5.Checked) panel3.Visible = true;
            }
            else
            {
                panel4.Visible = false;
                panel3.Visible = false;
            }
        }

        private void RadCheckBox5_ToggleStateChanged(object sender, Telerik.WinControls.UI.StateChangedEventArgs args)
        {
            if (!radCheckBox5.Checked && radCheckBox4.Checked)
            {
                panel3.Visible = true;
            }
            else
            {
               panel3.Visible = false;
            }
        }
    }
}
