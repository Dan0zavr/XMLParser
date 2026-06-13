using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.SpecialClasses.InputOutput;

namespace XMLParser.DocumentPipeline.Steps
{
    public class ApplyNumberingChangesStep : IStep
    {
        public void Execute(PiplineContext context)
        {
            if (context.NumberingRoot.Children.Count > 0)
            {
                XMLWrite.TreeToXMLDocument(context.NumberingRoot, context.NumberingSpecialTokens, PiplineContext.NUMBERING, Path.Combine(context.TempDocumentDirectory, "word"));
            }
        }
    }
}
