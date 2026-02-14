using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using XMLParser;

namespace XMLParser.DocumentPipeline.Steps
{
    public class UnzipStep : IStep
    {
        public void Execute(PiplineContext context) // возможно стоит разделить на 2 шага
        {
            string tempPath = CreateTempPath();
            context.TempPath = tempPath;

            XMLRead.UnZipDocx(context.InputPath, context.TempPath);
        }

        private string CreateTempPath()
        {
            string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);
            return tempFolder;
        }

    }
}
