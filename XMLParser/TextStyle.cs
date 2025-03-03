using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser
{
    public class TextStyle : TreeNode
    {
        public string FontName { get; set; }
        public int FontSize { get; set; }

        const string fileName = "styles.xml";

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
                        styleNode.Attributes.Add("w:ascii", FontName);
                        styleNode.Attributes.Add("w:hAnsi", FontName);
                        styleNode.Attributes.Add("w:cs", FontName);
                        style.Add(styleNode);
                        break;

                    case "FontSize":
                        styleNode.TagName = "w:sz";
                        FontSize = FontSize * 2;
                        styleNode.Attributes.Add("w:val", FontSize.ToString());
                        style.Add(styleNode);
                        break;
                }
            }
            return style;
        }

        public TreeNode CreateTextStyleNode(List<TreeNode> styleChildren)
        {
            TreeNode styleNode = new TreeNode()
            {
                TagName = "w:style",
                Attributes = 
                { 
                    {"w:type", "character" },
                    {"w:styleId", "WordRegTextStyle" }
                },
                Children = styleChildren,
                CloseTag = true
            };

            

            return styleNode;
        }



        public void InroduceStyleInFile(TreeNode stylesNodeParent, TreeNode styleChilld)
        {
             AddChild(stylesNodeParent, styleChilld);
        }
    }
}
