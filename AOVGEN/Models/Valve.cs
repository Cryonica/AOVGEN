using System;
using System.ComponentModel;


namespace AOVGEN.Models
{
	[Serializable]
	class Valve : ElectroDevice, ICompGUID
    {
		[TypeConverter(typeof(EnumDescConverter))]
		internal enum ValveType
		{
			[Description("Нет")]
			No,
			[Description("0-10V")]
			Analogue_0_10,
			[Description("4-20mA")]
			Analogue_4_20,
			[Description("3-х позиционный")]
			ThreePos
		}
		private ValveType valveType;
		internal ValveType _ValveType
		{
			get => valveType;
			set => valveType = value;
		}

		public string Posname = "FV";
		public string BlockName = "ED-24-010V";
		public string GUID { get; set; }
		internal Cable Cable2;
		internal Cable Cable3;
		internal Cable Cable4;
		internal Cable Cable5;
		internal ShemaASU ShemaASU;

		internal string SortPriority
		{
			get => sortpriority;
			set => MakeSortPriority(value);
		}
		virtual internal string MakeSortPriority(object val)
		{
			sortpriority = string.Empty;
			switch (Voltage)
			{
				case _Voltage.AC220:
					sortpriority = "14";
					break;
				default:
					sortpriority = val.ToString();
					
					break;

			}
			return sortpriority;

		}
		//internal Valve()
		//{

		//	ShemaASU = ShemaASU.CreateBaseShema(VentComponents.WaterHeater);
		//	ShemaASU.SetIO(ShemaASU.IOType.AO, 1);
		//	ShemaASU.ShemaUp = "WaterHeater_Valve";
		//	ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
		//	ShemaASU.ShemaPos = ShemaASUbase.DummyPos(VentComponents.WaterHeater);

		//	valveType = ValveType.Analogue_0_10;
		//	Voltage = _Voltage.AC24;
		//	Description = "Э/д клапана водяного нагревателя";
		//	SortPriority = "17";
		//	Cable1 = new Cable
		//	{
		//		Attrubute = Cable.CableAttribute.PL,
		//		WriteBlock = true,
		//		ToPosName = Posname,
		//		ToBlockName = BlockName,
		//		SortPriority = SortPriority,
		//		Description = Description,
		//		WireNumbers = 3
		//	};
		//	Cable2 = new Cable
		//	{
		//		Attrubute = Cable.CableAttribute.A,
		//		WriteBlock = false,
		//		ToPosName = Posname,
		//		ToBlockName = BlockName,
		//		Description = "Управление клапаном нагревателя",
		//		WireNumbers = 2
		//	};
		//	Cable2.MakeControlSortpriority(SortPriority + "2");
		//	Power = "10";
			
		//}
	}
	
}
