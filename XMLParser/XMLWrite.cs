using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;

namespace XMLParser
{
    public static class XMLWrite
    {
        public static void StringToXMLDocument(string text, string fileName, string tempFolder)
        {
            string doc = Path.Combine(tempFolder, "word", fileName);
            File.WriteAllText(doc, text);
        }

        public static string FilesInZip(string tempFolder, string oldFileName, string savePath)
        {
            string fileNameDocx = EnsureUniqueFileName(Path.GetFileNameWithoutExtension(oldFileName) + "_new" + ".docx", savePath);
            string savePathWithFileDocx = Path.Combine(savePath, fileNameDocx);
            ZipFile.CreateFromDirectory(tempFolder, savePathWithFileDocx);
            return savePathWithFileDocx;
        }

        public static string EnsureUniqueFileName(string fileName, string savePath)
        {
            string newFileName = Path.GetFileNameWithoutExtension(fileName);
            int counter = 1;
            while (File.Exists(Path.Combine(savePath, newFileName + ".docx")))
            {
                newFileName = Path.GetFileNameWithoutExtension(fileName) + counter;
                counter++;
            }
            newFileName = newFileName + ".docx";
            return newFileName;
        }

        public static void TreeToXMLDocument(TreeNode root, List<string> specialTokens, string docName, string tempFolder)
        {
            string serializedTree = SerializeNode(root, specialTokens);
            StringToXMLDocument(serializedTree, docName, tempFolder);
        }

        public static string SerializeNode(TreeNode treeNode, List<string> specialTokens = null)
        {
            string node = "";
            if (specialTokens != null)
            {
                foreach (var token in specialTokens)
                {
                    node += token;
                }
            }

            // Создание открывающего тега
            node += "<" + treeNode.TagName;
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
    }
}
