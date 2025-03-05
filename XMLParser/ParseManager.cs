using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XMLParser
{
    public class ParseManager
    {
        private readonly TreeNode _root;
        private readonly XMLRead _xmlRead;
        private readonly TextStyle _style;

        private const string document = "document.xml";
        private const string styles = "styles.xml";

        private readonly string tempReadPath = "C:\\Лабы\\AppTestDocx\\5 Лаба.docx";
        private readonly string tempSavePath = "C:\\Лабы\\AppTestDocx";
        private string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        private TextStyle testStyle = new TextStyle()
        {
            FontName = "Times New Roman",
            FontSize = 14
        };

        public ParseManager(XMLRead xmlRead, TextStyle textStyle)
        {
            _xmlRead = xmlRead; 
            _style = textStyle;

            try
            {
                xmlRead.UnZipDocx(tempReadPath, tempFolder);
                CleanHandTextStyles(_xmlRead, tempReadPath, tempSavePath);
                TreeNode styleNode = CreateTextStyleInFile(_xmlRead, _style, tempReadPath, tempSavePath);
                ApplyTextStyle(styleNode, _xmlRead, tempReadPath);
                xmlRead.FilesInZip(tempReadPath, tempFolder, ExtractFileNameFromPath(tempReadPath), tempSavePath);
            }
            finally
            {
                Directory.Delete(tempFolder, true);
            }

           
        }
        private void CleanHandTextStyles(XMLRead xmlRead, string readPath, string savePath)
        {
            List<TreeNode> foundedParents = new List<TreeNode>();

            var (root, specialTokens) = ReadXMLDocument(xmlRead, readPath, document);
            
            foundedParents = root.BreadthFirstSearch(root, "w:rPr");
            root.TerminateChildren(foundedParents);

            string serializedTree = xmlRead.SerializeNode(root, specialTokens);
            xmlRead.StringToXMLDocument(serializedTree, document, tempFolder);
        }

        private string ExtractFileNameFromPath(string path)
        {
            return Path.GetFileName(path);
        }

        private TreeNode CreateTextStyleInFile(XMLRead xmlRead, TextStyle textStyle, string readPath, string savePath)
        {
            TreeNode styleNode = new TreeNode();

            var (root, specialTokens) = ReadXMLDocument(xmlRead, readPath, styles);

            styleNode = textStyle.CreateTextStyleNode(textStyle.CreateTextStyle(testStyle), root, "w:style");
            textStyle.InroduceStyleInTree(root, styleNode);

            string serializedTree = xmlRead.SerializeNode(root, specialTokens);
            xmlRead.StringToXMLDocument(serializedTree, styles, tempFolder);
            return styleNode;
        }

        private void ApplyTextStyle(TreeNode style, XMLRead xmlRead, string readPath)
        {
            string styleName = style.Attributes["w:styleId"];
            TreeNode styleToApply = new TreeNode()
            {
                TagName = "w:rStyle",
                Attributes = { { "w:val", styleName } },
                CloseTag = false
            };

            var (root, specialTokens) = ReadXMLDocument(xmlRead, readPath, document);

            List<TreeNode> foundedParents = new List<TreeNode>();
            foundedParents = root.BreadthFirstSearch(root, "w:rPr");
            for (int i = 0; i < foundedParents.Count; i++) 
            {
                root.AddChild(foundedParents[i], styleToApply);
            }
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
