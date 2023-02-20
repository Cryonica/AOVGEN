using System;
using System.ComponentModel;

namespace AOVGEN.Models
{
    [Serializable]
    class OutdoorTemp : SensorT
    {
		//private VentComponents ComponentVariable;
		internal new string SortPriority => sortpriority;
		[DisplayName("Тип элемента")]
		public VentComponents comp
		{
			get => ComponentVariable;
			internal set => ComponentVariable = value;
		}
		internal override void CheckSensorType(object val)
		{
			sensorType = (SensorType)val;
			switch (sensorType)
			{
				case SensorType.Analogue:
					PosName = "TE";
					Blockname = "SENS-TE-2WIRE";
					sortpriority = "22";
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
						ShemaASU = ShemaASU.CreateBaseShema(VentComponents.OutdoorTemp);
						ShemaASU.ShemaUp = "Temp_Supply";
						ShemaASU.SetIO(ShemaASU.IOType.AI, 1);

					}


					break;
				case SensorType.Discrete:
					PosName = "TS";
					Blockname = "SENS-TS-2WIRE";
					sortpriority = "29";
					SetVendorInfo(null, null, null, "SensTS", null);
					MainDBTable = "SensT";
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
						ShemaASU = ShemaASU.CreateBaseShema(VentComponents.OutdoorTemp);
						ShemaASU.ShemaUp = "Temp_Supply";
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
			if (Cable1 != null)
			{
				Cable1.SortPriority = sortpriority;
				Cable1.ToBlockName = Blockname;
				Cable1.ToPosName = PosName;
			}
			
		}
		public OutdoorTemp()
		{
			ComponentVariable = VentComponents.OutdoorTemp;
			Description = "Датчик наружной температуры";
			_SensorType = SensorType.Analogue;
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
			ShemaASU.ShemaUp = "Temp_Supply";
			ShemaASU.ShemaPos = ShemaASUbase.DummyPos(ComponentVariable);
			ShemaASU.ScheUpSize = ShemaASUbase.DummySize(ComponentVariable);
			ShemaASU.Description1 = Description;


		}
	}
}
