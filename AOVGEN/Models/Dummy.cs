using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOVGEN.Models
{
	[Serializable]
	abstract class Dummy
    {
		internal SensorT _SensorT;
		internal HumiditySens _SensorH;
		private protected Sensor.SensorType sensorTypeT;
		private protected Sensor.SensorType sensorTypeH;
		private protected string blockNameT;
		private protected string blockNameH;
		internal ShemaASU ShemaASU;
		internal abstract void SetBlockName(string image);
		internal abstract void SetBlockName();
		[DisplayName("Тип датчика температуры")]
		public Sensor.SensorType SensorTType
		{
			get => sensorTypeT;
			set => SetSensorTType(_SensorT, value);
		}
		[DisplayName("Тип датчика влажности")]
		public Sensor.SensorType SensorHType
		{
			get => sensorTypeH;
			set => SetSensorHType(_SensorH, value);
		}
		[DisplayName("Имя блока датчика температуры")]
		public string SensTBlockName => blockNameT;
		[DisplayName("Имя блока датчика влажности")]
		public string SensHBlockName => blockNameH;
		[Browsable(false)]
		public string GUID { get; set; }
		internal void SetSensorTType(dynamic Sens, Sensor.SensorType val)
		{
			if (val != Sensor.SensorType.No)
            {
				if (Sens == null)
				{
					EnableSensorT();
				}
				_SensorT._SensorType = val;
				sensorTypeT = val;
				blockNameT = _SensorT.Blockname;
			}
			else
            {
				_SensorT = null;
            }
		}
		internal void SetSensorHType(dynamic Sens, Sensor.SensorType val)
		{
			if (val != Sensor.SensorType.No)
            {
				if (Sens == null)
				{
					EnableSensorH();
				}
				_SensorH._SensorType = val;
				sensorTypeH = val;
				_SensorH.SensorInsideCrossSection = true;
			}
			else
            {
				_SensorH = null;
            }
		}
		internal abstract void EnableSensorT();
		internal abstract void EnableSensorH();		
		public Dummy(bool sensT, bool sensH)
		{
			if (sensT)
			{
				EnableSensorT();
			}
			if (sensH)
			{
				EnableSensorH();

			}
			SetBlockName();
		}
		public Dummy(bool sensT, bool sensH, string imagename)
		{

			if (sensT)
			{
				EnableSensorT();
			}
			if (sensH)
			{
				EnableSensorH();

			}
			SetBlockName(imagename);
		}
		public Dummy()
		{

		}
	}
}
