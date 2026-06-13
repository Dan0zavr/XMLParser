using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Graphics.Operations.PathPainting;
using XMLParser.SpecialClasses.InputOutput;
using XMLParser.Styles;

namespace XMLParser.DocumentPipeline.Steps
{
    public class ZipStep : IStep
    {
        public void Execute(PiplineContext context)
        {
            context.OutputFile = XMLWrite.FilesInZip(context.TempDocumentDirectory, Path.GetFileName(context.InputPath), context.OutputPath);
        }
    }
}
