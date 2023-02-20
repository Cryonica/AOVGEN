using System;
using System.ComponentModel;

namespace AOVGEN.Models
{
	[Serializable]
	abstract class Damper: ElectroDevice, ICompGUID
    {
		internal ShemaASU ShemaASU;
		[DisplayName("Обозначение")]
		public string PosName { get; internal set; }
		[DisplayName("Имя блока")]
		public string BlockName => MakeDamperBlockName();
		internal abstract string MakeDamperBlockName();
		internal string Description1 { get; set; }
		internal string Description2 { get; set; }
		protected bool hascontrol;
		public string GUID { get; set; }
		internal Cable Cable2;
		protected virtual void SetCable2Type(object val)
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
}
