using NUnit.Framework;
using NSubstitute;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using CudaHelioCommanderLight.Operations;
using CudaHelioCommanderLight.Dtos;
using CudaHelioCommanderLight.Models;
using CudaHelioCommanderLight.Enums; // Ensure you include this

[TestFixture]
public class ExportAsJsonOperationTests
{
    private ExecutionListExportModel _sampleExecutionModel;
    private string _testFilePath;

    [SetUp]
    public void SetUp()
    {
        _testFilePath = "test.json";


        _sampleExecutionModel = new ExecutionListExportModel
        {
            FilePath = _testFilePath,
            Executions = new List<Execution>
            {
                new Execution(2.0, 1.0, 10, 0.1, MethodType.FP_1D),
            }
        };
    }

    [Test]
    public void Operate_ShouldCreateJsonFileWithExpectedContent()
    {
        // Act
        ExportAsJsonOperation.Operate(_sampleExecutionModel);

        // Assert
        Assert.That(File.Exists(_testFilePath), Is.True, "JSON file was not created.");

        string jsonContent = File.ReadAllText(_testFilePath);
        List<ExecutionDto> deserializedData = JsonConvert.DeserializeObject<List<ExecutionDto>>(jsonContent);

        Assert.That(deserializedData, Is.Not.Null);
        Assert.That(deserializedData.Count, Is.EqualTo(_sampleExecutionModel.Executions.Count));
        Assert.That(deserializedData[0].K0, Is.EqualTo(_sampleExecutionModel.Executions[0].K0));
        Assert.That(deserializedData[0].Method, Is.EqualTo(nameof(MethodType.FP_1D)));

        File.Delete(_testFilePath);
    }

    [Test]
    public void Operate_WhenIOExceptionOccurs_ShouldThrowException()
    {
        // Arrange
        var executionModel = new ExecutionListExportModel
        {
            FilePath = @"C:\invalid\path\test.json", 
            Executions = new List<Execution>
        {
            new Execution(1.0, 2.0, 3.0, 4.0, MethodType.FP_1D)
        }
        };

        // Act & Assert
        var ex = Assert.Throws<IOException>(() => ExportAsJsonOperation.Operate(executionModel));
        Assert.That(ex, Is.InstanceOf<IOException>());
    }
}
