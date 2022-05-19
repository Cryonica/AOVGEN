using System;
using Telerik.WinControls.UI;

namespace AOVGEN
{
    public partial class IO_Summ : RadForm
    {
        private readonly string IO;
        public IO_Summ(string message)
        {
            InitializeComponent();
            IO = message;
        }

        private void IO_Summ_Load(object sender, EventArgs e)
        {
            radTextBox1.AppendText(IO);
        }
    }
}
