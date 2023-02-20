using System;

namespace AOVGEN.Models
{
    [Serializable]
	class Pump : Damper, IPower
    {
		private string _blockname;
		internal bool HasTK
		{
			get => hascontrol;
			set => PumpSetTK(value, this);
		}
		internal override string MakeDamperBlockName()
		{
			if (HasTK)

			{
				_blockname = "ED_Pump_220_TK";
			}
			else
			{
				_blockname = "ED_Pump_220";
			}

			if (Cable1 != null)
			{
				Cable1.ToBlockName = _blockname;
			}
			if (Cable2 != null)
			{
				Cable2.ToPosName = PosName;
				Cable2.ToBlockName = _blockname;
			}
			return _blockname;
		}
		internal string SortPriority => MakeSortPriority();
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
				ShemaASU.SetIO(ShemaASU.IOType.DI, 1);



				Cable2.SortPriority = pump.SortPriority + "1";


			}
			else
			{
				Cable2 = null;
				ShemaASU.ReSetIO(ShemaASU.IOType.DI);
			}
		}
		public Pump()
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
			Cable1.SortPriority = SortPriority;
			ShemaASU = ShemaASU.CreateBaseShema(VentComponents.WaterHeater);
			ShemaASU.ShemaUp = "Supply_Pump";
			ShemaASU.componentPlace = ShemaASUbase.ComponentPlace.Supply;
			ShemaASU.SetIO(ShemaASU.IOType.DO, 1);
			Power = "300";

		}
	}
}
