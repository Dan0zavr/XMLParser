using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;

namespace XMLParser.ApplyStrategies
{
    public class ApplyTextStyleStrategy : ApplyStrategy
    {
        public override void Apply(TreeNode root,TreeNode style) //сделать игнорирование формул
        {
            string styleTagName = "w:rStyle";
            string tagName = "w:rPr";
            string styleName = style.Attributes["w:styleId"];

            TreeNode applyStyle = CreateStyleNode(styleTagName, styleName);

            ApplyStylesToNodes(root, tagName, applyStyle);
        }
    }
}
