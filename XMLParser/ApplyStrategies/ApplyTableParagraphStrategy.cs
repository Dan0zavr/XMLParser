using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;
using static XMLParser.TreeNode;

namespace XMLParser.ApplyStrategies
{
    public class ApplyTableParagraphStrategy : ApplyStrategy
    {
        public override void Apply(TreeNode root, TreeNode style)
        {
            string styleTagName = "w:pStyle";
            string tagName = "w:pPr";

            List<TreeNode> cells = root.LongBreadthFirstSearch("w:tc");

            foreach (TreeNode cell in cells)
            {
                string styleName = style.Attributes["w:styleId"];

                TreeNode applyStyle = CreateStyleNode(styleTagName, styleName);

                ApplyStylesToNodes(cell, tagName, applyStyle);
            }
        }
    }
}
