using System;
using System.Collections.Generic;

namespace AOVGEN.Models
{
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
		public int DIcnt;
		public int DOcnt;
		public int AIcnt;
		public int AOcnt;
		public int ShemaLink1;
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
			Dictionary<string, int> IO = new()
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
			ShemaASU shema = new();
			shema.ReSetAllIO();

			return shema;

		}
		private int GetIOCount()
		{
			return DIcnt + DOcnt + AIcnt + AOcnt;

		}
		public static ShemaASU CreateBaseShema(VentComponents ventComponents)
		{
			ShemaASU shema = new()
            {
				ScheUpSize = DummySize(ventComponents),
				ShemaPos = DummyPos(ventComponents)
			};
			shema.ReSetAllIO();

			return shema;

		}
	}
}
