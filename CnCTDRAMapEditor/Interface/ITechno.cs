using MobiusEditor.Model;

namespace MobiusEditor.Interface
{
    public interface ITechno
    {
        HouseType House { get; set; }

        int Strength { get; set; }

        string Trigger { get; set; }
    }
}
