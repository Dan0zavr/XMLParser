using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;
using static XMLParser.TreeNode;

namespace XMLParser
{
    public static class Cleaner
    {
        private const string rPrTagName = "w:rPr";
        private const string pPrTagName = "w:pPr";
        private const string numPrTagName = "w:numPr";
        private const string tblPrTagName = "w:tblPr";
        private const string tablecellTagName = "w:tc";
        private const string runBlockTagName = "w:r";

        public static TreeNode CleanHandStyles(TreeNode root, Template template, List<string> specialTokens, string savePath)
        {
            List<TreeNode> foundedParents = new List<TreeNode>();

            foundedParents = root.QuikBreadthFirstSearch(rPrTagName);
            root.TerminateChildren(foundedParents);
            foundedParents = root.QuikBreadthFirstSearch(pPrTagName);
            root.TerminateChildren(foundedParents);

            if (template.NumberingStyle != null) {
                foundedParents = root.QuikBreadthFirstSearch(numPrTagName);
                root.TerminateChildren(foundedParents);
            }

            if (template.TableStyle != null) {
                foundedParents = root.QuikBreadthFirstSearch(tblPrTagName);
                root.TerminateChildren(foundedParents);
            }
            FillMissingTags(runBlockTagName, root);

            return root;
        }

        public static TreeNode CleanHandTableStyle(TreeNode root)
        {
            List<TreeNode> foundedCells = new List<TreeNode>();

            foundedCells = root.LongBreadthFirstSearch(tablecellTagName);

            foreach (var cell in foundedCells)
            {
                List<TreeNode> foundedParents = new List<TreeNode>();

                foundedParents = cell.QuikBreadthFirstSearch(rPrTagName);
                root.TerminateChildren(foundedParents);
                foundedParents = cell.QuikBreadthFirstSearch(pPrTagName);
                root.TerminateChildren(foundedParents);
            }

            return root;
        }

        public static void FillMissingTags(string parentTagName, TreeNode root)
        {
            List<TreeNode> runBlocks = root.LongBreadthFirstSearch(parentTagName);
            foreach (var block in runBlocks)
            {
                if (!block.Children.Any(child => child.TagName == rPrTagName))
                {
                    List<TreeNode> children = block.Children;
                    block.Children.Clear();
                    block.Children.Add(CreateRPrTag());
                    block.Children.AddRange(children);
                }
            }
        }

        private static TreeNode CreateRPrTag()
        {
            return new TreeNode()
            {
                TagName = rPrTagName,
                CloseTag = true,
                Attributes = new Dictionary<string, string>(),
                Values = new List<string>(),
                Children = new List<TreeNode>()
            };
        }

    }
}
