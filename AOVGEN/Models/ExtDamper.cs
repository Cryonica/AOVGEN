using System;
using System.ComponentModel;

namespace AOVGEN.Models
{
    [Serializable]
	class ExtDamper : Damper, IPower
    {
		public static string ImagePath = VentSystem.AppFolder + "Images\\6_1.png";
		public static string ImageNullPath = VentSystem.AppFolder + "Images\\6_0.png";
		[DisplayName("Тип элемента")]
		public VentComponents comp
		{
			get => ComponentVariable;
			internal set => ComponentVariable = value;
		}
		private VentComponents ComponentVariable;
		public ExtDamper()
		{

			comp = VentComponents.ExtDamper;
			ShemaASU = ShemaASU.CreateBaseShema(comp);
			ShemaASU.ShemaUp = "Ext_Damper";
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Exhaust;

			PosName = "M";
			VoltageVariable = _Voltage.AC220;
			Power = "3";
			Description1 = "Э/д вытяжной заслонки";
			Description = Description1;
			Cable1 = new Cable
			{
				Attrubute = Cable.CableAttribute.P,
				WriteBlock = true,
				ToPosName = PosName,
				ToBlockName = BlockName,
				SortPriority = SortPriority,
				Description = Description1,
				WireNumbers = 3

			};

			ShemaASU.SetIO(ShemaASU.IOType.DO, 1);

		}
		[DisplayName("Возвратная пружина")]
		public bool Spring { get; set; }
        [DisplayName("Концевые выключатели")]
        public bool HasControl
        {
            get => hascontrol;
            set => SetCable2Type(value);
        }
        internal override string MakeDamperBlockName()
		{
			string blockname;
			if (hascontrol)
			{
				blockname = "ZASL-220_SQ";
				Description2 = "Контроль положения вытяжной заслонки";
			}
			else
			{
				blockname = "ZASL-220";
			}

			return blockname;
		}
		internal string SortPriority => MakeSortPriority();
		private string MakeSortPriority()
		{
			switch (Voltage)
			{
				case _Voltage.AC220:
					sortpriority = "13";
					break;
				default:
					sortpriority = "16";
					break;

			}
			if (Cable1 != null) Cable1.SortPriority = sortpriority;
			if (Cable2 != null) Cable2.MakeControlSortpriority(sortpriority);
			return sortpriority;

		}
	}
}
