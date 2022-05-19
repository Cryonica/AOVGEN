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
        internal string BuildGUID { get => guid;
            set => SetGUID(value);
        }

        internal string SetGUID(object val)
        {
            guid = val.ToString();
            return guid;

        }
        
        
        

    }
}
