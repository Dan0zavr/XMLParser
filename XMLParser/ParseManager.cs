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
                CleanHandTextStyles(_xmlRead, tempReadPath, tempSavePath);
                CreateTextStyleInFile(_xmlRead, _style, tempReadPath, tempSavePath);
            }
            finally
            {
                Directory.Delete(tempFolder, true);
            }

           
        }
        private void CleanHandTextStyles(XMLRead xmlRead, string readPath, string savePath)
        {
            List<TreeNode> foundedParents = new List<TreeNode>();
            TreeNode root = new TreeNode();
            xmlRead.UnZipDocx(readPath, tempFolder);
            var (fileInTockens, specialTokens) = xmlRead.Tokenize(xmlRead.XMLDocumentFileToString(document, tempFolder));
            root = root.BuildTree(fileInTockens);
            foundedParents = root.BreadthFirstSearch(root, "w:rPr");
            root.TerminateChildren(foundedParents);

            string serializedTree = xmlRead.SerializeNode(root, specialTokens);
            xmlRead.StringToXMLDocument(serializedTree, document, tempFolder);
        }

        private string ExtractFileNameFromPath(string path)
        {
            return Path.GetFileName(path);
        }

        private void CreateTextStyleInFile(XMLRead xmlRead, TextStyle textStyle, string readPath, string savePath)
        {
            TreeNode root = new TreeNode();
            TreeNode styleNode = new TreeNode();
            var (fileInTokens, specialTokens) = xmlRead.Tokenize(xmlRead.XMLDocumentFileToString(styles, tempFolder));
            root = root.BuildTree(fileInTokens);

            styleNode = textStyle.CreateTextStyleNode(textStyle.CreateTextStyle(testStyle), root, "w:style");
            textStyle.InroduceStyleInTree(root, styleNode);

            string serializedTree = xmlRead.SerializeNode(root, specialTokens);
            xmlRead.StringToXMLDocument(serializedTree, styles, tempFolder);
            xmlRead.FilesInZip(readPath, tempFolder, ExtractFileNameFromPath(readPath), savePath);
        }

    }
}
