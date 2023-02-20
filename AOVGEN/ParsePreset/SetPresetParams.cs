using System;
using System.Reflection;
using System.Xml;

namespace AOVGEN.ParsePreset
{
    class SetPresetParams: SetParamsBase
    {
        readonly object _entity;
        public SetPresetParams(object obj, XmlNode items):base(items)
        {
           _entity = obj;
        }
        public override object GetEntity() => _entity;
        public override void SetParams()
        {
            foreach (XmlNode param in paramNode.ChildNodes)
            {
                
                string PropertValue = param.InnerText;
                string PropertyName = param.LocalName;
                PropertyInfo pinfo = _entity.GetType()
                    .GetProperty(PropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (pinfo == null) pinfo = _entity.GetType().BaseType
                        .GetProperty(PropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (pinfo == null) continue;
                if (pinfo.PropertyType.IsEnum)
                {
                    pinfo?.SetValue(_entity, Enum.Parse(pinfo.PropertyType, PropertValue));
                }
                else
                {
                    pinfo?.SetValue(_entity, Convert.ChangeType(PropertValue, pinfo.PropertyType), null);
                }
                
            }
        }
    }
}
