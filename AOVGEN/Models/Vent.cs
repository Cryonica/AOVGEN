using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AOVGEN.Models
{
    [Serializable]
	abstract class Vent : ElectroDevice, ICompGUID, IGetSensors
    {
		private _PressureProtect PressureProtectVariable;
		private AHUProtect aHUProtectVariable;
		private AHUContolType ContolTypeVariable;
		internal ShemaASU ShemaASU;
		internal FControl _FControl;
		protected bool _isReserved;
		protected bool _isSpare;
		protected internal string Location { get; set; }
		protected internal string AttributeSpare;
		[TypeConverter(typeof(EnumDescConverter))]
		public enum AHUContolType
		{
			[Description("Прямой пуск")]
			Direct,
			[Description("Преобразователь частоты")]
			Soft,
			[Description("Регулятор оборотов")]
			FCControl,
			[Description("Трансформатор")]
			Transworm
		}
		[TypeConverter(typeof(EnumDescConverter))]
		public enum AHUProtect
		{
			[Description("Нет")]
			No,
			[Description("Термоконтакт")]
			Thermokontakt,
			[Description("Позистор")]
			Posistor
		}
		[TypeConverter(typeof(EnumDescConverter))]
		public enum _PressureProtect
		{
			[Description("Нет")]
			None,
			[Description("Аналоговый датчик")]
			Analogue,
			[Description("Дискретный")]
			Discrete

		}
		[DisplayName("Тип элемента")]
		public VentComponents comp { get; internal set; }
		[DisplayName("Тип управления")]
		public AHUContolType ControlType { get => ContolTypeVariable; set => CheckControl(value); }
		[DisplayName("Напряжение")]
		public new _Voltage Voltage { get => VoltageVariable; set => CheckVoltage(value); }
		[DisplayName("Мощность")]
		public new string Power { get; set; }
		[DisplayName("Защита двигателя")]
		public AHUProtect Protect { get => aHUProtectVariable; set => SetCable2Type(value); }
		[DisplayName("Датчик переда давления")]
		public _PressureProtect PressureProtect { get => PressureProtectVariable; set => CheckPressureType(value); }
		internal string SortPriority => MakeSortPriority(VoltageVariable);
		internal Cable Cable2;
		internal Cable Cable3;
		internal Cable Cable4;
		internal string Description2;
		public string PosName = "M";
		private string blockname;private string MakeBlockName()
		{

			blockname = string.Empty;
			{
				switch (Voltage)
				{
					case _Voltage.AC380:
						switch (ControlType)
						{
							case AHUContolType.Direct:
								switch (Protect)
								{
									case AHUProtect.Thermokontakt:
										blockname = "ED-380-TK-NORO";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										Cable2 = null;
										Cable3 = null;
										break;
									case AHUProtect.Posistor:
										blockname = "ED-380-Posistor-NORO";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										Cable2 = null;
										Cable3 = null;
										break;
									default:
										blockname = "ED-380-NORO";
										Cable2 = null;
										Cable3 = null;
										Cable4 = null;
										break;
								}
								break;
							case AHUContolType.Soft:
								switch (Protect)
								{
									case AHUProtect.Thermokontakt:
										blockname = "ED-380-TK-RO";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										Cable2 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.P,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Кабель от регулятора к вентилятору",
											WireNumbers = 5

										};
										Cable3 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.D,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Контроль перегрева регулятором",
											WireNumbers = 2

										};
										Cable4 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.D,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Сигналы пуск и авария",
											WireNumbers = 4
										};
										Cable2.MakeControlSortpriority(SortPriority + "2");
										Cable3.MakeControlSortpriority(SortPriority + "3");
										Cable4.MakeControlSortpriority(SortPriority + "4");
										break;
									case AHUProtect.Posistor:
										blockname = "ED-380-Posistor-RO";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);

										Cable2 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.P,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Кабель от регулятора к вентилятору",
											WireNumbers = 5

										};
										Cable3 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.D,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Контроль перегрева регулятором",
											WireNumbers = 2

										};
										Cable4 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.D,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Сигналы пуск и авария",
											WireNumbers = 4
										};
										Cable2.MakeControlSortpriority(SortPriority + "2");
										Cable3.MakeControlSortpriority(SortPriority + "3");
										Cable4.MakeControlSortpriority(SortPriority + "4");
										break;

									default:
										blockname = "ED-380-RO";
										//Cable2 = null;
										Cable2 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.P,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Кабель от регулятора к вентилятору",
											WireNumbers = 5

										};
										Cable2.MakeControlSortpriority(sortpriority + "2");
										Cable3 = null;
										Cable4 = null;
										break;

								}
								break;
							case AHUContolType.Transworm:
								switch (Protect)
								{
									case AHUProtect.Thermokontakt:
										blockname = "ED-380-TK-Transform";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										Cable2 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.P,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Кабель от регулятора к вентилятору",
											WireNumbers = 5

										};
										Cable3 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.D,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Контроль перегрева регулятором",
											WireNumbers = 2

										};
										Cable4 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.D,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Сигнал пуск",
											WireNumbers = 2
										};
										Cable2.MakeControlSortpriority(SortPriority + "2");
										Cable3.MakeControlSortpriority(SortPriority + "3");
										Cable4.MakeControlSortpriority(SortPriority + "4");
										break;
									case AHUProtect.Posistor:
										blockname = "ED-380-Posistor-Transform";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										ShemaASU.SetIO(ShemaASU.IOType.AO, 1);
										Cable2 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.P,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Кабель от регулятора к вентилятору",
											WireNumbers = 5

										};
										Cable3 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.D,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Контроль перегрева регулятором",
											WireNumbers = 2

										};
										Cable4 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.D,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Сигнал пуск",
											WireNumbers = 2
										};
										Cable2.MakeControlSortpriority(SortPriority + "2");
										Cable3.MakeControlSortpriority(SortPriority + "3");
										Cable4.MakeControlSortpriority(SortPriority + "4");
										break;
									default:
										blockname = "ED-380-Transform";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										Cable2 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.P,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Кабель от регулятора к вентилятору",
											WireNumbers = 5

										};
										Cable3 = null;
										Cable4 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.D,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Сигнал пуск",
											WireNumbers = 2
										};
										Cable2.MakeControlSortpriority(SortPriority + "2");
										Cable4.MakeControlSortpriority(SortPriority + "4");
										break;
								}
								break;
						}
						break;
					case _Voltage.AC220:
						switch (ControlType)
						{
							case AHUContolType.Direct:
								switch (Protect)
								{
									case AHUProtect.Thermokontakt:
										blockname = "ED-220-TK-NORO";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										ShemaASU.SetIO(ShemaASU.IOType.DI, 1);

										break;
									case AHUProtect.Posistor:
										blockname = "ED-220-Posistor-NORO";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
										break;
									case AHUProtect.No:
										blockname = "ED-220-NORO";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										break;

								}
								break;
							case AHUContolType.FCControl:
								switch (Protect)
								{
									case AHUProtect.Thermokontakt:
										blockname = "ED-220-TK-RO";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
										Cable2 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.P,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Кабель от регулятора к вентилятору",
											WireNumbers = 3

										};

										Cable3 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.D,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Контроль перегрева",
											WireNumbers = 2

										};
										Cable4 = null;
										Cable2.MakeControlSortpriority(SortPriority + "2");
										Cable3.MakeControlSortpriority(SortPriority + "3");
										break;
									case AHUProtect.Posistor:
										blockname = "ED-220-Posistor-RO";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
										Cable2 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.P,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Кабель от регулятора к вентилятору",
											WireNumbers = 3

										};
										Cable3 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.D,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Контроль перегрева",
											WireNumbers = 2

										};
										Cable4 = null;
										Cable2.MakeControlSortpriority(SortPriority + "2");
										Cable3.MakeControlSortpriority(SortPriority + "3");
										break;
									default:
										blockname = "ED-220-RO";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										Cable2 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.P,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Кабель от регулятора к вентилятору",
											WireNumbers = 3

										};
										Cable3 = null;
										Cable4 = null;
										Cable2.MakeControlSortpriority(SortPriority + "2");

										break;


								}
								break;
							case AHUContolType.Transworm:
								switch (Protect)
								{
									case AHUProtect.Thermokontakt:
										blockname = "ED-220-TK-Transform";
										break;
									case AHUProtect.Posistor:
										blockname = "ED-220-Posistor-Transform";
										break;
									default:
										blockname = "ED-220-Transform";
										break;

								}
								break;
							case AHUContolType.Soft:
								blockname = string.Empty;
								break;


						}
						break;
					case _Voltage.AC24:
					case _Voltage.DC24:
						blockname = "ED-220-TK-NORO";
						ShemaASU.ReSetAllIO();
						ShemaASU.SetIO(ShemaASU.IOType.DO, 1);

						break;
					default:
						blockname = string.Empty;
						break;

				}
			}

			//{
			//	switch (Voltage)
			//	{
			//		case _Voltage.AC380:
			//			if (ECType) blockname = "ED-380-EC-TK";
			//			if (Protect == AHUProtect.Thermokontakt) blockname = "ED-380-TK";
			//			break;
			//		case _Voltage.AC220:
			//			blockname = "ED-220-NORO";
			//			break;
			//		default:
			//			blockname = string.Empty;
			//			break;
			//	}
			//}

			return blockname;
		}
		[DisplayName("Имя блока вентилятора")]
		public string BlockName => MakeBlockName();
		internal PressureContol _PressureContol;
		private void MakePressureSensor(_PressureProtect protect)
		{
			_PressureContol = null;
			switch (protect)
			{
				case _PressureProtect.Analogue:
					_PressureContol = new PressureContol
					{
						_SensorType = Sensor.SensorType.Analogue
					};
					_PressureContol.ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
					_PressureContol.ShemaASU.ShemaPos = ShemaASUbase.DummyPos(comp);
					_PressureContol.ShemaASU.ScheUpSize = ShemaASUbase.DummySize(comp);

					break;
				case _PressureProtect.Discrete:
					_PressureContol = new PressureContol
					{
						_SensorType = Sensor.SensorType.Discrete
					};

					_PressureContol.ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
					_PressureContol.ShemaASU.ShemaPos = ShemaASUbase.DummyPos(comp);
					_PressureContol.ShemaASU.ScheUpSize = ShemaASUbase.DummySize(comp);


					break;

			}
			switch (comp)
			{
				case VentComponents.SupplyVent:
					ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
					if (_PressureContol != null)
					{
						_PressureContol.SetDescrtiption("Контроль работы приточного вентилятора");
						_PressureContol.ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
						_PressureContol.ShemaASU.ShemaUp = "SupplyVent_PD";
					}

					break;
				case VentComponents.ExtVent:
					ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Exhaust;
					if (_PressureContol != null)
					{
						_PressureContol.SetDescrtiption("Контроль работы вытяжного вентилятора");
						_PressureContol.ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Exhaust;
						_PressureContol.ShemaASU.ShemaUp = "ExtVent_PD";
					}

					break;

			}
			switch (comp)
			{
				case VentComponents.SupplyVent:
					if (_PressureContol != null)
					{
						_PressureContol.ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
						_PressureContol.ShemaASU.ShemaUp = "SupplyVent_PD";
					}

					break;
				case VentComponents.ExtVent:
					if (_PressureContol != null)
					{
						_PressureContol.ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Exhaust;
						_PressureContol.ShemaASU.ShemaUp = "ExtVent_PD";
					}

					break;

			}
		}
		[DisplayName("Обозначение датчика")]
		public string PressureName => MakePressureName();
		private string MakePressureName()
		{
			string posname = string.Empty;
			switch (PressureProtect)
			{
				case _PressureProtect.Discrete:
					posname = "PDS";
					break;
				case _PressureProtect.Analogue:
					posname = "PE";
					break;
			}
			return posname;
		}
		internal string MakePressureBlockName()
		{
			//string blockname = string.Empty;
			switch (PressureProtect)
			{
				case _PressureProtect.Discrete:
					blockname = "SENS-PDS-2WIRE";
					break;
				case _PressureProtect.Analogue:
					blockname = "SENS-PE-2WIRE";
					break;

			}
			return blockname;
		}
		[DisplayName("Имя блока датчика")]
		public string PressureBlockname => MakePressureBlockName();
		public string GUID { get; set; }
		private void CheckPressureType(object val)
		{
			PressureProtectVariable = (_PressureProtect)val;

			MakePressureSensor(PressureProtectVariable);
		}
		private void SetCable2Type(object val)
		{
			AHUProtect aHU = (AHUProtect)val;
			
			switch (aHU)
			{
				case AHUProtect.Thermokontakt:
					Cable4 = new Cable
					{
						Attrubute = Cable.CableAttribute.D,
						WriteBlock = false,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = Description2,
						WireNumbers = 2

					};
					Cable4.MakeControlSortpriority(sortpriority + "4");
					ShemaASU.ReSetIO(ShemaASU.IOType.AI);
					ShemaASU.SetIO(ShemaASU.IOType.DI, 1);


					break;
				case AHUProtect.Posistor:
					Cable4 = new Cable
					{
						Attrubute = Cable.CableAttribute.A,
						WriteBlock = false,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = Description2,
						WireNumbers = 2
					};
					Cable4.MakeControlSortpriority(sortpriority + "4");
					ShemaASU.ReSetIO(ShemaASU.IOType.DI);
					ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
					break;
				case AHUProtect.No:
					Cable4 = null;
					if (aHUProtectVariable == AHUProtect.No) break;
					ShemaASU = ShemaASU.CreateBaseShema();
					break;


			}
			aHUProtectVariable = aHU;
		}
		private void CheckVoltage(object val)
		{
			VoltageVariable = (_Voltage)val;
			if (Cable1 != null)
			{
				Cable1.SortPriority = SortPriority; Cable1.ToBlockName = BlockName;
				switch (VoltageVariable)
				{
					case _Voltage.AC380:
						Cable1.WireNumbers = 5;
						break;
					case _Voltage.AC220:
					case _Voltage.AC24:
					case _Voltage.DC24:
						Cable1.WireNumbers = 3;
						break;
				}
			}
			if (Cable4 != null) { Cable4.MakeControlSortpriority(SortPriority + "4"); Cable4.ToBlockName = BlockName; }
		}
		internal abstract string MakeSortPriority(_Voltage voltage);
		private void CheckControl(object val)
		{
			ContolTypeVariable = (AHUContolType)val;
			if (Cable1 != null) { Cable1.ToBlockName = BlockName; }
			if (Cable4 != null) { Cable4.ToBlockName = BlockName; }


			GetShemaAsuShemaUp(comp);

			switch (ContolTypeVariable)
			{
				case AHUContolType.Soft:
					_FControl = new FControl();
					Voltage = _Voltage.AC380;
					break;
				case AHUContolType.FCControl:
					_FControl = new FControl();
					Voltage = _Voltage.AC220;
					break;
				default:
					_FControl = null;
					break;

			}


			//нужно переделывать имя блока у схемы автоматизации в зависимости от П/В и есть РО или нет

			void GetShemaAsuShemaUp(VentComponents ventComponents)
			{



				switch (ventComponents)
				{
					case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.Direct && !_isSpare:
						ShemaASU.ShemaUp = "Supply_Vent";
						ShemaASU.Description1 = "Управление приточным вентилятором";
						break;
					case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.FCControl && !_isSpare:
					case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.Soft && !_isSpare:
					case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.Transworm && !_isSpare:
						ShemaASU.ShemaUp = "Supply_Vent_RO";
						ShemaASU.Description1 = "Пуск и управление скоростью приточного вентилятора";
						break;
					case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.Direct && !_isSpare:
						ShemaASU.ShemaUp = "Ext_Vent";
						ShemaASU.Description1 = "Управление вытяжным вентилятором";
						break;
					case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.FCControl && !_isSpare:
					case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.Soft && !_isSpare:
					case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.Transworm && !_isSpare:
						ShemaASU.ShemaUp = "Ext_Vent_RO";
						ShemaASU.Description1 = "Пуск и управление скоростью вытяжного вентилятора";
						break;
					case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.Direct && _isSpare && !_isReserved:
						ShemaASU.ShemaUp = "Supply_Vent_Spare";
						ShemaASU.Description1 = "Управление основным приточным вентилятором";
						break;
					case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.FCControl && _isSpare && !_isReserved:
					case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.Soft && !_isSpare:
					case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.Transworm && !_isSpare:
						ShemaASU.ShemaUp = "Supply_Vent_Spare_RO";
						ShemaASU.Description1 = "Управление скоростьью основного приточного вентилятора";
						break;
					case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.Direct && _isSpare && !_isReserved:
						ShemaASU.ShemaUp = "Ext_Vent_Spare";
						ShemaASU.Description1 = "Управление основным вытяжным вентилятором";
						break;
					case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.FCControl && _isSpare && !_isReserved:
					case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.Soft && _isSpare && !_isReserved:
					case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.Transworm && _isSpare && !_isReserved:
						ShemaASU.ShemaUp = "Ext_Vent_Spare_RO";
						ShemaASU.Description1 = "Управление скоростьью основного вытяжного вентилятора";
						break;
					case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.Direct && _isSpare && _isReserved:
						ShemaASU.ShemaUp = "Supply_Vent_Spare";
						ShemaASU.Description1 = "Управление резервным приточным вентилятором";
						break;
					case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.FCControl && _isSpare && _isReserved:
					case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.Soft && _isSpare:
					case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.Transworm && _isSpare:
						ShemaASU.ShemaUp = "Supply_Vent_Spare_RO";
						ShemaASU.Description1 = "Управление скоростьью резервеного приточного вентилятора";
						break;
					case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.Direct && _isSpare && _isReserved:
						ShemaASU.ShemaUp = "Ext_Vent_Spare";
						ShemaASU.Description1 = "Управление резервным вытяжным вентилятором";
						break;
					case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.FCControl && _isSpare && _isReserved:
					case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.Soft && _isSpare && _isReserved:
					case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.Transworm && _isSpare && _isReserved:
						ShemaASU.ShemaUp = "Ext_Vent_Spare_RO";
						ShemaASU.Description1 = "Управление скоростьью резервного вытяжного вентилятора";
						break;


				}


			}
		}
		public List<dynamic> GetSensors()
		{
			return new List<dynamic>
			{
				_PressureContol,
				_FControl
			};

		}
	}
}
