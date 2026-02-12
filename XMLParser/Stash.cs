using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace XMLParser
{
    public class Stash
    {
        private const int MATCH_MIN_PERCENT = 75;
        private TreeNode _root;

        public Dictionary<int, TreeNode> StashedParagraphs {  get; private set; }

        public Stash(TreeNode root)
        {
            _root = root;
        }

        public void UnStash()
        {
            Queue<TreeNode> paragraphs = new Queue<TreeNode>(_root.Children);
            Queue<TreeNode> newParagraphStructure = new Queue<TreeNode>();

            for (int i = 0; i < paragraphs.Count; i++)
            {
                if (StashedParagraphs.ContainsKey(i))
                {
                    newParagraphStructure.Enqueue(StashedParagraphs[i]);
                    StashedParagraphs.Remove(i);
                }
                else
                {
                    newParagraphStructure.Enqueue(paragraphs.Dequeue());
                }
            }
            _root.Children.Clear();

            while (newParagraphStructure.Count > 0)
            {
                _root.Children.Add(newParagraphStructure.Dequeue());
            }
        }

        public void StashPages(Dictionary<int, List<string>> pages)
        {
            Dictionary<int, TreeNode> stash = new Dictionary<int, TreeNode>();
            int lastParagraph = 0;
            List<List<int>> startEndPages = new List<List<int>>();
            foreach (var page in pages)
            {
                List<int> startEnd = DetectPage(page.Value, lastParagraph);
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
                    stash.Add(j, child);
                    _root.Children.RemoveAt(j);
                }
            }

            StashedParagraphs = stash;
        }

        private List<int> DetectPage(List<string> pageWords, int lastParagraph)
        {
            List<int> ignore = new List<int>();
            List<TreeNode> rootChildren = _root.Children;
            int pageIndex = 0;
            for (int i = lastParagraph; i < rootChildren.Count; i++)
            {
                List<string> paragraphWords = GetParagraphWords(rootChildren[i]);
                bool match = MatchTokensSequence(paragraphWords, pageWords, pageIndex, out double matchPercent, out int lastPageIndex);
                if (match)
                {
                    ignore.Add(i);
                    pageIndex = lastPageIndex;
                }
            }
            if (ignore.Count == 0)
            {
                return new List<int> { -1,  -1 };
            }
            return new List<int> { ignore.Min(), ignore.Max() };
        }

        private List<string> GetParagraphWords(TreeNode paragraph)
        {
            List<string> paragraphWords = new List<string>();
            List<TreeNode> textNodes = paragraph.LongBreadthFirstSearch("w:t");
            string nodeText = "";
            foreach (TreeNode node in textNodes)
            {
                nodeText += node.Values[0];
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
            lastPageIndex = lastMatchIndex - 2;
            matchPercent = (double)matchCount / paragraphWords.Count * 100;
            return matchPercent >= MATCH_MIN_PERCENT;
        }
    }
}
