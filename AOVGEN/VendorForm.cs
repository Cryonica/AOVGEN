using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Windows.Forms;
using Telerik.WinControls.UI;
using PositionChangedEventArgs = Telerik.WinControls.UI.Data.PositionChangedEventArgs;

namespace AOVGEN
{
    public partial class VendorForm : RadForm
    {
        readonly string DBFilePath;
		internal SQLiteConnection connection;
        public Dictionary<string, string> Sensors { get; private set; }
        internal string vendorname;
		internal string vendorID;
        internal (string VendorID, string VendorName) GetVendorInfo() { return (vendorID, vendorname); }
		public VendorForm(string dbpath, string vendor)
        {
            
			DBFilePath = dbpath;
			connection = OpenDB();
			connection.Open();
			InitializeComponent();
			vendorname = vendor;
        }
		private SQLiteConnection OpenDB()
		{
			string BDPath = DBFilePath;
			string connectionstr = "Data Source=" + BDPath;
			return new SQLiteConnection(connectionstr);


		}
		private void VendorForm_Load(object sender, EventArgs e)
        {
			
			if (connection.State == ConnectionState.Closed) connection = OpenDB();
			SQLiteCommand sQLiteCommand = new()
            {
				Connection = connection,
				CommandText = "SELECT VendorName FROM Vendors"
			};
			RadListDataItem vendorPresent = null;
			using (SQLiteDataReader reader = sQLiteCommand.ExecuteReader())
			{
				while (reader.Read())
				{
                    RadListDataItem radListDataItem = new()
                    {
                        Text = reader[0].ToString()
                    };
                    if (reader[0].ToString() == vendorname) vendorPresent = radListDataItem;
					radDropDownList1.Items.Add(radListDataItem);
				}
			}
			sQLiteCommand.Dispose();
			if (vendorPresent != null)
			{
				radDropDownList1.SelectedItem = vendorPresent;
			}
			else
            {
				radDropDownList1.Text = @"Выброр вендора";
			}
			
		}


        private void RadButton2_Click_1(object sender, EventArgs e)
        {
			connection.Close();
			DialogResult = DialogResult.Cancel;
		}

        private void RadDropDownList1_SelectedIndexChanged(object sender, PositionChangedEventArgs e)
        {
			string selectedVendor = radDropDownList1.SelectedItem.Text;
			List <RadListDataItemCollection> listclear = new()
            {
				radDropDownList2.Items,
				radDropDownList3.Items,
				radDropDownList4.Items,
				radDropDownList5.Items,
				radDropDownList6.Items,
				radDropDownList7.Items,
				radDropDownList8.Items,
				radDropDownList9.Items,
				radDropDownList10.Items,
				radDropDownList11.Items,
				radDropDownList12.Items,
				radDropDownList13.Items,
				radDropDownList14.Items,
				radDropDownList15.Items
			};
			List<(string, RadDropDownList, string)> dropDownLists = new()
            {
				("SensTE", radDropDownList2, "Sens1"),
				("SensTE", radDropDownList3, "Sens2"),
				("SensTE", radDropDownList4, "Sens3"),
				("SensTE", radDropDownList5, "Sens4"),
				("SensTS", radDropDownList6, "Sens5"),
				("SensTS", radDropDownList7, "Sens6"),
				("SensPS", radDropDownList8, "Sens7"),
				("SensPS", radDropDownList9, "Sens8"),
				("SensPS", radDropDownList10, "Sens9"),
				("SensPS", radDropDownList11, "Sens10"),
				("SensHE", radDropDownList12, "Sens11"),
				("SensTE", radDropDownList13, "Sens12"),
				("FControl", radDropDownList14, "Sens13"),
				("FControl", radDropDownList15, "Sens14"),
			};
			foreach (var itemForDel in listclear) itemForDel.Clear();
			SQLiteCommand sQLiteCommand = new()
            {
				Connection = connection,
				//$"SELECT * FROM VendorPresets WHERE VendorName = '{selectedVendor}'"
			};
			for (int t = 0; t <= 13; t++)
            {
				sQLiteCommand.CommandText = $"SELECT VendorCode, Description FROM {dropDownLists[t].Item1} " +
					$"INNER JOIN VendorPresets ON {dropDownLists[t].Item1}.VendorCode =  VendorPresets.{dropDownLists[t].Item3} " +
					$"WHERE {dropDownLists[t].Item1}.VendorName = '{selectedVendor}'";
				using (SQLiteDataReader reader = sQLiteCommand.ExecuteReader())
                {
					while (reader.Read())
                    {
						dropDownLists[t].Item2.Text = reader[1].ToString();
						dropDownLists[t].Item2.Tag = (reader[0].ToString(), dropDownLists[t].Item3);// example: ("5141104010", "Sens1")

					}

				}

			}
			//using (SQLiteDataReader reader = sQLiteCommand.ExecuteReader())
			//{
			//	while (reader.Read())
			//	{
			//		for (int t =0; t<=11; t ++)
			//		{
			//			dropDownLists[t].Item2.Text = reader[t+2].ToString();
			//			dropDownLists[t].Item2.Tag = dropDownLists[t].Item1;
			//		}
			//	}
			//}
			Dictionary<string, List<(string, string)>> dict = new();
			string[] dbTables = { "SensTE", "SensTS", "SensPE", "SensPS", "SensHE", "SensHS", "FControl" };
			foreach (string s in dbTables)
            {
				sQLiteCommand.CommandText = $"SELECT Description, VendorCode FROM {s} WHERE VendorName = '{selectedVendor}'";
				using (SQLiteDataReader reader = sQLiteCommand.ExecuteReader())
                {
					dict.Add(s, GetReaderColumnValues(reader));
				}
					
            }
			foreach (var d in dropDownLists)
            {
				switch (d.Item1)
                {
					case "SensPS":
						List<(string, string)> PEPS = dict["SensPS"];
						if (dict["SensPE"] != null)
						{
							PEPS.AddRange(dict["SensPE"]);
						}
						AddItemsFromList(d.Item2, PEPS);
						break;
					case "SensHE":
						List<(string, string)> HEHS = dict["SensHE"];
						if (dict["SensHS"] != null)
						{
							HEHS.AddRange(dict["SensHS"]);
						}
						AddItemsFromList(radDropDownList12, HEHS);
						break;
					default:
						AddItemsFromList(d.Item2, dict[d.Item1]);
						break;
				}
			
			}
			
		}

        private void RadButton1_Click(object sender, EventArgs e)
        {
			Sensors = new Dictionary<string, string>
            {
                { "Sens1", radDropDownList2.Text},
                { "Sens2", radDropDownList3.Text},
				{ "Sens3", radDropDownList4.Text},
				{ "Sens4", radDropDownList5.Text},
				{ "Sens5", radDropDownList6.Text},
				{ "Sens6", radDropDownList7.Text},
				{ "Sens7", radDropDownList8.Text},
				{ "Sens8", radDropDownList9.Text},
				{ "Sens9", radDropDownList10.Text},
				{ "Sens10",radDropDownList11.Text},
				{ "Sens11",radDropDownList12.Text},
				{ "Sens12",radDropDownList13.Text},
				{ "Sens13",radDropDownList14.Text},
				{ "Sens14",radDropDownList15.Text}
			};
			vendorname = radDropDownList1.Text;
			SQLiteCommand command = new()
            {
				Connection = connection,
				CommandText = $"SELECT ID From VendorPresets WHERE VendorName = '{vendorname}'"
			};
			using (SQLiteDataReader reader = command.ExecuteReader())
            {
				while (reader.Read())
                {
					vendorID = reader[0].ToString();
				}
            }
            DialogResult = DialogResult.OK;
		}
		List<(string, string)> GetReaderColumnValues(SQLiteDataReader reader)
        {
            if (!reader.HasRows) return null;
            DataTable dataTable = new();
            dataTable.Load(reader);
            return dataTable.AsEnumerable()
                .Select(x => 
                {
                    (string item1, string item2) sens;
                    sens.item1 = x.ItemArray[0].ToString();
                    sens.item2 = x.ItemArray[1].ToString();
                    return sens;
                })
                .ToList();

        }

        private static void AddItemsFromList(RadDropDownList radDropDownList, List<(string, string)> list)
        {
			if (list != null)
            {
                foreach (var dataItem in list.Select(s => new RadListDataItem
                {
                    Text = s.Item1,
                    Tag = s.Item2
                }))
                {
                    radDropDownList.Items.Add(dataItem);
                }
            }
			else
            {
				radDropDownList.Items.Clear();

			}
			
        }

        private void RadRepeatButton1_Click(object sender, EventArgs e)
        {
			if (timer != null)
            {
				if (timer.Enabled) timer.Stop();
			}
			
			if (radProgressBar1.Value2 == 100)
            {
				radProgressBar1.Value1 = 0;
				
				return;

			}
			
			if (radProgressBar1.Value1< radProgressBar1.Maximum)
			{
				
				radProgressBar1.Value1++;
				radProgressBar1.Text = radProgressBar1.Value1 + @"%";

			}
			else
			{
				radProgressBar1.Value1 = radProgressBar1.Minimum;
			}

            if (radProgressBar1.Value1 != radProgressBar1.Maximum) return;
            if (radDropDownList1.SelectedItem != null)
            {
                bool result = SavePreset();
                if (result)
                {
                    radProgressBar1.Value2 = 100;
                }
                else
                {
                    if (timer != null)
                    {
                        timer.Start();
                    }
                    else
                    {
                        timer = new Timer
                        {
                            Interval = 10
                        };
                        timer.Tick += TimerProcessor;
                        timer.Start();
                    }
                }
            }
            else
            {
                MessageBox.Show(@"Следует выбрать вендора");
                if (timer != null)
                {
                    timer.Start();
                }
                else
                {
                    timer = new Timer
                    {
                        Interval = 10
                    };
                    timer.Tick += TimerProcessor;
                    timer.Start();
                }
            }

        }
		internal Timer timer;

		private void RadRepeatButton1_MouseUp(object sender, MouseEventArgs e)
        {
			if (radProgressBar1.Value1 < 100)
            {
				if (timer == null)
				{
					timer = new Timer();
				}
				else
                {
					if (!timer.Enabled) timer.Start();
                }

				timer.Interval = 10;
				timer.Tick += TimerProcessor;
				timer.Start();
            }

            if (radProgressBar1.Value2 != 100) return;
            radProgressBar1.Value2 = 0;
            radProgressBar1.Value1 = 0;
            radProgressBar1.Text = radProgressBar1.Value1 + @"%";
        }
		private void TimerProcessor(object obj, EventArgs args)
        {
			if (radProgressBar1.Value1<=0)
            {
				Timer ProcessorTimer = (Timer)obj;
				ProcessorTimer.Stop();
            }

            if (radProgressBar1.Value1 <= 0) return;
            if (radDropDownList1.SelectedItem == null)
            {
                if (radProgressBar1.Value1 >= 30)
                {
                    radProgressBar1.Value1 -= 5;
                }
                else
                {
                    radProgressBar1.Value1--;
                }
					

            }
            else
            {
                radProgressBar1.Value1--;
            }
            radProgressBar1.Text = radProgressBar1.Value1 + @"%";

        }

        private bool SavePreset()
        {
			bool result = false;
			try
            {
				
				List<(RadListDataItemCollection, RadDropDownList)> rads = new()
                {
					(radDropDownList2.Items,radDropDownList2),
					(radDropDownList3.Items,radDropDownList3),
					(radDropDownList4.Items,radDropDownList4),
					(radDropDownList5.Items, radDropDownList5),
					(radDropDownList6.Items, radDropDownList6),
					(radDropDownList7.Items,radDropDownList7),
					(radDropDownList8.Items,radDropDownList8),
					(radDropDownList9.Items,radDropDownList9),
					(radDropDownList10.Items,radDropDownList10),
					(radDropDownList11.Items,radDropDownList11),
					(radDropDownList12.Items,radDropDownList12),
					(radDropDownList13.Items,radDropDownList13),
					(radDropDownList14.Items,radDropDownList14),
					(radDropDownList15.Items,radDropDownList15)
				};
				List<object> senstab = rads.Select(t => 
                    (from RadListDataItem tr in t.Item1 
                        where tr.Text == t.Item2.Text 
                        select tr)
                    .FirstOrDefault()?.Tag)
                    .ToList();
                var newsens= senstab
						.Select(e => e != null ? (string)e : string.Empty)
						.ToArray();

				string updatepresetquery = $"UPDATE VendorPresets SET Sens1 = '{newsens[0]}', Sens2 = '{newsens[1]}', Sens3 = '{newsens[2]}', " +
					$"Sens4 = '{newsens[3]}', Sens5 = '{newsens[4]}', Sens6 = '{newsens[5]}', Sens7 = '{newsens[6]}', Sens8 = '{newsens[7]}', " +
					$"Sens9 = '{newsens[8]}', Sens10 = '{newsens[9]}', Sens11 = '{newsens[10]}', Sens12 = '{newsens[11]}', Sens13 = '{newsens[12]}', " +
					$"Sens14 = '{newsens[13]}' " +
					$"WHERE VendorName = '{radDropDownList1.SelectedItem.Text}'";
				SQLiteCommand sQLiteCommand = new()
                {
					Connection = connection,
					CommandText = updatepresetquery
				};
				sQLiteCommand.ExecuteNonQuery();
				return result = true;
			}
			catch
			{ }
			return result;
			
		}

        private void RadButton3_Click(object sender, EventArgs e)
        {
            if (radDropDownList1.SelectedItem == null) return;
            AddSensorForm addSensorForm = new(connection);
            DialogResult dialogResult =  addSensorForm.ShowDialog();
            if (dialogResult == DialogResult.OK || dialogResult == DialogResult.Cancel) addSensorForm.Close();
        }

        private void RadDropDownList6_MouseClick(object sender, MouseEventArgs e)
        {
            _ = radDropDownList2.SelectedItem;
        }
    }

}
