using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Updater
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        private static readonly ManualResetEvent mre = new ManualResetEvent(false);
        [STAThread]
        
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string destpath = Application.StartupPath;//Environment.ExpandEnvironmentVariables(@"%AppData%");
            
            string source = @"W:\Группа автоматизации\Revit Plugins\Плагины собственной разработки\GKSASU\AOVGen\AOVGEN.exe";
            if (File.Exists(source))
            {
                Thread t = new Thread(new ThreadStart(SleepAndSet));
                t.Start();
                mre.WaitOne();
                try
                {
                    File.Copy(source, destpath + @"\AOVGEN.exe", true);
                    
                   
                }
                catch
                {
                    DialogResult dialogResult = MessageBox.Show("При обновлении следует закрыть Revit", "Ошибка копирования файлов", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                    if (dialogResult == DialogResult.OK)
                    {
                        Process.Start(destpath + @"\AOVGEN.exe");
                    }

                }
                Process.Start(destpath + @"\AOVGEN.exe");
                Application.Exit();
            }
            //Application.Run(new Form1());
            Application.Exit();
        }
        public static void SleepAndSet()
        {
            Thread.Sleep(1000);
            mre.Set();
        }
    }
}
