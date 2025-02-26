using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Linq.Expressions;
using static System.Net.Mime.MediaTypeNames;
using System.Xml;

namespace XMLParser
{
    public class XMLRead
    {
        private readonly string tempReadPath = "C:\\Лабы\\AppTestDocx\\5 Лаба.docx";
        private readonly string tempWritePath = "C:\\Лабы\\AppTestDocx\\Arhive";
        private string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        private readonly string _readPath;
        private readonly string _writePath;
        private readonly string _tempFolder;

        public XMLRead()
        {
            _readPath = tempReadPath;
            _writePath = tempWritePath;
            _tempFolder = tempFolder;
            try
            {
                UnZipDocx();
                var (fileInTockens, specialTokens) = Tokenize(XMLDocumentFileToString());
                List<TreeNode> parents = new List<TreeNode>();
                //for (int i = 0; i < fileInTockens.Count; i++)
                //{
                //    Console.WriteLine(fileInTockens[i]);
                //}
                TreeNode root = new TreeNode();
                root = root.BuildTree(fileInTockens);
                parents = root.BreadthFirstSearch(root, "w:rPr");
                root.TerminateChildren(parents, "w:rPr");
                string serializedTree = SerializeNode(root, specialTokens);
                StringToXMLDocument(serializedTree);
                FilesInZip();
                //root.PrintTree(root);
            }
            finally
            {
                Directory.Delete(_tempFolder, true);
            }
            
        }

        public string SerializeNode(TreeNode treeNode, List<string> specialTokens = null)
        {
            string node ="";
            if (specialTokens != null)
            {
                foreach (var token in specialTokens)
                {
                    node += token;
                }
            }


            // Создание открывающего тега
            node = "<" + treeNode.TagName;
            foreach (var attribute in treeNode.Attributes)
            {
                node += " " + attribute.Key + "=" + $"\"{attribute.Value}\"";
            }

            //Закрытие открывающего тега
            if (treeNode.CloseTag == true)
            {
                node += ">";
                //Дабавление значений
                foreach (string value in treeNode.Values)
                {
                    node += value;
                }
                //Сериализация потомков
                foreach (var child in treeNode.Children)
                {
                    node = node + SerializeNode(child);
                }
                node = node + $"</{treeNode.TagName}>";

            }
            else
            {
                node += "/>";
            }

            return node;
        }

        public (List<string> tokens, List<string> specialTokens) Tokenize(string file)
        {
            List<string> tokens = new List<string>();
            List<string> specialTokens = new List<string>();

            for (int i = 0; i < file.Length; i++)
            {
                if(file[i] == '<') //поиск тега
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

        private void StringToXMLDocument(string text)
        {
            string doc = _tempFolder + "\\word\\document.xml";
            File.WriteAllText(doc, text);
        }

        private string XMLDocumentFileToString()
        {
            string doc = _tempFolder + "\\word\\document.xml";
            string list = File.ReadAllText(doc);
            return list;
        }

        private void UnZipDocx()
        {
            Directory.CreateDirectory(_tempFolder);
            ZipFile.ExtractToDirectory(_readPath, _tempFolder);
        }

        private void FilesInZip()
        {
            string savePath = _readPath.Replace("5 Лаба.docx", "5 Лаба1.docx");
            ZipFile.CreateFromDirectory(_tempFolder, savePath);
        }
    }
}
