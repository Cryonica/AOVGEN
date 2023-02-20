using System;

namespace AOVGEN.Models
{
    [Serializable]
	public class FControl : IVendoInfo, ICompGUID
    {
		string VendorName;
		string VendorDescription;
		string DBTable;
		string MainDBTable;
		string ID;
		string Assignment;
		readonly string DefaultDescription = "Регулятор оборотов вентилятора";
		
		public string GUID { get; set; }
		internal string Description { get; set; }
		public (string VendorName, string ID, string VendorDescription, string DBTable, string Assignment, string DefaultDescription, string MainDBTable) GetVendorInfo()
		{
			return (VendorName, ID, VendorDescription, DBTable, Assignment, DefaultDescription, MainDBTable);
		}
		public void SetVendorInfo(string vendorName, string ID, string vendorDescription, string dbTable, string assignment)
		{
			if (!string.IsNullOrEmpty(vendorName)) VendorName = vendorName;
			if (!string.IsNullOrEmpty(ID)) this.ID = ID;
			if (!string.IsNullOrEmpty(vendorDescription)) VendorDescription = vendorDescription;
			if (!string.IsNullOrEmpty(assignment)) Assignment = assignment;
			DBTable = string.IsNullOrEmpty(dbTable) ? "FControl" : dbTable;
			MainDBTable = "FControl";

		}
		public void ClearVendorInfo()
		{
			VendorName = string.Empty;
			ID = string.Empty;
			VendorDescription = string.Empty;
			Assignment = string.Empty;
			DBTable = string.Empty;
		}
		public FControl()
		{
			DBTable = "FControl";
			Description = "Регулятор оборотов вентилятора";

		}
	}
}
