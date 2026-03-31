using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;

namespace XMLParser.DocumentPipeline.Steps
{
    public class ParseXMLStep : IStep
    {
        public void Execute(PiplineContext context)
        {
            bool isNumberingExists = File.Exists(Path.Combine(context.TempDocumentDirectory, "word", PiplineContext.NUMBERING));

            var (docRoot, docSpecialTokens) = XMLRead.ReadXMLDocument(PiplineContext.DOCUMENT, context.TempDocumentDirectory);
            context.DocumentRoot = docRoot;
            context.DocumentSpecialTokens = docSpecialTokens;

            var (styleRoot, styleSpecialTokens) = XMLRead.ReadXMLDocument(PiplineContext.STYLES, context.TempDocumentDirectory);
            context.StylesRoot = styleRoot;
            context.StyleSpecialTokens = styleSpecialTokens;

            if (isNumberingExists)
            {
                var (numberingRoot, numberingSpecialTokens) = XMLRead.ReadXMLDocument(PiplineContext.NUMBERING, context.TempDocumentDirectory);
                context.NumberingRoot = numberingRoot;
                context.NumberingSpecialTokens = numberingSpecialTokens;
            }
            else
            {
                context.NumberingRoot = new TreeNode();
            }
        }
    }
}
