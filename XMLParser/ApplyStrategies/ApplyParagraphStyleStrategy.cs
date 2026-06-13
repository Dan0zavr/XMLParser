using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.SpecialClasses.Tree;

namespace XMLParser.ApplyStrategies
{
    public class ApplyParagraphStyleStrategy : ApplyStrategy
    {
        private TreeNode _numberingRoot;
        public ApplyParagraphStyleStrategy(TreeNode numberingRoot)
        {
            _numberingRoot = numberingRoot;
        }

        public override void Apply(TreeNode root, TreeNode style)
        {
            string styleTagName = "w:pStyle";
            string tagName = "w:pPr";
            string styleName = style.Attributes["w:styleId"];
            Stash stash = new Stash(root);

            TreeNode applyStyle = CreateStyleNode(styleTagName, styleName);

            ApplyStylesToNodes(root, tagName, applyStyle);

            //ApplyParagraphStyleToNumbering(root, applyStyle);

        }

        private void ApplyParagraphStyleToNumbering(TreeNode root, TreeNode style)
        {
            List<TreeNode> paragraphs = root.LongBreadthFirstSearch("w:p");
            foreach (TreeNode paragraph in paragraphs)
            {
                if (paragraph.ContainsChild("w:numPr"))
                {
                    ApplyParagraphInNumberingDocument(_numberingRoot, paragraph, style);
                }
            }
        }

        private void ApplyParagraphInNumberingDocument(TreeNode numberingRoot, TreeNode paragraph, TreeNode style)
        {
            string lvl = paragraph.QuikBreadthFirstSearch("w:ilvl").First().Attributes["w:val"];
            string numId = paragraph.QuikBreadthFirstSearch("w:numId").First().Attributes["w:val"];

            string abstractNumId = numberingRoot.SearchByAttributeValue("w:num", "w:numId", numId).Children.First().Attributes["w:val"];

            TreeNode abstructNumNode = numberingRoot.SearchByAttributeValue("w:abstractNum", "w:abstractNumId", abstractNumId);
            TreeNode lvlNode = abstructNumNode.SearchByAttributeValue("w:lvl", "w:ilvl", lvl);

            TreeNode paragraphStyleNode = lvlNode.QuikBreadthFirstSearch("w:pPr").First();
            paragraphStyleNode.Children.Clear();
            paragraphStyleNode.Children.Add(style);
        }
    }
}
