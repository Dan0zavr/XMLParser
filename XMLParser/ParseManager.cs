using System.Xml;
using XMLParser.Styles;
using System.Collections.Generic;

namespace XMLParser
{
    public class ParseManager
    {
        private readonly XMLRead _xmlRead;
        private readonly StyleCreator _creator;

        private const string document = "document.xml";
        private const string styles = "styles.xml";
        private const string numbering = "numbering.xml";

        private readonly string _readPath;
        private readonly string _savePath;
        private string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());


        public ParseManager(XMLRead xmlRead, StyleCreator creator, Template template, string readPath, string savePath)
        {
            _xmlRead = xmlRead;
            _creator = creator;
            _readPath = readPath;
            _savePath = savePath;

            try
            {
                xmlRead.UnZipDocx(_readPath, tempFolder);

                var (styleRoot, styleSpecialTokens) = ReadXMLDocument(xmlRead, _readPath, styles);
                var (documentRoot, documentSpecialTokens) = ReadXMLDocument(xmlRead, _readPath, document);

                var (titlePage, content, mainTag) = SplitDocument(documentRoot);

                Dictionary<int, List<TreeNode>> name = SplitParagraphsWithDrawings(ExtractPicturesFromParagraphToDictionary(content));

                if (name != null)
                {

                    ReconstructParagraphs(content, name);
                }

                CleanHandStyles(content, documentSpecialTokens, _xmlRead,_savePath);

                (TreeNode paragraphStyleNode, styleRoot) = CreateStyleInFile(_xmlRead, template.ParagraphStyle, _readPath, _savePath, styleRoot, styleSpecialTokens);
                ApplyStyle(content, documentSpecialTokens, paragraphStyleNode, _xmlRead, "paragraph");

                if (template.PictureStyle != null)
                {

                    (TreeNode pictureStyleNode, styleRoot) = CreateStyleInFile(_xmlRead, template.PictureStyle, _readPath, _savePath, styleRoot, styleSpecialTokens);
                    ApplyPictureStyle(content, pictureStyleNode, _xmlRead, _readPath);


                    CorrectParagraphChildren("w:pPr", _xmlRead, _readPath);
                }

                (TreeNode textStyleNode, styleRoot) = CreateStyleInFile(_xmlRead, template.TextStyle, _readPath, _savePath, styleRoot, styleSpecialTokens);
                ApplyStyle(content, documentSpecialTokens, textStyleNode, _xmlRead, "character");

                if (template.TableStyle != null) 
                {
                    (TreeNode tableStyleNode, styleRoot) = CreateStyleInFile(_xmlRead, template.TableStyle, _readPath, _savePath, styleRoot, styleSpecialTokens);
                    ApplyStyle(content, documentSpecialTokens, tableStyleNode, _xmlRead, "table");
                }

                if (template.NumberingStyle != null)
                {
                    var (numberingStyleNode, appliedStyle) = CreateNumberingStyleInFile(_xmlRead, template.NumberingStyle, _readPath, _savePath);
                    ApplyNumberingStyle(content, appliedStyle, _xmlRead, _readPath, template.NumberingStyle.Levels);
                }
                if (template.TableStyle != null) {
                    //Стиль для ячеек таблиц
                    (TreeNode paragraphTableStyleNode, styleRoot) = CreateStyleInFile(_xmlRead, template.TableStyle.ParagraphStyle, _readPath, _savePath, styleRoot, styleSpecialTokens);
                    (TreeNode textTableStyleNode, styleRoot) = CreateStyleInFile(_xmlRead, template.TableStyle.TextStyle, _readPath, _savePath, styleRoot, styleSpecialTokens);
                    CleanHandTableStyle(content);

                    ApplyTableCellStyle(content, documentSpecialTokens, textTableStyleNode, paragraphTableStyleNode, _xmlRead);
                }

                SerializeStyle(xmlRead, styleRoot, styleSpecialTokens);

                TreeNode endRoot = MergeDocument(titlePage, content, mainTag);

                SaveApply(xmlRead, endRoot, documentSpecialTokens);


                xmlRead.FilesInZip(_readPath, tempFolder, ExtractFileNameFromPath(_readPath), _savePath);


            }
            finally
            {
                Directory.Delete(tempFolder, true);
            }


        }

        private void ReconstructParagraphs(TreeNode root, Dictionary<int, List<TreeNode>> extensiveParagrahps)
        {
            List<TreeNode> newRoot = new List<TreeNode>();
            List<TreeNode> oldParagraphs = root.Children;

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

            //List<TreeNode> body = root.QuikBreadthFirstSearch(root, "w:body");
            //body[0].Children.Clear();
            //body[0].Children = newRoot;

            root.Children.Clear();
            root.Children = newRoot;

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
            if (paragraphs.Count != 0)
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
                                    Children = new List<TreeNode> { paragraphStyle.Clone() } // Копия списка
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
            else
            {
                return null;
            }
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

        private (TreeNode, TreeNode) CreateStyleInFile(XMLRead xmlRead, IStyle style, string readPath, string savePath,
            TreeNode root, List<string> specialTokens, XMLParser.Styles.TextStyle? tableTextStyle = null, XMLParser.Styles.ParagraphStyle? tableParagraphStyle = null)
        {
            TreeNode styleNode = new TreeNode();

            if (style is XMLParser.Styles.TextStyle textStyle)
            {
                styleNode = _creator.CreateTextAndParagraphStyleNode(style, textStyle.CreateTextStyle(textStyle), root);
            }
            else if (style is XMLParser.Styles.ParagraphStyle paragraphStyle)
            {
                styleNode = _creator.CreateTextAndParagraphStyleNode(style, paragraphStyle.CreateParagraphStyle(paragraphStyle), root);
            }
            else if (style is XMLParser.Styles.TableStyle tableStyle)
            {
                styleNode = _creator.CreateTableStyleNode(tableStyle.CreateTableStyle(tableStyle), root);
            }
            _creator.InroduceStyleInTree(root, styleNode);

            return (styleNode, root);
        }

        private void SerializeStyle(XMLRead xmlRead, TreeNode root, List<string> specialTokens)
        {
            string serializedTree = xmlRead.SerializeNode(root, specialTokens);
            xmlRead.StringToXMLDocument(serializedTree, styles, tempFolder);
        }

        private (TreeNode, TreeNode) CreateNumberingStyleInFile(XMLRead xmlRead, XMLParser.Styles.NumberingStyle style, string readPath, string savePath)
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

        private (TreeNode titlePage, TreeNode content, TreeNode mainTag) SplitDocument(TreeNode root)
        {
            List<TreeNode> titlePageChildren = new List<TreeNode>();
            List<TreeNode> contentChildren = new List<TreeNode>();
            TreeNode? sectionProperties = null; // Секция документа
 

            bool pageBreakFounded = false;

            foreach (TreeNode paragraph in root.Children[0].Children)
            {
                // Проверяем, есть ли разрыв страницы или секции
                if (!pageBreakFounded)
                {
                    if (paragraph.QuikBreadthFirstSearch(paragraph, "w:sectPr").Any())
                    {
                        pageBreakFounded = true;
                        sectionProperties = paragraph.QuikBreadthFirstSearch(paragraph, "w:sectPr").First();
                    }

                    foreach (TreeNode breakNode in paragraph.QuikBreadthFirstSearch(paragraph, "w:br"))
                    {
                        if (breakNode.Attributes.TryGetValue("w:type", out string value) && value == "page")
                        {
                            pageBreakFounded = true;
                            break;
                        }
                    }
                }

                if (!pageBreakFounded)
                {
                    titlePageChildren.Add(paragraph);
                }
                else
                {
                    contentChildren.Add(paragraph);
                }
            }

            // Если секция была в титульном листе, перенесём её в конец контента
            if (sectionProperties != null && !contentChildren.Contains(sectionProperties))
            {
                contentChildren.Add(sectionProperties);
            }

            TreeNode titlePage = new TreeNode()
            {
                TagName = "w:body",
                CloseTag = true,
                Children = titlePageChildren
            };

            TreeNode content = new TreeNode()
            {
                TagName = "w:body",
                CloseTag = true,
                Children = contentChildren
            };

            TreeNode mainTag = new TreeNode()
            {
                TagName = root.TagName,
                CloseTag = true,    
                Attributes = root.Attributes
            };

            return (titlePage, content, mainTag);
        }

        private TreeNode MergeDocument(TreeNode titlePage, TreeNode content, TreeNode mainTag)
        {
            TreeNode document = new TreeNode() 
            {
                TagName = "w:body",
                CloseTag = true,
            };
            foreach (TreeNode child in titlePage.Children) 
            {
                document.Children.Add(child);
            }
            foreach (TreeNode child in content.Children)
            {
                document.Children.Add(child);
            }
            mainTag.Children.Add(document);

            return mainTag;
        }
    }
}
