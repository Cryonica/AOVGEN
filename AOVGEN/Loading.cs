using System;
using System.Windows.Forms;
using Telerik.WinControls.UI;

namespace AOVGEN
{
    public partial class Loading : RadForm
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
