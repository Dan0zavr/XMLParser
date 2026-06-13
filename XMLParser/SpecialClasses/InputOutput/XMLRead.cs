using System.IO.Compression;
using XMLParser.SpecialClasses.Tree;
using XMLParser.Styles;
using static XMLParser.SpecialClasses.Tree.Tokenizator;

namespace XMLParser.SpecialClasses.InputOutput
{
    public static class XMLRead
    {
        public static (TreeNode root, List<string> specialTokens) ReadXMLDocument(string path)
        {
            TreeNode root = new TreeNode();
            var (fileInTockens, specialTokens) = Tokenize(XMLDocumentFileToString(path));
            root = root.BuildTree(fileInTockens);
            return (root, specialTokens);
        }

        public static string XMLDocumentFileToString(string path)
        {
            string list = File.ReadAllText(path);
            return list;
        }

        public static void UnZipDocx(string readPath, string tempFolder)
        {
            try
            {
                ZipFile.ExtractToDirectory(readPath, tempFolder);
            }
            catch (Exception ex)
            {
                throw new Exception("Закройте документ");
            }
        }
        
        
    }
}
