using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Telerik.WinControls.UI;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Linq;

namespace AOVGEN
{
    public partial class SelectPannelForm : Telerik.WinControls.UI.RadForm
    {
        private readonly List<RadTreeNode> radTreeNodes = new List<RadTreeNode>();
        internal Dictionary<string, Pannel> CheckedItems;
        SQLiteConnection connection;

        public string DBFilePath { get; set; }
       
        public SelectPannelForm(List<RadTreeNode> inputnodes, SQLiteConnection inputconnection, string dbpath)
        {
            DBFilePath = dbpath;
            connection = inputconnection;
            radTreeNodes = inputnodes;
            InitializeComponent();
            this.InitializeData();
            
            this.radCheckedListBox1.VisualItemFormatting += radCheckedListBox1_VisualItemFormatting;
            
            

        }

        private void radButton3_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
            
        }

        private void SelectPannelForm_Load(object sender, EventArgs e)
        {
            CheckedItems = new Dictionary<string, Pannel>();
        }
        private void InitializeData()
        {
            List<Pannel> Pannels = this.CreatePannels();
            if (connection.State == ConnectionState.Closed)
            {
                connection = OpenDB();
                connection.Open();
            }
            SQLiteCommand command = new SQLiteCommand { Connection = connection };
            foreach (Pannel pannel in Pannels)
            {
                this.radCheckedListBox1.Items.Add(this.CreateItem(pannel, command));
            }
            command.Dispose();
            
            
            
            
            
        }

        private List<Pannel> CreatePannels()
        {
            return (from pannel in radTreeNodes
            .Where(e => e != null)
            .Select(e => (Pannel)e.Tag)
                    select pannel)
                    .OrderBy(e => e.PannelName)
                    .ToList();
                    
                    

            //List<Pannel> pannels = new List<Pannel>();

            //foreach (RadTreeNode node in radTreeNodes)
            //{
            //    Pannel pannel = (Pannel)node.Tag;
            //    if (pannel != null) pannels.Add(pannel);

            //}

            //return pannels;
        }
        private ListViewDataItem CreateItem(Pannel pannel, SQLiteCommand command)
        {
            string query = $"SELECT Cable.GUID FROM Cable WHERE FromGUID = '{pannel.GetGUID()}'";
            command.CommandText = query;
            SQLiteDataReader reader = command.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);
            int numRows = dt.Rows.Count;
            ArrayList arrayList = new ArrayList();
            ListViewDataItem item = new ListViewDataItem
            {
                Image = new Bitmap(AOVGEN.Properties.Resources.documents_32__1_),
                Text = pannel.PannelName
            };
            arrayList.Add(numRows);
            arrayList.Add(pannel);
            item.Tag = arrayList;

            return item;
        }
        private void radCheckedListBox1_VisualItemFormatting(object sender, ListViewVisualItemEventArgs e)
        {
            SimpleListViewVisualItem item = e.VisualItem as SimpleListViewVisualItem;
            //ArrayList arrayList = new ArrayList();
            ArrayList arrayList = item.Data.Tag as ArrayList;
            Pannel pannel = arrayList[1] as Pannel;
            int cablecount = (int)arrayList[0];
            //string color = "#681406";
            
            if (item.Children.Count > 0)
            {
                ListViewItemCheckbox checkBoxItem = item.Children[0] as ListViewItemCheckbox;
                checkBoxItem.Margin = new Padding(2, 2, 3, 2);
            }

            item.Data.Text = "<html>" +
                   "<span style=\"font-size:10.5pt;font-family:Segoe UI Semibold;\">  " + pannel.PannelName + "</span>" +
                   "<br><span style=\"font-size:8pt;\"><i>   " + cablecount.ToString() +   " кабелей </i></span>";
                   //"<br><span style=\"font-size:14pt;\"> </span>";

            item.ImageLayout = ImageLayout.Center;
            item.ImageAlignment = ContentAlignment.MiddleLeft;
        }
        private void radListView1_CellFormatting(object sender, ListViewCellFormattingEventArgs e)
        {
            if (e.CellElement is DetailListViewDataCellElement cell)
            {
                if (cell.Text != string.Empty)
                {
                    cell.Text = new string(' ', 5) + string.Format("{0}", cell.Text);
                    e.CellElement.BorderGradientStyle = Telerik.WinControls.GradientStyles.Solid;
                }
                else
                {
                    e.CellElement.ResetValue(LightVisualElement.BorderGradientStyleProperty, Telerik.WinControls.ValueResetFlags.Local);
                }
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
            catch { }
            return null;

        }

        private void radCheckedListBox1_ItemCheckedChanged(object sender, ListViewItemEventArgs e)
        {
            ArrayList array = e.Item.Tag as ArrayList;
            Pannel pannel = (Pannel)array[1];
            if (e.Item.CheckState == Telerik.WinControls.Enumerations.ToggleState.On)
            {
                
                if(!CheckedItems.ContainsKey(pannel.PannelName))
                {
                    CheckedItems.Add(pannel.PannelName, pannel);
                }
                
            }
            if (e.Item.CheckState == Telerik.WinControls.Enumerations.ToggleState.Off)
            {
                if (CheckedItems.ContainsKey(pannel.PannelName))
                {
                    CheckedItems.Remove(pannel.PannelName);
                }

            }
            
        }

        private void radButton1_Click(object sender, EventArgs e)
        {
            foreach (var item in radCheckedListBox1.Items)
            {
                item.CheckState = Telerik.WinControls.Enumerations.ToggleState.On;
                
            }
        }

        private void radButton2_Click(object sender, EventArgs e)
        {
            foreach (var item in radCheckedListBox1.Items)
            {
                item.CheckState = Telerik.WinControls.Enumerations.ToggleState.Off;

            }
        }
    }
}
