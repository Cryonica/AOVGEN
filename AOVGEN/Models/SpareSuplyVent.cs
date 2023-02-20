using System;
using System.Collections.Generic;
using System.ComponentModel;
using static AOVGEN.Models.ElectroDevice;

namespace AOVGEN.Models
{
    [Serializable]
    class SpareSuplyVent : IPower, ICompGUID, IGetSensors
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
                        if (MainSupplyVent._FControl != null) MainSupplyVent._FControl.Description = "Регулятор оборотов основного приточного вентилятора";
                        if (ReservedSupplyVent._FControl != null) ReservedSupplyVent._FControl.Description = "Регулятор оборотов резервного приточного вентилятора";
                        break;
                    case Vent.AHUContolType.FCControl:
                        Voltage = _Voltage.AC220;
                        break;
                }
                MainSupplyVent.ControlType = contolType;
                ReservedSupplyVent.ControlType = contolType;
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
                        MainSupplyVent.Voltage = voltage;
                        ReservedSupplyVent.Voltage = voltage;
                        break;
                    default:
                        voltage = value;
                        MainSupplyVent.Voltage = voltage;
                        ReservedSupplyVent.Voltage = voltage;
                        break;
                }

                switch (ControlType)
                {
                    case Vent.AHUContolType.Soft:
                    case Vent.AHUContolType.Transworm:
                        voltage = _Voltage.AC380;
                        MainSupplyVent.Voltage = voltage;
                        ReservedSupplyVent.Voltage = voltage;
                        break;
                    case Vent.AHUContolType.FCControl:
                        voltage = _Voltage.AC220;
                        MainSupplyVent.Voltage = voltage;
                        ReservedSupplyVent.Voltage = voltage;
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
                MainSupplyVent.Power = power;
                ReservedSupplyVent.Power = power;
            }
        }
        [DisplayName("Защита двигателя")]
        public Vent.AHUProtect Protect
        {
            get => protect;
            set
            {
                protect = value;
                MainSupplyVent.Protect = protect;
                ReservedSupplyVent.Protect = protect;

            }
        }
        [DisplayName("Имя блока основного")]
        public string BlockNameMain => MainSupplyVent.BlockName;
        [DisplayName("Имя блока резевного")]
        public string BlockNameReserved => ReservedSupplyVent?.BlockName;
        public string GUID { get; set; }
        internal SupplyVent MainSupplyVent;
        internal SupplyVent ReservedSupplyVent;
        internal ShemaASU shemaAsu;
        internal PressureContol _PressureContol;
        internal static string Description = "Сдвоенный приточный вентилятор";
        public SpareSuplyVent()
        {
            comp = VentComponents.SpareSuplyVent;
            power = "0";
            Location = "Supply";

            MainSupplyVent = new SupplyVent(false, true) //1 вентилятор
            {
                Voltage = _Voltage.AC380,
                PressureProtect = Vent._PressureProtect.None,
                Description = "Э/д основного приточного вентилятора",
                Location = Location,
                AttributeSpare = "Main"
            };
            ReservedSupplyVent = new SupplyVent(true, true) //2 вентилятор
            {
                Voltage = _Voltage.AC380,
                PressureProtect = Vent._PressureProtect.None,
                Description = "Э/д резервного приточного вентилятора",
                Location = Location,
                AttributeSpare = "Reserved"
            };

            shemaAsu = new ShemaASU
            {
                ShemaUp = "Supply_Vent_Spare"
            };

            _PressureContol = new PressureContol //датчик Р
            {
                _SensorType = Sensor.SensorType.Discrete,
                Description = "Датчик перепада давления на сдвоенном приточном вентиляторе"
            };
            _PressureContol.Cable1.Description = _PressureContol.Description;
        }
        public List<dynamic> GetSensors()
        {
            return new List<dynamic>
            {
                _PressureContol,
                MainSupplyVent._FControl,
                ReservedSupplyVent._FControl
            };


        }
    }
}
