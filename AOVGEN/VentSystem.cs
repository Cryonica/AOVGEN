using System;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Data;
using System.Reflection;


namespace AOVGEN
{
#pragma warning disable IDE1006
	#region CommonVentsystem

	[System.ComponentModel.TypeConverter(typeof(EnumDescConverter))]
	internal enum VentComponents
	{
		[Description("Приточный вентилятор")]
		SupplyVent,
		[Description("Приточный фильтр")]
		SupplyFiltr,
		[Description("Фильтр")]
		Filtr,
		[Description("Приточная заслонка")]
		SupplyDamper,
		[Description("Водяной нагреватель")]
		WaterHeater,
		[Description("Электрический нагреватель")]
		ElectroHeater,
		[Description("Охладитель")]
		Froster,
		[Description("Увлажнитель")]
		Humidifier,
		[Description("Вытяжной вентилятор")]
		ExtVent,
		[Description("Вытяжной фильтр")]
		ExtFiltr,
		[Description("Вытяжная заслонка")]
		ExtDamper,
		[Description("Рекуператор")]
		Recuperator,
		[Description("Датчик наружной Т")]
		OutdoorTemp,
		[Description("Датчик Т в приточном канале")]
		SupplyTemp,
		[Description("Датчик Т в вытяжном канале")]
		ExtTemp,
		[Description("Датчик Т в помещении")]
		IndoorTemp,
        [Description("Приточный вентилятор с резервом")]
        SpareSuplyVent,
        [Description("Вытяжной вентилятор с резервом")]
        SpareExtVent

	}
	[System.ComponentModel.TypeConverter(typeof(EnumDescConverter))]
	internal enum CableTypes
	{
		[Description("Аналоговый")]
		A,
		[Description("Дискретный")]
		D,
		[Description("Управления")]
		C,
		[Description("Силовой")]
		P,
		[Description("Нет")]
		No
	}




	[Serializable]
	class VentSystem : IEnumerable,  ICloneable 
	{
        static string Appfolder()
		{
			return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2021\GKSASU\AOVGen\";

		}
		[DisplayName("Имя системы")]
		public string SystemName { get; internal set; }
		internal string GUID { get; set; }
		[DisplayName("Подключено к шкафу")]
		public string ConnectedTo { get; internal set; }
		public static string ImagePath = Appfolder() + "Images\\Main.png";
		public static string AppFolder = Appfolder();
		internal SupplyVent _SupplyVent { get; set; }
		internal ExtVent _ExtVent { get; set; }
		internal SupplyFiltr _SupplyFiltr { get; set; }
		internal ExtFiltr _ExtFiltr { get; set; }
		internal SupplyDamper _SupplyDamper { get; set; }
		internal ExtDamper _ExtDamper { get; set; }
		internal WaterHeater _WaterHeater { get; set; }
		internal ElectroHeater _ElectroHeater { get; set; }
		internal Froster _Froster { get; set; }
		internal Humidifier _Humidifier { get; set; }
		internal Recuperator _Recuperator { get; set; }
		internal OutdoorTemp _OutdoorTemp { get; set; }
		internal SupplyTemp _SupplyTemp { get; set; }
		internal IndoorTemp _IndoorTemp { get; set; }
		internal ExhaustTemp _ExhaustTemp { get; set; }
		internal List<EditorV2.PosInfo> ComponentsV2;
		//internal HumiditySens _HuniditySens { get; set; }
		public IEnumerator<object> GetEnumerator()
		{

			yield return _OutdoorTemp;
			yield return _SupplyDamper;
			yield return _SupplyFiltr;
			yield return _Recuperator;
			yield return _SupplyVent;
			yield return _WaterHeater;
			yield return _ElectroHeater;
			yield return _Froster;
			yield return _Humidifier;
			yield return _SupplyTemp;
			yield return _ExtFiltr;
			yield return _ExhaustTemp;
			yield return _ExtVent;
			yield return _ExtDamper;
			yield return _IndoorTemp;


		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		public object Clone()
        {
			return this.MemberwiseClone();
        }
		public List<dynamic> GetKIP()
        {
			
			List<dynamic> ListKIP = new List<dynamic>();
            
            foreach (var posInfo in ComponentsV2)
            {
                object component = posInfo.Tag;
				string  objname = component.GetType().Name;
				switch (objname)
                {
					case nameof(OutdoorTemp):
						//ListKIP.Add("Датчик наружной температуры");
						ListKIP.Add(component);
						break;
					
					case nameof(SupplyFiltr):
						SupplyFiltr supplyFiltr = (SupplyFiltr)component;
						if (supplyFiltr._PressureContol != null) ListKIP.Add(supplyFiltr._PressureContol);//ListKIP.Add("Датчик перепада давления на приточном фильтре");
						break;
					case nameof(Recuperator):
						Recuperator recuperator = (Recuperator)component;
						if (recuperator.protectSensor1 != null) ListKIP.Add(recuperator.protectSensor1);
						if (recuperator.protectSensor2 != null) ListKIP.Add(recuperator.protectSensor2);

						//ListKIP.Add(recuperator.protectSensor1?.Description);
						//ListKIP.Add("Датчик перепада давления на рекуператоре");
						break;
					case nameof(SupplyVent):
						SupplyVent supplyVent = (SupplyVent)component;
						if (supplyVent._PressureContol != null) ListKIP.Add(supplyVent._PressureContol);//ListKIP.Add("Датчик перепада давления на приточном вентиляторе");
						if (supplyVent._FControl != null) ListKIP.Add(supplyVent._FControl);
						

						//switch (supplyVent.ControlType)
      //                  {
						//	case Vent.AHUContolType.Soft:
						//		ListKIP.Add("Устройство плавного пуска");
						//		break;
						//	case Vent.AHUContolType.FCControl:
						//		ListKIP.Add("Регулятор оборотов");
						//		break;
						//	case Vent.AHUContolType.Transworm:
						//		ListKIP.Add("Трансформатный регулятор скорости");
						//		break;
						//}
							
						break;
					case nameof(WaterHeater):
						WaterHeater waterHeater = (WaterHeater)component;
						if (waterHeater.PS1 != null) ListKIP.Add(waterHeater.PS1);//ListKIP.Add("Датчик температуры в канале");
						if (waterHeater.PS2 != null) ListKIP.Add(waterHeater.PS2);//ListKIP.Add("Датчик температуры в обратном теплоносителе");
						
						break;
					
					case nameof(Froster):
						Froster froster = (Froster)component;
						if (froster.Sens1 != null) ListKIP.Add(froster.Sens1);//ListKIP.Add("Охладитель сенсор 1");
						if (froster.Sens2 != null) ListKIP.Add(froster.Sens2);// ListKIP.Add("Охладитель сенсор 2");
						
						break;
					case nameof(Humidifier):
						Humidifier humidifier = (Humidifier)component;
						if (humidifier.HumiditySensor != null) ListKIP.Add(humidifier.HumiditySensor);//ListKIP.Add("Увлажнитель сенсор");
						
						break;
					case nameof(SupplyTemp):
						//ListKIP.Add("Датчик температуры в приточном канале");
						ListKIP.Add(component);
						break;
					case nameof(IndoorTemp):
						//ListKIP.Add("Датчик температуры наружного воздуха");
						ListKIP.Add(component);
						break;
					case nameof(ExtVent):
						ExtVent extVent = (ExtVent) component;
						if (extVent._PressureContol != null) ListKIP.Add(extVent._PressureContol);//ListKIP.Add("Датчик перепада давления на вытяжном вентиляторе");
						if (extVent._FControl != null) ListKIP.Add(extVent._FControl);
						//switch (extVent.ControlType)
						//{
						//	case Vent.AHUContolType.Soft:
						//		ListKIP.Add("Устройство плавного пуска");
						//		break;
						//	case Vent.AHUContolType.FCControl:
						//		ListKIP.Add("Регулятор оборотов");
						//		break;
						//	case Vent.AHUContolType.Transworm:
						//		ListKIP.Add("Трансформатный регулятор скорости");
						//		break;
						//}
						break;
					case nameof(ExhaustTemp):
						ListKIP.Add(component);
						//ListKIP.Add("Датчик температуры в вытяжном канале");
						break;
					case nameof(ExtFiltr):
						ExtFiltr extFiltr = (ExtFiltr)component;
						if (extFiltr._PressureContol != null) ListKIP.Add(extFiltr._PressureContol);//ListKIP.Add("Датчик перепада давления на вытяжном фильтре");
						break;
					case nameof(Room):
						Room room = (Room)component;
						if (room._SensorT != null) ListKIP.Add(room._SensorT);
						if (room._SensorH != null) ListKIP.Add(room._SensorH);
						break;
					case nameof(CrossSection):
						CrossSection crossSection = (CrossSection)component;
						if (crossSection._SensorT != null) ListKIP.Add(crossSection._SensorT);
						if (crossSection._SensorH != null) ListKIP.Add(crossSection._SensorH);
						break;
					case nameof(SupplyDamper):
                        SupplyDamper supplyDamper = (SupplyDamper) component;
						if (supplyDamper.outdoorTemp != null) ListKIP.Add(supplyDamper.outdoorTemp);
						break;

				}
			}
            return ListKIP;

		}
		public VentSystem()
        {
			ComponentsV2 = new List<EditorV2.PosInfo>();
        }

    }




	#endregion
	#region Vent
	abstract class Vent : ElectroDevice, ICompGUID, IGetSensors
	{
        //private _Voltage VoltageVariable;
		private _PressureProtect PressureProtectVariable;
		private AHUProtect aHUProtectVariable;
		private AHUContolType ContolTypeVariable;
		internal ShemaASU ShemaASU;
		internal FControl _FControl;
        protected bool _isReserved;
        protected bool _isSpare;
        protected internal string Location;
        protected internal string AttributeSpare;
		[System.ComponentModel.TypeConverter(typeof(EnumDescConverter))]
		public enum AHUContolType
		{
			[Description("Прямой пуск")]
			Direct,
			[Description("Преобразователь частоты")]
			Soft,
			[Description("Регулятор оборотов")]
			FCControl,
			[Description("Трансформатор")]
			Transworm
		}
		[System.ComponentModel.TypeConverter(typeof(EnumDescConverter))]
		public enum AHUProtect
		{
			[Description("Нет")]
			No,
			[Description("Термоконтакт")]
			Thermokontakt,
			[Description("Позистор")]
			Posistor

		}
		
		[System.ComponentModel.TypeConverter(typeof(EnumDescConverter))]
		public enum _PressureProtect
		{
			[Description("Нет")]
			None,
			[Description("Аналоговый датчик")]
			Analogue,
			[Description("Дискретный")]
			Discrete

		}
		[DisplayName("Тип элемента")]
		public VentComponents comp { get; internal set; }

        [DisplayName("Тип управления")]
		public AHUContolType ControlType { get => ContolTypeVariable; set => CheckControl(value); }
		[DisplayName("Напряжение")]
		public new _Voltage Voltage { get => VoltageVariable; set => CheckVoltage(value); }
		[DisplayName("Мощность")]
		public new string Power { get; set; }
		[DisplayName("Защита двигателя")]
		public AHUProtect Protect { get => aHUProtectVariable; set => SetCable2Type(value); }
		[DisplayName("Датчик переда давления")]
		public _PressureProtect PressureProtect { get => PressureProtectVariable; set => CheckPressureType(value); }
		internal string SortPriority => MakeSortPriority(VoltageVariable);

        //public bool ECType { get; set; }

		internal Cable Cable2;
		internal Cable Cable3;
		internal Cable Cable4;
		internal string Description2;

		public	string PosName = "M";
		private string blockname;
		private string MakeBlockName()
		{

			blockname = string.Empty;
			{
				switch (Voltage)
				{
					case _Voltage.AC380:
						switch (ControlType)
						{
							case AHUContolType.Direct:
								switch (Protect)
								{
									case AHUProtect.Thermokontakt:
										blockname = "ED-380-TK-NORO";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										Cable2 = null;
										Cable3 = null;
										break;
									case AHUProtect.Posistor:
										blockname = "ED-380-Posistor-NORO";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										Cable2 = null;
										Cable3 = null;
										break;
									case AHUProtect.No:
									default:
										blockname = "ED-380-NORO";
										Cable2 = null;
										Cable3 = null;
										Cable4 = null;
										break;
								}
								break;
							case AHUContolType.Soft:
								switch (Protect)
								{
									case AHUProtect.Thermokontakt:
										blockname = "ED-380-TK-RO";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										Cable2 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.P,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Кабель от регулятора к вентилятору",
											WireNumbers = 5

										};
										Cable3 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.D,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Контроль перегрева регулятором",
											WireNumbers = 2

										};
										Cable4 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.D,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Сигналы пуск и авария",
											WireNumbers = 4
										};
										Cable2.MakeControlSortpriority(SortPriority + "2");
										Cable3.MakeControlSortpriority(SortPriority + "3");
										Cable4.MakeControlSortpriority(SortPriority + "4");
										break;
									case AHUProtect.Posistor:
										blockname = "ED-380-Posistor-RO";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);

										Cable2 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.P,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Кабель от регулятора к вентилятору",
											WireNumbers = 5

										};
										Cable3 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.D,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Контроль перегрева регулятором",
											WireNumbers = 2

										};
										Cable4 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.D,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Сигналы пуск и авария",
											WireNumbers = 4
										};
										Cable2.MakeControlSortpriority(SortPriority + "2");
										Cable3.MakeControlSortpriority(SortPriority + "3");
										Cable4.MakeControlSortpriority(SortPriority + "4");
										break;
									
									default:
										blockname = "ED-380-RO";
										//Cable2 = null;
										Cable2 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.P,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Кабель от регулятора к вентилятору",
											WireNumbers = 5

										};
										Cable2.MakeControlSortpriority(sortpriority + "2");
										Cable3 = null;
										Cable4 = null;
										break;

								}
								break;
							case AHUContolType.Transworm:
								switch (Protect)
								{
									case AHUProtect.Thermokontakt:
										blockname = "ED-380-TK-Transform";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										Cable2 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.P,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Кабель от регулятора к вентилятору",
											WireNumbers = 5

										};
										Cable3 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.D,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Контроль перегрева регулятором",
											WireNumbers = 2

										};
										Cable4 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.D,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Сигнал пуск",
											WireNumbers = 2
										};
										Cable2.MakeControlSortpriority(SortPriority + "2");
										Cable3.MakeControlSortpriority(SortPriority + "3");
										Cable4.MakeControlSortpriority(SortPriority + "4");
										break;
									case AHUProtect.Posistor:
										blockname = "ED-380-Posistor-Transform";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										ShemaASU.SetIO(ShemaASU.IOType.AO, 1);
										Cable2 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.P,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Кабель от регулятора к вентилятору",
											WireNumbers = 5

										};
										Cable3 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.D,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Контроль перегрева регулятором",
											WireNumbers = 2

										};
										Cable4 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.D,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Сигнал пуск",
											WireNumbers = 2
										};
										Cable2.MakeControlSortpriority(SortPriority + "2");
										Cable3.MakeControlSortpriority(SortPriority + "3");
										Cable4.MakeControlSortpriority(SortPriority + "4");
										break;
									case AHUProtect.No:
									default:
										blockname = "ED-380-Transform";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										Cable2 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.P,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Кабель от регулятора к вентилятору",
											WireNumbers = 5

										};
										Cable3 = null;
										Cable4 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.D,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Сигнал пуск",
											WireNumbers = 2
										};
										Cable2.MakeControlSortpriority(SortPriority + "2");
										Cable4.MakeControlSortpriority(SortPriority + "4");
										break;
								}
								break;
						}
						break;
					case _Voltage.AC220:
						switch (ControlType)
						{
							case AHUContolType.Direct:
								switch (Protect)
								{
									case AHUProtect.Thermokontakt:
										blockname = "ED-220-TK-NORO";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
										
										break;
									case AHUProtect.Posistor:
										blockname = "ED-220-Posistor-NORO";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
										break;
									case AHUProtect.No:
										blockname = "ED-220-NORO";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										break;

								}
								break;
							case AHUContolType.FCControl:
								switch (Protect)
								{
									case AHUProtect.Thermokontakt:
										blockname = "ED-220-TK-RO";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
										Cable2 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.P,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Кабель от регулятора к вентилятору",
											WireNumbers = 3

										};

										Cable3 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.D,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Контроль перегрева",
											WireNumbers = 2

										};
										Cable4 = null;
										Cable2.MakeControlSortpriority(SortPriority + "2");
										Cable3.MakeControlSortpriority(SortPriority + "3");
										break;
									case AHUProtect.Posistor:
										blockname = "ED-220-Posistor-RO";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
										Cable2 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.P,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Кабель от регулятора к вентилятору",
											WireNumbers = 3

										};
										Cable3 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.D,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Контроль перегрева",
											WireNumbers = 2

										};
										Cable4 = null;
										Cable2.MakeControlSortpriority(SortPriority + "2");
										Cable3.MakeControlSortpriority(SortPriority + "3");
										break;
									case AHUProtect.No:
									default:
										blockname = "ED-220-RO";
										ShemaASU.ReSetAllIO();
										ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
										Cable2 = new Cable
										{
											WriteBlock = false,
											Attrubute = Cable.CableAttribute.P,
											ToPosName = PosName,
											ToBlockName = blockname,
											Description = "Кабель от регулятора к вентилятору",
											WireNumbers = 3

										};
										Cable3 = null;
										Cable4 = null;
										Cable2.MakeControlSortpriority(SortPriority + "2");

										break;


								}
								break;
							case AHUContolType.Transworm:
								switch (Protect)
								{
									case AHUProtect.Thermokontakt:
										blockname = "ED-220-TK-Transform";
										break;
									case AHUProtect.Posistor:
										blockname = "ED-220-Posistor-Transform";
										break;
									case AHUProtect.No:
									default:
										blockname = "ED-220-Transform";
										break;

								}
								break;
							case AHUContolType.Soft:
								blockname = string.Empty;
								break;


						}
						break;
					case _Voltage.AC24:
					case _Voltage.DC24:
						blockname = "ED-220-TK-NORO";
						ShemaASU.ReSetAllIO();
						ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
						
						break;
					default:
						blockname = string.Empty;
						break;

				}
			}

			//{
			//	switch (Voltage)
			//	{
			//		case _Voltage.AC380:
			//			if (ECType) blockname = "ED-380-EC-TK";
			//			if (Protect == AHUProtect.Thermokontakt) blockname = "ED-380-TK";
			//			break;
			//		case _Voltage.AC220:
			//			blockname = "ED-220-NORO";
			//			break;
			//		default:
			//			blockname = string.Empty;
			//			break;
			//	}
			//}

			return blockname;
		}
		[DisplayName("Имя блока вентилятора")]
		public string BlockName => MakeBlockName();

        internal PressureContol _PressureContol;
		private void MakePressureSensor(_PressureProtect protect)
		{
			_PressureContol = null;
			switch (protect)
			{
				case _PressureProtect.Analogue:
					_PressureContol = new PressureContol
					{
						SensorType = Sensor.SensorType.Analogue
					};
					_PressureContol.ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
					_PressureContol.ShemaASU.ShemaPos = ShemaASUbase.DummyPos(comp);
					_PressureContol.ShemaASU.ScheUpSize = ShemaASUbase.DummySize(comp);
					
					break;
				case _PressureProtect.Discrete:
					_PressureContol = new PressureContol
					{
						SensorType = Sensor.SensorType.Discrete
					};

					_PressureContol.ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
					_PressureContol.ShemaASU.ShemaPos = ShemaASUbase.DummyPos(comp);
					_PressureContol.ShemaASU.ScheUpSize = ShemaASUbase.DummySize(comp);


					break;

			}
			switch (comp)
			{
				case VentComponents.SupplyVent:
					ShemaASU.componentPlace = ShemaASU.ComponentPlace.Supply;
					if (_PressureContol != null)
					{
						_PressureContol.SetDescrtiption("Контроль работы приточного вентилятора");
						_PressureContol.ShemaASU.componentPlace = ShemaASU.ComponentPlace.Supply;
						_PressureContol.ShemaASU.ShemaUp = "SupplyVent_PD";
					}

					break;
				case VentComponents.ExtVent:
					ShemaASU.componentPlace = ShemaASU.ComponentPlace.Exhaust;
					if (_PressureContol != null)
					{
						_PressureContol.SetDescrtiption("Контроль работы вытяжного вентилятора");
						_PressureContol.ShemaASU.componentPlace = ShemaASU.ComponentPlace.Exhaust;
						_PressureContol.ShemaASU.ShemaUp = "ExtVent_PD";
					}

					break;

			}
			switch (comp)
			{
				case VentComponents.SupplyVent:
					if (_PressureContol != null)
                    {
						_PressureContol.ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
						_PressureContol.ShemaASU.ShemaUp = "SupplyVent_PD";
					}
					
					break;
				case VentComponents.ExtVent:
					if (_PressureContol != null)
                    {
						_PressureContol.ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Exhaust;
						_PressureContol.ShemaASU.ShemaUp = "ExtVent_PD";
					}
					
					break;

			}
        }

		[DisplayName("Обозначение датчика")]
		public string PressureName => MakePressureName();

        private string MakePressureName()
		{
			string posname = string.Empty;
			switch (PressureProtect)
			{
				case _PressureProtect.Discrete:
					posname = "PDS";
					break;
				case _PressureProtect.Analogue:
					posname = "PE";
					break;
			}
			return posname;
		}
		internal string MakePressureBlockName()
		{
			//string blockname = string.Empty;
			switch (PressureProtect)
			{
				case _PressureProtect.Discrete:
					blockname = "SENS-PDS-2WIRE";
					break;
				case _PressureProtect.Analogue:
					blockname = "SENS-PE-2WIRE";
					break;

			}
			return blockname;
		}
		[DisplayName("Имя блока датчика")]
		public string PressureBlockname => MakePressureBlockName();

        public string GUID { get; set; }
		private void CheckPressureType(object val)
		{
			PressureProtectVariable = (_PressureProtect)val;

			MakePressureSensor(PressureProtectVariable);
        }
		private void SetCable2Type(object val)
		{
			AHUProtect aHU = (AHUProtect)val;
			aHUProtectVariable = aHU;
			switch (aHU)
			{
				case AHUProtect.Thermokontakt:
					Cable4 = new Cable
					{
						Attrubute = Cable.CableAttribute.D,
						WriteBlock = false,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = Description2,
						WireNumbers = 2

					};
					Cable4.MakeControlSortpriority(sortpriority + "4");
					ShemaASU.ReSetIO(ShemaASU.IOType.AI);
					ShemaASU.SetIO(ShemaASU.IOType.DI, 1);


					break;
				case AHUProtect.Posistor:
					Cable4 = new Cable
					{
						Attrubute = Cable.CableAttribute.A,
						WriteBlock = false,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = Description2,
						WireNumbers = 2
					};
					Cable4.MakeControlSortpriority(sortpriority + "4");
					ShemaASU.ReSetIO(ShemaASU.IOType.DI);
					ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
					break;
				case AHUProtect.No:
					Cable4 = null;
					break;


			}
		}
		private void CheckVoltage(object val)
		{
			VoltageVariable = (_Voltage)val;
			if (Cable1 != null)
			{
				Cable1.SortPriority = SortPriority; Cable1.ToBlockName = BlockName;
				switch (VoltageVariable)
				{
					case _Voltage.AC380:
						Cable1.WireNumbers = 5;
						break;
					case _Voltage.AC220:
					case _Voltage.AC24:
					case _Voltage.DC24:
						Cable1.WireNumbers = 3;
						break;
				}
			}
			if (Cable4 != null) { Cable4.MakeControlSortpriority(SortPriority + "4"); Cable4.ToBlockName = BlockName; }
        }
		internal abstract string MakeSortPriority(_Voltage voltage);

		private void CheckControl(object val)
		{
			ContolTypeVariable = (AHUContolType)val;
			if (Cable1 != null) { Cable1.ToBlockName = BlockName; }
			if (Cable4 != null) { Cable4.ToBlockName = BlockName; }


			GetShemaAsuShemaUp(comp);

			switch (ContolTypeVariable)
			{
				case AHUContolType.Soft:
					_FControl = new FControl();
					Voltage = _Voltage.AC380;
					break;
				case AHUContolType.FCControl:
					_FControl = new FControl();
					Voltage = _Voltage.AC220;
					break;
				default:
                    _FControl = null;
					break;
                    
			}

			
			//нужно переделывать имя блока у схемы автоматизации в зависимости от П/В и есть РО или нет
			
			void GetShemaAsuShemaUp (VentComponents ventComponents)
            {

                

				switch (ventComponents)
                {
					case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.Direct && !_isSpare:
						ShemaASU.ShemaUp = "Supply_Vent";
						ShemaASU.Description1 = "Управление приточным вентилятором";
						break;
					case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.FCControl && !_isSpare:
					case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.Soft && !_isSpare:
					case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.Transworm && !_isSpare:
						ShemaASU.ShemaUp = "Supply_Vent_RO";
						ShemaASU.Description1 = "Пуск и управление скоростью приточного вентилятора";
						break;
					case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.Direct && !_isSpare:
						ShemaASU.ShemaUp = "Ext_Vent";
						ShemaASU.Description1 = "Управление вытяжным вентилятором";
						break;
					case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.FCControl && !_isSpare:
					case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.Soft && !_isSpare:
					case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.Transworm && !_isSpare:
						ShemaASU.ShemaUp = "Ext_Vent_RO";
						ShemaASU.Description1 = "Пуск и управление скоростью вытяжного вентилятора";
						break;
					case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.Direct && _isSpare && !_isReserved:
                        ShemaASU.ShemaUp = "Supply_Vent_Spare";
                        ShemaASU.Description1 = "Управление основным приточным вентилятором";
                        break;
                    case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.FCControl && _isSpare && !_isReserved:
                    case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.Soft && !_isSpare:
                    case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.Transworm && !_isSpare:
                        ShemaASU.ShemaUp = "Supply_Vent_Spare_RO";
                        ShemaASU.Description1 = "Управление скоростьью основного приточного вентилятора";
                        break;
                    case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.Direct && _isSpare && !_isReserved:
                        ShemaASU.ShemaUp = "Ext_Vent_Spare";
						ShemaASU.Description1 = "Управление основным вытяжным вентилятором";
                        break;
                    case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.FCControl && _isSpare && !_isReserved:
                    case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.Soft && _isSpare && !_isReserved:
                    case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.Transworm && _isSpare && !_isReserved:
                        ShemaASU.ShemaUp = "Ext_Vent_Spare_RO";
                        ShemaASU.Description1 = "Управление скоростьью основного вытяжного вентилятора";
                        break;
                    case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.Direct && _isSpare && _isReserved:
                        ShemaASU.ShemaUp = "Supply_Vent_Spare";
                        ShemaASU.Description1 = "Управление резервным приточным вентилятором";
                        break;
                    case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.FCControl && _isSpare && _isReserved:
                    case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.Soft && _isSpare:
                    case VentComponents.SupplyVent when ContolTypeVariable == AHUContolType.Transworm && _isSpare:
                        ShemaASU.ShemaUp = "Supply_Vent_Spare_RO";
                        ShemaASU.Description1 = "Управление скоростьью резервеного приточного вентилятора";
                        break;
                    case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.Direct && _isSpare && _isReserved:
                        ShemaASU.ShemaUp = "Ext_Vent_Spare";
                        ShemaASU.Description1 = "Управление резервным вытяжным вентилятором";
                        break;
                    case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.FCControl && _isSpare && _isReserved:
                    case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.Soft && _isSpare && _isReserved:
                    case VentComponents.ExtVent when ContolTypeVariable == AHUContolType.Transworm && _isSpare && _isReserved:
                        ShemaASU.ShemaUp = "Ext_Vent_Spare_RO";
                        ShemaASU.Description1 = "Управление скоростьью резервного вытяжного вентилятора";
						break;


				}
				
				
            }
        }
		internal class FControl : IVendoInfo, ICompGUID
		{

			string VendorName;
			string VendorDescription;
			string DBTable;
			string MainDBTable;
			string ID;
			string Assignment;
            readonly string DefaultDescription = "Регулятор оборотов вентилятора";
			public string GUID { get; set; }
			internal string Description { get; set; }
			public (string VendorName, string ID, string VendorDescription, string DBTable, string Assignment, string DefaultDescription, string MainDBTable) GetVendorInfo()
			{
				return (this.VendorName, this.ID, this.VendorDescription, this.DBTable, this.Assignment, this.DefaultDescription, this.MainDBTable);
			}
			public void SetVendorInfo(string vendorName, string ID, string vendorDescription, string dbTable, string assignment)
			{
				if (!string.IsNullOrEmpty(vendorName)) this.VendorName = vendorName;
				if (!string.IsNullOrEmpty(ID)) this.ID = ID;
				if (!string.IsNullOrEmpty(vendorDescription)) this.VendorDescription= vendorDescription;
				if (!string.IsNullOrEmpty(assignment)) this.Assignment = assignment;
				this.DBTable = string.IsNullOrEmpty(dbTable) ? "FControl" : dbTable;
				this.MainDBTable = "FControl";

			}
			public void ClearVendorInfo()
            {
				this.VendorName = string.Empty;
				this.ID = string.Empty;
				this.VendorDescription = string.Empty;
				this.Assignment = string.Empty;
				this.DBTable = string.Empty;
				


			}
			internal FControl()
            {
				this.DBTable = "FControl";
				Description = "Регулятор оборотов вентилятора";

			}
			
		 }
		
		public List<dynamic> GetSensors()
		{
			return new List<dynamic>
			{
				this._PressureContol,
				this._FControl
			};

		}

	}

    internal class SupplyVent : Vent, IPower
    {
        
		
        public SupplyVent()
		{
            Location = "Supply";

			comp = VentComponents.SupplyVent;
			ShemaASU =  ShemaASU.CreateBaseShema(comp);
			ShemaASU.ShemaUp = "Supply_Vent";
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
			ShemaASU.SetIO(ShemaASU.IOType.DO, 1);

			Power = "0";
			VoltageVariable = _Voltage.AC220;
			Description = "Э/д приточного вентилятора";
			Description2 = "Защита э/д приточ.вент.";
			PressureProtect = _PressureProtect.Discrete;
			
			Cable1 = new Cable
			{
				Attrubute = Cable.CableAttribute.P,
				WriteBlock = true,
				ToPosName = PosName,
				ToBlockName = BlockName,
				Description = Description,
				SortPriority = SortPriority,
				WireNumbers = 3
			};
			
			
		}
        public SupplyVent(bool isReserved, bool isSpared)
        {
            _isReserved = isReserved;
            _isSpare = isSpared;
            Location = "Supply";
			comp = VentComponents.SupplyVent;
            ShemaASU = ShemaASU.CreateBaseShema(comp);
            ShemaASU.ShemaUp = isSpared ? "Supply_Vent_Spare": "Supply_Vent";
            ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
            ShemaASU.SetIO(ShemaASU.IOType.DO, 1);

            Power = "0";
            VoltageVariable = _Voltage.AC220;
            Description = isReserved ? "Э/д резервного приточного вентилятора": "Э/д основного приточного вентилятора";
            Description2 = isReserved ? "Защита э/д резервн. приточн.вент." : "Защита э/д осн. приточ.вент.";
            PressureProtect = _PressureProtect.Discrete;

            Cable1 = new Cable
            {
                Attrubute = Cable.CableAttribute.P,
                WriteBlock = true,
                ToPosName = PosName,
                ToBlockName = BlockName,
                Description = Description,
                SortPriority = SortPriority,
                WireNumbers = 3
            };


        }
		internal override string MakeSortPriority(_Voltage voltage)
		{

			string additional = string.Empty;

            if (_isReserved)
            {
                additional = "6";
				
            }
            switch (voltage)
			{
				case _Voltage.AC380:
					sortpriority = "01" + additional;
					break;
				default:
					sortpriority = "06" + additional;
					break;

			}

			return sortpriority;

		}
	}

    class SpareSuplyVent : IPower, ICompGUID, IGetSensors
	{
        private ElectroDevice._Voltage voltage;
        private string power;
        private Vent.AHUProtect protect;
        private Vent.AHUContolType contolType;
        internal string Location;
        
		[DisplayName("Тип элемента")]
        public VentComponents comp { get; internal set; }
        [DisplayName("Тип управления")]
        public Vent.AHUContolType ControlType
        {
            get => contolType;
            set
            {
                contolType = value;
                switch (contolType)
                {
					case Vent.AHUContolType.Soft:
					case Vent.AHUContolType.Transworm:
                        Voltage = ElectroDevice._Voltage.AC380;
                        if (MainSupplyVent._FControl != null) MainSupplyVent._FControl.Description = "Регулятор оборотов основного приточного вентилятора";
                        if (ReservedSupplyVent._FControl != null) ReservedSupplyVent._FControl.Description = "Регулятор оборотов резервного приточного вентилятора";
						break;
					case Vent.AHUContolType.FCControl:
                        Voltage = ElectroDevice._Voltage.AC220;
						break;
                }
                MainSupplyVent.ControlType = contolType ;
                ReservedSupplyVent.ControlType = contolType;
            }
        }

        [DisplayName("Напряжение ")]
        public ElectroDevice._Voltage Voltage
        {
            get =>voltage;
            set
            {
                switch (value)
                {
					case ElectroDevice._Voltage.AC24:
					case ElectroDevice._Voltage.DC24:
                        voltage = ElectroDevice._Voltage.AC220;
                        MainSupplyVent.Voltage = voltage;
                        ReservedSupplyVent.Voltage = voltage;
						break;
					default:
                        voltage= value;
                        MainSupplyVent.Voltage = voltage;
                        ReservedSupplyVent.Voltage = voltage;
						break;
                }

                switch (ControlType)
                {
					case Vent.AHUContolType.Soft:
					case Vent.AHUContolType.Transworm:
                        voltage = ElectroDevice._Voltage.AC380;
                        MainSupplyVent.Voltage = voltage;
                        ReservedSupplyVent.Voltage = voltage;
						break;
					case Vent.AHUContolType.FCControl:
                        voltage = ElectroDevice._Voltage.AC220;
                        MainSupplyVent.Voltage = voltage;
                        ReservedSupplyVent.Voltage = voltage;
						break;
				}
            }
        }
        [DisplayName("Мощность")]
		public string Power
        {
            get => power;
            set
            {
                power = double.TryParse(value, out double _)? value: "0";
                MainSupplyVent.Power = power;
                ReservedSupplyVent.Power = power;
            }
        }
        
        [DisplayName("Защита двигателя")]
        public Vent.AHUProtect Protect
        {
            get => protect;
            set
            {
                protect = value;
                MainSupplyVent.Protect = protect;
                ReservedSupplyVent.Protect = protect;
				
            }
        }
		[DisplayName("Имя блока основного")]
		public string BlockNameMain => MainSupplyVent.BlockName;

        [DisplayName("Имя блока резевного")]
        public string BlockNameReserved => ReservedSupplyVent?.BlockName;
        public string GUID { get; set; }

		internal SupplyVent MainSupplyVent;
        internal SupplyVent ReservedSupplyVent;
        internal ShemaASU shemaAsu;
        internal PressureContol _PressureContol;
		internal static string Description = "Сдвоенный приточный вентилятор";
        public SpareSuplyVent()
         {
			comp = VentComponents.SpareSuplyVent;
            power = "0";
            Location = "Supply";

			MainSupplyVent = new SupplyVent(false, true) //1 вентилятор
            {
                Voltage = ElectroDevice._Voltage.AC380,
				PressureProtect = Vent._PressureProtect.None,
				Description = "Э/д основного приточного вентилятора",
				Location = Location,
				AttributeSpare = "Main"


			};
            ReservedSupplyVent = new SupplyVent(true, true) //2 вентилятор
            {
                Voltage = ElectroDevice._Voltage.AC380,
				PressureProtect = Vent._PressureProtect.None,
				Description = "Э/д резервного приточного вентилятора",
				Location = Location,
				AttributeSpare = "Reserved"
			};
           
            shemaAsu = new ShemaASU
            {
                ShemaUp = "Supply_Vent_Spare"
			};

            _PressureContol = new PressureContol //датчик Р
            {
                _SensorType = Sensor.SensorType.Discrete,
				Description = "Датчик перепада давления на сдвоенном приточном вентиляторе"
            };
            _PressureContol.Cable1.Description = _PressureContol.Description;
         }
        public List<dynamic> GetSensors()
         {
			return new List<dynamic>
            {
                this._PressureContol,
				MainSupplyVent._FControl,
				ReservedSupplyVent._FControl
            };

           
		}
    }

    class SpareExtVent : IPower, ICompGUID, IGetSensors
	{
		private ElectroDevice._Voltage voltage;
		private string power;
		private Vent.AHUProtect protect;
		private Vent.AHUContolType contolType;
        internal string Location;

		[DisplayName("Тип элемента")]
		public VentComponents comp { get; internal set; }
		[DisplayName("Тип управления")]
		public Vent.AHUContolType ControlType
		{
			get => contolType;
			set
			{
				contolType = value;
                switch (contolType)
                {
					case Vent.AHUContolType.Soft:
					case Vent.AHUContolType.Transworm:
                        Voltage = ElectroDevice._Voltage.AC380;
                        if (MainExtVent._FControl != null) MainExtVent._FControl.Description = "Регулятор оборотов основного вытяжного вентилятора";
                        if (ReservedExtVent._FControl != null) ReservedExtVent._FControl.Description = "Регулятор оборотов резервного приточного вентилятора";
						break;
					case Vent.AHUContolType.FCControl:
                        Voltage = ElectroDevice._Voltage.AC220;
						break;
                }
				MainExtVent.ControlType = contolType;
				ReservedExtVent.ControlType = contolType;
			}
		}

		[DisplayName("Напряжение ")]
		public ElectroDevice._Voltage Voltage
		{
			get => voltage;
			set
			{
				switch (value)
				{
					case ElectroDevice._Voltage.AC24:
					case ElectroDevice._Voltage.DC24:
						voltage = ElectroDevice._Voltage.AC220;
						ReservedExtVent.Voltage = voltage;
						ReservedExtVent.Voltage = voltage;
						break;
					default:
						voltage = value;
						MainExtVent.Voltage = voltage;
						ReservedExtVent.Voltage = voltage;
						break;
				}

                switch (ControlType)
                {
					case Vent.AHUContolType.Soft:
					case Vent.AHUContolType.Transworm:
                        voltage = ElectroDevice._Voltage.AC380;
                        MainExtVent.Voltage = voltage;
                        ReservedExtVent.Voltage = voltage;
						break;
					case Vent.AHUContolType.FCControl:
                        voltage = ElectroDevice._Voltage.AC220;
                        MainExtVent.Voltage = voltage;
                        ReservedExtVent.Voltage = voltage;
						break;
				}
			}
		}
		[DisplayName("Мощность")]
		public string Power
		{
			get => power;
			set
			{
				power = double.TryParse(value, out double _) ? value : "0";
				MainExtVent.Power = power;
				ReservedExtVent.Power = power;
			}
		}

		[DisplayName("Защита двигателя")]
		public Vent.AHUProtect Protect
		{
			get => protect;
			set
			{
				protect = value;
				
                MainExtVent.Protect = protect;
				ReservedExtVent.Protect = protect;

			}
		}
		[DisplayName("Имя блока основного")]
		public string BlockNameMain => MainExtVent.BlockName;

		[DisplayName("Имя блока резевного")]
		public string BlockNameReserved => ReservedExtVent?.BlockName;
		public string GUID { get; set; }

		internal ExtVent MainExtVent;
		internal ExtVent ReservedExtVent;
		internal ShemaASU shemaAsu;
		internal PressureContol _PressureContol;
		internal static string Description = "Сдвоенный вытяжной вентилятор";


		public SpareExtVent()
		{
			comp = VentComponents.SpareExtVent;
			power = "0";
            Location = "Exhaust";

			MainExtVent = new ExtVent(false, true) //1 вентилятор
			{
				Voltage = ElectroDevice._Voltage.AC380,
				PressureProtect = Vent._PressureProtect.None,
				Description = "Э/д основного вытяжного вентилятора",
				Location = Location,
				AttributeSpare = "Main"


			};
			ReservedExtVent = new ExtVent(true, true) //2 вентилятор
			{
				Voltage = ElectroDevice._Voltage.AC380,
				PressureProtect = Vent._PressureProtect.None,
				Description = "Э/д резервного вытяжного вентилятора",
				Location = Location,
                AttributeSpare = "Reserved"
			};

			shemaAsu = new ShemaASU
			{
				ShemaUp = "Ext_Vent_Spare"
			};

			_PressureContol = new PressureContol //датчик Р
			{
				_SensorType = Sensor.SensorType.Discrete,
				Description = "Датчик перепада давления на сдвоенном вытяжном вентиляторе"
			};
			_PressureContol.Cable1.Description = _PressureContol.Description;
		}


		public List<dynamic> GetSensors()
		{
			return new List<dynamic>
			{
				this._PressureContol,
				MainExtVent._FControl,
				ReservedExtVent._FControl
			};


		}
	}
	class ExtVent : Vent, IPower
	{
		public static string ImagePath = VentSystem.AppFolder + "Images\\4_1.png";
		public static string ImageNullPath = VentSystem.AppFolder + "Images\\4_0.png";

		public ExtVent()
		{
            Location = "Exhaust";
			comp = VentComponents.ExtVent;
			ShemaASU = ShemaASU.CreateBaseShema(comp);
			ShemaASU.ShemaUp = "Ext_Vent";
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Exhaust;
			ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
			Power = "0";
			VoltageVariable = _Voltage.AC220;
			Description = "Э/д вытяжного вентилятора";
			Description2 = "Защита э/д вытяжн.вент.";
			PressureProtect = _PressureProtect.Discrete;
			Cable1 = new Cable
			{
				Attrubute = Cable.CableAttribute.P,
				WriteBlock = true,
				ToPosName = PosName,
				ToBlockName = BlockName,
				Description = Description,
				SortPriority = SortPriority,
				WireNumbers = 3
			};

			
			


		}
        public ExtVent(bool isReserved, bool isSpared)
        {
            _isReserved = isReserved;
            _isSpare = isSpared;
            Location = "Exhaust";
			comp = VentComponents.ExtVent;
            ShemaASU = ShemaASU.CreateBaseShema(comp);
            ShemaASU.ShemaUp = isSpared? "Supply_Vent_Spare" : "Ext_Vent";
            ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Exhaust;
            ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
            Power = "0";
            VoltageVariable = _Voltage.AC220;
            Description = "Э/д вытяжного вентилятора";
            Description2 = "Защита э/д вытяжн.вент.";
            PressureProtect = _PressureProtect.Discrete;
            Cable1 = new Cable
            {
                Attrubute = Cable.CableAttribute.P,
                WriteBlock = true,
                ToPosName = PosName,
                ToBlockName = BlockName,
                Description = Description,
                SortPriority = SortPriority,
                WireNumbers = 3
            };
        }
		internal override string MakeSortPriority(_Voltage voltage)
		{
            string additional = string.Empty;

            if (_isReserved)
            {
                additional = "6";

            }
			switch (voltage)
			{
				case _Voltage.AC380:
					sortpriority = "02" + additional;
					break;
				default:
					sortpriority = "07" + additional;
					break;

			}
			return sortpriority;

		}


	}
	#endregion
	#region Filtr
	class Filtr : ICompGUID, IGetSensors
	{
		private protected VentComponents ComponentVariable;
		internal ShemaASU ShemaASU;
		[System.ComponentModel.TypeConverter(typeof(EnumDescConverter))]
		//public enum _PressureProtect
		//{
		//	[Description("Нет")]
		//	None,
		//	[Description("Аналоговый датчик")]
		//	Analogue,
		//	[Description("Дискретный")]
		//	Discrete
		//}
		private Sensor.SensorType PressureProtectVariable;

		internal PressureContol _PressureContol;
		[DisplayName("Тип элемента")]
		public VentComponents comp => ComponentVariable;

        [DisplayName("Датчик перепада давления")]
		public Sensor.SensorType PressureProtect { get => PressureProtectVariable;
            set => CheckPressureType(value);
        }
		internal string Description { get; set; }

		[DisplayName("Обозначение")]
		public string PosName { get; internal set; }


		[DisplayName("Имя блока")]
		public string Blockname { get; internal set; }
		public string GUID { get; set; }
		private void CheckPressureType(object val)
		{
			PressureProtectVariable = (Sensor.SensorType)val;
			if (PressureProtectVariable != Sensor.SensorType.No)
			{
				MakePressureSensor(PressureProtectVariable);
			}
			else
			{
				Blockname = string.Empty;
				PosName = string.Empty;
			}

			//return PressureProtectVariable;
		}
		private void MakePressureSensor(Sensor.SensorType protect)
		{
			_PressureContol = null;
			switch (protect)
			{
				case Sensor.SensorType.Analogue:
					_PressureContol = new PressureContol()
					{
						SensorType = Sensor.SensorType.Analogue
					};
					_PressureContol.ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
					break;
				case Sensor.SensorType.Discrete:
					_PressureContol = new PressureContol()
					{
						SensorType = Sensor.SensorType.Discrete
					};
					_PressureContol.ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
					break;
			}

            if (_PressureContol == null) return;
            _PressureContol.CheckSensorType(protect);
            switch (comp)
            {
                case VentComponents.SupplyFiltr:
                    if (_PressureContol != null)
                    {
                        _PressureContol.Description = "Контроль чистоты приточного фильтра";
                        _PressureContol.ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
                        _PressureContol.ShemaASU.ShemaUp = "Supply_Filtr_PD";
                        _PressureContol.ShemaASU.ShemaPos = ShemaASUbase.DummyPos(comp);
                    }

                    break;
                case VentComponents.ExtFiltr:
                    if (_PressureContol != null)
                    {
                        _PressureContol.Description = "Контроль чистоты вытяжного фильтра";
                        _PressureContol.ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Exhaust;
                        _PressureContol.ShemaASU.ShemaUp = "Ext_Filtr_PD";
                        _PressureContol.ShemaASU.ShemaPos = ShemaASUbase.DummyPos(comp);
                    }

                    break;
                default:
                    _PressureContol.Description = "Контроль чистоты фильтра";
                    _PressureContol.ShemaASU.ShemaUp = "Supply_Filtr_PD";
                    break;
            }

            if (_PressureContol == null) return;
            Blockname = _PressureContol.Cable1.ToBlockName;
            PosName = _PressureContol.Cable1.ToPosName;
            _PressureContol.SetDescrtiption(_PressureContol.Description);
        }
		public List<dynamic> GetSensors()
		{
			return new List<dynamic>
			{
				this._PressureContol
			};

		}
		public Filtr() //это базовый класс фильтр
        {
			ComponentVariable = VentComponents.Filtr;
			ShemaASU = ShemaASU.CreateBaseShema(comp);
			ShemaASU.ShemaUp = "Filtr";
			Description = "Фильтр";
			PressureProtect = Sensor.SensorType.Discrete;
		}
	}

    internal class SupplyFiltr : Filtr //это производный для притока. они нужны были изначально чтобы хранить в себе разные картиники
	{
		public static string Imagepath = VentSystem.AppFolder + "Images\\2_1.png";
		public static string ImageNullPath = VentSystem.AppFolder + "Images\\2_0.png";
		public SupplyFiltr()
		{
			ComponentVariable = VentComponents.SupplyFiltr;
			ShemaASU = ShemaASU.CreateBaseShema(comp);
			ShemaASU.ShemaUp = "Filtr";
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;

			Description = "Приточный фильтр";

			PressureProtect = Sensor.SensorType.Discrete;
		}
	}

    internal class ExtFiltr : Filtr //это производный для вытяжки
	{
		public static string Imagepath = VentSystem.AppFolder + "Images\\5_1.png";
		public static string ImageNullPath = VentSystem.AppFolder + "Images\\5_0.png";
		public ExtFiltr()
		{
			ComponentVariable = VentComponents.ExtFiltr;
			Description = "Вытяжной фильтр";
			ShemaASU = ShemaASU.CreateBaseShema(comp);
			ShemaASU.ShemaUp = "Filtr";
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Exhaust;
			PressureProtect = Sensor.SensorType.Discrete;
			
			ShemaASU.ShemaUp = "Filtr";
			
		}

	}
	#endregion
	#region Damper

    internal abstract class Damper : ElectroDevice, ICompGUID
	{
		internal ShemaASU ShemaASU;
		[DisplayName("Обозначение")]
		public string PosName { get; internal set; }
		[DisplayName("Имя блока")]
		public string BlockName => MakeDamperBlockName();

        internal abstract string MakeDamperBlockName();
		internal string Description1 { get; set; }
		internal string Description2 { get; set; }
		public string GUID { get; set; }
		internal Cable Cable2;
		//internal CableTypes Cable2Type { get; set; }
		protected internal bool hascontrol;

		protected internal void SetCable2Type(object val)
		{
			hascontrol = Convert.ToBoolean(val);
			if (hascontrol)
			{
				Cable2 = new Cable
				{
					Attrubute = Cable.CableAttribute.D,
					WriteBlock = false,
					ToPosName = PosName,
					ToBlockName = BlockName,
					Description = Description2,
					WireNumbers = 4


				};
				Cable2.MakeControlSortpriority(sortpriority);
				Cable1.ToBlockName = BlockName;
				ShemaASU.SetIO(ShemaASU.IOType.DI, 1);


			}
			else
			{
				Cable2 = null;
				Cable1.ToBlockName = BlockName;
				ShemaASU.ReSetIO(ShemaASU.IOType.DI);

			}


		}


	}

    internal class SupplyDamper : Damper, IPower
	{
		public static string ImagePath = VentSystem.AppFolder + "Images\\3_1.png";
		public static string ImageNullPath = VentSystem.AppFolder + "Images\\3_0.png";
		internal OutdoorTemp outdoorTemp;
		private Sensor.SensorType sensorType;
		[DisplayName("Тип элемента")]
		public VentComponents comp { get => ComponentVariable;
            internal set => ComponentVariable = value;
        }
		private VentComponents ComponentVariable;
		//private _Voltage VoltageVariable;
		public SupplyDamper()
		{

			comp = VentComponents.SupplyDamper;
			ShemaASU = ShemaASU.CreateBaseShema(comp);
			ShemaASU.ShemaUp = "Supply_Damper";
			ShemaASU.componentPlace = ShemaASU.ComponentPlace.Supply;
			ShemaASU.ShemaPos = ShemaASUbase.DummyPos(comp);
			ShemaASU.ScheUpSize = ShemaASUbase.DummySize(comp);
			PosName = "M";
			VoltageVariable = _Voltage.AC220;
			Power = "3";
			Description1 = "Э/д приточной заслонки";
			Cable1 = new Cable
			{
				Attrubute = Cable.CableAttribute.P,
				WriteBlock = true,
				ToPosName = PosName,
				ToBlockName = BlockName,
				SortPriority = SortPriority,
				Description = Description1,
				WireNumbers = 3

			};

			
			
			ShemaASU.SetIO(ShemaASU.IOType.DO, 1);



		}
		[DisplayName("Возвратная пружина")]
		public bool Spring { get; set; }
		[DisplayName("Концевые выключатели")]
		public bool HasContol { get => hascontrol;
            set => SetCable2Type(value);
        }
		[DisplayName("Тип датчика температуры")]
		public Sensor.SensorType SensorType { get => sensorType;
            internal set => SetSensorType(value);
        }
		[DisplayName("Имя блока датчика температуры")]
		public string SensorBlockName => outdoorTemp?.Blockname;

        internal bool SetSensor { set => CreateSensor(); }
		public new _Voltage Voltage { get => VoltageVariable;
            set => VoltageVariable = value;
        }
		private void SetSensorType(object val)
        {
			if (outdoorTemp != null)
            {
				sensorType = (Sensor.SensorType)val;
				outdoorTemp._SensorType = sensorType;
			}
            else
            {
                sensorType = Sensor.SensorType.No;
            }
        }
		private void CreateSensor()
        {
			if (outdoorTemp == null)
            {
                outdoorTemp = new OutdoorTemp
                {
                    _SensorType = Sensor.SensorType.Analogue,
					Location = "Outdoor"
                };
                sensorType = outdoorTemp._SensorType;
				
				
            }
        }

		internal override string MakeDamperBlockName()
		{
			string blockname;
			if (HasContol)
			{
				blockname = "ZASL-220_SQ";
				Description2 = "Контроль положения приточной заслонки";
			}
			else
			{
				blockname = "ZASL-220";
			}
			return blockname;
		}
		internal string SortPriority { get { return MakeSortPriority(); } }
		private string MakeSortPriority()
		{
			switch (this.Voltage)
			{
				case _Voltage.AC220:
					sortpriority = "12";
					break;
				default:
					sortpriority = "15";
					break;

			}
			if (Cable1 != null) Cable1.SortPriority = sortpriority;
			if (Cable2 != null) Cable2.MakeControlSortpriority(sortpriority);
			return sortpriority;

		}

	}
	class ExtDamper : Damper, IPower
	{
		public static string ImagePath = VentSystem.AppFolder + "Images\\6_1.png";
		public static string ImageNullPath = VentSystem.AppFolder + "Images\\6_0.png";
		[DisplayName("Тип элемента")]
		public VentComponents comp { get { return ComponentVariable; } internal set { ComponentVariable = value; } }
		private VentComponents ComponentVariable;
		//private _Voltage VoltageVariable;
		public ExtDamper()
		{

			comp = VentComponents.ExtDamper;
			ShemaASU = ShemaASU.CreateBaseShema(comp);
			ShemaASU.ShemaUp = "Ext_Damper";
			ShemaASU.componentPlace = ShemaASU.ComponentPlace.Exhaust;
			
			PosName = "M";
			VoltageVariable = _Voltage.AC220;
			Power = "3";
			Description1 = "Э/д вытяжной заслонки";
			Description = Description1;
			Cable1 = new Cable
			{
				Attrubute = Cable.CableAttribute.P,
				WriteBlock = true,
				ToPosName = PosName,
				ToBlockName = BlockName,
				SortPriority = SortPriority,
				Description = Description1,
				WireNumbers = 3

			};
			
			ShemaASU.SetIO(ShemaASU.IOType.DO, 1);

		}
		[DisplayName("Возвратная пружина")]
		public bool Spring { get; set; }

		[DisplayName("Концевые выключатели")]
		public bool HasContol { get { return hascontrol; } set { SetCable2Type(value); } }
		public new _Voltage Voltage { get { return VoltageVariable; } set { VoltageVariable = value; } }
		internal override string MakeDamperBlockName()
		{
			string blockname;
			if (hascontrol)
			{
				blockname = "ZASL-220_SQ";
				Description2 = "Контроль положения вытяжной заслонки";
			}
			else
			{
				blockname = "ZASL-220";
			}

			return blockname;
		}
		internal string SortPriority { get { return MakeSortPriority(); } }
		private string MakeSortPriority()
		{
            switch (this.Voltage)
			{
				case _Voltage.AC220:
					sortpriority = "13";
					break;
				default:
					sortpriority = "16";
					break;

			}
			if (Cable1 != null) Cable1.SortPriority = sortpriority;
			if (Cable2 != null) Cable2.MakeControlSortpriority(sortpriority);
			return sortpriority;

		}

	}
	#endregion
	#region Heaters
	class WaterHeater : IEnumerable, ICompGUID, IGetSensors
	{

		[System.ComponentModel.TypeConverter(typeof(EnumDescConverter))]
		public enum WaterHeaterProtect
		{
			[Description("Нет")]
			No,
			[Description("В канале")]
			Duct,
			[Description("На обрат.воде")]
			BackWater,
			[Description("В канале и обрат.воде")]
			Duct_BackWater
		}
		public static string Imagepath = VentSystem.AppFolder + "Images\\7_1.png"; //это пути для картинок
		public static string ImageNullPath = VentSystem.AppFolder + "Images\\7_0.png";

		private VentComponents ComponentVariable;
		internal Valve.ValveType valveType;
		private bool pumpTK;
		internal ShemaASU ShemaASU;
		internal SensorT PS1;
		internal SensorT PS2;
		private Sensor.SensorType PS1sensorType;
		private Sensor.SensorType PS2sensorType;
		internal Pump _Pump;
		internal Valve _Valve;

		[DisplayName("Тип элемента")]
		public VentComponents comp { get => ComponentVariable;
            private set => ComponentVariable = value;
        }
		private WaterHeaterProtect waterprotect;
		[DisplayName("Тип клапана")]
		public Valve.ValveType _valveType { get => valveType;
            set => CheckValveType(value);
        }
		[DisplayName("Термоконтакты насоса")]
		public bool HasTK { get => pumpTK;
            set => PumpSetTK(value);
        }

		[DisplayName("Защита нагревателя")]
		public WaterHeaterProtect Waterprotect
		{


			get => waterprotect;
            set
			{
				waterprotect = value;
				PS1 = null;
				PS2 = null;
				switch (waterprotect)
				{
					case WaterHeaterProtect.Duct:
						PS1 = new SensorT("Защита нагревателя по Т в канале") // CreateProtect(waterprotect);
						{
							_SensorType = Sensor.SensorType.Discrete
						};
						PS1sensorType = PS1._SensorType;

						PS1.ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
						PS1.ShemaASU.ShemaUp = "Supply_WaterHeater_TempAir";
						PS1.ShemaASU.ShemaPos = ShemaASUbase.DummyPos(VentComponents.WaterHeater);

						PS2sensorType = Sensor.SensorType.No;
						break;
					case WaterHeaterProtect.BackWater:
						PS2 = new SensorT("Защита нагревателя по Т обратной воды") //CreateProtect(waterprotect);
						{
							_SensorType = Sensor.SensorType.Analogue
						};
						PS2sensorType = PS2._SensorType;

						PS2.ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
						PS2.ShemaASU.ShemaUp = "Supply_WaterHeater_TempBackWater";
						PS2.ShemaASU.ShemaPos = ShemaASUbase.DummyPos(VentComponents.WaterHeater);



						PS1sensorType = Sensor.SensorType.No;
						break;
					case WaterHeaterProtect.Duct_BackWater:
						PS1 = new SensorT("Защита нагревателя по Т в канале")
						{
							_SensorType = Sensor.SensorType.Discrete,
                            ShemaASU =
                            {
                                componentPlace = ShemaASU.ComponentPlace.Supply,
                                ShemaUp = "Supply_WaterHeater_TempAir",
                                ShemaPos = ShemaASUbase.DummyPos(VentComponents.WaterHeater)
                            }
                        }; //CreateProtect(waterprotect);

                        PS2 = new SensorT("Защита нагревателя по Т обратной воды")
						{
							_SensorType = Sensor.SensorType.Analogue,
                            ShemaASU =
                            {
                                componentPlace = ShemaASU.ComponentPlace.Supply,
                                ShemaUp = "Supply_WaterHeater_TempBackWater",
                                ShemaPos = ShemaASUbase.DummyPos(VentComponents.WaterHeater)
                            }
                        };// CreateProtect(waterprotect);

                        PS1sensorType = PS1._SensorType;
						PS2sensorType = PS2._SensorType;
						break;
					case WaterHeaterProtect.No:
						PS1 = null;
						PS2 = null;
						PS1sensorType = Sensor.SensorType.No;
						PS2sensorType = Sensor.SensorType.No;


						break;
				}

			}
		}
		internal Sensor.SensorType _PS1SensType { get => PS1sensorType;
            set => SetSensorType(ref PS1, value);
        }
		internal Sensor.SensorType _PS2SensType { get => PS2sensorType;
            set => SetSensorType(ref PS2, value);
        }
		[DisplayName("Поз.обозн. датчика Т в канале")]
		public string PS1PosName => MakePSPosName(PS1, PS1sensorType);

        [DisplayName("Поз.обозн. датчика Т обр.воды")]
		public string PS2PosName => MakePSPosName(PS2, PS2sensorType);

        [DisplayName("Поз.обозн. насоса")]
		public string PumpPosName => MakePumpPosName(_Pump);

        [DisplayName("Поз.обозн. клапана")]
		public string ValvePosName => _Valve.Posname;

        [DisplayName("Имя блока датч. Т в кнале")]
		public string PS1Blockname => MakePSBockName(PS1, PS1sensorType);

        [DisplayName("Имя блока датч. Т на обр.воде")]
		public string PS2Blockname => MakePSBockName(PS2, PS2sensorType);

        [DisplayName("Имя блока насоса")]
		public string PumpBlockName => MakePumpBlockName(_Pump, HasTK);

        [DisplayName("Имя блока клапана")]
		public string ValveBlockName => _Valve.BlockName;

        public string GUID { get; set; }
		internal string PS1GUID { get => _getPS1GUID();
            set => PS1.GUID = value;
        }
		internal string PS2GUID { get => _getPS2GUID();
            set => PS2.GUID = value;
        }
		internal string ValveGUID { get => _getValeveGUID();
            set => _Valve.GUID = value;
        }
		internal string PumpGUID { get => _getPumpGUID();
            set => _Pump.GUID = value;
        }
		private static string MakePSBockName(Sensor protectSensor, Sensor.SensorType sensorType)
		{
			string blockname = string.Empty;

			if (protectSensor != null)
			{

				switch (sensorType)
				{
					case (Sensor.SensorType.Discrete):
						blockname = "SENS-TS-2WIRE";
						protectSensor.Blockname = blockname;
						break;
					case (Sensor.SensorType.Analogue):
						blockname = "SENS-TE-2WIRE";
						protectSensor.Blockname = blockname;
						break;

				}

			}
			return blockname;
		}
		private string MakePSPosName(SensorT protectSensor, Sensor.SensorType sensorType)
		{
			string PosName = "null";

			if (protectSensor != null)
			{
				switch (sensorType)
				{
					case (Sensor.SensorType.Analogue):
						PosName = "TE";
						protectSensor.PosName = PosName;



						break;
					case (Sensor.SensorType.Discrete):
						PosName = "TS";
						protectSensor.PosName = PosName;
						break;

				}

			}
			return PosName;

		}
		private static string MakePumpBlockName(Pump pump, bool val)
		{
			string PumpBlockName = string.Empty;
            if (pump == null) return PumpBlockName;
            pump.HasTK = val;
            PumpBlockName = pump.BlockName;
            return PumpBlockName;
		}
		private static string MakePumpPosName(Pump pump)
		{
			string PumpPosName = string.Empty;
			if (pump != null)
			{
				PumpPosName = pump.PosName;
			}
			return PumpPosName;
		}
		private string _getPS1GUID()
		{
			string ps1g = string.Empty;
			if (PS1 != null) ps1g = this.PS1.GUID;
			return ps1g;
		}
		private string _getPS2GUID()
		{
			string ps2g = string.Empty;
			if (PS2 != null) ps2g = this.PS2.GUID;
			return ps2g;
		}
		private string _getPumpGUID()
		{
			string pumpg = string.Empty;
			if (_Pump != null) pumpg = _Pump.GUID;
			return pumpg;
		}
		private string _getValeveGUID()
		{
			string valvg = string.Empty;
			if (_Valve != null) valvg = _Valve.GUID;
			return valvg;
		}
		private void PumpSetTK(object val)
		{
			pumpTK = Convert.ToBoolean(val);
			if (_Pump != null)
			{
				_Pump.HasTK = pumpTK;

			};
		}
		private void CheckValveType(object val)
		{
			valveType = (Valve.ValveType)val;
			switch (valveType)
			{
				case Valve.ValveType.Analogue_0_10:
					_Valve.Cable2.Attrubute = Cable.CableAttribute.A;
					_Valve.Cable3 = null;
					_Valve.Cable4 = null;
					_Valve.Cable5 = null;

					_Valve.BlockName = "ED-24-010V";
					break;
				case Valve.ValveType.Analogue_4_20:
					_Valve.Cable2.Attrubute = Cable.CableAttribute.A;
					_Valve.BlockName = "ED-220-420mA";
					_Valve.Cable3 = null;
					_Valve.Cable4 = null;
					_Valve.Cable5 = null;
					break;
				case Valve.ValveType.ThreePos:
					//_Valve.Cable2.Attrubute = Cable.CableAttribute.P;
					_Valve.Cable2 = null;
					_Valve.BlockName = "ED-220-3POS";
					_Valve.Cable3 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _Valve.Posname,
						ToBlockName = _Valve.BlockName,
						Description = "Индикация конечных положений клапана",
						WireNumbers = 2
					};

					_Valve.Cable4 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _Valve.Posname,
						ToBlockName = _Valve.BlockName,
						Description = "Концевой выключатель 1",
						WireNumbers = 3
					};
					_Valve.Cable5 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _Valve.Posname,
						ToBlockName = _Valve.BlockName,
						Description = "Концевой выключатель 2",
						WireNumbers = 3
					};
					_Valve.Cable3.MakeControlSortpriority(_Valve.SortPriority + "3");
					_Valve.Cable4.MakeControlSortpriority(_Valve.SortPriority + "4");
					_Valve.Cable5.MakeControlSortpriority(_Valve.SortPriority + "5");

					break;
			}
        }
		//private void SetValveControlType(ref Valve valve, object val)
		//{
		//	valveType = (Valve.ValveType)val;
		//	valve._ValveType = valveType;


		//}
		private void SetSensorType(ref SensorT sensor, object val)
		{
			if (sensor != null)
			{
				Sensor.SensorType sensorType = (Sensor.SensorType)val;
				switch (sensorType)
				{
					case Sensor.SensorType.Analogue:
						if (sensor == PS1)
						{
							PS1._SensorType = sensorType;

						}
						if (sensor == PS2)
						{
							PS2._SensorType = sensorType;
						}


						break;
					case Sensor.SensorType.Discrete:
						if (sensor == PS1)
						{
							PS1._SensorType = sensorType;

						}
						if (sensor == PS2)
						{
							PS2._SensorType = sensorType;

						}
						break;
					case Sensor.SensorType.No:
						sensor = null;
						break;

				}
			}


		}

		public WaterHeater()
		{

			comp = VentComponents.WaterHeater;
			ShemaASU = ShemaASU.CreateBaseShema(comp);
			ShemaASU.ShemaUp = "Supply_WaterHeater_Pump_3WayValve";
			ShemaASU.componentPlace = ShemaASU.ComponentPlace.Supply;
			Pump pump = new WaterHeater.Pump();
			this._Pump = pump;
			this.HasTK = pump.HasTK;
			pump.Voltage = ElectroDevice._Voltage.AC220;
			Valve valve = new WaterHeater.Valve();
			//valve.SortPriority = "17";
			//valve.Description = "Э/д клапана водяного нагревателя";
			this._Valve = valve;
			
			valveType = valve._ValveType;
			this.Waterprotect = WaterHeaterProtect.No;
			
			ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
			ShemaASU.SetIO(ShemaASU.IOType.AO, 1);

		}

		internal class Pump : Damper, IPower
		{

			private string blockname;
			internal Pump()
			{
				PosName = "M";
				Description = "Э/д насоса водяного нагревателя";
				Cable1 = new Cable
				{
					Attrubute = Cable.CableAttribute.P,
					WriteBlock = true,
					ToPosName = PosName,
					Description = Description,
					ToBlockName = BlockName,
					WireNumbers = 3
				};
				Cable1.SortPriority = this.SortPriority;
				this.ShemaASU = ShemaASU.CreateBaseShema(VentComponents.WaterHeater);
				this.ShemaASU.ShemaUp = "Supply_Pump";
				this.ShemaASU.componentPlace = ShemaASU.ComponentPlace.Supply;
				ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
				Power = "300";

			}
			internal bool HasTK { get { return hascontrol; } set { PumpSetTK(value, this); } }
			
			internal override string MakeDamperBlockName()
			{
				if (this.HasTK)

				{
					blockname = "ED_Pump_220_TK";
				}
				else
				{
					blockname = "ED_Pump_220";
				}

				if (Cable1 != null)
				{
					Cable1.ToBlockName = blockname;
				}
				if (Cable2 != null)
				{
					Cable2.ToPosName = PosName;
					Cable2.ToBlockName = blockname;
				}
				return blockname;
			}
			internal string SortPriority { get { return MakeSortPriority(); } }
			private string MakeSortPriority()
			{
				string sortpriority = "11";
				Cable1.SortPriority = sortpriority;
                Cable2?.MakeControlSortpriority(sortpriority);
                return sortpriority;

			}
			internal void PumpSetTK(object val, Pump pump) //вот здесь ТК насоса
			{
				hascontrol = Convert.ToBoolean(val);
				if (hascontrol)
				{
					Cable2 = new Cable
					{
						Attrubute = Cable.CableAttribute.D,
						WriteBlock = false,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = "Термоконтакты насоса нагревателя",
						WireNumbers = 2
					};
					this.ShemaASU.SetIO(ShemaASU.IOType.DI, 1);



					Cable2.SortPriority = pump.SortPriority + "1";


				}
				else
				{
					Cable2 = null;
					this.ShemaASU.ReSetIO(ShemaASU.IOType.DI);
				}
			}
		}
		internal class Valve : ElectroDevice, IPower, ICompGUID
		{
			[System.ComponentModel.TypeConverter(typeof(EnumDescConverter))]
			internal enum ValveType
			{
				[Description("0-10V")]
				Analogue_0_10,
				[Description("4-20mA")]
				Analogue_4_20,
				[Description("3-х позиционный")]
				ThreePos
			}
			private ValveType valveType;
			internal ValveType _ValveType { get { return valveType; } set { valveType = value; } }

			public string Posname = "FV";
			public string BlockName = "ED-24-010V";
			public string GUID { get; set; }
			internal Cable Cable2;
			internal Cable Cable3;
			internal Cable Cable4;
			internal Cable Cable5;
			internal ShemaASU ShemaASU;

			internal string SortPriority { get { return sortpriority; } set { MakeSortPriority(value); } }
			private string MakeSortPriority(object val)
			{
				sortpriority = string.Empty;
				switch (this.Voltage)
				{
					case _Voltage.AC220:
						sortpriority = "14";
						break;
					default:
						sortpriority = val.ToString();
						break;

				}
				return sortpriority;

			}
			internal Valve()
			{

				this.ShemaASU = ShemaASU.CreateBaseShema(VentComponents.WaterHeater) ;
				this.ShemaASU.SetIO(ShemaASU.IOType.AO, 1);
				this.ShemaASU.ShemaUp = "WaterHeater_Valve";
				this.ShemaASU.componentPlace = ShemaASU.ComponentPlace.Supply;
				this.ShemaASU.ShemaPos = ShemaASUbase.DummyPos(VentComponents.WaterHeater);

				valveType = ValveType.Analogue_0_10;
				Voltage = _Voltage.AC24;
				Description = "Э/д клапана водяного нагревателя";
				SortPriority = "17";
				Cable1 = new Cable
				{
					Attrubute = Cable.CableAttribute.PL,
					WriteBlock = true,
					ToPosName = Posname,
					ToBlockName = BlockName,
					SortPriority = SortPriority,
					Description = Description,
					WireNumbers = 3
				};
				Cable2 = new Cable
				{
					Attrubute = Cable.CableAttribute.A,
					WriteBlock = false,
					ToPosName = Posname,
					ToBlockName = BlockName,
					Description = "Управление клапаном нагревателя",
					WireNumbers = 2
				};
				Cable2.MakeControlSortpriority(SortPriority + "2");
				Power = "10";

			}

		}
		public IEnumerator<object> GetEnumerator()
		{
			yield return PS1;
			yield return PS2;
			yield return _Pump;
			yield return _Valve;

		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		public List<dynamic> GetSensors()
		{
			return new List<dynamic>
			{
				this.PS1,
				this.PS2
			};

		}
	}
	class ElectroHeater : ElectroDevice, IPower ,ICompGUID
	{
		#region Enums


		[System.ComponentModel.TypeConverter(typeof(EnumDescConverter))]
		public enum _Stairs
		{
			[Description("1")]
			S1,
			[Description("2")]
			S2,
			[Description("3")]
			S3,
			[Description("4")]
			S4


		}
		#endregion
		#region Private Variables
		private VentComponents ComponentVariable;
		private _Stairs stairs;

		#endregion
		#region Path To Images
		public static string Imagepath = VentSystem.AppFolder + "Images\\8_1.png"; //это пути для картинок
		public static string ImageNullPath = VentSystem.AppFolder + "Images\\8_0.png";
		internal ShemaASU ShemaASU;
		#endregion
		#region Properties
		[DisplayName("Тип элемента")]
		public VentComponents comp { get { return ComponentVariable; } private set { ComponentVariable = value; } }
		[DisplayName("Напряжение")]
		public new _Voltage Voltage { get { return VoltageVariable; } set { CheckVoltage(value); } }
		[DisplayName("Мощность")]
		public new string Power { get; set; }
		[DisplayName("Количество ступеней")]
		public _Stairs Stairs { get { return stairs; } set { CheckStairs(value); } }
		//[DisplayName("Дистанционный пуск")]
		//public bool RemoteStart { get { return remotestart; } set { MakeRemoteStart(value); } }
		[DisplayName("Поз.обозн.")]
		public string PosName { get; internal set; }
		[DisplayName("Имя блока")]
		public string BlockName { get { return MakeBlockName(); } }
		public string GUID { get; set; }
		internal string Description1 { get; set; }
		internal string Description2 { get; set; }
		internal double Displacement { get; set; }
		internal string StairString { get; set; }
		internal Cable Cable2;
		internal Cable Cable3;
		internal Cable Cable4;
		internal Cable Cable5;
		internal Cable Cable6;
		#endregion
		#region Methods
		private string MakeBlockName()
		{
			string blockname = string.Empty;
			switch (Voltage)
			{
				case _Voltage.AC220:
					blockname = "EH-220-TK";
					break;
				case _Voltage.AC380:
					blockname = "ED-380-FC";
					break;
			}
			return blockname;

		}
		private _Stairs CheckStairs(object val)
		{
			stairs = (_Stairs)val;
			switch (stairs)
			{
				case _Stairs.S1:
					Displacement = -28.75646364;
					Cable2 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = "Управление нагревателем 1 ступень",
						WireNumbers = 2
					};
					Cable2.SortPriority = Cable2.MakeControlSortpriority(SortPriority + "2");
					StairString = "1";
					Cable3 = null;
					Cable4 = null;
					Cable5 = null;
					ShemaASU.SetIO(ShemaASU.IOType.DO, 1);

					break;
				case _Stairs.S2:
					Displacement = -18.85578925;
					Cable2 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = "Управление нагревателем 1 ступень",
						WireNumbers = 2
					};
					Cable3 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = "Управление нагревателем 2 ступень",
						WireNumbers = 2
					};
					Cable2.SortPriority = Cable2.MakeControlSortpriority(SortPriority + "2");
					Cable3.SortPriority = Cable3.MakeControlSortpriority(SortPriority + "3");
					StairString = "2";
					Cable4 = null;
					Cable5 = null;
					ShemaASU.SetIO(ShemaASU.IOType.DO, 2);
					break;
				case _Stairs.S3:
					Displacement = -9.90067453;
					Cable2 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = "Управление нагревателем 1 ступень",
						WireNumbers = 2
					};
					Cable3 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = "Управление нагревателем 2 ступень",
						WireNumbers = 2
					};
					Cable4 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = "Управление нагревателем 3 ступень",
						WireNumbers = 2
					};
					Cable2.SortPriority = Cable2.MakeControlSortpriority(SortPriority + "2");
					Cable3.SortPriority = Cable3.MakeControlSortpriority(SortPriority + "3");
					Cable4.SortPriority = Cable4.MakeControlSortpriority(SortPriority + "4");
					StairString = "3";
					Cable5 = null;
					ShemaASU.SetIO(ShemaASU.IOType.DO, 3);
					break;
				case _Stairs.S4:
					Displacement = 0;
					Cable2 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = "Управление нагревателем 1 ступень",
						WireNumbers = 2
					};
					Cable3 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = "Управление нагревателем 2 ступень",
						WireNumbers = 2
					};
					Cable4 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = "Управление нагревателем 3 ступень",
						WireNumbers = 2
					};
					Cable5 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = "Управление нагревателем 4 ступень",
						WireNumbers = 2
					};
					Cable2.SortPriority = Cable2.MakeControlSortpriority(SortPriority + "2");
					Cable3.SortPriority = Cable3.MakeControlSortpriority(SortPriority + "3");
					Cable4.SortPriority = Cable4.MakeControlSortpriority(SortPriority + "4");
					Cable4.SortPriority = Cable5.MakeControlSortpriority(SortPriority + "5");
					StairString = "4";
					ShemaASU.SetIO(ShemaASU.IOType.DO, 4);
					break;





			}
			return stairs;

		}
		private _Voltage CheckVoltage(object val)
		{
			VoltageVariable = (_Voltage)val;
			switch (VoltageVariable)
			{
				case _Voltage.AC24:
				case _Voltage.DC24:
					VoltageVariable = _Voltage.AC220;
					if (Cable1 != null) Cable1.WireNumbers = 3;
					break;
				case _Voltage.AC380:
					VoltageVariable = _Voltage.AC380;
					if (Cable1 != null) Cable1.WireNumbers = 5;

					break;
				case _Voltage.AC220:
					VoltageVariable = _Voltage.AC220;
					if (Cable1 != null) Cable1.WireNumbers = 3;
					break;
			}
			return VoltageVariable;
		}
		#endregion
		#region Constructors
		public ElectroHeater()
		{
			ComponentVariable = VentComponents.ElectroHeater;
			ShemaASU = ShemaASU.CreateBaseShema(ComponentVariable);
			ShemaASU.ShemaUp = "Supply_ElectroHeater";
			ShemaASU.componentPlace = ShemaASU.ComponentPlace.Supply;

			Stairs = _Stairs.S1;
			ShemaASU.SetIO(ShemaASU.IOType.DI, 1);

			PosName = "H";
			Voltage = _Voltage.AC220;

			Power = "0";
			Description = "Электрический нагреватель";
			Description1 = Description;
			Cable1 = new Cable
			{
				SortPriority = SortPriority,
				WriteBlock = true,
				Attrubute = Cable.CableAttribute.P,
				ToPosName = PosName,
				ToBlockName = BlockName,
				Description = Description,
				WireNumbers = 3
			};
			Cable2 = new Cable
			{
				WriteBlock = false,
				Attrubute = Cable.CableAttribute.D,
				ToPosName = this.PosName,
				ToBlockName = this.BlockName,
				Description = "Управление нагревателем 1 ступень",
				WireNumbers = 2

			};
			Cable6 = new Cable
			{
				WriteBlock = false,
				Attrubute = Cable.CableAttribute.D,
				ToPosName = this.PosName,
				ToBlockName = this.BlockName,
				Description = "Авария нагревателя",
				WireNumbers = 2

			};
			Cable2.MakeControlSortpriority(this.SortPriority + "2");
			Cable6.MakeControlSortpriority(this.SortPriority + "6");

		}
		#endregion
		internal string SortPriority { get { return MakeSortPriority(); } }
		private string MakeSortPriority()
		{
            string priority;
            switch (this.Voltage)
			{
				case _Voltage.AC380:
					priority = "03";
					break;
				default:
					priority = "08";
					break;

			}
			if (this.Cable1 != null) Cable1.SortPriority = priority;
			if (this.Cable2 != null) Cable2.MakeControlSortpriority(priority + "2");
			if (this.Cable3 != null) Cable3.MakeControlSortpriority(priority + "3");
			if (this.Cable4 != null) Cable4.MakeControlSortpriority(priority + "4");
			if (this.Cable5 != null) Cable5.MakeControlSortpriority(priority + "5");
			if (this.Cable6 != null) Cable6.MakeControlSortpriority(priority + "6");

			return priority;

		}
		//private bool MakeRemoteStart(object val)
		//      {
		//	remotestart = (bool)val;
		//	Description2 = "Дистанционный пуск нагревателя";
		//	if (remotestart)
		//	{
		//		Cable2 = new Cable
		//		{
		//			WriteBlock = false,
		//			Attrubute = Cable.CableAttribute.D,
		//			ToPosName = PosName,
		//			ToBlockName = BlockName,
		//			Description = Description2

		//		};
		//		Cable2.MakeControlSortpriority(SortPriority);

		//	}
		//	else { Cable2 = null; }
		//	return remotestart;

		//}

	}
	#endregion
	#region Froster
	class Froster : ElectroDevice, IEnumerable, IPower, ICompGUID, IGetSensors
	{
		#region Enums
		[System.ComponentModel.TypeConverter(typeof(EnumDescConverter))]
		public enum FrosterType 
		{
			[Description("Фреоновый")]
			Freon,
			[Description("Водяной")]
			Water
		}
		[System.ComponentModel.TypeConverter(typeof(EnumDescConverter))]
		public enum FrosterSensor
		{
			[Description("Нет")]
			No,
			[Description("На притоке")]
			SupplySens,
			[Description("На вытяжке")]
			ExhaustSens,
			[Description("На притоке и вытяжке")]
			Supp_ExhSens
		}
		

		//[System.ComponentModel.TypeConverter(typeof(EnumDescConverter))]
		
		#endregion
		#region Pictures Path
		public static string Imagepath;
		public static string ImageNullPath = VentSystem.AppFolder + "Images\\10_0.png";
		#endregion
		#region Variabes
		private VentComponents ComponentVariable;
		FrosterType frostertype;
		FrosterSensor frostersensor;
		KKB.FrosterStairs frosterStairs;
		Froster.ProtectSensor.SensorType sensor1Type;
		Froster.ProtectSensor.SensorType sensor2Type;
		internal ProtectSensor Sens1;
		internal ProtectSensor Sens2;
		string sens1PosName;
		string sens2PosName;
		string sens1BlockName;
		string sens2BlockName;
		public Valve _Valve;
		internal KKB _KKB;
		private KKB.KKBControlType kKBControlType;
		private Valve.ValveType _valveType;
		//internal _Voltage voltage;
		internal ShemaASU ShemaASU;
		#endregion
		#region Properties
		[DisplayName("Тип элемента")]
		public VentComponents comp { get { return ComponentVariable; } private set { ComponentVariable = value; } }
		[DisplayName("Тип охладителя")]
		public FrosterType _FrosterType { get { return frostertype; } set { CheckFrosterType(value); } }

		[DisplayName("Количество ступеней")]
		public KKB.FrosterStairs Stairs { get { return frosterStairs; } set { CheckfrosterStairs(value); } }
		
		[DisplayName("Тип управления ККБ")]
		public KKB.KKBControlType KKBControlType { get { return kKBControlType; } set { kKBControlType = MakeKKBControlType(value, frostertype); } }
		[DisplayName("Тип управления клапаном")]
		public Valve.ValveType valveType { get { return _valveType; } set { _valveType = CheckValveType(value, ref _Valve); } }
		[DisplayName("Напряжение")]
		public new _Voltage Voltage { get { return VoltageVariable; } set { CheckVoltage(ref _KKB, ref _Valve, value); } }
		[DisplayName("Мощность")]
		public new string Power { get; set; }
		[DisplayName("Контроль тмпературы")]
		internal FrosterSensor _FrosterSensor
		{
			get
			{
				return frostersensor;
			}
			set
			{
				frostersensor = value;
				sensor1Type = Sensor.SensorType.No;
				sensor2Type = Sensor.SensorType.No;
				Sens1 = null;
				Sens2 = null;
				switch (frostersensor)
				{
					case FrosterSensor.SupplySens:
						Sens1 = new Froster.ProtectSensor
						{
							frostersensor = FrosterSensor.SupplySens,
							Description = "Контроль Т охладителя в приточном канале"
						};
						Sens1.ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
						Sens1.ShemaASU.ShemaPos = ShemaASUbase.DummyPos(comp);
						Sens1.ShemaASU.ShemaUp = "Froster_Water_SupplyTemp";
						Sens1._SensorType = MakeSensorType(Sens1);
						MakeSensPosName(ref Sens1);
						MakeSensBlockName(ref Sens1);

						break;
					case FrosterSensor.ExhaustSens: //вот это надо проверить и исключить?
						Sens2 = new Froster.ProtectSensor
						{
							frostersensor = FrosterSensor.ExhaustSens,
							Description = "Контроль Т охладителя в вытяжном канале"
						};
						Sens2._SensorType = MakeSensorType(Sens2);
						MakeSensPosName(ref Sens2);
						MakeSensBlockName(ref Sens2);
						break;
					case FrosterSensor.Supp_ExhSens: // вот это надо проверить и исключить?
						Sens1 = new Froster.ProtectSensor
						{
							frostersensor = FrosterSensor.SupplySens,
							Description = "Контроль Т охладителя в приточном канале"
						};
						Sens2 = new Froster.ProtectSensor
						{
							frostersensor = FrosterSensor.ExhaustSens,
							Description = "Контроль Т охладителя в вытяжном канале"
						};
						Sens1._SensorType = MakeSensorType(Sens1);
						Sens2._SensorType = MakeSensorType(Sens2);
						sens1PosName = MakeSensPosName(ref Sens1);
						sens2PosName = MakeSensPosName(ref Sens2);
						sens1BlockName = MakeSensBlockName(ref Sens1);
						sens2BlockName = MakeSensBlockName(ref Sens2);
						break;

					case FrosterSensor.No:
						sens1PosName = MakeSensPosName(ref Sens1);
						sens2PosName = MakeSensPosName(ref Sens2);

						break;
				}



			}
		}
		[DisplayName("Тип датчика 1")]
		internal Froster.ProtectSensor.SensorType Sensor1Type { get { return sensor1Type; } set { sensor1Type = CheckSensType(value, ref Sens1); } }
		[DisplayName("Тип датчика 2")]
		internal Froster.ProtectSensor.SensorType Sensor2Type { get { return sensor2Type; } set { sensor2Type = CheckSensType(value, ref Sens2); } }
		[DisplayName("Поз.обозначение датчика 1")]
		internal string Sens1PosName { get { return sens1PosName; } private set { sens1PosName = value; } }
		[DisplayName("Поз.обозначение датчика 2")]
		internal string Sens2PosName { get { return sens2PosName; } private set { sens2PosName = value; } }
		[DisplayName("Имя блока датчика 1")]
		internal string Sens1BlockName { get { return sens1BlockName; } private set { sens1BlockName = value; } }
		[DisplayName("Имя блока датчика 2")]
		public string Sens2BlockName { get { return sens2BlockName; } private set { sens2BlockName = value; } }
		[DisplayName("Поз.обозначение клапана")]
		public string ValvePosName { get { return GetValveInfo()[0]; } }
		[DisplayName("Имя блока клапана")]
		public string ValveBlockName { get { return GetValveInfo()[1]; } }
		[DisplayName("Поз.обозначение ККБ")]
		public string KKBPosName { get { return GetKKBInfo()[0]; } }
		[DisplayName("Имя блока ККБ")]
		public string KKBBlockName { get { return GetKKBInfo()[1]; } }
		public string GUID { get; set; }
		internal string Sens1GUID { get { return _getPS1GUID(); } set { Sens1.GUID = value; } }
		internal string Sens2GUID { get { return _getPS2GUID(); } set { Sens2.GUID = value; } }
		internal string ValveGUID { get { return _getGUID(_Valve); } set { _Valve.GUID = value; } }
		internal string KKBGUID { get { return _getGUID(_KKB); } set { _KKB.GUID = value; } }

		#endregion
		#region Methods
		public static string makeimagepath(FrosterType frostertype)
		{
			Imagepath = string.Empty;
			if (frostertype == FrosterType.Freon)
			{
				Imagepath = VentSystem.AppFolder + "Images\\10_2.png";
			}
			if (frostertype == FrosterType.Water)
			{
				Imagepath = VentSystem.AppFolder + "Images\\10_1.png";
			}
			return Imagepath;
		}
		protected Froster.ProtectSensor.SensorType CheckSensType(object val, ref Froster.ProtectSensor protectSensor)
		{
			Froster.ProtectSensor.SensorType sensorType = Sensor.SensorType.No;
			if (frostersensor != FrosterSensor.No && protectSensor != null)
			{
				sensorType = (Sensor.SensorType)val;
				protectSensor._SensorType = sensorType;
				switch (sensorType)
                {
					case Sensor.SensorType.Analogue:
						protectSensor.ShemaASU.ReSetAllIO();
						protectSensor.ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
						break;
					case Sensor.SensorType.Discrete:
						protectSensor.ShemaASU.ReSetAllIO();
						protectSensor.ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
						break;

				}
				//protectSensor._FrosterSensorType = (ProtectSensor.FrosterSensorType)sensorType; //а нужно ли это???




			}

			if (protectSensor == Sens1)
			{
				sens1PosName = MakeSensPosName(ref Sens1);
				sens1BlockName = MakeSensBlockName(ref Sens1);

			}
			if (protectSensor == Sens2)
			{
				sens2PosName = MakeSensPosName(ref Sens2);
				sens2BlockName = MakeSensBlockName(ref Sens2);
			}

			return sensorType;

		}
		protected string MakeSensPosName(ref Froster.ProtectSensor protectSensor)
		{

			string posname = string.Empty;

			if (protectSensor != null)
			{
				switch (protectSensor._SensorType)
				{
					case Sensor.SensorType.Analogue:
						posname = "TE";
						break;
					case Sensor.SensorType.Discrete:
						posname = "TS";
						break;
				}
				switch (this.frostersensor)
				{
					case FrosterSensor.SupplySens:
						sens1PosName = posname;
						sens2PosName = string.Empty;
						break;
					case FrosterSensor.ExhaustSens:
						sens1PosName = string.Empty;
						sens2PosName = posname;
						break;

					case FrosterSensor.No:
						sens1PosName = string.Empty;
						sens2PosName = string.Empty;
						break;
				}


				//if (protectSensor == Sens1)
				//{
				//	sens1PosName = posname;
				//}
				//if (protectSensor == Sens2)
				//{
				//	sens2PosName = posname;
				//}
			}


			return posname;
		}
		protected string MakeSensBlockName(ref Froster.ProtectSensor protectSensor)
		{

			string blockname = string.Empty;

			if (protectSensor != null)
			{
				switch (protectSensor._SensorType)
				{
					case Sensor.SensorType.Analogue:
						blockname = "SENS-TE-2WIRE";
						break;
					case Sensor.SensorType.Discrete:
						blockname = "SENS-TS-2WIRE";
						break;
				}
				switch (this.frostersensor)
				{
					case FrosterSensor.SupplySens:
						sens1BlockName = blockname;
						sens2BlockName = string.Empty;
						break;
					case FrosterSensor.ExhaustSens:
						sens1BlockName = string.Empty;
						sens2BlockName = blockname;
						break;

					case FrosterSensor.No:
						sens1BlockName = string.Empty;
						sens2BlockName = string.Empty;
						break;
				}

			}


			return blockname;
		}
		protected string[] GetValveInfo()
		{
			string[] array = new string[] { string.Empty, string.Empty };
			if (_Valve != null)
			{
				array[0] = _Valve.Posname;
				array[1] = _Valve.BlockName;
			}
			return array;
		}
		protected string[] GetKKBInfo()
		{
			string[] array = new string[] { string.Empty, string.Empty };
			if (_KKB != null)
			{
				array[0] = _KKB.PosName;
				array[1] = _KKB.BlockName;
				if (_KKB.Cable1 != null) _KKB.Cable1.ToBlockName = _KKB.BlockName;


			}

			return array;
		}
		protected Valve.ValveType CheckValveType(object val, ref Froster.Valve valve)
		{

			_valveType = (Valve.ValveType)val;
			try
			{
				if (valve != null)
				{
					valve._ValveType = valveType;
					switch (_valveType)
					{
						case Valve.ValveType.Analogue_0_10:
						case Valve.ValveType.Analogue_4_20:
							valve.Cable2 = new Cable
							{
								WriteBlock = false,
								Description = "Управление клапаном охладителя",
								Attrubute = Cable.CableAttribute.A,
								ToBlockName = _Valve.BlockName,
								ToPosName = _Valve.Posname,
								WireNumbers = 2
							};
							int sp = int.Parse(_Valve.Cable1.SortPriority);
							valve.Cable2.SortPriority = (sp + 1).ToString();


							break;
						case Valve.ValveType.ThreePos:
							valve.Cable2 = null;
							break;
						case Valve.ValveType.No:
							_Valve.Cable1 = null;
							_Valve.Cable2 = null;
							_Valve = null;
							break;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace);
			}

			return valveType;
		}
		protected void CheckVoltage(ref KKB kKB, ref Valve valve, object val)
		{
			VoltageVariable = (_Voltage)val;
			if (kKB != null)
			{
				switch (VoltageVariable)
				{
					case _Voltage.AC380:

						kKB.Voltage = VoltageVariable;
						if (Cable1 != null) Cable1.WireNumbers = 5;
						break;
					case _Voltage.AC220:
						kKB.Voltage = VoltageVariable;
						if (Cable1 != null) Cable1.WireNumbers = 3;
						break;
					case _Voltage.AC24:
					case _Voltage.DC24:
						VoltageVariable = _Voltage.AC380;
						break;

				}
				kKB.MakeSortPriority(VoltageVariable);


			}
			if (valve != null)
			{
				VoltageVariable = _Voltage.AC24;
				_Valve.Voltage = VoltageVariable;
			}
			//return voltage;
		}
		protected Sensor.SensorType MakeSensorType(Froster.ProtectSensor protectSensor)
		{

            Sensor.SensorType sensorType = Sensor.SensorType.No;
			switch (this._FrosterSensor)
			{
				case FrosterSensor.No:
					sensor1Type = Sensor.SensorType.No;

					break;
				case FrosterSensor.SupplySens:
					sensor1Type = Sensor.SensorType.Analogue;
					sensorType = sensor1Type;


					break;
				case FrosterSensor.ExhaustSens:
					sensor2Type = Sensor.SensorType.Analogue;
					sensorType = sensor2Type;

					break;
				case FrosterSensor.Supp_ExhSens:
					sensor1Type = Sensor.SensorType.Analogue;
					sensor2Type = Sensor.SensorType.Analogue;
					sensorType = Sensor.SensorType.Analogue;
					break;

			}
			return sensorType;
		}
		protected KKB.KKBControlType MakeKKBControlType(object val, FrosterType frosterType)
		{
			KKB.KKBControlType kKBControlType1 = KKB.KKBControlType.No;
			if (frosterType == FrosterType.Freon)
			{
				kKBControlType1 = (KKB.KKBControlType)val;
				if (_KKB != null)
				{
					_KKB._KKBControlType = kKBControlType1;
					switch (kKBControlType1)
					{
						case KKB.KKBControlType.Analogue:
							if (_KKB.Cable2 != null)
							{
								_KKB.Cable2.Attrubute = Cable.CableAttribute.A;

							}
							if (_KKB.Cable3 != null)
							{
								_KKB.Cable3.Attrubute = Cable.CableAttribute.A;

							}
							if (_KKB.Cable3 != null)
							{
								_KKB.Cable3.Attrubute = Cable.CableAttribute.A;

							}
							if (_KKB.Cable4 != null)
							{
								_KKB.Cable4.Attrubute = Cable.CableAttribute.A;

							}
							if (_KKB.Cable5 != null)
							{
								_KKB.Cable5.Attrubute = Cable.CableAttribute.A;

							}
							_KKB.ShemaASU.ReSetAllIO();
							_KKB.ShemaASU.SetIO(ShemaASU.IOType.AO, 1);
							//_KKB.Cable2 = new Cable
							//{
							//	Attrubute = Cable.CableAttribute.A,
							//	WriteBlock = false,
							//	ToPosName = _KKB.PosName,
							//	ToBlockName = _KKB.BlockName,
							//	Description = "Упавление охладителем"

							//};

							break;
						case KKB.KKBControlType.Discrete:
							//_KKB.Cable2 = new Cable
							//{
							//	Attrubute = Cable.CableAttribute.D,
							//	WriteBlock = false,
							//	ToPosName = _KKB.PosName,
							//	ToBlockName = _KKB.BlockName,
							//	Description = "Упавление охладителем"
							//};
							if (_KKB.Cable2 != null)
							{
								_KKB.Cable2.Attrubute = Cable.CableAttribute.D;

							}
							if (_KKB.Cable3 != null)
							{
								_KKB.Cable3.Attrubute = Cable.CableAttribute.D;

							}
							if (_KKB.Cable3 != null)
							{
								_KKB.Cable3.Attrubute = Cable.CableAttribute.D;

							}
							if (_KKB.Cable4 != null)
							{
								_KKB.Cable4.Attrubute = Cable.CableAttribute.D;

							}
							if (_KKB.Cable5 != null)
							{
								_KKB.Cable5.Attrubute = Cable.CableAttribute.D;

							}
							_KKB.ShemaASU.ReSetAllIO();
							_KKB.ShemaASU.SetIO(ShemaASU.IOType.DO, 1);

							break;
						case KKB.KKBControlType.No:
						default:
							_KKB.Cable2 = null;
							_KKB.Cable3 = null;
							_KKB.Cable4 = null;
							_KKB.Cable5 = null;
							break;
					}
				}


			}

			return kKBControlType1;
		}
		private FrosterType CheckFrosterType(object val)
		{
			frostertype = (FrosterType)val;
			Imagepath = makeimagepath(frostertype);
			switch (frostertype)
			{
				case FrosterType.Freon:
					_KKB = new Froster.KKB
					{
						_KKBControlType = KKB.KKBControlType.Analogue,
						Voltage = VoltageVariable,
						Description = "Компрессор охладителя"

					};
					
					_KKB.MakeSortPriority(_KKB.Voltage);
					_Valve = null;
					_valveType = Valve.ValveType.No;
					VoltageVariable = _KKB.Voltage;
					KKBControlType = KKB.KKBControlType.Analogue;
					this.Stairs = KKB.FrosterStairs.One;
					ShemaASU.ShemaUp = "Froster_Frion_Steps";
					break;
				case FrosterType.Water:
					_Valve = new Froster.Valve();
					VoltageVariable = _Voltage.AC24;
					this.sortpriority = _Valve.SortPriority;
					ShemaASU.ShemaUp = "Supply_Froster_Water";
					_KKB = null;
					break;
			}
			return frostertype;

		}

		private string _getPS1GUID()
		{
			string ps1g = string.Empty;
			if (Sens1 != null) ps1g = this.Sens1.GUID;
			return ps1g;
		}
		private string _getPS2GUID()
		{
			string ps2g = string.Empty;
			if (Sens2 != null) ps2g = this.Sens2.GUID;
			return ps2g;
		}
		private string _getGUID(object obj)
		{
			string guid = string.Empty;
            if (obj is Froster.KKB)
            {
                KKB kkb = _KKB;
                guid = kkb.GUID;
            }

            if (obj is Froster.Valve)
			{
                Valve valve = _Valve;
                guid = valve.GUID;
			}
			return guid;

		}
		private Froster.KKB.FrosterStairs CheckfrosterStairs(object val)
		{
			frosterStairs = (KKB.FrosterStairs)val;
			_KKB.Stairs = frosterStairs;
			_KKB.StairsString = GetEnumDescription(Stairs);
			switch (frosterStairs)
			{
				case KKB.FrosterStairs.One:
					_KKB.Displacement = -28.75646364;
					
					_KKB.Cable2 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _KKB.PosName,
						ToBlockName = _KKB.BlockName,
						Description = "Управление ККБ 1 ступень",
						WireNumbers = 2
					};
					_KKB.Cable3 = null;
					_KKB.Cable4 = null;
					_KKB.Cable5 = null;
					_KKB.Cable2.SortPriority = _KKB.Cable2.MakeControlSortpriority(_KKB.SortPriority + "2");
					ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
					break;
				case KKB.FrosterStairs.Two:
					_KKB.Displacement = -19.80135002;
					
					_KKB.Cable2 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _KKB.PosName,
						ToBlockName = _KKB.BlockName,
						Description = "Управление ККБ 1 ступень",
						WireNumbers = 2
					};
					_KKB.Cable3 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _KKB.PosName,
						ToBlockName = _KKB.BlockName,
						Description = "Управление ККБ 2 ступень",
						WireNumbers = 2
					};
					_KKB.Cable4 = null;
					_KKB.Cable5 = null;
					_KKB.Cable2.SortPriority = _KKB.Cable2.MakeControlSortpriority(_KKB.SortPriority + "2");
					_KKB.Cable3.SortPriority = _KKB.Cable3.MakeControlSortpriority(_KKB.SortPriority + "3");
					ShemaASU.SetIO(ShemaASU.IOType.DO, 2);


					break;
				case KKB.FrosterStairs.Three:
					_KKB.Displacement = -9.90067439;
					
					_KKB.Cable2 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _KKB.PosName,
						ToBlockName = _KKB.BlockName,
						Description = "Управление ККБ 1 ступень",
						WireNumbers = 2
					};
					_KKB.Cable3 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _KKB.PosName,
						ToBlockName = _KKB.BlockName,
						Description = "Управление ККБ 2 ступень",
						WireNumbers = 2
					};
					_KKB.Cable4 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _KKB.PosName,
						ToBlockName = _KKB.BlockName,
						Description = "Управление ККБ 3 ступень",
						WireNumbers = 2
					};
					_KKB.Cable5 = null;
					_KKB.Cable2.SortPriority = _KKB.Cable2.MakeControlSortpriority(_KKB.SortPriority + "2");
					_KKB.Cable3.SortPriority = _KKB.Cable3.MakeControlSortpriority(_KKB.SortPriority + "3");
					_KKB.Cable4.SortPriority = _KKB.Cable4.MakeControlSortpriority(_KKB.SortPriority + "4");
					ShemaASU.SetIO(ShemaASU.IOType.DO, 3);
					break;
				case KKB.FrosterStairs.Four:
					_KKB.Displacement = 0;
					_KKB.StairsString = "4";
					_KKB.Cable2 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _KKB.PosName,
						ToBlockName = _KKB.BlockName,
						Description = "Управление ККБ 1 ступень",
						WireNumbers = 2
					};
					_KKB.Cable3 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _KKB.PosName,
						ToBlockName = _KKB.BlockName,
						Description = "Управление ККБ 2 ступень",
						WireNumbers = 2
					};
					_KKB.Cable4 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _KKB.PosName,
						ToBlockName = _KKB.BlockName,
						Description = "Управление ККБ 3 ступень",
						WireNumbers = 2
					};
					_KKB.Cable5 = new Cable
					{
						WriteBlock = false,
						Attrubute = Cable.CableAttribute.D,
						ToPosName = _KKB.PosName,
						ToBlockName = _KKB.BlockName,
						Description = "Управление ККБ 4 ступень",
						WireNumbers = 2
					};
					_KKB.Cable2.SortPriority = _KKB.Cable2.MakeControlSortpriority(_KKB.SortPriority + "2");
					_KKB.Cable3.SortPriority = _KKB.Cable3.MakeControlSortpriority(_KKB.SortPriority + "3");
					_KKB.Cable4.SortPriority = _KKB.Cable4.MakeControlSortpriority(_KKB.SortPriority + "4");
					_KKB.Cable5.SortPriority = _KKB.Cable5.MakeControlSortpriority(_KKB.SortPriority + "5");
					ShemaASU.SetIO(ShemaASU.IOType.DO, 4);
					break;

			}

			return frosterStairs;

		}
		#endregion
		#region Interenal Classes
		internal class Valve : ElectroDevice, IPower, ICompGUID
		{
			[System.ComponentModel.TypeConverter(typeof(EnumDescConverter))]
			internal enum ValveType
			{
				[Description("Нет")]
				No,
				[Description("0-10V")]
				Analogue_0_10,
				[Description("4-20mA")]
				Analogue_4_20,
				[Description("3-х позиционный")]
				ThreePos
			}
			private ValveType valveType;
			internal ShemaASU ShemaASU;
			public ValveType _ValveType { get { return valveType; } set { valveType = value; } }

			public string Posname = "FV";
			public string BlockName = "ED-24";
			public string GUID { get; set; }
			internal Cable Cable2;
			internal Valve()
			{
				valveType = ValveType.Analogue_0_10;
				Description = "Э/д клапана охладителя";
				Cable1 = new Cable
				{
					WriteBlock = true,
					Attrubute = Cable.CableAttribute.P,
					ToPosName = this.Posname,
					ToBlockName = this.BlockName,
					SortPriority = this.SortPriority,
					Description = this.Description


				};
				ShemaASU = ShemaASU.CreateBaseShema(VentComponents.Froster);
				ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
				ShemaASU.SetIO(ShemaASU.IOType.AO, 1);
				Power = "10";
			}
			internal string SortPriority { get { return MakeSortPriority(); } }
			private string MakeSortPriority()
			{
                string priority;
                switch (this.Voltage)
				{
					case _Voltage.AC220: //this value will newer be assigned. Main Class (Froster) always set valve voltage to AC.24
						priority = "14";
						break;
					default:
						priority = "17";
						break;

				}
				if (Cable1 != null) Cable1.MakeControlSortpriority(priority);
				return priority;

			}

		}
		internal class ProtectSensor : SensorT
		{
			public enum FrosterSensorType
			{
				Discrete,
				Analogue
			}
			internal FrosterSensor frostersensor;
			
			//public FrosterSensorType _FrosterSensorType
			//{
			//	get
			//	{
			//		return frostersensortype;
			//	}
			//	set
			//	{
			//		frostersensortype = value;
			//		switch (frostersensortype)
   //                 {
			//			case FrosterSensorType.Analogue:
			//				PosName = "TE";
			//				Blockname = "SENS-TE-2WIRE";
			//				Cable1.Attrubute = Cable.CableAttribute.A;
			//				Cable1.ToPosName = PosName;
			//				Cable1.ToBlockName = Blockname;
			//				Cable1.SortPriority = SortPriority;
			//				this.ShemaASU.ReSetAllIO();
			//				this.ShemaASU.SetIO(ShemaASU.IOType.AI, 1);

			//				break;
			//			case FrosterSensorType.Discrete:
			//				PosName = "TS";
			//				Blockname = "SENS-TS-2WIRE";
			//				Cable1.Attrubute = Cable.CableAttribute.D;
			//				Cable1.ToPosName = PosName;
			//				Cable1.ToBlockName = Blockname;
			//				Cable1.SortPriority = SortPriority;
			//				this.ShemaASU.ReSetAllIO();
			//				this.ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
			//				break;


			//		}
					
					
			//	}
			//}


		}
		internal class KKB : ElectroDevice, IPower, ICompGUID
		{
			[System.ComponentModel.TypeConverter(typeof(EnumDescConverter))]
			public enum FrosterStairs
			{
				[Description("1")]
				One,
				[Description("2")]
				Two,
				[Description("3")]
				Three,
				[Description("4")]
				Four

			}
			[System.ComponentModel.TypeConverter(typeof(EnumDescConverter))]
			public enum KKBControlType
			{
				[Description("Нет")]
				No,
				[Description("Аналоговый")]
				Analogue,
				[Description("Дискретный")]
				Discrete
			}
			//public string Voltage { get; set; }
			public FrosterStairs Stairs { get; set; }
			public KKBControlType _KKBControlType { get; set; }
			internal Cable Cable2;
			internal Cable Cable3;
			internal Cable Cable4;
			internal Cable Cable5;
			internal Cable Cable6;
			internal ShemaASU ShemaASU;
			
			public string GUID { get; set; }
			internal double Displacement { get; set; }
			internal string StairsString { get => GetEnumDescription(Stairs); set { } } 
			public string PosName = "KKB";

            private string MakeBlockName()
			{
                string blockname;
                switch (Voltage)
				{
					case _Voltage.AC380:

						blockname = "ED-380-FC";
						if (Cable1 != null) Cable1.WireNumbers = 5;
						break;
					case _Voltage.AC220:
						blockname = "ED-220-FC";
						if (Cable1 != null) Cable1.WireNumbers = 3;
						break;
					default:
						blockname = string.Empty;
						break;
				}
				return blockname;
			}
			public string BlockName => MakeBlockName();
            internal string SortPriority { get => sortpriority; set => MakeSortPriority(Voltage);
            }
			internal string MakeSortPriority(_Voltage voltage)
			{
                string priority;
                switch (voltage)
				{
					case _Voltage.AC380:
						priority = "05";
						break;
					default:
						priority = "10";
						break;

				}

				if (this.Cable1 != null) Cable1.SortPriority = priority;
				if (this.Cable2 != null) Cable2.MakeControlSortpriority(priority + "2");
				if (this.Cable3 != null) Cable3.MakeControlSortpriority(priority + "3");
				if (this.Cable4 != null) Cable4.MakeControlSortpriority(priority + "4");
				if (this.Cable5 != null) Cable5.MakeControlSortpriority(priority + "5");
				if (this.Cable6 != null) Cable6.MakeControlSortpriority(priority + "6");
				sortpriority = priority;
				return priority;

			}
			internal KKB()
			{
				this.ShemaASU = ShemaASU.CreateBaseShema(VentComponents.Froster);
				this.ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
				_KKBControlType = KKBControlType.Discrete;
				Voltage = _Voltage.AC380;
				Cable1 = new Cable
				{
					WriteBlock = true,
					Attrubute = Cable.CableAttribute.P,
					ToPosName = this.PosName,
					ToBlockName = this.BlockName,
					Description = "ККБ охладителя",
					SortPriority = SortPriority,
					WireNumbers = 5

				};
				Cable2 = new Cable
				{
					WriteBlock = false,
					Attrubute = Cable.CableAttribute.D,
					ToPosName = this.PosName,
					ToBlockName = this.BlockName,
					Description = "Управление ККБ 1 ступень",
					WireNumbers = 2

				};
				Cable6 = new Cable
				{
					WriteBlock = false,
					Attrubute = Cable.CableAttribute.D,
					ToPosName = this.PosName,
					ToBlockName = this.BlockName,
					Description = "Авария ККБ",
					WireNumbers = 2

				};
				Cable2.MakeControlSortpriority(this.SortPriority + "2");
				Cable6.MakeControlSortpriority(this.SortPriority + "6");
				this.ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
				this.ShemaASU.SetIO(ShemaASU.IOType.DI, 1);

			}



		}
		#endregion
		#region Constructors
		public Froster(bool MinRequest)
		{
			if (MinRequest)
            {
				ComponentVariable = VentComponents.Froster;
				ShemaASU = ShemaASU.CreateBaseShema(ComponentVariable);
				ShemaASU.componentPlace = ShemaASU.ComponentPlace.Supply;
				Voltage = _Voltage.AC220;
				Power = "0";
			}
			else
            {
				ComponentVariable = VentComponents.Froster;
				ShemaASU = ShemaASU.CreateBaseShema(ComponentVariable);
				ShemaASU.componentPlace = ShemaASU.ComponentPlace.Supply;
				Voltage = _Voltage.AC220;
				Power = "0";
				_FrosterType = FrosterType.Freon;
				_KKB._KKBControlType = KKB.KKBControlType.Discrete;
				_KKB.MakeSortPriority(Voltage);
			}
			
		}
		public Froster( FrosterType frosterType)
        {
			ComponentVariable = VentComponents.Froster;
			ShemaASU = ShemaASU.CreateBaseShema(ComponentVariable);
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
			Voltage = _Voltage.AC220;
			Power = "0";
			_FrosterType = frostertype;
			if (frostertype == FrosterType.Freon)
            {
				_KKB._KKBControlType = KKB.KKBControlType.Discrete;
				_KKB.MakeSortPriority(Voltage);
			}
			
		}

        

        #endregion
		#region Enumerator
		public IEnumerator<object> GetEnumerator()
		{
			if (_KKB != null)
			{
				yield return _KKB;
			}
			else
			{
				yield return Sens1;
				yield return Sens2;
				yield return _Valve;
			}

		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		public static string GetEnumDescription(Enum enumValue)
		{
			var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

			var descriptionAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

			return descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : enumValue.ToString();
		}
		public List<dynamic> GetSensors()
		{
			return new List<dynamic>
			{
				this.Sens1,
				this.Sens2
			};

		}
		#endregion

	}
	#endregion
	#region Humudifier
	class Humidifier : ElectroDevice, IPower, ICompGUID, IGetSensors
	{
		#region Pictures Path
		public static string ImageNullPath = VentSystem.AppFolder + "Images\\11_0.png";
		public static string Imagepath = VentSystem.AppFolder + "Images\\11_1.png";
		#endregion
		#region Enum
		[System.ComponentModel.TypeConverter(typeof(EnumDescConverter))]
		public enum _HumType
		{
			[Description("Пароувлажнитель")]
			Steam,
			[Description("Сотовый увлажнитель")]
			Honeycomb,
			[Description("Камера орошения")]
			Irrigation_chamber
		}
		[System.ComponentModel.TypeConverter(typeof(EnumDescConverter))]
		public enum HumContolType
		{
			[Description("Аналоговое")]
			Analogue,
			[Description("Дискретное")]
			Discrete
		}
		#endregion
		#region Variabes
		private VentComponents ComponentVariable;
		private _HumType humtype;

		private bool senspresent;
		private string sensblockname;
		private string sensposkname;

		private HumContolType humContol;
		private Sensor.SensorType sensortype;
		private HumiditySens _HumiditySens;
		internal Cable Cable2;
		internal Cable Cable3;
		internal ShemaASU ShemaASU;
		internal HumiditySens HumiditySensor { get { return _HumiditySens; } set { _HumiditySens = value; } }
		#endregion
		#region Properties
		[DisplayName("Тип элемента")]
		public VentComponents comp { get { return ComponentVariable; } private set { ComponentVariable = value; } }
		[DisplayName("Тип увлажнителя")]
		public _HumType HumType { get { return humtype; } set { humtype = value; } }
		[DisplayName("Тип управления")]
		public HumContolType HumControlType { get { return humContol; } set { CheckHumContol(value); } }
		[DisplayName("Напряжение")]
		public new _Voltage Voltage { get { return VoltageVariable; } set { CheckVoltage(value); } }
		[DisplayName("Мощность")]
		public new string Power { get; set; }

		[DisplayName("Гигростат в канале")]
		//public bool HumSensPresent { get { return MakeSens(ref _HumiditySens, ref sensblockname, ref sensposkname); } set { CheckSens(value); } }
		public bool HumSensPresent { get => senspresent;
            //return MakeSens(ref _HumiditySens, ref sensblockname, ref sensposkname); 
            set => CheckSens(value);
        }
		[DisplayName("Тип гигростата")]
		public Sensor.SensorType SensorType { get => sensortype;
            set => SetSensorType(_HumiditySens, value, ref sensposkname);
        }
		[DisplayName("Обозначение датчика")]

		//public string SensPosName { get { return SetPosName(_HumiditySens, sensortype); } internal set { } }
		public string SensPosName { get => GetPosName();
            internal set => SetPosName(value);
        }
		[DisplayName("Имя блока датчика")]
		public string SensBlockName => GetBlockName(_HumiditySens, sensortype);

        [DisplayName("Поз.обозначение")]
		public string PosName => MakePosName();

        [DisplayName("Имя блока увлажнителя")]
		public string BlockName => MakeBockName(VoltageVariable);

        public string GUID { get; set; }
		internal double Displacement { get; set; }
		#endregion
		#region Methods
		private string MakeBockName(_Voltage voltage)
		{
            string blockname;
            switch (voltage)
			{
				case (_Voltage.AC380):
					blockname = "ED-380-HC";
					if (Cable1 != null) Cable1.WireNumbers = 5;
					break;
				default:
					blockname = "ED-220-HC";
					if (Cable1 != null) Cable1.WireNumbers = 3;
					break;

			}
			if (Cable1 != null) Cable1.ToBlockName = blockname;
			if (Cable2 != null) Cable2.ToBlockName = blockname;
			if (Cable3 != null) Cable3.ToBlockName = blockname;
			return blockname;

		}
		private string MakePosName()
		{
			return "HUM";
		}
		private void MakeSens(ref HumiditySens humiditySens, ref string sensname, ref string senposname)
		{
			humiditySens = null;
			sensname = "null";
			senposname = "null";
			if (senspresent)
			{
				humiditySens = new HumiditySens
				{
					Description = "Капилярный гигростат в канале",
					insideHum = true
				};
				sensname = humiditySens.Blockname;
				senposname = humiditySens.PosName;
				senspresent = true;
				humiditySens.ShemaASU = ShemaASU.CreateBaseShema(comp);
				humiditySens.ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
				humiditySens.ShemaASU.ShemaUp = "Humidifier_Hygrostat";
				
			}
			else
			{
				sensortype = Sensor.SensorType.No;
			}
        }
		private void SetSensorType(HumiditySens huniditySens, object val, ref string sensposname)
		{
			sensortype = Sensor.SensorType.No;
			if (huniditySens != null)
			{
				sensortype = (Sensor.SensorType)val;
				huniditySens._SensorType = sensortype;
				sensposname = huniditySens.Blockname;
			}
        }
		private string GetBlockName(HumiditySens humiditySens, Sensor.SensorType sensorType)
		{
			string ret = "null";
			if (humiditySens != null)
			{
				humiditySens._SensorType = sensorType;
				ret = humiditySens.Blockname;
			}
			return ret;
		}
		private string SetPosName(string value	)
		{
			string ret = "null";
			
			if (_HumiditySens != null)
			{

				_HumiditySens.PosName = value;
				
				ret = _HumiditySens.PosName;
			}
			return ret;
		}
		private string GetPosName()
		{
			if (_HumiditySens != null)
			{
				return _HumiditySens.PosName;
			}
			return string.Empty;
		}
		internal string SortPriority { get => sortpriority;
            set => MakeSortPriority(Voltage);
        }
		private string MakeSortPriority(_Voltage voltage)
		{
			sortpriority = string.Empty;
			switch (voltage)
			{
				case _Voltage.AC380:
					sortpriority = "04";
					break;
				default:
					sortpriority = "09";
					break;

			}
			if (this.Cable1 != null) Cable1.SortPriority = sortpriority;
			if (this.Cable2 != null) Cable2.MakeControlSortpriority(sortpriority + "2");
			if (this.Cable3 != null) Cable3.MakeControlSortpriority(sortpriority + "3");

			return sortpriority;

		}
		private void CheckHumContol(object val)
		{
			humContol = (HumContolType)val;
			switch (humContol)
			{
				case HumContolType.Analogue:
					if (Cable2 != null) Cable2.Attrubute = Cable.CableAttribute.A;
					ShemaASU.ReSetIO(ShemaASU.IOType.DO);
					ShemaASU.SetIO(ShemaASU.IOType.AO, 1);
					break;
				case HumContolType.Discrete:
					if (Cable2 != null) Cable2.Attrubute = Cable.CableAttribute.D;
					ShemaASU.ReSetIO(ShemaASU.IOType.AO);
					ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
					break;
			}
        }
		private void CheckVoltage(object val)
		{
			VoltageVariable = (_Voltage)val;
			MakeSortPriority(VoltageVariable);
        }
		private void CheckSens(object val)
		{
			senspresent = Convert.ToBoolean(val);
			if (senspresent)
			{
				MakeSens(ref _HumiditySens, ref sensblockname, ref sensposkname);
				_HumiditySens._SensorType = Sensor.SensorType.Discrete;
				
				SensorType = _HumiditySens._SensorType;

			}
			else
            {
				_HumiditySens = null;
				SensorType = Sensor.SensorType.No;
            }
        }
		public List<dynamic> GetSensors()
        {
			return new List<dynamic>
			{
				this._HumiditySens
			};


				
        }
		#endregion
		internal class HumiditySens : Sensor
		{
			internal new string SortPriority => sortpriority;
            internal bool insideHum;
            internal override void CheckSensorType(object val)
			{
				
				sensorType= (SensorType)val;
				if (ShemaASU == null) ShemaASU= ShemaASU.CreateBaseShema();
				switch (sensorType)
				{
					case SensorType.Analogue:
						this.SetVendorInfo(null, null, null, "SensHE", null);
						this.MainDBTable = "SensHUM";
						PosName = "ME";
						Blockname = "SENS-ME-2WIRE";
						sortpriority = "21";
						if (Cable1 != null) Cable1.Attrubute = Cable.CableAttribute.A;
						else
						{
							Cable1 = new Cable
							{
								Attrubute = Cable.CableAttribute.A,
								Description = Description,
								WireNumbers = 2,
								WriteBlock = true
							};

						}
						ShemaASU?.ReSetIO(ShemaASU.IOType.DI);
						ShemaASU?.SetIO(ShemaASU.IOType.AI, 1);
						this.SensorInsideCrossSection = false;


						break;
					case SensorType.Discrete:
						this.SetVendorInfo(null, null, null, "SensHS", null);
						this.MainDBTable = "SensHUM";
						PosName = "MS";
						Blockname = "SENS-MS-2WIRE";
						sortpriority = "28";
						if (Cable1 != null) Cable1.Attrubute = Cable.CableAttribute.D;
						else
						{
							Cable1 = new Cable
							{
								Attrubute = Cable.CableAttribute.D,
								Description = Description,
								WireNumbers = 2,
								WriteBlock = true
							};
						}
						ShemaASU?.ReSetIO(ShemaASU.IOType.AI);
						ShemaASU?.SetIO(ShemaASU.IOType.DI, 1);
						this.SensorInsideCrossSection = false;

						break;
					case SensorType.No:
						
						PosName = string.Empty;
						Blockname = string.Empty;
						Cable1 = null;
						ShemaASU = null;
						break;

				}
				if (Cable1 != null)
				{
					Cable1.SortPriority = sortpriority;
					Cable1.ToBlockName = Blockname;
					Cable1.ToPosName = PosName;
					Cable1.WireNumbers = 2;
					Cable1.WriteBlock = true;
				}

			}

            private bool sensorInsideCrossSection;
			internal bool SensorInsideCrossSection
			{
				get => sensorInsideCrossSection;
                set
				{
					sensorInsideCrossSection = value;
					if (value)
					{
						if (ShemaASU != null) ShemaASU.ShemaUp = "Hum_CrossSection";

					}
					else
					{
                        if (ShemaASU != null)
                        {
                            ShemaASU.ShemaUp = insideHum? "Humidifier_Hygrostat": "Hum_Room";
                        }
					}

				}
			}
			public HumiditySens()
            {
				//this.ShemaASU = ShemaASU.CreateBaseShema();
                base.Description = "Капилярный гиростат";
               
            }
		}
		public Humidifier()
		{
			Voltage = _Voltage.AC220;
			Displacement = -28.75646364;
			Power = "0";
			ComponentVariable = VentComponents.Humidifier;
			Description = "Увлажнитель в приточном канале";
			ShemaASU = ShemaASU.CreateBaseShema(ComponentVariable);
			ShemaASU.componentPlace = ShemaASU.ComponentPlace.Supply;
			ShemaASU.ShemaUp = "Supply_Humidifier";
			HumControlType = HumContolType.Analogue;
			ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
			Cable1 = new Cable
			{
				Attrubute = Cable.CableAttribute.P,
				Description = Description,
				ToBlockName = BlockName,
				ToPosName = PosName,
				SortPriority = this.SortPriority,
				WriteBlock = true,
				WireNumbers = 3
			};
			Cable2 = new Cable
			{
				Attrubute = Cable.CableAttribute.A,
				Description = "Управление увлажнителем",
				ToBlockName = BlockName,
				ToPosName = PosName,
				WriteBlock = false,
				WireNumbers = 2
			};
			Cable3 = new Cable
			{
				Attrubute = Cable.CableAttribute.D,
				Description = "Авария увлажнителя",
				ToBlockName = BlockName,
				ToPosName = PosName,
				WriteBlock = false,
				WireNumbers = 2
			};
			Cable2.MakeControlSortpriority(SortPriority + "2");
			Cable3.MakeControlSortpriority(SortPriority + "3");

			
			


		}

	}
	#endregion
	#region Recuperator

    internal class Recuperator :IEnumerable, ICompGUID, IGetSensors
	{
		#region Pictures Path
		public static string ImageNullPath = VentSystem.AppFolder + "Images\\9_0.png";
		public string Imagepath;
		#endregion
		#region Enums
		[System.ComponentModel.TypeConverter(typeof(EnumDescConverter))]
		public enum RecuperatorType
		{
			[Description("Рециркуляция")]
			Recirculation,
			[Description("Роторный с регулированием")]
			RotorControl,
			[Description("Пластинчатый без байпаса")]
			LaminatedNoBypass,
			[Description("Пластинчатый с байпасом")]
			LaminatedBypass,
			[Description("Гликолиевый")]
			Glycol

		}
		[System.ComponentModel.TypeConverter(typeof(EnumDescConverter))]
		public enum RecirculationType
		{
			[Description("Нет")]
			No,
			[Description("1")]
			One,
			[Description("2")]
			Two,
			[Description("3")]
			Three
		}
		#endregion
		#region Variables

        private RecuperatorType recuperatortype;
        private RecirculationType recirculationtype;
		protected Drive _Drive1;
		protected Drive _Drive2;
		protected Drive _Drive3;
		protected SensorT _ProtectSensor1;
		protected PressureContol _ProtectSensor2;
		private VentComponents ComponentVariable;
		private string SensorBockName;

		private string Dev1BlockName;
		private string Dev2BlockName;
		private string Dev3BlockName;
		internal ShemaASU ShemaASU;
		#endregion
		#region Properties
		internal Drive Drive1 { get => _Drive1; set => _Drive1 = value; }
		internal Drive Drive2 { get => _Drive2; set => _Drive2 = value; }
		internal Drive Drive3 { get => _Drive3; set => _Drive3 = value; }
		internal SensorT protectSensor1 { get => _ProtectSensor1; set => _ProtectSensor1 = value; }
		internal PressureContol protectSensor2 { get { return _ProtectSensor2; } set { _ProtectSensor2 = value; } }
		[DisplayName("Тип элемента")]
		public VentComponents comp { get { return ComponentVariable; } private set { ComponentVariable = value; } }
		[DisplayName("Тип рекуператора")]
		public RecuperatorType _RecuperatorType { get { return recuperatortype; } set { SetRecuperatorType(value, ref Imagepath, ref _ProtectSensor1, ref _ProtectSensor2, ref _Drive1, ref _Drive2, ref _Drive3, ref recirculationtype); } }
		[DisplayName("Количество приводов")]
		public RecirculationType _RecirculationType { get { return recirculationtype; } internal set { recirculationtype = value; } }
		[DisplayName("Имя блока привода 1")]
		public string Drive1BlockName { get { return GetDriveBlockName(_Drive1); } protected set { Dev1BlockName = value; } }
		internal string Drive1PosName { get { return GetDrivePosName(_Drive1); } }
		[DisplayName("Имя блока привода 2")]
		public string Drive2BlockName { get { return GetDriveBlockName(_Drive2); } protected set { Dev2BlockName = value; } }
		internal string Drive2PosName { get { return GetDrivePosName(_Drive2); } }
		[DisplayName("Имя блока привода 3")]
		public string Drive3BlockName { get { return GetDriveBlockName(_Drive3); } protected set { Dev3BlockName = value; } }
		internal string Drive3PosName { get { return GetDrivePosName(_Drive3); } }
		[DisplayName("Обозначение датчика защиты 1")]
		public string Sens1PosName { get { return GetSensorPos(_ProtectSensor1); } }
		[DisplayName("Имя блока датчика защиты 1")]
		public string PS1BlockName { get { return GetSensorName(_ProtectSensor1); } }
		[DisplayName("Обозначение датчика защиты 2")]
		public string Sens2PosName { get { return GetSensorPos(_ProtectSensor2); } }

		[DisplayName("Имя блока датчика защиты 2")]
		public string PS2BlockName { get { return GetSensorName(_ProtectSensor2); } }
		public string GUID { get; set; }
		#endregion
		#region Methods
		internal string makeimagepath(RecuperatorType recuperatorType)
		{
			Imagepath = ImageNullPath;
			switch (recuperatorType)
			{
				case RecuperatorType.Recirculation:
					Imagepath = VentSystem.AppFolder + "Images\\9_4.png";
					break;

				case RecuperatorType.RotorControl:
					Imagepath = VentSystem.AppFolder + "Images\\9_1.png";
					break;
				case RecuperatorType.LaminatedNoBypass:
					Imagepath = VentSystem.AppFolder + "Images\\9_2.png";
					break;
				case RecuperatorType.LaminatedBypass:
					Imagepath = VentSystem.AppFolder + "Images\\9_3.png";
					break;
				case RecuperatorType.Glycol:
					Imagepath = VentSystem.AppFolder + "Images\\9_5.png";
					break;


			}
			return Imagepath;
		}
		private void SetRecuperatorType(object val, ref string imagepath, ref SensorT ps1, ref PressureContol ps2,
            ref Drive drive1, ref Drive drive2, ref Drive drive3, ref RecirculationType recirculation)
		{
			recuperatortype = (RecuperatorType)val;
			ps1 = null;
			ps2 = null;
			drive1 = null;
			drive2 = null;
			drive3 = null;
			imagepath = makeimagepath(recuperatortype);
			if (drive1 != null) drive1.Cable2 = null;
			if (drive2 != null) drive2.Cable2 = null;
			if (drive3 != null) drive3.Cable2 = null;


			switch (recuperatortype)
			{

				case RecuperatorType.RotorControl:
					recirculation = RecirculationType.One;
					drive1 = new Drive
					{
						Description = "Э/д привода рекуператоора",
						SortPriority = "18"

					};
					Dev1BlockName = GetDriveBlockName(drive1);

					drive1.Cable1 = new Cable
					{
						WriteBlock = true,
						ToPosName = drive1.Posname,
						ToBlockName = drive1.BlockName,
						SortPriority = drive1.SortPriority,
						Attrubute = Cable.CableAttribute.PL,
						Description = drive1.Description,
						WireNumbers = 3

					};
					drive1.Cable2 = new Cable
					{
						WriteBlock = false,
						ToPosName = drive1.Posname,
						ToBlockName = drive1.BlockName,
						Attrubute = Cable.CableAttribute.A,
						Description = "Управление приводом рекуператора",
						WireNumbers = 2
					};
					drive1.Cable2.MakeControlSortpriority(drive1.SortPriority);
					ps2 = new PressureContol
					{
						Blockname = "SENS-PDS-2WIRE",
						PosName = "PDS",
						Description = "Защита рекупертора по перепаду давления",
						_SensorType = Sensor.SensorType.Discrete

					};
					ps2.Cable1 = new Cable
					{
						Attrubute = Cable.CableAttribute.D,
						WriteBlock = true,
						ToPosName = ps2.PosName,
						ToBlockName = ps2.Blockname,
						Description = ps2.Description,
						SortPriority = ps2.SortPriority,
						WireNumbers = 2
					};
					
					ShemaASU.ShemaUp = "Recuperator_Rotor";
					ShemaASU.ReSetAllIO();
					ShemaASU.SetIO(ShemaASU.IOType.AO, 1);
					ShemaASU.SetIO(ShemaASU.IOType.DI, 1);

					break;
				case RecuperatorType.LaminatedBypass:
					recirculation = RecirculationType.One;
					drive1 = new Recuperator.Drive
					{
						Description = "Э/д привода заслонки байпаса рекуператора",
						SortPriority = "18"
					};
					Dev1BlockName = GetDriveBlockName(drive1);
					drive1.Cable1 = new Cable
					{
						WriteBlock = true,
						ToPosName = drive1.Posname,
						ToBlockName = drive1.BlockName,
						SortPriority = drive1.SortPriority,
						Attrubute = Cable.CableAttribute.P,
						Description = drive1.Description,
						WireNumbers = 3

					};
					drive1.Cable2 = new Cable
					{
						WriteBlock = false,
						ToPosName = drive1.Posname,
						ToBlockName = drive1.BlockName,
						Attrubute = Cable.CableAttribute.A,
						Description = "Управление приводом рекуператора",
						WireNumbers = 2
					};
					drive1.Cable2.MakeControlSortpriority(drive1.SortPriority);
					ps2 = new PressureContol
					{
						//Blockname = "SENS-PDS-2WIRE",
						//PosName = "PDS",
						_SensorType = Sensor.SensorType.Discrete,
						Description = "Защита рекупертора по перепаду давления",
					};
					ps2.Cable1 = new Cable
					{

						Attrubute = Cable.CableAttribute.D,
						WriteBlock = true,
						ToPosName = ps2.PosName,
						ToBlockName = ps2.Blockname,
						Description = ps2.Description,
						SortPriority = ps2.SortPriority,
						WireNumbers = 2
					};

					ps1 = new SensorT
					{
						//Blockname = "SENS-TE-2WIRE",
						//PosName = "TE",
						_SensorType = Sensor.SensorType.Discrete,
						Description = "Защита рекуператора по температуре",
					};
					ps1.Cable1 = new Cable
					{
						Attrubute = Cable.CableAttribute.D,
						WriteBlock = true,
						ToPosName = ps1.PosName,
						ToBlockName = ps1.Blockname,
						Description = ps1.Description,
						SortPriority = ps1.SortPriority,
						WireNumbers = 2

					};
					ShemaASU.ShemaUp = "Recuperator_Plast_Bypass";
					ShemaASU.ReSetAllIO();
					ShemaASU.SetIO(ShemaASU.IOType.AO, 1);
					ShemaASU.SetIO(ShemaASU.IOType.DI, 1);

					break;
				case RecuperatorType.Recirculation:
					recirculation = RecirculationType.Three;
					drive1 = new Drive
					{
						Description = "Э/д заслонки 1 рекуператора",
						SortPriority = "18"
					};
					drive2 = new Drive
					{
						Description = "Э/д заслонки 2 рекуператора",
						SortPriority = "19"
					};
					drive3 = new Drive
					{
						Description = "Э/д заслонки 3 рекуператора",
						SortPriority = "20"
					};
					Dev1BlockName = GetDriveBlockName(drive1);
					Dev2BlockName = GetDriveBlockName(drive2);
					Dev3BlockName = GetDriveBlockName(drive3);
					drive1.Cable1 = new Cable
					{
						WriteBlock = true,
						ToPosName = drive1.Posname,
						ToBlockName = Dev1BlockName,
						SortPriority = drive1.SortPriority,
						Attrubute = Cable.CableAttribute.PL,
						Description = drive1.Description,
						WireNumbers = 3

					};
					drive2.Cable1 = new Cable
					{
						WriteBlock = true,
						ToPosName = drive2.Posname,
						ToBlockName = Dev2BlockName,
						SortPriority = drive2.SortPriority,
						Attrubute = Cable.CableAttribute.PL,
						Description = drive2.Description,
						WireNumbers = 3

					};
					drive3.Cable1 = new Cable
					{
						WriteBlock = true,
						ToPosName = drive3.Posname,
						ToBlockName = Dev3BlockName,
						SortPriority = drive3.SortPriority,
						Attrubute = Cable.CableAttribute.PL,
						Description = drive3.Description,
						WireNumbers = 3

					};

					drive1.Cable2 = new Cable
					{
						WriteBlock = false,
						ToPosName = drive1.Posname,
						ToBlockName = drive1.BlockName,
						Attrubute = Cable.CableAttribute.A,
						Description = "Управление приводом рекуператора",
						WireNumbers = 2
					};
					drive1.Cable2.MakeControlSortpriority(drive1.SortPriority);

					drive2.Cable2 = new Cable
					{
						WriteBlock = false,
						ToPosName = drive2.Posname,
						ToBlockName = drive2.BlockName,
						Attrubute = Cable.CableAttribute.A,
						Description = "Управление приводом рекуператора",
						WireNumbers = 2
					};
					drive2.Cable2.MakeControlSortpriority(drive2.SortPriority);

					drive3.Cable2 = new Cable
					{
						WriteBlock = false,
						ToPosName = drive3.Posname,
						ToBlockName = drive3.BlockName,
						Attrubute = Cable.CableAttribute.A,
						Description = "Управление приводом рекуператора",
						WireNumbers = 2
					};
					drive3.Cable2.MakeControlSortpriority(drive3.SortPriority);
					
					ShemaASU.ShemaUp = "Recirculation";
					ShemaASU.ReSetAllIO();
					ShemaASU.SetIO(ShemaASU.IOType.DO, 3);
					break;
				case RecuperatorType.LaminatedNoBypass:
					recirculation = RecirculationType.No;
					ps2 = new PressureContol
					{
						//Blockname = "SENS-PDS-2WIRE",
						//PosName = "PDS",
						_SensorType = Sensor.SensorType.Discrete,
						Description = "Защита рекупертора по перепаду давления",
					};
					ps2.Cable1 = new Cable
					{

						Attrubute = Cable.CableAttribute.D,
						WriteBlock = true,
						ToPosName = ps2.PosName,
						ToBlockName = ps2.Blockname,
						Description = ps2.Description,
						SortPriority = ps2.SortPriority,
						WireNumbers = 2
					};
					ShemaASU.ShemaUp = "Recuperator_Plast";
					ShemaASU.ReSetAllIO();
					ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
					break;
				case RecuperatorType.Glycol:
					recirculation = RecirculationType.Two;
					drive1 = new Recuperator.Drive
					{
						Description = "Э/д привода рекуператоора",
						SortPriority = "18"

					};
					Dev1BlockName = GetDriveBlockName(drive1);

					drive1.Cable1 = new Cable
					{
						WriteBlock = true,
						ToPosName = drive1.Posname,
						ToBlockName = drive1.BlockName,
						SortPriority = drive1.SortPriority,
						Attrubute = Cable.CableAttribute.PL,
						Description = drive1.Description,
						WireNumbers = 3

					};
					drive1.Cable2 = new Cable
					{
						WriteBlock = false,
						ToPosName = drive1.Posname,
						ToBlockName = drive1.BlockName,
						Attrubute = Cable.CableAttribute.A,
						Description = "Управление приводом рекуператора",
						WireNumbers = 2
					};
					drive1.Cable2.MakeControlSortpriority(drive1.SortPriority);

					drive2 = new Recuperator.Drive
					{
						Description = "Э/д насоса рекуператора",
						SortPriority = "19"
					};
					drive2.Cable1 = new Cable
					{
						WriteBlock = true,
						ToPosName = drive2.Posname,
						ToBlockName = Dev2BlockName,
						SortPriority = drive2.SortPriority,
						Attrubute = Cable.CableAttribute.PL,
						Description = drive2.Description,
						WireNumbers = 3

					};
					ps2 = new PressureContol
					{
						_SensorType = Sensor.SensorType.Discrete,
						Description = "Защита рекупертора по перепаду давления",
					};
					ps2.Cable1 = new Cable
					{

						Attrubute = Cable.CableAttribute.D,
						WriteBlock = true,
						ToPosName = ps2.PosName,
						ToBlockName = ps2.Blockname,
						Description = ps2.Description,
						SortPriority = ps2.SortPriority,
						WireNumbers = 2
					};

					ShemaASU.ShemaUp = "Recuperator_Glicol";
					ShemaASU.ReSetAllIO();
					ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
					ShemaASU.SetIO(ShemaASU.IOType.AO, 1);
					ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
					break;
			}
        }
		private string GetDriveBlockName(Drive drive)
		{
			string DiveBlockName = string.Empty;
			if (drive != null)
			{
				DiveBlockName = drive.MakeDriveBlockName(recuperatortype);

			}
			return DiveBlockName;
		}
		private string GetDrivePosName(Drive drive)
		{
			string DivePosName = string.Empty;
			if (drive != null)
			{

				DivePosName = drive.Posname;
			}
			return DivePosName;
		}
		private string GetSensorName(dynamic protectSensor)
		{
			SensorBockName = string.Empty;
			if (protectSensor != null)
			{
				SensorBockName = protectSensor.Blockname;
			}
			return SensorBockName;
		}
		private string GetSensorPos(dynamic protectSensor)
		{
			if (protectSensor != null)
			{

				return protectSensor.PosName;

			}
			return string.Empty;
		}
		public IEnumerator<object> GetEnumerator()
		{
			yield return _Drive1;
			yield return _Drive2;
			yield return _Drive3;
			yield return _ProtectSensor1;
			yield return _ProtectSensor2;

		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		public List<dynamic> GetSensors()
		{
			return new List<dynamic>
			{
				this._ProtectSensor1,
				this._ProtectSensor2
			};
		}
		#endregion
		#region Internal Classes
		internal class Drive : WaterHeater.Valve, IPower
		{

			public Drive()
			{
				Posname = "FV";
				_ValveType = ValveType.Analogue_0_10;
				Power = "10";


			}
			internal string MakeDriveBlockName(object val)

			{
                RecuperatorType recuperatorType = (RecuperatorType)val;
                string driveblockname;
                switch (recuperatorType)
                {
                    case RecuperatorType.RotorControl:
                    case RecuperatorType.LaminatedBypass:
                        driveblockname = "ED-24-010V";
                        break;
                    default:
                        driveblockname = "ED-24";
                        break;

                }


                return driveblockname;
			}
			//internal override string MakeDamperBlockName()
			//{
			//    throw new NotImplementedException();
			//}


		}
		
		
		internal class Pump : Damper, IPower
		{

			private string blockname;
			internal Pump()
			{
				PosName = "M";
				Description = "Э/д насоса рекуператора";
				Cable1 = new Cable
				{
					Attrubute = Cable.CableAttribute.P,
					WriteBlock = true,
					ToPosName = PosName,
					Description = Description,
					ToBlockName = BlockName,
					WireNumbers = 3
				};
				Cable1.SortPriority = this.SortPriority;
				Power = "300";
				
			}
			internal bool HasTK { get { return hascontrol; } set { PumpSetTK(value, this); } }
			
			internal override string MakeDamperBlockName()
			{
				if (this.HasTK)

				{
					blockname = "ED_Pump_220_TK";
				}
				else
				{
					blockname = "ED_Pump_220";
				}

				if (Cable1 != null)
				{
					Cable1.ToBlockName = blockname;
				}
				if (Cable2 != null)
				{
					Cable2.ToPosName = PosName;
					Cable2.ToBlockName = blockname;
				}
				return blockname;
			}
			internal string SortPriority { get { return MakeSortPriority(); } }
			private string MakeSortPriority()
			{
				string sortpriority = "11";
				Cable1.SortPriority = sortpriority;
				if (Cable2 != null) Cable2.MakeControlSortpriority(sortpriority);
				return sortpriority;

			}
			private void PumpSetTK(object val, Pump pump)
			{
				hascontrol = Convert.ToBoolean(val);
				if (hascontrol)
				{
					Cable2 = new Cable
					{
						Attrubute = Cable.CableAttribute.D,
						WriteBlock = false,
						ToPosName = PosName,
						ToBlockName = BlockName,
						Description = "Термоконтакты насоса нагревателя",
						WireNumbers = 2
					};

					Cable2.SortPriority = pump.SortPriority + "1";

				}
				else { Cable2 = null; }
			}
		}


		#endregion
		#region Constructors
		public Recuperator()
		{
			ComponentVariable = VentComponents.Recuperator;
			ShemaASU = ShemaASU.CreateBaseShema(ComponentVariable);
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;

			_RecuperatorType = RecuperatorType.LaminatedBypass;
			Imagepath = makeimagepath(recuperatortype);


		}
		#endregion


	}
	#endregion
	#region Sensors
	
	abstract class Sensor  : ICompGUID, IVendoInfo
	{
		
		[System.ComponentModel.TypeConverter(typeof(EnumDescConverter))]
		public enum SensorType
		{
			[Description("Нет")]
			No,
			[Description("Аналоговый")]
			Analogue,
			[Description("Дискретный")]
			Discrete
		}
		private protected SensorType sensorType;
		internal string sortpriority;
		internal string VendorName;
		internal string ID;
		internal string VendorDescription;
		internal string VendorDBTable;
		internal string MainDBTable;
		internal string Assignment;
		//internal string DefaultDescription = "Абстрактный датчик";


		[DisplayName("Имя блока датчика")]
		public string Blockname { get; internal set; }
		[DisplayName("Обозначение")]
		public string PosName { get; internal set; }
		//private SensorType _sensorType;
		//public SensorType _SensorType  { get { return _sensorType; } set { _sensorType = value; }}
		public virtual SensorType _SensorType { get { return sensorType; } set { CheckSensorType(value); } }
		public string GUID { get; set; }
		internal string Description { get; set; }
		internal string Location { get; set; }
		
		

		internal virtual string SortPriority { get { return sortpriority; } }
		//internal CableTypes Cable1Type { get; set; }
		internal Cable Cable1;
		internal abstract void CheckSensorType(object val);
		internal ShemaASU ShemaASU;
		public (string VendorName, string ID, string VendorDescription, string DBTable, string Assignment, string DefaultDescription, string MainDBTable) GetVendorInfo()
        {
			(string VendorName, string ID, string VendorDescription, string DBTable, string Assignment, string DefaultDescription, string MainDBTable) vendorinfo;
			vendorinfo.VendorName = VendorName;
			vendorinfo.ID = ID;
			vendorinfo.VendorDescription = VendorDescription;
			vendorinfo.DBTable = VendorDBTable;
			vendorinfo.DefaultDescription = this.Description;//;Description;
			vendorinfo.Assignment = Assignment;
			vendorinfo.MainDBTable = this.MainDBTable;
			return vendorinfo;
        }
		public void SetVendorInfo(string vendorName, string ID, string vendorDescription, string dbTable, string assignment)
        {
			if (!string.IsNullOrEmpty(vendorName)) this.VendorName = vendorName;
			if (!string.IsNullOrEmpty(ID)) this.ID = ID;
			if (!string.IsNullOrEmpty(vendorDescription)) this.VendorDescription = vendorDescription;
			if (!string.IsNullOrEmpty(dbTable)) this.VendorDBTable = dbTable;
			if (!string.IsNullOrEmpty(assignment)) this.Assignment = assignment;
		}
		public void ClearVendorInfo()
		{
			this.VendorName = string.Empty;
			this.ID = string.Empty;
			this.VendorDescription = string.Empty;
			this.Assignment = string.Empty;
			this.VendorDBTable = string.Empty;
			
		}


	}
	class SensorT : Sensor, IGetSensors, IVendoInfo
	{
		internal override void CheckSensorType(object val)
		{
			sensorType = (SensorType)val;
			switch ((SensorType)val)
			{
				case SensorType.Analogue:
					
					PosName = "TE";
					Blockname = "SENS-TE-2WIRE";
					sortpriority = "26";
					
					this.SetVendorInfo(null, null, null, "SensTE", null);
					this.MainDBTable = "SensT";
					
					if (Cable1 != null) Cable1.Attrubute = Cable.CableAttribute.A;
					else {
						Cable1 = new Cable
						{
							Attrubute = Cable.CableAttribute.A,
							Description = Description,
							WriteBlock = true,
							WireNumbers = 2
						};
					}

					if (ShemaASU != null)
					{
						ShemaASU.ReSetAllIO();
						ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
					}
					else
					{
						ShemaASU = ShemaASU.CreateBaseShema();
						ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
						
					}

					break;
				case SensorType.Discrete:
					this.SetVendorInfo(null, null, null, "SensTS", null);
					this.MainDBTable = "SensT";
					PosName = "TS";
					Blockname = "SENS-TS-2WIRE";
					sortpriority = "33";
					if (Cable1 != null) Cable1.Attrubute = Cable.CableAttribute.D;
					else {
						Cable1 = new Cable
						{
							Attrubute = Cable.CableAttribute.D,
							Description = Description,
							WriteBlock = true,
							WireNumbers = 2
						};
					}
					if (ShemaASU != null)
					{
						ShemaASU.ReSetAllIO();
						ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
					}
					else
					{
						ShemaASU = ShemaASU.CreateBaseShema();
						ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
						
					}

					break;
				case SensorType.No:
					PosName = string.Empty;
					Blockname = string.Empty;
					Cable1 = null;
					this.ShemaASU = null;

					break;

			}
			if (Cable1 != null)
			{
				Cable1.SortPriority = sortpriority;
				Cable1.ToBlockName = Blockname;
				Cable1.ToPosName = PosName;
			}
			//return sensorType;

		}
		public SensorT(string description)
		{
			Description = description;
			ShemaASU = ShemaASU.CreateBaseShema();
			ShemaASU.ShemaUp = "Temp_NoPos";
            ShemaASU.Description1 = this.Description;


        }
		public SensorT()
		{
			ShemaASU = ShemaASU.CreateBaseShema();
			ShemaASU.ShemaUp = "Temp_NoPos";
		}
		public List<dynamic> GetSensors()
        {
			return new List<dynamic> { this };
        }

	}

    internal class OutdoorTemp : SensorT
	{
		private VentComponents ComponentVariable;
		
		internal new string SortPriority { get { return sortpriority; } }
		[DisplayName("Тип элемента")]
		public VentComponents comp { get { return ComponentVariable; } internal set { ComponentVariable = value; } }
		internal override void CheckSensorType(object val)
		{
			sensorType = (SensorType)val;
			switch (sensorType)
			{
				case SensorType.Analogue:
					PosName = "TE";
					Blockname = "SENS-TE-2WIRE";
					sortpriority = "22";
					this.SetVendorInfo(null, null, null, "SensTE", null);
					this.MainDBTable = "SensT";
					if (Cable1 != null) Cable1.Attrubute = Cable.CableAttribute.A;
					else
					{
						Cable1 = new Cable
						{
							Attrubute = Cable.CableAttribute.A,
							Description = Description,
							WriteBlock = true,
							WireNumbers = 2
						};

					}
					if (ShemaASU != null)
                    {
						ShemaASU.ReSetAllIO();
						ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
					}
					else
                    {
						ShemaASU = ShemaASU.CreateBaseShema(VentComponents.OutdoorTemp);
						ShemaASU.ShemaUp = "Temp_Supply";
						ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
						
                    }
					
					
					break;
				case SensorType.Discrete:
					PosName = "TS";
					Blockname = "SENS-TS-2WIRE";
					sortpriority = "29";
					this.SetVendorInfo(null, null, null, "SensTS", null);
					this.MainDBTable = "SensT";
					if (Cable1 != null) Cable1.Attrubute = Cable.CableAttribute.D;
					else
					{
						Cable1 = new Cable
						{
							Attrubute = Cable.CableAttribute.D,
							Description = Description,
							WriteBlock = true,
							WireNumbers = 2
						};
					}
					if (ShemaASU != null)
                    {
						ShemaASU.ReSetAllIO();
						ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
					}
					else
                    {
						ShemaASU = ShemaASU.CreateBaseShema(VentComponents.OutdoorTemp);
						ShemaASU.ShemaUp = "Temp_Supply";
						ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
						
					}
					break;
				case SensorType.No:
					this.SetVendorInfo(null, null, null, null, null);
					PosName = string.Empty;
					Blockname = string.Empty;
					Cable1 = null;
					ShemaASU = null;
					break;

			}
			if (Cable1 != null)
			{
				Cable1.SortPriority = sortpriority;
				Cable1.ToBlockName = Blockname;
				Cable1.ToPosName = PosName;
			}
			//return sensorType;

		}
		public OutdoorTemp()
		{
			ComponentVariable = VentComponents.OutdoorTemp;
			Description = "Датчик наружной температуры";
			_SensorType = SensorType.Analogue;
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
			ShemaASU.ShemaUp = "Temp_Supply";
			ShemaASU.ShemaPos = ShemaASUbase.DummyPos(ComponentVariable);
			ShemaASU.ScheUpSize = ShemaASUbase.DummySize(ComponentVariable);
            ShemaASU.Description1 = this.Description;


        }

	}//в заслонке
	class SupplyTemp : SensorT
	{
		private VentComponents ComponentVariable;
		
		internal new string SortPriority { get { return sortpriority; } }

		[DisplayName("Тип элемента")]
		public VentComponents comp { get { return ComponentVariable; } internal set { ComponentVariable = value; } }
		internal override void CheckSensorType(object val)
		{
			sensorType = (SensorType)val;
			switch (sensorType)
			{
				case SensorType.Analogue:
					PosName = "TE";
					Blockname = "SENS-TE-2WIRE";
					sortpriority = "23";
					this.SetVendorInfo(null, null, null, "SensTE", null);
					this.MainDBTable = "SensT";
					if (Cable1 != null) Cable1.Attrubute = Cable.CableAttribute.A;
					else
					{
						Cable1 = new Cable
						{
							Attrubute = Cable.CableAttribute.A,
							SortPriority = sortpriority,
							ToPosName = PosName,
							ToBlockName = Blockname,
							WriteBlock = true,
							Description = this.Description,
							WireNumbers = 2
						};
					}
					if (ShemaASU != null)
					{
						ShemaASU.ReSetAllIO();
						ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
					}
					else
					{
						ShemaASU = ShemaASU.CreateBaseShema(VentComponents.SupplyTemp);
						ShemaASU.ShemaUp = "Temp_Supply";
						ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
						
					}
					break;
				case SensorType.Discrete:
					PosName = "TS";
					Blockname = "SENS-TS-2WIRE";
					sortpriority = "30";
					this.SetVendorInfo(null, null, null, "SensTS", null);
					this.MainDBTable = "SensT";
					if (Cable1 != null) Cable1.Attrubute = Cable.CableAttribute.D;
					else
					{
						Cable1 = new Cable
						{
							Attrubute = Cable.CableAttribute.D,
							SortPriority = sortpriority,
							ToPosName = PosName,
							ToBlockName = Blockname,
							WriteBlock = true,
							Description = this.Description,
							WireNumbers = 2

						};
					}
					if (ShemaASU != null)
					{
						ShemaASU.ReSetAllIO();
						ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
					}
					else
					{
						ShemaASU = ShemaASU.CreateBaseShema(VentComponents.SupplyTemp);
						ShemaASU.ShemaUp = "Temp_Supply";
						ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
						
					}
					break;
				case SensorType.No:
					this.SetVendorInfo(null, null, null, null, null);
					PosName = string.Empty;
					Blockname = string.Empty;
					Cable1 = null;
					ShemaASU = null;
					break;

			}
			if (Cable1 != null)
			{
				Cable1.SortPriority = sortpriority;
				Cable1.ToBlockName = Blockname;
				Cable1.ToPosName = PosName;
			}
			//return sensorType;

		}
		public SupplyTemp()
		{
			ComponentVariable = VentComponents.SupplyTemp;
			Description = "Датчик темпертуры в приточном канале";
			_SensorType = SensorType.Analogue;
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
			ShemaASU.ShemaUp = "Temp_Supply";
			ShemaASU.ShemaPos = ShemaASUbase.DummyPos(ComponentVariable);
			ShemaASU.ScheUpSize = ShemaASUbase.DummySize(ComponentVariable);
            ShemaASU.Description1 = this.Description;
        }
	}//в воздуховоде
	class ExhaustTemp : SensorT
	{
		private VentComponents ComponentVariable;
		
		internal override string SortPriority { get { return sortpriority; } }

		[DisplayName("Тип элемента")]
		public VentComponents comp { get { return ComponentVariable; } internal set { ComponentVariable = value; } }
		[DisplayName("Тип датчика")]
		public new SensorType _SensorType
		{
			get
			{
				return sensorType;
			}
			set
			{
				sensorType = value;
				switch (sensorType)
				{
					case SensorType.Analogue:
						PosName = "TE";
						Blockname = "SENS-TE-2WIRE";
						sortpriority = "24";
						this.SetVendorInfo(null, null, null, "SensTE", null);
						this.MainDBTable = "SensT";
						if (Cable1 == null)
						{
							Cable1 = new Cable
							{
								Attrubute = Cable.CableAttribute.A,
								SortPriority = sortpriority,
								ToPosName = PosName,
								ToBlockName = Blockname,
								WriteBlock = true,
								Description = this.Description,
								WireNumbers = 2
							};
						}
						else
						{
							Cable1.Attrubute = Cable.CableAttribute.A;
							Cable1.SortPriority = sortpriority;
							Cable1.ToPosName = PosName;
							Cable1.ToBlockName = Blockname;
						}
						
						ShemaASU.ReSetAllIO();
						ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
						break;
					case SensorType.Discrete:
						PosName = "TS";
						Blockname = "SENS-TS-2WIRE";
						sortpriority = "31";
						this.SetVendorInfo(null, null, null, "SensTS", null);
						this.MainDBTable = "SensT";
						if (Cable1 == null)
						{
							Cable1 = new Cable
							{
								Attrubute = Cable.CableAttribute.D,
								SortPriority = sortpriority,
								ToPosName = PosName,
								ToBlockName = Blockname,
								WriteBlock = true,
								Description = this.Description,
								WireNumbers = 2
							};
						}
						else
						{
							Cable1.Attrubute = Cable.CableAttribute.D;
							Cable1.SortPriority = sortpriority;
							Cable1.ToPosName = PosName;
							Cable1.ToBlockName = Blockname;
						}
						ShemaASU.ReSetAllIO();
						ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
						break;
					case SensorType.No:
						this.SetVendorInfo(null, null, null, null, null);
						PosName = string.Empty;
						Blockname = string.Empty;
						Cable1 = null;
						ShemaASU = null;
						break;
				}
			}
		}
		internal override void CheckSensorType(object val)
		{
			sensorType = (SensorType)val;
			switch (sensorType)
			{
				case SensorType.Analogue:
					PosName = "TE";
					Blockname = "SENS-TE-2WIRE";
					sortpriority = "24";
					if (Cable1 != null) Cable1.Attrubute = Cable.CableAttribute.A;
					else
					{
						Cable1 = new Cable
						{
							Attrubute = Cable.CableAttribute.A,
							SortPriority = sortpriority,
							ToPosName = PosName,
							ToBlockName = Blockname,
							WriteBlock = true,
							Description = this.Description,
							WireNumbers = 2
						};
					}
					ShemaASU.ReSetAllIO();
					ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
					break;
				case SensorType.Discrete:
					PosName = "TS";
					Blockname = "SENS-TS-2WIRE";
					sortpriority = "31";
					if (Cable1 != null) Cable1.Attrubute = Cable.CableAttribute.D;
					else
					{
						Cable1 = new Cable
						{
							Attrubute = Cable.CableAttribute.D,
							SortPriority = sortpriority,
							ToPosName = PosName,
							ToBlockName = Blockname,
							WriteBlock = true,
							Description = this.Description,
							WireNumbers = 2
						};
					}
					ShemaASU.ReSetAllIO();
					ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
					break;
				case SensorType.No:
					PosName = string.Empty;
					Blockname = string.Empty;
					ShemaASU = null;
					Cable1 = null;
					break;

			}
			if (Cable1 != null)
			{
				Cable1.SortPriority = sortpriority;
				Cable1.ToBlockName = Blockname;
				Cable1.ToPosName = PosName;
			}
			//return sensorType;

		}
		public ExhaustTemp()
		{
			ComponentVariable = VentComponents.ExtTemp;
			Description = "Датчик температуры в вытяжном канале";
			_SensorType = SensorType.Analogue;
			ShemaASU.ShemaUp = "Temp_Exihaust";
			ShemaASU.ShemaPos = ShemaASUbase.DummyPos(ComponentVariable);
            ShemaASU.Description1 = this.Description;

        }
	} //в воздуховоде
	class IndoorTemp : SensorT
	{
		private VentComponents ComponentVariable;
		
		internal new string SortPriority { get { return sortpriority; } }
		[DisplayName("Тип элемента")]
		public VentComponents comp { get { return ComponentVariable; } internal set { ComponentVariable = value; } }
		public new SensorType _SensorType
		{
			get
			{
				return sensorType;
			}
			set
			{
				sensorType = value;
				switch (sensorType)
				{
					case SensorType.Analogue:
						PosName = "TE";
						Blockname = "SENS-TE-2WIRE";
						sortpriority = "25";
						this.SetVendorInfo(null, null, null, "SensTE", null);
						this.MainDBTable = "SensT";
						if (Cable1 == null)
						{
							Cable1 = new Cable
							{
								Attrubute = Cable.CableAttribute.A,
								SortPriority = sortpriority,
								ToPosName = PosName,
								ToBlockName = Blockname,
								WriteBlock = true,
								Description = this.Description,
								WireNumbers = 2
							};
						}
						else
						{
							Cable1.Attrubute = Cable.CableAttribute.A;
							Cable1.SortPriority = sortpriority;
							Cable1.ToPosName = PosName;
							Cable1.ToBlockName = Blockname;
						}
						if (ShemaASU != null)
						{
							ShemaASU.ReSetAllIO();
							ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
						}
						else
						{
							ShemaASU = ShemaASU.CreateBaseShema(VentComponents.IndoorTemp);
							ShemaASU.ShemaUp = "Temp_Indoor";
							ShemaASU.SetIO(ShemaASU.IOType.AI, 1);

						}
						break;
						
					case SensorType.Discrete:
						PosName = "TS";
						Blockname = "SENS-TS-2WIRE";
						sortpriority = "32";
						this.SetVendorInfo(null, null, null, "SensTS", null);
						this.MainDBTable = "SensT";
						if (Cable1 == null)
						{
							Cable1 = new Cable
							{
								Attrubute = Cable.CableAttribute.D,
								SortPriority = sortpriority,
								ToPosName = PosName,
								ToBlockName = Blockname,
								WriteBlock = true,
								Description = this.Description,
								WireNumbers = 2
							};
						}
						else
						{
							Cable1.Attrubute = Cable.CableAttribute.D;
							Cable1.SortPriority = sortpriority;
							Cable1.ToPosName = PosName;
							Cable1.ToBlockName = Blockname;
						}
						if (ShemaASU != null)
						{
							ShemaASU.ReSetAllIO();
							ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
						}
						else
						{
							ShemaASU = ShemaASU.CreateBaseShema(VentComponents.IndoorTemp);
							ShemaASU.ShemaUp = "Temp_Indoor";
							ShemaASU.SetIO(ShemaASU.IOType.DI, 1);

						}
						break;
					case SensorType.No:
						this.SetVendorInfo(null, null, null, null, null);
						PosName = string.Empty;
						Blockname = string.Empty;
						Cable1 = null;
						ShemaASU = null;
						break;
				}

			}
		}
		internal override void CheckSensorType(object val)
		{
			sensorType = (SensorType)val;
			switch (sensorType)
			{
				case SensorType.Analogue:
					PosName = "TE";
					Blockname = "SENS-TE-2WIRE";
					sortpriority = "25";
					if (Cable1 != null) Cable1.Attrubute = Cable.CableAttribute.A;
					else
					{
						Cable1 = new Cable
						{
							Attrubute = Cable.CableAttribute.A,
							Description = Description,
							WriteBlock = true,
							WireNumbers = 2
						};
					}
					break;
				case SensorType.Discrete:
					PosName = "TS";
					Blockname = "SENS-TS-2WIRE";
					sortpriority = "32";
					if (Cable1 != null) Cable1.Attrubute = Cable.CableAttribute.D;
					else
					{
						Cable1 = new Cable
						{
							Attrubute = Cable.CableAttribute.D,
							Description = Description,
							WriteBlock = true,
							WireNumbers = 2
						};
					}
					break;
				case SensorType.No:
					PosName = string.Empty;
					Blockname = string.Empty;
					Cable1 = null;
					break;

			}
			if (Cable1 != null)
			{
				Cable1.SortPriority = sortpriority;
				Cable1.ToBlockName = Blockname;
				Cable1.ToPosName = PosName;
			}
			//return sensorType;

		}
		public IndoorTemp()
		{
			ComponentVariable = VentComponents.IndoorTemp;
			Description = "Датчик температуры в помещении";
			_SensorType = SensorType.Analogue;
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
			ShemaASU.ShemaUp = "Temp_Indoor";
			ShemaASU.ShemaPos = ShemaASUbase.DummyPos(ComponentVariable);
			ShemaASU.ScheUpSize = ShemaASUbase.DummySize(ComponentVariable);
            ShemaASU.Description1 = this.Description;
        }
	} // в комнате
	class PressureContol : Sensor
	{
		
		internal new string SortPriority => MakeSortPriority(sensorType);

        public new SensorType SensorType
		{
			get => sensorType;
            set
			{
				sensorType = value;
				switch (sensorType)
				{
					case SensorType.Analogue:
						this.SetVendorInfo(null, null, null, "SensPE", null);
						this.MainDBTable = "SensPDS";
						this.PosName = "PE";
						this.Blockname = "SENS-PE-2WIRE";
						if (Cable1 == null) 
							Cable1 = new Cable
							{
								SortPriority =SortPriority,
								WireNumbers =2,
								WriteBlock = true,
								Attrubute = Cable.CableAttribute.A,
								Description = Description
							};
						break;
					case SensorType.Discrete:
						this.SetVendorInfo(null, null, null, "SensPS", null);
						this.MainDBTable = "SensPDS";
						this.PosName = "PDS";
						this.Blockname = "SENS-PDS-2WIRE";
						if (Cable1 == null) Cable1 = new Cable
						{
							SortPriority = SortPriority,
							WireNumbers = 2,
							WriteBlock = true,
							Attrubute = Cable.CableAttribute.D,
							Description = Description
							
						};
						
						break;
					case SensorType.No:
						PosName = string.Empty;
						Blockname = string.Empty;
						Cable1 = null;
						break;
				}
			}
		}

		private string MakeSortPriority(SensorType sensorType)
		{
			switch (sensorType)
			{
				case SensorType.Analogue:
					this.sortpriority = "27";
					if (Cable1 == null)
					{
						Cable1 = new Cable
						{
							Attrubute = Cable.CableAttribute.A,
							SortPriority = sortpriority,
							ToPosName = PosName,
							ToBlockName = Blockname,
							WriteBlock = true,
							Description = Description,
							WireNumbers = 2
						};
					}
					else
					{
						Cable1.Attrubute = Cable.CableAttribute.A;
						Cable1.SortPriority = sortpriority;
						Cable1.ToPosName = PosName;
						Cable1.ToBlockName = Blockname;
					}
					break;
				case SensorType.Discrete:
					this.sortpriority = "34";
					if (Cable1 == null)
					{
						Cable1 = new Cable
						{
							Attrubute = Cable.CableAttribute.D,
							SortPriority = sortpriority,
							ToPosName = PosName,
							ToBlockName = Blockname,
							WriteBlock = true,
							Description = Description,
							WireNumbers = 2
						};
					}
					else
					{
						Cable1.Attrubute = Cable.CableAttribute.D;
						Cable1.SortPriority = sortpriority;
						Cable1.ToPosName = PosName;
						Cable1.ToBlockName = Blockname;
						Cable1.WireNumbers = 2;
					}
					break;
				case SensorType.No:
					Cable1 = null;
					break;
			}
			return sortpriority;


		}
		internal override void CheckSensorType(object val)
		{
			
			sensorType = (SensorType)val;
			switch (sensorType)
			{
				case SensorType.Analogue:
					PosName = "PE";
					Blockname = "SENS-PE-2WIRE";
					sortpriority = "27";
					this.SetVendorInfo(null, null, null, "SensPE", null);
					this.MainDBTable = "SensPDS";
					if (Cable1 != null) Cable1.Attrubute = Cable.CableAttribute.A;
					else
					{
						Cable1 = new Cable
						{
							Attrubute = Cable.CableAttribute.A,
							Description = Description,


						};
					}
					ShemaASU.ReSetAllIO();
					ShemaASU.SetIO(ShemaASU.IOType.AI, 1);
					break;
				case SensorType.Discrete:
					PosName = "PDS";
					Blockname = "SENS-PDS-2WIRE";
					sortpriority = "34";
					this.SetVendorInfo(null, null, null, "SensPS", null);
					this.MainDBTable = "SensPDS";
					if (Cable1 != null) Cable1.Attrubute = Cable.CableAttribute.D;
					else
					{
						Cable1 = new Cable
						{
							Attrubute = Cable.CableAttribute.D,
							Description = Description

						};
					}
					ShemaASU.ReSetAllIO();
					ShemaASU.SetIO(ShemaASU.IOType.DI, 1);
					break;
				case SensorType.No:
					PosName = string.Empty;
					Blockname = string.Empty;
					Cable1 = null;
					ShemaASU.ReSetAllIO();
					break;

			}
			if (Cable1 != null)
			{
				Cable1.SortPriority = sortpriority;
				Cable1.ToBlockName = Blockname;
				Cable1.ToPosName = PosName;
				Cable1.WriteBlock = true;
				Cable1.WireNumbers = 2;
			}
			//return sensorType;

		}
		internal void SetDescrtiption(string descriptiontext)
		{
			if (Cable1 != null)
			{
				Description = descriptiontext;
				Cable1.Description = Description;
			}
		}

		public  PressureContol()
		{
			
			this.ShemaASU = ShemaASU.CreateBaseShema();
			
			_SensorType = SensorType.Discrete;
			
		}


	}
	class ElectroDevice 
	{
		[System.ComponentModel.TypeConverter(typeof(EnumDescConverter))]
		public enum _Voltage
		{
			[Description("~380")]
			AC380,
			[Description("~220")]
			AC220,
			[Description("~24")]
			AC24,
			[Description("=24")]
			DC24
		}
		[DisplayName("Напряжение")]
		public _Voltage Voltage { get { return VoltageVariable; } set { VoltageVariable = value; } }

		[DisplayName("Мощность")]
		public  string Power { get { return GetPower(); } set { SetPower(value); } }
		internal string Description { get; set; }
        protected _Voltage VoltageVariable;

		internal Cable Cable1;
		protected string sortpriority;
		private string _power;
		internal string GetPower()
        {
			return this._power;
        }
		internal void SetPower(object val)
        {
			_power = (string)val;
        }
	}
	#endregion
	class Cable
	{
		internal enum CableAttribute
		{
			[Description("Аналоговый")]
			A,
			[Description("Дискретный")]
			D,
			[Description("Управления")]
			C,
			[Description("Силовой")]
			P,
			[Description("Питание 24")]
			PL,
			[Description("Нет")]
			No
		}

        internal string SortPriority { get; set; }


        internal bool WriteBlock { get; set; }
		internal string cableGUID { get; set; }
		[DisplayName("Откуда")]
		public string FromPosName { get; set; }
		[DisplayName("Куда")]
		public string ToPosName { get; set; }
		//public string FromBlockName { get; set; }
		internal string ToBlockName { get; set; }
		internal string FromGUID { get; set; }
		internal string ToGUID { get; set; }
		[DisplayName("Имя кабеля")]
		public string CableName { get; set; }
		[DisplayName("Тип кабеля")]
		public string CableType { get; set; }
		//public string HostGUID { get; set; }
		internal string HostTable { get; set; }
		[DisplayName("Описание")]
		public string Description { get; set; }
		[DisplayName("Длина")]
		public double Lenght { get; set; }
		internal string CompTable { get; set; }
		[DisplayName("Количество жил")]
		public int WireNumbers { get; set; }
		[DisplayName("Атрибут")]
		public CableAttribute Attrubute { get; set; }
		internal string MakeControlSortpriority(string hostpriority)
		{

			SortPriority = hostpriority + "1";
			return SortPriority;
		}

	}
	[Serializable]
    abstract class ShemaASUbase : ISchemaASU
	{
		public enum ComponentPlace
		{
			Supply,
			Exhaust

		}
		public double ScheUpSize { get; set; }
		public int ShemaPos { get; set; }
		public string ShemaUp { get; set; }
		public ComponentPlace componentPlace { get; set; }
		
		public static double DummySize(VentComponents components)
		{
			switch (components)
			{
				case VentComponents.SupplyVent:
				case VentComponents.ExtVent:

					return 29.54;
				case VentComponents.SupplyFiltr:
				case VentComponents.ExtFiltr:
					return 29.85;
				case VentComponents.SupplyDamper:
				case VentComponents.ExtDamper:
					return 10;
				case VentComponents.WaterHeater:
					return 78.43;
				case VentComponents.ElectroHeater:
					return 24.48;
				case VentComponents.Froster:
					return 45.02;
				case VentComponents.Humidifier:
					return 34.48;
				case VentComponents.Recuperator:
					return 54.13;
				case VentComponents.OutdoorTemp:
				case VentComponents.SupplyTemp:
				case VentComponents.ExtTemp:
				case VentComponents.IndoorTemp:
					return 10.09;
				default:
					return 0;
			}
		}
		public static int DummyPos(VentComponents components)
        {
			switch (components)
			{
				case VentComponents.SupplyVent:
				case VentComponents.ExtVent:
					return 5;
				case VentComponents.SupplyFiltr:
					return 3;
				case VentComponents.ExtFiltr:
					return 11;
				case VentComponents.SupplyDamper:
				case VentComponents.ExtDamper:
					return 2;
				case VentComponents.WaterHeater:
					return 6;
				case VentComponents.ElectroHeater:
					return 7;
				case VentComponents.Froster:
					return 8;
				case VentComponents.Humidifier:
					return 9;
				case VentComponents.Recuperator:
					return 4;
				case VentComponents.OutdoorTemp:
					return 1;
				case VentComponents.SupplyTemp:
				case VentComponents.ExtTemp:
					return 10;
				case VentComponents.IndoorTemp:
					return 13;
					
				default:
					return 0;
			}
		}
		
	}
	[Serializable]
    class ShemaASU : ShemaASUbase
	{
		
		public enum IOType
		{
			DI,
			DO,
			AI,
			AO
		}

		public static string ShemaDown = "ФСА_Подвал_(КИП)";
		public static string BlockSignal = "ФСА_Подвал_(сигнал)";
		public bool DI;
		public bool DO;
		public bool AI;
		public bool AO;
		public int DIcnt ;
		public int DOcnt ;
		public int AIcnt ;
		public int AOcnt;
		public int ShemaLink1 ;
		public string ShemaLink2;
        public string Description1;
        public string Description2;
		public int IOCount => GetIOCount();

        public void SetIO(IOType iOType, int count)
		{
			switch (iOType)
			{
				case IOType.DO:
					DO = true;
					DOcnt = count;
					break;
				case IOType.DI:
					DI = true;
					DIcnt = count;
					break;
				case IOType.AO:
					AO = true;
					AOcnt = count;
					break;
				case IOType.AI:
					AI = true;
					AIcnt = count;
					break;
			}

		}
		public void AddIO(IOType iOType, int count)
		{
			switch (iOType)
			{
				case IOType.DO:
					DO = true;
					DOcnt += count;
					break;
				case IOType.DI:
					DI = true;
					DIcnt += count;
					break;
				case IOType.AO:
					AO = true;
					AOcnt += count;
					break;
				case IOType.AI:
					AI = true;
					AIcnt += count;
					break;
			}

		}
		public void AddIO(IOType iOType)
		{
			switch (iOType)
			{
				case IOType.DO:
					DO = true;
					DOcnt++;
					break;
				case IOType.DI:
					DI = true;
					DIcnt++;
					break;
				case IOType.AO:
					AO = true;
					AOcnt++;
					break;
				case IOType.AI:
					AI = true;
					AIcnt++;
					break;
			}

		}
		public void ReSetIO(IOType iOType)
		{
			switch (iOType)
			{
				case IOType.DO:
					DO = false;
					DOcnt = 0;
					break;
				case IOType.DI:
					DI = false;
					DIcnt = 0;
					break;
				case IOType.AO:
					AO = false;
					AOcnt = 0;
					break;
				case IOType.AI:
					AI = false;
					AIcnt = 0;
					break;
			}



		}
		public void ReSetAllIO()
		{
			DO = false;
			DI = false;
			AO = false;
			AI = false;
			DOcnt = 0;
			DIcnt = 0;
			AOcnt = 0;
			AIcnt = 0;
		}
		public void MultiplyIO(IOType iOType, int factor)
		{

			switch (iOType)
			{

				case IOType.DO:
					DI = true;
					DOcnt = Multiply(DOcnt, factor);

					break;
				case IOType.DI:
					DI = true;
					DIcnt = Multiply(DIcnt, factor);
					break;
				case IOType.AO:
					AO = true;
					AOcnt = Multiply(AOcnt, factor);
					break;
				case IOType.AI:
					AI = true;
					AIcnt = Multiply(AIcnt, factor);
					break;
			}

			int Multiply(int x, int fact)
			{
				return (x == 1) ? fact : x * fact;
			}

		}
		public Dictionary<string, int> GetIO()
		{
			Dictionary<string, int> IO = new Dictionary<string, int>
			{
				{ "DI", DIcnt },
				{ "DO", DOcnt },
				{ "AI", AIcnt },
				{ "AO", AOcnt }
			};
			return IO;

		}
		public static ShemaASU CreateBaseShema()
		{
			ShemaASU shema = new ShemaASU();
			shema.ReSetAllIO();
			
			return shema;

		}
		private int GetIOCount()
        {
			return DIcnt + DOcnt + AIcnt + AOcnt;
			
        }
		public static ShemaASU CreateBaseShema(VentComponents ventComponents)
		{
            ShemaASU shema = new ShemaASU
            {
                ScheUpSize = DummySize(ventComponents),
                ShemaPos = DummyPos(ventComponents)
            };
            shema.ReSetAllIO();
			
			return shema;

		}
		
	}

    internal class CrossSection
    {
		internal SensorT _SensorT;
		internal Humidifier.HumiditySens _SensorH;
		private protected Sensor.SensorType sensorTypeT;
		private protected Sensor.SensorType sensorTypeH;
		private protected string blockNameT;
		private protected string blockNameH;
		internal ShemaASU ShemaASU;
		internal virtual void SetBlockName(string image)
        {
			ShemaASU = ShemaASU.CreateBaseShema();
			switch (image)
            {
				
				case "cross1T":
				case "cross1TH":
				case "cross1":
					ShemaASU.ShemaUp = "CrossSection";
					break;
				case "cross2":
					ShemaASU.ShemaUp = "CrossSection2";
					break;
				case "cross3":
					ShemaASU.ShemaUp = "CrossSection3";
					break;
				case "cross4":
					ShemaASU.ShemaUp = "CrossSection4";
					break;
				case "cross5":
					ShemaASU.ShemaUp = "CrossSection5";
					break;
				case "cross6":
					ShemaASU.ShemaUp = "CrossSection6";
					break;
				case "cross7":
					ShemaASU.ShemaUp = "CrossSection7";
					break;
				case "cross8":
					ShemaASU.ShemaUp = "CrossSection8";
					break;
				case "cross9":
					ShemaASU.ShemaUp = "CrossSection9";
					break;
				case "cross10":
					ShemaASU.ShemaUp = "CrossSection10";
					break;
				case "cross11":
					ShemaASU.ShemaUp = "CrossSection11";
					break;

			}
			


		}
		internal virtual void SetBlockName()
        {
			ShemaASU = ShemaASU.CreateBaseShema();
			ShemaASU.ShemaUp = "CrossSection";
		}
		public CrossSection(bool sensT, bool sensH)
        {
			
			if (sensT)
			{
				_SensorT = new SensorT
				{
					Description = "Датчик температуры в канале",
					_SensorType = Sensor.SensorType.Analogue
				};
				this.sensorTType = _SensorT._SensorType;
			}
			if (sensH)
			{
				_SensorH = new Humidifier.HumiditySens
				{
					Description = "Датчик влажности в канале",
					_SensorType = Sensor.SensorType.Analogue
				};
				this.sensorHType = _SensorH._SensorType;
				_SensorH.SensorInsideCrossSection =true;

			}
			SetBlockName();
		}
		public CrossSection(bool sensT, bool sensH, string imagename)
		{

			if (sensT)
			{
				_SensorT = new SensorT
				{
					Description = "Датчик температуры в канале",
					_SensorType = Sensor.SensorType.Analogue
				};
				this.sensorTType = _SensorT._SensorType;
			}
			if (sensH)
			{
				_SensorH = new Humidifier.HumiditySens
				{
					Description = "Датчик влажности в канале",
					_SensorType = Sensor.SensorType.Analogue
				};
				this.sensorHType = _SensorH._SensorType;
				_SensorH.SensorInsideCrossSection = true;

			}
			SetBlockName(imagename);

		}
		[DisplayName("Тип датчика температуры")]
		public Sensor.SensorType sensorTType { get => sensorTypeT;
            set => SetSensorType(_SensorT, value);
        }
		[DisplayName("Тип датчика влажности")]
		public Sensor.SensorType sensorHType { get => sensorTypeH;
            set => SetSensorType(_SensorH, value);
        }
		[DisplayName("Имя блока датчика температуры")]
		public string SensTBlockName => blockNameT;

        [DisplayName("Имя блока датчика влажности")]
		public string SensHBlockName => blockNameH;

        internal string GUID { get; set; }

        private void SetSensorType(dynamic Sens, Sensor.SensorType val) 
        {
			Sens._SensorType = val;
			switch (Sens.GetType().Name)
            {
				case nameof(SensorT):
					sensorTypeT = val;
					blockNameT = Sens.Blockname;
					
					break;
				case nameof(Humidifier.HumiditySens):
					
					sensorTypeH = val;
					_SensorH.SensorInsideCrossSection =true;
					blockNameH = Sens.Blockname;
					
					break;	
			}
		}
		
	}
	
	class Room : CrossSection
	{
		//private static Room instance;
		internal override void SetBlockName()
		{
			ShemaASU = ShemaASU.CreateBaseShema();
			ShemaASU.ShemaUp = "Room_Supply";
		}
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
					blockname= "Room_Supply";
					break;
				case "room__arrow_exhaust_L":
					blockname = "Room_Exthaust";
					break;
				case "room__arrow_exhaust_R":
					blockname = "Room_Exthaust_R";
					break;
				//case "room__arrow_supply_T":
				//	break;
				//case "room__arrow_supp_exh_TH":
				//	break;
				//case "room__arrow_supp_exh_TH_big":
				//	break;
				//case "room__arrow_supply_TH":
				//	break;

				default:
					blockname = "Room_Supply";
					break;

			}
			ShemaASU.ShemaUp = blockname;


		}
		public Room(bool sensT, bool sensH) : base(sensT, sensH)
		{
			if (sensT)
			{
                _SensorT = new IndoorTemp
                {
					Description = "Датчик температуры в помещении",
					_SensorType = Sensor.SensorType.Analogue,
					Location = "Indoor"
					
                };
				sensorTypeT = _SensorT._SensorType;
				blockNameT = _SensorT.Blockname;
            }
			if (sensH)
			{
				_SensorH = new Humidifier.HumiditySens
				{
					Description = "Датчик влажности в помещении",
					_SensorType = Sensor.SensorType.Analogue,
					Location = "Indoor"
				};
				sensorTypeH = _SensorH._SensorType;
				blockNameH = _SensorH.Blockname;
			}
			SetBlockName();
			
		}
		public Room(bool sensT, bool sensH, string image) : base(sensT, sensH)
		{
			if (sensT)
			{
				_SensorT = new IndoorTemp
				{
					Description = "Датчик температуры в помещении",
					_SensorType = Sensor.SensorType.Analogue,
					Location = "Indoor"

				};
				sensorTypeT = _SensorT._SensorType;
				blockNameT = _SensorT.Blockname;
			}
			if (sensH)
			{
				_SensorH = new Humidifier.HumiditySens
				{
					Description = "Датчик влажности в помещении",
					_SensorType = Sensor.SensorType.Analogue,
					Location = "Indoor"
				};
				sensorTypeH = _SensorH._SensorType;
				blockNameH = _SensorH.Blockname;
			}
			SetBlockName(image);
		}

	}
	public static class PropertyDescriptorExtensions
	{
		public static void SetReadOnlyAttribute(this PropertyDescriptor p, bool value)
		{
			var attributes = p.Attributes.Cast<Attribute>()
				.Where(x => !(x is ReadOnlyAttribute)).ToList();
			attributes.Add(new ReadOnlyAttribute(value));
			typeof(MemberDescriptor).GetProperty("AttributeArray",
				BindingFlags.Instance | BindingFlags.NonPublic)
				.SetValue((MemberDescriptor)p, attributes.ToArray());
		}
	}
	interface ISchemaASU
    {
		double ScheUpSize { get; set; }
		int ShemaPos { get; set; }
		string ShemaUp { get; set; }

	}
	interface IPower
    {
		string Power { get; set; }
		 ElectroDevice._Voltage Voltage { get; set; }
	}
	interface ICompGUID
    {
		 string GUID { get; set; }
	}
	interface IVendoInfo
    {
		(string VendorName, string ID, string VendorDescription, string DBTable, string Assignment, string DefaultDescription, string MainDBTable) GetVendorInfo();
		void SetVendorInfo(string vendorName, string ID, string vendorDescription, string dbTable, string assignment);
		void ClearVendorInfo();		
	}
	interface IGetSensors
    {
		List<dynamic> GetSensors();
    }

	class Singleton
	{
        
		
		private static Singleton instance;

		private Singleton()
		{}

		public static Singleton getInstance()
        {
            
			
            if (instance == null)
				instance = new Singleton();
			return instance;
		}     
		
	}
    
}
