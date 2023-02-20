using System;
using System.ComponentModel;

namespace AOVGEN.Models
{
    [Serializable]
	class KKB : ElectroDevice, IPower, ICompGUID
    {
		[TypeConverter(typeof(EnumDescConverter))]
		public enum FrosterStairs
		{
			[Description("1")]
			One,
			[Description("2")]
			Two,
			[Description("3")]
			Three,
			[Description("4")]
			Four

		}
		[TypeConverter(typeof(EnumDescConverter))]
		public enum KKBControlType
		{
			[Description("Нет")]
			No,
			[Description("Аналоговый")]
			Analogue,
			[Description("Дискретный")]
			Discrete
		}

		public FrosterStairs Stairs { get; set; }
		public KKBControlType _KKBControlType { get; set; }
		internal Cable Cable2;
		internal Cable Cable3;
		internal Cable Cable4;
		internal Cable Cable5;
		internal Cable Cable6;
		internal ShemaASU ShemaASU;

		public string GUID { get; set; }
		internal double Displacement { get; set; }
		internal string StairsString { get => Froster.GetEnumDescription(Stairs); set { } }
		public string PosName = "KKB";

		private string MakeBlockName()
		{
			string blockname;
			switch (Voltage)
			{
				case _Voltage.AC380:

					blockname = "ED-380-FC";
					if (Cable1 != null) Cable1.WireNumbers = 5;
					break;
				case _Voltage.AC220:
					blockname = "ED-220-FC";
					if (Cable1 != null) Cable1.WireNumbers = 3;
					break;
				default:
					blockname = string.Empty;
					break;
			}
			return blockname;
		}
		public string BlockName => MakeBlockName();
		internal string SortPriority
		{
			get => sortpriority; set => MakeSortPriority(Voltage);
		}
		internal string MakeSortPriority(_Voltage voltage)
		{
			string priority;
			switch (voltage)
			{
				case _Voltage.AC380:
					priority = "05";
					break;
				default:
					priority = "10";
					break;

			}

			if (Cable1 != null) Cable1.SortPriority = priority;
			if (Cable2 != null) Cable2.MakeControlSortpriority(priority + "2");
			if (Cable3 != null) Cable3.MakeControlSortpriority(priority + "3");
			if (Cable4 != null) Cable4.MakeControlSortpriority(priority + "4");
			if (Cable5 != null) Cable5.MakeControlSortpriority(priority + "5");
			if (Cable6 != null) Cable6.MakeControlSortpriority(priority + "6");
			sortpriority = priority;
			return priority;

		}
		public KKB()
		{
			ShemaASU = ShemaASU.CreateBaseShema(VentComponents.Froster);
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
			_KKBControlType = KKBControlType.Discrete;
			Voltage = _Voltage.AC380;
			Cable1 = new Cable
			{
				WriteBlock = true,
				Attrubute = Cable.CableAttribute.P,
				ToPosName = PosName,
				ToBlockName = BlockName,
				Description = "ККБ охладителя",
				SortPriority = SortPriority,
				WireNumbers = 5

			};
			Cable2 = new Cable
			{
				WriteBlock = false,
				Attrubute = Cable.CableAttribute.D,
				ToPosName = PosName,
				ToBlockName = BlockName,
				Description = "Управление ККБ 1 ступень",
				WireNumbers = 2

			};
			Cable6 = new Cable
			{
				WriteBlock = false,
				Attrubute = Cable.CableAttribute.D,
				ToPosName = PosName,
				ToBlockName = BlockName,
				Description = "Авария ККБ",
				WireNumbers = 2

			};
			Cable2.MakeControlSortpriority(SortPriority + "2");
			Cable6.MakeControlSortpriority(SortPriority + "6");
			ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
			ShemaASU.SetIO(ShemaASU.IOType.DI, 1);

		}
	}
}
