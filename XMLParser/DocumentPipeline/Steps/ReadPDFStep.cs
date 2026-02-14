using PDFReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Graphics.Operations.PathPainting;
using XMLParser.Styles;

namespace XMLParser.DocumentPipeline.Steps
{
    public class ReadPDFStep : IStep
    {
        public void Execute(PiplineContext context)
        {
            context.PagesWords = PDFReaderEntry.ReadPDF(context.InputPath, context.TempPath, context.IgnorePages, Priority.Word);
        }
    }
}
