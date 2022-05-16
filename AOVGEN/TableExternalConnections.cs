using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;
using AutoCAD;
//using Autodesk.AutoCAD.Runtime;
using System.Threading;
using System.ComponentModel;

namespace AOVGEN
{
    
    class TableExternalConnections
    {
        
        [DllImport("user32.dll")]
        internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        internal string Author { get; set; }
        internal string BuildingName { get; set; }
        internal string Project { get; set; }

        
        AcadDocument acadDoc;
        #region Custom Exception
        
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

        readonly Dictionary<string, Dictionary<string, Dictionary<string, List<Cable>>>> level3;
        readonly Dictionary<string, VentSystem> ventsystemDict = new Dictionary<string, VentSystem>();
        readonly Dictionary<string, Pannel> checkedpannels = new Dictionary<string, Pannel>();
        readonly Dictionary<Cable.CableAttribute, AutoCAD.AcadBlockReference> firstCable = new Dictionary<Cable.CableAttribute, AutoCAD.AcadBlockReference>();
        Pannel Pannel;
        readonly List<AutoCAD.AcadBlockReference> cableP3_Down = new List<AutoCAD.AcadBlockReference>();
        readonly List<AutoCAD.AcadBlockReference> cableP5_Down = new List<AutoCAD.AcadBlockReference>();
        readonly bool writecabeP;
        readonly bool writecablePump;
        readonly bool writecableValve;
        readonly bool[] cabsettings = new bool[3];
        double[] min;
        double[] max;
        AutoCAD.AcadBlockReference PrevioscablePblock { get; set; }
        //Dictionary<Cable.CableAttribute, string> lastdict = new Dictionary<Cable.CableAttribute, string>();

        #endregion
        //[CommandMethod("NewDrawing", CommandFlags.Session)]
        internal int Execute()
        {

            (string path, string progID) Acad2019;
            Acad2019.progID = "AutoCAD.Application";
            Acad2019.path = @"\Autodesk\AutoCAD 2019\acad.exe";
            dynamic cadApp = null;
            int result = 0;
            try
            {
                cadApp = Marshal.GetActiveObject(Acad2019.progID);

            }
            catch
            {
               cadApp = StartAutocad(Acad2019);
            }
            if (cadApp != null)
            {
                AutoCAD.AcadApplication acadApp;
                try
                {
                    
                    Process[] localByName = Process.GetProcessesByName("acad");
                    Process process = localByName[0];
                    IntPtr hWnd = process.MainWindowHandle;
                    if (hWnd != IntPtr.Zero)
                    {
                        SetForegroundWindow(hWnd);
                        
                    }


                    acadApp = cadApp;
                    if (acadApp != null)
                    {
                        acadApp.WindowState =  AutoCAD.AcWindowState.acMax;
                    
                        if (acadApp.Documents.Count>0)
                        {

                            foreach (AcadDocument acadDocument in acadApp.Documents) //
                            {
                                if (acadDocument.Name == "Блоки.dwg")
                                {
                                    acadDoc = acadDocument;
                                    acadDoc.Activate();
                                    System.Windows.Forms.SendKeys.Send("{ESC}");
                                    break;
                                }
                                else
                                {
                                    AcadBlocks blocks = acadDocument.Blocks;
                                    if (blocks.Count > 0)
                                    {
                                        AcadBlock shapka = (from AcadBlock block in blocks.AsParallel()
                                                            where block.Name == "Шапка-11"
                                                            select block).FirstOrDefault();
                                        if (shapka != null)
                                        {
                                            acadDoc = acadDocument;
                                            acadDoc.Activate();
                                            System.Windows.Forms.SendKeys.Send("{ESC}");
                                        }


                                    }
                                }
                            }
                        }
                        if (acadDoc == null)
                        {
                            string docpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2021\GKSASU\AOVGen\Blocks.dwt";
                            acadDoc = acadApp.Documents.Add(docpath);
                            acadDoc.Activate();
                        
                        }
                    
                        string prompt1 = "Вставьте блок";
                        double[] startPnt = acadDoc.Utility.GetPoint(Type.Missing, prompt1);

                        double xi = startPnt[0];
                        double yi = startPnt[1];
                        double size;
                        Dictionary<string, List<Cable>> allcables = new Dictionary<string, List<Cable>>();
                        

                        //foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, List<Cable>>>> keyValuePair1 in level3)
                        level3.AsParallel().ForAll(keyValuePair1 =>
                        {
                            Dictionary<string, Dictionary<string, List<Cable>>> level2 = keyValuePair1.Value;
                            List<Cable> cablelist = new List<Cable>();
                            foreach (KeyValuePair<string, Dictionary<string, List<Cable>>> keyValuePair in level2)
                            {
                                string ventname = keyValuePair.Key;
                                VentSystem ventSystem = ventsystemDict[ventname];
                                Dictionary<string, List<Cable>> ventcompdic = keyValuePair.Value;

                                foreach (KeyValuePair<string, List<Cable>> ventcomp in ventcompdic)
                                {
                                    string comppos = ventcomp.Key;
                                    List<Cable> cables = ventcomp.Value;
                                    foreach (Cable cable in cables)
                                    {
                                        cablelist.Add(cable);
                                    }
                                }
                            }
                            allcables.Add(keyValuePair1.Key, cablelist);

                        });   
                        foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, List<Cable>>>> keyValuePair1 in level3)
                        {
                            Dictionary<string, Dictionary<string, List<Cable>>> level2 = keyValuePair1.Value;
                            Dictionary<string, string> lastdict = new Dictionary<string, string>();
                            List<Cable> cablelist = allcables[keyValuePair1.Key];
                        
                            Pannel = new Pannel();
                            Pannel = checkedpannels[keyValuePair1.Key];

                            Cable lastP3 = cablelist.AsParallel()
                                .Where(s => s.Attrubute == Cable.CableAttribute.P && s.WireNumbers == 3)
                                .LastOrDefault();
                            Cable lastP5 = cablelist.AsParallel()
                                .Where(s => s.Attrubute == Cable.CableAttribute.P && s.WireNumbers == 5)
                                .LastOrDefault();
                            Cable lastA2 = cablelist.AsParallel()
                                .Where(s => s.Attrubute == Cable.CableAttribute.A && s.WireNumbers == 2)
                                .LastOrDefault();
                            Cable lastA3 = cablelist.AsParallel()
                                .Where(s => s.Attrubute == Cable.CableAttribute.A && s.WireNumbers == 3)
                                .LastOrDefault();
                            Cable lastD2 = cablelist.AsParallel()
                                .Where(s => s.Attrubute == Cable.CableAttribute.D && s.WireNumbers == 2)
                                .LastOrDefault();
                            Cable lastD3 = cablelist.AsParallel()
                                .Where(s => s.Attrubute == Cable.CableAttribute.D && s.WireNumbers == 3)
                                .LastOrDefault();
                            Cable lastD4 = cablelist.AsParallel()
                                .Where(s => s.Attrubute == Cable.CableAttribute.D && s.WireNumbers == 4)
                                .LastOrDefault();
                            Cable lastC2 = cablelist.AsParallel()
                                .Where(s => s.Attrubute == Cable.CableAttribute.C && s.WireNumbers == 2)
                                .LastOrDefault();
                            Cable lastC3 = cablelist.AsParallel()
                                .Where(s => s.Attrubute == Cable.CableAttribute.C && s.WireNumbers == 3)
                                .LastOrDefault();
                            Cable lastPL3 = cablelist.AsParallel()
                                .Where(s => s.Attrubute == Cable.CableAttribute.PL && s.WireNumbers == 3)
                                .LastOrDefault();
                            if (lastP3 != null) lastdict.Add(lastP3.Attrubute.ToString() + lastP3.WireNumbers.ToString(), lastP3.cableGUID);
                            if (lastP5 != null) lastdict.Add(lastP5.Attrubute.ToString() + lastP5.WireNumbers.ToString(), lastP5.cableGUID);
                            if (lastA2 != null) lastdict.Add(lastA2.Attrubute.ToString() + lastA2.WireNumbers.ToString(), lastA2.cableGUID);
                            if (lastA3 != null) lastdict.Add(lastA3.Attrubute.ToString() + lastA3.WireNumbers.ToString(), lastA3.cableGUID);
                            if (lastD2 != null) lastdict.Add(lastD2.Attrubute.ToString() + lastD2.WireNumbers.ToString(), lastD2.cableGUID);
                            if (lastD3 != null) lastdict.Add(lastD3.Attrubute.ToString() + lastD3.WireNumbers.ToString(), lastD3.cableGUID);
                            if (lastD4 != null) lastdict.Add(lastD4.Attrubute.ToString() + lastD4.WireNumbers.ToString(), lastD4.cableGUID);
                            if (lastC2 != null) lastdict.Add(lastC2.Attrubute.ToString() + lastC2.WireNumbers.ToString(), lastC2.cableGUID);
                            if (lastC3 != null) lastdict.Add(lastC3.Attrubute.ToString() + lastC3.WireNumbers.ToString(), lastC3.cableGUID);
                            if (lastPL3 != null) lastdict.Add(lastPL3.Attrubute.ToString() + lastPL3.WireNumbers.ToString(), lastPL3.cableGUID);
                        
                            //insert first element
                            var blockObj = acadDoc.ModelSpace.InsertBlock(startPnt, "Шапка-11", 1, 1, 1, 0, Type.Missing);
                            // end insert first element
                            (blockObj as AutoCAD.AcadEntity).GetBoundingBox(out object minPt, out object maxPt);
                            min = (double[])minPt;
                            max = (double[])maxPt;
                            size = max[0] - min[0];

                            startPnt[0] += size;

                            foreach (KeyValuePair<string, Dictionary<string, List<Cable>>> keyValuePair in level2)
                            {

                                string ventname = keyValuePair.Key;
                                VentSystem ventSystem = ventsystemDict[ventname];
                                Dictionary<string, List<Cable>> ventcompdic = keyValuePair.Value;
                                foreach (KeyValuePair<string, List<Cable>> ventcomp in ventcompdic)
                                {
                                    string comppos = ventcomp.Key;
                                    List<Cable> cables = ventcomp.Value;
                                    int cnt = cables.Count;
                                    int cabcnt = 1;
                                    double displacement = 0;
                                    int i = 0;
                                    double[] cablearrpt = new double[] { 0, 0, 0 };
                                    foreach (Cable cable in cables)
                                    {

                                        bool printblock = cable.WriteBlock;

                                        if (printblock)
                                        {

                                            ArrayList list = InsertDevBlock(startPnt, cable.ToBlockName);
                                            AutoCAD.AcadBlockReference acadBlock = (AutoCAD.AcadBlockReference)list[0];
                                            switch (cable.CompTable)
                                            {
                                                case "KKB":
                                                    Froster froster = ventSystem.ComponentsV2.AsParallel()
                                                        .Select(e => e.Tag)
                                                        .Where(e => e.GetType().Name == nameof(Froster))
                                                        .Cast<Froster>()
                                                        .Where(e => e.KKBGUID == cable.ToGUID)
                                                        .FirstOrDefault();

                                                    displacement = froster._KKB.Displacement;
                                                    if (acadBlock.IsDynamicBlock)
                                                    {
                                                        var propsKKB = acadBlock.GetDynamicBlockProperties();
                                                        foreach (AutoCAD.AcadDynamicBlockReferenceProperty prop in propsKKB)
                                                        {

                                                            switch (prop.PropertyName)
                                                            {
                                                                case "Выбор1":
                                                                    //EnumExtensionMethods.GetEnumDescription(ventSystem._Froster._KKB.Stairs);
                                                                    prop.Value = EnumExtensionMethods.GetEnumDescription(froster._KKB.Stairs);
                                                                    break;
                                                            }

                                                        }

                                                    }
                                                    break;

                                                case "ElectroHeater":

                                                    ElectroHeater electroheater = ventSystem.ComponentsV2.AsParallel()
                                                        .Select(e => e.Tag)
                                                        .Where(e => e.GetType().Name == nameof(ElectroHeater))
                                                        .Cast<ElectroHeater>()
                                                        .Where(e => e.GUID == cable.ToGUID)
                                                        .FirstOrDefault();
                                                    displacement = electroheater.Displacement;
                                                    if (acadBlock.IsDynamicBlock)
                                                    {
                                                        var propsEH = acadBlock.GetDynamicBlockProperties();
                                                        foreach (AutoCAD.AcadDynamicBlockReferenceProperty prop in propsEH)
                                                        {

                                                            switch (prop.PropertyName)
                                                            {
                                                                case "Выбор1":
                                                                    prop.Value = ventSystem._ElectroHeater.StairString.ToString();
                                                                    break;
                                                            }

                                                        }
                                                    }

                                                    break;
                                                case "Humidifier":
                                                    if (acadBlock.IsDynamicBlock)
                                                    {
                                                        var propsHUM = acadBlock.GetDynamicBlockProperties();
                                                        Humidifier humidifier = ventSystem.ComponentsV2.AsParallel()
                                                            .Select(e => e.Tag)
                                                            .Where(e => e.GetType().Name == nameof(Humidifier))
                                                            .Cast<Humidifier>()
                                                            .Where(e => e.GUID == cable.ToGUID)
                                                            .FirstOrDefault();
                                                        displacement = humidifier.Displacement;
                                                        foreach (AutoCAD.AcadDynamicBlockReferenceProperty prop in propsHUM)
                                                        {

                                                            switch (prop.PropertyName)
                                                            {
                                                                case "Выбор1":
                                                                    switch (acadBlock.EffectiveName)
                                                                    {
                                                                        case "ED-220-HC":
                                                                            prop.Value = "1";
                                                                            break;
                                                                        case "ED-380-HC":
                                                                            prop.Value = "Упр. Вкл./Выкл.";
                                                                            break;

                                                                    }

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
                                            InsertCableBlock(cablearrpt, cable, i, lastdict);//, FLCables);
                                        }
                                        cabcnt++;
                                        if (cabcnt > cnt)
                                        {
                                            startPnt[0] += size + displacement;
                                            //listBox1.Items.Add("change coord " + displacement);
                                        }
                                        i++;
                                        //acadDoc.Regen(AcRegenType.acActiveViewport);
                                    }
                                }

                            }
                            if (cableP3_Down.Count > 0)
                            {
                                AutoCAD.AcadBlockReference P3DownLast = cableP3_Down.Last();
                                SetLastLine(P3DownLast, min[0]);
                            }
                            if (cableP5_Down.Count > 0)
                            {
                                AutoCAD.AcadBlockReference P5DownLast = cableP5_Down.Last();
                                SetLastLine(P5DownLast, min[0]);
                            }
                            AutoCAD.AcadBlockReference PannelBlock = InsertPannelBlock(min, startPnt[0], Pannel); // вставка символа шкафа
                            double[] Fire_DispatchingLastPoint = InsertFire_Dispatching(PannelBlock);
                            startPnt = SetNoteAndStamp(PannelBlock);
                            startPnt[0] += 20;
                            startPnt[1] -= 5;

                            acadDoc.Regen(AutoCAD.AcRegenType.acActiveViewport);
                        
                        }
                    }
                    else
                    {
                        MessageBox.Show("Не удалось найти Autocad в авт.режиме\nЗапустите Autocad самостоятельно\nи дождитесь полной его загрузки");
                    }
                                        
                    
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.StackTrace + ex.Message);
                    result = 1;
                    //MessageBox.Show(ex.StackTrace);
                }

                
            }
            
               


                
                //string strTemplatePath = "acad.dwt";
                //DocumentCollection acDocMgr = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
                //Document acDoc = acDocMgr.Add(strTemplatePath);
                //acDocMgr.MdiActiveDocument = acDoc;

            
            return result;
            
        }
        private void WriteDevAttribute(AutoCAD.AcadBlockReference acadBlock, Cable cable)
        {
            if (acadBlock != null)
            {
                //if (acadBlock.IsDynamicBlock)
                //{
                object[] prop = acadBlock.GetAttributes();
                IEnumerable<AutoCAD.AcadAttributeReference> ppp1 = prop.OfType<AutoCAD.AcadAttributeReference>().Where(i => i.TagString == "DEVNAME1"); 
                IEnumerable<AutoCAD.AcadAttributeReference> ppp2 = prop.OfType<AutoCAD.AcadAttributeReference>().Where(i => i.TagString == "DEVNAME2"); 
                if (ppp1 != null) 
                    if (ppp1.Count()>0)
                        ppp1.ElementAt(0).TextString = cable.Description;
               
                if (ppp2 != null) 
                    if (ppp2.Count()>0)
                        ppp2.ElementAt(0).TextString = cable.ToPosName;

                //for (int c = 0; c < prop.Length; c++)
                //    {

                //        AcadAttributeReference reference = prop[c] as AcadAttributeReference;
                //        string PropertyName = reference.TagString;
                //        switch (PropertyName)
                //        {
                //            case "DEVNAME1":
                //                reference.TextString = cable.Description;
                //                break;
                //            case "DEVNAME2":
                //                reference.TextString = cable.ToPosName;
                //                break;
                //        }
                //    }
              
            }
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
        private void InsertCableBlock(double[] cablearrpt, Cable cable, int index, Dictionary<string, string> lastdict)
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
                            cabpt[0] = startx + 25.0980425 + (10 * (index-1));
                            cabpt[1] = starty - 72.16543687;
                            break;
                        case 2:
                            cabpt[0] = startx + 25.0980425 + (10 * (index - 1));
                            cabpt[1] = starty - 72.16543687;
                            break;
                        case 3:
                            cabpt[0] = startx + 25.0980425 + (10 * (index - 1));
                            cabpt[1] = starty - 72.16543687;
                            break;
                        case 4:
                            cabpt[0] = startx + 25.0980425 + (10 * (index - 1));
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
            var blockObj1 = (dynamic)null;
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
           
            blockObj1 = acadDoc.ModelSpace.InsertBlock(cabpt, cableblockname, 1, 1, 1, 0, Type.Missing); //вставка блока
            AutoCAD.AcadBlockReference cableblock = blockObj1;
           
            string CableID = cableblock.Handle;
            object[] attributes = cableblock.GetAttributes();
            object[] blockproperies = cableblock.GetDynamicBlockProperties();
            IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> distprop1 = blockproperies.OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
                .Where(i => i.PropertyName == "Расстояние1");
            IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> distprop2 = blockproperies.OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
               .Where(i => i.PropertyName == "Расстояние2");
            IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> visible = blockproperies.OfType<AutoCAD.AcadDynamicBlockReferenceProperty>()
                .Where(i => i.PropertyName == "Видимость");
            IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> cabYpos = blockproperies.OfType<AutoCAD.AcadDynamicBlockReferenceProperty>()
                .Where(i => i.PropertyName == "Положение марки кабеля Y");
            IEnumerable<AutoCAD.AcadAttributeReference> cablenameattr = attributes.OfType<AcadAttributeReference>()
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
                //string[] splittype = cable.CableType.Split(' ');
                cabletypeattr.ElementAt(0).TextString = cable.CableType;
                
                   
            }
            else
            {
                if (cabletype != "Кабель по проекту ЭОМ")
                {
                    cabletypeattr.ElementAt(0).TextString = "Null";
                    //crosssection.ElementAt(0).TextString = "";
                }
                
            }
                
            if (cable.Lenght != 0 && cabletype != "Кабель по проекту ЭОМ")
            {
              
                cablelenght.ElementAt(0).TextString = cable.Lenght.ToString() + " м";
            }


           
            if (cable.cableGUID != string.Empty)
            {
                blockGUIDattr.ElementAt(0).TextString = cable.cableGUID;
            }

            switch (cable.Attrubute.ToString() + cable.WireNumbers.ToString())
            {
                case "P3":
                    //cabYpos.ElementAt(0).Value += 9.30395688;
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
            
            lastdict.TryGetValue(cable.Attrubute.ToString() + cable.WireNumbers.ToString(), out string lastguid);
            if (lastguid != string.Empty)
            {
                
                if (cable.cableGUID == lastguid)
                {
                    
                    if (cable.Attrubute == Cable.CableAttribute.P && index ==1)
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
                        previoscablePblock_cableline.ElementAt(0).Value = previoscablePblock_cableline.ElementAt(0).Value + tempsize;

                    }
                    
                    
                    visible.ElementAt(0).Value = "ВСЁ_овал"; // last cable with specifical property
                    IEnumerable<AcadDynamicBlockReferenceProperty> cableline = blockproperies.OfType<AcadDynamicBlockReferenceProperty>()
                        .Where(i => i.PropertyName == "Расстояние5");
                    double minx = min[0];
                    double maxx = cabpt[0];
                    double size = (maxx - minx - 53.7 < 0) ? 0 : maxx - minx - 53.7;
                    
                    //cablename.ElementAt(0).TextString = cable.Attrubute.ToString() + cable.WireNumbers.ToString();
                   
                    cableline.ElementAt(0).Value = cableline.ElementAt(0).Value + size;

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
            if (cable.Attrubute == Cable.CableAttribute.P) this.PrevioscablePblock = cableblock;

        }
        private void SetLastLine(AutoCAD.AcadBlockReference acadBlock, double minx)
        {
            object[] blockproperies = acadBlock.GetDynamicBlockProperties();
            IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> visible = blockproperies.OfType<AutoCAD.AcadDynamicBlockReferenceProperty>()
                .Where(i => i.PropertyName == "Видимость");
            if (visible.ElementAt(0).Value != "ВСЁ_овал")
            {
                visible.ElementAt(0).Value = "ВСЁ_овал";
                IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> cableline = blockproperies.OfType<AutoCAD.AcadDynamicBlockReferenceProperty>()
                    .Where(i => i.PropertyName == "Расстояние5");
                double maxx = acadBlock.InsertionPoint[0];
                double size = maxx - minx - 53.7;
                if (size < 0) size = 0;
                cableline.ElementAt(0).Value = cableline.ElementAt(0).Value + size;
            }

        }
        private AutoCAD.AcadBlockReference InsertPannelBlock(double[] min, double maxX, Pannel pannel)
        {
            var blockObj = (dynamic)null;
            double[] PannelPoint = new double[3];
            PannelPoint[0] = min[0];
            PannelPoint[1] = min[1] - 170.46952812;
            blockObj = acadDoc.ModelSpace.InsertBlock(PannelPoint, "Шкаф", 1, 1, 1, 0, Type.Missing); //вставка блока
            AutoCAD.AcadBlockReference pannelblock = blockObj;
            object[] blockproperies = pannelblock.GetDynamicBlockProperties();
            IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> distance1 = blockproperies.OfType<AutoCAD.AcadDynamicBlockReferenceProperty>()
                .Where(i => i.PropertyName == "Расстояние1");
            double[] pannelNamePoint = new double[3];
            double distance = maxX - min[0];
            pannelNamePoint[0] = PannelPoint[0] + distance / 2;
            pannelNamePoint[1] = PannelPoint[1] - 8;
            distance1.ElementAt(0).Value = distance + 24.51687801;
            double heighttext = 3;
            dynamic textObj = acadDoc.ModelSpace.AddText(pannel.PannelName, pannelNamePoint, heighttext);
            return pannelblock;

        }
        private double[] InsertFire_Dispatching(AutoCAD.AcadBlockReference PannelBlock)
        {
            object insertpoint;
            insertpoint = PannelBlock.InsertionPoint;
            double[] ip = (double[])insertpoint;
            PannelBlock.GetBoundingBox(out _, out object maxPt);
            double[] maxP = (double[])maxPt;
            double[] latpoint = new double[3];
            latpoint[0] = maxP[0] - 24.51687801;
            latpoint[1] = ip[1];
           
            if (Pannel.FireProtect == Pannel._FireProtect.Yes)
            {
                acadDoc.ModelSpace.InsertBlock(latpoint, "Fire_Alarm", 1, 1, 1, 0, Type.Missing);
                latpoint[0] = latpoint[0] + 12.258439;
                
            }
            if (Pannel.Dispatching == Pannel._Dispatching.Yes)
            {
                dynamic blockObj = acadDoc.ModelSpace.InsertBlock(latpoint, "dispatching", 1, 1, 1, 0, Type.Missing); //вставка блока
                AutoCAD.AcadBlockReference dispatchingblock = blockObj;
                object[] dispatchingAttributes = dispatchingblock.GetAttributes();
                IEnumerable<AutoCAD.AcadAttributeReference> dispatching = dispatchingAttributes.OfType<AutoCAD.AcadAttributeReference>()
                    .Where(i => i.TagString == "ПРОТОКОЛ");
                dispatching.ElementAt(0).TextString = Pannel.Protocol.ToString();

            }
            return latpoint;

        }
        private double[] SetNoteAndStamp(AutoCAD.AcadBlockReference PannelBlock)
        {
            object insertpoint;
            insertpoint = PannelBlock.InsertionPoint;
            PannelBlock.GetBoundingBox(out object minPt, out object maxPt);
            double[] szminPt = (double[])minPt;
            double[] szmaxPt = (double[])maxPt;
            double size = szmaxPt[0] - szminPt[0];

            double[] ip = (double[])insertpoint;
            double[] noteIP = new double[3];
            double[] frameIP = new double[3];
            noteIP[0] = ip[0];
            noteIP[1] = ip[1]-20;
            frameIP[0] = ip[0] -20;
            frameIP[1] = ip[1] - 73.53047188;
            double multiplicity = Math.Ceiling(size / 185);
            
            acadDoc.ModelSpace.InsertBlock(noteIP, "note", 1, 1, 1, 0, Type.Missing); //вставка блока примечаний
            dynamic frame= acadDoc.ModelSpace.InsertBlock(frameIP, "_Рамка", 1, 1, 1, 0, Type.Missing); //вставка блока рамки
            AutoCAD.AcadBlockReference frameblock = frame;
            object[] blockproperies = frameblock.GetDynamicBlockProperties();
            IEnumerable<AutoCAD.AcadDynamicBlockReferenceProperty> frameType = blockproperies.OfType<AutoCAD.AcadDynamicBlockReferenceProperty>() //получение свойств
                .Where(i => i.PropertyName == "Выбор формата");
            
            
            
            if (multiplicity < 3 && multiplicity > 0)
            {
                frameType.ElementAt(0).Value = "A3альбом";
            }
            if (multiplicity >2 && multiplicity <10)
            {
                frameType.ElementAt(0).Value = $"А4х{Convert.ToInt32(multiplicity)}";
            }
            
            double[] StampInsertionPoint = new double[3];
            double newpt = 0;
            if ( multiplicity==1)
            {
                newpt = 210 * 2 + frameIP[0] - 5;
            }
            else
            {
                 newpt= 210 * multiplicity + frameIP[0] - 5;
            }
            StampInsertionPoint[0] = newpt;
            StampInsertionPoint[1] = frameIP[1] + 5;
            //dynamic stampobj = acadDoc.ModelSpace.InsertBlock(StampInsertionPoint, "Штамп_новый", 1, 1, 1, 0, Type.Missing); //вставка блока штампа
            AutoCAD.AcadBlockReference StampBlock = acadDoc.ModelSpace.InsertBlock(StampInsertionPoint, "Штамп_новый", 1, 1, 1, 0, Type.Missing);// stampobj;
            object[] StampAttributes = StampBlock.GetAttributes();
            IEnumerable<AutoCAD.AcadAttributeReference> drawingname = StampAttributes.OfType<AutoCAD.AcadAttributeReference>()
                .Where(i => i.TagString == "НАИМЕНОВАНИЕ_ЧЕРТЕЖА");
            IEnumerable<AutoCAD.AcadAttributeReference> drawingdeveloper = StampAttributes.OfType<AutoCAD.AcadAttributeReference>()
                .Where(i => i.TagString == "Developer");
            IEnumerable<AutoCAD.AcadAttributeReference> drawingbuild = StampAttributes.OfType<AutoCAD.AcadAttributeReference>()
                .Where(i => i.TagString == "Building");
            IEnumerable<AutoCAD.AcadAttributeReference> drawingproject = StampAttributes.OfType<AutoCAD.AcadAttributeReference>()
                .Where(i => i.TagString == "PROJECT");
            drawingname.ElementAt(0).TextString = $"Схема внешних подключений щита {Pannel.PannelName}";
            drawingdeveloper.ElementAt(0).TextString = Author;
            drawingbuild.ElementAt(0).TextString = BuildingName;
            drawingproject.ElementAt(0).TextString = Project;
            return frameIP;


            //MessageBox.Show($"Distance = {(latpoint[0] - insertPoint[1]).ToString()}");
        }
        private dynamic StartAutocad((string path, string progID) Acad)
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
                //MessageBox.Show("Не могу найти Autocad 2016, генерация схем невозомжна");
                throw new ApplicationException($"Не могу найти Autocad по пути \n{ acadfile }\nгенерация схем невозомжна");
                //return null;
            }
            
            acadProc.Start();
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
        internal  TableExternalConnections(Dictionary<string, Dictionary<string, Dictionary<string, List<Cable>>>> inputdict, Dictionary<string, VentSystem> inputvensystemDict, bool[] inputcabsettings, Dictionary<string, Pannel> pannels  )
        {
            cabsettings = inputcabsettings;
            level3 = inputdict;
            writecabeP = cabsettings[0];
            writecablePump = cabsettings[1];
            writecableValve = cabsettings[2];
            ventsystemDict = inputvensystemDict;
            checkedpannels = pannels;


        }
        

    }
    public static class EnumExtensionMethods
    {
        public static string GetEnumDescription(this Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            var descriptionAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : enumValue.ToString();
        }
    }

}

