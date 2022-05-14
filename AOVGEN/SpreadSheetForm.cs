﻿using ClosedXML.Excel;
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
using Telerik.Windows.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using Telerik.Windows.Documents.Spreadsheet.Model;

namespace AOVGEN
{
    public partial class SpreadSheetForm : Telerik.WinControls.UI.RadForm
    {
        internal RadTreeView radTreeView4 { get; set; }
        public SpreadSheetForm()
        {
            
            InitializeComponent();
        }


        private void SpreadSheetForm_Load(object sender, EventArgs e)
        {

            Dictionary<string, List<dynamic>> VentDict = new Dictionary<string, List<dynamic>>();
            List<dynamic> listkip = null;
            if (radTreeView4 != null)
            {

                List<Pannel> PannelList = new List<Pannel>();
                foreach (RadTreeNode radTreeNode in radTreeView4.Nodes)
                {
                    Pannel pannel = (Pannel)radTreeNode.Tag;
                    if (radTreeNode.Nodes.Count > 0)
                    {
                        foreach (RadTreeNode ventnode in radTreeNode.Nodes)
                        {
                            VentSystem ventSystem = (VentSystem)ventnode.Tag;
                            listkip = ventSystem.GetKIP();
                            listkip.Insert(0, $"Комплект автоматики для системы {ventSystem.SystemName}");
                            listkip = listkip.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList(); //remove list null entry
                            VentDict.Add(ventSystem.GUID, listkip);
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
            if (VentDict.Count>0 )
            {
                foreach (KeyValuePair <string, List<dynamic> > keyValuePair in VentDict)
                {
                    string GUID = keyValuePair.Key;
                    List<dynamic> kipcomt = keyValuePair.Value;
                    if (kipcomt.Count > 0)
                    {
                        int cnt = 1;
                        foreach (string s in kipcomt)
                        {
                            DataRow kiprow = dt.Rows.Add();
                            kiprow[dt.Columns[0]] = cnt + ".";
                            //kiprow[dt.Columns[1]] = s;
                            kiprow[dt.Columns[1]] = "Тут должно быть значение см SpreadSheetForm строка 88";
                            cnt++;
                        }
                    }
                }
                
               
            }
            



            //for (int i = 0; i < 1000; i++)
            //{
            //    DataRow row = dt.Rows.Add();
            //    foreach (DataColumn col in dt.Columns)
            //    {
            //        row[col.ColumnName] = "Data" + i + "." + col.ColumnName;
            //    }
            //}
            bool shouldImportColumnHeaders = true;
            PopulateSpreadsheet(dt, shouldImportColumnHeaders);
        }
        private void PopulateSpreadsheet(DataTable data, bool shouldImportColumnHeaders)
        {
            Worksheet worksheet = this.radSpreadsheet1.ActiveSheet as Worksheet;
            int startRowIndex = 0;
            if (shouldImportColumnHeaders)
            {
                startRowIndex++;
                for (int i = 0; i < data.Columns.Count; i++)
                {
                    worksheet.Cells[0, i].SetValue(data.Columns[i].ColumnName);
                }
            }

            for (int i = 0; i < data.Rows.Count; i++)
            {
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    worksheet.Cells[startRowIndex + i, j].SetValue(data.Rows[i][j] + string.Empty);
                }
            }
            worksheet.Columns[worksheet.UsedCellRange].AutoFitWidth();
        }

    }
}