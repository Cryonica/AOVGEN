using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace AOVGEN
{
    public partial class PlacePannelsForm : Telerik.WinControls.UI.RadForm
    {
        private readonly RadTreeView LevelsTree;
        private readonly Dictionary<string, List<string>> FamDictionary;
        public List<(string, string, string, string, string, string, string, string, string, double)> listinfo = new List<(string, string, string, string, string, string, string, string, string, double)>();
        public PlacePannelsForm(RadTreeView radTreeView, Dictionary<string, List<string>> famdict)
        {
            LevelsTree = radTreeView;
            FamDictionary = famdict;
            InitializeComponent();
                    
        }
        public struct RevitDataInfo
        {
            public string LevelGUID { get; set; }
            public string LevelRevitID { get; set; }
            public string RoomGUID { get; set; }
            public string RoomRevitID { get; set; }
            public string PannelName { get; set; }
            public string PannelGUID { get; set; }
            public string PannelPower { get; set; }
            
        }
        
        private void radButton2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void radToggleSwitch1_ValueChanged(object sender, EventArgs e)
        {
            panel2.Enabled = radToggleSwitch1.Value;
        }

        private void PlacePannelsForm_Load(object sender, EventArgs e)
        {
            
            radGridView1.CellBeginEdit += RadGridView1_CellBeginEdit;
            
            if (FamDictionary?.Count>0)
            {
                radButton1.Enabled = true;
                radButton3.Enabled = true;
                radGroupBox3.Enabled = true;
                radGroupBox2.Enabled = true;
                radGroupBox1.Enabled = true;
                radDropDownList1.Enabled = true;
                radDropDownList2.Enabled = true;
                radDropDownList1.Text = string.Empty;
                radDropDownList2.Text = string.Empty;
                
                foreach (KeyValuePair<string, List<string>> famtypes in FamDictionary)
                {
                    radDropDownList1.Items.Add(famtypes.Key);
                }

                DataTable datatable = new DataTable("PannelsTable");
                datatable.Columns.Add("ID", typeof(string));
                datatable.Columns.Add("Имя шкафа", typeof(string));
                datatable.Columns.Add("Уровень", typeof(string));
                datatable.Columns.Add("Помещение", typeof(string));
                datatable.Columns.Add("Мощность", typeof(string));
                datatable.Columns.Add("Тип семейства", typeof(string));
                datatable.Columns.Add("Имя семейства", typeof(string));
                datatable.Columns.Add("Атрибут", typeof(string));
                datatable.Columns.Add("Значение атрибута", typeof(string));
                datatable.Columns.Add("Высота установки", typeof(double));
                datatable.Columns.Add("RevitInfo", typeof(RevitDataInfo));

                List<string> list = new List<string>();
                for (int t = 1; t <= 10; t++)
                {
                    list.Add(t.ToString());
                }

                GridViewComboBoxColumn comboCol1 = new GridViewComboBoxColumn("Тип семейства")
                {
                    DataSource = FamDictionary.Keys.ToArray(),

                    FieldName = "Тип семейства",
                    DropDownStyle = RadDropDownStyle.DropDownList,


                };
                GridViewComboBoxColumn comboCol2 = new GridViewComboBoxColumn("Имя семейства")
                {
                    //DataSource = list.ToArray(),
                    FieldName = "Имя семейства"
                };

                radGridView1.Columns.Add(new GridViewTextBoxColumn("ID"));
                radGridView1.Columns.Add(new GridViewTextBoxColumn("Имя шкафа"));
                radGridView1.Columns.Add(new GridViewTextBoxColumn("Уровень"));
                radGridView1.Columns.Add(new GridViewTextBoxColumn("Помещение"));
                radGridView1.Columns.Add(new GridViewTextBoxColumn("Мощность"));
                radGridView1.Columns.Add(comboCol1);
                radGridView1.Columns.Add(comboCol2);
                radGridView1.Columns.Add(new GridViewTextBoxColumn("Атрибут"));
                radGridView1.Columns.Add(new GridViewTextBoxColumn("Значение атрибута"));
                radGridView1.Columns.Add(new GridViewTextBoxColumn("Высота установки"));
                radGridView1.Columns.Add(new GridViewTextBoxColumn("RevitInfo"));
                this.radGridView1.Columns["ID"].IsVisible = false;
                this.radGridView1.Columns["RevitInfo"].IsVisible = false;
               
                foreach (var column in this.radGridView1.Columns)
                {
                    column.TextAlignment = ContentAlignment.MiddleCenter;
                }
                this.radGridView1.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;

                this.radGridView1.DataSource = datatable;

                DataTable dataSource = radGridView1.DataSource as DataTable;
                List<RadTreeNode> pannelnodes = LevelsTree.TreeViewElement.GetNodes()
                    .Where(z => z.Level == 2)
                    .ToList();

                if (pannelnodes.Count > 0)
                {
                    foreach (RadTreeNode pannelnode in pannelnodes)
                    {
                        string Room = pannelnode.Parent.Text;
                        string Level = pannelnode.Parent.Parent.Text;
                        string LevelGUID = pannelnode.Parent.Parent.Name;
                        string RoomGUIUD = pannelnode.Parent.Name;
                        string LevelID = (string)pannelnode.Parent.Parent.Tag;
                        (string, string, string) roominfo = ((string, string, string))pannelnode.Parent.Tag;
                        string RoomID = roominfo.Item3;

                        Pannel pannel = (Pannel)pannelnode.Tag;
                        DataRow drToAdd = dataSource.NewRow();
                        drToAdd["ID"] = dataSource.Rows.Count + 1;
                        drToAdd["Имя шкафа"] = pannel.PannelName;
                        drToAdd["Уровень"] = Level;
                        drToAdd["Помещение"] = Room;
                        drToAdd["Мощность"] = pannel.Power;
                        RevitDataInfo revitDataInfo = new RevitDataInfo
                        {
                            PannelName = pannel.PannelName,
                            PannelPower = pannel.Power,
                            LevelGUID = LevelGUID,
                            LevelRevitID = LevelID,
                            RoomGUID = RoomGUIUD,
                            RoomRevitID = RoomID,
                            PannelGUID = pannel.GetGUID()

                        };


                        drToAdd["RevitInfo"] = revitDataInfo;
                        dataSource.Rows.Add(drToAdd);

                    }
                }

            }
            
        }

        private void RadGridView1_CellBeginEdit(object sender, GridViewCellCancelEventArgs e)
        {
           if (e.ColumnIndex ==6)
            {
                GridViewComboBoxColumn comboCol2 = radGridView1.Columns["Имя семейства"] as GridViewComboBoxColumn;
                string fam = e.Row.Cells[5].Value.ToString();
                if (!string.IsNullOrEmpty(fam))
                {
                    List<string> ff = FamDictionary[fam];
                    comboCol2.DataSource = ff;
                }
                

            }
        }

        


        private void radButton3_Click(object sender, EventArgs e)
        {
            if (this.radGridView1.Rows.Count >0)
            {
                foreach (var row in this.radGridView1.Rows)
                {
                    
                    if (radToggleSwitch1.Value)
                    {
                        row.Cells["Атрибут"].Value = radTextBox1.Text;
                        row.Cells["Значение атрибута"].Value = radTextBox2.Text;
                    }
                    else
                    {
                        row.Cells["Атрибут"].Value = string.Empty;
                        row.Cells["Значение атрибута"].Value = string.Empty;
                    }
                    bool canparsedouble =  double.TryParse(radTextBox3.Text, out double height);
                    if (canparsedouble) row.Cells["Высота установки"].Value = height;

                    row.Cells["Тип семейства"].Value =radDropDownList1.Text;
                    row.Cells["Имя семейства"].Value = radDropDownList2.Text;

                }
            }
            
        }

        private void radDropDownList1_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            List<string> devices = FamDictionary[radDropDownList1.SelectedItem.Text];
            radDropDownList2.Items.Clear();
            foreach (string device in devices)
            {
                radDropDownList2.Items.Add(device);
            }

            
        }

        private void radButton1_Click(object sender, EventArgs e)
        {

            try
            {
                foreach (var row in radGridView1.Rows)
                {
                    RevitDataInfo revitDataInfo = (RevitDataInfo)row.Cells["RevitInfo"].Value;
                    (string GUID, string PannelName, string Level, string Room, string Power, string Family, string FamilyType, string Attrib, string AttribValue, double Height) info;
                    info.GUID = revitDataInfo.PannelGUID;
                    info.PannelName = revitDataInfo.PannelName;
                    info.Level = revitDataInfo.LevelRevitID;
                    info.Room = revitDataInfo.RoomRevitID;
                    info.Power = revitDataInfo.PannelPower;
                    info.Family = row.Cells["Тип семейства"].Value.ToString();
                    info.FamilyType = row.Cells["Имя семейства"].Value.ToString();
                    info.Attrib = row.Cells["Атрибут"].Value.ToString();
                    info.AttribValue = row.Cells["Значение атрибута"].Value.ToString();
                    info.Height = (double)row.Cells["Высота установки"].Value;

                    listinfo.Add(info);
                }
                DialogResult = DialogResult.OK;
            }
            catch
            {
                MessageBox.Show("Каждый шкаф должен иметь сведения как минимум о семействе для размещения в Ревит");
                return;
            }
            
           
        }
    }
}
