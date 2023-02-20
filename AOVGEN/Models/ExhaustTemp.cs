using System;
using System.ComponentModel;

namespace AOVGEN.Models
{
    [Serializable]
    class ExhaustTemp : SensorT
	{
		internal override string SortPriority => sortpriority;
		[DisplayName("Тип элемента")]
		public VentComponents comp
		{
			get => ComponentVariable;
			internal set => ComponentVariable = value;
		}
		internal override void CheckSensorType(object val)
		{
			switch (sensorType)
			{
				case SensorType.Analogue:
					PosName = "TE";
					Blockname = "SENS-TE-2WIRE";
					sortpriority = "24";
					SetVendorInfo(null, null, null, "SensTE", null);
					MainDBTable = "SensT";
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
					ShemaASU.ReSetAllIO();
					ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
					break;
				case SensorType.Discrete:
					PosName = "TS";
					Blockname = "SENS-TS-2WIRE";
					sortpriority = "31";
					SetVendorInfo(null, null, null, "SensTS", null);
					MainDBTable = "SensT";
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
					}
					ShemaASU.ReSetAllIO();
					ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
					break;
				case SensorType.No:
					SetVendorInfo(null, null, null, null, null);
					PosName = string.Empty;
					Blockname = string.Empty;
					Cable1 = null;
					ShemaASU = null;
					break;
			}

		}
		public ExhaustTemp()
		{
			ComponentVariable = VentComponents.ExtTemp;
			Description = "Датчик температуры в вытяжном канале";
			_SensorType = SensorType.Analogue;
			ShemaASU.ShemaUp = "Temp_Exihaust";
			ShemaASU.ShemaPos = ShemaASUbase.DummyPos(ComponentVariable);
			ShemaASU.Description1 = Description;

		}
	}
}
