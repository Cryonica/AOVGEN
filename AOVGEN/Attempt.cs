using System;
using Telerik.WinControls.UI;
using WinFormAnimation;

namespace AOVGEN
{
    public partial class Attempt : RadForm
    {
        public int Num { get; set; }
        public Attempt()
        {
            InitializeComponent();
            
        }

        private void Attempt_Load(object sender, EventArgs e)
        {
            label2.Text = Num.ToString();
            AnimationClose();
        }
        private void AnimationClose()
        {
            timer1.Start();

            Animator animator = new Animator
            {
                Paths = new Path(1, 0, 800, 100).ToArray()
            };
            animator.Play(this, Animator.KnownProperties.Opacity);

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Opacity == 0) Close();
        }
    }
}
