using System;
using System.ComponentModel;


namespace AOVGEN.Models
{
    [Serializable]
	public class Cable
    {
		public enum CableAttribute
		{
			[Description("Аналоговый")]
			A,
			[Description("Дискретный")]
			D,
			[Description("Управления")]
			C,
			[Description("Силовой")]
			P,
			[Description("Питание 24")]
			PL,
			[Description("Нет")]
			No
		}

		internal string SortPriority { get; set; }
		internal bool WriteBlock { get; set; }
		internal string cableGUID { get; set; }
		[DisplayName("Откуда")]
		public string FromPosName { get; set; }
		[DisplayName("Куда")]
		public string ToPosName { get; set; }

		internal string ToBlockName { get; set; }
		internal string FromGUID { get; set; }
		internal string ToGUID { get; set; }
		[DisplayName("Имя кабеля")]
		public string CableName { get; set; }
		[DisplayName("Тип кабеля")]
		public string CableType { get; set; }

		internal string HostTable { get; set; }
		[DisplayName("Описание")]
		public string Description { get; set; }
		[DisplayName("Длина")]
		public double Lenght { get; set; }
		internal string CompTable { get; set; }
		[DisplayName("Количество жил")]
		public int WireNumbers { get; set; }
		[DisplayName("Атрибут")]
		public CableAttribute Attrubute { get; set; }
		internal string MakeControlSortpriority(string hostpriority)
		{

			SortPriority = hostpriority + "1";
			return SortPriority;
		}
	}
}
