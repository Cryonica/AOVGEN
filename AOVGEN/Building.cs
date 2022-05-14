using System.ComponentModel;

namespace AOVGEN
{
    class Building
    {
        string guid;
        [DisplayName("Имя здания")]
        public string Buildname { get; set; }
        [DisplayName("Номер здания")]
        public string BuildNum { get; set; }
        [DisplayName("Адрес")]
        public string Address { get; set; }
        internal string BuildGUID { get { return guid; } set { SetGUID(value); } }
        string SetGUID(object val)
        {
            guid = val.ToString();
            return guid;

        }
        
        
        

    }
}
