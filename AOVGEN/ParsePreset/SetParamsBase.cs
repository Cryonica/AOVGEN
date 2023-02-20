using System.Xml;

namespace AOVGEN.ParsePreset
{
    abstract class SetParamsBase: ISetParam
    {
        readonly XmlNode posInfoNode;
        protected XmlNode paramNode;
        public SetParamsBase(XmlNode items)
        {
            paramNode = items.ChildNodes[0];
            posInfoNode = items.ChildNodes[1];   
        }
        public EditorV2.PosInfo GetPosInfo()
        {
            return EditorV2.SetXmlProPertyToPosInfo(posInfoNode);
        }
        public abstract object GetEntity();
        public abstract void SetParams();

    }
}
