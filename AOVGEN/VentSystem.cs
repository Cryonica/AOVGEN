using AOVGEN.Models;
using AOVGEN.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace AOVGEN
{
#pragma warning disable IDE1006
   
    [TypeConverter(typeof(EnumDescConverter))]
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
	[TypeConverter(typeof(EnumDescConverter))]
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
    internal class VentSystem : IEnumerable,  ICloneable 
	{
        private static string Appfolder()
		{
			return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Resources.PluginFolder; //@"\Autodesk\Revit\Addins\2022\ASU\AOVGen\";

		}
		[DisplayName("Имя системы")]
		public string SystemName { get; internal set; }
		public string GUID { get; set; }
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
			return MemberwiseClone();
        }
		public List<dynamic> GetKIP()
        {
			
			List<dynamic> ListKIP = new();
            
		

            foreach (var posInfo in ComponentsV2)
            {
                try
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
							if (supplyVent._FControl != null)
                            {
								//ListKIP.Add(supplyVent._FControl);
							}
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
							if (extVent._FControl != null)
							{
								//ListKIP.Add(extVent._FControl);
							}
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
				catch
                {
					var t = posInfo;

				}
				
			}
            return ListKIP;

		}
		public VentSystem()
        {
			ComponentsV2 = new List<EditorV2.PosInfo>();
        }
		private ShemaASU[] _GetShemasFromKip()
        {
			return GetKIP()
				.Cast<Sensor>()
				.Select(s => s.ShemaASU)
				.ToArray();
		}
		[Serializable]
		private struct SignificantInfo2
		{
			internal string PosName;
			internal int PosX;
			internal int PosY;
		}
		[Serializable]
		private struct SignificantInfo3
		{
			internal List<SignificantInfo2> significantInfo2s1;
			internal List<string[]> shemas;
			internal List<FControl> fControls;


		}
        public new string GetHashCode()
        {
			List<SignificantInfo2> significantInfo2s = new();
			significantInfo2s.AddRange(from EditorV2.PosInfo posInfo1 in ComponentsV2
									   let p = new SignificantInfo2
									   {
										   PosName = posInfo1.ImageName,
										   PosX = posInfo1.PozX,
										   PosY = posInfo1.PozY
									   }
									   select p);

			//Get sensor shemas 
			var _shemas = GetKIP()
				.Cast<Sensor>()
				.Select(s => s.ShemaASU)
				.ToArray();

			List<FControl> fControls = new();

			var vents = ComponentsV2
				.Select(c => c.Tag)
				.OfType<Vent>() // Используйте метод OfType<T> для фильтрации и приведения типа одновременно
				.ToList();

			if (vents !=null)
            {
				foreach (Vent vent in vents)
                {
					if (vent._FControl != null) fControls.Add(vent._FControl);

				}
            }


			//Create list of meaningful information from sensor shemas
			List<string[]> shemas = new();
			shemas.AddRange(from ShemaASU shema in _shemas
							   let p1 = new string[]
							   {
								 shema.ShemaUp,
								 shema.componentPlace.ToString()
							   }
							   select p1);

			//Create summary struct for genetation MD5Hash
			SignificantInfo3 significantInfo3 = new()
			{
				significantInfo2s1 = significantInfo2s,
				shemas = shemas,
				fControls = fControls
			};

			return EditorV2.MD5HashGenerator.GenerateKey(significantInfo3);
		}
    }

}
