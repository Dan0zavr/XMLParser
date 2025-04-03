using System.IO.Compression;

namespace XMLParser
{
    public class XMLRead
    {
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

        public void StringToXMLDocument(string text, string fileName, string tempFolder)
        {
            string doc = tempFolder + $"\\word\\{fileName}";
            File.WriteAllText(doc, text);
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

        public void FilesInZip(string readPath, string tempFolder, string oldFileName, string savePath)
        {
            string fileNameDocx = EnsureUniqueFileName(ExtractExtension(oldFileName) + "_new" + ".docx", savePath);
            string fileNameZip = fileNameDocx.Replace(".docx", ".zip");
            string savePathWithFileDocx = savePath + "\\" + fileNameDocx;
            string saxePathWithFileZip = savePath + "\\" + fileNameZip;
            ZipFile.CreateFromDirectory(tempFolder, savePathWithFileDocx);
            ZipFile.CreateFromDirectory(tempFolder, saxePathWithFileZip);
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
    }
}
