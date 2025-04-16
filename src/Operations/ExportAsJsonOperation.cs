using CudaHelioCommanderLight.Dtos;
using CudaHelioCommanderLight.Models;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace CudaHelioCommanderLight.Operations
{
    public class ExportAsJsonOperation : Operation<ExecutionListExportModel, object>
    {
        public static new void Operate(ExecutionListExportModel executionModel)
        {
            try
            {
                List<ExecutionDto> executionDTOs = new List<ExecutionDto>();

                foreach (Execution execution in executionModel.Executions)
                {
                    executionDTOs.Add(new ExecutionDto()
                    {
                        K0 = execution.K0,
                        dt = execution.dt,
                        V = execution.V,
                        N = execution.N,
                        Percentage = execution.Percentage,
                        Error = execution.Error,
                        Method = execution.MethodType.ToString()
                    });
                }

                using (StreamWriter file = File.CreateText(executionModel.FilePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(file, executionDTOs);
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new IOException("Directory not found while exporting JSON.", ex);
            }
        }
    }
}
