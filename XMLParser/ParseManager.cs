using System.Xml;
using XMLParser.Styles;
using System.Collections.Generic;

namespace XMLParser
{
    public class ParseManager
    {
        private readonly TreeNode _root;
        private readonly XMLRead _xmlRead;
        private readonly StyleCreator _creator;

        private const string document = "document.xml";
        private const string styles = "styles.xml";
        private const string numbering = "numbering.xml";

        private readonly string tempReadPath = "C:\\Лабы\\AppTestDocx\\5 Лаба.docx";
        private readonly string tempSavePath = "C:\\Лабы\\AppTestDocx";
        private string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        private TextStyle textStyle = new TextStyle()
        {
            FontName = "Times New Roman",
            FontSize = 14
        };

        private ParagraphStyle paragraphStyle = new ParagraphStyle()
        {
            Alingnment = "both",
            IntervalInText = 360,
            FirstLineIndent = 1.25
        };

        private NumberingStyle numberingStyle = new NumberingStyle()
        {
            Levels = 1,
            NumberingType = NumberingStyle.NumberingFormat.Bullet,
            Marker = "•"
        };

        private TableStyle tableStyle = new TableStyle()
        {
            MinCellHeight = 453,
            VerticalAlignment = "center",
            BorderThilness = 4

        };

        private TextStyle tableTextStyle = new TextStyle()
        {
            FontName = "Times New Roman",
            FontSize = 10,
        };

        private ParagraphStyle tableParagraphStyle = new ParagraphStyle()
        {
            Alingnment = "right",
            IntervalInText = 240
        };

        private ParagraphStyle pictureStyle = new ParagraphStyle()
        {
            Alingnment = "center",
            FirstLineIndent = 0
        };

        public ParseManager(XMLRead xmlRead, StyleCreator creator)
        {
            _xmlRead = xmlRead;
            _creator = creator;

            try
            {
                xmlRead.UnZipDocx(tempReadPath, tempFolder);

                var (styleRoot, styleSpecialTokens) = ReadXMLDocument(xmlRead, tempReadPath, styles);
                var (documentRoot, documentSpecialTokens) = ReadXMLDocument(xmlRead, tempReadPath, document);

                Dictionary<int, List<TreeNode>> name = SplitParagraphsWithDrawings(ExtractPicturesFromParagraphToDictionary(documentRoot));

                ReconstructParagraphs(documentRoot, name);

                CleanHandStyles(documentRoot, documentSpecialTokens, _xmlRead, tempSavePath);

                (TreeNode paragraphStyleNode, styleRoot) = CreateStyleInFile(_xmlRead, paragraphStyle, tempReadPath, tempSavePath, styleRoot, styleSpecialTokens);
                ApplyStyle(documentRoot, documentSpecialTokens, paragraphStyleNode, _xmlRead, "paragraph");

                (TreeNode pictureStyleNode, styleRoot) = CreateStyleInFile(_xmlRead, pictureStyle, tempReadPath, tempSavePath, styleRoot, styleSpecialTokens);
                ApplyPictureStyle(documentRoot, pictureStyleNode, _xmlRead, tempReadPath);

                CorrectParagraphChildren("w:pPr", _xmlRead, tempReadPath);

                (TreeNode textStyleNode, styleRoot) = CreateStyleInFile(_xmlRead, textStyle, tempReadPath, tempSavePath, styleRoot, styleSpecialTokens);
                ApplyStyle(documentRoot, documentSpecialTokens, textStyleNode, _xmlRead, "character");

                (TreeNode tableStyleNode, styleRoot) = CreateStyleInFile(_xmlRead, tableStyle, tempReadPath, tempSavePath, styleRoot, styleSpecialTokens);
                ApplyStyle(documentRoot, documentSpecialTokens, tableStyleNode, _xmlRead, "table");

                var (numberingStyleNode, appliedStyle) = CreateNumberingStyleInFile(_xmlRead, numberingStyle, tempReadPath, tempSavePath);
                ApplyNumberingStyle(documentRoot, appliedStyle, _xmlRead, tempReadPath, numberingStyle.Levels);

                //Стиль для ячеек таблиц
                (TreeNode paragraphTableStyleNode, styleRoot) = CreateStyleInFile(_xmlRead, tableParagraphStyle, tempReadPath, tempSavePath, styleRoot, styleSpecialTokens);
                (TreeNode textTableStyleNode, styleRoot) = CreateStyleInFile(_xmlRead, tableTextStyle, tempReadPath, tempSavePath, styleRoot, styleSpecialTokens);
                CleanHandTableStyle(documentRoot);

                ApplyTableCellStyle(documentRoot, documentSpecialTokens, textTableStyleNode, paragraphTableStyleNode, _xmlRead);

                SerializeStyle(xmlRead, styleRoot, styleSpecialTokens);

                SaveApply(xmlRead, documentRoot, documentSpecialTokens);


                xmlRead.FilesInZip(tempReadPath, tempFolder, ExtractFileNameFromPath(tempReadPath), tempSavePath);


            }
            finally
            {
                Directory.Delete(tempFolder, true);
            }


        }

        private void ReconstructParagraphs(TreeNode root, Dictionary<int, List<TreeNode>> extensiveParagrahps)
        {
            int count = FindParagraphsCount(root, extensiveParagrahps);

            List<TreeNode> newRoot = new List<TreeNode>();
            List<TreeNode> oldParagraphs = root.LongBreadthFirstSearch(root, "w:p");

            count = count + (oldParagraphs.Count - extensiveParagrahps.Count);

            for (int i = 0; i < oldParagraphs.Count; i++)
            {
                if(extensiveParagrahps.Any(c => c.Key == i))
                {
                    foreach (var paragraph in extensiveParagrahps[i])
                    {
                        newRoot.Add(paragraph);
                    }
                }
                else
                {
                    newRoot.Add(oldParagraphs[i]);
                }
            }

            List<TreeNode> body = root.QuikBreadthFirstSearch(root, "w:body");
            body[0].Children.Clear();
            body[0].Children = newRoot;
            
        }

        private int FindParagraphsCount(TreeNode root, Dictionary<int, List<TreeNode>> extensiveParagrahps)
        {
            int count = 0;

            foreach(var paragraphs in extensiveParagrahps)
            {
                foreach (var paragraph in paragraphs.Value) 
                { 
                    count++;
                }
            }

            return count;
        }

        private Dictionary<int, TreeNode> ExtractPicturesFromParagraphToDictionary(TreeNode root)
        {
            List<TreeNode> paragraphs = root.LongBreadthFirstSearch(root, "w:p");
            Dictionary<int, TreeNode> paragraphsWithPic = new Dictionary<int, TreeNode>();

            //проход по <w:p>
            for (int i = 0; i < paragraphs.Count; i++)
            {
                //грубо говоря проход по <w:r>
                for (int j = 0; j < paragraphs[i].Children.Count; j++)
                {
                    //поиск <w:drawing>
                    for (int k = 0; k < paragraphs[i].Children[j].Children.Count; k++)
                    {
                        if (paragraphs[i].Children[j].Children[k].TagName == "w:drawing")
                        {
                            paragraphsWithPic.Add(i, paragraphs[i]);
                            break;
                        }
                    }
                }
            }
            return paragraphsWithPic;
        }

        private List<TreeNode> ExtractPicturesFromParagraphToList(TreeNode root)
        {
            List<TreeNode> paragraphs = root.LongBreadthFirstSearch(root, "w:p");
            List<TreeNode> paragraphsWithPic = new List<TreeNode>();

            //проход по <w:p>
            for (int i = 0; i < paragraphs.Count; i++)
            {
                //грубо говоря проход по <w:r>
                for (int j = 0; j < paragraphs[i].Children.Count; j++)
                {
                    //поиск <w:drawing>
                    for (int k = 0; k < paragraphs[i].Children[j].Children.Count; k++)
                    {
                        if (paragraphs[i].Children[j].Children[k].TagName == "w:drawing")
                        {
                            paragraphsWithPic.Add(paragraphs[i]);
                            break;
                        }
                    }
                }
            }
            return paragraphsWithPic;
        }

        private Dictionary<int, List<TreeNode>> SplitParagraphsWithDrawings(Dictionary<int, TreeNode> paragraphs)
        {
            Dictionary<int, List<TreeNode>> splittedParagraphs = new Dictionary<int, List<TreeNode>>();

            TreeNode paragraphStyle = ExtractStyle(paragraphs.Values.First(), "w:pPr");

            Dictionary<int, TreeNode> nonStyleParagraphs = new Dictionary<int, TreeNode>();
            foreach (var paragraph in paragraphs)
            {
                paragraph.Value.TerminateSpecialCildren(paragraph.Value, "w:pPr");
                nonStyleParagraphs.Add(paragraph.Key, paragraph.Value);
            }

            foreach (var paragraph in nonStyleParagraphs)
            {
                List<TreeNode> paragrahpsOneNumber = new List<TreeNode>(); // Новый список для каждого абзаца
                List<TreeNode> kit = new List<TreeNode>(); // Список для текстовых элементов

                for (int i = 0; i < paragraph.Value.Children.Count; i++)
                {
                    TreeNode child = paragraph.Value.Children[i];

                    if (child.Children.Any(c => c.TagName == "w:drawing"))
                    {
                        // Если перед рисунком был текст, создаем новый текстовый абзац
                        if (kit.Count > 0)
                        {
                            var textPackage = new TreeNode()
                            {
                                TagName = "w:p",
                                CloseTag = true,
                                Children = new List<TreeNode>{ paragraphStyle.Clone() } // Копия списка
                            };
                            textPackage.Children.AddRange(kit);
                            paragrahpsOneNumber.Add(textPackage);
                            kit.Clear();
                        }

                        // Создаем отдельный абзац для рисунка
                        var drawingPackage = new TreeNode()
                        {
                            TagName = "w:p",
                            CloseTag = true,
                            Children = new List<TreeNode> { paragraphStyle.Clone(), child.Clone() }
                        };
                        paragrahpsOneNumber.Add(drawingPackage);
                    }
                    else
                    {
                        kit.Add(child);
                    }
                }

                // Если остался текст без рисунков, добавляем его в отдельный абзац
                if (kit.Count > 0)
                {
                    var textPackage = new TreeNode()
                    {
                        TagName = "w:p",
                        CloseTag = true,
                        Children = new List<TreeNode> { paragraphStyle.Clone() }
                    };
                    textPackage.Children.AddRange(kit);
                    paragrahpsOneNumber.Add(textPackage);
                }

                splittedParagraphs.Add(paragraph.Key, paragrahpsOneNumber);
            }

            return splittedParagraphs;
        }


        private TreeNode ExtractStyle(TreeNode parent, string styleTagName)
        {
            List<TreeNode>extractedStyles = parent.QuikBreadthFirstSearch(parent, styleTagName);

            if (extractedStyles.Count == 1)
            {
                return extractedStyles[0];
            }
            else
            {
                return null;
            }

        }

        private void CleanHandStyles(TreeNode root, List<string> specialTokens,XMLRead xmlRead, string savePath)
        {
            List<TreeNode> foundedParents = new List<TreeNode>();

            foundedParents = root.QuikBreadthFirstSearch(root, "w:rPr");
            root.TerminateChildren(foundedParents);
            foundedParents = root.QuikBreadthFirstSearch(root, "w:pPr");
            root.TerminateChildren(foundedParents);
            foundedParents = root.QuikBreadthFirstSearch(root, "w:numPr");
            root.TerminateChildren(foundedParents);
            foundedParents = root.QuikBreadthFirstSearch(root, "w:tblPr");
            root.TerminateChildren(foundedParents);


            SaveApply(xmlRead, root, specialTokens);
        }

        private void CleanHandTableStyle(TreeNode root)
        {
            List<TreeNode> foundedCells = new List<TreeNode>();

            foundedCells = root.LongBreadthFirstSearch(root, "w:tc");

            foreach (var cell in foundedCells) 
            {
                List<TreeNode> foundedParents = new List<TreeNode>();

                foundedParents = cell.QuikBreadthFirstSearch(cell, "w:rPr");
                root.TerminateChildren(foundedParents);
                foundedParents = cell.QuikBreadthFirstSearch(cell, "w:pPr");
                root.TerminateChildren(foundedParents);
            }

        }

        private string ExtractFileNameFromPath(string path)
        {
            return Path.GetFileName(path);
        }

        private (TreeNode, TreeNode) CreateStyleInFile(XMLRead xmlRead, IStyle style, string readPath, string savePath, TreeNode root, List<string> specialTokens, TextStyle? tableTextStyle = null, ParagraphStyle? tableParagraphStyle = null)
        {
            TreeNode styleNode = new TreeNode();

            if (style is TextStyle textStyle)
            {
                styleNode = _creator.CreateTextAndParagraphStyleNode(style, textStyle.CreateTextStyle(textStyle), root);
            }
            else if (style is ParagraphStyle paragraphStyle)
            {
                styleNode = _creator.CreateTextAndParagraphStyleNode(style, paragraphStyle.CreateParagraphStyle(paragraphStyle), root);
            }
            else if (style is TableStyle tableStyle)
            {
                styleNode = _creator.CreateTableStyleNode(tableStyle.CreateTableStyle(tableStyle, tableTextStyle, tableParagraphStyle), root);
            }
            _creator.InroduceStyleInTree(root, styleNode);

            return (styleNode, root);
        }

        private void SerializeStyle(XMLRead xmlRead, TreeNode root, List<string> specialTokens)
        {
            string serializedTree = xmlRead.SerializeNode(root, specialTokens);
            xmlRead.StringToXMLDocument(serializedTree, styles, tempFolder);
        }

        private (TreeNode, TreeNode) CreateNumberingStyleInFile(XMLRead xmlRead, NumberingStyle style, string readPath, string savePath)
        {
            var (root, specialTokens) = ReadXMLDocument(xmlRead, readPath, numbering);

            var (numberingStyle, appliedStyle) = _creator.CreateNumberingStyleNodes(style.CreateNumberingStyle(), root);

            _creator.InroduceStyleInTree(root, numberingStyle);
            _creator.InroduceStyleInTree(root, appliedStyle);

            string serializedTree = xmlRead.SerializeNode(root, specialTokens);
            xmlRead.StringToXMLDocument(serializedTree, numbering, tempFolder);
            return (numberingStyle, appliedStyle);
        }

        private void ApplyPictureStyle(TreeNode root, TreeNode style, XMLRead xmlRead, string readPath)
        {
            List<TreeNode> paragraphsWithDrawings = ExtractPicturesFromParagraphToList(root);

            foreach (var paragraph in paragraphsWithDrawings)
            {
                List<TreeNode> oldStyle = paragraph.QuikBreadthFirstSearch(paragraph, "w:pPr");

                paragraph.TerminateChildren(oldStyle);

                TreeNode styleToApply = new TreeNode()
                {
                    TagName = "w:pStyle",
                    Attributes = { { "w:val", style.Attributes["w:styleId"] } },
                    CloseTag = false
                };

                foreach (TreeNode styleElement in oldStyle)
                {
                    styleElement.Children.Add(styleToApply);
                }
            }

        }
        private void ApplyNumberingStyle(TreeNode root, TreeNode aplliedStyle, XMLRead xmlRead, string readPath, int numLevel)
        {
            string styleTagName = "";
            string numberingStyleId = aplliedStyle.Attributes["w:numId"];
            List<TreeNode> children = new List<TreeNode>();

            TreeNode numberingLevel = new TreeNode()
            {
                TagName = "w:ilvl",
                Attributes = { { "w:val", $"{numLevel}" } }
            };

            TreeNode numberingStyle = new TreeNode()
            {
                TagName = "w:numId",
                Attributes = { { "w:val", numberingStyleId } },
            };

            children.Add(numberingLevel);
            children.Add(numberingStyle);

            List<TreeNode> foundedParents = new List<TreeNode>();
            foundedParents = root.QuikBreadthFirstSearch(root, "w:numPr");

            root.AddChildren(foundedParents, children);


        }
        private void ApplyTableCellStyle(TreeNode root, List<string> specialTokens, TreeNode textStyle, TreeNode paragraphStyle, XMLRead xmlRead)
        {
            List<TreeNode> cells = root.LongBreadthFirstSearch(root, "w:tc");

            foreach (TreeNode cell in cells)
            {
                ApplyStyle(cell, specialTokens, paragraphStyle, xmlRead, "paragraph");
                ApplyStyle(cell, specialTokens, textStyle, xmlRead, "character");
            }
        }

        private void ApplyStyle(TreeNode root, List<string> specialTokens, TreeNode style, XMLRead xmlRead, string styleType)
        {
            string styleTagName = "";
            string tagName = "";
            string styleName = style.Attributes["w:styleId"];
            switch (styleType)
            {
                case "character":
                    styleTagName = "w:rStyle";
                    tagName = "w:rPr";
                    break;

                case "paragraph":
                    styleTagName = "w:pStyle";
                    tagName = "w:pPr";
                    break;

                case "table":
                    styleTagName = "w:tblStyle";
                    tagName = "w:tblPr";
                    break;

            }

            TreeNode styleToApply = new TreeNode()
            {
                TagName = styleTagName,
                Attributes = { { "w:val", styleName } },
                CloseTag = false
            };

            List<TreeNode> foundedParents = new List<TreeNode>();
            foundedParents = root.QuikBreadthFirstSearch(root, tagName);

            for (int i = 0; i < foundedParents.Count; i++)
            {
                root.AddChild(foundedParents[i], styleToApply);
            }
            
        }

        private void SaveApply(XMLRead xmlRead, TreeNode root, List<string> specialTokens)
        {
            string serializedTree = xmlRead.SerializeNode(root, specialTokens);
            xmlRead.StringToXMLDocument(serializedTree, document, tempFolder);
        }

        private void CorrectParagraphChildren(string parentName, XMLRead xmlRead, string readPath)
        {
            // Чтение XML-документа
            var (root, specialTokens) = ReadXMLDocument(xmlRead, readPath, document);

            // Поиск всех родительских элементов с указанным именем
            List<TreeNode> foundedParents = root.QuikBreadthFirstSearch(root, parentName);

            // Обработка каждого родительского элемента
            foreach (var parent in foundedParents)
            {
                // Пропускаем, если нет дочерних элементов
                if (parent.Children.Count == 0)
                {
                    continue;
                }

                // Находим первый элемент <w:pStyle> (если он есть)
                var pStyleNode = parent.Children.FirstOrDefault(child => child.TagName == "w:pStyle");

                if (pStyleNode != null)
                {

                    // Удаляем <w:pStyle> из текущей позиции
                    parent.Children.Remove(pStyleNode);

                    // Вставляем <w:pStyle> на первое место
                    parent.Children.Insert(0, pStyleNode);
                }
            }

            // Сериализация и десериализация дерева
            string serializedTree = xmlRead.SerializeNode(root, specialTokens);
            xmlRead.StringToXMLDocument(serializedTree, document, tempFolder);
        }

        private (TreeNode root, List<string> specialTokens) ReadXMLDocument(XMLRead xmlRead, string readPath, string fileName)
        {
            TreeNode root = new TreeNode();
            var (fileInTockens, specialTokens) = xmlRead.Tokenize(xmlRead.XMLDocumentFileToString(fileName, tempFolder));
            root = root.BuildTree(fileInTockens);
            return (root, specialTokens);
        }

    }
}
