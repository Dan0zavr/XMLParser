using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Graphics.Operations.PathPainting;
using XMLParser.Styles;

namespace XMLParser.DocumentPipeline.Steps
{
    public class ZipStep : IStep
    {
        public void Execute(PiplineContext context)
        {
            XMLWrite.FilesInZip(context.TempPath, Path.GetFileName(context.InputPath), context.OutputPath);
        }
    }
}
