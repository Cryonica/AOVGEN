using System;
using System.ComponentModel;

namespace AOVGEN.Models
{
	[Serializable]
	abstract class Sensor : ICompGUID, IVendoInfo
    {
		[TypeConverter(typeof(EnumDescConverter))]
		public enum SensorType
		{
			[Description("Нет")]
			No,
			[Description("Аналоговый")]
			Analogue,
			[Description("Дискретный")]
			Discrete
		}
		private protected SensorType sensorType;
		internal string sortpriority;
		internal string VendorName;
		internal string ID;
		internal string VendorDescription;
		internal string VendorDBTable;
		internal string MainDBTable;
		internal string Assignment;
		[DisplayName("Имя блока датчика")]
		public string Blockname { get; internal set; }
		[DisplayName("Обозначение")]
		public string PosName { get; internal set; }
		[DisplayName("Тип датчика")]
		public virtual SensorType _SensorType
		{
			get => sensorType;
			set => CheckSensorType(value);
		}
		public string GUID { get; set; }
		internal string Description { get; set; }
		internal string Location { get; set; }
		internal virtual string SortPriority => sortpriority;
		internal Cable Cable1;
		internal abstract void CheckSensorType(object val);
		internal ShemaASU ShemaASU;
		public (string VendorName, string ID, string VendorDescription, string DBTable, string Assignment, string DefaultDescription, string MainDBTable) GetVendorInfo()
		{
			(string VendorName, string ID, string VendorDescription, string DBTable, string Assignment, string DefaultDescription, string MainDBTable) vendorinfo;
			vendorinfo.VendorName = VendorName;
			vendorinfo.ID = ID;
			vendorinfo.VendorDescription = VendorDescription;
			vendorinfo.DBTable = VendorDBTable;
			vendorinfo.DefaultDescription = Description;//;Description;
			vendorinfo.Assignment = Assignment;
			vendorinfo.MainDBTable = MainDBTable;
			return vendorinfo;
		}
		public void SetVendorInfo(string vendorName, string ID, string vendorDescription, string dbTable, string assignment)
		{
			if (!string.IsNullOrEmpty(vendorName)) VendorName = vendorName;
			if (!string.IsNullOrEmpty(ID)) this.ID = ID;
			if (!string.IsNullOrEmpty(vendorDescription)) VendorDescription = vendorDescription;
			if (!string.IsNullOrEmpty(dbTable)) VendorDBTable = dbTable;
			if (!string.IsNullOrEmpty(assignment)) Assignment = assignment;
		}
		public void ClearVendorInfo()
		{
			VendorName = string.Empty;
			ID = string.Empty;
			VendorDescription = string.Empty;
			Assignment = string.Empty;
			VendorDBTable = string.Empty;

		}
	}
}
