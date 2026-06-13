using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.SpecialClasses.Tree;
using XMLParser.Styles;

namespace XMLParser.DocumentPipeline
{
    public class PiplineContext
    {
        public const string DOCUMENT = "document.xml";
        public const string NUMBERING = "numbering.xml";
        public const string STYLES = "styles.xml";

        public string InputPath;
        public string? TempDocumentDirectory;
        public string OutputPath;
        public string? OutputFile;
        public string? TempPdfDirectory;
        public string? TempPdfPath;
        public List<string> DocumentSpecialTokens;
        public TreeNode DocumentRoot;
        public List<string> StyleSpecialTokens;
        public TreeNode StylesRoot;
        public List<string>? NumberingSpecialTokens;
        public TreeNode? NumberingRoot;
        public Template Template;
        public int[]? IgnorePages;
        public Dictionary<int, List<string>> PagesWords;
        public Dictionary<int, List<string>> ForColontitulPagesWords;
        public Stash Stash;
        public Dictionary<StyleCategory, TreeNode> Styles;
    }
}
