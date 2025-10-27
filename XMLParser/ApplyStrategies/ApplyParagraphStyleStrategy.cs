using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser.ApplyStrategies
{
    public class ApplyParagraphStyleStrategy : ApplyStrategy
    {
        public override void Apply(TreeNode root, TreeNode style)
        {
            string styleTagName = "w:pStyle";
            string tagName = "w:pPr";
            string styleName = style.Attributes["w:styleId"];

            TreeNode applyStyle = CreateStyleNode(styleTagName, styleName);

            ApplyStylesToNodes(root, tagName, applyStyle);
        }
    }
}
