
using System;
using System.Collections.Generic;
using GKS_ASU_Loader;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace AOVGEN
{

    class RunRevitCommand
    {
        private IRevitExternalService Service { get; }
        public RunRevitCommand(IRevitExternalService externalService )
        {
            //Service = externalService;
            string uri = "net.tcp://localhost:6565/";
            NetTcpBinding binding = new(SecurityMode.None)
            {
                MaxBufferPoolSize = 2147483647,
                MaxBufferSize = 2147483647,
                MaxReceivedMessageSize = 2147483647,
                ReaderQuotas =
                {
                    MaxStringContentLength = 2147483647,
                    MaxArrayLength = 2147483647,
                    MaxDepth = 2147483647,
                    MaxBytesPerRead = 2147483647
                }
            };
            var endpoint = new EndpointAddress(uri);
            var channel = new ChannelFactory<IRevitExternalService>(binding, endpoint);
            foreach (OperationDescription op in channel.Endpoint.Contract.Operations)
            {
                DataContractSerializerOperationBehavior dataContractBehavior =
                    op.Behaviors.Find<DataContractSerializerOperationBehavior>();
                if (dataContractBehavior != null)
                {
                    dataContractBehavior.MaxItemsInObjectGraph = 2147483646;
                }
            }
            IRevitExternalService proxy = channel.CreateChannel();

            //var proxy = channel.CreateChannel(endpoint);
           
            Service = proxy;

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
            var info = Service.GetStateInfo();
            if (!string.IsNullOrEmpty(info.Item1))
            {
                
                StatusChanged?.Invoke(this, new  StatuChangeArgs(){docinfo = info.Item1});
                
            }
            else
            {
                StatusChanged?.Invoke(this, new StatuChangeArgs() { docinfo = String.Empty });
            }
            return info;
        }
        public int LoadPannels(List<(string, string, string, string, string, string, string, string, string, double)> IDPannelList)
        {
            int result = Service.LoadPannels(IDPannelList);
            return result;
        }
        public int PlacePannels()
        {
            int result = Service.PlacePannels();
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

        public void SetUIApp()
        {
            try
            {
                Service.SetUIApp();
            }
            catch 
            {
              
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

        public event EventHandler StatusChanged;
        public class StatuChangeArgs: EventArgs
        {
            public string docinfo { get; set; }
        }


    }
}
