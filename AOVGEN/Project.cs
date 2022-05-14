
using System.ComponentModel;

namespace AOVGEN
{
    class Project
    {
        [DisplayName("Название проекта")]
        public string ProjectName { get; set; }
        [DisplayName("Шифр")]
        public string Chiper { get; set; }
        [DisplayName("ГИП")]
        public string CheefEngeneer { get; set; }
        private string ProjectGUID { get; set; }
        public string GetGUID()
        {
            string guid = ProjectGUID;
            return guid;

        }
        public void SetGUID(string GUID)
        {
            ProjectGUID = GUID;
        }




    }
}
