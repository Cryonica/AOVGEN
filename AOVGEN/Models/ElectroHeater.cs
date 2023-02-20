using System;
using System.ComponentModel;

namespace AOVGEN.Models
{
	[Serializable]
	class ElectroHeater : ElectroDevice, IPower, ICompGUID
    {
		#region Enums
		[TypeConverter(typeof(EnumDescConverter))]
		public enum _Stairs
		{
			[Description("1")]
			S1,
			[Description("2")]
			S2,
			[Description("3")]
			S3,
			[Description("4")]
			S4


		}
		#endregion

		#region Private Variables
		private VentComponents ComponentVariable;
		private _Stairs stairs;
		#endregion

		#region Path To Images
		public static string Imagepath = VentSystem.AppFolder + "Images\\8_1.png"; //это пути для картинок
		public static string ImageNullPath = VentSystem.AppFolder + "Images\\8_0.png";
		internal ShemaASU ShemaASU;
		#endregion

		#region Properties

		[DisplayName("Тип элемента")]
		public VentComponents comp
		{
			get => ComponentVariable;
			private set => ComponentVariable = value;
		}

		[DisplayName("Напряжение")]
		public new _Voltage Voltage
		{
			get => VoltageVariable;
			set => CheckVoltage(value);
		}

		[DisplayName("Мощность")]
		public new string Power { get; set; }

		[DisplayName("Количество ступеней")]
		public _Stairs Stairs
		{
			get => stairs;
			set => CheckStairs(value);
		}
	
		[DisplayName("Поз.обозн.")]
		public string PosName { get; internal set; }

		[DisplayName("Имя блока")]
		public string BlockName => MakeBlockName();
	
		public string GUID { get; set; }
		internal string Description1 { get; set; }
		internal string Description2 { get; set; }
		internal double Displacement { get; set; }
		internal string StairString { get; set; }
		internal Cable Cable2;
		internal Cable Cable3;
		internal Cable Cable4;
		internal Cable Cable5;
		internal Cable Cable6;
		#endregion

		#region Methods
		private string MakeBlockName()
		{
			string blockname = string.Empty;
			switch (Voltage)
			{
				case _Voltage.AC220:
					blockname = "EH-220-TK";
					break;
				case _Voltage.AC380:
					blockname = "ED-380-FC";
					break;
			}
			return blockname;

		}
		private _Stairs CheckStairs(object val)
		{
			stairs = (_Stairs)val;
			switch (stairs)
			{
				case _Stairs.S1:
					Displacement = -28.75646364;
					Cable2 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = "Управление нагревателем 1 ступень",
						WireNumbers = 2
					};
					Cable2.SortPriority = Cable2.MakeControlSortpriority(SortPriority + "2");
					StairString = "1";
					Cable3 = null;
					Cable4 = null;
					Cable5 = null;
					ShemaASU.SetIO(ShemaASU.IOType.DO, 1);

					break;
				case _Stairs.S2:
					Displacement = -18.85578925;
					Cable2 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = "Управление нагревателем 1 ступень",
						WireNumbers = 2
					};
					Cable3 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = "Управление нагревателем 2 ступень",
						WireNumbers = 2
					};
					Cable2.SortPriority = Cable2.MakeControlSortpriority(SortPriority + "2");
					Cable3.SortPriority = Cable3.MakeControlSortpriority(SortPriority + "3");
					StairString = "2";
					Cable4 = null;
					Cable5 = null;
					ShemaASU.SetIO(ShemaASU.IOType.DO, 2);
					break;
				case _Stairs.S3:
					Displacement = -9.90067453;
					Cable2 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = "Управление нагревателем 1 ступень",
						WireNumbers = 2
					};
					Cable3 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = "Управление нагревателем 2 ступень",
						WireNumbers = 2
					};
					Cable4 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = "Управление нагревателем 3 ступень",
						WireNumbers = 2
					};
					Cable2.SortPriority = Cable2.MakeControlSortpriority(SortPriority + "2");
					Cable3.SortPriority = Cable3.MakeControlSortpriority(SortPriority + "3");
					Cable4.SortPriority = Cable4.MakeControlSortpriority(SortPriority + "4");
					StairString = "3";
					Cable5 = null;
					ShemaASU.SetIO(ShemaASU.IOType.DO, 3);
					break;
				case _Stairs.S4:
					Displacement = 0;
					Cable2 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = "Управление нагревателем 1 ступень",
						WireNumbers = 2
					};
					Cable3 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = "Управление нагревателем 2 ступень",
						WireNumbers = 2
					};
					Cable4 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = "Управление нагревателем 3 ступень",
						WireNumbers = 2
					};
					Cable5 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = "Управление нагревателем 4 ступень",
						WireNumbers = 2
					};
					Cable2.SortPriority = Cable2.MakeControlSortpriority(SortPriority + "2");
					Cable3.SortPriority = Cable3.MakeControlSortpriority(SortPriority + "3");
					Cable4.SortPriority = Cable4.MakeControlSortpriority(SortPriority + "4");
					Cable4.SortPriority = Cable5.MakeControlSortpriority(SortPriority + "5");
					StairString = "4";
					ShemaASU.SetIO(ShemaASU.IOType.DO, 4);
					break;





			}
			return stairs;

		}
		private _Voltage CheckVoltage(object val)
		{
			VoltageVariable = (_Voltage)val;
			switch (VoltageVariable)
			{
				case _Voltage.AC24:
				case _Voltage.DC24:
					VoltageVariable = _Voltage.AC220;
					if (Cable1 != null) Cable1.WireNumbers = 3;
					break;
				case _Voltage.AC380:
					VoltageVariable = _Voltage.AC380;
					if (Cable1 != null) Cable1.WireNumbers = 5;

					break;
				case _Voltage.AC220:
					VoltageVariable = _Voltage.AC220;
					if (Cable1 != null) Cable1.WireNumbers = 3;
					break;
			}
			return VoltageVariable;
		}
		internal string SortPriority => MakeSortPriority();
        private string MakeSortPriority()
		{
			string priority;
			switch (Voltage)
			{
				case _Voltage.AC380:
					priority = "03";
					break;
				default:
					priority = "08";
					break;

			}
			if (Cable1 != null) Cable1.SortPriority = priority;
			if (Cable2 != null) Cable2.MakeControlSortpriority(priority + "2");
			if (Cable3 != null) Cable3.MakeControlSortpriority(priority + "3");
			if (Cable4 != null) Cable4.MakeControlSortpriority(priority + "4");
			if (Cable5 != null) Cable5.MakeControlSortpriority(priority + "5");
			if (Cable6 != null) Cable6.MakeControlSortpriority(priority + "6");

			return priority;

		}
		#endregion

		#region Constructors
		public ElectroHeater()
		{
			ComponentVariable = VentComponents.ElectroHeater;
			ShemaASU = ShemaASU.CreateBaseShema(ComponentVariable);
			ShemaASU.ShemaUp = "Supply_ElectroHeater";
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;

			Stairs = _Stairs.S1;
			ShemaASU.SetIO(ShemaASU.IOType.DI, 1);

			PosName = "H";
			Voltage = _Voltage.AC220;

			Power = "0";
			Description = "Электрический нагреватель";
			Description1 = Description;
			Cable1 = new Cable
			{
				SortPriority = SortPriority,
				WriteBlock = true,
				Attrubute = Cable.CableAttribute.P,
				ToPosName = PosName,
				ToBlockName = BlockName,
				Description = Description,
				WireNumbers = 3
			};
			Cable2 = new Cable
			{
				WriteBlock = false,
				Attrubute = Cable.CableAttribute.D,
				ToPosName = PosName,
				ToBlockName = BlockName,
				Description = "Управление нагревателем 1 ступень",
				WireNumbers = 2

			};
			Cable6 = new Cable
			{
				WriteBlock = false,
				Attrubute = Cable.CableAttribute.D,
				ToPosName = PosName,
				ToBlockName = BlockName,
				Description = "Авария нагревателя",
				WireNumbers = 2

			};
			Cable2.MakeControlSortpriority(SortPriority + "2");
			Cable6.MakeControlSortpriority(SortPriority + "6");

		}
		#endregion
	}
}
