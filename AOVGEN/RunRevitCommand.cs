
using System.Collections.Generic;
using GKS_ASU_Loader;

namespace AOVGEN
{

    class RunRevitCommand
    {
        private IRevitExternalService Service { get; }
        public RunRevitCommand(IRevitExternalService externalService )
        {
            Service = externalService;
        }
        public string GetCurrentDocPath()
        {
            return Service.GetCurrentDocumentPath();
        }
        public Dictionary<string, string> Levels()
        {
            
            return Service.GetLevels();
        }
        public Dictionary<(string, string, double), List<(string, string, string)>> GetRooms()
        {
            return Service.GetRooms();
        }
        public string GetDocumentID()
        {
            return Service.GetDocumentID();
        }
        public (string, string) StateInfo()
        {
            return Service.GetStateInfo();
        }
        public int PlacePan(List<(string, string, string, string, string, string, string, string, string, double)> IDPannelList)
        {
            int result = Service.PlacePannels(IDPannelList);
            return result;
        }
        public Dictionary<string, List<string>> FamilyTypes()
        {
            try
            {
                return Service.GetFamilyTypes();
            }
            catch
            {
                return null;
            }
            
            
            
        }
        public List<(string, string)> GetFamilyListID()
        {
            return Service?.GetFamilyListID();
        }
        public int GetPlaceResult()
        {
            return (int)Service?.GetPlaceResult();
        }

    }
}
