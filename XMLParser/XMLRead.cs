using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Linq.Expressions;

namespace XMLParser
{
    public class XMLRead
    {
        private readonly string tempReadPath = "C:\\Лабы\\AppTestDocx\\5 Лаба.docx";
        private readonly string tempWritePath = "C:\\Лабы\\AppTestDocx\\Arhive";
        private string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        private List<string> file;

        private readonly string _readPath;
        private readonly string _writePath;
        private readonly string _tempFolder;

        public XMLRead()
        {
            _readPath = tempReadPath;
            _writePath = tempWritePath;
            _tempFolder = tempFolder;
            UnZipDocx();
            file = SeparateXML(XMLDocumentFileToString());
            StringToXMLDocument(DeleteExcess (file, SearchPageBreak(file)));
            FilesInZip();
        }

        private string testString = "<></> <w:rPr></w:rPr><w:rPr> something!</w:rPr> ebg";

        private string DeleteExcess(List<string> separatedString, int start)
        {
            List<string> separatedStringCopy = separatedString;

            for (int i = start; i < separatedString.Count; i++) 
            { 
                if (separatedString[i][0] == '<')
                {
                    string ethalon = "<w:rPr>";
                    for (int j = 0; j < 7; j++)
                    {
                        if (separatedString[i][j] == ethalon[j])
                        {
                            if (separatedString[i][j] == '>')
                            {
                                separatedStringCopy.Remove(separatedString[i]);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            return ArrayToString(separatedStringCopy);

        }

        private int SearchPageBreak(List<string> strings)
        {
            for (int i = 0; i < strings.Count; i++)
            {
                if (strings[i][0] == '<')
                {
                    string ethalon = "<w:lastRenderedPageBreak/>";
                    if (strings[i].Length >= ethalon.Length) {
                        if (strings[i].Substring(0, ethalon.Length) == ethalon)
                        {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }


        private string ArrayToString(List<string> strings)
        {
            string newData = string.Empty;

            foreach (string word in strings)
            {
                newData = newData + word;
            }
            
            return newData;
        }

        private List<string> SeparateXML(string fileInString)
        {
            List<string> textInBlocks = new List<string>();
            int? startIndexToDelete;
            int? endIndexToDelete = 0;
            string finalXML = string.Empty;

            // Проход по документу
            for(int i = 0; i < fileInString.Length; i++)
            {
                //Поиск открывающего тега
                if (fileInString[i] == '<')
                {
                    startIndexToDelete = i;
                    string startHypothesis = "";
                    while (fileInString[i] != '>')
                    {
                        startHypothesis += fileInString[i];
                        i++;
                    }
                    //Если открывающий тег найден, ищем закрывающий
                    if (SearchStartIndexToDelete(startHypothesis + '>') == true)
                    {
                        if (textInBlocks.Count == 0)
                        {
                            textInBlocks.Add(fileInString.Substring(0, Convert.ToInt32(startIndexToDelete)));
                        }
                        else
                        {
                            textInBlocks.Add(fileInString.Substring(Convert.ToInt32(endIndexToDelete + 1), Convert.ToInt32(startIndexToDelete) - Convert.ToInt32(endIndexToDelete)-1));
                           
                        }

                        for(int j = i; j < fileInString.Length; j++)
                        {
                            if (fileInString[j] == '<')
                            {
                                string endHypothesis = "";
                                while (fileInString[j] != '>')
                                {
                                    endHypothesis += fileInString[j];
                                    j++;
                                }
                                if(SearchEndIndexToDelete(endHypothesis + '>') == true)
                                {
                                    i = j;
                                    endIndexToDelete = i;
                                    int countToSeparate = Convert.ToInt32(endIndexToDelete) - Convert.ToInt32(startIndexToDelete) + 1;
                                    textInBlocks.Add(fileInString.Substring(Convert.ToInt32(startIndexToDelete), countToSeparate));
                                    break;
                                }
                            }
                        }
                    }


                }
            }
            textInBlocks.Add(fileInString.Substring(Convert.ToInt32(endIndexToDelete), fileInString.Length - Convert.ToInt32(endIndexToDelete)));
            return textInBlocks;
        }

        private bool SearchStartIndexToDelete(string hypotheticalString)
        {
            string openTeg = "<w:rPr>";
            if (hypotheticalString == openTeg) 
            { 
                return true; 
            }
            else
            {
                return false;
            }
        }

        private bool SearchEndIndexToDelete(string hypotheticalString)
        {
            string closeTeg = "</w:rPr>";

            if (hypotheticalString == closeTeg)
            {
                return true;
            }
            else
            {
                return false;
            }
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
