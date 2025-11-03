using XMLParser.ApplyStrategies;
using XMLParser.Builders;
using XMLParser.Styles;
using static XMLParser.TreeNode;
using static XMLParser.XMLRead;
using static XMLParser.XMLWrite;
using static XMLParser.Cleaner;
using static XMLParser.Tokenizator;
using static XMLParser.DocumentComposer;

namespace XMLParser
{
    public class ParseManager
    {
        private const string document = "document.xml";
        private const string numbering = "numbering.xml";
        private const string styles = "styles.xml";

        public string MainScript(string readPath, string savePath, Template template, bool splitDocument = false)
        {
            string tempPath = CreateTempPath();
            try
            {
                UnZipDocx(readPath, tempPath);
                var (styleRoot, styleSpecialTokens) = ReadXMLDocument(styles, tempPath);
                var (numberingRoot, numberingSpecialTokens) = ReadXMLDocument(numbering, tempPath);

                BuildStyleDirector buildDirector = new BuildStyleDirector(styleRoot, numberingRoot, new StylesUniquelizer(styleRoot));
                // для styles.xml  для numbering.xml
                (var inStyles, var inNumbering) = buildDirector.BuildAllStyles(template.GetStyles());
                Dictionary<StyleCategory, TreeNode> allStyles = inStyles.Union(inNumbering).ToDictionary(x => x.Key, y => y.Value);

                XMLParser.StyleIntegrator.IntegrateStylesToTree(styleRoot, inStyles.Values.ToList());
                XMLParser.StyleIntegrator.IntegrateStylesToTree(numberingRoot, inNumbering.Values.ToList());

                TreeToXMLDocument(styleRoot, styleSpecialTokens, styles, tempPath);

                //Применение стилей
                var (docRoot, docSpecialTokens) = ReadXMLDocument(document, tempPath);

                var (titlePage, content, mainTag) = SplitDocument(docRoot, splitDocument);

                if (template.PictureStyle != null)
                {
                    Dictionary<int, TreeNode> paragraphsWithPictures = ExtractPicturesFromParagraphToDictionary(content);

                    if (paragraphsWithPictures != null)
                    {
                        ReconstructParagraphs(content, SeparateDrawingsAndText(paragraphsWithPictures));
                    }
                }

                CleanHandStyles(content, template, docSpecialTokens, savePath);

                // применение стилей
                ApplyContext applyContext = new ApplyContext();
                foreach (var strategy in allStyles)
                {
                    applyContext.SetStrategy(strategy.Key);
                    applyContext.ApplyStyle(docRoot, strategy.Value);

                }

                TreeNode endRoot = MergeDocument(titlePage, content, mainTag);

                TreeToXMLDocument(endRoot, docSpecialTokens, document, tempPath);

                return FilesInZip(tempPath, Path.GetFileName(readPath), savePath);

            }
            finally
            {
                Directory.Delete(tempPath, true);
            }

        }

        private string CreateTempPath()
        {
            string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            return tempFolder;
        }
    }
}
