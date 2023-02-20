using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Telerik.WinControls.Data;
using Telerik.WinControls.UI;

namespace AOVGEN
{
    public partial class AddSensorForm : RadForm
    {
        readonly SQLiteConnection connection;
        bool isdeleted;
        bool isupdated;
       
        internal DataTable VendorsDataTable;
        
        public AddSensorForm(SQLiteConnection qLiteConnection)
        {
            connection = qLiteConnection;
           
            InitializeComponent();
           
            SQLiteCommand sQLiteCommand = new()
            {
                Connection = connection,
                CommandText = "SELECT VendorName FROM Vendors"
            };
            using (SQLiteDataReader reader = sQLiteCommand.ExecuteReader())
            {
                VendorsDataTable = new DataTable();
                VendorsDataTable.Load(reader);
            }
        }

        private void AddSensorForm_Load(object sender, EventArgs e)
        {

            
            radGridView1.MasterTemplate.AutoGenerateColumns = false;
            List<string> DBTable = new()
            {
               "SensTE", "SensTS", "SensPE", "SensPS", "SensHE", "SensHS"
            };
            GridViewComboBoxColumn comboCol = new("Тип датчика(*)")
            {
                DataSource = DBTable.ToArray(),
                FieldName = "Тип датчика(*)"
            };
            
            var vendorsarr = VendorsDataTable.AsEnumerable()
                .Select(r => r.ItemArray[0].ToString());
            GridViewComboBoxColumn selectvendor = new("Имя вендора(*)")
            {
                DataSource = vendorsarr,
                FieldName = "Имя вендора(*)"
            };
            radGridView1.Columns.Add(new GridViewTextBoxColumn("ID"));
            //radGridView1.Columns.Add(new GridViewTextBoxColumn("Имя вендора(*)"));
            radGridView1.Columns.Add(selectvendor);
            radGridView1.Columns.Add(new GridViewTextBoxColumn("Название блока"));
            radGridView1.Columns.Add(new GridViewTextBoxColumn("Артикул"));
            radGridView1.Columns.Add(new GridViewTextBoxColumn("Описание(*)"));
            radGridView1.Columns.Add(new GridViewTextBoxColumn("Принадлежности"));
            radGridView1.Columns.Add(comboCol);
            
            radGridView1.Columns["ID"].IsVisible = false;
            radGridView1.Columns["Тип датчика(*)"].TextAlignment = ContentAlignment.MiddleCenter;
            radGridView1.Columns["Тип датчика(*)"].AutoSizeMode = BestFitColumnMode.SummaryRowCells;
            radGridView1.Columns["Описание(*)"].TextAlignment = ContentAlignment.MiddleLeft;
            radGridView1.Columns["Описание(*)"].AutoSizeMode = BestFitColumnMode.SummaryRowCells;
            radGridView1.Columns["Артикул"].TextAlignment = ContentAlignment.MiddleCenter;
            //radGridView1.Columns["Артикул"].ReadOnly = true;
                      
            radGridView1.AutoSizeColumnsMode = GridViewAutoSizeColumnsMode.Fill;
            radGridView1.Columns["Имя вендора(*)"].Width = 15;
            radGridView1.Columns["Артикул"].Width = 15;
            radGridView1.Columns["Название блока"].Width = 15;
            radGridView1.Columns["Тип датчика(*)"].Width = 15;
            radGridView1.RowsChanged += radGridView1_RowsChanged;
        }

        private void radButton1_Click(object sender, EventArgs e)
        {
            //if (radGridView1.RowCount>0)
            //{
                
            //    List<(string, string, string, string, string, string)> datalist = radGridView1.Rows
            //            .Where(r => !string.IsNullOrEmpty(r.Cells["Имя вендора(*)"]?.Value?.ToString()))
            //            .Where(r => !string.IsNullOrEmpty(r.Cells["Тип датчика(*)"]?.Value?.ToString()))
            //            .Select(d =>
            //            {
            //                (string VendorName, string BlockName, string VendorCode, string Description, string Assignment, string DBtable) tuple;
            //                tuple.VendorName = d?.Cells[1].Value?.ToString();
            //                tuple.BlockName = d?.Cells[2].Value?.ToString();
            //                tuple.VendorCode = d?.Cells[3].Value?.ToString();
            //                tuple.Description = d?.Cells[4].Value?.ToString();
            //                tuple.Assignment = d?.Cells[5].Value?.ToString();
            //                tuple.DBtable = d?.Cells[6].Value?.ToString();
            //                return tuple;
                           
            //            })
            //            .ToList();

            //    if (datalist.Count>0)
            //    {
                    
            //        try
            //        {
            //            SQLiteCommand command = new SQLiteCommand
            //            {
            //                Connection = connection
            //            };
            //            foreach ((string VendorName, string BlockName, string VendorCode, string Description, string Assignment, string DBtable) in datalist)
            //            {
            //                if (VendorName != null && BlockName != null && VendorCode != null && Description != null && Assignment != null && DBtable != null)
            //                {
            //                    string query;
            //                    string checkquery = $"SELECT VendorCode FROM {DBtable} WHERE VendorName = '{VendorName}'";
            //                    command.CommandText = checkquery;
            //                    using (SQLiteDataReader reader = command.ExecuteReader())
            //                    {
            //                        if (reader.HasRows)
            //                        {
            //                            query = $"UPDATE {DBtable} SET Description= '{Description}' WHERE VendorCode= '{VendorCode}'";
            //                        }
            //                        else
            //                        {
            //                            query = $"INSERT INTO {DBtable} (VendorName, BlockName, VendorCode, Assignment, Description) " +
            //                                $"VALUES ('{VendorName}', '{BlockName}', '{VendorCode}', '{Assignment}', '{Description}')";
            //                        }
            //                    }

            //                    command.CommandText = query;
            //                    command.ExecuteNonQuery();
            //                }
                            
            //                this.Close();

            //            }
            //        }
            //        catch { MessageBox.Show("Запись в БД не удалась"); }
                    
            //    }
                
            //}
            //DialogResult = DialogResult.OK;

        }

        private void radButton2_Click(object sender, EventArgs e)
        {
            radGridView1.RowsChanging -= radGridView1_RowsChanging;
            radGridView1.RowsChanged -= radGridView1_RowsChanged;
            radGridView1.Rows.Clear();

            List<string> DBTable = new()
            {
               "SensTE", "SensTS", "SensPE", "SensPS", "SensHE", "SensHS"
            };
            List<string> vendorsList = VendorsDataTable.AsEnumerable()
                .Select(r => r.ItemArray[0].ToString())
                .ToList();
            List<(string SenstDB, string Vendor)> Listforquery = new();
            foreach (string vendor in vendorsList)
            {
                foreach (string sens in DBTable)
                {
                    Listforquery.Add((vendor, sens));
                }
            }
            SQLiteCommand sQLiteCommand = new()
            {
                Connection = connection
            };
            int cnt = radGridView1.RowCount;
            foreach ((string vendor, string dbtable) in Listforquery)
            {
                cnt++;
                sQLiteCommand.CommandText = $"SELECT VendorName, BlockName, VendorCode, Assignment, Description FROM {dbtable} " +
                    $"WHERE VendorName = '{vendor}'";
                

                using (SQLiteDataReader reader1 = sQLiteCommand.ExecuteReader())
                {
                    
                    while (reader1.Read())
                    {
                        GridViewDataRowInfo rowInfo = new(radGridView1.MasterView);
                        rowInfo.Cells[0].Value = cnt;
                        
                        rowInfo.Cells[1].Value = reader1[0].ToString();
                        rowInfo.Cells[2].Value = reader1[1].ToString();
                        rowInfo.Cells[3].Value = reader1[2].ToString();
                        rowInfo.Cells[4].Value = reader1[4].ToString();
                        rowInfo.Cells[5].Value = reader1[3].ToString();
                        rowInfo.Cells[6].Value = dbtable;
                        radGridView1.Rows.Add(rowInfo);
                    }
                    


                }

            }
            radGridView1.RowsChanging += radGridView1_RowsChanging;
            radGridView1.RowsChanged += radGridView1_RowsChanged;
        }

        private void radGridView1_RowsChanging(object sender, GridViewCollectionChangingEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                GridViewDataRowInfo row = (GridViewDataRowInfo)e.OldItems[0] ;
                if (row != null)
                {
                    string dbtable = row.Cells[6].Value?.ToString();
                    string vendorcode = row.Cells[3].Value?.ToString();
                    if (dbtable != null && vendorcode != null)
                    {
                        string query = $"SELECT * FROM {dbtable} WHERE VendorCode = '{vendorcode}'";
                        SQLiteCommand sQLiteCommand = new()
                        {
                            Connection = connection,
                            CommandText = query
                        };
                        using (SQLiteDataReader reader = sQLiteCommand.ExecuteReader())
                        {
                            query = reader.HasRows ? $"DELETE FROM {dbtable} WHERE VendorCode = '{vendorcode}'" : string.Empty;

                        }
                        if (!string.IsNullOrEmpty(query))
                        {
                            sQLiteCommand.CommandText = query;
                            sQLiteCommand.ExecuteNonQuery();
                        }
                        e.Cancel = false;
                        isdeleted = true;
                        
                        

                    }                    
                }
                

            }
            
            if (e.Action == NotifyCollectionChangedAction.ItemChanged)
            {
                _ = (GridViewDataRowInfo)e.OldItems[0];
                isupdated = true;
            }
            if (e.Action == NotifyCollectionChangedAction.ItemChanging)
            {
                isupdated = true;
            }


        }

        private void radGridView1_CellEndEdit(object sender, GridViewCellEventArgs e)
        {

            if ( radGridView1.CurrentCell.RowIndex > 0) //not new row
            {
                if (!string.IsNullOrEmpty(e.Row.Cells[6].Value.ToString())) //present datatable name
                {
                    if (e.ColumnIndex != 3)
                    {
                        string columnname = string.Empty;
                        switch (e.Column.HeaderText)
                        {
                            case "Описание(*)":
                                columnname = "Description";
                                break;
                            case "Принадлежности":
                                columnname = "Assignment";
                                break;
                            case "Название блока":
                                columnname = "BlockName";
                                break;
                        }
                        if (!string.IsNullOrEmpty(columnname))
                        {
                            SQLiteCommand command = new()
                            {
                                Connection = connection,
                                CommandText = $"UPDATE {e.Row.Cells[6].Value} SET {columnname}  = '{e.Row.Cells[e.Column.Index].Value}' WHERE VendorCode = '{e.Row.Cells[3].Value}'"
                            };
                            command.ExecuteNonQuery();
                            command.Dispose();
                            isupdated = true;
                        }

                    }
                }
            }

            _ = e.ColumnIndex;
            _ = e.Column.Index;
            _ = e.Row.Cells[e.Column.Index].Value;
            if (isupdated) isupdated = false;

            
            
        }
        private void radButton3_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void radGridView1_RowsChanged(object sender, GridViewCollectionChangedEventArgs e)
        {
            GridViewDataRowInfo row = (GridViewDataRowInfo)e.NewItems[0];
            if (row != null)
            {
                List<string> u1 = cellsinfo(row).ToList();
               
                if (u1[1] == null || u1[3] == null || u1[6] == null)
                {
                    radGridView1.Rows.Remove(row);
                }
                else
                {
                    if (!isdeleted)
                    {
                        if (!isupdated)
                        {
                            SQLiteCommand command = new()
                            {
                                Connection = connection,
                                CommandText = $"INSERT INTO {u1[6]} (VendorName, BlockName, VendorCode, Assignment, Description) " +
                                            $"VALUES ('{u1[1]}', '{u1[2]}', '{u1[3]}', '{u1[5]}', '{u1[4]}')"
                            };
                            command.ExecuteNonQuery();
                            command.Dispose();
                        }
                        //else isupdated = false;
                        
                    }
                    else isdeleted = false;
                }
                //write data to database!!!!
                
                IEnumerable<string> cellsinfo(GridViewDataRowInfo inputrow)
                {
                    int cnt = inputrow.Cells.Count; 
                    for (int n = 0; n < cnt; n++) 
                    {
                        yield return row.Cells[n]?.Value?.ToString(); 
                    }

                }


            }
            
        }
    }
}
