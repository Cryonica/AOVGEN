using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AOVGEN.Models
{
    [Serializable]
    class Filtr : ICompGUID, IGetSensors
    {
		private protected VentComponents ComponentVariable;
		internal ShemaASU ShemaASU;
		[TypeConverter(typeof(EnumDescConverter))]
		private Sensor.SensorType PressureProtectVariable;

		internal PressureContol _PressureContol;
		[DisplayName("Тип элемента")]
		public VentComponents comp => ComponentVariable;

		[DisplayName("Датчик перепада давления")]
		public Sensor.SensorType PressureProtect
		{
			get => PressureProtectVariable;
			set => CheckPressureType(value);
		}
		internal string Description { get; set; }

		[DisplayName("Обозначение")]
		public string PosName { get; internal set; }

		[DisplayName("Имя блока")]
		public string Blockname { get; internal set; }
		public string GUID { get; set; }
		private void CheckPressureType(object val)
		{
			PressureProtectVariable = (Sensor.SensorType)val;
			if (PressureProtectVariable != Sensor.SensorType.No)
			{
				MakePressureSensor(PressureProtectVariable);
			}
			else
			{
				Blockname = string.Empty;
				PosName = string.Empty;
			}

			//return PressureProtectVariable;
		}
		private void MakePressureSensor(Sensor.SensorType protect)
		{
			_PressureContol = null;
			switch (protect)
			{
				case Sensor.SensorType.Analogue:

					_PressureContol = new PressureContol
					{
						_SensorType = Sensor.SensorType.Analogue
					};
					_PressureContol.ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
					break;
				case Sensor.SensorType.Discrete:
					_PressureContol = new PressureContol
					{
						_SensorType = Sensor.SensorType.Discrete
					};
					_PressureContol.ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
					break;
			}

			if (_PressureContol == null) return;
			_PressureContol.CheckSensorType(protect);
			switch (comp)
			{
				case VentComponents.SupplyFiltr:
					if (_PressureContol != null)
					{
						_PressureContol.Description = "Контроль чистоты приточного фильтра";
						_PressureContol.ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
						_PressureContol.ShemaASU.ShemaUp = "Supply_Filtr_PD";
						_PressureContol.ShemaASU.ShemaPos = ShemaASUbase.DummyPos(comp);
					}

					break;
				case VentComponents.ExtFiltr:
					if (_PressureContol != null)
					{
						_PressureContol.Description = "Контроль чистоты вытяжного фильтра";
						_PressureContol.ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Exhaust;
						_PressureContol.ShemaASU.ShemaUp = "Ext_Filtr_PD";
						_PressureContol.ShemaASU.ShemaPos = ShemaASUbase.DummyPos(comp);
					}

					break;
				default:
					_PressureContol.Description = "Контроль чистоты фильтра";
					_PressureContol.ShemaASU.ShemaUp = "Supply_Filtr_PD";
					break;
			}

			if (_PressureContol == null) return;
			Blockname = _PressureContol.Cable1.ToBlockName;
			PosName = _PressureContol.Cable1.ToPosName;
			_PressureContol.SetDescrtiption(_PressureContol.Description);
		}
		public List<dynamic> GetSensors()
		{
			return new List<dynamic>
			{
				_PressureContol
			};

		}
		public Filtr() //это базовый класс фильтр
		{
			ComponentVariable = VentComponents.Filtr;
			ShemaASU = ShemaASU.CreateBaseShema(comp);
			ShemaASU.ShemaUp = "Filtr";
			Description = "Фильтр";
			PressureProtect = Sensor.SensorType.Discrete;
		}
	}
}
