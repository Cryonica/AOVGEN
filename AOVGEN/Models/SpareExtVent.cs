using System;
using System.Collections.Generic;
using System.ComponentModel;
using static AOVGEN.Models.ElectroDevice;

namespace AOVGEN.Models
{
    [Serializable]
    class SpareExtVent : IPower, ICompGUID, IGetSensors
    {
		private _Voltage voltage;
		private string power;
		private Vent.AHUProtect protect;
		private Vent.AHUContolType contolType;
		internal string Location;

		[DisplayName("Тип элемента")]
		public VentComponents comp { get; internal set; }
		[DisplayName("Тип управления")]
		public Vent.AHUContolType ControlType
		{
			get => contolType;
			set
			{
				contolType = value;
				switch (contolType)
				{
					case Vent.AHUContolType.Soft:
					case Vent.AHUContolType.Transworm:
						Voltage = _Voltage.AC380;
						if (MainExtVent._FControl != null) MainExtVent._FControl.Description = "Регулятор оборотов основного вытяжного вентилятора";
						if (ReservedExtVent._FControl != null) ReservedExtVent._FControl.Description = "Регулятор оборотов резервного приточного вентилятора";
						break;
					case Vent.AHUContolType.FCControl:
						Voltage = _Voltage.AC220;
						break;
				}
				MainExtVent.ControlType = contolType;
				ReservedExtVent.ControlType = contolType;
			}
		}

		[DisplayName("Напряжение ")]
		public _Voltage Voltage
		{
			get => voltage;
			set
			{
				switch (value)
				{
					case _Voltage.AC24:
					case _Voltage.DC24:
						voltage = _Voltage.AC220;
						ReservedExtVent.Voltage = voltage;
						ReservedExtVent.Voltage = voltage;
						break;
					default:
						voltage = value;
						MainExtVent.Voltage = voltage;
						ReservedExtVent.Voltage = voltage;
						break;
				}

				switch (ControlType)
				{
					case Vent.AHUContolType.Soft:
					case Vent.AHUContolType.Transworm:
						voltage = _Voltage.AC380;
						MainExtVent.Voltage = voltage;
						ReservedExtVent.Voltage = voltage;
						break;
					case Vent.AHUContolType.FCControl:
						voltage = _Voltage.AC220;
						MainExtVent.Voltage = voltage;
						ReservedExtVent.Voltage = voltage;
						break;
				}
			}
		}
		[DisplayName("Мощность")]
		public string Power
		{
			get => power;
			set
			{
				power = double.TryParse(value, out double _) ? value : "0";
				MainExtVent.Power = power;
				ReservedExtVent.Power = power;
			}
		}

		[DisplayName("Защита двигателя")]
		public Vent.AHUProtect Protect
		{
			get => protect;
			set
			{
				protect = value;

				MainExtVent.Protect = protect;
				ReservedExtVent.Protect = protect;

			}
		}
		[DisplayName("Имя блока основного")]
		public string BlockNameMain => MainExtVent.BlockName;

		[DisplayName("Имя блока резевного")]
		public string BlockNameReserved => ReservedExtVent?.BlockName;
		public string GUID { get; set; }

		internal ExtVent MainExtVent;
		internal ExtVent ReservedExtVent;
		internal ShemaASU shemaAsu;
		internal PressureContol _PressureContol;
		internal static string Description = "Сдвоенный вытяжной вентилятор";


		public SpareExtVent()
		{
			comp = VentComponents.SpareExtVent;
			power = "0";
			Location = "Exhaust";

			MainExtVent = new ExtVent(false, true) //1 вентилятор
			{
				Voltage = _Voltage.AC380,
				PressureProtect = Vent._PressureProtect.None,
				Description = "Э/д основного вытяжного вентилятора",
				Location = Location,
				AttributeSpare = "Main"


			};
			ReservedExtVent = new ExtVent(true, true) //2 вентилятор
			{
				Voltage = _Voltage.AC380,
				PressureProtect = Vent._PressureProtect.None,
				Description = "Э/д резервного вытяжного вентилятора",
				Location = Location,
				AttributeSpare = "Reserved"
			};

			shemaAsu = new ShemaASU
			{
				ShemaUp = "Ext_Vent_Spare"
			};

			_PressureContol = new PressureContol //датчик Р
			{
				_SensorType = Sensor.SensorType.Discrete,
				Description = "Датчик перепада давления на сдвоенном вытяжном вентиляторе"
			};
			_PressureContol.Cable1.Description = _PressureContol.Description;
		}
		public List<dynamic> GetSensors()
		{
			return new List<dynamic>
			{
				_PressureContol,
				MainExtVent._FControl,
				ReservedExtVent._FControl
			};


		}
	}
}
