using System;
using System.Windows.Forms;
using Telerik.WinControls.UI;

namespace AOVGEN
{
    public partial class SetCabName : RadForm
    {
        public string Cablename { get; set; }
        public int Cablestartnum { get; set; }
        public SetCabName()
        {
            InitializeComponent();
            

        }

        private void RadButton1_Click(object sender, EventArgs e)
        {
            Cablename = radButton1.Text;
            string cablestartnumstring = radTextBox2.Text;
            if (Cablename != string.Empty && cablestartnumstring != string.Empty)
            {
                bool result =  int.TryParse(cablestartnumstring, out int tempnum);
                if (result)
                {
                    Cablestartnum = tempnum;
                    Cablename = radTextBox1.Text;
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Введите числовое значение начала отсчета");
                }
            }
        }

        private void SetCabName_Load(object sender, EventArgs e)
        {
            radTextBox1.Text = Cablename;
        }
    }
}
