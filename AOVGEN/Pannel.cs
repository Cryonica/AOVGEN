using System;
using System.ComponentModel;

namespace AOVGEN
{
    
    public class Pannel
    {
        private _Voltage VoltageVariable;
        private _Protocol ProtocolVariable;
        private _Protocol OldProtocolVariable;
        private _Category Categoryvariable;
        private _FireProtect FireProtectVariable;
        private _Dispatching  DispatchingVariable;
        private string name;
        internal int Version { get; set; }

        [DisplayName("Имя щита")]
        public string PannelName { get => name;
            set => SetName(value);
        }
        [DisplayName("Мощность")]
        public string Power { get; internal set; }

        [TypeConverter(typeof(EnumDescConverter))]
        public enum _Category 
        {
            [Description("1")]
            one,
            [Description("2")]
            two,
            [Description("3")]
            three

        }
        [TypeConverter(typeof(EnumDescConverter))]
        public enum _FireProtect
        {
            [Description("Да")]
            Yes,
            [Description("Нет")]
            No

        }
        [TypeConverter(typeof(EnumDescConverter))]
        public enum _Dispatching
        {
            [Description("Да")]
            Yes,
            [Description("Нет")]
            No

        }
        [TypeConverter(typeof(EnumDescConverter))]
        public enum _Protocol
        {
            [Description("Нет")]
            None,
            [Description("Modbus RTU")]
            ModBus_RTU,
            [Description("Modbus TCP")]
            ModBus_TCP,
            [Description("Bacnet IP")]
            Bacnet_IP,
            [Description("LON")]
            LON
        }
        [TypeConverter(typeof(EnumDescConverter))]
        public enum _Voltage
        {
            [Description("220")]
            AC220,
            [Description("380")]
            AC380

        }
        [DisplayName("Категория")]
        public _Category Category { get => Categoryvariable;
            set => SetCategory(value);
        }
        [DisplayName("Пож.защита")]
        public _FireProtect FireProtect { get => FireProtectVariable;
            set => SetFireProtect(value);
        }
        [DisplayName("Диспетч-я")]
        public _Dispatching Dispatching { get => DispatchingVariable;
            set => SetDispatching(value);
        }
        [DisplayName("Протокол")]
        public _Protocol Protocol { get => ProtocolVariable;
            set => SetProtocol(value);
        }
        private string PannelGUID;
        [DisplayName("Напряжение")]
        public _Voltage Voltage { get => VoltageVariable;
            set => SetVoltage(value);
        }
        internal _Protocol OldProtocol => OldProtocolVariable;

        public string GetGUID()
        {
            string guid = PannelGUID;
            return guid;

        }
        public void SetGUID (string GUID)
        {
            PannelGUID = GUID;
        }
        private void SetProtocol(object val)
        {
            if (ProtocolVariable != _Protocol.None) OldProtocolVariable = ProtocolVariable;
            if ((_Protocol)val != _Protocol.None) OldProtocolVariable = ProtocolVariable;
            ProtocolVariable = (_Protocol)val;
            ProtocolChange?.Invoke(this, ProtocolVariable);
        }
        private void SetVoltage(object val)
        {
            VoltageVariable = (_Voltage)val;
        }
        private void SetFireProtect(object val)
        {
            FireProtectVariable = (_FireProtect)val;
        }
        private void SetName (object val)
        {
            name = val.ToString();
            NameChange?.Invoke(this, name);
        }
        private void SetDispatching (object val)
        {
            DispatchingVariable = (_Dispatching)val;
            ChangeDispaptching?.Invoke(this, DispatchingVariable);
        }
        private void SetCategory(object val)
        {
            Categoryvariable = (_Category)val;
        }
        #region User Events
        public event EventHandler<_Protocol> ProtocolChange;
        public event EventHandler<string> NameChange;
        public event EventHandler<_Dispatching> ChangeDispaptching;
        
        
                
        #endregion

    }
    
    

    
}

