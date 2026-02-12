using System.IO.Compression;
using XMLParser.Styles;
using static XMLParser.Tokenizator;

namespace XMLParser
{
    public static class XMLRead
    {
        public static (TreeNode root, List<string> specialTokens) ReadXMLDocument(string fileName, string tempFolder)
        {
            TreeNode root = new TreeNode();
            var (fileInTockens, specialTokens) = Tokenize(XMLDocumentFileToString(fileName, tempFolder));
            root = root.BuildTree(fileInTockens);
            return (root, specialTokens);
        }

        public static string XMLDocumentFileToString(string endFile, string tempFolder)
        {
            string doc = Path.Combine(tempFolder, "word", endFile);
            string list = File.ReadAllText(doc);
            return list;
        }

        public static void UnZipDocx(string readPath, string tempFolder)
        {
             ZipFile.ExtractToDirectory(readPath, tempFolder);
        }
        
        
    }
}
