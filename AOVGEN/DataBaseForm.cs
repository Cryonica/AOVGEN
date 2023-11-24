﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using AOVGEN.Properties;
using GKS_ASU_Loader;
using Telerik.WinControls.UI;
using WinFormAnimation;
using Path = System.IO.Path;

namespace AOVGEN
{
    public partial class DataBaseForm : Form
    {
        private bool connectOK;
        private bool mainformload;
        private string _acadVers;
        public DataBaseForm(IRevitExternalService externalService)
        {
            InitializeComponent();
            Icon = Resources.DataBaseIcon;
            try
            {
                LoadXML();
            }
            catch (Exception e) { MessageBox.Show(e.Message); };
            
            AnimationEmergence();
            Service = externalService;
        }
        public DataBaseForm()
        {
            InitializeComponent();
            Icon = Resources.DataBaseIcon;
            try
            {
                LoadXML();
            }
            catch { };
            AnimationEmergence();
        }

        public string Configpath { get; set; }
        public IRevitExternalService Service { get; set; }
        public XmlDocument Xmldoc { get; set; }

        public static int SpawnProcessSynchronous(string fileName, string args, out string stdOut, bool isVisible, DataReceivedEventHandler OutputDataReceivedDelegate)
        {
            int returnValue = 0;
            var processInfo = new ProcessStartInfo();
            stdOut = "";
            processInfo.FileName = fileName;
            processInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            Debug.Print("Set working directory to: {0}", processInfo.WorkingDirectory);

            processInfo.WindowStyle = isVisible ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardOutput = true;
            processInfo.CreateNoWindow = true;

            processInfo.Arguments = args;
            using (Process process = Process.Start(processInfo))
            {
                if (OutputDataReceivedDelegate != null)
                {
                    if (process != null)
                    {
                        process.OutputDataReceived += OutputDataReceivedDelegate;
                        process.BeginOutputReadLine();
                    }
                }
                else
                {
                    if (process != null) stdOut = process.StandardOutput.ReadToEnd();
                }
                // do not reverse order of synchronous read to end and WaitForExit or deadlock
                // Wait for the process to end.  
                if (process == null) return returnValue;
                process.WaitForExit();
                returnValue = process.ExitCode;
            }
            return returnValue;
        }

        public string FindProvider()
        {
            string prov = string.Empty;
            var reader = OleDbEnumerator.GetRootEnumerator();

            var list = new List<String>();
            while (reader.Read())
            {
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetName(i) == "SOURCES_NAME")
                    {
                        list.Add(reader.GetValue(i).ToString());
                    }
                }
            }
            reader.Close();
            foreach (var provider in list.Where(provider => provider.StartsWith("Microsoft.ACE.OLEDB")))
            {
                prov = provider;
                break;
            }
            return prov;
        }

        private void CheckAndStartmainForm()
        {
          return;
        }

        private void DataBaseForm_Load(object sender, EventArgs e)
        {
            timer2.Start();

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            System.Windows.Forms.Timer timer = (System.Windows.Forms.Timer)sender;
            if (!mainformload) return;
            timer.Stop();
            Close();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            CheckAndStartmainForm();
        }
        #region XML
        private static void ShowMessage(string message) =>
            MessageBox.Show(message, "Ахтунг!", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        private void LoadXML()
        {
            XmlDocument doc = new();
            string path = @"%AppData%";
            path = Environment.ExpandEnvironmentVariables(path);
            path += Resources.PluginFolder;// @"\Autodesk\Revit\Addins\2020\ASU\AOVGen\";
            doc.Load(path + @"Config.xml");
            Xmldoc = doc;
            Configpath = path + "Config.xml";

            XmlElement documentElement = doc.DocumentElement;
            XmlNodeList aNodes = documentElement?.SelectNodes("/Config/DataBase");
            XmlNodeList users = documentElement?.SelectNodes("/Config/Users");
            XmlNodeList acadVersions = documentElement?.SelectNodes("/Config/AcadVersion/ProgId");
            XmlNodeList configVersions = documentElement?.SelectNodes("/Config/ConfigVersion");

            
            ResourceManager resourceManager = new(Resources.ResourceManager.BaseName, typeof(Program).Assembly);
            ResourceSet resourceSet = resourceManager.GetResourceSet(System.Threading.Thread.CurrentThread.CurrentCulture, true, true);
            if (resourceSet != null)
            {
                var resourceNames = resourceSet.Cast<DictionaryEntry>()
                .Where(entry => entry.Value.ToString().Contains("acad.exe"))
                .Select(entry => new string[2] { entry.Key.ToString(), entry.Value.ToString() })
                .OrderBy(entry => entry[0]);

                if (resourceNames.Count() > 0)
                {
                    foreach(var resourceName in resourceNames)
                    {
                        var vals = resourceName[1].Split(';');
                        RadListDataItem radListDataItem = new(vals[1]);
                        radListDataItem.Tag = resourceName[0];
                        radDropDownList2.Items.Add(radListDataItem);

                    }
                }
            }


            if (configVersions != null)
            {
                string vers = (from XmlNode version in configVersions
                          let v = version.InnerText
                          select v).First();
                if (vers != Resources.PluginVersion)
                {
                    MessageBox.Show("Файл настроек не от данной версии плагина");
                    Environment.Exit(1);

                }
            }

            string dbname = string.Empty;
            string dbpath = string.Empty;
            int cnt = 0;
            int trueindex = 0;

            if (aNodes != null)
                foreach (XmlNode childnodes in aNodes)
                {
                    XmlNode root = childnodes;

                    if (root.Attributes != null && root.Attributes["last"].Value == "true") trueindex = cnt;
                    cnt++;
                    foreach (XmlNode child in root.ChildNodes)
                    {
                        switch (child.Name)
                        {
                            case "DBPATH" when dbpath == string.Empty:
                                dbpath = child.InnerText;
                                break;
                            case "DBNAME" when dbname == string.Empty:
                                dbname = child.InnerText;
                                break;
                        }
                    }

                    if (dbpath != string.Empty)
                    {
                        ListViewDataItem listViewDataItem = new()
                        {
                            Text = dbpath
                        };
                        radListView1.Items.Add(listViewDataItem);
                        listViewDataItem.Tag = dbname;
                        dbname = string.Empty;
                        dbpath = string.Empty;
                    }
                }

            radListView1.SelectedIndex = trueindex;
            //XmlNodeList showdialognodes = documentElement?.SelectNodes("/Config/STARTUP/SHOWDIALOG");
            
            if (configVersions != null)
            {
                _acadVers = (from XmlNode version in acadVersions
                                   let v = version.InnerText
                            select v).First();

                
                radDropDownList2.SelectedItem = radDropDownList2.Items
                    .Where(i => i.Tag.ToString() == _acadVers)
                    .FirstOrDefault();
                
            }
            radDropDownList1.DataSource = (from XmlNode user in users
                                          let u = user.InnerText
                                          let xmlAttributeCollection = user.Attributes
                                          where xmlAttributeCollection != null
                                          let l = xmlAttributeCollection["last"].Value
                                           select (u, l))
                                          .Select(e=> 
                                          {
                                              var (u, l) = e;
                                              RadListDataItem radListDataItem = new()
                                              {
                                                  Text = u,
                                                  Tag = Convert.ToBoolean(l)
                                              };
                                              return radListDataItem;
                                          })
                                          .ToList();
            try
            {
                radDropDownList1.Text = (radDropDownList1.DataSource as IEnumerable<RadListDataItem>)
                    .ToList()
                    .Where(e => (bool)e.Tag)
                    .Select(e => e)
                    .FirstOrDefault()
                    ?.Text;

            }
            catch (Exception ex)
            {
               Console.Write(ex.Message);
            }
            
        }
       
        private void WriteToXMLAttr(string dbname)
        {
            try
            {
                XmlDocument doc = Xmldoc;
                XmlElement documentElement = doc.DocumentElement;
                XmlNodeList aNodes = documentElement?.SelectNodes("/Config/DataBase");
                XmlNodeList users = documentElement?.SelectNodes("/Config/Users");
                XmlNodeList acadVers = documentElement?.SelectNodes("/Config/AcadVersion/ProgId");

                bool flag = false;
                if (aNodes != null)
                    foreach (XmlNode childnodes in aNodes)
                    {
                        if (childnodes.ChildNodes
                            .Cast<XmlNode>()
                            .Any(child => child.Name == "DBNAME" && child.InnerText == dbname)) flag = true;
                        if (flag)
                        {
                            if (childnodes.Attributes != null)
                            {
                                XmlAttribute Attribute = childnodes.Attributes["last"];
                                Attribute.Value = "true";
                            }

                            flag = false;
                        }
                        else
                        {
                            if (childnodes.Attributes == null) continue;
                            XmlAttribute Attribute = childnodes.Attributes["last"];
                            Attribute.Value = "false";
                        }
                    }

                (from XmlNode user in users
                 where user.Attributes["last"].Value == "true"
                 select user)
                 .FirstOrDefault()
                 .Attributes[0].Value = "false";
                XmlNode first = users?
                    .Cast<XmlNode>()
                    .FirstOrDefault(user => user.InnerText == radDropDownList1.Text);

                if (first?.Attributes != null)
                    first.Attributes[0].Value = "true";

                string selectedAcadVers = radDropDownList2.SelectedItem.Tag.ToString();

                XmlNode second = acadVers?
                    .Cast<XmlNode>()
                    .FirstOrDefault().LastChild;
                second.InnerText = selectedAcadVers;
                _acadVers = selectedAcadVers;
                doc.Save(Configpath);
            }
            catch 
            {
                Thread thread = new(()=> ShowMessage("Не нужно менять фамилии"));
                thread.Start();
                RadButton2_Click(this, EventArgs.Empty);
                connectOK = false;
            }
            
        }
        #endregion

        #region ButtonsClick
        private void MainForm_Load(object sender, EventArgs e)
        {
            mainformload = true;
        }

        private void RadButton1_Click(object sender, EventArgs e)
        {
            //check selected item
            if (radListView1.SelectedItem == null) return;
            string FileExtension= Path.GetExtension(radListView1.SelectedItem.Tag.ToString());
            string BDPath = radListView1.SelectedItem.Text;
            string DBFileName = radListView1.SelectedItem.Tag.ToString();
                
            string DataBaseType = string.Empty;
            if (!string.IsNullOrEmpty(BDPath))
            {
                string connectionstr;
                switch (FileExtension)
                {
                    case ".sqlite":
                        connectionstr = "Data Source=" + BDPath;
                        try
                        {
                            SQLiteConnection connection = new(connectionstr);
                            connection.Open();
                            if (connection.State == ConnectionState.Open) connectOK = true;
                            connection.Close();
                            DataBaseType = "SQLite";
                            WriteToXMLAttr(radListView1.SelectedItem.Tag.ToString());

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        break;
                    case ".mdb":
                    case ".accdb":

                        connectionstr = "Provider=" + FindProvider() + ";Data Source=" + BDPath + ";";
                        try
                        {
                                
                            OleDbConnection connection = new(connectionstr);
                            connection.Open();
                            if (connection.State == ConnectionState.Open) connectOK = true;
                            connection.Close();
                            DataBaseType = "Access";
                            WriteToXMLAttr(radListView1.SelectedItem.Tag.ToString());
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            //MessageBox.Show(ex.StackTrace);
                        }
                        break;
                }
            }

            if (!connectOK) return;
            Thread _thread = new(() =>
            {
                if (Service != null)
                {
                    string username = radDropDownList1.Text;
                    MainForm mainForm = new(DBFileName, BDPath, DataBaseType, Service, username, _acadVers );
                    mainForm.Load += MainForm_Load;
                    Application.Run(mainForm);
                            

                }
                else
                {
                    string username = radDropDownList1.Text;
                    MainForm mainForm = new(DBFileName, BDPath, DataBaseType, username);
                    mainForm.Load += MainForm_Load;
                    Application.Run(mainForm);
                    mainForm.Load += MainForm_Load;

                }
                        
            });
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
            
            System.Windows.Forms.Timer t = new()
            {
                Interval = 10 // specify interval time as you want
            };
            t.Tick += Timer_Tick;
            Visible = false;
            Loading loading = new();
            loading.Show();
            t.Start();
            
        }

        private void RadButton2_Click(object sender, EventArgs e) => Close();
        
        private void RadButton3_Click(object sender, EventArgs e)
        {
            string BDPath = radListView1.SelectedItem.Text;
            if (BDPath.Contains("sqlite"))
            {
                string consql = "Data Source=" + BDPath;
                try
                {
                    SQLiteConnection connection = new(consql);
                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                    {
                        MessageBox.Show(BDMessage.MessageOk);
                    }
                    connection.Close();
                }
                catch
                {
                    MessageBox.Show(BDMessage.MessageSQLLiteFail);
                }
                
            }
            else
            {
                string connectionstr = "Provider=" + FindProvider() + ";Data Source=" + BDPath + ";";
                try
                {
                    OleDbConnection connection = new(connectionstr);
                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                        MessageBox.Show(BDMessage.MessageOk);
                    }
                }
                catch
                {
                    MessageBox.Show(BDMessage.MessageAccessFail);
                }
            }

        }
        #endregion
        #region Animation
        private void AnimationEmergence()
        {
            //new Animator2D(new Path2D(806, 0, 0, 0, 400)
            //    .ContinueTo(50, 0, 150))
            //    .Play(pictureBox1, Animator2D.KnownProperties.Location);
            Animator animator = new()
            {
                Paths = new WinFormAnimation.Path(0, 1, 200, 100).ToArray()
            };
            animator.Play(this, Animator.KnownProperties.Opacity);
        }

        #endregion
        private void Timer2_Tick(object sender, EventArgs e)
        {
            if (!timer2.Enabled) return;
            if (Opacity == 0)
            {
                AnimationEmergence();
            }
            else
            {
                timer2.Stop();
            }
        }

        private static class BDMessage
        {
            internal const string MessageAccessFail = "Невозможно открыть базу данных \n Возможно не установлен Microsoft Access 2010 \n" + "или программа запущена в режиеме x32";
            internal const string MessageOk = "Соединение установлено";
            internal const string MessageSQLLiteFail = "Невозможно открыть базу данных SQL Lite";
        }
    }
}
