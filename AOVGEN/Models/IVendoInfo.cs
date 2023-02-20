namespace AOVGEN.Models
{
    public interface IVendoInfo
    {
        (string VendorName, string ID, string VendorDescription, string DBTable, string Assignment, string DefaultDescription, string MainDBTable) GetVendorInfo();
        void SetVendorInfo(string vendorName, string ID, string vendorDescription, string dbTable, string assignment);
        void ClearVendorInfo();
    }
}
