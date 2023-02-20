using System;
using static AOVGEN.Models.Recuperator;


namespace AOVGEN.Models
{
    [Serializable]
    class Drive : Valve, IPower
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
    }
}
