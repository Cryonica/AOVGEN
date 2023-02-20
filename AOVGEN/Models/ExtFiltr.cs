using System;
namespace AOVGEN.Models
{
    [Serializable]
    class ExtFiltr : Filtr
    {
		public static string Imagepath = VentSystem.AppFolder + "Images\\5_1.png";
		public static string ImageNullPath = VentSystem.AppFolder + "Images\\5_0.png";
		public ExtFiltr()
		{
			ComponentVariable = VentComponents.ExtFiltr;
			Description = "Вытяжной фильтр";
			ShemaASU = ShemaASU.CreateBaseShema(comp);
			ShemaASU.ShemaUp = "Filtr";
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Exhaust;
			PressureProtect = Sensor.SensorType.Discrete;

			ShemaASU.ShemaUp = "Filtr";

		}
	}
}
