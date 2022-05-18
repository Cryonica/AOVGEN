using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Telerik.WinControls;
using Telerik.WinControls.UI;
using System.IO;
using System.Net;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Linq;

namespace AOVGEN
{
    public partial class CableSettings : Telerik.WinControls.UI.RadForm
    {
        
        public string DBFilePath { get; set; }
        internal Building Building { get; set; }
        internal Project Project { get; set; }

        public CableSettings()
        {
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void radButton1_Click(object sender, EventArgs e)
        {
            try
            {
                string checkempty(string input) => input != string.Empty ? input : string.Empty;
                bool checkpipetray(RadCheckedListDataItem item) => item.Checked;
                string cableD = checkempty(radDropDownList1.Text);
                string cableA = checkempty(radDropDownList3.Text);
                string cableC = checkempty(radDropDownList5.Text);
                string cableP = checkempty(radDropDownList7.Text);

                bool WriteCableP = radToggleSwitch1.Value;
                var pipeA = checkpipetray(radCheckedDropDownList1.Items[0]);
                var pipeD = checkpipetray(radCheckedDropDownList2.Items[0]);
                var pipeC = checkpipetray(radCheckedDropDownList3.Items[0]);
                var pipeP = checkpipetray(radCheckedDropDownList4.Items[0]);
                var trayA = checkpipetray(radCheckedDropDownList1.Items[1]);
                var trayD = checkpipetray(radCheckedDropDownList2.Items[1]);
                var trayC = checkpipetray(radCheckedDropDownList3.Items[1]);
                var trayP = checkpipetray(radCheckedDropDownList4.Items[1]);
                var writepumpcable = checkpipetray(radCheckedDropDownList5.Items[0]);
                var writevalvecable = checkpipetray(radCheckedDropDownList5.Items[1]);
                

                var buildpreset = radDropDownList9.Text;
                var devider = radDropDownList10.Text;
                var cablename = radTextBoxControl1.Text;
                
                bool flag = WriteCableP == false && cableP == string.Empty;

                if
                (
                    cableD != string.Empty
                    && cableA != string.Empty
                    && cableC != string.Empty
                    && (flag == false)
                       
                )
                   

                {
                    bool flagCheckExist = CheckExist("BuildSetting", "Place", this.Building.BuildGUID);
                    string query;
                    if (flagCheckExist)
                    {
                       
                        query = $"UPDATE BuildSetting SET CableA = '{cableA}', CableD = '{cableD}', CableC = '{cableC}', CableP = '{cableP}'," +
                                $"CableAPipe = '{pipeA}', CableDPipe = '{pipeD}', CableCPipe = '{pipeC}', CablePPipe = '{pipeP}', " +
                                $"CableATray = '{trayA}', CableDTray = '{trayD}', CableCTray = '{trayC}', CablePTray = '{trayP}', BuildPreset = '{buildpreset}', Devider = '{devider}', CableName = '{cablename}', WriteCableP = '{WriteCableP}', " +
                                $"WritePumpCable = '{writepumpcable}', WriteValveCable = '{writevalvecable}' " +
                                $"WHERE Place = '{this.Building.BuildGUID}'";
                    }
                    else
                    {
                        query = $"INSERT INTO BuildSetting ([Project], [Place], CableA, CableD, CableC, CableP, " +
                                "CableAPipe, CableDPipe, CableCPipe, CablePPipe, " +
                                "CableATray, CableDTray, CableCTray, CablePTray, BuildPreset, Devider, Cablename, WriteCableP, WritePumpCable, WriteValveCable) " +
                                $"VALUES('{this.Project.GetGUID()}','{this.Building.BuildGUID}','{cableA}','{cableD}','{cableC}', '{cableP}', " +
                                $"'{pipeA}', '{pipeD}', '{pipeC}', '{pipeP}', " +
                                $"'{trayA}', '{trayD}', '{trayC}', '{trayP}', '{buildpreset}', '{devider}', '{cablename}' , '{WriteCableP}', " +
                                $"'{writepumpcable}', '{writevalvecable}');";
                    }

                    using (SQLiteConnection connection = OpenDB())
                    {
                       
                        connection.Open();
                        if (connection.State == ConnectionState.Open)
                        {
                            SQLiteCommand command = new SQLiteCommand
                            {
                                Connection = connection,
                                CommandText = query
                            };
                            try
                            {
                                command.ExecuteNonQuery();

                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }

                            command.Dispose();
                        }
                        connection.Close();
                    }



                    this.Close();
                }
                else
                {
                    MessageBox.Show("Заполните все поля с типами кабелей и их сечением");
                }

                //foreach (RadCheckedListDataItem item in radCheckedDropDownList1.Items)
                //{
                //    if (item.Checked)
                //    {
                //        MessageBox.Show(item.Text);
                //    }
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }
        }

        private void bunifuImageButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CableSettings_Load(object sender, EventArgs e)
        {
            
            
            label3.Text = string.Empty;
            using (SQLiteConnection connection = OpenDB())
            {
                connection.Open();
                if (connection.State == ConnectionState.Open)
                {
                    string querycabletypes = "SELECT DISTINCT CableType FROM CableTypes";
                    SQLiteCommand command = new SQLiteCommand
                    {
                        Connection = connection,
                        CommandText = querycabletypes
                    };
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        radDropDownList1.Items.Add(reader[0].ToString());
                        radDropDownList3.Items.Add(reader[0].ToString());
                        radDropDownList5.Items.Add(reader[0].ToString());
                        radDropDownList7.Items.Add(reader[0].ToString());
                    }
                    reader.Close();
                    
                    string settingsguery = $"SELECT * FROM BuildSetting WHERE Place = '{this.Building.BuildGUID}'";
                    command.CommandText = settingsguery;
                    using (SQLiteDataReader reader2 = command.ExecuteReader())
                    {

                        if (reader2.HasRows)
                        {
                            string cabA, cabD, cabC, cabP, cabACS, cabDCS, cabCCS, cabPCS, devider, cablename, cabApipe, cabDpipe, cabCpipe, cabPpipe, cabAtray, cabDtray, cabCtray, cabPtray, buildpreset, writecableP, writepumpcable, writevalvecable;
                            cabA = cabD = cabC = cabP = cabACS = cabDCS = cabCCS = cabPCS = devider = cablename = cabApipe = cabDpipe = cabCpipe = cabPpipe = cabAtray = cabDtray = cabCtray = cabPtray = buildpreset  = writecableP = writepumpcable = writevalvecable = string.Empty;
                            while (reader2.Read())
                            {
                                //reader2[0] - ID
                                //reader2[1] - project guid
                                //reader2[3] - build guid
                                cabA = reader2[3].ToString();
                                cabD = reader2[4].ToString();
                                cabC = reader2[5].ToString();
                                cabP = reader2[6].ToString();
                                cabACS = reader2[7].ToString();
                                cabDCS = reader2[8].ToString();
                                cabCCS = reader2[9].ToString();
                                cabPCS = reader2[10].ToString();
                                devider = reader2[11].ToString();
                                cablename = reader2[12].ToString();
                                cabApipe = reader2[13].ToString();
                                cabDpipe = reader2[14].ToString();
                                cabCpipe = reader2[15].ToString();
                                cabPpipe = reader2[16].ToString();
                                cabAtray = reader2[17].ToString();
                                cabDtray = reader2[18].ToString();
                                cabCtray = reader2[19].ToString();
                                cabPtray = reader2[20].ToString();
                                buildpreset = reader2[21].ToString();
                                writecableP = reader2[22].ToString();
                                writepumpcable = reader2[23].ToString();
                                writevalvecable = reader2[24].ToString();
                            }
                            reader2.Close();
                            radDropDownList1.Text = cabD;
                            radDropDownList3.Text = cabA;
                            radDropDownList5.Text = cabC;
                            radDropDownList7.Text = cabP;
                            radTextBoxControl1.Text = cablename;
                            radDropDownList9.Text = buildpreset;
                            radDropDownList10.Text = devider;
                            label3.Text = cablename + devider;
                            radCheckedDropDownList1.Items[0].Checked = Convert.ToBoolean(cabApipe);
                            radCheckedDropDownList1.Items[1].Checked = Convert.ToBoolean(cabAtray);
                            radCheckedDropDownList2.Items[0].Checked = Convert.ToBoolean(cabDpipe);
                            radCheckedDropDownList2.Items[1].Checked = Convert.ToBoolean(cabDtray);
                            radCheckedDropDownList3.Items[0].Checked = Convert.ToBoolean(cabCpipe);
                            radCheckedDropDownList3.Items[1].Checked = Convert.ToBoolean(cabCtray);
                            radCheckedDropDownList4.Items[0].Checked = Convert.ToBoolean(cabPpipe);
                            radCheckedDropDownList4.Items[1].Checked = Convert.ToBoolean(cabPtray);
                            radCheckedDropDownList5.Items[0].Checked = Convert.ToBoolean(writepumpcable);
                            radCheckedDropDownList5.Items[1].Checked = Convert.ToBoolean(writevalvecable);
                            radToggleSwitch1.Value = Convert.ToBoolean(writecableP);

                        }
                        


                    }
                    
                    command.Dispose();
                   
                }
                connection.Close();
            }
        }
        private SQLiteConnection OpenDB()
        {
            string BDPath = this.DBFilePath;
            string connectionstr = @"Data Source=" + BDPath + ";";
            
            
            try
            {
                SQLiteConnection connection = new SQLiteConnection(connectionstr);
                return connection;
            }
            catch
            {
            }
            return null;

        }
        private void FillCabeTypes(string cablename, RadDropDownList radDropDownList)
        {
            
            if (cablename != string.Empty)
            {
                string query;
                using (SQLiteConnection connection = OpenDB())
                {
                    SQLiteCommand command = new SQLiteCommand
                    { Connection = connection };
                    query = $"SELECT CrossSection FROM CableTypes WHERE CableType = '{cablename}'";
                    command.CommandText = query;
                    connection.Open();
                    radDropDownList.Items.Clear();

                    if (connection.State == ConnectionState.Open)
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                radDropDownList.Items.Add(reader[0].ToString());
                            }
                            reader.Close();
                        }
                    }
                    command.Dispose();
                    connection.Close();

                }
            }
            

        }
        private DataTable GetPresetCables(string building, string signaltype)
        {
            string query = string.Empty; 
            DataTable dataTable = new DataTable();
            switch (signaltype)
            {
                case "A":

                    query = $"SELECT CablePresets.CableA, CablePresets.CableACrossSection FROM CablePresets WHERE CablePresets.BuildingType = '{building}'";
                    break;
                case "D":
                    query = $"SELECT CablePresets.CableD, CablePresets.CableDCrossSection FROM CablePresets WHERE CablePresets.BuildingType = '{building}'";
                    break;
                case "P":
                case "PL":
                    query = $"SELECT CablePresets.CableP, CablePresets.CablePCrossSection FROM CablePresets WHERE CablePresets.BuildingType = '{building}'";
                    break;
                case "C":
                    query = $"SELECT CablePresets.CableC, CablePresets.CableCCrossSection FROM CablePresets WHERE CablePresets.BuildingType = '{building}'";
                    break;
            }
            using (SQLiteConnection connection = OpenDB())
            {
                connection.Open();
                if (connection.State == ConnectionState.Open)
                {
                    SQLiteCommand command = new SQLiteCommand { Connection = connection, CommandText = query };
                    
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        dataTable.Load(reader);
                        reader.Close();
                    }
                    command.Dispose();
                }
                connection.Close();
            }
            return dataTable;
        }
        private void SetCablePreset(DataTable dataTable, RadDropDownList cabtypelist, RadDropDownList cabcrosssectionlist)
        {
            //cabtypelist.Items.Clear();
            //cabcrosssectionlist.Items.Clear();
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {

                string cabtype = dataTable.Rows[i].ItemArray[0].ToString();
                string cabcrosssection = dataTable.Rows[i].ItemArray[1].ToString();
                //cabtypelist.Items.Add(cabtype);
                //cabcrosssectionlist.Items.Add(cabcrosssection);
                cabtypelist.Text = cabtype;
                cabcrosssectionlist.Text = cabcrosssection;
            }
        }

        private void radDropDownList9_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            
            string SelectedPreset = radDropDownList9.SelectedItem.Text;
            string query = $"SELECT CablePresets.CableD FROM CablePresets WHERE CablePresets.BuildingType = '{SelectedPreset}' " +
                $"UNION ALL " +
                $"SELECT CablePresets.CableA FROM CablePresets WHERE CablePresets.BuildingType = '{SelectedPreset}' " +
                $"UNION ALL " +
                $"SELECT CablePresets.CableC FROM CablePresets WHERE CablePresets.BuildingType = '{SelectedPreset}' " +
                $"UNION ALL " +
                $"SELECT CablePresets.CableP FROM CablePresets WHERE CablePresets.BuildingType = '{SelectedPreset}' ";
            SQLiteConnection connection = OpenDB();
            connection.Open();
            SQLiteCommand command = new SQLiteCommand { Connection = connection, CommandText = query };
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                DataTable dataTable = new DataTable();
                dataTable.Load(reader);
                reader.Close();
                List<string> cabs = (from row in dataTable.AsEnumerable()
                                    select row.ItemArray[0].ToString()).ToList();
                radDropDownList1.Text = cabs[0];
                radDropDownList3.Text = cabs[1];
                radDropDownList5.Text = cabs[2];
                radDropDownList7.Text = cabs[3];

            }
            connection.Close();
        }

        private void radDropDownList1_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {

        }
       private void radDropDownList3_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            
        }

        private void radDropDownList5_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            
        }

        private void radDropDownList7_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            
        }

        private void bunifuImageButton2_Click(object sender, EventArgs e)
        {

            Bitmap bmp = ReadBitmap(radDropDownList1.Text);
            CableImage cableImage = new CableImage
            {
                Bitmap = bmp
            };
            cableImage.ShowDialog();

        }
        
        
        private Bitmap ReadBitmap (string cabletype)
        {
            Bitmap bitmap = null;
            string imagepath = string.Empty;
            string query = $"SELECT CableTypes.Image FROM CableTypes WHERE CableTypes.CableType = '{cabletype}' LIMIT 1";
            using (SQLiteConnection connection = OpenDB())
            {
                connection.Open();
                if (connection.State == ConnectionState.Open)
                {
                    SQLiteCommand command = new SQLiteCommand { Connection = connection, CommandText = query };
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader[0].ToString() != string.Empty)
                            {
                                imagepath = reader[0].ToString();
                                break;
                            }
                        }
                        reader.Close();

                    }
                    command.Dispose();
                }
                connection.Close();
            }
            if (imagepath != string.Empty)
            {
                using (WebClient wc  = new WebClient())
                {
                    try
                    {
                        wc.DownloadData(imagepath);
                        byte[] bytes = wc.DownloadData(imagepath);
                        MemoryStream ms = new MemoryStream(bytes);
                        Image pic = Image.FromStream(ms);
                        bitmap = (Bitmap)pic;
                        return bitmap;

                    }
                    catch (WebException ex)
                    {
                        if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                        {
                            //error 404 page not found
                            bitmap = null;
                        }
                    }
                }
                
            }
            return bitmap;
        }

        private void bunifuImageButton3_Click(object sender, EventArgs e)
        {
            Bitmap bmp = ReadBitmap(radDropDownList3.Text);
            CableImage cableImage = new CableImage
            {
                Bitmap = bmp
            };
            cableImage.ShowDialog();
        }

        private void bunifuImageButton4_Click(object sender, EventArgs e)
        {
            Bitmap bmp = ReadBitmap(radDropDownList5.Text);
            CableImage cableImage = new CableImage
            {
                Bitmap = bmp
            };
            cableImage.ShowDialog();
        }

        private void bunifuImageButton5_Click(object sender, EventArgs e)
        {
            Bitmap bmp = ReadBitmap(radDropDownList7.Text);
            CableImage cableImage = new CableImage
            {
                Bitmap = bmp
            };
            cableImage.ShowDialog();
        }

        private void radDropDownList1_KeyPress(object sender, KeyPressEventArgs e)
        {
           
        }

        private void radDropDownList3_KeyPress(object sender, KeyPressEventArgs e)
        {
           
        }

        private void radDropDownList5_KeyPress(object sender, KeyPressEventArgs e)
        {
           
        }

        private void radDropDownList7_KeyPress(object sender, KeyPressEventArgs e)
        {
           
        }
        private bool CheckExist(string table, string column,  string columnval)
        {
            bool outval = false;
            string query = $"SELECT * FROM {table} WHERE {column} = '{columnval}'";
            SQLiteConnection connection = OpenDB();
            connection.Open();
            if (connection.State == ConnectionState.Open)
            {
                SQLiteCommand command = new SQLiteCommand
                {
                    Connection = connection,
                    CommandText = query
                };
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows) outval = true;
                    reader.Close();
                }
                command.Dispose();
                connection.Close();
                
            }
            
            return outval;
        }

        private void radTextBoxControl1_TextChanging(object sender, TextChangingEventArgs e)
        {
            string oldval = radTextBoxControl1.Text;
            string newwal = e.NewValue;
            string newname;
            if (string.IsNullOrEmpty(newwal))
            {
                newname = radTextBoxControl1.Text.Remove(radTextBoxControl1.Text.Trim().Length - 1);

            }
            else
            {
                newname = oldval + newwal;
                
            }
            newname = newname + radDropDownList10.Text;
            string checkname(string ckeckedname) => ckeckedname == radDropDownList10.Text ? string.Empty : ckeckedname;
            newname = checkname(newname);
            label3.Text = newname;
            
            
        }

        private void radDropDownList10_SelectedIndexChanging(object sender, Telerik.WinControls.UI.Data.PositionChangingCancelEventArgs e)
        {
            label3.Text = radTextBoxControl1.Text + radDropDownList10.Items[e.Position].Text;
            
        }

        private void radToggleSwitch1_ValueChanged(object sender, EventArgs e)
        {
            radCheckedDropDownList5.Visible = radToggleSwitch1.Value;
            if (radCheckedDropDownList5.Visible == false)
            {
                foreach (var radListDataItem in radCheckedDropDownList5.Items)
                {
                    var item = (RadCheckedListDataItem) radListDataItem;
                    item.Checked = false;
                };
            }
        }

        private void radButton2_Click(object sender, EventArgs e)
        {
            string building = radDropDownList9.Text;
            string CableD = radDropDownList1.Text;
            string CableA = radDropDownList3.Text;
            string CableC = radDropDownList5.Text;
            string CableP = radDropDownList3.Text;
            string query = $"UPDATE CablePresets SET CableD = '{CableD}', CableA = '{CableA}', CableC = '{CableC}', CableP = '{CableP}' " +
                $"WHERE BuildingType = '{building}'";
            try
            {
                SQLiteConnection connection = OpenDB();
                connection.Open();
                
                    SQLiteCommand command = new SQLiteCommand
                    {
                        Connection = connection,
                        CommandText = query
                    };
                    command.ExecuteNonQuery();
                connection.Close();
                
                    
            }
            catch
            {

            }
        }
    }
}
       
    
