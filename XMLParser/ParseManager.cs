using XMLParser.ApplyStrategies;
using XMLParser.Builders;
using XMLParser.Styles;
using static XMLParser.TreeNode;
using static XMLParser.XMLRead;
using static XMLParser.XMLWrite;
using static XMLParser.Cleaner;
using static XMLParser.Tokenizator;
using static XMLParser.DocumentComposer;
using PDFReader;

namespace XMLParser
{
    public class ParseManager
    {
        private const string document = "document.xml";
        private const string numbering = "numbering.xml";
        private const string styles = "styles.xml";

        public string MainScript(string readPath, string savePath, Template template, bool splitDocument = false, int[] pages = null)
        {
            string tempPath = CreateTempPath();
            try
            {
                UnZipDocx(readPath, tempPath);
                bool isNumberingExists = File.Exists(Path.Combine(tempPath, numbering));
                var (docRoot, docSpecialTokens) = ReadXMLDocument(document, tempPath);
                var (styleRoot, styleSpecialTokens) = ReadXMLDocument(styles, tempPath);

                //Получаем текст со страниц

                Dictionary<int, List<string>> pagesWords = PDFReaderEntry.ReadPDF(readPath, tempPath, pages, Priority.Word);

                // Игнорируем абзацы
                Stash stash = new Stash(docRoot.LongBreadthFirstSearch("w:body").First());
                stash.StashPages(pagesWords);

                TreeNode numberingRoot = new TreeNode();
                List<string> numberingSpecialTokens = new List<string>();

                if (isNumberingExists) // заменить на цепочку зависимостей, т.к. такой подход плохой
                {
                    var (numberingRoot1, numberingSpecialTokens1) = ReadXMLDocument(numbering, tempPath);
                    numberingRoot = numberingRoot1;
                    numberingSpecialTokens = numberingSpecialTokens1;
                    template.NumberingStyle = null;
                }

                BuildStyleDirector buildDirector = new BuildStyleDirector(styleRoot, numberingRoot, new StylesUniquelizer(styleRoot));
                // для styles.xml  для numbering.xml
                (var inStyles, var inNumbering) = buildDirector.BuildAllStyles(template.GetStyles());
                Dictionary<StyleCategory, TreeNode> allStyles = inStyles.Union(inNumbering).ToDictionary(x => x.Key, y => y.Value);

                XMLParser.StyleIntegrator.IntegrateStylesToTree(styleRoot, inStyles.Values.ToList());
                TreeNode paragraphStyle = inStyles[StyleCategory.ParagraphStyle];
                if (isNumberingExists) 
                {
                    XMLParser.StyleIntegrator.IntegrateNumberingStylesToTree(docRoot, numberingRoot, inNumbering.Values.ToList(), paragraphStyle);
                }

                TreeToXMLDocument(styleRoot, styleSpecialTokens, styles, tempPath);

                //Начало применения стилей
                
                var (titlePage, content, mainTag) = SplitDocument(docRoot, splitDocument);
                TreeNode contentBody = content.Children[0];

                if (template.PictureStyle != null)
                {
                    Dictionary<int, TreeNode> paragraphsWithPictures = ExtractPicturesFromParagraphToDictionary(contentBody);

                    if (paragraphsWithPictures != null)
                    {
                        ReconstructParagraphs(contentBody, SeparateDrawingsAndText(paragraphsWithPictures));
                        Dictionary<int, List<TreeNode>> paragraphsWithCaptions =  CaptionAdder.AddCaption(contentBody, template.PictureStyle); //Оптимизировать так, чтобы реконструировать абзацы только 1 раз
                        ReconstructParagraphs(contentBody, paragraphsWithCaptions);
                    }
                }

                CleanHandStyles(content, template, docSpecialTokens, savePath);

                // применение стилей
                ApplyContext applyContext = new ApplyContext(numberingRoot);
                foreach (var strategy in allStyles)
                {
                    applyContext.SetStrategy(strategy.Key);
                    applyContext.ApplyStyle(docRoot, strategy.Value);
                }

                stash.UnStash();

                TreeNode endRoot = MergeDocument(titlePage, content, mainTag);

                TreeToXMLDocument(endRoot, docSpecialTokens, document, tempPath);

                if (isNumberingExists) 
                {
                    TreeToXMLDocument(numberingRoot, numberingSpecialTokens, numbering, tempPath);
                }

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
            Directory.CreateDirectory(tempFolder);
            return tempFolder;
        }
    }
}
