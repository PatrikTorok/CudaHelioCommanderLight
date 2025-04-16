using CudaHelioCommanderLight.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CudaHelioCommanderLight.Interfaces
{
    public interface IButtonService
    {
        void AboutUsButton();
        void ExportJsonBtn(ObservableCollection<ExecutionDetail> executionDetailList, int executionDetailSelectedIdx);
    }
}
