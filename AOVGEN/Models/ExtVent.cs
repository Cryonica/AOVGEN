using System;

namespace AOVGEN.Models
{
    [Serializable]
	class ExtVent : Vent, IPower
    {
		public static string ImagePath = VentSystem.AppFolder + "Images\\4_1.png";
		public static string ImageNullPath = VentSystem.AppFolder + "Images\\4_0.png";

		public ExtVent()
		{
			Location = "Exhaust";
			comp = VentComponents.ExtVent;
			ShemaASU = ShemaASU.CreateBaseShema(comp);
			ShemaASU.ShemaUp = "Ext_Vent";
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Exhaust;
			ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
			Power = "0";
			VoltageVariable = _Voltage.AC220;
			Description = "Э/д вытяжного вентилятора";
			Description2 = "Защита э/д вытяжн.вент.";
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
		public ExtVent(bool isReserved, bool isSpared)
		{
			_isReserved = isReserved;
			_isSpare = isSpared;
			Location = "Exhaust";
			comp = VentComponents.ExtVent;
			ShemaASU = ShemaASU.CreateBaseShema(comp);
			ShemaASU.ShemaUp = isSpared ? "Supply_Vent_Spare" : "Ext_Vent";
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Exhaust;
			ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
			Power = "0";
			VoltageVariable = _Voltage.AC220;
			Description = "Э/д вытяжного вентилятора";
			Description2 = "Защита э/д вытяжн.вент.";
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
					sortpriority = "02" + additional;
					break;
				default:
					sortpriority = "07" + additional;
					break;

			}
			return sortpriority;

		}
	}
}
