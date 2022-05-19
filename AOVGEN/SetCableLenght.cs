using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using AutoCAD;
using Telerik.WinControls.UI;

namespace AOVGEN
{
#pragma warning disable
    class SetCableLenght
    {
        #region Variables
        readonly RadGridView RadGridView;
        readonly SQLiteConnection connection;
        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);
        const uint WM_KEYDOWN = 0x0100;
        const int VK_ESCAPE = 0x01B;
        const int SC_ESCAPE = 0x10001;
        private enum ShowWindowEnum
        {
            Hide = 0,
            ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
            Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
            Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
            Restore = 9, ShowDefault = 10, ForceMinimized = 11
        }

        #endregion
        internal SetCableLenght (RadGridView radGridView, SQLiteConnection sQLiteConnection)
        {
            RadGridView = new RadGridView();
            RadGridView = radGridView;
            connection= sQLiteConnection;
        }
        internal void ExcecuteRandom(double lenght, double addition, bool allowgen, bool usepowercables, double powercablelenght)
        {
            
            DataTable dt = new DataTable();
            foreach (GridViewDataColumn column in RadGridView.Columns)
            {
                dt.Columns.Add(column.Name, column.DataType);
            }
            foreach (GridViewRowInfo row in RadGridView.SelectedRows)
            {
                DataRow dataRow = dt.NewRow();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    dataRow[i] = row.Cells[i].Value;
                }
                dt.Rows.Add(dataRow);
            }
            var grouped = from DataRow dr in dt.Rows group dr by dr["Куда"];
            Dictionary<string, double> dict = new Dictionary<string, double>();
            var rnd = new Random();
            foreach (var k in grouped)
            {
                
                int min = Convert.ToInt32(lenght - addition);
                int max = Convert.ToInt32(lenght + addition);
                var From = (string)(k.ElementAt(0)["Куда"]);
                var Lenght = rnd.Next(min, max);
                dict.Add(From, Lenght);

            }

            SQLiteCommand command = new SQLiteCommand { Connection = connection };
            foreach (var row in RadGridView.SelectedRows)
            {
                Cable cable = (Cable)row.Tag;
                if (cable.Attrubute == Cable.CableAttribute.P)
                {
                    if (usepowercables)
                    {
                        cable.Lenght = allowgen ? Convert.ToDouble(dict[cable.ToPosName]) : powercablelenght;
                    }
                    else
                    {
                        cable.Lenght = 0;
                    }
                }
                else
                {
                    cable.Lenght = Convert.ToDouble(dict[cable.ToPosName]);
                }


                row.Cells[6].Value = cable.Lenght.ToString();
                var query = $"UPDATE Cable SET CableLenght = '{cable.Lenght}' WHERE GUID = '{cable.cableGUID}'";
                command.CommandText = query;
                command.ExecuteNonQuery();
            }
            command.Dispose();
             
            //var result = from row in RadGridView.SelectedRows
            //            group row.Cells[1].
        }
        internal void ExcecuteAutocadFileName(AcadDocument acadDoc)
        {
            //object[] attributes = cableblock.GetAttributes();
            Dictionary<string, string> cableblockdict = new Dictionary<string, string>();

            foreach (AcadEntity ent in acadDoc.ModelSpace)
            {
                if (ent is AcadBlockReference block && (block.EffectiveName == "adv_cable" || block.EffectiveName == "cable_dummy"))
                {
                    object[] attributes = block.GetAttributes();
                    IEnumerable<AcadAttributeReference> guidattr = attributes.OfType<AcadAttributeReference>()
                        .Where(i => i.TagString == "AOVGEN_GUID");
                    if (guidattr.Count() > 0)
                    {
                        IEnumerable<AcadAttributeReference> cablen = attributes.OfType<AcadAttributeReference>()
                            .Where(i => i.TagString == "ДЛИНА_КАБЕЛЯ");
                        if (cablen.Count() > 0 && guidattr.ElementAt(0).TextString != string.Empty)
                        {
                            if (!cableblockdict.ContainsKey(guidattr.ElementAt(0).TextString))
                            {
                                string cablelenstring = cablen.ElementAt(0).TextString;
                                if (cablelenstring.Contains("м"))
                                {
                                    string[] arr = cablelenstring.Split(' ');
                                    double.TryParse(arr[0], out double len);
                                    cableblockdict.Add(guidattr.ElementAt(0).TextString, len.ToString());
                                }
                                else
                                {
                                    cableblockdict.Add(guidattr.ElementAt(0).TextString, cablen.ElementAt(0).TextString);
                                }

                            }

                        }
                    }
                }
                else
                {
                }
            }

            foreach (var row in RadGridView.SelectedRows)
            {
                Cable cable = (Cable)row.Tag;
                string guid = cable.cableGUID;
                if (cableblockdict.ContainsKey(guid))
                {
                    string cablelenstr = cableblockdict[guid];
                    if (cablelenstr.Contains("м"))
                    {
                        string[] arr = cablelenstr.Split(' ');
                        double.TryParse(arr[0], out double len);
                        cable.Lenght = len;
                    }
                    else
                    {
                        double.TryParse(cablelenstr, out double len);
                        cable.Lenght = len;
                    }
                }
                else
                {
                    cable.Lenght = 0;

                }
                row.Cells[6].Value = cable.Lenght.ToString();



            }
           
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection
            };
            foreach (KeyValuePair<string, string> keyValuePair in cableblockdict)
            {
                string query = $"UPDATE Cable SET CableLenght = '{keyValuePair.Value}' WHERE GUID = '{keyValuePair.Key}'";
                command.CommandText = query;
                command.ExecuteNonQuery();
            }
            command.Dispose();
            
        }
        internal void ExecuteExchange(string filename)
        {
            dynamic cadApp = null;
            try
            {
                (string path, string progID) Acad2016;
                Acad2016.progID = "AutoCAD.Application";
                Acad2016.path = @"\Autodesk\AutoCAD 2019\acad.exe";

                cadApp = Marshal.GetActiveObject(Acad2016.progID);

            }
            catch
            {

            }
            if (cadApp != null)
            {
                Dictionary<string, string> cabs = new Dictionary<string, string>();
                IntPtr hWnd =IntPtr.Zero;
                try
                {

                    Process[] localByName = Process.GetProcessesByName("acad");
                    Process process = localByName[0];
                    hWnd = process.MainWindowHandle;
                    if (hWnd != IntPtr.Zero)
                    {
                        //ShowWindow(hWnd, (int)ShowWindowEnum.ShowNormalNoActivate);

                        AcadApplication acadApp = cadApp;
                        AcadDocument acadDocument = acadApp.ActiveDocument;
                        if (acadDocument != null)
                        {
                            
                            acadApp.Visible = false;
                            
                            acadDocument.SendCommand("adv-programme-cable-routings-data-propagated-show\n");
                            acadDocument.SendCommand("(SETQ myvar nil)\n");
                            acadDocument.SendCommand("(SETQ myvar(vl-bb-ref '*adv_cable_routings_data*))\n");
                            acadDocument.SendCommand($"(setq TargetFile (open \u0022{filename}\u0022 \u0022w\u0022))\n");
                            acadDocument.SendCommand("(Princ (vl-bb-ref '*adv_cable_routings_data*) TargetFile)\n");
                            acadDocument.SendCommand("(close TargetFile)\n");
                            acadDocument.SendCommand("(textpage)\n");
                            
                            //System.Windows.Forms.SendKeys.Send("{ESC}");
                            acadApp.Visible = true;
                            string alltext = File.ReadAllText(filename, Encoding.Default);
                            alltext = alltext.Replace("(", "").Replace(")", "");
                            string[] arr = alltext.Split(' ');
                            
                            
                            for (int i = 0; i < arr.Length - 1; i += 2)
                            {
                                if (!cabs.ContainsKey(arr[i])) cabs.Add(arr[i], arr[i + 1]);
                            }
                        }
                    }
                    //ShowWindow(hWnd, (int)ShowWindowEnum.Restore);
                    



                }
                catch (Exception ex)
                {
                    if (cadApp != null)
                    {
                        ((AcadApplication)cadApp).Visible = true;
                    }
                    else
                    {
                        ShowWindow(hWnd, (int)ShowWindowEnum.Restore);
                    }
                    
                    
                    MessageBox.Show(ex.Message + "\n"+ ex.StackTrace);
                }

                string CableUpdate;
                SQLiteCommand command = new SQLiteCommand
                {
                    Connection = connection
                };
                foreach (GridViewRowInfo row in RadGridView.SelectedRows)
                {
                    if (row.Tag != null)
                    {
                        CableUpdate = string.Empty;
                        Cable cable = (Cable)row.Tag;
                        string cabName = cable.CableName;
                        if (cabs.ContainsKey(cabName))
                        {
                            cable.Lenght = Convert.ToDouble(cabs[cabName]);
                            row.Cells[6].Value = cable.Lenght.ToString();
                            CableUpdate = $"UPDATE Cable SET CableLenght= '{cable.Lenght}' " +
                                                    $"WHERE [GUID] = '{cable.cableGUID}'";
                            command.CommandText = CableUpdate;
                            command.ExecuteNonQuery();
                        }    
                        
                        
                    }
                }



            }
            
            WindowHelper.BringProcessToFront(Process.GetCurrentProcess());
        }

    }
    public static class WindowHelper
    {
        public static void BringProcessToFront(Process process)
        {
            IntPtr handle = process.MainWindowHandle;
            if (IsIconic(handle))
            {
                ShowWindow(handle, SW_RESTORE);
            }

            SetForegroundWindow(handle);
        }

        const int SW_RESTORE = 9;

        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);
        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);
        [DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);
    }

    

    
}
