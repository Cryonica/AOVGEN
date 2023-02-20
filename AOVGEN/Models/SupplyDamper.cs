using System;
using System.ComponentModel;

namespace AOVGEN.Models
{
    [Serializable]
	class SupplyDamper : Damper, IPower
    {
		public static string ImagePath = VentSystem.AppFolder + "Images\\3_1.png";
		public static string ImageNullPath = VentSystem.AppFolder + "Images\\3_0.png";
		internal OutdoorTemp outdoorTemp;
		private Sensor.SensorType sensorType;
		[DisplayName("Тип элемента")]
		public VentComponents comp
		{
			get => ComponentVariable;
			internal set => ComponentVariable = value;
		}
		private VentComponents ComponentVariable;
		public SupplyDamper()
		{

			comp = VentComponents.SupplyDamper;
			ShemaASU = ShemaASU.CreateBaseShema(comp);
			ShemaASU.ShemaUp = "Supply_Damper";
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
			ShemaASU.ShemaPos = ShemaASUbase.DummyPos(comp);
			ShemaASU.ScheUpSize = ShemaASUbase.DummySize(comp);
			PosName = "M";
			VoltageVariable = _Voltage.AC220;
			Power = "3";
			Description1 = "Э/д приточной заслонки";
			Cable1 = new Cable
			{
				Attrubute = Cable.CableAttribute.P,
				WriteBlock = true,
				ToPosName = PosName,
				ToBlockName = BlockName,
				SortPriority = SortPriority,
				Description = Description1,
				WireNumbers = 3

			};



			ShemaASU.SetIO(ShemaASU.IOType.DO, 1);



		}
		[DisplayName("Возвратная пружина")]
		public bool Spring { get; set; }
        [DisplayName("Концевые выключатели")]
        public bool HasControl
        {
            get => hascontrol;
            set => SetCable2Type(value);
        }
        [DisplayName("Тип датчика температуры")]
		public Sensor.SensorType SensorType
		{
			get => sensorType;
			internal set => SetSensorType(value);
		}
		[DisplayName("Имя блока датчика температуры")]
		public string SensorBlockName => outdoorTemp?.Blockname;

		internal bool SetSensor { set => CreateSensor(); }
		private void SetSensorType(object val)
		{
			Sensor.SensorType _sensorType = (Sensor.SensorType)val;
			if (sensorType != Sensor.SensorType.No && outdoorTemp == null) outdoorTemp = new();
			if (outdoorTemp != null)
			{
				outdoorTemp._SensorType = _sensorType;
			}
			sensorType = _sensorType;
		}
		private void CreateSensor()
		{
			if (outdoorTemp == null)
			{
				outdoorTemp = new OutdoorTemp
				{
					_SensorType = Sensor.SensorType.Analogue,
					Location = "Outdoor"
				};
				sensorType = outdoorTemp._SensorType;


			}
		}

		internal override string MakeDamperBlockName()
		{
			string blockname;
			if (HasControl)
			{
				blockname = "ZASL-220_SQ";
				Description2 = "Контроль положения приточной заслонки";
			}
			else
			{
				blockname = "ZASL-220";
			}
			return blockname;
		}
		internal string SortPriority => MakeSortPriority();
		private string MakeSortPriority()
		{
			switch (Voltage)
			{
				case _Voltage.AC220:
					sortpriority = "12";
					break;
				default:
					sortpriority = "15";
					break;

			}
			if (Cable1 != null) Cable1.SortPriority = sortpriority;
			if (Cable2 != null) Cable2.MakeControlSortpriority(sortpriority);
			return sortpriority;

		}
	}
}
