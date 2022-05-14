using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WinFormAnimation;
using System.Data.SQLite;

namespace AOVGEN
{
    public partial class CreateAnalogue : Telerik.WinControls.UI.RadForm
    {
        readonly SQLiteConnection connection;
        public CreateAnalogue(SQLiteConnection Connection)
        {
            connection = Connection;
            InitializeComponent();
        }
        public string PannelGUIDForCopy { get; set; }
        public string DBFilePath { get; set; }
        public Telerik.WinControls.UI.RadGridView RadGridView { get; set; }
        public Telerik.WinControls.UI.RadPropertyGrid RadPropertyGrid { get; set; }
        
        private void radTextBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            var textbox = sender as Telerik.WinControls.UI.RadTextBox;
            _ = textbox.Text;
            if (!char.IsDigit(e.KeyChar)) e.Handled = true;
            if (e.KeyChar == (char)8) e.Handled = false;
            
        }

        
        private void bunifuImageButton1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void radTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (radTextBox1.Text == "Имя шкафа")
            {
                radTextBox1.Text = string.Empty;
                radTextBox1.ForeColor = Color.Black;
            }
            UpdateExampleName();
        }

        private void radTextBox2_KeyUp(object sender, KeyEventArgs e)
        {
            UpdateExampleName();
        }

        private void radTextBox3_KeyUp(object sender, KeyEventArgs e)
        {
            UpdateExampleName();
        }

        private void radTextBox4_KeyUp(object sender, KeyEventArgs e)
        {
            UpdateExampleName();
        }

        private void radTextBox5_KeyUp(object sender, KeyEventArgs e)
        {
            UpdateExampleName();
        }

        private void radButton1_Click(object sender, EventArgs e)
        {
            AnimationClose();

        }
        private void AnimationClose()
        {
            timer1.Start();


            Animator animator = new Animator
            {
                Paths = new Path(1, 0, 400, 100).ToArray()
            };
            animator.Play(this, Animator.KnownProperties.Opacity);

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.Opacity == 0)
            {
                timer1.Stop();
                WriteToDB();
                Close();
            }
        }
        private void WriteToDB()
        {

            
           

            
            //нужно реадизовать цикл!!!!
            //connection.Open();
            
            
            if (connection.State == ConnectionState.Open)
            {
                try
                {
                    int.TryParse(radTextBox4.Text, out int imax);
                    int.TryParse(radTextBox6.Text, out int startvalue);
                    int.TryParse(radTextBox5.Text, out int step);
                    string prefix = radTextBox2.Text;
                    string name = radTextBox1.Text;
                    string suffix = radTextBox3.Text;
                    string pannelname;
                    
                    DataTable dataSource = this.RadGridView.DataSource as DataTable;
                    SQLiteCommand command = new SQLiteCommand
                    {
                        Connection = connection
                    };

                    for (int i = 1; i <= imax; i++)
                    {

                        var guid = Guid.NewGuid().ToString();
                        pannelname = prefix + name + startvalue.ToString() + suffix;
                        startvalue += step;
                        string copy = $"INSERT INTO Pannel ([GUID], PannelName, Project, Power, Voltage, Category, FireProtect, Dispatching, Protocol, Place, Modyfied, Version, Author) " +
                            $"SELECT '{guid}','{pannelname}', Project, Power, Voltage, Category, FireProtect, Dispatching, Protocol, Place, Modyfied, Version, Author " +
                            $"FROM Pannel " +
                            $"WHERE GUID ='{this.PannelGUIDForCopy}'";
                        string NulledPower = $"UPDATE Pannel " +
                            $"SET Power = '0' " +
                            $"WHERE GUID = '{guid}'";
                        command.CommandText = copy;
                        command.ExecuteNonQuery();
                        command.CommandText = NulledPower;
                        command.ExecuteNonQuery();
                        
                        string tableselect = $"SELECT Pannel.ID, Pannel.GUID, Pannel.PannelName, Pannel.Modyfied, Pannel.Author, Pannel.Version, Pannel.Power, Pannel.Voltage, Pannel.Category, Pannel.FireProtect, Pannel.Dispatching, Pannel.Protocol FROM Pannel WHERE GUID = '{guid}'";
                        command.CommandText = tableselect;
                        SQLiteDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            DataRow drToAdd = dataSource.NewRow();
                            drToAdd["ID"] = dataSource.Rows.Count + 1;
                            pannelname = reader[2].ToString();
                            string power = reader[6].ToString();
                            string voltage = reader[7].ToString();
                            string category = reader[8].ToString();
                            string fireprotect = reader[9].ToString();
                            string dispatching = reader[10].ToString();
                            string protocol = reader[11].ToString();
                            int version = Convert.ToInt32(reader[5].ToString());
                            drToAdd["GUID"] = guid;
                            drToAdd["PannelName"] = pannelname;
                            drToAdd["Power"] = power;
                            drToAdd["Voltage"] = voltage;
                            drToAdd["Category"] = category;
                            drToAdd["FireProtect"] = fireprotect;
                            drToAdd["Dispatching"] = dispatching;
                            drToAdd["Protocol"] = protocol;
                            drToAdd["Modyfied"] = reader[3].ToString();
                            drToAdd["Version"] = version;
                            drToAdd["Author"] = reader[4].ToString();
                            dataSource.Rows.Add(drToAdd);
                            //dataSource.AcceptChanges();
                            Pannel pannel = new Pannel
                            {
                                PannelName = pannelname,
                                Power = power,
                                Version = version
                                
                            };
                            switch (voltage)
                            {
                                case "AC380":
                                    pannel.Voltage = Pannel._Voltage.AC380;
                                    break;
                                case "AC220":
                                    pannel.Voltage = Pannel._Voltage.AC220;
                                    break;
                            }
                            switch (category)
                            {
                                case "one":
                                    pannel.Category = Pannel._Category.one;
                                    break;
                                case "two":
                                    pannel.Category = Pannel._Category.two;
                                    break;
                                case "three":
                                    pannel.Category = Pannel._Category.three;
                                    break;
                            }
                            switch (fireprotect)
                            {
                                case "Yes":
                                    pannel.FireProtect = Pannel._FireProtect.Yes;
                                    break;
                                case "No":
                                    pannel.FireProtect = Pannel._FireProtect.No;
                                    break;
                            }
                            switch (dispatching)
                            {
                                case "Yes":
                                    pannel.Dispatching = Pannel._Dispatching.Yes;
                                    break;
                                case "No":
                                    pannel.Dispatching = Pannel._Dispatching.No;
                                    pannel.Protocol = Pannel._Protocol.None;
                                    break;
                            }
                            switch (protocol)
                            {
                                case "ModBus_RTU":
                                    pannel.Protocol = Pannel._Protocol.ModBus_RTU;
                                    break;
                                case "ModBus_TCP":
                                    pannel.Protocol = Pannel._Protocol.ModBus_TCP;
                                    break;
                                case "Bacnet IP":
                                    pannel.Protocol = Pannel._Protocol.Bacnet_IP;
                                    break;
                                case "LON":
                                    pannel.Protocol = Pannel._Protocol.LON;
                                    break;
                                case "None":
                                    pannel.Protocol = Pannel._Protocol.None;
                                    break;
                                   
                            }
                            pannel.SetGUID(guid);
                            this.RadGridView.SelectedRows[0].Tag = pannel;
                            if (i == imax) this.RadPropertyGrid.SelectedObject = RadGridView.SelectedRows[0].Tag;
                            

                        }
                        reader.Close();

                    }
                    command.Dispose();
                    

                }
                catch (Exception ex)
                {
                    
                    MessageBox.Show(ex.Message + ex.StackTrace);
                }
                finally
                {
                    
                    
                }

            }

        }
        
        private Pannel CreatePannelClass(string pannelguid)
        {
            Pannel pannel = null;
            string GUID = string.Empty;
            string pannelname = string.Empty;
            string power = string.Empty;
            int version = 0;
            //OleDbConnection connection = OpenDB();
            string tableselect = $"SELECT Pannel.GUID, Pannel.PannelName, Pannel.Power, Pannel.Voltage, Pannel.Category, Pannel.FireProtect, Pannel.Dispatching, Pannel.Protocol, Pannel.Version FROM Pannel WHERE GUID = '{pannelguid}'";
            connection.Open();
            SQLiteCommand command = new SQLiteCommand(tableselect, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                GUID = reader[0].ToString();
                pannelname = reader[1].ToString();
                power = reader[2].ToString();
                _ = reader[3].ToString();
                _ = reader[4].ToString();
                _ = reader[5].ToString();
                _ = reader[6].ToString();
                _ = reader[7].ToString();
                version = Convert.ToInt32(reader[8].ToString());
            }
            reader.Close();
            command.Dispose();
            connection.Close();
            try
            {

                pannel = new Pannel
                {
                    PannelName = pannelname,
                    Power = power,
                    Version = version
                };
                
                pannel.SetGUID(GUID);



            }
            catch { }
            return pannel;


        }        
        private void UpdateExampleName()
        {
            string prefix = radTextBox2.Text;
            if (prefix == "Префикс") prefix = string.Empty;
            string name = radTextBox1.Text;
            string suffix = radTextBox3.Text;
            if (suffix == "Суффикс") suffix = string.Empty;
            string summary = radTextBox6.Text;
            if (summary == "Количество") summary = string.Empty;
            this.radLabel7.Text = prefix + name + summary + suffix;
        }

        private void radTextBox6_KeyUp(object sender, KeyEventArgs e)
        {
            UpdateExampleName();
        }
    }
}
