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
        private readonly string tempWritePath = "C:\\Лабы\\AppTestDocx\\Arhive";
        private string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        public ParseManager(XMLRead xmlRead, TextStyle textStyle)
        {
            _xmlRead = xmlRead; 
            _style = textStyle;

            try
            {
                CleanHandTextStyles(_xmlRead, tempReadPath, tempReadPath);
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
            xmlRead.FilesInZip(readPath, tempFolder, ExtractFileNameFromPath(readPath), savePath);
        }

        private string ExtractFileNameFromPath(string path)
        {
            return Path.GetFileName(path);
        }

        private void CreateTextStyle(XMLRead xmlRead, TextStyle textStyle)
        {

        }

    }
}
