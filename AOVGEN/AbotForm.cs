using System;
using System.Reflection;
using System.Windows.Forms;
using WinFormAnimation;
using Timer = System.Windows.Forms.Timer;


namespace AOVGEN
{
    public partial class AbotForm : Telerik.WinControls.UI.RadForm
    {
        public AbotForm()
        {
            InitializeComponent();
        }

        private void AbotForm_Load(object sender, EventArgs e)
        {
            label1.Text = "Build v. " + Assembly.GetExecutingAssembly().GetName().Version;
            bunifuImageButton1.Rotate(360, true, 2000);
        }

        private void radButton1_Click(object sender, EventArgs e)
        {

            //Create animation

            var timer = new Timer
            {
                Interval = 100
            };
            timer.Tick += Timer_Tick;
            timer.Start();
            new Animator(
                    new Path(1, 0, 400, 0).ToArray(),
                    FPSLimiterKnownValues.NoFPSLimit)
                .Play(this, Animator.KnownProperties.Opacity);

            
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (Opacity != 0) return;
            ((Timer)sender).Stop();
            DialogResult = DialogResult.Cancel;
        }

        private void bunifuImageButton1_ZoomedIn(object sender, EventArgs e)
        {
            
            
            
        }
    }
}
