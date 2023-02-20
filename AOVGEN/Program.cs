using System;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.Windows.Forms;
using GKS_ASU_Loader;

namespace AOVGEN
{
    static class Program
    {
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            CheckExistApp();
            CheckUpdate();
            IRevitExternalService revitService = CheckRevvitConnections();
            RunApplication(revitService);
            

#region Functions
            void CheckExistApp()
            {
                if (Process.GetProcessesByName("AOVGEN").Length <= 1) return;
                MessageBox.Show("Приложение уже запущено");
                Environment.Exit(1);
            }
            void CheckUpdate()
            {
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
                    //Environment.Exit(1);
                   
                }

            }
            IRevitExternalService CheckRevvitConnections()
            {
                IRevitExternalService service = null;
                try
                {

                    ChannelFactory<IRevitExternalService> channelFactory =
                        new("IRevitExternalService");
                    service = channelFactory.CreateChannel();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не смог соединиться с Revit\n" + ex.Message);
                    
                }

                return service;
            }
            void RunApplication(IRevitExternalService externalService)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                if (externalService != null)
                {
                    Application.Run(new DataBaseForm(externalService));
                }
                else
                {
                    Application.Run(new DataBaseForm());
                }

            }
#endregion

        }

    }
}