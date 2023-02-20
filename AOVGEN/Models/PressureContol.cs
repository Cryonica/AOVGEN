using System;

namespace AOVGEN.Models
{
    [Serializable]
    class PressureContol : Sensor
    {
		internal override string SortPriority => MakeSortPriority(sensorType);
		private string MakeSortPriority(SensorType sensorType)
		{
			switch (sensorType)
			{
				case SensorType.Analogue:
					sortpriority = "27";
					if (Cable1 == null)
					{
						Cable1 = new Cable
						{
							Attrubute = Cable.CableAttribute.A,
							SortPriority = sortpriority,
							ToPosName = PosName,
							ToBlockName = Blockname,
							WriteBlock = true,
							Description = Description,
							WireNumbers = 2
						};
					}
					else
					{
						Cable1.Attrubute = Cable.CableAttribute.A;
						Cable1.SortPriority = sortpriority;
						Cable1.ToPosName = PosName;
						Cable1.ToBlockName = Blockname;
					}
					break;
				case SensorType.Discrete:
					sortpriority = "34";
					if (Cable1 == null)
					{
						Cable1 = new Cable
						{
							Attrubute = Cable.CableAttribute.D,
							SortPriority = sortpriority,
							ToPosName = PosName,
							ToBlockName = Blockname,
							WriteBlock = true,
							Description = Description,
							WireNumbers = 2
						};
					}
					else
					{
						Cable1.Attrubute = Cable.CableAttribute.D;
						Cable1.SortPriority = sortpriority;
						Cable1.ToPosName = PosName;
						Cable1.ToBlockName = Blockname;
						Cable1.WireNumbers = 2;
					}
					break;
				case SensorType.No:
					Cable1 = null;
					break;
			}
			return sortpriority;


		}
		internal override void CheckSensorType(object val)
		{

			sensorType = (SensorType)val;
			switch (sensorType)
			{
				case SensorType.Analogue:
					SetVendorInfo(null, null, null, "SensPE", null);
					MainDBTable = "SensPDS";
					PosName = "PE";
					Blockname = "SENS-PE-2WIRE";
					if (Cable1 == null)
						Cable1 = new Cable
						{
							SortPriority = SortPriority,
							WireNumbers = 2,
							WriteBlock = true,
							Attrubute = Cable.CableAttribute.A,
							Description = Description
						};
					break;
				case SensorType.Discrete:
					SetVendorInfo(null, null, null, "SensPS", null);
					MainDBTable = "SensPDS";
					PosName = "PDS";
					Blockname = "SENS-PDS-2WIRE";
					if (Cable1 == null) Cable1 = new Cable
					{
						SortPriority = SortPriority,
						WireNumbers = 2,
						WriteBlock = true,
						Attrubute = Cable.CableAttribute.D,
						Description = Description

					};

					break;
				case SensorType.No:
					PosName = string.Empty;
					Blockname = string.Empty;
					Cable1 = null;
					break;
			}
			if (Cable1 != null)
			{
				Cable1.SortPriority = sortpriority;
				Cable1.ToBlockName = Blockname;
				Cable1.ToPosName = PosName;
				Cable1.WriteBlock = true;
				Cable1.WireNumbers = 2;
			}
			//return sensorType;

		}
		internal void SetDescrtiption(string descriptiontext)
		{
			if (Cable1 != null)
			{
				Description = descriptiontext;
				Cable1.Description = Description;
			}
		}
		public PressureContol()
		{
			ShemaASU = ShemaASU.CreateBaseShema();
			_SensorType = SensorType.Discrete;
		}
	}
}
