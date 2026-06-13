using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser.SpecialClasses.Tree
{
    public class Stash
    {
        private const int MATCH_MIN_PERCENT = 75;
        private const string TABLE_NODE_NAME = "w:tbl";
        private TreeNode _root;

        public Dictionary<int, TreeNode> StashedParagraphs {  get; private set; } = new Dictionary<int, TreeNode>();
        public Dictionary<int, TreeNode> StashedTables { get; private set; } = new Dictionary<int, TreeNode>();

        public Stash(TreeNode root)
        {
            _root = root;
        }

        public void UnStashPages()
        {
            Queue<TreeNode> paragraphs = new Queue<TreeNode>(_root.Children);
            Queue<TreeNode> newParagraphStructure = new Queue<TreeNode>();

            int i = 0;

            while (paragraphs.Count > 0 || StashedParagraphs.Count > 0)
            {
                if (StashedParagraphs.TryGetValue(i, out TreeNode stashedNode))
                {
                    newParagraphStructure.Enqueue(stashedNode);
                    StashedParagraphs.Remove(i);
                }
                else if (paragraphs.Count > 0)
                {
                    newParagraphStructure.Enqueue(paragraphs.Dequeue());
                }
                else
                {
                    break;
                }

                i++;
            }

            _root.Children.Clear();

            while (newParagraphStructure.Count > 0)
            {
                _root.Children.Add(newParagraphStructure.Dequeue());
            }
        }

        public void StashPages(Dictionary<int, List<string>> pages, int[] targetPages)
        {
            Dictionary<int, TreeNode> stash = new Dictionary<int, TreeNode>();
            int lastParagraph = 0;
            List<List<int>> startEndPages = new List<List<int>>();
            List<List<int>> pagesNumbersSequences = DetectSequences(pages.Keys.ToList());
            List<Dictionary<int, List<string>>> pagesSequences = new List<Dictionary<int, List<string>>>();

            foreach (var numberSequence in pagesNumbersSequences)
            {
                Dictionary<int, List<string>> sequence = new Dictionary<int, List<string>>();
                foreach (var number in numberSequence)
                {
                    sequence.Add(number, pages[number]);
                }
                pagesSequences.Add(sequence);
            }

            for(int i = 0; i < pagesNumbersSequences.Count; i++)
            {
                if (!pagesNumbersSequences[i].Any(p => targetPages.Contains(p)))
                    continue;

                List<int> startEnd = DetectPage(pagesSequences[i], pagesNumbersSequences[i], targetPages);
                if (startEnd[0] == -1 && startEnd[1] == -1)
                {
                    continue;
                }
                startEndPages.Add(startEnd);
                lastParagraph = startEnd[1];
            }

            for (int i = startEndPages.Count - 1; i >= 0 ; i --)
            {
                for (int j = startEndPages[i][1]; j >= startEndPages[i][0]; j--)
                {
                    TreeNode child = _root.Children[j];
                    stash.Add(j, child.Clone());
                    _root.Children.RemoveAt(j);
                }
            }

            StashedParagraphs = stash;
        }

        public void UnStashTables()
        {
            List<TreeNode> tables = _root.LongBreadthFirstSearch(TABLE_NODE_NAME);
            for(int i = 0; i < tables.Count; i++)
            {
                if (StashedTables.ContainsKey(i))
                {
                    tables[i].Children = StashedTables[i].Children;
                    StashedTables.Remove(i);
                }
            }
        }

        public void StashTables()
        {
            Dictionary<int, TreeNode> stash = new Dictionary<int, TreeNode>();

            List<TreeNode> tables = _root.LongBreadthFirstSearch(TABLE_NODE_NAME);

            for (int i = 0; i < tables.Count; i++)
            {
                TreeNode childrenHolder = new TreeNode
                {
                    TagName = "holder",
                    CloseTag = true,
                    Children = tables[i].Clone().Children
                };

                stash.Add(i, childrenHolder);
                tables[i].Children.Clear();
            }

            StashedTables = stash;
        }

        public List<List<int>> DetectSequences(List<int> pages)
        {
            List<List<int>> sequences = new();

            if (pages == null || pages.Count == 0)
                return sequences;

            pages.Sort();
            List<int> sequence = new List<int> { pages[0] };

            for (int i = 1; i < pages.Count; i++)
            {
                if (pages[i] == pages[i - 1] + 1)
                {
                    sequence.Add(pages[i]);
                }
                else
                {
                    sequences.Add(new List<int>(sequence));
                    sequence.Clear();
                    sequence.Add(pages[i]);
                }
            }
            sequences.Add(sequence);
            return sequences;
        }

        public List<int> DetectPage(Dictionary<int, List<string>> pagesWords, List<int> pagesSequence, int[] targetPages)
        {
            List<int> neighbourPages = DetectNeighbourPages(pagesSequence, targetPages);
            int startIgnoreIndex;
            int lastIgnoreIndex;

            if (neighbourPages[0] == -1)
            {
                startIgnoreIndex = 0;
            }
            else
            {
                startIgnoreIndex = GetLastIndexNeighbourPage(pagesWords[neighbourPages[0]]) + 1;
            }

            if (neighbourPages[1] == -1)
            {
                lastIgnoreIndex = _root.Children.Count - 1;
            }
            else
            {
                int lastParagraph = GetStartIndexNeighbourPage(pagesWords[neighbourPages[1]]);
                lastIgnoreIndex = GetFirstParagraphWithText(lastParagraph);// w:p где есть w:r
            }

            return new List<int> { startIgnoreIndex, lastIgnoreIndex};

        }

        private int GetFirstParagraphWithText(int lastParagraph)
        {
            for (int i = lastParagraph - 1; i >= 0; i--)
            {
                TreeNode textRun = _root.Children[i].LongBreadthFirstSearch("w:r").FirstOrDefault();
                TreeNode sectPr = _root.Children[i].LongBreadthFirstSearch("w:sectPr").FirstOrDefault();
                if (textRun != null)
                {
                    return i;
                }
                else if (sectPr != null)
                {
                    return i;
                }
            }
            return 0;
        }

        private int GetStartIndexNeighbourPage(List<string> pageWords)
        {
            List<int> ignore = new List<int>();
            int pageIndex = 0;

            for (int i = 0; i < _root.Children.Count; i++)
            {
                List<string> paragraphWords = GetParagraphWords(_root.Children[i]);
                bool match = MatchTokensSequence(paragraphWords, pageWords, 0, out double matchPercent, out int lastPageIndex);
                if (match)
                {
                    ignore.Add(i);
                    pageIndex = lastPageIndex;
                }
            }
            List<int> trueIgnore = GetBiggestClaster(ignore);

            if (trueIgnore.Count == 0) return -1;

            return trueIgnore.Min();
        }

        private int GetLastIndexNeighbourPage(List<string> pageWords)
        {
            List<int> ignore = new List<int>();
            int pageIndex = 0;

            for (int i = 0; i < _root.Children.Count; i++)
            {
                List<string> paragraphWords = GetParagraphWords(_root.Children[i]);
                bool match = MatchTokensSequence(paragraphWords, pageWords, 0, out double matchPercent, out int lastPageIndex);
                if (match)
                {
                    ignore.Add(i);
                    pageIndex = lastPageIndex;
                }
            }
            List<int> trueIgnore = GetBiggestClaster(ignore);

            if (trueIgnore.Count == 0) return -1;

            return trueIgnore.Max();
        }

        private List<int> GetBiggestClaster(List<int> sequence)
        {
            int maxGap = 10;
            int lastIndex = 0;

            List<List<int>> clasters = new List<List<int>>();

            for (int i = 0; i < sequence.Count; i++)
            {
                if(i + 1 < sequence.Count)
                {
                    if (sequence[i + 1] - sequence[i] >= maxGap)
                    {
                        List<int> claster = new List<int>();
                        for (int j = lastIndex; j <= i; j++)
                        {
                            claster.Add(sequence[j]);
                        }
                        clasters.Add(claster);
                        lastIndex = i + 1;
                    }
                }
            }
            List<int> lastClaster = new List<int>();
            for (int i = lastIndex; i < sequence.Count; i++)
            {
                lastClaster.Add(sequence[i]);
            }
            clasters.Add(lastClaster);

            List<int> biggestClaster = new List<int>();
            foreach(var claster in clasters)
            {
                if(biggestClaster.Count < claster.Count)
                {
                    biggestClaster = claster;
                }
            }
            return biggestClaster;
        }

        private List<int> DetectNeighbourPages(List<int> sequence, int[] targetPages)
        {
            if (targetPages == null || targetPages.Length == 0)
                return new List<int> { -1, -1 };

            List<int> neighbours = sequence.Except(targetPages.ToList()).ToList();

            if (neighbours.Count == 1)
            {
                if (neighbours[0] > targetPages[0])
                {
                    int n = neighbours[0];
                    neighbours = new List<int> { -1, n };
                }
                else if (neighbours[0] < targetPages[targetPages.Length - 1])
                {
                    int n = neighbours[0];
                    neighbours = new List<int> { n, -1 };
                }
            }
            return neighbours;
        }

        private List<string> GetParagraphWords(TreeNode paragraph)
        {
            List<string> paragraphWords = new List<string>();
            List<TreeNode> textNodes = paragraph.LongBreadthFirstSearch("w:t");
            string nodeText = "";
            foreach (TreeNode node in textNodes)
            {
                if (node.Values.Count > 0)
                {
                    nodeText += node.Values[0];
                }
            }
            paragraphWords = nodeText.Split(" ").ToList();
            return paragraphWords;
        }

        public bool MatchTokensSequence(List<string> paragraphWords, List<string> pageWords, int startPageIndex, out double matchPercent,out int lastPageIndex)
        {
            matchPercent = 0;
            int matchCount = 0;

            int paragraphWordIndex = 0;
            int pageWordIndex = startPageIndex;
            int lastMatchIndex = 0;

            if (paragraphWords.Count == 0)
            {
                lastPageIndex = -1;
                matchPercent = 0;
                return false;
            }

            while (pageWordIndex < pageWords.Count && paragraphWordIndex < paragraphWords.Count)
            {
                if (paragraphWords[paragraphWordIndex] == pageWords[pageWordIndex])
                {
                    pageWordIndex++;
                    paragraphWordIndex++;
                    matchCount++;
                    lastMatchIndex = pageWordIndex;
                }
                else
                {
                    pageWordIndex++;
                }
            }
            lastPageIndex = Math.Max(0, lastMatchIndex - 2);
            matchPercent = (double)matchCount / paragraphWords.Count * 100;
            return matchPercent >= MATCH_MIN_PERCENT;
        }

        public int FindLastElementIndexOfPages(Dictionary<int, List<string>> pagesWords, int[] targetPages)
        {
            if (targetPages == null || targetPages.Length == 0)
                return -1;

            int lastTargetPage = targetPages.Max();

            if (!pagesWords.TryGetValue(lastTargetPage, out var pageWords) || pageWords.Count == 0)
                return -1;

            // 1. Создаем "плоский" поток слов документа: Слово + Индекс его корневого узла
            var docStream = new List<(string Word, int NodeIndex)>();
            for (int i = 0; i < _root.Children.Count; i++)
            {
                var nodeWords = GetNodeWords(_root.Children[i]);
                foreach (var w in nodeWords)
                {
                    if (!string.IsNullOrWhiteSpace(w))
                    {
                        // Нормализуем слово для защиты от опечаток парсера
                        docStream.Add((NormalizeWord(w), i));
                    }
                }
            }

            if (docStream.Count == 0) return -1;

            // Нормализуем слова искомой страницы
            var targetWords = pageWords
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Select(NormalizeWord)
                .ToList();

            // 2. Ищем "якорь" с конца страницы
            // Берем блок из 20 слов. Если не совпало (например, из-за колонтитулов в PDF),
            // отступаем на 10 слов назад и пробуем снова.
            int chunkSize = Math.Min(20, targetWords.Count);
            int step = 10;

            for (int offset = 0; offset <= targetWords.Count - chunkSize; offset += step)
            {
                var chunk = targetWords
                    .Skip(targetWords.Count - chunkSize - offset)
                    .Take(chunkSize)
                    .ToList();

                int matchedNodeIndex = FindChunkInStream(docStream, chunk);

                if (matchedNodeIndex != -1)
                {
                    return matchedNodeIndex;
                }
            }

            return -1;
        }

        private int FindChunkInStream(List<(string Word, int NodeIndex)> docStream, List<string> chunk)
        {
            // Требуем совпадения хотя бы 80% слов в окне (защита от мусора)
            int requiredMatches = (int)(chunk.Count * 0.80);

            for (int i = 0; i <= docStream.Count - chunk.Count; i++)
            {
                int matches = 0;
                int lastNodeIndex = -1;

                // Накладываем наш chunk на текущий отрезок документа
                for (int j = 0; j < chunk.Count; j++)
                {
                    if (docStream[i + j].Word == chunk[j])
                    {
                        matches++;
                        // Запоминаем узел последнего совпавшего слова
                        lastNodeIndex = docStream[i + j].NodeIndex;
                    }
                }

                // Если нашли сильное совпадение
                if (matches >= requiredMatches)
                {
                    // Возвращаем индекс корневого узла (будь то абзац или целая таблица), 
                    // в котором оказалось последнее слово этого блока
                    return lastNodeIndex;
                }
            }
            return -1;
        }

        private string NormalizeWord(string word)
        {
            // Убираем все знаки препинания и приводим к нижнему регистру.
            // Это решает проблему, когда в PDF "слово.", а в Word "слово"
            return new string(word.Where(c => !char.IsPunctuation(c)).ToArray()).ToLowerInvariant();
        }

        private List<string> GetNodeWords(TreeNode node)
        {
            List<TreeNode> textNodes = node.LongBreadthFirstSearch("w:t");

            StringBuilder sb = new StringBuilder();

            foreach (var t in textNodes)
            {
                if (t.Values.Count > 0) sb.Append(t.Values[0]).Append(' ');
            }



            return sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

        }
    }
}
