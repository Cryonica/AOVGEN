using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Telerik.WinControls;

namespace AOVGEN
{
    public partial class IO_Summ : Telerik.WinControls.UI.RadForm
    {
        private readonly string IO;
        public IO_Summ(string message)
        {
            InitializeComponent();
            IO = message;
        }

        private void IO_Summ_Load(object sender, EventArgs e)
        {
            this.radTextBox1.AppendText(IO);
        }
    }
}
