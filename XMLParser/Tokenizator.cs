using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser
{
    public static class Tokenizator
    {
        public static (List<string> tokens, List<string> specialTokens) Tokenize(string file)
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
    }
}
