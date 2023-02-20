using System;
using System.ComponentModel;

namespace AOVGEN.Models
{
    [Serializable]
	public class ElectroDevice
    {
		[TypeConverter(typeof(EnumDescConverter))]
		public enum _Voltage
		{
			[Description("~380")]
			AC380,
			[Description("~220")]
			AC220,
			[Description("~24")]
			AC24,
			[Description("=24")]
			DC24
		}
		[DisplayName("Напряжение")]
		public virtual _Voltage Voltage
		{
			get => VoltageVariable;
			set => VoltageVariable = value;
		}
		[DisplayName("Мощность")]
		public string Power
		{
			get => GetPower();
			set => SetPower(value);
		}
		internal string Description { get; set; }
		protected _Voltage VoltageVariable;
		internal Cable Cable1;
		protected string sortpriority;
		private string _power;
		internal string GetPower()
		{
			return _power;
		}
		internal void SetPower(object val)
		{
			_power = (string)val;
		}
	}
}
