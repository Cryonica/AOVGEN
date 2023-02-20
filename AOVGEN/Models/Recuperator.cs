using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace AOVGEN.Models
{
    [Serializable]
	class Recuperator : IEnumerable, ICompGUID, IGetSensors
    {
		#region Pictures Path
		public static string ImageNullPath = VentSystem.AppFolder + "Images\\9_0.png";
		public string Imagepath;
		#endregion
		#region Enums
		[TypeConverter(typeof(EnumDescConverter))]
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
		[TypeConverter(typeof(EnumDescConverter))]
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
		internal PressureContol protectSensor2
		{
			get => _ProtectSensor2;
			set => _ProtectSensor2 = value;
		}
		[DisplayName("Тип элемента")]
		public VentComponents comp
		{
			get => ComponentVariable;
			private set => ComponentVariable = value;
		}
		[DisplayName("Тип рекуператора")]
		public RecuperatorType _RecuperatorType
		{
			get => recuperatortype;
			set => SetRecuperatorType(value, ref Imagepath, ref _ProtectSensor1, ref _ProtectSensor2, ref _Drive1, ref _Drive2, ref _Drive3, ref recirculationtype);
		}
		[DisplayName("Количество приводов")]
		public RecirculationType _RecirculationType
		{
			get => recirculationtype;
			internal set => recirculationtype = value;
		}
		[DisplayName("Имя блока привода 1")]
		public string Drive1BlockName
		{
			get => GetDriveBlockName(_Drive1);
			protected set => Dev1BlockName = value;
		}
		internal string Drive1PosName => GetDrivePosName(_Drive1);

		[DisplayName("Имя блока привода 2")]
		public string Drive2BlockName
		{
			get => GetDriveBlockName(_Drive2);
			protected set => Dev2BlockName = value;
		}
		internal string Drive2PosName => GetDrivePosName(_Drive2);

		[DisplayName("Имя блока привода 3")]
		public string Drive3BlockName
		{
			get => GetDriveBlockName(_Drive3);
			protected set => Dev3BlockName = value;
		}
		internal string Drive3PosName => GetDrivePosName(_Drive3);

		[DisplayName("Обозначение датчика защиты 1")]
		public string Sens1PosName => GetSensorPos(_ProtectSensor1);

		[DisplayName("Имя блока датчика защиты 1")]
		public string PS1BlockName => GetSensorName(_ProtectSensor1);

		[DisplayName("Обозначение датчика защиты 2")]
		public string Sens2PosName => GetSensorPos(_ProtectSensor2);

		[DisplayName("Имя блока датчика защиты 2")]
		public string PS2BlockName => GetSensorName(_ProtectSensor2);
		
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
						SortPriority = "18",
						ShemaASU = ShemaASU.CreateBaseShema(VentComponents.Recuperator)

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
					drive1 = new Drive
					{
						Description = "Э/д привода заслонки байпаса рекуператора",
						SortPriority = "18",
						ShemaASU = ShemaASU.CreateBaseShema(VentComponents.Recuperator)
					};
					//drive1.ShemaASU 
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
						SortPriority = "18",
						ShemaASU = ShemaASU.CreateBaseShema(VentComponents.Recuperator)
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

					drive2 = new Drive
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
				_ProtectSensor1,
				_ProtectSensor2
			};
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
}
