using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using AutoCAD;
//using Autodesk.AutoCAD.Runtime;
using System.Threading;
using System.Threading.Tasks;
using GKS_ASU_Loader;

namespace AOVGEN
{
#pragma warning disable IDE1006
    class ShemaASUTP
    {
        #region Set Autocad windw to front
        [DllImport("user32.dll")]
        internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_SHOWWINDOW = 0x0040;


        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);
        const uint WM_KEYDOWN = 0x0100;
        const int VK_ESCAPE = 0x01B;
        const int SC_ESCAPE = 0x10001;

        #endregion
        #region Set inital variables
        internal Dictionary<Pannel, List<(string, VentSystem)>> VentSystemS { get; set; }
        private const double YOffset = 53.84;
        private const double BasementYOffset = 133;
        private const double BasementXOffset = 30;
        private const double ElementXOffset = 10;
        private const double DOOffset = 62.5;
        private const double DIOffset = 57.5;
        private const double AOOffset = 52.5;
        private const double AIOffset = 47.5;
        private const double DispatchingOffset = 73;
        int AI, DI, AO, DO;
        int ventcompcnt;
        double[] shapkaPnt;
        internal string Author { get; set; }
        private ShemaASU shemaASU;
        private double[] basementPnt;
        Pannel pannel;
        internal List<string> nonUniqueSystemsNames;
        internal List<VentSystem> nonUniqueSystems;
        internal VentSystem curVentSystem;

        AcadDocument acadDoc;
        
        delegate AcadAttributeReference GetAttrib (AcadBlockReference acadBlock, string attrname);
        delegate AcadDynamicBlockReferenceProperty GetProperty (AcadBlockReference acadBlock, string propertyName);
        #endregion
        #region Set user classes
        protected class BlockTroubleException : System.Exception
        {
            public string BlockName { get; set; }
            public BlockTroubleException()
            {

            }

            public BlockTroubleException(string message)
                : base(String.Format("Invalid block: {0}", message))
            {

            }

            public BlockTroubleException(string message, System.Exception innerException)
                : base(message, innerException)
            { }
            

        }
        #endregion
        //[CommandMethod("NewDrawing", CommandFlags.Session)]
        [DllImport("ole32.dll")]
        static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);
        [DllImport("ole32.dll")]
        public static extern void GetRunningObjectTable(
            int reserved,
            out IRunningObjectTable prot);
        private List<object> GetRunningInstances(string[] progIds)
        {

            // get the app clsid
            List<string> clsIds = (from progId in progIds select Type.GetTypeFromProgID(progId) into type where type != null select type.GUID.ToString().ToUpper()).ToList();
            // get Running Object Table ...
            IRunningObjectTable Rot;
            GetRunningObjectTable(0, out Rot);
            if (Rot == null) return null;
            // get enumerator for ROT entries
            Rot.EnumRunning(out var monikerEnumerator);
            if (monikerEnumerator == null) return null;
            monikerEnumerator.Reset();
            List<object> instances = new List<object>();
            IntPtr pNumFetched = new IntPtr();
            IMoniker[] monikers = new IMoniker[1];
            // go through all entries and identifies app instances
            while (monikerEnumerator.Next(1, monikers, pNumFetched) == 0)
            {
                CreateBindCtx(0, out IBindCtx bindCtx);
                if (bindCtx == null) continue;
                monikers[0].GetDisplayName(bindCtx, null, out var displayName);
                foreach (var clsId in clsIds.Where(clsId => displayName.ToUpper().IndexOf(clsId) > 0))
                {
                    Rot.GetObject(monikers[0], out var ComObject);
                    if (ComObject == null) continue;
                    instances.Add(ComObject);
                    break;
                }
            }
            return instances;

        }
        private readonly string[] progIds =
        {
            "AutoCAD.Application.17",
            "AutoCAD.Application.17.1",
            "AutoCAD.Application.17.2",
            "AutoCAD.Application.18",
            "AutoCAD.Application.18.1",
            "AutoCAD.Application.18.2",
            "AutoCAD.Application.19",
            "AutoCAD.Application.19.1",
            "AutoCAD.Application.19.2",
            "AutoCAD.Application.20",
            "AutoCAD.Application.20.1",
            "AutoCAD.Application.20.2",
            "AutoCAD.Application.21",
            "AutoCAD.Application.21.1",
            "AutoCAD.Application.21.2",
            "AutoCAD.Application.23",
            "AutoCAD.Application.23.0"
        };

        internal int Execute()
        {
            int result = 0;
            List<object> instances = GetRunningInstances(progIds);
            dynamic GetAutoCad2019()
            {
                return instances
                    .Cast<dynamic>()
                    .FirstOrDefault(e => ((string)e.Version).Contains("23"));
            }
            
            (string path, string progID) Acad2019;
            Acad2019.progID = "AutoCAD.Application";
            Acad2019.path = @"\Autodesk\AutoCAD 2019\acad.exe";
            dynamic Acad2019COM = null;
            Task<dynamic> StartAcadTask = null;
            try
            {

                Acad2019COM = GetAutoCad2019();
                if (Acad2019COM == null)
                {
                    MessageBox.Show("Попытка запуска Autocad 2019\n" +
                                    "Или попробуйте запустить его вручную");
                    StartAcadTask = Task.Factory.StartNew(() => StartAutocad(Acad2019));
                }

            }
            catch
            {
                //cadApp = StartAutocad(Acad2019); //MessageBox.Show("Не нашел акад");
            }

            StartAcadTask?.Wait();
            if (Acad2019COM == null)
            {

                Acad2019COM = Marshal.GetActiveObject(Acad2019.progID);
                if (Acad2019COM == null) return result;
            }
            
            try
            {
                AutoCAD.AcadApplication acadApp = Acad2019COM;
                
                //Process[] localByName = Process.GetProcessesByName("acad");
                //Process process = localByName[0];
                //long pid = Acad2019COM.HWND;
                IntPtr hWnd = new IntPtr(Acad2019COM.HWND);
                if (hWnd != IntPtr.Zero)
                {
                    SetForegroundWindow(hWnd);
                    //SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                }
                acadApp.WindowState = AutoCAD.AcWindowState.acMax;
                System.Windows.Forms.SendKeys.Send("{ESC}");

                if (acadApp.Documents.Count > 0)
                {
                    foreach (AcadDocument acadDocument in acadApp.Documents)
                    {
                        if (acadDocument.Name == "Схема автоматизации.dwg")
                        {
                            acadDoc = acadDocument;
                            acadDoc.Activate();
                            System.Windows.Forms.SendKeys.Send("{ESC}"); //drop all selection and commands in Autocad
                            break;
                        }
                        else
                        {
                            AcadBlocks blocks = acadDocument.Blocks;
                            if (blocks.Count>0)
                            {
                                AcadBlock podval = (from AcadBlock block in blocks
                                    where block.Name == "Подвал1"
                                    select block).FirstOrDefault();
                                if (podval != null)
                                {
                                    acadDoc = acadDocument;
                                    acadDoc.Activate();
                                    System.Windows.Forms.SendKeys.Send("{ESC}");
                                    break;
                                }
                                   

                            }
                                                                
                        }
                    }
                }

                if (acadDoc == null)
                {
                        
                        
                    string docpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2021\GKSASU\AOVGen\ShchemaASU.dwt";
                    acadDoc = acadApp.Documents.Add(docpath);
                    acadDoc.Activate();
                }
                MainCycle();

                //acadDoc = acadApp.ActiveDocument;

            }
            catch (System.Exception ex)
            {
                if (ex.HResult == -2147467262)
                {
                    string message = "Не получилось найти среду для генерации схемы\n" +
                                     "Генерация должна быть выполнена в Autocad 2019\n" +
                                     "Если запущены разные версии Autocad, закройте лишние, оставьте 1 экземпляр v2019";
                    const string caption = "Ошибка!";
                    MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
                    

                return 0;
                //MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

            return result;
        }
        private ArrayList InsertDevBlock(double[] insertPoint, string blockname)
        {
            double[] min;
            double[] max;
            double size;
            var blockObj1 = acadDoc.ModelSpace.InsertBlock(insertPoint, blockname, 1, 1, 1, 0, Type.Missing);
            (blockObj1 as AutoCAD.AcadEntity).GetBoundingBox(out object minPt, out object maxPt);
            min = (double[])minPt;
            max = (double[])maxPt;
            size = max[0] - min[0];
            //startPnt[0] += size;

            AutoCAD.AcadBlockReference acadBlock = blockObj1;
            ArrayList list = new ArrayList
            {
                acadBlock,
                size
            };

            return list;

        }
        internal  T KeyByValue<T, W>(Dictionary<T, W> dict, W val)
        {
            T key = default;
            foreach (KeyValuePair<T, W> pair in dict)
            {
                if (EqualityComparer<W>.Default.Equals(pair.Value, val))
                {
                    key = pair.Key;
                    break;
                }
            }
            return key;
        }

        [Serializable] //обязательный атрибут, без него нельзя получить hash от класса/структуры. Этот атрибут говорит что поля можно представить в виде структуры, которую можно выгрузить, например, в xml или json. Но мне это нужно для того чтобы получить из структуры поток байтов, которые я и буду прогонять через md5 hash
        private struct SignificantInfo
        {

            internal string md5hashSens; //в ней, собственно, хранить буду имена блоков для схемы автоматизации
            internal string md5hashFiltr;
            internal string md5hashVent;
            internal string md5hashDamp;
            internal string md5hashExtVentPressure;


        }
        
        private List<(Pannel, Dictionary<string, List<VentSystem>>)> CheckEqual()
        {

            List<(Pannel, Dictionary<string, List<VentSystem>>)> ssList =
                new List<(Pannel, Dictionary<string, List<VentSystem>>)>(); //созадем список из кортежа вида (шкаф, словарь(md5Hash, список систм))
            foreach (var keyvalues in VentSystemS)
            {
                var equalDictionary = new Dictionary<string, List<VentSystem>>();//инициализируем пустой словарь
                var ventsystems = keyvalues.Value;//
                foreach (var (_, ventSystem) in ventsystems)//начинаем перебор вент.систем
                {
                    var Posinfos = ventSystem.ComponentsV2; //захватываем компоненты вент.системы
                    bool SupplyVentPresent = Posinfos //вычисляем, есть ли в вент.системе приточный вентилятор
                        .FirstOrDefault(e => e.Tag is SupplyVent || e.Tag is SpareSuplyVent) != null;
                    bool ExtVentPresent = Posinfos //вычисляем, есть ли вытяжной вентилятор
                        .FirstOrDefault(e => e.Tag is ExtVent || e.Tag is SpareExtVent) != null;
                    if (!SupplyVentPresent || ExtVentPresent) //если вент.система содержит только вытяжной вентилятор
                    {
                        SignificantInfo significantInfo = new SignificantInfo();//создаем экземпляр структуры
                        foreach (var component in from posInfo in Posinfos where (posInfo.Tag != null) select posInfo.Tag) //начинаем перебирать Posinfo в компонентах
                        {
                            switch (component.GetType().Name) //прыгаем по условиям в зависимости от имени компонента. Я проверяю только вот эти элементы
                            {
                                case nameof(ExtVent): //вытяжной вентилятор
                                    ExtVent extVent = (ExtVent)component;
                                    significantInfo.md5hashVent = extVent.ShemaASU.ShemaUp; //и  в структуру со значимой инфой кладу названия блоков для схемы автоматизации
                                    if (extVent._PressureContol != null)
                                    {
                                        significantInfo.md5hashExtVentPressure =extVent._PressureContol.ShemaASU.ShemaUp;
                                    }
                                    break;
                                case nameof(SpareExtVent): //сдвоенный вытяжной
                                    ExtVent mainExtVent = ((SpareExtVent)component).MainExtVent; //у сдвоенного я забираю инфу у основного вытяжного вентилятора, тк у резервного тоже самое
                                    significantInfo.md5hashVent =mainExtVent.ShemaASU.ShemaUp;
                                    if (mainExtVent._PressureContol != null)
                                    {
                                        significantInfo.md5hashExtVentPressure =mainExtVent._PressureContol.ShemaASU.ShemaUp; //если есть датчик перепада то забираю его название
                                    }
                                    break;
                                case nameof(CrossSection): //воздуховод
                                    CrossSection crossSection = (CrossSection)component;
                                    if (crossSection._SensorT != null)//там есть датчик Т
                                    {
                                        significantInfo.md5hashSens = crossSection._SensorT.ShemaASU.ShemaUp;
                                    }

                                    break;
                                case nameof(ExtDamper): //вытяжную заслонку
                                    significantInfo.md5hashDamp = ((ExtDamper)component).ShemaASU.ShemaUp;
                                    break;
                                case nameof(Filtr): //фильр, вытяжной фильтр
                                case nameof(ExtFiltr):
                                    significantInfo.md5hashFiltr = ((Filtr)component).ShemaASU.ShemaUp;
                                    break;
                            }

                        } //в общем, в результате я получаю структуру со значимой информацией, а именно 5 полей

                        string MD5Hash = EditorV2.MD5HashGenerator.GenerateKey(significantInfo); //генерирую хэш для этой структуры
                        if (!equalDictionary.ContainsKey(MD5Hash)) //проверяем словарь. если нет такого хэша в словаре
                        {
                            equalDictionary.Add(MD5Hash, new List<VentSystem> { ventSystem });//создаем ключ = хэшу и список, куда кладем нашу вент.систему

                        }
                        else
                        {
                            var ventSystems = equalDictionary[MD5Hash]; //а если есть такой ключ, то вынимаем список по ключу
                            ventSystems.Add(ventSystem); //и добавляем в этот список нашу вент.систему
                        }
                    }
                    else //а если есть приточный вентилятор в вент.системе
                    {
                        string MD5Hash = EditorV2.MD5HashGenerator.GenerateKey(ventSystem.GUID); //то я тупо генерирую хэш от GUID вент.системы 
                        if (!equalDictionary.ContainsKey(MD5Hash))
                        {
                            equalDictionary.Add(MD5Hash, new List<VentSystem> {ventSystem});//и кладу в словарь список с вент.системой. в результате приточки всегда будут считаться уникальными и у них всегда будет список из 1 вент.агрегата
                        }
                    } //таким образом, алгоритм нацелен на выявление отдельных вент.систем, которые не содержат приточных вентиляторов, то есть только вытяжки обычные
                    
                }
                (Pannel, Dictionary<string, List<VentSystem>>) myTuple; //создаем кортеж, в который кладем щит и наш словарь
                myTuple.Item1 = keyvalues.Key;
                myTuple.Item2 = equalDictionary;
                ssList.Add(myTuple); //добавляем кортеж в список и 
            }
            return ssList; //конец
            
        }
       

        private void MainCycle()
        {
            var dict = CheckEqual();//сразу идет функция проверки //смотри что прилетает сюда
            #region Get Autocad first insertion point
            const string prompt1 = "Вставьте блок";
            double[] startPnt;
            try
            {
                startPnt = acadDoc.Utility.GetPoint(Type.Missing, prompt1); //дальше начанется обычный алгоритм построения
            }
            catch
            {
                const string message = "Не смог получить точку вставки";
                const string caption = "Ошибка!";
                MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            
            double xi = startPnt[0];
            double yi = startPnt[1];
            
            // set start basement isertion point
            basementPnt = new [] { xi, yi-BasementYOffset, 0 };
            #endregion
            
            #region Draw Ventsystems
            double YOffsetForNextPannel = 0;
            // look over array with pannels and ventsystems
            
            foreach (var SplitList in dict)
            {
                pannel = SplitList.Item1;
                int maxYline = 0;
                int maxXline = 0;


                nonUniqueSystems = null;
                var ConnectedVents = SplitList.Item2;

                cnt = 1;
                //AI = DI = AO = DO = 0;
                double XOffsetForNextSystem = 0;
                nonUniqueSystemsNames = null;

                foreach (var keyValues in ConnectedVents)
                {
                    var VentSystems = keyValues.Value;
                    //если не уникальная вентсистма

                    if (VentSystems.Count > 1)
                    {
                        nonUniqueSystems = VentSystems;
                    }
                    nonUniqueSystemsNames = VentSystems
                        .Select(e => e.SystemName)
                        .ToList();
                    

                    curVentSystem = VentSystems[0]; //и вот еще, я всегда из списка забираю 1 элемент, вне зависимости от того, уникальный он или нет. если он уникальный, то он и так будет всего 1 в списке. а если не уникальный, то я забираю первый из них. ибо вторые и дальше мне как бы не нужны. единственное чего не реализовано, так это подсчет DI/DO

                    maxYline = curVentSystem.ComponentsV2
                        .Max(e => e.PozY);
                    double YOffsetByCompLine = maxYline > 1 ? 197 : 74;
                    ventcompcnt = 0;
                    startPnt[0] = xi + XOffsetForNextSystem; //смещение по Х для следующей вент.системы
                    startPnt[1] = yi + YOffsetForNextPannel; //смещение по Y если новый шкаф
                    shapkaPnt = new double[3]
                        {startPnt[0], basementPnt[1] + YOffsetForNextPannel - YOffsetByCompLine, basementPnt[2]};
                    var basement = acadDoc.ModelSpace.InsertBlock(shapkaPnt, "Подвал1", 1, 1, 1, 0, Type.Missing);
                    //pannel = GetKeyFromValue(VentSystemS, systemlist);

                    curVentSystem.ComponentsV2 = curVentSystem.ComponentsV2
                        .OrderBy(x => x.PozX)
                        .ToList()
                        .OrderBy(y => y.PozY)
                        .ToList();

                    #region NewVers

                    {
                        AcadBlockReference acadBlock = null;
                        GetAttrib posname = GetAttrFromBlock;
                        GetAttrib link = GetAttrFromBlock;
                        if (curVentSystem.ComponentsV2.Count == 0) return;

                        AI = DI = AO = DO = 0;
                        foreach (EditorV2.PosInfo posInfo in curVentSystem.ComponentsV2)
                        {
                            object component = posInfo.Tag;
                            shemaASU = null;
                            switch (component.GetType().Name)
                            {
                                case nameof(CrossSection):
                                case nameof(Room):
                                {
                                    dynamic comp = component;
                                    shemaASU = comp.ShemaASU;
                                    if (shemaASU != null)
                                    {
                                        acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
                                        shemaASU = null;
                                    }

                                    shemaASU = comp._SensorT?.ShemaASU;
                                    if (shemaASU != null)
                                    {
                                        if (comp._SensorH == null)
                                        {
                                            switch (component.GetType().Name)
                                            {
                                                case nameof(Room):
                                                    acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo,
                                                        new double[] {25, 33});
                                                    break;
                                                case nameof(CrossSection):
                                                    acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo,
                                                        new double[] {25, 33});
                                                    break;
                                            }

                                        }
                                        else
                                        {
                                            switch (component.GetType().Name)
                                            {
                                                case nameof(Room):
                                                    acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo,
                                                        new double[] {12.5, 33});
                                                    break;
                                                case nameof(CrossSection):
                                                    SetCrossSectionShemaASUName();
                                                    acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo,
                                                        new double[] {12.5, 33});
                                                    break;
                                            }

                                            void SetCrossSectionShemaASUName()
                                            {

                                                // "cross1", "cross1T", "cross1TH", "cross2", "cross3", "cross4", "cross5", "cross6", "cross7", "cross8", "cross9", "cross10", "cross11"




                                            }

                                        }

                                        posname(acadBlock, "POSNAME").TextString = comp._SensorT.PosName;
                                        link(acadBlock, "LINK").TextString = cnt.ToString();
                                        shemaASU.ShemaLink1 = cnt;
                                        shemaASU.Description1 = comp._SensorT.Description;
                                        cnt++;
                                        shemaASU = null;
                                    }

                                    shemaASU = comp._SensorH?.ShemaASU;
                                    if (shemaASU != null)
                                    {
                                        acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo, new double[] {37.5, 33});
                                        posname(acadBlock, "POSNAME").TextString = comp._SensorH.PosName;
                                        link(acadBlock, "LINK").TextString = cnt.ToString();
                                        shemaASU.ShemaLink1 = cnt;
                                        shemaASU.Description1 = comp._SensorH.Description;
                                        cnt++;
                                        shemaASU = null;


                                    }
                                }
                                    break;
                                case nameof(OutdoorTemp):
                                    OutdoorTemp outdoorTemp = (OutdoorTemp) component;
                                    shemaASU = outdoorTemp.ShemaASU;
                                    acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
                                    posname(acadBlock, "POSNAME").TextString = outdoorTemp.PosName;
                                    link(acadBlock, "LINK").TextString = cnt.ToString();
                                    shemaASU.ShemaLink1 = cnt;
                                    shemaASU.Description1 = outdoorTemp.Description;
                                    cnt++;

                                    break;
                                case nameof(SupplyDamper):
                                case nameof(ExtDamper):
                                    dynamic Damper = component;
                                    if (component.GetType().Name == nameof(SupplyDamper))
                                    {
                                        OutdoorTemp outdoor = Damper.outdoorTemp;
                                        if (outdoor != null)
                                        {
                                            shemaASU = outdoor.ShemaASU;


                                            acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo,
                                                new double[] {13, 32.67, 0});
                                            posname(acadBlock, "POSNAME").TextString = outdoor.PosName;
                                            link(acadBlock, "LINK").TextString = cnt.ToString();
                                            shemaASU.ShemaLink1 = cnt;
                                            shemaASU.Description1 = outdoor.Description;
                                            cnt++;

                                        }
                                    }

                                    shemaASU = Damper.ShemaASU;
                                    acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
                                    posname(acadBlock, "POSNAME").TextString = Damper.PosName;
                                    link(acadBlock, "LINK").TextString = cnt.ToString();
                                    shemaASU.ShemaLink1 = cnt;
                                    shemaASU.Description1 = Damper.Description1;
                                    cnt++;
                                    break;
                                case nameof(Recuperator):
                                    Recuperator recuperator = (Recuperator) component;
                                    shemaASU = recuperator.ShemaASU;
                                    acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
                                    switch (recuperator._RecuperatorType)
                                    {
                                        case Recuperator.RecuperatorType.Recirculation:
                                            posname(acadBlock, "POSNAME_ACTUATOR1").TextString =
                                                recuperator.Drive1.Posname;
                                            posname(acadBlock, "POSNAME_ACTUATOR2").TextString =
                                                recuperator.Drive2.Posname;
                                            posname(acadBlock, "POSNAME_ACTUATOR3").TextString =
                                                recuperator.Drive3.Posname;
                                            link(acadBlock, "LINK1").TextString = cnt.ToString();
                                            recuperator.Drive1.ShemaASU.ShemaLink1 = cnt;
                                            shemaASU = recuperator.Drive1.ShemaASU;
                                            shemaASU.Description1 = recuperator.Drive1.Description;
                                            cnt++;
                                            link(acadBlock, "LINK2").TextString = (cnt).ToString();
                                            recuperator.Drive2.ShemaASU.ShemaLink1 = cnt;
                                            shemaASU = recuperator.Drive2.ShemaASU;
                                            shemaASU.Description1 = recuperator.Drive2.Description;
                                            cnt++;
                                            link(acadBlock, "LINK3").TextString = (cnt).ToString();
                                            recuperator.Drive3.ShemaASU.ShemaLink1 = cnt;
                                            shemaASU = recuperator.Drive3.ShemaASU;
                                            shemaASU.Description1 = recuperator.Drive3.Description;
                                            cnt++;
                                            break;
                                        case Recuperator.RecuperatorType.RotorControl:
                                            posname(acadBlock, "POSNAME_ACTUATOR").TextString =
                                                recuperator.Drive1.Posname;
                                            link(acadBlock, "LINK1").TextString = cnt.ToString();
                                            recuperator.Drive1.ShemaASU.ShemaLink1 = cnt;
                                            shemaASU = recuperator.Drive1.ShemaASU;
                                            shemaASU.Description1 = recuperator.Drive1.Description;
                                            cnt++;
                                            posname(acadBlock, "POSNAME_PRESD").TextString =
                                                recuperator.protectSensor2.PosName;
                                            link(acadBlock, "LINK2").TextString = cnt.ToString();
                                            recuperator.protectSensor2.ShemaASU.ShemaLink1 = cnt;
                                            shemaASU = recuperator.protectSensor2.ShemaASU;
                                            shemaASU.Description1 = recuperator.protectSensor2.Description;
                                            cnt++;
                                            break;
                                        case Recuperator.RecuperatorType.LaminatedNoBypass:
                                            posname(acadBlock, "POSNAME_PRESD").TextString =
                                                recuperator.protectSensor2.PosName;
                                            link(acadBlock, "LINK").TextString = cnt.ToString();
                                            recuperator.protectSensor2.ShemaASU.ShemaLink1 = cnt;
                                            shemaASU = recuperator.protectSensor2.ShemaASU;
                                            shemaASU.Description1 = recuperator.protectSensor2.Description;
                                            cnt++;
                                            break;
                                        case Recuperator.RecuperatorType.LaminatedBypass:
                                            posname(acadBlock, "POSNAME_TEMP").TextString =
                                                recuperator.protectSensor1.PosName;
                                            link(acadBlock, "LINK1").TextString = cnt.ToString();
                                            recuperator.protectSensor1.ShemaASU.ShemaLink1 = cnt;
                                            shemaASU = recuperator.protectSensor1.ShemaASU;
                                            shemaASU.Description1 = recuperator.protectSensor1.Description;
                                            cnt++;
                                            posname(acadBlock, "POSNAME_BYPASS").TextString =
                                                recuperator.Drive1?.Posname;
                                            link(acadBlock, "LINK2").TextString = cnt.ToString();
                                            recuperator.Drive1.ShemaASU.ShemaLink1 = cnt;
                                            shemaASU = recuperator.Drive1.ShemaASU;
                                            shemaASU.Description1 = recuperator.Drive1.Description;
                                            cnt++;
                                            posname(acadBlock, "POSNAME_PRESD").TextString =
                                                recuperator.protectSensor2?.PosName;
                                            link(acadBlock, "LINK3").TextString = cnt.ToString();
                                            recuperator.protectSensor2.ShemaASU.ShemaLink1 = cnt;
                                            shemaASU = recuperator.protectSensor2.ShemaASU;
                                            shemaASU.Description1 = recuperator.protectSensor2.Description;
                                            cnt++;

                                            break;
                                        case Recuperator.RecuperatorType.Glycol:
                                            posname(acadBlock, "POSNAME_VALVE").TextString =
                                                recuperator.Drive1?.Posname;
                                            posname(acadBlock, "POSNAME_PUMP").TextString = recuperator.Drive2?.Posname;
                                            posname(acadBlock, "POSNAME_PRESD").TextString =
                                                recuperator.protectSensor2.PosName;
                                            link(acadBlock, "LINK1").TextString = cnt.ToString();
                                            recuperator.Drive1.ShemaASU.ShemaLink1 = cnt;
                                            shemaASU = recuperator.Drive1.ShemaASU;
                                            shemaASU.Description1 = recuperator.Drive1.Description;
                                            cnt++;
                                            link(acadBlock, "LINK2").TextString = (cnt).ToString();
                                            recuperator.Drive2.ShemaASU.ShemaLink1 = cnt;
                                            shemaASU = recuperator.Drive2.ShemaASU;
                                            shemaASU.Description1 = recuperator.Drive2.Description;
                                            cnt++;
                                            link(acadBlock, "LINK3").TextString = (cnt).ToString();
                                            recuperator.protectSensor2.ShemaASU.ShemaLink1 = cnt;
                                            shemaASU = recuperator.protectSensor2.ShemaASU;
                                            shemaASU.Description1 = recuperator.protectSensor2.Description;
                                            cnt++;
                                            break;
                                    }

                                    break;
                                case nameof(SupplyVent):
                                case nameof(ExtVent):

                                    dynamic vent = component;
                                    shemaASU = vent.ShemaASU;
                                    acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
                                    Vent.AHUContolType aHUContolType = vent.ControlType;
                                    switch (aHUContolType)
                                    {
                                        case Vent.AHUContolType.Direct:
                                            posname(acadBlock, "POSNAME").TextString = vent.PosName;
                                            link(acadBlock, "LINK").TextString = cnt.ToString();
                                            break;
                                        case Vent.AHUContolType.Soft:
                                        case Vent.AHUContolType.FCControl:
                                        case Vent.AHUContolType.Transworm:
                                            posname(acadBlock, "POSNAME1").TextString = vent.PosName;
                                            link(acadBlock, "LINK").TextString = cnt.ToString();
                                            posname(acadBlock, "POSNAME2").TextString = "RO";
                                            break;

                                    }

                                    shemaASU.ShemaLink1 = cnt;
                                    //shemaASU.Description = vent.Description;
                                    cnt++;
                                    if (vent._PressureContol != null)
                                    {
                                        PressureContol pressureContol = vent._PressureContol;
                                        shemaASU = pressureContol.ShemaASU;

                                        acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
                                        posname(acadBlock, "POSNAME").TextString = pressureContol.PosName;
                                        link(acadBlock, "LINK").TextString = cnt.ToString();
                                        shemaASU.ShemaLink1 = cnt;
                                        shemaASU.Description1 = pressureContol.Description;
                                        cnt++;

                                    }

                                    break;
                                case nameof(SpareSuplyVent):
                                case nameof(SpareExtVent):
                                    dynamic sparevent = component;
                                    dynamic MainVent = null;
                                    dynamic Reserved = null;
                                    switch (component.GetType().Name)
                                    {
                                        case nameof(SpareSuplyVent):
                                            MainVent = sparevent.MainSupplyVent;
                                            Reserved = sparevent.ReservedSupplyVent;
                                            break;
                                        case nameof(SpareExtVent):
                                            MainVent = sparevent.MainExtVent;
                                            Reserved = sparevent.ReservedExtVent;
                                            break;
                                    }

                                    shemaASU = sparevent.shemaAsu;
                                    acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo); //отрисовали верхний блок
                                    Vent.AHUContolType aHUSpareContolType = sparevent.ControlType;
                                    switch (aHUSpareContolType)
                                    {
                                        case Vent.AHUContolType.Direct:
                                            posname(acadBlock, "POSNAME1").TextString =
                                                MainVent.PosName; //заполнили поз.1
                                            posname(acadBlock, "POSNAME2").TextString =
                                                Reserved.PosName; //заполнили поз2
                                            link(acadBlock, "LINK1").TextString =
                                                cnt.ToString(); //заполнили номер ссылки 1 наверху
                                            shemaASU = MainVent.ShemaASU;
                                            shemaASU.ShemaLink1 = cnt; //запомнили ссылку 1
                                            cnt++; //вот тут будет отрисовка низа
                                            link(acadBlock, "LINK2").TextString =
                                                cnt.ToString(); //заполнили номер ссылки 2 наверху
                                            shemaASU = Reserved.ShemaASU;
                                            shemaASU.ShemaLink1 = cnt; //запомнили ссылку 2
                                            cnt++;
                                            break;
                                        case Vent.AHUContolType.Soft:
                                        case Vent.AHUContolType.FCControl:
                                        case Vent.AHUContolType.Transworm:
                                            posname(acadBlock, "POSNAME1").TextString = MainVent.PosName;
                                            posname(acadBlock, "POSNAME2").TextString = Reserved.PosName;
                                            posname(acadBlock, "POSNAME3").TextString = "RO";
                                            posname(acadBlock, "POSNAME4").TextString = "RO";
                                            link(acadBlock, "LINK1").TextString = cnt.ToString();
                                            shemaASU = MainVent.ShemaASU;
                                            shemaASU.ShemaLink1 = cnt;
                                            cnt++;
                                            link(acadBlock, "LINK2").TextString = cnt.ToString();
                                            shemaASU = Reserved.ShemaASU;
                                            shemaASU.ShemaLink1 = cnt;
                                            cnt++;
                                            break;

                                    }

                                    if (sparevent._PressureContol != null)
                                    {
                                        PressureContol pressureContol = sparevent._PressureContol;
                                        shemaASU = pressureContol.ShemaASU;
                                        acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
                                        posname(acadBlock, "POSNAME").TextString = pressureContol.PosName;
                                        link(acadBlock, "LINK").TextString = cnt.ToString();
                                        shemaASU.ShemaLink1 = cnt;
                                        shemaASU.Description1 = pressureContol.Description;
                                        cnt++;

                                    }

                                    break;

                                case nameof(WaterHeater):
                                    WaterHeater waterHeater = (WaterHeater) component;
                                    shemaASU = waterHeater.ShemaASU;

                                    acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
                                    posname(acadBlock, "POSNAME_PUMP").TextString = waterHeater._Pump.PosName;
                                    ;
                                    posname(acadBlock, "POSNAME_Valve").TextString = waterHeater._Valve.Posname;
                                    ;
                                    link(acadBlock, "LINK1").TextString = cnt.ToString();
                                    if (waterHeater._Pump != null) waterHeater._Pump.ShemaASU.ShemaLink1 = cnt;
                                    shemaASU = waterHeater._Pump?.ShemaASU;
                                    shemaASU.Description1 = waterHeater._Pump?.Description;
                                    cnt++;
                                    link(acadBlock, "LINK2").TextString = cnt.ToString();
                                    if (waterHeater._Valve != null) waterHeater._Valve.ShemaASU.ShemaLink1 = cnt;
                                    shemaASU = waterHeater._Valve?.ShemaASU;
                                    shemaASU.Description1 = waterHeater._Valve?.Description;
                                    cnt++;
                                    if (waterHeater.PS2 != null)
                                    {
                                        SensorT sensorPS2 = waterHeater.PS2;
                                        shemaASU = sensorPS2.ShemaASU;
                                        //Сделать блоки с датчиками и без них, не рисовать отдельно датчики

                                        acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
                                        posname(acadBlock, "POSNAME").TextString = waterHeater.PS2.PosName;
                                        ;
                                        link(acadBlock, "LINK").TextString = cnt.ToString();
                                        shemaASU.ShemaLink1 = cnt;
                                        shemaASU.Description1 = sensorPS2.Description;
                                        cnt++;
                                    }

                                    if (waterHeater.PS1 != null)
                                    {
                                        SensorT sensorPS1 = waterHeater.PS1;
                                        shemaASU = sensorPS1.ShemaASU;
                                        //Сделать блоки с датчиками и без них, не рисовать отдельно датчики

                                        acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
                                        posname(acadBlock, "POSNAME").TextString = waterHeater.PS1.PosName;
                                        ;
                                        link(acadBlock, "LINK").TextString = cnt.ToString();
                                        shemaASU.ShemaLink1 = cnt;
                                        shemaASU.Description1 = sensorPS1.Description;
                                        cnt++;
                                    }


                                    break;
                                case nameof(ElectroHeater):
                                    ElectroHeater electroHeater = (ElectroHeater) component;
                                    shemaASU = electroHeater.ShemaASU;
                                    acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
                                    posname(acadBlock, "POSNAME").TextString = electroHeater.PosName;
                                    ;
                                    link(acadBlock, "LINK").TextString = cnt.ToString();
                                    shemaASU.ShemaLink1 = cnt;
                                    shemaASU.Description1 = electroHeater.Description;
                                    cnt++;

                                    break;
                                case nameof(Froster):
                                    Froster froster = (Froster) component;
                                    shemaASU = froster.ShemaASU;
                                    shemaASU.ShemaLink1 = cnt;
                                    acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
                                    switch (froster._FrosterType)
                                    {
                                        case Froster.FrosterType.Freon:

                                            link(acadBlock, "LINK").TextString = cnt.ToString();
                                            shemaASU.ShemaLink1 = cnt;
                                            shemaASU.Description1 = froster._KKB.Description;
                                            cnt++;
                                            break;
                                        case Froster.FrosterType.Water:
                                            posname(acadBlock, "POSNAME_Valve").TextString = froster._Valve.Posname;
                                            link(acadBlock, "LINK").TextString = cnt.ToString();
                                            froster._Valve.ShemaASU.ShemaLink1 = cnt;
                                            shemaASU = froster._Valve.ShemaASU;
                                            shemaASU.Description1 = froster._Valve.Description;
                                            cnt++;
                                            break;
                                    }

                                    if (froster.Sens1 != null)
                                    {
                                        Froster.ProtectSensor PS1 = froster.Sens1;
                                        shemaASU = PS1.ShemaASU;
                                        shemaASU.Description1 = PS1.Description;
                                    }

                                    if (froster.Sens2 != null)
                                    {
                                        Froster.ProtectSensor PS2 = froster.Sens2;
                                        shemaASU = PS2.ShemaASU;
                                        shemaASU.Description1 = PS2.Description;
                                    }

                                    break;
                                case nameof(Humidifier):
                                    Humidifier humidifier = (Humidifier) component;
                                    shemaASU = humidifier.ShemaASU;
                                    acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
                                    posname(acadBlock, "POSNAME").TextString = humidifier.PosName;
                                    link(acadBlock, "LINK").TextString = cnt.ToString();
                                    shemaASU.ShemaLink1 = cnt;
                                    shemaASU.Description1 = humidifier.Description;
                                    cnt++;
                                    if (humidifier.HumSensPresent)
                                    {

                                        shemaASU = humidifier.HumiditySensor.ShemaASU;
                                        acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
                                        posname(acadBlock, "POSNAME").TextString = humidifier.SensPosName;
                                        link(acadBlock, "LINK").TextString = cnt.ToString();
                                        shemaASU.ShemaLink1 = cnt;
                                        shemaASU.Description1 = humidifier.HumiditySensor.Description;
                                        cnt++;
                                    }

                                    break;
                                case nameof(SupplyTemp):
                                    SupplyTemp supplyTemp = (SupplyTemp) component;
                                    shemaASU = supplyTemp.ShemaASU;
                                    acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
                                    posname(acadBlock, "POSNAME").TextString = supplyTemp.PosName;
                                    link(acadBlock, "LINK").TextString = cnt.ToString();
                                    shemaASU.ShemaLink1 = cnt;
                                    shemaASU.Description1 = supplyTemp.Description;
                                    cnt++;
                                    break;
                                case nameof(IndoorTemp):
                                    IndoorTemp indoorTemp = (IndoorTemp) component;
                                    shemaASU = indoorTemp.ShemaASU;
                                    acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
                                    posname(acadBlock, "POSNAME").TextString = indoorTemp.PosName;
                                    link(acadBlock, "LINK").TextString = cnt.ToString();
                                    shemaASU.ShemaLink1 = cnt;
                                    shemaASU.Description1 = indoorTemp.Description;
                                    cnt++;
                                    break;


                                case nameof(ExhaustTemp):
                                    ExhaustTemp exhaustTemp = (ExhaustTemp) component;
                                    shemaASU = exhaustTemp.ShemaASU;
                                    acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
                                    posname(acadBlock, "POSNAME").TextString = exhaustTemp.PosName;
                                    link(acadBlock, "LINK").TextString = cnt.ToString();
                                    shemaASU.ShemaLink1 = cnt;
                                    shemaASU.Description1 = exhaustTemp.Description;
                                    cnt++;
                                    break;

                                case nameof(Filtr):
                                case nameof(ExtFiltr):
                                case nameof(SupplyFiltr):
                                    dynamic filtr = component;
                                    shemaASU = filtr.ShemaASU;
                                    acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
                                    if (filtr._PressureContol != null)
                                    {
                                        PressureContol pressureContol = filtr._PressureContol;
                                        shemaASU = pressureContol.ShemaASU;
                                        acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
                                        posname(acadBlock, "POSNAME").TextString = pressureContol.PosName;
                                        link(acadBlock, "LINK").TextString = cnt.ToString();
                                        pressureContol.ShemaASU.ShemaLink1 = cnt;
                                        shemaASU.Description1 = pressureContol.Description;
                                        cnt++;
                                    }

                                    break;

                            }
                        }
                    }

                    #endregion

                    #region Displacesement for next system

                    maxXline = curVentSystem.ComponentsV2
                        .Max(e => e.PozX);
                    
                    double basementoffset = 40 + (ventcompcnt + 3) * 10;

                    if (maxXline <= 4 && maxYline <= 1)
                    {
                        XOffsetForNextSystem += 420;

                        //draw frame
                        AcadBlockReference frameblock = DrawFrame();
                        object[] blockproperies = frameblock.GetDynamicBlockProperties();
                        object[] basementpropyrties = basement.GetDynamicBlockProperties();
                        IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> frameType = blockproperies
                            .OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
                            .Where(i => i.PropertyName == "Выбор формата");
                        frameType.ElementAt(0).Value = "A3альбом";

                        //draw stamp
                        AcadBlockReference stamp = DrawStamp(frameblock, "A3");
                        

                        //set basement size
                        IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> basementProp = basementpropyrties
                            .OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
                            .Where(i => i.PropertyName == "Расстояние2");
                        //basementProp.ElementAt(0).Value -= 77.96;
                        basementProp.ElementAt(0).Value = basementoffset;
                        DrawApplicability_table(frameblock, "A3");


                    }
                    else if (maxXline > 4 && maxXline <= 9 && maxYline <= 1)
                    {
                        XOffsetForNextSystem += 630;

                        //draw frame
                        AcadBlockReference frameblock = DrawFrame();
                        object[] blockproperies = frameblock.GetDynamicBlockProperties();
                        object[] basementpropyrties = basement.GetDynamicBlockProperties();
                        IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> frameType = blockproperies
                            .OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
                            .Where(i => i.PropertyName == "Выбор формата");
                        frameType.ElementAt(0).Value = "А4х3";

                        //draw stamp
                        AcadBlockReference stamp = DrawStamp(frameblock, "A4x3");

                        //set basement size
                        IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> basementProp = basementpropyrties
                            .OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
                            .Where(i => i.PropertyName == "Расстояние2");
                        basementProp.ElementAt(0).Value = basementoffset;
                        DrawApplicability_table(frameblock, "A4x3");


                    }
                    else if (maxXline > 9 && maxYline <= 1)
                    {
                        XOffsetForNextSystem += 891;

                        //draw frame
                        AcadBlockReference frameblock = DrawFrame();
                        object[] blockproperies = frameblock.GetDynamicBlockProperties();
                        object[] basementpropyrties = basement.GetDynamicBlockProperties();
                        IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> frameType = blockproperies
                            .OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
                            .Where(i => i.PropertyName == "Выбор формата");
                        frameType.ElementAt(0).Value = "А4х4";

                        //draw stamp
                        AcadBlockReference stamp = DrawStamp(frameblock, "А4х4");

                        //set basement size

                        IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> basementProp = basementpropyrties
                            .OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
                            .Where(i => i.PropertyName == "Расстояние2");



                        basementProp.ElementAt(0).Value = basementoffset;
                        DrawApplicability_table(frameblock, "А4х4");


                    }
                    else if (maxXline > 9 && maxYline > 1)
                    {
                        XOffsetForNextSystem += 891;

                        //draw frame
                        AcadBlockReference frameblock = DrawFrame();
                        object[] blockproperies = frameblock.GetDynamicBlockProperties();
                        object[] basementpropyrties = basement.GetDynamicBlockProperties();
                        IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> frameType = blockproperies
                            .OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
                            .Where(i => i.PropertyName == "Выбор формата");
                        frameType.ElementAt(0).Value = "А3х3";

                        //draw stamp
                        AcadBlockReference stamp = DrawStamp(frameblock, "А3х3");

                        //set basement size

                        IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> basementProp = basementpropyrties
                            .OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
                            .Where(i => i.PropertyName == "Расстояние2");



                        basementProp.ElementAt(0).Value = basementoffset;
                        DrawApplicability_table(frameblock, "А3х3");


                    }
                    else if (maxYline > 1) //возможно нужно повернуть А3
                    {
                        AcadBlockReference frameblock = DrawFrame();
                        object[] blockproperies = frameblock.GetDynamicBlockProperties();
                        object[] basementpropyrties = basement.GetDynamicBlockProperties();
                        IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> frameType = blockproperies
                            .OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
                            .Where(i => i.PropertyName == "Выбор формата");
                        frameType.ElementAt(0).Value = "А3х3";

                        //draw stamp
                        AcadBlockReference stamp = DrawStamp(frameblock, "А3х3");

                        //set basement size

                        IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> basementProp = basementpropyrties
                            .OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
                            .Where(i => i.PropertyName == "Расстояние2");



                        basementProp.ElementAt(0).Value = basementoffset;
                        DrawApplicability_table(frameblock, "А3х3");

                    }


                    #endregion

                    #region Set basement attributes

                    object[] attributes = basement.GetAttributes();
                    IEnumerable<AutoCAD.AcadAttributeReference> basementAttribut = attributes
                        .OfType<AcadAttributeReference>()
                        .Where(i => i.TagString == "МАРКА_ЩИТА_C");
                    basementAttribut.ElementAt(0).TextString = pannel.PannelName;

                    #endregion

                    //DrawRoomBlock();

                    #region Draw dispatching line and calculated IO
                    (int, int, int, int) IO = (AI, AO, DI, DO);

                    if (pannel.Dispatching != Pannel._Dispatching.No)
                    {
                        MakeDispatcing(shapkaPnt, ventcompcnt + 1, IO);
                        MakeDispatchingSignals(shapkaPnt, ventcompcnt + 2, IO);

                    }
                    else
                    {
                        MakeDispatchingSignals(shapkaPnt, ventcompcnt + 1, IO);
                    }

                    #endregion

                }
                //displaysment
                if (maxXline <= 4 && maxYline <= 1)
                {
                    YOffsetForNextPannel -= 297;
                }
                else if (maxXline > 4 && maxXline <= 9 && maxYline <= 1)
                {
                    YOffsetForNextPannel -= 297;
                }
                else if (maxXline > 9 && maxYline <= 1)
                {
                    YOffsetForNextPannel -= 297;
                }
                else if (maxXline > 9 && maxYline > 1)
                {
                    YOffsetForNextPannel -= 594;
                }
                else if (maxYline > 1)
                {
                    YOffsetForNextPannel -= 420;
                }
            }
            
            
            //foreach (var systemlist in VentSystemS.Values)
            //{
            //    cnt = 1;
            //    AI = DI = AO = DO = 0;
            //    double XOffsetForNextSystem = 0;
                
            //    //look over array with ventsystems
            //    foreach (var tuple in systemlist)
            //    {
            //        VentSystem ventSystem = tuple.Item2;


            //        int maxYline = ventSystem.ComponentsV2
            //            .Max(e => e.PozY);
            //        double YOffsetByCompLine = maxYline > 1 ? 197 : 74;
            //        ventcompcnt = 0;
            //        startPnt[0] = xi + XOffsetForNextSystem; //смещение по Х для следующей вент.системы
            //        startPnt[1] = yi + YOffsetForNextPannel; //смещение по Y если новый шкаф
            //        shapkaPnt = new double[3] { startPnt[0], basementPnt[1]+ YOffsetForNextPannel - YOffsetByCompLine, basementPnt[2] };
            //        var basement = acadDoc.ModelSpace.InsertBlock(shapkaPnt, "Подвал1", 1, 1, 1, 0, Type.Missing);


            //        pannel = GetKeyFromValue(VentSystemS, systemlist);
                    

            //        ventSystem.ComponentsV2 = ventSystem.ComponentsV2
            //            .OrderBy(x => x.PozX)
            //            .ToList()
            //            .OrderBy(y => y.PozY)
            //            .ToList();


                    

            //        #region NewVers
                   
            //        {
            //            AcadBlockReference acadBlock = null;
            //            GetAttrib posname = GetAttrFromBlock;
            //            GetAttrib link = GetAttrFromBlock;
            //            if (ventSystem.ComponentsV2.Count==0) return;
                        
            //            foreach (EditorV2.PosInfo posInfo in ventSystem.ComponentsV2)
            //            {
            //                object component = posInfo.Tag;
            //                shemaASU = null;
            //                switch (component.GetType().Name)
            //                {
            //                    case nameof(CrossSection):
            //                    case nameof(Room):
            //                        {
            //                            dynamic comp = component;
            //                            shemaASU = comp.ShemaASU;
            //                            if (shemaASU != null)
            //                            {
            //                                acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
            //                                shemaASU = null;
            //                            }
            //                            shemaASU = comp._SensorT?.ShemaASU;
            //                            if (shemaASU != null)
            //                            {
            //                                if (comp._SensorH == null)
            //                                {
            //                                    switch(component.GetType().Name)
            //                                    {
            //                                        case nameof(Room):
            //                                            acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo, new double[] { 25, 33 });
            //                                            break;
            //                                        case nameof(CrossSection):
            //                                            acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo, new double[] { 25, 33 });
            //                                            break;
            //                                    }
                                                
            //                                }
            //                                else
            //                                {
            //                                    switch (component.GetType().Name)
            //                                    {
            //                                        case nameof(Room):
            //                                            acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo, new double[] { 12.5, 33 });
            //                                            break;
            //                                        case nameof(CrossSection):
            //                                            SetCrossSectionShemaASUName();
            //                                            acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo, new double[] { 12.5, 33 });
            //                                            break;
            //                                    }
            //                                    void SetCrossSectionShemaASUName()
            //                                    {

            //                                       // "cross1", "cross1T", "cross1TH", "cross2", "cross3", "cross4", "cross5", "cross6", "cross7", "cross8", "cross9", "cross10", "cross11"


                                                    

            //                                    }
                                                
            //                                }
                                            
            //                                posname(acadBlock, "POSNAME").TextString = comp._SensorT.PosName;
            //                                link(acadBlock, "LINK").TextString = cnt.ToString();
            //                                shemaASU.ShemaLink1 = cnt;
            //                                shemaASU.Description1 = comp._SensorT.Description;
            //                                cnt++;
            //                                shemaASU = null;
            //                            }
            //                            shemaASU = comp._SensorH?.ShemaASU;
            //                            if (shemaASU != null)
            //                            {
            //                                acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo, new double[] { 37.5, 33 });
            //                                posname(acadBlock, "POSNAME").TextString = comp._SensorH.PosName;
            //                                link(acadBlock, "LINK").TextString = cnt.ToString();
            //                                shemaASU.ShemaLink1 = cnt;
            //                                shemaASU.Description1 = comp._SensorH.Description;
            //                                cnt++;
            //                                shemaASU = null;
                                            
                                            
            //                            }
            //                        }
            //                        break;
            //                    case nameof(OutdoorTemp):
            //                        OutdoorTemp outdoorTemp = (OutdoorTemp)component;
            //                        shemaASU = outdoorTemp.ShemaASU;
            //                        acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
            //                        posname(acadBlock, "POSNAME").TextString = outdoorTemp.PosName;
            //                        link(acadBlock, "LINK").TextString = cnt.ToString();
            //                        shemaASU.ShemaLink1 = cnt;
            //                        shemaASU.Description1 = outdoorTemp.Description;
            //                        cnt++;

            //                        break;
            //                    case nameof(SupplyDamper):
            //                    case nameof(ExtDamper):
            //                        dynamic Damper = component;
            //                        if (component.GetType().Name == nameof(SupplyDamper))
            //                        {
            //                            OutdoorTemp outdoor = Damper.outdoorTemp;
            //                            if (outdoor != null)
            //                            {
            //                                shemaASU = outdoor.ShemaASU;
                                            

            //                                acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo, new double[] { 13, 32.67, 0});
            //                                posname(acadBlock, "POSNAME").TextString = outdoor.PosName;
            //                                link(acadBlock, "LINK").TextString = cnt.ToString();
            //                                shemaASU.ShemaLink1 = cnt;
            //                                shemaASU.Description1 = outdoor.Description;
            //                                cnt++;

            //                            }
            //                        }
            //                        shemaASU = Damper.ShemaASU;
            //                        acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
            //                        posname(acadBlock, "POSNAME").TextString = Damper.PosName;
            //                        link(acadBlock, "LINK").TextString = cnt.ToString();
            //                        shemaASU.ShemaLink1 = cnt;
            //                        shemaASU.Description1 = Damper.Description1;
            //                        cnt++;
            //                        break;
            //                   case nameof(Recuperator):
            //                        Recuperator recuperator = (Recuperator)component;
            //                        shemaASU = recuperator.ShemaASU;
            //                        acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
            //                        switch (recuperator._RecuperatorType)
            //                        {
            //                            case Recuperator.RecuperatorType.Recirculation:
            //                                posname(acadBlock, "POSNAME_ACTUATOR1").TextString = recuperator.Drive1.Posname;
            //                                posname(acadBlock, "POSNAME_ACTUATOR2").TextString = recuperator.Drive2.Posname;
            //                                posname(acadBlock, "POSNAME_ACTUATOR3").TextString = recuperator.Drive3.Posname;
            //                                link(acadBlock, "LINK1").TextString = cnt.ToString();
            //                                recuperator.Drive1.ShemaASU.ShemaLink1 = cnt;
            //                                shemaASU = recuperator.Drive1.ShemaASU;
            //                                shemaASU.Description1 = recuperator.Drive1.Description;
            //                                cnt++;
            //                                link(acadBlock, "LINK2").TextString = (cnt).ToString();
            //                                recuperator.Drive2.ShemaASU.ShemaLink1 = cnt;
            //                                shemaASU = recuperator.Drive2.ShemaASU;
            //                                shemaASU.Description1 = recuperator.Drive2.Description;
            //                                cnt++;
            //                                link(acadBlock, "LINK3").TextString = (cnt).ToString();
            //                                recuperator.Drive3.ShemaASU.ShemaLink1 = cnt;
            //                                shemaASU = recuperator.Drive3.ShemaASU;
            //                                shemaASU.Description1 = recuperator.Drive3.Description;
            //                                cnt++;
            //                                break;
            //                            case Recuperator.RecuperatorType.RotorControl:
            //                                posname(acadBlock, "POSNAME_ACTUATOR").TextString = recuperator.Drive1.Posname;
            //                                link(acadBlock, "LINK1").TextString = cnt.ToString();
            //                                recuperator.Drive1.ShemaASU.ShemaLink1 = cnt;
            //                                shemaASU = recuperator.Drive1.ShemaASU;
            //                                shemaASU.Description1 = recuperator.Drive1.Description;
            //                                cnt++;
            //                                posname(acadBlock, "POSNAME_PRESD").TextString = recuperator.protectSensor2.PosName;
            //                                link(acadBlock, "LINK2").TextString = cnt.ToString();
            //                                recuperator.protectSensor2.ShemaASU.ShemaLink1 = cnt;
            //                                shemaASU = recuperator.protectSensor2.ShemaASU;
            //                                shemaASU.Description1 = recuperator.protectSensor2.Description;
            //                                cnt++;
            //                                break;
            //                            case Recuperator.RecuperatorType.LaminatedNoBypass:
            //                                posname(acadBlock, "POSNAME_PRESD").TextString = recuperator.protectSensor2.PosName;
            //                                link(acadBlock, "LINK").TextString = cnt.ToString();
            //                                recuperator.protectSensor2.ShemaASU.ShemaLink1 = cnt;
            //                                shemaASU = recuperator.protectSensor2.ShemaASU;
            //                                shemaASU.Description1 = recuperator.protectSensor2.Description;
            //                                cnt++;
            //                                break;
            //                            case Recuperator.RecuperatorType.LaminatedBypass:
            //                                posname(acadBlock, "POSNAME_TEMP").TextString = recuperator.protectSensor1.PosName;
            //                                link(acadBlock, "LINK1").TextString = cnt.ToString();
            //                                recuperator.protectSensor1.ShemaASU.ShemaLink1 = cnt;
            //                                shemaASU = recuperator.protectSensor1.ShemaASU;
            //                                shemaASU.Description1 = recuperator.protectSensor1.Description;
            //                                cnt++;
            //                                posname(acadBlock, "POSNAME_BYPASS").TextString = recuperator.Drive1?.Posname;
            //                                link(acadBlock, "LINK2").TextString = cnt.ToString();
            //                                recuperator.Drive1.ShemaASU.ShemaLink1 = cnt;
            //                                shemaASU = recuperator.Drive1.ShemaASU;
            //                                shemaASU.Description1 = recuperator.Drive1.Description;
            //                                cnt++;
            //                                posname(acadBlock, "POSNAME_PRESD").TextString = recuperator.protectSensor2?.PosName;
            //                                link(acadBlock, "LINK3").TextString = cnt.ToString();
            //                                recuperator.protectSensor2.ShemaASU.ShemaLink1 = cnt;
            //                                shemaASU = recuperator.protectSensor2.ShemaASU;
            //                                shemaASU.Description1 = recuperator.protectSensor2.Description;
            //                                cnt++;

            //                                break;
            //                            case Recuperator.RecuperatorType.Glycol:
            //                                posname(acadBlock, "POSNAME_VALVE").TextString = recuperator.Drive1?.Posname;
            //                                posname(acadBlock, "POSNAME_PUMP").TextString = recuperator.Drive2?.Posname;
            //                                posname(acadBlock, "POSNAME_PRESD").TextString = recuperator.protectSensor2.PosName;
            //                                link(acadBlock, "LINK1").TextString = cnt.ToString();
            //                                recuperator.Drive1.ShemaASU.ShemaLink1 = cnt;
            //                                shemaASU = recuperator.Drive1.ShemaASU;
            //                                shemaASU.Description1 = recuperator.Drive1.Description;
            //                                cnt++;
            //                                link(acadBlock, "LINK2").TextString = (cnt).ToString();
            //                                recuperator.Drive2.ShemaASU.ShemaLink1 = cnt;
            //                                shemaASU = recuperator.Drive2.ShemaASU;
            //                                shemaASU.Description1 = recuperator.Drive2.Description;
            //                                cnt++;
            //                                link(acadBlock, "LINK3").TextString = (cnt).ToString();
            //                                recuperator.protectSensor2.ShemaASU.ShemaLink1 = cnt;
            //                                shemaASU = recuperator.protectSensor2.ShemaASU;
            //                                shemaASU.Description1 = recuperator.protectSensor2.Description;
            //                                cnt++;
            //                                break;
            //                        }
            //                        break;
            //                    case nameof(SupplyVent):
            //                    case nameof(ExtVent):

            //                        dynamic vent = component;
            //                        shemaASU = vent.ShemaASU;
            //                        acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
            //                        Vent.AHUContolType aHUContolType = vent.ControlType;
            //                        switch (aHUContolType)
            //                        {
            //                            case Vent.AHUContolType.Direct:
            //                                posname(acadBlock, "POSNAME").TextString = vent.PosName;
            //                                link(acadBlock, "LINK").TextString = cnt.ToString();
            //                                break;
            //                            case Vent.AHUContolType.Soft:
            //                            case Vent.AHUContolType.FCControl:
            //                            case Vent.AHUContolType.Transworm:
            //                                posname(acadBlock, "POSNAME1").TextString = vent.PosName;
            //                                link(acadBlock, "LINK").TextString = cnt.ToString();
            //                                posname(acadBlock, "POSNAME2").TextString = "RO";
            //                                break;
                                        
            //                        }
                                    
            //                        shemaASU.ShemaLink1 = cnt;
            //                        //shemaASU.Description = vent.Description;
            //                        cnt++;
            //                        if (vent._PressureContol != null)
            //                        {
            //                            PressureContol pressureContol = vent._PressureContol;
            //                            shemaASU = pressureContol.ShemaASU;
                                        
            //                            acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
            //                            posname(acadBlock, "POSNAME").TextString = pressureContol.PosName;
            //                            link(acadBlock, "LINK").TextString = cnt.ToString();
            //                            shemaASU.ShemaLink1 = cnt;
            //                            shemaASU.Description1 = pressureContol.Description;
            //                            cnt++;

            //                        }
            //                        break;
            //                    case nameof(SpareSuplyVent):
            //                    case nameof(SpareExtVent):
            //                        dynamic sparevent = component;
            //                        dynamic MainVent = null;
            //                        dynamic Reserved = null;
            //                        switch (component.GetType().Name)
            //                        {
            //                            case nameof(SpareSuplyVent):
            //                                MainVent = sparevent.MainSupplyVent;
            //                                Reserved = sparevent.ReservedSupplyVent;
            //                                break;
            //                            case nameof(SpareExtVent):
            //                                MainVent = sparevent.MainExtVent;
            //                                Reserved = sparevent.ReservedExtVent;
            //                                break;
            //                        }
            //                        shemaASU = sparevent.shemaAsu;
            //                        acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);//отрисовали верхний блок
            //                        Vent.AHUContolType aHUSpareContolType = sparevent.ControlType;
            //                        switch (aHUSpareContolType)
            //                        {
            //                            case Vent.AHUContolType.Direct:
            //                                posname(acadBlock, "POSNAME1").TextString = MainVent.PosName; //заполнили поз.1
            //                                posname(acadBlock, "POSNAME2").TextString = Reserved.PosName; //заполнили поз2
            //                                link(acadBlock, "LINK1").TextString = cnt.ToString(); //заполнили номер ссылки 1 наверху
            //                                shemaASU = MainVent.ShemaASU;
            //                                shemaASU.ShemaLink1 = cnt; //запомнили ссылку 1
            //                                cnt++; //вот тут будет отрисовка низа
            //                                link(acadBlock, "LINK2").TextString = cnt.ToString(); //заполнили номер ссылки 2 наверху
            //                                shemaASU = Reserved.ShemaASU;
            //                                shemaASU.ShemaLink1 = cnt; //запомнили ссылку 2
            //                                cnt++;
            //                                break;
            //                            case Vent.AHUContolType.Soft:
            //                            case Vent.AHUContolType.FCControl:
            //                            case Vent.AHUContolType.Transworm:
            //                                posname(acadBlock, "POSNAME1").TextString = MainVent.PosName;
            //                                posname(acadBlock, "POSNAME2").TextString = Reserved.PosName;
            //                                posname(acadBlock, "POSNAME3").TextString = "RO";
            //                                posname(acadBlock, "POSNAME4").TextString = "RO";
            //                                link(acadBlock, "LINK1").TextString = cnt.ToString();
            //                                shemaASU = MainVent.ShemaASU;
            //                                shemaASU.ShemaLink1 = cnt;
            //                                cnt++;
            //                                link(acadBlock, "LINK2").TextString = cnt.ToString();
            //                                shemaASU = Reserved.ShemaASU;
            //                                shemaASU.ShemaLink1 = cnt;
            //                                cnt++;
            //                                break;
                                        
            //                        }

            //                        if (sparevent._PressureContol != null)
            //                        {
            //                            PressureContol pressureContol = sparevent._PressureContol;
            //                            shemaASU = pressureContol.ShemaASU;
            //                            acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
            //                            posname(acadBlock, "POSNAME").TextString = pressureContol.PosName;
            //                            link(acadBlock, "LINK").TextString = cnt.ToString();
            //                            shemaASU.ShemaLink1 = cnt;
            //                            shemaASU.Description1 = pressureContol.Description;
            //                            cnt++;

            //                        }
            //                        break;
                                    
            //                    case nameof(WaterHeater):
            //                        WaterHeater waterHeater = (WaterHeater)component;
            //                        shemaASU = waterHeater.ShemaASU;

            //                        acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
            //                        posname(acadBlock, "POSNAME_PUMP").TextString = waterHeater._Pump.PosName; ;
            //                        posname(acadBlock, "POSNAME_Valve").TextString = waterHeater._Valve.Posname; ;
            //                        link(acadBlock, "LINK1").TextString = cnt.ToString();
            //                        if (waterHeater._Pump != null) waterHeater._Pump.ShemaASU.ShemaLink1 = cnt;
            //                        shemaASU = waterHeater._Pump?.ShemaASU;
            //                        shemaASU.Description1 = waterHeater._Pump?.Description;
            //                        cnt++;
            //                        link(acadBlock, "LINK2").TextString = cnt.ToString();
            //                        if (waterHeater._Valve != null) waterHeater._Valve.ShemaASU.ShemaLink1 = cnt;
            //                        shemaASU = waterHeater._Valve?.ShemaASU;
            //                        shemaASU.Description1 = waterHeater._Valve?.Description;
            //                        cnt++;
            //                        if (waterHeater.PS2 != null)
            //                        {
            //                            SensorT sensorPS2 = waterHeater.PS2;
            //                            shemaASU = sensorPS2.ShemaASU;
            //                            //Сделать блоки с датчиками и без них, не рисовать отдельно датчики

            //                            acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
            //                            posname(acadBlock, "POSNAME").TextString = waterHeater.PS2.PosName; ;
            //                            link(acadBlock, "LINK").TextString = cnt.ToString();
            //                            shemaASU.ShemaLink1 = cnt;
            //                            shemaASU.Description1 = sensorPS2.Description;
            //                            cnt++;
            //                        }
            //                        if (waterHeater.PS1 != null)
            //                        {
            //                            SensorT sensorPS1 = waterHeater.PS1;
            //                            shemaASU = sensorPS1.ShemaASU;
            //                            //Сделать блоки с датчиками и без них, не рисовать отдельно датчики

            //                            acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
            //                            posname(acadBlock, "POSNAME").TextString = waterHeater.PS1.PosName; ;
            //                            link(acadBlock, "LINK").TextString = cnt.ToString();
            //                            shemaASU.ShemaLink1 = cnt;
            //                            shemaASU.Description1 = sensorPS1.Description;
            //                            cnt++;
            //                        }


            //                        break;
            //                    case nameof(ElectroHeater):
            //                        ElectroHeater electroHeater = (ElectroHeater)component;
            //                        shemaASU = electroHeater.ShemaASU;
            //                        acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
            //                        posname(acadBlock, "POSNAME").TextString = electroHeater.PosName; ;
            //                        link(acadBlock, "LINK").TextString = cnt.ToString();
            //                        shemaASU.ShemaLink1 = cnt;
            //                        shemaASU.Description1 = electroHeater.Description;
            //                        cnt++;

            //                        break;
            //                    case nameof(Froster):
            //                        Froster froster = (Froster)component;
            //                        shemaASU = froster.ShemaASU;
            //                        shemaASU.ShemaLink1 = cnt;
            //                        acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
            //                        switch (froster._FrosterType)
            //                        {
            //                            case Froster.FrosterType.Freon:

            //                                link(acadBlock, "LINK").TextString = cnt.ToString();
            //                                shemaASU.ShemaLink1 = cnt;
            //                                shemaASU.Description1 = froster._KKB.Description;
            //                                cnt++;
            //                                break;
            //                            case Froster.FrosterType.Water:
            //                                posname(acadBlock, "POSNAME_Valve").TextString = froster._Valve.Posname;
            //                                link(acadBlock, "LINK").TextString = cnt.ToString();
            //                                froster._Valve.ShemaASU.ShemaLink1 = cnt;
            //                                shemaASU = froster._Valve.ShemaASU;
            //                                shemaASU.Description1 = froster._Valve.Description;
            //                                cnt++;
            //                                break;
            //                        }

            //                        if (froster.Sens1 != null)
            //                        {
            //                            Froster.ProtectSensor PS1 = froster.Sens1;
            //                            shemaASU = PS1.ShemaASU;
            //                            shemaASU.Description1 = PS1.Description;
            //                        }
            //                        if (froster.Sens2 != null)
            //                        {
            //                            Froster.ProtectSensor PS2 = froster.Sens2;
            //                            shemaASU = PS2.ShemaASU;
            //                            shemaASU.Description1 = PS2.Description;
            //                        }
            //                        break;
            //                    case nameof(Humidifier):
            //                        Humidifier humidifier = (Humidifier)component;
            //                        shemaASU = humidifier.ShemaASU;
            //                        acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
            //                        posname(acadBlock, "POSNAME").TextString = humidifier.PosName;
            //                        link(acadBlock, "LINK").TextString = cnt.ToString();
            //                        shemaASU.ShemaLink1 = cnt;
            //                        shemaASU.Description1 = humidifier.Description;
            //                        cnt++;
            //                        if (humidifier.HumSensPresent)
            //                        {

            //                            shemaASU = humidifier.HumiditySensor.ShemaASU;
            //                            acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
            //                            posname(acadBlock, "POSNAME").TextString = humidifier.SensPosName;
            //                            link(acadBlock, "LINK").TextString = cnt.ToString();
            //                            shemaASU.ShemaLink1 = cnt;
            //                            shemaASU.Description1 = humidifier.HumiditySensor.Description;
            //                            cnt++;
            //                        }

            //                        break;
            //                    case nameof(SupplyTemp):
            //                        SupplyTemp supplyTemp = (SupplyTemp)component;
            //                        shemaASU = supplyTemp.ShemaASU;
            //                        acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
            //                        posname(acadBlock, "POSNAME").TextString = supplyTemp.PosName;
            //                        link(acadBlock, "LINK").TextString = cnt.ToString();
            //                        shemaASU.ShemaLink1 = cnt;
            //                        shemaASU.Description1 = supplyTemp.Description;
            //                        cnt++;
            //                        break;
            //                    case nameof(IndoorTemp):
            //                        IndoorTemp indoorTemp = (IndoorTemp)component;
            //                        shemaASU = indoorTemp.ShemaASU;
            //                        acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
            //                        posname(acadBlock, "POSNAME").TextString = indoorTemp.PosName;
            //                        link(acadBlock, "LINK").TextString = cnt.ToString();
            //                        shemaASU.ShemaLink1 = cnt;
            //                        shemaASU.Description1 = indoorTemp.Description;
            //                        cnt++;
            //                        break;
                                
                                
            //                    case nameof(ExhaustTemp):
            //                        ExhaustTemp exhaustTemp = (ExhaustTemp)component;
            //                        shemaASU = exhaustTemp.ShemaASU;
            //                        acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
            //                        posname(acadBlock, "POSNAME").TextString = exhaustTemp.PosName;
            //                        link(acadBlock, "LINK").TextString = cnt.ToString();
            //                        shemaASU.ShemaLink1 = cnt;
            //                        shemaASU.Description1 = exhaustTemp.Description;
            //                        cnt++;
            //                        break;
                                
            //                    case nameof(Filtr):
            //                    case nameof(ExtFiltr):
            //                    case nameof(SupplyFiltr):
            //                        dynamic filtr = component;
            //                        shemaASU = filtr.ShemaASU;
            //                        acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
            //                        if (filtr._PressureContol != null)
            //                        {
            //                            PressureContol pressureContol = filtr._PressureContol;
            //                            shemaASU = pressureContol.ShemaASU;
            //                            acadBlock = DrawUpBlockV2(shemaASU, startPnt, posInfo);
            //                            posname(acadBlock, "POSNAME").TextString = pressureContol.PosName;
            //                            link(acadBlock, "LINK").TextString = cnt.ToString();
            //                            pressureContol.ShemaASU.ShemaLink1 = cnt;
            //                            shemaASU.Description1 = pressureContol.Description;
            //                            cnt++;
            //                        }

            //                        break;

            //                };


            //            }
            //        }
            //        #endregion
            //        #region Displacesement for next system

            //        int maxXline = ventSystem.ComponentsV2
            //            .Max(e => e.PozX);
            //        double basementoffset = 40 + (ventcompcnt + 3) * 10;

            //        if (maxXline <= 4 && maxYline <=1)
            //        {
            //            XOffsetForNextSystem += 420;
                        
            //            //draw frame
            //            AcadBlockReference frameblock = DrawFrame();
            //            object[] blockproperies = frameblock.GetDynamicBlockProperties();
            //            object[] basementpropyrties = basement.GetDynamicBlockProperties();
            //            IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> frameType = blockproperies.OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
            //                .Where(i => i.PropertyName == "Выбор формата");
            //            frameType.ElementAt(0).Value = "A3альбом";
                        
            //            //draw stamp
            //            AcadBlockReference stamp = DrawStamp(frameblock, "A3");

            //            //set basement size
            //            IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> basementProp = basementpropyrties.OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
            //                .Where(i => i.PropertyName == "Расстояние2");
            //            //basementProp.ElementAt(0).Value -= 77.96;
            //            basementProp.ElementAt(0).Value = basementoffset;
            //            YOffsetForNextPannel -= 297;

            //        }
            //        else if (maxXline >4 && maxXline <= 9 && maxYline <=1) 
            //        {
            //            XOffsetForNextSystem += 630;

            //            //draw frame
            //            AcadBlockReference frameblock = DrawFrame();
            //            object[] blockproperies = frameblock.GetDynamicBlockProperties();
            //            object[] basementpropyrties = basement.GetDynamicBlockProperties();
            //            IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> frameType = blockproperies.OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
            //                .Where(i => i.PropertyName == "Выбор формата");
            //            frameType.ElementAt(0).Value = "А4х3";
                        
            //            //draw stamp
            //            AcadBlockReference stamp = DrawStamp(frameblock, "A4x3");

            //            //set basement size
            //            IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> basementProp = basementpropyrties.OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
            //                .Where(i => i.PropertyName == "Расстояние2");
            //            basementProp.ElementAt(0).Value = basementoffset;
            //            YOffsetForNextPannel -= 297;

            //        }
            //        else if (maxXline > 9 && maxYline <= 1)
            //        {
            //            XOffsetForNextSystem += 891;

            //            //draw frame
            //            AcadBlockReference frameblock = DrawFrame();
            //            object[] blockproperies = frameblock.GetDynamicBlockProperties();
            //            object[] basementpropyrties = basement.GetDynamicBlockProperties();
            //            IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> frameType = blockproperies.OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
            //                .Where(i => i.PropertyName == "Выбор формата");
            //            frameType.ElementAt(0).Value = "А4х4";

            //            //draw stamp
            //            AcadBlockReference stamp = DrawStamp(frameblock, "А4х4");

            //            //set basement size
                        
            //            IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> basementProp = basementpropyrties.OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
            //                .Where(i => i.PropertyName == "Расстояние2");

                        
                        
            //            basementProp.ElementAt(0).Value = basementoffset;
            //            YOffsetForNextPannel -= 297;

            //        }
            //        else if (maxXline > 9 && maxYline > 1)
            //        {
            //            XOffsetForNextSystem += 891;

            //            //draw frame
            //            AcadBlockReference frameblock = DrawFrame();
            //            object[] blockproperies = frameblock.GetDynamicBlockProperties();
            //            object[] basementpropyrties = basement.GetDynamicBlockProperties();
            //            IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> frameType = blockproperies.OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
            //                .Where(i => i.PropertyName == "Выбор формата");
            //            frameType.ElementAt(0).Value = "А3х3";

            //            //draw stamp
            //            AcadBlockReference stamp = DrawStamp(frameblock, "А3х3");

            //            //set basement size

            //            IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> basementProp = basementpropyrties.OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
            //                .Where(i => i.PropertyName == "Расстояние2");



            //            basementProp.ElementAt(0).Value = basementoffset;
            //            YOffsetForNextPannel -= 594;

            //        }
            //        else if (maxYline > 1) //возможно нужно повернуть А3
            //        {
            //            AcadBlockReference frameblock = DrawFrame();
            //            object[] blockproperies = frameblock.GetDynamicBlockProperties();
            //            object[] basementpropyrties = basement.GetDynamicBlockProperties();
            //            IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> frameType = blockproperies.OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
            //                .Where(i => i.PropertyName == "Выбор формата");
            //            frameType.ElementAt(0).Value = "А3х3";

            //            //draw stamp
            //            AcadBlockReference stamp = DrawStamp(frameblock, "А3х3");

            //            //set basement size

            //            IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> basementProp = basementpropyrties.OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
            //                .Where(i => i.PropertyName == "Расстояние2");



            //            basementProp.ElementAt(0).Value = basementoffset;
            //            YOffsetForNextPannel -= 420;
            //        }
            //        #endregion
            //        #region Set basement attributes
            //        object[] attributes = basement.GetAttributes();
            //        IEnumerable<AutoCAD.AcadAttributeReference> basementAttribut = attributes.OfType<AcadAttributeReference>()
            //            .Where(i => i.TagString == "МАРКА_ЩИТА_C");
            //        basementAttribut.ElementAt(0).TextString = pannel.PannelName;
            //        #endregion
            //        //DrawRoomBlock();
            //    }
            //    #region Draw dispatching line and calculated IO
            //    (int, int, int, int) IO = (AI, AO, DI, DO);

            //    if (pannel.Dispatching != Pannel._Dispatching.No)
            //    {
            //        MakeDispatcing(shapkaPnt, ventcompcnt + 1, IO);
            //        MakeDispatchingSignals(shapkaPnt, ventcompcnt + 2, IO);

            //    }
            //    else
            //    {
            //        MakeDispatchingSignals(shapkaPnt, ventcompcnt + 1, IO);
            //    }
                
            //    #endregion

            //    // Set Y offset for next pannel
            //   //YOffsetForNextPannel -= 297;


            //}
            #endregion
        }
        #region Autocad functions
        private AcadBlockReference DrawUpBlockV2(ISchemaASU schemaASU, double[] starpoint, EditorV2.PosInfo posInfo)
        {

            //double[] point = new double[3];
            double[] point = EditorV2.PosInfo.PosToAcadPoint(posInfo.Pos);// GetPointByDrawingPos(schemaASU.ShemaPos, starpoint);
            point[0] = starpoint[0] + point[0];
            point[1] = starpoint[1] - point[1];
            var blockObj = acadDoc.ModelSpace.InsertBlock(point, schemaASU.ShemaUp, 1, 1, 1, 0, Type.Missing);
            if (!string.IsNullOrEmpty(shemaASU.ShemaLink2))
                acadDoc.ModelSpace.InsertBlock(point, shemaASU.ShemaLink2, 1, 1, 1, 0, Type.Missing);
            //var blockObj = acadDoc.ModelSpace.InsertBlock(point, "testV2", 1, 1, 1, 0, Type.Missing);
            return blockObj;
        }
        private AcadBlockReference DrawUpBlockV2(ISchemaASU schemaASU, double[] starpoint, EditorV2.PosInfo posInfo, double[] correction)
        {

            //double[] point = new double[3];
            double[] point = EditorV2.PosInfo.PosToAcadPoint(posInfo.Pos);// GetPointByDrawingPos(schemaASU.ShemaPos, starpoint);
            point[0] = starpoint[0] + point[0] + correction[0];
            point[1] = starpoint[1] - point[1] - correction[1];
            var blockObj = acadDoc.ModelSpace.InsertBlock(point, schemaASU.ShemaUp, 1, 1, 1, 0, Type.Missing);
            //var blockObj = acadDoc.ModelSpace.InsertBlock(point, "testV2", 1, 1, 1, 0, Type.Missing);
            return blockObj;
        }

        private AcadBlockReference DrawDownBlock()
        {
            
            double[] point = new double[3];
            //point[0] = shapkaPnt[0] + BasementXOffset + (shemaASU.ShemaLink1)*ElementXOffset;
            //point[1] = basementPnt[1];
            point[0] = shapkaPnt[0] + BasementXOffset + (ventcompcnt) * ElementXOffset;
            point[1] = shapkaPnt[1];
            var blockObj = acadDoc.ModelSpace.InsertBlock(point, "ФСА_Подвал_(КИП)", 1, 1, 1, 0, Type.Missing);
            return blockObj;
        }
        private AcadBlockReference DrawSignalBlock (ShemaASU shemaASU, double[] fsalinepnt, double signalDisplacement)
        {
            double[] point = new double[3];
            //point[0] = basementPnt[0] + BasementXOffset + ElementXOffset + (shemaASU.ShemaLink1) * ElementXOffset;
            //point[0] = shapkaPnt[0] + BasementXOffset + (ventcompcnt) * ElementXOffset;
            point[0] = fsalinepnt[0] + ElementXOffset;
            //point[1] = basementPnt[1] - signalDisplacement ;
            point[1] = fsalinepnt[1] - signalDisplacement;
            point[2] = 0;
            var blockObj = acadDoc.ModelSpace.InsertBlock(point, "ФСА_Подвал_(сигнал)", 1, 1, 1, 0, Type.Missing);
            return blockObj;
        }
        private AcadBlockReference DrawDispatchingSignal(double[] fsaline)
        {
            double[] point = new double[3];
            //point[0] = basementPnt[0] + BasementXOffset + ElementXOffset + (cnt) * ElementXOffset;
            //point[0] = shapkaPnt[0] + BasementXOffset + (localcnt) * ElementXOffset;
            point[0] = fsaline[0] + ElementXOffset;
            //point[1] = basementPnt[1] - DispatchingOffset;
            point[1] = fsaline[1] - DispatchingOffset;
            point[2] = 0;
            var blockObj = acadDoc.ModelSpace.InsertBlock(point, "ФСА_Подвал_(сигнал)", 1, 1, 1, 0, Type.Missing);
            return blockObj;
        }

        private AcadBlockReference DrawDispatchingLine( double[] basementPnt, int cnt)
        {

            double[] point = new double[3];
            point[0] = basementPnt[0] + BasementXOffset + (cnt) * ElementXOffset;
            point[1] = basementPnt[1];
            point[2] = 0;
            var blockObj = acadDoc.ModelSpace.InsertBlock(point, "ФСА_Подвал_(КИП)", 1, 1, 1, 0, Type.Missing);
            return blockObj;
        }
        private void MakeDispatcing (double[] basementPnt, int localcnt, (int AI, int AO, int DI, int DO) IO)
        {

            int IOSumm = IO.AI + IO.AO + IO.DI + IO.DO;
            AcadBlockReference FSALineBlock = DrawDispatchingLine(basementPnt, localcnt);
            AcadDynamicBlockReferenceProperty distance = GetBlockProperty(FSALineBlock, "Расстояние1");
            distance.Value += DispatchingOffset;
            AcadAttributeReference KIP1 = GetAttrFromBlock(FSALineBlock, "КИП_1");
            AcadAttributeReference KIP2 = GetAttrFromBlock(FSALineBlock, "КИП_2");
            AcadAttributeReference FSASignalAttr = GetAttrFromBlock(FSALineBlock, "SIGNAL");
            KIP1.TextString = "В систему диспетчеризации";
            KIP2.TextString = pannel.Protocol.ToString();
            FSASignalAttr.TextString = string.Empty;
            
            AcadBlockReference SignalBlock = DrawDispatchingSignal(FSALineBlock.InsertionPoint);
            AcadAttributeReference DispatchingSignalSumm = GetAttrFromBlock(SignalBlock, "SIGNAL");
            if (nonUniqueSystems != null && nonUniqueSystems.Contains(curVentSystem))
            {
                DispatchingSignalSumm.TextString = $"{IOSumm}x{nonUniqueSystems.Count}";// IOSumm.ToString();
            }
            else
            {
                DispatchingSignalSumm.TextString = IOSumm.ToString();
            }
            
            
        }
        private void MakeDispatchingSignals(IReadOnlyList<double> basementPnt, int cnt, (int AI, int AO, int DI, int DO) IO)
        {
            double[] point = new double[3];
            double signalDisplacement;
            if (nonUniqueSystems != null && nonUniqueSystems.Contains(curVentSystem))
            {
                IO.AI = nonUniqueSystems.Count * IO.AI;
                IO.DI = nonUniqueSystems.Count * IO.DI;
                IO.AO = nonUniqueSystems.Count * IO.AO;
                IO.DO = nonUniqueSystems.Count * IO.DO;
            }

            if (IO.AI > 0)
            {
                signalDisplacement = AIOffset;
                point[0] = basementPnt[0] + BasementXOffset + ElementXOffset + (cnt) * ElementXOffset;
                point[1] = basementPnt[1] - signalDisplacement;
                point[2] = 0;
                var blockObj = acadDoc.ModelSpace.InsertBlock(point, "ФСА_Подвал_(сигнал)", 1, 1, 1, 0, Type.Missing);
                GetAttrFromBlock(blockObj, "SIGNAL").TextString = IO.AI.ToString();
            }
            if (IO.AO > 0)
            {
                signalDisplacement = AOOffset;
                point[0] = basementPnt[0] + BasementXOffset + ElementXOffset + (cnt) * ElementXOffset;
                point[1] = basementPnt[1] - signalDisplacement;
                point[2] = 0;
                var blockObj = acadDoc.ModelSpace.InsertBlock(point, "ФСА_Подвал_(сигнал)", 1, 1, 1, 0, Type.Missing);
                GetAttrFromBlock(blockObj, "SIGNAL").TextString = IO.AO.ToString();
            }
            if (IO.DI > 0)
            {
                signalDisplacement = DIOffset;
                point[0] = basementPnt[0] + BasementXOffset + ElementXOffset + (cnt) * ElementXOffset;
                point[1] = basementPnt[1] - signalDisplacement;
                point[2] = 0;
                var blockObj = acadDoc.ModelSpace.InsertBlock(point, "ФСА_Подвал_(сигнал)", 1, 1, 1, 0, Type.Missing);
                GetAttrFromBlock(blockObj, "SIGNAL").TextString = IO.DI.ToString();
            }
            if (IO.DO > 0)
            {
                signalDisplacement = DOOffset;
                point[0] = basementPnt[0] + BasementXOffset + ElementXOffset + (cnt) * ElementXOffset;
                point[1] = basementPnt[1] - signalDisplacement;
                point[2] = 0;
                var blockObj = acadDoc.ModelSpace.InsertBlock(point, "ФСА_Подвал_(сигнал)", 1, 1, 1, 0, Type.Missing);
                GetAttrFromBlock(blockObj, "SIGNAL").TextString = IO.DO.ToString();
            }





        }

        private AcadAttributeReference GetAttrFromBlock(AcadBlockReference acadBlock, string attrname)
        {
            object[] prop = acadBlock.GetAttributes();
            IEnumerable<AutoCAD.AcadAttributeReference> Properties = prop.OfType<AutoCAD.AcadAttributeReference>()
                .Where(i => i.TagString == attrname);
            return Properties?.ElementAt(0);
        }
        private static AcadDynamicBlockReferenceProperty GetBlockProperty (AcadBlockReference acadBlock, string propertyName)
        {
            object[] blockproperies = acadBlock.GetDynamicBlockProperties();
            IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> property = blockproperies.OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
                    .Where(i => i.PropertyName == propertyName);
            return property.ElementAt(0);
        }
        private AcadBlockReference DrawFrame()
        {
            double[] point = new double[3];
            point[0] = shapkaPnt[0] - 20;
            point[1] = shapkaPnt[1] -85;
            
            var blockObj = acadDoc.ModelSpace.InsertBlock(point, "_РАМКА", 1, 1, 1, 0, Type.Missing);
            return blockObj;
        }
        private AcadBlockReference DrawStamp(AcadBlockReference frame, string framesize)
        {
            double[] point = new double[3];
            point[1] = frame.InsertionPoint[1]+5;
            switch (framesize)
            {
                case "A3":
                    point[0] = frame.InsertionPoint[0] + 415;
                    break;
                case "A4x3":
                    point[0] = frame.InsertionPoint[0] + 625;
                    break;
                case "А4х4":
                    point[0] = frame.InsertionPoint[0] + 836;
                    break;
                case "А3х3":
                    point[0] = frame.InsertionPoint[0] + 886;
                    break;
            }
            var blockObj = acadDoc.ModelSpace.InsertBlock(point, "Штамп_новый", 1, 1, 1, 0, Type.Missing);

            AcadAttributeReference drawName = GetAttrFromBlock(blockObj, "НАИМЕНОВАНИЕ_ЧЕРТЕЖА");
            if (nonUniqueSystems != null)
            {
                if (nonUniqueSystems.Count > 1 && nonUniqueSystems.Contains(curVentSystem))
                {
                    drawName.TextString = $"Схема функциональная автоматизации вент.систем {string.Join(", ", nonUniqueSystemsNames)}";
                }
                else
                {
                    drawName.TextString = $"Схема функциональная автоматизации вент.системы {nonUniqueSystemsNames[0]}";
                }

            }
            else
            {
                drawName.TextString = $"Схема функциональная автоматизации вент.системы {curVentSystem.SystemName}";
            }
            
            

            return blockObj;

        }
        private void DrawApplicability_table(AcadBlockReference frame, string framesize)
        {
            if (nonUniqueSystems == null || !nonUniqueSystems.Contains(curVentSystem)) return;
            double[] point = new double[3];
            point[1] = frame.InsertionPoint[1] + 5;
            switch (framesize)
            {
                case "A3":
                    point[0] = frame.InsertionPoint[0] + 415;
                    point[1] = frame.InsertionPoint[1] + 292;
                    break;
                case "A4x3":
                    point[0] = frame.InsertionPoint[0] + 625;
                    point[1] = frame.InsertionPoint[1] + 292;
                    break;
                case "А4х4":
                    point[0] = frame.InsertionPoint[0] + 836;
                    point[1] = frame.InsertionPoint[1] + 292;
                    break;
                case "А3х3":
                    point[0] = frame.InsertionPoint[0] + 886;
                    point[1] = frame.InsertionPoint[1] + 415;
                    break;
            }
            AcadBlockReference headeBlockReference = acadDoc.ModelSpace.InsertBlock(point, "applicability_table_header", 1, 1, 1, 0, Type.Missing);
            var s1 = point[1];
            point[1] -= 20;
            
            foreach (var ventSystem in nonUniqueSystems)
            {
                string FCName = string.Empty;
                string SYSTEMNAME = ventSystem.SystemName;
                string DamperName = ventSystem.ComponentsV2
                    .Select(e => e.Tag)
                    .Where(e => e is ExtDamper)
                    .Select(e => ((ExtDamper)e).PosName)
                    .FirstOrDefault();
                string FiltrPDS = ventSystem.ComponentsV2
                    .Select(e => e.Tag)
                    .Where(e => e is ExtFiltr || e is Filtr)
                    .Select(e =>
                    {
                        PressureContol contrContol = null;
                        switch (e)
                        {
                            case ExtFiltr extFiltr:
                                contrContol = extFiltr._PressureContol;
                                break;
                            case Filtr filtr :
                                contrContol = filtr._PressureContol;
                                break;
                        }
                        return contrContol != null ? contrContol.PosName : string.Empty;
                    })
                    .FirstOrDefault();
                string VentPDS = ventSystem.ComponentsV2
                    .Select(e => e.Tag)
                    .Where(e => e is ExtVent || e is SpareExtVent)
                    .Select(e =>
                    {
                        PressureContol contrContol = null;
                            
                        switch (e)
                        {
                            case ExtVent vent:
                                contrContol = vent._PressureContol;
                                if (vent._FControl != null) FCName = $"{vent.PosName}-FC";
                                break;
                            case SpareExtVent vent:
                                contrContol = vent._PressureContol;
                                if (vent.MainExtVent._FControl != null) FCName = $"{vent.MainExtVent.PosName}-FC";
                                break;
                        }

                        return contrContol != null ? contrContol.PosName : string.Empty;
                    })
                    .FirstOrDefault();
                var lINEObj = acadDoc.ModelSpace.InsertBlock(point, "applicability_table_line", 1, 1, 1, 0, Type.Missing);
                AcadAttributeReference SYSTEMNAMEAttr = GetAttrFromBlock(lINEObj, "SYSTEMNAME");
                AcadAttributeReference VentPDSAttr = GetAttrFromBlock(lINEObj, "VentPDS");
                AcadAttributeReference DamperNameAttr = GetAttrFromBlock(lINEObj, "DamperName");
                AcadAttributeReference FCNameAttr = GetAttrFromBlock(lINEObj, "FCName");
                AcadAttributeReference FiltrPDSAttr = GetAttrFromBlock(lINEObj, "FiltrPDS");
                SYSTEMNAMEAttr.TextString = !string.IsNullOrEmpty(SYSTEMNAME) ? SYSTEMNAME : "-";
                DamperNameAttr.TextString = !string.IsNullOrEmpty(DamperName) ? DamperName : "-";
                FCNameAttr.TextString = !string.IsNullOrEmpty(FCName) ? FCName : "-";
                FiltrPDSAttr.TextString = !string.IsNullOrEmpty(FiltrPDS) ? FiltrPDS : "-";
                VentPDSAttr.TextString = !string.IsNullOrEmpty(VentPDS) ? VentPDS: "-";
                point[1] = lINEObj.InsertionPoint[1]-5;
            }

        }
        #endregion
        #region Application functions
        //get key from dictionary vaue
        internal Pannel GetKeyFromValue(Dictionary<Pannel, List<(string, VentSystem)>> VentSystemS, List<(string, VentSystem)> valueVar)
        {
            foreach (Pannel keyVar in VentSystemS.Keys)
            {
                if (VentSystemS[keyVar] == valueVar)
                {
                    return keyVar;
                }
            }
            return null;
        }
        private dynamic  StartAutocad((string path, string progID) Acad)
        {
            string programFiles = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
            string acadfile = programFiles + Acad.path;// + " //product ACAD //language " + '\u0022' + "ru - RU" + '\u0022';
            Process acadProc = new Process();
            acadProc.StartInfo.FileName = acadfile;
            acadProc.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            try
            {
                acadProc.Start();
            }
            catch
            {
                throw new ApplicationException($"Не могу найти Autocad по пути \n {acadfile}\nгенерация схем невозомжна");
            }
            
            if (!acadProc.WaitForInputIdle(300000))
                throw new ApplicationException("Слишком долго стартует Autocad, выход");
            dynamic acadApp;
            while (true)
            {
                
                try
                {
                    acadApp = Marshal.GetActiveObject(Acad.progID);
                    return acadApp;
                }
                catch (COMException ex)
                {
                    const uint MK_E_UNAVAILABLE = 0x800401e3;
                    if ((uint)ex.ErrorCode != MK_E_UNAVAILABLE) throw;
                    Thread.Sleep(1000);
                }
            }
            

            


        }
        #endregion
        #region Create variable events
        internal int cnt
        {
            get { return _cnt; }
            set
            {
                _cnt = value;
                if (_cnt> 1)
                {
                    if (shemaASU != null)
                    {
                        #region Draw FSA Line and set FSA Line attribute
                        //draw fsa line
                        AcadBlockReference FSALineBlock =  DrawDownBlock();

                        //set fsa attributes
                        GetAttrib GetAttr = GetAttrFromBlock;
                        GetProperty BlockPropery = GetBlockProperty;
                        
                        var KIP1 = GetAttr(FSALineBlock, "КИП_1");
                        var KIP2 = GetAttr(FSALineBlock, "КИП_2");
                        var FSASignalAttr = GetAttr(FSALineBlock, "SIGNAL");
                        var distance = BlockPropery(FSALineBlock, "Расстояние1");
                        
                        // devide text if char count >32
                        if (shemaASU.Description1?.Length >32)
                        {
                            char[] chars = shemaASU.Description1.ToCharArray(0, 33);
                            string s = new string(chars);
                            string[] s2 = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            var s3 = s2.Take(s.Length - 1);
                            string[] devidedescription = shemaASU.Description1.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            var k1 = devidedescription.Intersect(s3);
                            var k2 = devidedescription.Except(s3);

                            KIP1.TextString = string.Join(" ", k1);
                            KIP2.TextString = string.Join(" ", k2);
                        }
                        else
                        {
                            KIP1.TextString = shemaASU.Description1;
                            KIP2.TextString = string.Empty;
                        }
                        
                        // set number to fsa line
                        FSASignalAttr.TextString = shemaASU.ShemaLink1.ToString();
                        
                        
                        int koefficient = 1;
                        if (nonUniqueSystems != null && nonUniqueSystems.Contains(curVentSystem)) koefficient = nonUniqueSystems.Count;
                        string DispatcherString(int IONums)
                        {
                            if (nonUniqueSystems != null && nonUniqueSystems.Contains(curVentSystem))
                            {
                                return $"{IONums}x{koefficient}";
                            }

                            return IONums.ToString();

                        }

                        //set fsa line lenght
                        if (pannel.Dispatching == Pannel._Dispatching.Yes) //if dispatching
                        {
                            distance.Value += DispatchingOffset;
                            
                            //draw signal circle
                            
                            AcadBlockReference SignalBlock = DrawSignalBlock(shemaASU, FSALineBlock.InsertionPoint, DispatchingOffset);

                            GetAttr(SignalBlock, "SIGNAL").TextString = DispatcherString(shemaASU.IOCount);// shemaASU.IOCount.ToString();

                        }
                        else
                        {
                            if (shemaASU.DO)
                            {
                                distance.Value += DOOffset;
                            }
                            else if (shemaASU.DI)
                            {
                                distance.Value += DIOffset;
                            }
                            else if (shemaASU.AO)
                            {
                                distance.Value += AOOffset;
                            }
                            else if (shemaASU.AI)
                            {
                                distance.Value += AIOffset;
                            }
                        }
                        #endregion
                        #region Draw FSA signal and set signal attribute

                        //draw fsa signal
                        

                        if (shemaASU.DO)
                        {
                            AcadBlockReference SignalBlock = DrawSignalBlock(shemaASU, FSALineBlock.InsertionPoint, DOOffset);
                            GetAttr(SignalBlock, "SIGNAL").TextString = DispatcherString(shemaASU.DOcnt);//(shemaASU.DOcnt * koefficient).ToString();
                            DO += shemaASU.DOcnt;
                        }
                        // calculate IO
                        if (shemaASU.DI)
                        {
                            AcadBlockReference SignalBlock = DrawSignalBlock(shemaASU, FSALineBlock.InsertionPoint, DIOffset);
                            GetAttr(SignalBlock, "SIGNAL").TextString = DispatcherString(shemaASU.DIcnt);//(shemaASU.DIcnt * koefficient).ToString();
                            DI += shemaASU.DIcnt;

                        }
                        if (shemaASU.AO)
                        {
                            AcadBlockReference SignalBlock = DrawSignalBlock(shemaASU, FSALineBlock.InsertionPoint, AOOffset);
                            GetAttr(SignalBlock, "SIGNAL").TextString = DispatcherString(shemaASU.AOcnt);//(shemaASU.AOcnt * koefficient).ToString();
                            AO += shemaASU.AOcnt;
                        }
                        if (shemaASU.AI)
                        {
                            AcadBlockReference SignalBlock = DrawSignalBlock(shemaASU, FSALineBlock.InsertionPoint, AIOffset);
                            GetAttr(SignalBlock, "SIGNAL").TextString = DispatcherString(shemaASU.AIcnt);//(shemaASU.AIcnt * koefficient).ToString();
                            AI += shemaASU.AIcnt;
                        }
                        #endregion
                        
                        // increase vent component count
                        ventcompcnt++;
                    }
                }
            }
        }
        private int _cnt;
        #endregion



    }
}
