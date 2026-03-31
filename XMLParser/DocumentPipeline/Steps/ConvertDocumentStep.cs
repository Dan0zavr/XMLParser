using PDFReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser.DocumentPipeline.Steps
{
    public class ConvertDocumentStep : IStep
    {
        public void Execute(PiplineContext context)
        {
            context.TempPdfDirectory = CreateTempPath();

            if (context.TempPdfPath == null) 
            {
                context.TempPdfPath = PDFReaderEntry.Convert(context.InputPath, context.TempPdfDirectory, Priority.Word);
            }
        }

        private static string CreateTempPath()
        {
            string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);
            return tempFolder;
        }
    }
}
