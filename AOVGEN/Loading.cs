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
    public partial class Loading : Telerik.WinControls.UI.RadForm
    {
        private readonly string SendMeString;
        public Loading(string input)
        {
            SendMeString = input;
            InitializeComponent();
            
        }
        public Loading()
        {
         
            InitializeComponent();

        }
        private void Loading_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(SendMeString))
            {
                MessageBox.Show(SendMeString);
            }
            
        }
    }
}
