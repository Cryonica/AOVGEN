using System.ComponentModel;

namespace AOVGEN.Models
{
    public interface ICompGUID
    {
        [Browsable(false)]
        string GUID { get; set; }
    }
}
