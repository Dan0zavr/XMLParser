using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.SpecialClasses.Tree;
using XMLParser.Styles;
using static XMLParser.SpecialClasses.Tree.TreeNode;

namespace XMLParser.ApplyStrategies
{
    public class ApplyTableTextStyleStrategy : ApplyStrategy
    {
        public override void Apply(TreeNode root, TreeNode style)
        {
            string styleTagName = "w:rStyle";
            string tagName = "w:rPr";

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
