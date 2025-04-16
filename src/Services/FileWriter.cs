using CudaHelioCommanderLight.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CudaHelioCommanderLight.Services
{
    public class FileWriter : IFileWriter
    {
        public void WriteToFile(string path, string content)
        {
            using (StreamWriter file = File.CreateText(path))
            {
                file.Write(content);
            }
        }
    }
}

