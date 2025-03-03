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

        public ParseManager(XMLRead xmlRead, TextStyle textStyle)
        {
            _xmlRead = xmlRead; 
            _style = textStyle;

            try
            {
                CleanHandTextStyles(_xmlRead, _style);
            }
            finally
            {
                Directory.Delete(xmlRead._tempFolder, true);
            }

           
        }
        private void CleanHandTextStyles(XMLRead xmlRead, TextStyle textStyle)
        {
            List<TreeNode> foundedParents = new List<TreeNode>();
            TreeNode root = new TreeNode();
            xmlRead.UnZipDocx();
            var (fileInTockens, specialTokens) = xmlRead.Tokenize(xmlRead.XMLDocumentFileToString(document));
            root = root.BuildTree(fileInTockens);
            foundedParents = root.BreadthFirstSearch(root, "w:rPr");
            root.TerminateChildren(foundedParents);
            string serializedTree = xmlRead.SerializeNode(root, specialTokens);
            xmlRead.StringToXMLDocument(serializedTree, document);
            xmlRead.FilesInZip();
        }

    }
}
