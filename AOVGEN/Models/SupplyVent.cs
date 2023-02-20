using System;

namespace AOVGEN.Models
{
    [Serializable]
	class SupplyVent : Vent, IPower
    {
		public SupplyVent()
		{
			Location = "Supply";

			comp = VentComponents.SupplyVent;
			ShemaASU = ShemaASU.CreateBaseShema(comp);
			ShemaASU.ShemaUp = "Supply_Vent";
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
			ShemaASU.SetIO(ShemaASU.IOType.DO, 1);

			Power = "0";
			VoltageVariable = _Voltage.AC220;
			Description = "Э/д приточного вентилятора";
			Description2 = "Защита э/д приточ.вент.";
			PressureProtect = _PressureProtect.Discrete;

			Cable1 = new Cable
			{
				Attrubute = Cable.CableAttribute.P,
				WriteBlock = true,
				ToPosName = PosName,
				ToBlockName = BlockName,
				Description = Description,
				SortPriority = SortPriority,
				WireNumbers = 3
			};


		}
		public SupplyVent(bool isReserved, bool isSpared)
		{
			_isReserved = isReserved;
			_isSpare = isSpared;
			Location = "Supply";
			comp = VentComponents.SupplyVent;
			ShemaASU = ShemaASU.CreateBaseShema(comp);
			ShemaASU.ShemaUp = isSpared ? "Supply_Vent_Spare" : "Supply_Vent";
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
			ShemaASU.SetIO(ShemaASU.IOType.DO, 1);

			Power = "0";
			VoltageVariable = _Voltage.AC220;
			Description = isReserved ? "Э/д резервного приточного вентилятора" : "Э/д основного приточного вентилятора";
			Description2 = isReserved ? "Защита э/д резервн. приточн.вент." : "Защита э/д осн. приточ.вент.";
			PressureProtect = _PressureProtect.Discrete;

			Cable1 = new Cable
			{
				Attrubute = Cable.CableAttribute.P,
				WriteBlock = true,
				ToPosName = PosName,
				ToBlockName = BlockName,
				Description = Description,
				SortPriority = SortPriority,
				WireNumbers = 3
			};


		}
		internal override string MakeSortPriority(_Voltage voltage)
		{

			string additional = string.Empty;

			if (_isReserved)
			{
				additional = "6";

			}
			switch (voltage)
			{
				case _Voltage.AC380:
					sortpriority = "01" + additional;
					break;
				default:
					sortpriority = "06" + additional;
					break;

			}

			return sortpriority;

		}
	}
}
