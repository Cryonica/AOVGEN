using System;
using System.Drawing;
using Telerik.WinControls.UI;

namespace AOVGEN
{
    public partial class CableImage : RadForm
    {
        internal Bitmap Bitmap { get; set; }
        public CableImage()
        {
            InitializeComponent();
        }

        private void CableImage_Load(object sender, EventArgs e)
        {
            if (Bitmap != null)
            {
                pictureBox1.Image = Bitmap;
            }

           
        }
    }
}
