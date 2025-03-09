using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml;

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
            Allingnment = "both",
            IntervalInText = 360,
            FirstLineIndent = 1.25
        };

        private NumberingStyle numberingStyle = new NumberingStyle()
        {
            Levels = 1,
            NumberingType = NumberingStyle.NumberingFormat.Bullet,
            Marker = "•"
        };

        public ParseManager(XMLRead xmlRead, StyleCreator creator)
        {
            _xmlRead = xmlRead;
            _creator = creator;

            try
            {
                xmlRead.UnZipDocx(tempReadPath, tempFolder);
                CleanHandStyles(_xmlRead, tempReadPath, tempSavePath);

                TreeNode paragraphStyleNode = CreateStyleInFile(_xmlRead, paragraphStyle, tempReadPath, tempSavePath, "paragraph");
                ApplyTextAndParagraphStyle(paragraphStyleNode, _xmlRead, tempReadPath, "paragraph");
                CorrectParagraphChildren("w:pPr", _xmlRead, tempReadPath);

                TreeNode textStyleNode = CreateStyleInFile(_xmlRead, textStyle, tempReadPath, tempSavePath, "character");
                ApplyTextAndParagraphStyle(textStyleNode, _xmlRead, tempReadPath, "character");

                var (numberingStyleNode, appliedStyle) = CreateNumberingStyleInFile(_xmlRead, numberingStyle, tempReadPath, tempSavePath);
                ApplyNumberingStyle(appliedStyle, _xmlRead, tempReadPath, numberingStyle.Levels);


                xmlRead.FilesInZip(tempReadPath, tempFolder, ExtractFileNameFromPath(tempReadPath), tempSavePath);
            }
            finally
            {
                Directory.Delete(tempFolder, true);
            }

           
        }
        private void CleanHandStyles(XMLRead xmlRead, string readPath, string savePath)
        {
            List<TreeNode> foundedParents = new List<TreeNode>();

            var (root, specialTokens) = ReadXMLDocument(xmlRead, readPath, document);
            
            foundedParents = root.QuikBreadthFirstSearch(root, "w:rPr");
            root.TerminateChildren(foundedParents);
            foundedParents = root.QuikBreadthFirstSearch(root, "w:pPr");
            root.TerminateChildren(foundedParents);
            foundedParents = root.QuikBreadthFirstSearch(root, "w:numPr");
            root.TerminateChildren(foundedParents);

            string serializedTree = xmlRead.SerializeNode(root, specialTokens);
            xmlRead.StringToXMLDocument(serializedTree, document, tempFolder);
        }

        private string ExtractFileNameFromPath(string path)
        {
            return Path.GetFileName(path);
        }

        private TreeNode CreateStyleInFile(XMLRead xmlRead, IStyle style, string readPath, string savePath, string styleType)
        {
            TreeNode styleNode = new TreeNode();

            var (root, specialTokens) = ReadXMLDocument(xmlRead, readPath, styles);

            if (style is TextStyle textStyle)
            {
                styleNode = _creator.CreateTextAndParagraphStyleNode(styleType, textStyle.CreateTextStyle(textStyle), root);
            }
            else if (style is ParagraphStyle paragraphStyle)
            {
                styleNode = _creator.CreateTextAndParagraphStyleNode(styleType, paragraphStyle.CreateParagraphStyle(paragraphStyle), root);
            }
            _creator.InroduceStyleInTree(root, styleNode);

            string serializedTree = xmlRead.SerializeNode(root, specialTokens);
            xmlRead.StringToXMLDocument(serializedTree, styles, tempFolder);
            return styleNode;
        }

        private (TreeNode, TreeNode) CreateNumberingStyleInFile(XMLRead xmlRead, NumberingStyle style, string readPath, string savePath)
        {
            var (root, specialTokens) = ReadXMLDocument(xmlRead, readPath, numbering);

            var(numberingStyle, appliedStyle) = _creator.CreateNumberingStyleNodes(style.CreateNumberingStyle(), root);

            _creator.InroduceStyleInTree(root, numberingStyle);
            _creator.InroduceStyleInTree(root, appliedStyle);

            string serializedTree = xmlRead.SerializeNode(root, specialTokens);
            xmlRead.StringToXMLDocument(serializedTree, numbering, tempFolder);
            return (numberingStyle, appliedStyle);
        }

        private void ApplyNumberingStyle(TreeNode aplliedStyle, XMLRead xmlRead, string readPath, int numLevel)
        {
            string styleTagName = "";
            string numberingStyleId = aplliedStyle.Attributes["w:numId"];
            List<TreeNode> children = new List<TreeNode>();

            TreeNode numberingLevel = new TreeNode()
            {
                TagName = "w:ilvl",
                Attributes = { { "w:val", $"{numLevel}"} }
            };

            TreeNode numberingStyle = new TreeNode()
            {
                TagName = "w:numId",
                Attributes = { { "w:val",  numberingStyleId } },
            };

            children.Add(numberingLevel);
            children.Add(numberingStyle);

            var (root, specialTokens) = ReadXMLDocument(xmlRead, readPath, document);

            List<TreeNode> foundedParents = new List<TreeNode>();
            foundedParents = root.QuikBreadthFirstSearch(root, "w:numPr");
            
            root.AddChildren(foundedParents, children);
            
            string serializedTree = xmlRead.SerializeNode(root, specialTokens);
            xmlRead.StringToXMLDocument(serializedTree, document, tempFolder);

        }
        private void ApplyTextAndParagraphStyle(TreeNode style, XMLRead xmlRead, string readPath, string styleType)
        {
            string styleTagName = "";
            string tagName = "";
            string styleName = style.Attributes["w:styleId"];
            if (styleType == "character")
            {
                styleTagName = "w:rStyle";
                tagName = "w:rPr";
            }
            else if (styleType == "paragraph")
            {
                styleTagName = "w:pStyle";
                tagName = "w:pPr";
            }

            TreeNode styleToApply = new TreeNode()
            {
                TagName = styleTagName,
                Attributes = { { "w:val", styleName } },
                CloseTag = false
            };

            var (root, specialTokens) = ReadXMLDocument(xmlRead, readPath, document);

            List<TreeNode> foundedParents = new List<TreeNode>();
            foundedParents = root.QuikBreadthFirstSearch(root, tagName);
            for (int i = 0; i < foundedParents.Count; i++) 
            {
                root.AddChild(foundedParents[i], styleToApply);
            }
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
