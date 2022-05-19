using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Telerik.WinControls.UI;
using WinFormAnimation;

namespace AOVGEN
{
    public partial class CreatePannel : RadForm
    {
        private readonly MainForm mainForm;
        private readonly RadTreeNode buildnode;
        private readonly string author;
        public CreatePannel(MainForm refform, RadTreeNode refbuildnode, string author)
        {
            mainForm = refform;
            buildnode = refbuildnode;
            InitializeComponent();
            AnimationPannelForm();
            this.author = author;
        }

        private void bunifuImageButton1_Click(object sender, EventArgs e)
        {

            Close();
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
            if (Opacity == 0)
            {
                timer1.Stop();
                WriteToDB();
                Close();
            }
            //if (this.Location.X == 0) WriteToDB(); 
            //if (this.Opacity == 1) WriteToDB();

        }

        private void radButton1_Click(object sender, EventArgs e)
        {
            try
            {
                string pannelname = radTextBox1.Text;
                string category = radDropDownList3.Text;
                if (pannelname == "Имя шкафа") pannelname = string.Empty;
                if (category == "Категория") category = string.Empty;
                if (pannelname == string.Empty) throw new Exception();
                if (category == string.Empty) throw new Exception();
                pGUID = Guid.NewGuid().ToString();
                timer1.Start();
                //AnimationPannelFormAccept();
                AnimationClose();

            }
            catch
            {
                MessageBox.Show("не все поля заполнены");
            }



        }
        private void AnimationPannelForm()
        {
            new Animator2D(new Path2D(Location.X, 800, Location.Y, Location.Y, 200)
                .ContinueTo(776, Location.Y, 250))
                .Play(this, Animator2D.KnownProperties.Location);
        }
        private void AnimationPannelFormAccept()
        {
            new Animator2D(new Path2D(Location.X, Location.X + 50, Location.Y, Location.Y, 150)
                            .ContinueTo(0, Location.Y, 80))
                            .Play(this, Animator2D.KnownProperties.Location);
        }
        public string BUILDGUID { get; set; }
        public string DBFilePath { get; set; }
        public DataTable dataTable { get; set; }
        public RadTreeView RadTreeView { get; set; }

        public RadGridView RadGridView { get; set; }
        public RadPropertyGrid RadPropertyGrid { get; set; }
        private string pGUID { get; set; }

        private void radTextBox1_Click(object sender, EventArgs e)
        {
            if (radTextBox1.Text == "Имя шкафа" && radTextBox1.ForeColor == Color.Silver)
            {
                radTextBox1.Text = string.Empty;
                radTextBox1.ForeColor = Color.Black;
            }

        }

        private void radDropDownList1_MouseEnter(object sender, EventArgs e)
        {
           
        }

        private void radDropDownList3_MouseEnter(object sender, EventArgs e)
        {
            if (radDropDownList3.Text == "Категория" && radDropDownList3.ForeColor == Color.Silver)
            {
                radDropDownList3.Text = string.Empty;
                radDropDownList3.ForeColor = Color.Black;
            }
        }

        private void radDropDownList3_Leave(object sender, EventArgs e)
        {
            try
            {
                string b = radDropDownList3.Text;

                //int b = Convert.ToInt32(radDropDownList3.Text);
                if (b == "1" || b == "2" || b == "3")
                {
                    //radDropDownList3.Text = string.Empty;
                }
                else
                {
                    radDropDownList3.Text = string.Empty;
                }

            }
            catch
            {
                radDropDownList3.Text = string.Empty;
            }
        }

        private void radToggleSwitch2_ValueChanged(object sender, EventArgs e)
        {
            if (radToggleSwitch2.Value)
            {
                radDropDownList2.Enabled = true;
            }
            else
            {
                radDropDownList2.Text = "Протокол";
                radDropDownList2.Enabled = false;
            }
        }

        private void radDropDownList2_MouseEnter(object sender, EventArgs e)
        {
            if (radDropDownList2.Text == "Протокол") radDropDownList2.Text = string.Empty;
        }
        private void WriteToDB()
        {
            
            SQLiteConnection connection = OpenDB();

            //string queryString = "SELECT Project FROM Buildings  Pannel SET PannelName= @newpannelname WHERE GUID= @GUID";
            string prgGUID = string.Empty;
            connection.Open();
            if (connection.State == ConnectionState.Open)
            {
                try
                {
                    
                    string selectprjguid = $"SELECT Buildings.Project FROM Buildings WHERE GUID = '{BUILDGUID}'";
                    SQLiteCommand command = new SQLiteCommand(selectprjguid, connection);
                    var dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        prgGUID = dataReader[0].ToString();

                        if (prgGUID != string.Empty)
                        {
                            dataReader.Close();
                            command.Dispose();
                            break;
                        }
                    }
                    if (prgGUID != string.Empty)
                    {
                        string Author = author;
                        var NewPannelGUID = Guid.NewGuid().ToString();
                        string PannelName = radTextBox1.Text;
                        string Category = string.Empty;
                        switch (radDropDownList3.Text)
                        {
                            case "1":
                                Category = "one";
                                break;
                            case "2":
                                Category = "two";
                                break;
                            case "3":
                                Category = "three";
                                break;
                        }
                        string FireProtect = string.Empty;
                        switch (radToggleSwitch1.Value.ToString())
                        {
                            case "True":
                                FireProtect = "Yes";
                                break;
                            case "False":
                                FireProtect = "No";
                                break;
                        }
                        string Dispatching = string.Empty;
                        string Protocol = "None";
                        switch (radToggleSwitch2.Value.ToString())
                        {
                            case "True":
                                Dispatching = "Yes";
                                Protocol = radDropDownList2.Text;
                                break;
                            case "False":
                                Dispatching = "No";
                                break;
                        }

                        string Place = BUILDGUID;
                        var Modyfied = DateTime.Now.ToString("dd-MM-yyyy");
                        string Version = "1";

                        string InsertQuery = $"INSERT INTO Pannel ([GUID], PannelName, [Project], Power, Voltage, Category, FireProtect, Dispatching, Protocol, [Place], Modyfied, Version, Author) VALUES ('{NewPannelGUID}','{PannelName}','{prgGUID}','0','{Pannel._Voltage.AC220}','{Category}','{FireProtect}','{Dispatching}','{Protocol}','{Place}','{Modyfied}','{Version}','{Author}')";
                        SQLiteCommand command1 = new SQLiteCommand(InsertQuery, connection);
                        command1.ExecuteNonQuery();
                        command1.Dispose();
                        connection.Close();
                        //timer1.Stop();

                        DataTable dataSource = dataTable;
                        DataRow drToAdd = dataSource.NewRow();
                        drToAdd["ID"] = dataSource.Rows.Count + 1;
                        //drToAdd["ParentID"] = DBNull.Value;
                        drToAdd["GUID"] = NewPannelGUID;
                        drToAdd["PannelName"] = PannelName;
                        //drToAdd["Project"] = prgGUID;
                        drToAdd["Power"] ="0";
                        drToAdd["Voltage"] = Pannel._Voltage.AC220.ToString();
                        drToAdd["Category"] = Category;
                        drToAdd["FireProtect"] = FireProtect;
                        drToAdd["Dispatching"] = Dispatching;
                        drToAdd["Protocol"] = Protocol;
                        //drToAdd["Place"] =Place;
                        drToAdd["Modyfied"] = Modyfied;
                        drToAdd["Version"] = Version;
                        drToAdd["Author"] = Author;
                        dataSource.Rows.Add(drToAdd);
                        dataSource.AcceptChanges();
                        
                        //var gridview = this.RadGridView;
                        Pannel pannel= CreatePannelClass(NewPannelGUID);
                        
                        foreach (var row in RadGridView.Rows)
                        {
                            
                            if (row.Cells[1].Value.ToString() == NewPannelGUID)
                            {
                                
                                row.Tag = pannel;
                                RadPropertyGrid.SelectedObject =pannel;
                                break;
                            }
                        }
                        UpdateDataGrid();
                        mainForm.UpdateBuildNode(buildnode);

                        //this.RadGridView.Rows[this.RadGridView.Rows.Count].IsSelected = true;
                        //this.RadPropertyGrid.SelectedObject = pannel;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }


            }

        }
        private SQLiteConnection OpenDB()
        {
            string BDPath = DBFilePath;
            string connectionstr = @"Data Source=" + BDPath + ";";
            try
            {
                SQLiteConnection connection = new SQLiteConnection (connectionstr);
                return connection;
            }
            catch { }
            return null;

        }
        private Pannel CreatePannelClass(string pannelguid)
        {
            Pannel pannel = null;
            string GUID = string.Empty;
            string pannelname = string.Empty;
            string power = string.Empty;
            string voltage = string.Empty;
            string category = string.Empty;
            string fireprotect = string.Empty;
            string dispatching = string.Empty;
            string protocol = string.Empty;
            int version = 0;
            SQLiteConnection connection = OpenDB();
            string tableselect = $"SELECT Pannel.GUID, Pannel.PannelName, Pannel.Power, Pannel.Voltage, Pannel.Category, Pannel.FireProtect, Pannel.Dispatching, Pannel.Protocol, Pannel.Version FROM Pannel WHERE GUID = '{pannelguid}'";
            connection.Open();
            SQLiteCommand command = new SQLiteCommand(tableselect, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                GUID = reader[0].ToString();
                pannelname = reader[1].ToString();
                power = reader[2].ToString();
                voltage = reader[3].ToString();
                category = reader[4].ToString();
                fireprotect = reader[5].ToString();
                dispatching = reader[6].ToString();
                protocol = reader[7].ToString();
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

                PropertyInfo pinfoVoltage = pannel.GetType().GetProperty("Voltage");
                pinfoVoltage?.SetValue(pannel, Enum.Parse(pinfoVoltage.PropertyType, voltage));
                PropertyInfo pinfoCategory = pannel.GetType().GetProperty("Category");
                pinfoCategory?.SetValue(pannel, Enum.Parse(pinfoCategory.PropertyType, category));
                PropertyInfo pinfoFireProtect = pannel.GetType().GetProperty("FireProtect");
                pinfoFireProtect?.SetValue(pannel, Enum.Parse(pinfoFireProtect.PropertyType, fireprotect));
                PropertyInfo pinfoDispatching = pannel.GetType().GetProperty("Dispatching");
                pinfoDispatching?.SetValue(pannel, Enum.Parse(pinfoFireProtect.PropertyType, dispatching));
                //PropertyInfo pinfoProtocol = pannel.GetType().GetProperty("Protocol");
                //pinfoProtocol.SetValue(pannel, Enum.Parse(pinfoFireProtect.PropertyType, protocol));
                
                Enum.TryParse(protocol, out Pannel._Protocol pannelprotocol);
                pannel.Protocol = pannelprotocol;

                

                pannel.SetGUID(GUID);
                
                        
                    
            }
            catch { }
            return pannel;
           

        }
        private void UpdateDataGrid()
        {

            foreach (var Row in RadGridView.Rows)
            {
                try
                {
                    if (Row.Tag == null)
                    {

                        string GUID = Row.Cells[1].Value.ToString();
                        string pannelname = Row.Cells[2].Value.ToString();
                        string power = Row.Cells[6].Value.ToString();
                        string voltage = Row.Cells[7].Value.ToString();
                        string category = Row.Cells[8].Value.ToString();
                        string fireprotect = Row.Cells[9].Value.ToString();
                        string dispatching = Row.Cells[10].Value.ToString();
                        string protocol = Row.Cells[11].Value.ToString();


                        Pannel pannel = new Pannel
                        {
                            PannelName = pannelname,
                            Power = power,
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
                            case "Bacnet_IP":
                                pannel.Protocol = Pannel._Protocol.Bacnet_IP;

                                break;
                            case "LON":
                                pannel.Protocol = Pannel._Protocol.LON;

                                break;
                            case "None":

                                pannel.Protocol = Pannel._Protocol.None;
                                break;
                        }
                        pannel.SetGUID(GUID);
                        Row.Tag = pannel;

                    }
                }
                catch
                {

                }
                //MessageBox.Show(Row.Cells[1].Value.ToString());
            }
            
        }

    }
}