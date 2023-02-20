using System;

namespace AOVGEN.Models
{
    [Serializable]
	abstract class ShemaASUbase : ISchemaASU
    {
		public enum ComponentPlace
		{
			Supply,
			Exhaust
		}
		public double ScheUpSize { get; set; }
		public int ShemaPos { get; set; }
		public string ShemaUp { get; set; }
		public ComponentPlace componentPlace { get; set; }
		public static double DummySize(VentComponents components)
		{
			switch (components)
			{
				case VentComponents.SupplyVent:
				case VentComponents.ExtVent:

					return 29.54;
				case VentComponents.SupplyFiltr:
				case VentComponents.ExtFiltr:
					return 29.85;
				case VentComponents.SupplyDamper:
				case VentComponents.ExtDamper:
					return 10;
				case VentComponents.WaterHeater:
					return 78.43;
				case VentComponents.ElectroHeater:
					return 24.48;
				case VentComponents.Froster:
					return 45.02;
				case VentComponents.Humidifier:
					return 34.48;
				case VentComponents.Recuperator:
					return 54.13;
				case VentComponents.OutdoorTemp:
				case VentComponents.SupplyTemp:
				case VentComponents.ExtTemp:
				case VentComponents.IndoorTemp:
					return 10.09;
				default:
					return 0;
			}
		}
		public static int DummyPos(VentComponents components)
		{
			switch (components)
			{
				case VentComponents.SupplyVent:
				case VentComponents.ExtVent:
					return 5;
				case VentComponents.SupplyFiltr:
					return 3;
				case VentComponents.ExtFiltr:
					return 11;
				case VentComponents.SupplyDamper:
				case VentComponents.ExtDamper:
					return 2;
				case VentComponents.WaterHeater:
					return 6;
				case VentComponents.ElectroHeater:
					return 7;
				case VentComponents.Froster:
					return 8;
				case VentComponents.Humidifier:
					return 9;
				case VentComponents.Recuperator:
					return 4;
				case VentComponents.OutdoorTemp:
					return 1;
				case VentComponents.SupplyTemp:
				case VentComponents.ExtTemp:
					return 10;
				case VentComponents.IndoorTemp:
					return 13;

				default:
					return 0;
			}
		}
	}
}
