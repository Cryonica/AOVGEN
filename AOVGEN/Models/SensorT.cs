using System;
using System.Collections.Generic;

namespace AOVGEN.Models
{
	[Serializable]
	class SensorT : Sensor, IGetSensors, IVendoInfo
    {
		internal VentComponents ComponentVariable;
		internal override void CheckSensorType(object val)
		{
			sensorType = (SensorType)val;
			switch ((SensorType)val)
			{
				case SensorType.Analogue:

					PosName = "TE";
					Blockname = "SENS-TE-2WIRE";
					sortpriority = "26";

					SetVendorInfo(null, null, null, "SensTE", null);
					MainDBTable = "SensT";

					if (Cable1 != null) Cable1.Attrubute = Cable.CableAttribute.A;
					else
					{
						Cable1 = new Cable
						{
							Attrubute = Cable.CableAttribute.A,
							Description = Description,
							WriteBlock = true,
							WireNumbers = 2
						};
					}

					if (ShemaASU != null)
					{
						ShemaASU.ReSetAllIO();
						ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
					}
					else
					{
						ShemaASU = ShemaASU.CreateBaseShema();
						ShemaASU.SetIO(ShemaASU.IOType.AI, 1);

					}

					break;
				case SensorType.Discrete:
					SetVendorInfo(null, null, null, "SensTS", null);
					MainDBTable = "SensT";
					PosName = "TS";
					Blockname = "SENS-TS-2WIRE";
					sortpriority = "33";
					if (Cable1 != null) Cable1.Attrubute = Cable.CableAttribute.D;
					else
					{
						Cable1 = new Cable
						{
							Attrubute = Cable.CableAttribute.D,
							Description = Description,
							WriteBlock = true,
							WireNumbers = 2
						};
					}
					if (ShemaASU != null)
					{
						ShemaASU.ReSetAllIO();
						ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
					}
					else
					{
						ShemaASU = ShemaASU.CreateBaseShema();
						ShemaASU.SetIO(ShemaASU.IOType.DI, 1);

					}

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
			}
			//return sensorType;

		}
		public SensorT(string description)
		{
			Description = description;
			ShemaASU = ShemaASU.CreateBaseShema();
			ShemaASU.ShemaUp = "Temp_NoPos";
			ShemaASU.Description1 = Description;
		}
		public SensorT()
		{
			ShemaASU = ShemaASU.CreateBaseShema();
			ShemaASU.ShemaUp = "Temp_NoPos";
		}
		public List<dynamic> GetSensors()
		{
			return new List<dynamic> { this };
		}
	}
}
