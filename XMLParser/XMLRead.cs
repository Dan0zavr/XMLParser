using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Linq.Expressions;
using static System.Net.Mime.MediaTypeNames;

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
                List<string> fileInTockens = Tokenize(XMLDocumentFileToString());
                foreach (string file in fileInTockens)
                {
                    Console.WriteLine(file);
                }
            }
            finally
            {
                Directory.Delete(_tempFolder, true);
            }
            
        }

        public List<string> Tokenize(string file)
        {
            List<string> tokens = new List<string>();

            for (int i = 0; i < file.Length; i++)
            {
                if(file[i] == '<') //поиск тега
                {
                    int end = file.IndexOf('>', i);
                    if (end == -1) throw new Exception("Некорректный XML: незакрытый тег.");
                    tokens.Add(file.Substring(i, end - i + 1));
                    i = end;
                }
                else //поиск значения
                {
                    int end = file.IndexOf('<', i);
                    if (end == -1) end = file.Length;

                    string text = file.Substring(i, end - i).Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        tokens.Add(text);
                    }
                    i = end - 1;
                }
            }

            return tokens;
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
            Directory.Delete(_tempFolder, true);
        }
    }
}
