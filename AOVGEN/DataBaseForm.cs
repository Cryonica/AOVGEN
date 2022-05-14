﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Telerik.WinControls.UI;
using WinFormAnimation;
using System.Data.OleDb;
using System.Data.SQLite;
using System.Threading;
using GKS_ASU_Loader;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace AOVGEN
{
    public partial class DataBaseForm : Form
    {
        public IRevitExternalService Service { get; set; }
        bool mainformload = false;
        bool connectOK = false;
        public DataBaseForm(IRevitExternalService externalService)
        {
            InitializeComponent();
            this.Icon = Properties.Resources.DataBaseIcon;
            LoadXML();
            AnimationEmergence();
            Service = externalService;
        }
        private void Timer1_Tick(object sender, EventArgs e)
        {
            CheckAndStartmainForm();
        }
        #region XML
        private void LoadXML()
        {
            XmlDocument doc = new XmlDocument();
            string path = @"%AppData%";
            path = Environment.ExpandEnvironmentVariables(path);
            path += @"\Autodesk\Revit\Addins\2021\GKSASU\AOVGen\";
            doc.Load(path + @"Config.xml");
            this.Xmldoc = doc;
            this.Configpath = path + "Config.xml";

            XmlElement documentElement = doc.DocumentElement;
            XmlNodeList aNodes = documentElement.SelectNodes("/Config/DataBase");
            XmlNodeList users = documentElement.SelectNodes("/Config/Users");

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
                        ListViewDataItem listViewDataItem = new ListViewDataItem
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
            XmlNodeList showdialognodes = documentElement.SelectNodes("/Config/STARTUP/SHOWDIALOG");
           

            if (showdialognodes.Item(0).InnerText == "True")
            {
                
            }
            
            radDropDownList1.DataSource = (from XmlNode user in users
                                          let u = user.InnerText
                                          let l = user.Attributes["last"].Value
                                           select (u, l))
                                          .Select(e=> 
                                          {
                                              RadListDataItem radListDataItem = new RadListDataItem
                                              {
                                                  Text = e.u,
                                                  Tag = Convert.ToBoolean(e.l)
                                              };
                                              return radListDataItem;
                                          })
                                          .ToList();
            radDropDownList1.Text = (radDropDownList1.DataSource as IEnumerable<RadListDataItem>)
                .ToList()
                .Where(e => (bool)e.Tag == true)
                .Select(e => e)
                .FirstOrDefault()
                .Text;
        }
        private void WriteToXMLAttr(string dbname)
        {
            try
            {
                XmlDocument doc = this.Xmldoc;
                XmlElement documentElement = doc.DocumentElement;
                XmlNodeList aNodes = documentElement.SelectNodes("/Config/DataBase");
                XmlNodeList users = documentElement.SelectNodes("/Config/Users");
                bool flag = false;
                foreach (XmlNode childnodes in aNodes)
                {
                    XmlNode root = childnodes;
                    foreach (XmlNode child in root.ChildNodes)
                    {

                        if (child.Name == "DBNAME" && child.InnerText == dbname)
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        XmlAttribute Attribute = root.Attributes["last"];
                        Attribute.Value = "true";
                        flag = false;
                    }
                    else
                    {
                        XmlAttribute Attribute = root.Attributes["last"];
                        Attribute.Value = "false";
                    }
                }
                (from XmlNode user in users
                 where user.Attributes["last"].Value == "true"
                 select user)
                 .FirstOrDefault()
                 .Attributes[0].Value = "false";
                (from XmlNode user in users
                 where user.InnerText == radDropDownList1.Text
                 select user)
                        .FirstOrDefault()
                        .Attributes[0].Value = "true";
                doc.Save(this.Configpath);
            }
            catch 
            {
                Thread thread = new Thread(()=> ShowMessage("Не нужно менять фамилии"));
                thread.Start();
                RadButton2_Click(this, new EventArgs());
                connectOK = false;
            }
            
        }
        void ShowMessage(string message) => MessageBox.Show(message, "Ахтунг!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        #endregion

        #region ButtonsClick
        private void RadButton1_Click(object sender, EventArgs e)
        {
            
            
            //check selected item
            if (radListView1.SelectedItem != null)
            {
                string FileExtension= System.IO.Path.GetExtension(radListView1.SelectedItem.Tag.ToString());
                string connectionstr;
                string BDPath = radListView1.SelectedItem.Text;
                string DBFileName = radListView1.SelectedItem.Tag.ToString();
                
                string DataBaseType = string.Empty;
                if (!string.IsNullOrEmpty(BDPath))
                {
                    
                    switch (FileExtension)
                    {
                        case ".sqlite":

                            connectionstr = "Data Source=" + BDPath;
                            try
                            {
                                SQLiteConnection connection = new SQLiteConnection(connectionstr);
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
                            //connectionstr = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + BDPath + ";";
                            //connectionstr = @"Provider=Microsoft.ACE.OLEDB.16.0;Data Source=" + BDPath + ";";
                            try
                            {
                                
                                OleDbConnection connection = new OleDbConnection(connectionstr);
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
                if (connectOK)
                {

                    Thread _thread = new Thread(() =>
                    {
                        if (Service != null)
                        {
                            string username = radDropDownList1.Text;
                            MainForm mainForm = new MainForm(DBFileName, BDPath, DataBaseType, Service, username);
                            mainForm.Load += MainForm_Load;
                            Application.Run(mainForm);
                            

                        }
                        else
                        {
                            string username = radDropDownList1.Text;
                            MainForm mainForm = new MainForm(DBFileName, BDPath, DataBaseType, username);
                            mainForm.Load += MainForm_Load;
                            Application.Run(mainForm);
                            mainForm.Load += MainForm_Load;

                        }
                        
                    });
                    _thread.SetApartmentState(ApartmentState.STA);
                    _thread.Start();
                    System.Timers.Timer aTimer = new System.Timers.Timer();
                    System.Windows.Forms.Timer t = new System.Windows.Forms.Timer
                    {
                        Interval = 10 // specify interval time as you want
                    };
                    t.Tick += new EventHandler(Timer_Tick);
                    this.Visible = false;
                    Loading loading = new Loading();
                    loading.Show();
                    
                    t.Start();


                   // this.Close();
                   
                }

            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            mainformload = true;
        }

        private void RadButton2_Click(object sender, EventArgs e)
        {
            
            Close();
        }
        private void RadButton3_Click(object sender, EventArgs e)
        {
            //string sqlpath = @"C:\Users\akoltakov\Desktop\testbdSQLLite.db";
            //string consql = "Data Source=" + sqlpath + "; Version=3";
            //SQLiteConnection connection1 = new SQLiteConnection(consql);
            //connection1.Open();
            //string qury = "SELECT [ID], [TestCol] FROM [TestTable] ORDER BY [ID] DESC LIMIT 500";
            //SQLiteCommand command = new SQLiteCommand(consql);

            //command.CommandText = qury;
            //SQLiteDataReader sqliteDataReader = command.ExecuteReader();

            //while (sqliteDataReader.Read())
            //{
            //    MessageBox.Show(sqliteDataReader[1].ToString());
            //}
            //sqliteDataReader.Close();
            //connection1.Close();
            string BDPath = radListView1.SelectedItem.Text;
            if (BDPath.Contains("sqlite"))
            {
                string consql = "Data Source=" + BDPath;
                try
                {
                    SQLiteConnection connection = new SQLiteConnection(consql);
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
                    OleDbConnection connection = new OleDbConnection(connectionstr);
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
            foreach (var provider in list)
            {
                if (provider.StartsWith("Microsoft.ACE.OLEDB"))
                {
                    prov = provider.ToString();
                    break;
                }
            }
            return prov;
        }
        #region Animation
        private void AnimationEmergence()
        {
            //new Animator2D(new Path2D(806, 0, 0, 0, 400)
            //    .ContinueTo(50, 0, 150))
            //    .Play(pictureBox1, Animator2D.KnownProperties.Location);
            Animator animator = new Animator
            {
                Paths = new WinFormAnimation.Path(0, 1, 200, 100).ToArray()
            };
            animator.Play(this, Animator.KnownProperties.Opacity);
        }
       
        #endregion

        public XmlDocument Xmldoc { get; set; }
        public string Configpath { get; set; }
        private void CheckAndStartmainForm()
        {
            if (this.Opacity ==0)
           
            {
                
                //timer1.Stop();
                
                //IList<string> bd = timer1.Tag as IList<string>;

                //MainForm mainForm = new MainForm(bd[1].ToString(), bd[0].ToString()); 
                
                //mainForm.Show();
                //Close();

            }
        }
        private class BDMessage
        {
            public static string MessageOk = "Соединение установлено";
            public static string MessageAccessFail = "Невозможно открыть базу данных \n Возможно не установлен Microsoft Access 2010 \n" +
                "или программа запущена в режиеме x32";
            public static string MessageSQLLiteFail = "Невозможно открыть базу данных SQL Lite";


        }

        private void DataBaseForm_Load(object sender, EventArgs e)
        {
            timer2.Start();
            
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            if (timer2.Enabled)
            {
                if (this.Opacity ==0)
                {
                    AnimationEmergence();
                }
                else
                {
                    timer2.Stop();
                }

            }
            
            
        }
        //private static bool IsAlreadyRunning()
        //{
        //    string strLoc = Assembly.GetExecutingAssembly().Location;
        //    FileSystemInfo fileInfo = new FileInfo(strLoc);
        //    string sExeName = fileInfo.Name;
        //    bool bCreatedNew;

        //    Mutex mutex = new Mutex(true, "Global\\" + sExeName, out bCreatedNew);
        //    if (bCreatedNew)
        //        mutex.ReleaseMutex();

        //    return !bCreatedNew;
        //}
        void Timer_Tick(object sender, EventArgs e)
        {
            System.Windows.Forms.Timer timer = (System.Windows.Forms.Timer)sender;
            if (mainformload)
            {
                timer.Stop();

                this.Close();
            }

            //Call method
        }
        public static int SpawnProcessSynchronous(string fileName, string args, out string stdOut, bool isVisible, DataReceivedEventHandler OutputDataReceivedDelegate)
        {
            int returnValue = 0;
            var processInfo = new ProcessStartInfo();
            stdOut = "";
            processInfo.FileName = fileName;
            processInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
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
                    process.OutputDataReceived += OutputDataReceivedDelegate;
                    process.BeginOutputReadLine();
                }
                else
                {
                    stdOut = process.StandardOutput.ReadToEnd();
                }
                // do not reverse order of synchronous read to end and WaitForExit or deadlock
                // Wait for the process to end.  
                process.WaitForExit();
                returnValue = process.ExitCode;
            }
            return returnValue;
        }


    }
}