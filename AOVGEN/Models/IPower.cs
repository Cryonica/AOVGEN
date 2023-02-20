namespace AOVGEN.Models
{
    public interface IPower
    {
        string Power { get; set; }
        ElectroDevice._Voltage Voltage { get; set; }
    }
}
