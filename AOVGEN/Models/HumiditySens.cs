using System;

namespace AOVGEN.Models
{
	[Serializable]
	class HumiditySens : Sensor
    {
		internal bool insideHum;
		internal override void CheckSensorType(object val)
		{
			sensorType = (SensorType)val;
			if (ShemaASU == null) ShemaASU = ShemaASU.CreateBaseShema();
			switch (sensorType)
			{
				case SensorType.Analogue:
					SetVendorInfo(null, null, null, "SensHE", null);
					MainDBTable = "SensHUM";
					PosName = "ME";
					Blockname = "SENS-ME-2WIRE";
					sortpriority = "21";
					if (Cable1 != null) Cable1.Attrubute = Cable.CableAttribute.A;
					else
					{
						Cable1 = new Cable
						{
							Attrubute = Cable.CableAttribute.A,
							Description = Description,
							WireNumbers = 2,
							WriteBlock = true
						};

					}
					ShemaASU?.ReSetIO(ShemaASU.IOType.DI);
					ShemaASU?.SetIO(ShemaASU.IOType.AI, 1);
					SensorInsideCrossSection = false;


					break;
				case SensorType.Discrete:
					SetVendorInfo(null, null, null, "SensHS", null);
					MainDBTable = "SensHUM";
					PosName = "MS";
					Blockname = "SENS-MS-2WIRE";
					sortpriority = "28";
					if (Cable1 != null) Cable1.Attrubute = Cable.CableAttribute.D;
					else
					{
						Cable1 = new Cable
						{
							Attrubute = Cable.CableAttribute.D,
							Description = Description,
							WireNumbers = 2,
							WriteBlock = true
						};
					}
					ShemaASU?.ReSetIO(ShemaASU.IOType.AI);
					ShemaASU?.SetIO(ShemaASU.IOType.DI, 1);
					SensorInsideCrossSection = false;

					break;
				case SensorType.No:

					PosName = string.Empty;
					Blockname = string.Empty;
					Cable1 = null;
					ShemaASU = null;
					break;

			}
			if (Cable1 != null)
			{
				Cable1.SortPriority = sortpriority;
				Cable1.ToBlockName = Blockname;
				Cable1.ToPosName = PosName;
				Cable1.WireNumbers = 2;
				Cable1.WriteBlock = true;
			}
		}
		private bool sensorInsideCrossSection;
		internal bool SensorInsideCrossSection
		{
			get => sensorInsideCrossSection;
			set
			{
				sensorInsideCrossSection = value;
				if (value)
				{
					if (ShemaASU != null) ShemaASU.ShemaUp = "Hum_CrossSection";

				}
				else
				{
					if (ShemaASU != null)
					{
						ShemaASU.ShemaUp = insideHum ? "Humidifier_Hygrostat" : "Hum_Room";
					}
				}

			}
		}
		public HumiditySens()
		{
			Description = "Капилярный гиростат";

		}
	}
}
