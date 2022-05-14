using System;
using System.Windows.Forms;
using GKS_ASU_Loader;
using System.ServiceModel;
using System.Diagnostics;
using System.IO;
using  System.Linq;


namespace AOVGEN
{
    static class Program
    {
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {


            var present = Process
                .GetProcesses()
                .Where(e=> e.ProcessName == Process.GetCurrentProcess().ProcessName)
                .ToList();
            if (present.Count > 1)
            {
                MessageBox.Show("Приложение уже запущено");
                Environment.Exit(1);
            }
                
            

            IRevitExternalService service = null;

            const string source = @"W:\Группа автоматизации\Revit Plugins\Плагины собственной разработки\GKSASU\AOVGen\buildinfo.xml";
            if (File.Exists(source))
            {
                string destpath = Application.StartupPath;
                try
                {
                    string zipfile = destpath + @"\AOVGen.zip";
                    if (File.Exists(zipfile))
                    {
                        File.Delete(zipfile);
                    }

                }
                catch { }
                
                
            }
            else
            {
                MessageBox.Show("Похоже, проблема с доступом на сервер группы автоматизации");
                Environment.Exit(1);
                return;
            }

            try
            {
                System.ServiceModel.ChannelFactory<IRevitExternalService> channelFactory =
                new ChannelFactory<IRevitExternalService>("IRevitExternalService");
                service = channelFactory.CreateChannel();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Не смог соединиться с Revit\n" + ex.Message);
                Process.GetCurrentProcess().Kill();
                    //Console.WriteLine(ex);
                    //Console.ReadKey();
                    //return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DataBaseForm(service));
            
        }

    }
}