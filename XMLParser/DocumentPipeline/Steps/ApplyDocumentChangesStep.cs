using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;

namespace XMLParser.DocumentPipeline.Steps
{
    public class ApplyDocumentChangesStep : IStep
    {
        public void Execute(PiplineContext context)
        {
            XMLWrite.TreeToXMLDocument(context.DocumentRoot, context.DocumentSpecialTokens, PiplineContext.DOCUMENT, context.TempPath);
        }
    }
}
