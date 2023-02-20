using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace AOVGEN.Models
{
    [Serializable]
	class WaterHeater : IEnumerable, ICompGUID, IGetSensors
    {
		[TypeConverter(typeof(EnumDescConverter))]
		public enum WaterHeaterProtect
		{
			[Description("Нет")]
			No,
			[Description("В канале")]
			Duct,
			[Description("На обрат.воде")]
			BackWater,
			[Description("В канале и обрат.воде")]
			Duct_BackWater
		}
		public static string Imagepath = VentSystem.AppFolder + "Images\\7_1.png"; //это пути для картинок
		public static string ImageNullPath = VentSystem.AppFolder + "Images\\7_0.png";
		private VentComponents ComponentVariable;
		internal Valve.ValveType valveType;
		private bool pumpTK;
		internal ShemaASU ShemaASU;
		internal SensorT PS1;
		internal SensorT PS2;
		private Sensor.SensorType PS1sensorType;
		private Sensor.SensorType PS2sensorType;
		internal Pump _Pump;
		
		internal Valve _Valve;

		[DisplayName("Тип элемента")]
		public VentComponents comp
		{
			get => ComponentVariable;
			private set => ComponentVariable = value;
		}
		private WaterHeaterProtect waterprotect;

		[DisplayName("Тип клапана")]
		public Valve.ValveType _valveType
		{
			get => valveType;
			set => CheckValveType(value);
		}

		[DisplayName("Термоконтакты насоса")]
		public bool HasTK
		{
			get => pumpTK;
			set => PumpSetTK(value);
		}

		[DisplayName("Защита нагревателя")]
		public WaterHeaterProtect Waterprotect
		{


			get => waterprotect;
			set
			{
				waterprotect = value;
				PS1 = null;
				PS2 = null;
				switch (waterprotect)
				{
					case WaterHeaterProtect.Duct:
						PS1 = new SensorT("Защита нагревателя по Т в канале") // CreateProtect(waterprotect);
						{
							_SensorType = Sensor.SensorType.Discrete
						};
						PS1sensorType = PS1._SensorType;

						PS1.ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
						PS1.ShemaASU.ShemaUp = "Supply_WaterHeater_TempAir";
						PS1.ShemaASU.ShemaPos = ShemaASUbase.DummyPos(VentComponents.WaterHeater);

						PS2sensorType = Sensor.SensorType.No;
						break;
					case WaterHeaterProtect.BackWater:
						PS2 = new SensorT("Защита нагревателя по Т обратной воды") //CreateProtect(waterprotect);
						{
							_SensorType = Sensor.SensorType.Analogue
						};
						PS2sensorType = PS2._SensorType;

						PS2.ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
						PS2.ShemaASU.ShemaUp = "Supply_WaterHeater_TempBackWater";
						PS2.ShemaASU.ShemaPos = ShemaASUbase.DummyPos(VentComponents.WaterHeater);



						PS1sensorType = Sensor.SensorType.No;
						break;
					case WaterHeaterProtect.Duct_BackWater:
						PS1 = new SensorT("Защита нагревателя по Т в канале")
						{
							_SensorType = Sensor.SensorType.Discrete,
							ShemaASU =
							{
								componentPlace = ShemaASUbase.ComponentPlace.Supply,
								ShemaUp = "Supply_WaterHeater_TempAir",
								ShemaPos = ShemaASUbase.DummyPos(VentComponents.WaterHeater)
							}
						}; //CreateProtect(waterprotect);

						PS2 = new SensorT("Защита нагревателя по Т обратной воды")
						{
							_SensorType = Sensor.SensorType.Analogue,
							ShemaASU =
							{
								componentPlace = ShemaASUbase.ComponentPlace.Supply,
								ShemaUp = "Supply_WaterHeater_TempBackWater",
								ShemaPos = ShemaASUbase.DummyPos(VentComponents.WaterHeater)
							}
						};// CreateProtect(waterprotect);

						PS1sensorType = PS1._SensorType;
						PS2sensorType = PS2._SensorType;
						break;
					case WaterHeaterProtect.No:
						PS1 = null;
						PS2 = null;
						PS1sensorType = Sensor.SensorType.No;
						PS2sensorType = Sensor.SensorType.No;


						break;
				}

			}
		}

		internal Sensor.SensorType _PS1SensType
		{
			get => PS1sensorType;
			set => SetSensorType(ref PS1, value);
		}

		internal Sensor.SensorType _PS2SensType
		{
			get => PS2sensorType;
			set => SetSensorType(ref PS2, value);
		}

		[DisplayName("Поз.обозн. датчика Т в канале")]
		public string PS1PosName => MakePSPosName(PS1, PS1sensorType);

		[DisplayName("Поз.обозн. датчика Т обр.воды")]
		public string PS2PosName => MakePSPosName(PS2, PS2sensorType);

		[DisplayName("Поз.обозн. насоса")]
		public string PumpPosName => MakePumpPosName(_Pump);

		[DisplayName("Поз.обозн. клапана")]
		public string ValvePosName => _Valve.Posname;

		[DisplayName("Имя блока датч. Т в кнале")]
		public string PS1Blockname => MakePSBockName(PS1, PS1sensorType);

		[DisplayName("Имя блока датч. Т на обр.воде")]
		public string PS2Blockname => MakePSBockName(PS2, PS2sensorType);

		[DisplayName("Имя блока насоса")]
		public string PumpBlockName => MakePumpBlockName(_Pump, HasTK);

		[DisplayName("Имя блока клапана")]
		public string ValveBlockName => _Valve.BlockName;

		public string GUID { get; set; }

		internal string PS1GUID
		{
			get => _getPS1GUID();
			set => PS1.GUID = value;
		}

		internal string PS2GUID
		{
			get => _getPS2GUID();
			set => PS2.GUID = value;
		}

		internal string ValveGUID
		{
			get => _getValeveGUID();
			set => _Valve.GUID = value;
		}

		internal string PumpGUID
		{
			get => _getPumpGUID();
			set => _Pump.GUID = value;
		}

		private static string MakePSBockName(Sensor protectSensor, Sensor.SensorType sensorType)
		{
			string blockname = string.Empty;

			if (protectSensor != null)
			{

				switch (sensorType)
				{
					case (Sensor.SensorType.Discrete):
						blockname = "SENS-TS-2WIRE";
						protectSensor.Blockname = blockname;
						break;
					case (Sensor.SensorType.Analogue):
						blockname = "SENS-TE-2WIRE";
						protectSensor.Blockname = blockname;
						break;

				}

			}
			return blockname;
		}

		private string MakePSPosName(SensorT protectSensor, Sensor.SensorType sensorType)
		{
			string PosName = "null";

			if (protectSensor != null)
			{
				switch (sensorType)
				{
					case (Sensor.SensorType.Analogue):
						PosName = "TE";
						protectSensor.PosName = PosName;



						break;
					case (Sensor.SensorType.Discrete):
						PosName = "TS";
						protectSensor.PosName = PosName;
						break;

				}

			}
			return PosName;

		}

		private static string MakePumpBlockName(Pump pump, bool val)
		{
			string PumpBlockName = string.Empty;
			if (pump == null) return PumpBlockName;
			pump.HasTK = val;
			PumpBlockName = pump.BlockName;
			return PumpBlockName;
		}

		private static string MakePumpPosName(Pump pump)
		{
			string PumpPosName = string.Empty;
			if (pump != null)
			{
				PumpPosName = pump.PosName;
			}
			return PumpPosName;
		}

		private string _getPS1GUID()
		{
			string ps1g = string.Empty;
			if (PS1 != null) ps1g = PS1.GUID;
			return ps1g;
		}

		private string _getPS2GUID()
		{
			string ps2g = string.Empty;
			if (PS2 != null) ps2g = PS2.GUID;
			return ps2g;
		}

		private string _getPumpGUID()
		{
			string pumpg = string.Empty;
			if (_Pump != null) pumpg = _Pump.GUID;
			return pumpg;
		}

		private string _getValeveGUID()
		{
			string valvg = string.Empty;
			if (_Valve != null) valvg = _Valve.GUID;
			return valvg;
		}

		private void PumpSetTK(object val)
		{
			pumpTK = Convert.ToBoolean(val);
			if (_Pump != null)
			{
				_Pump.HasTK = pumpTK;

			}
		}

		private void CheckValveType(object val)
		{
			valveType = (Valve.ValveType)val;
			switch (valveType)
			{
				case Valve.ValveType.Analogue_0_10:
					_Valve.Cable2.Attrubute = Cable.CableAttribute.A;
					_Valve.Cable3 = null;
					_Valve.Cable4 = null;
					_Valve.Cable5 = null;

					_Valve.BlockName = "ED-24-010V";
					break;
				case Valve.ValveType.Analogue_4_20:
					_Valve.Cable2.Attrubute = Cable.CableAttribute.A;
					_Valve.BlockName = "ED-220-420mA";
					_Valve.Cable3 = null;
					_Valve.Cable4 = null;
					_Valve.Cable5 = null;
					break;
				case Valve.ValveType.ThreePos:
					//_Valve.Cable2.Attrubute = Cable.CableAttribute.P;
					_Valve.Cable2 = null;
					_Valve.BlockName = "ED-220-3POS";
					_Valve.Cable3 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _Valve.Posname,
						ToBlockName = _Valve.BlockName,
						Description = "Индикация конечных положений клапана",
						WireNumbers = 2
					};

					_Valve.Cable4 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _Valve.Posname,
						ToBlockName = _Valve.BlockName,
						Description = "Концевой выключатель 1",
						WireNumbers = 3
					};
					_Valve.Cable5 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _Valve.Posname,
						ToBlockName = _Valve.BlockName,
						Description = "Концевой выключатель 2",
						WireNumbers = 3
					};
					_Valve.Cable3.MakeControlSortpriority(_Valve.SortPriority + "3");
					_Valve.Cable4.MakeControlSortpriority(_Valve.SortPriority + "4");
					_Valve.Cable5.MakeControlSortpriority(_Valve.SortPriority + "5");

					break;
			}
		}

		private void SetSensorType(ref SensorT sensor, object val)
		{
			if (sensor != null)
			{
				Sensor.SensorType sensorType = (Sensor.SensorType)val;
				switch (sensorType)
				{
					case Sensor.SensorType.Analogue:
						if (sensor == PS1)
						{
							PS1._SensorType = sensorType;

						}
						if (sensor == PS2)
						{
							PS2._SensorType = sensorType;
						}


						break;
					case Sensor.SensorType.Discrete:
						if (sensor == PS1)
						{
							PS1._SensorType = sensorType;

						}
						if (sensor == PS2)
						{
							PS2._SensorType = sensorType;

						}
						break;
					case Sensor.SensorType.No:
						sensor = null;
						break;

				}
			}


		}

		public IEnumerator<object> GetEnumerator()
		{
			yield return PS1;
			yield return PS2;
			yield return _Pump;
			yield return _Valve;

		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public List<dynamic> GetSensors()
		{
			return new List<dynamic>
			{
				PS1,
				PS2
			};

		}

		public WaterHeater()
		{

			comp = VentComponents.WaterHeater;
			ShemaASU = ShemaASU.CreateBaseShema(comp);
			ShemaASU.ShemaUp = "Supply_WaterHeater_Pump_3WayValve";
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
			Pump pump = new();
			_Pump = pump;
			HasTK = pump.HasTK;
			pump.Voltage = ElectroDevice._Voltage.AC220;
            _Valve = new Valve
            {
                _ValveType = Valve.ValveType.Analogue_0_10,
                Voltage = ElectroDevice._Voltage.AC24,
                Description = "Э/д клапана водяного нагревателя",
                SortPriority = "17",
				Power="10"
            };
            _Valve.Cable1 = new Cable
			{
				Attrubute = Cable.CableAttribute.PL,
				WriteBlock = true,
				ToPosName = _Valve.Posname,
				ToBlockName = _Valve.BlockName,
				SortPriority = _Valve.SortPriority,
				Description = _Valve.Description,
				WireNumbers = 3
			};
			_Valve.Cable2 = new Cable
			{
				Attrubute = Cable.CableAttribute.A,
				WriteBlock = false,
				ToPosName = _Valve.Posname,
				ToBlockName = _Valve.BlockName,
				Description = "Управление клапаном нагревателя",
				WireNumbers = 2
			};
			_Valve.Cable2.MakeControlSortpriority(_Valve.SortPriority + "2");
			_Valve.ShemaASU = ShemaASU.CreateBaseShema(VentComponents.WaterHeater);
			_Valve.ShemaASU.SetIO(ShemaASU.IOType.AO, 1);
			_Valve.ShemaASU.ShemaUp = "WaterHeater_Valve";
			_Valve.ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
			_Valve.ShemaASU.ShemaPos = ShemaASUbase.DummyPos(VentComponents.WaterHeater);
			








			valveType = _Valve._ValveType;
			Waterprotect = WaterHeaterProtect.No;

			ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
			ShemaASU.SetIO(ShemaASU.IOType.AO, 1);

		}
	}
}
