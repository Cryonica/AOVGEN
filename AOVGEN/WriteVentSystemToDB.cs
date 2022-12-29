using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AOVGEN
{
    internal class WriteVentSystemToDB
	{
		public static bool Execute(string connectionstr, VentSystem ventSystem, Project Project, Building Building)
		{
			#region Initialisation variables
			bool result = false;
			string ProjectGuid = Project.GetGUID();
			bool writpumpcable = false;
			bool writevalvecable = false;
			string sensguid1 = string.Empty;
			string query = $"SELECT WritePumpCable, WriteValveCable FROM BuildSetting WHERE Place = '{Building.BuildGUID}'";
			SQLiteConnection connection = new SQLiteConnection(connectionstr);
			connection.Open();
			SQLiteCommand command = new SQLiteCommand
			{
				Connection = connection,
				CommandText = query
			};
			
			#endregion
			#region Check enables Pump and Valve Cables for write
			using (SQLiteDataReader readersettings = command.ExecuteReader())
			{
				while (readersettings.Read())
				{
					writpumpcable = Convert.ToBoolean(readersettings[0].ToString());
					writevalvecable = Convert.ToBoolean(readersettings[1].ToString());
				}
			}
			#endregion
			try
			{
				#region Find Supply and Exhaust
				//***change filtr to filtr supply and exhaust filtr
				EditorV2.PosInfo SupplyYLine = (from y in ventSystem.ComponentsV2
						.Where(d => d.Tag.GetType().Name == nameof(SupplyVent) || d.Tag.GetType().Name == nameof(SpareSuplyVent))
												select y)
						.FirstOrDefault();
				EditorV2.PosInfo ExhaustYLine = (from y in ventSystem.ComponentsV2
						.Where(d => d.Tag.GetType().Name == nameof(ExtVent) || d.Tag.GetType().Name == nameof(SpareExtVent))
												 select y)
						.FirstOrDefault();
				List<EditorV2.PosInfo> Rooms = ventSystem.ComponentsV2
						.Where(d => d.Tag.GetType().Name == nameof(Room))
						.ToList();
				List<EditorV2.PosInfo> CrossSectionNulled = ventSystem.ComponentsV2
					.Where(d => d.Tag.GetType().Name == nameof(CrossSection))
					.Where(d => d.PozY != SupplyYLine?.PozY)
					.Where(d => d.PozY != ExhaustYLine?.PozY)
					.ToList();

				
				if (SupplyYLine != null) //если есть приточный вентилятор (то есть линия по Y для притока не равна Null
				{
					var supplyFiltrList = ventSystem.ComponentsV2
						.Where(d => d.Tag.GetType().Name == nameof(Filtr)) //вот тут мы ищем фильтры. у меня есть 2 класса фильтров:
						.Where(d => d.PozY == SupplyYLine.PozY)
						.ToList();
					List<EditorV2.PosInfo> CrossectionSupply = ventSystem.ComponentsV2
					   .Where(d => d.Tag.GetType().Name == nameof(CrossSection)) //тут мы ищем воздуховоды на приточной линии. это нужно для того что у меня есть датчики Т SupplyTemp, ExihaustTemp, просто SensorT,
					   .Where(d => d.PozY == SupplyYLine.PozY)
					   .ToList();
					if (supplyFiltrList.Count> 0) //и вот тут волшебство. если мы находим список фильтров на приточной линии, то мы 
					{
						supplyFiltrList.AsParallel()
							.ForAll(posinfo =>
							{
								SupplyFiltr supplyFiltr = new SupplyFiltr //создаем экземпляр именно приточного фильтра
							{
									PressureProtect = ((Filtr)posinfo.Tag).PressureProtect,
									_PressureContol =
									{
										Location = "Supply"
									} //и перетягиваем на него свойства от класса Filtr, т.е. тот что был изначально
							};
								posinfo.Tag = supplyFiltr; //и в tag класса posinfo кладем уже новый приточный фильтр, а не тот который был (просто фильтр)

						});
					}
					if (CrossectionSupply.Count > 0)
					{
						CrossectionSupply.AsParallel()
							.Where(d => ((CrossSection)d.Tag)._SensorT != null)
							.ForAll(posinfo =>
							{
								((CrossSection)posinfo.Tag)._SensorT = new SupplyTemp //тут аналогично с воздуховодом. мы класс sensorT заменяем на SupplyTemp
							{
									_SensorType = ((CrossSection)posinfo.Tag).sensorTType,
									Location = "Supply"
								};
							});
						CrossectionSupply.AsParallel()
							.Where(d => ((CrossSection)d.Tag)._SensorH != null)
							.ForAll(posinfo =>
							{
								((CrossSection)posinfo.Tag)._SensorH.Location = "Supply"; //ну и напихаваем в него нужный текст который будет отобрадат
							((CrossSection)posinfo.Tag)._SensorH.Description = "Датчик влажности в приточном канале";
								((CrossSection)posinfo.Tag)._SensorH.Cable1.Description = "Датчик влажности в приточном канале";
							});
                        



                    }
				}
				if (ExhaustYLine != null)
				{
					var exihaustFiltrList = ventSystem.ComponentsV2
						.Where(d => d.Tag.GetType().Name == nameof(Filtr))
						.Where(d => d.PozY == ExhaustYLine.PozY)
						.ToList();
					List<EditorV2.PosInfo> CrossectionExhaust = ventSystem.ComponentsV2
						.Where(d => d.Tag.GetType().Name == nameof(CrossSection))
						.Where(d => d.PozY == ExhaustYLine.PozY)
						.ToList();
					//List<EditorV2.PosInfo> CrossectionIntermediate = ventSystem.ComponentsV2 //not use
					//    .Where(d => d.Tag is CrossSection)
					//    .Where(d => d.PozY != ExhaustYLine.PozY)
					//    .Where(d => d.PozY != SupplyYLine.PozY)
					//    .ToList();


					if (exihaustFiltrList.Count > 0)
					{
						exihaustFiltrList.AsParallel()
							.ForAll(posinfo =>
							{
								posinfo.Tag = new ExtFiltr
								{
									PressureProtect = ((Filtr)posinfo.Tag).PressureProtect
								};


							});
					}
					if (CrossectionExhaust.Count > 0)
					{
						CrossectionExhaust.AsParallel()
							.Where(d => ((CrossSection)d.Tag)._SensorT != null)
							.ForAll(posinfo =>
							{
								CrossectionExhaust.AsParallel()
								.Where(d => ((CrossSection)d.Tag)._SensorT != null)
								.ForAll(pos =>
								{
									((CrossSection)pos.Tag)._SensorT = new ExhaustTemp
									{
										_SensorType = ((CrossSection)pos.Tag).sensorTType,
										Location = "Exhaust"
									};
								});
							});
						CrossectionExhaust.AsParallel()
							.Where(d => ((CrossSection)d.Tag)._SensorH != null)
							.ForAll(posinfo =>
							{
								((CrossSection)posinfo.Tag)._SensorH.Location = "Exhaust";
								((CrossSection)posinfo.Tag)._SensorH.Description = "Датчик влажности в вытяжном канале";
								((CrossSection)posinfo.Tag)._SensorH.Cable1.Description = "Датчик влажности в вытяжном канале";
							});
					}
				}
				if (Rooms.Count > 0)
				{
					Rooms.AsParallel()
						.Where(d => ((Room)d.Tag)._SensorT != null)
						.ForAll(posinfo =>
						{
							((Room)posinfo.Tag)._SensorT = new IndoorTemp
							{
								_SensorType = ((Room)posinfo.Tag)._SensorT._SensorType,
								Location = "Indoor"
							};
						});
					Rooms.AsParallel()
						.Where(d => ((Room)d.Tag)._SensorH != null)
						.ForAll(posinfo =>
						{
							((Room)posinfo.Tag)._SensorH.Location = "Indoor";

						});

				}
				if (CrossSectionNulled.Count > 0)
				{
					CrossSectionNulled.AsParallel()
						.Where(d => ((CrossSection)d.Tag)._SensorH != null)
						.ForAll(posinfo =>
						{
							((CrossSection)posinfo.Tag)._SensorH.Description = "Датчик влажности в канале";
							((CrossSection)posinfo.Tag)._SensorH.Cable1.Description = "Датчик влажности в канале";
							((CrossSection)posinfo.Tag)._SensorH.Location = string.Empty;
						});
					CrossSectionNulled.AsParallel()
						.Where(d => ((CrossSection)d.Tag)._SensorT != null)
						.ForAll(posinfo =>
						{

							switch (((CrossSection)posinfo.Tag)._SensorT.GetType().Name)
							{
								case nameof(SupplyTemp):
								case nameof(ExhaustTemp):
									((CrossSection)posinfo.Tag)._SensorT = new SensorT
									{
										_SensorType = ((CrossSection)posinfo.Tag)._SensorT._SensorType,
										Description = "Датчик температуры в канале",
										Cable1 =
										{
											Description = "Датчик температуры в канале"
										}
									};
									break;
								default:
									((CrossSection)posinfo.Tag)._SensorT
									.Description = "Датчик температуры в канале";
									((CrossSection)posinfo.Tag)._SensorT.Cable1.Description = "Датчик температуры в канале";
									break;
							}
						});
				}

				List<EditorV2.PosInfo> filtrListNoPos = ventSystem.ComponentsV2
						.Where(d => d.Tag is Filtr)
						.Where(d => d.PozY != SupplyYLine?.PozY)
						.Where(d => d.PozY != ExhaustYLine?.PozY)
						.ToList();
				if (filtrListNoPos.Count > 0)
				{

					filtrListNoPos.AsParallel()
						.Where(d => ((Filtr)d.Tag)._PressureContol != null)
						.ForAll(posinfo =>
						{
							((Filtr)posinfo.Tag)._PressureContol.Description = "Датчик перепада давления на фильтре";
							((Filtr)posinfo.Tag)._PressureContol.Cable1.Description = "Датчик перепада давления на фильтре";
						});
				}
				//***
				#endregion
				#region Write Ventsystem components to DataBase
				#region Local functions
				string WriteSensor(dynamic sensor, string ventsystemColumn, string senscat, string senstype, string location)
				{
					string ID = string.Empty;
					if (sensor == null) return ID;
					Cable Cab1 = sensor.Cable1;
					if (Cab1 != null)
					{
						string sensblockname = sensor.Blockname;
						string sensposname = sensor.PosName;
						string hostguid = ventSystem.GUID;
						string sortpriority = Cab1.SortPriority;
						string description = Cab1.Description;
						bool writeblock = Cab1.WriteBlock;
						string cabattribute = Cab1.Attrubute.ToString();
						ID = WriteSensToDB(ref command, senscat, sensblockname, sensposname, senstype, hostguid, sortpriority, description, writeblock, cabattribute, Cab1, location);
						sensor.GUID = ID;
					}
					UpdateVensystem(ventsystemColumn);
					return ID;
				}

				#endregion
				#region Algorythm core
				foreach (EditorV2.PosInfo item in ventSystem.ComponentsV2) // перебираем все элементы в вентсистеме
				{
					string sensguid2;
					switch (item.Tag.GetType().Name) //выбор кода для этого элемента
					{
						#region Vents
						#region SupplyVent
						case nameof(SupplyVent): //если это приточный вентилятор
							try
							{
								var ventguid = Guid.NewGuid().ToString();
								SupplyVent supplyVent = (SupplyVent)item.Tag; //прямое преобразование item к причтоному вентилятору чтобы были доступны свойства его
								
								Vent.FControl fControl = supplyVent._FControl;
								string voltage = supplyVent.Voltage.ToString();
								string controltype = supplyVent.ControlType.ToString();
								string power = supplyVent.Power;
								string protectype = supplyVent.Protect.ToString();
								string blockname = supplyVent.BlockName;
								const string posname = "M";
								string FCGuid = null;
								
								//write Posinfo
                                string posinfoguid = WritePosInfoToDB(ref command, item, ventguid, ventSystem.GUID);
								
                                //write supplyvent cables
                                Cable[] supplyVentCables =
                                {
                                    supplyVent.Cable1,
                                    supplyVent.Cable2,
                                    supplyVent.Cable3,
                                    supplyVent.Cable4
                                };
                                foreach (Cable supplyVentCable in supplyVentCables)
                                {
                                    if (supplyVentCable == null) continue;
                                    supplyVentCable.cableGUID = Guid.NewGuid().ToString();
                                    WriteCableToDB(ref command, supplyVentCable.cableGUID, posname, ventguid, supplyVentCable.SortPriority, "Ventilator",
                                        supplyVentCable.Description, supplyVentCable.WriteBlock, supplyVentCable.Attrubute.ToString(), blockname, supplyVentCable.WireNumbers);

                                }

								//write supplyvent pressure control cable
                                if (supplyVent._PressureContol != null)
                                {
                                    PressureContol pressureContol = supplyVent._PressureContol; // выем датчика давления из вентилятора
                                    Cable Cab5 = pressureContol.Cable1;
                                    if (Cab5 != null)
                                    {
                                        string senstype = pressureContol.SensorType.ToString();
                                        const string senscat = "Pressure";
                                        string sensblockname = Cab5.ToBlockName;
                                        string sensposname = Cab5.ToPosName;
                                        string sortpriority = Cab5.SortPriority;
                                        string description = Cab5.Description;
                                        bool writeblock = Cab5.WriteBlock;
                                        string cabattribute = Cab5.Attrubute.ToString();
                                        sensguid1 = WriteSensToDB(ref command, senscat, sensblockname, sensposname, senstype, ventguid, sortpriority, description, writeblock, cabattribute, Cab5, pressureContol.Location);
                                        Cab5.cableGUID = sensguid1;
                                    }
								}

                                //generation GUID for FC Control
                                if (fControl != null)
								{

									FCGuid = Guid.NewGuid().ToString();
									fControl.GUID = FCGuid;
								}

								//Write Supply Ventilator
								string InsertQueryVent = "INSERT INTO Ventilator ([GUID], [SystemGUID], SystemName, Project, Voltage, " +
														 "ControlType, Power, ProtectType, [SensPDS], [Cable1], BlockName, PosName, " +
														 "Description, FControl, Location, Position, PosInfoGUID) VALUES " +
														 $"('{ventguid}','{ventSystem.GUID}','{ventSystem.SystemName}','{ProjectGuid}'," +
														 $"'{voltage}','{controltype}','{power}','{protectype}','{sensguid1}','{ supplyVent.Cable1?.cableGUID}'," +
														 $"'{blockname}', '{posname}', '{supplyVent.Description}', '{FCGuid}', 'Supply', " +
														 $"'{EditorV2.PosInfo.PosToString(item.Pos)}', '{posinfoguid}')";
								command.CommandText = InsertQueryVent;
								command.ExecuteNonQuery();

								UpdateVensystem("SupplyVent");
							}
							catch (Exception ex)
							{
								MessageBox.Show(ex.StackTrace);
							}
							break;
						#endregion
						#region ExtVent
						case nameof(ExtVent):
							try
							{
								var ventguid = Guid.NewGuid().ToString();
								ExtVent extVent = (ExtVent)item.Tag;
								extVent.GUID = ventguid;
								Vent.FControl fControl = extVent._FControl;
								PressureContol pressureContol = extVent._PressureContol;
								string voltage1 = extVent.Voltage.ToString();
								string controltype1 = extVent.ControlType.ToString();
								string power1 = extVent.Power;
								string protectype1 = extVent.Protect.ToString();
								string blockname1 = extVent.BlockName;
								string FCGuid = null;
								const string posname = "M";
								sensguid1 = string.Empty;
								
                                //write Posinfo
								string posinfoguid = WritePosInfoToDB(ref command, item, ventguid, ventSystem.GUID);

								//Write Exhaust Vent Cabs
                                Cable[] extVentCables =
                                {
                                    extVent.Cable1,
                                    extVent.Cable2,
                                    extVent.Cable3,
                                    extVent.Cable4
                                };
                                foreach (Cable extVentCable in extVentCables)
                                {
									if (extVentCable == null) continue;
									extVentCable.cableGUID = Guid.NewGuid().ToString();
                                    WriteCableToDB(ref command, extVentCable.cableGUID, posname, ventguid, extVentCable.SortPriority, "Ventilator",
                                        extVentCable.Description, extVentCable.WriteBlock, extVentCable.Attrubute.ToString(), blockname1, extVentCable.WireNumbers);
								}

                                if (extVent._PressureContol != null)
                                {
                                    PressureContol ExtVentPressureContol = extVent._PressureContol;
                                    Cable Cab5 = ExtVentPressureContol.Cable1;
                                    if (Cab5 != null)
                                    {
                                        string senstype = pressureContol.SensorType.ToString();
                                        const string senscat = "Pressure";
                                        string sensblockname = Cab5.ToBlockName;
                                        string sensposname = Cab5.ToPosName;
                                        string sortpriority = Cab5.SortPriority;
                                        string description = Cab5.Description;
                                        bool writeblock = Cab5.WriteBlock;
                                        string cabattribute = Cab5.Attrubute.ToString();
                                        sensguid1 = WriteSensToDB(ref command, senscat, sensblockname, sensposname, senstype, ventguid, sortpriority, description, writeblock, cabattribute, Cab5, pressureContol.Location);
                                        Cab5.cableGUID = sensguid1;
                                    }

								}
								if (fControl != null)
								{
									FCGuid = Guid.NewGuid().ToString();
									fControl.GUID = FCGuid;
								}
								string InsertQueryVent1 = "INSERT INTO Ventilator ([GUID], [SystemGUID], SystemName, [Project], " +
                                                          "Voltage, ControlType, Power, ProtectType, [SensPDS], [Cable1], " +
                                                          "BlockName, PosName, Description, FControl, Location, Position, " +
                                                          "PosInfoGUID) " +
                                                          $"VALUES ('{ventguid}','{ventSystem.GUID}','{ventSystem.SystemName}'," +
                                                          $"'{ProjectGuid}','{voltage1}','{controltype1}','{power1}','{protectype1}'," +
                                                          $"'{sensguid1}','{extVent.Cable1?.cableGUID}','{blockname1}', '{posname}', " +
                                                          $"'{extVent.Description}', '{FCGuid}', 'Exhaust', '{EditorV2.PosInfo.PosToString(item.Pos)}', " +
                                                          $"'{posinfoguid}')";
								command.CommandText = InsertQueryVent1;
								command.ExecuteNonQuery();
                                UpdateVensystem("ExtVent");
							}
							catch
							{
								// ignored
							}

							break;
						case nameof(SpareSuplyVent): //если это приточный вентилятор
                            try
							{
								var Spareventguid = Guid.NewGuid().ToString();
                                SpareSuplyVent spareSupplyVent = (SpareSuplyVent)item.Tag; //прямое преобразование item к причтоному вентилятору чтобы были доступны свойства его
								PressureContol pressureContol = spareSupplyVent._PressureContol; // выем датчика давления из вентилятора
                                Vent mainSupplyVent = spareSupplyVent.MainSupplyVent;
								Vent reserevedSupplyVent = spareSupplyVent.ReservedSupplyVent;
                                item.GUID = WritePosInfoToDB(ref command, item, Spareventguid, ventSystem.GUID);
                                spareSupplyVent.GUID = Spareventguid;
								mainSupplyVent.GUID = Guid.NewGuid().ToString();
								reserevedSupplyVent.GUID = Guid.NewGuid().ToString();
                                
                                Cable Cab5 = pressureContol?.Cable1;
								if (Cab5 != null)
								{
									string senstype = pressureContol.SensorType.ToString();
									const string senscat = "Pressure";
									string sensblockname = Cab5.ToBlockName;
									string sensposname = Cab5.ToPosName;
									string sortpriority = Cab5.SortPriority;
									string description = Cab5.Description;
									bool writeblock = Cab5.WriteBlock;
									string cabattribute = Cab5.Attrubute.ToString();
									sensguid1 = WriteSensToDB(ref command, senscat, sensblockname, sensposname, senstype, Spareventguid, sortpriority, description, writeblock, cabattribute, Cab5, pressureContol.Location);
									Cab5.cableGUID = sensguid1;
                                    pressureContol.GUID = sensguid1;
                                }

                                WriteVent(mainSupplyVent, item, Spareventguid);
								WriteVent(reserevedSupplyVent, item, Spareventguid);
                                WriteSpare<SpareSuplyVent>(spareSupplyVent, item);
                                if (mainSupplyVent._FControl != null)
                                {
                                    mainSupplyVent._FControl.Description = "Регулятор оборотов основного приточного вентилятора";
                                    WriteFControl(mainSupplyVent);
                                }

                                if (reserevedSupplyVent._FControl != null)
                                {
                                    reserevedSupplyVent._FControl.Description = "Регулятор оборотов резервного приточного вентилятора";
									WriteFControl(reserevedSupplyVent);
								}

								UpdateVensystem("SpareSupplyVent");
							}
							catch (Exception ex)
							{
								MessageBox.Show(ex.StackTrace);
							}
							break;
						case nameof(SpareExtVent): //если это вытяжной вентилятор
							try
							{
								var Spareventguid = Guid.NewGuid().ToString();
								SpareExtVent spareExtVent = (SpareExtVent)item.Tag; //прямое преобразование item к причтоному вентилятору чтобы были доступны свойства его
								PressureContol pressureContol = spareExtVent._PressureContol; // выем датчика давления из вентилятора
								Vent mainExtVent = spareExtVent.MainExtVent;
								Vent reserevedExtVent = spareExtVent.ReservedExtVent;
								item.GUID = WritePosInfoToDB(ref command, item, Spareventguid, ventSystem.GUID);
								spareExtVent.GUID = Spareventguid;
								mainExtVent.GUID = Guid.NewGuid().ToString();
								reserevedExtVent.GUID = Guid.NewGuid().ToString();
								Cable Cab5 = pressureContol?.Cable1;
								if (Cab5 != null)
								{
									string senstype = pressureContol.SensorType.ToString();
									const string senscat = "Pressure";
									string sensblockname = Cab5.ToBlockName;
									string sensposname = Cab5.ToPosName;
									string sortpriority = Cab5.SortPriority;
									string description = Cab5.Description;
									bool writeblock = Cab5.WriteBlock;
									string cabattribute = Cab5.Attrubute.ToString();
									sensguid1 = WriteSensToDB(ref command, senscat, sensblockname, sensposname, senstype, Spareventguid, sortpriority, description, writeblock, cabattribute, Cab5, pressureContol.Location);
									Cab5.cableGUID = sensguid1;
									pressureContol.GUID = sensguid1;
								}
								WriteVent(mainExtVent, item, Spareventguid);
								WriteVent(reserevedExtVent, item, Spareventguid);
								WriteSpare<SpareExtVent>(spareExtVent, item);
								if (mainExtVent._FControl != null)
								{
									mainExtVent._FControl.Description = "Регулятор оборотов основного вытяжного вентилятора";
									WriteFControl(mainExtVent);
								}

								if (reserevedExtVent._FControl != null)
								{
									reserevedExtVent._FControl.Description = "Регулятор оборотов резервного вытяжного вентилятора";
									WriteFControl(reserevedExtVent);
								}

								UpdateVensystem("SpareExtVent");
							}
							catch (Exception ex)
							{
								MessageBox.Show(ex.StackTrace);
							}
							break;
						#endregion
						#endregion
						#region Filters
						#region SupplyFilter
						case nameof(SupplyFiltr):
							try
							{

								var supplyfilterguid = Guid.NewGuid().ToString();
								SupplyFiltr supplyFiltr = (SupplyFiltr)item.Tag;
								PressureContol pressureContol = supplyFiltr._PressureContol;
								sensguid1 = string.Empty;
								string controltype = string.Empty;
								string blockname = string.Empty;
								Cable Cab1 = pressureContol?.Cable1;
								if (Cab1 != null)
								{
									string senstype = pressureContol.SensorType.ToString();
									const string senscat = "Pressure";
									string sensblockname = pressureContol.Blockname;
									string sensposname = pressureContol.PosName;
									controltype = supplyFiltr.PressureProtect.ToString();
									blockname = supplyFiltr.Blockname;
									string sortpriority = Cab1.SortPriority;
									string description = Cab1.Description;
									bool writeblock = Cab1.WriteBlock;
									string cabattribute = Cab1.Attrubute.ToString();
									sensguid1 = WriteSensToDB(ref command, senscat, sensblockname, sensposname, senstype, supplyfilterguid, sortpriority, description, writeblock, cabattribute, Cab1, pressureContol.Location);
									Cab1.cableGUID = sensguid1;
								}
								string posinfoguid = WritePosInfoToDB(ref command, item, supplyfilterguid, ventSystem.GUID);
								string InsertQueryfiltr = $"INSERT INTO Filter ([GUID], [SystemGUID], SystemName, [Project], ControlType, [SensPDS], BlockName, Description, Location, Image, Position, PosInfoGUID) VALUES ('{supplyfilterguid}','{ventSystem.GUID}','{ventSystem.SystemName}','{ProjectGuid}','{controltype}','{sensguid1}','{blockname}', '{supplyFiltr.Description}', 'Supply', '{item.ImageName}', '{EditorV2.PosInfo.PosToString(item.Pos)}', '{posinfoguid}')";
								command.CommandText = InsertQueryfiltr;
								command.ExecuteNonQuery();
								UpdateVensystem("Filter");
							}
							catch
							{
								// ignored
							}

							break;
						#endregion
						#region ExtFilter
						case nameof(ExtFiltr):
							try
							{
								var extfilterguid = Guid.NewGuid().ToString();
								ExtFiltr extFiltr = (ExtFiltr)item.Tag;
								PressureContol pressureContol = extFiltr._PressureContol;
								sensguid1 = string.Empty;
								string controltype = string.Empty;
								string blockname = string.Empty;
								Cable Cab1 = pressureContol?.Cable1;
								if (Cab1 != null)
								{
									string senstype = pressureContol.SensorType.ToString();
									const string senscat = "Pressure";
									string sensblockname = pressureContol.Blockname;
									string sensposname = pressureContol.PosName;
									controltype = extFiltr.PressureProtect.ToString();
									blockname = extFiltr.Blockname;
									string sortpriority = Cab1.SortPriority;
									string description = Cab1.Description;
									bool writeblock = Cab1.WriteBlock;
									string cabattribute = Cab1.Attrubute.ToString();
									sensguid1 = WriteSensToDB(ref command, senscat, sensblockname, sensposname, senstype, extfilterguid, sortpriority, description, writeblock, cabattribute, Cab1, pressureContol.Location);
								}
								string posinfoguid = WritePosInfoToDB(ref command, item, extfilterguid, ventSystem.GUID);
								string InsertQueryfiltr = $"INSERT INTO Filter ([GUID], [SystemGUID], SystemName, [Project], ControlType, [SensPDS], BlockName, Description, Location, Image, Position, PosInfoGUID) VALUES ('{extfilterguid}','{ventSystem.GUID}','{ventSystem.SystemName}','{ProjectGuid}','{controltype}','{sensguid1}','{blockname}', '{extFiltr.Description}', 'Exhaust', '{item.ImageName}', '{EditorV2.PosInfo.PosToString(item.Pos)}', '{posinfoguid}')";
								command.CommandText = InsertQueryfiltr;
								command.ExecuteNonQuery();
								UpdateVensystem("Filter");
							}
							catch
							{
								// ignored
							}

							break;
						#endregion
						#region Filter
						case nameof(Filtr):
							try
							{
								var filterguid = Guid.NewGuid().ToString();
								Filtr Filtr = (Filtr)item.Tag;
								PressureContol pressureContol = Filtr._PressureContol;
								sensguid1 = string.Empty;
								string controltype = string.Empty;
								string blockname = string.Empty;
								Cable Cab1 = pressureContol?.Cable1;
								if (Cab1 != null)
								{
									string senstype = pressureContol.SensorType.ToString();
									const string senscat = "Pressure";
									string sensblockname = pressureContol.Blockname;
									string sensposname = pressureContol.PosName;
									controltype = Filtr.PressureProtect.ToString();
									blockname = Filtr.Blockname;
									string sortpriority = Cab1.SortPriority;
									string description = Cab1.Description;
									bool writeblock = Cab1.WriteBlock;
									string cabattribute = Cab1.Attrubute.ToString();
									sensguid1 = WriteSensToDB(ref command, senscat, sensblockname, sensposname, senstype, filterguid, sortpriority, description, writeblock, cabattribute, Cab1, pressureContol.Location);
									Cab1.cableGUID = sensguid1;
								}
								string posinfoguid = WritePosInfoToDB(ref command, item, filterguid, ventSystem.GUID);
								string InsertQueryfiltr = $"INSERT INTO Filter ([GUID], [SystemGUID], SystemName, [Project], ControlType, [SensPDS], BlockName, Description, Location, Image, Position, PosInfoGUID) VALUES ('{filterguid}','{ventSystem.GUID}','{ventSystem.SystemName}','{ProjectGuid}','{controltype}','{sensguid1}','{blockname}', '{Filtr.Description}', '{string.Empty}', '{item.ImageName}', '{EditorV2.PosInfo.PosToString(item.Pos)}', '{posinfoguid}')";
								command.CommandText = InsertQueryfiltr;
								command.ExecuteNonQuery();
								UpdateVensystem("Filter");
							}
							catch
							{
								// ignored
							}

							break;
						#endregion
						#endregion
						#region Dampers
						#region SupplyDamper
						case nameof(SupplyDamper):
							try
							{
								var supplyDamperguid = Guid.NewGuid().ToString();
								SupplyDamper supplyDamper = (SupplyDamper)item.Tag;
								string blockname = supplyDamper.BlockName;
								string voltage = supplyDamper.Voltage.ToString();
								string spring = supplyDamper.Spring.ToString();
								string hascntrol = supplyDamper.HasContol.ToString();
								string posname = supplyDamper.PosName;
								var cable1guid = Guid.NewGuid().ToString();
								var cable2guid = string.Empty;
								Cable Cab1 = supplyDamper.Cable1;
								Cab1.cableGUID = cable1guid;
								string posinfoguid = WritePosInfoToDB(ref command, item, supplyDamperguid, ventSystem.GUID);
								if (Cab1 != null) WriteCableToDB(ref command, cable1guid, posname, supplyDamperguid, Cab1.SortPriority, "Damper", Cab1.Description, Cab1.WriteBlock, Cab1.Attrubute.ToString(), blockname, Cab1.WireNumbers);
								if (supplyDamper.HasContol)
								{
									Cable Cab2 = supplyDamper.Cable2;
									cable2guid = Guid.NewGuid().ToString();
									if (Cab2 != null)
									{
										WriteCableToDB(ref command, cable2guid, posname, supplyDamperguid, Cab2.SortPriority, "Damper", Cab2.Description, Cab2.WriteBlock, Cab2.Attrubute.ToString(), blockname, Cab2.WireNumbers);
										Cab2.cableGUID = cable2guid;
									}
								}

								OutdoorTemp OutdoorTempSensor = supplyDamper.outdoorTemp;
								sensguid1 = string.Empty;
								if (OutdoorTempSensor != null)
								{

									Cable Cab2 = OutdoorTempSensor.Cable1;
									if (Cab2 != null)
									{
										string senstype = OutdoorTempSensor._SensorType.ToString();
										const string senscat = "Temperature";
										string sensblockname = OutdoorTempSensor.Blockname;
										string sensposname = OutdoorTempSensor.PosName;
										string hostguid = ventSystem.GUID;
										string sortpriority = Cab2.SortPriority;
										string description = Cab2.Description;
										bool writeblock = Cab2.WriteBlock;
										string cabattribute = Cab2.Attrubute.ToString();
										sensguid1 = WriteSensToDB(ref command, senscat, sensblockname, sensposname, senstype, hostguid, sortpriority, description, writeblock, cabattribute, Cab2, OutdoorTempSensor.Location);
										OutdoorTempSensor.GUID = sensguid1;
									}
									UpdateVensystem("SensTOutdoor");
								}
								string InsertSupplyDamper = "INSERT INTO Damper ([GUID], [SystemGUID], SystemName, [Project], HasControl, Spring, Voltage, [Cable1], [Cable2],  BlockName, Description, Location, Image, Position, SensT, PosInfoGUID) " +
								$"VALUES ('{supplyDamperguid}','{ventSystem.GUID}','{ventSystem.SystemName}','{ProjectGuid}', '{hascntrol}', '{spring}','{voltage}','{cable1guid}', '{cable2guid}', '{blockname}', '{supplyDamper.Description1}', 'Supply', '{item.ImageName}', '{EditorV2.PosInfo.PosToString(item.Pos)}', '{sensguid1}', '{posinfoguid}')";
								command.CommandText = InsertSupplyDamper;
								command.ExecuteNonQuery();
								UpdateVensystem("SupplyDamper");
							}

							catch {}

							break;
						#endregion
						#region ExtDamper
						case nameof(ExtDamper):
							try
							{
								var extDamperguid = Guid.NewGuid().ToString();
								ExtDamper extDamper = (ExtDamper)item.Tag;
								string blockname = extDamper.BlockName;
								string voltage = extDamper.Voltage.ToString();
								string spring = extDamper.Spring.ToString();
								string hascntrol = extDamper.HasContol.ToString();
								string posname = extDamper.PosName;
								var cable1guid = Guid.NewGuid().ToString();
								var cable2guid = string.Empty;
								Cable Cab1 = extDamper.Cable1;
								string posinfoguid = WritePosInfoToDB(ref command, item, extDamperguid, ventSystem.GUID);
								if (Cab1 != null) WriteCableToDB(ref command, cable1guid, posname, extDamperguid, Cab1.SortPriority, "Damper", Cab1.Description, Cab1.WriteBlock, Cab1.Attrubute.ToString(), blockname, Cab1.WireNumbers);
								//WriteCableToDB(ref command, cable1guid5, posname5, extDamperguid, extDamper.SortPriority, "Damper", extDamper.Description1);
								if (extDamper.HasContol)
								{
									Cable Cab2 = extDamper.Cable2;
									cable2guid = Guid.NewGuid().ToString();
									if (Cab2 != null)
									{
										WriteCableToDB(ref command, cable2guid, posname, extDamperguid, Cab2.SortPriority, "Damper", Cab2.Description, Cab2.WriteBlock, Cab2.Attrubute.ToString(), blockname, Cab2.WireNumbers);
										Cab2.cableGUID = cable2guid;
									}
									//WriteCableToDB(ref command, cable2guid5, posname5, extDamperguid, extDamper.SortPriority, "Damper", extDamper.Description2);
								}
								string InsertExtDamper = "INSERT INTO Damper ([GUID], [SystemGUID], SystemName, [Project], HasControl, Spring, Voltage, [Cable1], [Cable2],  BlockName, Description, Location, Image, Position, PosInfoGUID ) " +
									$"VALUES ('{extDamperguid}','{ventSystem.GUID}','{ventSystem.SystemName}','{ProjectGuid}', '{hascntrol}', '{spring}','{voltage}','{cable1guid}', '{cable2guid}', '{blockname}', '{extDamper.Description1}', 'Exhaust', '{item.ImageName}', '{EditorV2.PosInfo.PosToString(item.Pos)}', '{posinfoguid}')";
								command.CommandText = InsertExtDamper;
								command.ExecuteNonQuery();
								UpdateVensystem("ExtDamper");
							}
							catch { }
							break;
						#endregion
						#endregion
						#region Heaters
						// Waterheater
						case nameof(WaterHeater):
							try
							{
								var waterHeaterguid = Guid.NewGuid().ToString();
								WaterHeater waterHeater = (WaterHeater)item.Tag;
								WaterHeater.Valve valve = waterHeater._Valve;
								WaterHeater.Pump pump = waterHeater._Pump;
								Sensor protectSensor1 = waterHeater.PS1;
								Sensor protectSensor2 = waterHeater.PS2;
								sensguid1 = string.Empty;
								sensguid2 = string.Empty;
								string valveguid = string.Empty;
								string pumpguid = string.Empty;
								string protecttype = waterHeater.Waterprotect.ToString();
								if (protectSensor1 != null)
								{
									Cable Cab1 = protectSensor1.Cable1;
									if (Cab1 != null)
									{
										string senstype = waterHeater._PS1SensType.ToString();
										const string senscat = "Temperature";
										string sensblockname = waterHeater.PS1Blockname;
										string sensposname = waterHeater.PS1PosName;
										string sortpriority = Cab1.SortPriority;
										string description = Cab1.Description;
										bool writeblock = Cab1.WriteBlock;
										string cabattribute = Cab1.Attrubute.ToString();
										sensguid1 = WriteSensToDB(ref command, senscat, sensblockname, sensposname, senstype, waterHeaterguid, sortpriority, description, writeblock, cabattribute, Cab1, protectSensor1.Location);
										Cab1.cableGUID = sensguid1;
									}
								}
								if (protectSensor2 != null)
								{
									Cable Cab2 = protectSensor2.Cable1;
									if (Cab2 != null)
									{
										string senstype = waterHeater._PS2SensType.ToString();
										const string senscat = "Temperature";
										string sensblockname = waterHeater.PS2Blockname;
										string sensposname = waterHeater.PS2PosName;
										string sortpriority = Cab2.SortPriority;
										string description = Cab2.Description;
										bool writeblock = Cab2.WriteBlock;
										string cabattribute = Cab2.Attrubute.ToString();
										sensguid2 = WriteSensToDB(ref command, senscat, sensblockname, sensposname, senstype, waterHeaterguid, sortpriority, description, writeblock, cabattribute, Cab2, protectSensor2.Location);
										Cab2.cableGUID = sensguid2;
									}
								}
								if (valve != null)
								{
									Cable Cab1 = valve.Cable1;
									Cable Cab2 = valve.Cable2;
									Cable Cab3 = valve.Cable3;
									Cable Cab4 = valve.Cable4;
									Cable Cab5 = valve.Cable5;
									string[] array = WriteValveToDB(ref command, waterHeater.ValveBlockName, waterHeater.ValvePosName, waterHeater._valveType.ToString(), waterHeaterguid, valve.Description, Cab1, Cab2, Cab3, Cab4, Cab5);
									valve.GUID = valveguid = array[0];
									if (Cab1 != null) Cab1.cableGUID = array[1];
									if (Cab2 != null) Cab2.cableGUID = array[2];
									if (Cab3 != null) Cab3.cableGUID = array[3];
									if (Cab4 != null) Cab4.cableGUID = array[4];
									if (Cab5 != null) Cab5.cableGUID = array[5];
								}
								if (pump != null)
								{
									pumpguid = Guid.NewGuid().ToString();
									string blockname = waterHeater.PumpBlockName;
									string voltage = pump.Voltage.ToString();
									string hasTK = waterHeater.HasTK.ToString();
									string posname = waterHeater.PumpPosName;
									var cable1guid = Guid.NewGuid().ToString();
									Cable Cab1 = pump.Cable1;

									var cable2guid = string.Empty;
									if (Cab1 != null)
									{
										if (writpumpcable) Cab1.Attrubute = Cable.CableAttribute.PL;

										WriteCableToDB(ref command, cable1guid, posname, pumpguid, Cab1.SortPriority, "Pump", Cab1.Description, Cab1.WriteBlock, Cab1.Attrubute.ToString(), blockname, Cab1.WireNumbers);
										Cab1.cableGUID = cable1guid;
									}
									if (pump.HasTK)
									{
										Cable Cab2 = pump.Cable2;
										if (Cab2 != null)
										{
											cable2guid = Guid.NewGuid().ToString();
											WriteCableToDB(ref command, cable2guid, posname, pumpguid, Cab2.SortPriority, "Pump", Cab2.Description, Cab2.WriteBlock, Cab2.Attrubute.ToString(), blockname, Cab2.WireNumbers);
											Cab2.cableGUID = cable2guid;
										}
									}
									string InsertPump = "INSERT INTO Pump ([GUID], [SystemGUID], SystemName, [Project], HasTK, Voltage, [Cable1], [Cable2],  BlockName, Description) " +
											$"VALUES ('{pumpguid}','{ventSystem.GUID}','{ventSystem.SystemName}','{ProjectGuid}', '{hasTK}', '{voltage}','{cable1guid}', '{cable2guid}', '{blockname}', '{pump.Description}')";
									command.CommandText = InsertPump;
									command.ExecuteNonQuery();
								}
								string posinfoguid = WritePosInfoToDB(ref command, item, waterHeaterguid, ventSystem.GUID);
								string InsertWaterHeaterItemsQuery7 = "INSERT INTO WaterHeater ([GUID], [Project], [SystemGUID], SystemName, [SensT1], [SensT2], [Pump], [Valve], ControlType, Position, PosInfoGUID) " +
									$"VALUES ('{waterHeaterguid}','{Project.GetGUID()}', '{ventSystem.GUID}', '{ventSystem.SystemName}','{sensguid1}', '{sensguid2}','{pumpguid}', '{valveguid}', '{protecttype}', '{EditorV2.PosInfo.PosToString(item.Pos)}', '{posinfoguid}')";
								command.CommandText = InsertWaterHeaterItemsQuery7;
								command.ExecuteNonQuery();
								UpdateVensystem("WaterHeater");
							}
							catch (Exception ex)
							{
								MessageBox.Show(ex.StackTrace);
							}
							break;
						// Electroheater
						case nameof(ElectroHeater):
							try
							{
								var ElectroHeaterguid = Guid.NewGuid().ToString();
								ElectroHeater electroHeater = (ElectroHeater)item.Tag;
								string posname = electroHeater.PosName;
								string blockname = electroHeater.BlockName;
								string power = electroHeater.Power;
								string voltage = electroHeater.Voltage.ToString();
								string stairs = electroHeater.Stairs.ToString();
								var cableguid1 = Guid.NewGuid().ToString();
								Cable Cab1 = electroHeater.Cable1;
								Cable Cab2 = electroHeater.Cable2;
								Cable Cab3 = electroHeater.Cable3;
								Cable Cab4 = electroHeater.Cable4;
								Cable Cab5 = electroHeater.Cable5;
								Cable Cab6 = electroHeater.Cable6;
								if (Cab1 != null)
								{
									cableguid1 = Guid.NewGuid().ToString();
									WriteCableToDB(ref command, cableguid1, posname, ElectroHeaterguid, Cab1.SortPriority, "ElectroHeater", Cab1.Description, Cab1.WriteBlock, Cab1.Attrubute.ToString(), blockname, Cab1.WireNumbers);
								}
								if (Cab2 != null)
								{
									var cableguid2 = Guid.NewGuid().ToString();
									WriteCableToDB(ref command, cableguid2, posname, ElectroHeaterguid, Cab2.SortPriority, "ElectroHeater", Cab2.Description, Cab2.WriteBlock, Cab2.Attrubute.ToString(), blockname, Cab2.WireNumbers);
								}
								if (Cab3 != null)
								{
									var cableguid3 = Guid.NewGuid().ToString();
									WriteCableToDB(ref command, cableguid3, posname, ElectroHeaterguid, Cab3.SortPriority, "ElectroHeater", Cab3.Description, Cab3.WriteBlock, Cab3.Attrubute.ToString(), blockname, Cab3.WireNumbers);
								}
								if (Cab4 != null)
								{
									var cableguid4 = Guid.NewGuid().ToString();
									WriteCableToDB(ref command, cableguid4, posname, ElectroHeaterguid, Cab4.SortPriority, "ElectroHeater", Cab4.Description, Cab4.WriteBlock, Cab4.Attrubute.ToString(), blockname, Cab4.WireNumbers);
								}
								if (Cab5 != null)
								{
									var cableguid5 = Guid.NewGuid().ToString();
									WriteCableToDB(ref command, cableguid5, posname, ElectroHeaterguid, Cab5.SortPriority, "ElectroHeater", Cab5.Description, Cab5.WriteBlock, Cab5.Attrubute.ToString(), blockname, Cab5.WireNumbers);
								}
								if (Cab6 != null)
								{
									var cableguid6 = Guid.NewGuid().ToString();
									WriteCableToDB(ref command, cableguid6, posname, ElectroHeaterguid, Cab6.SortPriority, "ElectroHeater", Cab6.Description, Cab6.WriteBlock, Cab6.Attrubute.ToString(), blockname, Cab6.WireNumbers);
								}
								string posinfoguid = WritePosInfoToDB(ref command, item, ElectroHeaterguid, ventSystem.GUID);
								string InsertElectroHeaterItemsQuery = "INSERT INTO ElectroHeater ([GUID], [Project], [SystemGUID], SystemName, Stairs, Voltage, Power, BlockName, PosName, [Cable1], Description, Position, PosInfoGUID) " +
									$"VALUES ('{ElectroHeaterguid}','{Project.GetGUID()}', '{ventSystem.GUID}', '{ventSystem.SystemName}','{stairs}', '{voltage}','{power}', '{blockname}', '{posname}', '{cableguid1}', '{electroHeater.Description1}', '{EditorV2.PosInfo.PosToString(item.Pos)}', '{posinfoguid}')";
								command.CommandText = InsertElectroHeaterItemsQuery;
								command.ExecuteNonQuery();
								UpdateVensystem("ElectroHeat");
							}
							catch { }
							break;
						#endregion
						#region Froster
						case nameof(Froster):
							try
							{
								Froster froster = (Froster)item.Tag;
								var Frosterguid = Guid.NewGuid().ToString();

								Froster.Valve frostervalve = froster._Valve;
								Froster.KKB frosterKKB = froster._KKB;
								Froster.FrosterType frosterType = froster._FrosterType;
								ElectroDevice._Voltage _Voltage = froster.Voltage;
								Froster.FrosterSensor frosterSensor = froster._FrosterSensor;

								string Power = froster.Power;
								sensguid1 = string.Empty;
								sensguid2 = string.Empty;
								string frostervalveguid = string.Empty;
								string KKBGuid = string.Empty;


								if (frostervalve != null)
								{
									Cable Cab1 = frostervalve.Cable1;
									Cable Cab2 = frostervalve.Cable2;

									string[] array = WriteValveToDB(ref command, froster.ValveBlockName, froster.ValvePosName, froster.valveType.ToString(), Frosterguid, frostervalve.Description, Cab1, Cab2, null, null, null);
									frostervalve.GUID = frostervalveguid = array[0];

									if (Cab1 != null) Cab1.cableGUID = array[1];
									if (Cab2 != null) Cab2.cableGUID = array[2];


								}
								if (frosterKKB != null)
								{


									KKBGuid = WriteKKBToDB(ref command, froster.KKBBlockName, froster.KKBPosName, frosterKKB._KKBControlType.ToString(), Frosterguid, froster.Stairs.ToString(), frosterKKB.Description, frosterKKB.Cable1, frosterKKB.Cable2, frosterKKB.Cable3, frosterKKB.Cable4, frosterKKB.Cable5, frosterKKB.Cable6);
								}
								string posinfoguid = WritePosInfoToDB(ref command, item, Frosterguid, ventSystem.GUID);
								string InsertFrosterItemsQuery = "INSERT INTO Froster ([GUID], [Project], [SystemGUID], SystemName, [SensT1], [SensT2], [KKB], [Valve], ControlType, Power, Voltage, FrosterSensor, Position, PosInfoGUID) " +
									$"VALUES ('{Frosterguid}','{Project.GetGUID()}', '{ventSystem.GUID}', '{ventSystem.SystemName}','{sensguid1}', '{sensguid2}','{KKBGuid}', '{frostervalveguid}', '{frosterType}', '{Power}', '{_Voltage}', '{frosterSensor}', '{EditorV2.PosInfo.PosToString(item.Pos)}', '{posinfoguid}')";
								command.CommandText = InsertFrosterItemsQuery;

								command.ExecuteNonQuery();
								UpdateVensystem("Froster");
							}
							catch { }
							break;
						#endregion
						#region Humidifier
						case nameof(Humidifier):

							try
							{
								Humidifier humidifier = (Humidifier)item.Tag;
								Humidifier.HumiditySens humiditySens = humidifier.HumiditySensor;
								var Humidifierguid = Guid.NewGuid().ToString();
								var cable1guid = Guid.NewGuid().ToString();
								var cable2guid = Guid.NewGuid().ToString();
								var cable3guid = Guid.NewGuid().ToString();
								string humsenspresent = humidifier.HumSensPresent.ToString();
								string voltage10 = humidifier.Voltage.ToString();
								string power10 = humidifier.Power;
								string humtype = humidifier.HumType.ToString();
								string humblockname = humidifier.BlockName;
								string humposname = humidifier.PosName;
								Cable Cab1 = humidifier.Cable1;
								Cable Cab2 = humidifier.Cable2;
								Cable Cab3 = humidifier.Cable3;
								sensguid1 = string.Empty;
								if (humiditySens != null)
								{
									Cable Cab4 = humiditySens.Cable1;
									if (Cab4 != null)
									{
										string senstype = humiditySens._SensorType.ToString();
										string senscat = "Humidity";
										string sensblockname = humiditySens.Blockname;
										string sensposname = humiditySens.PosName;
										string hostguid = Humidifierguid;
										string sortpriority = Cab4.SortPriority;
										string description = Cab4.Description;
										bool writeblock = Cab4.WriteBlock;
										string cabattribute = Cab4.Attrubute.ToString();
										sensguid1 = WriteSensToDB(ref command, senscat, sensblockname, sensposname, senstype, hostguid, sortpriority, description, writeblock, cabattribute, Cab4, humiditySens.Location);
										Cab4.cableGUID = sensguid1;
									}
								}
								string posinfoguid = WritePosInfoToDB(ref command, item, Humidifierguid, ventSystem.GUID);
								string InsertHumidifierItemsQuery = "INSERT INTO Humidifier ([GUID], [Project], [SystemGUID], SystemName, [SensHum], Power, Voltage, HumSensPresent, Type, BlockName, Cable1, PosName, Description, Cable2, Position, PosInfoGUID) " +
									$"VALUES ('{Humidifierguid}','{Project.GetGUID()}', '{ventSystem.GUID}', '{ventSystem.SystemName}','{sensguid1}', '{power10}','{voltage10}', '{humsenspresent}', '{humtype}', '{humblockname}', '{cable1guid}', '{humposname}', '{humidifier.Description}', '{cable2guid}', '{EditorV2.PosInfo.PosToString(item.Pos)}', '{posinfoguid}')";
								command.CommandText = InsertHumidifierItemsQuery;
								command.ExecuteNonQuery();
								if (Cab1 != null)
								{
									WriteCableToDB(ref command, cable1guid, humposname, Humidifierguid, Cab1.SortPriority, "Humidifier", Cab1.Description, Cab1.WriteBlock, Cab1.Attrubute.ToString(), humblockname, Cab1.WireNumbers);
									Cab1.cableGUID = cable1guid;
								}
								if (Cab2 != null)
								{
									WriteCableToDB(ref command, cable2guid, humposname, Humidifierguid, Cab2.SortPriority, "Humidifier", Cab2.Description, Cab2.WriteBlock, Cab2.Attrubute.ToString(), humblockname, Cab2.WireNumbers);
									Cab2.cableGUID = cable2guid;
								}
								if (Cab3 != null)
								{
									WriteCableToDB(ref command, cable3guid, humposname, Humidifierguid, Cab3.SortPriority, "Humidifier", Cab3.Description, Cab3.WriteBlock, Cab3.Attrubute.ToString(), humblockname, Cab3.WireNumbers);
									Cab3.cableGUID = cable3guid;
								}
								UpdateVensystem("Humidifier");
							}
							catch (Exception ex)
							{
								MessageBox.Show(ex.StackTrace);
							}
							break;
						#endregion
						#region Recuperator
						case nameof(Recuperator):
							try
							{

								Recuperator recuperator = (Recuperator)item.Tag;
								SensorT PS1 = recuperator.protectSensor1;
								PressureContol PS2 = recuperator.protectSensor2;
								Recuperator.Drive Drive1 = recuperator.Drive1;
								Recuperator.Drive Drive2 = recuperator.Drive2;
								Recuperator.Drive Drive3 = recuperator.Drive3;
								var Recuperatorguid = Guid.NewGuid().ToString();
								string RecType = recuperator._RecuperatorType.ToString();
								string PS2GUID, Drive1GUID, Drive2GUID, Drive3GUID;
								var PS1GUID = PS2GUID = Drive1GUID = Drive2GUID = Drive3GUID = string.Empty;
								if (Drive1 != null)
								{
									Cable Cab1 = Drive1.Cable1;
									Cable Cab2 = Drive1.Cable2;
									string[] array = WriteValveToDB(ref command, Drive1.BlockName, Drive1.Posname, Drive1._ValveType.ToString(), Recuperatorguid, Drive1.Description, Cab1, Cab2, null, null, null);
									Drive1.GUID = Drive1GUID = array[0];
									if (Cab1 != null) Cab1.cableGUID = array[1];
									if (Cab2 != null) Cab2.cableGUID = array[2];
								}
								if (Drive2 != null)
								{
									Cable Cab1 = Drive2.Cable1;
									Cable Cab2 = Drive2.Cable2;
									string[] array = WriteValveToDB(ref command, Drive2.BlockName, Drive2.Posname, Drive2._ValveType.ToString(), Recuperatorguid, Drive2.Description, Cab1, Cab2, null, null, null);
									Drive2.GUID = Drive2GUID = array[0];
									if (Cab1 != null) Cab1.cableGUID = array[1];
									if (Cab2 != null) Cab2.cableGUID = array[2];
								}
								if (Drive3 != null)
								{
									Cable Cab1 = Drive3.Cable1;
									Cable Cab2 = Drive3.Cable2;
									string[] array = WriteValveToDB(ref command, Drive3.BlockName, Drive3.Posname, Drive3._ValveType.ToString(), Recuperatorguid, Drive3.Description, Cab1, Cab2, null, null, null);
									Drive3.GUID = Drive2GUID = array[0];
									if (Cab1 != null) Cab1.cableGUID = array[1];
									if (Cab2 != null) Cab2.cableGUID = array[2];
								}

								if (PS1 != null)
								{
									Cable Cab1 = PS1.Cable1;
									if (Cab1 != null)
									{
										string senstype = PS1._SensorType.ToString();
										const string senscat = "Temperature";
										string sensblockname = PS1.Blockname;
										string sensposname = PS1.PosName;
										string sortpriority = Cab1.SortPriority;
										string description = Cab1.Description;
										bool writeblock = Cab1.WriteBlock;
										string cabattribute = Cab1.Attrubute.ToString();
										sensguid1 = WriteSensToDB(ref command, senscat, sensblockname, sensposname, senstype, Recuperatorguid, sortpriority, description, writeblock, cabattribute, Cab1, PS1.Location);
										PS1.GUID = sensguid1;
										PS1GUID = sensguid1;
									}
								}
								if (PS2 != null)
								{
									Cable Cab2 = PS2.Cable1;
									if (Cab2 != null)
									{
										string senstype = PS2._SensorType.ToString();
										string senscat = "Pressure";
										string sensblockname = PS2.Blockname;
										string sensposname = PS2.PosName;
										string hostguid = Recuperatorguid;
										string sortpriority = Cab2.SortPriority;
										string description = Cab2.Description;
										bool writeblock = Cab2.WriteBlock;
										string cabattribute = Cab2.Attrubute.ToString();
										sensguid2 = WriteSensToDB(ref command, senscat, sensblockname, sensposname, senstype, hostguid, sortpriority, description, writeblock, cabattribute, Cab2, PS2.Location);
										PS2.GUID = sensguid2;
										PS2GUID = sensguid2;
									}
								}
								string posinfoguid = WritePosInfoToDB(ref command, item, Recuperatorguid, ventSystem.GUID);
								string InsertRecuperatorQuery = "INSERT INTO Recuperator ([GUID], [Project], [SystemGUID], SystemName, [PS1], [PS2], [Drive1], [Drive2], [Drive3], Type, Position, PosInfoGUID) " +
									$"VALUES ('{Recuperatorguid}', '{Project.GetGUID()}', '{ventSystem.GUID}', '{ventSystem.SystemName}', '{PS1GUID}', '{PS2GUID}', '{Drive1GUID}', '{Drive2GUID}', '{Drive3GUID}', '{RecType}', '{EditorV2.PosInfo.PosToString(item.Pos)}', '{posinfoguid}')";
								command.CommandText = InsertRecuperatorQuery;
								command.ExecuteNonQuery();
								UpdateVensystem("Recuperator");
							}
							catch { }
							break;
						#endregion
						#region Sensors
						case nameof(CrossSection):
							try
							{
								sensguid1 = string.Empty;
								sensguid2 = string.Empty;
								//sensguid2 = string.Empty;
								string Senstype;
								var CrossectionGUID = Guid.NewGuid().ToString();
								CrossSection crossSection = (CrossSection)item.Tag;
								string posinfoguid = WritePosInfoToDB(ref command, item, CrossectionGUID, ventSystem.GUID);
								SensorT sensorT = crossSection._SensorT;
								if (sensorT != null)
								{
									switch (crossSection._SensorT.GetType().Name)
									{
										case nameof(SupplyTemp):
											Senstype = crossSection.sensorTType.ToString();
											var SupplyTempSensor = (SupplyTemp)crossSection._SensorT;
											sensguid1 = WriteSensor(SupplyTempSensor, "SensTSupply", "Temperature", Senstype, SupplyTempSensor.Location);
											break;
										case nameof(ExhaustTemp):
											Senstype = crossSection.sensorTType.ToString();
											var exhaustTemp = (ExhaustTemp)crossSection._SensorT;
											sensguid1 = WriteSensor(exhaustTemp, "SensTexhaust", "Temperature", Senstype, exhaustTemp.Location);
											break;
										case nameof(SensorT):
											Senstype = crossSection.sensorTType.ToString();
											sensguid1 = WriteSensor(crossSection._SensorT, "SensTNoLocation", "Temperature", Senstype, crossSection._SensorT.Location);
											break;
									}
								}

								Humidifier.HumiditySens humiditySens = crossSection._SensorH;
								Cable cableH = humiditySens?.Cable1;
								if (cableH != null)
								{
									Senstype = humiditySens._SensorType.ToString();
									sensguid2 = WriteSensor(humiditySens, "SensHSupply", "Humidity", Senstype, humiditySens.Location);
								}
								string InsertCrossConnection = "INSERT INTO CrossConnection ([GUID], [Project], [SystemGUID], Position, Image, SensT, SensH, SensP, PosInfoGUID) " +
															   $"VALUES ('{CrossectionGUID}', '{Project.GetGUID()}', '{ventSystem.GUID}', '{EditorV2.PosInfo.PosToString(item.Pos)}', '{item.ImageName}', '{sensguid1}', '{sensguid2}', '{string.Empty}', '{posinfoguid}')";
								command.CommandText = InsertCrossConnection;
								command.ExecuteNonQuery();
								UpdateVensystem("CrossConnection");



							}
							catch (Exception ex)
							{
								MessageBox.Show(ex.StackTrace);
							}
							break;
						case nameof(Room):
							{
								try
								{
									sensguid1 = string.Empty;
									sensguid2 = string.Empty;
									Room room = (Room)item.Tag;
									IndoorTemp indoorTemp = (IndoorTemp)room._SensorT;
									Humidifier.HumiditySens humiditySens = room._SensorH;
									var RoomGUID = Guid.NewGuid().ToString();
									string posinfoguid = WritePosInfoToDB(ref command, item, RoomGUID, ventSystem.GUID);
									if (indoorTemp != null)
									{
										var Senstype1 = indoorTemp._SensorType.ToString();
										sensguid1 = WriteSensor(indoorTemp, "SensTIndoor", "Temperature", Senstype1, indoorTemp.Location);
									}
									if (humiditySens != null)
									{
										var Senstype2 = humiditySens._SensorType.ToString();
										sensguid2 = WriteSensor(humiditySens, "SensHIndoor", "Humidity", Senstype2, humiditySens.Location);
									}
									string InsertRoom = "INSERT INTO Room ([GUID], [Project], [SystemGUID], Position, Image, SensT, SensH, PosInfoGUID) " +
																				 $"VALUES ('{RoomGUID}', '{Project.GetGUID()}', '{ventSystem.GUID}', '{EditorV2.PosInfo.PosToString(item.Pos)}', '{item.ImageName}', '{sensguid1}', '{sensguid2}', '{posinfoguid}')";
									command.CommandText = InsertRoom;
									command.ExecuteNonQuery();
									UpdateVensystem("Room");
								}
								catch { }
							}
							break;
						case nameof(ExhaustTemp):
							try
							{
								sensguid1 = string.Empty;
								//ExhaustTemp ExhaustTempSensor = (ExhaustTemp)item.Tag;
								ExhaustTemp ExhaustTempSensor = ventSystem._ExhaustTemp;
								Cable Cab1 = ExhaustTempSensor.Cable1;
								if (Cab1 != null)
								{
									string senstype = ExhaustTempSensor._SensorType.ToString();
									const string senscat = "Temperature";
									string sensblockname = ExhaustTempSensor.Blockname;
									string sensposname = ExhaustTempSensor.PosName;
									string hostguid = ventSystem.GUID;
									string sortpriority = Cab1.SortPriority;
									string description = Cab1.Description;
									bool writeblock = Cab1.WriteBlock;
									string cabattribute = Cab1.Attrubute.ToString();
									sensguid1 = WriteSensToDB(ref command, senscat, sensblockname, sensposname, senstype, hostguid, sortpriority, description, writeblock, cabattribute, Cab1, ExhaustTempSensor.Location);
									ExhaustTempSensor.GUID = sensguid1;

								}

								UpdateVensystem("SensTExhaust");
							}
							catch { }
							break;
						case nameof(IndoorTemp):
							try
							{
								sensguid1 = string.Empty;
								IndoorTemp IndoorTempSensor = (IndoorTemp)item.Tag;
								Cable Cab1 = IndoorTempSensor.Cable1;
								if (Cab1 != null)
								{
									string senstype = IndoorTempSensor._SensorType.ToString();
									const string senscat = "Temperature";
									string sensblockname = IndoorTempSensor.Blockname;
									string sensposname = IndoorTempSensor.PosName;
									string hostguid = ventSystem.GUID;
									string sortpriority = Cab1.SortPriority;
									string description = Cab1.Description;
									bool writeblock = Cab1.WriteBlock;
									string cabattribute = Cab1.Attrubute.ToString();
									sensguid1 = WriteSensToDB(ref command, senscat, sensblockname, sensposname, senstype, hostguid, sortpriority, description, writeblock, cabattribute, Cab1, IndoorTempSensor.Location);
									IndoorTempSensor.GUID = sensguid1;

								}


								UpdateVensystem("SensTIndoor");
							}
							catch { }
							break;
						case nameof(OutdoorTemp):
							try
							{
								sensguid1 = string.Empty;
								OutdoorTemp OutdoorTempSensor = (OutdoorTemp)item.Tag;
								Cable Cab1 = OutdoorTempSensor.Cable1;
								if (Cab1 != null)
								{
									string senstype = OutdoorTempSensor._SensorType.ToString();
									const string senscat = "Temperature";
									string sensblockname = OutdoorTempSensor.Blockname;
									string sensposname = OutdoorTempSensor.PosName;
									string hostguid = ventSystem.GUID;
									string sortpriority = Cab1.SortPriority;
									string description = Cab1.Description;
									bool writeblock = Cab1.WriteBlock;
									string cabattribute = Cab1.Attrubute.ToString();
									sensguid1 = WriteSensToDB(ref command, senscat, sensblockname, sensposname, senstype, hostguid, sortpriority, description, writeblock, cabattribute, Cab1, OutdoorTempSensor.Location);
									OutdoorTempSensor.GUID = sensguid1;

								}
								UpdateVensystem("SensTOutdoor");
							}
							catch { }
							break;
							#endregion

					}
				}
				#endregion
				#endregion
				#region Common Functions
				
				string WriteSensToDB(ref SQLiteCommand oleDbCommand, string senscat, string sensbockname, string sensposname, string senstype, string hostguid, string sortpriority, string description, bool writeblock, string attribute, Cable cab, string location)
				{
					string vendor1 = string.Empty;
					string vendorID1 = string.Empty;
					string BDsensguid = Guid.NewGuid().ToString();
					var senscableguid = Guid.NewGuid().ToString();
					string QuerySens = string.Empty;
					command.CommandText = string.Empty;
					int WireNumbers = cab.WireNumbers;

					switch (senscat)
					{
						case "Pressure":
							QuerySens = $"INSERT INTO SensPDS ([GUID], [Project], SystemName, [Cable], Vendor, VendorCode, BlockName, PosName, [SystemGUID], SensType, ElementGUID, Description) VALUES ('{BDsensguid}','{Project.GetGUID()}','{ventSystem.SystemName}','{senscableguid}','{vendor1}','{vendorID1}','{sensbockname}','{sensposname}', '{ventSystem.GUID}', '{senstype}', '{hostguid}', '{description}')";
							break;
						case "Temperature":
							QuerySens = "INSERT INTO SensT ([GUID], [Project], SystemName, [Cable1], Vendor, VendorCode, BlockName, PosName, [SystemGUID], SensType, ElementGUID, Description, Location) VALUES " +
										$"('{BDsensguid}','{Project.GetGUID()}','{ventSystem.SystemName}','{senscableguid}','{vendor1}','{vendorID1}','{sensbockname}','{sensposname}', '{ventSystem.GUID}', '{senstype}', '{hostguid}', '{description}', '{location}')";
							break;

						case "Humidity":
							QuerySens = "INSERT INTO SensHum ([GUID], [Project], SystemName, [Cable1], Vendor, VendorCode, BlockName, PosName, [SystemGUID], SensType, ElementGUID, Description, Location) VALUES " +
										$"('{BDsensguid}','{Project.GetGUID()}','{ventSystem.SystemName}','{senscableguid}','{vendor1}','{vendorID1}','{sensbockname}','{sensposname}', '{ventSystem.GUID}', '{senstype}', '{hostguid}', '{description}', '{location}')";
							break;

					}

					oleDbCommand.CommandText = QuerySens;
					oleDbCommand.ExecuteNonQuery();
                    
                    switch (senscat)
                    {
                           

                        case "Pressure":
							WriteCableToDB(ref oleDbCommand, senscableguid, sensposname, BDsensguid, sortpriority, "SensPDS", description, writeblock, attribute, sensbockname, WireNumbers);
							break;
						case "Temperature":
							WriteCableToDB(ref oleDbCommand, senscableguid, sensposname, BDsensguid, sortpriority, "SensT", description, writeblock, attribute, sensbockname, WireNumbers);
							break;
						case "Humidity":
							WriteCableToDB(ref oleDbCommand, senscableguid, sensposname, BDsensguid, sortpriority, "SensHum", description, writeblock, attribute, sensbockname, WireNumbers);
							break;

					}
					return BDsensguid;
				}
				

				string[] WriteValveToDB(ref SQLiteCommand oleDbCommand, string valveblockname, string valveposname, string valvetype, string hostguid, string description, Cable Cab1, Cable Cab2, Cable Cab3, Cable Cab4, Cable Cab5)
				{
					string Cab1Sort = string.Empty;
					bool Cab1WriteBlock = false;
					string Cab1Attr = string.Empty;
					string Cab1Descr = string.Empty;
					string Cab2Sort = string.Empty;
					bool Cab2WriteBlock = false;
					string Cab2Attr = string.Empty;
					string Cab2Descr = string.Empty;
					string Cab3Sort = string.Empty;
					bool Cab3WriteBlock = false;
					string Cab3Attr = string.Empty;
					string Cab3Descr = string.Empty;
					string Cab4Sort = string.Empty;
					bool Cab4WriteBlock = false;
					string Cab4Attr = string.Empty;
					string Cab4Descr = string.Empty;
					string Cab5Sort = string.Empty;
					bool Cab5WriteBlock = false;
					string Cab5Attr = string.Empty;
					string Cab5Descr = string.Empty;
					var valveguid = Guid.NewGuid().ToString();
					string cable1guid = string.Empty;
					string cable2guid = string.Empty;
					string cable3guid = string.Empty;
					string cable4guid = string.Empty;
					string cable5guid = string.Empty;
					if (Cab1 != null)
					{
						Cab1Sort = Cab1.SortPriority;
						Cab1WriteBlock = Cab1.WriteBlock;
						Cab1Attr = writevalvecable ? Cable.CableAttribute.PL.ToString() : Cab1.Attrubute.ToString();

						cable1guid = Guid.NewGuid().ToString();
						Cab1Descr = Cab1.Description;


					}
					if (Cab2 != null)
					{
						Cab2Sort = Cab2.SortPriority;
						Cab2WriteBlock = Cab2.WriteBlock;
						Cab2Attr = Cab2.Attrubute.ToString();
						cable2guid = Guid.NewGuid().ToString();
						Cab2Descr = Cab2.Description;
					}
					if (Cab3 != null)
					{
						Cab3Sort = Cab3.SortPriority;
						Cab3WriteBlock = Cab3.WriteBlock;
						Cab3Attr = Cab3.Attrubute.ToString();
						cable3guid = Guid.NewGuid().ToString();
						Cab3Descr = Cab3.Description;
					}
					if (Cab4 != null)
					{
						Cab4Sort = Cab4.SortPriority;
						Cab4WriteBlock = Cab4.WriteBlock;
						Cab4Attr = Cab4.Attrubute.ToString();
						cable4guid = Guid.NewGuid().ToString();
						Cab4Descr = Cab4.Description;
					}
					if (Cab5 != null)
					{
						Cab5Sort = Cab5.SortPriority;
						Cab5WriteBlock = Cab5.WriteBlock;
						Cab5Attr = Cab5.Attrubute.ToString();
						cable5guid = Guid.NewGuid().ToString();
						Cab5Descr = Cab5.Description;
					}
					string[] array = { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty };



					string InsertValveQuery = "INSERT INTO Valve ([GUID], [Project], SystemName, [Cable1], BlockName, PosName, [SystemGUID], ValveType, ElementGUID, Description, [Cable2]) VALUES " +
									   $"('{valveguid}','{Project.GetGUID()}','{ventSystem.SystemName}','{cable1guid}', '{valveblockname}','{valveposname}', '{ventSystem.GUID}', '{valvetype}', '{hostguid}', '{description}', '{cable2guid}')";
					oleDbCommand.CommandText = InsertValveQuery;
					oleDbCommand.ExecuteNonQuery();
					if (Cab1 != null)
					{
						WriteCableToDB(ref oleDbCommand, cable1guid, valveposname, valveguid, Cab1Sort, "Valve", Cab1Descr, Cab1WriteBlock, Cab1Attr, valveblockname, Cab1.WireNumbers);
					}
					if (Cab2 != null)
					{
						WriteCableToDB(ref oleDbCommand, cable2guid, valveposname, valveguid, Cab2Sort, "Valve", Cab2Descr, Cab2WriteBlock, Cab2Attr, valveblockname, Cab2.WireNumbers);
					}
					if (Cab3 != null)
					{
						WriteCableToDB(ref oleDbCommand, cable3guid, valveposname, valveguid, Cab3Sort, "Valve", Cab3Descr, Cab3WriteBlock, Cab3Attr, valveblockname, Cab3.WireNumbers);
					}
					if (Cab4 != null)
					{
						WriteCableToDB(ref oleDbCommand, cable4guid, valveposname, valveguid, Cab4Sort, "Valve", Cab4Descr, Cab4WriteBlock, Cab4Attr, valveblockname, Cab4.WireNumbers);
					}
					if (Cab5 != null)
					{
						WriteCableToDB(ref oleDbCommand, cable5guid, valveposname, valveguid, Cab5Sort, "Valve", Cab5Descr, Cab5WriteBlock, Cab5Attr, valveblockname, Cab5.WireNumbers);
					}
					array[0] = valveguid;
					array[1] = cable1guid;
					array[2] = cable2guid;
					array[3] = cable3guid;
					array[4] = cable4guid;
					array[5] = cable5guid;

					return array;

				}
				string WriteKKBToDB(ref SQLiteCommand oleDbCommand, string KKBblockname, string KKBposname, string KKBtype, string hostguid, string stairs, string description, Cable Cab1, Cable Cab2, Cable Cab3, Cable Cab4, Cable Cab5, Cable Cab6)
				{
					var KKBguid = Guid.NewGuid().ToString();
					var cableguid = Guid.NewGuid().ToString();
					string InsertKKBQuery = "INSERT INTO KKB ([GUID], [Project], SystemName, [Cable1], BlockName, PosName, [SystemGUID], KKBType, ElementGUID, Stairs, Description) VALUES " +
									   $"('{KKBguid}','{Project.GetGUID()}','{ventSystem.SystemName}','{cableguid}', '{KKBblockname}','{KKBposname}', '{ventSystem.GUID}', '{KKBtype}', '{hostguid}', '{stairs}', '{description}')";
					oleDbCommand.CommandText = InsertKKBQuery;
					oleDbCommand.ExecuteNonQuery();
					if (Cab1 != null)
					{
						WriteCableToDB(ref command, cableguid, KKBposname, KKBguid, Cab1.SortPriority, "KKB", Cab1.Description, Cab1.WriteBlock, Cab1.Attrubute.ToString(), KKBblockname, Cab1.WireNumbers);
					}
					if (Cab2 != null)
					{
						var cableguid2 = Guid.NewGuid().ToString();
						WriteCableToDB(ref command, cableguid2, KKBposname, KKBguid, Cab2.SortPriority, "KKB", Cab2.Description, Cab2.WriteBlock, Cab2.Attrubute.ToString(), KKBblockname, Cab2.WireNumbers);
					}
					if (Cab3 != null)
					{
						var cableguid3 = Guid.NewGuid().ToString();
						WriteCableToDB(ref command, cableguid3, KKBposname, KKBguid, Cab3.SortPriority, "KKB", Cab3.Description, Cab3.WriteBlock, Cab3.Attrubute.ToString(), KKBblockname, Cab3.WireNumbers);
					}
					if (Cab4 != null)
					{
						var cableguid4 = Guid.NewGuid().ToString();
						WriteCableToDB(ref command, cableguid4, KKBposname, KKBguid, Cab4.SortPriority, "KKB", Cab4.Description, Cab4.WriteBlock, Cab4.Attrubute.ToString(), KKBblockname, Cab4.WireNumbers);
					}
					if (Cab5 != null)
					{
						var cableguid5 = Guid.NewGuid().ToString();
						WriteCableToDB(ref command, cableguid5, KKBposname, KKBguid, Cab5.SortPriority, "KKB", Cab5.Description, Cab5.WriteBlock, Cab5.Attrubute.ToString(), KKBblockname, Cab5.WireNumbers);
					}
					if (Cab6 != null)
					{
						var cableguid6 = Guid.NewGuid().ToString();
						WriteCableToDB(ref command, cableguid6, KKBposname, KKBguid, Cab6.SortPriority, "KKB", Cab6.Description, Cab6.WriteBlock, Cab6.Attrubute.ToString(), KKBblockname, Cab6.WireNumbers);
					}

					//WriteCableToDB(ref oleDbCommand, cableguid, KKBposname, KKBposname, sortpriority, "KKB", description);
					return KKBguid;

				}
				void WriteCableToDB(ref SQLiteCommand oleDbCommand, string senscableguid, string posname, string ToGuid, string sort, string tableforsearch, string description, bool writeblock, string cabattribute, string blockname, int WireNumbers)
                {
					
                    string QuerySensCab = "INSERT INTO Cable (SortPriority, [GUID], [Project], [To], SystemName, [SystemGUID], [ToGUID], TableForSearch, DefaultName, Description, WriteBlock, CableAttribute, BlockName, WireNumbers) VALUES " +
						$"('{sort}', '{senscableguid}', '{Project.GetGUID()}', '{posname}', '{ventSystem.SystemName}', '{ventSystem.GUID}', '{ToGuid}', '{tableforsearch}', '{Regex.Replace(posname, @"[^A-Z]+", string.Empty, RegexOptions.IgnoreCase)}', '{description}', '{writeblock}', '{cabattribute}', '{blockname}', '{WireNumbers}')";
					oleDbCommand.CommandText = QuerySensCab;
					oleDbCommand.ExecuteNonQuery();
				}
				void UpdateVensystem(string columnname)
				{
					string UpdatetQuryntsystems = $"UPDATE VentSystems SET {columnname} = 'True' WHERE GUID= '{ventSystem.GUID}'";
					command.CommandText = UpdatetQuryntsystems;
					command.ExecuteNonQuery();

				}
				string WritePosInfoToDB(ref SQLiteCommand oleDbCommand, EditorV2.PosInfo posInfo, string elementGUID, string SystemGUID)
				{
					var posinfoGUID = Guid.NewGuid().ToString();
					oleDbCommand.CommandText = "INSERT INTO PosInfo ([GUID], Pos, Size, Image, [HostGUID], [SystemGUID]) VALUES " +
						$"('{posinfoGUID}', " +
						$"'{EditorV2.PosInfo.PosToString(posInfo.Pos)}', " +
						$"'{posInfo.SizeX};{posInfo.SizeY}', " +
						$"'{posInfo.ImageName}', " +
						$"'{elementGUID}', " +
						$"'{SystemGUID}')";
					command.ExecuteNonQuery();
					posInfo.GUID = posinfoGUID;
					return posinfoGUID;
				}

                void WriteFControl(Vent vent)
                {
                    command.CommandText =
                        "INSERT INTO FControl ([GUID], Project, SystemName, SystemGUID, ElementGUID, Description) VALUES " +
                        $"('{vent._FControl.GUID}', " +
                        $"'{ProjectGuid}', " +
                        $"'{ventSystem.SystemName}', " +
                        $"'{ventSystem.GUID}', " +
                        $"'{vent.GUID}', " +
                        $"'{vent._FControl.Description}')";

					command.ExecuteNonQuery();
				}

                void WriteSpare<T>(dynamic spareVent, EditorV2.PosInfo item)
                {
                    dynamic MainVent = null;
                    dynamic ReservedVent = null;

					switch (typeof(T).Name)
                    {
						case nameof(SpareSuplyVent):
                            MainVent = spareVent.MainSupplyVent;
                            ReservedVent = spareVent.ReservedSupplyVent;
							break;
						case nameof(SpareExtVent):
                            MainVent = spareVent.MainExtVent;
                            ReservedVent = spareVent.ReservedExtVent;
                            break;


					}
                    
                    string InsertQuerySpareSupplyVent = "INSERT INTO SpareVentilator ([GUID], [SystemGUID], SystemName, Project, Voltage, " +
                                                        "ControlType, Power, ProtectType, [SensPDS], MainVent, ReservedVent, Location, " +
                                                        "Position, PosInfoGUID) " +
                                                        $"VALUES ('{spareVent.GUID}', '{ventSystem.GUID}', '{ventSystem.SystemName}', '{ProjectGuid}', '{spareVent.Voltage}', " +
                                                        $"'{spareVent.ControlType}', '{spareVent.Power}', '{spareVent.Protect}', '{spareVent._PressureContol.GUID}', " +
                                                        $"'{MainVent?.GUID}', '{ReservedVent?.GUID}', '{spareVent.Location}', " +
                                                        $"'{EditorV2.PosInfo.PosToString(item.Pos)}', '{item.GUID}')";
                    command.CommandText = InsertQuerySpareSupplyVent;
                    command.ExecuteNonQuery();

                }
				void WriteVent(Vent vent, EditorV2.PosInfo item, string Spareventguid)
				{
					Vent.FControl fControl = vent._FControl;
					string voltage = vent.Voltage.ToString();
					string controltype = vent.ControlType.ToString();
					string power = vent.Power;
					string protectype = vent.Protect.ToString();
					string blockname = vent.BlockName;
					const string posname = "M";
					string FCGuid = null;
					Cable Cab1 = vent.Cable1;
					Cable Cab2 = vent.Cable2;
					Cable Cab3 = vent.Cable3;
					Cable Cab4 = vent.Cable4;

					var cab1guid = Guid.NewGuid().ToString();
					if (fControl != null)
					{

						FCGuid = Guid.NewGuid().ToString();
						fControl.GUID = FCGuid;
					}
					string InsertQueryVent = "INSERT INTO Ventilator ([GUID], [SystemGUID], SystemName, Project, Voltage, " +
											 "ControlType, Power, ProtectType, [SensPDS], [Cable1], BlockName, PosName, " +
											 "Description, FControl, Location, Position, PosInfoGUID, AttributeSpare, ElementGUID) VALUES " +
											 $"('{vent.GUID}','{ventSystem.GUID}','{ventSystem.SystemName}','{ProjectGuid}'," +
											 $"'{voltage}','{controltype}','{power}','{protectype}','{sensguid1}','{ cab1guid}'," +
											 $"'{blockname}', '{posname}', '{vent.Description}', '{FCGuid}', '{vent.Location}', " +
											 $"'{EditorV2.PosInfo.PosToString(item.Pos)}', '{item.GUID}', '{vent.AttributeSpare}', '{Spareventguid}')";
					command.CommandText = InsertQueryVent;
					command.ExecuteNonQuery();

                    
                    
                    if (Cab1 != null)
					{
						WriteCableToDB(ref command, cab1guid, posname, vent.GUID, Cab1.SortPriority, "Ventilator",
							Cab1.Description, Cab1.WriteBlock, Cab1.Attrubute.ToString(), blockname, Cab1.WireNumbers);
					}
					if (Cab2 != null)
					{
						var cab2guid = Guid.NewGuid().ToString();
						WriteCableToDB(ref command, cab2guid, posname, vent.GUID, Cab2.SortPriority, "Ventilator",
							Cab2.Description, Cab2.WriteBlock, Cab2.Attrubute.ToString(), blockname, Cab2.WireNumbers);
					}
					if (Cab3 != null)
					{
						var cab3guid = Guid.NewGuid().ToString();
						WriteCableToDB(ref command, cab3guid, posname, vent.GUID, Cab3.SortPriority, "Ventilator",
							Cab3.Description, Cab3.WriteBlock, Cab3.Attrubute.ToString(), blockname, Cab3.WireNumbers);
					}
					if (Cab4 != null)
					{
						var cab4guid = Guid.NewGuid().ToString();
						WriteCableToDB(ref command, cab4guid, posname, vent.GUID, Cab4.SortPriority, "Ventilator",
							Cab4.Description, Cab4.WriteBlock, Cab4.Attrubute.ToString(), blockname, Cab4.WireNumbers);
					}
				}
				#endregion
				result = true;
			}
			catch
			{

			}
			return result;
			
			
			

		}
		

	}
	

}
