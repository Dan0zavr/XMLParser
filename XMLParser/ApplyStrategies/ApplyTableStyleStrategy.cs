using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static XMLParser.TreeNode;

namespace XMLParser.ApplyStrategies
{
    public class ApplyTableStyleStrategy : ApplyStrategy
    {
        public override void Apply(TreeNode root, TreeNode style)
        {
            string styleTagName = "w:tblStyle";
            string tagName = "w:tblPr";
            string styleName = style.Attributes["w:styleId"];

            TreeNode applyStyle = CreateStyleNode(styleTagName, styleName);

            ApplyStylesToNodes(root, tagName, applyStyle);
        }
    }
}
