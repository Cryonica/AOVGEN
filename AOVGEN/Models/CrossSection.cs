using System;
using System.ComponentModel;
namespace AOVGEN.Models
{
	[Serializable]
	class CrossSection:Dummy
    {
		internal override void SetBlockName(string image)
		{
			ShemaASU = ShemaASU.CreateBaseShema();
			switch (image)
			{

				case "cross1T":
				case "cross1TH":
				case "cross1":
					ShemaASU.ShemaUp = "CrossSection";
					break;
				case "cross2":
					ShemaASU.ShemaUp = "CrossSection2";
					break;
				case "cross3":
					ShemaASU.ShemaUp = "CrossSection3";
					break;
				case "cross4":
					ShemaASU.ShemaUp = "CrossSection4";
					break;
				case "cross5":
					ShemaASU.ShemaUp = "CrossSection5";
					break;
				case "cross6":
					ShemaASU.ShemaUp = "CrossSection6";
					break;
				case "cross7":
					ShemaASU.ShemaUp = "CrossSection7";
					break;
				case "cross8":
					ShemaASU.ShemaUp = "CrossSection8";
					break;
				case "cross9":
					ShemaASU.ShemaUp = "CrossSection9";
					break;
				case "cross10":
					ShemaASU.ShemaUp = "CrossSection10";
					break;
				case "cross11":
					ShemaASU.ShemaUp = "CrossSection11";
					break;
			}
		}
		internal override void SetBlockName()
		{
			ShemaASU = ShemaASU.CreateBaseShema();
			ShemaASU.ShemaUp = "CrossSection";
		}
        internal override void EnableSensorT()
        {
			_SensorT = new SensorT
			{
				Description = "Датчик температуры в канале",
				_SensorType = Sensor.SensorType.Analogue
			};
			SensorTType = _SensorT._SensorType;
			blockNameT = _SensorT.Blockname;
		}
		internal override void EnableSensorH()
		{
			_SensorH = new HumiditySens
			{
				Description = "Датчик влажности в канале",
				_SensorType = Sensor.SensorType.Analogue
			};
			SensorHType = _SensorH._SensorType;
			_SensorH.SensorInsideCrossSection = true;
			blockNameH = _SensorH.Blockname;
		}
		public CrossSection(bool sensT, bool sensH) : base(sensT, sensH) { }
		public CrossSection(bool sensT, bool sensH, string imagename) : base(sensT, sensH, imagename) { }
		public CrossSection() { }
	}
}
