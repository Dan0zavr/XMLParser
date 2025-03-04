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

        public TreeNode CreateTextStyleNode(List<TreeNode> styleChildren, TreeNode root, string tag)
        {
            string styleName = EnsureUniqueStyleName(root, tag);
            TreeNode styleIdAndName = new TreeNode()
            {
                TagName = $"w:name",
                Attributes = { {"w:val", styleName} },
                CloseTag = false
            };

            TreeNode rPr = new TreeNode()
            {
                TagName = "w:rPr",
                Children = styleChildren,
                CloseTag = true
            };

            List<TreeNode> textParent = new List<TreeNode>();
            List<TreeNode> name = new List<TreeNode>();
            textParent.Add(rPr);
            name.Add(styleIdAndName);

            TreeNode styleNode = new TreeNode()
            {
                TagName = "w:style",
                Attributes = 
                { 
                    {"w:type", "character" },
                    {"w:styleId", styleName }
                },
                Children = name.Union(textParent).ToList(),
                CloseTag = true
            };

            return styleNode;
        }

        public void InroduceStyleInTree(TreeNode stylesNodeParent, TreeNode styleChilld)
        {
             AddChild(stylesNodeParent, styleChilld);
        }

        private string EnsureUniqueStyleName(TreeNode root, string tag)
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
