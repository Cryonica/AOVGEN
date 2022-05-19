using System.ComponentModel;
using System.Windows.Forms;
using Bunifu.Framework.UI;
using Telerik.WinControls;
using Telerik.WinControls.Tests;
using Telerik.WinControls.Themes;
using Telerik.WinControls.UI;
using CustomShape = Telerik.WinControls.OldShapeEditor.CustomShape;

namespace AOVGEN
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition1 = new Telerik.WinControls.UI.TableViewDefinition();
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition2 = new Telerik.WinControls.UI.TableViewDefinition();
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition3 = new Telerik.WinControls.UI.TableViewDefinition();
            Telerik.WinControls.UI.TableViewDefinition tableViewDefinition4 = new Telerik.WinControls.UI.TableViewDefinition();
            this.office2007BlackTheme1 = new Telerik.WinControls.Themes.Office2007BlackTheme();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.bunifuImageButton1 = new Bunifu.Framework.UI.BunifuImageButton();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.radPageView1 = new Telerik.WinControls.UI.RadPageView();
            this.radPageViewPage1 = new Telerik.WinControls.UI.RadPageViewPage();
            this.radGridView2 = new Telerik.WinControls.UI.RadGridView();
            this.radPageViewPage2 = new Telerik.WinControls.UI.RadPageViewPage();
            this.radSplitContainer2 = new Telerik.WinControls.UI.RadSplitContainer();
            this.splitPanel3 = new Telerik.WinControls.UI.SplitPanel();
            this.radTreeView2 = new Telerik.WinControls.UI.RadTreeView();
            this.splitPanel4 = new Telerik.WinControls.UI.SplitPanel();
            this.radGridView1 = new Telerik.WinControls.UI.RadGridView();
            this.splitPanel8 = new Telerik.WinControls.UI.SplitPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.radDropDownList1 = new Telerik.WinControls.UI.RadDropDownList();
            this.panel2 = new System.Windows.Forms.Panel();
            this.radGridView4 = new Telerik.WinControls.UI.RadGridView();
            this.radPageViewPage3 = new Telerik.WinControls.UI.RadPageViewPage();
            this.radSplitContainer1 = new Telerik.WinControls.UI.RadSplitContainer();
            this.splitPanel1 = new Telerik.WinControls.UI.SplitPanel();
            this.radTreeView4 = new Telerik.WinControls.UI.RadTreeView();
            this.radContextMenu2 = new Telerik.WinControls.UI.RadContextMenu(this.components);
            this.radMenuItem1 = new Telerik.WinControls.UI.RadMenuItem();
            this.splitPanel2 = new Telerik.WinControls.UI.SplitPanel();
            this.radTreeView3 = new Telerik.WinControls.UI.RadTreeView();
            this.splitPanel5 = new Telerik.WinControls.UI.SplitPanel();
            this.radGridView3 = new Telerik.WinControls.UI.RadGridView();
            this.radPageViewPage4 = new Telerik.WinControls.UI.RadPageViewPage();
            this.radSplitContainer3 = new Telerik.WinControls.UI.RadSplitContainer();
            this.splitPanel6 = new Telerik.WinControls.UI.SplitPanel();
            this.radTreeView5 = new Telerik.WinControls.UI.RadTreeView();
            this.radContextMenu3 = new Telerik.WinControls.UI.RadContextMenu(this.components);
            this.radMenuItem2 = new Telerik.WinControls.UI.RadMenuItem();
            this.splitPanel7 = new Telerik.WinControls.UI.SplitPanel();
            this.radTreeView6 = new Telerik.WinControls.UI.RadTreeView();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.radContextMenu1 = new Telerik.WinControls.UI.RadContextMenu(this.components);
            this.radTreeViewMenuItem1 = new Telerik.WinControls.UI.RadMenuItem();
            this.radTreeViewMenuItem2 = new Telerik.WinControls.UI.RadMenuItem();
            this.radTreeViewMenuItem3 = new Telerik.WinControls.UI.RadMenuItem();
            this.radTreeViewMenuItem4 = new Telerik.WinControls.UI.RadMenuItem();
            this.radTreeView1 = new Telerik.WinControls.UI.RadTreeView();
            this.radPropertyGrid1 = new Telerik.WinControls.UI.RadPropertyGrid();
            this.radContextMenuManager1 = new Telerik.WinControls.UI.RadContextMenuManager();
            this.radContextMenu5 = new Telerik.WinControls.UI.RadContextMenu(this.components);
            this.radMenuItem5 = new Telerik.WinControls.UI.RadMenuItem();
            this.ellipseShape1 = new Telerik.WinControls.EllipseShape();
            this.qaShape1 = new Telerik.WinControls.Tests.QAShape();
            this.donutShape1 = new Telerik.WinControls.Tests.DonutShape();
            this.object_7ce56807_c935_4a35_b10c_632b1e7a3d80 = new Telerik.WinControls.RootRadElement();
            this.chamferedRectShape1 = new Telerik.WinControls.ChamferedRectShape();
            this.tabEdgeShape1 = new Telerik.WinControls.UI.TabEdgeShape();
            this.radRibbonBar1 = new Telerik.WinControls.UI.RadRibbonBar();
            this.ribbonTab1 = new Telerik.WinControls.UI.RibbonTab();
            this.radRibbonBarGroup1 = new Telerik.WinControls.UI.RadRibbonBarGroup();
            this.radButtonElement1 = new Telerik.WinControls.UI.RadButtonElement();
            this.radButtonElement2 = new Telerik.WinControls.UI.RadButtonElement();
            this.radButtonElement3 = new Telerik.WinControls.UI.RadButtonElement();
            this.radRibbonBarGroup2 = new Telerik.WinControls.UI.RadRibbonBarGroup();
            this.radButtonElement4 = new Telerik.WinControls.UI.RadButtonElement();
            this.ribbonTab2 = new Telerik.WinControls.UI.RibbonTab();
            this.radRibbonBarGroup3 = new Telerik.WinControls.UI.RadRibbonBarGroup();
            this.radButtonElement5 = new Telerik.WinControls.UI.RadButtonElement();
            this.radButtonElement6 = new Telerik.WinControls.UI.RadButtonElement();
            this.ribbonTab3 = new Telerik.WinControls.UI.RibbonTab();
            this.radRibbonBarGroup4 = new Telerik.WinControls.UI.RadRibbonBarGroup();
            this.radButtonElement7 = new Telerik.WinControls.UI.RadButtonElement();
            this.radButtonElement8 = new Telerik.WinControls.UI.RadButtonElement();
            this.ribbonTab4 = new Telerik.WinControls.UI.RibbonTab();
            this.radRibbonBarGroup7 = new Telerik.WinControls.UI.RadRibbonBarGroup();
            this.radButtonElement16 = new Telerik.WinControls.UI.RadButtonElement();
            this.radButtonElement17 = new Telerik.WinControls.UI.RadButtonElement();
            this.radButtonElement18 = new Telerik.WinControls.UI.RadButtonElement();
            this.radRibbonBarGroup8 = new Telerik.WinControls.UI.RadRibbonBarGroup();
            this.radButtonElement9 = new Telerik.WinControls.UI.RadButtonElement();
            this.ribbonTab6 = new Telerik.WinControls.UI.RibbonTab();
            this.radRibbonBarGroup9 = new Telerik.WinControls.UI.RadRibbonBarGroup();
            this.radButtonElement21 = new Telerik.WinControls.UI.RadButtonElement();
            this.radButtonElement22 = new Telerik.WinControls.UI.RadButtonElement();
            this.radButtonElement24 = new Telerik.WinControls.UI.RadButtonElement();
            this.ribbonTab5 = new Telerik.WinControls.UI.RibbonTab();
            this.radRibbonBarGroup5 = new Telerik.WinControls.UI.RadRibbonBarGroup();
            this.radButtonElement10 = new Telerik.WinControls.UI.RadButtonElement();
            this.radButtonElement11 = new Telerik.WinControls.UI.RadButtonElement();
            this.radRibbonBarGroup6 = new Telerik.WinControls.UI.RadRibbonBarGroup();
            this.radButtonElement12 = new Telerik.WinControls.UI.RadButtonElement();
            this.radButtonElement13 = new Telerik.WinControls.UI.RadButtonElement();
            this.radButtonElement14 = new Telerik.WinControls.UI.RadButtonElement();
            this.radButtonElement15 = new Telerik.WinControls.UI.RadButtonElement();
            this.object_5f69bfee_7344_4cce_bbe1_3dbd405e4410 = new Telerik.WinControls.RootRadElement();
            this.object_b1f54696_96bb_4979_8c83_0b8c2be0bade = new Telerik.WinControls.RootRadElement();
            this.customShape1 = new Telerik.WinControls.OldShapeEditor.CustomShape();
            this.object_34cc611d_65cc_4954_a90c_4c2f0b9d335a = new Telerik.WinControls.RootRadElement();
            this.radCollapsiblePanel1 = new Telerik.WinControls.UI.RadCollapsiblePanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.circleShape1 = new Telerik.WinControls.CircleShape();
            this.trackBarDThumbShape1 = new Telerik.WinControls.UI.TrackBarDThumbShape();
            this.trackBarLThumbShape1 = new Telerik.WinControls.UI.TrackBarLThumbShape();
            this.diamondShape1 = new Telerik.WinControls.UI.DiamondShape();
            this.starShape1 = new Telerik.WinControls.UI.StarShape();
            this.mediaShape1 = new Telerik.WinControls.Tests.MediaShape();
            this.officeShape1 = new Telerik.WinControls.UI.OfficeShape();
            this.tabItemShape1 = new Telerik.WinControls.UI.TabItemShape();
            this.tabOffice12Shape1 = new Telerik.WinControls.UI.TabOffice12Shape();
            this.radContextMenu4 = new Telerik.WinControls.UI.RadContextMenu(this.components);
            this.radMenuItem3 = new Telerik.WinControls.UI.RadMenuItem();
            this.radMenuItem4 = new Telerik.WinControls.UI.RadMenuItem();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bunifuImageButton1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radPageView1)).BeginInit();
            this.radPageView1.SuspendLayout();
            this.radPageViewPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radGridView2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radGridView2.MasterTemplate)).BeginInit();
            this.radPageViewPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radSplitContainer2)).BeginInit();
            this.radSplitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel3)).BeginInit();
            this.splitPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radTreeView2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel4)).BeginInit();
            this.splitPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radGridView1.MasterTemplate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel8)).BeginInit();
            this.splitPanel8.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radDropDownList1)).BeginInit();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radGridView4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radGridView4.MasterTemplate)).BeginInit();
            this.radPageViewPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radSplitContainer1)).BeginInit();
            this.radSplitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel1)).BeginInit();
            this.splitPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radTreeView4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel2)).BeginInit();
            this.splitPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radTreeView3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel5)).BeginInit();
            this.splitPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radGridView3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radGridView3.MasterTemplate)).BeginInit();
            this.radPageViewPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radSplitContainer3)).BeginInit();
            this.radSplitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel6)).BeginInit();
            this.splitPanel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radTreeView5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel7)).BeginInit();
            this.splitPanel7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radTreeView6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radTreeView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radPropertyGrid1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radRibbonBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radCollapsiblePanel1)).BeginInit();
            this.radCollapsiblePanel1.PanelContainer.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Crimson;
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.pictureBox2);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.bunifuImageButton1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1684, 31);
            this.panel1.TabIndex = 0;
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseDown);
            this.panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseMove);
            this.panel1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseUp);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(42, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(143, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Статус соединения с Revit";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(8, 2);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(27, 27);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 5;
            this.pictureBox2.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1552, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "UserName";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1438, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Добро пожаловать,";
            // 
            // bunifuImageButton1
            // 
            this.bunifuImageButton1.Image = ((System.Drawing.Image)(resources.GetObject("bunifuImageButton1.Image")));
            this.bunifuImageButton1.ImageActive = null;
            this.bunifuImageButton1.Location = new System.Drawing.Point(1650, 3);
            this.bunifuImageButton1.Name = "bunifuImageButton1";
            this.bunifuImageButton1.Size = new System.Drawing.Size(26, 26);
            this.bunifuImageButton1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.bunifuImageButton1.TabIndex = 3;
            this.bunifuImageButton1.TabStop = false;
            this.bunifuImageButton1.Zoom = 10;
            this.bunifuImageButton1.Click += new System.EventHandler(this.BunifuImageButton1_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // radPageView1
            // 
            this.radPageView1.Controls.Add(this.radPageViewPage1);
            this.radPageView1.Controls.Add(this.radPageViewPage2);
            this.radPageView1.Controls.Add(this.radPageViewPage3);
            this.radPageView1.Controls.Add(this.radPageViewPage4);
            this.radPageView1.Location = new System.Drawing.Point(240, 3);
            this.radPageView1.Name = "radPageView1";
            this.radPageView1.SelectedPage = this.radPageViewPage4;
            this.radPageView1.Size = new System.Drawing.Size(1068, 567);
            this.radPageView1.TabIndex = 11;
            this.radPageView1.ThemeName = "Office2007Black";
            ((Telerik.WinControls.UI.RadPageViewStripElement)(this.radPageView1.GetChildAt(0))).ShowItemPinButton = false;
            ((Telerik.WinControls.UI.RadPageViewStripElement)(this.radPageView1.GetChildAt(0))).StripButtons = Telerik.WinControls.UI.StripViewButtons.None;
            ((Telerik.WinControls.UI.RadPageViewStripElement)(this.radPageView1.GetChildAt(0))).StripAlignment = Telerik.WinControls.UI.StripViewAlignment.Bottom;
            ((Telerik.WinControls.UI.RadPageViewStripElement)(this.radPageView1.GetChildAt(0))).ShowItemCloseButton = false;
            // 
            // radPageViewPage1
            // 
            this.radPageViewPage1.Controls.Add(this.radGridView2);
            this.radPageViewPage1.ItemSize = new System.Drawing.SizeF(118F, 28F);
            this.radPageViewPage1.Location = new System.Drawing.Point(10, 10);
            this.radPageViewPage1.Name = "radPageViewPage1";
            this.radPageViewPage1.Size = new System.Drawing.Size(1047, 519);
            this.radPageViewPage1.Text = "Шкафы управления";
            // 
            // radGridView2
            // 
            this.radGridView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radGridView2.Location = new System.Drawing.Point(0, 0);
            // 
            // 
            // 
            this.radGridView2.MasterTemplate.AllowAddNewRow = false;
            this.radGridView2.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            this.radGridView2.MasterTemplate.ViewDefinition = tableViewDefinition1;
            this.radGridView2.Name = "radGridView2";
            this.radGridView2.Padding = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.radGridView2.Size = new System.Drawing.Size(1047, 519);
            this.radGridView2.TabIndex = 6;
            this.radGridView2.ThemeName = "Office2007Black";
            this.radGridView2.CellEndEdit += new Telerik.WinControls.UI.GridViewCellEventHandler(this.radGridView2_CellEndEdit);
            this.radGridView2.SelectionChanging += new Telerik.WinControls.UI.GridViewSelectionCancelEventHandler(this.radGridView2_SelectionChanging);
            this.radGridView2.CellClick += new Telerik.WinControls.UI.GridViewCellEventHandler(this.radGridView2_CellClick);
            this.radGridView2.CellValueChanged += new Telerik.WinControls.UI.GridViewCellEventHandler(this.radGridView2_CellValueChanged);
            this.radGridView2.ContextMenuOpening += new Telerik.WinControls.UI.ContextMenuOpeningEventHandler(this.radGridView2_ContextMenuOpening);
            // 
            // radPageViewPage2
            // 
            this.radPageViewPage2.Controls.Add(this.radSplitContainer2);
            this.radPageViewPage2.ItemSize = new System.Drawing.SizeF(86F, 28F);
            this.radPageViewPage2.Location = new System.Drawing.Point(10, 10);
            this.radPageViewPage2.Name = "radPageViewPage2";
            this.radPageViewPage2.Size = new System.Drawing.Size(1047, 519);
            this.radPageViewPage2.Text = "Вент.системы";
            // 
            // radSplitContainer2
            // 
            this.radSplitContainer2.Controls.Add(this.splitPanel3);
            this.radSplitContainer2.Controls.Add(this.splitPanel4);
            this.radSplitContainer2.Controls.Add(this.splitPanel8);
            this.radSplitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radSplitContainer2.Location = new System.Drawing.Point(0, 0);
            this.radSplitContainer2.Name = "radSplitContainer2";
            // 
            // 
            // 
            this.radSplitContainer2.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.radSplitContainer2.Size = new System.Drawing.Size(1047, 519);
            this.radSplitContainer2.TabIndex = 9;
            this.radSplitContainer2.TabStop = false;
            this.radSplitContainer2.ThemeName = "Office2007Black";
            // 
            // splitPanel3
            // 
            this.splitPanel3.Controls.Add(this.radTreeView2);
            this.splitPanel3.Location = new System.Drawing.Point(0, 0);
            this.splitPanel3.Name = "splitPanel3";
            // 
            // 
            // 
            this.splitPanel3.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.splitPanel3.Size = new System.Drawing.Size(184, 519);
            this.splitPanel3.SizeInfo.AutoSizeScale = new System.Drawing.SizeF(-0.15624F, 0F);
            this.splitPanel3.SizeInfo.SplitterCorrection = new System.Drawing.Size(-163, 0);
            this.splitPanel3.TabIndex = 0;
            this.splitPanel3.TabStop = false;
            this.splitPanel3.Text = "splitPanel3";
            // 
            // radTreeView2
            // 
            this.radTreeView2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(229)))), ((int)(((byte)(234)))));
            this.radTreeView2.Cursor = System.Windows.Forms.Cursors.Default;
            this.radTreeView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radTreeView2.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.radTreeView2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.radTreeView2.LineStyle = Telerik.WinControls.UI.TreeLineStyle.Solid;
            this.radTreeView2.Location = new System.Drawing.Point(0, 0);
            this.radTreeView2.Name = "radTreeView2";
            this.radTreeView2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.radTreeView2.Size = new System.Drawing.Size(184, 519);
            this.radTreeView2.SortOrder = System.Windows.Forms.SortOrder.Ascending;
            this.radTreeView2.SpacingBetweenNodes = -1;
            this.radTreeView2.TabIndex = 8;
            this.radTreeView2.ThemeName = "Office2007Black";
            this.radTreeView2.ValueChanged += new Telerik.WinControls.UI.TreeNodeValueChangedEventHandler(this.radTreeView2_ValueChanged);
            this.radTreeView2.NodeMouseClick += new Telerik.WinControls.UI.RadTreeView.TreeViewEventHandler(this.radTreeView2_NodeMouseClick);
            this.radTreeView2.Click += new System.EventHandler(this.radTreeView2_Click);
            this.radTreeView2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.radTreeView2_MouseClick);
            // 
            // splitPanel4
            // 
            this.splitPanel4.Controls.Add(this.radGridView1);
            this.splitPanel4.Location = new System.Drawing.Point(188, 0);
            this.splitPanel4.Name = "splitPanel4";
            // 
            // 
            // 
            this.splitPanel4.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.splitPanel4.Size = new System.Drawing.Size(210, 519);
            this.splitPanel4.SizeInfo.AutoSizeScale = new System.Drawing.SizeF(-0.1312159F, 0F);
            this.splitPanel4.SizeInfo.SplitterCorrection = new System.Drawing.Size(-135, 0);
            this.splitPanel4.TabIndex = 1;
            this.splitPanel4.TabStop = false;
            this.splitPanel4.Text = "splitPanel4";
            // 
            // radGridView1
            // 
            this.radGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radGridView1.Location = new System.Drawing.Point(0, 0);
            // 
            // 
            // 
            this.radGridView1.MasterTemplate.AllowAddNewRow = false;
            this.radGridView1.MasterTemplate.AllowDeleteRow = false;
            this.radGridView1.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            this.radGridView1.MasterTemplate.MultiSelect = true;
            this.radGridView1.MasterTemplate.ViewDefinition = tableViewDefinition2;
            this.radGridView1.Name = "radGridView1";
            this.radGridView1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.radGridView1.Size = new System.Drawing.Size(210, 519);
            this.radGridView1.TabIndex = 7;
            this.radGridView1.ThemeName = "Office2007Black";
            this.radGridView1.CellClick += new Telerik.WinControls.UI.GridViewCellEventHandler(this.radGridView1_CellClick);
            this.radGridView1.ContextMenuOpening += new Telerik.WinControls.UI.ContextMenuOpeningEventHandler(this.radGridView1_ContextMenuOpening_1);
            // 
            // splitPanel8
            // 
            this.splitPanel8.Controls.Add(this.flowLayoutPanel2);
            this.splitPanel8.Location = new System.Drawing.Point(402, 0);
            this.splitPanel8.Name = "splitPanel8";
            // 
            // 
            // 
            this.splitPanel8.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.splitPanel8.Size = new System.Drawing.Size(645, 519);
            this.splitPanel8.SizeInfo.AutoSizeScale = new System.Drawing.SizeF(0.2874559F, 0F);
            this.splitPanel8.SizeInfo.SplitterCorrection = new System.Drawing.Size(298, 0);
            this.splitPanel8.TabIndex = 2;
            this.splitPanel8.TabStop = false;
            this.splitPanel8.Text = "splitPanel8";
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.radDropDownList1);
            this.flowLayoutPanel2.Controls.Add(this.panel2);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(645, 519);
            this.flowLayoutPanel2.TabIndex = 0;
            // 
            // radDropDownList1
            // 
            this.radDropDownList1.Dock = System.Windows.Forms.DockStyle.Top;
            this.radDropDownList1.DropDownHeight = 500;
            this.radDropDownList1.DropDownStyle = Telerik.WinControls.RadDropDownStyle.DropDownList;
            this.radDropDownList1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.radDropDownList1.Location = new System.Drawing.Point(3, 3);
            this.radDropDownList1.Name = "radDropDownList1";
            this.radDropDownList1.NullText = "Выбор вендора";
            this.radDropDownList1.Size = new System.Drawing.Size(639, 0);
            this.radDropDownList1.TabIndex = 7;
            this.radDropDownList1.ThemeName = "Office2007Black";
            this.radDropDownList1.SelectedIndexChanged += new Telerik.WinControls.UI.Data.PositionChangedEventHandler(this.radDropDownList1_SelectedIndexChanged);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.radGridView4);
            this.panel2.Location = new System.Drawing.Point(3, 9);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(639, 480);
            this.panel2.TabIndex = 8;
            // 
            // radGridView4
            // 
            this.radGridView4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radGridView4.Location = new System.Drawing.Point(0, 0);
            // 
            // 
            // 
            this.radGridView4.MasterTemplate.AllowAddNewRow = false;
            this.radGridView4.MasterTemplate.AllowDeleteRow = false;
            this.radGridView4.MasterTemplate.ViewDefinition = tableViewDefinition3;
            this.radGridView4.Name = "radGridView4";
            this.radGridView4.Padding = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.radContextMenuManager1.SetRadContextMenu(this.radGridView4, this.radContextMenu5);
            this.radGridView4.Size = new System.Drawing.Size(639, 480);
            this.radGridView4.TabIndex = 0;
            this.radGridView4.ThemeName = "Office2007Black";
            this.radGridView4.CellEndEdit += new Telerik.WinControls.UI.GridViewCellEventHandler(this.radGridView4_CellEndEdit);
            this.radGridView4.CellClick += new Telerik.WinControls.UI.GridViewCellEventHandler(this.radGridView4_CellClick);
            this.radGridView4.ContextMenuOpening += new Telerik.WinControls.UI.ContextMenuOpeningEventHandler(this.radGridView4_ContextMenuOpening);
            // 
            // radPageViewPage3
            // 
            this.radPageViewPage3.Controls.Add(this.radSplitContainer1);
            this.radPageViewPage3.ItemSize = new System.Drawing.SizeF(79F, 28F);
            this.radPageViewPage3.Location = new System.Drawing.Point(10, 10);
            this.radPageViewPage3.Name = "radPageViewPage3";
            this.radPageViewPage3.Size = new System.Drawing.Size(1047, 519);
            this.radPageViewPage3.Text = "Связывание";
            // 
            // radSplitContainer1
            // 
            this.radSplitContainer1.Controls.Add(this.splitPanel1);
            this.radSplitContainer1.Controls.Add(this.splitPanel2);
            this.radSplitContainer1.Controls.Add(this.splitPanel5);
            this.radSplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radSplitContainer1.Location = new System.Drawing.Point(0, 0);
            this.radSplitContainer1.Name = "radSplitContainer1";
            // 
            // 
            // 
            this.radSplitContainer1.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.radSplitContainer1.Size = new System.Drawing.Size(1047, 519);
            this.radSplitContainer1.TabIndex = 19;
            this.radSplitContainer1.TabStop = false;
            this.radSplitContainer1.ThemeName = "Office2007Black";
            // 
            // splitPanel1
            // 
            this.splitPanel1.Controls.Add(this.radTreeView4);
            this.splitPanel1.Location = new System.Drawing.Point(0, 0);
            this.splitPanel1.Name = "splitPanel1";
            // 
            // 
            // 
            this.splitPanel1.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.splitPanel1.Size = new System.Drawing.Size(159, 519);
            this.splitPanel1.SizeInfo.AutoSizeScale = new System.Drawing.SizeF(-0.1803016F, 0F);
            this.splitPanel1.SizeInfo.SplitterCorrection = new System.Drawing.Size(-187, 0);
            this.splitPanel1.TabIndex = 0;
            this.splitPanel1.TabStop = false;
            this.splitPanel1.Text = "splitPanel1";
            // 
            // radTreeView4
            // 
            this.radTreeView4.AllowDragDrop = true;
            this.radTreeView4.AllowDrop = true;
            this.radTreeView4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(229)))), ((int)(((byte)(234)))));
            this.radTreeView4.Cursor = System.Windows.Forms.Cursors.Default;
            this.radTreeView4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radTreeView4.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.radTreeView4.ForeColor = System.Drawing.SystemColors.ControlText;
            this.radTreeView4.LineStyle = Telerik.WinControls.UI.TreeLineStyle.Solid;
            this.radTreeView4.Location = new System.Drawing.Point(0, 0);
            this.radTreeView4.MultiSelect = true;
            this.radTreeView4.Name = "radTreeView4";
            this.radTreeView4.RadContextMenu = this.radContextMenu2;
            this.radTreeView4.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.radTreeView4.Size = new System.Drawing.Size(159, 519);
            this.radTreeView4.SpacingBetweenNodes = -1;
            this.radTreeView4.TabIndex = 10;
            this.radTreeView4.ThemeName = "Office2007Black";
            this.radTreeView4.DragEnded += new Telerik.WinControls.UI.RadTreeView.DragEndedHandler(this.radTreeView4_DragEnded);
            this.radTreeView4.DragOverNode += new System.EventHandler<Telerik.WinControls.UI.RadTreeViewDragCancelEventArgs>(this.radTreeView4_DragOverNode);
            this.radTreeView4.SelectedNodeChanging += new Telerik.WinControls.UI.RadTreeView.RadTreeViewCancelEventHandler(this.radTreeView4_SelectedNodeChanging);
            this.radTreeView4.NodeMouseClick += new Telerik.WinControls.UI.RadTreeView.TreeViewEventHandler(this.radTreeView4_NodeMouseClick);
            // 
            // radContextMenu2
            // 
            this.radContextMenu2.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.radMenuItem1});
            // 
            // radMenuItem1
            // 
            this.radMenuItem1.Name = "radMenuItem1";
            this.radMenuItem1.Text = "Отключить";
            // 
            // splitPanel2
            // 
            this.splitPanel2.Controls.Add(this.radTreeView3);
            this.splitPanel2.Location = new System.Drawing.Point(163, 0);
            this.splitPanel2.Name = "splitPanel2";
            // 
            // 
            // 
            this.splitPanel2.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.splitPanel2.Size = new System.Drawing.Size(144, 519);
            this.splitPanel2.SizeInfo.AutoSizeScale = new System.Drawing.SizeF(-0.1947385F, 0F);
            this.splitPanel2.SizeInfo.SplitterCorrection = new System.Drawing.Size(-202, 0);
            this.splitPanel2.TabIndex = 1;
            this.splitPanel2.TabStop = false;
            this.splitPanel2.Text = "splitPanel2";
            // 
            // radTreeView3
            // 
            this.radTreeView3.AllowDragDrop = true;
            this.radTreeView3.AllowDrop = true;
            this.radTreeView3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(229)))), ((int)(((byte)(234)))));
            this.radTreeView3.Cursor = System.Windows.Forms.Cursors.Default;
            this.radTreeView3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radTreeView3.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.radTreeView3.ForeColor = System.Drawing.SystemColors.ControlText;
            this.radTreeView3.LineStyle = Telerik.WinControls.UI.TreeLineStyle.Solid;
            this.radTreeView3.Location = new System.Drawing.Point(0, 0);
            this.radTreeView3.Name = "radTreeView3";
            this.radTreeView3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.radTreeView3.Size = new System.Drawing.Size(144, 519);
            this.radTreeView3.SortOrder = System.Windows.Forms.SortOrder.Ascending;
            this.radTreeView3.SpacingBetweenNodes = -1;
            this.radTreeView3.TabIndex = 9;
            this.radTreeView3.ThemeName = "Office2007Black";
            this.radTreeView3.DragEnded += new Telerik.WinControls.UI.RadTreeView.DragEndedHandler(this.radTreeView3_DragEnded);
            this.radTreeView3.DragOverNode += new System.EventHandler<Telerik.WinControls.UI.RadTreeViewDragCancelEventArgs>(this.radTreeView3_DragOverNode);
            this.radTreeView3.NodeMouseClick += new Telerik.WinControls.UI.RadTreeView.TreeViewEventHandler(this.radTreeView3_NodeMouseClick);
            // 
            // splitPanel5
            // 
            this.splitPanel5.Controls.Add(this.radGridView3);
            this.splitPanel5.Location = new System.Drawing.Point(311, 0);
            this.splitPanel5.Name = "splitPanel5";
            // 
            // 
            // 
            this.splitPanel5.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.splitPanel5.Size = new System.Drawing.Size(736, 519);
            this.splitPanel5.SizeInfo.AutoSizeScale = new System.Drawing.SizeF(0.3750401F, 0F);
            this.splitPanel5.SizeInfo.SplitterCorrection = new System.Drawing.Size(389, 0);
            this.splitPanel5.TabIndex = 2;
            this.splitPanel5.TabStop = false;
            this.splitPanel5.Text = "splitPanel5";
            // 
            // radGridView3
            // 
            this.radGridView3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radGridView3.Location = new System.Drawing.Point(0, 0);
            // 
            // 
            // 
            this.radGridView3.MasterTemplate.AllowAddNewRow = false;
            this.radGridView3.MasterTemplate.AllowDeleteRow = false;
            this.radGridView3.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            this.radGridView3.MasterTemplate.MultiSelect = true;
            this.radGridView3.MasterTemplate.ViewDefinition = tableViewDefinition4;
            this.radGridView3.Name = "radGridView3";
            this.radGridView3.Padding = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.radGridView3.Size = new System.Drawing.Size(736, 519);
            this.radGridView3.TabIndex = 11;
            this.radGridView3.ThemeName = "Office2007Black";
            this.radGridView3.ViewCellFormatting += new Telerik.WinControls.UI.CellFormattingEventHandler(this.radGridView3_ViewCellFormatting);
            this.radGridView3.CellBeginEdit += new Telerik.WinControls.UI.GridViewCellCancelEventHandler(this.radGridView3_CellBeginEdit);
            this.radGridView3.CellEndEdit += new Telerik.WinControls.UI.GridViewCellEventHandler(this.radGridView3_CellEndEdit);
            this.radGridView3.SelectionChanging += new Telerik.WinControls.UI.GridViewSelectionCancelEventHandler(this.radGridView3_SelectionChanging);
            this.radGridView3.ContextMenuOpening += new Telerik.WinControls.UI.ContextMenuOpeningEventHandler(this.radGridView3_ContextMenuOpening);
            // 
            // radPageViewPage4
            // 
            this.radPageViewPage4.Controls.Add(this.radSplitContainer3);
            this.radPageViewPage4.Controls.Add(this.pictureBox1);
            this.radPageViewPage4.ItemSize = new System.Drawing.SizeF(55F, 28F);
            this.radPageViewPage4.Location = new System.Drawing.Point(10, 10);
            this.radPageViewPage4.Name = "radPageViewPage4";
            this.radPageViewPage4.Size = new System.Drawing.Size(1047, 519);
            this.radPageViewPage4.Text = "Уровни";
            // 
            // radSplitContainer3
            // 
            this.radSplitContainer3.Controls.Add(this.splitPanel6);
            this.radSplitContainer3.Controls.Add(this.splitPanel7);
            this.radSplitContainer3.Dock = System.Windows.Forms.DockStyle.Left;
            this.radSplitContainer3.Location = new System.Drawing.Point(0, 0);
            this.radSplitContainer3.Name = "radSplitContainer3";
            // 
            // 
            // 
            this.radSplitContainer3.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.radSplitContainer3.Size = new System.Drawing.Size(661, 519);
            this.radSplitContainer3.TabIndex = 0;
            this.radSplitContainer3.TabStop = false;
            this.radSplitContainer3.ThemeName = "Office2007Black";
            // 
            // splitPanel6
            // 
            this.splitPanel6.Controls.Add(this.radTreeView5);
            this.splitPanel6.Location = new System.Drawing.Point(0, 0);
            this.splitPanel6.Name = "splitPanel6";
            // 
            // 
            // 
            this.splitPanel6.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.splitPanel6.Size = new System.Drawing.Size(430, 519);
            this.splitPanel6.SizeInfo.AutoSizeScale = new System.Drawing.SizeF(0.1544901F, 0F);
            this.splitPanel6.SizeInfo.SplitterCorrection = new System.Drawing.Size(102, 0);
            this.splitPanel6.TabIndex = 0;
            this.splitPanel6.TabStop = false;
            this.splitPanel6.Text = "splitPanel6";
            this.splitPanel6.ThemeName = "Office2007Black";
            // 
            // radTreeView5
            // 
            this.radTreeView5.AllowDragDrop = true;
            this.radTreeView5.AllowDrop = true;
            this.radTreeView5.AllowShowFocusCues = true;
            this.radTreeView5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radTreeView5.Location = new System.Drawing.Point(0, 0);
            this.radTreeView5.Name = "radTreeView5";
            this.radTreeView5.RadContextMenu = this.radContextMenu3;
            this.radTreeView5.Size = new System.Drawing.Size(430, 519);
            this.radTreeView5.SpacingBetweenNodes = -1;
            this.radTreeView5.TabIndex = 0;
            this.radTreeView5.ThemeName = "Office2007Black";
            this.radTreeView5.DragEnded += new Telerik.WinControls.UI.RadTreeView.DragEndedHandler(this.radTreeView5_DragEnded);
            this.radTreeView5.DragOverNode += new System.EventHandler<Telerik.WinControls.UI.RadTreeViewDragCancelEventArgs>(this.radTreeView5_DragOverNode);
            this.radTreeView5.ContextMenuOpening += new Telerik.WinControls.UI.TreeViewContextMenuOpeningEventHandler(this.radTreeView5_ContextMenuOpening);
            this.radTreeView5.DragDrop += new System.Windows.Forms.DragEventHandler(this.radTreeView5_DragDrop);
            // 
            // radContextMenu3
            // 
            this.radContextMenu3.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.radMenuItem2});
            // 
            // radMenuItem2
            // 
            this.radMenuItem2.Name = "radMenuItem2";
            this.radMenuItem2.Text = "Отключить";
            // 
            // splitPanel7
            // 
            this.splitPanel7.Controls.Add(this.radTreeView6);
            this.splitPanel7.Location = new System.Drawing.Point(434, 0);
            this.splitPanel7.Name = "splitPanel7";
            // 
            // 
            // 
            this.splitPanel7.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.splitPanel7.Size = new System.Drawing.Size(227, 519);
            this.splitPanel7.SizeInfo.AutoSizeScale = new System.Drawing.SizeF(-0.1544901F, 0F);
            this.splitPanel7.SizeInfo.SplitterCorrection = new System.Drawing.Size(-102, 0);
            this.splitPanel7.TabIndex = 1;
            this.splitPanel7.TabStop = false;
            this.splitPanel7.Text = "splitPanel7";
            this.splitPanel7.ThemeName = "Office2007Black";
            // 
            // radTreeView6
            // 
            this.radTreeView6.AllowDragDrop = true;
            this.radTreeView6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radTreeView6.Location = new System.Drawing.Point(0, 0);
            this.radTreeView6.MultiSelect = true;
            this.radTreeView6.Name = "radTreeView6";
            this.radTreeView6.Size = new System.Drawing.Size(227, 519);
            this.radTreeView6.SpacingBetweenNodes = -1;
            this.radTreeView6.TabIndex = 1;
            this.radTreeView6.ThemeName = "Office2007Black";
            this.radTreeView6.DragEnded += new Telerik.WinControls.UI.RadTreeView.DragEndedHandler(this.radTreeView6_DragEnded);
            this.radTreeView6.DragOverNode += new System.EventHandler<Telerik.WinControls.UI.RadTreeViewDragCancelEventArgs>(this.radTreeView6_DragOverNode);
            this.radTreeView6.NodeMouseClick += new Telerik.WinControls.UI.RadTreeView.TreeViewEventHandler(this.radTreeView6_NodeMouseClick);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(683, 78);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(349, 353);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // radContextMenu1
            // 
            this.radContextMenu1.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.radTreeViewMenuItem1,
            this.radTreeViewMenuItem2,
            this.radTreeViewMenuItem3,
            this.radTreeViewMenuItem4});
            // 
            // radTreeViewMenuItem1
            // 
            this.radTreeViewMenuItem1.Name = "radTreeViewMenuItem1";
            this.radTreeViewMenuItem1.Text = "Переименовать";
            // 
            // radTreeViewMenuItem2
            // 
            this.radTreeViewMenuItem2.Name = "radTreeViewMenuItem2";
            this.radTreeViewMenuItem2.Text = "Создать копию";
            // 
            // radTreeViewMenuItem3
            // 
            this.radTreeViewMenuItem3.Name = "radTreeViewMenuItem3";
            this.radTreeViewMenuItem3.Text = "Редактировать";
            // 
            // radTreeViewMenuItem4
            // 
            this.radTreeViewMenuItem4.Name = "radTreeViewMenuItem4";
            this.radTreeViewMenuItem4.Text = "Удалить";
            // 
            // radTreeView1
            // 
            this.radTreeView1.Location = new System.Drawing.Point(3, 3);
            this.radTreeView1.Name = "radTreeView1";
            this.radTreeView1.Size = new System.Drawing.Size(190, 558);
            this.radTreeView1.SortOrder = System.Windows.Forms.SortOrder.Ascending;
            this.radTreeView1.SpacingBetweenNodes = -1;
            this.radTreeView1.TabIndex = 5;
            this.radTreeView1.ThemeName = "Office2007Black";
            this.radTreeView1.NodeMouseClick += new Telerik.WinControls.UI.RadTreeView.TreeViewEventHandler(this.radTreeView1_NodeMouseClick);
            this.radTreeView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.radTreeView1_MouseDown);
            // 
            // radPropertyGrid1
            // 
            this.radPropertyGrid1.HelpVisible = false;
            this.radPropertyGrid1.Location = new System.Drawing.Point(1324, 191);
            this.radPropertyGrid1.Name = "radPropertyGrid1";
            this.radPropertyGrid1.Size = new System.Drawing.Size(354, 572);
            this.radPropertyGrid1.TabIndex = 9;
            this.radPropertyGrid1.ThemeName = "Office2007Black";
            this.radPropertyGrid1.SelectedObjectChanged += new Telerik.WinControls.UI.PropertyGridSelectedObjectChangedEventHandler(this.radPropertyGrid1_SelectedObjectChanged);
            this.radPropertyGrid1.PropertyValueChanged += new Telerik.WinControls.UI.PropertyGridItemValueChangedEventHandler(this.radPropertyGrid1_PropertyValueChanged);
            // 
            // radContextMenu5
            // 
            this.radContextMenu5.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.radMenuItem5});
            // 
            // radMenuItem5
            // 
            this.radMenuItem5.Name = "radMenuItem5";
            this.radMenuItem5.Text = "Очистить вендора";
            // 
            // ellipseShape1
            // 
            this.ellipseShape1.IsRightToLeft = false;
            // 
            // qaShape1
            // 
            this.qaShape1.IsRightToLeft = false;
            // 
            // donutShape1
            // 
            this.donutShape1.IsRightToLeft = false;
            // 
            // object_7ce56807_c935_4a35_b10c_632b1e7a3d80
            // 
            this.object_7ce56807_c935_4a35_b10c_632b1e7a3d80.Name = "object_7ce56807_c935_4a35_b10c_632b1e7a3d80";
            this.object_7ce56807_c935_4a35_b10c_632b1e7a3d80.StretchHorizontally = true;
            this.object_7ce56807_c935_4a35_b10c_632b1e7a3d80.StretchVertically = true;
            // 
            // chamferedRectShape1
            // 
            this.chamferedRectShape1.IsRightToLeft = false;
            // 
            // tabEdgeShape1
            // 
            this.tabEdgeShape1.IsRightToLeft = false;
            // 
            // radRibbonBar1
            // 
            this.radRibbonBar1.CommandTabs.AddRange(new Telerik.WinControls.RadItem[] {
            this.ribbonTab1,
            this.ribbonTab2,
            this.ribbonTab3,
            this.ribbonTab4,
            this.ribbonTab6,
            this.ribbonTab5});
            // 
            // 
            // 
            this.radRibbonBar1.ExitButton.Text = "Exit";
            this.radRibbonBar1.LocalizationSettings.LayoutModeText = "Simplified Layout";
            this.radRibbonBar1.Location = new System.Drawing.Point(0, 31);
            this.radRibbonBar1.Name = "radRibbonBar1";
            // 
            // 
            // 
            this.radRibbonBar1.OptionsButton.Text = "Options";
            this.radRibbonBar1.ShowExpandButton = false;
            this.radRibbonBar1.Size = new System.Drawing.Size(1684, 156);
            this.radRibbonBar1.TabIndex = 15;
            this.radRibbonBar1.Text = "radRibbonBar2";
            this.radRibbonBar1.ThemeName = "Office2007Black";
            // 
            // ribbonTab1
            // 
            this.ribbonTab1.IsSelected = false;
            this.ribbonTab1.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.radRibbonBarGroup1,
            this.radRibbonBarGroup2});
            this.ribbonTab1.Name = "ribbonTab1";
            this.ribbonTab1.Text = "Проекты";
            this.ribbonTab1.UseMnemonic = false;
            this.ribbonTab1.Click += new System.EventHandler(this.ribbonTab1_Click);
            // 
            // radRibbonBarGroup1
            // 
            this.radRibbonBarGroup1.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.radButtonElement1,
            this.radButtonElement2,
            this.radButtonElement3});
            this.radRibbonBarGroup1.Margin = new System.Windows.Forms.Padding(40, 2, 2, 2);
            this.radRibbonBarGroup1.Name = "radRibbonBarGroup1";
            this.radRibbonBarGroup1.Text = "Управление";
            // 
            // radButtonElement1
            // 
            this.radButtonElement1.Image = ((System.Drawing.Image)(resources.GetObject("radButtonElement1.Image")));
            this.radButtonElement1.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.radButtonElement1.Name = "radButtonElement1";
            this.radButtonElement1.Text = "Добавить проект";
            this.radButtonElement1.TextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.radButtonElement1.Click += new System.EventHandler(this.radButtonElement1_Click);
            // 
            // radButtonElement2
            // 
            this.radButtonElement2.Image = ((System.Drawing.Image)(resources.GetObject("radButtonElement2.Image")));
            this.radButtonElement2.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.radButtonElement2.Name = "radButtonElement2";
            this.radButtonElement2.Text = "Добавить здание";
            this.radButtonElement2.TextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.radButtonElement2.Click += new System.EventHandler(this.radButtonElement2_Click);
            // 
            // radButtonElement3
            // 
            this.radButtonElement3.Image = ((System.Drawing.Image)(resources.GetObject("radButtonElement3.Image")));
            this.radButtonElement3.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.radButtonElement3.Name = "radButtonElement3";
            this.radButtonElement3.Shape = null;
            this.radButtonElement3.Text = "Удалить";
            this.radButtonElement3.TextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.radButtonElement3.UseCompatibleTextRendering = false;
            this.radButtonElement3.Click += new System.EventHandler(this.radButtonElement3_Click);
            // 
            // radRibbonBarGroup2
            // 
            this.radRibbonBarGroup2.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.radButtonElement4});
            this.radRibbonBarGroup2.Name = "radRibbonBarGroup2";
            this.radRibbonBarGroup2.Text = "Настройки";
            // 
            // radButtonElement4
            // 
            this.radButtonElement4.Enabled = false;
            this.radButtonElement4.Image = ((System.Drawing.Image)(resources.GetObject("radButtonElement4.Image")));
            this.radButtonElement4.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.radButtonElement4.Name = "radButtonElement4";
            this.radButtonElement4.Text = "Выбор вендора";
            this.radButtonElement4.TextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.radButtonElement4.Click += new System.EventHandler(this.radButtonElement4_Click_1);
            // 
            // ribbonTab2
            // 
            this.ribbonTab2.IsSelected = false;
            this.ribbonTab2.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.radRibbonBarGroup3});
            this.ribbonTab2.Name = "ribbonTab2";
            this.ribbonTab2.Text = "Шкафы управления";
            this.ribbonTab2.UseMnemonic = false;
            this.ribbonTab2.Click += new System.EventHandler(this.ribbonTab2_Click);
            // 
            // radRibbonBarGroup3
            // 
            this.radRibbonBarGroup3.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.radButtonElement5,
            this.radButtonElement6});
            this.radRibbonBarGroup3.Margin = new System.Windows.Forms.Padding(40, 2, 2, 2);
            this.radRibbonBarGroup3.Name = "radRibbonBarGroup3";
            this.radRibbonBarGroup3.Text = "Управление шкафами";
            // 
            // radButtonElement5
            // 
            this.radButtonElement5.Image = ((System.Drawing.Image)(resources.GetObject("radButtonElement5.Image")));
            this.radButtonElement5.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.radButtonElement5.Margin = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.radButtonElement5.Name = "radButtonElement5";
            this.radButtonElement5.Text = "Добавить";
            this.radButtonElement5.TextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.radButtonElement5.Click += new System.EventHandler(this.radButtonElement5_Click);
            // 
            // radButtonElement6
            // 
            this.radButtonElement6.Image = ((System.Drawing.Image)(resources.GetObject("radButtonElement6.Image")));
            this.radButtonElement6.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.radButtonElement6.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.radButtonElement6.Name = "radButtonElement6";
            this.radButtonElement6.Text = "Удалить";
            this.radButtonElement6.TextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.radButtonElement6.Click += new System.EventHandler(this.radButtonElement6_Click);
            // 
            // ribbonTab3
            // 
            this.ribbonTab3.IsSelected = false;
            this.ribbonTab3.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.radRibbonBarGroup4});
            this.ribbonTab3.Name = "ribbonTab3";
            this.ribbonTab3.Text = "Вент.системы";
            this.ribbonTab3.UseMnemonic = false;
            this.ribbonTab3.Click += new System.EventHandler(this.ribbonTab3_Click);
            // 
            // radRibbonBarGroup4
            // 
            this.radRibbonBarGroup4.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.radButtonElement7,
            this.radButtonElement8});
            this.radRibbonBarGroup4.Margin = new System.Windows.Forms.Padding(40, 2, 2, 2);
            this.radRibbonBarGroup4.Name = "radRibbonBarGroup4";
            this.radRibbonBarGroup4.Text = "Управление вент.системами";
            // 
            // radButtonElement7
            // 
            this.radButtonElement7.Image = ((System.Drawing.Image)(resources.GetObject("radButtonElement7.Image")));
            this.radButtonElement7.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.radButtonElement7.Margin = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.radButtonElement7.Name = "radButtonElement7";
            this.radButtonElement7.Text = "Добавить";
            this.radButtonElement7.TextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.radButtonElement7.Click += new System.EventHandler(this.radButtonElement7_Click);
            // 
            // radButtonElement8
            // 
            this.radButtonElement8.Image = ((System.Drawing.Image)(resources.GetObject("radButtonElement8.Image")));
            this.radButtonElement8.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.radButtonElement8.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.radButtonElement8.Name = "radButtonElement8";
            this.radButtonElement8.Text = "Удалить";
            this.radButtonElement8.TextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.radButtonElement8.Click += new System.EventHandler(this.radButtonElement8_Click);
            // 
            // ribbonTab4
            // 
            this.ribbonTab4.IsSelected = false;
            this.ribbonTab4.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.radRibbonBarGroup7,
            this.radRibbonBarGroup8});
            this.ribbonTab4.Name = "ribbonTab4";
            this.ribbonTab4.Text = "Подключения и КЖ";
            this.ribbonTab4.UseMnemonic = false;
            this.ribbonTab4.Click += new System.EventHandler(this.ribbonTab4_Click);
            // 
            // radRibbonBarGroup7
            // 
            this.radRibbonBarGroup7.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.radButtonElement16,
            this.radButtonElement17,
            this.radButtonElement18});
            this.radRibbonBarGroup7.Margin = new System.Windows.Forms.Padding(40, 2, 2, 2);
            this.radRibbonBarGroup7.Name = "radRibbonBarGroup7";
            this.radRibbonBarGroup7.Text = "КЖ";
            // 
            // radButtonElement16
            // 
            this.radButtonElement16.Image = ((System.Drawing.Image)(resources.GetObject("radButtonElement16.Image")));
            this.radButtonElement16.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.radButtonElement16.Name = "radButtonElement16";
            this.radButtonElement16.Text = "Назначить имена";
            this.radButtonElement16.TextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.radButtonElement16.Click += new System.EventHandler(this.radButtonElement16_Click);
            // 
            // radButtonElement17
            // 
            this.radButtonElement17.Image = ((System.Drawing.Image)(resources.GetObject("radButtonElement17.Image")));
            this.radButtonElement17.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.radButtonElement17.Name = "radButtonElement17";
            this.radButtonElement17.Text = "Установить длину";
            this.radButtonElement17.TextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.radButtonElement17.Click += new System.EventHandler(this.radButtonElement17_Click);
            // 
            // radButtonElement18
            // 
            this.radButtonElement18.Image = ((System.Drawing.Image)(resources.GetObject("radButtonElement18.Image")));
            this.radButtonElement18.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.radButtonElement18.Name = "radButtonElement18";
            this.radButtonElement18.Text = "Назначить типы";
            this.radButtonElement18.TextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.radButtonElement18.Click += new System.EventHandler(this.radButtonElement18_Click);
            // 
            // radRibbonBarGroup8
            // 
            this.radRibbonBarGroup8.Alignment = System.Drawing.ContentAlignment.TopLeft;
            this.radRibbonBarGroup8.AngleTransform = 0F;
            this.radRibbonBarGroup8.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.radButtonElement9});
            this.radRibbonBarGroup8.Margin = new System.Windows.Forms.Padding(20, 2, 2, 2);
            this.radRibbonBarGroup8.Name = "radRibbonBarGroup8";
            this.radRibbonBarGroup8.Text = "Настройки";
            // 
            // radButtonElement9
            // 
            this.radButtonElement9.Image = ((System.Drawing.Image)(resources.GetObject("radButtonElement9.Image")));
            this.radButtonElement9.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.radButtonElement9.Name = "radButtonElement9";
            this.radButtonElement9.Text = "Настройки КЖ";
            this.radButtonElement9.TextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.radButtonElement9.Click += new System.EventHandler(this.radButtonElement9_Click);
            // 
            // ribbonTab6
            // 
            this.ribbonTab6.IsSelected = false;
            this.ribbonTab6.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.radRibbonBarGroup9});
            this.ribbonTab6.Name = "ribbonTab6";
            this.ribbonTab6.Text = "Уровни";
            this.ribbonTab6.UseMnemonic = false;
            this.ribbonTab6.Click += new System.EventHandler(this.ribbonTab6_Click_1);
            // 
            // radRibbonBarGroup9
            // 
            this.radRibbonBarGroup9.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.radButtonElement21,
            this.radButtonElement22,
            this.radButtonElement24});
            this.radRibbonBarGroup9.Name = "radRibbonBarGroup9";
            this.radRibbonBarGroup9.Text = "REVIT";
            // 
            // radButtonElement21
            // 
            this.radButtonElement21.Image = ((System.Drawing.Image)(resources.GetObject("radButtonElement21.Image")));
            this.radButtonElement21.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.radButtonElement21.Name = "radButtonElement21";
            this.radButtonElement21.Text = "Считать уровни";
            this.radButtonElement21.TextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.radButtonElement21.Click += new System.EventHandler(this.radButtonElement21_Click);
            // 
            // radButtonElement22
            // 
            this.radButtonElement22.Image = ((System.Drawing.Image)(resources.GetObject("radButtonElement22.Image")));
            this.radButtonElement22.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.radButtonElement22.Name = "radButtonElement22";
            this.radButtonElement22.Text = "Удалить уровни";
            this.radButtonElement22.TextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.radButtonElement22.Click += new System.EventHandler(this.radButtonElement22_Click);
            // 
            // radButtonElement24
            // 
            this.radButtonElement24.Image = ((System.Drawing.Image)(resources.GetObject("radButtonElement24.Image")));
            this.radButtonElement24.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.radButtonElement24.Name = "radButtonElement24";
            this.radButtonElement24.Text = "Шкафы - в модель";
            this.radButtonElement24.TextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.radButtonElement24.Click += new System.EventHandler(this.radButtonElement24_Click);
            // 
            // ribbonTab5
            // 
            this.ribbonTab5.IsSelected = true;
            this.ribbonTab5.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.radRibbonBarGroup5,
            this.radRibbonBarGroup6});
            this.ribbonTab5.Name = "ribbonTab5";
            this.ribbonTab5.Text = "Генераторы";
            this.ribbonTab5.UseMnemonic = false;
            this.ribbonTab5.Click += new System.EventHandler(this.ribbonTab5_Click);
            // 
            // radRibbonBarGroup5
            // 
            this.radRibbonBarGroup5.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.radButtonElement10,
            this.radButtonElement11});
            this.radRibbonBarGroup5.Margin = new System.Windows.Forms.Padding(40, 2, 2, 2);
            this.radRibbonBarGroup5.Name = "radRibbonBarGroup5";
            this.radRibbonBarGroup5.Text = "Autocad";
            // 
            // radButtonElement10
            // 
            this.radButtonElement10.Image = ((System.Drawing.Image)(resources.GetObject("radButtonElement10.Image")));
            this.radButtonElement10.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.radButtonElement10.Name = "radButtonElement10";
            this.radButtonElement10.Text = "Схема внешних";
            this.radButtonElement10.TextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.radButtonElement10.Click += new System.EventHandler(this.radButtonElement10_Click);
            // 
            // radButtonElement11
            // 
            this.radButtonElement11.Image = ((System.Drawing.Image)(resources.GetObject("radButtonElement11.Image")));
            this.radButtonElement11.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.radButtonElement11.Name = "radButtonElement11";
            this.radButtonElement11.Text = "Схема автоматизации";
            this.radButtonElement11.TextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.radButtonElement11.Click += new System.EventHandler(this.radButtonElement11_Click);
            // 
            // radRibbonBarGroup6
            // 
            this.radRibbonBarGroup6.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.radButtonElement12,
            this.radButtonElement13,
            this.radButtonElement14,
            this.radButtonElement15});
            this.radRibbonBarGroup6.Name = "radRibbonBarGroup6";
            this.radRibbonBarGroup6.Text = "Excel";
            // 
            // radButtonElement12
            // 
            this.radButtonElement12.Image = ((System.Drawing.Image)(resources.GetObject("radButtonElement12.Image")));
            this.radButtonElement12.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.radButtonElement12.Name = "radButtonElement12";
            this.radButtonElement12.Text = "Кабельный журнал";
            this.radButtonElement12.TextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.radButtonElement12.Click += new System.EventHandler(this.radButtonElement12_Click);
            // 
            // radButtonElement13
            // 
            this.radButtonElement13.Image = ((System.Drawing.Image)(resources.GetObject("radButtonElement13.Image")));
            this.radButtonElement13.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.radButtonElement13.Name = "radButtonElement13";
            this.radButtonElement13.Text = "Задание ЭОМ";
            this.radButtonElement13.TextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.radButtonElement13.Click += new System.EventHandler(this.radButtonElement13_Click);
            // 
            // radButtonElement14
            // 
            this.radButtonElement14.Image = ((System.Drawing.Image)(resources.GetObject("radButtonElement14.Image")));
            this.radButtonElement14.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.radButtonElement14.Name = "radButtonElement14";
            this.radButtonElement14.Text = "Спецификация";
            this.radButtonElement14.TextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.radButtonElement14.Click += new System.EventHandler(this.radButtonElement14_Click);
            // 
            // radButtonElement15
            // 
            this.radButtonElement15.Image = ((System.Drawing.Image)(resources.GetObject("radButtonElement15.Image")));
            this.radButtonElement15.ImageAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.radButtonElement15.Name = "radButtonElement15";
            this.radButtonElement15.Text = "Список IO";
            this.radButtonElement15.TextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.radButtonElement15.Click += new System.EventHandler(this.radButtonElement15_Click);
            // 
            // object_5f69bfee_7344_4cce_bbe1_3dbd405e4410
            // 
            this.object_5f69bfee_7344_4cce_bbe1_3dbd405e4410.Name = "object_5f69bfee_7344_4cce_bbe1_3dbd405e4410";
            this.object_5f69bfee_7344_4cce_bbe1_3dbd405e4410.StretchHorizontally = true;
            this.object_5f69bfee_7344_4cce_bbe1_3dbd405e4410.StretchVertically = true;
            // 
            // object_b1f54696_96bb_4979_8c83_0b8c2be0bade
            // 
            this.object_b1f54696_96bb_4979_8c83_0b8c2be0bade.Name = "object_b1f54696_96bb_4979_8c83_0b8c2be0bade";
            this.object_b1f54696_96bb_4979_8c83_0b8c2be0bade.Shape = null;
            this.object_b1f54696_96bb_4979_8c83_0b8c2be0bade.StretchHorizontally = true;
            this.object_b1f54696_96bb_4979_8c83_0b8c2be0bade.StretchVertically = true;
            // 
            // customShape1
            // 
            this.customShape1.Dimension = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.customShape1.IsRightToLeft = false;
            // 
            // object_34cc611d_65cc_4954_a90c_4c2f0b9d335a
            // 
            this.object_34cc611d_65cc_4954_a90c_4c2f0b9d335a.BorderHighlightThickness = 0;
            this.object_34cc611d_65cc_4954_a90c_4c2f0b9d335a.Name = "object_34cc611d_65cc_4954_a90c_4c2f0b9d335a";
            this.object_34cc611d_65cc_4954_a90c_4c2f0b9d335a.StretchHorizontally = true;
            this.object_34cc611d_65cc_4954_a90c_4c2f0b9d335a.StretchVertically = true;
            // 
            // radCollapsiblePanel1
            // 
            this.radCollapsiblePanel1.AnimationEasingType = Telerik.WinControls.RadEasingType.OutBack;
            this.radCollapsiblePanel1.AnimationInterval = 15;
            this.radCollapsiblePanel1.AnimationType = Telerik.WinControls.UI.CollapsiblePanelAnimationType.Slide;
            this.radCollapsiblePanel1.ExpandDirection = Telerik.WinControls.UI.RadDirection.Right;
            this.radCollapsiblePanel1.HeaderText = "Панель проектов";
            this.radCollapsiblePanel1.HorizontalHeaderAlignment = Telerik.WinControls.UI.RadHorizontalAlignment.Stretch;
            this.radCollapsiblePanel1.Location = new System.Drawing.Point(3, 3);
            this.radCollapsiblePanel1.Name = "radCollapsiblePanel1";
            // 
            // radCollapsiblePanel1.PanelContainer
            // 
            this.radCollapsiblePanel1.PanelContainer.Controls.Add(this.radTreeView1);
            this.radCollapsiblePanel1.PanelContainer.Size = new System.Drawing.Size(196, 564);
            this.radCollapsiblePanel1.Size = new System.Drawing.Size(231, 566);
            this.radCollapsiblePanel1.TabIndex = 17;
            this.radCollapsiblePanel1.ThemeName = "Office2007Black";
            this.radCollapsiblePanel1.VerticalHeaderAlignment = Telerik.WinControls.UI.RadVerticalAlignment.Stretch;
            this.radCollapsiblePanel1.Expanded += new System.EventHandler(this.radCollapsiblePanel1_Expanded);
            this.radCollapsiblePanel1.Collapsed += new System.EventHandler(this.radCollapsiblePanel1_Collapsed);
            this.radCollapsiblePanel1.Expanding += new System.ComponentModel.CancelEventHandler(this.radCollapsiblePanel1_Expanding);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.radCollapsiblePanel1);
            this.flowLayoutPanel1.Controls.Add(this.radPageView1);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(7, 191);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1311, 572);
            this.flowLayoutPanel1.TabIndex = 18;
            // 
            // circleShape1
            // 
            this.circleShape1.IsRightToLeft = false;
            // 
            // trackBarDThumbShape1
            // 
            this.trackBarDThumbShape1.IsRightToLeft = false;
            // 
            // trackBarLThumbShape1
            // 
            this.trackBarLThumbShape1.IsRightToLeft = false;
            // 
            // diamondShape1
            // 
            this.diamondShape1.IsRightToLeft = false;
            // 
            // starShape1
            // 
            this.starShape1.Arms = 5;
            this.starShape1.InnerRadiusRatio = 0.375F;
            this.starShape1.IsRightToLeft = false;
            // 
            // mediaShape1
            // 
            this.mediaShape1.IsRightToLeft = false;
            // 
            // officeShape1
            // 
            this.officeShape1.IsRightToLeft = false;
            // 
            // tabItemShape1
            // 
            this.tabItemShape1.IsRightToLeft = false;
            // 
            // tabOffice12Shape1
            // 
            this.tabOffice12Shape1.IsRightToLeft = false;
            // 
            // radContextMenu4
            // 
            this.radContextMenu4.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.radMenuItem3,
            this.radMenuItem4});
            // 
            // radMenuItem3
            // 
            this.radMenuItem3.Name = "radMenuItem3";
            this.radMenuItem3.Text = "Назначить вендора";
            // 
            // radMenuItem4
            // 
            this.radMenuItem4.Name = "radMenuItem4";
            this.radMenuItem4.Text = "Очистить вендора";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1684, 773);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.radRibbonBar1);
            this.Controls.Add(this.radPropertyGrid1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainForm";
            // 
            // 
            // 
            this.RootElement.ApplyShapeToControl = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "radRibbonBar2";
            this.ThemeName = "Office2007Black";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bunifuImageButton1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radPageView1)).EndInit();
            this.radPageView1.ResumeLayout(false);
            this.radPageViewPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.radGridView2.MasterTemplate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radGridView2)).EndInit();
            this.radPageViewPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.radSplitContainer2)).EndInit();
            this.radSplitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel3)).EndInit();
            this.splitPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.radTreeView2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel4)).EndInit();
            this.splitPanel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.radGridView1.MasterTemplate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel8)).EndInit();
            this.splitPanel8.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radDropDownList1)).EndInit();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.radGridView4.MasterTemplate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radGridView4)).EndInit();
            this.radPageViewPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.radSplitContainer1)).EndInit();
            this.radSplitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel1)).EndInit();
            this.splitPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.radTreeView4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel2)).EndInit();
            this.splitPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.radTreeView3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel5)).EndInit();
            this.splitPanel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.radGridView3.MasterTemplate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radGridView3)).EndInit();
            this.radPageViewPage4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.radSplitContainer3)).EndInit();
            this.radSplitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel6)).EndInit();
            this.splitPanel6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.radTreeView5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel7)).EndInit();
            this.splitPanel7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.radTreeView6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radTreeView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radPropertyGrid1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radRibbonBar1)).EndInit();
            this.radCollapsiblePanel1.PanelContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.radCollapsiblePanel1)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        

        #endregion

        private Office2007BlackTheme office2007BlackTheme1;
        private Panel panel1;
        private BunifuImageButton bunifuImageButton1;
        private Timer timer1;
        private RadPageView radPageView1;
        private RadPageViewPage radPageViewPage1;
        private RadPageViewPage radPageViewPage2;
        private RadTreeView radTreeView1;
        private RadGridView radGridView2;
        private RadPropertyGrid radPropertyGrid1;
        private RadGridView radGridView1;
        private RadTreeView radTreeView2;
        private RadContextMenu radContextMenu1;
        private RadMenuItem radTreeViewMenuItem1;
        private RadMenuItem radTreeViewMenuItem2;
        private RadContextMenuManager radContextMenuManager1;
        private RadMenuItem radTreeViewMenuItem3;
        private RadMenuItem radTreeViewMenuItem4;
        private RadPageViewPage radPageViewPage3;
        private RadGridView radGridView3;
        private RadContextMenu radContextMenu2;
        private RadMenuItem radMenuItem1;
        private EllipseShape ellipseShape1;
        private QAShape qaShape1;
        private DonutShape donutShape1;
        private RootRadElement object_7ce56807_c935_4a35_b10c_632b1e7a3d80;
        private ChamferedRectShape chamferedRectShape1;
        private TabEdgeShape tabEdgeShape1;
        private RadRibbonBar radRibbonBar1;
        private RootRadElement object_5f69bfee_7344_4cce_bbe1_3dbd405e4410;
        private RibbonTab ribbonTab1;
        private RibbonTab ribbonTab2;
        private RibbonTab ribbonTab3;
        private RadRibbonBarGroup radRibbonBarGroup1;
        private RadButtonElement radButtonElement1;
        private RadButtonElement radButtonElement2;
        private RadButtonElement radButtonElement3;
        private RadRibbonBarGroup radRibbonBarGroup3;
        private RadButtonElement radButtonElement5;
        private RadButtonElement radButtonElement6;
        private RadRibbonBarGroup radRibbonBarGroup4;
        private RadButtonElement radButtonElement7;
        private RadButtonElement radButtonElement8;
        private RootRadElement object_b1f54696_96bb_4979_8c83_0b8c2be0bade;
        private CustomShape customShape1;
        private RootRadElement object_34cc611d_65cc_4954_a90c_4c2f0b9d335a;
        private RadCollapsiblePanel radCollapsiblePanel1;
        private FlowLayoutPanel flowLayoutPanel1;
        private RadSplitContainer radSplitContainer1;
        private RadSplitContainer radSplitContainer2;
        private SplitPanel splitPanel3;
        private SplitPanel splitPanel4;
        private SplitPanel splitPanel1;
        private SplitPanel splitPanel2;
        private SplitPanel splitPanel5;
        private RibbonTab ribbonTab4;
        private CircleShape circleShape1;
        private TrackBarDThumbShape trackBarDThumbShape1;
        private TrackBarLThumbShape trackBarLThumbShape1;
        private DiamondShape diamondShape1;
        private StarShape starShape1;
        private MediaShape mediaShape1;
        private OfficeShape officeShape1;
        private TabItemShape tabItemShape1;
        private TabOffice12Shape tabOffice12Shape1;
        private RibbonTab ribbonTab5;
        private RadRibbonBarGroup radRibbonBarGroup5;
        private RadRibbonBarGroup radRibbonBarGroup6;
        private RadButtonElement radButtonElement10;
        private RadButtonElement radButtonElement11;
        private RadButtonElement radButtonElement12;
        private RadButtonElement radButtonElement13;
        private RadButtonElement radButtonElement14;
        private RadButtonElement radButtonElement15;
        private RadRibbonBarGroup radRibbonBarGroup7;
        private RadButtonElement radButtonElement16;
        private RadButtonElement radButtonElement17;
        private RadButtonElement radButtonElement18;
        private RadRibbonBarGroup radRibbonBarGroup8;
        private RadButtonElement radButtonElement9;
        private RadPageViewPage radPageViewPage4;
        private RadTreeView radTreeView4;
        private RadTreeView radTreeView3;
        private RadTreeView radTreeView6;
        private RadTreeView radTreeView5;
        private PictureBox pictureBox1;
        private RadContextMenu radContextMenu3;
        private RadMenuItem radMenuItem2;
        private RibbonTab ribbonTab6;
        private RadRibbonBarGroup radRibbonBarGroup9;
        private RadButtonElement radButtonElement21;
        private RadButtonElement radButtonElement22;
        private RadButtonElement radButtonElement24;
        private RadSplitContainer radSplitContainer3;
        private SplitPanel splitPanel6;
        private SplitPanel splitPanel7;
        private RadContextMenu radContextMenu4;
        private RadMenuItem radMenuItem3;
        private RadMenuItem radMenuItem4;
        private SplitPanel splitPanel8;
        private RadGridView radGridView4;
        private RadDropDownList radDropDownList1;
        private FlowLayoutPanel flowLayoutPanel2;
        private Panel panel2;
        private RadRibbonBarGroup radRibbonBarGroup2;
        private RadButtonElement radButtonElement4;
        private Label label1;
        private Label label2;
        private RadContextMenu radContextMenu5;
        private RadMenuItem radMenuItem5;
        private PictureBox pictureBox2;
        private Label label3;
    }
}
