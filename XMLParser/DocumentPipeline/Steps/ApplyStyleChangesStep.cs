using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.SpecialClasses.InputOutput;

namespace XMLParser.DocumentPipeline.Steps
{
    public class ApplyStyleChangesStep : IStep
    {
        public void Execute(PiplineContext context)
        {
            XMLWrite.TreeToXMLDocument(context.StylesRoot, context.StyleSpecialTokens, PiplineContext.STYLES, Path.Combine(context.TempDocumentDirectory, "word"));
        }
    }
}
