using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AOVGEN.Models
{
	[Serializable]
	class Humidifier : ElectroDevice, IPower, ICompGUID, IGetSensors
    {
		#region Pictures Path
		public static string ImageNullPath = VentSystem.AppFolder + "Images\\11_0.png";
		public static string Imagepath = VentSystem.AppFolder + "Images\\11_1.png";
		#endregion
		#region Enum
		[TypeConverter(typeof(EnumDescConverter))]
		public enum _HumType
		{
			[Description("Пароувлажнитель")]
			Steam,
			[Description("Сотовый увлажнитель")]
			Honeycomb,
			[Description("Камера орошения")]
			Irrigation_chamber
		}
		[TypeConverter(typeof(EnumDescConverter))]
		public enum HumContolType
		{
			[Description("Аналоговое")]
			Analogue,
			[Description("Дискретное")]
			Discrete
		}
		#endregion
		#region Variabes
		private VentComponents ComponentVariable;
		private _HumType humtype;

		private bool senspresent;
		private string sensblockname;
		private string sensposkname;

		private HumContolType humContol;
		private Sensor.SensorType sensortype;
		private HumiditySens _HumiditySens;
		internal Cable Cable2;
		internal Cable Cable3;
		internal ShemaASU ShemaASU;
		internal HumiditySens HumiditySensor
		{
			get => _HumiditySens;
			set => _HumiditySens = value;
		}
		#endregion
		#region Properties
		[DisplayName("Тип элемента")]
		public VentComponents comp
		{
			get => ComponentVariable;
			private set => ComponentVariable = value;
		}
		[DisplayName("Тип увлажнителя")]
		public _HumType HumType
		{
			get => humtype;
			set => humtype = value;
		}
		[DisplayName("Тип управления")]
		public HumContolType HumControlType
		{
			get => humContol;
			set => CheckHumContol(value);
		}
		[DisplayName("Напряжение")]
		public override _Voltage Voltage
		{
			get => VoltageVariable;
			set => CheckVoltage(value);
		}
		[DisplayName("Мощность")]
		public new string Power { get; set; }

		[DisplayName("Гигростат в канале")]
		//public bool HumSensPresent { get { return MakeSens(ref _HumiditySens, ref sensblockname, ref sensposkname); } set { CheckSens(value); } }
		public bool HumSensPresent
		{
			get => senspresent;
			//return MakeSens(ref _HumiditySens, ref sensblockname, ref sensposkname); 
			set => CheckSens(value);
		}
		[DisplayName("Тип гигростата")]
		public Sensor.SensorType SensorType
		{
			get => sensortype;
			set => SetSensorType(_HumiditySens, value, ref sensposkname);
		}
		[DisplayName("Обозначение датчика")]

		//public string SensPosName { get { return SetPosName(_HumiditySens, sensortype); } internal set { } }
		public string SensPosName
		{
			get => GetPosName();
			internal set => SetPosName(value);
		}
		[DisplayName("Имя блока датчика")]
		public string SensBlockName => GetBlockName(_HumiditySens, sensortype);

		[DisplayName("Поз.обозначение")]
		public string PosName => MakePosName();

		[DisplayName("Имя блока увлажнителя")]
		public string BlockName => MakeBockName(VoltageVariable);
		
		public string GUID { get; set; }
		internal double Displacement { get; set; }
		#endregion
		#region Methods
		private string MakeBockName(_Voltage voltage)
		{
			string blockname;
			switch (voltage)
			{
				case (_Voltage.AC380):
					blockname = "ED-380-HC";
					if (Cable1 != null) Cable1.WireNumbers = 5;
					break;
				default:
					blockname = "ED-220-HC";
					if (Cable1 != null) Cable1.WireNumbers = 3;
					break;

			}
			if (Cable1 != null) Cable1.ToBlockName = blockname;
			if (Cable2 != null) Cable2.ToBlockName = blockname;
			if (Cable3 != null) Cable3.ToBlockName = blockname;
			return blockname;

		}
		private string MakePosName()
		{
			return "HUM";
		}
		private void MakeSens(ref HumiditySens humiditySens, ref string sensname, ref string senposname)
		{
			humiditySens = null;
			sensname = "null";
			senposname = "null";
			if (senspresent)
			{
				humiditySens = new HumiditySens
				{
					Description = "Капилярный гигростат в канале",
					insideHum = true
				};
				sensname = humiditySens.Blockname;
				senposname = humiditySens.PosName;
				senspresent = true;
				humiditySens.ShemaASU = ShemaASU.CreateBaseShema(comp);
				humiditySens.ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
				humiditySens.ShemaASU.ShemaUp = "Humidifier_Hygrostat";

			}
			else
			{
				sensortype = Sensor.SensorType.No;
			}
		}
		private void SetSensorType(HumiditySens huniditySens, object val, ref string sensposname)
		{
			sensortype = Sensor.SensorType.No;
			if (huniditySens != null)
			{
				sensortype = (Sensor.SensorType)val;
				huniditySens._SensorType = sensortype;
				sensposname = huniditySens.Blockname;
			}
		}
		private string GetBlockName(HumiditySens humiditySens, Sensor.SensorType sensorType)
		{
			string ret = "null";
			if (humiditySens != null)
			{
				humiditySens._SensorType = sensorType;
				ret = humiditySens.Blockname;
			}
			return ret;
		}
		private string SetPosName(string value)
		{
			string ret = "null";

			if (_HumiditySens != null)
			{

				_HumiditySens.PosName = value;

				ret = _HumiditySens.PosName;
			}
			return ret;
		}
		private string GetPosName()
		{
			if (_HumiditySens != null)
			{
				return _HumiditySens.PosName;
			}
			return string.Empty;
		}
		internal string SortPriority
		{
			get => sortpriority;
			set => MakeSortPriority(Voltage);
		}
		private string MakeSortPriority(_Voltage voltage)
		{
			sortpriority = string.Empty;
			switch (voltage)
			{
				case _Voltage.AC380:
					sortpriority = "04";
					break;
				default:
					sortpriority = "09";
					break;

			}
			if (Cable1 != null) Cable1.SortPriority = sortpriority;
			if (Cable2 != null) Cable2.MakeControlSortpriority(sortpriority + "2");
			if (Cable3 != null) Cable3.MakeControlSortpriority(sortpriority + "3");

			return sortpriority;

		}
		private void CheckHumContol(object val)
		{
			humContol = (HumContolType)val;
			switch (humContol)
			{
				case HumContolType.Analogue:
					if (Cable2 != null) Cable2.Attrubute = Cable.CableAttribute.A;
					ShemaASU.ReSetIO(ShemaASU.IOType.DO);
					ShemaASU.SetIO(ShemaASU.IOType.AO, 1);
					break;
				case HumContolType.Discrete:
					if (Cable2 != null) Cable2.Attrubute = Cable.CableAttribute.D;
					ShemaASU.ReSetIO(ShemaASU.IOType.AO);
					ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
					break;
			}
		}
		private void CheckVoltage(object val)
		{
			VoltageVariable = (_Voltage)val;
			MakeSortPriority(VoltageVariable);
		}
		private void CheckSens(object val)
		{
			senspresent = Convert.ToBoolean(val);
			if (senspresent)
			{
				MakeSens(ref _HumiditySens, ref sensblockname, ref sensposkname);
				_HumiditySens._SensorType = Sensor.SensorType.Discrete;

				SensorType = _HumiditySens._SensorType;

			}
			else
			{
				_HumiditySens = null;
				SensorType = Sensor.SensorType.No;
			}
		}
		public List<dynamic> GetSensors()
		{
			return new List<dynamic>
			{
				_HumiditySens
			};



		}
		#endregion

		public Humidifier()
		{
			Voltage = _Voltage.AC220;
			Displacement = -28.75646364;
			Power = "0";
			ComponentVariable = VentComponents.Humidifier;
			Description = "Увлажнитель в приточном канале";
			ShemaASU = ShemaASU.CreateBaseShema(ComponentVariable);
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
			ShemaASU.ShemaUp = "Supply_Humidifier";
			HumControlType = HumContolType.Analogue;
			ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
			Cable1 = new Cable
			{
				Attrubute = Cable.CableAttribute.P,
				Description = Description,
				ToBlockName = BlockName,
				ToPosName = PosName,
				SortPriority = SortPriority,
				WriteBlock = true,
				WireNumbers = 3
			};
			Cable2 = new Cable
			{
				Attrubute = Cable.CableAttribute.A,
				Description = "Управление увлажнителем",
				ToBlockName = BlockName,
				ToPosName = PosName,
				WriteBlock = false,
				WireNumbers = 2
			};
			Cable3 = new Cable
			{
				Attrubute = Cable.CableAttribute.D,
				Description = "Авария увлажнителя",
				ToBlockName = BlockName,
				ToPosName = PosName,
				WriteBlock = false,
				WireNumbers = 2
			};
			Cable2.MakeControlSortpriority(SortPriority + "2");
			Cable3.MakeControlSortpriority(SortPriority + "3");





		}
	}
}
