using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Telerik.WinControls.UI;
using WinFormAnimation;

namespace AOVGEN
{
    public partial class Editor : Form
    {
        #region Global Variables
        internal VentSystem VentSystem { get; set; }
        public string DBFilePath { get; set; }
        public RadTreeView Ventree { get; set; }
        public RadTreeView Projecttree { get; set; }
        public RadTreeNodeCollection Joinedsystems { get; set; }
        internal string ProjectGuid { get; set; }
        internal Building Building { get; set; }
        internal Project Project { get; set; }
        internal bool OpenForEdit { get; set; }
        internal RadTreeNode SelectedNode { get; set; }
        internal SQLiteConnection Connection { get; set; }
        //internal string oldpower = string.Empty;
        private readonly MainForm mainForm;
        #endregion
        #region Custom Exceptions
        [Serializable]
        protected class VentSystemEmptyException : Exception
        {
            public VentSystemEmptyException()
            { }

            public VentSystemEmptyException(string message)
                : base(message)
            { }

            public VentSystemEmptyException(string message, Exception innerException)
                : base(message, innerException)
            { }
        }
        #endregion
        #region EditorForm Constructors
        public Editor()
        {
            InitializeComponent();
            
        }
        public Editor(MainForm refform)
        {
            mainForm = refform;
            InitializeComponent();
        }
        #endregion
        private void Editor_Load(object sender, EventArgs e)
        {
            if (VentSystem == null)
            {
                VentSystem myventSystem = new VentSystem();
                VentSystem = myventSystem;
                radAutoCompleteBox1.Text = "VentsystemName;";
            }
            else
            {
                radAutoCompleteBox1.Text = VentSystem.SystemName;
                radAutoCompleteBox1.Enabled = false;
                //отрисовка картинок у существующей системы
                foreach (var item in VentSystem
                    .Cast<object>()
                    .Where(item => item != null))
                {
                    switch (item.GetType().Name)
                    {
                        case nameof(SupplyVent):
                            radButton4.Tag = VentSystem._SupplyVent;
                            pictureBox6.Tag = VentSystem._SupplyVent;
                            pictureBox6.Visible = true;
                            break;
                        case nameof(ExtVent):
                            radButton10.Tag = VentSystem._ExtVent;
                            pictureBox11.Tag = VentSystem._ExtVent;
                            pictureBox11.Visible = true;
                            break;
                        case nameof(SupplyFiltr):
                            radButton1.Tag = VentSystem._SupplyFiltr;
                            pictureBox1.Tag = VentSystem._SupplyFiltr;
                            pictureBox1.Visible = true;
                            break;
                        case nameof(ExtFiltr):
                            radButton11.Tag = VentSystem._ExtFiltr;
                            pictureBox10.Tag = VentSystem._ExtFiltr;
                            pictureBox10.Visible = true;
                            break;
                        case nameof(SupplyDamper):
                            radButton2.Tag = VentSystem._SupplyDamper;
                            pictureBox4.Tag = VentSystem._SupplyDamper;
                            pictureBox4.Visible = true;
                            break;
                        case nameof(ExtDamper):
                            radButton9.Tag = VentSystem._ExtDamper;
                            pictureBox12.Tag = VentSystem._ExtDamper;
                            pictureBox12.Visible = true;
                            break;
                        case nameof(WaterHeater):
                            radButton5.Tag = VentSystem._WaterHeater;
                            pictureBox7.Tag = VentSystem._WaterHeater;
                            pictureBox7.Visible = true;
                            break;
                        case nameof(ElectroHeater):
                            radButton6.Tag = VentSystem._ElectroHeater;
                            pictureBox8.Tag = VentSystem._ElectroHeater;
                            pictureBox8.Visible = true;
                            break;
                        case nameof(Froster):
                            radButton7.Tag = VentSystem._Froster;
                            pictureBox5.Tag = VentSystem._Froster;
                            pictureBox5.Visible = true;
                            break;
                        case nameof(Humidifier):
                            radButton8.Tag = VentSystem._Humidifier;
                            pictureBox9.Tag = VentSystem._Humidifier;
                            pictureBox9.Visible = true;
                            break;
                        case nameof(Recuperator):
                            radButton3.Tag = VentSystem._Recuperator;
                            pictureBox3.Tag = VentSystem._Recuperator;
                            pictureBox3.Image = new Bitmap(VentSystem._Recuperator.Imagepath);
                            pictureBox3.Visible = true;
                            break;
                        case nameof(SupplyTemp):
                            radButton13.Tag = VentSystem._SupplyTemp;
                            pictureBox13.Tag = VentSystem._SupplyTemp;
                            pictureBox13.Visible = true;

                            break;
                        case nameof(ExhaustTemp):
                            radButton14.Tag = VentSystem._ExhaustTemp;
                            pictureBox14.Tag = VentSystem._ExhaustTemp;
                            pictureBox14.Visible = true;
                            break;
                        case nameof(IndoorTemp):
                            radButton16.Tag = VentSystem._IndoorTemp;
                            pictureBox16.Tag = VentSystem._IndoorTemp;
                            pictureBox16.Visible = true;
                            break;
                        case nameof(OutdoorTemp):
                            radButton15.Tag = VentSystem._OutdoorTemp;
                            pictureBox15.Tag = VentSystem._OutdoorTemp;
                            pictureBox15.Visible = true;
                            break;
                    }
                }
            }

            Building = Projecttree.SelectedNode.Tag as Building;
            Project = Projecttree.SelectedNode.Parent.Tag as Project;

        }
        #region PropertyGridEventHendler
        
        private void RadPropertyGrid1_PropertyValueChanged(object sender, PropertyGridItemValueChangedEventArgs e)
        {
            
            try
            {
                switch (radPropertyGrid1.SelectedObject.GetType().Name)
                {
                    case "Froster":
                        if (radPropertyGrid1.SelectedObject is Froster froster)
                        {
                            var frosterType = froster._FrosterType;

                            pictureBox5.Image = new Bitmap(Froster.makeimagepath(frosterType));

                            radPropertyGrid1.PropertyGridElement.PropertyTableElement.BeginUpdate();
                            radPropertyGrid1.Items["Stairs"].Visible = frosterType == Froster.FrosterType.Freon;
                            radPropertyGrid1.Items["KKBControlType"].Visible = frosterType == Froster.FrosterType.Freon;
                            radPropertyGrid1.Items["valveType"].Visible = frosterType == Froster.FrosterType.Water;
                            radPropertyGrid1.PropertyGridElement.PropertyTableElement.EndUpdate();
                        }
                        break;

                    case "Recuperator":
                        if (radPropertyGrid1.SelectedObject is Recuperator recuperator) pictureBox3.Image = new Bitmap(recuperator.Imagepath);
                        break;
                    case "ExtVent":
                        
                        
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
            }
            
        }
        #endregion
        #region ToolStripMenuItemEventHendler
        private void Minus_Click(object sender, EventArgs e)
        {

            ToolStripMenuItem t = (ToolStripMenuItem)sender;
            ContextMenuStrip s = (ContextMenuStrip)t.Owner;
            switch (s.SourceControl.Name)
            {
                case "radButton2":
                    VentSystem._SupplyDamper = null;
                    pictureBox4.Visible = false;
                    pictureBox4.Tag = null;
                    radPropertyGrid1.SelectedObject = null;
                    break;
                case "radButton1":
                    VentSystem._SupplyFiltr = null;
                    pictureBox1.Visible = false;
                    pictureBox1.Tag = null;
                    radPropertyGrid1.SelectedObject = null;
                    break;
                case "radButton3":
                    VentSystem._Recuperator = null;
                    pictureBox3.Visible = false;
                    pictureBox3.Tag = null;
                    radPropertyGrid1.SelectedObject = null;
                    break;
                case "radButton4":
                    VentSystem._SupplyVent = null;
                    pictureBox6.Visible = false;
                    pictureBox6.Tag = null;
                    radPropertyGrid1.SelectedObject = null;
                    break;
                case "radButton5":
                    VentSystem._WaterHeater = null;
                    pictureBox7.Visible = false;
                    pictureBox7.Tag = null;
                    radPropertyGrid1.SelectedObject = null;
                    break;
                case "radButton6":
                    VentSystem._ElectroHeater = null;
                    pictureBox8.Visible = false;
                    pictureBox8.Tag = null;
                    radPropertyGrid1.SelectedObject = null;
                    break;
                case "radButton7":
                    VentSystem._Froster = null;
                    pictureBox5.Visible = false;
                    pictureBox5.Tag = null;
                    radPropertyGrid1.SelectedObject = null;
                    break;
                case "radButton8":
                    VentSystem._Humidifier = null;
                    pictureBox9.Visible = false;
                    pictureBox9.Tag = null;
                    radPropertyGrid1.SelectedObject = null;
                    break;
                case "radButton11":
                    VentSystem._ExtFiltr = null;
                    pictureBox10.Visible = false;
                    pictureBox10.Tag = null;
                    radPropertyGrid1.SelectedObject = null;
                    break;
                case "radButton10":
                    VentSystem._ExtVent = null;
                    pictureBox11.Visible = false;
                    pictureBox11.Tag = null;
                    radPropertyGrid1.SelectedObject = null;
                    break;
                case "radButton9":
                    VentSystem._ExtDamper = null;
                    pictureBox12.Visible = false;
                    pictureBox12.Tag = null;
                    radPropertyGrid1.SelectedObject = null;
                    break;
                case "radButton13":
                    VentSystem._ExhaustTemp = null;
                    pictureBox13.Visible = false;
                    pictureBox13.Tag = null;
                    radPropertyGrid1.SelectedObject = null;
                    break;
                case "radButton14":
                    VentSystem._SupplyTemp = null;
                    pictureBox14.Visible = false;
                    pictureBox14.Tag = null;
                    radPropertyGrid1.SelectedObject = null;
                    break;
                case "radButton15":
                    VentSystem._OutdoorTemp = null;
                    pictureBox15.Visible = false;
                    pictureBox15.Tag = null;
                    radPropertyGrid1.SelectedObject = null;
                    break;
                case "radButton16":
                    VentSystem._IndoorTemp = null;
                    pictureBox16.Visible = false;
                    pictureBox16.Tag = null;
                    radPropertyGrid1.SelectedObject = null;
                    break;

            }
        }
        private void Plus_Click(object sender, EventArgs e) //это обработчик события контекстного меню нажатия кнопки
        {
            ToolStripMenuItem t = (ToolStripMenuItem)sender; //тут я получаю само контекстное меню
            ContextMenuStrip s = (ContextMenuStrip)t.Owner; //а вот тут я получаю кто послал это меню, то есть саму кнопку
            switch (s.SourceControl.Name) //дальше я начиню смотреть на имя этой кнопки
            {
                case nameof(radButton2):
                    SupplyDamper supplyDamper = new SupplyDamper(); //вот тут я создаю экземпляр класса заслонки
                    pictureBox4.Visible = true; //включаю картинку
                    VentSystem._SupplyDamper = supplyDamper; //в вент.систему в приточную заслонку кладу саму заслонку
                    pictureBox4.Tag = supplyDamper; //и вот это место, я еще в tag картинки кладу заслонку
                    radPropertyGrid1.SelectedObject = supplyDamper; //ну и контрол свойств назначаю заслонку (чтобы менять свойства у заслонки)
                    break;
                case nameof(radButton1):
                    SupplyFiltr supplyFiltr = new SupplyFiltr();
                    pictureBox1.Visible = true;
                    VentSystem._SupplyFiltr = supplyFiltr;
                    pictureBox1.Tag = supplyFiltr;
                    radPropertyGrid1.SelectedObject = supplyFiltr;
                    break;
                case nameof(radButton3):
                    Recuperator recuperator = new Recuperator
                    {
                        //_RecuperatorType = Recuperator.RecuperatorType.RotorNoControl
                    };
                    pictureBox3.Visible = true;
                    //pictureBox3.Image = new Bitmap(Recuperator.makeimagepath(Recuperator.RecuperatorType.Recirculation).ToString());
                    recuperator._RecuperatorType = Recuperator.RecuperatorType.LaminatedBypass;
                    pictureBox3.Image = new Bitmap(recuperator.Imagepath);
                    VentSystem._Recuperator = recuperator;
                    pictureBox3.Tag = recuperator;
                    radPropertyGrid1.SelectedObject = recuperator;
                    break;
                case nameof(radButton4):
                    SupplyVent supplyVent = new SupplyVent();
                    pictureBox6.Visible = true;
                    pictureBox6.Tag = supplyVent;
                    VentSystem._SupplyVent = supplyVent;
                    radPropertyGrid1.SelectedObject = supplyVent;
                    break;
                case nameof(radButton5):
                    WaterHeater waterHeater = new WaterHeater();
                    pictureBox7.Visible = true;
                    pictureBox7.Tag = waterHeater;
                    VentSystem._WaterHeater = waterHeater;
                    radPropertyGrid1.SelectedObject = waterHeater;
                    break;
                case nameof(radButton6):
                    ElectroHeater electroHeater = new ElectroHeater();
                    pictureBox8.Visible = true;
                    pictureBox8.Tag = electroHeater;
                    VentSystem._ElectroHeater = electroHeater;
                    radPropertyGrid1.SelectedObject = electroHeater;
                    break;
                case nameof(radButton7):
                    Froster froster = new Froster(false);
                    pictureBox5.Visible = true;
                    pictureBox5.Tag = froster;
                    VentSystem._Froster = froster;
                    radPropertyGrid1.SelectedObject = froster;
                    radPropertyGrid1.Items["valveType"].Visible = false;
                    break;
                case nameof(radButton8):
                    Humidifier humidifier = new Humidifier
                    {

                        HumType = Humidifier._HumType.Steam
                    };
                    pictureBox9.Visible = true;
                    pictureBox9.Tag = humidifier;
                    VentSystem._Humidifier = humidifier;
                    radPropertyGrid1.SelectedObject = humidifier;
                    break;
                case nameof(radButton11):
                    ExtFiltr extFiltr = new ExtFiltr();
                    pictureBox10.Visible = true;
                    pictureBox10.Tag = extFiltr;
                    VentSystem._ExtFiltr = extFiltr;
                    radPropertyGrid1.SelectedObject = extFiltr;
                    break;
                case nameof(radButton10):
                    ExtVent extVent = new ExtVent();
                    pictureBox11.Visible = true;
                    pictureBox11.Tag = extVent;
                    VentSystem._ExtVent = extVent;
                    radPropertyGrid1.SelectedObject = extVent;
                    break;
                case nameof(radButton9):
                    ExtDamper extDamper = new ExtDamper();
                    pictureBox12.Visible = true;
                    pictureBox12.Tag = extDamper;
                    VentSystem._ExtDamper = extDamper;
                    radPropertyGrid1.SelectedObject = extDamper;
                    break;
                case nameof(radButton13):
                    
                    SupplyTemp supplyTemp = new SupplyTemp
                    {
                       _SensorType = Sensor.SensorType.Analogue
                    };
                    pictureBox13.Visible = true;
                    pictureBox13.Tag = supplyTemp;
                    VentSystem._SupplyTemp = supplyTemp;
                    radPropertyGrid1.SelectedObject = supplyTemp;
                    break;
                case nameof(radButton14):
                    ExhaustTemp exhaustTemp = new ExhaustTemp
                    {
                        _SensorType = Sensor.SensorType.Analogue
                    };
                    pictureBox14.Visible = true;
                    pictureBox14.Tag = exhaustTemp;
                    VentSystem._ExhaustTemp = exhaustTemp;
                    radPropertyGrid1.SelectedObject = exhaustTemp;
                    break;
                case nameof(radButton15):
                    OutdoorTemp outdoorTemp = new OutdoorTemp
                    {
                        _SensorType = Sensor.SensorType.Analogue
                    };
                    pictureBox15.Visible = true;
                    pictureBox15.Tag = outdoorTemp;
                    VentSystem._OutdoorTemp = outdoorTemp;
                    radPropertyGrid1.SelectedObject = outdoorTemp;
                    break;
                case nameof(radButton16):
                    IndoorTemp indoorTemp = new IndoorTemp
                    {
                        _SensorType = Sensor.SensorType.Analogue
                    };
                    pictureBox16.Visible = true;
                    pictureBox16.Tag = indoorTemp;
                    VentSystem._IndoorTemp = indoorTemp;
                    radPropertyGrid1.SelectedObject = indoorTemp;
                    break;}
        }
        #endregion
        #region ButtonClicked

        private void MouseClickReaction(MouseEventArgs e, RadButton button, object SelectedObject)
        {
            switch (e.Button)
            {

                case MouseButtons.Left:
                    // Left click
                    radPropertyGrid1.SelectedObject = SelectedObject;
                    break;

                case MouseButtons.Right:
                    // Right click
                    CreateContexMenu(button, e);
                    break;
            }
        }
        private void RadButton1_MouseClick(object sender, MouseEventArgs e)
        {
            MouseClickReaction(e, (RadButton)sender, VentSystem._SupplyFiltr);
        }
        private void RadButton2_MouseClick(object sender, MouseEventArgs e) //обработчик события клика по кнопке
        {
            MouseClickReaction(e, (RadButton)sender, VentSystem._SupplyDamper);
        }
        private void RadButton3_MouseClick(object sender, MouseEventArgs e)
        {
            MouseClickReaction(e, (RadButton)sender, VentSystem._Recuperator);
        }
        private void RadButton4_MouseClick(object sender, MouseEventArgs e)
        {
            MouseClickReaction(e, (RadButton)sender, VentSystem._SupplyVent);
           
        }
        private void RadButton5_MouseClick(object sender, MouseEventArgs e)
        {
            MouseClickReaction(e, (RadButton)sender, VentSystem._WaterHeater);
            
        }
        private void RadButton6_MouseClick(object sender, MouseEventArgs e)
        {
            MouseClickReaction(e, (RadButton)sender, VentSystem._ElectroHeater);
        }
        private void RadButton7_MouseClick(object sender, MouseEventArgs e)
        {
            MouseClickReaction(e, (RadButton)sender, VentSystem._Froster);
            
        }
        private void RadButton8_MouseClick(object sender, MouseEventArgs e)
        {
            MouseClickReaction(e, (RadButton)sender, VentSystem._Humidifier);
            
        }
        private void RadButton11_MouseClick(object sender, MouseEventArgs e)
        {
            MouseClickReaction(e, (RadButton)sender, VentSystem._ExtFiltr);
          
        }
        private void RadButton10_MouseClick(object sender, MouseEventArgs e)
        {
            MouseClickReaction(e, (RadButton)sender, VentSystem._ExtVent);
           
        }
        private void RadButton9_MouseClick(object sender, MouseEventArgs e)
        {
            MouseClickReaction(e, (RadButton)sender, VentSystem._ExtDamper);
            
        }
        private void RadButton13_MouseClick(object sender, MouseEventArgs e)
        {
            MouseClickReaction(e, (RadButton)sender, VentSystem._SupplyTemp);
           
        }
        private void RadButton14_MouseClick(object sender, MouseEventArgs e)
        {
            MouseClickReaction(e, (RadButton)sender, VentSystem._ExhaustTemp);
            
        }
        private void RadButton15_MouseClick(object sender, MouseEventArgs e)
        {
            MouseClickReaction(e, (RadButton)sender, VentSystem._OutdoorTemp);
            
        }
        private void RadButton16_MouseClick(object sender, MouseEventArgs e)
        {
            MouseClickReaction(e, (RadButton)sender, VentSystem._IndoorTemp);
        }
        void CreateContexMenu(RadButton button, MouseEventArgs e )
        {
            var cm = new ContextMenuStrip();
            ToolStripItem plus = cm.Items.Add("Создать новый");
            ToolStripItem minus = cm.Items.Add("Удалить");            
            plus.Click += Plus_Click;
            minus.Click += Minus_Click;
            cm.Show(button, new Point(e.X, e.Y));
        }
        private void BunifuImageButton1_Click_1(object sender, EventArgs e)
        {
            AnimationClose();
        }
        private void RadButton12_Click(object sender, EventArgs e)
        {
            try
            {
                int cnt = 0;
                foreach (object item in VentSystem)
                {
                    if (item != null) cnt += 1;
                }
                if (cnt == 0) throw new VentSystemEmptyException("Не создано ни одного элемента");
                string oldvensystemguid = VentSystem.GUID;
                VentSystem.GUID = Guid.NewGuid().ToString(); //create ventsystem GUID
                if (!OpenForEdit) VentSystem.SystemName = radAutoCompleteBox1.Text; //get ventsystem name
                
                // Open DataBase
                SQLiteConnection connection = OpenDB(); //open DataBase
                connection.Open();
                
                if (connection.State == ConnectionState.Open)
                {
                    SQLiteCommand command = new SQLiteCommand
                    {
                        Connection = connection
                    };
                
                    //Write Ventsystem (host) to DataBase
                    var date = DateTime.Now.ToString("dd-MM-yyyy");
                    string vesrsion = "1";
                    string author = "Колтаков";
                    string InsertQueryVensystem = $"INSERT INTO Ventsystems ([GUID], SystemName, [Project], Modyfied, Version, Author, [Place]) VALUES ('{VentSystem.GUID}', '{VentSystem.SystemName}', '{Project.GetGUID()}', '{date}', '{vesrsion}', '{author}', '{Building.BuildGUID}')";
                    command.CommandText = InsertQueryVensystem;
                    command.ExecuteNonQuery();
                    // Write Ventsystem components to DataBase
                    WriteVentSystemToDB.Execute(command.Connection.ConnectionString, VentSystem, Project, Building) ;
                    //using (WriteVentSystemToDB writeVentSystemToDB = new WriteVentSystemToDB(command, VentSystem, this.Project, Building)) { }
                    
                    command.Dispose();
                    connection.Close();
                  
                   // Make TreeNode in Ventsystems Tree and Update Posnames
                    
                    RadTreeNode ventsystemnode = new RadTreeNode
                    {
                        Name = VentSystem.GUID,
                        Value = VentSystem.SystemName,
                        Text = VentSystem.SystemName,
                        Tag = VentSystem
                    };
                    if (OpenForEdit)
                    {
                        DeleteVentSystem(oldvensystemguid);
                        RadTreeNode treeNode = mainForm.FindNodeByName(oldvensystemguid, Joinedsystems);
                        RadTreeNode projectnode = mainForm.FindNodeByName(ProjectGuid, Projecttree.Nodes);
                        RadTreeNode buildNode = mainForm.FindNodeByName(Building.BuildGUID, projectnode.Nodes);
                        if (treeNode != null) //если поключена вент.система к шкафу
                        {
                            RadTreeNode pannelnode = treeNode.Parent;
                            
                            if (pannelnode != null)
                            {
                                SelectedNode.Name = VentSystem.GUID;
                                SelectedNode.Tag = VentSystem;
                                RadTreeNode ventnode = mainForm.FindNodeByName(oldvensystemguid, pannelnode.Nodes);
                                
                                ventnode.Name = VentSystem.GUID;
                                ventnode.Tag = VentSystem;
                                int ventcnt = pannelnode.Nodes.Count;
                                string pannelGUID = pannelnode.Name;
                                string pannelname = pannelnode.Text;
                                UpdatePannel(ventsystemnode.Name, pannelGUID);
                                UpdateVentSystemQuery(ventsystemnode.Name, pannelGUID, pannelname);
                                UpdateConnectedCables(ventsystemnode.Name, pannelGUID, pannelname);
                                UpdateConnectedPosNames(ventsystemnode.Name, ventcnt, ".");
                                Pannel pannel = (Pannel)pannelnode.Tag;
                                //UpdatePannelPower(VentSystem, pannel);
                                UpdatePannelVoltage(pannel, pannelnode);
                                
                               

                            }





                        }
                        mainForm.UpdateBuildNode(buildNode);
                        //SelectedNode.Name = ventSystem.GUID;
                        //SelectedNode.Tag = ventSystem;

                    }
                    else
                    {
                        Ventree.Nodes.Add(ventsystemnode);
                    }
                    Ventree.Update();
                    
                    Ventree.SelectedNode = null;
                    Close();
                    
                }

            }
            catch (VentSystemEmptyException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion
        #region PictureBoxClicked
        private void PictureBox1_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Tag is SupplyFiltr supplyFiltr) radPropertyGrid1.SelectedObject = supplyFiltr;
        }
        private void PictureBox6_Click(object sender, EventArgs e)
        {
            if (pictureBox6.Tag is SupplyVent supplyVent) radPropertyGrid1.SelectedObject = supplyVent;

        }
        private void PictureBox7_Click(object sender, EventArgs e)
        {
            if (pictureBox7.Tag is WaterHeater waterHeater)
            {
                radPropertyGrid1.SelectedObject = waterHeater;
                
            }
        }
        private void PictureBox8_Click(object sender, EventArgs e)
        {
            if (pictureBox8.Tag is ElectroHeater electroHeater) radPropertyGrid1.SelectedObject = electroHeater;
        }
        private void PictureBox5_Click(object sender, EventArgs e)
        {
            if (pictureBox5.Tag is Froster froster) radPropertyGrid1.SelectedObject = froster;
        }
        private void PictureBox10_Click(object sender, EventArgs e)
        {
            if (pictureBox10.Tag is ExtFiltr extFiltr) radPropertyGrid1.SelectedObject = extFiltr;
        }
        private void PictureBox11_Click(object sender, EventArgs e)
        {
            if (pictureBox11.Tag is ExtVent extVent) radPropertyGrid1.SelectedObject = extVent;
        }
        private void PictureBox12_Click(object sender, EventArgs e)
        {
            if (pictureBox12.Tag is ExtDamper extDamper) radPropertyGrid1.SelectedObject = extDamper;
        }
        private void PictureBox3_Click(object sender, EventArgs e)
        {
            if (pictureBox3.Tag is Recuperator recuperator) radPropertyGrid1.SelectedObject = recuperator;

        }
        private void PictureBox4_Click(object sender, EventArgs e)
        {
            if (pictureBox4.Tag is SupplyDamper supplyDamper) radPropertyGrid1.SelectedObject = supplyDamper;

        }
        private void PictureBox9_Click(object sender, EventArgs e)
        {
            if (pictureBox9.Tag is Humidifier humidifier) radPropertyGrid1.SelectedObject = humidifier;
        }
        private void PictureBox15_Click(object sender, EventArgs e)
        {
            if (pictureBox15.Tag is OutdoorTemp outdoorTemp) radPropertyGrid1.SelectedObject = outdoorTemp;
        }
        private void PictureBox14_Click(object sender, EventArgs e)
        {
            if (pictureBox14.Tag is ExhaustTemp exhaustTemp) radPropertyGrid1.SelectedObject = exhaustTemp;
        }
        private void PictureBox13_Click(object sender, EventArgs e)
        {
            if (pictureBox13.Tag is SupplyTemp supplyTemp) radPropertyGrid1.SelectedObject = supplyTemp;
        }
        private void PictureBox16_Click(object sender, EventArgs e)
        {
            if (pictureBox16.Tag is IndoorTemp indoorTemp) radPropertyGrid1.SelectedObject = indoorTemp;
        }

        #endregion
        #region Func
        private void AnimationClose()
        {
            timer1.Start();

            Animator animator = new Animator
            {
                Paths = new Path(1, 0, 400, 100).ToArray()
            };
            animator.Play(this, Animator.KnownProperties.Opacity);

        }
        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (Opacity == 0) Close();
        }
        private SQLiteConnection OpenDB()
        {
            string BDPath = DBFilePath;
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
  
        private void DeleteVentSystem(string ventsystemGUID)
        {
            SQLiteConnection connection = OpenDB();
            connection.Open();
            if (connection.State == ConnectionState.Open)
            {
                SQLiteCommand command = new SQLiteCommand
                {
                    Connection = connection
                };

                List<string> DeleteQuery = new List<string>
                {
                    $"DELETE FROM VentSystems WHERE [GUID] = '{ventsystemGUID}'",
                    $"DELETE FROM Ventilator WHERE [SystemGUID] = '{ventsystemGUID}'",
                    $"DELETE FROM Filter WHERE [SystemGUID] = '{ventsystemGUID}'",
                    $"DELETE FROM SensPDS WHERE [SystemGUID] = '{ventsystemGUID}'",
                    $"DELETE FROM Cable WHERE [SystemGUID] = '{ventsystemGUID}'",
                    $"DELETE FROM Damper WHERE [SystemGUID] = '{ventsystemGUID}'",
                    $"DELETE FROM WaterHeater WHERE [SystemGUID] = '{ventsystemGUID}'",
                    $"DELETE FROM ElectroHeater WHERE [SystemGUID] = '{ventsystemGUID}'",
                    $"DELETE FROM Pump WHERE [SystemGUID] = '{ventsystemGUID}'",
                    $"DELETE FROM Valve WHERE [SystemGUID] = '{ventsystemGUID}'",
                    $"DELETE FROM SensT WHERE [SystemGUID] = '{ventsystemGUID}'",
                    $"DELETE FROM Humidifier WHERE [SystemGUID] = '{ventsystemGUID}'",
                    $"DELETE FROM SensHum WHERE [SystemGUID] = '{ventsystemGUID}'",
                    $"DELETE FROM Recuperator WHERE [SystemGUID] = '{ventsystemGUID}'",
                    $"DELETE FROM Froster WHERE [SystemGUID] = '{ventsystemGUID}'",
                    $"DELETE FROM KKB WHERE [SystemGUID] = '{ventsystemGUID}'",
                    $"DELETE FROM FControl WHERE [SystemGUID] = '{ventsystemGUID}'"
                };
                
                foreach (string query in DeleteQuery)
                {
                    command.CommandText = query;
                    command.ExecuteNonQuery();
                }
                command.Dispose();
                connection.Close();
                
            }
        }
        private void UpdatePannel(string newventystemGUID, string pannelguid)
        {
           
            if (pannelguid != string.Empty)
            {
                using (SQLiteConnection connection = OpenDB())
                {
                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                    {
                        SQLiteCommand command = new SQLiteCommand
                        {
                            Connection = connection
                        };

                        string updatepannelquery1 = $"UPDATE Pannel SET SystemGUID = '{newventystemGUID}' WHERE [GUID] = '{pannelguid}'";
                        
                        command.CommandText = updatepannelquery1;
                        command.ExecuteNonQuery();
                        command.Dispose();
                    }
                    connection.Close();
                }
            }
            
        }
        private void UpdateVentSystemQuery(string newventsystemGUID, string pannelguid, string pannelname)
        {
            if (pannelguid != string.Empty)
            {
                using (SQLiteConnection connection = OpenDB())
                {
                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                    {
                        SQLiteCommand command = new SQLiteCommand
                        {
                            Connection = connection
                        };
                        List<string> updatepannelquery = new List<string>
                        {
                            $"UPDATE VentSystems SET Pannel = '{pannelguid}' WHERE [GUID] = '{newventsystemGUID}'",
                            $"UPDATE VentSystems SET PannelName = '{pannelname}' WHERE [GUID] = '{newventsystemGUID}'"

                        };
                        foreach(string query in updatepannelquery)
                        {
                            command.CommandText = query;
                            command.ExecuteNonQuery();
                        }
                        

                        command.Dispose();
                    }
                    connection.Close();
                }
            }
        }
        private void UpdateConnectedCables(string ventystegGUID, string pannelguid, string pannelname)
        {
            try
            {

                using (SQLiteConnection connection = OpenDB())
                {

                    string InsertPannelToCableQuery = $"UPDATE Cable SET FromPannel = '{pannelname}', FromGUID = '{pannelguid}' WHERE SystemGUID = '{ventystegGUID}'";


                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                    {
                        SQLiteCommand command = new SQLiteCommand
                        {
                            Connection = connection
                        };
                        command.CommandText = InsertPannelToCableQuery;
                        command.ExecuteNonQuery();
                        command.Dispose();
                        connection.Close();
                    }

                }


            }
            catch { }
        }
        private void UpdateConnectedPosNames(string systemguid, int AllConnectedSumm, string devider)
        {
            try
            {
              
                string Numbering = $"SELECT [To], COUNT(*) AS ToCount FROM Cable WHERE SystemGUID = '{systemguid}' GROUP BY [To]";

                SQLiteCommand command = new SQLiteCommand
                {
                    Connection = Connection
                };
                SQLiteCommand command1 = new SQLiteCommand
                {
                    Connection = Connection
                };
                SQLiteCommand command2 = new SQLiteCommand
                {
                    Connection = Connection
                };
                SQLiteCommand command3 = new SQLiteCommand
                {
                    Connection = Connection
                };
                command.CommandText = Numbering;
                SQLiteDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    string Pos = dataReader[0].ToString();
                    var query = "SELECT [To], Cable.ToGUID, Cable.GUID, Cable.SortPriority, Cable.WriteBlock, Cable.TableForSearch FROM Cable " +
                                $"WHERE((([To]) = '{Pos}') AND((Cable.SystemGUID) = '{systemguid}')) " +
                                "ORDER BY Cable.SortPriority;";
                    command1.CommandText = query;
                    SQLiteDataReader readerchild1 = command1.ExecuteReader();
                    int cnt = 1;
                    while (readerchild1.Read())
                    {
                        Posnames posnames = new Posnames();
                        bool writeblock = bool.Parse(readerchild1[4].ToString());
                        string hosttable = readerchild1[5].ToString();
                        string posguid = readerchild1[1].ToString();
                        string newposname = (AllConnectedSumm) + devider + readerchild1[0] + cnt;
                        posnames.Oldposname = readerchild1[0].ToString();
                        posnames.Newposname = newposname;
                        if (writeblock)
                        {
                            string query2 = $"UPDATE {hosttable} SET PosName = '{posnames.Newposname}' WHERE GUID = '{posguid}'";
                            command3.CommandText = query2;
                            command3.ExecuteNonQuery();
                            cnt++;

                        }
                        else
                        {
                            string query3 = $"SELECT PosName FROM {hosttable} WHERE GUID = '{posguid}'";
                            command3.CommandText = query3;
                            SQLiteDataReader readerchild2 = command3.ExecuteReader();
                            while (readerchild2.Read())
                            {
                                posnames.Newposname = readerchild2[0].ToString();
                            }
                            readerchild2.Close();
                            
                        }

                        
                        string updateposquery = $"UPDATE Cable Set [To] = '{posnames.Newposname}' " +
                                              $"WHERE Cable.ToGUID = '{posguid}'";// AND Cable.GUID = '{cableguid}'";
                        command2.CommandText = updateposquery;
                        command2.ExecuteNonQuery();

                      //description = readerchild[0].ToString();
                    }
                    readerchild1.Close();
                   
                } 
                dataReader.Close();
                
                command.Dispose();
                command1.Dispose();
                command2.Dispose();

            }
            catch (Exception ex)
            {
                
                MessageBox.Show(ex.Message + @"; " +  ex.StackTrace);
            }

            
        }
        private RadTreeNode FindNodeByName(object text, RadTreeNodeCollection nodes)
        {

            foreach (RadTreeNode node in nodes)
            {

                if (node.Name.Equals(text))
                {
                    return node;
                }

                RadTreeNode n = FindNodeByName(text, node.Nodes);
                if (n != null)
                {
                    return n;
                }


            }

            return null;
        }
        private void UpdatePannelVoltage(Pannel pannel, RadTreeNode pannelnode)
        {
            
            string findventsGUID = $"Select GUID FROM VentSystems WHERE Pannel = '{pannel.GetGUID()}'";
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = Connection,
                CommandText = findventsGUID
            };
            command.CommandText = findventsGUID;
            List<string> VSGuids = new List<string>();
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader[0].ToString() != string.Empty) VSGuids.Add(reader[0].ToString());
                }


            }
            if (VSGuids.Count > 0)
            {
                Pannel._Voltage _Voltage = Pannel._Voltage.AC220;

                foreach (string GUID in VSGuids)
                {
                    RadTreeNode VSnode = mainForm.FindNodeByName(GUID, pannelnode.Nodes);
                    if (VSnode != null)
                    {
                        VentSystem vent = (VentSystem)VSnode.Tag;
                        if (mainForm.GetVetSystemMaxVoltage(vent) == Pannel._Voltage.AC380)
                        {
                            _Voltage = Pannel._Voltage.AC380;
                            break;
                        }

                    }
                }
                pannel.Voltage = _Voltage;

            }
            else
            {
                pannel.Voltage = Pannel._Voltage.AC220;
            }
            string UpdatePannelVoltageQuery = $"UPDATE Pannel SET Power = '{pannel.Power}' WHERE [GUID] = '{pannel.GetGUID()}'";
            command.CommandText = UpdatePannelVoltageQuery;
            command.ExecuteNonQuery();
            command.Dispose();



        }
        #endregion
        private void RadPropertyGrid1_SelectedObjectChanged(object sender, PropertyGridSelectedObjectChangedEventArgs e)
        {
            if (e.SelectedObject == null) return;
            switch (e.SelectedObject.GetType().Name)
            {
                case "Froster":
                    if (radPropertyGrid1.SelectedObject is Froster froster)
                    {
                        var frosterType = froster._FrosterType;

                        pictureBox5.Image = new Bitmap(Froster.makeimagepath(frosterType));
                        radPropertyGrid1.PropertyGridElement.PropertyTableElement.BeginUpdate();

                        radPropertyGrid1.Items["Stairs"].Visible = frosterType == Froster.FrosterType.Freon;
                        radPropertyGrid1.Items["KKBControlType"].Visible = frosterType == Froster.FrosterType.Freon;
                        radPropertyGrid1.Items["valveType"].Visible = frosterType == Froster.FrosterType.Water;
                        radPropertyGrid1.PropertyGridElement.PropertyTableElement.EndUpdate();
                    }
                    break;
            }
        }
        #region Internal Classes
        internal class Posnames
        {
            internal string Oldposname { get; set; }
            internal string Newposname { get; set; }
        }
        #endregion

    }
}