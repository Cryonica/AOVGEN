using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using AOVGEN.Properties;
using Bunifu.UI.WinForms;
using Telerik.WinControls.UI;
using WinFormAnimation;
using Path = WinFormAnimation.Path;
using Point = System.Drawing.Point;
using PositionChangedEventArgs = Telerik.WinControls.UI.Data.PositionChangedEventArgs;
using Size = System.Drawing.Size;
using Timer = System.Windows.Forms.Timer;

namespace AOVGEN
{
#pragma warning disable IDE1006
     public partial class EditorV2
     {
        #region Global Variables
        internal VentSystem VentSystem { get; set; }
        public string DBFilePath { get; set; }
        public RadTreeView Ventree { get; set; }
        public RadTreeView Projecttree { get; set; }
        public RadTreeNodeCollection Joinedsystems { get; set; }
        internal string projectGuid { get; set; }
        internal Building Building { get; set; }
        internal Project Project { get; set; }
        internal bool OpenForEdit { get; set; }
        internal RadTreeNode SelectedNode { get; set; }
        internal SQLiteConnection connection { get; set; }
        //internal string oldpower = string.Empty;
        private int alarmCnt;
        internal string Author;
        private readonly MainForm mainForm;
        private List<ToolStripButton> CrossectionButtons;
        private bool SupplyVentPresent;
        private bool ExtVentPresent;
        private bool SupplyVentSparePresent;
        private bool ExtVentSparePresent;

        #endregion


        public EditorV2()
        {
            InitializeComponent();
            //this.TransparencyKey = this.BackColor;
            MouseMove += mouseEvent;
            MouseClick += mouseClick;
        }
        public EditorV2(MainForm refform)
        {
            mainForm = refform;
            InitializeComponent();
            MouseMove += mouseEvent;
            MouseClick += mouseClick;
            Location = Center();

        }

        private Point Center()
        {
            return new Point((Screen.PrimaryScreen.Bounds.Size.Width / 2) - (Size.Width / 2), (Screen.PrimaryScreen.Bounds.Size.Height / 2) - (Size.Height / 2));
        }

        private void EditorV2_Load(object sender, EventArgs e)
        {
                        
            Activate();
            Focus();
            var animator = new Animator
            {
                Paths = new Path(0, 1, 500, 0).ToArray()
            };
            animator.Play(this, Animator.KnownProperties.Opacity);

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.DoubleBuffer,
                true);
            GenerateCells();
            bunifuImageButton3.MouseDown += BunifuImageButton3_MouseDown;
            bunifuImageButton1.MouseDown += BunifuImageButton1_MouseDown;
            bunifuImageButton9.MouseDown += BunifuImageButton9_MouseDown;
            bunifuImageButton10.MouseDown += BunifuImageButton10_MouseDown;

            if (VentSystem == null)
            {
                var myventSystem = new VentSystem();
                VentSystem = myventSystem;
                radAutoCompleteBox1.Text = "VentsystemName;";
            }
            else
            {
                //процедура восстановления состояния вентсистемы в редакторе
                radAutoCompleteBox1.Text = VentSystem.SystemName;
                radAutoCompleteBox1.Enabled = false;
                //oldpower = mainForm.GetVentSystemPower(VentSystem).ToString(); //сохраняем мощность вент.агрегата до редактирования
                var rm = Resources.ResourceManager;
                foreach (var posInfo in VentSystem.ComponentsV2)
                {
                    var additionalHeight = 0;
                    var additionalWidth = 0;
                    if(posInfo.SizeY == 1)
                    {
                        additionalHeight = 10;
                        additionalWidth = 2;
                    }

                    var button = new DoubleBufferedBunifuImageButton
                    {
                        Image = (Bitmap)rm.GetObject(posInfo.ImageName),
                        Width = 50 + (50 *posInfo.SizeX) + additionalWidth,
                        Height = 50 + (50*posInfo.SizeY) + additionalHeight,
                        BackgroundImageLayout = ImageLayout.Stretch,
                        BackColor = Color.Transparent,
                        Location = CreatePointFromCell(posInfo.Pos),
                        Padding = new Padding(3),
                        InitialImage = (Bitmap)rm.GetObject(posInfo.ImageName),
                        SizeMode = PictureBoxSizeMode.StretchImage,
                    
                        Tag = posInfo,
                        
                    };

                    button.Zoom = posInfo.SizeY > 0 ? 5 : 15;
                    
                    OccupDict.Add(MD5HashGenerator.GenerateKey(posInfo.Pos), posInfo);
                    button.Click += button_Click;
                    Controls.Add(button);
                    Controls.SetChildIndex(button, 0);
                    
                }

                ExtVentPresent = VentSystem.ComponentsV2
                    .Where(posExtVent =>
                    {
                        bool b = false;
                        string name = posExtVent.Tag.GetType().Name;
                        switch (name)
                        {
                            case nameof(SpareExtVent):
                            case nameof(ExtVent):
                                b = true;
                                break;
                        }

                        return b;
                    }).Any();
                SupplyVentPresent = VentSystem.ComponentsV2
                    .Where(poSupplyVent =>
                    {
                        bool b = false;
                        string name = poSupplyVent.Tag.GetType().Name;
                        switch (name)
                        {
                            case nameof(SpareSuplyVent):
                            case nameof(SupplyVent):
                                b = true;
                                break;
                        }

                        return b;
                    }).Any();
                

            }
            Building = Projecttree.SelectedNode.Tag as Building;
            Project = Projecttree.SelectedNode.Parent.Tag as Project;
            CreateCrossSectionButtons();
            void CreateCrossSectionButtons()
            {
                CrossectionButtons = new List<ToolStripButton>();
                var Item1 = new ToolStripButton("Соединение1", Resources.cross1, imageButton9LayoutClicked, "Соединение1");
                var Item1T = new ToolStripButton("Соединение1 с датчиком Т в канале", Resources.cross1T, imageButton9LayoutClicked, "Соединение1Т");//Соединение1ТН
                var Item1TH = new ToolStripButton("Соединение1 с датчиками Т,H в канале", Resources.cross1TH, imageButton9LayoutClicked, "Соединение1ТН");
                var Item2 = new ToolStripButton("Соединение2", Resources.cross2, imageButton9LayoutClicked, "Соединение2");
                var Item3 = new ToolStripButton("Соединение3", Resources.cross3, imageButton9LayoutClicked, "Соединение3");
                var Item4 = new ToolStripButton("Соединение4", Resources.cross4, imageButton9LayoutClicked, "Соединение4");
                var Item5 = new ToolStripButton("Соединение5", Resources.cross5, imageButton9LayoutClicked, "Соединение5");
                var Item6 = new ToolStripButton("Соединение6", Resources.cross6, imageButton9LayoutClicked, "Соединение6");
                var Item7 = new ToolStripButton("Соединение7", Resources.cross7, imageButton9LayoutClicked, "Соединение7");
                var Item8 = new ToolStripButton("Соединение8", Resources.cross8, imageButton9LayoutClicked, "Соединение8");
                var Item9 = new ToolStripButton("Соединение9", Resources.cross9, imageButton9LayoutClicked, "Соединение9");
                var Item10 = new ToolStripButton("Соединение10", Resources.cross10, imageButton9LayoutClicked, "Соединение10");
                var Item11 = new ToolStripButton("Соединение11", Resources.cross11, imageButton9LayoutClicked, "Соединение11");
                Item1.Tag = "cross1";
                Item1T.Tag = "cross1T";
                Item1TH.Tag = "cross1TH";
                string[] images = {
                        "cross1", "cross1T", "cross1TH", "cross2", "cross3", "cross4", "cross5", "cross6", "cross7", "cross8", "cross9", "cross10", "cross11"
                };
                CrossectionButtons.Add(Item1);
                CrossectionButtons.Add(Item1T);
                CrossectionButtons.Add(Item1TH);
                CrossectionButtons.Add(Item2);
                CrossectionButtons.Add(Item3);
                CrossectionButtons.Add(Item4);
                CrossectionButtons.Add(Item5);
                CrossectionButtons.Add(Item6);
                CrossectionButtons.Add(Item7);
                CrossectionButtons.Add(Item8);
                CrossectionButtons.Add(Item9);
                CrossectionButtons.Add(Item10);
                CrossectionButtons.Add(Item11);
                for (var t = 0; t <= CrossectionButtons.Count - 1; t++)
                {
                    CrossectionButtons[t].Tag = images[t];
                }
            }

            string path = @"%AppData%";
            path = Environment.ExpandEnvironmentVariables(path);
            path += @"\Autodesk\Revit\Addins\2021\GKSASU\AOVGen\";
            XmlDocument doc = new XmlDocument();
            doc.Load(path + @"Presets.xml");
            ReadXMLPresets(doc);

        }

        private void ReadXMLPresets(XmlDocument xmlPresetsDocument)
        {

            
            XmlElement documentElement = xmlPresetsDocument.DocumentElement;
            if (documentElement == null) return;
            XmlNodeList PresetsNodes = documentElement.SelectNodes("/Presets/Preset");
            Dictionary<(string, string),  List<PosInfo>> presetsDictionary = new Dictionary<(string, string), List<PosInfo>>();
            //if (VentSystem == null) VentSystem = new VentSystem();

            if (PresetsNodes == null) return;
            foreach (XmlNode presets in PresetsNodes)//пресеты
            {
                string presetName = presets.Attributes["presetName"].Value;
                string presetType = presets.Attributes["presetType"].Value;
                XmlNode ComponentNode = presets.FirstChild;
                List<PosInfo> posInfos = new List<PosInfo>();
                foreach (XmlNode items in ComponentNode)
                {
                    if (items.Attributes != null)
                    {
                        string ItemName = items.Attributes["itemName"].Value;
                        PropertyInfo pinfo;
                        PosInfo posInfo;
                        switch (ItemName)
                        {
                            
                            case nameof(SupplyVent):
                                SupplyVent supplyVent = new SupplyVent();

                                XmlNode ParamNodeSupplyVent = items.ChildNodes[0];
                                XmlNode posInfoNodeSupplyVent = items.ChildNodes[1];

                                foreach (XmlNode param in ParamNodeSupplyVent.ChildNodes)
                                {
                                    switch (param.LocalName)
                                    {
                                        case "Voltage":
                                            var voltage = param.InnerText;
                                            pinfo = supplyVent.GetType().GetProperty("Voltage");
                                            pinfo?.SetValue(supplyVent, Enum.Parse(pinfo.PropertyType, voltage));
                                            break;
                                        case "Power":
                                            supplyVent.Power = param.InnerText;
                                            break;
                                        case "Loction":
                                            supplyVent.Location = param.InnerText;
                                            break;
                                        case "Description":
                                            supplyVent.Description = param.InnerText;
                                            break;
                                        case "ControlType":
                                            string controlType = param.InnerText;
                                            pinfo = supplyVent.GetType().GetProperty("ControlType");
                                            pinfo?.SetValue(supplyVent,
                                                Enum.Parse(pinfo.PropertyType, controlType));
                                            break;
                                        case "Protect":
                                            string protect = param.InnerText;
                                            pinfo = supplyVent.GetType().GetProperty("Protect");
                                            pinfo?.SetValue(supplyVent, Enum.Parse(pinfo.PropertyType, protect));
                                            break;
                                    }
                                }
                                posInfo = SetXmlProPertyToPosInfo(posInfoNodeSupplyVent);
                                posInfo.Tag = supplyVent;
                                posInfos.Add(posInfo);
                                break;
                            case nameof(ExtVent):
                                
                                ExtVent extVent = new ExtVent();

                                XmlNode ParamNodeExtVent = items.ChildNodes[0];
                                XmlNode posInfoNodeExtVent = items.ChildNodes[1];

                                foreach (XmlNode param in ParamNodeExtVent.ChildNodes)
                                {
                                    
                                    switch (param.LocalName)
                                    {
                                        case "Voltage":
                                            var voltage = param.InnerText;
                                            pinfo = extVent.GetType().GetProperty("Voltage");
                                            pinfo?.SetValue(extVent, Enum.Parse(pinfo.PropertyType, voltage));
                                            break;
                                        case "Power":
                                            extVent.Power = param.InnerText;
                                            break;
                                        case "Loction":
                                            extVent.Location = param.InnerText;
                                            break;
                                        case "Description":
                                            extVent.Description = param.InnerText;
                                            break;
                                        case "ControlType":
                                            string controlType = param.InnerText;
                                            pinfo = extVent.GetType().GetProperty("ControlType");
                                            pinfo?.SetValue(extVent, Enum.Parse(pinfo.PropertyType, controlType));
                                            break;
                                        case "Protect":
                                            string protect = param.InnerText;
                                            pinfo = extVent.GetType().GetProperty("Protect");
                                            pinfo?.SetValue(extVent, Enum.Parse(pinfo.PropertyType, protect));
                                            break;
                                    }
                                }
                                posInfo = SetXmlProPertyToPosInfo(posInfoNodeExtVent);
                                posInfo.Tag = extVent;
                                posInfos.Add(posInfo);
                                break;
                            case nameof(SupplyDamper):
                                SupplyDamper supplyDamper = new SupplyDamper();

                                XmlNode ParamNodeSupplyDamper = items.ChildNodes[0];
                                XmlNode posInfoNodeSupplyDamper = items.ChildNodes[1];

                                foreach (XmlNode param in ParamNodeSupplyDamper.ChildNodes)
                                {
                                    switch (param.LocalName)
                                    {
                                        case "HasControl":
                                            supplyDamper.HasContol = Convert.ToBoolean(param.InnerText);
                                            break;
                                        case "Description":
                                            supplyDamper.Description = param.InnerText;
                                            break;
                                        case nameof(OutdoorTemp):
                                            XmlNode outdoorNode = param.ChildNodes[0];
                                            XmlNode controlNode = outdoorNode.FirstChild;
                                            var controlType = controlNode.InnerText;
                                            switch (controlType)
                                            {
                                                case "Analogue":
                                                case "Discrete":
                                                    supplyDamper.SetSensor = true;
                                                    pinfo = supplyDamper.GetType().GetProperty("SensorType");
                                                    pinfo?.SetValue(supplyDamper,
                                                        Enum.Parse(pinfo.PropertyType, controlType));
                                                    break;
                                                case "No":
                                                    break;
                                            }
                                            break;
                                    }
                                }
                                posInfo = SetXmlProPertyToPosInfo(posInfoNodeSupplyDamper);
                                posInfo.Tag = supplyDamper;
                                posInfos.Add(posInfo);
                                break;
                            case nameof(ExtDamper):
                                ExtDamper extDamper = new ExtDamper();

                                XmlNode ParamNodeExtDamper = items.ChildNodes[0];
                                XmlNode posInfoNodeExtDamper = items.ChildNodes[1];

                                foreach (XmlNode param in ParamNodeExtDamper.ChildNodes)
                                {
                                    switch (param.LocalName)
                                    {
                                        case "HasControl":
                                            extDamper.HasContol = Convert.ToBoolean(param.InnerText);
                                            break;
                                        case "Description":
                                            extDamper.Description = param.InnerText;
                                            break;
                                    }
                                }

                                posInfo = SetXmlProPertyToPosInfo(posInfoNodeExtDamper);
                                posInfo.Tag = extDamper;
                                posInfos.Add(posInfo);
                                break;

                            case nameof(SupplyFiltr):
                                SupplyFiltr supplyFiltr = new SupplyFiltr();

                                XmlNode ParamNodeSupplyFiltr = items.ChildNodes[0];
                                XmlNode posInfoNodeSupplyFiltr = items.ChildNodes[1];

                                foreach (XmlNode param in ParamNodeSupplyFiltr.ChildNodes)
                                {
                                    switch (param.LocalName)
                                    {
                                        case "Description":
                                            supplyFiltr.Description = param.InnerText;
                                            break;
                                        
                                        case nameof(PressureContol):
                                            XmlNode pressureControlNode = param.ChildNodes[0];
                                            XmlNode controlNode = pressureControlNode.FirstChild;
                                            var controlType = controlNode.InnerText;
                                            switch (controlType)
                                            {
                                                case "Analogue":
                                                case "Discrete":
                                                    
                                                    pinfo = supplyFiltr.GetType().GetProperty("PressureProtect");
                                                    pinfo?.SetValue(supplyFiltr,
                                                        Enum.Parse(pinfo.PropertyType, controlType));
                                                    break;
                                                case "No":
                                                    break;
                                            }
                                            break;
                                    }
                                }
                                posInfo = SetXmlProPertyToPosInfo(posInfoNodeSupplyFiltr);
                                posInfo.Tag = supplyFiltr;
                                posInfos.Add(posInfo);
                                break;
                            case nameof(ExtFiltr):
                                ExtFiltr extFiltr = new ExtFiltr();
                                
                                XmlNode ParamNodeExtFiltr = items.ChildNodes[0];
                                XmlNode posInfoNodeExtFiltr = items.ChildNodes[1];

                                foreach (XmlNode param in ParamNodeExtFiltr.ChildNodes)
                                {
                                    switch (param.LocalName)
                                    {
                                        case "Description":
                                            extFiltr.Description = param.InnerText;
                                            break;

                                        case nameof(PressureContol):
                                            XmlNode pressureControlNode = param.ChildNodes[0];
                                            XmlNode controlNode = pressureControlNode.FirstChild;
                                            var controlType = controlNode.InnerText;
                                            switch (controlType)
                                            {
                                                case "Analogue":
                                                case "Discrete":

                                                    pinfo = extFiltr.GetType().GetProperty("PressureProtect");
                                                    pinfo?.SetValue(extFiltr,
                                                        Enum.Parse(pinfo.PropertyType, controlType));
                                                    break;
                                                case "No":
                                                    break;
                                            }
                                            break;
                                    }
                                }
                                
                                posInfo = SetXmlProPertyToPosInfo(posInfoNodeExtFiltr);
                                posInfo.Tag = extFiltr;
                                posInfos.Add(posInfo);
                                break;
                            case nameof(Filtr):
                                Filtr filtr= new Filtr();

                                XmlNode ParamNodeFiltr = items.ChildNodes[0];
                                XmlNode posInfoNodeFiltr = items.ChildNodes[1];

                                foreach (XmlNode param in ParamNodeFiltr.ChildNodes)
                                {
                                    switch (param.LocalName)
                                    {
                                        case "Description":
                                            filtr.Description = param.InnerText;
                                            break;

                                        case nameof(PressureContol):
                                            XmlNode pressureControlNode = param.ChildNodes[0];
                                            XmlNode controlNode = pressureControlNode.FirstChild;
                                            var controlType = controlNode.InnerText;
                                            switch (controlType)
                                            {
                                                case "Analogue":
                                                case "Discrete":

                                                    pinfo = filtr.GetType().GetProperty("PressureProtect");
                                                    pinfo?.SetValue(filtr,
                                                        Enum.Parse(pinfo.PropertyType, controlType));
                                                    break;
                                                case "No":
                                                    break;
                                            }
                                            break;
                                    }
                                }

                                posInfo = SetXmlProPertyToPosInfo(posInfoNodeFiltr);
                                posInfo.Tag = filtr;
                                posInfos.Add(posInfo);
                                break;
                            case nameof(ElectroHeater):
                                ElectroHeater electroHeater = new ElectroHeater();

                                XmlNode ParamNodeEllectroHeater = items.ChildNodes[0];
                                XmlNode posInfoNodeElectroHeater = items.ChildNodes[1];

                                foreach (XmlNode param in ParamNodeEllectroHeater.ChildNodes)
                                {
                                    switch (param.LocalName)
                                    {
                                        case "Description":
                                            electroHeater.Description = param.InnerText;
                                            break;
                                        case "Voltage":
                                            var voltage = param.InnerText;
                                            pinfo = electroHeater.GetType().GetProperty("Voltage");
                                            pinfo?.SetValue(electroHeater, Enum.Parse(pinfo.PropertyType, voltage));
                                            break;
                                        case "Power":
                                            electroHeater.Power = param.InnerText;
                                            break;
                                        case "Stairs":
                                            
                                            break;
                                        case "ControlType":
                                            
                                            break;
                                        case "Protect":

                                            break;

                                    }
                                    
                                }
                                
                                posInfo = SetXmlProPertyToPosInfo(posInfoNodeElectroHeater);
                                posInfo.Tag = electroHeater;
                                posInfos.Add(posInfo);
                                break;
                            case nameof(WaterHeater):
                                WaterHeater waterHeater = new WaterHeater();

                                XmlNode ParamNodeWaterHeater = items.ChildNodes[0];
                                XmlNode posInfoNodeWaterHeater = items.ChildNodes[1];

                                foreach (XmlNode param in ParamNodeWaterHeater.ChildNodes)
                                {
                                    switch (param.LocalName)
                                    {
                                        case "ValveType":
                                            pinfo = waterHeater.GetType().GetProperty("valveType");
                                            pinfo?.SetValue(waterHeater,
                                                Enum.Parse(pinfo.PropertyType, param.InnerText));
                                            
                                            break;
                                        case "PumpTK":
                                            waterHeater.HasTK = Convert.ToBoolean(param.InnerText);
                                            break;
                                        case "WaterProtect":
                                            
                                            pinfo = waterHeater.GetType().GetProperty("Waterprotect");
                                            pinfo?.SetValue(waterHeater,
                                                Enum.Parse(pinfo.PropertyType, param.InnerText));
                                            break;
                                       

                                    }

                                }


                                posInfo = SetXmlProPertyToPosInfo(posInfoNodeWaterHeater);
                                posInfo.Tag = waterHeater;
                                posInfos.Add(posInfo);
                                break;
                            case nameof(Humidifier):
                                Humidifier humidifier = new Humidifier();

                                XmlNode ParamNodeHumidifier = items.ChildNodes[0];
                                XmlNode posInfoNodeHumidifier = items.ChildNodes[1];

                                foreach (XmlNode param in ParamNodeHumidifier.ChildNodes)
                                {
                                    switch (param.LocalName)
                                    {
                                        case "HumType":
                                            
                                            pinfo = humidifier.GetType().GetProperty("HumType");
                                            pinfo?.SetValue(humidifier,
                                                Enum.Parse(pinfo.PropertyType, param.InnerText));

                                            break;
                                        case "ControlType":
                                            pinfo = humidifier.GetType().GetProperty("HumControlType");
                                            pinfo?.SetValue(humidifier,
                                                Enum.Parse(pinfo.PropertyType, param.InnerText));
                                            break;
                                        case "Voltage":
                                            
                                            pinfo = humidifier.GetType().GetProperty("Voltage");
                                            pinfo?.SetValue(humidifier,
                                                Enum.Parse(pinfo.PropertyType, param.InnerText));
                                            break;
                                        case "Power":
                                            humidifier.Power = param.InnerText;
                                            break;
                                        case "HumSensPresent":
                                            humidifier.HumSensPresent = Convert.ToBoolean(param.InnerText);
                                            break;
                                        case "SensorType":
                                            if (humidifier.HumSensPresent)
                                            {
                                                
                                                pinfo = humidifier.GetType().GetProperty("SensorType");
                                                pinfo?.SetValue(humidifier,
                                                    Enum.Parse(pinfo.PropertyType, param.InnerText));
                                            }
                                            break;
                                    }

                                }



                                posInfo = SetXmlProPertyToPosInfo(posInfoNodeHumidifier);
                                posInfo.Tag = humidifier;
                                posInfos.Add(posInfo);

                                break;
                            case nameof(Froster):
                                
                                XmlNode ParamNodeFroster = items.ChildNodes[0];
                                XmlNode posInfoNodeFroster= items.ChildNodes[1];
                                Froster froster = null;
                                
                                foreach (XmlNode param in ParamNodeFroster.ChildNodes)
                                {
                                    
                                    switch (param.LocalName)
                                    {
                                        case "FrosterType":
                                            Froster.FrosterType frosterType =
                                                (Froster.FrosterType) Enum.Parse(typeof(Froster.FrosterType),
                                                    param.InnerText, true);
                                            froster = new Froster(frosterType);
                                            break;
                                        case "Stairs":
                                            if (froster != null)
                                            {
                                                pinfo = froster.GetType().GetProperty("Stairs");
                                                pinfo?.SetValue(froster,
                                                    Enum.Parse(pinfo.PropertyType, param.InnerText));
                                            }
                                            break;
                                        case "KKBControlType":
                                            if (froster != null)
                                            {
                                                pinfo = froster.GetType().GetProperty("KKBControlType");
                                                pinfo?.SetValue(froster,
                                                    Enum.Parse(pinfo.PropertyType, param.InnerText));
                                            }

                                            break;
                                        case "ValveType":
                                            if (froster != null)
                                            {
                                                pinfo = froster.GetType().GetProperty("valveType");
                                                pinfo?.SetValue(froster,
                                                    Enum.Parse(pinfo.PropertyType, param.InnerText));
                                            }
                                            break;
                                        case "Voltage":
                                            if (froster != null)
                                            {
                                                pinfo = froster.GetType().GetProperty("Voltage");
                                                pinfo?.SetValue(froster,
                                                    Enum.Parse(pinfo.PropertyType, param.InnerText));
                                            }
                                            break;
                                        case "Power":
                                            if (froster != null)
                                            {
                                                froster.Power = param.InnerText;
                                            }
                                            
                                            break;
                                    }

                                }

                                posInfo = SetXmlProPertyToPosInfo(posInfoNodeFroster);
                                posInfo.Tag = froster;
                                posInfos.Add(posInfo);

                                break;
                            case nameof(Recuperator):
                                XmlNode ParamNodeRecuperator = items.ChildNodes[0];
                                XmlNode posInfoNodeRecuperator = items.ChildNodes[1];
                                Recuperator recuperator = new Recuperator();

                                foreach (XmlNode param in ParamNodeRecuperator.ChildNodes)
                                {

                                    switch (param.LocalName)
                                    {
                                        case "RecuperatorType":
                                            pinfo = recuperator.GetType().GetProperty("_RecuperatorType");
                                            pinfo?.SetValue(recuperator,
                                                Enum.Parse(pinfo.PropertyType, param.InnerText));
                                            break;
                                    }
                                }

                                posInfo = SetXmlProPertyToPosInfo(posInfoNodeRecuperator);
                                posInfo.Tag = recuperator;
                                posInfos.Add(posInfo);

                                break;
                            case nameof(Room):
                            {
                                XmlNode ParamNodeCrossection = items.ChildNodes[0];
                                XmlNode posInfoNodeCrossSection = items.ChildNodes[1];
                                string sensTtype = string.Empty;
                                string sensHtype = string.Empty;

                                foreach (XmlNode param in ParamNodeCrossection.ChildNodes)
                                {

                                    switch (param.LocalName)
                                    {
                                        case "SensorTType":
                                            sensTtype = param.InnerText;
                                            break;
                                        case "SensorHType":
                                            sensHtype = param.InnerText;
                                            break;
                                    }
                                }

                                bool sentTPresent = sensTtype != "No";
                                bool sentHPresent = sensHtype != "No";

                                Room room = new Room(sentTPresent, sentHPresent);
                                if (room._SensorT != null)
                                {
                                    pinfo = room.GetType().GetProperty("sensorTType");
                                    pinfo?.SetValue(room, Enum.Parse(pinfo.PropertyType, sensTtype));
                                }

                                if (room._SensorH != null)
                                {
                                    pinfo = room.GetType().GetProperty("sensorHType");
                                    pinfo?.SetValue(room, Enum.Parse(pinfo.PropertyType, sensHtype));
                                }

                                posInfo = SetXmlProPertyToPosInfo(posInfoNodeCrossSection);
                                posInfo.Tag = room;
                                posInfos.Add(posInfo);
                            }
                                
                                break;
                            case nameof(CrossSection):
                            {
                                    XmlNode ParamNodeCrossection = items.ChildNodes[0];
                                    XmlNode posInfoNodeCrossSection = items.ChildNodes[1];
                                    string sensTtype = string.Empty;
                                    string sensHtype = string.Empty;
                                    foreach (XmlNode param in ParamNodeCrossection.ChildNodes)
                                    {

                                        switch (param.LocalName)
                                        {
                                            case "SensorTType":
                                                sensTtype = param.InnerText;
                                                break;
                                            case "SensorHType":
                                                sensHtype = param.InnerText;
                                                break;
                                        }
                                    }

                                    bool sentTPresent = sensTtype != "No";
                                    bool sentHPresent = sensHtype != "No";

                                    CrossSection crossSection = new CrossSection(sentTPresent, sentHPresent);
                                    if (crossSection._SensorT != null)
                                    {
                                        pinfo = crossSection.GetType().GetProperty("sensorTType");
                                        pinfo?.SetValue(crossSection, Enum.Parse(pinfo.PropertyType, sensTtype));
                                    }

                                    if (crossSection._SensorH != null)
                                    {
                                        pinfo = crossSection.GetType().GetProperty("sensorHType");
                                        pinfo?.SetValue(crossSection, Enum.Parse(pinfo.PropertyType, sensHtype));
                                    }



                                    posInfo = SetXmlProPertyToPosInfo(posInfoNodeCrossSection);
                                    posInfo.Tag = crossSection;
                                    posInfos.Add(posInfo);
                                }
                                


                                break;
                           
                        }
                    }
                }

                var presetkey = (presetName, presetType);
                presetsDictionary.Add(presetkey, posInfos);

            }

            var t = presetsDictionary
                .GroupBy(e => e.Key.Item2)
                .ToList();
            foreach (var dataItem in t.Select(VARIABLE => new RadListDataItem
            {
                Text = VARIABLE.Key,
                Tag = VARIABLE
                    .ToDictionary(e=> e.Key.Item1)
            }))
            {
                radDropDownList1.Items.Add(dataItem);
            }

            foreach (KeyValuePair<(string, string), List<PosInfo>> keyValuePair in presetsDictionary)
            {
                RadListDataItem dataItem = new RadListDataItem();
                dataItem.Text = keyValuePair.Key.Item1;
                dataItem.Tag = keyValuePair.Value;
                radDropDownList2.Items.Add(dataItem);
            }
        }
        private static PosInfo SetXmlProPertyToPosInfo(XmlNode posInfoNode)
        {
            PosInfo posInfo = new PosInfo();
            foreach (XmlNode posinfo in posInfoNode)
            {
                switch (posinfo.LocalName)
                {
                    case "Pos":
                        var Pos = posinfo
                            .InnerText
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(e => Convert.ToInt32(e))
                            .ToArray();
                        posInfo.Pos = Pos;
                        break;
                    case "Size":
                        var Size = posinfo
                            .InnerText
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(e => Convert.ToInt32(e))
                            .ToArray();
                        posInfo.Size = Size;
                        break;
                    case "Image":
                        posInfo.ImageName = posinfo.InnerText;
                        break;
                }
            }
            return posInfo;
        }
        private void mouseEvent(object sender, MouseEventArgs e)
        {
            //radButton2.Text = currObjects?.Count().ToString();
            if (currObject != null)
            {
                if (currObjects == null)
                {
                    int[] pos = GetCellPos(currObject);
                    var newPoint = new Point(pos[0], pos[1]);
                    currObject.GetType().GetProperty("Location")
                        ?.SetValue(currObject, newPoint);
                }
                
            }
            if (currObjects == null || activeObject == null) return;
            DoubleBufferedBunifuImageButton button = (DoubleBufferedBunifuImageButton)activeObject;
            PosInfo posInfo = (PosInfo)button.Tag;
            var oldX = posInfo.PozX;
            var oldY = posInfo.PozY;
            int[] pos1 = GetCellPos(activeObject);
            var newPoint1 = new Point(pos1[0], pos1[1]);
            button.Location = newPoint1;
                    
            int dX = posInfo.PozX - oldX;
            int dY = posInfo.PozY - oldY;
            if (dX ==0 && dY ==0) return;
            foreach (var curButton in currObjects
                .Where(objCurrObject => objCurrObject != activeObject)
                .Cast<DoubleBufferedBunifuImageButton>())
            {
                PosInfo curobjPosInfo = (PosInfo)curButton.Tag;
                

                curobjPosInfo.Pos = new[]
                {
                    curobjPosInfo.PozX + dX,
                    curobjPosInfo.PozY + dY
                };

                radButton2.Text = curobjPosInfo.PozY.ToString();
                if (curobjPosInfo.PozY == 4 && curobjPosInfo.SizeY > 0 || curobjPosInfo.PozX<0 || curobjPosInfo.PozY > 4)
                {
                    curButton.BackColor = Color.Red;
                }

                else
                {
                    
                    curButton.BackColor = button.BackColor;
                }
                curButton.Location = CreatePointFromCell(curobjPosInfo.Pos);
                curButton.Visible = curButton.BackColor != Color.Red;
                
            }



        }

        private void mouseClick(object s, MouseEventArgs e)
        {
            
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (currObject is DoubleBufferedBunifuImageButton Button)
                    {
                        var posInfo = (PosInfo)Button.Tag;
                        if (posInfo.Pos[0] > 14 || posInfo.Pos[1] > 5)
                        {
                            Controls.Remove(Button);
                            OccupDict.Remove(MD5HashGenerator.GenerateKey(posInfo.Pos));

                        }
                        currObject.GetType().GetProperty("BackColor")?.SetValue(currObject, Color.Transparent);
                        currObject = null;
                    }
                    break;
                case MouseButtons.Right:
                    if (currObject == null)
                    {
                        if (lastPosInfo?.SizeX==0 && lastPosInfo?.SizeY ==0)
                        {
                            CreateButton(lastSelectedImage, lastComponent.GetType(), ((dynamic)lastPosInfo.ImageName));
                        }
                        else
                        {
                            
                            if (lastPosInfo != null && lastComponent != null)
                            {
                                CreateBigButton(lastSelectedImage, lastComponent.GetType(), lastPosInfo.SizeX, lastPosInfo.SizeY, ((dynamic)lastPosInfo.ImageName));
                            }
                            
                        }
                        
                    }
                    else
                    {
                        var button = (DoubleBufferedBunifuImageButton)currObject;
                        var posInfo = (PosInfo)button.Tag;
                        if (posInfo.PozX == 50 || posInfo.PozY == 50)
                        {
                            Controls.Remove((Control)currObject);
                            currObject = null;
                        }
                    }
                    break;
            }
        }

        private void EndSelectGroup()
        {
            foreach (var sbutton in currObjects)
            {
                DoubleBufferedBunifuImageButton b = (DoubleBufferedBunifuImageButton)sbutton;
                PosInfo sPosInfo = (PosInfo)b.Tag;
                //remove wrond buttons
                if ((sPosInfo.Pos[0] > 14 || sPosInfo.Pos[1] > 4) || (sPosInfo.PozY == 4 && sPosInfo.SizeY == 1))
                {
                    if (OccupDict != null)
                    {

                        foreach (var i in OccupDict.Where(d => (d.Value == sPosInfo)).ToList())
                        {
                            OccupDict.Remove(i.Key);
                        }


                    }
                    Controls.Remove(b);
                    switch (sPosInfo.Tag.GetType().Name)
                    {
                        case nameof(SupplyVent):
                        case nameof(SpareSuplyVent):
                            SupplyVentPresent = false;
                            SupplyVentSparePresent = false;
                            break;
                        case nameof(ExtVent):
                        case nameof(SpareExtVent):
                            ExtVentPresent = false;
                            ExtVentSparePresent = false;
                            break;

                    }
                    continue;
                }
                //write correct buttons
                string generateKey = MD5HashGenerator.GenerateKey(sPosInfo.Pos);
                var occupied = OccupDict != null && OccupDict.ContainsKey(generateKey);
                if (!occupied)
                {
                    OccupDict?.Add(generateKey, sPosInfo);
                }

                if (sPosInfo.SizeY > 0)
                {
                    for (int i = 0; i <= sPosInfo.SizeY; i++)
                    {
                        string genGen = MD5HashGenerator.GenerateKey(new[] {sPosInfo.PozX, sPosInfo.PozY + i});
                        if (OccupDict != null && !OccupDict.ContainsKey(genGen)) OccupDict.Add(genGen, sPosInfo);
                    }
                }

                b.BackColor = Color.Transparent;
            }
            activeObject = null;
            currObjects = null;
        }
        private void button_Click(object sender, EventArgs e)
        {
            var args = (MouseEventArgs)e;

            switch (args.Button)
            {
                case MouseButtons.Left:
                    
                    if (currObjects != null)// if selected group
                    {
                        if (activeObject == null) // start select 
                        {
                            activeObject = sender;
                            
                            //delete occupied positions
                            var forreemove = currObjects
                                .Cast<DoubleBufferedBunifuImageButton>()
                                .Select(e1 => (PosInfo)e1.Tag)
                                .Select(e1=> MD5HashGenerator.GenerateKey(e1.Pos))
                                .ToList();
                            var forremove2 = currObjects
                                .Cast<DoubleBufferedBunifuImageButton>()
                                .Select(e2 => (PosInfo) e2.Tag)
                                .Where(e2 => e2.SizeY > 0)
                                .ToList();
                            if (forremove2.Count>0)
                            {
                                List<string> addhashes = new List<string>();
                                foreach (var pos in forremove2)
                                {
                                    for (int i = 0; i <= pos.SizeY; i++)
                                    {
                                        addhashes.Add(MD5HashGenerator.GenerateKey(new[]{pos.PozX, pos.PozY+i}));
                                    }
                                }
                                forreemove.AddRange(addhashes);
                            }

                            foreach (var hashForRemove in forreemove
                                .Where(hashForRemove => OccupDict.ContainsKey(hashForRemove)))
                            {
                                OccupDict.Remove(hashForRemove);
                            }
                        }
                        else //end select
                        {
                            EndSelectGroup();
                        }
                    }
                    else //if selected one object
                    {
                        switch (currObject)
                        {
                            case null: //start select
                            {
                                currObject = sender;
                                DoubleBufferedBunifuImageButton sbutton = (DoubleBufferedBunifuImageButton) currObject;
                                OccupDict.Remove(MD5HashGenerator.GenerateKey(((PosInfo) sbutton.Tag).Pos));
                                PosInfo posInfo = (PosInfo) ((DoubleBufferedBunifuImageButton) currObject).Tag;
                                if (posInfo.SizeY > 0)
                                {
                                    for (int i = 0; i <= posInfo.SizeY; i++)
                                    {
                                        string genGen =
                                            MD5HashGenerator.GenerateKey(new[] {posInfo.PozX, posInfo.PozY + i});
                                        if (OccupDict.ContainsKey(genGen)) OccupDict.Remove(genGen);
                                    }
                                }
                                break;
                            }
                            case DoubleBufferedBunifuImageButton button1: //end select
                            {
                                if (currObject.Equals(sender))
                                {
                                    var posInfo1 = (PosInfo)button1.Tag;
                                    hash = MD5HashGenerator.GenerateKey(posInfo1.Pos);
                                    if (posInfo1.Pos[0] > 14 || posInfo1.Pos[1] > 4)
                                    {
                                        Controls.Remove(button1);
                                        currObject = null;
                                        switch (posInfo1.Tag.GetType().Name)
                                        {
                                            case nameof(SupplyVent):
                                            case nameof(SpareSuplyVent):
                                                SupplyVentPresent = false;
                                                SupplyVentSparePresent = false;
                                                break;
                                            case nameof(ExtVent):
                                            case nameof(SpareExtVent):
                                                ExtVentPresent = false;
                                                ExtVentSparePresent = false;
                                                break;

                                        }
                                        return;
                                    }
                                    if (!present)
                                    {
                                        if (posInfo1.SizeX > 0 || posInfo1.SizeY > 0)
                                        {
                                            for (var Y1 = 0; Y1 <= posInfo1.SizeY; Y1++)
                                            {
                                                for (var X1 = 0; X1 <= posInfo1.SizeX; X1++)
                                                {
                                                    var hashmas = new[] { posInfo1.PozX + X1, posInfo1.PozY + Y1 };
                                                    var occupied = OccupDict.ContainsKey(MD5HashGenerator.GenerateKey(hashmas));
                                                    if (!occupied)
                                                    {
                                                        OccupDict.Add(MD5HashGenerator.GenerateKey(hashmas), posInfo1);

                                                    }
                                                }

                                            }
                                        }
                                        else
                                        {
                                            OccupDict.Add(hash, posInfo1);
                                        }
                                        currObject?.GetType().GetProperty("BackColor")?.SetValue(currObject, Color.Transparent);
                                        switch (posInfo1.Tag?.GetType().Name)
                                        {
                                            case nameof(SupplyVent):
                                                SupplyVentPresent = true;
                                                break;
                                            case nameof(ExtVent):
                                                ExtVentPresent = true;
                                                break;
                                            case nameof(SpareSuplyVent):
                                                SupplyVentSparePresent = true;
                                                break;
                                            case nameof(SpareExtVent):
                                                ExtVentSparePresent = true;
                                                break;
                                        }

                                        currObject = null;
                                    }
                                    else
                                    {
                                        currObject = null;
                                    }


                                }

                                break;
                            }
                        }
                    }
                    





                    //if (currObject is DoubleBufferedBunifuImageButton button1)
                    //{
                        
                    //    if (currObject.Equals(sender))
                    //    {
                    //        if (currObjects != null && activeObject == null)
                    //        {
                    //            activeObject = currObject;
                    //        }
                    //        else
                    //        {
                    //            activeObject = null;
                    //        }
                    //        var posInfo1 = (PosInfo)button1.Tag;
                    //        hash = MD5HashGenerator.GenerateKey(posInfo1.Pos);
                    //        if (posInfo1.Pos[0] > 14 || posInfo1.Pos[1] > 4)
                    //        {
                                
                    //            Controls.Remove(button1);
                    //            if (currObjects != null)
                    //            {
                    //                var forreemove = OccupDict
                    //                    .Where(e1 => e1.Value.PozX > 14 || e1.Value.PozY >= 14)
                    //                    .Select(e1 => e1.Key)
                    //                    .ToList();
                    //                foreach (var obj in currObjects)
                    //                {
                    //                    DoubleBufferedBunifuImageButton buffered = (DoubleBufferedBunifuImageButton) obj;
                    //                    PosInfo posInfo = (PosInfo) buffered.Tag;


                    //                    if (forreemove != null)
                    //                        foreach (var hashForRemove in forreemove)
                    //                        {
                    //                            OccupDict.Remove(hashForRemove);
                    //                        }

                    //                    forreemove = null;

                                        

                                        
                    //                    if (OccupDict.ContainsKey(hash)) OccupDict.Remove(hash);
                                        
                    //                    Controls.Remove((DoubleBufferedBunifuImageButton) obj);
                    //                }

                    //                currObjects = null;
                    //            }
                    //            currObject = null;
                    //            switch (posInfo1.Tag.GetType().Name)
                    //            {
                    //                case nameof(SupplyVent) :
                    //                case nameof(SpareSuplyVent):
                    //                    SupplyVentPresent = false;
                    //                    SupplyVentSparePresent = false;
                    //                    break;
                    //                case nameof(ExtVent):
                    //                case nameof(SpareExtVent):
                    //                    ExtVentPresent = false;
                    //                    ExtVentSparePresent = false;
                    //                    break;
                                    
                    //            }

                    //            return;

                    //        }

                    //        if (!present)
                    //        {
                    //            if (posInfo1.SizeX > 0 || posInfo1.SizeY > 0)
                    //            {
                    //                for (var Y1 = 0; Y1 <= posInfo1.SizeY; Y1++)
                    //                {
                    //                    for (var X1 = 0; X1 <= posInfo1.SizeX; X1++)
                    //                    {
                    //                        hashmas = new[] { posInfo1.PozX + X1, posInfo1.PozY + Y1 };
                    //                        var occupied = OccupDict.ContainsKey(MD5HashGenerator.GenerateKey(hashmas));
                    //                        if (!occupied)
                    //                        {
                    //                            OccupDict.Add(MD5HashGenerator.GenerateKey(hashmas), posInfo1);

                    //                        }
                    //                    }

                    //                }
                    //            }
                    //            else
                    //            {
                    //                OccupDict.Add(hash, posInfo1);
                    //            }
                    //            currObject?.GetType().GetProperty("BackColor")?.SetValue(currObject, Color.Transparent);
                    //            switch (posInfo1?.Tag?.GetType().Name)
                    //            {
                    //                case nameof(SupplyVent):
                    //                    SupplyVentPresent = true;
                    //                    break;
                    //                case nameof(ExtVent):
                    //                    ExtVentPresent = true;
                    //                    break;
                    //                case nameof(SpareSuplyVent):
                    //                    SupplyVentSparePresent = true;
                    //                    break;
                    //                case nameof(SpareExtVent):
                    //                    ExtVentSparePresent = true;
                    //                    break;
                    //            }

                    //            currObject = null;
                    //        }
                    //        else
                    //        {
                    //            currObject = null;
                    //        }
                    //    }

                        
                    //}
                    ////Object not selected
                    //else
                    //{

                    //    if (currObjects != null)
                    //    {
                    //        activeObject = sender;
                    //    }
                        
                    //    currObject = sender;
                    //    var button3 = (DoubleBufferedBunifuImageButton)sender;
                    //    var posInfo = (PosInfo)button3.Tag;
                    //    Controls.SetChildIndex(button3, 0);
                    //    if (posInfo.SizeX > 0 || posInfo.SizeY > 0)
                    //    {
                    //        for (var Y1 = 0; Y1 <= posInfo.SizeY; Y1++)
                    //        {
                    //            for (var X1 = 0; X1 <= posInfo.SizeX; X1++)
                    //            {
                    //                hashmas = new[] { posInfo.PozX + X1, posInfo.PozY + Y1 };
                    //                var occupied = OccupDict.ContainsKey(MD5HashGenerator.GenerateKey(hashmas));
                    //                if (occupied)
                    //                {
                    //                    OccupDict.Remove(MD5HashGenerator.GenerateKey(hashmas));
                    //                }
                    //            }

                    //        }

                    //    }
                    //    else
                    //    {
                    //        hashmas = new[] { posInfo.PozX, posInfo.PozY };
                    //        var occupied = OccupDict.ContainsKey(MD5HashGenerator.GenerateKey(hashmas));
                    //        if (occupied)
                    //        {
                    //            OccupDict.Remove(MD5HashGenerator.GenerateKey(hashmas));
                    //        }
                    //    }
                    //    radPropertyGrid1.SelectedObject = null;
                    //}
                    break;
                case MouseButtons.Right:
                    var button = (DoubleBufferedBunifuImageButton)sender;

                    if (button.BackColor == Color.Transparent)
                    {
                        var posInfo = (PosInfo)button.Tag;
                        var obj = posInfo.Tag;
                        radPropertyGrid1.SelectedObject = obj;
                        
                    }
                    else
                    {
                        if (button.BackColor == Color.Red)
                        {
                            Controls.Remove(currObject as Control);
                            currObject = null;
                        }
                        else
                        {
                            button_Click(currObject, new MouseEventArgs(MouseButtons.Left, 0, Cursor.Position.X, Cursor.Position.Y, 0));
                        }

                    }
                    break;
            }
        }
        private async void BunifuImageButton1_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left && currObject == null)
                {
                    var islongpress = await CheckLongPress(sender as BunifuImageButton, 650);
                    if (!islongpress) return;
                    //string[] layouts = new string[] { "Test 1", "Test 2", "Test 3" };
                    var items = new List<ToolStripButton>();

                    var SupplyItem = new ToolStripButton("Приточный вентилятор", Resources.fan_right, imageButton1LayoutClicked, "Приток")
                    {
                        Tag = "fan_right"
                    };

                    var ExtItem = new ToolStripButton("Вытяжной вентилятор", Resources.fan_left, imageButton1LayoutClicked, "Вытяжка")
                    {
                        Tag = "fan_left"
                    };

                    var SupplyItemDual = new ToolStripButton("Приточный вентилятор с резервом", Resources.fan_right_dual, imageButton1LayoutClicked, "Приток сдвоенный")
                    {
                        Tag = "fan_right_dual"
                    };
                    var ExtItemDual = new ToolStripButton("Вытяжной вентилятор с резервом", Resources.fan_left_dual, imageButton1LayoutClicked, "Вытяжка сдвоенный")
                    {
                        Tag = "fan_left_dual"
                    };
                    if (SupplyVentPresent || SupplyVentSparePresent)
                    {
                        SupplyItem.Enabled = false;
                        SupplyItemDual.Enabled = false;
                    }

                    if (ExtVentPresent || ExtVentSparePresent)
                    {
                        ExtItem.Enabled = false;
                        ExtItemDual.Enabled = false;

                    }

                    items.Add(SupplyItem);
                    items.Add(ExtItem);
                    items.Add(SupplyItemDual);
                    items.Add(ExtItemDual);



                    var layoutMenus = new ContextMenuStrip
                    {
                        ImageScalingSize = new Size(40, 40),
                        AutoSize = false,
                        Width = 300,
                        Height = 200
                    };
                    layoutMenus.Items.Clear();
                    layoutMenus.Items.AddRange(items.ToArray());
                    layoutMenus.Show(Cursor.Position.X, Cursor.Position.Y);

                }
                else
                {
                    if (!(currObject is PosInfo posInfo)) return;
                    if (posInfo.PozX != 50 && posInfo.PozY != 50) return;
                    Controls.Remove((Control)currObject);
                    currObject = null;
                }
            }
            catch 
            {
                //ignored
            }
        }
        private async void BunifuImageButton3_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left && currObject == null)
                {
                    var islongpress = await CheckLongPress(sender as BunifuImageButton, 650);
                    if (!islongpress) return;
                    //string[] layouts = new string[] { "Test 1", "Test 2", "Test 3" };
                    var items = new List<ToolStripButton>();
                    var SupplyItem = new ToolStripButton("Приточная заслонка", Resources.shutter_right, imageButton3LayoutClicked, "Приток")
                    {
                        Tag = "shutter_right"
                    };
                    var ExtItem = new ToolStripButton("Вытяжная заслонка", Resources.shutter_left, imageButton3LayoutClicked, "Вытяжка")
                    {
                        Tag = "shutter_left"
                    };
                    var SupplyItemT = new ToolStripButton("Приточная заслонка с датчком наружной Т", Resources.shutter_right_T, imageButton3LayoutClicked, "ПритокТ")
                    {
                        Tag = "shutter_right_T"
                    };
                    items.Add(SupplyItemT);
                    items.Add(SupplyItem);
                    items.Add(ExtItem);
                    //foreach (string layout in layouts)
                    //{
                    //    ToolStripButton item = new ToolStripButton(layout, TestApp.Properties.Resources.air_filter, LayoutClicked);
                    //    //item.AutoSize = false;
                    //    //item.Height = 50;
                    //    //item.Width = 50;
                    //    items.Add(item);
                    //}
                    var layoutMenus = new ContextMenuStrip
                    {
                        ImageScalingSize = new Size(40, 40),
                        AutoSize = false,
                        Width = 350,
                        Height = 150
                    };
                    layoutMenus.Items.Clear();
                    layoutMenus.Items.AddRange(items.ToArray());
                    layoutMenus.Show(Cursor.Position.X, Cursor.Position.Y);
                }
                else
                {

                    if (currObject != null && currObject is PosInfo posInfo)
                    {

                        if (posInfo.PozX == 50 || posInfo.PozY == 50)
                        {
                            Controls.Remove(currObject as Control);
                            currObject = null;
                        }
                    }

                }
            }
            catch 
            {
                //ignored
            }
            
        }
        private async void BunifuImageButton9_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left && currObject == null)
                {
                    var islongpress = await CheckLongPress(sender as BunifuImageButton, 650);
                    if (!islongpress) return;

                    var layoutMenus = new ContextMenuStrip
                    {
                        ImageScalingSize = new Size(40, 40),
                        AutoSize = false,
                        Width = 300,
                        Height = 650
                    };
                    layoutMenus.Items.Clear();
                    layoutMenus.Items.AddRange(CrossectionButtons.ToArray());
                    layoutMenus.Show(Cursor.Position.X, Cursor.Position.Y);
                }
                else
                {
                    if (currObject == null || !(currObject is PosInfo posInfo)) return;
                    if (posInfo.PozX != 50 && posInfo.PozY != 50) return;
                    Controls.Remove(currObject as Control);
                    currObject = null;
                }
            }
            catch 
            {
                //ignored
            }
            
        }
        private async void BunifuImageButton10_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left && currObject == null)
                {
                    var islongpress = await CheckLongPress(sender as BunifuImageButton, 650);
                    if (!islongpress) return;
                    //string[] layouts = new string[] { "Test 1", "Test 2", "Test 3" };
                    var items = new List<ToolStripButton>();
                    var Item1 = new ToolStripButton("Помещение, приток", Resources.room__arrow_supply, imageButton10LayoutClicked, "Соединение1");
                    var Item1T = new ToolStripButton("Помещение, приток с датчик.Т ", Resources.room__arrow_supply_T, imageButton10LayoutClicked, "Соединение1Т");//Соединение1ТН
                    var Item1TH = new ToolStripButton("Помещение, приток с датчик.Т, Н", Resources.room__arrow_supply_TH, imageButton10LayoutClicked, "Соединение1ТН");
                    var Item2 = new ToolStripButton("Помещение, вытяжка влево", Resources.room__arrow_exhaust_L, imageButton10LayoutClicked, "Соединение2");
                    var Item3 = new ToolStripButton("Помещение, вытяжка вправо", Resources.room__arrow_exhaust_R, imageButton10LayoutClicked, "Соединение3");
                    var Item4 = new ToolStripButton("Помещение, приток, вытяжка", Resources.room__arrow_supp_exh, imageButton10LayoutClicked, "Соединение4");
                    var Item5 = new ToolStripButton("Помещение, приток, вытяжка, с датч.Т", Resources.room__arrow_supp_exh_T, imageButton10LayoutClicked, "Соединение5");
                    var Item6 = new ToolStripButton("Помещение, приток, вытяжка, с датч.Т, Н", Resources.room__arrow_supp_exh_TH, imageButton10LayoutClicked, "Соединение6");
                    string[] images = {
                    "room__arrow_supply", "room__arrow_supply_T", "room__arrow_supply_TH", "room__arrow_exhaust_L", "room__arrow_exhaust_R", "room__arrow_supp_exh", "room__arrow_supp_exh_T", "room__arrow_supp_exh_TH"
                };
                    items.Add(Item1);
                    items.Add(Item1T);
                    items.Add(Item1TH);
                    items.Add(Item2);
                    items.Add(Item3);
                    items.Add(Item4);
                    items.Add(Item5);
                    items.Add(Item6);
                    for (var t = 0; t <= items.Count - 1; t++)
                    {
                        items[t].Tag = images[t];
                    }

                    var layoutMenus = new ContextMenuStrip
                    {
                        ImageScalingSize = new Size(40, 40),
                        AutoSize = false,
                        Width = 350,
                        Height = 400
                    };
                    layoutMenus.Items.Clear();
                    layoutMenus.Items.AddRange(items.ToArray());
                    layoutMenus.Show(Cursor.Position.X, Cursor.Position.Y);
                }
                else
                {
                    if (!(currObject is PosInfo posInfo)) return;
                    if (posInfo.PozX != 50 && posInfo.PozY != 50) return;
                    Controls.Remove(currObject as Control);
                    currObject = null;
                }
            }
            catch 
            {
                //ignored
            }
            

        }
        private void imageButton1LayoutClicked(object sender, EventArgs eventArgs)
        {
            switch (sender.GetType().GetProperty("Name")?.GetValue(sender, null).ToString())
            {
                case ("Приток"):
                    CreateButton(Resources.fan_right, typeof(SupplyVent), ((dynamic)sender).Tag);
                    break;
                case ("Вытяжка"):
                    CreateButton(Resources.fan_left, typeof(ExtVent), ((dynamic)sender).Tag);
                    break;
                case ("Приток сдвоенный"):
                    CreateButton(Resources.fan_right_dual, typeof(SpareSuplyVent), ((dynamic)sender).Tag);
                    break;
                case ("Вытяжка сдвоенный"):
                    CreateButton(Resources.fan_left_dual, typeof(SpareExtVent), ((dynamic)sender).Tag);
                    break;


            }
            Cursor.Position = new Point(Cursor.Position.X + 50, Cursor.Position.Y);

        }
        private void imageButton3LayoutClicked(object sender, EventArgs eventArgs)
        {
            switch (sender.GetType().GetProperty("Name")?.GetValue(sender, null))
            {
                case ("Приток"):
                    CreateButton(Resources.shutter_right, typeof(SupplyDamper), ((dynamic)sender).Tag);
                    break;
                case ("Вытяжка"):
                    CreateButton(Resources.shutter_left, typeof(ExtDamper), ((dynamic)sender).Tag);
                    break;
                case ("ПритокТ"):
                    CreateButton(Resources.shutter_right_T, typeof(SupplyDamper), ((dynamic)sender).Tag);
                    break;

            }
            Cursor.Position = new Point(Cursor.Position.X + 50, Cursor.Position.Y);

        }
        private void imageButton9LayoutClicked(object sender, EventArgs eventArgs)
        {

            switch (sender.GetType().GetProperty("Name")?.GetValue(sender, null))
            {
                case ("Соединение1"):
                    CreateButton(Resources.cross1, typeof(CrossSection), ((dynamic)sender).Tag);
                    break;
                case ("Соединение1Т"):
                    CreateButton(Resources.cross1T, typeof(CrossSection), ((dynamic)sender).Tag);
                    break;
                case ("Соединение1ТН"):
                    CreateButton(Resources.cross1TH, typeof(CrossSection), ((dynamic)sender).Tag);
                    break;
                case ("Соединение2"):
                    CreateButton(Resources.cross2, typeof(CrossSection), ((dynamic)sender).Tag);
                    break;
                case ("Соединение3"):
                    CreateButton(Resources.cross3, typeof(CrossSection), ((dynamic)sender).Tag);
                    break;
                case ("Соединение4"):
                    CreateButton(Resources.cross4, typeof(CrossSection), ((dynamic)sender).Tag);
                    break;
                case ("Соединение5"):
                    CreateButton(Resources.cross5, typeof(CrossSection), ((dynamic)sender).Tag);
                    break;
                case ("Соединение6"):
                    CreateButton(Resources.cross6, typeof(CrossSection), ((dynamic)sender).Tag);
                    break;
                case ("Соединение7"):
                    CreateButton(Resources.cross7, typeof(CrossSection), ((dynamic)sender).Tag);
                    break;
                case ("Соединение8"):
                    CreateButton(Resources.cross8, typeof(CrossSection), ((dynamic)sender).Tag);
                    break;
                case ("Соединение9"):
                    CreateButton(Resources.cross9, typeof(CrossSection), ((dynamic)sender).Tag);
                    break;
                case ("Соединение10"):
                    CreateButton(Resources.cross10, typeof(CrossSection), ((dynamic)sender).Tag);
                    break;
                case ("Соединение11"):
                    CreateButton(Resources.cross11, typeof(CrossSection), ((dynamic)sender).Tag);
                    break;

            }
            Cursor.Position = new Point(Cursor.Position.X + 50, Cursor.Position.Y);

        }
        private void imageButton10LayoutClicked(object sender, EventArgs eventArgs)
        {
           
            switch (sender.GetType().GetProperty("Name")?.GetValue(sender, null))
            {
                case ("Соединение1"):
                    CreateButton(Resources.room__arrow_supply, typeof(Room), ((dynamic)sender).Tag);
                    break;
                case ("Соединение1Т"):
                    CreateButton(Resources.room__arrow_supply_T, typeof(Room), ((dynamic)sender).Tag);
                    break;
                case ("Соединение1ТН"):
                    CreateButton(Resources.room__arrow_supply_TH, typeof(Room), ((dynamic)sender).Tag);
                    break;
                case ("Соединение2"):
                    CreateButton(Resources.room__arrow_exhaust_L, typeof(Room), ((dynamic)sender).Tag);
                    break;
                case ("Соединение3"):
                    CreateButton(Resources.room__arrow_exhaust_R, typeof(Room), ((dynamic)sender).Tag);
                    break;
                case ("Соединение4"):
                    ((dynamic)sender).Tag = "room__arrow_supp_exh_big";
                    CreateBigButton(Resources.room__arrow_supp_exh_big, typeof(Room), 0, 1, ((dynamic)sender).Tag);
                    break;
                case ("Соединение5"):
                    ((dynamic)sender).Tag = "room__arrow_supp_exh_T_big";
                    CreateBigButton(Resources.room__arrow_supp_exh_T_big, typeof(Room), 0, 1, ((dynamic)sender).Tag);
                    break;
                case ("Соединение6"):
                    ((dynamic)sender).Tag = "room__arrow_supp_exh_TH_big";
                    CreateBigButton(Resources.room__arrow_supp_exh_TH_big, typeof(Room), 0, 1, ((dynamic)sender).Tag);
                    break;
            }
            Cursor.Position = new Point(Cursor.Position.X + 50, Cursor.Position.Y);

        }

        private void radPropertyGrid1_SelectedObjectChanged(object sender, PropertyGridSelectedObjectChangedEventArgs e)
        {
            var selobj = e.SelectedObject?.GetType().Name;
            void SetReadOnlyAttribute(object obj, string attrname, bool _readonly)
            {
                
                TypeDescriptor.GetProperties(obj)[attrname]
                            .SetReadOnlyAttribute(_readonly);
            }
            switch (selobj)
            {
                case nameof(CrossSection):
                    {
                        
                        var crossSection = (CrossSection)e.SelectedObject;
                        
                        //var t = TypeDescriptor.GetProperties(crossSection);


                        var setreadonlyH = crossSection._SensorH == null;
                        var setreadonlyT = crossSection._SensorT == null;
                        if (setreadonlyT && setreadonlyH)
                        {
                            radPropertyGrid1.SelectedObject = null;
                            return;
                        }
                        
                        SetReadOnlyAttribute(crossSection, "sensorHType", setreadonlyH);
                        SetReadOnlyAttribute(crossSection, "sensorTType", setreadonlyT);
                    }
                    
                    break;
                case nameof(Room):
                    {
                        var room = (Room)e.SelectedObject;
                        var setreadonlyH = room._SensorH == null;
                        var setreadonlyT = room._SensorT == null;

                        if (setreadonlyT && setreadonlyH)
                        {
                            radPropertyGrid1.SelectedObject = null;
                            return;
                        }
                        SetReadOnlyAttribute(room, "sensorHType", setreadonlyH);
                        SetReadOnlyAttribute(room, "sensorTType", setreadonlyT);
                    }
                    break;
            }
        }

        List<PosInfo> AddCrossection(List<PosInfo> posInfos)
        {
            List<PosInfo> exitPosInfos = new List<PosInfo>();
            PosInfo ySupplyPosInfo = posInfos
                .FirstOrDefault(e => e.Tag is SupplyVent || e.Tag is SpareSuplyVent);
            PosInfo yExtPosInfo = posInfos
                .FirstOrDefault(e => e.Tag is ExtVent || e.Tag is SpareExtVent);
            if (ySupplyPosInfo != null)
            {
                int YSupplyPos = ySupplyPosInfo.PozY;
                var LeftX = posInfos
                    .Where(e => e.PozY == YSupplyPos)
                    .OrderBy(e => e.PozX)
                    .DefaultIfEmpty()
                    .FirstOrDefault()?.PozX;
                var RightX = posInfos.Where(e => e.PozY == YSupplyPos)
                    .OrderBy(e => e.PozX)
                    .DefaultIfEmpty()
                    .Last().PozX;
                var OccupiedPosInfos = posInfos
                    .Where(e => e.PozY == YSupplyPos)
                    .ToDictionary(e => MD5HashGenerator.GenerateKey(e.Pos));

                for (int i = 0; i < RightX-LeftX; i++)
                {
                    int[] arr = {i, YSupplyPos};
                    if (!OccupiedPosInfos.ContainsKey(MD5HashGenerator.GenerateKey(arr)))
                    {
                        CrossSection crossSection = new CrossSection(false, false);
                        PosInfo posInfo = new PosInfo
                        {
                            Pos = arr,
                            ImageName = "cross1",
                            Tag = crossSection
                        };
                        exitPosInfos.Add(posInfo);
                    }
                    
                }
            }
            if (yExtPosInfo != null)
            {
                int YExtPos = yExtPosInfo.PozY;
                var LeftX = posInfos
                    .Where(e => e.PozY == YExtPos)
                    .OrderBy(e => e.PozX)
                    .DefaultIfEmpty()
                    .FirstOrDefault()?.PozX;
                var RightX = posInfos.Where(e => e.PozY == YExtPos)
                    .OrderBy(e => e.PozX)
                    .DefaultIfEmpty()
                    .Last().PozX;
                var OccupiedPosInfos = posInfos
                    .Where(e => e.PozY == YExtPos)
                    .ToDictionary(e => MD5HashGenerator.GenerateKey(e.Pos));

                for (int i = 0; i < LeftX; i++)//for (int i = 0; i < RightX - LeftX; i++)
                {
                    int[] arr = { i, YExtPos };
                    if (!OccupiedPosInfos.ContainsKey(MD5HashGenerator.GenerateKey(arr)))
                    {
                        CrossSection crossSection = new CrossSection(false, false);
                        PosInfo posInfo = new PosInfo
                        {
                            Pos = arr,
                            ImageName = "cross1",
                            Tag = crossSection
                        };
                        exitPosInfos.Add(posInfo);
                    }

                }
            }

            return exitPosInfos;
        }
        private void radButton12_Click(object sender, EventArgs e)
        {
            try
            {
                var controlsWithPosInfo = Controls
                    .Cast<Control>()
                    .Where(c => c.Tag is PosInfo)
                    .Select(c => (PosInfo)c.Tag)
                    .ToList()
                    .OrderBy(x => x.PozX)
                    .ToList()
                    .OrderBy(y => y.PozY)
                    .ToList();
                var t = AddCrossection(controlsWithPosInfo);
                if (t.Count>0) controlsWithPosInfo.AddRange(t);
                if (controlsWithPosInfo.Count>0)
                {
                    var Components = controlsWithPosInfo
                        .Where(r => r.Tag != null)
                        .Select(r => r.Tag)
                        .ToList();
                    if (Components.Count>0)
                    {
                        if (radAutoCompleteBox1.Text == "VentsystemName;" && alarmCnt <= 1)
                        {
                            alarmCnt++;
                            PlayAlarm();
                           
                            
                            return;
                        }
                        
                        var oldvensystemguid = VentSystem.GUID;
                        VentSystem.GUID = Guid.NewGuid().ToString(); //create ventsystem GUID
                        if (!OpenForEdit) VentSystem.SystemName = radAutoCompleteBox1.Text; //get ventsystem name
                        VentSystem.ComponentsV2.Clear();

                        VentSystem.ComponentsV2 = controlsWithPosInfo;

                        // Open DataBase
                        if (connection.State == ConnectionState.Closed)
                        {
                            connection = OpenDB(); //open DataBase
                            connection.Open();
                        }
                        else
                        {
                            var command = new SQLiteCommand
                            {
                                Connection = connection
                            };

                            //Write Ventsystem (host) to DataBase
                            var date = DateTime.Now.ToString("dd-MM-yyyy");
                            const string vesrsion = "1";
                            var InsertQueryVensystem = $"INSERT INTO Ventsystems ([GUID], SystemName, [Project], Modyfied, Version, Author, [Place]) VALUES ('{VentSystem.GUID}', '{VentSystem.SystemName}', '{Project.GetGUID()}', '{date}', '{vesrsion}', '{Author}', '{Building.BuildGUID}')";
                            command.CommandText = InsertQueryVensystem;
                            command.ExecuteNonQuery();
                            var connectionstring = command.Connection.ConnectionString;
                            // Write Ventsystem components to DataBase
                            WriteVentSystemToDB.Execute(connectionstring, VentSystem, Project, Building);
                            
                            command.Dispose();
                            connection.Close();
                            // Make TreeNode in Ventsystems Tree and Update Posnames
                            var ventsystemnode = new RadTreeNode
                            {
                                Name = VentSystem.GUID,
                                Value = VentSystem.SystemName,
                                Text = VentSystem.SystemName,
                                Tag = VentSystem
                            };
                            var projectnode = mainForm.FindNodeByName(projectGuid, Projecttree.Nodes);
                            var buildNode = mainForm.FindNodeByName(Building.BuildGUID, projectnode.Nodes);
                            if (OpenForEdit)
                            {
                                
                                DeleteVentSystem(oldvensystemguid);
                                var treeNode = mainForm.FindNodeByName(oldvensystemguid, Joinedsystems);
                                Task task = null;
                                var pannelnode = treeNode?.Parent;

                                if (pannelnode != null)
                                {
                                    SelectedNode.Name = VentSystem.GUID;
                                    SelectedNode.Tag = VentSystem;
                                    var ventnode = mainForm.FindNodeByName(oldvensystemguid, pannelnode.Nodes);

                                    ventnode.Name = VentSystem.GUID;
                                    ventnode.Tag = VentSystem;
                                    var ventcnt = pannelnode.Nodes.Count;
                                    var pannelGUID = pannelnode.Name;
                                    var pannelname = pannelnode.Text;
                                    var pannel = (Pannel)pannelnode.Tag;
                                    task = Task.Factory.StartNew(()=>
                                    {
                                        UpdatePannel(ventsystemnode.Name, pannelGUID);
                                        UpdateVentSystemQuery(ventsystemnode.Name, pannelGUID, pannelname);
                                        UpdateConnectedCables(ventsystemnode.Name, pannelGUID, pannelname);
                                        UpdateConnectedPosNames(ventsystemnode.Name, ventcnt, ".");
                                        Task.Factory.StartNew(() =>
                                                pannel.Power = UpdatePannelPower(pannelnode, connectionstring)
                                        );
                                        Task.Factory.StartNew(() =>
                                                pannel.Voltage = UpdatePannelVoltage(pannelnode, connectionstring)
                                        );
                                        

                                    });
                                }
                                if (task != null)
                                {
                                    task.Wait();
                                    mainForm.UpdateBuildNode(buildNode);

                                }
                                else
                                {
                                    mainForm.UpdateBuildNode(buildNode);
                                }
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
                }
                else
                {
                    
                    throw new VentSystemEmptyException("Не создано ни одного элемента");
                }
            }
            catch (VentSystemEmptyException ex)
            {
                MessageBox.Show(ex.Message);
            }
            void PlayAlarm()
            {
                
                new Animator3D(
                new Path3D(BackColor.ToFloat3D(), Color.Red.ToFloat3D(), 1000, 0, AnimationFunctions.CircularEaseIn),
                FPSLimiterKnownValues.NoFPSLimit)
                .Play(panel3, "BackColor");
                new Animator3D(
                    new Path3D(Color.Red.ToFloat3D(), BackColor.ToFloat3D(), 1000, 1001, AnimationFunctions.CircularEaseOut),
                    FPSLimiterKnownValues.NoFPSLimit)
                    .Play(panel3, "BackColor");
                
               
            }
            
        }

        private void radButton1_Click(object sender, EventArgs e)
        {
            Controls
                .Cast<Control>()
                .All(cntl => cntl.Enabled = false);

            try
            {
                var timer = new Timer
                {
                    Interval = 100
                };
                timer.Tick += Timer_Tick;
                timer.Start();
                new Animator(
                new Path(1, 0, 400, 0).ToArray(),
                FPSLimiterKnownValues.NoFPSLimit)
                .Play(this, Animator.KnownProperties.Opacity);
            }
            catch
            {
                // ignored
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine(Opacity);
            if (Opacity != 0) return;
            ((Timer)sender).Stop();
            DialogResult = DialogResult.Cancel;
        }

        private void EditorV2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDrag = true;
                
            }

            var control = (Control)sender;

            // Calculate the startPoint by using the PointToScreen 
            // method.
            startPoint = control.PointToScreen(new Point(e.X, e.Y));
            Invalidate();
        }

        private void EditorV2_MouseMove(object sender, MouseEventArgs e)
        {
            // If the mouse is being dragged, 
            // undraw and redraw the rectangle as the mouse moves.
            if (!isDrag) return;
            ControlPaint.DrawReversibleFrame(theRectangle,
                BackColor, FrameStyle.Dashed);

            // Calculate the endpoint and dimensions for the new 
            // rectangle, again using the PointToScreen method.
            var endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            var width = endPoint.X - startPoint.X;
            var height = endPoint.Y - startPoint.Y;
            theRectangle = new Rectangle(startPoint.X,
                startPoint.Y, width, height);

            // Draw the new rectangle by calling DrawReversibleFrame
            // again.  
            ControlPaint.DrawReversibleFrame(theRectangle,
                BackColor, FrameStyle.Dashed);
        }

        private void EditorV2_MouseUp(object sender, MouseEventArgs e)
        {
            isDrag = false;

            // Draw the rectangle to be evaluated. Set a dashed frame style 
            // using the FrameStyle enumeration.
            ControlPaint.DrawReversibleFrame(theRectangle,
                BackColor, FrameStyle.Dashed);

            // Find out which controls intersect the rectangle and 
            // change their color. The method uses the RectangleToScreen  
            // method to convert the Control's client coordinates 
            // to screen coordinates.

            var t = (from Control y in Controls
                     where y is DoubleBufferedBunifuImageButton
                     select y)
                    .ToList();
                
            for (var i = 0; i < t.Count; i++)
            {
                var controlRectangle = Controls[i].RectangleToScreen
                    (Controls[i].ClientRectangle);
                if (controlRectangle.IntersectsWith(theRectangle))
                {
                    Controls[i].BackColor = Color.BurlyWood;
                }
                else
                {
                    Controls[i].BackColor = Color.Transparent;
                    currObjects?.Remove(Controls[i]);
                }
            }
            currObjects = (from Control y in Controls
                           where y is DoubleBufferedBunifuImageButton
                           where y.BackColor == Color.BurlyWood
                           select y)
                           .Cast<object>()
                           .ToList();

            // Reset the rectangle.
            theRectangle = new Rectangle(0, 0, 0, 0);
            if (currObjects?.Count == 0) currObjects = null;
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    if (currObjects?.Count> 0)
                    {
                        foreach (var obj in currObjects)
                        {
                            var posInfo = (PosInfo)((DoubleBufferedBunifuImageButton)obj).Tag;
                            if (posInfo.SizeX > 0 || posInfo.SizeY > 0)
                            {
                                for (var Y1 = 0; Y1 <= posInfo.SizeY; Y1++)
                                {
                                    for (var X1 = 0; X1 <= posInfo.SizeX; X1++)
                                    {
                                        var hashmas = new[] { posInfo.PozX + X1, posInfo.PozY + Y1 };
                                        var occupied = OccupDict.ContainsKey(MD5HashGenerator.GenerateKey(hashmas));
                                        if (occupied)
                                        {
                                            OccupDict.Remove(MD5HashGenerator.GenerateKey(hashmas));
                                        }
                                    }

                                }

                            }
                            else
                            {
                                var occupied = OccupDict.ContainsKey(MD5HashGenerator.GenerateKey(posInfo.Pos));
                                if (occupied)OccupDict.Remove(MD5HashGenerator.GenerateKey(posInfo.Pos));
                                
                               
                            }
                            Controls.Remove((Control)obj);
                            
                        }

                        SupplyVentPresent = myf<SupplyVent>();
                        ExtVentPresent = myf<ExtVent>();
                        ExtVentSparePresent = myf<SpareExtVent>();

                        bool myf<T>()
                        {
                            return currObjects
                                .Cast<DoubleBufferedBunifuImageButton>()
                                .Select(e1 => (PosInfo)e1.Tag)
                                .Select(e1 => e1.Tag)
                                .FirstOrDefault(e1 => e.GetType().Name == nameof(T)) != null;
                        }
                    }
                    break;

                    /* ... */
            }

            base.OnKeyDown(e);
        }


        private void radDropDownList1_SelectedIndexChanged(object sender, PositionChangedEventArgs e)
        {
            
            radDropDownList2.Items.Clear();
            
            
            var dataItem = ((Dictionary<string, KeyValuePair<(string, string), List<PosInfo>>>)radDropDownList1.Items[e.Position].Tag)
                .Values;
            foreach (var radListDataItem in dataItem.Select(keyValuePair => new RadListDataItem
            {
                Text = keyValuePair.Key.Item1,
                Tag = keyValuePair.Value
            }))
            {
                radDropDownList2.Items.Add(radListDataItem);
            }
            
            
        }
        private Point GetRelativePoint(object sender)
        {
            var button = (BunifuImageButton)sender;
            return new Point(Center().X + button.Location.X + 120, Center().Y + button.Location.Y + button.Size.Height / 2);
        }

        private void bunifuImageButton2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || currObject != null) return;
            CreateBigButton(Resources.effective5, typeof(Recuperator), 0, 1, ((dynamic)sender).Tag, sender);
            Cursor.Position = GetRelativePoint(sender);


        }

        private void bunifuImageButton5_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || currObject != null) return;
            CreateBigButton(Resources.waterheater1, typeof(WaterHeater), 0, 1, ((dynamic)sender).Tag, sender);
            Cursor.Position = GetRelativePoint(sender);
        }

        private void bunifuImageButton6_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || currObject != null) return;
            CreateButton(Resources.electroheater, typeof(ElectroHeater), ((dynamic)sender).Tag, sender);
            Cursor.Position = GetRelativePoint(sender);

        }

        private void bunifuImageButton7_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || currObject != null) return;
            CreateBigButton(Resources.froster_big, typeof(Froster), 0, 1, ((dynamic)sender).Tag, sender);
            Cursor.Position = GetRelativePoint(sender);

        }

        private void bunifuImageButton8_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || currObject != null) return;
            CreateButton(Resources.Humidifier, typeof(Humidifier), ((dynamic)sender).Tag, sender);

            Cursor.Position = GetRelativePoint(sender);

        }

        private void bunifuImageButton4_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || currObject != null) return;
            CreateButton(Resources.air_filter, typeof(Filtr), ((dynamic)sender).Tag, sender);
            Cursor.Position = GetRelativePoint(sender);
        }
        

        private void radButton2_Click(object sender, EventArgs e)
        {

            var controlsWithPosInfo = Controls
                .Cast<Control>()
                .Where(c => c.Tag is PosInfo)
                .ToList();
            foreach (var control in controlsWithPosInfo)
            {
                Controls.Remove(control);
            }
            OccupDict.Clear();
            SupplyVentPresent = false;
            ExtVentPresent = false;

            try
            {
                RadListDataItem dataItem = radDropDownList2.SelectedItem;

                if (dataItem == null) return;
                List<PosInfo> posInfos = (List<PosInfo>)dataItem.Tag;
                SupplyVentPresent = posInfos
                    .Count(re => re.Tag is SupplyVent || re.Tag is SpareSuplyVent) > 0;
                ExtVentPresent = posInfos
                    .Count(re => re.Tag is ExtVent|| re.Tag is SpareExtVent) > 0;

                var rm = Resources.ResourceManager;
                foreach (var posInfo in posInfos)
                {
                    var additionalHeight = 0;
                    var additionalWidth = 0;
                    if (posInfo.SizeY == 1)
                    {
                        additionalHeight = 10;
                        additionalWidth = 2;
                    }

                    var button = new DoubleBufferedBunifuImageButton
                    {
                        Image = (Bitmap)rm.GetObject(posInfo.ImageName),
                        Width = 50 + (50 * posInfo.SizeX) + additionalWidth,
                        Height = 50 + (50 * posInfo.SizeY) + additionalHeight,
                        BackgroundImageLayout = ImageLayout.Stretch,
                        BackColor = Color.Transparent,
                        Location = CreatePointFromCell(posInfo.Pos),
                        Padding = new Padding(3),
                        InitialImage = (Bitmap)rm.GetObject(posInfo.ImageName),
                        SizeMode = PictureBoxSizeMode.StretchImage,

                        Tag = posInfo,

                    };

                    button.Zoom = posInfo.SizeY > 0 ? 5 : 15;

                    OccupDict.Add(MD5HashGenerator.GenerateKey(posInfo.Pos), posInfo);
                    if (posInfo.SizeY > 0)
                    {
                        for (int i = 0; i <= posInfo.SizeY; i++)
                        {
                            string genGey = MD5HashGenerator.GenerateKey(new[] {posInfo.PozX, posInfo.PozY + i});
                            if (!OccupDict.ContainsKey(genGey)) OccupDict.Add(genGey, posInfo);
                        }
                    }
                    button.Click += button_Click;
                    Controls.Add(button);
                    Controls.SetChildIndex(button, 0);
                }
            }
            catch 
            {
                MessageBox.Show("Вероятно, не выбран тип установки");
            }
            
            
        }
        internal static class WritePrestToXML
        {
            internal static void Write(Control.ControlCollection controls)
            {
                var controlsWithPosInfo = controls
                .Cast<Control>()
                .Where(c => c.Tag is PosInfo)
                .Select(c => (PosInfo)c.Tag)
                .ToList()
                .OrderBy(x => x.PozX)
                .ToList()
                .OrderBy(y => y.PozY)
                .ToList();
                //xml Decalration:
                XmlDocument xmlDoc = new XmlDocument();
                XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);

                XmlElement root = xmlDoc.CreateElement("Preset");
                XmlAttribute rootAttribute = xmlDoc.CreateAttribute("presetName");
                rootAttribute.Value = "AutoPreset";
                root.Attributes.Append(rootAttribute);

                xmlDoc.AppendChild(root);
                xmlDoc.InsertBefore(xmlDeclaration, root);

                // add body
                XmlElement body = xmlDoc.CreateElement(string.Empty, "Compontents", string.Empty);
                xmlDoc.DocumentElement?.AppendChild(body);
                foreach (PosInfo posInfo in controlsWithPosInfo)
                {
                    object component = posInfo.Tag;
                    List<(string, string)> compList = null;
                    switch (component.GetType().Name)
                    {
                        case nameof(SupplyVent):
                            SupplyVent supplyVent = (SupplyVent)component;

                            {
                                var comp1 = ("Voltage", supplyVent.Voltage.ToString());
                                var comp2 = ("Power", supplyVent.Power);
                                var comp3 = ("Location", supplyVent.Location);
                                var comp4 = ("Description", supplyVent.Description);
                                var comp5 = ("ControlType", supplyVent.ControlType.ToString());
                                var comp6 = ("Protect", supplyVent.Protect.ToString());
                                compList = new List<(string, string)>
                        {
                            comp1,
                            comp2,
                            comp3,
                            comp4,
                            comp5,
                            comp6
                        };
                            }

                            break;
                        case nameof(ExtVent):
                            ExtVent extVent = (ExtVent)component;
                            {
                                var comp1 = ("Voltage", extVent.Voltage.ToString());
                                var comp2 = ("Power", extVent.Power);
                                var comp3 = ("Location", extVent.Location);
                                var comp4 = ("Description", extVent.Description);
                                var comp5 = ("ControlType", extVent.ControlType.ToString());
                                var comp6 = ("Protect", extVent.Protect.ToString());
                                compList = new List<(string, string)>
                        {
                            comp1,
                            comp2,
                            comp3,
                            comp4,
                            comp5,
                            comp6
                        };
                            }

                            break;
                        case nameof(SupplyFiltr):
                            SupplyFiltr supplyFiltr = (SupplyFiltr)component;
                            {
                                var comp1 = ("Description", supplyFiltr.Description);
                                var comp2 = ("ControlType", supplyFiltr.PressureProtect.ToString());
                                compList = new List<(string, string)>
                        {
                            comp1,
                            comp2
                        };
                            }

                            break;
                        case nameof(ExtFiltr):
                            ExtFiltr extFiltr = (ExtFiltr)component;
                            {
                                var comp1 = ("Description", extFiltr.Description);
                                var comp2 = ("ControlType", extFiltr.PressureProtect.ToString());
                                compList = new List<(string, string)>
                        {
                            comp1,
                            comp2
                        };
                            }
                            break;
                        case nameof(Filtr):
                            Filtr filtr = (Filtr)component;
                            {

                                var comp1 = ("Description", filtr.Description);
                                var comp2 = ("ControlType", filtr.PressureProtect.ToString());
                                compList = new List<(string, string)>
                        {
                            comp1,
                            comp2
                        };
                            }

                            break;
                        case nameof(SupplyDamper):
                            SupplyDamper supplyDamper = (SupplyDamper)component;
                            {
                                var comp1 = ("Description", supplyDamper.Description);
                                var comp2 = ("HasControl", supplyDamper.hascontrol.ToString());
                                var comp3 = ("ControlType", supplyDamper.SensorType.ToString());
                                compList = new List<(string, string)>
                        {
                            comp1,
                            comp2,
                            comp3
                        };
                            }
                            break;
                        case nameof(ExtDamper):
                            ExtDamper extDamper = (ExtDamper)component;
                            {
                                var comp1 = ("Description", extDamper.Description);
                                var comp2 = ("HasControl", extDamper.HasContol.ToString());
                                compList = new List<(string, string)>
                        {
                            comp1,
                            comp2,
                        };
                            }
                            break;
                        case nameof(ElectroHeater):
                            ElectroHeater electroHeater = (ElectroHeater)component;
                            {
                                var comp1 = ("Voltage", electroHeater.Voltage.ToString());
                                var comp2 = ("Power", electroHeater.Power);
                                var comp3 = ("Description", electroHeater.Description);
                                var comp4 = ("Stairs", electroHeater.Stairs.ToString());
                                compList = new List<(string, string)>
                        {
                            comp1,
                            comp2,
                            comp3,
                            comp4
                        };
                            }
                            break;
                        case nameof(WaterHeater):
                            WaterHeater waterHeater = (WaterHeater)component;
                            {
                                var comp1 = ("ValveType", waterHeater._valveType.ToString());
                                var comp2 = ("PumpTK", waterHeater.HasTK.ToString());
                                var comp3 = ("WaterProtect", waterHeater.Waterprotect.ToString());
                                compList = new List<(string, string)>
                        {
                            comp1,
                            comp2,
                            comp3
                        };
                            }
                            break;
                        case nameof(Humidifier):
                            Humidifier humidifier = (Humidifier)component;
                            {
                                var comp1 = ("HumType", humidifier.HumType.ToString());
                                var comp2 = ("ControlType", humidifier.HumControlType.ToString());
                                var comp3 = ("Voltage", humidifier.Voltage.ToString());
                                var comp4 = ("Power", humidifier.Power);
                                var comp5 = ("HumSensPresent", humidifier.HumSensPresent.ToString());
                                var comp6 = ("SensorType", humidifier.SensorType.ToString());
                                compList = new List<(string, string)>
                        {
                            comp1,
                            comp2,
                            comp3,
                            comp4,
                            comp5,
                            comp6
                        };
                            }
                            break;
                        case nameof(Froster):
                            Froster froster = (Froster)component;
                            {
                                var comp1 = ("FrosterType", froster._FrosterType.ToString());
                                var comp2 = ("Stairs", froster.Stairs.ToString());
                                var comp3 = ("KKBControlType", froster.KKBControlType.ToString());
                                var comp4 = ("ValveType", froster.valveType.ToString());
                                var comp5 = ("Voltage", froster.Voltage.ToString());
                                var comp6 = ("Power", froster.Power);
                                compList = new List<(string, string)>
                        {
                            comp1,
                            comp2,
                            comp3,
                            comp4,
                            comp5,
                            comp6
                        };
                            }
                            break;
                        case nameof(Recuperator):

                            {
                                Recuperator recuperator = (Recuperator)component;
                                var comp1 = ("RecuperatorType", recuperator._RecuperatorType.ToString());
                                compList = new List<(string, string)>
                        {
                            comp1
                        };
                            }
                            break;
                        case nameof(CrossSection):
                            CrossSection crossSection = (CrossSection)component;
                            {
                                var comp1 = ("SensorTType", crossSection.sensorTType.ToString());
                                var comp2 = ("SensorHType", crossSection.sensorHType.ToString());
                                compList = new List<(string, string)>
                        {
                            comp1,
                            comp2
                        };
                            }
                            break;
                        case nameof(Room):
                            Room room = (Room)component;
                            {
                                var comp1 = ("SensorTType", room.sensorTType.ToString());
                                var comp2 = ("SensorHType", room.sensorHType.ToString());
                                compList = new List<(string, string)>
                        {
                            comp1,
                            comp2
                        };
                            }
                            break;
                    }

                    XmlNode itemNode = xmlDoc.CreateElement("Item");
                    XmlAttribute attr = xmlDoc.CreateAttribute("itemName");
                    attr.Value = component.GetType().Name;
                    itemNode.Attributes.SetNamedItem(attr);
                    XmlNode paramNode = xmlDoc.CreateElement("Parameters");
                    XmlNode posInfoNode = xmlDoc.CreateElement("PosInfo");
                    if (compList != null)
                    {
                        //Parameters node
                        foreach (var paramsTuple in compList)
                        {
                            XmlNode parametr = xmlDoc.CreateElement(paramsTuple.Item1);
                            parametr.InnerText = paramsTuple.Item2;
                            paramNode.AppendChild(parametr);
                        }

                        //PosInfo Parameters Nodes
                        XmlNode posinfo1 = xmlDoc.CreateElement("Pos");
                        posinfo1.InnerText = $"{posInfo.PozX},{posInfo.PozY}";
                        XmlNode posinfo2 = xmlDoc.CreateElement("Size");
                        posinfo2.InnerText = $"{posInfo.SizeX},{posInfo.SizeY}";
                        XmlNode posinfo3 = xmlDoc.CreateElement("Image");
                        posinfo3.InnerText = $"{posInfo.ImageName}";
                        //Add PosInfo Params into PosinfoNode
                        posInfoNode.AppendChild(posinfo1);
                        posInfoNode.AppendChild(posinfo2);
                        posInfoNode.AppendChild(posinfo3);
                        //Add ParamNode with params to Item node
                        itemNode.AppendChild(paramNode);
                        //Add PosInfoNode with params to Item node
                        itemNode.AppendChild(posInfoNode);
                        //Add itemNode to body Node
                        body.AppendChild(itemNode);
                    }

                }

                string path = @"%AppData%";
                path = Environment.ExpandEnvironmentVariables(path);
                path += @"\Autodesk\Revit\Addins\2021\GKSASU\AOVGen\";
                xmlDoc.Save(path + @"shema.xml");
                


            }
        }

        private void label2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            WritePrestToXML.Write(Controls);
            
        }

        
    }
}
