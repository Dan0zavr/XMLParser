using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.SpecialClasses.InputOutput;
using XMLParser.Styles;
using XMLParser.SpecialClasses.Tree;

namespace XMLParser.DocumentPipeline.Steps
{
    public class ParseXMLStep : IStep
    {
        public void Execute(PiplineContext context)
        {
            bool isNumberingExists = File.Exists(Path.Combine(context.TempDocumentDirectory, "word", PiplineContext.NUMBERING));

            var (docRoot, docSpecialTokens) = XMLRead.ReadXMLDocument(Path.Combine(context.TempDocumentDirectory, "word", PiplineContext.DOCUMENT));
            context.DocumentRoot = docRoot;
            context.DocumentSpecialTokens = docSpecialTokens;

            var (styleRoot, styleSpecialTokens) = XMLRead.ReadXMLDocument(Path.Combine(context.TempDocumentDirectory, "word", PiplineContext.STYLES));
            context.StylesRoot = styleRoot;
            context.StyleSpecialTokens = styleSpecialTokens;

            if (isNumberingExists)
            {
                var (numberingRoot, numberingSpecialTokens) = XMLRead.ReadXMLDocument(Path.Combine( context.TempDocumentDirectory, "word", PiplineContext.NUMBERING));
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
