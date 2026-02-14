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
            bool isNumberingExists = File.Exists(Path.Combine(context.TempPath, PiplineContext.NUMBERING));

            var (docRoot, docSpecialTokens) = XMLRead.ReadXMLDocument(PiplineContext.DOCUMENT, context.TempPath);
            context.DocumentRoot = docRoot;
            context.DocumentSpecialTokens = docSpecialTokens;

            var (styleRoot, styleSpecialTokens) = XMLRead.ReadXMLDocument(PiplineContext.STYLES, context.TempPath);
            context.StylesRoot = styleRoot;
            context.StyleSpecialTokens = styleSpecialTokens;

            if (isNumberingExists)
            {
                var (numberingRoot, numberingSpecialTokens) = XMLRead.ReadXMLDocument(PiplineContext.NUMBERING, context.TempPath);
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
