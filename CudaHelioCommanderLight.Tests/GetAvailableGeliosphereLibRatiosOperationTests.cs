using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CudaHelioCommanderLight.Operations;
using NUnit.Framework;

namespace CudaHelioCommanderLight.Tests
{
    [TestFixture]
    public class GetAvailableGeliosphereLibRatiosOperationTests
    {
        private string _tempDir;
        private string _originalCurrentDir;

        [SetUp]
        public void Setup()
        {
            _originalCurrentDir = Environment.CurrentDirectory;
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
            Environment.CurrentDirectory = _tempDir;
        }

        [TearDown]
        public void TearDown()
        {
            Environment.CurrentDirectory = _originalCurrentDir;
            Directory.Delete(_tempDir, recursive: true);
        }

        [Test]
        public void Operate_WhenLibFilesDoesNotExist_CreatesDirectoryAndReturnsEmptyList()
        {
            // Act
            var result = GetAvailableGeliosphereLibRatiosOperation.Operate("proton");

            // Assert
            var libFilesPath = Path.Combine(_tempDir, "libFiles");
            Assert.Multiple(() =>
            {
                Assert.That(Directory.Exists(libFilesPath), Is.True);
                Assert.That(result, Is.Empty);
            });
        }

        [Test]
        public void Operate_WhenNoMatchingDirectories_ReturnsEmptyList()
        {
            // Arrange
            var libFilesPath = CreateLibFilesDirectory();
            Directory.CreateDirectory(Path.Combine(libFilesPath, "electron-100"));

            // Act
            var result = GetAvailableGeliosphereLibRatiosOperation.Operate("proton");

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Operate_WhenMatchingDirectoriesExist_ReturnsCorrectRatios()
        {
            // Arrange
            var libFilesPath = CreateLibFilesDirectory();
            var expectedRatios = new List<string> { "100", "200", "test", "456" };
            CreateTestDirectories(libFilesPath, new[]
            {
                "proton-100",
                "lib-proton-200",
                "proton-test",
                "proton-123-456",
                "invalid_format"
            });

            // Act
            var result = GetAvailableGeliosphereLibRatiosOperation.Operate("proton");

            // Assert
            Assert.That(result, Is.EquivalentTo(expectedRatios));
        }

        [Test]
        public void Operate_WithCaseSensitiveSearch_ReturnsOnlyExactMatches()
        {
            // Arrange
            var libFilesPath = CreateLibFilesDirectory();
            CreateTestDirectories(libFilesPath, new[]
            {
                "Proton-100",
                "PROTON-200",
                "proton-300"
            });

            // Act
            var result = GetAvailableGeliosphereLibRatiosOperation.Operate("proton");

            // Assert
            Assert.That(result, Is.EquivalentTo(new[] { "300" }));
        }

        private string CreateLibFilesDirectory()
        {
            var path = Path.Combine(_tempDir, "libFiles");
            Directory.CreateDirectory(path);
            return path;
        }

        private void CreateTestDirectories(string basePath, IEnumerable<string> directories)
        {
            foreach (var dir in directories)
            {
                Directory.CreateDirectory(Path.Combine(basePath, dir));
            }
        }
    }
}
