using System;
using System.ComponentModel;

namespace AOVGEN.Models
{
    [Serializable]
	class IndoorTemp : SensorT
    {
		internal new string SortPriority => sortpriority;
		[DisplayName("Тип элемента")]
		public VentComponents comp
		{
			get => ComponentVariable;
			internal set => ComponentVariable = value;
		}
		public new SensorType _SensorType
		{
			get => sensorType;
			set
			{
				sensorType = value;
				switch (sensorType)
				{
					case SensorType.Analogue:
						PosName = "TE";
						Blockname = "SENS-TE-2WIRE";
						sortpriority = "25";
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
						if (ShemaASU != null)
						{
							ShemaASU.ReSetAllIO();
							ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
						}
						else
						{
							ShemaASU = ShemaASU.CreateBaseShema(VentComponents.IndoorTemp);
							ShemaASU.ShemaUp = "Temp_Indoor";
							ShemaASU.SetIO(ShemaASU.IOType.AI, 1);

						}
						break;

					case SensorType.Discrete:
						PosName = "TS";
						Blockname = "SENS-TS-2WIRE";
						sortpriority = "32";
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
						if (ShemaASU != null)
						{
							ShemaASU.ReSetAllIO();
							ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
						}
						else
						{
							ShemaASU = ShemaASU.CreateBaseShema(VentComponents.IndoorTemp);
							ShemaASU.ShemaUp = "Temp_Indoor";
							ShemaASU.SetIO(ShemaASU.IOType.DI, 1);

						}
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
		}
		internal override void CheckSensorType(object val)
		{
			sensorType = (SensorType)val;
			switch (sensorType)
			{
				case SensorType.Analogue:
					PosName = "TE";
					Blockname = "SENS-TE-2WIRE";
					sortpriority = "25";
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
					break;
				case SensorType.Discrete:
					PosName = "TS";
					Blockname = "SENS-TS-2WIRE";
					sortpriority = "32";
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
			}
			//return sensorType;

		}
		public IndoorTemp()
		{
			ComponentVariable = VentComponents.IndoorTemp;
			Description = "Датчик температуры в помещении";
			_SensorType = SensorType.Analogue;
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
			ShemaASU.ShemaUp = "Temp_Indoor";
			ShemaASU.ShemaPos = ShemaASUbase.DummyPos(ComponentVariable);
			ShemaASU.ScheUpSize = ShemaASUbase.DummySize(ComponentVariable);
			ShemaASU.Description1 = Description;
		}
	}
}
