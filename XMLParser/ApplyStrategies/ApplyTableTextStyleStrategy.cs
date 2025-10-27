using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;
using static XMLParser.TreeNode;

namespace XMLParser.ApplyStrategies
{
    public class ApplyTableTextStyleStrategy : ApplyStrategy
    {
        public override void Apply(TreeNode root, TreeNode style)
        {
            string styleTagName = "w:rStyle";
            string tagName = "w:rPr";

            List<TreeNode> cells = LongBreadthFirstSearch(root, "w:tc");

            foreach (TreeNode cell in cells)
            {
                string styleName = style.Attributes["w:styleId"];

                TreeNode applyStyle = CreateStyleNode(styleTagName, styleName);

                ApplyStylesToNodes(root, tagName, applyStyle);
            }
        }
    }
}
