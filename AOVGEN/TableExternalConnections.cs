using AOVGEN.Models;
using AutoCAD; //using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AOVGEN
{
    public static class EnumExtensionMethods
    {
        public static string GetEnumDescription(this Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            var descriptionAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : enumValue.ToString();
        }
    }

    internal class TableExternalConnections
    {
        private static readonly string[] progIds =
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

        private AcadDocument acadDoc;

        internal TableExternalConnections(Dictionary<string, Dictionary<string, Dictionary<string, List<Cable>>>> inputdict, Dictionary<string, VentSystem> inputvensystemDict, bool[] inputcabsettings, Dictionary<string, Pannel> pannels, string tabdocname)
        {
            var cabsettings = inputcabsettings;
            level3 = inputdict;
            writecabeP = cabsettings[0];
            writecablePump = cabsettings[1];
            writecableValve = cabsettings[2];
            ventsystemDict = inputvensystemDict;
            checkedpannels = pannels;
            TableExternalConnectionDocName = tabdocname;
        }

        internal string Author { get; set; }

        internal string BuildingName { get; set; }

        internal string Project { get; set; }
        internal string TableExternalConnectionDocName { get; set; }

        [DllImport("ole32.dll")]
        public static extern void GetRunningObjectTable(
            int reserved,
            out IRunningObjectTable prot);

        [DllImport("user32.dll")]
        internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        //[CommandMethod("NewDrawing", CommandFlags.Session)]
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
                    MessageBox.Show(@"Попытка запуска Autocad 2019\n" +
                                    @"Или попробуйте запустить его вручную");
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
                AcadApplication acadApp = Acad2019COM;
                {
                    IntPtr hWnd = new(Acad2019COM.HWND);
                    if (hWnd != IntPtr.Zero)
                    {
                        SetForegroundWindow(hWnd);
                    }
                    acadApp.WindowState = AcWindowState.acMax;
                    SendKeys.Send("{ESC}");

                    
                    if (acadApp.Documents.Count > 0)
                    {
                        foreach (AcadDocument acadDocument in acadApp.Documents) //
                        {
                            
                            
                            if (acadDocument.Name == "Блоки.dwg" || acadDocument.Name == TableExternalConnectionDocName )
                            {
                                acadDoc = acadDocument;
                                acadDoc.Activate();
                                SendKeys.Send("{ESC}");
                                break;
                            }

                            //AcadBlocks blocks = acadDocument.Blocks;
                            //if (blocks == null) continue;
                            //if (blocks.Count <= 0) continue;
                            
                            //AcadBlock shapka = blocks
                            //    .Cast<AcadBlock>()
                            //    .FirstOrDefault(e => e.Name == "Шапка-11");
                            
                            //if (shapka == null) continue;
                            //acadDoc = acadDocument;
                            //acadDoc.Activate();
                            //SendKeys.Send("{ESC}");
                            //break;
                        }
                    }

                    if (acadDoc == null)
                    {
                        string docpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                         @"\Autodesk\Revit\Addins\2022\ASU\AOVGen\Blocks.dwt";
                        acadDoc = acadApp.Documents.Add(docpath);
                        TableExternalConnectionDocName = acadDoc.Name;
                        //acadDoc.Activate();
                    }

                    const string prompt1 = "Вставьте блок";
                    double[] startPnt;
                    Thread.Sleep(500);
                    try
                    {
                        startPnt = acadDoc.Utility.GetPoint(Type.Missing, prompt1); //дальше начанется обычный алгоритм построения
                    }
                    catch
                    {
                        const string message = "Не смог получить точку вставки";
                        const string caption = "Ошибка!";
                        MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        return 0;
                    }

                    Dictionary<string, List<Cable>> allcables = new();
                    level3.AsParallel().ForAll(keyValuePair1 =>
                    {
                        Dictionary<string, Dictionary<string, List<Cable>>> level2 = keyValuePair1.Value;
                        List<Cable> cablelist = new();
                        foreach (var ventcompdic in
                            from keyValuePair in level2
                            let ventname = keyValuePair.Key
                            select keyValuePair.Value)
                        {
                            cablelist.AddRange(from ventcomp in ventcompdic
                                               let comppos = ventcomp.Key
                                               from cable in ventcomp.Value
                                               select cable);
                        }

                        allcables.Add(keyValuePair1.Key, cablelist);
                    });
                    Cable GetCableFromCableList(IEnumerable<Cable> cabs, Cable.CableAttribute attr, int cableNums)
                    {
                        return cabs
                            .AsParallel()
                            .LastOrDefault(e => e.Attrubute == attr && e.WireNumbers == cableNums);
                    }

                    foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, List<Cable>>>> keyValuePair1 in
                        level3)
                    {
                        Dictionary<string, Dictionary<string, List<Cable>>> level2 = keyValuePair1.Value;
                        List<Cable> cablelist = allcables[keyValuePair1.Key];

                        Pannel = new Pannel();
                        Pannel = checkedpannels[keyValuePair1.Key];

                        Cable[] lastCables =
                        {
                            GetCableFromCableList(cablelist, Cable.CableAttribute.P, 3),
                            GetCableFromCableList(cablelist, Cable.CableAttribute.P, 5),
                            GetCableFromCableList(cablelist, Cable.CableAttribute.A, 2),
                            GetCableFromCableList(cablelist, Cable.CableAttribute.A, 3),
                            GetCableFromCableList(cablelist, Cable.CableAttribute.D, 2),
                            GetCableFromCableList(cablelist, Cable.CableAttribute.D, 3),
                            GetCableFromCableList(cablelist, Cable.CableAttribute.D, 4),
                            GetCableFromCableList(cablelist, Cable.CableAttribute.C, 2),
                            GetCableFromCableList(cablelist, Cable.CableAttribute.C, 3),
                            GetCableFromCableList(cablelist, Cable.CableAttribute.PL, 3)
                        };

                        Dictionary<string, string> lastdict = lastCables
                            .Where(lastCable => lastCable != null)
                            .ToDictionary(lastCable =>
                                    lastCable.Attrubute + lastCable.WireNumbers.ToString(),
                                lastCable => lastCable.cableGUID);
                        //insert first element
                        var blockObj = acadDoc.ModelSpace.InsertBlock(startPnt, "Шапка-11", 1, 1, 1, 0, Type.Missing);
                        // end insert first element
                        ((AcadEntity)blockObj).GetBoundingBox(out object minPt, out object maxPt);
                        min = (double[])minPt;
                        max = (double[])maxPt;
                        var size = max[0] - min[0];

                        startPnt[0] += size;

                        foreach (KeyValuePair<string, Dictionary<string, List<Cable>>> keyValuePair in level2)
                        {
                            string ventname = keyValuePair.Key;
                            VentSystem ventSystem = ventsystemDict[ventname];
                            Dictionary<string, List<Cable>> ventcompdic = keyValuePair.Value;
                            foreach (KeyValuePair<string, List<Cable>> ventcomp in ventcompdic)
                            {
                                List<Cable> cables = ventcomp.Value;
                                int cnt = cables.Count;
                                int cabcnt = 1;
                                double displacement = 0;
                                int i = 0;
                                double[] cablearrpt = { 0, 0, 0 };
                                foreach (Cable cable in cables)
                                {
                                    bool printblock = cable.WriteBlock;

                                    if (printblock)
                                    {
                                        ArrayList list = InsertDevBlock(startPnt, cable.ToBlockName);
                                        AcadBlockReference acadBlock = (AcadBlockReference)list[0];
                                        switch (cable.CompTable)
                                        {
                                            case "KKB":
                                                Froster froster = ventSystem.ComponentsV2
                                                    .Select(e => e.Tag)
                                                    .Where(e => e.GetType().Name == nameof(Froster))
                                                    .Cast<Froster>()
                                                    .FirstOrDefault(e => e.KKBGUID == cable.ToGUID);

                                                displacement = froster._KKB.Displacement;
                                                if (acadBlock.IsDynamicBlock)
                                                {
                                                    var propsKKB = acadBlock.GetDynamicBlockProperties();
                                                    foreach (AcadDynamicBlockReferenceProperty prop in propsKKB)
                                                    {
                                                        if (prop.PropertyName == "Выбор1")
                                                            prop.Value = froster._KKB.Stairs.GetEnumDescription();
                                                    }
                                                }

                                                break;

                                            case "ElectroHeater":

                                                ElectroHeater electroheater = ventSystem.ComponentsV2
                                                    .AsParallel()
                                                    .Select(e => e.Tag)
                                                    .Where(e => e.GetType().Name == nameof(ElectroHeater))
                                                    .Cast<ElectroHeater>()
                                                    .FirstOrDefault(e => e.GUID == cable.ToGUID);
                                                displacement = electroheater.Displacement;
                                                if (acadBlock.IsDynamicBlock)
                                                {
                                                    var propsEH = acadBlock.GetDynamicBlockProperties();
                                                    foreach (AcadDynamicBlockReferenceProperty prop in propsEH)
                                                    {
                                                        if (prop.PropertyName == "Выбор1")
                                                            prop.Value = electroheater.StairString;
                                                    }
                                                }
                                                break;

                                            case "Humidifier":
                                                if (acadBlock.IsDynamicBlock)
                                                {
                                                    var propsHUM = acadBlock.GetDynamicBlockProperties();
                                                    Humidifier humidifier = ventSystem.ComponentsV2
                                                        .AsParallel()
                                                        .Select(e => e.Tag)
                                                        .Where(e => e.GetType().Name == nameof(Humidifier))
                                                        .Cast<Humidifier>()
                                                        .FirstOrDefault(e => e.GUID == cable.ToGUID);
                                                    displacement = humidifier.Displacement;
                                                    foreach (AcadDynamicBlockReferenceProperty prop in propsHUM)
                                                    {
                                                        if (prop.PropertyName == "Выбор1")
                                                            switch (acadBlock.EffectiveName)
                                                            {
                                                                case "ED-220-HC":
                                                                    prop.Value = "1";
                                                                    break;

                                                                case "ED-380-HC":
                                                                    prop.Value = "Упр. Вкл./Выкл.";
                                                                    break;
                                                            }
                                                    }
                                                }

                                                break;
                                        } //read displacement

                                        size = Convert.ToDouble(list[1]);
                                        WriteDevAttribute(acadBlock, cable);
                                        cablearrpt = acadBlock.InsertionPoint;
                                        InsertCableBlock(cablearrpt, cable, i, lastdict);
                                    }
                                    else
                                    {
                                        InsertCableBlock(cablearrpt, cable, i, lastdict); //, FLCables);
                                    }

                                    cabcnt++;
                                    if (cabcnt > cnt)
                                    {
                                        startPnt[0] += size + displacement;
                                        //listBox1.Items.Add("change coord " + displacement);
                                    }

                                    i++;
                                }
                            }
                        }

                        if (cableP3_Down.Count > 0)
                        {
                            AcadBlockReference P3DownLast = cableP3_Down.Last();
                            SetLastLine(P3DownLast, min[0]);
                        }

                        if (cableP5_Down.Count > 0)
                        {
                            AcadBlockReference P5DownLast = cableP5_Down.Last();
                            SetLastLine(P5DownLast, min[0]);
                        }

                        AcadBlockReference
                            PannelBlock = InsertPannelBlock(min, startPnt[0], Pannel); // вставка символа шкафа
                        InsertFire_Dispatching(PannelBlock);
                        startPnt = SetNoteAndStamp(PannelBlock);
                        startPnt[0] += 20;
                        startPnt[1] -= 5;

                        acadDoc.Regen(AcRegenType.acActiveViewport);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace + ex.Message);
                result = 1;
                //MessageBox.Show(ex.StackTrace);
            }

            return result;
        }

        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);
        private static List<object> GetRunningInstances(IEnumerable<string> AcadIDs)
        {
            // get the app clsid
            List<string> clsIds = (from progId in AcadIDs select Type.GetTypeFromProgID(progId) into type where type != null select type.GUID.ToString().ToUpper()).ToList();
            // get Running Object Table ...
            GetRunningObjectTable(0, out var Rot);
            if (Rot == null) return null;
            // get enumerator for ROT entries
            Rot.EnumRunning(out var monikerEnumerator);
            if (monikerEnumerator == null) return null;
            monikerEnumerator.Reset();
            List<object> instances = new();
            IntPtr pNumFetched = new();
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
        #region Custom Exception

        private readonly List<AcadBlockReference> cableP3_Down = new();
        private readonly List<AcadBlockReference> cableP5_Down = new();
        private readonly Dictionary<string, Pannel> checkedpannels = new();
        private readonly Dictionary<Cable.CableAttribute, AcadBlockReference> firstCable = new();
        private readonly Dictionary<string, Dictionary<string, Dictionary<string, List<Cable>>>> level3;
        private readonly Dictionary<string, VentSystem> ventsystemDict = new();
        private readonly bool writecabeP;
        private readonly bool writecablePump;
        private readonly bool writecableValve;
        private double[] max;
        private double[] min;
        private Pannel Pannel;
        private AcadBlockReference PrevioscablePblock { get; set; }
        //Dictionary<Cable.CableAttribute, string> lastdict = new Dictionary<Cable.CableAttribute, string>();

        #endregion Custom Exception
        private static void SetLastLine(IAcadBlockReference acadBlock, double minx)
        {
            object[] blockproperies = acadBlock.GetDynamicBlockProperties();
            IEnumerable<AcadDynamicBlockReferenceProperty> visible = blockproperies.OfType<AcadDynamicBlockReferenceProperty>()
                .Where(i => i.PropertyName == "Видимость");
            var acadDynamicBlockReferenceProperties = visible as AcadDynamicBlockReferenceProperty[] ?? visible.ToArray();
            if (acadDynamicBlockReferenceProperties.ElementAt(0).Value == "ВСЁ_овал") return;
            {
                acadDynamicBlockReferenceProperties.ElementAt(0).Value = "ВСЁ_овал";
                IEnumerable<AcadDynamicBlockReferenceProperty> cableline = blockproperies.OfType<AcadDynamicBlockReferenceProperty>()
                    .Where(i => i.PropertyName == "Расстояние5");
                double maxx = acadBlock.InsertionPoint[0];
                double size = maxx - minx - 53.7;
                if (size < 0) size = 0;
                var dynamicBlockReferenceProperties = cableline as AcadDynamicBlockReferenceProperty[] ?? cableline.ToArray();
                dynamicBlockReferenceProperties.ElementAt(0).Value += size;
            }
        }

        private static dynamic StartAutocad((string path, string progID) Acad)
        {
            string programFiles = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
            string acadfile = programFiles + Acad.path;// + " //product ACAD //language " + '\u0022' + "ru - RU" + '\u0022';
            Process acadProc = new();
            acadProc.StartInfo.FileName = acadfile;
            acadProc.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            try
            {
                acadProc.Start();
            }
            catch
            {
                //MessageBox.Show("Не могу найти Autocad 2016, генерация схем невозомжна");
                throw new ApplicationException($"Не могу найти Autocad по пути \n{ acadfile }\nгенерация схем невозомжна");
                //return null;
            }

            acadProc.Start();
            if (!acadProc.WaitForInputIdle(300000))
                throw new ApplicationException("Слишком долго стартует Autocad, выход");
            while (true)
            {
                try
                {
                    dynamic acadApp = Marshal.GetActiveObject(Acad.progID);
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

        private static void WriteDevAttribute(IAcadBlockReference acadBlock, Cable cable)
        {
            if (acadBlock == null) return;
            object[] prop = acadBlock.GetAttributes();
            IEnumerable<AcadAttributeReference> ppp1 = prop.OfType<AcadAttributeReference>().Where(i => i.TagString == "DEVNAME1");
            IEnumerable<AcadAttributeReference> ppp2 = prop.OfType<AcadAttributeReference>().Where(i => i.TagString == "DEVNAME2");
            var acadAttributeReferences = ppp1 as AcadAttributeReference[] ?? ppp1.ToArray();
            if (acadAttributeReferences.Any())
                acadAttributeReferences.ElementAt(0).TextString = cable.Description;

            var attributeReferences = ppp2 as AcadAttributeReference[] ?? ppp2.ToArray();
            if (attributeReferences.Any())
                attributeReferences.ElementAt(0).TextString = cable.ToPosName;
        }

        private void InsertCableBlock(IReadOnlyList<double> cablearrpt, Cable cable, int index, Dictionary<string, string> lastdict)
        {
            double startx = cablearrpt[0];
            double starty = cablearrpt[1];
            double[] cabpt = new double[3];

            switch (cable.ToBlockName)
            {
                case "ED_Pump_220_TK":
                case "ED-220-TK-NORO":
                case "ED-220-Posistor-NORO":
                    switch (index)
                    {
                        case 0:
                            cabpt[0] = startx + 9.8684274;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 1:

                            cabpt[0] = startx + 22.3684274;
                            cabpt[1] = starty - 72.16543687;
                            break;
                    }
                    break;

                case "ED_Pump_220":
                case "ED-24":
                case "ED-220-NORO":
                case "ZASL-220":
                    cabpt[0] = startx + 9.86842739;
                    cabpt[1] = starty - 72.16543687;
                    break;

                case "SENS-TS-2WIRE":
                case "SENS-MS-2WIRE":
                case "SENS-PDS-2WIRE":
                case "Manage-ONOFF":

                    cabpt[0] = startx + 7.5;
                    cabpt[1] = starty - 72.16543687;
                    break;

                case "SENS-TE-2WIRE":
                case "SENS-ME-2WIRE":
                case "SENS-PE-2WIRE":
                    cabpt[0] = startx + 7.5;
                    cabpt[1] = starty - 72.16543687;

                    break;

                case "ED-380-FC":

                    switch (index)
                    {
                        case 0:
                            cabpt[0] = startx + 15.11449886;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 1:
                            cabpt[0] = startx + 30.10098211;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 2:
                            cabpt[0] = startx + 40.13389483;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 3:
                            cabpt[0] = startx + 50.05161304;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 4:
                            cabpt[0] = startx + 60.03515668;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 5:
                            cabpt[0] = startx + 70.01870033;
                            cabpt[1] = starty - 72.16543687;
                            break;
                    }
                    break;

                case "ED-220-HC":
                    switch (index)
                    {
                        case 0:
                            cabpt[0] = startx + 12.61449885;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 1:
                            cabpt[0] = startx + 30.10098211;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 2:
                            cabpt[0] = startx + 40.05161304;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 3:
                            cabpt[0] = startx + 50.05161304;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 4:
                            cabpt[0] = startx + 60.11743847;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 5:
                            cabpt[0] = startx + 70.01870033;
                            cabpt[1] = starty - 72.16543687;
                            break;
                    }
                    break;

                case "ED-380-TK-NORO":
                case "ED-380-Posistor-NORO":
                    switch (index)
                    {
                        case 0:
                            cabpt[0] = startx + 15;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 1:
                            cabpt[0] = startx + 53.72110679;
                            cabpt[1] = starty - 72.16543687;
                            break;
                    }

                    break;

                case "ED-380-NORO":
                    cabpt[0] = startx + 15;
                    cabpt[1] = starty - 72.16543687;
                    break;

                case "ED-380-TK-RO":
                case "ED-380-Posistor-RO":
                case "ED-380-RO":

                    switch (index)
                    {
                        case 0:
                            cabpt[0] = startx + 15;
                            cabpt[1] = starty - 149.96324493;
                            break;

                        case 1:
                            cabpt[0] = startx + 15;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 2:

                            cabpt[0] = startx + 53.72110679;
                            cabpt[1] = starty - 72.16543687;

                            break;

                        case 3:
                            cabpt[0] = startx + 34.99999992;
                            cabpt[1] = starty - 149.96324493;

                            break;
                    }
                    break;

                case "ED-380-TK-Transform":
                    switch (index)
                    {
                        case 0:
                            cabpt[0] = startx + 16.74999994;
                            cabpt[1] = starty - 149.96324493;
                            break;

                        case 1:
                            cabpt[0] = startx + 16.74999994;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 2:
                            cabpt[0] = startx + 34.99999992;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 3:
                            cabpt[0] = startx + 53.72110679;
                            cabpt[1] = starty - 149.96324493;
                            break;
                    }
                    break;

                case "ED-380-Posistor-Transform":
                    switch (index)
                    {
                        case 0:
                            cabpt[0] = startx + 15;
                            cabpt[1] = starty - 149.96324493;
                            break;

                        case 1:
                            cabpt[0] = startx + 15;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 2:
                            cabpt[0] = startx + 34.99999992;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 3:
                            cabpt[0] = startx + 53.72110679;
                            cabpt[1] = starty - 149.96324493;
                            break;
                    }
                    break;

                case "ED-380-Transform":
                    switch (index)
                    {
                        case 0:
                            cabpt[0] = startx + 17.49999992;
                            cabpt[1] = starty - 149.96324493;
                            break;

                        case 1:
                            cabpt[0] = startx + 17.49999992;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 2:
                            cabpt[0] = startx + 53.72110679;
                            cabpt[1] = starty - 72.16543687;

                            break;

                        case 3:
                            cabpt[0] = startx + 34.99999992;
                            cabpt[1] = starty - 149.96324493;

                            break;
                    }
                    break;

                case "ED-380-HC"://увлажнитель
                    switch (index)
                    {
                        case 0:
                            cabpt[0] = startx + 15.11449887;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 1:
                            cabpt[0] = startx + 30.11449894;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 2:
                            cabpt[0] = startx + 40.11449894;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 3:
                            cabpt[0] = startx + 52.61449886;
                            cabpt[1] = starty - 72.16543687;
                            break;
                    }
                    break;

                case "ZASL-220_SQ":
                    switch (index)
                    {
                        case 0:
                            cabpt[0] = startx + 9.8684274;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 1:
                            cabpt[0] = startx + 27.3684274;
                            cabpt[1] = starty - 72.16543687;
                            break;
                    }
                    break;

                case "ED-24-010V":
                    switch (index)
                    {
                        case 0:
                            cabpt[0] = startx + 9.87127349;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 1:
                            cabpt[0] = startx + 22.40532387;
                            cabpt[1] = starty - 72.16543687;
                            break;
                    }
                    break;

                case "EH-220-TK":
                    switch (index)
                    {
                        case 0:
                            cabpt[0] = startx + 12.61449887;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 1:
                            cabpt[0] = startx + 30.10098211;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 2:
                            cabpt[0] = startx + 40.13389483;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 3:
                            cabpt[0] = startx + 50.05161304;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 4:
                            cabpt[0] = startx + 60.03515668;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 5:
                            cabpt[0] = startx + 70.01870033;
                            cabpt[1] = starty - 72.16543687;
                            break;
                    }
                    break;

                case "ED-220-FC":
                    switch (index)
                    {
                        case 0:
                            cabpt[0] = startx + 12.61449887;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 1:
                            cabpt[0] = startx + 25.0980425 + 10 * (index - 1);
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 2:
                            cabpt[0] = startx + 25.0980425 + 10 * (index - 1);
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 3:
                            cabpt[0] = startx + 25.0980425 + 10 * (index - 1);
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 4:
                            cabpt[0] = startx + 25.0980425 + 10 * (index - 1);
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 5:
                            cabpt[0] = startx + 25.0980425 + (10 * (index - 1));
                            cabpt[1] = starty - 72.16543687;
                            break;
                    }
                    break;

                case "ED-220-3POS":
                    switch (index)
                    {
                        case 0:
                            cabpt[0] = startx + 9.87127349;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 1:
                            cabpt[0] = startx + 22.41;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 2:
                            cabpt[0] = startx + 34.87;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 3:
                            cabpt[0] = startx + 49.87;
                            cabpt[1] = starty - 72.16543687;
                            break;
                    }
                    break;

                case "ED-220-TK-RO":
                case "ED-220-Posistor-RO":

                    switch (index)
                    {
                        case 0:
                            cabpt[0] = startx + 9.8684274;
                            cabpt[1] = starty - 149.96324493;
                            break;

                        case 1:
                            cabpt[0] = startx + 9.8684274;
                            cabpt[1] = starty - 72.16543687;
                            break;

                        case 2:
                            cabpt[0] = startx + 22.3684274;
                            cabpt[1] = starty - 72.16543687;
                            break;
                    }
                    break;

                case "ED-220-RO": //вынести отдельно
                    switch (index)
                    {
                        case 0:
                            cabpt[0] = startx + 9.26607424;
                            cabpt[1] = starty - 149.96324493;
                            break;

                        case 1:
                            cabpt[0] = startx + 9.26607424;
                            cabpt[1] = starty - 72.16543687;
                            break;
                    }
                    break;
            }

            string cableblockname;
            string cabletype;
            string cablename;
            if (writecabeP && cable.Attrubute == Cable.CableAttribute.P)
            {
                cableblockname = "cable_dummy";
                cabletype = "Кабель по проекту ЭОМ";
                cablename = "*";
                switch (cable.HostTable)
                {
                    case "Pump":
                        if (writecablePump)
                        {
                            cableblockname = "adv_cable";
                            cabletype = cable.CableType;
                            cablename = cable.CableName;
                        }
                        break;

                    case "Damper":
                    case "Valve":
                        if (writecableValve)
                        {
                            cableblockname = "adv_cable";
                            cabletype = cable.CableType;
                            cablename = cable.CableName;
                        }
                        break;
                }
            }
            else
            {
                cableblockname = "adv_cable";
                cabletype = cable.CableType;
                cablename = cable.CableName;
            }

            dynamic blockObj1 = acadDoc.ModelSpace.InsertBlock(cabpt, cableblockname, 1, 1, 1, 0, Type.Missing);
            AcadBlockReference cableblock = blockObj1;

            object[] attributes = cableblock.GetAttributes();
            object[] blockproperies = cableblock.GetDynamicBlockProperties();
            IEnumerable<AcadDynamicBlockReferenceProperty> distprop1 = blockproperies.OfType<AcadDynamicBlockReferenceProperty>() //получение свойств
                .Where(i => i.PropertyName == "Расстояние1");
            IEnumerable<AcadDynamicBlockReferenceProperty> distprop2 = blockproperies.OfType<AcadDynamicBlockReferenceProperty>() //получение свойств
               .Where(i => i.PropertyName == "Расстояние2");
            IEnumerable<AcadDynamicBlockReferenceProperty> visible = blockproperies.OfType<AcadDynamicBlockReferenceProperty>()
                .Where(i => i.PropertyName == "Видимость");
            IEnumerable<AcadDynamicBlockReferenceProperty> cabYpos = blockproperies.OfType<AcadDynamicBlockReferenceProperty>()
                .Where(i => i.PropertyName == "Положение марки кабеля Y");
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

            distprop1.ElementAt(0).Value = 5.694192;
            distprop2.ElementAt(0).Value = 13.35990458;
            cablenameattr.ElementAt(0).TextString = cablename;
            crosssection.ElementAt(0).TextString = string.Empty;
            if (cabletype != "Кабель по проекту ЭОМ" && cabletype != string.Empty)
            {
                cabletypeattr.ElementAt(0).TextString = cable.CableType;
            }
            else
            {
                if (cabletype != "Кабель по проекту ЭОМ")
                {
                    cabletypeattr.ElementAt(0).TextString = "Null";
                }
            }

            if (cable.Lenght != 0 && cabletype != "Кабель по проекту ЭОМ")
            {
                cablelenght.ElementAt(0).TextString = cable.Lenght + " м";
            }

            if (cable.cableGUID != string.Empty)
            {
                blockGUIDattr.ElementAt(0).TextString = cable.cableGUID;
            }

            switch (cable.Attrubute + cable.WireNumbers.ToString())
            {
                case "P3":

                    cabYpos.ElementAt(0).Value += 4.86;
                    if (cabpt[1] < starty - 133)
                    {
                        cableP3_Down.Add(cableblock);
                    }
                    break;

                case "P5":
                    cabYpos.ElementAt(0).Value += 9.36155431;
                    if (cabpt[1] < starty - 133)
                    {
                        cableP5_Down.Add(cableblock);
                    }
                    break;

                case "A2":
                    cabYpos.ElementAt(0).Value -= 8.83247477;
                    break;

                case "A3":
                    break;

                case "D2":
                    cabYpos.ElementAt(0).Value -= 0;
                    break;

                case "D3":
                    cabYpos.ElementAt(0).Value -= 50.10804803;
                    break;

                case "D4":
                    if (cabpt[1] < starty - 133)
                    {
                        cabYpos.ElementAt(0).Value -= 4.37704919;
                    }
                    else
                    {
                        cabYpos.ElementAt(0).Value -= 82.06611587;
                    }

                    break;

                case "C2":
                    break;

                case "C3":
                    break;

                case "PL3":
                    cabYpos.ElementAt(0).Value -= 87.73567356;
                    break;
            }

            lastdict.TryGetValue(cable.Attrubute + cable.WireNumbers.ToString(), out string lastguid);
            if (lastguid != string.Empty)
            {
                if (cable.cableGUID == lastguid)
                {
                    if (cable.Attrubute == Cable.CableAttribute.P && index == 1)
                    {
                        object[] previoscablePblock_blockproperies = PrevioscablePblock.GetDynamicBlockProperties();
                        IEnumerable<AcadDynamicBlockReferenceProperty> previoscablePblock_visible = previoscablePblock_blockproperies.OfType<AcadDynamicBlockReferenceProperty>()
                            .Where(i => i.PropertyName == "Видимость");
                        IEnumerable<AcadDynamicBlockReferenceProperty> previoscablePblock_cableline = previoscablePblock_blockproperies.OfType<AcadDynamicBlockReferenceProperty>()
                            .Where(i => i.PropertyName == "Расстояние5");
                        double tempminx = min[0];
                        double tempmaxx = cabpt[0];
                        double tempsize = tempmaxx - tempminx - 53.7;
                        previoscablePblock_visible.ElementAt(0).Value = "ВСЁ_овал";
                        var acadDynamicBlockReferenceProperties = previoscablePblock_cableline as AcadDynamicBlockReferenceProperty[] ?? previoscablePblock_cableline.ToArray();
                        acadDynamicBlockReferenceProperties.ElementAt(0).Value += tempsize;
                    }

                    visible.ElementAt(0).Value = "ВСЁ_овал"; // last cable with specifical property
                    IEnumerable<AcadDynamicBlockReferenceProperty> cableline = blockproperies.OfType<AcadDynamicBlockReferenceProperty>()
                        .Where(i => i.PropertyName == "Расстояние5");
                    double minx = min[0];
                    double maxx = cabpt[0];
                    double size = (maxx - minx - 53.7 < 0) ? 0 : maxx - minx - 53.7;
                    var dynamicBlockReferenceProperties = cableline as AcadDynamicBlockReferenceProperty[] ?? cableline.ToArray();
                    dynamicBlockReferenceProperties.ElementAt(0).Value += size;
                }
                else
                {
                    visible.ElementAt(0).Value = "НОМЕР_КАБЕЛЯ_И_ДЛИНЫ_овал";
                }
            }
            else
            {
                if (!(firstCable.ContainsKey(cable.Attrubute)))
                {
                    firstCable.Add(cable.Attrubute, cableblock);
                }
            }
            if (cable.Attrubute == Cable.CableAttribute.P) PrevioscablePblock = cableblock;
        }

        private ArrayList InsertDevBlock(double[] insertPoint, string blockname)
        {
            var blockObj1 = acadDoc.ModelSpace.InsertBlock(insertPoint, blockname, 1, 1, 1, 0, Type.Missing);
            ((AcadEntity)blockObj1).GetBoundingBox(out object minPt, out object maxPt);
            var minPoint = (double[])minPt;
            var maxPoint = (double[])maxPt;
            var size = maxPoint[0] - minPoint[0];
            ArrayList list = new()
            {
                blockObj1,
                size
            };
            return list;
        }
        private void InsertFire_Dispatching(AcadBlockReference PannelBlock)
        {
            object insertpoint = PannelBlock.InsertionPoint;
            double[] ip = (double[])insertpoint;
            PannelBlock.GetBoundingBox(out _, out object maxPt);
            double[] maxP = (double[])maxPt;
            double[] latpoint = new double[3];
            latpoint[0] = maxP[0] - 24.51687801;
            latpoint[1] = ip[1];

            if (Pannel.FireProtect == Pannel._FireProtect.Yes)
            {
                acadDoc.ModelSpace.InsertBlock(latpoint, "Fire_Alarm", 1, 1, 1, 0, Type.Missing);
                latpoint[0] += 12.258439;
            }
            if (Pannel.Dispatching != Pannel._Dispatching.Yes) return;
            dynamic blockObj = acadDoc.ModelSpace.InsertBlock(latpoint, "dispatching", 1, 1, 1, 0, Type.Missing); //вставка блока
            IAcadBlockReference dispatchingblock = blockObj;
            object[] dispatchingAttributes = dispatchingblock.GetAttributes();
            IEnumerable<AcadAttributeReference> dispatching = dispatchingAttributes.OfType<AcadAttributeReference>()
                .Where(i => i.TagString == "ПРОТОКОЛ");
            dispatching.ElementAt(0).TextString = Pannel.Protocol.ToString();
        }

        private AcadBlockReference InsertPannelBlock(double[] min, double maxX, Pannel pannel)
        {
            double[] PannelPoint = new double[3];
            PannelPoint[0] = min[0];
            PannelPoint[1] = min[1] - 170.46952812;
            dynamic blockObj = acadDoc.ModelSpace.InsertBlock(PannelPoint, "Шкаф", 1, 1, 1, 0, Type.Missing);
            AcadBlockReference pannelblock = blockObj;
            object[] blockproperies = pannelblock.GetDynamicBlockProperties();
            IEnumerable<AcadDynamicBlockReferenceProperty> distance1 = blockproperies.OfType<AcadDynamicBlockReferenceProperty>()
                .Where(i => i.PropertyName == "Расстояние1");
            double[] pannelNamePoint = new double[3];
            double distance = maxX - min[0];
            pannelNamePoint[0] = PannelPoint[0] + distance / 2;
            pannelNamePoint[1] = PannelPoint[1] - 8;
            distance1.ElementAt(0).Value = distance + 24.51687801;
            double heighttext = 3;
            acadDoc.ModelSpace.AddText(pannel.PannelName, pannelNamePoint, heighttext);
            return pannelblock;
        }
        private double[] SetNoteAndStamp(AcadBlockReference PannelBlock)
        {
            object insertpoint = PannelBlock.InsertionPoint;
            PannelBlock.GetBoundingBox(out object minPt, out object maxPt);
            double[] szminPt = (double[])minPt;
            double[] szmaxPt = (double[])maxPt;
            double size = szmaxPt[0] - szminPt[0];

            double[] ip = (double[])insertpoint;
            double[] noteIP = new double[3];
            double[] frameIP = new double[3];
            noteIP[0] = ip[0];
            noteIP[1] = ip[1] - 20;
            frameIP[0] = ip[0] - 20;
            frameIP[1] = ip[1] - 73.53047188;
            double multiplicity = Math.Ceiling(size / 185);

            acadDoc.ModelSpace.InsertBlock(noteIP, "note", 1, 1, 1, 0, Type.Missing); //вставка блока примечаний
            dynamic frame = acadDoc.ModelSpace.InsertBlock(frameIP, "_Рамка", 1, 1, 1, 0, Type.Missing); //вставка блока рамки
            AcadBlockReference frameblock = frame;
            object[] blockproperies = frameblock.GetDynamicBlockProperties();
            IEnumerable<AcadDynamicBlockReferenceProperty> frameType = blockproperies.OfType<AcadDynamicBlockReferenceProperty>() //получение свойств
                .Where(i => i.PropertyName == "Выбор формата");

            var acadDynamicBlockReferenceProperties = frameType as AcadDynamicBlockReferenceProperty[] ?? frameType.ToArray();
            if (multiplicity < 3 && multiplicity > 0)
            {
                acadDynamicBlockReferenceProperties.ElementAt(0).Value = "A3альбом";
            }
            if (multiplicity > 2 && multiplicity < 10)
            {
                acadDynamicBlockReferenceProperties.ElementAt(0).Value = $"А4х{Convert.ToInt32(multiplicity)}";
            }

            double[] StampInsertionPoint = new double[3];
            double newpt;
            if (multiplicity == 1)
            {
                newpt = 210 * 2 + frameIP[0] - 5;
            }
            else
            {
                newpt = 210 * multiplicity + frameIP[0] - 5;
            }
            StampInsertionPoint[0] = newpt;
            StampInsertionPoint[1] = frameIP[1] + 5;
            //dynamic stampobj = acadDoc.ModelSpace.InsertBlock(StampInsertionPoint, "Штамп_новый", 1, 1, 1, 0, Type.Missing); //вставка блока штампа
            AcadBlockReference StampBlock = acadDoc.ModelSpace.InsertBlock(StampInsertionPoint, "Штамп_новый", 1, 1, 1, 0, Type.Missing);// stampobj;
            object[] StampAttributes = StampBlock.GetAttributes();
            IEnumerable<AcadAttributeReference> drawingname = StampAttributes.OfType<AcadAttributeReference>()
                .Where(i => i.TagString == "НАИМЕНОВАНИЕ_ЧЕРТЕЖА");
            IEnumerable<AcadAttributeReference> drawingdeveloper = StampAttributes.OfType<AcadAttributeReference>()
                .Where(i => i.TagString == "Developer");
            IEnumerable<AcadAttributeReference> drawingbuild = StampAttributes.OfType<AcadAttributeReference>()
                .Where(i => i.TagString == "Building");
            IEnumerable<AcadAttributeReference> drawingproject = StampAttributes.OfType<AcadAttributeReference>()
                .Where(i => i.TagString == "PROJECT");
            drawingname.ElementAt(0).TextString = $"Схема внешних подключений щита {Pannel.PannelName}";
            drawingdeveloper.ElementAt(0).TextString = Author;
            drawingbuild.ElementAt(0).TextString = BuildingName;
            drawingproject.ElementAt(0).TextString = Project;
            return frameIP;

            //MessageBox.Show($"Distance = {(latpoint[0] - insertPoint[1]).ToString()}");
        }
    }
}