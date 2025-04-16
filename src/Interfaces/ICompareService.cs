using System.Windows.Controls;
using CudaHelioCommanderLight.Enums;

namespace CudaHelioCommanderLight.Interfaces
{
    public interface ICompareService
    {
        (string?, LibStructureType) CompareWithLib(string tag, ComboBox geliosphereLibRatio, ComboBox geliosphereLibType);
    }
}