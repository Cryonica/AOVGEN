using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AOVGEN.Helpers
{
    static class Helper
    {
        public static dynamic StartAutocad(string path, string progID)
        {
            string programFiles = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
            //string acadfile = programFiles + Acad.path;// + " //product ACAD //language " + '\u0022' + "ru - RU" + '\u0022';
            Process acadProc = new();
            acadProc.StartInfo.FileName = path;
            acadProc.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            try
            {
                acadProc.Start();
            }
            catch
            {
                //MessageBox.Show($"Не могу найти Autocad по пути \n {acadfile}\nгенерация схем невозомжна");

                throw new ApplicationException($"Не могу найти Autocad по пути \n {path}\nгенерация схем невозомжна");
            }

            if (!acadProc.WaitForInputIdle(300000))
                throw new ApplicationException("Слишком долго стартует Autocad, выход");
            dynamic acadApp;
            while (true)
            {

                try
                {
                    acadApp = Marshal.GetActiveObject(progID);
                    return acadApp;
                }
                catch (COMException ex)
                {
                    const uint MK_E_UNAVAILABLE = 0x800401e3;
                    if ((uint)ex.ErrorCode != MK_E_UNAVAILABLE) throw;
                    Thread.Sleep(1000);
                }
            }





        }
    }
}
