using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;

namespace XMLParser
{
    public class TextStyle : TreeNode, IStyle
    {
        public string StyleType => "TextStyle";
        public string FontName { get; set; }
        public int FontSize { get; set; }

        public List<TreeNode> CreateTextStyle(TextStyle styleToTree) 
        { 
            List<TreeNode> style = new List<TreeNode>();
            foreach(var prop in typeof(TextStyle).GetProperties())
            {
                TreeNode styleNode = new TreeNode();
                switch (prop.Name) 
                {
                    case "FontName":
                        styleNode.TagName = "w:rFonts";
                        styleNode.Attributes.Add("w:ascii", styleToTree.FontName);
                        styleNode.Attributes.Add("w:hAnsi", styleToTree.FontName);
                        styleNode.Attributes.Add("w:cs", styleToTree.FontName);
                        style.Add(styleNode);
                        break;

                    case "FontSize":
                        styleNode.TagName = "w:sz";
                        styleToTree.FontSize = styleToTree.FontSize * 2;
                        styleNode.Attributes.Add("w:val", styleToTree.FontSize.ToString());
                        style.Add(styleNode);
                        break;
                }
            }
            return style;
        }
    }
}
