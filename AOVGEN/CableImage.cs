using System;
using System.Drawing;


namespace AOVGEN
{
    public partial class CableImage : Telerik.WinControls.UI.RadForm
    {
        internal Bitmap Bitmap { get; set; }
        public CableImage()
        {
            InitializeComponent();
        }

        private void CableImage_Load(object sender, EventArgs e)
        {
            if (this.Bitmap != null)
            {
                pictureBox1.Image = Bitmap;
            }
        }
    }
}
