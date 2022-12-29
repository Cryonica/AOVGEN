using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AOVGEN.Properties;
using AutoCAD;
using AutoUpdaterDotNET;
using ClosedXML.Excel;
using GKS_ASU_Loader;
using Telerik.WinControls;
using Telerik.WinControls.Data;
using Telerik.WinControls.UI;
using Telerik.WinControls.UI.Localization;
using WinFormAnimation;
using Path = WinFormAnimation.Path;
using PositionChangedEventArgs = Telerik.WinControls.UI.Data.PositionChangedEventArgs;
using Timer = System.Windows.Forms.Timer;
using static AOVGEN.RunRevitCommand;

namespace AOVGEN
{
#pragma warning disable IDE1006
    public partial class MainForm : RadForm
    {
        public enum RevitState
        {
            Open,
            Close,
            DocumentPresent
        }
        

        public SQLiteConnection connection;
        public SQLiteConnection connectionvendor;
        private bool mouseDown;
        private RadContextMenu contextMenu;
        private RadContextMenu cablecontextMenu;
        public string DBFileName { get; set; }
        public string DBFilePath { get; set; }
        public string DBType { get; set; }
        private string tableExtDocName { get; set; }
        

        private readonly string Author;
        public IRevitExternalService Service { get; set; }
        private RevitState RevitConnectionState { get; set; }
        protected class ElementExistException : Exception
        {
            public ElementExistException(string elementname, string ventsystemname)
                : base(string.Format($"Отключите от шкафа и подключите вентустановку снова, \n проблема  в вентсистеме: {ventsystemname} \n элемент: {elementname}", elementname))
            {
               
            }
        }

        internal Timer timer;
     

        [DllImport("user32.dll")]
        internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);


        public MainForm(string dbfilename, string dbfilepath, string DataBaseType, IRevitExternalService externalService, string author)
        {
            DBFileName = dbfilename;
            DBFilePath = dbfilepath;
            DBType = DataBaseType;
            Service = externalService;
            Author = author;
            
            InitializeComponent();
            
            
            radRibbonBar1.RibbonBarElement.ApplicationButtonElement.Visibility = ElementVisibility.Collapsed; //круглая кнопка
            //this.radRibbonBar1.RibbonBarElement.QuickAccessToolBar.Visibility = Telerik.WinControls.ElementVisibility.Collapsed;
            //this.radRibbonBar1.RibbonBarElement.CaptionFill.Visibility = Telerik.WinControls.ElementVisibility.Collapsed;
            radRibbonBar1.RibbonBarElement.RibbonCaption.Visibility = ElementVisibility.Collapsed; //верхняя строка 
            radRibbonBar1.RibbonBarElement.CaptionBorder.Visibility = ElementVisibility.Collapsed;
            //this.radRibbonBar1.RibbonBarElement.TabStripElement.ItemContainer.Margin = new Padding(5, 0, 76, 0);
            radRibbonBar1.RibbonBarElement.TabStripElement.ItemContainer.Padding = new Padding(5, 0, 76, 0);
            RunRevitCommand revitCommand = new RunRevitCommand(Service);
            
            revitCommand.SetUIApp();
            Thread.Sleep(TimeSpan.FromMilliseconds(1));
            CheckRevitStaus();
            timer = new Timer();
            timer.Interval = (int)TimeSpan.FromMinutes(10).TotalMilliseconds;
            timer.Tick += delegate(object sender, EventArgs e)
            {
               
                CheckRevitStaus();
            };
            // connect to Revit
            timer.Start();

            Icon = Resources.AOVGEN_white_ico;
            ribbonTab1.IsSelected = true;
            RadGridLocalizationProvider.CurrentProvider = new MyRussianRadGridLocalizationProvider();
            
            

        }

        public class RevitStatusArg: EventArgs
        {
            public RevitState revitState { get; set; }

            public RevitStatusArg(RevitState state)     
            {
                this.revitState = state;
            }


        }

        private void CheckRevitStaus()
        {
            try
            {

                //var t = CheckStaus().Status;


                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                var token = cts.Token;
                RunRevitCommand revitCommand = new RunRevitCommand(Service);
                revitCommand.StatusChanged += (s, args) =>
                {

                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                    var docArgs = args as RunRevitCommand.StatuChangeArgs;
                    if (!string.IsNullOrEmpty(docArgs?.docinfo))
                    {
                        pictureBox2.Image = Resources.green_light;
                        RevitConnectionState = RevitState.DocumentPresent;
                       
                    }
                    else
                    {
                        pictureBox2.Image = Resources.red_light;
                       
                        RevitConnectionState = RevitState.Close;
                        
                        
                    }
                };


                Task<RevitStatusArg>.Factory.StartNew(() =>
                {

                    try
                    {
                        // occasionally, execute this line:
                        var docinfo = revitCommand.StateInfo().Item1;

                        token.ThrowIfCancellationRequested();

                    }
                    catch (OperationCanceledException)
                    {
                        var docinfo = revitCommand.StateInfo().Item1;
                        if (string.IsNullOrEmpty(docinfo))
                        {
                            RevitStatusArg arg = new RevitStatusArg(RevitState.Close);



                            return arg;
                        }
                        else
                        {
                            RevitConnectionState = RevitState.DocumentPresent;
                            RevitStatusArg arg = new RevitStatusArg(RevitState.DocumentPresent);

                            return arg;
                        }




                    }
                    catch (Exception ex)
                    {
                        return new RevitStatusArg(RevitState.Close);
                    }

                    return new RevitStatusArg(RevitState.Open);

                }).ContinueWith((x) => RevitConnectionState, TaskScheduler.FromCurrentSynchronizationContext());


                // Thread.Sleep(1000);
                //if (t.Status == TaskStatus.Canceled)
                //{
                //    RevitConnectionState = RevitState.Close;
                //}
                //else
                //{
                //     var (docId, _) = revitCommand.StateInfo();
                //    RevitConnectionState = string.IsNullOrEmpty(docId) ? RevitState.Open : RevitState.DocumentPresent;
                // }



            }
            catch (Exception ex)
            {
                if (ex.HResult == -2146233087)
                {
                    RevitConnectionState = RevitState.Close;
                }
            }

        }

        public MainForm(string dbfilename, string dbfilepath, string DataBaseType, string author)
        {
            DBFileName = dbfilename;
            DBFilePath = dbfilepath;
            DBType = DataBaseType;
            Author = author;
            InitializeComponent();
            Icon = Resources.AOVGEN_white_ico;
            ribbonTab1.IsSelected = true;            
            RadGridLocalizationProvider.CurrentProvider = new MyRussianRadGridLocalizationProvider();
           
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            radTreeViewMenuItem1.Click += RadTreeViewMenuItem1_Click1;
            radTreeViewMenuItem2.Click += RadTreeViewMenuItem2_Click;
            radTreeViewMenuItem3.Click += RadTreeViewMenuItem3_Click;
            radTreeViewMenuItem4.Click += RadTreeViewMenuItem4_Click;
            radMenuItem1.Click += RaMenuItem1_Click;
            radMenuItem2.Click += RadTreeView5MenuItem2_Click;
            radMenuItem5.Click += RadMenuItem5_Click;
           
            //в Levels лежат уровни, считанные из ревит
            //foreach (string level in Levels.Values)

            var firstproject = (from t in radTreeView1.Nodes
                                where t.Parent == null
                                select t).FirstOrDefault();            
            //radTreeView1.NodeMouseClick += new RadTreeView.TreeViewEventHandler(nodeclick);
            if (firstproject != null)
            {
                firstproject.Expand();
                //nodeclick(firstproject, null);
                UpdateBuildNode(firstproject);
                radTreeView1.SelectedNode = firstproject;
                radPageView1.SelectedPage = radPageViewPage1;
            }
            label2.Text = Author;
            

        }
        private void RadTreeViewMenuItem4_Click(object sender, EventArgs e)
        {
            radButtonElement8.PerformClick();
        }
        private void RadTreeViewMenuItem3_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(radTreeView2.SelectedNode.Tag is VentSystem ventSystem)) return;
                RadTreeNode radTreeNode = radTreeView1.SelectedNode;
                if (!(radTreeNode?.Tag is Building)) return;
                RadTreeNode parent = radTreeNode.Parent;

                if (parent.Tag is Project project)
                {
                    EditorV2 editor = new EditorV2(this)
                    {
                        Opacity = 0,
                        DBFilePath = DBFilePath,
                        Ventree = radTreeView2,
                        Projecttree = radTreeView1,
                        projectGuid = project.GetGUID(),
                        VentSystem = ventSystem,
                        OpenForEdit = true,
                        SelectedNode = radTreeView2.SelectedNode,
                        Joinedsystems = radTreeView4.Nodes,
                        connection = connection


                    };

                    DialogResult _ = editor.ShowDialog();
                    editor = null;
                }

                GC.Collect();
            }
            catch
            {
                // ignored
            }

            //MessageBox.Show("После редактирования вент.системы следует заново подключить ее к шкафу управления");

        }
        private void RadTreeViewMenuItem2_Click(object sender, EventArgs e)
        {
            //Copying VentSystem
            if (radTreeView2.SelectedNode.Tag is VentSystem ventSystem)
            {
                MakeVentSystemCopy(ventSystem);
            }

        }
        private void RadTreeViewMenuItem1_Click1(object sender, EventArgs e)
        {
            //Rename VentSystem
            if (!(radTreeView2.SelectedNode.Tag is VentSystem)) return;
            radTreeView2.AllowEdit = true;
            radTreeView2.BeginEdit();
            RadTreeNode editednode = radTreeView2.SelectedNode;
            editednode.BeginEdit();

            //MessageBox.Show(ventSystem.SystemName + "; GUID:" + ventSystem.GUID);
            //MessageBox.Show("1");
        }
        private void RaMenuItem1_Click(object sender, EventArgs e)
        {
            RadTreeNode selectednode = radTreeView4.SelectedNode;
            if (selectednode != null)
            {
                Pannel pannel;
                if (selectednode.Level == 0)
                {
                    pannel = (Pannel)selectednode.Tag;
                    if (selectednode.Nodes.Count >0)
                    {
                        List<VentSystem> ventlist = new List<VentSystem>();

                        foreach(var ventnode in selectednode.Nodes)
                        {
                            VentSystem ventSystem = ventnode.Tag as VentSystem;
                            ventlist.Add(ventSystem);
                            RadTreeNode clone = (RadTreeNode)ventnode.Clone();
                            RadTreeNode ventsystemnode = FindNodeByName(ventnode.Name, radTreeView2.Nodes);
                            clone.ForeColor = Color.Black;
                            if (ventsystemnode != null) ventsystemnode.ForeColor = Color.Black;
                            radTreeView3.Nodes.Add(clone);
                        }
                        
                        foreach(var ventSystem in ventlist)
                        {
                            ventSystem.ConnectedTo = string.Empty;
                            selectednode.Nodes.Remove(ventSystem.GUID);
                            UpdateVentSystem(ventSystem.GUID);
                        }
                        Task.Factory.StartNew(() =>
                            pannel.Power = EditorV2.UpdatePannelPower(selectednode, connection.ConnectionString)
                        );
                        Task.Factory.StartNew(() => 
                            pannel.Voltage = EditorV2.UpdatePannelVoltage(selectednode, connection.ConnectionString)
                        );
                        radTreeView4.SelectedNode = null;
                        radGridView3.DataSource = null;
                    }
                }
                else
                {

                    pannel = (Pannel)selectednode.Parent.Tag;

                    if (selectednode.Tag is VentSystem ventSystem) ventSystem.ConnectedTo = string.Empty;
                    RadTreeNode clone = (RadTreeNode)selectednode.Clone();
                    RadTreeNode ventsystemnode = FindNodeByName(selectednode.Name, radTreeView2.Nodes);
                    clone.ForeColor = Color.Black;
                    if (ventsystemnode != null) ventsystemnode.ForeColor = Color.Black;
                    radTreeView3.Nodes.Add(clone);
                    selectednode.Remove();
                    RadTreeNode PannelNode = FindNodeByName(pannel.GetGUID(), radTreeView4.Nodes);
                    if (PannelNode != null)
                    {
                        Task.Factory.StartNew(() =>
                        pannel.Power = EditorV2.UpdatePannelPower(PannelNode, connection.ConnectionString).ToString(CultureInfo.InvariantCulture));
                        Task.Factory.StartNew(() =>
                            pannel.Voltage = EditorV2.UpdatePannelVoltage(PannelNode, connection.ConnectionString)
                        );

                    }
                    radPropertyGrid1.SelectedObject = pannel;
                    

                    UpdateVentSystemAndPannel(selectednode.Name);

                    //find maximumvoltage

                    string findventsGUID = $"Select GUID FROM VentSystems WHERE Pannel = '{pannel.GetGUID()}'";
                    SQLiteCommand command = new SQLiteCommand
                    {
                        Connection = connection,
                        CommandText = findventsGUID
                    };
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
                        VSGuids.AsParallel().ForAll(GUID =>
                        {
                            RadTreeNode VSnode = FindNodeByName(GUID, radTreeView2.Nodes);
                            if (VSnode == null) return;
                            VentSystem vent = (VentSystem)VSnode.Tag;
                            if (GetVetSystemMaxVoltage(vent) == Pannel._Voltage.AC380) _Voltage = Pannel._Voltage.AC380;
                        });
                        pannel.Voltage = _Voltage;

                    }
                    else
                    { pannel.Voltage = Pannel._Voltage.AC220; }


                    UpdateVentSystemAndPannel(selectednode.Name);

                }
            }
            void UpdateVentSystemAndPannel(string ventsystemGUID)
            {
                try
                {
                    if (connection.State != ConnectionState.Open) connection.Open();
                    string sqldelete1 = $"UPDATE VentSystems SET Pannel = '{string.Empty}', PannelName = '{string.Empty}' WHERE [GUID] = '{ventsystemGUID}'";
                    string sqldelete2 = $"UPDATE Cable SET FromPannel = '{string.Empty}', FromGUID = '{string.Empty}' WHERE [SystemGUID] = '{ventsystemGUID}'";
                    string setdefaultnames = $"Update Cable SET [To] = DefaultName WHERE SystemGUID = '{ventsystemGUID}'";
                    //string setpannelpower = $"Update Pannel SET Power = '{pannel.Power}' WHERE GUID = '{pannel.GetGUID()}'"; //записать в БД новую мощность шкафа управления
                    SQLiteCommand command = new  SQLiteCommand(sqldelete1, connection);
                    try
                    {
                        command.ExecuteNonQuery();
                        command.CommandText = sqldelete1;
                        command.ExecuteNonQuery();
                        command.CommandText = sqldelete2;
                        command.ExecuteNonQuery();
                        command.CommandText = setdefaultnames;
                        command.ExecuteNonQuery();
                        //command.CommandText = setpannelpower;
                        //command.ExecuteNonQuery();
                        command.Dispose();
                            
                        radTreeView4.SelectedNode = null;
                        radGridView3.DataSource = null;
                    }
                    catch
                    {
                        // ignored
                    }
                }
                catch
                {
                    // ignored
                }
            }
            void UpdateVentSystem(string ventsystemGUID)
            {
                try
                {

                    if (connection.State == ConnectionState.Open)
                    {
                        string sqldelete1 = $"UPDATE VentSystems SET Pannel = '{string.Empty}', PannelName = '{string.Empty}' WHERE [GUID] = '{ventsystemGUID}'";
                        string sqldelete2 = $"UPDATE Cable SET FromPannel = '{string.Empty}', FromGUID = '{string.Empty}' WHERE [SystemGUID] = '{ventsystemGUID}'";
                        string setdefaultnames = $"Update Cable SET [To] = DefaultName WHERE SystemGUID = '{ventsystemGUID}'";
                        SQLiteCommand command = new SQLiteCommand(sqldelete1, connection);
                        try
                        {
                            command.ExecuteNonQuery();
                            command.CommandText = sqldelete1;
                            command.ExecuteNonQuery();
                            command.CommandText = sqldelete2;
                            command.ExecuteNonQuery();
                            command.CommandText = setdefaultnames;
                            command.ExecuteNonQuery();
                            command.Dispose();

                        }
                        catch
                        {

                        }

                    }
                }
                catch { }
            }
        }
        private void RadTreeView5MenuItem2_Click(object sender, EventArgs e)
        {
            RadTreeNode selectednode = radTreeView5.SelectedNode;
            if (selectednode.Nodes.Count>0)
            {
                //несколько шкафов
                List<RadTreeNode> DeleteNodeList = new List<RadTreeNode>();
                selectednode.Nodes.AsParallel().ForAll(pannelnode =>
                {
                    Pannel pannel = (Pannel)pannelnode.Tag;
                    DeleteNodeList.Add(pannelnode);
                    //this.radTreeView6.Nodes.Add(clone);
                    ClearPannelLevel(pannel);

                });
                DeleteNodeList.AsParallel().ForAll(radTreeNode =>
                {
                    RadTreeNode clone = (RadTreeNode)radTreeNode.Clone();
                    Font RegularFont = new Font(radTreeView6.Font, FontStyle.Regular);
                    clone.Font = RegularFont;
                    radTreeView6.Nodes.Add(clone);
                    selectednode.Nodes.Remove(radTreeNode);

                });
                radTreeView5.Update();
            }
            else
            {
                //1 шкаф
                Pannel pannel = (Pannel)selectednode.Tag;
                RadTreeNode clone = (RadTreeNode)selectednode.Clone();
                Font RegularFont = new Font(radTreeView6.Font, FontStyle.Regular);
                clone.Font = RegularFont;
                radTreeView6.Nodes.Add(clone);
                ClearPannelLevel(pannel);
                selectednode.Remove();

                //label1.Text = $"levelGUID = {LevelGUID}\n" + $"roomGUID = {RoomGUID}\n" + $"roominfo = {RoomInfo.Item1}; {RoomInfo.Item2}";
            }
            void ClearPannelLevel(Pannel pannel)
            {
                string clearpannelqury = $"UPDATE Pannel SET [LevelGUID] = NULL, [RoomGUID] = NULL, RoomName = NULL, RoomNumber = NULL WHERE [GUID] = '{pannel.GetGUID()}'";
                SQLiteCommand command = new SQLiteCommand
                {
                    Connection = connection,
                    CommandText = clearpannelqury
                };
                try
                {
                    command.ExecuteNonQuery();
                }
                catch
                {
                    // ignored
                }
            }
            
        }
        private void RadMenuItem5_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Очистка вендора пока не реализована");
        }
        public class MyRussianRadGridLocalizationProvider : RadGridLocalizationProvider
        {
            public override string GetLocalizedString(string id)
            {
                switch (id)
                {
                    case RadGridStringId.ConditionalFormattingPleaseSelectValidCellValue: return "Please select valid cell value";
                    case RadGridStringId.ConditionalFormattingPleaseSetValidCellValue: return "Please set a valid cell value";
                    case RadGridStringId.ConditionalFormattingPleaseSetValidCellValues: return "Please set a valid cell values";
                    case RadGridStringId.ConditionalFormattingPleaseSetValidExpression: return "Please set a valid expression";
                    case RadGridStringId.ConditionalFormattingItem: return "Элемент";
                    case RadGridStringId.ConditionalFormattingInvalidParameters: return "Invalid parameters";
                    case RadGridStringId.FilterFunctionBetween: return "Между";
                    case RadGridStringId.FilterFunctionContains: return "Содержит";
                    case RadGridStringId.FilterFunctionDoesNotContain: return "Не соержит";
                    case RadGridStringId.FilterFunctionEndsWith: return "Заканчивается с";
                    case RadGridStringId.FilterFunctionEqualTo: return "Равно";
                    case RadGridStringId.FilterFunctionGreaterThan: return "Больше чем";
                    case RadGridStringId.FilterFunctionGreaterThanOrEqualTo: return "Больше чем или равно";
                    case RadGridStringId.FilterFunctionIsEmpty: return "Пусто";
                    case RadGridStringId.FilterFunctionIsNull: return "Не существует";
                    case RadGridStringId.FilterFunctionLessThan: return "Less than";
                    case RadGridStringId.FilterFunctionLessThanOrEqualTo: return "Больше чем или равно";
                    case RadGridStringId.FilterFunctionNoFilter: return "Без фильтра";
                    case RadGridStringId.FilterFunctionNotBetween: return "Не между";
                    case RadGridStringId.FilterFunctionNotEqualTo: return "Не равно";
                    case RadGridStringId.FilterFunctionNotIsEmpty: return "Не пусто";
                    case RadGridStringId.FilterFunctionNotIsNull: return "Существует";
                    case RadGridStringId.FilterFunctionStartsWith: return "Начинается с";
                    case RadGridStringId.FilterFunctionCustom: return "Пользователь";
                    case RadGridStringId.FilterOperatorBetween: return "Между";
                    case RadGridStringId.FilterOperatorContains: return "Содержит";
                    case RadGridStringId.FilterOperatorDoesNotContain: return "Не содержит";
                    case RadGridStringId.FilterOperatorEndsWith: return "Оканчивается с";
                    case RadGridStringId.FilterOperatorEqualTo: return "Равны";
                    case RadGridStringId.FilterOperatorGreaterThan: return "Больше чем";
                    case RadGridStringId.FilterOperatorGreaterThanOrEqualTo: return "Больше чем или равно";
                    case RadGridStringId.FilterOperatorIsEmpty: return "Пусто";
                    case RadGridStringId.FilterOperatorIsNull: return "Не существует";
                    case RadGridStringId.FilterOperatorLessThan: return "Меньше чем";
                    case RadGridStringId.FilterOperatorLessThanOrEqualTo: return "Больше чем или равно";
                    case RadGridStringId.FilterOperatorNoFilter: return "Без фильтра";
                    case RadGridStringId.FilterOperatorNotBetween: return "Не между";
                    case RadGridStringId.FilterOperatorNotEqualTo: return "Не равно";
                    case RadGridStringId.FilterOperatorNotIsEmpty: return "Не пусто";
                    case RadGridStringId.FilterOperatorNotIsNull: return "Существует";
                    case RadGridStringId.FilterOperatorStartsWith: return "Начинается с";
                    case RadGridStringId.FilterOperatorIsLike: return "Подобно";
                    case RadGridStringId.FilterOperatorNotIsLike: return "Не подобно";
                    case RadGridStringId.FilterOperatorIsContainedIn: return "Содержится в";
                    case RadGridStringId.FilterOperatorNotIsContainedIn: return "Не содержится в";
                    case RadGridStringId.FilterOperatorCustom: return "Пользователь";
                    case RadGridStringId.CustomFilterMenuItem: return "Пользователь";
                    case RadGridStringId.CustomFilterDialogCaption: return "RadGridView Фильтр диалог [{0}]";
                    case RadGridStringId.CustomFilterDialogLabel: return "Показать строки где:";
                    case RadGridStringId.CustomFilterDialogRbAnd: return "И";
                    case RadGridStringId.CustomFilterDialogRbOr: return "Или";
                    case RadGridStringId.CustomFilterDialogBtnOk: return "OK";
                    case RadGridStringId.CustomFilterDialogBtnCancel: return "Отменить";
                    case RadGridStringId.CustomFilterDialogCheckBoxNot: return "Нет";
                    case RadGridStringId.CustomFilterDialogTrue: return "Правда";
                    case RadGridStringId.CustomFilterDialogFalse: return "Ложь";
                    case RadGridStringId.FilterMenuBlanks: return "Пусто";
                    case RadGridStringId.FilterMenuAvailableFilters: return "Доступные фильтры";
                    case RadGridStringId.FilterMenuSearchBoxText: return "Поиск...";
                    case RadGridStringId.FilterMenuClearFilters: return "Очистить фильтр";
                    case RadGridStringId.FilterMenuButtonOK: return "OK";
                    case RadGridStringId.FilterMenuButtonCancel: return "Отмена";
                    case RadGridStringId.FilterMenuSelectionAll: return "Все";
                    case RadGridStringId.FilterMenuSelectionAllSearched: return "Результат поиска всего";
                    case RadGridStringId.FilterMenuSelectionNull: return "Не существует";
                    case RadGridStringId.FilterMenuSelectionNotNull: return "Существует";
                    case RadGridStringId.FilterFunctionSelectedDates: return "Фильтр по указанным датам:";
                    case RadGridStringId.FilterFunctionToday: return "Today";
                    case RadGridStringId.FilterFunctionYesterday: return "Yesterday";
                    case RadGridStringId.FilterFunctionDuringLast7days: return "В течении последних 7 дней";
                    case RadGridStringId.FilterLogicalOperatorAnd: return "И";
                    case RadGridStringId.FilterLogicalOperatorOr: return "ИЛИ";
                    case RadGridStringId.FilterCompositeNotOperator: return "НЕТ";
                    case RadGridStringId.DeleteRowMenuItem: return "Удалить строку";
                    case RadGridStringId.SortAscendingMenuItem: return "Соритоварать по возрастанию";
                    case RadGridStringId.SortDescendingMenuItem: return "Сориторвать по убыванию";
                    case RadGridStringId.ClearSortingMenuItem: return "Очистить сортировку";
                    case RadGridStringId.ConditionalFormattingMenuItem: return "Условия форматирования";
                    case RadGridStringId.GroupByThisColumnMenuItem: return "Группировка по этой колонке";
                    case RadGridStringId.UngroupThisColumn: return "разгруппировать эту колонку";
                    case RadGridStringId.ColumnChooserMenuItem: return "Селектор колонки";
                    case RadGridStringId.HideMenuItem: return "Скрыть колонку";
                    case RadGridStringId.HideGroupMenuItem: return "Скрыть группу";
                    case RadGridStringId.UnpinMenuItem: return "Unpin Column";
                    case RadGridStringId.UnpinRowMenuItem: return "Unpin Row";
                    case RadGridStringId.PinMenuItem: return "Pinned state";
                    case RadGridStringId.PinAtLeftMenuItem: return "Pin at left";
                    case RadGridStringId.PinAtRightMenuItem: return "Pin at right";
                    case RadGridStringId.PinAtBottomMenuItem: return "Pin at bottom";
                    case RadGridStringId.PinAtTopMenuItem: return "Pin at top";
                    case RadGridStringId.BestFitMenuItem: return "Best Fit";
                    case RadGridStringId.PasteMenuItem: return "Paste";
                    case RadGridStringId.EditMenuItem: return "Edit";
                    case RadGridStringId.ClearValueMenuItem: return "Clear Value";
                    case RadGridStringId.CopyMenuItem: return "Copy";
                    case RadGridStringId.CutMenuItem: return "Cut";
                    case RadGridStringId.AddNewRowString: return "Click here to add a new row";
                    case RadGridStringId.ConditionalFormattingSortAlphabetically: return "Sort columns alphabetically";
                    case RadGridStringId.ConditionalFormattingCaption: return "Conditional Formatting Rules Manager";
                    case RadGridStringId.ConditionalFormattingLblColumn: return "Format only cells with";
                    case RadGridStringId.ConditionalFormattingLblName: return "Rule name";
                    case RadGridStringId.ConditionalFormattingLblType: return "Cell value";
                    case RadGridStringId.ConditionalFormattingLblValue1: return "Value 1";
                    case RadGridStringId.ConditionalFormattingLblValue2: return "Value 2";
                    case RadGridStringId.ConditionalFormattingGrpConditions: return "Rules";
                    case RadGridStringId.ConditionalFormattingGrpProperties: return "Rule Properties";
                    case RadGridStringId.ConditionalFormattingChkApplyToRow: return "Apply this formatting to entire row";
                    case RadGridStringId.ConditionalFormattingChkApplyOnSelectedRows: return "Apply this formatting if the row is selected";
                    case RadGridStringId.ConditionalFormattingBtnAdd: return "Add new rule";
                    case RadGridStringId.ConditionalFormattingBtnRemove: return "Remove";
                    case RadGridStringId.ConditionalFormattingBtnOK: return "OK";
                    case RadGridStringId.ConditionalFormattingBtnCancel: return "Cancel";
                    case RadGridStringId.ConditionalFormattingBtnApply: return "Apply";
                    case RadGridStringId.ConditionalFormattingRuleAppliesOn: return "Rule applies to";
                    case RadGridStringId.ConditionalFormattingCondition: return "Condition";
                    case RadGridStringId.ConditionalFormattingExpression: return "Expression";
                    case RadGridStringId.ConditionalFormattingChooseOne: return "[Choose one]";
                    case RadGridStringId.ConditionalFormattingEqualsTo: return "equals to [Value1]";
                    case RadGridStringId.ConditionalFormattingIsNotEqualTo: return "is not equal to [Value1]";
                    case RadGridStringId.ConditionalFormattingStartsWith: return "starts with [Value1]";
                    case RadGridStringId.ConditionalFormattingEndsWith: return "ends with [Value1]";
                    case RadGridStringId.ConditionalFormattingContains: return "contains [Value1]";
                    case RadGridStringId.ConditionalFormattingDoesNotContain: return "does not contain [Value1]";
                    case RadGridStringId.ConditionalFormattingIsGreaterThan: return "is greater than [Value1]";
                    case RadGridStringId.ConditionalFormattingIsGreaterThanOrEqual: return "is greater than or equal [Value1]";
                    case RadGridStringId.ConditionalFormattingIsLessThan: return "is less than [Value1]";
                    case RadGridStringId.ConditionalFormattingIsLessThanOrEqual: return "is less than or equal to [Value1]";
                    case RadGridStringId.ConditionalFormattingIsBetween: return "is between [Value1] and [Value2]";
                    case RadGridStringId.ConditionalFormattingIsNotBetween: return "is not between [Value1] and [Value1]";
                    case RadGridStringId.ConditionalFormattingLblFormat: return "Format";
                    case RadGridStringId.ConditionalFormattingBtnExpression: return "Expression editor";
                    case RadGridStringId.ConditionalFormattingTextBoxExpression: return "Expression";
                    case RadGridStringId.ConditionalFormattingPropertyGridCaseSensitive: return "CaseSensitive";
                    case RadGridStringId.ConditionalFormattingPropertyGridCellBackColor: return "CellBackColor";
                    case RadGridStringId.ConditionalFormattingPropertyGridCellForeColor: return "CellForeColor";
                    case RadGridStringId.ConditionalFormattingPropertyGridEnabled: return "Enabled";
                    case RadGridStringId.ConditionalFormattingPropertyGridRowBackColor: return "RowBackColor";
                    case RadGridStringId.ConditionalFormattingPropertyGridRowForeColor: return "RowForeColor";
                    case RadGridStringId.ConditionalFormattingPropertyGridRowTextAlignment: return "RowTextAlignment";
                    case RadGridStringId.ConditionalFormattingPropertyGridTextAlignment: return "TextAlignment";
                    case RadGridStringId.ConditionalFormattingPropertyGridCellFont: return "My Cell Font";
                    case RadGridStringId.ConditionalFormattingPropertyGridCellFontDescription: return "My Font Description";
                    case RadGridStringId.ConditionalFormattingPropertyGridCaseSensitiveDescription: return "Determines whether case-sensitive comparisons will be made when evaluating string values.";
                    case RadGridStringId.ConditionalFormattingPropertyGridCellBackColorDescription: return "Enter the background color to be used for the cell.";
                    case RadGridStringId.ConditionalFormattingPropertyGridCellForeColorDescription: return "Enter the foreground color to be used for the cell.";
                    case RadGridStringId.ConditionalFormattingPropertyGridEnabledDescription: return "Determines whether the condition is enabled (can be evaluated and applied).";
                    case RadGridStringId.ConditionalFormattingPropertyGridRowBackColorDescription: return "Enter the background color to be used for the entire row.";
                    case RadGridStringId.ConditionalFormattingPropertyGridRowForeColorDescription: return "Enter the foreground color to be used for the entire row.";
                    case RadGridStringId.ConditionalFormattingPropertyGridRowTextAlignmentDescription: return "Enter the alignment to be used for the cell values, when ApplyToRow is true.";
                    case RadGridStringId.ConditionalFormattingPropertyGridTextAlignmentDescription: return "Enter the alignment to be used for the cell values.";
                    case RadGridStringId.ColumnChooserFormCaption: return "Column Chooser";
                    case RadGridStringId.ColumnChooserFormMessage: return "Drag a column header from the\ngrid here to remove it from\nthe current view.";
                    case RadGridStringId.GroupingPanelDefaultMessage: return "Перетащите колонку для группировки по этой колонке.";
                    case RadGridStringId.GroupingPanelHeader: return "Группировка по:";
                    case RadGridStringId.PagingPanelPagesLabel: return "Page";
                    case RadGridStringId.PagingPanelOfPagesLabel: return "of";
                    case RadGridStringId.NoDataText: return "Нет данных для отображения";
                    case RadGridStringId.CompositeFilterFormErrorCaption: return "Filter Error";
                    case RadGridStringId.CompositeFilterFormInvalidFilter: return "The composite filter descriptor is not valid.";
                    case RadGridStringId.ExpressionMenuItem: return "Expression";
                    case RadGridStringId.ExpressionFormTitle: return "Expression Builder";
                    case RadGridStringId.ExpressionFormFunctions: return "Functions";
                    case RadGridStringId.ExpressionFormFunctionsText: return "Text";
                    case RadGridStringId.ExpressionFormFunctionsAggregate: return "Aggregate";
                    case RadGridStringId.ExpressionFormFunctionsDateTime: return "Date-Time";
                    case RadGridStringId.ExpressionFormFunctionsLogical: return "Logical";
                    case RadGridStringId.ExpressionFormFunctionsMath: return "Math";
                    case RadGridStringId.ExpressionFormFunctionsOther: return "Other";
                    case RadGridStringId.ExpressionFormOperators: return "Operators";
                    case RadGridStringId.ExpressionFormConstants: return "Constants";
                    case RadGridStringId.ExpressionFormFields: return "Fields";
                    case RadGridStringId.ExpressionFormDescription: return "Description";
                    case RadGridStringId.ExpressionFormResultPreview: return "Result preview";
                    case RadGridStringId.ExpressionFormTooltipPlus: return "Plus";
                    case RadGridStringId.ExpressionFormTooltipMinus: return "Minus";
                    case RadGridStringId.ExpressionFormTooltipMultiply: return "Multiply";
                    case RadGridStringId.ExpressionFormTooltipDivide: return "Divide";
                    case RadGridStringId.ExpressionFormTooltipModulo: return "Modulo";
                    case RadGridStringId.ExpressionFormTooltipEqual: return "Equal";
                    case RadGridStringId.ExpressionFormTooltipNotEqual: return "Not Equal";
                    case RadGridStringId.ExpressionFormTooltipLess: return "Less";
                    case RadGridStringId.ExpressionFormTooltipLessOrEqual: return "Less Or Equal";
                    case RadGridStringId.ExpressionFormTooltipGreaterOrEqual: return "Greater Or Equal";
                    case RadGridStringId.ExpressionFormTooltipGreater: return "Greater";
                    case RadGridStringId.ExpressionFormTooltipAnd: return "Logical \"AND\"";
                    case RadGridStringId.ExpressionFormTooltipOr: return "Logical \"OR\"";
                    case RadGridStringId.ExpressionFormTooltipNot: return "Logical \"NOT\"";
                    case RadGridStringId.ExpressionFormAndButton: return string.Empty; //if empty, default button image is used
                    case RadGridStringId.ExpressionFormOrButton: return string.Empty; //if empty, default button image is used
                    case RadGridStringId.ExpressionFormNotButton: return string.Empty; //if empty, default button image is used
                    case RadGridStringId.ExpressionFormOKButton: return "OK";
                    case RadGridStringId.ExpressionFormCancelButton: return "Cancel";
                    case RadGridStringId.SearchRowChooseColumns: return "SearchRowChooseColumns";
                    case RadGridStringId.SearchRowSearchFromCurrentPosition: return "SearchRowSearchFromCurrentPosition";
                    case RadGridStringId.SearchRowMenuItemMasterTemplate: return "SearchRowMenuItemMasterTemplate";
                    case RadGridStringId.SearchRowMenuItemChildTemplates: return "SearchRowMenuItemChildTemplates";
                    case RadGridStringId.SearchRowMenuItemAllColumns: return "SearchRowMenuItemAllColumns";
                    case RadGridStringId.SearchRowTextBoxNullText: return "SearchRowTextBoxNullText";
                    case RadGridStringId.SearchRowResultsOfLabel: return "SearchRowResultsOfLabel";
                    case RadGridStringId.SearchRowMatchCase: return "Match case";
                }
                return string.Empty;
            }
        }
        internal struct Posnames
        {
            internal string Oldposname { get; set; }
            internal string Newposname { get; set; }
        }
        private void BunifuImageButton1_Click(object sender, EventArgs e)
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
        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (Opacity == 0)
            {
                connection.Close();
                connectionvendor.Close();
                Process.GetCurrentProcess().Kill();
                //Close();
            }
        }
        private void Timer2_Tick(object sender, EventArgs e)
        {
            
        }


        private void MainForm_Load(object sender, EventArgs e)
        {

            
            AutoUpdate();

            connection = OpenDB(DBFilePath);
            connection.Open();
            connectionvendor = OpenDB(DBFilePath.Replace(DBFileName, string.Empty) + "VendorsDB.db");
            connectionvendor.Open();
            VendorFill vendorFill = new VendorFill
            {
                connection = connectionvendor
            };
            vendorFill.FillVendorList(radDropDownList1);


            DataLoadToTree();
            MakeContextMenu1();
            MakeContextMenu2();
            radPageViewPage1.Item.Visibility = ElementVisibility.Collapsed;
            radPageViewPage2.Item.Visibility = ElementVisibility.Collapsed;
            radPageViewPage3.Item.Visibility = ElementVisibility.Collapsed;
            radPageViewPage4.Item.Visibility = ElementVisibility.Collapsed;
            radMenuItem3.Click += RadMenuItem3_Click;
            radMenuItem4.Click += RadMenuItem4_Click;
            label2.Text = Author;

            //test revit connection
            if (RevitConnectionState == RevitState.DocumentPresent)
            {
                pictureBox2.Image = Resources.green_light;
            }
            else
            {
                radRibbonBarGroup9.Enabled = false;
            }
        }

        private void RadMenuItem4_Click(object sender, EventArgs e)
        {
            try
            {
                RadTreeNode ventnode = radTreeView2.SelectedNode;

                if (connection.State == ConnectionState.Closed) connection.Open();
                if (!(ventnode.Tag is VentSystem ventSystem)) return;
                string ventguid = ventSystem.GUID;
                if (string.IsNullOrEmpty(ventguid)) return;
                List<string> dbtables = new List<string>
                {
                    "SensPDS",
                    "SensHUM",
                    "SensT",
                    "FControl"
                };
                SQLiteCommand clearvendorcommand = new SQLiteCommand
                {
                    Connection = connection
                };
                var t = (from cell in radGridView1.SelectedRows
                        let obj = cell.Tag
                        where (obj != null)
                        where (obj is IGetSensors)
                        select obj)
                    .ToDictionary(arg => arg, arg => ((IGetSensors)arg).GetSensors().Where(rs => rs != null).ToList());
                foreach (string dbtable in dbtables)
                {
                    clearvendorcommand.CommandText = $"UPDATE {dbtable} SET " +
                                                     "Vendor = NULL, " +
                                                     "VendorCode = NULL, " +
                                                     "VendorTable = NULL " +
                                                     $"WHERE [SystemGUID] = '{ventguid}'";
                    clearvendorcommand.ExecuteNonQuery();
                }
                foreach (var sensvendorinfo in t.SelectMany(keyValuePair => keyValuePair.Value).Where(sensvendorinfo => !string.IsNullOrEmpty(((IVendoInfo) sensvendorinfo)?.GetVendorInfo().ID)))
                {
                    ((IVendoInfo) sensvendorinfo).ClearVendorInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка:{ex.Message}; стэк:{ex.StackTrace}");
            }

            
            
        }

        private void RadMenuItem3_Click(object sender, EventArgs e)
        {
            try
            {
                List<(dynamic, string)> ListForApplyVendorInfo = new List<(dynamic, string)>();
                var t = (from cell in radGridView1.SelectedRows
                         let obj = cell.Tag
                         where (obj != null)
                         where (obj is IGetSensors)
                         
                         select obj)
                        .ToDictionary(arg => arg, arg => ((IGetSensors)arg).GetSensors()
                            .Where(rs=> rs != null)
                            .ToList());

                foreach (KeyValuePair<object, List<dynamic>> keyValuePair in t)
                {
                    foreach (dynamic sens in keyValuePair.Value)
                    {
                        var columnname = string.Empty;
                        IVendoInfo sensvendorinfo = sens;
                        
                        switch ((keyValuePair.Key).GetType().Name)
                        {
                            case nameof(OutdoorTemp):
                                columnname = "Sens1";
                                break;
                            case nameof(SupplyTemp):
                                columnname = "Sens2";
                                break;
                            case nameof(ExhaustTemp):
                                columnname = "Sens3";
                                break;
                            case nameof(IndoorTemp):
                                columnname = "Sens12";
                                break;
                            case nameof(WaterHeater):
                                if (sensvendorinfo != null)
                                {
                                    switch (sensvendorinfo.GetVendorInfo().DBTable)
                                    {
                                        case "SensTE":
                                            columnname = "Sens4";
                                            break;
                                        case "SensTS":
                                            columnname = "Sens5";
                                            break;
                                    }
                                }
                                break;
                            case nameof(Recuperator):
                                if (sensvendorinfo != null)
                                {
                                    switch (sensvendorinfo.GetVendorInfo().DBTable)
                                    {
                                        case "SensTS":
                                            columnname = "Sens6";
                                            break;
                                        case "SensPS":
                                            columnname = "Sens10";
                                            break;
                                    }
                                }
                                break;
                            case nameof(SupplyFiltr):
                            case nameof(ExtFiltr):
                                columnname = "Sens7";
                                break;
                            case nameof(SupplyVent):
                            case nameof(SpareSuplyVent):
                                if (sensvendorinfo != null)
                                {
                                    switch (sensvendorinfo.GetVendorInfo().DBTable)
                                    {
                                        case "SensPS":
                                        case "SensPE":
                                            columnname = "Sens8";
                                            break;
                                        case "FControl":
                                            columnname = "Sens13";
                                            break;
                                    }
                                }
                                break;
                            case nameof(ExtVent):
                            case nameof(SpareExtVent):
                                if (sensvendorinfo != null)
                                {
                                    switch (sensvendorinfo.GetVendorInfo().DBTable)
                                    {
                                        case "SensPS":
                                        case "SensPE":
                                            columnname = "Sens9";
                                            break;
                                        case "FControl":
                                            columnname = "Sens14";
                                            break;
                                    }
                                }
                                break;
                            case nameof(Humidifier):
                                columnname = "Sens11";
                                break;
                            case nameof(Froster):
                                continue;

                        }
                        ListForApplyVendorInfo.Add((sens, columnname));
                        //output = string.Format("Компонент:{0}; Инфу брать в таблице: {1}", keyValuePair.Key.GetType().Name + ": " + sens.GetType().Name, columnname);
                    }
                }
                //запрос в БД для чтения данных
                if (connectionvendor.State == ConnectionState.Closed) connectionvendor.Open();
                string VendorDBPath = DBFilePath.Replace(DBFileName, string.Empty) + "VendorsDB.db";
                Building building = (Building)radTreeView1.SelectedNode.Tag;
                SQLiteCommand commandvendor = new SQLiteCommand
                {
                    Connection = connection,
                    CommandText = $"ATTACH '{VendorDBPath}' as DB1; " +
                                "SELECT VendorName FROM db1.VendorPresets WHERE ID = " +
                                $"(SELECT VendorPreset FROM BuildSetting WHERE Place = '{building.BuildGUID}')"
                };
                string vendorname = string.Empty;
                using (SQLiteDataReader reader = commandvendor.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        vendorname = reader[0].ToString();
                    }
                }
                commandvendor.CommandText = "DETACH DB1";
                commandvendor.ExecuteNonQuery();
                
                foreach ((dynamic, string) element in ListForApplyVendorInfo)
                {
                    IVendoInfo vendoInfo = element.Item1;
                    string column = element.Item2;
                    commandvendor.Connection = connectionvendor;
                    if (vendoInfo == null || string.IsNullOrEmpty(column)) continue;
                    var tablename = vendoInfo.GetVendorInfo().DBTable;
                    commandvendor.CommandText = $"SELECT Description, Assignment, VendorCode, BlockName FROM {tablename} " +
                                                $"WHERE VendorCode = (SELECT {column} FROM VendorPresets WHERE VendorName = '{vendorname}')";
                    using (SQLiteDataReader reader = commandvendor.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            vendoInfo.SetVendorInfo(vendorname, reader[2].ToString(), reader[0].ToString(), null, reader[1].ToString());
                        }
                    }
                }

                commandvendor.Connection = connection;
                foreach ((dynamic, string) elementforsave in ListForApplyVendorInfo)
                {
                    var commondbtable = string.Empty;
                    ICompGUID elementGUID = elementforsave.Item1;
                    IVendoInfo elementvendorinfo = elementforsave.Item1;
                    var guid = elementGUID.GUID;
                    var dbtable = elementvendorinfo.GetVendorInfo().DBTable;
                    if (string.IsNullOrEmpty(guid)) continue;
                    switch (dbtable)
                    {
                        case "SensTE":
                        case "SensTS":
                            commondbtable = "SensT";
                            break;
                        case "SensPE":
                        case "SensPS":
                            commondbtable = "SensPDS";
                            break;
                        case "FControl":
                            commondbtable = "FControl";
                            break;
                        case "SensHE":
                        case "SensHS":
                            commondbtable = "SensHum";
                            break;
                    }

                    if (string.IsNullOrEmpty(commondbtable)) continue;
                    commandvendor.CommandText = $"UPDATE {commondbtable} SET " +
                                                $"Vendor = '{elementvendorinfo.GetVendorInfo().VendorName}', " +
                                                $"VendorCode = '{elementvendorinfo.GetVendorInfo().ID}', " +
                                                $"VendorTable = '{elementvendorinfo.GetVendorInfo().DBTable}' " +
                                                $"WHERE [GUID] = '{guid}'";
                    commandvendor.ExecuteNonQuery();

                }
                commandvendor.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Текст ошибки: {ex.Message}; Трассировка: {ex.StackTrace}");
            }
            
        }


        private void MakeContextMenu1()
        {
            contextMenu = new RadContextMenu();
            RadMenuItem menuItem1 = new RadMenuItem("Создаить по аналогии");
            menuItem1.Click += MenuItem1_Click;
            RadMenuItem menuItem2 = new RadMenuItem("Удалить")
            {
                ForeColor = Color.Red
            };
            menuItem2.Click += MenuItem2_Click;
            contextMenu.Items.Add(menuItem1);
            contextMenu.Items.Add(menuItem2);
        }

        private void MakeContextMenu2()
        {
            cablecontextMenu = new RadContextMenu();
            RadMenuItem cablesetnames = new RadMenuItem("Назначить имена");
            cablesetnames.Click += Cablesetnames_Click;
            RadMenuItem cablesettypes = new RadMenuItem("Установить тип кабелей");
            cablesettypes.Click += cablesettypes_Click;
            RadMenuItem cablelenghtset = new RadMenuItem("Установить длину");
            cablelenghtset.Click += Cablelenghtset_Click;
            RadMenuItem setcablelenghtinAcadDrawing = new RadMenuItem("Обновить длины у блоков в Autocad");
            setcablelenghtinAcadDrawing.Click += SetcablelenghtinAcadDrawing_Click;
            cablecontextMenu.Items.Add(cablesetnames);
            cablecontextMenu.Items.Add(cablesettypes);
            cablecontextMenu.Items.Add(cablelenghtset);
            cablecontextMenu.Items.Add(setcablelenghtinAcadDrawing);
        }
        

        private void SetcablelenghtinAcadDrawing_Click(object sender, EventArgs e)
        {
            var selectedcells = radGridView3.SelectedRows;
            var CabInfoList = selectedcells.AsParallel()
            .Select(q =>
            {
                (string GUID, string Name, string Type, string Cross, string Lenght) cabifo = (null, null, null, null, null);
                cabifo.GUID= q.Cells["GUID"].Value.ToString();
                cabifo.Name = q.Cells["Имя"].Value.ToString();
                cabifo.Type = q.Cells["Тип"].Value.ToString();
                cabifo.Lenght = q.Cells["Длина"].Value.ToString();

                return cabifo;
            })
            .ToList();
            
            
            const string progId = "AutoCAD.Application";
            try
            {
                dynamic cadApp = Marshal.GetActiveObject(progId);
                if (cadApp == null) return;
                Process[] localByName = Process.GetProcessesByName("acad");
                Process process = localByName[0];
                IntPtr hWnd = process.MainWindowHandle;

                if (hWnd == IntPtr.Zero) return;
                AcadApplication acadApp = cadApp;
                SetForegroundWindow(hWnd);
                acadApp.WindowState = AcWindowState.acMax;
                if (acadApp.Documents.Count <= 0) return;
                bool flagFindDraw = false;


                foreach (AcadDocument acadDocument in acadApp.Documents)
                {
                    if (!flagFindDraw)
                    {
                        
                        AcadBlocks blocks = acadDocument.Blocks;
                        //acadDoc.Activate();
                        if (blocks.Count <= 0) continue;
                        AcadBlock shapka = (from AcadBlock block in blocks.AsParallel()
                            where block.Name == "Шапка-11"
                            select block).FirstOrDefault();
                        if (shapka == null) continue;
                        acadDocument.Activate();
                        SendKeys.Send("{ESC}");
                        var acdb = acadDocument.Database;
                        List<AcadBlockReference> bl = (acdb.ModelSpace.Cast<AcadEntity>()
                            .Where(ent => ent.EntityName == "AcDbBlockReference")
                            .Select(ent => (AcadBlockReference) ent)
                            .Where(acadBlockReference => acadBlockReference.EffectiveName == "adv_cable")).ToList();
                        if (bl.Count <= 0) continue;
                        int cnt = 0;
                        foreach (var b in bl)
                        {
                            object[] attributes = b.GetAttributes();
                            IEnumerable<AcadAttributeReference> cablenameattr = attributes.OfType<AcadAttributeReference>()
                                .Where(i => i.TagString == "НОМЕР_КАБЕЛЯ");
                            IEnumerable<AcadAttributeReference> cabletypeattr = attributes.OfType<AcadAttributeReference>()
                                .Where(i => i.TagString == "ТИП_КАБЕЛЯ");
                            IEnumerable<AcadAttributeReference> crosssection = attributes.OfType<AcadAttributeReference>()
                                .Where(i => i.TagString == "СЕЧЕНИЕ_КАБЕЛЯ");
                            IEnumerable<AcadAttributeReference> cablelenght = attributes.OfType<AcadAttributeReference>()
                                .Where(i => i.TagString == "ДЛИНА_КАБЕЛЯ");
                            IEnumerable<AcadAttributeReference> blockGUIDattr = attributes.OfType<AcadAttributeReference>()
                                .Where(i => i.TagString == "AOVGEN_GUID");

                            var cabtuple = CabInfoList
                                .AsParallel()
                                .FirstOrDefault(r => r.GUID == blockGUIDattr.ElementAt(0).TextString);
                            if (cabtuple.GUID == null) continue;
                            cnt++;
                            cablenameattr.ElementAt(0).TextString = cabtuple.Name;
                            cabletypeattr.ElementAt(0).TextString = cabtuple.Type;
                            crosssection.ElementAt(0).TextString = string.Empty;
                            cablelenght.ElementAt(0).TextString = cabtuple.Lenght;
                        }
                        if (cnt > 0) flagFindDraw = true;
                    }
                    else
                    {
                        return;
                    }
                                    
                }
                //AcadDocument acadDoc = acadApp.ActiveDocument;
                //MessageBox.Show(CabInfoList.Count.ToString());
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Не нашел акад");
                MessageBox.Show(ex.Message + $"\n{ ex.StackTrace}"); 
            }
            
            
        }

        void MenuItem1_Click(object sender, EventArgs e)
        {

            CreateAnalogue createAnalogue = new CreateAnalogue(connection)
            {
                PannelGUIDForCopy = radGridView2.CurrentRow.Cells[1].Value.ToString(),
                DBFilePath = DBFilePath,
                RadGridView = radGridView2,
                RadPropertyGrid = radPropertyGrid1
            };
            createAnalogue.ShowDialog();

        }

        private void MenuItem2_Click(object sender, EventArgs e)
        {

            DeletePannelFromDB();  
            
        }

        private void Cablesetnames_Click (object sender, EventArgs e)
        {
            SetCableNames();
        }

        private void SetCableNames()
        {
            string cablename = "K";
            string devider = string.Empty;
            bool writecabP = false;
            bool writepumpcable = false;
            bool writevalvecable = false;

            Building building = null;
            RadTreeNode SelectedNode = radTreeView1.SelectedNode;
            if (SelectedNode != null && SelectedNode.Level == 1)
            {
                building = SelectedNode.Tag as Building;
            }
            if (building != null)
            {

                if (connection.State == ConnectionState.Open)
                {
                    string query = $"Select CableName, Devider, WriteCableP, WritePumpCable, WriteValveCable FROM BuildSetting WHERE Place = '{building.BuildGUID}'";
                    SQLiteCommand command = new SQLiteCommand
                    {
                        Connection = connection,
                        CommandText = query
                    };

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cablename = reader[0].ToString();
                            devider = reader[1].ToString();
                            writecabP = Convert.ToBoolean(reader[2].ToString());
                            writepumpcable = Convert.ToBoolean(reader[3].ToString());
                            writevalvecable = Convert.ToBoolean(reader[4].ToString());

                        }
                    }
                    command.Dispose();
                }

            }


            if (connection.State != ConnectionState.Open) return;
            {
                SQLiteCommand command1 = new SQLiteCommand { Connection = connection };
                SetCabName cabNameForm = new SetCabName
                {
                    Cablename = cablename
                };
                DialogResult dialogResult = cabNameForm.ShowDialog();
                if (dialogResult == DialogResult.Cancel) return;
                cablename = cabNameForm.Cablename;
                int cnt = cabNameForm.Cablestartnum;
                foreach (var row in radGridView3.SelectedRows)
                {
                    string newcabname;
                    //row.Cells[2].Value = cablename + devider + cnt.ToString();
                    if (row.Tag == null) continue;
                    Cable cable = (Cable)row.Tag;
                    if (writecabP && cable.Attrubute == Cable.CableAttribute.P)
                    {
                        newcabname = "*";
                        switch (cable.HostTable)
                        {
                            case "Pump":
                                if (writepumpcable)
                                {
                                    newcabname = cablename + devider + cnt;
                                }
                                break;
                            case "Damper":
                            case "Valve":
                                if (writevalvecable)
                                {
                                    newcabname = cablename + devider + cnt;
                                }
                                break;
                        }


                        //cable.CableName = "*";
                    }
                    else
                    {

                        newcabname = cablename + devider + cnt;

                        //cable.CableName = cablename + devider + cnt.ToString();
                        cnt += 1;
                    }
                    cable.CableName = newcabname;
                    row.Cells[2].Value = newcabname;


                    string query = $"UPDATE Cable SET CableName= '{cable.CableName}' " +
                                   $"WHERE GUID= '{cable.cableGUID}'";
                    command1.CommandText = query;
                    command1.ExecuteNonQuery();


                }

                command1.Dispose();
            }
        }

        private void cablesettypes_Click(object sender, EventArgs e)
        {
            SetCabeTypes();
            //MessageBox.Show("установка типов кабелей");
        }

        private void Cablelenghtset_Click(object sender, EventArgs e)
        {
            SetCableLenght();
        }

        private void SetCableLenght()
        {
            CableLenghtForm cableLenghtForm = new CableLenghtForm();
            DialogResult dialogResult = cableLenghtForm.ShowDialog();

            if (dialogResult != DialogResult.OK) return;
            SetCableLenght setcablelenght = new SetCableLenght(radGridView3, connection);
            if (cableLenghtForm.ReadTranslationFie)
            {
                setcablelenght.ExecuteExchange(@"C:\\ADV_Toolkit\\change.txt");


            }
            if (cableLenghtForm.ReadAcadFile)
            {
                string progId = "AutoCAD.Application";

                try
                {
                    dynamic cadApp = Marshal.GetActiveObject(progId);
                    if (cadApp != null)
                    {
                        Process[] localByName = Process.GetProcessesByName("acad");
                        Process process = localByName[0];
                        IntPtr hWnd = process.MainWindowHandle;

                        if (hWnd != IntPtr.Zero)
                        {
                            
                            AcadApplication acadApp = cadApp;

                            AcadDocument acadDoc = acadApp.ActiveDocument;
                            setcablelenght.ExcecuteAutocadFileName(acadDoc);
                            //SetForegroundWindow(hWnd);

                        }






                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("Не нашел акад");
                    MessageBox.Show(ex.Message);
                    MessageBox.Show(ex.StackTrace);
                }



            }
            if (cableLenghtForm.SetRandom)
            {
                setcablelenght.ExcecuteRandom(cableLenghtForm.Lenght, cableLenghtForm.Addition, cableLenghtForm.UseGenPowerCables, cableLenghtForm.AllowPowerCables, cableLenghtForm.PowerCableLenght);
            }
        }

        private void SetCabeTypes()
        {
            RadTreeNode radTreeNode = radTreeView1.SelectedNode;
            Dictionary<string, Cable> cables = new Dictionary<string, Cable>();
            Dictionary<string, Cable> cables2 = new Dictionary<string, Cable>();
            bool trouble = false;
            GridViewRowInfo troubleRow = null;
            if (radTreeNode.Tag is Building building)
            {


                if (connection.State == ConnectionState.Open)
                {
                    //string query1 = $"SELECT CableA, CableD, CableP, CableC, CableACrossSection, CableDCrossSection, CablePCrossSection, CableCCrossSection FROM BuildSetting WHERE Place = '{building.BuildGUID}'";
                    string query1 = $"SELECT CableA, CableD, CableP, CableC, WriteCableP, WritePumpCable, WriteValveCable FROM BuildSetting WHERE Place = '{building.BuildGUID}'";
                    bool WritePumpCable, WriteValveCable;
                    var CableInEOM = WritePumpCable = WriteValveCable = false;

                    SQLiteCommand command = new SQLiteCommand
                    { Connection = connection, CommandText = query1 };
                    

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            Cable cableA = new Cable
                            {
                                Attrubute = Cable.CableAttribute.A,
                            };
                            Cable cableD = new Cable
                            {
                                Attrubute = Cable.CableAttribute.D,
                            };
                            Cable cableP = new Cable
                            {
                                Attrubute = Cable.CableAttribute.P,
                            };
                            Cable cableC = new Cable
                            {
                                Attrubute = Cable.CableAttribute.C,
                            };
                            Cable cablePL = new Cable
                            {
                                Attrubute = Cable.CableAttribute.PL,
                            };
                            cableA.CableType = reader[0].ToString();
                            cableD.CableType = reader[1].ToString();
                            cableP.CableType = reader[2].ToString();
                            cablePL.CableType = reader[2].ToString();
                            cableC.CableType = reader[3].ToString();
                            CableInEOM = Convert.ToBoolean(reader[4].ToString());
                            WritePumpCable = Convert.ToBoolean(reader[5].ToString());
                            WriteValveCable = Convert.ToBoolean(reader[6].ToString());

                            cables.Add("A", cableA);
                            cables.Add("D", cableD);
                            cables.Add("P", cableP);
                            cables.Add("C", cableC);
                            cables.Add("PL", cablePL);
                        }
                        reader.Close();
                    }
                    
                    foreach (GridViewRowInfo row in radGridView3.SelectedRows)
                    {
                        Cable CableFromTag = (Cable)row.Tag;
                        int wirenumbers = CableFromTag.WireNumbers;// (int)row.Cells[13].Value;
                        string attribute = CableFromTag.Attrubute.ToString();// row.Cells[10].Value.ToString();
                        string cabguid = CableFromTag.cableGUID;// row.Cells[1].Value.ToString();
                        Cable cable = cables[attribute];
                        CableFromTag.CableType = cable.CableType;
                        //cable.cableGUID = cabguid;
                        //cable.WireNumbers = wirenumbers;
                        string SelectCabType = string.Empty;
                        bool IsHighPowerCable = CableFromTag.Attrubute == Cable.CableAttribute.P;
                        bool IsLowPowerCable = CableFromTag.Attrubute == Cable.CableAttribute.PL;
                        bool IsPowerCable = IsHighPowerCable || IsLowPowerCable;
                        try
                        {
                            if (!IsPowerCable)
                            {
                                SelectCabType = "SELECT CableFullName FROM" +
                                $"(SELECT CableFullName, WireNumbers AS WN FROM CableTypes WHERE CableType = '{CableFromTag.CableType}')" +
                                $" WHERE WN >='{wirenumbers}' LIMIT 1";
                                SQLiteCommand SelectCabTypeCommand = new SQLiteCommand
                                {
                                    CommandText = SelectCabType,
                                    Connection = connection
                                };
                                using (SQLiteDataReader reader = SelectCabTypeCommand.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {
                                            SelectCabType = reader[0].ToString();
                                        }
                                    }
                                    else
                                    {
                                        SelectCabType = "Тип остутствует";
                                        trouble = true;
                                        troubleRow = row;
                                    }
                                    
                                    
                                }

                            }
                            else
                            {
                                
                                switch (CableInEOM)
                                {
                                    //если это силовой кабель и не у электриков
                                    case false when IsHighPowerCable:
                                    //если насос у нас или заслонка у нас
                                    case true when (WritePumpCable || WriteValveCable) && !IsHighPowerCable:
                                        SelectCabType = GetCableFromBD(CableFromTag, WritePumpCable, WriteValveCable, CableInEOM);
                                        break;
                                    case true:
                                        SelectCabType = string.Empty;
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                        }
                        CableFromTag.CableType = SelectCabType;

                        //bool writeblock = Convert.ToBoolean(row.Cells[11].Value.ToString());
                        //string cabname = row.Cells[2].Value.ToString();
                        //string cabfrompos = row.Cells[3].Value.ToString();
                        //string cabtopos = row.Cells[4].Value.ToString();
                        //double.TryParse(row.Cells[6].Value.ToString(), out double lenght);
                        //string description = row.Cells[7].Value.ToString();
                        //string blockname = row.Cells[12].Value.ToString();

                        //Cable cable1 = new Cable
                        //{
                        //    Attrubute = cable.Attrubute,
                        //    WriteBlock = writeblock,
                        //    cableGUID = cabguid,
                        //    CableType = SelectCabType,//cable.CableType,
                        //    CableName = cabname,
                        //    FromPosName = cabfrompos,
                        //    ToPosName = cabtopos,
                        //    Lenght = lenght,
                        //    Description = description,
                        //    ToBlockName = blockname,
                        //    WireNumbers = wirenumbers

                        //};

                        row.Cells[5].Value = CableFromTag.CableType;//cable1.CableType;
                        //row.Tag = cable1;
                        //cables2.Add(cabguid, cable1);
                        cables2.Add(cabguid, CableFromTag);

                    }
                }


                //update DataBase


                if (connection.State == ConnectionState.Open)
                {
                    SQLiteCommand command = new SQLiteCommand { Connection = connection };
                try
                {
                    foreach (var query in cables2.Select(keyValuePair => $"UPDATE Cable SET CableType= '{keyValuePair.Value.CableType}', " +
                                                                         $"CableName = '{keyValuePair.Value.CableName}', " +
                                                                         $"CableLenght = '{keyValuePair.Value.Lenght}' " +
                                                                         $"WHERE GUID= '{keyValuePair.Key}'"))
                    {
                        command.CommandText = query;
                        command.ExecuteNonQuery();
                    }
                }
                    catch
                    {
                        MessageBox.Show("Похоже выбран кабель не из базы данных");
                    }
                    
                
                }



            }
            if (trouble)
            {
                radGridView3.Rows[troubleRow.Index].IsSelected = true;
                radGridView3.Rows[troubleRow.Index].IsCurrent = true;
                MessageBox.Show("Проверьте типы кабелей! \nСкорее всего в БД нет типа для одного из кабелей. \nАвтоматически подобрать кабель не удалось");
            }
        }
        private void DeletePannelFromDB()
        {
            RadTreeNode buildnode = radTreeView1.SelectedNode;
            var Row = radGridView2.CurrentRow;
            if (!(Row?.Tag is Pannel)) return;
            string pannelGUID = radGridView2.CurrentRow.Cells[1].Value.ToString();
            string pannelname = radGridView2.CurrentRow.Cells[2].Value.ToString();
            int selectedIndex = radGridView2.Rows.IndexOf(radGridView2.CurrentRow);
            DialogResult dialogResult = MessageBox.Show($"Сейчас будет удалено из базы данных устройство: {pannelname}", "Удаление шкафа", MessageBoxButtons.YesNo);
            if (dialogResult != DialogResult.Yes) return;
            try
            {
                if (connection.State != ConnectionState.Open) return;
                string sqldelete = $"DELETE FROM Pannel WHERE [GUID] = '{pannelGUID}'";
                SQLiteCommand command = new SQLiteCommand(sqldelete, connection);
                command.ExecuteNonQuery();
                command.Dispose();

                radGridView2.Rows.RemoveAt(selectedIndex);
                radGridView2.Update();
                if (buildnode != null) UpdateBuildNode(buildnode);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private void DeleteProjectFromDB()
        {
            if (radTreeView1.SelectedNode == null) return;
            RadTreeNode radTreeNode = radTreeView1.SelectedNode;
            if (!(radTreeNode.Tag is Project project)) return;
            DialogResult dialogResult = MessageBox.Show($"Сейчас будет удалено из базы данных проект {project.ProjectName} со всеми данными, включая здания и шкафы", "Удаление проекта", MessageBoxButtons.YesNo);
            if (dialogResult != DialogResult.Yes) return;
            try
            {
                string[] arrayfordelete = { "Buildings", "VentSystems", "Pannel", "Cable", "Ventilator", "Damper", "ElectroHeater", "WaterHeater", "Filter", "Froster", "Humidifier", "KKB", "Pump", "Recuperator", "SensHum", "SensPDS", "SensT", "Valve", "Levels", "Rooms", "SpareVentilator" };
                string ProjectGUID = project.GetGUID();
                string sqlprojectdelete = $"DELETE FROM Project WHERE [GUID] = '{ProjectGUID}'";
                if (connection.State != ConnectionState.Open) return;
                SQLiteCommand command1 = new SQLiteCommand
                {
                    Connection = connection
                };
                command1.CommandText = sqlprojectdelete;
                command1.ExecuteNonQuery();
                foreach (string var in arrayfordelete)
                {
                    var query = $"DELETE FROM {var} WHERE [Project] = '{ProjectGUID}'";
                    command1.CommandText = query;
                    command1.ExecuteNonQuery();
                }
                command1.Dispose();

                radTreeView1.AllowRemove = true;
                radTreeView1.Nodes.Remove(radTreeNode);
                radTreeView1.TreeViewElement.Update(RadTreeViewElement.UpdateActions.Reset);
                radTreeView1.AllowRemove = false;
                
                DataTable dataSource = new DataTable("fileSystem");
                dataSource.Columns.Add("ProjectName", typeof(string));
                dataSource.Columns.Add("DateofCreate", typeof(string));
                dataSource.Columns.Add("Chiper", typeof(string));
                dataSource.Columns.Add("CheefEngeneer", typeof(string));

                radGridView2.DataSource = dataSource;
                


            }
            catch
            {
                // ignored
            }
        }
        private void DeleteBuildingFromDB()
        {
            if (radTreeView1.SelectedNode == null) return;
            RadTreeNode radTreeNode = radTreeView1.SelectedNode;
            RadTreeNode parent = radTreeNode.Parent;
            if (!(radTreeNode.Tag is Building building)) return;
            DialogResult dialogResult = MessageBox.Show($"Сейчас будет удалено из базы данных здание {building.Buildname} со всеми данными, включая шкафы", "Удаление здания", MessageBoxButtons.YesNo);
            if (dialogResult != DialogResult.Yes) return;
            try
            {
                string BuildGUID = building.BuildGUID;
                string[] arrayfordelete1 =
                {
                    "VentSystems", "Pannel", "BuildSetting", "Levels", "Rooms"
                };
                string[] arrayfordelete2 = 
                {
                    "Cable", "Ventilator", "Damper", "ElectroHeater", "WaterHeater",
                    "Filter", "Froster", "Humidifier", "KKB", "Pump", "Recuperator", "SensHum", "SensPDS", 
                    "SensT", "Valve", "SpareVentilator"

                };
                string selectventsystems = $"SELECT VentSystems.GUID FROM VentSystems WHERE Place = '{BuildGUID}'";
                string sqlbuildingdelete = $"DELETE FROM Buildings WHERE [GUID] = '{BuildGUID}'";

                if (connection.State != ConnectionState.Open) return;
                SQLiteCommand command = new SQLiteCommand
                {
                    Connection = connection
                };
                #region Get ventsystems by Build GUID
                command.CommandText = selectventsystems;
                SQLiteDataReader reader = command.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(reader);
                reader.Close();

                #endregion
                #region Delete VentSystems components

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    string guid = dt.Rows[i].ItemArray[0].ToString();
                    if (guid == string.Empty) continue;
                    foreach (string var in arrayfordelete2)
                    {
                        var query1 = $"DELETE FROM {var} WHERE SystemGUID = '{guid}'";
                        command.CommandText = query1;
                        command.ExecuteNonQuery();
                    }


                }
                #endregion
                #region Delete Build
                command.CommandText = sqlbuildingdelete;
                command.ExecuteNonQuery();
                #endregion
                #region Delete Ventsystem hosts and Pannels by Build GUID

                foreach (string var in arrayfordelete1)
                {
                    var query2 = $"DELETE FROM {var} WHERE [Place] = '{BuildGUID}'";
                    command.CommandText = query2;
                    command.ExecuteNonQuery();
                }
                #endregion
                #region Close DB
                command.Dispose();

                #endregion
                #region Update Project Treeview
                parent.Nodes.Remove(radTreeNode.Name);
                radTreeView1.Update();
                #endregion
                #region Set empty table
                DataTable dataSource = new DataTable("fileSystem");
                radGridView2.DataSource = dataSource;
                #endregion


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private static Building CreateBuildingClass(string buildingname)
        {

            Building building = new Building
            {
                Buildname = buildingname,
                BuildNum = "01",

            };
            return building;

        }
        private void CreateProject()
        {
            
            int cnt = 0;
            Parallel.ForEach(radTreeView1.Nodes, radTreeNode =>
            {
                if (radTreeNode.Parent == null) cnt++;
            });
            cnt++;
            var guid = Guid.NewGuid().ToString();
            var date = DateTime.Now.ToString("dd-MM-yyyy");
            Project NewProject = new Project
            {
                ProjectName = "Новый проект" + cnt,
                Chiper = "Шифр ххх",
                CheefEngeneer = "Фамилия",
            };
            NewProject.SetGUID(guid);
            RadTreeNode NodeProject = new RadTreeNode(NewProject.ProjectName)
            {
                Value = NewProject.ProjectName,
                Tag = NewProject,
                Name = NewProject.GetGUID()
            };
           
           
            radTreeView1.Nodes.Add(NodeProject);
            radTreeView1.Update();
            radPropertyGrid1.SelectedObject = NodeProject.Tag;

            try
            {

                //from---------------------
                
                //---------to---------

                if (connection.State != ConnectionState.Open) return;
                string InsertQuery = $"INSERT INTO Project ([GUID], ProjectName, Chiper, CheefEngeneer, DateofCreate) VALUES('{guid}','{NewProject.ProjectName}','{NewProject.Chiper}','{NewProject.CheefEngeneer}','{date}')";
                   
                SQLiteCommand command = new  SQLiteCommand(InsertQuery, connection);
                command.ExecuteNonQuery();
                command.Dispose();
                //connection.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace + " " + ex.Message);
            }

        }
        private void DataLoadToTree()
        {
            
            
            try
            {
               
                if (connection.State == ConnectionState.Open)
                {
                    const string tableselect = "SELECT Project.ProjectName, Project.Chiper, Project.CheefEngeneer, Project.GUID, Buildings.Buildname, Buildings.GUID FROM Project INNER JOIN Buildings ON Project.GUID = Buildings.Project ;";
                    SQLiteCommand command = new SQLiteCommand(tableselect, connection);
                    SQLiteDataReader reader = command.ExecuteReader();
                    
                    while (reader.Read())
                    {

                        
                        RadTreeNode foundNode = FindNodeByValue(reader[0].ToString(), radTreeView1.Nodes);
                        
                        if (foundNode == null)
                        {
                            RadTreeNode parent = new RadTreeNode(reader[0].ToString());
                            Project project = new Project
                            {
                                ProjectName = reader[0].ToString(),
                                Chiper = reader[1].ToString(),
                                CheefEngeneer = reader[2].ToString()
                            };
                            project.SetGUID(reader[3].ToString());
                            parent.Value = reader[0].ToString();
                            parent.Tag = project;
                            parent.Name = project.GetGUID();
                            
                            //Parent.Tag = reader[3].ToString();
                            radTreeView1.Nodes.Add(parent);
                            Building building = CreateBuildingClass(reader[4].ToString());
                            building.BuildGUID = (reader[5].ToString());

                            RadTreeNode childNode = new RadTreeNode(reader[4].ToString())
                            {
                                Value = reader[4].ToString(),
                                Name=  building.BuildGUID,
                                Tag = building
                            };

                            parent.Nodes.Add(childNode);
                        }
                        else
                        {
                            Building building = CreateBuildingClass(reader[4].ToString());
                            building.BuildGUID = reader[5].ToString();
                            RadTreeNode childNode = new RadTreeNode(reader[4].ToString())
                            {
                                Value = reader[4].ToString(),
                                Tag = building
                                //Tag = reader[5].ToString()
                               
                            };


                            foundNode.Nodes.Add(childNode);
                        }                 
                    }
                    reader.Close();
                    const string tableselectemty = "SELECT ProjectName, Project.GUID, Chiper, CheefEngeneer FROM Project ;";
                    SQLiteCommand command1 = new SQLiteCommand(tableselectemty, connection);
                    SQLiteDataReader reader1 = command1.ExecuteReader();
                    while (reader1.Read())
                    {
                        
                        RadTreeNode foundNode = FindNodeByValue(reader1[0].ToString(), radTreeView1.Nodes);
                        if (foundNode != null) continue;
                        RadTreeNode parent = new RadTreeNode(reader1[0].ToString())
                        {
                            Value = reader1[0].ToString()
                                
                        };
                        Project project = new Project
                        {
                            ProjectName = reader1[0].ToString(),
                            Chiper = reader1[2].ToString(),
                            CheefEngeneer = reader1[3].ToString()
                        };
                        project.SetGUID(reader1[1].ToString());
                        parent.Tag = project;

                        radTreeView1.Nodes.Add(parent);
                    }
                    reader1.Close();
                   


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
        }

        private RadTreeNode FindNodeByValue(object value, RadTreeNodeCollection nodes)
       {
            
            return (from t in nodes
                .Where(e => e.Value.Equals(value))
                select t)
                .FirstOrDefault();            
        }
        internal RadTreeNode FindNodeByName(object text, RadTreeNodeCollection nodes)
        {


            //старый метод
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
        private Pannel FindPannel (string pannelGUID)
        {

            return (Pannel)(radGridView2.Rows
                .AsParallel()
                .FirstOrDefault(e => e.Cells[1].Value.ToString() == pannelGUID))?.Tag;
            
        }

        private void radTreeView1_NodeMouseClick(object sender, RadTreeViewEventArgs e)
        {
            
            UpdateBuildNode(e.Node);
            radRibbonBarGroup9.Enabled = RevitConnectionState == RevitState.DocumentPresent && radTreeView1.SelectedNode.Level == 1;
            radButtonElement4.Enabled = radTreeView1.SelectedNode.Level == 1;
            radTreeView2.RadContextMenu = radTreeView2.Nodes.Count > 0 ? radContextMenu1 : null;



        }
        internal void UpdateBuildNode(RadTreeNode BuildNode)
        {
            
            radTreeView2.Nodes.Clear();
            radTreeView3.Nodes.Clear();
            radTreeView4.Nodes.Clear();
            radTreeView5.Nodes.Clear();
            radTreeView6.Nodes.Clear();
            

            if (radGridView1.Rows.Count > 0) radGridView1.DataSource = null;
            bool nodeparentpresent = (BuildNode.Parent != null);
            string ventsystemselect, levelselect;
            var tableselect = ventsystemselect = levelselect = string.Empty;
            string nodeguid;
            Building building = null;
            switch (nodeparentpresent)
            {
                case true:
                    building = BuildNode.Tag as Building;
                    if (building != null)
                    {
                        if (BuildNode.Name != building.BuildGUID) BuildNode.Name = building.BuildGUID;
                        nodeguid = building.BuildGUID;
                        tableselect = "SELECT Pannel.ID, Pannel.GUID, Pannel.PannelName, Pannel.Modyfied, Pannel.Author, Pannel.Version, Pannel.Power, Pannel.Voltage, Pannel.Category, Pannel.FireProtect, Pannel.Dispatching, Pannel.Protocol " +
                                        "FROM Pannel " +
                                        $"WHERE Place = '{nodeguid}'";
                        ventsystemselect = "SELECT VentSystems.GUID, VentSystems.SystemName, SupplyVent, SupplyFilter, SupplyDamper, WaterHeater, ElectroHeat, Froster, Humidifier, ExtVent, ExtFilter, ExtDamper, Recuperator, Pannel, SensTOutdoor, SensTIndoor, SensTexhaust, SensTSupply, SensHIndoor, Crossconnection, Room, Filter, SpareSupplyVent, SpareExtVent " +
                                            "FROM VentSystems " +
                                            //$"INNER JOIN Ventilator ON VentSystems.GUID = Ventilator.SystemGUID " +
                                            $"WHERE[Place] = '{nodeguid}'";
                        levelselect = $"SELECT LevelGUID, LevelID, Name, Elevation FROM Levels WHERE [Place] = '{nodeguid}'";
                        
                    }
                    
                    break;
                case false:
                    if (BuildNode.Tag is Project project)
                    {
                        nodeguid = project.GetGUID();
                        tableselect = $"SELECT Project.ID, Project.GUID, Project.ProjectName, Project.DateofCreate, Project.Chiper, Project.CheefEngeneer FROM Project WHERE GUID = '{nodeguid}'";
                    }

                    break;
            }
            
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection
            };
            if (tableselect != string.Empty)
            {
                if (connection.State == ConnectionState.Closed) connection.Open();
                if (connection.State == ConnectionState.Open)
                {
                    try
                    {
                        
                        command.CommandText = tableselect;
                        SQLiteDataReader dataReader = command.ExecuteReader(); // чтение записей проекта из БД
                        DataTable dt = new DataTable(); //создание промежуточной таблицы (чтобы не заполнять radGridView по строчкам, а сразу скопом
                        dt.Load(dataReader); //назначение таблице всего того что лежит в dataReader, то есть поcле чтения БД
                        dataReader.Close();
                        radGridView2.DataSource = dt;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    switch (nodeparentpresent)
                    {
                        case true:
                            ReadpannelGrig();
                            break;
                        case false:
                            ReadProjectGrid(radGridView2);
                            break;
                    }
                }
            }
            if (ventsystemselect != string.Empty)
            {
                if (connection.State == ConnectionState.Closed) connection.Open();
                if (connection.State == ConnectionState.Open)
                {
                    try
                    {
                       // this.radTreeView2.Nodes.Clear();
                        //this.radTreeView3.Nodes.Clear();
                        command.CommandText = ventsystemselect;
                        DataTable dt = new DataTable();
                        using (SQLiteDataReader dataReader = command.ExecuteReader())
                        {
                            dt.Load(dataReader);
                        }

                        foreach (DataRow row in dt.Rows)
                        {
                            RadTreeNode node = new RadTreeNode
                            {
                                //Text = "Systemname"].ToString(),
                                //Value = row["GUID"].ToString()
                                Text = row["Systemname"].ToString(),
                                Value = row["Systemname"].ToString(),
                                Name = row["GUID"].ToString()

                            };
                            VentSystem ventSystem = new VentSystem();
                            string ventsystemguid = row["GUID"].ToString();
                            ventSystem.GUID = ventsystemguid;
                            ventSystem.SystemName = row["Systemname"].ToString();
                            bool.TryParse(row["SupplyVent"].ToString(), out bool supplyVent);
                            //bool.TryParse(row["SupplyFilter"].ToString(), out bool SupplyFilter);
                            bool.TryParse(row["SupplyDamper"].ToString(), out bool supplyDamper);
                            bool.TryParse(row["WaterHeater"].ToString(), out bool waterHeater);
                            bool.TryParse(row["ElectroHeat"].ToString(), out bool electroHeat);
                            bool.TryParse(row["Froster"].ToString(), out bool froster);
                            bool.TryParse(row["Humidifier"].ToString(), out bool humidifier);
                            bool.TryParse(row["ExtVent"].ToString(), out bool extVent);
                            //bool.TryParse(row["ExtFilter"].ToString(), out bool ExtFilter);
                            bool.TryParse(row["ExtDamper"].ToString(), out bool extDamper);
                            bool.TryParse(row["Recuperator"].ToString(), out bool recuperator);
                            bool.TryParse(row["Filter"].ToString(), out bool filter);
                            bool.TryParse(row["Room"].ToString(), out bool room);
                            bool.TryParse(row["SpareSupplyVent"].ToString(), out bool spareSupplyVent);
                            bool.TryParse(row["SpareExtVent"].ToString(), out bool spareExtVent);

                            if (supplyVent)
                            {

                                
                                List<EditorV2.PosInfo> posInfos = ReadVent<SupplyVent>(ventSystem.GUID, command, "SystemGUID");
                                if (posInfos.Count > 0)
                                {
                                    foreach (EditorV2.PosInfo pos in posInfos)
                                    {
                                        ventSystem.ComponentsV2.Add(pos);
                                    }
                                }
                            }
                            if (spareSupplyVent)
                            {
                                
                                List<EditorV2.PosInfo> posInfos = ReadSpareVent<SpareSuplyVent>(ventSystem.GUID, command, "SystemGUID");
                                if (posInfos.Count > 0)
                                {
                                    foreach (EditorV2.PosInfo pos in posInfos)
                                    {
                                        ventSystem.ComponentsV2.Add(pos);
                                    }
                                }
                            }
                            if (spareExtVent)
                            {
                                List<EditorV2.PosInfo> posInfos = ReadSpareVent<SpareExtVent>(ventSystem.GUID, command, "SystemGUID");
                                if (posInfos.Count > 0)
                                {
                                    foreach (EditorV2.PosInfo pos in posInfos)
                                    {
                                        ventSystem.ComponentsV2.Add(pos);
                                    }
                                }
                            }
                            if (supplyDamper)
                            {
                                List<EditorV2.PosInfo> posInfos = ReadDamper<SupplyDamper>(ventsystemguid, command, "SystemGUID");
                                if (posInfos.Count>0)
                                {
                                    foreach (EditorV2.PosInfo pos in posInfos)
                                    {
                                        ventSystem.ComponentsV2.Add(pos);
                                    }
                                }
                            }
                            if (waterHeater)
                            {
                                List<EditorV2.PosInfo> posInfos = ReadWaterHeater(ventsystemguid, command, "SystemGUID");
                                if (posInfos.Count > 0)
                                {
                                    foreach (EditorV2.PosInfo pos in posInfos)
                                    {
                                        ventSystem.ComponentsV2.Add(pos);
                                    }
                                }
                                
                            }
                            if (electroHeat)
                            {
                                List<EditorV2.PosInfo> posInfos = ReadElectroHeater(ventsystemguid, command, "SystemGUID");
                                if (posInfos.Count>0)
                                {
                                    foreach(EditorV2.PosInfo pos in posInfos)
                                    {
                                        ventSystem.ComponentsV2.Add(pos);
                                    }
                                }
                            }
                            if (froster)
                            {
                                List<EditorV2.PosInfo> posInfos = ReadFroster(ventsystemguid, command, "SystemGUID");
                                if (posInfos.Count > 0)
                                {
                                    foreach (EditorV2.PosInfo pos in posInfos)
                                    {
                                        ventSystem.ComponentsV2.Add(pos);
                                    }
                                }

                                
                            }
                            if (humidifier)
                            {
                                List<EditorV2.PosInfo> posInfos = ReadHumidifier(ventsystemguid, command, "SystemGUID");
                                if (posInfos.Count > 0)
                                {
                                    foreach (EditorV2.PosInfo pos in posInfos)
                                    {
                                        ventSystem.ComponentsV2.Add(pos);
                                    }
                                }

                                
                            }
                            if (extVent)
                            {
                                List<EditorV2.PosInfo> posInfos = ReadVent<ExtVent>(ventSystem.GUID, command, "SystemGUID");
                                if (posInfos.Count>0)
                                    foreach(EditorV2.PosInfo pos in posInfos)
                                    {
                                        ventSystem.ComponentsV2.Add(pos);
                                    }
                                
                            }
                            if (filter)
                            {
                                List<EditorV2.PosInfo> posInfos = ReadFiltr(ventSystem.GUID, command, "SystemGUID");
                                if (posInfos.Count > 0)
                                {
                                    foreach (EditorV2.PosInfo pos in posInfos)
                                    {
                                        ventSystem.ComponentsV2.Add(pos);
                                    }
                                }

                            }
                            if (extDamper)
                            {
                                List<EditorV2.PosInfo> posInfos = ReadDamper<ExtDamper>(ventsystemguid, command, "SystemGUID");
                                if (posInfos.Count > 0)
                                {
                                    foreach (EditorV2.PosInfo pos in posInfos)
                                    {
                                        ventSystem.ComponentsV2.Add(pos);
                                    }
                                }
                            }
                            if (recuperator)
                            {
                                List<EditorV2.PosInfo> posInfos = ReadRecuperator(ventsystemguid, command, "SystemGUID");
                                if (posInfos.Count > 0)
                                {
                                    foreach (EditorV2.PosInfo pos in posInfos)
                                    {
                                        ventSystem.ComponentsV2.Add(pos);
                                    }
                                }
                                
                            }
                            if (room)
                            {
                                List<EditorV2.PosInfo> posInfos = ReadRoom(ventsystemguid, command, "SystemGUID");
                                if (posInfos.Count>0)
                                {
                                    foreach (EditorV2.PosInfo pos in posInfos)
                                    {
                                        ventSystem.ComponentsV2.Add(pos);
                                    }
                                }
                            }
                            //read crossections nolocation
                            {
                                List<EditorV2.PosInfo> posInfos = ReadCrossection(ventsystemguid, command, "SystemGUID");
                                if (posInfos.Count>0)
                                {
                                    foreach (EditorV2.PosInfo pos in posInfos)
                                    {
                                        ventSystem.ComponentsV2.Add(pos);
                                    }
                                }
                            }
                            ventSystem.ComponentsV2
                                .RemoveAll(item => item == null);

                            //Rename First Crossection BlockName in SupplyLine
                            
                            if (!supplyDamper)
                            {
                                var YList = ventSystem.ComponentsV2
                                    .Where(e =>
                                        e.Tag.GetType().Name == nameof(SupplyVent) ||
                                        e.Tag.GetType().Name == nameof(SpareSuplyVent))
                                    .Select(e => e.PozY)
                                    .ToList();
                                if (YList.Count > 0)
                                {
                                    int Y = YList.First();


                                    if (((dynamic)ventSystem.ComponentsV2
                                        .Where(e => e.PozY == Y)
                                        .DefaultIfEmpty()
                                        .OrderBy(e => e?.PozX)
                                        .FirstOrDefault()
                                        .Tag).ShemaASU is ShemaASU shemaAsu) shemaAsu.ShemaLink2 = "Arrow_Supply_Left"; //Arrow_Ext_Left

                                    var s = ventSystem.ComponentsV2
                                        .AsQueryable()
                                        .Where(e => e.PozY == Y)
                                        .DefaultIfEmpty()
                                        .OrderBy(e => e.PozX)
                                        .FirstOrDefault();
                                    


                                }
                            }
                            //Rename Forst Crossection BlockName in ExihaustLine
                            if (!extDamper)
                            {
                                var YList = ventSystem.ComponentsV2
                                    .Where(e =>
                                        e.Tag.GetType().Name == nameof(ExtVent) ||
                                        e.Tag.GetType().Name == nameof(SpareExtVent))
                                    .Select(e => e.PozY)
                                    .ToList();
                                if (YList.Count > 0)
                                {
                                    int Y = YList.First();
                                    //if (ventSystem.ComponentsV2
                                    //    .Where(e => e.Tag.GetType().Name == nameof(CrossSection) && e.PozY == Y)
                                    //    .DefaultIfEmpty()
                                    //    .OrderBy(e => e?.PozX)
                                    //    .First()
                                    //    ?.Tag is CrossSection ExtCrossSectionLeft)
                                    //    ExtCrossSectionLeft.ShemaASU.ShemaUp = "CrossSection_Ext_Start";
                                    
                                    if (((dynamic)ventSystem.ComponentsV2
                                        .Where(e => e.PozY == Y)
                                        .DefaultIfEmpty()
                                        .OrderBy(e => e?.PozX)
                                        .FirstOrDefault()
                                        .Tag).ShemaASU is ShemaASU shemaAsu) shemaAsu.ShemaLink2 = "Arrow_Ext_Left"; //Arrow_Ext_Left
                                }
                            }

                            node.Tag = ventSystem;
                            radTreeView2.Nodes.Add(node);

                        }
                        //dataReader.Close();
                        ReadJoinedSystems2();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message + ex.StackTrace);
                    }
                }
            }
            if (levelselect != string.Empty)
            {
                try
                {
                    if (connection.State == ConnectionState.Closed) connection.Open();
                    command.CommandText = levelselect;
                    List<(string, string, string, string)> PannelList = new List<(string, string, string, string)>();
                    List<Pannel> PannelClassList = new List<Pannel>();
                    
                    using (SQLiteDataReader dataReader = command.ExecuteReader())
                    {
                        List<(string, string, string, double)> LevelList = new List<(string, string, string, double)>();
                        //add levels to list
                        while (dataReader.Read())
                        {
                            string levelguid = dataReader[0].ToString();
                            string levelid = dataReader[1].ToString();
                            string levelname = dataReader[2].ToString();
                            double elevation = Convert.ToDouble(dataReader[3].ToString());
                            (string LevelID, string LivelGUID, string LevelName, double Elevation) levelinfo;
                            levelinfo.LevelID = levelid;
                            levelinfo.LivelGUID = levelguid;
                            levelinfo.LevelName = levelname;
                            levelinfo.Elevation = elevation;
                            LevelList.Add(levelinfo);
                        }
                        //add pannels to common list

                        var pannelselect = $"SELECT PannelName, GUID, LevelGUID, RoomGUID FROM Pannel WHERE Place = '{building.BuildGUID}'";
                        SQLiteCommand getpannelscommand = new SQLiteCommand
                        {
                            CommandText = pannelselect,
                            Connection = connection
                        };
                        using (SQLiteDataReader pannelreader = getpannelscommand.ExecuteReader())
                        {
                            while (pannelreader.Read())
                            {
                                (string PannelName, string PannelGUID, string LevelGID, string RoomGUID) pannelinfo;
                                pannelinfo.PannelName = pannelreader[0].ToString();
                                pannelinfo.PannelGUID = pannelreader[1].ToString();
                                pannelinfo.LevelGID = pannelreader[2].ToString();
                                pannelinfo.RoomGUID = pannelreader[3].ToString();
                                PannelList.Add(pannelinfo);
                            }


                        }
                        if (LevelList.Count>0)
                        {
                            //sorting Level list
                            List<(string, string, string, double)> SortedLevelList = LevelList
                                .OrderBy(o => o.Item4)
                                .ToList();

                            PannelClassList = (from gridrow in radGridView2.Rows
                                           select gridrow.Tag as Pannel)
                                          .ToList();


                            foreach ((string levelid, string levelguid, string levelname, _) in SortedLevelList)
                            {
                                //create level Node
                                RadTreeNode LevelNode = new RadTreeNode
                                {
                                    Text = levelname,
                                    Name = levelguid, 
                                    Tag = levelid
                                };
                                string GetRoomsQuery = $"SELECT [RoomGUID], RoomID, RoomName, RoomNumber FROM Rooms WHERE LevelGUID = '{levelguid}'";
                                SQLiteCommand sQLiteCommand = new SQLiteCommand
                                {
                                    Connection = connection,
                                    CommandText = GetRoomsQuery
                                };
                                
                                List<(string, string, string, string)> RoomList = new List<(string, string, string, string)>();
                                using (SQLiteDataReader reader = sQLiteCommand.ExecuteReader())
                                {
                                    //add rooms to list by levelGuid
                                    while(reader.Read())
                                    {
                                        string RoomGUID = reader[0].ToString();
                                        string RoomID = reader[1].ToString();
                                        string RoomName = reader[2].ToString();
                                        string RoomNumber = reader[3].ToString();

                                        (string, string, string, string) RoomInfo;
                                        RoomInfo.Item1 = RoomGUID;
                                        RoomInfo.Item2 = RoomID;
                                        RoomInfo.Item3 = RoomName;
                                        RoomInfo.Item4 = RoomNumber;
                                        
                                        RoomList.Add(RoomInfo);
                                    }
                                }
                                //sorting rooms by room number
                                List<(string, string, string, string)> SortedRoomList = RoomList
                                    .OrderBy(o => o.Item4)
                                    .ThenBy(o => o.Item3)
                                    .ToList();
                                foreach ((string RoomGUID, string RoomID, string RoomName, string RoomNumber) in SortedRoomList)
                                {
                                    //create room node
                                    (string, string, string) roominfo;
                                    roominfo.Item1 = RoomNumber;
                                    roominfo.Item2 = RoomName;
                                    roominfo.Item3 = RoomID;
                                    RadTreeNode roomnode = new RadTreeNode
                                    {
                                        Text = $"({RoomNumber}) {RoomName}",
                                        Name = RoomGUID,
                                        Tag = roominfo
                                    };

                                    //List<(string, string, string, string)> PannelList = new List<(string, string, string, string)>();
                                    
                                    //pannelsinlevel = (from pannels in PannelList
                                    //                      where pannels.Item2 == levelguid
                                    //                      select pannels)
                                    //                     .ToList();

                                    List<(string, string, string, string)> pannelsinroom = (from pannel in PannelList
                                                                                           where pannel.Item4 == RoomGUID
                                                                                           select pannel).ToList();
                                    pannelsinroom.AsParallel().ForAll(pannel =>
                                    {

                                        Pannel findpannel = (from o in PannelClassList
                                                             where o.GetGUID() == pannel.Item2
                                                             select o)
                                                             .FirstOrDefault();
                                        RadTreeNode pannelnode = new RadTreeNode
                                        {
                                            Text = pannel.Item1,
                                            Name = pannel.Item2,
                                            Tag = findpannel
                                        };
                                        Font BoldFont = new Font(radTreeView6.Font, FontStyle.Bold);
                                        pannelnode.Font = BoldFont;



                                        roomnode.Nodes.Add(pannelnode);
                                        PannelList.Remove(pannel);
                                    });


                                    //add room node to level node
                                    LevelNode.Nodes.Add(roomnode);
                                }
                                //add level node to treeview
                                radTreeView5.Nodes.Add(LevelNode);

                            }
                            radTreeView1.Update();
                            CreatepannelNode();
                            
                            
                        }
                        else
                        {
                            CreatepannelNode();
                        }
                        
                    }


                    void CreatepannelNode()
                    {
                        if (PannelList.Count <= 0) return;
                        foreach (var pannelnode in from pannel in PannelList let findpannel = (from o in PannelClassList
                                where o.GetGUID() == pannel.Item2
                                select o)
                            .FirstOrDefault() select new RadTreeNode
                        {
                            Text = pannel.Item1,
                            Name = pannel.Item2,
                            Tag = findpannel
                        })
                        {
                            radTreeView6.Nodes.Add(pannelnode);
                        }
                    }
                    
                }
                
                catch  (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                }
            }
            
            command.Dispose();
            radPropertyGrid1.SelectedObject = BuildNode.Tag;
        }
        private void ReadpannelGrig()
        {
            try
            {
                radGridView2.Columns["ID"].IsVisible = false;
                radGridView2.Columns["GUID"].IsVisible = false;
                radGridView2.Columns["Power"].IsVisible = false;
                radGridView2.Columns["Voltage"].IsVisible = false;
                radGridView2.Columns["Category"].IsVisible = false;
                radGridView2.Columns["FireProtect"].IsVisible = false;
                radGridView2.Columns["Dispatching"].IsVisible = false;
                radGridView2.Columns["Protocol"].IsVisible = false;
                radGridView2.TableElement.RowHeight = 40;
                radGridView2.Columns["Modyfied"].TextAlignment = ContentAlignment.MiddleCenter;
                radGridView2.Columns["Author"].TextAlignment = ContentAlignment.MiddleCenter;
                radGridView2.Columns["Version"].TextAlignment = ContentAlignment.MiddleCenter;
                radGridView2.Columns["PannelName"].ReadOnly = true;
                radGridView2.Columns["Modyfied"].ReadOnly = true;
                radGridView2.Columns["Version"].ReadOnly = true;
                radGridView2.Columns["Author"].ReadOnly = true;
                CreatePannelClass();
                
            }
            catch
            {
                // ignored
            }
        }
        private void ReadProjectGrid(RadGridView radGridView)
        {
            try
            {

                
                radGridView.Columns["ID"].IsVisible = false;
                radGridView.Columns["GUID"].IsVisible = false;
                radGridView.TableElement.RowHeight = 40;
                radGridView.Columns["DateofCreate"].TextAlignment = ContentAlignment.MiddleCenter;
                radGridView.Columns["ProjectName"].TextAlignment = ContentAlignment.MiddleCenter;
                radGridView.Columns["Chiper"].TextAlignment = ContentAlignment.MiddleCenter;
                radGridView.Columns["CheefEngeneer"].TextAlignment = ContentAlignment.MiddleCenter;
                radGridView.Columns["ProjectName"].ReadOnly = true;
                radGridView.Columns["DateofCreate"].ReadOnly = true;
                radGridView.Columns["Chiper"].ReadOnly = true;
                radGridView.Columns["CheefEngeneer"].ReadOnly = true;
                CreateProjectClass(radGridView);
                
                if (radGridView.Rows.Count > 0)
                {
                    radGridView.Rows[0].IsSelected = true;
                }
               

            }
            catch
            {
                // ignored
            }
        }

        private SQLiteConnection OpenDB(string dbfilepath)
        {
            string connectionstr = "Data Source=" + dbfilepath;
            return new SQLiteConnection(connectionstr);
        }
        
        private void radGridView2_CellEndEdit(object sender, GridViewCellEventArgs e)
        {
            try
            {
               
                string GUID = e.Row.Cells["GUID"].Value.ToString();
                string newpannelname = e.Row.Cells["PannelName"].Value.ToString();
                string queryString = "UPDATE Pannel SET PannelName= @newpannelname WHERE GUID= @GUID";
                
                connection.Open();
                if (connection.State != ConnectionState.Open) return;
                SQLiteCommand command = new  SQLiteCommand(queryString, connection);
                command.Parameters.AddWithValue("@newpannelname", newpannelname);
                command.Parameters.AddWithValue("@GUID", GUID);
                command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
                
        }
        private void radGridView2_ContextMenuOpening(object sender, ContextMenuOpeningEventArgs e)
        {
            e.ContextMenu = contextMenu.DropDown;
        }
        private void CreatePannelClass()
        {

            foreach(GridViewRowInfo Row in  radGridView2.Rows)
            {
                try
                {
                    if (Row.Tag != null) continue;
                    string GUID = Row.Cells[1].Value.ToString();
                    string pannelname = Row.Cells[2].Value.ToString();
                    string power = Row.Cells[6].Value.ToString();
                    string voltage = Row.Cells[7].Value.ToString();
                    string category = Row.Cells[8].Value.ToString();
                    string fireprotect = Row.Cells[9].Value.ToString();
                    string dispatching = Row.Cells[10].Value.ToString();
                    string protocol = Row.Cells[11].Value.ToString();
                    int version = Convert.ToInt32(Row.Cells[5].Value.ToString());


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
                catch { }
                
                //MessageBox.Show(Row.Cells[1].Value.ToString());
            }

            if (radGridView2.Rows.Count <= 0) return;
            {
                Pannel pannel = radGridView2.Rows[0].Tag as Pannel;
                radPropertyGrid1.SelectedObject = pannel;
            }
        }
        private void CreateProjectClass(RadGridView radGridView)
        {
            radGridView.Rows.AsParallel().ForAll(Row =>
            {
                try
                {
                    if (Row.Tag != null) return;
                    //Project.ID, Project.GUID, Project.ProjectName, Project.DateofCreate, Project.Chiper, Project.CheefEngeneer

                    Project project = new Project();
                    project.SetGUID(Row.Cells[1].Value.ToString());
                    project.ProjectName = Row.Cells[2].Value.ToString(); //[0]

                    project.Chiper = Row.Cells[4].Value.ToString();
                    project.CheefEngeneer = Row.Cells[5].Value.ToString();
                    Row.Tag = project;
                    RadTreeNode foundNode = FindNodeByValue(project.ProjectName, radTreeView1.Nodes);
                    radPropertyGrid1.SelectedObject = foundNode?.Tag;
                }
                catch { }

            });
        }
        private void radGridView2_SelectionChanging(object sender, GridViewSelectionCancelEventArgs e)
        {
            //AOVGEN.Pannel pannel = e.Rows[0].Tag as AOVGEN.Pannel;

            radPropertyGrid1.SelectedObject = e.Rows[0].Tag;
            //MessageBox.Show(e.Rows[0].Cells[1].Value.ToString());
        }
        private void radPropertyGrid1_PropertyValueChanged(object sender, PropertyGridItemValueChangedEventArgs e)
        {
            string objtype= (radPropertyGrid1.SelectedObject).GetType().Name;
           
            switch (objtype)
                {
                case nameof(Project):
                    ChangeProjectProperty(e);
                    break;
                case nameof(Pannel):
                    ChangePannelProperty(e);
                    break;
                case nameof(Building):
                    ChangBuildProperty(e);
                    break;
                case nameof(SupplyVent):
                    ChangSupplyVentProperty();
                    break;
                case nameof(Froster):
                    ChangeFrosterProperty(e);
                    break;

            }
            
        }
        private void ChangePannelProperty(PropertyGridItemValueChangedEventArgs e)
        {
            try
            {
                var date = DateTime.Now.ToString("dd-MM-yyyy");
                PropertyGridItem current = (PropertyGridItem)e.Item;
                Pannel pannel = radPropertyGrid1.SelectedObject as Pannel;
                if (pannel != null)
                {
                    int vers = pannel.Version;
                    vers++;
                    foreach (GridViewCellInfo cells in radGridView2.SelectedRows[0].Cells)
                    {
                        if (cells.ColumnInfo.Name == current.Name) cells.Value = current.Value;
                        switch (cells.ColumnInfo.Name)
                        {
                            case "Modyfied":
                                cells.Value = date;
                                break;
                            case "Protocol":
                                switch (cells.Value.ToString())
                                {
                                    case "True":
                                        break;
                                    case "False":
                                        break;

                                }
                                //if (cells.Value.ToString() == "none") pannel.Protocol = Pannel._Protocol.None;
                                break;
                            case "Version":
                                cells.Value = vers;
                                pannel.Version = vers;
                                break;
                        }
                    }
                }

                foreach (PropertyGridItem gridItem in radPropertyGrid1.Items)
                {
                    switch (gridItem.Name)
                    {
                        case "Protocol":
                            if (pannel != null) gridItem.Enabled = pannel.Dispatching == Pannel._Dispatching.Yes;


                            break;
                        case "Power":
                            bool b = int.TryParse(gridItem.Value.ToString(), out var number);
                            gridItem.Value = b ? number.ToString() : "0";

                            break;
                        case "PannelName":
                            if (pannel != null)
                            {
                                string queryString = $"UPDATE Pannel SET PannelName= '{gridItem.Value}' WHERE GUID= '{pannel.GetGUID()}'";
                                SQLiteCommand command = new SQLiteCommand
                                {
                                    Connection = connection,
                                    CommandText = queryString
                                };
                                command.ExecuteNonQuery();
                                command.Dispose();
                            }

                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }
        }
        private void Pannel_ProtocolChange(object sender, Pannel._Protocol e)
        {
            Pannel pannel = (Pannel)sender;
            string queryString = $"UPDATE Pannel SET Protocol= '{pannel.Protocol}' WHERE GUID= '{pannel.GetGUID()}'";
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection,
                CommandText = queryString
            };
            command.ExecuteNonQuery();
            command.Dispose();

        }
        private void Pannel_NameChange(object sender, string e)
        {
        }
        private void Pannel_ChangeDispaptching(object sender, Pannel._Dispatching e)
        {
            Pannel pannel = (Pannel)sender;
            if (e == Pannel._Dispatching.No)
            {
                pannel.Protocol = Pannel._Protocol.None;
            }
            else
            {
                if (pannel.Protocol == Pannel._Protocol.None) pannel.Protocol = pannel.OldProtocol;
                
            }
            string queryString = $"UPDATE Pannel SET Dispatching= '{e}' WHERE GUID= '{pannel.GetGUID()}'";
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection,
                CommandText = queryString
            };
            command.ExecuteNonQuery();
            command.Dispose();




        }
        private void ChangeProjectProperty(PropertyGridItemValueChangedEventArgs e)
        {
            try
            {
                var date = DateTime.Now.ToString("dd-MM-yyyy");
                PropertyGridItem current = (PropertyGridItem)e.Item;

                (from GridViewCellInfo t in radGridView2.SelectedRows[0].Cells
                 select t)
                .ToList()
                .AsParallel()
                .ForAll(cells =>
                {
                    
                    if (cells.ColumnInfo.Name == current.Name) cells.Value = current.Value;
                    if (cells.ColumnInfo.Name == "DateofCreate") cells.Value = date;
                });

                //foreach (GridViewCellInfo cells in this.radGridView2.SelectedRows[0].Cells)
                //{
                //    if (cells.ColumnInfo.Name == current.Name) cells.Value = current.Value;
                //    if (cells.ColumnInfo.Name == "DateofCreate") cells.Value = date;
                    
                //}
                if (current.Name != "ProjectName") return;
                RadTreeNode projectnode = radTreeView1.SelectedNode;
                //projectnode.Name = current.Value.ToString();
                projectnode.Value = current.Value;
                projectnode.Text = current.Value.ToString();
                radTreeView1.Update();
                projectnode.Selected = false;
                projectnode.Selected = true;


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }

        }
        private void ChangBuildProperty(PropertyGridItemValueChangedEventArgs e)
        {
            var date = DateTime.Now.ToString("dd-MM-yyyy");
            if (radGridView2.RowCount>0)
            {
                (from GridViewCellInfo t in radGridView2.SelectedRows[0].Cells
                 select t)
                .ToList()
                .AsParallel()
                .ForAll(cells =>
                {
                    if (cells.ColumnInfo.Name == "Modyfied") cells.Value = date;
                });

                //foreach (GridViewCellInfo cells in this.radGridView2.SelectedRows[0].Cells)
                //{
                //    if (cells.ColumnInfo.Name == "Modyfied") cells.Value = date;
                //}
            }
            
            

            PropertyGridItem current = (PropertyGridItem)e.Item;
            string newval = current.Value.ToString();
            if (!(radPropertyGrid1.SelectedObject is Building building)) return;
            string GUID = building.BuildGUID;
            
            string queryString = $"UPDATE Buildings SET {current.Name}= @newval WHERE GUID= @GUID";
            SQLiteCommand command = new  SQLiteCommand(queryString, connection);
            command.Parameters.AddWithValue("@newval", newval);
            command.Parameters.AddWithValue("@GUID", GUID);

            if (connection.State != ConnectionState.Open) return;
            try
            {
                command.ExecuteNonQuery();
                    
                if (current.Name == "Buildname")
                {
                    RadTreeNode buildingnode = radTreeView1.SelectedNode;
                    buildingnode.Name = newval;
                    buildingnode.Value = newval;
                    buildingnode.Text = newval;
                    radTreeView1.Update();
                    buildingnode.Selected = false;
                    buildingnode.Selected = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
                
            command.Dispose();
        }
        private static void ChangSupplyVentProperty ()
        {
            MessageBox.Show("Код для изменения вентилятора в БД");
        }
        private void ChangeFrosterProperty(RadPropertyGridEventArgs e)
        {
            Froster froster = (Froster)radPropertyGrid1.SelectedObject;
            var frosterType = froster._FrosterType;
            string query = string.Empty;
            switch (frosterType)
            {
                
                case Froster.FrosterType.Freon:
                    radPropertyGrid1.PropertyGridElement.PropertyTableElement.BeginUpdate();
                    radPropertyGrid1.Items["Stairs"].Visible = true;
                    radPropertyGrid1.Items["KKBControlType"].Visible = true;
                    radPropertyGrid1.Items["valveType"].Visible = false;
                    radPropertyGrid1.PropertyGridElement.PropertyTableElement.EndUpdate();
                    switch (e.Item.Name)
                    {
                        case "Stairs":
                            query = $"UPDATE KKB SET Stairs = '{froster.Stairs}' WHERE GUID= '{froster._KKB.GUID}'";
                            break;
                    }
                    break;
                case Froster.FrosterType.Water:
                    radPropertyGrid1.PropertyGridElement.PropertyTableElement.BeginUpdate();
                    radPropertyGrid1.Items["Stairs"].Visible = false;
                    radPropertyGrid1.Items["KKBControlType"].Visible = false;
                    radPropertyGrid1.Items["valveType"].Visible = true;
                    radPropertyGrid1.PropertyGridElement.PropertyTableElement.EndUpdate();
                    break;
            }
            _= (PropertyGridItem)e.Item;
            if (query == string.Empty) return;
            bool connectionopen = connection.State == ConnectionState.Open;
            if (!connectionopen) return;
            SQLiteCommand command = new  SQLiteCommand { CommandText = query, Connection = connection };
                       
            command.ExecuteNonQuery();
            command.Dispose();
            //PropertyGridItem current = (PropertyGridItem)e.Item;
            //MessageBox.Show(current.Name); //имя изменяемого свойства


        }
        private void radGridView2_CellValueChanged(object sender, GridViewCellEventArgs e)
        {
            
            try
            {
                string objtype = (e.Row.Tag).GetType().ToString();
                string queryString = string.Empty;
                string GUID = string.Empty;
                string newval = e.Value.ToString();
                string column = e.Column.HeaderText;
                 SQLiteCommand command = new  SQLiteCommand();
                

                switch (objtype)
                {
                    case "AOVGEN.Pannel":
                        if (e.Row.Tag is Pannel pannel)
                        {
                            GUID = pannel.GetGUID();
                            queryString = $"UPDATE Pannel SET {column}= @newval WHERE GUID= @GUID";
                        }
                        break;
                    case "AOVGEN.Building":
                        if (e.Row.Tag is Building building)
                        {
                            GUID = building.BuildGUID;
                            queryString = $"UPDATE Buildings SET {column}= @newval WHERE GUID= @GUID";
                        }
                        break;
                    case "AOVGEN.Project":
                        if (e.Row.Tag is Project project)
                        {
                            GUID = project.GetGUID();
                            queryString = $"UPDATE Project SET {column}= @newval WHERE GUID= @GUID";
                        }
                        break;
                }

                if (connection.State != ConnectionState.Open) return;
                command.CommandText = queryString;
                command.Connection = connection;
                command.Parameters.AddWithValue("@newval", newval);
                command.Parameters.AddWithValue("@GUID", GUID);
                command.ExecuteNonQuery();
                command.Dispose();




            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                
            }

        }
        private void radPropertyGrid1_SelectedObjectChanged(object sender, PropertyGridSelectedObjectChangedEventArgs e)
        {
            string selobj = e.SelectedObject?.GetType().Name;
            switch (selobj)
            {
                case nameof(Pannel):
                    Pannel pannel = (Pannel)e.SelectedObject;
                    
                    pannel.ProtocolChange += Pannel_ProtocolChange;
                    pannel.NameChange += Pannel_NameChange;
                    pannel.ChangeDispaptching += Pannel_ChangeDispaptching;
                    var ProtocolItem = radPropertyGrid1.Items
                        .FirstOrDefault(item => item.Name == "Protocol");
                   if (ProtocolItem != null)
                    {
                        ProtocolItem.Enabled = pannel.Dispatching != Pannel._Dispatching.No;
                    }
                    break;
            }
        }
        private void radGridView2_CellClick(object sender, GridViewCellEventArgs e)
        {
            var Row = e.Row;
            radPropertyGrid1.SelectedObject = Row.Tag;
            _ = Row.Tag as Pannel;

        }
        private GridViewRowInfo CreateVentSystemsRows(string elementGUID, string ComponentType, string location)
        {
            if (!(radGridView1.DataSource is DataTable dataSource)) return radGridView1.CurrentRow;
            DataRow drToAdd = dataSource.NewRow();
            drToAdd["ID"] = dataSource.Rows.Count + 1;
            drToAdd["GUID"] = elementGUID;
            drToAdd["Тип элемента"] = ComponentType;
            drToAdd["Расположение"] = location;
            dataSource.Rows.Add(drToAdd);

            //dataSource.AcceptChanges();
            return radGridView1.CurrentRow;
            
        }
        //private GridViewRowInfo CreateCableRows(string cableGUID, string CableName, string fromm, string to, string cabletype, string lenght)
        private void CreateCableRows(string cableGUID, string CableName, string fromm, string to, string cabletype, string lenght, string systemname, string description, string sort, string attribute, string writeblock, string blockname, string WireNumbers, string tableforsearch, string devguid, bool writepumpcable, bool writevalvecable, string fromGUID)
        {
            //GridViewComboBoxColumn comboColumn = new GridViewComboBoxColumn("listbox");

            double lenght1 = 0;
            if (radGridView3.DataSource is DataTable dataSource)
            {
                DataRow drToAdd = dataSource.NewRow();
                drToAdd["ID"] = dataSource.Rows.Count + 1;
                drToAdd["GUID"] = cableGUID;
                drToAdd["Имя"] = CableName;
                drToAdd["Откуда"] = fromm;
                drToAdd["Куда"] = to;
                drToAdd["Тип"] = cabletype;
                drToAdd["Длина"] = lenght;
                drToAdd["Система"] = systemname;
                drToAdd["Описание"] = description;
                drToAdd["Сортировка"] = sort;
                drToAdd["Атрибут"] = attribute;
                drToAdd["WriteBlock"] = writeblock;
                drToAdd["BlockName"] = blockname;
                drToAdd["WireNumbers"] = WireNumbers;
                double.TryParse(lenght, out lenght1);
                dataSource.Rows.Add(drToAdd);
            }

            GridViewRowInfo row = radGridView3.CurrentRow;
            SQLiteCommand command = new  SQLiteCommand { Connection = connection };
            Cable cable = new Cable
            {
                WriteBlock = Convert.ToBoolean(writeblock),
                cableGUID = cableGUID,
                CableType = cabletype,
                CableName = CableName,
                FromPosName = fromm,
                ToPosName = to,
                Lenght = lenght1,
                Description = description,
                ToBlockName = blockname,
                WireNumbers = Convert.ToInt32(WireNumbers),
                SortPriority = sort,
                HostTable = tableforsearch,
                ToGUID = devguid,
                FromGUID = fromGUID
                

            };
            PropertyInfo pinfo = cable.GetType().GetProperty("Attrubute");
                        
            switch (tableforsearch)
            {
                case "Pump":
                    switch (writepumpcable)
                    {
                        case true when cable.WriteBlock:
                            attribute = "PL";
                            break;
                        case false when cable.WriteBlock:
                            attribute = "P";
                            break;
                    }
                   
                    string query1 = $"UPDATE Cable SET CableAttribute = '{attribute}' WHERE GUID = '{cable.cableGUID}'";
                    command.CommandText = query1;
                    command.ExecuteNonQuery();

                    break;
                case "Valve":
                case "Damper":
                    switch (writevalvecable)
                    {
                        case true when cable.WriteBlock:
                            attribute = "PL";
                            break;
                        case false when cable.WriteBlock:
                            attribute = "P";
                            break;
                    }
                    string query2 = $"UPDATE Cable SET CableAttribute = '{attribute}' WHERE GUID = '{cable.cableGUID}'";
                    command.CommandText = query2;
                    command.ExecuteNonQuery();

                    break;
                


            }
            
            command.Dispose();

            pinfo?.SetValue(cable, Enum.Parse(pinfo.PropertyType, attribute));
            row.Tag = cable;
           
            
            //dataSource.AcceptChanges();
            //return this.radGridView3.SelectedRows[0];
        }
        private void CreateHeader()
        {
            DataTable dataSource = new DataTable("fileSystem");
            dataSource.Columns.Add("ID", typeof(string));
            dataSource.Columns.Add("GUID", typeof(string));
            dataSource.Columns.Add("Тип элемента", typeof(string));
            dataSource.Columns.Add("Расположение", typeof(string));
            dataSource.Columns.Add("Сортировка", typeof(string));
           
            radGridView1.DataSource = dataSource;
            radGridView1.Columns["ID"].IsVisible = false;
            radGridView1.Columns["GUID"].IsVisible = false;
            radGridView1.Columns["Сортировка"].IsVisible = false;
           
            radGridView1.Columns["Тип элемента"].TextAlignment = ContentAlignment.MiddleCenter;
            radGridView1.Columns["Тип элемента"].ReadOnly = true;
            radGridView1.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
            

            radGridView1.GroupDescriptors.Add(new GroupDescriptor("Расположение"));
            
        }
        private void CreateCableHeader()
        {
            radGridView3.Columns.Clear();
            List<string> list = new List<string>();
            DataTable dataSource = new DataTable("Cables");
            dataSource.Columns.Add("ID", typeof(string));
            dataSource.Columns.Add("GUID", typeof(string));
            dataSource.Columns.Add("Имя", typeof(string));
            dataSource.Columns.Add("Откуда", typeof(string));
            dataSource.Columns.Add("Куда", typeof(string));
            dataSource.Columns.Add("Тип", typeof(string));
            dataSource.Columns.Add("Длина", typeof(string));
            dataSource.Columns.Add("Описание", typeof(string));
            dataSource.Columns.Add("Система", typeof(string));
            dataSource.Columns.Add("Сортировка", typeof(string));
            dataSource.Columns.Add("Атрибут", typeof(string));
            dataSource.Columns.Add("WriteBlock", typeof(string));
            dataSource.Columns.Add("BlockName", typeof(string));
            dataSource.Columns.Add("WireNumbers", typeof(int));

            radGridView3.MasterTemplate.AutoGenerateColumns = false;
            if (connection.State == ConnectionState.Open)
            {
                string query = "SELECT CableFullName FROM CableTypes";
                SQLiteCommand command = new  SQLiteCommand { Connection = connection, CommandText = query };
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(reader.GetString(0));
                    }
                    reader.Close();
                }
                command.Dispose();
            }
                
           
            GridViewComboBoxColumn comboCol = new GridViewComboBoxColumn("Тип")
            {
                DataSource = list.ToArray(),
                FieldName = "Тип",
                DropDownStyle = RadDropDownStyle.DropDownList
            };

            radGridView3.Columns.Add(new GridViewTextBoxColumn("ID"));
            radGridView3.Columns.Add(new GridViewTextBoxColumn("GUID"));
            radGridView3.Columns.Add(new GridViewTextBoxColumn("Имя"));
            radGridView3.Columns.Add(new GridViewTextBoxColumn("Откуда"));
            radGridView3.Columns.Add(new GridViewTextBoxColumn("Куда"));
            radGridView3.Columns.Add(comboCol);
            radGridView3.Columns.Add(new GridViewTextBoxColumn("Длина"));
            radGridView3.Columns.Add(new GridViewTextBoxColumn("Описание"));
            radGridView3.Columns.Add(new GridViewTextBoxColumn("Система"));
            radGridView3.Columns.Add(new GridViewTextBoxColumn("Сортировка"));
            radGridView3.Columns.Add(new GridViewTextBoxColumn("Атрибут"));
            radGridView3.Columns.Add(new GridViewTextBoxColumn("WriteBlock"));
            radGridView3.Columns.Add(new GridViewTextBoxColumn("BlockName"));
            radGridView3.Columns.Add(new GridViewTextBoxColumn("WireNumbers"));


            radGridView3.Columns["ID"].IsVisible = false;
            radGridView3.Columns["GUID"].IsVisible = false;
            radGridView3.Columns["Сортировка"].IsVisible = false;
            radGridView3.Columns["Атрибут"].IsVisible = false;
            radGridView3.Columns["WriteBlock"].IsVisible = false;
            radGridView3.Columns["BlockName"].IsVisible = false;
            radGridView3.Columns["WireNumbers"].IsVisible = false;
            radGridView3.Columns["Имя"].TextAlignment = ContentAlignment.MiddleCenter;
            radGridView3.Columns["Куда"].TextAlignment = ContentAlignment.MiddleCenter;
            radGridView3.Columns["Откуда"].TextAlignment = ContentAlignment.MiddleCenter;
            radGridView3.Columns["Система"].TextAlignment = ContentAlignment.MiddleCenter;
            radGridView3.Columns["Длина"].TextAlignment = ContentAlignment.MiddleCenter;
            radGridView3.Columns["Откуда"].ReadOnly = true;
            radGridView3.Columns["Куда"].ReadOnly = true;
            radGridView3.Columns["Система"].ReadOnly = true;
            radGridView3.Columns["Описание"].ReadOnly = true;
            radGridView3.Columns["Описание"].AutoSizeMode = BestFitColumnMode.SummaryRowCells;
            
            radGridView3.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
            radGridView3.Columns["Имя"].Width = 20;
            radGridView3.Columns["Куда"].Width = 30;
            radGridView3.Columns["Откуда"].Width = 30;
            radGridView3.Columns["Тип"].Width = 74;
            radGridView3.Columns["Длина"].Width = 20;
            radGridView3.Columns["Система"].Width = 30;
            radGridView3.DataSource = dataSource;

            //this.radGridView2.GroupDescriptors.Add(new Telerik.WinControls.Data.GroupDescriptor("Расположение"));
        }
        void SetSensorVendorInfo(IVendoInfo vendoInfo, string article)
        {
            if (!string.IsNullOrEmpty(article))
            {
                vendoInfo.SetVendorInfo(null, article, null, null, null);
                string sensDBTable = vendoInfo.GetVendorInfo().DBTable;
                if (!string.IsNullOrEmpty(sensDBTable))
                {
                    SQLiteCommand command = new SQLiteCommand
                    {
                        Connection = connectionvendor,
                        CommandText = $"SELECT VendorName, BlockName, Assignment, Description FROM {sensDBTable} WHERE VendorCode = '{vendoInfo.GetVendorInfo().ID}'"
                    };
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            vendoInfo.SetVendorInfo(reader[0].ToString(), null, reader[3].ToString(), null, reader[2].ToString());
                        }
                    }
                    command.Dispose();
                }
                
            }
           
        }
        private EditorV2.PosInfo GetPosInfo(string PosInfoGUID, object obj)
        {
            
            if (!string.IsNullOrEmpty(PosInfoGUID))
            {
                SQLiteCommand PosInfoCommand = new SQLiteCommand
                {
                    Connection = connection,
                    CommandText = $"SELECT Pos, Size, Image FROM PosInfo WHERE [GUID] = '{PosInfoGUID}'"
                };
                
                using (SQLiteDataReader readerPosInfo = PosInfoCommand.ExecuteReader())
                {

                    EditorV2.PosInfo posInfo = new EditorV2.PosInfo();
                    while (readerPosInfo.Read())
                    {
                        posInfo.ImageName = readerPosInfo[2].ToString();
                        posInfo.GUID = PosInfoGUID;
                        posInfo.PosDBToClassPos(readerPosInfo[0].ToString());
                        posInfo.SizeDBtoClassSize(readerPosInfo[1].ToString());
                    }
                    posInfo.Tag = obj;
                    return posInfo;
                }
            }
            return null;
        }
        private List<EditorV2.PosInfo> ReadVent<T>(string guid, SQLiteCommand command, string WHEREQuery)
        {
            
            
            string additionalSelect = string.Empty;
            switch (typeof(T).Name)
            {
                case nameof(SupplyVent):
                    additionalSelect = "AND Location = 'Supply'";
                    break;
                case nameof(ExtVent):
                    additionalSelect = "AND Location = 'Exhaust'";
                    break;
                
            }
            string tableselect = $"SELECT SystemGUID, Voltage, ControlType, Power, ProtectType, SensPDS, Cable1, GUID, FControl, PosInfoGUID, Location, Description, AttributeSpare FROM Ventilator WHERE {WHEREQuery} = '{guid}' {additionalSelect}";
            command.CommandText = tableselect;
            string senstype = string.Empty;
            string sensguid = string.Empty;
            string sensVendorCode = string.Empty;
            string FControlVendorcode = string.Empty;
            string FControlDescription = string.Empty;
            List<EditorV2.PosInfo> posinfosList = new List<EditorV2.PosInfo>();
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var voltage = reader[1].ToString();
                    var controltype = reader[2].ToString();
                    var protect = reader[4].ToString();
                    var power = reader[3].ToString();
                    var senspds = reader[5].ToString();
                    var compguid = reader[7].ToString();
                    var FControlGUID = reader[8].ToString();
                    var PosInfoGUID = reader[9].ToString();
                    var location = reader[10].ToString();
                    var description = reader[11].ToString();
                    var attributeSpare= reader[12].ToString();
                    if (!string.IsNullOrEmpty(senspds))
                    {
                        SQLiteCommand sensCommand = new SQLiteCommand
                        {
                            CommandText = sensPQery(senspds),
                            Connection = connection
                        };
                        using (SQLiteDataReader readerSens = sensCommand.ExecuteReader())
                        {
                            
                            while (readerSens.Read())
                            {
                                senstype = readerSens[0].ToString();
                                sensguid = readerSens[1].ToString();
                                sensVendorCode = readerSens[3].ToString();
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(FControlGUID))
                    {
                        SQLiteCommand FCCommand = new SQLiteCommand
                        {
                            CommandText = $"SELECT VendorCode, Description FROM FControl WHERE [GUID] = '{FControlGUID}'",
                            Connection = connection
                        };
                        using (SQLiteDataReader readerFC = FCCommand.ExecuteReader())
                        {
                            while (readerFC.Read())
                            {
                                FControlVendorcode = readerFC[0].ToString();
                                FControlDescription = readerFC[1].ToString();
                            }
                        }
                    }
                    PropertyInfo pinfo;
                    switch (typeof(T).Name)
                    {
                        case nameof(SupplyVent):
                            if (!string.IsNullOrEmpty(location))
                            {
                                SupplyVent supplyVent;
                                bool isReserved = attributeSpare == "Reserved";

                                if (string.IsNullOrEmpty(attributeSpare))
                                {
                                    supplyVent = new SupplyVent
                                    {
                                        GUID = compguid,
                                        Description = description
                                    };
                                }
                                else
                                {
                                    if (isReserved)
                                    {
                                        supplyVent = new SupplyVent(true, true)
                                        {
                                            GUID = compguid,
                                            Description = description,
                                            AttributeSpare = attributeSpare
                                        };
                                    }
                                    else
                                    {
                                        supplyVent= new SupplyVent(false, true)
                                        {
                                            GUID = compguid,
                                            Description = description,
                                            AttributeSpare = attributeSpare
                                        };
                                    }
                                }
                                
                                pinfo = supplyVent.GetType().GetProperty("Voltage");
                                pinfo?.SetValue(supplyVent, Enum.Parse(pinfo.PropertyType, voltage));
                                pinfo = supplyVent.GetType().GetProperty("ControlType");
                                pinfo?.SetValue(supplyVent, Enum.Parse(pinfo.PropertyType, controltype));
                                supplyVent.Power = power;
                                pinfo = supplyVent.GetType().GetProperty("Protect");
                                pinfo?.SetValue(supplyVent, Enum.Parse(pinfo.PropertyType, protect));
                                
                                supplyVent.Power = power;
                                
                                if (!string.IsNullOrEmpty(senspds))
                                {
                                    pinfo = supplyVent.GetType().GetProperty("PressureProtect");
                                    pinfo?.SetValue(supplyVent, Enum.Parse(pinfo.PropertyType, senstype));//
                                    supplyVent._PressureContol.GUID = sensguid;
                                    IVendoInfo vendoInfo = supplyVent._PressureContol;
                                    SetSensorVendorInfo(vendoInfo, sensVendorCode);
                                }
                                else
                                {
                                    supplyVent.PressureProtect = Vent._PressureProtect.None;
                                }
                                if (supplyVent._FControl != null)//if (!string.IsNullOrEmpty(FControlVendorcode))
                                {
                                    supplyVent._FControl.GUID = FControlGUID;
                                    supplyVent._FControl.Description = FControlDescription;
                                    IVendoInfo vendoInfo = supplyVent._FControl;
                                    SetSensorVendorInfo(vendoInfo, FControlVendorcode);
                                    
                                    //if (supplyVent.ShemaASU != null)
                                    //{
                                    //    switch (supplyVent.ControlType)
                                    //    {
                                    //        case Vent.AHUContolType.Direct:
                                    //            supplyVent.ShemaASU.Description1 = "Пуск приточного вентилятора";
                                    //            break;
                                    //        case Vent.AHUContolType.Soft:
                                    //            //supplyVent.ShemaASU.ShemaUp = "Supply_Vent_RO";
                                    //            supplyVent.ShemaASU.Description1 = "Управление приточым вентилятором, плавный пуск";
                                    //            break;
                                    //        case Vent.AHUContolType.FCControl:
                                    //            //supplyVent.ShemaASU.ShemaUp = "Supply_Vent_RO";
                                    //            supplyVent.ShemaASU.Description1= "Управление приточым вентилятором, регулирование скорости";
                                    //            break;
                                    //        case Vent.AHUContolType.Transworm:
                                    //            //supplyVent.ShemaASU.ShemaUp = "Supply_Vent_RO";
                                    //            supplyVent.ShemaASU.Description1= "Управление приточым вентилятором через трансформатор";
                                    //            break;

                                    //    }
                                    //}
                                }
                                EditorV2.PosInfo SupplyVentPosInfo = GetPosInfo(PosInfoGUID, supplyVent);
                                if (SupplyVentPosInfo != null)
                                {
                                    SupplyVentPosInfo.Tag = supplyVent;
                                    posinfosList.Add(SupplyVentPosInfo);
                                }
                            }
                            break;
                        case nameof(ExtVent):
                            if (!string.IsNullOrEmpty(location))
                            {
                                ExtVent extVent;
                                bool isReserved = attributeSpare == "Reserved";

                                if (string.IsNullOrEmpty(attributeSpare))
                                {
                                    extVent = new ExtVent
                                    {
                                        GUID = compguid,
                                        Description = description
                                    };
                                }
                                else
                                {
                                    if (isReserved)
                                    {
                                        extVent = new ExtVent(true, true)
                                        {
                                            GUID = compguid,
                                            Description = description,
                                            AttributeSpare = attributeSpare
                                        };
                                    }
                                    else
                                    {
                                        extVent = new ExtVent(false, true)
                                        {
                                            GUID = compguid,
                                            Description = description,
                                            AttributeSpare = attributeSpare
                                        };
                                    }
                                    
                                    
                                }

                                pinfo = extVent.GetType().GetProperty("Voltage");
                                pinfo?.SetValue(extVent, Enum.Parse(pinfo.PropertyType, voltage));
                                pinfo = extVent.GetType().GetProperty("ControlType");
                                pinfo?.SetValue(extVent, Enum.Parse(pinfo.PropertyType, controltype));
                                extVent.Power = power;
                                pinfo = extVent.GetType().GetProperty("Protect");
                                pinfo?.SetValue(extVent, Enum.Parse(pinfo.PropertyType, protect));
                                extVent.Power = power;

                                if (!string.IsNullOrEmpty(senspds))
                                {
                                    pinfo = extVent.GetType().GetProperty("PressureProtect");
                                    pinfo.SetValue(extVent, Enum.Parse(pinfo.PropertyType, senstype));
                                    extVent._PressureContol.GUID = sensguid;
                                    IVendoInfo vendoInfo = extVent._PressureContol;
                                    SetSensorVendorInfo(vendoInfo, sensVendorCode);
                                }
                                else
                                {
                                    extVent.PressureProtect = Vent._PressureProtect.None;
                                }
                                if (extVent._FControl != null)//if (!string.IsNullOrEmpty(FControlVendorcode))
                                {
                                    extVent._FControl.GUID = FControlGUID;
                                    extVent._FControl.Description = FControlDescription;
                                    IVendoInfo vendoInfo = extVent._FControl;
                                    SetSensorVendorInfo(vendoInfo, FControlVendorcode);
                                    //if (extVent.ShemaASU != null)
                                    //{

                                    //    switch (extVent.ControlType)
                                    //    {
                                    //        case Vent.AHUContolType.Direct:
                                    //            break;
                                    //        case Vent.AHUContolType.Soft:
                                    //        extVent.ShemaASU.ShemaUp = "Ext_Vent_RO";
                                    //            break;
                                    //        case Vent.AHUContolType.FCControl:
                                    //            extVent.ShemaASU.ShemaUp = "Ext_Vent_RO";
                                    //            break;
                                    //        case Vent.AHUContolType.Transworm:
                                    //            extVent.ShemaASU.ShemaUp = "Ext_Vent_RO";
                                    //            break;
                                    //    }
                                    //}
                                    
                                }
                                EditorV2.PosInfo ExtVentpos = GetPosInfo(PosInfoGUID, extVent);
                                if (ExtVentpos != null)
                                {
                                    ExtVentpos.Tag = extVent;
                                    posinfosList.Add(ExtVentpos);
                                }
                            }
                            break;
                       
                            
                    }
                    
                }

                
            }
            return posinfosList;
        }
        private List<EditorV2.PosInfo> ReadSpareVent<T>(string guid, SQLiteCommand command, string WHEREQuery)
        {


            string additionalSelect = string.Empty;
            switch (typeof(T).Name)
            {
                
                case nameof(SpareExtVent):
                    additionalSelect = "AND Location = 'Exhaust'";
                break;
                case nameof(SpareSuplyVent):
                    additionalSelect = "AND Location = 'Supply'";
                    break;
            }
            string tableselect = $"SELECT SystemGUID, Voltage, ControlType, Power, ProtectType, SensPDS, GUID, PosInfoGUID, Location, [MainVent], [ReservedVent] FROM SpareVentilator WHERE {WHEREQuery} = '{guid}' {additionalSelect}";
            command.CommandText = tableselect;
            string senstype = string.Empty;
            string sensguid = string.Empty;
            string sensVendorCode = string.Empty;
            string FControlVendorcode = string.Empty;
            SupplyVent mainSupplyVent = null;
            SupplyVent reservedSupplyVent = null;
            ExtVent mainExtVent = null;
            ExtVent reserverdExtVent = null;
            List<EditorV2.PosInfo> posinfosList = new List<EditorV2.PosInfo>();
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                if (!reader.HasRows) return posinfosList;
                
                while (reader.Read())
                {
                    var voltage = reader[1].ToString();
                    var controltype = reader[2].ToString();
                    var protect = reader[4].ToString();
                    var power = reader[3].ToString();
                    var senspds = reader[5].ToString();
                    var compguid = reader[6].ToString();
                    var PosInfoGUID = reader[7].ToString();
                    var location = reader[8].ToString();
                    var mainVentID = reader[9].ToString();
                    var reservedVentID = reader[10].ToString();

                    if (!string.IsNullOrEmpty(senspds))
                    {
                        SQLiteCommand sensCommand = new SQLiteCommand
                        {
                            CommandText = sensPQery(senspds),
                            Connection = connection
                        };
                        using (SQLiteDataReader readerSens = sensCommand.ExecuteReader())
                        {

                            while (readerSens.Read())
                            {
                                senstype = readerSens[0].ToString();
                                sensguid = readerSens[1].ToString();
                                sensVendorCode = readerSens[3].ToString();
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(mainVentID))
                    {
                        SQLiteCommand readMainVentCommand = new SQLiteCommand
                        {
                            Connection = connection
                        };

                        switch (typeof(T).Name)
                        {
                            case nameof(SpareSuplyVent):
                                mainSupplyVent = ReadVent<SupplyVent>(mainVentID, readMainVentCommand, "GUID")
                                    .FirstOrDefault()
                                    ?.Tag as SupplyVent;
                                break;
                            case nameof(SpareExtVent):
                                mainExtVent = ReadVent<ExtVent>(mainVentID, readMainVentCommand, "GUID")
                                    .FirstOrDefault()
                                    ?.Tag as ExtVent;
                                
                                break;
                        }

                    }
                    if (!string.IsNullOrEmpty(reservedVentID))
                    {
                        SQLiteCommand readReservrdVentCommand = new SQLiteCommand
                        {
                            Connection = connection
                        };
                        switch (typeof(T).Name)
                        {
                            case nameof(SpareSuplyVent):
                                reservedSupplyVent = ReadVent<SupplyVent>(reservedVentID, readReservrdVentCommand, "GUID")
                                    .FirstOrDefault()
                                    ?.Tag as SupplyVent;
                                break;
                            case nameof(SpareExtVent):
                                reserverdExtVent = ReadVent<ExtVent>(reservedVentID, readReservrdVentCommand, "GUID")
                                    .FirstOrDefault()
                                    ?.Tag as ExtVent;
                                break;
                        }
                        

                    }



                    switch (typeof(T).Name)
                    {
                        case nameof(SpareSuplyVent):
                            var spareSuplyVent = new SpareSuplyVent
                            {
                                GUID = compguid
                            };

                            if (!string.IsNullOrEmpty(location))
                            {
                                spareSuplyVent.Power = power;
                                PropertyInfo pinfo = spareSuplyVent.GetType().GetProperty("Voltage");
                                pinfo?.SetValue(spareSuplyVent, Enum.Parse(pinfo.PropertyType, voltage));

                                pinfo = spareSuplyVent.GetType().GetProperty("ControlType");
                                pinfo?.SetValue(spareSuplyVent, Enum.Parse(pinfo.PropertyType, controltype));


                                pinfo = spareSuplyVent.GetType().GetProperty("Protect");
                                pinfo?.SetValue(spareSuplyVent, Enum.Parse(pinfo.PropertyType, protect));


                                SetVentAttr(mainSupplyVent, true);
                                SetVentAttr(reservedSupplyVent, false);
                                if (mainSupplyVent != null)
                                {
                                    spareSuplyVent.MainSupplyVent = mainSupplyVent;
                                    spareSuplyVent.shemaAsu.Description1 = mainSupplyVent.ShemaASU.Description1;
                                    spareSuplyVent.shemaAsu.ShemaUp = mainSupplyVent.ShemaASU.ShemaUp;
                                }

                                if (reservedSupplyVent != null)
                                {
                                    spareSuplyVent.ReservedSupplyVent = reservedSupplyVent;
                                    spareSuplyVent.shemaAsu.Description2 = reservedSupplyVent.ShemaASU.Description1;
                                    spareSuplyVent.shemaAsu.ShemaUp = reservedSupplyVent.ShemaASU.ShemaUp;
                                }


                                if (!string.IsNullOrEmpty(senspds))
                                {

                                    pinfo = spareSuplyVent.GetType().GetProperty("SensorType");
                                    pinfo?.SetValue(spareSuplyVent._PressureContol, Enum.Parse(pinfo.PropertyType, senstype));//
                                    spareSuplyVent._PressureContol.GUID = sensguid;
                                    IVendoInfo vendoInfo = spareSuplyVent._PressureContol;
                                    SetSensorVendorInfo(vendoInfo, sensVendorCode);
                                    if (spareSuplyVent._PressureContol != null && mainSupplyVent._PressureContol != null)
                                    {
                                        spareSuplyVent._PressureContol.ShemaASU.ShemaUp = mainSupplyVent._PressureContol.ShemaASU.ShemaUp;
                                    }
                                }
                                else
                                {
                                    spareSuplyVent._PressureContol.SensorType = Sensor.SensorType.No;//s.PressureProtect = Vent._PressureProtect.None;
                                }
                                EditorV2.PosInfo SupplyVentPosInfo = GetPosInfo(PosInfoGUID, spareSuplyVent);
                                if (SupplyVentPosInfo != null)
                                {
                                    SupplyVentPosInfo.Tag = spareSuplyVent;
                                    posinfosList.Add(SupplyVentPosInfo);
                                }



                                void SetVentAttr(Vent supplyVent, bool isMain)
                                {

                                    if (supplyVent.ShemaASU != null)
                                    {
                                        switch (supplyVent.ControlType)
                                        {
                                            case Vent.AHUContolType.Direct:
                                                supplyVent.ShemaASU.Description1 = isMain
                                                    ? "Пуск основного приточного вентилятора"
                                                    : "Пуск резервного приточного вентилятора";
                                                break;
                                            case Vent.AHUContolType.Soft:
                                                supplyVent.ShemaASU.Description1 = isMain
                                                    ? "Управление основным приточым вентилятором, плавный пуск"
                                                    : "Управление резевным приточым вентилятором, плавный пуск";
                                                break;
                                            case Vent.AHUContolType.FCControl:
                                                supplyVent.ShemaASU.Description1 = isMain
                                                    ? "Управление основным приточым вентилятором, регулирование скорости"
                                                    : "Управление резевным приточым вентилятором, регулирование скорости";
                                                break;
                                            case Vent.AHUContolType.Transworm:
                                                supplyVent.ShemaASU.Description1 = isMain
                                                    ? "Управление основным приточым вентилятором через трансформатор"
                                                    : "Управление резевным приточым вентилятором через трансформатор";
                                                break;
                                        }
                                    }


                                }

                            }
                            break;
                        case nameof(SpareExtVent):
                            var spareExtVent = new SpareExtVent
                            {
                                GUID = compguid
                            };
                            if (!string.IsNullOrEmpty(location))
                            {
                                spareExtVent.Power = power;
                                PropertyInfo pinfo = spareExtVent.GetType().GetProperty("Voltage");
                                pinfo?.SetValue(spareExtVent, Enum.Parse(pinfo.PropertyType, voltage));

                                pinfo = spareExtVent.GetType().GetProperty("ControlType");
                                pinfo?.SetValue(spareExtVent, Enum.Parse(pinfo.PropertyType, controltype));


                                pinfo = spareExtVent.GetType().GetProperty("Protect");
                                pinfo?.SetValue(spareExtVent, Enum.Parse(pinfo.PropertyType, protect));


                                SetVentAttr(mainExtVent, true);
                                SetVentAttr(reserverdExtVent, false);
                                if (mainExtVent != null)
                                {
                                    spareExtVent.MainExtVent= mainExtVent;
                                    spareExtVent.shemaAsu.Description1 = mainExtVent.ShemaASU.Description1;
                                    spareExtVent.shemaAsu.ShemaUp = mainExtVent.ShemaASU.ShemaUp;
                                }

                                if (reserverdExtVent != null)
                                {
                                    spareExtVent.ReservedExtVent= reserverdExtVent;
                                    spareExtVent.shemaAsu.Description2 = reserverdExtVent.ShemaASU.Description1;
                                    spareExtVent.shemaAsu.ShemaUp = reserverdExtVent.ShemaASU.ShemaUp;
                                }
                                

                                if (!string.IsNullOrEmpty(senspds))
                                {

                                    pinfo = spareExtVent.GetType().GetProperty("SensorType");
                                    pinfo?.SetValue(spareExtVent._PressureContol, Enum.Parse(pinfo.PropertyType, senstype));//
                                    spareExtVent._PressureContol.GUID = sensguid;
                                    IVendoInfo vendoInfo = spareExtVent._PressureContol;
                                    SetSensorVendorInfo(vendoInfo, sensVendorCode);
                                    if (spareExtVent._PressureContol != null && mainExtVent._PressureContol != null)
                                    {
                                        spareExtVent._PressureContol.ShemaASU.ShemaUp = mainExtVent._PressureContol.ShemaASU.ShemaUp;
                                    }
                                }
                                else
                                {
                                    spareExtVent._PressureContol.SensorType = Sensor.SensorType.No;//s.PressureProtect = Vent._PressureProtect.None;
                                }
                                EditorV2.PosInfo ExtVentPosinfo = GetPosInfo(PosInfoGUID, spareExtVent);
                                if (ExtVentPosinfo != null)
                                {
                                    ExtVentPosinfo.Tag = spareExtVent;
                                    posinfosList.Add(ExtVentPosinfo);
                                }



                                void SetVentAttr(Vent extVent, bool isMain)
                                {

                                    if (extVent.ShemaASU != null)
                                    {
                                        switch (extVent.ControlType)
                                        {
                                            case Vent.AHUContolType.Direct:
                                                extVent.ShemaASU.Description1 = isMain
                                                    ? "Пуск основного вытяжного вентилятора"
                                                    : "Пуск резервного вытяжного вентилятора";
                                                break;
                                            case Vent.AHUContolType.Soft:
                                                extVent.ShemaASU.Description1 = isMain
                                                    ? "Управление основным вытяжным вентилятором, плавный пуск"
                                                    : "Управление резевным вытяжным вентилятором, плавный пуск";
                                                break;
                                            case Vent.AHUContolType.FCControl:
                                                extVent.ShemaASU.Description1 = isMain
                                                    ? "Управление основным вытяжным вентилятором, регулирование скорости"
                                                    : "Управление резевным вытяжным вентилятором, регулирование скорости";
                                                break;
                                            case Vent.AHUContolType.Transworm:
                                                extVent.ShemaASU.Description1 = isMain
                                                    ? "Управление основным вытяжным вентилятором через трансформатор"
                                                    : "Управление резевным вытяжным вентилятором через трансформатор";
                                                break;
                                        }
                                    }


                                }

                            }
                            break;
                            

                    }
                }

            }
            return posinfosList;
        }
        private List<EditorV2.PosInfo> ReadFiltr(string guid, SQLiteCommand command, string WHEREQuery)
        {
            
            string tableselect = $"SELECT GUID, ControlType, SensPDS, GUID, PosInfoGUID, Location FROM Filter WHERE {WHEREQuery} = '{guid}'";
            command.CommandText = tableselect;
            string senspds;
            string sensguid = string.Empty;
            string controltype;
            string sensVendorCode = string.Empty;
            string PosInfoGUID;
            string SensDescription = string.Empty;
            PropertyInfo pinfo;
            List<EditorV2.PosInfo> posinfosList = new List<EditorV2.PosInfo>();
            using (SQLiteDataReader reader1 = command.ExecuteReader())
            {
                while (reader1.Read())
                {
                    controltype = reader1[1].ToString();
                    senspds = reader1[2].ToString();
                    var compguid = reader1[3].ToString();
                    PosInfoGUID = reader1[4].ToString();
                    var location = reader1[5].ToString();
                    if (!string.IsNullOrEmpty(senspds))
                    {
                        SQLiteCommand sensCommand = new SQLiteCommand
                        {
                            CommandText = sensPQery(senspds),
                            Connection = connection
                        };
                        using (SQLiteDataReader readerSens = sensCommand.ExecuteReader())
                        {

                            while (readerSens.Read())
                            {
                                sensguid = readerSens[1].ToString();
                                sensVendorCode = readerSens[3].ToString();
                                SensDescription = readerSens[2].ToString();
                            }
                        }

                    }
                    switch (location)
                    {
                        case "Exhaust":
                            ExtFiltr extFiltr = new ExtFiltr
                            {
                                GUID = compguid
                            };
                            SetProp(extFiltr);
                            break;
                        case "Supply":
                            SupplyFiltr supplyFiltr = new SupplyFiltr
                            {
                                GUID = compguid
                            };
                            SetProp(supplyFiltr);
                            break;
                        default:
                            Filtr filtr = new Filtr
                            {
                                GUID = compguid
                            };
                            
                            SetProp(filtr);


                            break;
                    }
                }
            }
            return posinfosList;
            void SetProp(dynamic filtr)
            {
                if (controltype != string.Empty)
                {
                    pinfo = filtr.GetType().GetProperty("PressureProtect");
                    pinfo.SetValue(filtr, Enum.Parse(pinfo.PropertyType, controltype));
                }
                if (!string.IsNullOrEmpty(senspds))
                {
                    filtr._PressureContol.GUID = sensguid;
                    filtr._PressureContol.Description = SensDescription;
                    IVendoInfo vendoInfo = filtr._PressureContol;
                    SetSensorVendorInfo(vendoInfo, sensVendorCode);
                }
                EditorV2.PosInfo FiltrPos = GetPosInfo(PosInfoGUID, filtr);
                if (filtr!= null)
                {
                    FiltrPos.Tag = filtr;
                    posinfosList.Add(FiltrPos);
                }
            }
        }
        private List<EditorV2.PosInfo>ReadCrossection(string guid, SQLiteCommand command, string WHEREQuery)
        {
            string tableselect = $"SELECT GUID, Position, Image, SensT, SensH, SensP, PosInfoGUID FROM CrossConnection WHERE {WHEREQuery} = '{guid}'";
            command.CommandText = tableselect;

            List<EditorV2.PosInfo> posinfosList = new List<EditorV2.PosInfo>();
            using (SQLiteDataReader reader1 = command.ExecuteReader())
            {
                while (reader1.Read())
                {
                    var crossConnectionGUID = reader1[0].ToString();
                    var image = reader1[2].ToString();
                    var sensT = reader1[3].ToString();
                    var sensH = reader1[4].ToString();
                    var posInfoGUID = reader1[6].ToString();
                    if (string.IsNullOrEmpty(crossConnectionGUID)) continue;
                    CrossSection crossSection = new CrossSection(!string.IsNullOrEmpty(sensT), !string.IsNullOrEmpty(sensH), image)
                    {
                        GUID = crossConnectionGUID
                    };
                        
                    if (!string.IsNullOrEmpty(sensT))
                    {
                        crossSection._SensorT = ReadSensor<SensorT>(sensT, new SQLiteCommand { Connection = connection }, "GUID");
                        crossSection.sensorTType = crossSection._SensorT._SensorType;
                            
                    }
                    if (!string.IsNullOrEmpty(sensH))
                    {
                        //процедура чтения датчика влажности в канале
                        crossSection._SensorH = ReadSensor<Humidifier.HumiditySens>(sensH, new SQLiteCommand { Connection = connection }, "GUID");
                        crossSection.sensorHType = crossSection._SensorH._SensorType;
                        crossSection._SensorH.SensorInsideCrossSection =true;

                    }
                    EditorV2.PosInfo crossectionPos = GetPosInfo(posInfoGUID, crossSection);
                    crossectionPos.Tag = crossSection;
                    posinfosList.Add(crossectionPos);
                }
            }
            return posinfosList;
        }
        private List<EditorV2.PosInfo> ReadRoom(string guid, SQLiteCommand command, string WHEREQuery)
        {
            string tableselect = $"SELECT GUID, Position, Image, SensT, SensH, PosInfoGUID FROM Room WHERE {WHEREQuery} = '{guid}'";
            command.CommandText = tableselect;

            List<EditorV2.PosInfo> posinfosList = new List<EditorV2.PosInfo>();
            using (SQLiteDataReader reader1 = command.ExecuteReader())
            {
                while (reader1.Read())
                {
                    var roomGUID = reader1[0].ToString();
                    var image = reader1[2].ToString();
                    var sensT = reader1[3].ToString();
                    var sensH = reader1[4].ToString();
                    var posInfoGUID = reader1[5].ToString();
                    Room room = new Room(!string.IsNullOrEmpty(sensT), !string.IsNullOrEmpty(sensH), image)
                    {
                        GUID = roomGUID
                    };
                    
                    
                    if (!string.IsNullOrEmpty(sensT))
                    {

                        room._SensorT = ReadSensor<IndoorTemp>(sensT, new SQLiteCommand { Connection = connection }, "GUID");
                        room.sensorTType = room._SensorT._SensorType;
                        
                    }
                    if (!string.IsNullOrEmpty(sensH))
                    {
                        //процедура чтения датчика влажности в канале
                        room._SensorH = ReadSensor<Humidifier.HumiditySens>(sensH, new SQLiteCommand { Connection = connection }, "GUID");
                        room.sensorHType = room._SensorH._SensorType;
                        room._SensorH.SensorInsideCrossSection = false;
                    }
                    EditorV2.PosInfo roomPos = GetPosInfo(posInfoGUID, room);
                    posinfosList.Add(roomPos);
                }
               
            }
            return posinfosList;
        }
        private List<EditorV2.PosInfo> ReadDamper<T>(string guid, SQLiteCommand command, string WHEREQuery)
        {
            string additionalSelect = string.Empty;
            switch (typeof(T).Name)
            {
                case nameof(SupplyDamper):
                    additionalSelect = "AND Location = 'Supply'";
                    break;
                case nameof(ExtDamper):
                    additionalSelect = "AND Location = 'Exhaust'";
                    break;
            }
            string tableselect = $"SELECT SystemGUID, HasControl, Spring, Voltage, GUID, Description, Location, Image, SensT, PosInfoGUID FROM Damper WHERE {WHEREQuery} = '{guid}' {additionalSelect}";
            command.CommandText = tableselect;
            string senstype = string.Empty;
            string sensVendorCode = string.Empty;
            string sensLocation = string.Empty;
            List<EditorV2.PosInfo> posinfosList = new List<EditorV2.PosInfo>();
            using (SQLiteDataReader reader1 = command.ExecuteReader())
            {
                while (reader1.Read())
                {
                    var voltage = reader1[3].ToString();
                    var spring = reader1[2].ToString();
                    var hascontrol = reader1[1].ToString();
                    var compguid = reader1[4].ToString();
                    var description = reader1[5].ToString();
                    var location = reader1[6].ToString();
                    var senstGUID = reader1[8].ToString();
                    var PosInfoGUID = reader1[9].ToString();

                    if (!string.IsNullOrEmpty(senstGUID))
                    {
                        SQLiteCommand sensCommand = new SQLiteCommand
                        {
                            CommandText = sensTQery(senstGUID),
                            Connection = connection
                        };
                        using (SQLiteDataReader readerSens = sensCommand.ExecuteReader())
                        {
                            while (readerSens.Read())
                            {
                                senstype = readerSens[1].ToString();
                                sensVendorCode = readerSens[3].ToString();
                                sensLocation = readerSens[5].ToString();
                            }
                        }
                    }

                    PropertyInfo pinfo;
                    switch (location)
                    {
                        case "Exhaust":
                            ExtDamper extDamper = new ExtDamper
                            {
                                GUID = compguid,
                                Description = description
                            };
                            EditorV2.PosInfo extDamperPos = GetPosInfo(PosInfoGUID, extDamper);
                            pinfo = extDamper.GetType().GetProperty("Voltage");
                            pinfo?.SetValue(extDamper, Enum.Parse(pinfo.PropertyType, voltage));
                            pinfo = extDamper.GetType().GetProperty("HasContol");
                            pinfo?.SetValue(extDamper, Boolean.Parse(hascontrol));
                            pinfo = extDamper.GetType().GetProperty("Spring");
                            pinfo?.SetValue(extDamper, Boolean.Parse(spring));
                            //extDamperPos.Tag = extDamper;
                            posinfosList.Add(extDamperPos);
                            break;
                        case "Supply":
                            SupplyDamper supplyDamper = new SupplyDamper
                            {
                                GUID = compguid,
                                Description = description
                            };
                            if (!string.IsNullOrEmpty(senstGUID))
                            {
                                supplyDamper.SetSensor = true;
                                supplyDamper.outdoorTemp.GUID = senstGUID;
                                supplyDamper.outdoorTemp.Location = sensLocation;
                                pinfo = supplyDamper.GetType().GetProperty("SensorType");
                                pinfo?.SetValue(supplyDamper, Enum.Parse(pinfo.PropertyType, senstype));

                                IVendoInfo vendoInfo = supplyDamper.outdoorTemp;
                                SetSensorVendorInfo(vendoInfo, sensVendorCode);
                            }
                            EditorV2.PosInfo supplyDamperPos= GetPosInfo(PosInfoGUID, supplyDamper);
                            pinfo = supplyDamper.GetType().GetProperty("Voltage");
                            pinfo?.SetValue(supplyDamper, Enum.Parse(pinfo.PropertyType, voltage));
                            pinfo = supplyDamper.GetType().GetProperty("HasContol");
                            pinfo?.SetValue(supplyDamper, bool.Parse(hascontrol));
                            pinfo = supplyDamper.GetType().GetProperty("Spring");
                            pinfo?.SetValue(supplyDamper, bool.Parse(spring));
                            supplyDamperPos.Tag = supplyDamper;
                            posinfosList.Add(supplyDamperPos);
                            break;

                    }
                }
            }
            return posinfosList;

        }
        private List<EditorV2.PosInfo> ReadWaterHeater (string guid, SQLiteCommand command, string WHEREQuery)
        {
            string hastk;
            string sensVendorCode1, sensVendorCode2;
            var valvetype = hastk = sensVendorCode1 = sensVendorCode2 = string.Empty;
            SensorT PS1 = null;
            SensorT PS2 = null;
            WaterHeater.Valve Valve = null;
            WaterHeater.Pump Pump = null;
            List<EditorV2.PosInfo> posinfosList = new List<EditorV2.PosInfo>();
            command.CommandText = "SELECT SensT1, SensT2, Pump, Valve, ControlType, GUID, PosInfoGUID " +
                "FROM WaterHeater " +
                $"WHERE {WHEREQuery} = '{guid}'";
            using (SQLiteDataReader reader1 = command.ExecuteReader())
            {
                while (reader1.Read())
                {
                    var sensT1 = reader1[0].ToString();
                    var sensT2 = reader1[1].ToString();
                    var pump = reader1[2].ToString();
                    var valve = reader1[3].ToString();
                    var protecttype = reader1[4].ToString();
                    var watreheaterguid = reader1[5].ToString();
                    var PosInfoGUID = reader1[6].ToString();

                    WaterHeater waterHeater = new WaterHeater
                    {
                        GUID = watreheaterguid
                    };
                    PropertyInfo pinfoWHP = waterHeater.GetType().GetProperty("Waterprotect");
                    pinfoWHP?.SetValue(waterHeater, Enum.Parse(pinfoWHP.PropertyType, protecttype));

                    if (sensT1 != string.Empty)
                    {
                        SQLiteCommand command1 = new SQLiteCommand
                        {
                            Connection = connection,
                            CommandText = sensTQery(sensT1)
                        };
                        //command.CommandText = sensTQery(sensT1);
                        using (SQLiteDataReader reader2 = command1.ExecuteReader())
                        {
                            while (reader2.Read())
                            {
                                var sens1type = reader2[1].ToString();
                                sensVendorCode1 = reader2[3].ToString();
                                waterHeater.PS1.GUID = reader2[0].ToString();
                                waterHeater.PS1.Location = reader2[5].ToString();
                                waterHeater.PS1.VendorName = reader2[4].ToString();

                                PropertyInfo pinfoS1Type = waterHeater.PS1.GetType().GetProperty("_SensorType");
                                pinfoS1Type?.SetValue(waterHeater.PS1,
                                    Enum.Parse(pinfoS1Type.PropertyType, sens1type));
                            }
                        }
                    }
                    if (sensT2 != string.Empty)
                    {
                        SQLiteCommand command2 = new SQLiteCommand
                        {
                            Connection = connection,
                            CommandText = sensTQery(sensT2)
                        };

                        using (SQLiteDataReader reader3 = command2.ExecuteReader())
                        {
                            while (reader3.Read())
                            {
                                var sens2type = reader3[1].ToString();
                                sensVendorCode2 = reader3[3].ToString();
                                waterHeater.PS2.GUID = reader3[0].ToString();
                                waterHeater.PS2.Location = reader3[5].ToString();
                                waterHeater.PS2.VendorName = reader3[4].ToString();
                                PropertyInfo pinfoS2Type = waterHeater.PS2.GetType().GetProperty("_SensorType");
                                pinfoS2Type?.SetValue(waterHeater.PS2,
                                    Enum.Parse(pinfoS2Type.PropertyType, sens2type));
                            }
                        }
                    }
                    if (valve != string.Empty)
                    {
                        SQLiteCommand command3 = new SQLiteCommand
                        { 
                            Connection = connection,
                            CommandText = "SELECT Valve.GUID, ValveType " +
                             "FROM Valve " +
                             $"WHERE GUID = '{valve}';"
                        };
                        using (SQLiteDataReader reader4 = command3.ExecuteReader())
                        {
                            while (reader4.Read())
                            {
                                valvetype = reader4[1].ToString();
                                Valve = new WaterHeater.Valve
                                {
                                    GUID = reader4[0].ToString()
                                };
                            }
                        }
                    }
                    if (pump != string.Empty)
                    {
                        SQLiteCommand command4 = new SQLiteCommand
                        {
                            Connection = connection,
                            CommandText = "SELECT Pump.GUID, HasTK " +
                             "FROM Pump " +
                              $"WHERE GUID = '{pump}';"
                        };
                        using (SQLiteDataReader reader5 = command4.ExecuteReader())
                        {
                            while (reader5.Read())
                            {
                                hastk = reader5[1].ToString();
                                Pump = new WaterHeater.Pump
                                {
                                    GUID = reader5[0].ToString()
                                };
                            }
                        }
                    }
                    PropertyInfo pinfoHasTK = waterHeater.GetType().GetProperty("HasTK");
                    pinfoHasTK?.SetValue(waterHeater, bool.Parse(hastk));
                    PropertyInfo pinfoValveType = waterHeater.GetType().GetProperty("_valveType");
                    pinfoValveType?.SetValue(waterHeater, Enum.Parse(pinfoValveType.PropertyType, valvetype));
                    if (PS1 != null)
                    {
                        waterHeater.PS1GUID = PS1.GUID;
                        IVendoInfo vendoInfo = waterHeater.PS1;
                        vendoInfo.SetVendorInfo(null, sensVendorCode1, null, null, null);
                        SetSensorVendorInfo(vendoInfo, sensVendorCode1);
                    }
                    if (PS2 != null)
                    {
                        waterHeater.PS2GUID = PS2.GUID;
                        IVendoInfo vendoInfo = waterHeater.PS2;
                        vendoInfo.SetVendorInfo(null, sensVendorCode2, null, null, null);
                        SetSensorVendorInfo(vendoInfo, sensVendorCode2);
                    }
                    waterHeater.ValveGUID = Valve?.GUID;
                    waterHeater.PumpGUID = Pump?.GUID;
                    EditorV2.PosInfo waterHeaterPos = GetPosInfo(PosInfoGUID, waterHeater);
                    posinfosList.Add(waterHeaterPos);
                }
            }
            return posinfosList;
        }
        private List<EditorV2.PosInfo> ReadFroster(string guid, SQLiteCommand command, string WHEREQuery)
        {
            string valvetype;
            string stairs;
            var KKBcontroltype = valvetype = stairs = string.Empty;
            List<EditorV2.PosInfo> posinfosList = new List<EditorV2.PosInfo>();
            
            command.CommandText = "SELECT SensT1, SensT2, KKB, Valve, ControlType, Power, Voltage, GUID, FrosterSensor, PosInfoGUID " +
                "FROM Froster " +
                $"WHERE {WHEREQuery} = '{guid}'; ";
            using (SQLiteDataReader reader1 = command.ExecuteReader())
            {
                while (reader1.Read())
                {
                    var sensT1 = reader1[0].ToString();
                    var sensT2 = reader1[1].ToString();
                    var kkb = reader1[2].ToString();
                    var valve = reader1[3].ToString();
                    var frostercontroltype = reader1[4].ToString();
                    var power = reader1[5].ToString();
                    var voltage = reader1[6].ToString();
                    var frosterguid = reader1[7].ToString();
                    var PosInfoGUID = reader1[9].ToString();
                    const bool minrequest = true;
                    Froster froster = new Froster(minrequest)
                    {
                        Power = power,
                        GUID = frosterguid
                    };
                    Enum.TryParse(voltage, out ElectroDevice._Voltage Voltage);
                    froster.Voltage = Voltage;
                    Enum.TryParse(frostercontroltype, out Froster.FrosterType frosterType);
                    froster._FrosterType = frosterType;
                    if (sensT1 != string.Empty)
                    {
                        SQLiteCommand sQLiteCommand = new SQLiteCommand
                        {
                            CommandText = sensTQery(sensT1),
                            Connection = connection
                        };

                        using (SQLiteDataReader reader2 = sQLiteCommand.ExecuteReader())
                        {
                            while (reader2.Read())
                            {
                                string sensarticle = reader2[3].ToString();
                                string senlocation = reader2[5].ToString();
                                if (froster.Sens1 == null) continue;
                                froster.Sens1.GUID = reader2[0].ToString();
                                froster.Sens1.Location = senlocation;
                                IVendoInfo vendoInfo = froster.Sens1;
                                if (!string.IsNullOrEmpty(sensarticle))
                                {
                                    vendoInfo.SetVendorInfo(null, sensarticle, null, null, null);

                                }
                                SetSensorVendorInfo(vendoInfo, sensarticle);
                            }
                        }
                        
                        //PropertyInfo pinfoS1T = PS1.GetType().GetProperty("_SensorType");
                        //pinfoS1T.SetValue(PS1, Enum.Parse(pinfoS1T.PropertyType, sens1type));
                    }
                    if (sensT2 != string.Empty)
                    {
                        SQLiteCommand sQLiteCommand = new SQLiteCommand
                        {
                            CommandText = sensTQery(sensT2),
                            Connection = connection
                        };
                        using (SQLiteDataReader reader3 = sQLiteCommand.ExecuteReader())
                        {
                            while (reader3.Read())
                            {
                                string sensarticle = reader3[3].ToString();
                                string senslocation = reader3[5].ToString();
                                if (froster.Sens2 == null) continue;
                                froster.Sens2.GUID = reader3[0].ToString();
                                froster.Sens2.Location = senslocation;
                                IVendoInfo vendoInfo = froster.Sens2;
                                if (!string.IsNullOrEmpty(sensarticle))
                                {

                                    vendoInfo.SetVendorInfo(null, sensarticle, null, null, null);

                                }
                                SetSensorVendorInfo(vendoInfo, sensarticle);
                            }
                        }
                        
                    }
                    if (valve != string.Empty)
                    {
                        SQLiteCommand sQLiteCommand = new SQLiteCommand
                        {
                            Connection = connection,
                            CommandText = "SELECT Valve.GUID, ValveType " +
                             "FROM Valve " +
                             $"WHERE GUID = '{valve}';"
                        };
                        string valveguid = string.Empty;
                        using (SQLiteDataReader reader4 = sQLiteCommand.ExecuteReader())
                        {
                            
                            while (reader4.Read())
                            {
                                valvetype = reader4[1].ToString();
                                valveguid = reader4[0].ToString();
                            }
                        }
                        if (froster._Valve != null)
                        {
                            froster._Valve.GUID = valveguid;
                        }

                        //Valve = new Froster.Valve
                        //{
                        //    GUID = valveguid
                        //};
                        if (froster._Valve != null)
                        {
                            PropertyInfo pinfoValveType = froster._Valve.GetType().GetProperty("_ValveType");
                            pinfoValveType?.SetValue(froster._Valve,
                                Enum.Parse(pinfoValveType.PropertyType, valvetype));
                        }
                    }
                    if (kkb != string.Empty)
                    {
                        SQLiteCommand kkbCommand = new SQLiteCommand
                        {
                            Connection = connection,
                            CommandText = "SELECT KKB.GUID, KKBType, Stairs " +
                             "FROM KKB " +
                             $"WHERE GUID = '{kkb}';"
                        };

                        using (SQLiteDataReader reader5 = kkbCommand.ExecuteReader())
                        {
                            while (reader5.Read())
                            {
                                //KKB = new Froster.KKB();
                                KKBcontroltype = reader5[1].ToString();
                                froster._KKB.GUID = reader5[0].ToString();
                                stairs = reader5[2].ToString();
                            }
                        }
                        
                        Enum.TryParse(KKBcontroltype, out Froster.KKB.KKBControlType KKBControltype);
                        froster.KKBControlType = KKBControltype;
                        PropertyInfo pinfoKKBSt = froster.GetType().GetProperty("Stairs");
                        pinfoKKBSt?.SetValue(froster, Enum.Parse(pinfoKKBSt.PropertyType, stairs));
                    }
                    EditorV2.PosInfo frosterPos = GetPosInfo(PosInfoGUID, froster);
                    posinfosList.Add(frosterPos);
                }
            }
            return posinfosList;
        }
        private List<EditorV2.PosInfo> ReadElectroHeater(string guid, SQLiteCommand command, string WHEREQuery)
        {
            List<EditorV2.PosInfo> posinfosList = new List<EditorV2.PosInfo>();
            string tableselect1 = "SELECT Stairs, Voltage, Power, ElectroHeater.GUID, PosInfoGUID " +
                "FROM ElectroHeater " +
                $"WHERE {WHEREQuery} = '{guid}'; ";
            command.CommandText = tableselect1;
            SQLiteDataReader reader1 = command.ExecuteReader();
            while (reader1.Read())
            {
                ElectroHeater electroHeater = new ElectroHeater();
                PropertyInfo pinfo = electroHeater.GetType().GetProperty("Voltage");
                pinfo.SetValue(electroHeater, Enum.Parse(pinfo.PropertyType, reader1[1].ToString()));
                                pinfo = electroHeater.GetType().GetProperty("Stairs");
                pinfo.SetValue(electroHeater, Enum.Parse(pinfo.PropertyType, reader1[0].ToString()));
                electroHeater.Power = reader1[2].ToString();
                electroHeater.GUID = reader1[3].ToString();
                string PosInfoGUID = reader1[4].ToString();
                EditorV2.PosInfo electroHeaterPos = GetPosInfo(PosInfoGUID, electroHeater);
                posinfosList.Add(electroHeaterPos);
            }
            reader1.Close();
            return posinfosList;
        }
        private List<EditorV2.PosInfo> ReadHumidifier (string guid, SQLiteCommand command, string WHEREQuery)
        {
            
            List<EditorV2.PosInfo> posinfosList = new List<EditorV2.PosInfo>();

            string tableselect1 = "SELECT Type, Voltage, Power, HumSensPresent, Humidifier.GUID, SensHum, PosInfoGUID " +
                                  "FROM Humidifier " +
                                  $"WHERE {WHEREQuery} = '{guid}'; ";
            command.CommandText = tableselect1;
            SQLiteDataReader reader1 = command.ExecuteReader();
            while (reader1.Read())
            {
                Humidifier humidifier = new Humidifier();
                var pinfo = humidifier.GetType().GetProperty("HumType");
                pinfo?.SetValue(humidifier, Enum.Parse(pinfo.PropertyType, reader1[0].ToString()));
                pinfo = humidifier.GetType().GetProperty("Voltage");
                pinfo?.SetValue(humidifier, Enum.Parse(pinfo.PropertyType, reader1[1].ToString()));
                humidifier.Power = reader1[2].ToString();
                pinfo = humidifier.GetType().GetProperty("HumSensPresent");
                pinfo?.SetValue(humidifier, Boolean.Parse(reader1[3].ToString()));
                humidifier.GUID = reader1[4].ToString();
                var senshumguid = reader1[5].ToString();
                var PosInfoGUID = reader1[6].ToString();
                if (humidifier.HumSensPresent)
                {
                    SQLiteCommand command2 = new SQLiteCommand
                    {
                        Connection = connection,
                        CommandText = "SELECT SensType, VendorCode " +
                                      "FROM SensHum " +
                                      $"WHERE SensHum.GUID = '{senshumguid}'"
                    };
                    using (SQLiteDataReader reader2 = command2.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            pinfo = humidifier.GetType().GetProperty("SensorType");
                            pinfo?.SetValue(humidifier, Enum.Parse(pinfo.PropertyType, reader2[0].ToString()));
                            string sensarticle = reader2[1].ToString();
                            IVendoInfo vendoInfo = humidifier.HumiditySensor;
                            if (!string.IsNullOrEmpty(sensarticle))
                            {
                                vendoInfo.SetVendorInfo(null, sensarticle, null, null, null);

                            }
                            SetSensorVendorInfo(vendoInfo, sensarticle);

                        }

                    }
                    humidifier.HumiditySensor.GUID = senshumguid;

                }
                EditorV2.PosInfo humidifierPos = GetPosInfo(PosInfoGUID, humidifier);
                posinfosList.Add(humidifierPos);
            }
            reader1.Close();
            return posinfosList;
        }
        private List<EditorV2.PosInfo> ReadRecuperator (string guid, SQLiteCommand command, string WHEREQuery)
        {
            
            List<EditorV2.PosInfo> posInfos = new List<EditorV2.PosInfo>();

            command.CommandText = "SELECT Type, PS1, PS2, Drive1, Drive2, Drive3, PosInfoGUID, Recuperator.GUID " +
                                  "FROM Recuperator " +
                                  $"WHERE {WHEREQuery} = '{guid}'; ";
            SQLiteDataReader reader1 = command.ExecuteReader();
            while (reader1.Read())
            {
                Recuperator recuperator = new Recuperator();
                SensorT PS1 = recuperator.protectSensor1;
                PressureContol PS2 = recuperator.protectSensor2;
                Recuperator.Drive Drive1 = recuperator.Drive1;
                Recuperator.Drive Drive2 = recuperator.Drive2;
                Recuperator.Drive Drive3 = recuperator.Drive3;
                PropertyInfo pinfo = recuperator.GetType().GetProperty("_RecuperatorType");
                pinfo.SetValue(recuperator, Enum.Parse(pinfo.PropertyType, reader1[0].ToString()));
                var PS1guid = reader1[1].ToString();
                var PS2guid = reader1[2].ToString();
                var Drive1Guid = reader1[3].ToString();
                var Drive2Guid = reader1[4].ToString();
                var Drive3Guid = reader1[5].ToString();
                var PosInfoGUID = reader1[6].ToString();
                recuperator.GUID = reader1[7].ToString();
                if (Drive1 != null && Drive1Guid != string.Empty)
                {
                    Drive1.GUID = Drive1Guid;
                    recuperator.Drive1.GUID = Drive1Guid;
                }
                if (Drive2 != null && Drive2Guid != string.Empty)
                {
                    Drive2.GUID = Drive2Guid;
                    recuperator.Drive2.GUID = Drive2Guid;
                }
                if (Drive3 != null && Drive3Guid != string.Empty)
                {
                    Drive3.GUID = Drive3Guid;
                    recuperator.Drive3.GUID = Drive3Guid;
                }
                if (PS1 != null && PS1guid != string.Empty)
                {
                    string sensarticle = string.Empty;
                    SQLiteCommand PS1Command = new SQLiteCommand
                    {
                        Connection = connection,
                        CommandText = sensTQery(PS1guid)// $"SELECT SensType FROM SensT WHERE GUID = '{PS1guid}'";
                    };

                    using (SQLiteDataReader sensTreader = PS1Command.ExecuteReader())
                    {
                        while (sensTreader.Read())
                        {
                            string senstype = sensTreader[1].ToString();
                            string senslocation = sensTreader[5].ToString();
                            sensarticle = sensTreader[3].ToString();
                            if (string.IsNullOrEmpty(senstype)) continue;
                            PropertyInfo propertyInfoSensT = recuperator.protectSensor1.GetType().GetProperty("_SensorType");
                            propertyInfoSensT.SetValue(recuperator.protectSensor1, Enum.Parse(propertyInfoSensT.PropertyType, senstype));
                            recuperator.protectSensor1.Location = senslocation;
                        }
                    }
                    PS1.GUID = PS1guid;
                    recuperator.protectSensor1.GUID = PS1guid;
                    IVendoInfo vendoInfo = recuperator.protectSensor1;
                    if (!string.IsNullOrEmpty(sensarticle))
                    {
                        vendoInfo.SetVendorInfo(null, sensarticle, null, null, null);

                    }
                    SetSensorVendorInfo(vendoInfo, sensarticle);

                }
                if (PS2 != null && PS2guid != string.Empty)
                {
                    SQLiteCommand PS2command = new SQLiteCommand
                    {
                        Connection = connection,
                        CommandText = "SELECT SensType, GUID, Description, VendorCode, Vendor " +
                        "FROM SensPDS " +
                        $"WHERE SensPDS.GUID = '{PS2guid}'"
                    };
                    string sensarticle = string.Empty;

                    using (SQLiteDataReader sensPreader = PS2command.ExecuteReader())
                    {
                        while (sensPreader.Read())
                        {
                            string senstype = sensPreader[0].ToString();
                            sensarticle = sensPreader[3].ToString();
                            if (!string.IsNullOrEmpty(senstype))
                            {
                                PropertyInfo propertyInfoSensP = recuperator.protectSensor2.GetType().GetProperty("_SensorType");
                                propertyInfoSensP.SetValue(recuperator.protectSensor2, Enum.Parse(propertyInfoSensP.PropertyType, senstype));

                            }
                        }
                    }

                    

                    PS2.GUID = PS2guid;
                    recuperator.protectSensor2.GUID = PS2guid;
                    IVendoInfo vendoInfo = recuperator.protectSensor2;
                    if (!string.IsNullOrEmpty(sensarticle))
                    {
                        vendoInfo.SetVendorInfo(null, sensarticle, null, null, null);
                    }
                    SetSensorVendorInfo(vendoInfo, sensarticle);
                }
                EditorV2.PosInfo recuperatorPos = GetPosInfo(PosInfoGUID, recuperator);
                posInfos.Add(recuperatorPos);
            }
            reader1.Close();
            return posInfos;
        }
        private dynamic ReadSensor<T>(string guid, SQLiteCommand command, string WHEREQuery)
        {
            
            string sensguid, sensarticle, senslocation, tableselect2, sendescription;
            var sensType = sensguid = sensarticle = senslocation = tableselect2 = sendescription = string.Empty;
            
            switch(typeof(T).Name)
            {
                case nameof(SupplyTemp):
                case nameof(ExhaustTemp):
                case nameof(IndoorTemp):
                case nameof(OutdoorTemp):
                case nameof(SensorT):
                    tableselect2 = "SELECT SensT.GUID, SensType, Description, VendorCode, Location " +
                         "FROM SensT " +
                          $"WHERE {WHEREQuery} = '{guid}';";
                    break;
                case nameof(Humidifier.HumiditySens):
                    tableselect2 = "SELECT SensHum.GUID, SensType, Description, VendorCode, Location " +
                         "FROM SensHum " +
                          $"WHERE {WHEREQuery} = '{guid}';";
                    break;
            }
            
            command.CommandText = tableselect2;

            SQLiteDataReader reader1 = command.ExecuteReader();
            while (reader1.Read())
            {
                sensType = reader1[1].ToString();
                sensguid = reader1[0].ToString();
                sensarticle = reader1[3].ToString();
                senslocation = reader1[4].ToString();
                sendescription = reader1[2].ToString();

            }
            reader1.Close();

            IVendoInfo vendoInfo;
            switch (typeof(T).Name)
            {
                case nameof(SupplyTemp):
                case nameof(ExhaustTemp):
                case nameof(OutdoorTemp):
                case nameof(IndoorTemp):
                case nameof(SensorT):
                    switch (senslocation)
                    {
                        case "Exhaust":
                            ExhaustTemp exhaustTemp = new ExhaustTemp
                            {
                                GUID = sensguid,
                                Location = senslocation,
                                Description = sendescription
                            };
                            PropertyInfo pinfoExt = exhaustTemp.GetType().GetProperty("_SensorType");
                            pinfoExt?.SetValue(exhaustTemp, Enum.Parse(pinfoExt.PropertyType, sensType));
                            if (string.IsNullOrEmpty(sensarticle)) return exhaustTemp;
                            vendoInfo = exhaustTemp;
                            vendoInfo.SetVendorInfo(null, sensarticle, null, null, null);
                            SetSensorVendorInfo(vendoInfo, sensarticle);

                            return exhaustTemp;

                        case "Supply":
                            SupplyTemp supplyTemp = new SupplyTemp
                            {
                                GUID = sensguid,
                                Location = senslocation,
                                Description = sendescription
                            };

                            PropertyInfo pinfoSupply = supplyTemp.GetType().GetProperty("_SensorType");
                            pinfoSupply?.SetValue(supplyTemp, Enum.Parse(pinfoSupply.PropertyType, sensType));
                            if (string.IsNullOrEmpty(sensarticle)) return supplyTemp;
                            vendoInfo = supplyTemp;
                            vendoInfo.SetVendorInfo(null, sensarticle, null, null, null);
                            SetSensorVendorInfo(vendoInfo, sensarticle);
                            return supplyTemp;
                        case "Outdoor":
                            OutdoorTemp outdoorTemp = new OutdoorTemp
                            {
                                GUID = sensguid,
                                Location = senslocation,
                                Description = sendescription
                            };
                            PropertyInfo pinfo = outdoorTemp.GetType().GetProperty("_SensorType");
                            pinfo?.SetValue(outdoorTemp, Enum.Parse(pinfo.PropertyType, sensType));
                            if (string.IsNullOrEmpty(sensarticle)) return outdoorTemp;
                            vendoInfo = outdoorTemp;
                            vendoInfo.SetVendorInfo(null, sensarticle, null, null, null);
                            SetSensorVendorInfo(vendoInfo, sensarticle);
                            return outdoorTemp;
                        case "Indoor":
                            IndoorTemp indoorTemp = new IndoorTemp
                            {
                                GUID = sensguid,
                                Location = senslocation,
                                Description = sendescription
                            };
                            PropertyInfo pinfoIndoor = indoorTemp.GetType().GetProperty("_SensorType");
                            pinfoIndoor?.SetValue(indoorTemp, Enum.Parse(pinfoIndoor.PropertyType, sensType));
                            if (string.IsNullOrEmpty(sensarticle)) return indoorTemp;
                            vendoInfo = indoorTemp;
                            vendoInfo.SetVendorInfo(null, sensarticle, null, null, null);
                            SetSensorVendorInfo(vendoInfo, sensarticle);
                            return indoorTemp;
                        default:
                            SensorT sensorT = new SensorT
                            {
                                GUID = sensguid,
                                Location = senslocation,
                                Description = sendescription
                            };
                            PropertyInfo pinfoSensT = sensorT.GetType().GetProperty("_SensorType");
                            pinfoSensT?.SetValue(sensorT, Enum.Parse(pinfoSensT.PropertyType, sensType));
                            if (string.IsNullOrEmpty(sensarticle)) return sensorT;
                            vendoInfo = sensorT;
                            vendoInfo.SetVendorInfo(null, sensarticle, null, null, null);
                            SetSensorVendorInfo(vendoInfo, sensarticle);
                            return sensorT;
                            


                    }
                case nameof(Humidifier.HumiditySens):
                    Humidifier.HumiditySens humiditySens = new Humidifier.HumiditySens
                    {
                        GUID = sensguid,
                        Location = senslocation,
                        Description = sendescription
                    };
                    PropertyInfo pinfoHumSens = humiditySens.GetType().GetProperty("_SensorType");
                    pinfoHumSens?.SetValue(humiditySens, Enum.Parse(pinfoHumSens.PropertyType, sensType));
                    if (string.IsNullOrEmpty(sensarticle)) return humiditySens;
                    vendoInfo = humiditySens;
                    vendoInfo.SetVendorInfo(null, sensarticle, null, null, null);
                    SetSensorVendorInfo(vendoInfo, sensarticle);
                    return humiditySens;
            }

            if (typeof(T) == typeof(SupplyTemp))
            {
                SupplyTemp supplyTemp = new SupplyTemp
                {
                    GUID = sensguid,
                    Location = senslocation
                };
                
                PropertyInfo pinfo = supplyTemp.GetType().GetProperty("_SensorType");
                pinfo?.SetValue(supplyTemp, Enum.Parse(pinfo.PropertyType, sensType));
                if (string.IsNullOrEmpty(sensarticle)) return supplyTemp;
                vendoInfo = supplyTemp;
                vendoInfo.SetVendorInfo(null, sensarticle, null, null, null);
                SetSensorVendorInfo(vendoInfo, sensarticle);
                return supplyTemp;
            }
            if (typeof(T) == typeof(ExhaustTemp))
            {
                ExhaustTemp exhaustTemp = new ExhaustTemp
                {
                    GUID = sensguid,
                    Location = senslocation
                };
                PropertyInfo pinfo = exhaustTemp.GetType().GetProperty("_SensorType");
                pinfo?.SetValue(exhaustTemp, Enum.Parse(pinfo.PropertyType, sensType));
                if (string.IsNullOrEmpty(sensarticle)) return exhaustTemp;
                vendoInfo = exhaustTemp;
                vendoInfo.SetVendorInfo(null, sensarticle, null, null, null);
                SetSensorVendorInfo(vendoInfo, sensarticle);
                return exhaustTemp;
            }
            if (typeof(T) == typeof(OutdoorTemp))
            {
                OutdoorTemp outdoorTemp = new OutdoorTemp
                {
                    GUID = sensguid,
                    Location = senslocation
                };
                PropertyInfo pinfo = outdoorTemp.GetType().GetProperty("_SensorType");
                pinfo?.SetValue(outdoorTemp, Enum.Parse(pinfo.PropertyType, sensType));
                if (string.IsNullOrEmpty(sensarticle)) return outdoorTemp;
                vendoInfo = outdoorTemp;
                vendoInfo.SetVendorInfo(null, sensarticle, null, null, null);
                SetSensorVendorInfo(vendoInfo, sensarticle);
                return outdoorTemp;
            }
            if (typeof(T) == typeof(IndoorTemp))
            {
                IndoorTemp indoorTemp = new IndoorTemp
                {
                    GUID = sensguid,
                    Location = senslocation
                };
                PropertyInfo pinfo = indoorTemp.GetType().GetProperty("_SensorType");
                pinfo?.SetValue(indoorTemp, Enum.Parse(pinfo.PropertyType, sensType));
                if (string.IsNullOrEmpty(sensarticle)) return indoorTemp;
                vendoInfo = indoorTemp;
                vendoInfo.SetVendorInfo(null, sensarticle, null, null, null);
                SetSensorVendorInfo(vendoInfo, sensarticle);
                return indoorTemp;
            }
            if (typeof(T) != typeof(SensorT)) return null;
            {
                SensorT sensorT = new SensorT
                {
                    GUID = sensguid,
                    Location = senslocation,
                    Description = sendescription
                };
                PropertyInfo pinfo = sensorT.GetType().GetProperty("_SensorType");
                pinfo?.SetValue(sensorT, Enum.Parse(pinfo.PropertyType, sensType));
                if (string.IsNullOrEmpty(sensarticle)) return sensorT;
                vendoInfo = sensorT;
                vendoInfo.SetVendorInfo(null, sensarticle, null, null, null);
                SetSensorVendorInfo(vendoInfo, sensarticle);
                return sensorT;
            }
        }

        private static string sensTQery(string sensGUID)
        {
            return "SELECT SensT.GUID, SensType, Description, VendorCode, Vendor, Location " +
                 "FROM SensT " +
                  $"WHERE SensT.GUID = '{sensGUID}';";
        }
        private static string sensPQery (string sensGUID)
        {
            return "SELECT SensType, GUID, Description, VendorCode, Vendor " +
                "FROM SensPDS " +
                $"WHERE [GUID] = '{sensGUID}'";
            

        }

        private void radGridView1_CellClick(object sender, GridViewCellEventArgs e)
        {
            radPropertyGrid1.SelectedObject = e.Row.Tag;
            object component = e.Row.Tag;
            if (component != null)
            {
                VendorFill.FillComponentColumns(radGridView4);
                switch (component.GetType().Name)
                {
                    case nameof(Froster):
                        if (radPropertyGrid1.SelectedObject is Froster froster)
                        {
                            var frosterType = froster._FrosterType;
                            switch (frosterType)
                            {
                                case Froster.FrosterType.Freon:
                                    radPropertyGrid1.PropertyGridElement.PropertyTableElement.BeginUpdate();
                                    radPropertyGrid1.Items["Stairs"].Visible = true;
                                    radPropertyGrid1.Items["KKBControlType"].Visible = true;
                                    radPropertyGrid1.Items["valveType"].Visible = false;
                                    radPropertyGrid1.PropertyGridElement.PropertyTableElement.EndUpdate();
                                    break;
                                case Froster.FrosterType.Water:
                                    radPropertyGrid1.PropertyGridElement.PropertyTableElement.BeginUpdate();
                                    radPropertyGrid1.Items["Stairs"].Visible = false;
                                    radPropertyGrid1.Items["KKBControlType"].Visible = false;
                                    radPropertyGrid1.Items["valveType"].Visible = true;
                                    radPropertyGrid1.PropertyGridElement.PropertyTableElement.EndUpdate();
                                    break;
                            }
                            FillSensCompRows(component);
                        }
                        break;
                    case nameof(SupplyVent):
                    case nameof(ExtVent):
                    case nameof(SupplyFiltr):
                    case nameof(ExtFiltr):
                    case nameof(WaterHeater):
                    case nameof(Humidifier):
                    case nameof(SupplyTemp):
                    case nameof(OutdoorTemp):
                    case nameof(ExhaustTemp):
                    case nameof(IndoorTemp):
                    case nameof(Recuperator):
                    case nameof(SensorT):
                    case nameof(Filtr):
                    case nameof(SpareSuplyVent):
                    case nameof(SpareExtVent):
                        FillSensCompRows(component);
                        break;
                    case nameof(Humidifier.HumiditySens):
                        var (_, ID, VendorDescription, _, _, _, _) = ((IVendoInfo)component).GetVendorInfo();
                        Humidifier.HumiditySens humiditySens = (Humidifier.HumiditySens)component;
                        GridViewDataRowInfo rowInfo = new GridViewDataRowInfo(radGridView4.MasterView);
                        rowInfo.Cells[0].Value = 1;
                        rowInfo.Cells["Тип датчика"].Value = humiditySens.Description;
                        rowInfo.Cells["Описание"].Value = VendorDescription;
                        rowInfo.Cells["Артикул"].Value = ID;
                        radGridView4.Rows.Add(rowInfo);
                        rowInfo.Tag = humiditySens;
                        break;
                        

                }
                void FillSensCompRows(dynamic ventcomp)
                {
                    IGetSensors sensors = ventcomp;
                    List<dynamic> list = sensors.GetSensors()
                        .Where(comp => comp != null).ToList();
                    //list.Add(new SupplyVent());
                    

                    int cnt = 1;
                    
                    foreach (var sens in list)
                    {
                        var (_, ID, VendorDescription, _, _, DefaultDescription, MainDBTable) = ((IVendoInfo)sens).GetVendorInfo();
                        switch (sens.GetType().Name)
                        {
                            case nameof(PressureContol):
                                PressureContol pressureContol = (PressureContol)sens;
                                GridViewDataRowInfo row1 = CreateRow(pressureContol.Description, VendorDescription, ID);
                                row1.Tag = pressureContol;
                                break;
                            case nameof(SensorT):
                            case nameof(SupplyTemp):
                            case nameof(OutdoorTemp):
                            case nameof(ExhaustTemp):
                            case nameof(IndoorTemp):
                                SensorT sensorT = (SensorT)sens;
                                
                                GridViewDataRowInfo row2 = CreateRow(sensorT.Description, VendorDescription, ID);
                                row2.Tag = sensorT;
                                break;
                            case (nameof(Humidifier.HumiditySens)):
                                Humidifier.HumiditySens humiditySens = (Humidifier.HumiditySens)sens;
                                GridViewDataRowInfo row3 = CreateRow(humiditySens.Description, VendorDescription, ID);
                                row3.Tag = humiditySens;
                                break;
                            case (nameof(Vent.FControl)):
                                Vent.FControl fcontrol = (Vent.FControl)sens;
                                GridViewDataRowInfo row4 = CreateRow(fcontrol.Description, VendorDescription, ID);
                                row4.Tag = fcontrol;
                                break;
                        }
                        cnt++;



                    }
                    GridViewDataRowInfo CreateRow(string s1, string s2, string s3)
                    {
                        GridViewDataRowInfo rowInfo = new GridViewDataRowInfo(radGridView4.MasterView);
                        rowInfo.Cells[0].Value = cnt;
                        rowInfo.Cells["Тип датчика"].Value = s1;
                        rowInfo.Cells["Описание"].Value = s2;
                        rowInfo.Cells["Артикул"].Value = s3;
                        radGridView4.Rows.Add(rowInfo);
                        return rowInfo;
                        
                    }
                }
                
            }
            radPropertyGrid1.Items.AsParallel().ForAll(item =>
            {
                item.Enabled = false;
            });
            
        }
        private void radTreeView2_Click(object sender, EventArgs e)
        {
            if (radTreeView2.SelectedNode == null) return;
            try
            {
                if (radTreeView2.SelectedNode.Tag is VentSystem ventSystem)
                {

                    radPropertyGrid1.SelectedObject = ventSystem;

                }
            }
            catch
            {
                // ignored
            }
        }
        private void radTreeView2_ValueChanged(object sender, TreeNodeValueChangedEventArgs e)
        {
           
            RadTreeNode editednode = e.Node;
            if (editednode != null)
            {
                #region variables
                string newname = editednode.Text;
                string vensystemguid = editednode.Name;
                #endregion
                #region Set new name to ventsystem
                if (editednode.Tag is VentSystem ventSystem)
                {
                    ventSystem.SystemName = newname;
                    radPropertyGrid1.SelectedObject = ventSystem;
                    
                }
                #endregion
                #region Update DataBase with new name
                
                try
                {
                    if (connection.State == ConnectionState.Open)
                    {
                         SQLiteCommand command = new  SQLiteCommand() //создание команды на зпись в БД
                        {
                            Connection = connection //с параметром соединения
                        };
                        UpdateBDTable(ref command, "Ventsystems", "SystemName", newname, "GUID", vensystemguid);
                        UpdateBDTable(ref command, "Ventilator", "SystemName", newname, "SystemGUID", vensystemguid);
                        UpdateBDTable(ref command, "Filter", "SystemName", newname, "SystemGUID", vensystemguid);
                        UpdateBDTable(ref command, "Damper", "SystemName", newname, "SystemGUID", vensystemguid);
                        UpdateBDTable(ref command, "WaterHeater", "SystemName", newname, "SystemGUID", vensystemguid);
                        UpdateBDTable(ref command, "ElectroHeater", "SystemName", newname, "SystemGUID", vensystemguid);
                        UpdateBDTable(ref command, "Froster", "SystemName", newname, "SystemGUID", vensystemguid);
                        UpdateBDTable(ref command, "Humidifier", "SystemName", newname, "SystemGUID", vensystemguid);
                        UpdateBDTable(ref command, "Recuperator", "SystemName", newname, "SystemGUID", vensystemguid);
                        UpdateBDTable(ref command, "SensPDS", "SystemName", newname, "SystemGUID", vensystemguid);
                        UpdateBDTable(ref command, "SensT", "SystemName", newname, "SystemGUID", vensystemguid);
                        UpdateBDTable(ref command, "Valve", "SystemName", newname, "SystemGUID", vensystemguid);
                        UpdateBDTable(ref command, "Pump", "SystemName", newname, "SystemGUID", vensystemguid);
                        UpdateBDTable(ref command, "KKB", "SystemName", newname, "SystemGUID", vensystemguid);
                        UpdateBDTable(ref command, "Cable", "SystemName", newname, "SystemGUID", vensystemguid);
                        UpdateBDTable(ref command, "Pannel", "VentSystemName", newname, "SystemGUID", vensystemguid);
                        command.Dispose();
                    }
                }
                catch 
                {
                   MessageBox.Show(@"Что-то пошло не так");
                }
                finally
                {
                    radTreeView2.AllowEdit = false;
                    radTreeView2.EndEdit();
                    editednode.EndEdit();
                   
                }
                #endregion
                //MessageBox.Show($"newname: {newname}; ventsystemguid: {vensystemguid} \nCode For Update Ventsystem Name");
            }
            void UpdateBDTable(ref  SQLiteCommand  SQLiteCommand, string tablename, string column, string newname, string where, string guid)
            {
                string Query = $"UPDATE {tablename} SET {column}= '{newname}' WHERE {where}= '{guid}'";
                 SQLiteCommand.CommandText = Query;
                 SQLiteCommand.ExecuteNonQuery();
            }
        }
        private void MakeVentSystemCopy(VentSystem ventSystemhost)
        {
            try
            {
                #region Get Ventsystem Name and generate ventsystem GUID
                var ventSystem = ventSystemhost;
                string newvensystemname = ventSystem.SystemName + "(Copy)";
                var newguid = Guid.NewGuid().ToString(); //create ventsystem GUID
                ventSystem.SystemName = newvensystemname;
                ventSystem.GUID = newguid;
                #endregion
                #region Get Project
                RadTreeNode radTreeNode = radTreeView1.SelectedNode;
                if (!(radTreeNode.Tag is Building building)) return;
                string buildGUID = building.BuildGUID;
                if (!(radTreeNode.Parent.Tag is Project project)) return;
                string projectGUID = project.GetGUID();

                #endregion
                #region Open DataBase
                #endregion
                #region Create Command variable

                if (connection.State != ConnectionState.Open) return;
                SQLiteCommand command = new  SQLiteCommand() //создание команды на зпись в БД
                {
                    Connection = connection //с параметром соединения
                };
                #endregion
                #region Write Ventsystem (host) to DataBase

                string UpdateQuery = string.Empty; // переменная для обновления вентсистемы в БД
                var date = DateTime.Now.ToString("dd-MM-yyyy");
                const string vesrsion = "1";
                string InsertQueryVensystem = "INSERT INTO Ventsystems ([GUID], SystemName, [Project], Modyfied, Version, Author, [Place]) " +
                                              $"VALUES ('{ventSystem.GUID}', '{ventSystem.SystemName}', '{projectGUID}', '{date}', '{vesrsion}', '{Author}', '{buildGUID}')";
                command.CommandText = InsertQueryVensystem;
                command.ExecuteNonQuery();
                #endregion
                #region Write Ventsystem components to DataBase
                WriteVentSystemToDB.Execute(command.Connection.ConnectionString, ventSystem, project, building);

                #endregion
                #region Close DataBase
                command.Dispose();
                   
                #endregion
                #region Make TreeNode in Ventsystems Tree
                RadTreeNode radTreeNode1 = radTreeView1.SelectedNode;
                UpdateBuildNode(radTreeNode1);

                #endregion
            }
            catch
            {
                // ignored
            }
        }
        
        private void ReadJoinedSystems2()
        {
            radTreeView3.Nodes.Clear();//clear ventsystems
            radTreeView4.Nodes.Clear();//clear pannels
            radGridView3.DataSource = null;
            RadTreeNode selectednde = radTreeView1.SelectedNode;
            if (selectednde == null) return;
            bool nodeparentpresent = (selectednde.Parent != null);
            string tableselect = string.Empty;
            string ventsystemselect = string.Empty;
            if (nodeparentpresent)
            {
                if (selectednde.Tag is Building building)
                {
                    var nodeguid = building.BuildGUID;
                    ventsystemselect = "SELECT VentSystems.GUID, VentSystems.SystemName, Pannel, PannelName " +
                                       "FROM VentSystems " +
                                       $"WHERE[Place] = '{nodeguid}'";
                    tableselect = "SELECT Pannel.GUID, PannelName " +
                                  "FROM Pannel " +
                                  $"WHERE[Place] = '{nodeguid}'";
                }
            }

            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection
            };



            SQLiteDataReader dataReader;
            if (ventsystemselect != string.Empty)
            {
                if (connection.State == ConnectionState.Open)
                {
                    try
                    {
                        command.CommandText = ventsystemselect;
                        dataReader = command.ExecuteReader();
                        while (dataReader.Read())
                        {
                            var ventSystemGUID = dataReader[0].ToString();
                            var pannelGUID = dataReader[2].ToString();
                            RadTreeNode VentSystemNode = null;
                            if (pannelGUID != string.Empty)
                            {
                                Pannel pannel = FindPannel(pannelGUID);
                                RadTreeNode PannelNode = null;
                                RadTreeNode PannelNodeExist = null;


                                if (pannel != null)
                                {
                                    PannelNodeExist = FindNodeByName(pannelGUID, radTreeView4.Nodes);
                                    if (PannelNodeExist == null)
                                    {
                                        PannelNode = new RadTreeNode
                                        {
                                            Name = pannel.GetGUID(),
                                            Value = pannel.PannelName,
                                            Text = pannel.PannelName,
                                            Tag = pannel

                                        };
                                    }
                                    else
                                    {
                                        PannelNode = PannelNodeExist;
                                    }



                                }
                                if (ventSystemGUID != string.Empty)
                                {
                                    VentSystemNode = FindNodeByName(ventSystemGUID, radTreeView2.Nodes);
                                    if (VentSystemNode != null && PannelNode != null)
                                    {
                                        VentSystemNode.ForeColor = Color.Green;
                                        VentSystem ventSystem = (VentSystem)VentSystemNode.Tag;
                                        ventSystem.ConnectedTo = PannelNode.Value.ToString();
                                        RadTreeNode clone = (RadTreeNode)VentSystemNode.Clone();
                                        PannelNode.Nodes.Add(clone);


                                    }
                                }

                                if (PannelNodeExist == null && PannelNode != null) radTreeView4.Nodes.Add(PannelNode);

                            }
                            else
                            {
                                if (ventSystemGUID == string.Empty) continue;
                                VentSystemNode = FindNodeByName(ventSystemGUID, radTreeView2.Nodes);
                                if (VentSystemNode == null) continue;
                                RadTreeNode clone = (RadTreeNode)VentSystemNode.Clone();
                                radTreeView3.Nodes.Add(clone);
                            }

                        }

                        dataReader.Close();


                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message + ex.StackTrace);
                    }
                }
            }
            if (tableselect != string.Empty)
            {
                if (connection.State != ConnectionState.Open) connection.Open();
                {
                    try
                    {
                        command.CommandText = tableselect;
                        dataReader = command.ExecuteReader();
                        while (dataReader.Read())
                        {
                            var pannelGUID = dataReader[0].ToString();
                            if (pannelGUID == string.Empty) continue;
                            var PannelNode = FindNodeByName(pannelGUID, radTreeView4.Nodes);
                            if (PannelNode != null) continue;
                            Pannel pannel = FindPannel(pannelGUID);
                            if (pannel == null) continue;
                            PannelNode = new RadTreeNode
                            {
                                Name = pannel.GetGUID(),
                                Value = pannel.PannelName,
                                Text = pannel.PannelName,
                                Tag = pannel

                            };
                            radTreeView4.Nodes.Add(PannelNode);
                        }
                        dataReader.Close();

                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            command.Dispose();


        }
        private void radTreeView3_DragOverNode(object sender, RadTreeViewDragCancelEventArgs e)
        {
            Point p = radTreeView4.PointToClient(new Point(Cursor.Position.X, Cursor.Position.Y));
            _ = radTreeView4.GetNodeAt(p.X, p.Y);
            if (e.DropPosition != DropPosition.AsChildNode || e.TargetNode.Level !=0)
            {
                e.Cancel = true;
            }
            else
            {
                radTreeView4.SelectedNode = e.TargetNode;
                radTreeView4.Update();
            }
        }
        private void radTreeView4_DragOverNode(object sender, RadTreeViewDragCancelEventArgs e)
        {

            Point p = radTreeView4.PointToClient(new Point(Cursor.Position.X, Cursor.Position.Y));
            RadTreeNode parent = radTreeView4.GetNodeAt(p.X, p.Y);
            if (parent !=null)
            {
                _ = parent.Nodes.Count;

            }
            if (e.Node.Level == 0 || e.DropPosition != DropPosition.AsChildNode || e.TargetNode.Tag is VentSystem)
            {
                e.Cancel = true;
            }
            
        }
        private void radTreeView3_DragEnded(object sender, RadTreeViewDragEventArgs e)
        {
            try
            {
                //e.Node its Ventsystem node (guid = Node.Name, name = Node.Value & Node.Text)
                e.Node.ForeColor = Color.Green;
                Pannel panel = (Pannel)e.Node.Parent.Tag;
                string updatvenystemquery = $"UPDATE VentSystems SET Pannel= '{panel.GetGUID()}', PannelName= '{panel.PannelName}' WHERE GUID= '{e.Node.Name}'";
                string InsertPannelToCableQuery = $"UPDATE Cable SET FromPannel = '{panel.PannelName}', FromGUID = '{panel.GetGUID()}' WHERE SystemGUID = '{e.Node.Name}'";
                if (connection.State != ConnectionState.Open) return;
                SQLiteCommand command = new  SQLiteCommand
                {
                    Connection = connection
                };

                int cnt = 1;
                command.CommandText = $"SELECT [To] FROM Cable WHERE FromGUID = '{e.Node.Parent.Name}'"; //получение всех устройств, подключенных к шкафу
                DataTable dt = new DataTable();
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                  dt.Load(reader);  
                }
                if (dt.Rows.Count > 0)
                {
                    int[] rowlist = dt.AsEnumerable().ToList()
                        .Select(e1 =>
                        {
                            int.TryParse(Regex.Match(e1.ItemArray[0].ToString(), @"^\d+").ToString(),
                                out int s);
                            return s;
                        })
                        .Distinct()
                        .OrderBy(e2 => e2)
                        .ToArray();

                    var results = Enumerable.Range(1, rowlist.Max()).Except(rowlist).FirstOrDefault();
                    cnt = results == 0 ? rowlist.Max() + 1 : results;
                    
                }



                command.CommandText = updatvenystemquery;
                command.ExecuteNonQuery();
                command.CommandText = InsertPannelToCableQuery;
                command.ExecuteNonQuery();
                UpdateConnectedPosNames(ref command, e.Node.Name, cnt, ".");
                    
                Task.Factory.StartNew(() => 
                    panel.Power = EditorV2.UpdatePannelPower(e.Node.Parent, command.Connection.ConnectionString)
                );
                Task.Factory.StartNew(() =>
                    panel.Voltage = EditorV2.UpdatePannelVoltage(e.Node.Parent, command.Connection.ConnectionString)
                );
                radPropertyGrid1.SelectedObject = panel;
                //UpdatePannelPower(ventSystem, panel);                    
            }
            catch (Exception ex)
            { 
                MessageBox.Show(ex.StackTrace); 
            }
        }
        internal double GetVentSystemPower(VentSystem ventSystem)
        {
            var electrdevices = (from EditorV2.PosInfo component in ventSystem.ComponentsV2
                                 where component.Tag is IPower
                                 select component.Tag)
                                .ToList();
                                
            double power = 0;
            electrdevices.AsParallel().ForAll(electrdevice =>
            {
                try
                {
                    power += Convert.ToDouble(((IPower)electrdevice).Power);
                }
                catch
                {
                    // ignored
                }
            });
            
            foreach (EditorV2.PosInfo posInfo in ventSystem.ComponentsV2)
            {
                switch (posInfo.Tag.GetType().Name)
                {
                    case nameof(Recuperator):
                        Recuperator recuperator = (Recuperator)posInfo.Tag;
                        (from t in recuperator
                                 .Cast<object>()
                                 .Where(e => e is IPower)
                         select (IPower)t)
                          .ToList()
                         .ForEach(e =>
                         {
                             try
                             {
                                 power += Convert.ToDouble(e.Power);
                             }
                             catch
                             {
                                 // ignored
                             }
                         });
                        break;
                    case nameof(WaterHeater):
                        WaterHeater waterHeater = (WaterHeater)posInfo.Tag;
                        (from t in waterHeater
                                .Cast<object>()
                                .Where(e => e is IPower)
                         select (IPower)t)
                         .ToList()
                         .ForEach(e =>
                         {
                             try
                             {
                                 power += Convert.ToDouble(e.Power);
                             }
                             catch { }
                         });
                        break;
                    case nameof(Froster):
                        Froster froster = (Froster)posInfo.Tag;
                        (from t in froster
                        .Cast<object>()
                        .Where(e => e is IPower)
                         select (IPower)t)
                         .ToList()
                         .ForEach(e =>
                         {
                             try
                             {
                                 power += Convert.ToDouble(e.Power);
                             }
                             catch { }
                         });
                        break;
                }
            }
            
            return power;
        }
        internal Pannel._Voltage GetVetSystemMaxVoltage(VentSystem ventSystem)
        {
            return ventSystem.ComponentsV2
                .Where(e => e.Tag is IPower)
                .ToList()
                .Select(e => e.Tag)
                .ToList()
                .Cast<IPower>()
                .FirstOrDefault(e => e.Voltage == ElectroDevice._Voltage.AC380) != null ? Pannel._Voltage.AC380 : Pannel._Voltage.AC220;
        }
        public void UpdateConnectedPosNames(ref  SQLiteCommand command, string systemguid, int AllConnectedSumm, string devider)
        {
            string setdefaultnames = $"Update Cable SET [To] = DefaultName WHERE SystemGUID = '{systemguid}'";
            string Numbering = $"SELECT [To], COUNT(*) AS ToCount FROM Cable WHERE SystemGUID = '{systemguid}' GROUP BY [To]";
            SQLiteConnection commandConnection = command.Connection;
             SQLiteCommand command1 = new  SQLiteCommand
            {
                Connection = commandConnection
            };
             SQLiteCommand command2 = new  SQLiteCommand
            {
                Connection = commandConnection
            };
             SQLiteCommand command3 = new  SQLiteCommand
            {
                Connection = commandConnection
            };            
            command.CommandText = setdefaultnames;
            command.ExecuteNonQuery();
            command.CommandText = Numbering;
            //if (command.Connection.State == ConnectionState.Closed) command.Connection.Open();
            SQLiteDataReader dataReader = command.ExecuteReader();
            try
            {
                while (dataReader.Read())
                {
                    string Pos = dataReader[0].ToString();
                    var query = "SELECT [To], Cable.ToGUID, Cable.GUID, Cable.SortPriority, Cable.WriteBlock, Cable.TableForSearch FROM Cable " +
                                $"WHERE((([To]) = '{Pos}') AND((Cable.SystemGUID) = '{systemguid}')) " +
                                "ORDER BY Cable.SortPriority;";
                    command1.CommandText = query;                        
                    using (SQLiteDataReader readerchild1 = command1.ExecuteReader())
                    {
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
                                using (SQLiteDataReader readerchild2 = command3.ExecuteReader())
                                {
                                    while (readerchild2.Read())
                                    {
                                        posnames.Newposname = readerchild2[0].ToString();
                                    }                                       
                                    readerchild2.Close();
                                }
                            }
                            
                            string updateposquery = $"UPDATE Cable Set [To] = '{posnames.Newposname}' " +
                                                    $"WHERE Cable.ToGUID = '{posguid}'";// AND Cable.GUID = '{cableguid}'";
                            command2.CommandText = updateposquery;                               
                            command2.ExecuteNonQuery();                                
                            //description = readerchild[0].ToString();
                        }
                        readerchild1.Close();
                    }
                }
                dataReader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.StackTrace);
            }
            finally
            {                    
                command1.Dispose();
                command2.Dispose();
                command3.Dispose();                    
            }            
            
        }
        private void radTreeView3_NodeMouseClick(object sender, RadTreeViewEventArgs e)
        {
            radPropertyGrid1.SelectedObject = e.Node.Tag;            
        }
        private void radTreeView4_DragEnded(object sender, RadTreeViewDragEventArgs e)
        {
            try
            {
                VentSystem ventSystem = (VentSystem)e.Node.Tag;
                string panelname = e.Node.Parent.Text;
                Pannel pannel = (Pannel)e.Node.Parent.Tag;
                string pannelguid = pannel.GetGUID();
                string updatvenystemquery = $"UPDATE VentSystems SET Pannel= '{pannelguid}', PannelName= '{panelname}' WHERE GUID= '{e.Node.Name}'";
                string InsertPannelToCableQuery = $"UPDATE Cable SET FromPannel = '{panelname}', FromGUID = '{pannelguid}' WHERE SystemGUID = '{e.Node.Name}'";
                string setdefaultnames = $"Update Cable SET [To] = DefaultName WHERE SystemGUID = '{ventSystem.GUID}'";
                if (connection.State != ConnectionState.Open) return;
                SQLiteCommand command = new  SQLiteCommand
                {
                    Connection = connection
                };                    
                command.CommandText = setdefaultnames;
                command.ExecuteNonQuery();
                command.CommandText = updatvenystemquery;
                command.ExecuteNonQuery();
                command.CommandText = InsertPannelToCableQuery;
                command.ExecuteNonQuery();
                command.Dispose();                    
                UpdateConnectedPosNames(ventSystem.GUID, e.Node.Parent.GetNodeCount(false), ".");
                Task.Factory.StartNew(() =>
                    pannel.Power = EditorV2.UpdatePannelPower(e.Node.Parent, connection.ConnectionString)
                );
                Task.Factory.StartNew(() =>
                    pannel.Voltage = EditorV2.UpdatePannelVoltage(e.Node.Parent, connection.ConnectionString)
                );

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }            
        }
        private void radTreeView4_NodeMouseClick(object sender, RadTreeViewEventArgs e)
        {           
            Building building = radTreeView1.SelectedNode.Tag as Building;            
            int childcnt = e.Node.GetNodeCount(false);
            radGridView3.DataSource = null;            
            CreateCableHeader();            
            if (childcnt>0)
            {                
                GroupDescriptor descriptor = new GroupDescriptor();
                descriptor.GroupNames.Add("Система", ListSortDirection.Ascending);
                descriptor.Aggregates.Add("Count(Система)");
                descriptor.Format = "{0}: {1} имеет {2} кабеля(ей)";
                radGridView3.GroupDescriptors.Add(descriptor);
                //this.radGridView3.GroupDescriptors.Add(new Telerik.WinControls.Data.GroupDescriptor("Система"));
                foreach(RadTreeNode chld in  e.Node.Nodes)
                {
                    if (chld != null)
                    {
                        CreateCableJournal(chld, building);
                    }
                }
            }
            else
            {
                if (e.Node.Tag is VentSystem) CreateCableJournal(e.Node, building);
            }
            foreach(var item in radGridView3.ChildRows)
            {
                if (!(item is GridViewGroupRowInfo)) continue;
                var groupRow = item as GridViewGroupRowInfo;
                groupRow.IsExpanded = false;
            }
            radPropertyGrid1.SelectedObject = e.Node.Tag;
        }

        private void CreateCableJournal(RadTreeNode ventsystemnode, Building building)
        {
            bool writepumpcable = false;
            bool writevalvecable = false;
            string SelectCablesQuery = $"Select Cable.GUID, CableName, FromPannel, [To], CableType, CableLenght, SortPriority, TableForSearch, ToGUID, SystemName, Description, CableAttribute, WriteBlock, BlockName, WireNumbers, FromGUID FROM Cable WHERE SystemGUID = '{ventsystemnode.Name}' ORDER BY [SortPriority], [TO] ASC";
            SQLiteCommand command = new  SQLiteCommand
            {
                Connection = connection
            };
             SQLiteCommand command1 = new  SQLiteCommand
             {
                Connection = connection
            };           
            if (connection.State == ConnectionState.Open)
            {
                try
                {
                    string changecableattr = $"SELECT WritePumpCable, WriteValveCable FROM BuildSetting WHERE Place = '{building.BuildGUID}'";
                    command.CommandText = changecableattr;
                    using (SQLiteDataReader readerchangeattr = command.ExecuteReader())
                    {
                        while (readerchangeattr.Read())
                        {
                            writepumpcable = Convert.ToBoolean(readerchangeattr[0].ToString());
                            writevalvecable = Convert.ToBoolean(readerchangeattr[1].ToString());
                        }
                        readerchangeattr.Close();
                    }
                    command.CommandText = SelectCablesQuery;

                    using (SQLiteDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var cbleguid = dataReader[0].ToString();
                            var cablename = dataReader[1].ToString();
                            var frompannel = dataReader[2].ToString();
                            var to = dataReader[3].ToString();
                            var cabletype = dataReader[4].ToString();
                            var lenght = dataReader[5].ToString();
                            var sort = dataReader[6].ToString();
                            var tableforsearch = dataReader[7].ToString();
                            var devguid = dataReader[8].ToString();
                            var systemname = dataReader[9].ToString();
                            var attribute = dataReader[11].ToString();
                            var writeblock = dataReader[12].ToString();
                            var blockname = dataReader[13].ToString();
                            var WireNumbers = dataReader[14].ToString();
                            var fromGUID = dataReader[15].ToString();
                            var description = string.Empty;
                            var SelectDescription = $"SELECT Description FROM Cable WHERE GUID = '{cbleguid}'";
                            command1.CommandText = SelectDescription;
                            using (SQLiteDataReader readerchild = command1.ExecuteReader())
                            {
                                while (readerchild.Read())
                                {
                                    description = readerchild[0].ToString();
                                }
                                readerchild.Close();
                            }                            
                            CreateCableRows(cbleguid, cablename, frompannel, to, cabletype, lenght, systemname, description, sort, attribute, writeblock, blockname, WireNumbers, tableforsearch, devguid, writepumpcable, writevalvecable, fromGUID);
                        }
                        command1.Dispose();                        
                        dataReader.Close();
                        //DataTable dataTable = radGridView3.DataSource as DataTable;
                        //dataTable.AcceptChanges();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + ex.StackTrace);
                }
            }
            command.Dispose();            
        }
        private void radGridView3_ContextMenuOpening(object sender, ContextMenuOpeningEventArgs e)
        {
            e.ContextMenu = cablecontextMenu.DropDown;
        }
        private void radTreeView2_NodeMouseClick(object sender, RadTreeViewEventArgs e)
        {
            
            
            
            if (e.Node != null )
            {
                try
                {
                    if (!(e.Node.Tag is VentSystem ventSystem)) return;
                    

                    void CreateRow<T>(dynamic obj)
                    {
                        Dictionary<string, string[]> dict = new Dictionary<string, string[]>
                        {
                            {nameof(ElectroHeater), new[] { "Электрический нагреватель", "Приток" }},
                            {nameof(WaterHeater), new[]{ "Водяной нагреватель", "Приток" } },
                            {nameof(SupplyDamper), new[]{ "Приточная заслонка", "Приток" } },
                            {nameof(OutdoorTemp), new[]{ "Датчик Т наружной", "Улица" } },
                            {nameof(SupplyFiltr), new[]{ "Приточный фильтр", "Приток" } },
                            {nameof(SupplyVent), new[]{ "Приточный вентилятор", "Приток" } },
                            {nameof(Froster), new[]{ "Охладитель", "Приток" } },
                            {nameof(Humidifier), new[]{ "Увлажнитель", "Приток" } },
                            {nameof(ExtVent), new[] { "Вытяжной вентилятор", "Вытяжка"} },
                            {nameof(ExtFiltr), new[]{ "Вытяжной фильтр", "Вытяжка" } },
                            {nameof(ExtDamper), new[]{ "Вытяжная заслонка", "Вытяжка" } },
                            {nameof(Recuperator), new[]{ "Рекуператор", "Приток/Вытяжка" } },
                            {nameof(SensorT), new[]{ "Датчик температуры", "Без расположения"} },
                            {nameof(Humidifier.HumiditySens), new[]{"Датчик влажности", "Без расположения"} },
                            {nameof(SupplyTemp), new[]{"Датчик температуры в приточном канале", "Приток"} },
                            {nameof(ExhaustTemp), new[]{"Датчик температуры в выятжном канале", "Вытяжка"} },
                            {nameof(Filtr), new[]{"Фильтр", "Без расположения"} },
                            {nameof(IndoorTemp), new[]{"Датчик температуры в помещении", "Помещение"} },
                            {nameof(SpareSuplyVent), new[]{"Сдвоенный приточный вентилятор", "Приток"} },
                            {nameof(SpareExtVent), new[]{"Сдвоенный вытяжной вентилятор", "Вытяжка"} },
                        };
                        var t1 = typeof(T).Name;
                        var t2 = obj.GetType().Name;


                        switch (obj.GetType().Name)
                        {
                            case nameof(ElectroHeater):
                            case nameof(WaterHeater):
                            case nameof(SupplyFiltr):
                            case nameof(SupplyVent):
                            case nameof(Froster):
                            case nameof(Humidifier):
                            case nameof(ExtVent):
                            case nameof(ExtFiltr):
                            case nameof(ExtDamper):
                            case nameof(Recuperator):
                            case nameof(Filtr):
                            
                                GridViewRowInfo row1 = CreateVentSystemsRows(obj.GUID, dict[obj.GetType().Name]?[0], dict[obj.GetType().Name]?[1]);
                                row1.Tag = obj;
                                break;
                            case nameof(SupplyDamper):

                                SupplyDamper supplyDamper = (SupplyDamper)obj;
                                GridViewRowInfo row11 = CreateVentSystemsRows(
                                    supplyDamper.GUID,
                                    "Приточная заслонка",
                                    "Приток");
                                row11.Tag = supplyDamper;
                                if (supplyDamper.outdoorTemp != null)
                                {
                                        
                                    //string s = (supplyDamper.outdoorTemp).GetType().Name;
                                    GridViewRowInfo row2 = CreateVentSystemsRows(supplyDamper.outdoorTemp.GUID, dict[supplyDamper.outdoorTemp.GetType().Name]?[0], dict[supplyDamper.outdoorTemp.GetType().Name]?[1]);
                                    row2.Tag = supplyDamper.outdoorTemp;
                                }
                                break;
                            case nameof(SpareSuplyVent):
                                SpareSuplyVent spareSuplyVent = (SpareSuplyVent) obj;
                                GridViewRowInfo row3 = CreateVentSystemsRows(spareSuplyVent.GUID, dict[spareSuplyVent.GetType().Name]?[0], dict[spareSuplyVent.GetType().Name]?[1]);
                                row3.Tag = spareSuplyVent;
                                break;
                            case nameof(SpareExtVent):
                                SpareExtVent spareExtVent = (SpareExtVent) obj;
                                GridViewRowInfo row4 = CreateVentSystemsRows(spareExtVent.GUID, dict[spareExtVent.GetType().Name]?[0], dict[spareExtVent.GetType().Name]?[1]);
                                row4.Tag = spareExtVent;
                                break;
                            case nameof(CrossSection):
                                List<ExhaustTemp> exhaustTemps = ventSystem.ComponentsV2
                                    .Where(v => v.Tag.GetType().Name == nameof(CrossSection))
                                    .Select(v => v.Tag)
                                    .Cast<CrossSection>()
                                    .Where(v => v._SensorT != null)
                                    .Where(v => v._SensorT.Location == "Exhaust")
                                    .Select(v => v._SensorT)
                                    .Cast<ExhaustTemp>()
                                    .ToList();
                                exhaustTemps.RemoveAll(item => item == null);
                                List<SupplyTemp> supplyTemps = ventSystem.ComponentsV2
                                    .Where(v => v.Tag.GetType().Name == nameof(CrossSection))
                                    .Select(v => v.Tag)
                                    .Cast<CrossSection>()
                                    .Where(v => v._SensorT != null)
                                    .Where(v => v._SensorT.Location == "Supply")
                                    .Select(v => v._SensorT)
                                    .Cast<SupplyTemp>()
                                    .ToList();
                                supplyTemps.RemoveAll(item => item == null);
                                List<SensorT> sensorTs = ventSystem.ComponentsV2
                                    .Where(v => v.Tag.GetType().Name == nameof(CrossSection))
                                    .Select(v => v.Tag)
                                    .Cast<CrossSection>()
                                    .Where(v => v._SensorT != null)
                                    .Where(v => string.IsNullOrEmpty(v._SensorT.Location))
                                    .Select(v => v._SensorT)
                                    .ToList();
                                sensorTs.RemoveAll(item => item == null);
                                List<Humidifier.HumiditySens> HumiditySens = ventSystem.ComponentsV2
                                    .Where(v => v.Tag.GetType().Name == nameof(CrossSection))
                                    .Select(v => v.Tag)
                                    .Cast<CrossSection>()
                                    .Select(v => v._SensorH)
                                    .ToList();
                                HumiditySens.RemoveAll(item => item == null);
                                if (supplyTemps.Count > 0)
                                {
                                    foreach (SupplyTemp supplyTemp in supplyTemps)
                                    {
                                        GridViewRowInfo row2  = CreateVentSystemsRows(supplyTemp.GUID, dict[supplyTemp.GetType().Name]?[0], dict[supplyTemp.GetType().Name]?[1]);
                                        row2.Tag = supplyTemp;
                                    }
                                }
                                if (exhaustTemps.Count > 0)
                                {
                                    foreach (ExhaustTemp exhaustTemp in exhaustTemps)
                                    {
                                        GridViewRowInfo row2 = CreateVentSystemsRows(exhaustTemp.GUID, 
                                            dict[exhaustTemp.GetType().Name]?[0], 
                                            dict[exhaustTemp.GetType().Name]?[1]);
                                        row2.Tag = exhaustTemp;
                                    }
                                }
                                if (sensorTs.Count > 0)
                                {
                                    foreach (SensorT sensorT in sensorTs)
                                    {
                                        GridViewRowInfo row2 = CreateVentSystemsRows(sensorT.GUID, 
                                            dict[sensorT.GetType().Name]?[0], 
                                            dict[sensorT.GetType().Name]?[1]);
                                        row2.Tag = sensorT;
                                    }
                                }
                                if (HumiditySens.Count > 0)
                                {
                                        
                                    foreach (Humidifier.HumiditySens humiditySens in HumiditySens)
                                    {
                                        GridViewRowInfo row33 = null;
                                        switch (humiditySens.Location)
                                        {
                                            case "Supply":
                                                row33= CreateVentSystemsRows(humiditySens.GUID, "Датчик влажности в приточном канале", "Приток");
                                                break;
                                            case "Exhaust":
                                                row33= CreateVentSystemsRows(humiditySens.GUID, "Датчик влажности в вытяжном канале", "Вытяжка");
                                                break;
                                            default:
                                                row33 = CreateVentSystemsRows(humiditySens.GUID, dict[humiditySens.GetType().Name]?[0], dict[humiditySens.GetType().Name]?[1]);
                                                break;
                                        }
                                        if (row33 != null) row33.Tag = humiditySens;
                                    }
                                }
                                break;
                                    
                            case nameof(Room):

                                List<IndoorTemp> indoorTemps = ventSystem.ComponentsV2
                                    .Where(v => v.Tag.GetType().Name == nameof(Room))
                                    .Select(v => v.Tag)
                                    .Cast<Room>()
                                    .Where(v => v._SensorT != null)
                                    .Where(v => v._SensorT is IndoorTemp)
                                    .Select(v => v._SensorT)
                                    .Cast<IndoorTemp>()
                                    .ToList();
                                List<Humidifier.HumiditySens> indoorHumiditySens = ventSystem.ComponentsV2
                                    .Where(v => v.Tag is Room)
                                    .Select(v => v.Tag)
                                    .Cast<Room>()
                                    .Where(v => v._SensorH != null)
                                    .Select(v => v._SensorH)
                                    .ToList();
                                if (indoorTemps.Count > 0)
                                {
                                    foreach(IndoorTemp indoorTemp in indoorTemps)
                                    {
                                        var row41 = CreateVentSystemsRows(indoorTemp.GUID, dict[indoorTemp.GetType().Name]?[0], dict[indoorTemp.GetType().Name]?[1]);
                                        row41.Tag = indoorTemp;
                                    }
                                }
                                if (indoorHumiditySens.Count>0)
                                {
                                    foreach(Humidifier.HumiditySens humiditySens in indoorHumiditySens)
                                    {
                                        var row42 = CreateVentSystemsRows(humiditySens.GUID, "Датчик влажности в помещении", "Помещение");
                                        row42.Tag = humiditySens;
                                    }
                                }

                                break;

                        }
                        if (t2 == "Missing" && t1 == nameof(CrossSection))
                        {
                            List<ExhaustTemp> exhaustTemps = ventSystem.ComponentsV2
                                    .Where(v => v.Tag.GetType().Name == nameof(CrossSection))
                                    .Select(v => v.Tag)
                                    .Cast<CrossSection>()
                                    .Where(v => v._SensorT != null)
                                    .Where(v => v._SensorT.Location == "Exhaust")
                                    .Select(v => v._SensorT)
                                    .Cast<ExhaustTemp>()
                                    .ToList();
                            exhaustTemps.RemoveAll(item => item == null);
                            List<SupplyTemp> supplyTemps = ventSystem.ComponentsV2
                                .Where(v => v.Tag.GetType().Name == nameof(CrossSection))
                                .Select(v => v.Tag)
                                .Cast<CrossSection>()
                                .Where(v => v._SensorT != null)
                                .Where(v => v._SensorT.Location == "Supply")
                                .Select(v => v._SensorT)
                                .Cast<SupplyTemp>()
                                .ToList();
                            supplyTemps.RemoveAll(item => item == null);
                            List<SensorT> sensorTs = ventSystem.ComponentsV2
                                .Where(v => v.Tag.GetType().Name == nameof(CrossSection))
                                .Select(v => v.Tag)
                                .Cast<CrossSection>()
                                .Where(v => v._SensorT != null)
                                .Where(v => string.IsNullOrEmpty(v._SensorT.Location))
                                .Select(v => v._SensorT)
                                .ToList();
                            sensorTs.RemoveAll(item => item == null);
                            List<Humidifier.HumiditySens> HumiditySens = ventSystem.ComponentsV2
                                .Where(v => v.Tag.GetType().Name == nameof(CrossSection))
                                .Select(v => v.Tag)
                                .Cast<CrossSection>()
                                .Select(v => v._SensorH)
                                .ToList();
                            HumiditySens.RemoveAll(item => item == null);
                            if (supplyTemps.Count > 0)
                            {
                                foreach (SupplyTemp supplyTemp in supplyTemps)
                                {
                                    GridViewRowInfo row2 = CreateVentSystemsRows(supplyTemp.GUID, dict[supplyTemp.GetType().Name]?[0], dict[supplyTemp.GetType().Name]?[1]);
                                    row2.Tag = supplyTemp;
                                }
                            }
                            if (exhaustTemps.Count > 0)
                            {
                                foreach (ExhaustTemp exhaustTemp in exhaustTemps)
                                {
                                    GridViewRowInfo row2 = CreateVentSystemsRows(exhaustTemp.GUID,
                                        dict[exhaustTemp.GetType().Name]?[0],
                                        dict[exhaustTemp.GetType().Name]?[1]);
                                    row2.Tag = exhaustTemp;
                                }
                            }
                            if (sensorTs.Count > 0)
                            {
                                foreach (SensorT sensorT in sensorTs)
                                {
                                    GridViewRowInfo row2 = CreateVentSystemsRows(sensorT.GUID,
                                        dict[sensorT.GetType().Name]?[0],
                                        dict[sensorT.GetType().Name]?[1]);
                                    row2.Tag = sensorT;
                                }
                            }
                            if (HumiditySens.Count > 0)
                            {

                                foreach (Humidifier.HumiditySens humiditySens in HumiditySens)
                                {
                                    GridViewRowInfo row33 = null;
                                    switch (humiditySens.Location)
                                    {
                                        case "Supply":
                                            row33 = CreateVentSystemsRows(humiditySens.GUID, humiditySens.Description, "Приток");
                                            break;
                                        case "Exhaust":
                                            row33 = CreateVentSystemsRows(humiditySens.GUID, "Датчик влажности в вытяжном канале", "Вытяжка");
                                            break;
                                        default:
                                            row33 = CreateVentSystemsRows(humiditySens.GUID, dict[humiditySens.GetType().Name]?[0], dict[humiditySens.GetType().Name]?[1]);
                                            break;
                                    }
                                    if (row33 != null) row33.Tag = humiditySens;
                                }
                            }
                        }


                    }
                    radGridView1.DataSource = null;
                    CreateHeader();
                    string ventsystemguid = ventSystem.GUID;
                    string tableselect = "SELECT SupplyVent, SupplyFilter, SupplyDamper, WaterHeater, ElectroHeat, Froster, Humidifier, ExtVent, ExtFilter, ExtDamper, Recuperator, SensTOutdoor, SensTIndoor, SensHSupply, Modyfied, Version, Author, Place, SensTExhaust, SensTSupply, SensHIndoor, CrossConnection, Room, Filter, SensTNoLocation, SpareSupplyVent, SpareExtVent " +
                                         "FROM VentSystems " +
                                         $"WHERE GUID = '{ventsystemguid}'";
                    SQLiteCommand command = new SQLiteCommand();
                    if (connection.State == ConnectionState.Closed) connection.Open();
                    if (connection.State == ConnectionState.Open)
                    {
                        command.Connection = connection;
                        command.CommandText = tableselect;
                        SQLiteDataReader reader = command.ExecuteReader();
                        bool _SupplyVent = false;
                        bool _SupplyFilter = false;
                        bool _SupplyDamper = false;
                        bool _WaterHeater = false;
                        bool _ElectroHeat = false;
                        bool _Froster = false;
                        bool _Humidifier = false;
                        bool _ExtVent = false;
                        bool _ExtFilter = false;
                        bool _ExtDamper = false;
                        bool _Recuperator = false;
                        bool _Room = false;
                        bool _CrossConnection = false;
                        bool _Filter = false;
                        bool _SpareSupplyVent = false;
                        bool _SpareExtVent = false;

                        while (reader.Read())
                        {
                            bool.TryParse(reader[0].ToString(),  out _SupplyVent);
                            bool.TryParse(reader[1].ToString(), out _SupplyFilter);
                            bool.TryParse(reader[2].ToString(), out _SupplyDamper);
                            bool.TryParse(reader[3].ToString(), out _WaterHeater);
                            bool.TryParse(reader[4].ToString(), out _ElectroHeat);
                            bool.TryParse(reader[5].ToString(), out _Froster);
                            bool.TryParse(reader[6].ToString(), out _Humidifier);
                            bool.TryParse(reader[7].ToString(), out _ExtVent);
                            bool.TryParse(reader[8].ToString(), out _ExtFilter);
                            bool.TryParse(reader[9].ToString(), out _ExtDamper);
                            bool.TryParse(reader[10].ToString(), out _Recuperator);
                            bool.TryParse(reader[11].ToString(), out _);
                            bool.TryParse(reader[12].ToString(), out _);
                            bool.TryParse(reader[18].ToString(), out _);
                            bool.TryParse(reader[19].ToString(), out _);
                            bool.TryParse(reader[20].ToString(), out _);
                            bool.TryParse(reader[21].ToString(), out _CrossConnection);
                            bool.TryParse(reader[22].ToString(), out _Room);
                            bool.TryParse(reader[23].ToString(), out _Filter);
                            bool.TryParse(reader[24].ToString(), out _);
                            bool.TryParse(reader[25].ToString(), out _SpareSupplyVent);
                            bool.TryParse(reader[26].ToString(), out _SpareExtVent);
                        }
                        reader.Close();
                        if (_SupplyVent)
                        {
                            List<SupplyVent> supplyVents  = (from ventcomp in ventSystem.ComponentsV2
                                        .Where(v => v.Tag is SupplyVent)
                                    select (SupplyVent)ventcomp.Tag)
                                .ToList();
                            if (supplyVents.Count > 0)
                            {
                                foreach (SupplyVent vent in supplyVents)
                                {
                                    CreateRow<SupplyVent>(vent);
                                }
                            }
                            
                        }

                        if (_SpareSupplyVent)
                        {
                            List<SpareSuplyVent> spareSupplyVents = (from ventcomp in ventSystem.ComponentsV2
                                        .Where(v => v.Tag is SpareSuplyVent)
                                    select (SpareSuplyVent)ventcomp.Tag)
                                .ToList();
                            if (spareSupplyVents.Count > 0)
                            {
                                foreach (SpareSuplyVent spareSuplyVent in spareSupplyVents)
                                {
                                    CreateRow<SpareSuplyVent>(spareSuplyVent);
                                }
                            }
                        }
                        if (_SpareExtVent)
                        {
                            List<SpareExtVent> spareExtVents = (from ventcomp in ventSystem.ComponentsV2
                                        .Where(v => v.Tag is SpareExtVent)
                                    select (SpareExtVent)ventcomp.Tag)
                                .ToList();
                            if (spareExtVents.Count > 0)
                            {
                                foreach (SpareExtVent spareExt in spareExtVents)
                                {
                                    CreateRow<SpareExtVent>(spareExt);
                                }
                            }
                        }
                        if (_ExtVent)
                        {
                            List<ExtVent> extVents = (from ventcomp in ventSystem.ComponentsV2
                                        .Where(v => v.Tag is ExtVent)
                                    select (ExtVent)ventcomp.Tag)
                                .ToList();
                            if (extVents.Count > 0)
                            {
                                foreach (ExtVent extVent in extVents)
                                {
                                    CreateRow<ExtVent>(extVent);
                                }
                            }

                        }
                        if (_SupplyFilter)
                        {
                            List<SupplyFiltr> FilterList = (from ventcomp in ventSystem.ComponentsV2
                                        .Where(v => v.Tag is SupplyFiltr)
                                    select (SupplyFiltr)ventcomp.Tag)
                                .ToList();
                            if (FilterList.Count > 0)
                            {
                                foreach (SupplyFiltr supplyFiltr in FilterList)
                                {
                                    CreateRow<SupplyFiltr>(supplyFiltr);
                                }
                            }
                        }
                        if (_ExtFilter)
                        {
                            List<ExtFiltr> extFiltrs = (from ventcomp in ventSystem.ComponentsV2
                                        .Where(v => v.Tag is ExtFiltr)
                                    select (ExtFiltr)ventcomp.Tag)
                                .ToList();
                            if (extFiltrs.Count > 0)
                            {
                                foreach (ExtFiltr extFiltr in extFiltrs)
                                {
                                    CreateRow<ExtFiltr>(extFiltr);
                                }
                            }
                        }
                        if (_Filter)
                        {
                            List<Filtr> filtrs = (from ventcomp in ventSystem.ComponentsV2
                                        .Where(v => v.Tag is Filtr)
                                    select (Filtr)ventcomp.Tag)
                                .ToList();
                            if (filtrs.Count > 0)
                            {
                                foreach (Filtr filtr in filtrs)
                                {
                                    CreateRow<Filtr>(filtr);
                                }
                            }
                        }
                        if (_SupplyDamper)
                        {

                            List<SupplyDamper> DamperList = (from ventcomp in ventSystem.ComponentsV2
                                        .Where(v => v.Tag is SupplyDamper)
                                    select (SupplyDamper)ventcomp.Tag)
                                .ToList();
                            if (DamperList.Count>0)
                            {
                                foreach(SupplyDamper supplyDamper in DamperList)
                                {
                                    CreateRow<SupplyDamper>(supplyDamper);
                                        
                                }
                            }
                        }
                        if (_ExtDamper)
                        {
                            List<ExtDamper> extDampers = (from ventcomp in ventSystem.ComponentsV2
                                        .Where(v => v.Tag is ExtDamper)
                                    select (ExtDamper)ventcomp.Tag)
                                .ToList();
                            if (extDampers.Count > 0)
                            {
                                foreach (ExtDamper extDamper in extDampers)
                                {
                                    CreateRow<ExtDamper>(extDamper);
                                }
                            }

                        }
                        if (_WaterHeater)
                        {
                            List<WaterHeater> waterHeaters = (from ventcomp in ventSystem.ComponentsV2
                                        .Where(v => v.Tag is WaterHeater)
                                    select (WaterHeater)ventcomp.Tag)
                                .ToList();
                            if (waterHeaters.Count > 0)
                            {
                                foreach (WaterHeater waterHeater in waterHeaters)
                                {
                                    CreateRow<WaterHeater>(waterHeater);
                                }
                            }
                        }
                        if (_ElectroHeat)
                        {
                            List<ElectroHeater> List = (from ventcomp in ventSystem.ComponentsV2
                                        .Where(v => v.Tag is ElectroHeater)
                                    select (ElectroHeater)ventcomp.Tag)
                                .ToList();
                            if (List.Count > 0)
                            {
                                foreach (ElectroHeater heater in List)
                                {
                                    CreateRow<ElectroHeater>(heater);
                                }
                            }
                        }
                        if (_Froster)
                        {
                            List<Froster> List = (from ventcomp in ventSystem.ComponentsV2
                                        .Where(v => v.Tag is Froster)
                                    select (Froster)ventcomp.Tag)
                                .ToList();
                            if (List.Count > 0)
                            {
                                foreach (Froster froster in List)
                                {
                                    CreateRow<Froster>(froster);
                                }
                            }

                        }
                        if (_Humidifier)
                        {
                            List<Humidifier> List = (from ventcomp in ventSystem.ComponentsV2
                                        .Where(v => v.Tag is Humidifier)
                                    select (Humidifier)ventcomp.Tag)
                                .ToList();
                            if (List.Count > 0)
                            {
                                foreach (Humidifier humidifier in List)
                                {
                                    CreateRow<Humidifier>(humidifier);
                                }
                            }
                        }
                        if (_Recuperator)
                        {
                            List<Recuperator> recuperators = (from ventcomp in ventSystem.ComponentsV2
                                        .Where(v => v.Tag is Recuperator)
                                    select (Recuperator)ventcomp.Tag)
                                .ToList();
                            if (recuperators.Count > 0)
                            {
                                foreach (Recuperator recuperator in recuperators)
                                {
                                    CreateRow<Recuperator>(recuperator);
                                }
                            }
                        }
                        if (_Room)
                        {
                            CreateRow<Room>(Type.Missing);
                        }
                        if (_CrossConnection)
                        {
                            CreateRow<CrossSection>(Type.Missing);
                        }
                    }
                    command.Dispose();                       
                    radPropertyGrid1.SelectedObject = ventSystem;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + ex.StackTrace);
                }
                finally
                {
                    radGridView1.CurrentRow = null;
                }
            }
            else
            {
                radGridView1.DataSource = null;
            }
        }
        private void UpdateConnectedPosNames(string systemguid, int AllConnectedSumm, string devider)
        {
            try
            {
                string Numbering = $"SELECT [To], COUNT(*) AS ToCount FROM Cable WHERE SystemGUID = '{systemguid}' GROUP BY [To]";
                if (connection.State != ConnectionState.Open) return;
                SQLiteCommand command = new SQLiteCommand
                {
                    Connection = connection
                };
                SQLiteCommand command1 = new SQLiteCommand
                {
                    Connection = connection
                };
                SQLiteCommand command2 = new SQLiteCommand
                {
                    Connection = connection
                };
                command.CommandText = Numbering;
                SQLiteDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    int cnt = 1;
                    //string cntstr; ;
                    string Pos = dataReader[0].ToString();
                    //cntstr = dataReader[1].ToString();
                    var query = "SELECT [To], Cable.ToGUID, Cable.GUID, Cable.SortPriority FROM Cable " +
                                $"WHERE((([To]) = '{Pos}') AND((Cable.SystemGUID) = '{systemguid}')) " +
                                "ORDER BY Cable.SortPriority;";
                    command1.CommandText = query;
                    SQLiteDataReader readerchild1 = command1.ExecuteReader();
                    while (readerchild1.Read())
                    {
                        Posnames posnames = new Posnames();
                        string newposname = AllConnectedSumm + devider + readerchild1[0] + cnt;
                        posnames.Oldposname = readerchild1[0].ToString();
                        posnames.Newposname = newposname;
                        string posguid = readerchild1[1].ToString();
                        string updateposquery = $"UPDATE Cable Set [To] = '{posnames.Newposname}' " +
                                                $"WHERE Cable.ToGUID = '{posguid}'";
                        command2.CommandText = updateposquery;
                        command2.ExecuteNonQuery();
                        cnt++;
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
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }
        private void radGridView3_ViewCellFormatting(object sender, CellFormattingEventArgs e)
        {
            Font normalFont = new Font("Segoe UI", 8, FontStyle.Regular);
            Font bigFont = new Font("Segoe UI", 8, FontStyle.Bold);
            if (e.CellElement is GridGroupContentCellElement)
            {
                e.CellElement.Font = bigFont;
            }
            else if (e.CellElement is GridDataCellElement)
            {
                e.CellElement.Font = normalFont;
            }
        }
        private void radGridView3_SelectionChanging(object sender, GridViewSelectionCancelEventArgs e)
        {
            radPropertyGrid1.SelectedObject = e.Rows[0].Tag;
        }
        private static Cable CreateCableFromDB(SQLiteDataReader reader)
        {
            var cable = new Cable
            {
                Description = reader[4].ToString(),
                ToPosName = reader[0].ToString(),
                ToGUID = reader[1].ToString(),
                ToBlockName = reader[3].ToString(),
                WriteBlock = Convert.ToBoolean(reader[5].ToString()),
                CableType = reader[8].ToString(),
                CompTable = reader[13].ToString(),
                SortPriority = reader[10].ToString(),
                cableGUID = reader[11].ToString(),
                CableName = reader[7].ToString(),
                WireNumbers = Convert.ToInt32(reader[12].ToString()),
                HostTable = reader[13].ToString()
            };
            double.TryParse(reader[6].ToString(), out double cablenght);
            string attribute = reader[9].ToString();
            cable.Lenght = cablenght;
            var pinfo = cable.GetType().GetProperty("Attrubute");
            pinfo?.SetValue(cable, Enum.Parse(pinfo.PropertyType, attribute));
            return cable;
        }
        private void radButton13_ToolTipTextNeeded(object sender, ToolTipTextNeededEventArgs e)
        {
            e.ToolTipText = "Запуск генератора схемы автоматизации";
        }
        private void radGridView3_CellEndEdit(object sender, GridViewCellEventArgs e)
        {            
            string column = e.Column.HeaderText;
            Cable cable = (Cable)radGridView3.CurrentRow.Tag;
            string query = string.Empty;
            switch (column)
            {
                case "Тип":
                    cable.CableType = e.Value.ToString();
                    query = $"UPDATE Cable SET CableType = '{cable.CableType}' WHERE GUID = '{cable.cableGUID}'";
                    break;
                case "Длина":
                    double.TryParse(e.Value.ToString(), out var lenght);
                    cable.Lenght = lenght;
                    query = $"UPDATE Cable SET CableLenght = '{cable.Lenght}' WHERE GUID = '{cable.cableGUID}'";
                    break;
            }
            try
            {
                if (connection.State == ConnectionState.Open)
                {
                    SQLiteCommand command = new SQLiteCommand { Connection = connection, CommandText = query };
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
            }
            catch
            {
                MessageBox.Show(@"Не смог обновить длины, ошибка в типе кабеля");
            }            
            radPropertyGrid1.SelectedObject = cable;
        }
        private void radTreeView4_SelectedNodeChanging(object sender, RadTreeViewCancelEventArgs e)
        {
            if (ModifierKeys == Keys.Shift)
            {
                radTreeView4.MultiSelect = false;
                e.Cancel = true;
            }

            if (ModifierKeys != Keys.Control) return;
            radTreeView4.MultiSelect = true;
            radTreeView4.Nodes.AsParallel().ForAll(radTreeNode =>
            {
                if (radTreeNode.Nodes.Count > 0)
                {
                    foreach (RadTreeNode child in radTreeNode.Nodes)
                    {
                        child.Selected = false;
                    }
                }
                else
                {
                    radTreeNode.Selected = false;
                }
            });
            if (e.Node.Level == 0 && e.Node.Nodes.Count>0)
            {
                e.Node.Nodes.AsParallel().ForAll(child =>
                {
                    //MessageBox.Show(e.Node.Text);
                    child.Selected = false;
                });
                e.Cancel = false;
                    
            }
            else
            {
                e.Cancel = true;
            }
        }
        //private dynamic DynamicReader<T>(dynamic connnection)
        //{
        //    if (typeof(T) == typeof(SQLiteDataReader))
        //    {
        //        SQLiteDataReader SQLiteDataReader =null;
        //        return SQLiteDataReader;
        //    }
        //    if (typeof(T) == typeof(SQLiteConnection))
        //    {
        //        SQLiteDataReader sQLiteDataReader = null;
        //        return sQLiteDataReader;
        //    }
        //    return null;
        //}        
        private Point LastLocation;
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            LastLocation = e.Location;
        }
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!mouseDown) return;
            Location = new Point(
                (Location.X - LastLocation.X) + e.X, 
                (Location.Y - LastLocation.Y) + e.Y);
            Update();
        }
        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private string getPosName(string guid, string from)
        {
            string retval = string.Empty;
            string guery = $"SELECT PosName FROM {from} WHERE GUID = '{guid}'";
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection,
                CommandText = guery
            };
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (!string.IsNullOrEmpty(reader[0].ToString())) retval = reader[0].ToString();
                    
                }
            }
            command.Dispose();
            return retval;
        }

        private string getPosNameSlave(string guid, string from)
        {
            string retval = string.Empty;
            string guery = $"SELECT PosName FROM {from} WHERE ElementGUID = '{guid}'";
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection,
                CommandText = guery
            };
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (!string.IsNullOrEmpty(reader[0].ToString())) retval = reader[0].ToString();
                }
            }
            command.Dispose();
            return retval;
        }

        private (string, string) getPosNameByDescription (string Description, string from, string guid)
        {
            string posname = string.Empty;
            string elementGUID = string.Empty;
            string guery = $"SELECT PosName, GUID FROM {from} WHERE ElementGUID = '{guid}' AND Description = '{Description}'";
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection,
                CommandText = guery
            };
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (!string.IsNullOrEmpty(reader[0].ToString())) posname = reader[0].ToString();
                    if (!string.IsNullOrEmpty(reader[1].ToString())) elementGUID = reader[1].ToString();
                }
            }
            command.Dispose();
            (string, string) retval = (posname, elementGUID);            
            return retval;
        }
        private void radCollapsiblePanel1_Collapsed(object sender, EventArgs e)
        {
            radPageView1.Width = 1271;
            radRibbonBarGroup1.Enabled = false;
        }
        private void radCollapsiblePanel1_Expanding(object sender, CancelEventArgs e)
        {
            radPageView1.Width = 1068;
        }
        private void radCollapsiblePanel1_Expanded(object sender, EventArgs e)
        {
            radRibbonBarGroup1.Enabled = true;
        }
        private void ribbonTab2_Click(object sender, EventArgs e)
        {
            radPageView1.SelectedPage = radPageViewPage1;
            if (radCollapsiblePanel1.IsExpanded) radCollapsiblePanel1.Collapse();
            radCollapsiblePanel1.Collapse();
            radPropertyGrid1.Enabled = true;
            //this.radPropertyGrid1.SelectedObject = null;
        }
        private void ribbonTab3_Click(object sender, EventArgs e)
        {
            if (radCollapsiblePanel1.IsExpanded) radCollapsiblePanel1.Collapse();
            radPageView1.SelectedPage = radPageViewPage2;
            radPropertyGrid1.Enabled = true;
            //this.radPropertyGrid1.SelectedObject = null;
            ReadJoinedSystems2();
        }
        private void ribbonTab4_Click(object sender, EventArgs e)
        {
            if (radCollapsiblePanel1.IsExpanded) radCollapsiblePanel1.Collapse();
            radPageView1.SelectedPage = radPageViewPage3;
            radPropertyGrid1.Enabled = false;
            //this.radPropertyGrid1.SelectedObject = null;
            ReadJoinedSystems2();
        }
        private void ribbonTab1_Click(object sender, EventArgs e)
        {
            radPageView1.SelectedPage = radPageViewPage1;
            radCollapsiblePanel1.Expand();
            radPropertyGrid1.Enabled = true;
        }
        private void radButtonElement11_Click(object sender, EventArgs e)
        {
            List<RadTreeNode> selectednodes = radTreeView4.Nodes
                .Where(selnode => selnode.Level == 0 && selnode.Nodes.Count > 0)
                .ToList();
            SelectPannelForm selectPannelForm = new SelectPannelForm(selectednodes, connection, DBFilePath);
            DialogResult dialogResult = selectPannelForm.ShowDialog();
            if (dialogResult != DialogResult.OK) return;
            if (selectPannelForm.CheckedItems.Count <= 0) return;
            Dictionary<string, Pannel> CheckedPannels = selectPannelForm.CheckedItems; //забираем словарь со шкафами из формы
            Dictionary<Pannel, List<(string, VentSystem)>> VentSystemS = new Dictionary<Pannel, List<(string, VentSystem)>>();

            foreach (RadTreeNode radTreeNode in radTreeView4.Nodes)//перебираем ноды со шкафами
            {
                Pannel pannel = (Pannel)radTreeNode.Tag;
                if (!CheckedPannels.ContainsKey(pannel.PannelName)) continue;
                RadTreeNodeCollection VentSystemArr = radTreeNode.Nodes;
                if (VentSystemArr.Count <= 0) continue;
                List<(string, VentSystem)> systemList = new List<(string, VentSystem)>();
                foreach (var item in VentSystemArr)
                {
                    VentSystem ventSystem = (VentSystem)item.Tag;
                    if (ventSystem == null) continue;
                    List<object> ventobjects = ventSystem.ComponentsV2
                        .Where(pos => pos.Tag != null)
                        .Select(pos=> pos.Tag)
                        .ToList();
                                        
                    if (connection.State == ConnectionState.Open)
                    {
                       
                    }
                    string objname;
                    
                    ventobjects
                        .AsParallel()
                        .ForAll(component =>
                        {
                            objname = component.GetType().Name;
                            switch (objname)
                            {
                                case nameof(OutdoorTemp):
                                    OutdoorTemp outdoorTemp = (OutdoorTemp)component;
                                    outdoorTemp.PosName = getPosName(outdoorTemp.GUID, "SensT");
                                    break;
                                case nameof(SupplyDamper):
                                    SupplyDamper supplyDamper = (SupplyDamper)component;
                                    supplyDamper.PosName = getPosName(supplyDamper.GUID, "Damper");
                                    if (supplyDamper.outdoorTemp != null)
                                    {
                                        supplyDamper.outdoorTemp.PosName = getPosName(supplyDamper.outdoorTemp.GUID, "SensT");
                                    }
                                    break;
                                case nameof(CrossSection):
                                case nameof(Room):
                                    dynamic senshost = component;
                                    if (senshost._SensorT !=null)
                                    {
                                        senshost._SensorT.PosName = getPosName(senshost._SensorT.GUID, "SensT");
                                    }
                                    if (senshost._SensorH != null)
                                    {
                                        senshost._SensorH.PosName = getPosName(senshost._SensorH.GUID, "SensHum");
                                    }
                                    break;
                                case nameof(ExtDamper):
                                    ExtDamper extDamper = (ExtDamper)component;
                                    extDamper.PosName = getPosName(extDamper.GUID, "Damper");
                                    break;
                                case nameof(SupplyFiltr):
                                case nameof(ExtFiltr):
                                case nameof(Filtr):
                                                   
                                    dynamic comp = component;
                                    if (comp._PressureContol != null)
                                    {
                                        PressureContol pressureContol = comp._PressureContol;
                                        pressureContol.PosName = getPosName(pressureContol.GUID, "SensPDS");
                                    }
                                    break;

                                               
                                case nameof(Recuperator):
                                    Recuperator recuperator = (Recuperator)component;
                                    (string PosName, string GUID) tuple1;
                                    (string PosName, string GUID) tuple2;
                                    switch (recuperator._RecuperatorType)
                                    {
                                        case Recuperator.RecuperatorType.Recirculation:
                                            tuple1 = getPosNameByDescription(recuperator.Drive1?.Description, "Valve", recuperator.GUID);
                                            tuple2 = getPosNameByDescription(recuperator.Drive2?.Description, "Valve", recuperator.GUID);
                                            (string PosName, string GUID) = getPosNameByDescription(recuperator.Drive3?.Description, "Valve", recuperator.GUID);
                                            if (recuperator.Drive1 != null)
                                            {
                                                recuperator.Drive1.Posname = tuple1.PosName;
                                                recuperator.Drive1.GUID = tuple1.GUID;
                                            }

                                            if (recuperator.Drive2 != null)
                                            {
                                                recuperator.Drive2.Posname = tuple2.PosName;
                                                recuperator.Drive2.GUID = tuple2.GUID;
                                            }

                                            if (recuperator.Drive3 != null)
                                            {
                                                recuperator.Drive3.Posname = PosName;
                                                recuperator.Drive3.GUID = GUID;
                                            }

                                            break;
                                        case Recuperator.RecuperatorType.RotorControl:
                                            recuperator.Drive1.Posname = getPosNameSlave(recuperator.GUID, "Valve");
                                            recuperator.protectSensor2.PosName = getPosName(recuperator.protectSensor2.GUID, "SensPDS");
                                            break;
                                        case Recuperator.RecuperatorType.LaminatedNoBypass:
                                            recuperator.protectSensor2.PosName = getPosName(recuperator.protectSensor2.GUID, "SensPDS");
                                            break;
                                        case Recuperator.RecuperatorType.LaminatedBypass:
                                            recuperator.protectSensor1.PosName = getPosNameSlave(recuperator.GUID, "SensT");
                                            recuperator.protectSensor2.PosName = getPosNameSlave(recuperator.GUID, "SensPDS");
                                            recuperator.Drive1.Posname = getPosNameSlave(recuperator.GUID, "Valve");

                                            break;
                                        case Recuperator.RecuperatorType.Glycol:
                                            tuple1 = getPosNameByDescription(recuperator.Drive1?.Description, "Valve", recuperator.GUID);
                                            tuple2 = getPosNameByDescription(recuperator.Drive2?.Description, "Valve", recuperator.GUID);
                                            recuperator.protectSensor2.PosName = getPosName(recuperator.protectSensor2.GUID, "SensPDS");
                                            if (recuperator.Drive1 != null)
                                            {
                                                recuperator.Drive1.Posname = tuple1.PosName;
                                                recuperator.Drive1.GUID = tuple1.GUID;
                                            }

                                            if (recuperator.Drive2 != null)
                                            {
                                                recuperator.Drive2.Posname = tuple2.PosName;
                                                recuperator.Drive2.GUID = tuple2.GUID;
                                            }

                                            break;
                                    }
                                    break;
                                case nameof(SupplyVent):
                                    SupplyVent supplyVent = (SupplyVent)component;
                                    supplyVent.PosName = getPosName(supplyVent.GUID, "Ventilator");
                                    if (supplyVent._PressureContol != null)
                                    {
                                        PressureContol pressureContol = supplyVent._PressureContol;
                                        pressureContol.PosName = getPosNameSlave(supplyVent.GUID, "SensPDS");
                                    }
                                    break;
                                case nameof(ExtVent):
                                    ExtVent extVent = (ExtVent)component;
                                    extVent.PosName = getPosName(extVent.GUID, "Ventilator");
                                    if (extVent._PressureContol != null)
                                    {
                                        PressureContol pressureContol = extVent._PressureContol;
                                        pressureContol.PosName = getPosNameSlave(extVent.GUID, "SensPDS");
                                    }

                                    break;
                                case nameof(SpareSuplyVent):
                                    
                                    SpareSuplyVent spareSuplyVent = (SpareSuplyVent) component;
                                    SupplyVent mainSupplyVent = spareSuplyVent.MainSupplyVent;
                                    SupplyVent reservedSupplyVent = spareSuplyVent.ReservedSupplyVent;
                                    mainSupplyVent.PosName = getPosName(mainSupplyVent.GUID, "Ventilator");
                                    reservedSupplyVent.PosName = getPosName(reservedSupplyVent.GUID, "Ventilator");
                                    if (spareSuplyVent._PressureContol != null)
                                    {
                                        PressureContol pressureContol = spareSuplyVent._PressureContol;
                                        pressureContol.PosName = getPosNameSlave(spareSuplyVent.GUID, "SensPDS");
                                    }
                                    break;
                                case nameof(SpareExtVent):

                                    SpareExtVent spareExtVent= (SpareExtVent)component;
                                    ExtVent mainExtVent = spareExtVent.MainExtVent;
                                    ExtVent reservrdExtVent = spareExtVent.ReservedExtVent;
                                    mainExtVent.PosName = getPosName(mainExtVent.GUID, "Ventilator");
                                    reservrdExtVent.PosName = getPosName(reservrdExtVent.GUID, "Ventilator");
                                    if (spareExtVent._PressureContol != null)
                                    {
                                        PressureContol pressureContol = spareExtVent._PressureContol;
                                        pressureContol.PosName = getPosNameSlave(spareExtVent.GUID, "SensPDS");
                                    }
                                    break;

                                case nameof(WaterHeater):
                                    WaterHeater waterHeater = (WaterHeater)component;
                                    waterHeater._Pump.PosName = getPosName(waterHeater._Pump.GUID, "Pump");
                                    waterHeater._Valve.Posname = getPosName(waterHeater._Valve.GUID, "Valve");
                                    if (waterHeater.PS2 != null)
                                    {
                                        SensorT sensorPS2 = waterHeater.PS2;
                                        sensorPS2.PosName = getPosName(sensorPS2.GUID, "SensT");
                                    }
                                    if (waterHeater.PS1 != null)
                                    {
                                        SensorT sensorPS1 = waterHeater.PS1;
                                        sensorPS1.PosName = getPosName(sensorPS1.GUID, "SensT");
                                    }
                                    break;
                                case nameof(ElectroHeater):
                                    ElectroHeater electroHeater = (ElectroHeater)component;
                                    electroHeater.PosName = getPosName(electroHeater.GUID, "ElectroHeater");
                                    break;
                                case nameof(Froster):
                                    Froster froster = (Froster)component;
                                    switch (froster._FrosterType)
                                    {
                                        case Froster.FrosterType.Freon:
                                            break;
                                        case Froster.FrosterType.Water:
                                            froster._Valve.Posname = getPosName(froster._Valve.GUID, "Valve");
                                            break;
                                    }

                                    if (froster.Sens1 != null)
                                    {
                                        Froster.ProtectSensor PS1 = froster.Sens1;
                                        PS1.PosName = getPosName(PS1.GUID, "SensT");

                                    }
                                    if (froster.Sens2 != null)
                                    {
                                        Froster.ProtectSensor PS2 = froster.Sens2;
                                        PS2.PosName = getPosName(PS2.GUID, "SensT");

                                    }
                                    break;
                                case nameof(Humidifier):
                                    Humidifier humidifier = (Humidifier)component;

                                    if (humidifier.HumSensPresent)
                                    {

                                        Humidifier.HumiditySens humiditySens = humidifier.HumiditySensor;
                                        humidifier.SensPosName = getPosNameSlave(humidifier.GUID, "SensHum");
                                        //humiditySens.PosName = getPosNameSlave(humidifier.GUID, "SensHum");

                                    }

                                    break;
                                case nameof(SupplyTemp):
                                    SupplyTemp supplyTemp = (SupplyTemp)component;
                                    supplyTemp.PosName = getPosName(supplyTemp.GUID, "SensT");

                                    break;
                                case nameof(ExhaustTemp):
                                    ExhaustTemp exhaustTemp = (ExhaustTemp)component;
                                    exhaustTemp.PosName = getPosName(exhaustTemp.GUID, "SensT");

                                    break;
                                case nameof(IndoorTemp):
                                    IndoorTemp indoorTemp = (IndoorTemp)component;
                                    indoorTemp.PosName = getPosName(indoorTemp.GUID, "SensT");
                                    break;

                            }

                        });
                    systemList.Add((ventSystem.GUID, ventSystem));
                }

                if (!VentSystemS.ContainsKey(pannel))
                {
                    VentSystemS.Add(pannel, systemList);
                }
            }
            ShemaASUTP shema = new ShemaASUTP
            {
                VentSystemS = VentSystemS,
                Author = Author
                        
            };
            shema.Execute();
            shema = null;
        }
        private void radButtonElement10_Click(object sender, EventArgs e)
        {
            List<RadTreeNode> selectednodes = new List<RadTreeNode>();
            radTreeView4.Nodes.AsParallel().ForAll(selnode =>
            {
                if (selnode.Level == 0 && selnode.Nodes.Count > 0) selectednodes.Add(selnode);
            });
            SelectPannelForm selectPannelForm = new SelectPannelForm(selectednodes, connection, DBFilePath);
            DialogResult dialogResult = selectPannelForm.ShowDialog();
            if (dialogResult != DialogResult.OK) return;
            if (selectPannelForm.CheckedItems.Count > 0) //если выбрны шкафы
            {
                Dictionary<string, Pannel> CheckedPannels = selectPannelForm.CheckedItems; //забираем словарь со шкафами из формы
                Dictionary<string, Dictionary<string, Dictionary<string, List<Cable>>>> level3 = new Dictionary<string, Dictionary<string, Dictionary<string, List<Cable>>>>();
                Dictionary<string, VentSystem> ventsystemDict = new Dictionary<string, VentSystem>(); //создаем словарь для вент.систем
                bool writepumpcable, writevalvecable;
                var writecabeP = writepumpcable = writevalvecable = false;
                bool[] cabsettings = new bool[3];
                bool connectionopen = connection.State == ConnectionState.Open;
                switch (connectionopen)
                {
                    case false:
                        connection = OpenDB(DBFilePath);
                        break;
                    case true:
                    {
                        foreach (RadTreeNode radTreeNode in radTreeView4.Nodes)//перебираем ноды со шкафами
                        {
                            Pannel pannel = (Pannel)radTreeNode.Tag;
                            if (!CheckedPannels.ContainsKey(pannel.PannelName)) continue;
                            SQLiteCommand command = new SQLiteCommand { Connection = connection };
                            Dictionary<string, Dictionary<string, List<Cable>>> level2 = new Dictionary<string, Dictionary<string, List<Cable>>>();
                            string querySelectvents = $"SELECT VentSystems.GUID, VentSystems.SystemName FROM VentSystems WHERE VentSystems.Pannel = '{pannel.GetGUID()}'";
                            Dictionary<string, string> ventinpannel = new Dictionary<string, string>();
                            command.CommandText = querySelectvents;
                            using (SQLiteDataReader selectventsReader = command.ExecuteReader())
                            {
                                while (selectventsReader.Read())
                                {

                                    ventinpannel.Add(selectventsReader[0].ToString(), selectventsReader[1].ToString());
                                }
                                selectventsReader.Close();
                            }
                            foreach (RadTreeNode node in radTreeView2.Nodes) //перебираем ноды с вент.системами
                            {
                                VentSystem ventSystem = (VentSystem)node.Tag;
                                if (ventinpannel.ContainsKey(ventSystem.GUID))
                                {
                                    if (!ventsystemDict.ContainsKey(ventSystem.SystemName)) ventsystemDict.Add(ventSystem.SystemName, ventSystem);
                                    string Query = "SELECT Count(*) AS Cnt, Cable.ToGUID, Cable.TableForSearch, [To] " +
                                                   $"FROM Cable WHERE(((Cable.SystemGUID) = '{ventSystem.GUID}')) " +
                                                   "GROUP BY Cable.ToGUID, Cable.TableForSearch, [To] " +
                                                   "ORDER BY Cable.SortPriority ASC, [To] ASC";
                                    command.CommandText = Query;
                                    Dictionary<string, List<Cable>> level1 = new Dictionary<string, List<Cable>>();
                                    using (SQLiteDataReader reader = command.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            string ToGUID = reader[1].ToString();
                                            string to = reader[3].ToString();
                                            //string comptable = reader[2].ToString();
                                            List<Cable> level0 = new List<Cable>();
                                            string Query2 = $"SELECT [To], Cable.ToGUID, Cable.CableType, Cable.BlockName, Cable.Description, Cable.WriteBlock, Cable.CableLenght, Cable.CableName, Cable.CableType, Cable.CableAttribute, Cable.SortPriority, Cable.GUID, WireNumbers, Cable.TableForSearch FROM Cable WHERE(((Cable.ToGUID) = '{ToGUID}')); ";
                                            SQLiteCommand command1 = new SQLiteCommand { CommandText = Query2, Connection = connection };
                                            command1.CommandText = Query2;
                                            try
                                            {
                                                using (SQLiteDataReader reader1 = command1.ExecuteReader())
                                                {
                                                    while (reader1.Read())
                                                    {
                                                        Cable cable = CreateCableFromDB(reader1);
                                                        level0.Add(cable);
                                                    }
                                                    if (!level1.ContainsKey(to))
                                                    {
                                                        level1.Add(to, level0);
                                                    }
                                                    else
                                                    {
                                                        throw new ElementExistException(to, ventSystem.SystemName);

                                                    }

                                                }
                                            }
                                            catch (ElementExistException ex)
                                            {
                                                MessageBox.Show(ex.Message);
                                            }

                                            command1.Dispose();
                                        }
                                        level2.Add(ventSystem.SystemName, level1);
                                    }
                                }
                                if (!level3.ContainsKey(pannel.PannelName)) level3.Add(pannel.PannelName, level2);

                            }
                                
                            //TableExternalConnections table = new TableExternalConnections(level2, ventsystemDict, cabsettings, cabs, pannel);
                            //int result = table.Execute();
                            //if (result == 1)
                            //{
                            //    this.TopMost = true;
                            //    MessageBox.Show("Что-то пошло не так");
                            //    this.TopMost = false;
                            //}


                        }
                        Building building = radTreeView1.SelectedNode.Tag as Building;
                        Project project = radTreeView1.SelectedNode.Parent.Tag as Project;
                        string readwritecableP = $"SELECT WriteCableP, WritePumpCable, WriteValveCable FROM BuildSetting WHERE Place = '{building?.BuildGUID}'";
                        SQLiteCommand command0 = new SQLiteCommand { Connection = connection, CommandText = readwritecableP };
                        using (SQLiteDataReader reader2 = command0.ExecuteReader())
                        {
                            while (reader2.Read())
                            {
                                writecabeP = Convert.ToBoolean(reader2[0].ToString());
                                writepumpcable = Convert.ToBoolean(reader2[1].ToString());
                                writevalvecable = Convert.ToBoolean(reader2[2].ToString());
                            }
                            cabsettings[0] = writecabeP;
                            cabsettings[1] = writepumpcable;
                            cabsettings[2] = writevalvecable;

                        }
                        command0.Dispose();
                        //foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, List<Cable>>>> keyValuePair1 in level3)
                        //{

                        //    MessageBox.Show(keyValuePair1.Key);
                        //    List<Cable> cablelist = cabsdic[keyValuePair1.Key];
                        //    MessageBox.Show(cablelist.Count.ToString());
                        //    Dictionary<string, Dictionary<string, List<Cable>>> dic1 = keyValuePair1.Value;
                        //    foreach (KeyValuePair<string, Dictionary<string, List<Cable>>> ventsystem in dic1)
                        //    {
                        //        //MessageBox.Show(ventsystem.Key);
                        //        Dictionary<string, List<Cable>> components = ventsystem.Value;
                        //        foreach (KeyValuePair<string, List<Cable>> compname in components)
                        //        {
                        //           // MessageBox.Show(compname.Key);
                        //            List<Cable> cables = compname.Value;
                        //        }
                        //    }
                        //}
                        var SortedDict = level3.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value);
                        TableExternalConnections table = new TableExternalConnections(SortedDict, ventsystemDict, cabsettings, CheckedPannels, tableExtDocName) 
                        {
                            Author = Author, 
                            BuildingName = building.Buildname, 
                            Project = project.ProjectName 
                        };
                        
                        int result = table.Execute();
                       
                        if (result == 1)
                        {
                            TopMost = true;
                            MessageBox.Show(@"Что-то пошло не так");
                            TopMost = false;
                        }
                        else
                        {
                            tableExtDocName = table.TableExternalConnectionDocName;
                            table = null;
                        }

                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show(@"Не выбраны шкафы для генерации схемы");
            }
        }
        private void radButtonElement1_Click(object sender, EventArgs e)
        {
            CreateProject();
        }
        private void radButtonElement8_Click(object sender, EventArgs e)
        {
            try
            {

                if (radTreeView2.SelectedNode != null)
                {
                    if (!(radTreeView2.SelectedNode.Tag is VentSystem ventSystem)) return;
                    string message = $"Сейчас будет полностью удалена вент.система {ventSystem.SystemName} \nбез возможности восстановления";
                    const string caption = "Удаление вентсистемы";
                    const MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                    var result = MessageBox.Show(message, caption, buttons);
                    if (result != DialogResult.Yes) return;
                    if (connection.State == ConnectionState.Closed) connection.Open();
                            
                    DeleVentSystemFromDB(ventSystem);

                    SQLiteCommand command = new SQLiteCommand
                    {
                        Connection = connection
                    };
                    string[] udatePannels = {
                        "VentSystemName", "SystemGUID"
                    };

                    foreach (string pannel in udatePannels)
                    {
                        command.CommandText = $"UPDATE Pannel Set {pannel} = '{string.Empty}' WHERE SystemGUID = '{ventSystem.GUID}'";
                        command.ExecuteNonQuery();
                    }
                    command.Dispose();

                    radTreeView2.Nodes.Remove(radTreeView2.SelectedNode);
                    radTreeView2.Update();
                    //radGridView.Rows.Clear();
                            
                    while (radGridView1.Rows.Count > 0)
                    {
                        radGridView1.Rows.RemoveAt(radGridView1.Rows.Count - 1);
                    }
                    while (radGridView4.Rows.Count > 0)
                    {
                        radGridView4.Rows.RemoveAt(radGridView4.Rows.Count - 1);
                    }
                    radGridView1.Columns?.Clear();
                    radGridView4.Columns?.Clear();
                }

                else
                {
                    MessageBox.Show(@"Выберите удаляемую систему");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }
        }
        internal void DeleVentSystemFromDB(VentSystem ventSystem)
        {
            if (connection.State == ConnectionState.Closed) connection.Open();
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection
            };
            string[] tables = 
            {
                "Ventilator", "Filter", "SensPDS", "Cable", "Damper", "WaterHeater", "ElectroHeater",
                "Pump", "Valve", "SensT", "Humidifier", "SensHum", "Recuperator", "Froster", "KKB", 
                "PosInfo", "CrossConnection", "Room", "SpareVentilator"
            };
            string deleteventsystem = $"DELETE FROM VentSystems WHERE [GUID] = '{ventSystem.GUID}'";
            command.CommandText = deleteventsystem;
            command.ExecuteNonQuery();
            foreach (string table in tables)
            {

                command.CommandText = $"DELETE FROM {table} WHERE [SystemGUID] = '{ventSystem.GUID}'";
                command.ExecuteNonQuery();
            }
            command.Dispose();
        }
        private void radButtonElement5_Click(object sender, EventArgs e)
        {
            try
            {
                if (radTreeView1.SelectedNode?.Parent == null) return;
                RadTreeNode buildnode = radTreeView1.SelectedNode;
                Building building = buildnode.Tag as Building;
                DataTable dataSource = radGridView2.DataSource as DataTable;
                CreatePannel pannelform = new CreatePannel(this, buildnode, Author)
                {
                    BUILDGUID = building?.BuildGUID,
                    DBFilePath = DBFilePath,
                    dataTable = dataSource,
                    RadGridView = radGridView2,
                    RadPropertyGrid = radPropertyGrid1,
                    RadTreeView = radTreeView1

                };
                pannelform.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }
        private void radButtonElement6_Click(object sender, EventArgs e)
        {
            DeletePannelFromDB();
        }
        private void ribbonTab5_Click(object sender, EventArgs e)
        {
            
            if (radCollapsiblePanel1.IsExpanded) radCollapsiblePanel1.Collapse();
        }
        private void radButtonElement7_Click(object sender, EventArgs e)
        {
            
            RadTreeNode radTreeNode = radTreeView1.SelectedNode;
            if (radTreeNode == null) return;
            if (radTreeNode.Tag is Building building)
            {
                RadTreeNode parent = radTreeNode.Parent;
                if (!(parent.Tag is Project project)) return;
                EditorV2 editor = new EditorV2(this)
                {
                    Opacity = 0,
                    DBFilePath = DBFilePath,
                    Ventree = radTreeView2,
                    Projecttree = radTreeView1,
                    projectGuid = project.GetGUID(),
                    connection = connection,
                    Building = building,
                    Author = Author,
                    Project = project
                };
                _  = editor.ShowDialog();
                
                
                //GC.Collect(0, GCCollectionMode.Forced);

            }
            else
            {
                MessageBox.Show(@"Выберите в дереве проектов здание, для которого предназначениа вентсистема");
            }
        }
        private void radButtonElement2_Click(object sender, EventArgs e)
        {
            try
            {
                if (radTreeView1.SelectedNode == null) return;
                RadTreeNode selectednode = radTreeView1.SelectedNode;
                if (selectednode.Parent != null) return;
                int cnt = 0;
                string ProjectGUID = string.Empty;
                Project project = selectednode.Tag as Project;
                if (project != null) ProjectGUID = project.GetGUID();

                //foreach (RadTreeNode childenode in selectednode.Nodes)
                selectednode.Nodes.AsParallel().ForAll(childenode =>
                {
                    if (childenode != null) cnt++;
                });
                cnt++;
                string buildingname = $"Здание {cnt}";
                var guid = Guid.NewGuid().ToString();
                RadTreeNode buildingnode = new RadTreeNode(buildingname)
                {
                    Value = buildingname
                };
                CreateBuildingClass(buildingname);
                Building building = CreateBuildingClass(buildingname);
                building.BuildGUID = guid;
                buildingnode.Tag = building;
                selectednode.Nodes.Add(buildingnode);
                selectednode.Expand();


                try
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        string InsertQuery = "INSERT INTO Buildings ([GUID], BuildName, [Project], Buildnum, Address) " +
                                             $"VALUES('{guid}','{buildingname}','{ProjectGUID}','{building.BuildNum}','{building.Address}')";

                        SQLiteCommand command = new SQLiteCommand(InsertQuery, connection);
                        command.ExecuteNonQuery();
                        command.Dispose();
                    }


                    CableSettings cableSettings = new CableSettings
                    {
                        DBFilePath = DBFilePath,
                        Project = project,
                        Building = building
                    };
                    cableSettings.ShowDialog();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.StackTrace + " " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }
        private void radButtonElement3_Click(object sender, EventArgs e)
        {
            if (radTreeView1.SelectedNode.Parent == null)
            {
                DeleteProjectFromDB();
            }
            else
            {
                DeleteBuildingFromDB();
            }
        }
        private void radButtonElement12_Click(object sender, EventArgs e)
        {
            if (radTreeView1.SelectedNode.Tag is Building building)
            {
                const string message = "Учитывать кабели ЭОМ?";
                const string caption = "Сделайте свой выбор";
                var result = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                bool UseEOMCable = result == DialogResult.Yes;
                string SelectPannelGUIDs = $"SELECT GUID, PannelName FROM Pannel WHERE Place = '{building.BuildGUID}' ORDER BY PannelName ASC";
                string UsePowerCables = $"SELECT WriteCableP, WritePumpCable, WriteValveCable FROM BuildSetting WHERE Place = '{building.BuildGUID}'";
                SQLiteCommand getpannelGUIDsCommand = new SQLiteCommand
                {
                    CommandText = SelectPannelGUIDs,
                    Connection = connection
                };
                List<(string, string)> PannelList = new List<(string, string)>();
                using (SQLiteDataReader PannelReader = getpannelGUIDsCommand.ExecuteReader())
                {
                    while (PannelReader.Read())
                    {
                        (string PannelGUID, string PannelName) pannels;
                        pannels.PannelGUID = PannelReader[0].ToString();
                        pannels.PannelName = PannelReader[1].ToString();
                        PannelList.Add(pannels);
                    }
                }
                getpannelGUIDsCommand.CommandText = UsePowerCables;
                bool WriteCableValve, WritePowerCables;
                var WriteCablePump = WriteCableValve = WritePowerCables = false;
                using (SQLiteDataReader GetPowerCables = getpannelGUIDsCommand.ExecuteReader())
                {
                    while (GetPowerCables.Read())
                    {
                        WritePowerCables = Convert.ToBoolean(GetPowerCables[0].ToString());
                        WriteCablePump = Convert.ToBoolean(GetPowerCables[1].ToString());
                        WriteCableValve = Convert.ToBoolean(GetPowerCables[2].ToString());

                    }
                }

                getpannelGUIDsCommand.Dispose();
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("Имя");
                dataTable.Columns.Add("Откуда");
                dataTable.Columns.Add("Куда");
                dataTable.Columns.Add("Тип");
                dataTable.Columns.Add("Длина");
                dataTable.Columns.Add("Система");
                dataTable.Columns.Add("Описание");
                dataTable.Columns.Add("Примечание");

                Dictionary<string, int> uniqueCable = new Dictionary<string, int>();
                foreach (var GetCablesCommand in from (string, string) PannelGUID in PannelList
                    select
                        $"Select Cable.GUID, CableName, FromPannel, [To], CableType, CableLenght, SortPriority, TableForSearch, ToGUID, SystemName, Description, CableAttribute, WriteBlock, BlockName, WireNumbers FROM Cable WHERE FromGUID = '{PannelGUID.Item1}' ORDER BY SystemName, [SortPriority], [TO] ASC"
                    into getcablesquery
                    select new SQLiteCommand
                    {
                        CommandText = getcablesquery,
                        Connection = connection

                    })
                    
                {
                    bool hasrows;
                    const string notetext = "По проекту ЭОМ";
                    using (SQLiteDataReader dataReader = GetCablesCommand.ExecuteReader())
                    {
                        hasrows = dataReader.HasRows;
                        while (dataReader.Read())
                        {
                            var Note = string.Empty;
                            var lenght = 0;
                            var cablename = dataReader[1].ToString();
                            var frompannel = dataReader[2].ToString();
                            var to = dataReader[3].ToString();
                            var cabletype = dataReader[4].ToString();
                            if (dataReader[5].ToString() != string.Empty)
                            {
                                lenght = Convert.ToInt32(dataReader[5].ToString());
                            }

                            var tableforsearch = dataReader[7].ToString();
                            var systemname = dataReader[9].ToString();
                            var description = dataReader[10].ToString();
                            var attribute = dataReader[11].ToString();
                            //if (!uniqueCable.ContainsKey(cabletype) && cabletype != string.Empty)
                            //{
                            //    uniqueCable.Add(cabletype, lenght);
                            //}
                            //else if (cabletype != string.Empty)
                            //{
                            //    int currlentgt = uniqueCable[cabletype];
                            //    currlentgt += lenght;
                            //    uniqueCable[cabletype] = currlentgt;
                            //}

                            if (cablename == "*" && !UseEOMCable) continue;

                            DataRow drToAdd = dataTable.NewRow();
                            drToAdd["Имя"] = cablename;
                            drToAdd["Откуда"] = frompannel;
                            drToAdd["Куда"] = to;
                            drToAdd["Тип"] = cabletype;
                            drToAdd["Длина"] = lenght;
                            drToAdd["Система"] = systemname;

                            if (WritePowerCables && attribute == "P")
                            {

                                Note = notetext;
                                switch (tableforsearch)
                                {
                                    case "Pump":
                                        if (!WriteCablePump)
                                        {
                                            Note = string.Empty;
                                            if (!UseEOMCable) continue;
                                        }
                                        break;
                                    case "Valve":
                                    case "Damper":
                                        if (!WriteCableValve)
                                        {
                                            Note = string.Empty;
                                            if (!UseEOMCable) continue;
                                        }
                                        break;
                                    default:
                                        if (!UseEOMCable) continue;
                                        break;

                                }
                            }
                            drToAdd["Описание"] = description;
                            drToAdd["Примечание"] = Note;

                            dataTable.Rows.Add(drToAdd);

                            if (!uniqueCable.ContainsKey(cabletype) && cabletype != string.Empty)
                            {
                                uniqueCable.Add(cabletype, lenght);
                            }
                            else if (cabletype != string.Empty)
                            {
                                int currlentgt = uniqueCable[cabletype];
                                currlentgt += lenght;
                                uniqueCable[cabletype] = currlentgt;
                            }



                        }
                    }
                    GetCablesCommand.Dispose();
                    
                    if (hasrows) dataTable.Rows.Add(dataTable.NewRow());
                }
                DataRow sumheader = dataTable.NewRow();
                sumheader["Тип"] = "Итого по объекту:";
                dataTable.Rows.Add(sumheader);
                foreach (KeyValuePair<string, int> keyValuePair in uniqueCable)
                {
                    DataRow cablesumrow = dataTable.NewRow();

                    cablesumrow["Тип"] = keyValuePair.Key;
                    cablesumrow["Длина"] = keyValuePair.Value;
                    dataTable.Rows.Add(cablesumrow);
                }


                var workbook = new XLWorkbook();

                const string sheetname = "Кабельный журнал";
                //var ws =  workbook.Worksheets.Add(sheetname);
                var ws = workbook.Worksheets.Add(dataTable, sheetname);
                for (int t = 1; t < 9; t++)
                {
                    ws.Column(t).AdjustToContents();
                }

                IXLCells cells5 = ws.Column(5).Cells();
                //set columt5 cells format as number
                foreach (var cell in cells5)
                {
                    try
                    {
                        cell.SetDataType(XLDataType.Number);
                    }
                    catch
                    {
                        // ignored
                    }

                    cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                }
                //set cell Alignment
                int[] columns = { 1, 2, 3, 4, 6 };
                foreach (int t in columns)
                {
                    ColumnAlignment(ws, t, XLAlignmentHorizontalValues.Center);
                }

                SaveXLS(workbook, "Сохранение кабельного журнала в XLS");


            }
            else
            {
                MessageBox.Show(@"Следует выбрать здание");
            }

            //int row = 1;
            //foreach(string s in strlist)
            //{
            //    ws.Cell(row, 1).Value = s;
            //    row++;
            //}
            //workbook.SaveAs(sheetname + ".xlsx");

            //workbook.AddWorksheet(yourDataset) 
            // workbook.AddWorksheet(yourDataTable)
        }
        private static void ColumnAlignment(IXLWorksheet ws, int column, XLAlignmentHorizontalValues val)
        {
           
            
            IXLCells cells = ws.Column(column).Cells();
            foreach (var cell in cells)
            {
                cell.Style.Alignment.SetHorizontal(val);
            }
            
        }

        private static void ColumnAlignment(XLWorkbook workbook, string docname, int column, XLAlignmentHorizontalValues val)
        {
            IXLWorksheet ws = workbook.Worksheet(docname);

            IXLCells cells = ws.Column(column).Cells();
            foreach (var cell in cells)
            {
                cell.Style.Alignment.SetHorizontal(val);
            }

        }
        private void radButtonElement17_Click(object sender, EventArgs e)
        {
            SetCableLenght();
        }
        private void radButtonElement16_Click(object sender, EventArgs e)
        {
            SetCableNames();
        }
        private void radButtonElement18_Click(object sender, EventArgs e)
        {
            SetCabeTypes();
        }
        private void radButtonElement9_Click(object sender, EventArgs e)
        {
            
            
            
            
            RadTreeNode radTreeNode = radTreeView1.SelectedNode;
            if (!(radTreeNode?.Tag is Building building)) return;
            RadTreeNode parent = radTreeNode.Parent;
            if (!(parent.Tag is Project project)) return;
            CableSettings cableSettings = new CableSettings
            {
                DBFilePath = DBFilePath,
                Project = project,
                Building = building
            };
            cableSettings.ShowDialog();
            radGridView3.DataSource = null;
        }
        private static double CurrentCalc(int Power, ElectroDevice._Voltage voltage)
        {
            switch (voltage)
            {
                case ElectroDevice._Voltage.AC380:
                    return Power / (380 * 1.73);
                case ElectroDevice._Voltage.AC220:
                    return Power / 220;
                case ElectroDevice._Voltage.AC24:
                case ElectroDevice._Voltage.DC24:
                    return Power / 24;
            }
            
            return 0;
        }
        private string GetCableFromBD(Cable cable, bool writePump, bool writeVave, bool CableInEOM)
        {
            _ = cable.cableGUID;
            int wirenumbers = cable.WireNumbers;
            string CableType = cable.CableType;
            string power = "0";
            string result = string.Empty;
            string voltagestring = string.Empty;
            //string SelectDev = $"Select [ToGUID], TableForSearch FROM Cable WHERE [GUID] = '{CabGUID}'";
            //SQLiteCommand SearchCommand = new SQLiteCommand
            //{
            //    CommandText = SelectDev,
            //    Connection = connection
            //};


            //string DevTable = string.Empty;
            //string DevGUID = string.Empty;
            //using (SQLiteDataReader reader = SearchCommand.ExecuteReader())
            //{
            //    while (reader.Read())
            //    {
            //        DevGUID = reader[0].ToString();
            //        DevTable = reader[1].ToString();
            //    }
            //}

            string DevGUID = cable.ToGUID;
            string DevTable = cable.HostTable;
            if (string.IsNullOrEmpty(DevGUID) || string.IsNullOrEmpty(DevTable)) return result;
            string PowerQuery = string.Empty; //начало функции
            string VoltageQuery = $"Select Voltage FROM {DevTable} WHERE [GUID] = '{DevGUID}'"; //общий запрос для всех устройств
            switch (DevTable)
            {
                case "Valve":
                        
                    if (CableInEOM && !writeVave) return string.Empty;
                    power = "10"; //тут жестко задается мощность для клапанов
                    voltagestring = "AC24"; //и жестко задается напряжение
                    VoltageQuery = string.Empty;
                       
                    break;
                case "Damper":
                    if (CableInEOM && !writeVave) return string.Empty;
                    power = "3"; //аналогично
                    break;
                case "Pump":
                    if (CableInEOM && !writePump) return string.Empty;
                    power = "300"; //аналогично
                    break;
                case "KKB":
                    PowerQuery = $"SELECT Power FROM Froster WHERE KKB = '{DevGUID}'";
                    VoltageQuery = $"Select Voltage FROM Froster WHERE KKB = '{DevGUID}'"; //перенаправление от ККБ к Froster
                    break;
                default:
                    PowerQuery = $"SELECT Power FROM {DevTable} WHERE [GUID] = '{DevGUID}'";
                    break;

            }
            if (PowerQuery != string.Empty)
            {
                SQLiteCommand SearchPowerCommand = new SQLiteCommand
                {
                    CommandText = PowerQuery,
                    Connection = connection
                };
                    
                using (SQLiteDataReader PowerReader = SearchPowerCommand.ExecuteReader())
                {
                    while (PowerReader.Read())
                    {
                        power = PowerReader[0].ToString();
                    }
                }
            }
            if (VoltageQuery != string.Empty)
            {
                SQLiteCommand VoltageCommand = new SQLiteCommand
                {
                    CommandText = VoltageQuery,
                    Connection = connection
                };
                    
                using (SQLiteDataReader PowerReader = VoltageCommand.ExecuteReader())
                {
                    while (PowerReader.Read())
                    {
                        voltagestring = PowerReader[0].ToString();
                    }
                }
                    

            }

            if (voltagestring == string.Empty) return result;
            {
                int Power = Convert.ToInt32(power);
                Enum.TryParse(voltagestring, out ElectroDevice._Voltage Voltage);
                double Current = 0;
                if (Power != 0) Current = Math.Ceiling(CurrentCalc(Power, Voltage));
                string GetCableByCurrentAndWires = string.Empty;
                switch (Voltage)
                {
                    case ElectroDevice._Voltage.AC380:
                        GetCableByCurrentAndWires = "SELECT CableFullName FROM CableTypes " +
                                                    $"WHERE MaxCurrent380 >= {Current} " +
                                                    $"AND WireNumbers >= {wirenumbers} " +
                                                    $"AND CableType = '{CableType}' " +
                                                    "LIMIT 1";
                        break;
                    case ElectroDevice._Voltage.AC220:
                    case ElectroDevice._Voltage.AC24:
                    case ElectroDevice._Voltage.DC24:
                        GetCableByCurrentAndWires = "SELECT CableFullName FROM CableTypes " +
                                                    $"WHERE MaxCurrent220 >= {Current} " +
                                                    $"AND WireNumbers >= {wirenumbers} " +
                                                    $"AND CableType = '{CableType}' " +
                                                    "LIMIT 1";
                        break;
                }

                if (GetCableByCurrentAndWires == string.Empty) return result;
                SQLiteCommand resultCommand = new SQLiteCommand
                {
                    CommandText = GetCableByCurrentAndWires,
                    Connection = connection
                };
                        
                using (SQLiteDataReader PowerReader = resultCommand.ExecuteReader())
                {
                    while (PowerReader.Read())
                    {
                        result = PowerReader[0].ToString();
                    }
                }
            }
            return result;
        }

        private void radButtonElement15_Click(object sender, EventArgs e)
        {
            if (radTreeView1.SelectedNode.Tag is Building building)
            {
                string SelectPannelGUIDs = $"SELECT GUID, PannelName FROM Pannel WHERE Place = '{building.BuildGUID}' ORDER BY PannelName ASC";

                SQLiteCommand getpannelGUIDsCommand = new SQLiteCommand
                {
                    CommandText = SelectPannelGUIDs,
                    Connection = connection
                };

                Dictionary<string, List<VentSystem>> JoinedSystems = new Dictionary<string, List<VentSystem>>();
                Dictionary<string, (int, int, int, int)> result = new Dictionary<string, (int, int, int, int)>();
                using (SQLiteDataReader PannelReader = getpannelGUIDsCommand.ExecuteReader())
                {
                    while (PannelReader.Read())
                    {
                        string PannelID = PannelReader[0].ToString();
                        if (JoinedSystems.ContainsKey(PannelID)) continue;
                        List<VentSystem> ventSystems = new List<VentSystem>();
                        JoinedSystems.Add(PannelID, ventSystems);

                    }
                }
                if (JoinedSystems.Count == 0)
                {
                    const string message = "В здании нет шкафов управления";
                    const string caption = "Предупреждение";
                    MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
                    return;
                }


                foreach (string key in JoinedSystems.Keys)
                {
                    var SelectVentSystemsByPannel = string.Empty;

                    SelectVentSystemsByPannel = $"SELECT GUID FROM VentSystems WHERE Pannel = '{key}'";
                    SQLiteCommand VentSystemComand = new SQLiteCommand
                    {
                        CommandText = SelectVentSystemsByPannel,
                        Connection = connection
                    };
                    using (SQLiteDataReader ventIDreader = VentSystemComand.ExecuteReader())
                    {
                        string VentIDinPannel;
                        while (ventIDreader.Read())
                        {
                            VentIDinPannel = string.Empty;
                            VentIDinPannel = ventIDreader[0].ToString();
                            if (string.IsNullOrEmpty(VentIDinPannel)) continue;
                            RadTreeNode ventnode = (from RadTreeNode node in radTreeView2.Nodes
                                where node.Name == VentIDinPannel
                                select node).FirstOrDefault();
                            if (ventnode?.Tag == null) continue;
                            VentSystem ventSystem = (VentSystem)ventnode.Tag;
                            JoinedSystems[key].Add(ventSystem);


                        }
                    }
                }
                if (JoinedSystems.Count > 0)
                {
                    foreach (KeyValuePair<string, List<VentSystem>> keyValuePair in JoinedSystems)
                    {
                        int AO;
                        int AI;
                        int DO;
                        var DI = DO = AI = AO = 0;
                        string PannelID = keyValuePair.Key;
                        List<VentSystem> ventSystems = JoinedSystems[PannelID];
                        foreach ((int DI, int DO, int AI, int AO) IO in ventSystems.Select(CalcIO))
                        {
                            DI += IO.DI;
                            DO += IO.DO;
                            AI += IO.AI;
                            AO += IO.AO;
                        }
                        if (!result.ContainsKey(PannelID))
                        {
                            result.Add(PannelID, (DI, DO, AI, AO));
                        }

                    }
                }
                if (result.Count > 0)
                {
                    string message = string.Empty;

                    foreach (KeyValuePair<string, (int DI, int DO, int AI, int AO)> keyValuePair in result)
                    {
                        string DI = keyValuePair.Value.DI.ToString();
                        string DO = keyValuePair.Value.DO.ToString();
                        string AI = keyValuePair.Value.AI.ToString();
                        string AO = keyValuePair.Value.AO.ToString();
                        string PannelID = keyValuePair.Key;
                        RadTreeNode pannelNode = FindNodeByName(PannelID, radTreeView4.Nodes);
                        if (pannelNode != null)
                        {
                            Pannel pannel = (Pannel)pannelNode.Tag;
                            PannelID = pannel.PannelName;
                        }

                        message += PannelID + ": " + $"DI-{DI}; DO-{DO}; AI-{AI}; AO-{AO}\r\n";
                    }
                    IO_Summ iO_SummForm = new IO_Summ(message);
                    iO_SummForm.ShowDialog();
                    //MessageBox.Show(message);
                }

            }
            else
            {
                const string message = "Выберите здание, для которого нужна таблица IO";
                const string caption = "Выбор здания";
                MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
                //Process process = Process.GetCurrentProcess();
                //IntPtr hWnd = process.MainWindowHandle;
                //TableExternalConnections.SetForegroundWindow(hWnd);
            }
        }
        private static (int, int, int, int) CalcIO(VentSystem ventSystem)
        {
            (int DI, int DO, int AI, int AO) IO;
            IO.AI = 0;
            IO.AO = 0;
            IO.DI = 0;
            IO.DO = 0;

            List<object> notNulledComponetns = ventSystem.ComponentsV2
                .Select(e => e.Tag)
                .ToList();
                

            foreach (var component in notNulledComponetns)
            {
                var objname = component?.GetType().Name;
                if (string.IsNullOrEmpty(objname)) continue;
                ShemaASU shemaASU;
                switch (objname)
                {
                    case nameof(OutdoorTemp):
                        OutdoorTemp outdoorTemp = (OutdoorTemp)component;
                        shemaASU = outdoorTemp.ShemaASU;
                        calc(shemaASU);
                            

                        break;
                    case nameof(SupplyDamper):
                        SupplyDamper supplyDamper = (SupplyDamper)component;
                        shemaASU = supplyDamper.ShemaASU;
                        calc(shemaASU);
                        if (supplyDamper.outdoorTemp != null) calc(supplyDamper.outdoorTemp.ShemaASU);
                        break;
                    case nameof(SupplyFiltr):
                        SupplyFiltr supplyFiltr = (SupplyFiltr)component;

                        if (supplyFiltr._PressureContol != null)
                        {
                            PressureContol pressureContol = supplyFiltr._PressureContol;
                            shemaASU = pressureContol.ShemaASU;
                            calc(shemaASU);
                                
                        }
                        break;
                    case nameof(Recuperator):
                        Recuperator recuperator = (Recuperator)component;
                        //calc(shemaASU);
                        switch (recuperator._RecuperatorType)
                        {
                            case Recuperator.RecuperatorType.Recirculation:
                                shemaASU = recuperator.Drive1.ShemaASU;
                                calc(shemaASU);
                                shemaASU = recuperator.Drive2.ShemaASU;
                                calc(shemaASU);
                                shemaASU = recuperator.Drive3.ShemaASU;
                                calc(shemaASU);
                                break;
                            case Recuperator.RecuperatorType.RotorControl:
                                shemaASU = recuperator.Drive1.ShemaASU;
                                calc(shemaASU);
                                shemaASU = recuperator.protectSensor2.ShemaASU;
                                calc(shemaASU);
                                break;
                            case Recuperator.RecuperatorType.LaminatedNoBypass:
                                shemaASU = recuperator.protectSensor2.ShemaASU;
                                calc(shemaASU);
                                break;
                            case Recuperator.RecuperatorType.LaminatedBypass:
                                shemaASU = recuperator.protectSensor1.ShemaASU;
                                calc(shemaASU);
                                shemaASU = recuperator.Drive1.ShemaASU;
                                calc(shemaASU);
                                shemaASU = recuperator.protectSensor2.ShemaASU;
                                calc(shemaASU);
                                break;
                            case Recuperator.RecuperatorType.Glycol:
                                shemaASU = recuperator.Drive1.ShemaASU;
                                calc(shemaASU);
                                shemaASU = recuperator.Drive2.ShemaASU;
                                calc(shemaASU);
                                shemaASU = recuperator.protectSensor2.ShemaASU;
                                calc(shemaASU);
                                break;
                        }
                        break;
                    case nameof(SupplyVent):
                        SupplyVent supplyVent = (SupplyVent)component;
                        shemaASU = supplyVent.ShemaASU;
                        calc(shemaASU);
                        if (supplyVent._PressureContol != null)
                        {
                            PressureContol pressureContol = supplyVent._PressureContol;
                            shemaASU = pressureContol.ShemaASU;
                            calc(shemaASU);                                
                        }
                        break;
                    case nameof(SpareSuplyVent):
                        SpareSuplyVent spareSuplyVent = (SpareSuplyVent) component;
                        shemaASU = spareSuplyVent.MainSupplyVent.ShemaASU;
                        calc(shemaASU);
                        shemaASU = spareSuplyVent.ReservedSupplyVent.ShemaASU;
                        calc(shemaASU);
                        if (spareSuplyVent._PressureContol != null)
                        {
                            PressureContol pressureContol = spareSuplyVent._PressureContol;
                            shemaASU = pressureContol.ShemaASU;
                            calc(shemaASU);
                        }
                        break;
                    case nameof(SpareExtVent):
                        SpareExtVent spareExtVent = (SpareExtVent) component;
                        shemaASU = spareExtVent.MainExtVent.ShemaASU;
                        calc(shemaASU);
                        shemaASU = spareExtVent.ReservedExtVent.ShemaASU;
                        calc(shemaASU);
                        if (spareExtVent._PressureContol != null)
                        {
                            PressureContol pressureContol = spareExtVent._PressureContol;
                            shemaASU = pressureContol.ShemaASU;
                            calc(shemaASU);
                        }
                        break;
                        
                    case nameof(WaterHeater):
                        WaterHeater waterHeater = (WaterHeater)component;
                        shemaASU = waterHeater.ShemaASU;
                        //calc(shemaASU);
                        if (waterHeater._Pump != null)
                        {
                            shemaASU = waterHeater._Pump.ShemaASU;
                            calc(shemaASU);
                        }
                        if (waterHeater._Valve != null)
                        {
                            shemaASU = waterHeater._Valve.ShemaASU;
                            calc(shemaASU);
                        }                            
                        if (waterHeater.PS2 != null)
                        {
                            SensorT sensorPS2 = waterHeater.PS2;
                            shemaASU = sensorPS2.ShemaASU;
                            calc(shemaASU);
                        }
                        if (waterHeater.PS1 != null)
                        {
                            SensorT sensorPS1 = waterHeater.PS1;
                            shemaASU = sensorPS1.ShemaASU;
                            calc(shemaASU);
                        }
                        break;
                    case nameof(ElectroHeater):
                        ElectroHeater electroHeater = (ElectroHeater)component;
                        shemaASU = electroHeater.ShemaASU;
                        calc(shemaASU);
                        break;
                    case nameof(Froster):
                        Froster froster = (Froster)component;
                        shemaASU = froster.ShemaASU;
                        calc(shemaASU);
                        switch (froster._FrosterType)
                        {
                            case Froster.FrosterType.Freon:
                                break;
                            case Froster.FrosterType.Water:
                                shemaASU = froster._Valve.ShemaASU;
                                calc(shemaASU);
                                break;
                        }
                        if (froster.Sens1 != null)
                        {
                            Froster.ProtectSensor PS1 = froster.Sens1;
                            shemaASU = PS1.ShemaASU;
                            calc(shemaASU);
                        }
                        if (froster.Sens2 != null)
                        {
                            Froster.ProtectSensor PS2 = froster.Sens2;
                            shemaASU = PS2.ShemaASU;
                            calc(shemaASU);
                        }
                        break;
                    case nameof(Humidifier):
                        Humidifier humidifier = (Humidifier)component;
                        shemaASU = humidifier.ShemaASU;
                        calc(shemaASU);
                        if (humidifier.HumSensPresent)
                        {

                            //Humidifier.HumiditySens humiditySens = humidifier.HumiditySensor;
                            shemaASU = humidifier.HumiditySensor.ShemaASU;
                            calc(shemaASU);
                        }
                        break;
                    case nameof(SupplyTemp):
                        SupplyTemp supplyTemp = (SupplyTemp)component;
                        shemaASU = supplyTemp.ShemaASU;
                        calc(shemaASU);
                        break;
                    case nameof(IndoorTemp):
                        IndoorTemp indoorTemp = (IndoorTemp)component;
                        shemaASU = indoorTemp.ShemaASU;
                        calc(shemaASU);
                        break;
                    case nameof(ExtDamper):
                        ExtDamper extDamper = (ExtDamper)component;
                        shemaASU = extDamper.ShemaASU;
                        calc(shemaASU);
                        break;
                    case nameof(ExtVent):
                        ExtVent extVent = (ExtVent)component;
                        shemaASU = extVent.ShemaASU;
                        calc(shemaASU);
                        if (extVent._PressureContol != null)
                        {
                            PressureContol pressureContol = extVent._PressureContol;
                            shemaASU = pressureContol.ShemaASU;
                            calc(shemaASU);
                        }
                        break;
                    case nameof(ExhaustTemp):
                        ExhaustTemp exhaustTemp = (ExhaustTemp)component;
                        shemaASU = exhaustTemp.ShemaASU;
                        calc(shemaASU);
                        break;
                    case nameof(ExtFiltr):
                        ExtFiltr extFiltr = (ExtFiltr)component;

                        if (extFiltr._PressureContol != null)
                        {
                            PressureContol pressureContol = extFiltr._PressureContol;
                            shemaASU = pressureContol.ShemaASU;
                            calc(shemaASU);
                        }
                        break;
                    case nameof(CrossSection):
                        CrossSection crossSection = (CrossSection)component;
                        if (crossSection._SensorT != null) calc(crossSection._SensorT.ShemaASU);
                        if (crossSection._SensorH != null) calc(crossSection._SensorH.ShemaASU);
                        break;
                    case nameof(Room):
                        Room room = (Room)component;
                        if (room._SensorT != null) calc(room._SensorT.ShemaASU);
                        if (room._SensorH != null) calc(room._SensorH.ShemaASU);
                        break;
                }
            }
            return IO;
            void calc (ShemaASU shemaASU)
            {
                if (shemaASU.AI) IO.AI += shemaASU.AIcnt;
                if (shemaASU.DI) IO.DI += shemaASU.DIcnt;
                if (shemaASU.AO) IO.AO += shemaASU.AOcnt;
                if (shemaASU.DO) IO.DO += shemaASU.DOcnt;
            }
          
        }
        //private void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        //{
            
        //    if (args.Error == null)
        //    {


        //        if (args.IsUpdateAvailable)
        //        {
        //            timer.Stop();
        //            AutoUpdater.ShowSkipButton = false;
        //            AutoUpdater.ShowRemindLaterButton = false;
        //            AutoUpdater.Mandatory = true;
        //            AutoUpdater.RunUpdateAsAdmin = false;
        //            var curentDirecttory = new DirectoryInfo(Environment.CurrentDirectory);
        //            if (curentDirecttory.Parent != null)
        //            {
        //                AutoUpdater.DownloadPath = Application.StartupPath;
        //                AutoUpdater.InstallationPath = curentDirecttory.FullName;
        //            }
        //            //DialogResult dialogResult;
        //            //if (args.Mandatory.Value)
        //            //{
        //            //    dialogResult =
        //            //        MessageBox.Show(
        //            //            $@"Доступна новая версия {args.CurrentVersion} программы AOVGEN. Вы используете версию {args.InstalledVersion} программы. Требуется обновление", @"Ура! Появилось обновление",
        //            //            MessageBoxButtons.OK,
        //            //            MessageBoxIcon.Information);
        //            //}
        //            //else
        //            //{
        //            //    dialogResult =
        //            //        MessageBox.Show(
        //            //            $@"Доступна новая версия {args.CurrentVersion} программы AOVGEN. Вы используете версию {args.InstalledVersion} программы. Требуется обновление", @"Ура! Появилось обновление",
        //            //            MessageBoxButtons.YesNo,
        //            //            MessageBoxIcon.Information);
        //            //}

        //            //// Uncomment the following line if you want to show standard update dialog instead.
        //            //// AutoUpdater.ShowUpdateForm(args);

        //            //if (dialogResult.Equals(DialogResult.Yes) || dialogResult.Equals(DialogResult.OK))
        //            //{
        //            //    try
        //            //    {
        //            //        Process.Start(Application.StartupPath + @"\Updater.exe");
        //            //        Application.Exit();
                            
        //            //    }
        //            //    catch (Exception exception)
        //            //    {
        //            //        MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButtons.OK,
        //            //            MessageBoxIcon.Error);
        //            //    }
        //            //}
        //        }
        //        else
        //        {
        //            timer.Interval = 2 * 60 * 1000;
                    
        //        }
        //    }
        //    else
        //    {
        //        if (args.Error is WebException)
        //        {
        //            MessageBox.Show(
        //                @"Похоже проблема с сервером обновлений. Попробуйте проверить интернет-соединение.",
        //                @"Проверка обновлений не удалась", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //        else
        //        {
        //            MessageBox.Show(args.Error.Message,
        //                args.Error.GetType().ToString(), MessageBoxButtons.OK,
        //                MessageBoxIcon.Error);
        //        }
        //    }
        //}
        private static void AutoUpdate()
        {
            AutoUpdater.ShowSkipButton = false;
            AutoUpdater.ShowRemindLaterButton = false;
            AutoUpdater.Mandatory = true;
            AutoUpdater.RunUpdateAsAdmin = false;
            var curentDirecttory = new DirectoryInfo(Environment.CurrentDirectory);
            AutoUpdater.ReportErrors = false;
            AutoUpdater.InstallationPath = Application.StartupPath;//curentDirecttory.FullName;
            AutoUpdater.DownloadPath = System.IO.Path.GetTempPath() + "\\UpdateExtrator\\";
            AutoUpdater.AppTitle = "AOVGEN";
            const string versioninfofile = @"W:\Группа автоматизации\Revit Plugins\Плагины собственной разработки\GKSASU\AOVGen\buildinfo.xml";
            AutoUpdater.Start(versioninfofile);
        }

        
        private bool ReadLevelsAndRooms(RunRevitCommand revitCommand, Building building, Project project)
        {
            bool result = false;

            //make treeview with levels and rooms
            try
            {
                //try to update variable in loader class via WCF
                Dictionary<(string, string, double), List<(string, string, string)>> rooms =
                    new Dictionary<(string, string, double), List<(string, string, string)>>();
                for (int i = 0; i <= 3; i++)
                {
                    //Task task = Task.Factory.StartNew(revitCommand.GetRooms);
                    //task.Wait();
                    revitCommand.GetRooms();
                    System.Threading.Thread.Sleep(1);
                    rooms = revitCommand.GetRooms(); 
                    if (rooms.Count> 0) break;
                }
                
                //try to get dictioanery with rooms via WCF

                //Dictionary<(string, string, double), List<(string, string, string)>> rooms = revitCommand.GetRooms();
                if (rooms.Count > 0)
                {
                    radTreeView5.Nodes.Clear();
                    //get levels from room dictionary keys, convert to List and sorting by level Elevation
                    List<(string, string, double)> levels = new List<(string, string, double)>(rooms.Keys)
                        .OrderBy(o => o.Item3)
                        .ToList();
                    foreach ((string, string, double) level in levels)
                    {
                        var levelname = level.Item2;
                        var levelID = level.Item1;
                        bool levelpresent = СheckLevelPresent(levelID);
                        string levelGUID = CreateLevel(levelID, levelname, level.Item3, building.BuildGUID,
                            project.GetGUID(), levelpresent);
                        //make level node
                        RadTreeNode LevelNode = new RadTreeNode
                        {
                            Name = levelGUID,
                            Text = levelname
                        };
                        //get Rooms from room dictionary by level as key and sorting by room number
                        List<(string, string, string)> levelrooms = rooms[level]
                            //.OrderBy(o => GetOnlyLetters(o.Item2))
                            //.ThenBy(o => Convert.ToInt32(GetOnlyNumbers(o.Item2)))
                            .OrderBy(o => o.Item2)
                            .ThenBy(o => o.Item1)
                            .ToList();
                        foreach (var (roomID, roomnum, roomname) in levelrooms)
                        {
                            (string RoomNumber, string RoomName, string RoomID) roomInfo;
                            roomInfo.RoomName = roomname;
                            roomInfo.RoomNumber = roomnum;
                            roomInfo.RoomID = roomID;


                            //make room node
                            bool RoomPresent = CheckRoomPresent(roomID, levelGUID);
                            CreateRoom(roomname, roomnum, roomID, levelGUID, levelname, building.BuildGUID,
                                project.GetGUID(), RoomPresent);
                            RadTreeNode RoomNode = new RadTreeNode
                            {
                                Name = roomID,
                                Text = $"({roomnum}) " + roomname,
                                Tag = roomInfo
                            };
                            //add room node to level node
                            LevelNode.Nodes.Add(RoomNode);
                        }

                        //add level node to treeview
                        radTreeView5.Nodes.Add(LevelNode);
                    }

                    radTreeView5.TreeViewElement.Update(RadTreeViewElement.UpdateActions.Reset);

                    //radTreeView5.Update();                        
                    result = true;
                }
                

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

            WindowState = FormWindowState.Normal;
            return result;
        }
        private string CreateLevel(string levelID, string levelName, double Elevation, string BuildGUID, string ProjectGUID, bool levelpresent)
        {
            string InsertLevelQuery ;
            string mylevelID = string.Empty;
            string getlevelID = string.Empty;
            if (levelpresent)
            {
                InsertLevelQuery = $"UPDATE Levels SET [LevelID] = '{levelID}', Name = '{levelName}', Elevation = '{Elevation}' WHERE [Place] = '{BuildGUID}' AND LevelID = '{levelID}' ";
                getlevelID = $"SELECT [LevelGUID] FROM Levels WHERE [Place] = '{BuildGUID}'";
            }
            else
            {
                mylevelID = Guid.NewGuid().ToString();
                InsertLevelQuery = "INSERT INTO Levels ([LevelID], [LevelGUID], Name, Elevation, [Place], [Project]) " +
                                                        $"VALUES('{levelID}','{mylevelID}','{levelName}','{Elevation}','{BuildGUID}', '{ProjectGUID}')";
            }
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection,
                CommandText = InsertLevelQuery
            };
            command.ExecuteNonQuery();
            if (string.IsNullOrEmpty(getlevelID)) return mylevelID;
            command.CommandText = getlevelID;
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    mylevelID = reader[0].ToString();
                    break;
                }
            }
            return mylevelID;
        }
        private bool DeleteLevels(string PlaceGuid)
        {
            bool result = false;
            string DeleteQuery = $"DELETE FROM Levels WHERE Place = '{PlaceGuid}'";
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection,
                CommandText = DeleteQuery
            };
            try
            {
                command.ExecuteNonQuery();
                result = true;
            }
            catch { }
            return result;
        }
        private bool DeleteRooms(string PlaceGuid)
        {
            bool result = false;
            string DeleteQuery = $"DELETE FROM Rooms WHERE Place = '{PlaceGuid}'";
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection,
                CommandText = DeleteQuery
            };
            try
            {
                command.ExecuteNonQuery();
                result = true;
            }
            catch { }
            return result;
        }
        private bool ClearPannelInfo(string PlaceGUID)
        {
            bool result = false;
            string clearpannelqury = $"UPDATE Pannel SET [LevelGUID] = '', [RoomGUID] = '', RoomName = '', RoomNumber = '' WHERE Place = '{PlaceGUID}'";
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection,
                CommandText = clearpannelqury
            };
            try
            {
                command.ExecuteNonQuery();
                result = true;
            }
            catch { }
            return result;
        }
        private void WriteLevelToPannel(Pannel pannel, string LevelGUID, string RoomGUID, string RoomName,string RoomNumber)
        {
            string UpdatePannelQuery = $"UPDATE Pannel SET [LevelGUID] = '{LevelGUID}', [RoomGUID] = '{RoomGUID}', RoomName = '{RoomName}', RoomNumber = '{RoomNumber}' WHERE [GUID] = '{pannel.GetGUID()}'";
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection,
                CommandText = UpdatePannelQuery
            };
            try
            {
                command.ExecuteNonQuery();
            }
            catch
            { }
        }
        private void CreateRoom(string roomname, string roomnum, string roomID, string levelGUID, string levelname,
            string BuildGUID, string projectGUID, bool RoomPresent)
        {
            string InsertRoomQuery;
            string getRoomID = string.Empty;
            if (RoomPresent)
            {
                InsertRoomQuery = $"UPDATE Rooms SET [RoomID] = '{roomID}', RoomName = '{roomname}', RoomNumber = '{roomnum}', [Place] = '{BuildGUID}', [Project] = '{projectGUID}' WHERE [LevelGUID] = '{levelGUID}' AND RoomID = '{roomID}'";
                getRoomID = $"SELECT [RoomGUID] FROM Rooms WHERE [RoomID] = '{roomID}'";
            }
            else
            {
                var myRoomID = Guid.NewGuid().ToString();
                InsertRoomQuery = "INSERT INTO Rooms (RoomID, [RoomGUID], RoomName, RoomNumber, [Place], [Project], [LevelGUID], LevelName) " +
                                  $"VALUES('{roomID}','{myRoomID}','{roomname}','{roomnum}','{BuildGUID}', '{projectGUID}', '{levelGUID}', '{levelname}')";
            }
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection,
                CommandText = InsertRoomQuery
            };
            command.ExecuteNonQuery();
            if (string.IsNullOrEmpty(getRoomID)) return;
            command.CommandText = getRoomID;
            
        }
        private bool СheckLevelPresent(string levelID )
        {
            string CheckLevelQuery = $"SELECT [LevelGUID] FROM Levels WHERE LevelID = '{levelID}'";
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection,
                CommandText = CheckLevelQuery
            };
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var i1 = reader[0].ToString();
                    if (!string.IsNullOrEmpty(i1)) return true;
                }
            }
            return false;
        }
        private bool CheckRoomPresent (string roomID,  string LevelGUID)
        {
            string CheckRoomQuery = $"SELECT [RoomGUID] FROM Rooms WHERE RoomID = '{roomID}' AND LevelGUID = '{LevelGUID}'";
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection,
                CommandText = CheckRoomQuery
            };
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var i1 = reader[0].ToString();
                    if (!string.IsNullOrEmpty(i1)) return true;
                }
            }
            return false;
        }

        private void radButtonElement14_Click(object sender, EventArgs e)
        {
            Specification();
        }
        private void Specification()
        {
            Dictionary<Pannel, List<VentSystem>> VentDict = new Dictionary<Pannel, List<VentSystem>>();
            Dictionary<string, int> uniquecables = new Dictionary<string, int>();
            if (radTreeView4 != null)
            {
                //List<Pannel> PannelList = new List<Pannel>();
                
                foreach (RadTreeNode radTreeNode in radTreeView4.Nodes)
                {
                    Pannel pannel = (Pannel)radTreeNode.Tag;
                    SQLiteCommand getcablescommand = new SQLiteCommand
                    {
                        Connection = connection,
                        CommandText = $"Select CableType, sum(CableLenght) FROM Cable WHERE FromGUID = '{pannel.GetGUID()}' GROUP BY CableType"
                    };
                    //DataTable dataTable = new DataTable();
                    using (SQLiteDataReader reader = getcablescommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string cabtype = string.IsNullOrEmpty(reader[0].ToString()) ? "Тип не установлен" : reader[0].ToString();
                            int lenght = Convert.ToInt32(string.IsNullOrEmpty(reader[1].ToString()) ? "0" : reader[1].ToString());
                            if (!uniquecables.ContainsKey(cabtype))
                            {
                                uniquecables.Add(cabtype, lenght);
                            }
                            else
                            {
                                uniquecables[cabtype] += lenght;
                            }
                        }
                    }
                    getcablescommand.Dispose();
                    if (radTreeNode.Nodes.Count <= 0) continue;
                    foreach (RadTreeNode ventnode in radTreeNode.Nodes)
                    {
                        VentSystem ventSystem = (VentSystem)ventnode.Tag;                            
                        //listkip = listkip.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList(); //remove list null entry
                        if (!VentDict.ContainsKey(pannel))
                        {
                            List<VentSystem> ventSystems = new List<VentSystem>
                            {
                                ventSystem
                            };
                            VentDict.Add(pannel, ventSystems);
                        }
                        else
                        {
                            List<VentSystem> vent = VentDict[pannel];
                            vent.Add(ventSystem);
                            VentDict[pannel] = vent;                                
                        }
                            
                    }
                }
            }
            DataTable dt = new DataTable();
            dt.Columns.Add("Поз.");
            dt.Columns.Add("Наименование и техническая характеристика");
            dt.Columns.Add("Тип, марка, обозначение опросного листа");
            dt.Columns.Add("Код продукции");
            dt.Columns.Add("Поставщик");
            dt.Columns.Add("Ед.измерения");
            dt.Columns.Add("Кол.");
            dt.Columns.Add("Масса 1ед, кг");
            dt.Columns.Add("Примечание");
            DataRow row = dt.Rows.Add();
            row[dt.Columns[0]] = "1";
            row[dt.Columns[1]] = "2";
            row[dt.Columns[2]] = "3";
            row[dt.Columns[3]] = "4";
            row[dt.Columns[4]] = "5";
            row[dt.Columns[5]] = "6";
            row[dt.Columns[6]] = "7";
            row[dt.Columns[7]] = "8";
            row[dt.Columns[8]] = "9";
            int pannelcnt = 1;
            if (VentDict.Count > 0)
            {
                foreach (KeyValuePair<Pannel, List<VentSystem>> keyValuePair in VentDict)
                {
                    Pannel pannel = keyValuePair.Key;
                    DataRow pannelrow = dt.Rows.Add();
                    pannelrow[dt.Columns[1]] = $"Комплект автоматики для шкафа {pannel.PannelName}";
                    pannelrow[dt.Columns[0]] = pannelcnt.ToString();
                    List<VentSystem> vents = keyValuePair.Value;
                    if (vents.Count>0)
                    {
                        int ventcnt = 1;
                        foreach (VentSystem vent in vents)
                        {
                            List<dynamic> ListKip = vent.GetKIP(); //в вентсистеме реализована функция которая возвращает все датчики
                            //созадем словарь. в качестве ключа будет строка (артикул), зачение - коретеж из IvendorInfo и числа (отвечает за количество)
                            Dictionary<string, (IVendoInfo, int)> compdict = new Dictionary<string, (IVendoInfo, int)>();
                            int abstractIDcnt = 1;
                            
                            foreach (dynamic kipcom in ListKip) //перебираем список с датчиками
                            {
                                string abstractID = $"Абстрактный артикул {abstractIDcnt}";
                                IVendoInfo vendoInfo = kipcom;//превращаем датчик в интерфейс
                                var ID = vendoInfo.GetVendorInfo().ID; //создаем переменную для артикула
                                if (!string.IsNullOrEmpty(ID)) //если он не пустой и не null
                                {
                                    if (!compdict.ContainsKey(ID)) //если артикула нет в качестве ключа в словаре
                                    {
                                        compdict.Add(ID, (vendoInfo, 1)); //то добавляем в словарь ключ и кортеж
                                    }
                                    else //а если есть ключ в словаре
                                    {
                                        var present = compdict[ID]; //достаем кортеж по артикулу
                                        present.Item2++; //и увеличиваем значение второго элемента кортежа на 1
                                        compdict[ID] = present; //кладем обратно кортеж в словарь по этому ID
                                    }
                                }
                                else //если акртикула вообще нет
                                {
                                    if (vendoInfo is Vent.FControl) //то проверяем, можно ли интерфейс привести к частостнику
                                    {
                                        if (!compdict.ContainsKey(abstractID)) //проверяем есть ли такой артикул в словаре
                                        {
                                            vendoInfo.SetVendorInfo("Абстракный вендор", abstractID, null, null, null);
                                            compdict.Add(abstractID, (vendoInfo, 1)); //если нет то добавляем частотник в словарь
                                           
                                        }
                                        else
                                        {
                                            var present = compdict[abstractID]; //а если есть то достаем кортеж
                                            present.Item2++; //увеличиваем второй элемент на 1
                                            compdict[abstractID] = present; //кладем кортеж обратно в словарь
                                        }
                                    }
                                    if (vendoInfo is Sensor)
                                    {
                                        if (!compdict.ContainsKey(abstractID))
                                        {
                                            vendoInfo.SetVendorInfo("Абстракный вендор", abstractID, null, null, null);
                                            compdict.Add(abstractID, (vendoInfo, 1));
                                           
                                        }
                                        
                                        
                                    }
                                    abstractIDcnt++;
                                }
                                
                            }
                            if (compdict.Count>0) //если в словаре что-то есть
                            {
                                DataRow ventlrow = dt.Rows.Add(); //в DataTable (объявлена ранее как dt) добавляем строку, называется ventlrow
                                ventlrow[dt.Columns[1]] = $"Приборы вентисистемы {vent.SystemName}"; //в колнку 1 кладем текст заголовка
                                ventlrow[dt.Columns[0]] = pannelcnt + "." + ventcnt; //в колонку 0 кладем нумерацию типа 1.1
                                int cnt = 1;//задаем счетчик (он нужен чтобы нумерцию делать типа 1.1.1, 1.1.2, 1.1.3, 1.1.4 и тд)
                                foreach (KeyValuePair<string, (IVendoInfo, int)> valuePair in compdict)//начинаем перебирать словарь
                                {
                                    DataRow kiprow = dt.Rows.Add();//создаем еще 1 строку (туда уже и будет попадать инфа о датчике)
                                    kiprow[dt.Columns[0]] = pannelcnt + "." + ventcnt + "." + cnt;//это нумреция
                                    IVendoInfo vendoInfo = valuePair.Value.Item1;
                                    var description = string.IsNullOrEmpty(vendoInfo.GetVendorInfo().VendorDescription) ? vendoInfo.GetVendorInfo().DefaultDescription : vendoInfo.GetVendorInfo().VendorDescription;
                                    var assignment = vendoInfo.GetVendorInfo().Assignment;
                                    kiprow[dt.Columns[1]] = description; //кладем в 1 колонку описание элемента.
                                    kiprow[dt.Columns[6]] = valuePair.Value.Item2.ToString();//кладем количество
                                    kiprow[dt.Columns[5]] = "шт.";
                                    string vendname = string.IsNullOrEmpty(valuePair.Value.Item1.GetVendorInfo().VendorName) ? "Абстрактный вендор" : valuePair.Value.Item1.GetVendorInfo().VendorName;
                                    kiprow[dt.Columns[4]] = vendname;//кладем вендора
                                    kiprow[dt.Columns[3]] = vendoInfo.GetVendorInfo().ID;//кладем вендора
                                    cnt++; //увеличиваем счетчик на 1
                                    if (string.IsNullOrEmpty(assignment)) continue;
                                    DataRow assignmentrow = dt.Rows.Add();
                                    assignmentrow[dt.Columns[0]] = pannelcnt + "." + ventcnt + "." + cnt;
                                    assignmentrow[dt.Columns[1]] = assignment;
                                    assignmentrow[dt.Columns[5]] = "шт.";
                                    assignmentrow[dt.Columns[4]] = vendname;
                                    assignmentrow[dt.Columns[6]] = valuePair.Value.Item2.ToString();
                                    cnt++;

                                }
                            }



                            //if (ListKip.Count > 0)
                            //{
                            //    DataRow ventlrow = dt.Rows.Add();
                            //    ventlrow[dt.Columns[1]] = $"Приборы вентисистемы {vent.SystemName}";
                            //    ventlrow[dt.Columns[0]] = pannelcnt.ToString()+ "." + ventcnt.ToString();
                            //    int cnt = 1;
                            //    foreach (dynamic kipcom in ListKip)
                            //    {
                            //        description = string.Empty;
                            //        DataRow kiprow = dt.Rows.Add();
                            //        kiprow[dt.Columns[0]] = pannelcnt.ToString() + "." + ventcnt.ToString() + "." + cnt.ToString();
                            //        IVendoInfo vendoInfo = kipcom;
                            //        description = string.IsNullOrEmpty(vendoInfo.GetVendorInfo().VendorDescription) ? vendoInfo.GetVendorInfo().DefaultDescription : vendoInfo.GetVendorInfo().VendorDescription;
                            //        kiprow[dt.Columns[1]] = description;
                            //        cnt++;                                    
                            //    }                                
                            //}
                            ventcnt++;
                        }
                    }
                    pannelcnt++;
                }
            }
            if (uniquecables.Count>0)
            {
                DataRow cablerowhead = dt.Rows.Add();
                cablerowhead[dt.Columns[1]] = "Кабельная продукция";
                cablerowhead[dt.Columns[0]] = pannelcnt.ToString();
                int cablecnt = 1;
                foreach (KeyValuePair<string, int> keyValuePair in uniquecables)
                {
                    DataRow cablerow = dt.Rows.Add();
                    cablerow[dt.Columns[1]] = keyValuePair.Key;
                    cablerow[dt.Columns[6]] = keyValuePair.Value;
                    cablerow[dt.Columns[5]] = "м";
                    cablerow[dt.Columns[0]] = $"{pannelcnt}.{cablecnt}";
                    cablecnt++;
                }
                pannelcnt++;
                
            }
            const string docname = "Спецификация";
            XLWorkbook workbook= CreateXLSWorkBook(dt, docname);
            IXLWorksheet ws = workbook.Worksheet(docname);
            const int firstrow = 3;
            int lastrow = ws.LastRowUsed().RowNumber();
            IXLRows range = ws.Rows(firstrow, lastrow);           
            Dictionary<IXLAddress, IXLCell> ListAuto = range
                .Where(r => r.Cell(2).Value.ToString().Contains("Комплект автоматики"))
                .Select(d =>
                {
                    IXLCell xLCell = d.Cell(2);
                    xLCell.Style.Font.Bold = true;
                    xLCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    xLCell.Style.Font.FontColor = XLColor.FromArgb(168, 7, 7);
                    return xLCell;

                })
                .ToDictionary(e => e.Address, e => e);
            Dictionary<IXLAddress, IXLCell> ListComplect = range
                .Where(r => r.Cell(2).Value.ToString().Contains("Приборы вентисистемы"))
                .Select(d =>
                {
                    IXLCell xLCell = d.Cell(2);
                    xLCell.Style.Font.Bold = true;
                    xLCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    xLCell.Style.Font.FontColor = XLColor.FromArgb(15, 36, 82);
                    return xLCell;
                })
                .ToDictionary(e => e.Address, e => e);
            Dictionary<IXLAddress, IXLCell> ListCables = range
                .Where(r => r.Cell(2).Value.ToString().Contains("Кабельная продукция"))
                .Select(d =>
                {
                    IXLCell xLCell = d.Cell(2);
                    xLCell.Style.Font.Bold = true;
                    xLCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    xLCell.Style.Font.FontColor = XLColor.FromArgb(168, 7, 7);
                    return xLCell;
                })
                .ToDictionary(e => e.Address, e => e);

            foreach (IXLRow Wrow in range)
            {
                IXLCell cell = Wrow.Cell(2);
                
                if (cell.Value.ToString().Contains("Комплект автоматики"))
                {
                    cell = ListAuto[cell.Address];
                }
                if (cell.Value.ToString().Contains("Приборы вентисистемы"))
                {
                    cell = ListComplect[cell.Address];
                }
                if (cell.Value.ToString().Contains("Кабельная продукция"))
                {
                    cell = ListCables[cell.Address];
                }
            }
            ws.Row(2).Cell(2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            SetXLSColumnAsNum(workbook, docname, 7);
            //SetXLSColumnAsNum(workbook, docname, 5);
            int[] columnsForCenter = { 1, 3, 4, 5, 6, 7, 8, 9 };
            foreach (int t in columnsForCenter)
            {
                ColumnAlignment(workbook, docname, t, XLAlignmentHorizontalValues.Center);
            }

            int[] columnsForAdjustForContent = Enumerable.Range(1, 9).ToArray();
            foreach (int t in columnsForAdjustForContent)
            {
                SetXLSAdjustToContents(workbook, docname, t);
            }            
            SaveXLS(workbook, "Сохранение спецификации в XLS");
        }
        private static XLWorkbook CreateXLSWorkBook(DataTable dt, string DocName)
        {
            XLWorkbook workbook = new XLWorkbook();
            workbook.Worksheets.Add(dt, DocName);
            return workbook;
        }
        private static void SetXLSColumnAsNum(XLWorkbook workbook, string sheetname, int columnNumber)
        {
            IXLWorksheet ws = workbook.Worksheet(sheetname);
            IXLCells cellss = ws.Column(columnNumber).Cells();
            //set columt5 cells format as number
            foreach (var cell in cellss)
            {
                try
                {
                    cell.SetDataType(XLDataType.Number);
                }
                catch { }

            }
        }
        private static void SetXLSAdjustToContents(XLWorkbook workbook, string sheetname, int columnNumber)
        {
            IXLWorksheet ws = workbook.Worksheet(sheetname);
            ws.Column(columnNumber).AdjustToContents();
        }
        private static void SaveXLS(IXLWorkbook workbook, string title)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = @"Excel workbook|*.xlsx",
                    Title = title,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };
                DialogResult dialogResult = saveFileDialog.ShowDialog();
                if (dialogResult != DialogResult.OK) return;
                if (string.IsNullOrEmpty(saveFileDialog.FileName)) return;
                workbook.SaveAs(saveFileDialog.FileName);
                Process.Start(saveFileDialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void radTreeView6_NodeMouseClick(object sender, RadTreeViewEventArgs e)
        {
            radPropertyGrid1.SelectedObject = e.Node.Tag as Pannel;
            radPropertyGrid1.Enabled = false;
        }
        private void radTreeView6_DragOverNode(object sender, RadTreeViewDragCancelEventArgs e)
        {

            Point p = radTreeView5.PointToClient(new Point(Cursor.Position.X, Cursor.Position.Y));
            RadTreeNode targetNode = radTreeView5.GetNodeAt(p);
            if (e.DropPosition == DropPosition.BeforeNode || e.DropPosition == DropPosition.AfterNode || e.TargetNode.Level ==0 || e.TargetNode.Tag != null) e.Cancel = true;
            if (targetNode == null || e.TargetNode.Level != 1 || e.DropPosition != DropPosition.AsChildNode) return;
            e.TargetNode.Selected = true;
            e.Cancel = false;                
            radTreeView5.Update();
        }
        private void radTreeView5_DragOverNode(object sender, RadTreeViewDragCancelEventArgs e)
        {
            Point p = radTreeView5.PointToClient(new Point(Cursor.Position.X, Cursor.Position.Y));
            RadTreeNode targetNode = radTreeView5.GetNodeAt(p);
            RadTreeNode selectednode = radTreeView5.SelectedNode;
            if (selectednode.Level == 2)
            {
                if (e.DropPosition == DropPosition.AsChildNode && targetNode?.Level ==1)
                {
                    e.Cancel = false;
                }
                else
                {
                    e.Cancel = true;                    
                }
            }
            else
            {
                e.Cancel = true;
            }            
        }

        private void radTreeView5_DragDrop(object sender, DragEventArgs e)
        {
            Point targetPoint = radTreeView5.PointToClient(new Point(Cursor.Position.X, Cursor.Position.Y));            
            // Retrieve the node at the drop location.
            RadTreeNode targetNode = radTreeView5.GetNodeAt(targetPoint);
            targetNode.Selected = true;
            // Retrieve the node that was dragged.
            RadTreeNode draggedNode = (RadTreeNode)e.Data.GetData(typeof(RadTreeNode));
            // Confirm that the node at the drop location is not 
            // the dragged node and that target node isn't null
            // (for example if you drag outside the control)
            if (!draggedNode.Equals(targetNode))
            {
                // Remove the node from its current 
                // location and add it to the node at the drop location.
                draggedNode.Remove();
                targetNode.Nodes.Add(draggedNode);
                // Expand the node at the location 
                // to show the dropped node.
                targetNode.Expand();
            }
        }
        

        private void radTreeView6_DragEnded(object sender, RadTreeViewDragEventArgs e)
        {
            try
            {
                RadTreeNode selectedNode = e.Node;
                Font boldFont = new Font(radTreeView6.Font, FontStyle.Bold);
                selectedNode.Font = boldFont;
                Pannel pannel = (Pannel)selectedNode.Tag;
                RadTreeNode targetNode = radTreeView5.SelectedNode;
                string LevelGUID = targetNode.Parent.Name;
                string RoomGUID = targetNode.Name;
                var (roomNumber, roomName, _) = ((string, string, string))targetNode.Tag;
                //animation
                        
                
                pictureBox1.Image = Resources.I7gj;
                pictureBox1.Update();
                Timer t = new Timer
                {
                    Interval = 9100 // specify interval time as you want
                };
                t.Tick += timer_Tick;
                t.Start();
                WriteLevelToPannel(pannel, LevelGUID, RoomGUID, roomName, roomNumber);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
            }
           
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            Timer timer = (Timer)sender;
            timer.Stop();
                     
            pictureBox1.Image = Resources.frame_000_delay_0_03s;
            pictureBox1.Update();
            
        }
        
        private void radTreeView5_DragEnded(object sender, RadTreeViewDragEventArgs e)
        {
            Pannel pannel = (Pannel)e.Node.Tag;
            string LevelGUID = e.TargetNode.Parent?.Name;
            string RoomGUID = e.TargetNode.Name;
            var (roomNumber, roomName, _) = ((string , string, string ))e.TargetNode.Tag;
            WriteLevelToPannel(pannel, LevelGUID, RoomGUID, roomName, roomNumber);

        }

        private void radTreeView5_ContextMenuOpening(object sender, TreeViewContextMenuOpeningEventArgs e)
        {
            RadTreeNode selectednode = radTreeView5.SelectedNode;
            switch (selectednode.Level)
            {
                case 0:
                    radMenuItem2.Enabled = false;
                    radMenuItem2.Text = @"Это уровень";
                    break;
                case 1:
                    if (selectednode.Nodes.Count > 0)
                    {
                        radMenuItem2.Enabled = true;
                        radMenuItem2.Text = @"Отключить все";
                    }
                    else
                    {
                        radMenuItem2.Enabled = false;
                        radMenuItem2.Text = @"Нет шкафов";
                    }
                    break;
                case 2:
                    radMenuItem2.Enabled = true;
                    radMenuItem2.Text = @"Отключить";
                    break;
            }
            
        }

        private struct PannelInfo
        {
            public string GUID { get; set; }
            public string PannelName { get; set; }
            public string Power { get; set; }
            public string Voltage { get; set; }
            public string Category { get; set; }
            public string LevelGUID { get; set; }
            public string LevelName { get; set; }
            public string RoomName { get; set; }
            public string RoomNumber { get; set; }
        }
        private void radButtonElement13_Click(object sender, EventArgs e)
        {
            Building building = radTreeView1.SelectedNode.Tag as Building;
            List<PannelInfo> PanneInfoList = new List<PannelInfo>();
            string SelectPannelInfo = $"SELECT GUID, PannelName, Power, Voltage, Category, [LevelGUID], RoomName, RoomNumber FROM Pannel WHERE Place = '{building?.BuildGUID}' ORDER BY PannelName ASC";
            SQLiteCommand getpannelGUIDsCommand = new SQLiteCommand
            {
                CommandText = SelectPannelInfo,
                Connection = connection
            };
            using (SQLiteDataReader reader = getpannelGUIDsCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    PannelInfo pannelInfo = new PannelInfo
                    {
                        GUID = reader[0].ToString(),
                        PannelName = reader[1].ToString(),
                        Power = reader[2].ToString(),
                        Voltage = reader[3].ToString(),
                        Category = reader[4].ToString(),
                        LevelGUID = reader[5].ToString(),
                        RoomName = reader[6].ToString(),
                        RoomNumber = reader[7].ToString()
                    };
                    SQLiteCommand levelnamecommand = new SQLiteCommand
                    {
                        Connection = connection,
                        CommandText = $"SELECT Name FROM Levels WHERE [LevelGUID] = '{pannelInfo.LevelGUID}'"
                    };
                    using (SQLiteDataReader reader1 = levelnamecommand.ExecuteReader())
                    {
                        while (reader1.Read())
                        {
                            pannelInfo.LevelName = reader1[0].ToString();
                        }
                    }
                    PanneInfoList.Add(pannelInfo);
                }                
            }
            DataTable datatable = new DataTable();
            datatable.Columns.Add("Имя щита");
            datatable.Columns.Add("Мощность (Вт)");
            datatable.Columns.Add("Напряжение");
            datatable.Columns.Add("Категория");
            datatable.Columns.Add("Уровень");
            datatable.Columns.Add("Помещение");            
            foreach (PannelInfo pannelInfo in PanneInfoList)
            {
                DataRow drToAdd = datatable.NewRow();
                drToAdd["Имя щита"] = pannelInfo.PannelName;
                drToAdd["Мощность (Вт)"] = Convert.ToDouble(pannelInfo.Power);
                drToAdd["Напряжение"] = pannelInfo.Voltage;
                int cat = 0;
                switch (pannelInfo.Category)
                {
                    case "one":
                        cat = 1;
                        break;
                    case "two":
                        cat = 2;
                        break;
                    case "three":
                        cat = 3;
                        break;
                }
                drToAdd["Категория"] = cat;
                drToAdd["Уровень"] = pannelInfo.LevelName;
                string roomname = string.Empty;
                if (!string.IsNullOrEmpty (pannelInfo.RoomName))
                {
                    roomname = $"({pannelInfo.RoomNumber}) {pannelInfo.RoomName}";
                }
                drToAdd["Помещение"] = roomname;
                datatable.Rows.Add(drToAdd);
            }
            var workbook = new XLWorkbook();
            string sheetname = "Задание ЭОМ";
            //var ws =  workbook.Worksheets.Add(sheetname);
            var ws = workbook.Worksheets.Add(datatable, sheetname);
            for (int t = 1; t < 7; t++)
            {
                ws.Column(t).AdjustToContents();
            }
            IXLCells cells2 = ws.Column(2).Cells();
            //set columt5 cells format as number
            foreach (var cell in cells2)
            {
                try
                {
                    cell.SetDataType(XLDataType.Number);
                }
                catch { }

                cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            }
            //set cell Alignment
            int[] columns = { 1, 2, 3, 4, 6 };
            foreach (int t in columns)
            {
                ColumnAlignment(ws, t, XLAlignmentHorizontalValues.Center);
            }
            SaveXLS(workbook, "Сохранение задания для ЭОМ в XLS");
        }
        private void ShowProgressHandler(object o)
        {
            Loading form = o as Loading;
            for (int i = 0; i < 100; i++)
            {
                int i1 = i;
                this.Invoke((MethodInvoker)(() => form.OnProgress(i1)));
                Thread.Sleep(100);
            }
        }
        private void radButtonElement21_Click(object sender, EventArgs e)
        {
            if (radTreeView1.SelectedNode != null)
            {
                RadTreeNode buildnode = radTreeView1.SelectedNode;
                if (buildnode.Tag is Building building)
                {
                    DialogResult dialogResult = MessageBox.Show("Сейчас будут считаны и перезаписаны уровни и помещения!\n" +
                                                                "Привязанные к помещениям шкафы нужно будеть привязать снова", "Изменения в базе данных!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dialogResult == DialogResult.Yes)
                    {
                        

                        Project project = buildnode.Parent.Tag as Project;
                        (string DocID, string DocTitle) StateInfo;
                        StateInfo.DocID = string.Empty;
                        RunRevitCommand revitCommand = new RunRevitCommand(Service);
                        //try
                        //{

                        //    StateInfo = revitCommand.StateInfo();
                        //    Thread.Sleep(500);
                        //    StateInfo = revitCommand.StateInfo();
                        //    if (string.IsNullOrEmpty(StateInfo.DocID)) RevitConnectionState = RevitState.Open;
                        //}
                        //catch
                        //{
                        //    RevitConnectionState = RevitState.Close;
                        //}
                        switch (RevitConnectionState)
                        {
                            case RevitState.Close:
                                pictureBox2.Image = Resources.red_light;
                                timer.Stop();
                                MessageBox.Show("Перезапустите AOVGEN с запущенным Revit.\n" +
                                    "Для связи с моделью сначала должен быть запущен Revit, а затем AOVGEN (не наоборот!)\n" +
                                    "Если в процессе работы был закрыт Revit, то AOVGEN придется перезапустить", "Не получится установить связь", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            case RevitState.Open:
                                DialogResult dialogResult1 = MessageBox.Show("Не найдена модель\n" +
                                    "Будет предпринято несколько попыток установить с ней связь", "Связь с моделью", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                if (dialogResult1 == DialogResult.Yes)
                                {
                                    try
                                    {
                                        for (int t = 1; t < 5; t++) //запускаем 5 попыток получить ID
                                        {
                                            try
                                            {
                                                Thread thread = new Thread(() =>
                                                {
                                                    var attempform = new Attempt { Num = t };
                                                    Application.Run(attempform);
                                                });
                                                thread.SetApartmentState(ApartmentState.STA);
                                                thread.Start();
                                            }
                                            catch { }
                                            StateInfo = revitCommand.StateInfo();
                                            if (!string.IsNullOrEmpty(StateInfo.DocID)) break;
                                        }
                                        if (!string.IsNullOrEmpty(StateInfo.DocID))
                                        {
                                            RevitConnectionState = RevitState.DocumentPresent;
                                            revitCommand.GetRooms();
                                            bool readresult = ReadLevelsAndRooms(revitCommand, building, project);
                                            if (readresult)
                                            {
                                                MessageBox.Show(radTreeView5.Nodes.Count > 0
                                                    ? "Готово"
                                                    : "Похоже, в модели нет уровней");
                                            }
                                            WindowState = FormWindowState.Normal;
                                        }
                                    }
                                    catch
                                    {
                                        MessageBox.Show("Не удалось подключиться к модели", "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        WindowState = FormWindowState.Normal;
                                        return;
                                    }
                                    if (string.IsNullOrEmpty(StateInfo.DocID))
                                    {
                                        MessageBox.Show("Похоже, в Revit по-прежнему нет открытой модели");
                                        WindowState = FormWindowState.Normal;
                                    }
                                }

                                break;
                            case RevitState.DocumentPresent:


                                bool result = ReadLevelsAndRooms(revitCommand, building, project);
                                if (result)
                                {
                                   
                                    MessageBox.Show(radTreeView5.Nodes.Count > 0
                                        ? "Готово"
                                        : "Похоже, в модели нет уровней\nПопробуйте прочитать их еще раз");
                                    //ReadLevelsAndRooms(revitCommand, building, project);
                                    

                                    //if (radTreeView5.Nodes.Count == 0)
                                    //{
                                    //    ReadLevelsAndRooms(revitCommand, building, project);
                                    //}
                                    
                                    //MessageBox.Show("Готово2");
                                }
                                else
                                {
                                    MessageBox.Show("Не удалось получить уровни из Revit, попробуйте еще раз");
                                }
                                WindowState = FormWindowState.Normal;
                                break;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Выберите здание");
                    }

                }


            }
        }
        private void radButtonElement22_Click(object sender, EventArgs e)
        {
            RadTreeNode buildnode = radTreeView1.SelectedNode;
            if (buildnode == null) return;
            Building building = (Building)buildnode.Tag;
            DialogResult dialogResult = MessageBox.Show("Сейчас будут удалены уровни и помещения!",
                "Изменения в базе данных!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Yes)
            {
                bool result1 = DeleteLevels(building.BuildGUID);
                bool result2 = DeleteRooms(building.BuildGUID);
                bool result3 = ClearPannelInfo(building.BuildGUID);
                //read pannels and add it to treeview6
                List<Pannel> PannelClassList = new List<Pannel>();
                PannelClassList = (from gridrow in radGridView2.Rows
                        select gridrow.Tag as Pannel)
                    .ToList();
                radTreeView6.Nodes.Clear();
                if (PannelClassList.Count > 0)
                {
                    foreach (var pannelnode in PannelClassList.Select(pannel => new RadTreeNode
                    {
                        Text = pannel.PannelName,
                        Name = pannel.GetGUID(),
                        Tag = pannel
                    }))
                    {
                        radTreeView6.Nodes.Add(pannelnode);
                    }

                    radTreeView6.Update();
                }
                if (result1 && result2 && result3)
                {
                    radTreeView5.Nodes.Clear();
                    MessageBox.Show("Уровни и помещения удалены");
                }
                else
                {
                    if (!result1) MessageBox.Show("Не удалились уровни из БД");
                    if (!result2) MessageBox.Show("Не удалились помещения из БД");
                    if (!result3) MessageBox.Show("Не очистилась информация из шкафов");
                }

            }
            //else return;
        }
        private void ribbonTab6_Click_1(object sender, EventArgs e)
        {
            
            radCollapsiblePanel1.Expand();
            radPageView1.SelectedPage = radPageViewPage4;
            radPropertyGrid1.Enabled = false;
            radRibbonBarGroup9.Enabled = RevitConnectionState == RevitState.DocumentPresent && radTreeView1.SelectedNode.Level == 1;                      
        }

        private async void radButtonElement24_Click(object sender, EventArgs e)
        {
            //List<RadTreeNode> allnodes =  radTreeView5.TreeViewElement.GetNodes().ToList();
            RunRevitCommand revitCommand = new RunRevitCommand(Service);
            Dictionary<string, List<string>> famdict = revitCommand.FamilyTypes();
            PlacePannelsForm placePannelsForm = new PlacePannelsForm(radTreeView5, famdict);
            //System.Threading.Thread.Sleep(1000);
            DialogResult dialogResult = placePannelsForm.ShowDialog();
            
            var ListInfo = placePannelsForm.listinfo;
            placePannelsForm.Close();
            RunRevitCommand revitCommand1 = new RunRevitCommand(Service);//создаем запрос в ревит
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var token = cts.Token;
            int returninfo = -1;
            await Task.Factory.StartNew(() =>
            {

                try
                {
                    token.ThrowIfCancellationRequested();
                    Stopwatch sw = new Stopwatch();
                   
                    returninfo =  revitCommand1.LoadPannels(ListInfo); //говорим что нужно сделать. если команда прошла в dll, то вернётся 1
                    Thread.Sleep(10);
                    if (returninfo == 1)
                    {
                        var reveitCommanr2 = new RunRevitCommand(Service);
                        int result2 = reveitCommanr2.PlacePannels();
                        Thread.Sleep(10);
                        returninfo = result2;
                    }
                   
                    
                    //if (returninfo ==2) MessageBox.Show("Готово");
                }
                catch (OperationCanceledException oc)
                {
                    MessageBox.Show("Вышло время ожидания размещения шкафов");
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }
            }, token);
            if (returninfo == 1)
            {

                Timer getressultTimer = new Timer();
                getressultTimer.Interval = 2000;
                RunRevitCommand reveitCommanr2 = new RunRevitCommand(Service);
                getressultTimer.Tick += (s, a) =>
                {
                    var retinfo2 = reveitCommanr2.GetPlaceResult();
                    if (retinfo2 != 1)
                    {
                        getressultTimer.Stop();

                        switch (retinfo2)
                        {
                            case 2:
                                RunRevitCommand revitCommand3 = new RunRevitCommand(Service); //создаем еще один запрос в ревит
                                var PennelIDS = revitCommand3.GetFamilyListID();
                                MessageBox.Show($"Готово, расставил {PennelIDS.Count} шкафов");
                                break;
                            case 3:
                                MessageBox.Show("Ошибка!");
                                break;
                            case -1:
                                MessageBox.Show("Похоже что команда не пришла в Ревит");
                                break;
                        }
                       
                        
                    }
                };
                getressultTimer.Start();
            }

           

        }
        private void radGridView1_ContextMenuOpening_1(object sender, ContextMenuOpeningEventArgs e)
        {
            e.ContextMenu = radContextMenu4.DropDown;
        }
        private void radGridView4_CellClick(object sender, GridViewCellEventArgs e)
        {
            radPropertyGrid1.SelectedObject = e.Row.Tag;
            var t1 = e.Value;
            RadListDataItem selectedvendor = radDropDownList1.SelectedItem;
            radPropertyGrid1.Enabled = false;
            if (!(e.Row.Tag is IVendoInfo vendoInfo) || selectedvendor == null) return;
            string dbtable = vendoInfo.GetVendorInfo().DBTable;
            string vendorname = selectedvendor.Text;
            SQLiteCommand sQLiteCommand = new SQLiteCommand
            {
                Connection = connectionvendor,
                CommandText = $"SELECT Description, VendorCode FROM {dbtable} WHERE VendorName = '{vendorname}'"
            };
            using (SQLiteDataReader reader = sQLiteCommand.ExecuteReader())
            {
                List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();

                while (reader.Read())
                {
                    list.Add(new KeyValuePair<string, string>(reader[1].ToString(), reader[0].ToString()));
                        
                        
                }

                if (radGridView4.Columns[2] is GridViewComboBoxColumn column)
                {
                    column.DataSource = list;
                    column.DisplayMember = "Value";
                    column.ValueMember = "Key";
                }

                e.Row.Cells[2].Value = t1;
                   

            }
        }
        private void radDropDownList1_SelectedIndexChanged(object sender, PositionChangedEventArgs e)
        {
            if (radGridView4.Rows.Count >0)
            {
                RadListDataItem selecteditem = radDropDownList1.Items[e.Position];
                _ = radGridView4.Columns[2] as GridViewComboBoxColumn;
                //List<string> items = (column?.DataSource as string[]).ToList();
                if (selecteditem != null)
                {
                    //SQLiteCommand sQLiteCommand = new SQLiteCommand
                    //{
                    //    Connection = connectionvendor,
                    //    CommandText = "SELECT * FROM SensTE " +
                    //    "INNER JOIN sqlite_sequence " +
                    //    "ON sqlite_sequence.name = 'SensTE' " +
                    //    $"WHERE VendorName = '{selecteditem.Text}' " +
                    //    "UNION " +
                    //    "SELECT * FROM SensTS " +
                    //    "INNER JOIN sqlite_sequence " +
                    //    "ON sqlite_sequence.name = 'SensTS' " +
                    //    $"WHERE VendorName = '{selecteditem.Text}' " +
                    //    "UNION " +
                    //    "SELECT * FROM SensPE " +
                    //    "INNER JOIN sqlite_sequence " +
                    //    "ON sqlite_sequence.name = 'SensPE' " +
                    //    $"WHERE VendorName = '{selecteditem.Text}' " +
                    //    "UNION " +
                    //    "SELECT * FROM SensPS " +
                    //    "INNER JOIN sqlite_sequence " +
                    //    "ON sqlite_sequence.name = 'SensPS' " +
                    //    $"WHERE VendorName = '{selecteditem.Text}' " +
                    //    "UNION " +
                    //    "SELECT * FROM SensHE " +
                    //    "INNER JOIN sqlite_sequence " +
                    //    "ON sqlite_sequence.name = 'SensHE' " +
                    //    $"WHERE VendorName = '{selecteditem.Text}' " +
                    //    "UNION " +
                    //    "SELECT * FROM SensHS " +
                    //    "INNER JOIN sqlite_sequence " +
                    //    "ON sqlite_sequence.name = 'SensHS' " +
                    //    $"WHERE VendorName = '{selecteditem.Text}'"
                    //};
                    //using (SQLiteDataReader reader = sQLiteCommand.ExecuteReader())
                    //{
                    //    List<string> list = new List<string>();
                    //    while (reader.Read())
                    //    {
                    //        list.Add(reader[6].ToString());
                    //    }
                    //    column.DataSource = list;
                    //}

                }
                
            }
        }
        private void radButtonElement4_Click_1(object sender, EventArgs e)
        {
            Building building = (Building)radTreeView1.SelectedNode.Tag;
            string VendorDBPath = DBFilePath.Replace(DBFileName, string.Empty) + "VendorsDB.db";
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection,
                CommandText = $"ATTACH '{VendorDBPath}' as DB1; " +
                            "SELECT VendorName FROM db1.VendorPresets WHERE ID = " +
                            $"(SELECT VendorPreset FROM BuildSetting WHERE Place = '{building.BuildGUID}')"
            };
            string vendorname = string.Empty;
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    vendorname = reader[0].ToString();
                }
            }
            command.CommandText = "DETACH DB1";
            command.ExecuteNonQuery();
            VendorForm vendorForm = new VendorForm(VendorDBPath, vendorname);
            DialogResult dialogResult = vendorForm.ShowDialog();
            switch (dialogResult)
            {
                case DialogResult.Cancel:
                    vendorForm.Close();
                    break;
                case DialogResult.OK:
                {
                    //процедура сохранения пресета                
                    var (VendorID, _) = vendorForm.GetVendorInfo();
                    command.CommandText = $"UPDATE  BuildSetting SET VendorPreset = '{VendorID}' WHERE Place = '{building.BuildGUID}'";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    vendorForm.Close();
                    break;
                }
            }
        }

        private void radGridView4_CellEndEdit(object sender, GridViewCellEventArgs e)
        {
            
            GridViewComboBoxColumn column = radGridView4.Columns[2] as GridViewComboBoxColumn;
            if (column?.DataSource == null) return;
            var list = (List<KeyValuePair<string, string>>)column.DataSource;
            if (e.Value == null) return;
            string vendorcode = e.Value.ToString();
            KeyValuePair<string, string> selected = (from z in list
                    where z.Key == vendorcode
                    select z)
                .FirstOrDefault();
            string vendorname = radDropDownList1.SelectedItem?.Text;
            string vendordescription = selected.Value;
            e.Row.Cells[3].Value = vendorcode;
            IVendoInfo vendoInfo = (IVendoInfo)e.Row.Tag;
            if (vendoInfo == null || !(e.Row.Tag is ICompGUID compGUID)) return;
            string guid = compGUID.GUID;
            string SensTable = vendoInfo.GetVendorInfo().MainDBTable;
            SQLiteCommand sQLiteCommand = new SQLiteCommand
            {
                Connection = connection,
                CommandText = $"UPDATE {SensTable} SET Vendor = '{vendorname}', VendorTable = '{vendoInfo.GetVendorInfo().DBTable}', VendorCode ='{vendorcode}' WHERE [GUID] = '{guid}'"
            };
            sQLiteCommand.ExecuteNonQuery();
            vendoInfo.SetVendorInfo(vendorname, vendorcode, vendordescription, null, null);
        }
        
        private void radTreeView1_MouseDown(object sender, MouseEventArgs e)
        {
            
        }

        private void radTreeView2_MouseClick(object sender, MouseEventArgs e)
        {
        }

        private void radGridView4_ContextMenuOpening(object sender, ContextMenuOpeningEventArgs e)
        {
            e.ContextMenu = radContextMenu5.DropDown;
        }

        private void radGridView3_CellBeginEdit(object sender, GridViewCellCancelEventArgs e)
        {
            if (!(radGridView3.ActiveEditor is RadDropDownListEditor dropDownEditor)) return;
            if (dropDownEditor.EditorElement is RadDropDownListEditorElement dropDownEditorElement) dropDownEditorElement.DefaultItemsCountInDropDown = 30;
        }

        private void bunifuImageButton2_Click(object sender, EventArgs e)
        {
            AbotForm abotForm = new AbotForm();
            DialogResult dr =  abotForm.ShowDialog();
            if (dr == DialogResult.Cancel)
            {
                abotForm.Close();
            }
        }
    }
    [DataContract]
    public class SerializeDictionary
    {
        [DataMember]
        public Dictionary<string, List<string>> Map_list { set; get; }
        public SerializeDictionary()
        {
            Map_list = new Dictionary<string, List<string>>();
        }
        private static DataContractSerializer serializer;
        public static void Serialize(string filePath, SerializeDictionary sTest)
        {
            serializer = new DataContractSerializer(typeof(SerializeDictionary));
            using (var writer = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                serializer.WriteObject(writer, sTest);
            }
        }
        public static SerializeDictionary DeSerialize(string filePath)
        {
            serializer = new DataContractSerializer(typeof(SerializeDictionary));
            SerializeDictionary serializeDictionary;
            using (var reader = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                serializeDictionary = serializer.ReadObject(reader) as SerializeDictionary;
            }
            return serializeDictionary;
        }
    }
    internal struct VendorFill
    {
        internal SQLiteConnection connection;
        internal void FillVendorList(RadDropDownList radDropDownList)
        {
            if (connection.State != ConnectionState.Open) return;
            SQLiteCommand sQLiteCommand = new SQLiteCommand
            {
                Connection = connection,
                CommandText = "SELECT VendorName FROM Vendors"
            };
            using (SQLiteDataReader reader = sQLiteCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    RadListDataItem radListDataItem = new RadListDataItem
                    {
                        Text = reader[0].ToString()
                    };
                    radDropDownList.Items.Add(radListDataItem);
                }
            }
            sQLiteCommand.Dispose();
            radDropDownList.Text = "Выбор вендора";
        }
        internal static void FillComponentColumns(RadGridView radGridView)
        {
            if (radGridView.Columns.Count > 0)
            {
                radGridView.Columns.Clear();
                radGridView.Rows.Clear();
            }
            
            radGridView.MasterTemplate.AutoGenerateColumns = false;
            GridViewComboBoxColumn comboCol = new GridViewComboBoxColumn("Описание")
            {
              FieldName = "Описание"
                
            };
            radGridView.Columns.Add(new GridViewTextBoxColumn("ID"));
            radGridView.Columns.Add(new GridViewTextBoxColumn("Тип датчика"));
            radGridView.Columns.Add(comboCol);
            radGridView.Columns.Add(new GridViewTextBoxColumn("Артикул"));
            radGridView.Columns["ID"].IsVisible = false;
            radGridView.Columns["Тип датчика"].TextAlignment = ContentAlignment.MiddleCenter;
            radGridView.Columns["Тип датчика"].AutoSizeMode = BestFitColumnMode.SummaryRowCells;
            radGridView.Columns["Тип датчика"].ReadOnly = true;
            radGridView.Columns["Описание"].TextAlignment = ContentAlignment.MiddleCenter;
            radGridView.Columns["Описание"].AutoSizeMode = BestFitColumnMode.SummaryRowCells;
            radGridView.Columns["Артикул"].TextAlignment = ContentAlignment.MiddleCenter;
            radGridView.Columns["Артикул"].ReadOnly = true;

            radGridView.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
            radGridView.Columns["Описание"].Width = 70;
            radGridView.Columns["Артикул"].Width = 30;

            //radGridView.DataSource = dataSource;
            //return dataSource;



        }
    }
    

}

