﻿
using Bunifu.UI.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using Telerik.WinControls.UI;

namespace AOVGEN
{
    public partial class EditorV2 : Telerik.WinControls.UI.RadForm
    {
        #region Global variables
        object currObject = null;
        List<object> currObjects = null;
        Image lastSelectedImage = null;
        PosInfo lastPosInfo = null;
        object lastComponent = null;
        readonly Dictionary<string, PosInfo> OccupDict = new Dictionary<string, PosInfo>();
        readonly int[] mymas = new int[2];
        readonly int[] sizeMas = new int[2];
        string hash;
        readonly PosInfo GlobalPosInfo = new PosInfo();
        bool present;
        
        bool isDrag = false;
        Rectangle theRectangle = new Rectangle(new Point(0, 0), new Size(0, 0));
        Point startPoint;
        
        #endregion
        #region User functions
        internal static Point CreatePointFromCell(int[] cell)
        {
            return new Point((cell[0] * 60) + 185, (cell[1] * 60) + 185);
        }
        private void GenerateCells()
        {
            var cellSize = 60;
            for (var t = 0; t <= 15; t++)
            {
                var pictureBox = new PictureBox
                {
                    Location = new Point(180 + t * cellSize, 180),
                    Size = new Size(1, 300),
                    BackColor = Color.Black
                };
                this.Controls.Add(pictureBox);
                switch (t)
                {
                    case 9:
                    {
                        var A4x3 = new Label
                        {
                            Location = new Point(200 + t * cellSize, 160),
                            AutoSize = true,
                            Text = "A4x3"
                        };
                        A4x3.SendToBack();
                        this.Controls.Add(A4x3);
                        break;
                    }
                    case 13:
                    {
                        var A4x4 = new Label
                        {
                            Location = new Point(200 + t * cellSize, 160),
                            AutoSize = true,
                            Text = "A4x4"
                        };
                        this.Controls.Add(A4x4);
                        break;
                    }
                    case 15:
                    {
                        var A3x3 = new Label
                        {
                            Location = new Point(140 + t * cellSize, 160),
                            AutoSize = true,
                            Text = "A3x3"
                        };
                        this.Controls.Add(A3x3);
                        break;
                    }
                }
            }
            for (var t = 0; t <= 5; t++)
            {
                var pictureBox = new PictureBox
                {
                    Location = new Point(180, 180 + t * cellSize),
                    Size = new Size(900, 1),
                    BackColor = Color.Red
                };
                this.Controls.Add(pictureBox);
                if (t == 1)
                {
                    var A4 = new Label
                    {
                        Location = new Point(160, 210 + t * cellSize),
                        AutoSize = true,
                        Text = "A4"
                    };
                    this.Controls.Add(A4);
                }
                if (t == 5)
                {
                    var A3 = new Label
                    {
                        Location = new Point(160, 150 + t * cellSize),
                        AutoSize = true,
                        Text = "A3"
                    };
                    this.Controls.Add(A3);
                }
            }
        }
        private int[] GetCellPos()
        {
            //correct position in mattrix
            if (currObject != null)
            {
                //DoubleBufferedPictureBox pictureBox = (DoubleBufferedPictureBox)currObject;
                if (currObject is Bunifu.Framework.UI.BunifuImageButton button)
                {
                    
                    PosInfo posInfo = (PosInfo)button.Tag;
                    bool bigout = posInfo.SizeX > 0 & posInfo.PozX == 14 & posInfo.PozX < 50 & posInfo.SizeY > 0 & posInfo.PozY == 4;
                    if (Cursor.Position.X > 389 && Cursor.Position.Y >= 390 && Cursor.Position.Y <= 675 && Cursor.Position.X < 1281 && !bigout)
                    {
                        var oldLocation = button.Location;
                        int mod1 = Cursor.Position.X % 60;
                        int mod2 = Cursor.Position.Y % 60;
                        int x;
                        int y;
                        if (mod1 < 30)
                        {
                            x = Cursor.Position.X - mod1;
                        }
                        else
                        {
                            x = Cursor.Position.X + 60 - mod1;
                        }

                        if (mod2 < 30)
                        {
                            y = (Cursor.Position.Y - 60) - mod2;
                        }
                        else
                        {
                            y = Cursor.Position.Y - mod2;
                        }

                        int CellX = (x - 400) / 60;
                        int CellY = (y - 360) / 60;

                        if ((posInfo.SizeY > 0 & CellY == 4) || (posInfo.SizeX > 0 & CellX == 14)) //if big object on edge of mattrix
                        {
                            OutOfMattrix(posInfo, button);
                            return new int[] { Cursor.Position.X + 20, Cursor.Position.Y - 50 };
                        }

                        mymas[0] = CellX; //calculated CellX
                        mymas[1] = CellY;//calculated CellY
                        int sizeX = posInfo.SizeX; //Object SizeX
                        int sizeY = posInfo.SizeY; //Object SizeY
                        bool occupied; //set occupied cell to false
                        if (currObject != null && CellX <= 15 && CellX >= 0)
                        {
                           
                            occupied = OccupDict.ContainsKey(MD5HashGenerator.GenerateKey(mymas));
                            if (sizeX > 0 || sizeY > 0)
                            {
                                for (int Y1 = 0; Y1 <= sizeY; Y1++)
                                {
                                    if (occupied) break;
                                    for (int X1 = 0; X1 <= sizeX; X1++)
                                    {
                                        sizeMas[0] = CellX + X1;
                                        sizeMas[1] = CellY + Y1;
                                        occupied = OccupDict.ContainsKey(MD5HashGenerator.GenerateKey(sizeMas));
                                        if (occupied)
                                        {
                                            mymas[0] = sizeMas[0];
                                            mymas[1] = sizeMas[1];
                                            break;
                                        }
                                    }

                                }
                            }
                            if (occupied)
                            {
                                //совпадает позиция                            
                                present = true;
                                return new int[] { oldLocation.X, oldLocation.Y};
                            }
                            else
                            {
                                present = false;
                                GlobalPosInfo.Pos = mymas;
                                posInfo.Pos = new int[] { CellX, CellY };
                                posInfo.Pos[0] = CellX;
                                posInfo.Pos[1] = CellY;
                                button.BackColor = Color.FromArgb(250, 4, 243, 100);
                            }
                            return new int[] { x, y };
                        }
                    }
                    //out of mattrix
                    OutOfMattrix(posInfo, button);
                }

            }
            return new int[] { Cursor.Position.X + 20, Cursor.Position.Y - 50 };
            void OutOfMattrix(PosInfo posInfo, Bunifu.Framework.UI.BunifuImageButton pictureBox)
            {
                posInfo.Pos[0] = 50;
                posInfo.Pos[1] = 50;
                pictureBox.BackColor = Color.Red;
                
            }
        }
        private int[] GetCellPos(object s)
        {
            //correct position in mattrix
            Point pursor = this.PointToClient(Cursor.Position);
            if (currObject != null)
            {
                
                if (currObject is Bunifu.Framework.UI.BunifuImageButton button)
                {
                    PosInfo posInfo = (PosInfo)button.Tag;
                    bool bigout = posInfo.SizeX > 0 & posInfo.PozX == 14 & posInfo.PozX < 50 & posInfo.SizeY > 0 & posInfo.PozY == 4;
                    if (pursor.X >= 180 && pursor.Y >= 180 && pursor.Y <= 480 && pursor.X < 1080 && !bigout)
                    {

                        int mod1 = pursor.X % 60;
                        int mod2 = pursor.Y % 60;
                        int x;
                        int y;
                        if (mod1 < 60)
                        {
                            x = pursor.X - mod1;
                        }
                        else
                        {
                            x = pursor.X + 60 - mod1;
                        }

                        if (mod2 < 60)
                        {
                            y = (pursor.Y - mod2) ;
                        }
                        else
                        {
                            y = pursor.Y - mod2 + 60;
                        }
                        
                        var oldLocation = button.Location;
                        int CellX = (x - 180) / 60;
                        int CellY = (y - 180) / 60;
                        
                        
                        
                       

                        if ((posInfo.SizeY > 0 & CellY == 4) || (posInfo.SizeX > 0 & CellX == 14) || (CellY >= 5) || (CellX >= 15))  //if big object on edge of mattrix
                        {
                            OutOfMattrix(posInfo, button);
                            return new int[] { pursor.X, pursor.Y };
                        }
                        

                        mymas[0] = CellX; //calculated CellX
                        mymas[1] = CellY;//calculated CellY
                        int sizeX = posInfo.SizeX; //Object SizeX
                        int sizeY = posInfo.SizeY; //Object SizeY
                        bool occupied; //set occupied cell to false
                        if (currObject != null && CellX <= 15 && CellX >= 0)
                        {

                            occupied = OccupDict.ContainsKey(MD5HashGenerator.GenerateKey(mymas));
                            if (sizeX > 0 || sizeY > 0)
                            {
                                for (int Y1 = 0; Y1 <= sizeY; Y1++)
                                {
                                    if (occupied) break;
                                    for (int X1 = 0; X1 <= sizeX; X1++)
                                    {
                                        sizeMas[0] = CellX + X1;
                                        sizeMas[1] = CellY + Y1;
                                        occupied = OccupDict.ContainsKey(MD5HashGenerator.GenerateKey(sizeMas));
                                        if (occupied)
                                        {
                                            mymas[0] = sizeMas[0];
                                            mymas[1] = sizeMas[1];
                                            break;
                                        }
                                    }

                                }
                            }
                            if (occupied)
                            {
                                //совпадает позиция                            
                                present = true;
                                return new int[] { oldLocation.X, oldLocation.Y};
                            }
                            else
                            {
                                present = false;
                                GlobalPosInfo.Pos = mymas;
                                posInfo.Pos = new int[] { CellX, CellY };
                                posInfo.Pos[0] = CellX;
                                posInfo.Pos[1] = CellY;
                                button.BackColor = Color.FromArgb(250, 4, 243, 100);
                            }

                            int kX =0, KY = 0;
                            switch (posInfo.SizeY)
                            {
                                case 0:
                                    kX = (60 - button.Width) / 2;
                                    KY = (60 - button.Height) / 2;
                                    break;
                                case 1:
                                    kX = (60 - button.Width) / 2;
                                    KY = (120 - button.Height) / 2;
                                    break;
                            }
                            return new int[] { x + kX , y + KY };
                        }
                    }
                    //out of mattrix
                    OutOfMattrix(posInfo, button);
                }

            }
            return new int[] { pursor.X , pursor.Y  };
            void OutOfMattrix(PosInfo posInfo, Bunifu.Framework.UI.BunifuImageButton pictureBox)
            {
                posInfo.Pos[0] = 50;
                posInfo.Pos[1] = 50;
                pictureBox.BackColor = Color.Red;

            }
        }





        private void CreateButton(Image image)
        {
            lastSelectedImage = image;
            DoubleBufferedBunifuImageButton button = new DoubleBufferedBunifuImageButton
            {
                Image = image,
                Width = 50,
                Height = 50,
                BackgroundImageLayout = ImageLayout.Stretch,
                BackColor = Color.Red,
                Location = new Point(Cursor.Position.X - 140, Cursor.Position.Y - 200),
                Padding = new Padding(10),
                InitialImage = image,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Zoom = 10


            };

            PosInfo posInfo = new PosInfo();
            lastPosInfo = posInfo;

            button.Tag = posInfo;
            button.Click += button_Click;
            
            Controls.Add(button);
            this.Controls.SetChildIndex(button, 0);
            currObject = button;
            //Cursor.Position = new Point(Cursor.Position.X+50, Cursor.Position.Y );
        }
        private void CreateButton(Image image, Type type, dynamic ImageName )
        {
            lastSelectedImage = image;
            
            DoubleBufferedBunifuImageButton button = new DoubleBufferedBunifuImageButton
            {
                Image = image,
                Size = new Size(50, 50),
                
                BackgroundImageLayout = ImageLayout.Stretch,
                BackColor = Color.Red,
                Location = new Point(Cursor.Position.X, Cursor.Position.Y),
                Padding = new Padding(3),
                InitialImage = image,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Zoom= 15
            };
            button.LocationChanged += Button_LocationChanged;
            object comp = CreateComp(type, image);
            PosInfo posInfo = new PosInfo
            {
                Tag = comp,
                ImageName = ImageName.ToString()
            };
            lastPosInfo = posInfo;
            switch (comp.GetType().Name)
            {
                case nameof(SupplyVent):
                    SupplyVentPresent = true;
                    lastComponent = null;
                    lastPosInfo = null;
                    break;
                case nameof(ExtVent):
                    ExtVentPresent = true;
                    lastComponent = null;
                    lastPosInfo = null;
                    break;
                case nameof(SpareSuplyVent):
                    SupplyVentSparePresent = true;
                    lastComponent = null;
                    lastPosInfo = null;
                    break;
                case nameof(SpareExtVent):
                    ExtVentSparePresent = true;
                    lastComponent = null;
                    lastPosInfo = null;
                    break;
                default:
                    lastComponent = comp;
                    break;
            }
           
            
            button.Tag = posInfo;
            button.Click += button_Click;
            Controls.Add(button);
            this.Controls.SetChildIndex(button, 0);
            currObject = button;
           
        }
        private void CreateButton(Image image, Type type, dynamic ImageName, object sender)
        {
            lastSelectedImage = image;
            BunifuImageButton bunifuImageButton = (BunifuImageButton)sender;
            DoubleBufferedBunifuImageButton button = new DoubleBufferedBunifuImageButton
            {
                Image = image,
                Width = 50,
                Height = 50,
                BackgroundImageLayout = ImageLayout.Stretch,
                BackColor = Color.Red,
                Location = new Point(bunifuImageButton.Location.X + 100, bunifuImageButton.Location.Y),
                Padding = new Padding(3),
                InitialImage = image,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Zoom = 15
            };
            button.LocationChanged += Button_LocationChanged;
            object comp = CreateComp(type, image);
            PosInfo posInfo = new PosInfo
            {
                Tag = comp,
                ImageName = ImageName.ToString()
            };
            lastPosInfo = posInfo;
            lastComponent = comp;

            button.Tag = posInfo;
            button.Click += button_Click;
            Controls.Add(button);
            this.Controls.SetChildIndex(button, 0);
            currObject = button;
            //return button;

        }

        private void Button_LocationChanged(object sender, EventArgs e)
        {
            var button = (DoubleBufferedBunifuImageButton)sender;
            

        }
        private void CreateBigButton(Image image, Type type, int x, int y, dynamic ImageName)
        {
            lastSelectedImage = image;
            var button = new DoubleBufferedBunifuImageButton
            {
                Image = image,
                Width = 52,
                Height = 110,
                BackgroundImageLayout = ImageLayout.Stretch,
                BackColor = Color.Red,
                Location = new Point(Cursor.Position.X - 140, Cursor.Position.Y - 200),
                Padding = new Padding(5),
                InitialImage = image,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Zoom = 5

            };
            object comp = CreateComp(type, image);
            PosInfo posInfo = new PosInfo
            {
                SizeX = x,
                SizeY = y,
                Tag = comp,
                ImageName = ImageName
            };
            lastPosInfo = posInfo;
            
            
            button.Tag = posInfo;
            button.Click += button_Click;
            Controls.Add(button);
            this.Controls.SetChildIndex(button, 0);
            currObject = button;
            lastComponent = comp;

        }
        private void CreateBigButton(Image image, Type type, int x, int y, dynamic ImageName, object sender)
        {
            lastSelectedImage = image;
            BunifuImageButton imageButton = (BunifuImageButton)sender;
            DoubleBufferedBunifuImageButton button = new DoubleBufferedBunifuImageButton
            {
                Image = image,
                Width = 50,
                Height = 110,
                BackgroundImageLayout = ImageLayout.Stretch,
                BackColor = Color.Red,
                Location = new Point(imageButton.Location.X + 100, imageButton.Location.Y),
                Padding = new Padding(5),
                InitialImage = image,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Zoom = 5

            };
            object comp = CreateComp(type, image);
            PosInfo posInfo = new PosInfo
            {
                SizeX = x,
                SizeY = y,
                Tag = comp,
                ImageName = ImageName
            };
            lastPosInfo = posInfo;


            button.Tag = posInfo;
            button.Click += button_Click;
            Controls.Add(button);
            this.Controls.SetChildIndex(button, 0);
            currObject = button;
            lastComponent = comp;

        }
        private object CreateComp(Type type, Image image)
        {
            object comp = null;
            bool equalObjects(object obj1, object obj2) => MD5HashGenerator.GenerateKey(obj1) == MD5HashGenerator.GenerateKey(obj2);
            string GetRoomImageName()
            {

                if (equalObjects(AOVGEN.Properties.Resources.room__arrow_supp_exh_T, image)) return "room__arrow_supp_exh_T";
                if (equalObjects(AOVGEN.Properties.Resources.room__arrow_supp_exh_T_big, image)) return "room__arrow_supp_exh_T_big";
                if (equalObjects(AOVGEN.Properties.Resources.room__arrow_supply_T, image)) return "room__arrow_supply_T";
                if (equalObjects(AOVGEN.Properties.Resources.room__arrow_supp_exh_TH, image)) return "room__arrow_supp_exh_TH";
                if (equalObjects(AOVGEN.Properties.Resources.room__arrow_supp_exh_TH_big, image)) return "room__arrow_supp_exh_TH_big";
                if (equalObjects(AOVGEN.Properties.Resources.room__arrow_supply_TH, image)) return "room__arrow_supply_TH";
                return string.Empty;
            }
            
            switch (type.Name)
            {
                case nameof(OutdoorTemp):
                    break;
                case nameof(SupplyTemp):
                    break;
                case nameof(ExhaustTemp):
                    break;
                case nameof(IndoorTemp):
                    break;
                case nameof(WaterHeater):
                    comp = new WaterHeater();
                    break;
                case nameof(ElectroHeater):
                    comp = new ElectroHeater();
                    break;
                case nameof(Recuperator):
                    comp = new Recuperator();
                    break;
                case nameof(SupplyFiltr):
                    comp = new SupplyFiltr();
                    break;
                case nameof(ExtFiltr):
                    comp = new ExtFiltr();
                    break;
                case nameof(Filtr):
                    comp = new Filtr()
                    {
                        PressureProtect = Sensor.SensorType.Discrete
                    };
                    break;
                case nameof(SupplyVent):
                    comp = new SupplyVent();
                    break;
                case nameof(ExtVent):
                    comp = new ExtVent();
                    break;
                case nameof(SpareSuplyVent):
                    comp = new SpareSuplyVent();
                    
                    break;
                case nameof(SpareExtVent):
                    comp = new SpareExtVent();
                    break;

                case nameof(Humidifier):
                    comp = new Humidifier();
                    break;
                case nameof(Froster):
                    comp = new Froster(Froster.FrosterType.Water);
                    break;
                case nameof(CrossSection):
                    {
                        bool isT = equalObjects(AOVGEN.Properties.Resources.cross1T, image);
                        bool isTH = equalObjects(AOVGEN.Properties.Resources.cross1TH, image);
                        if (isTH) comp = new CrossSection(true, true);
                        if (isT) comp = new CrossSection(true, false);
                        comp = comp ?? new CrossSection(false, false);
                    }
                    break;
                case nameof(Room):
                    {
                        bool isT = 
                            equalObjects(AOVGEN.Properties.Resources.room__arrow_supp_exh_T, image)||
                            equalObjects(AOVGEN.Properties.Resources.room__arrow_supp_exh_T_big, image)||
                            equalObjects(AOVGEN.Properties.Resources.room__arrow_supply_T, image);
                        bool isTH =
                            equalObjects(AOVGEN.Properties.Resources.room__arrow_supp_exh_TH, image) ||
                            equalObjects(AOVGEN.Properties.Resources.room__arrow_supp_exh_TH_big, image) ||
                            equalObjects(AOVGEN.Properties.Resources.room__arrow_supply_TH, image);
                        if (isT) comp = new Room(true, false, GetRoomImageName());
                        if (isTH) comp = new Room(true, true, GetRoomImageName());
                        comp = comp ?? new Room(false, false);
                    }
                    break;

                case nameof(SupplyDamper):

                    if (MD5HashGenerator.GenerateKey(image) == MD5HashGenerator.GenerateKey(AOVGEN.Properties.Resources.shutter_right_T))
                    {
                        SupplyDamper supplyDamper = new SupplyDamper()
                        {
                            SetSensor = true
                        };
                        comp = supplyDamper;

                    }
                    else
                    {
                        comp = new SupplyDamper();
                    }
                    break;
                case nameof(ExtDamper):
                    comp = new ExtDamper();
                    break;

            }
            return comp;
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
        private void DeleteVentSystem(string ventsystemGUID)
        {
            SQLiteConnection connection = OpenDB();
            connection.Open();
            if (connection.State == ConnectionState.Open)
            {
                SQLiteCommand command = new SQLiteCommand()
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
                    $"DELETE FROM FControl WHERE [SystemGUID] = '{ventsystemGUID}'",
                    $"DELETE FROM FControl WHERE [SystemGUID] = '{ventsystemGUID}'",
                    $"DELETE FROM PosInfo WHERE [SystemGUID] = '{ventsystemGUID}'",
                    $"DELETE FROM Room WHERE [SystemGUID] = '{ventsystemGUID}'",
                    $"DELETE FROM CrossConnection WHERE [SystemGUID] = '{ventsystemGUID}'",
                    $"DELETE FROM SpareVentilator WHERE [SystemGUID] = '{ventsystemGUID}'",
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
                        SQLiteCommand command = new SQLiteCommand()
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
                        SQLiteCommand command = new SQLiteCommand()
                        {
                            Connection = connection
                        };
                        List<string> updatepannelquery = new List<string>
                        {
                            $"UPDATE VentSystems SET Pannel = '{pannelguid}' WHERE [GUID] = '{newventsystemGUID}'",
                            $"UPDATE VentSystems SET PannelName = '{pannelname}' WHERE [GUID] = '{newventsystemGUID}'"

                        };
                        foreach (string query in updatepannelquery)
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
                string query;
                SQLiteCommand command = new SQLiteCommand
                {
                    Connection = connection,
                    CommandText = $"SELECT [To], COUNT(*) AS ToCount FROM Cable WHERE SystemGUID = '{systemguid}' GROUP BY [To]"
                };
                if (connection.State == ConnectionState.Closed) connection.Open();
                SQLiteDataReader dataReader = command.ExecuteReader();
                DataTable dataTable = new DataTable();
                dataTable.Load(dataReader);
                dataReader.Close();
                StringBuilder Pos;
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    Pos = new StringBuilder(dataRow["To"].ToString());
                    int.TryParse(dataRow["ToCount"].ToString(), out int cntstr);
                    if (cntstr <= 0) continue;
                    query = "SELECT [To], Cable.ToGUID, Cable.GUID, Cable.SortPriority, Cable.WriteBlock, Cable.TableForSearch FROM Cable " +
                            $"WHERE((([To]) = '{Pos}') AND((Cable.SystemGUID) = '{systemguid}')) " +
                            "ORDER BY Cable.SortPriority;";
                    command.CommandText = query;
                    SQLiteDataReader readerchild = command.ExecuteReader();
                    DataTable dataTableChild = new DataTable();
                    dataTableChild.Load(readerchild);
                    readerchild.Close();
                    if (dataTableChild.Rows.Count <= 0) continue;
                    for (int t1 = 0, j1 = 1; t1 <= dataTableChild.Rows.Count - 1; t1++, j1++)
                    {
                        {
                            DataRow row = dataTableChild.Rows[t1];
                            Posnames posnames = new Posnames();
                            bool writeblock = bool.Parse(row["WriteBlock"].ToString());
                            string hosttable = row["TableForSearch"].ToString();
                            string posguid = row["ToGUID"].ToString();
                            string newposname = AllConnectedSumm + devider + Pos + j1;
                            posnames.Newposname = newposname;
                            if (writeblock)
                            {
                                string query2 = $"UPDATE {hosttable} SET PosName = '{posnames.Newposname}' WHERE GUID = '{posguid}'";
                                command.CommandText = query2;
                                command.ExecuteNonQuery();
                            }
                            else
                            {
                                string query3 = $"SELECT PosName FROM {hosttable} WHERE GUID = '{posguid}'";
                                command.CommandText = query3;
                                SQLiteDataReader readerchild2 = command.ExecuteReader();
                                while (readerchild2.Read())
                                {
                                    posnames.Newposname = readerchild2[0].ToString();
                                }
                                readerchild2.Close();
                                j1--;
                            }
                            string updateposquery = $"UPDATE Cable Set [To] = '{posnames.Newposname}' " +
                                                    $"WHERE Cable.ToGUID = '{posguid}'";// AND Cable.GUID = '{cableguid}'";
                            command.CommandText = updateposquery;
                            command.ExecuteNonQuery();
                        }
                    }
                }
                command.Dispose();
                

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message + "; \n" + ex.StackTrace);
            }


        }
        internal static string UpdatePannelPower(RadTreeNode PannelNode, string connectionstring)
        {
            Pannel pannel = (Pannel)PannelNode.Tag;
            double Power = PannelNode.Nodes
                .Where(e => e.Level == 1)
                .Select(e => e.Tag)
                .Cast<VentSystem>()
                .Select(e => e.ComponentsV2)
                .SelectMany(e => e).ToList()
                .Select(e => e.Tag)
                .Where(e => e is IPower)
                .Cast<IPower>()
                .Select(e =>
                {
                    double.TryParse(e.Power, out double result);
                    return result;
                })
                .ToArray()
                .Sum();
            SQLiteConnection connection = new SQLiteConnection(connectionstring);
            connection.Open();
            string UpdatePannelPowerQuery = $"UPDATE Pannel SET Power = '{Power}' WHERE [GUID] = '{pannel.GetGUID()}'";
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection,
                CommandText = UpdatePannelPowerQuery
            };

            //if (connection.State == ConnectionState.Closed) connection.Open();
            command.ExecuteNonQuery();
            command.Dispose();
            return Power.ToString();

        }
        
        private RadTreeNode FindNodeByName(object text, Telerik.WinControls.UI.RadTreeNodeCollection nodes)
        {

            foreach (RadTreeNode node in nodes)
            {

                if (node.Name.Equals(text))
                {
                    return node;
                }
                else
                {
                    RadTreeNode n = FindNodeByName(text, node.Nodes);
                    if (n != null)
                    {
                        return n;
                    }
                }


            }

            return null;
        }
        internal static Pannel._Voltage UpdatePannelVoltage(RadTreeNode PannelNode, string connectionstring)
        {
            Pannel pannel = (Pannel)PannelNode.Tag;
            Pannel._Voltage voltage = PannelNode.Nodes
                .Where(e => e.Level == 1)
                .Select(e => e.Tag)
                .Cast<VentSystem>()
                .Select(e => e.ComponentsV2).SelectMany(e => e)
                .ToList()
                .Select(e => e.Tag)
                .Where(e => e is IPower)
                .Cast<IPower>()
                .FirstOrDefault(e => e.Voltage == ElectroDevice._Voltage.AC380) != null ? Pannel._Voltage.AC380 : Pannel._Voltage.AC220;
                



            SQLiteConnection connection = new SQLiteConnection(connectionstring);
            connection.Open();
            string UpdatePannelPowerQuery = $"UPDATE Pannel SET Voltage = '{voltage}' WHERE [GUID] = '{pannel.GetGUID()}'";
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection,
                CommandText = UpdatePannelPowerQuery
            };

            //if (connection.State == ConnectionState.Closed) connection.Open();
            command.ExecuteNonQuery();
            command.Dispose();
            return voltage;


            //string findventsGUID = $"Select GUID FROM VentSystems WHERE Pannel = '{pannel.GetGUID()}'";
            //SQLiteCommand command = new SQLiteCommand
            //{
            //    Connection = connection,
            //    CommandText = findventsGUID
            //};
            //command.CommandText = findventsGUID;
            //List<string> VSGuids = new List<string>();
            //using (SQLiteDataReader reader = command.ExecuteReader())
            //{
            //    while (reader.Read())
            //    {
            //        if (reader[0].ToString() != string.Empty) VSGuids.Add(reader[0].ToString());
            //    }


            //}
            //if (VSGuids.Count > 0)
            //{
            //    Pannel._Voltage _Voltage = Pannel._Voltage.AC220;

            //    foreach (string GUID in VSGuids)
            //    {
            //        RadTreeNode VSnode = mainForm.FindNodeByName(GUID, pannelnode.Nodes);
            //        if (VSnode != null)
            //        {
            //            VentSystem vent = (VentSystem)VSnode.Tag;
            //            if (mainForm.GetVetSystemMaxVoltage(vent) == Pannel._Voltage.AC380)
            //            {
            //                _Voltage = Pannel._Voltage.AC380;
            //                break;
            //            }

            //        }
            //    }
            //    pannel.Voltage = _Voltage;

            //}
            //else
            //{
            //    pannel.Voltage = Pannel._Voltage.AC220;
            //}
            //string UpdatePannelVoltageQuery = $"UPDATE Pannel SET Power = '{pannel.Power}', Voltage = '{pannel.Voltage}' WHERE [GUID] = '{pannel.GetGUID()}'";
            //command.CommandText = UpdatePannelVoltageQuery;
            //command.ExecuteNonQuery();
            //command.Dispose();



        }
        public static Task<bool> CheckLongPress(BunifuImageButton element, int duration)
        {
            Timer timer = new Timer();

            TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
            timer.Interval = duration;


            void touchUpHandler(object sender, MouseEventArgs e)
            {
                timer.Stop();
                if (task.Task.Status == TaskStatus.Running)
                {
                    task.SetResult(false);
                }
            }

            element.MouseUp += touchUpHandler;

            timer.Tick += delegate
            {
                element.MouseUp -= touchUpHandler;
                timer.Stop();
                task.SetResult(true);
            };

            timer.Start();
            return task.Task;
        }
        public void Repeater(BunifuImageButton btn, int interval)
        {
            var timer = new Timer { Interval = interval };
            timer.Tick += (sender, e) => DoProgress();
            btn.MouseDown += (sender, e) => timer.Start();
            btn.MouseUp += (sender, e) => timer.Stop();
            btn.Disposed += (sender, e) =>
            {
                timer.Stop();
                timer.Dispose();
            };
            void DoProgress()
            {

            }
        }
        #endregion
        #region User Classes
        public class MD5HashGenerator
        {
            private static readonly Object locker = new Object();

            /// <summary>
            /// Generates a hashed - key for an instance of a class.
            /// The hash is a classic MD5 hash (e.g. BF20EB8D2C4901112179BF5D242D996B). So you can distinguish different 
            /// instances of a class. Because the object is hashed on the internal state, you can also hash it, then send it to
            /// someone in a serialized way. Your client can then deserialize it and check if it is in
            /// the same state.
            /// The method just just estimates that the object implements the ISerializable interface. What's
            /// needed to save the state or so, is up to the implementer of the interface.
            /// <b>The method is thread-safe!</b>
            /// </summary>
            /// <param name="sourceObject">The object you'd like to have a key out of it.</param>
            /// <returns>An string representing a MD5 Hashkey corresponding to the object or null if the object couldn't be serialized.</returns>
            /// <exception cref="ApplicationException">Will be thrown if the key cannot be generated.</exception>
            public static String GenerateKey(Object sourceObject)
            {

                //Catch unuseful parameter values
                if (sourceObject == null)
                {
                    throw new ArgumentNullException("Null as parameter is not allowed");
                }
                else
                {
                    //We determine if the passed object is really serializable.
                    try
                    {
                        //Now we begin to do the real work.
                        string hashString = ComputeHash(ObjectToByteArray(sourceObject));
                        return hashString;
                    }
                    catch (AmbiguousMatchException ame)
                    {
                        throw new ApplicationException("Could not definitly decide if object is serializable. Message:" + ame.Message);
                    }
                }
            }

            /// <summary>
            /// Converts an object to an array of bytes. This array is used to hash the object.
            /// </summary>
            /// <param name="objectToSerialize">Just an object</param>
            /// <returns>A byte - array representation of the object.</returns>
            /// <exception cref="SerializationException">Is thrown if something went wrong during serialization.</exception>
            private static byte[] ObjectToByteArray(Object objectToSerialize)
            {
                MemoryStream fs = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    //Here's the core functionality! One Line!
                    //To be thread-safe we lock the object
                    lock (locker)
                    {
                        formatter.Serialize(fs, objectToSerialize);
                    }
                    return fs.ToArray();
                }
                catch (SerializationException se)
                {
                    Console.WriteLine("Error occured during serialization. Message: " + se.Message);
                    return null;
                }
                finally
                {
                    fs.Close();
                }
            }

            /// <summary>
            /// Generates the hashcode of an given byte-array. The byte-array can be an object. Then the
            /// method "hashes" this object. The hash can then be used e.g. to identify the object.
            /// </summary>
            /// <param name="objectAsBytes">bytearray representation of an object.</param>
            /// <returns>The MD5 hash of the object as a string or null if it couldn't be generated.</returns>
            private static string ComputeHash(byte[] objectAsBytes)
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                try
                {
                    byte[] result = md5.ComputeHash(objectAsBytes);

                    // Build the final string by converting each byte
                    // into hex and appending it to a StringBuilder
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < result.Length; i++)
                    {
                        sb.Append(result[i].ToString("X2"));
                    }

                    // And return it
                    return sb.ToString();
                }
                catch
                {
                    //If something occured during serialization, this method is called with an null argument. 
                    Console.WriteLine("Hash has not been generated.");
                    return null;
                }
            }
        }
        class DoubleBufferedPictureBox : PictureBox
        {
            public DoubleBufferedPictureBox() : base()
            {
                DoubleBuffered = true;
            }
        }
        class DoubleBufferedBunifuImageButton : Bunifu.Framework.UI.BunifuImageButton
        {
            public DoubleBufferedBunifuImageButton() : base()
            {
                DoubleBuffered = true;

            }

        }


        
        internal class PosInfo
        {
            private readonly int[] _Pos;
        
            internal int[] Pos { get => _Pos;
                set => SetNewPos(value);
            }
        
            internal string ImageName { get; set; }
        
            internal int[] Size;
            internal object Tag { get; set; }
            internal string GUID { get; set; }

            internal PosInfo()
            {
                _Pos = new int[] { 50, 50 };
                Size = new int[] { 0, 0 };
            }
            public int PozX => _Pos[0];
            public int PozY => _Pos[1];
            public int SizeX { get => Size[0];
                set => Size[0] = Convert.ToInt32(value);
            }
            public int SizeY { get => Size[1];
                set => Size[1] = Convert.ToInt32(value);
            }
            internal static Point PosToPoint(int[] cell)
            {
                return new Point((cell[0] * 60) + 205, (cell[1] * 60) + 205);
            }
            internal static double[] PosToAcadPoint(int[] cell)
            {
                return new double[] { cell[0]*50, cell[1]*50, 0};
            }
            internal static string PosToString(int[] Pos)
            {
                return $"{Pos[0]};{Pos[1]}";
            }
            internal void PosDBToClassPos(string pos)
            {
                
                Pos = pos.Split(';')
                    .Select(r => Convert.ToInt32(r))
                    .ToArray();
            }
            internal void SizeDBtoClassSize(string size)
            {
                Size = size.Split(';')
                    .Select(r => Convert.ToInt32(r))
                    .ToArray();
            }
            private void SetNewPos(object val)
            {
                int[] mas = (int[])val;
                _Pos[0] = mas[0];
                _Pos[1] = mas[1];
            }

        }
       
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
        public class TransparentPanel : Panel
        {
            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT
                    return cp;
                }
            }
            protected override void OnPaintBackground(PaintEventArgs e)
            {
                //base.OnPaintBackground(e);
            }
        }
        internal struct Posnames
        {
            internal string Oldposname { get; set; }
            internal string Newposname { get; set; }
        }
        #endregion
    }
}