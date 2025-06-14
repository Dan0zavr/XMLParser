using System.IO.Compression;
using XMLParser.Styles;

namespace XMLParser
{
    public class XMLRead
    {
        public (TreeNode root, List<string> specialTokens) ReadXMLDocument(string readPath, string fileName, string tempFolder)
        {
            TreeNode root = new TreeNode();
            var (fileInTockens, specialTokens) = Tokenize(XMLDocumentFileToString(fileName, tempFolder));
            root = root.BuildTree(fileInTockens);
            return (root, specialTokens);
        }

        public (List<string> tokens, List<string> specialTokens) Tokenize(string file)
        {
            List<string> tokens = new List<string>();
            List<string> specialTokens = new List<string>();

            for (int i = 0; i < file.Length; i++)
            {
                if (file[i] == '<') //поиск тега
                {
                    int end = file.IndexOf('>', i);
                    if (end == -1) throw new Exception("Некорректный XML: незакрытый тег.");
                    string token = file.Substring(i, end - i + 1);
                    if (token.StartsWith("<?"))
                    {
                        specialTokens.Add(token);
                    }
                    else
                    {
                        tokens.Add(token);
                    }
                    i = end;

                }
                else //поиск значения
                {
                    int end = file.IndexOf('<', i);
                    if (end == -1) end = file.Length;

                    string text = file.Substring(i, end - i);
                    if (!string.IsNullOrEmpty(text))
                    {
                        tokens.Add(text);
                    }
                    i = end - 1;
                }
            }

            return (tokens, specialTokens);
        }


        public string XMLDocumentFileToString(string endFile, string tempFolder)
        {
            string doc = tempFolder + $"\\word\\{endFile}";
            string list = File.ReadAllText(doc);
            return list;
        }

        public void UnZipDocx(string readPath, string tempFolder)
        {
            Directory.CreateDirectory(tempFolder);
            ZipFile.ExtractToDirectory(readPath, tempFolder);
        }
        
        
    }
}
