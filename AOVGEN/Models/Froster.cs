using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace AOVGEN.Models
{
    [Serializable]
    class Froster : ElectroDevice, IEnumerable, IPower, ICompGUID, IGetSensors
    {
		#region Enums
		[TypeConverter(typeof(EnumDescConverter))]
		public enum FrosterType
		{
			[Description("Фреоновый")]
			Freon,
			[Description("Водяной")]
			Water
		}
		[TypeConverter(typeof(EnumDescConverter))]
		public enum FrosterSensor
		{
			[Description("Нет")]
			No,
			[Description("На притоке")]
			SupplySens,
			[Description("На вытяжке")]
			ExhaustSens,
			[Description("На притоке и вытяжке")]
			Supp_ExhSens
		}


		//[System.ComponentModel.TypeConverter(typeof(EnumDescConverter))]

		#endregion

		#region Pictures Path
		public static string Imagepath;
		public static string ImageNullPath = VentSystem.AppFolder + "Images\\10_0.png";
		#endregion

		#region Variabes
		private VentComponents ComponentVariable;
		FrosterType frostertype;
		FrosterSensor frostersensor;
		KKB.FrosterStairs frosterStairs;
		Sensor.SensorType sensor1Type;
		Sensor.SensorType sensor2Type;
		internal ProtectSensor Sens1;
		internal ProtectSensor Sens2;
		string sens1PosName;
		string sens2PosName;
		string sens1BlockName;
		string sens2BlockName;
		public Models.Valve _Valve;
		internal KKB _KKB;
		private KKB.KKBControlType kKBControlType;
		private Models.Valve.ValveType _valveType;
		//internal _Voltage voltage;
		internal ShemaASU ShemaASU;
		#endregion

		#region Properties
		[DisplayName("Тип элемента")]
		public VentComponents comp
		{
			get => ComponentVariable;
			private set => ComponentVariable = value;
		}
		[DisplayName("Тип охладителя")]
		public FrosterType _FrosterType
		{
			get => frostertype;
			set => CheckFrosterType(value);
		}

		[DisplayName("Количество ступеней")]
		public KKB.FrosterStairs Stairs
		{
			get => frosterStairs;
			set => CheckfrosterStairs(value);
		}

		[DisplayName("Тип управления ККБ")]
		public KKB.KKBControlType KKBControlType
		{
			get => kKBControlType;
			set => kKBControlType = MakeKKBControlType(value, frostertype);
		}
		[DisplayName("Тип управления клапаном")]
		public Models.Valve.ValveType valveType
		{
			get => _valveType;
			set => _valveType = CheckValveType(value, ref _Valve);
		}
		[DisplayName("Напряжение")]
		public new _Voltage Voltage
		{
			get => VoltageVariable;
			set => CheckVoltage(ref _KKB, ref _Valve, value);
		}
		[DisplayName("Мощность")]
		public new string Power { get; set; }
		[DisplayName("Контроль тмпературы")]
		internal FrosterSensor _FrosterSensor
		{
			get => frostersensor;
			set
			{
				frostersensor = value;
				sensor1Type = Sensor.SensorType.No;
				sensor2Type = Sensor.SensorType.No;
				Sens1 = null;
				Sens2 = null;
				switch (frostersensor)
				{
					case FrosterSensor.SupplySens:
						Sens1 = new ProtectSensor
						{
							frostersensor = FrosterSensor.SupplySens,
							Description = "Контроль Т охладителя в приточном канале"
						};
						Sens1.ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
						Sens1.ShemaASU.ShemaPos = ShemaASUbase.DummyPos(comp);
						Sens1.ShemaASU.ShemaUp = "Froster_Water_SupplyTemp";
						Sens1._SensorType = MakeSensorType(Sens1);
						MakeSensPosName(ref Sens1);
						MakeSensBlockName(ref Sens1);

						break;
					case FrosterSensor.ExhaustSens: //вот это надо проверить и исключить?
						Sens2 = new ProtectSensor
						{
							frostersensor = FrosterSensor.ExhaustSens,
							Description = "Контроль Т охладителя в вытяжном канале"
						};
						Sens2._SensorType = MakeSensorType(Sens2);
						MakeSensPosName(ref Sens2);
						MakeSensBlockName(ref Sens2);
						break;
					case FrosterSensor.Supp_ExhSens: // вот это надо проверить и исключить?
						Sens1 = new ProtectSensor
						{
							frostersensor = FrosterSensor.SupplySens,
							Description = "Контроль Т охладителя в приточном канале"
						};
						Sens2 = new ProtectSensor
						{
							frostersensor = FrosterSensor.ExhaustSens,
							Description = "Контроль Т охладителя в вытяжном канале"
						};
						Sens1._SensorType = MakeSensorType(Sens1);
						Sens2._SensorType = MakeSensorType(Sens2);
						sens1PosName = MakeSensPosName(ref Sens1);
						sens2PosName = MakeSensPosName(ref Sens2);
						sens1BlockName = MakeSensBlockName(ref Sens1);
						sens2BlockName = MakeSensBlockName(ref Sens2);
						break;

					case FrosterSensor.No:
						sens1PosName = MakeSensPosName(ref Sens1);
						sens2PosName = MakeSensPosName(ref Sens2);

						break;
				}



			}
		}
		[DisplayName("Тип датчика 1")]
		internal Sensor.SensorType Sensor1Type
		{
			get => sensor1Type;
			set => sensor1Type = CheckSensType(value, ref Sens1);
		}
		[DisplayName("Тип датчика 2")]
		internal Sensor.SensorType Sensor2Type
		{
			get => sensor2Type;
			set => sensor2Type = CheckSensType(value, ref Sens2);
		}
		[DisplayName("Поз.обозначение датчика 1")]
		internal string Sens1PosName
		{
			get => sens1PosName;
			private set => sens1PosName = value;
		}
		[DisplayName("Поз.обозначение датчика 2")]
		internal string Sens2PosName
		{
			get => sens2PosName;
			private set => sens2PosName = value;
		}
		[DisplayName("Имя блока датчика 1")]
		internal string Sens1BlockName
		{
			get => sens1BlockName;
			private set => sens1BlockName = value;
		}
		[DisplayName("Имя блока датчика 2")]
		public string Sens2BlockName
		{
			get => sens2BlockName;
			private set => sens2BlockName = value;
		}
		[DisplayName("Поз.обозначение клапана")]
		public string ValvePosName => GetValveInfo()[0];

		[DisplayName("Имя блока клапана")]
		public string ValveBlockName => GetValveInfo()[1];

		[DisplayName("Поз.обозначение ККБ")]
		public string KKBPosName => GetKKBInfo()[0];

		[DisplayName("Имя блока ККБ")]
		public string KKBBlockName => GetKKBInfo()[1];
		[Browsable(false)]
		public string GUID { get; set; }
		internal string Sens1GUID
		{
			get => _getPS1GUID();
			set => Sens1.GUID = value;
		}
		internal string Sens2GUID
		{
			get => _getPS2GUID();
			set => Sens2.GUID = value;
		}
		internal string ValveGUID
		{
			get => _getGUID(_Valve);
			set => _Valve.GUID = value;
		}
		internal string KKBGUID
		{
			get => _getGUID(_KKB);
			set => _KKB.GUID = value;
		}

		#endregion

		#region Methods
		public static string makeimagepath(FrosterType frostertype)
		{
			Imagepath = string.Empty;
			if (frostertype == FrosterType.Freon)
			{
				Imagepath = VentSystem.AppFolder + "Images\\10_2.png";
			}
			if (frostertype == FrosterType.Water)
			{
				Imagepath = VentSystem.AppFolder + "Images\\10_1.png";
			}
			return Imagepath;
		}
		protected Sensor.SensorType CheckSensType(object val, ref ProtectSensor protectSensor)
		{
			Sensor.SensorType sensorType = Sensor.SensorType.No;
			if (frostersensor != FrosterSensor.No && protectSensor != null)
			{
				sensorType = (Sensor.SensorType)val;
				protectSensor._SensorType = sensorType;
				switch (sensorType)
				{
					case Sensor.SensorType.Analogue:
						protectSensor.ShemaASU.ReSetAllIO();
						protectSensor.ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
						break;
					case Sensor.SensorType.Discrete:
						protectSensor.ShemaASU.ReSetAllIO();
						protectSensor.ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
						break;
				}

			}

			if (protectSensor == Sens1)
			{
				sens1PosName = MakeSensPosName(ref Sens1);
				sens1BlockName = MakeSensBlockName(ref Sens1);

			}
			if (protectSensor == Sens2)
			{
				sens2PosName = MakeSensPosName(ref Sens2);
				sens2BlockName = MakeSensBlockName(ref Sens2);
			}

			return sensorType;

		}
		protected string MakeSensPosName(ref ProtectSensor protectSensor)
		{

			string posname = string.Empty;

			if (protectSensor != null)
			{
				switch (protectSensor._SensorType)
				{
					case Sensor.SensorType.Analogue:
						posname = "TE";
						break;
					case Sensor.SensorType.Discrete:
						posname = "TS";
						break;
				}
				switch (frostersensor)
				{
					case FrosterSensor.SupplySens:
						sens1PosName = posname;
						sens2PosName = string.Empty;
						break;
					case FrosterSensor.ExhaustSens:
						sens1PosName = string.Empty;
						sens2PosName = posname;
						break;

					case FrosterSensor.No:
						sens1PosName = string.Empty;
						sens2PosName = string.Empty;
						break;
				}

			}

			return posname;
		}
		protected string MakeSensBlockName(ref ProtectSensor protectSensor)
		{

			string blockname = string.Empty;

			if (protectSensor != null)
			{
				switch (protectSensor._SensorType)
				{
					case Sensor.SensorType.Analogue:
						blockname = "SENS-TE-2WIRE";
						break;
					case Sensor.SensorType.Discrete:
						blockname = "SENS-TS-2WIRE";
						break;
				}
				switch (frostersensor)
				{
					case FrosterSensor.SupplySens:
						sens1BlockName = blockname;
						sens2BlockName = string.Empty;
						break;
					case FrosterSensor.ExhaustSens:
						sens1BlockName = string.Empty;
						sens2BlockName = blockname;
						break;

					case FrosterSensor.No:
						sens1BlockName = string.Empty;
						sens2BlockName = string.Empty;
						break;
				}

			}


			return blockname;
		}
		protected string[] GetValveInfo()
		{
			string[] array = { string.Empty, string.Empty };
			if (_Valve != null)
			{
				array[0] = _Valve.Posname;
				array[1] = _Valve.BlockName;
			}
			return array;
		}
		protected string[] GetKKBInfo()
		{
			string[] array = { string.Empty, string.Empty };
			if (_KKB != null)
			{
				array[0] = _KKB.PosName;
				array[1] = _KKB.BlockName;
				if (_KKB.Cable1 != null) _KKB.Cable1.ToBlockName = _KKB.BlockName;


			}

			return array;
		}
		protected Models.Valve.ValveType CheckValveType(object val, ref Models.Valve valve)
		{

			_valveType = (Models.Valve.ValveType)val;
			try
			{
				if (valve != null)
				{
					valve._ValveType = valveType;
					switch (_valveType)
					{
						case Models.Valve.ValveType.Analogue_0_10:
						case  Models.Valve.ValveType.Analogue_4_20:
							valve.Cable2 = new Cable
							{
								WriteBlock = false,
								Description = "Управление клапаном охладителя",
								Attrubute = Cable.CableAttribute.A,
								ToBlockName = _Valve.BlockName,
								ToPosName = _Valve.Posname,
								WireNumbers = 2
							};
							int sp = int.Parse(_Valve.Cable1.SortPriority);
							valve.Cable2.SortPriority = (sp + 1).ToString();


							break;
						case Models.Valve.ValveType.ThreePos:
							valve.Cable2 = null;
							break;
						case Models.Valve.ValveType.No:
							_Valve.Cable1 = null;
							_Valve.Cable2 = null;
							_Valve = null;
							break;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace);
			}

			return valveType;
		}
		protected void CheckVoltage(ref KKB kKB, ref Models.Valve valve, object val)
		{
			VoltageVariable = (_Voltage)val;
			if (kKB != null)
			{
				switch (VoltageVariable)
				{
					case _Voltage.AC380:

						kKB.Voltage = VoltageVariable;
						if (Cable1 != null) Cable1.WireNumbers = 5;
						break;
					case _Voltage.AC220:
						kKB.Voltage = VoltageVariable;
						if (Cable1 != null) Cable1.WireNumbers = 3;
						break;
					case _Voltage.AC24:
					case _Voltage.DC24:
						VoltageVariable = _Voltage.AC380;
						break;

				}
				kKB.MakeSortPriority(VoltageVariable);


			}
			if (valve != null)
			{
				VoltageVariable = _Voltage.AC24;
				_Valve.Voltage = VoltageVariable;
			}
			//return voltage;
		}
		protected Sensor.SensorType MakeSensorType(ProtectSensor protectSensor)
		{

			Sensor.SensorType sensorType = Sensor.SensorType.No;
			switch (_FrosterSensor)
			{
				case FrosterSensor.No:
					sensor1Type = Sensor.SensorType.No;

					break;
				case FrosterSensor.SupplySens:
					sensor1Type = Sensor.SensorType.Analogue;
					sensorType = sensor1Type;


					break;
				case FrosterSensor.ExhaustSens:
					sensor2Type = Sensor.SensorType.Analogue;
					sensorType = sensor2Type;

					break;
				case FrosterSensor.Supp_ExhSens:
					sensor1Type = Sensor.SensorType.Analogue;
					sensor2Type = Sensor.SensorType.Analogue;
					sensorType = Sensor.SensorType.Analogue;
					break;

			}
			return sensorType;
		}
		protected KKB.KKBControlType MakeKKBControlType(object val, FrosterType frosterType)
		{
			KKB.KKBControlType kKBControlType1 = KKB.KKBControlType.No;
			if (frosterType == FrosterType.Freon)
			{
				kKBControlType1 = (KKB.KKBControlType)val;
				if (_KKB != null)
				{
					_KKB._KKBControlType = kKBControlType1;
					switch (kKBControlType1)
					{
						case KKB.KKBControlType.Analogue:
							if (_KKB.Cable2 != null)
							{
								_KKB.Cable2.Attrubute = Cable.CableAttribute.A;

							}
							if (_KKB.Cable3 != null)
							{
								_KKB.Cable3.Attrubute = Cable.CableAttribute.A;

							}
							if (_KKB.Cable3 != null)
							{
								_KKB.Cable3.Attrubute = Cable.CableAttribute.A;

							}
							if (_KKB.Cable4 != null)
							{
								_KKB.Cable4.Attrubute = Cable.CableAttribute.A;

							}
							if (_KKB.Cable5 != null)
							{
								_KKB.Cable5.Attrubute = Cable.CableAttribute.A;

							}
							_KKB.ShemaASU.ReSetAllIO();
							_KKB.ShemaASU.SetIO(ShemaASU.IOType.AO, 1);
							//_KKB.Cable2 = new Cable
							//{
							//	Attrubute = Cable.CableAttribute.A,
							//	WriteBlock = false,
							//	ToPosName = _KKB.PosName,
							//	ToBlockName = _KKB.BlockName,
							//	Description = "Упавление охладителем"

							//};

							break;
						case KKB.KKBControlType.Discrete:
							//_KKB.Cable2 = new Cable
							//{
							//	Attrubute = Cable.CableAttribute.D,
							//	WriteBlock = false,
							//	ToPosName = _KKB.PosName,
							//	ToBlockName = _KKB.BlockName,
							//	Description = "Упавление охладителем"
							//};
							if (_KKB.Cable2 != null)
							{
								_KKB.Cable2.Attrubute = Cable.CableAttribute.D;

							}
							if (_KKB.Cable3 != null)
							{
								_KKB.Cable3.Attrubute = Cable.CableAttribute.D;

							}
							if (_KKB.Cable3 != null)
							{
								_KKB.Cable3.Attrubute = Cable.CableAttribute.D;

							}
							if (_KKB.Cable4 != null)
							{
								_KKB.Cable4.Attrubute = Cable.CableAttribute.D;

							}
							if (_KKB.Cable5 != null)
							{
								_KKB.Cable5.Attrubute = Cable.CableAttribute.D;

							}
							_KKB.ShemaASU.ReSetAllIO();
							_KKB.ShemaASU.SetIO(ShemaASU.IOType.DO, 1);

							break;
						default:
							_KKB.Cable2 = null;
							_KKB.Cable3 = null;
							_KKB.Cable4 = null;
							_KKB.Cable5 = null;
							break;
					}
				}


			}

			return kKBControlType1;
		}
		private FrosterType CheckFrosterType(object val)
		{
			frostertype = (FrosterType)val;
			Imagepath = makeimagepath(frostertype);
			switch (frostertype)
			{
				case FrosterType.Freon:
					_KKB = new KKB
					{
						_KKBControlType = KKB.KKBControlType.Analogue,
						Voltage = VoltageVariable,
						Description = "Компрессор охладителя"

					};
					_KKB.MakeSortPriority(_KKB.Voltage);
					_Valve = null;
					_valveType = Models.Valve.ValveType.No;
					VoltageVariable = _KKB.Voltage;
					KKBControlType = KKB.KKBControlType.Analogue;
					Stairs = KKB.FrosterStairs.One;
					ShemaASU.ShemaUp = "Froster_Frion_Steps";
					break;
				case FrosterType.Water:
					_Valve = new Models.Valve
					{
						_ValveType = Models.Valve.ValveType.Analogue_0_10,
						Description = "Э/д клапана охладителя",
						SortPriority = "17",
						Power = "10",
						Voltage = _Voltage.AC24,
						ShemaASU = ShemaASU.CreateBaseShema(VentComponents.Froster),
						Posname = "FV",
						BlockName = "ED-24"

					};
					_Valve.Cable1 = new Cable
					{
						WriteBlock = true,
						Attrubute = Cable.CableAttribute.P,
						ToPosName = _Valve.Posname,
						ToBlockName = _Valve.BlockName,
						SortPriority = _Valve.SortPriority,
						Description = _Valve.Description
					};
					_Valve.ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
					_Valve.ShemaASU.SetIO(ShemaASU.IOType.AO, 1);
					_Valve.ShemaASU.ShemaUp = "Supply_Froster_Water";
					_KKB = null;
					
					break;
			}
			return frostertype;

		}
		private string _getPS1GUID()
		{
			string ps1g = string.Empty;
			if (Sens1 != null) ps1g = Sens1.GUID;
			return ps1g;
		}
		private string _getPS2GUID()
		{
			string ps2g = string.Empty;
			if (Sens2 != null) ps2g = Sens2.GUID;
			return ps2g;
		}
		private string _getGUID(object obj)
		{
			string guid = string.Empty;
			if (obj is KKB)
			{
				KKB kkb = _KKB;
				guid = kkb.GUID;
			}

			if (obj is Valve)
			{
				Valve valve = _Valve;
				guid = valve.GUID;
			}
			return guid;

		}
		private KKB.FrosterStairs CheckfrosterStairs(object val)
		{
			frosterStairs = (KKB.FrosterStairs)val;
			_KKB.Stairs = frosterStairs;
			_KKB.StairsString = GetEnumDescription(Stairs);
			switch (frosterStairs)
			{
				case KKB.FrosterStairs.One:
					_KKB.Displacement = -28.75646364;

					_KKB.Cable2 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _KKB.PosName,
						ToBlockName = _KKB.BlockName,
						Description = "Управление ККБ 1 ступень",
						WireNumbers = 2
					};
					_KKB.Cable3 = null;
					_KKB.Cable4 = null;
					_KKB.Cable5 = null;
					_KKB.Cable2.SortPriority = _KKB.Cable2.MakeControlSortpriority(_KKB.SortPriority + "2");
					ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
					break;
				case KKB.FrosterStairs.Two:
					_KKB.Displacement = -19.80135002;

					_KKB.Cable2 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _KKB.PosName,
						ToBlockName = _KKB.BlockName,
						Description = "Управление ККБ 1 ступень",
						WireNumbers = 2
					};
					_KKB.Cable3 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _KKB.PosName,
						ToBlockName = _KKB.BlockName,
						Description = "Управление ККБ 2 ступень",
						WireNumbers = 2
					};
					_KKB.Cable4 = null;
					_KKB.Cable5 = null;
					_KKB.Cable2.SortPriority = _KKB.Cable2.MakeControlSortpriority(_KKB.SortPriority + "2");
					_KKB.Cable3.SortPriority = _KKB.Cable3.MakeControlSortpriority(_KKB.SortPriority + "3");
					ShemaASU.SetIO(ShemaASU.IOType.DO, 2);
					break;
				case KKB.FrosterStairs.Three:
					_KKB.Displacement = -9.90067439;

					_KKB.Cable2 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _KKB.PosName,
						ToBlockName = _KKB.BlockName,
						Description = "Управление ККБ 1 ступень",
						WireNumbers = 2
					};
					_KKB.Cable3 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _KKB.PosName,
						ToBlockName = _KKB.BlockName,
						Description = "Управление ККБ 2 ступень",
						WireNumbers = 2
					};
					_KKB.Cable4 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _KKB.PosName,
						ToBlockName = _KKB.BlockName,
						Description = "Управление ККБ 3 ступень",
						WireNumbers = 2
					};
					_KKB.Cable5 = null;
					_KKB.Cable2.SortPriority = _KKB.Cable2.MakeControlSortpriority(_KKB.SortPriority + "2");
					_KKB.Cable3.SortPriority = _KKB.Cable3.MakeControlSortpriority(_KKB.SortPriority + "3");
					_KKB.Cable4.SortPriority = _KKB.Cable4.MakeControlSortpriority(_KKB.SortPriority + "4");
					ShemaASU.SetIO(ShemaASU.IOType.DO, 3);
					break;
				case KKB.FrosterStairs.Four:
					_KKB.Displacement = 0;
					_KKB.StairsString = "4";
					_KKB.Cable2 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _KKB.PosName,
						ToBlockName = _KKB.BlockName,
						Description = "Управление ККБ 1 ступень",
						WireNumbers = 2
					};
					_KKB.Cable3 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _KKB.PosName,
						ToBlockName = _KKB.BlockName,
						Description = "Управление ККБ 2 ступень",
						WireNumbers = 2
					};
					_KKB.Cable4 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _KKB.PosName,
						ToBlockName = _KKB.BlockName,
						Description = "Управление ККБ 3 ступень",
						WireNumbers = 2
					};
					_KKB.Cable5 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _KKB.PosName,
						ToBlockName = _KKB.BlockName,
						Description = "Управление ККБ 4 ступень",
						WireNumbers = 2
					};
					_KKB.Cable2.SortPriority = _KKB.Cable2.MakeControlSortpriority(_KKB.SortPriority + "2");
					_KKB.Cable3.SortPriority = _KKB.Cable3.MakeControlSortpriority(_KKB.SortPriority + "3");
					_KKB.Cable4.SortPriority = _KKB.Cable4.MakeControlSortpriority(_KKB.SortPriority + "4");
					_KKB.Cable5.SortPriority = _KKB.Cable5.MakeControlSortpriority(_KKB.SortPriority + "5");
					ShemaASU.SetIO(ShemaASU.IOType.DO, 4);
					break;

			}

			return frosterStairs;

		}
		private void SetMinRequest()
        {
			ComponentVariable = VentComponents.Froster;
			ShemaASU = ShemaASU.CreateBaseShema(ComponentVariable);
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
			Voltage = _Voltage.AC220;
			Power = "0";
		}
		private void SetFreon()
        {
			ComponentVariable = VentComponents.Froster;
			ShemaASU = ShemaASU.CreateBaseShema(ComponentVariable);
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
			Voltage = _Voltage.AC220;
			Power = "0";
			_FrosterType = FrosterType.Freon;
			_KKB._KKBControlType = KKB.KKBControlType.Discrete;
			_KKB.MakeSortPriority(Voltage);

		}
		#endregion

		#region Interenal Classes
	
		
		[Serializable]
		internal class ProtectSensor : SensorT
		{
			public enum FrosterSensorType
			{
				Discrete,
				Analogue
			}
			internal FrosterSensor frostersensor;

		}

		#endregion
		#region Constructors
		public Froster(bool MinRequest)
		{
			if (MinRequest) 
			{
				SetMinRequest();
			}
			else
			{
				SetFreon();
			}

		}
		public Froster(FrosterType frosterType)
		{
			ComponentVariable = VentComponents.Froster;
			ShemaASU = ShemaASU.CreateBaseShema(ComponentVariable);
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
			Voltage = _Voltage.AC220;
			Power = "0";
			_FrosterType = frostertype;
			if (frostertype == FrosterType.Freon)
			{
				_KKB._KKBControlType = KKB.KKBControlType.Discrete;
				_KKB.MakeSortPriority(Voltage);
			}

		}
		public Froster()
        {
			SetFreon();
		}

		#endregion
		#region Enumerator
		public IEnumerator<object> GetEnumerator()
		{
			if (_KKB != null)
			{
				yield return _KKB;
			}
			else
			{
				yield return Sens1;
				yield return Sens2;
				yield return _Valve;
			}

		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		public static string GetEnumDescription(Enum enumValue)
		{
			var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

			var descriptionAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

			return descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : enumValue.ToString();
		}
		public List<dynamic> GetSensors()
		{
			return new List<dynamic>
			{
				Sens1,
				Sens2
			};

		}
		#endregion
	}

    
}
