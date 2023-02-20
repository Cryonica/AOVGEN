using System;

namespace AOVGEN.Models
{
    [Serializable]
    class SupplyFiltr: Filtr
    {
		public static string Imagepath = VentSystem.AppFolder + "Images\\2_1.png";
		public static string ImageNullPath = VentSystem.AppFolder + "Images\\2_0.png";
		public SupplyFiltr()
		{
			ComponentVariable = VentComponents.SupplyFiltr;
			ShemaASU = ShemaASU.CreateBaseShema(comp);
			ShemaASU.ShemaUp = "Filtr";
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;

			Description = "Приточный фильтр";

			PressureProtect = Sensor.SensorType.Discrete;
		}


	}
}
