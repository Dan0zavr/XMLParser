using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;

namespace XMLParser
{
    public class XMLWrite
    {
        private const string styles = "styles.xml";


        public void StringToXMLDocument(string text, string fileName, string tempFolder)
        {
            string doc = tempFolder + $"\\word\\{fileName}";
            File.WriteAllText(doc, text);
        }

        public void FilesInZip(string tempFolder, string oldFileName, string savePath)
        {
            string fileNameDocx = EnsureUniqueFileName(ExtractExtension(oldFileName) + "_new" + ".docx", savePath);
            string fileNameZip = fileNameDocx.Replace(".docx", ".zip");
            string savePathWithFileDocx = savePath + "\\" + fileNameDocx;
            string savePathWithFileZip = savePath + "\\" + fileNameZip;
            ZipFile.CreateFromDirectory(tempFolder, savePathWithFileDocx);
            ZipFile.CreateFromDirectory(tempFolder, savePathWithFileZip);
        }

        private string EnsureUniqueFileName(string fileName, string savePath)
        {
            string newFileName = ExtractExtension(fileName);
            int counter = 1;
            while (File.Exists(savePath + "\\" + newFileName + ".docx"))
            {
                newFileName = newFileName + counter;
                counter++;
            }
            newFileName += ".docx";
            return newFileName;
        }
        private string ExtractExtension(string fileName)
        {
            string newFileName;
            if (fileName.Contains("."))
            {
                newFileName = fileName.Remove(fileName.IndexOf('.'));
                return newFileName;
            }
            return fileName;
        }

        public void SerializeStyle(TreeNode root, List<string> specialTokens, string tempFolder)
        {
            string serializedTree = SerializeNode(root, specialTokens);
            StringToXMLDocument(serializedTree, styles, tempFolder);
        }

        public string SerializeNode(TreeNode treeNode, List<string> specialTokens = null)
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
