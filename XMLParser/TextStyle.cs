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
                    {"w:styleId", $"{EnsureUniqueStyleName}" }
                },
                Children = styleChildren,
                CloseTag = true
            };

            

            return styleNode;
        }

        public void InroduceStyleInTree(TreeNode stylesNodeParent, TreeNode styleChilld)
        {
             AddChild(stylesNodeParent, styleChilld);
        }

        public string EnsureUniqueStyleName(TreeNode root, string tag)
        {
            string baseName = "WordRegStyle"; // Базовое имя стиля
            string styleName = baseName; // Начинаем с базового имени
            int counter = 1; // Счетчик для добавления суффикса

            // Поиск всех узлов с указанным тегом
            List<TreeNode> styles = BreadthFirstSearch(root, tag);

            // Проверяем, используется ли имя стиля
            while (styles.Any(node => node.Attributes.ContainsValue(styleName)))
            {
                // Если имя уже используется, добавляем суффикс
                styleName = $"{baseName}_{counter++}";
            }

            return styleName; // Возвращаем уникальное имя
        }
    }
}
