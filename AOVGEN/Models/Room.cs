using System;

namespace AOVGEN.Models
{
    [Serializable]
	class Room : Dummy
	{
		internal override void SetBlockName(string image)
		{
			ShemaASU = ShemaASU.CreateBaseShema();
			string blockname;
			switch (image)
			{
				case "room__arrow_supp_exh_T":
				case "room__arrow_supp_exh_T_big":
				case "room__arrow_supp_exh_TH":
				case "room__arrow_supp_exh_big":
				case "room__arrow_supp_exh_TH_big":
					blockname = "Room_Supply_Exthaust";
					break;
				case "room__arrow_supply_T":
				case "room__arrow_supply_TH":
					blockname = "Room_Supply";
					break;
				case "room__arrow_exhaust_L":
					blockname = "Room_Exthaust";
					break;
				case "room__arrow_exhaust_R":
					blockname = "Room_Exthaust_R";
					break;
				default:
					blockname = "Room_Supply";
					break;

			}
			ShemaASU.ShemaUp = blockname;


		}
		internal override void SetBlockName()
		{
			ShemaASU = ShemaASU.CreateBaseShema();
			ShemaASU.ShemaUp = "Room_Supply";
		}
		internal override void EnableSensorT()
		{
			_SensorT = new IndoorTemp
			{
				Description = "Датчик температуры в помещении",
				_SensorType = Sensor.SensorType.Analogue,
				Location = "Indoor"
			};
			SensorTType = _SensorT._SensorType;
			blockNameT = _SensorT.Blockname;
		}
		internal override void EnableSensorH()
		{
			_SensorH = new HumiditySens
			{
				Description = "Датчик влажности в помещении",
				_SensorType = Sensor.SensorType.Analogue,
				Location = "Indoor"
			};
			SensorHType = _SensorH._SensorType;
			blockNameH = _SensorH.Blockname;
			
		}
		public Room(bool sensT, bool sensH) : base(sensT, sensH) { }
		public Room(bool sensT, bool sensH, string image) : base(sensT, sensH, image) { }
		public Room() { }
	}
}
