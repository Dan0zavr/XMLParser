using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser
{
    public class StyleCreator : TreeNode
    {
        public TreeNode CreateTextAndParagraphStyleNode(string styleType, List<TreeNode> styleChildren, TreeNode root)
        {
            string tagName = "";
            string styleName = EnsureUniqueStyleName(root, "w:style", "WordRegStyle");
            //Формирование тега для имени стиля
            TreeNode styleIdAndName = new TreeNode()
            {
                TagName = $"w:name",
                Attributes = { { "w:val", styleName } },
                CloseTag = false
            };

            switch(styleType)
            {
                case "character":
                    tagName = "w:rPr";
                    break;

                case "paragraph":
                    tagName = "w:pPr";
                    break;

            }
            //Формирование тега, содержащего параметры стиля
            TreeNode parent = new TreeNode()
            {
                TagName = tagName,
                Children = styleChildren,
                CloseTag = true
            };

            List<TreeNode> textParent = new List<TreeNode>();
            List<TreeNode> name = new List<TreeNode>();
            TreeNode styleNode = new TreeNode();
            textParent.Add(parent);
            name.Add(styleIdAndName);


            //Формирование и заполнение тега для стиля
            
            styleNode = new TreeNode()
            {
                TagName = "w:style",
                Attributes =
                {
                    {"w:type", styleType },
                    {"w:styleId", styleName }
                },
                Children = name.Union(textParent).ToList(),
                CloseTag = true
            };

            return styleNode;
        }

        public (TreeNode, TreeNode) CreateNumberingStyleNodes(List<TreeNode> styleChildren, TreeNode root)
        {
            string numberingId = EnsureUniqueNumberingId(root);

            // Создаем узел `w:abstractNum`, содержащий все уровни `w:lvl`
            TreeNode mainStyleNode = new TreeNode
            {
                TagName = "w:abstractNum",
                Attributes = { { "w:abstractNumId", numberingId } },
                CloseTag = true
            };

            foreach (TreeNode childNode in styleChildren)
            {
                mainStyleNode.AddChild(mainStyleNode, childNode);
            }

            // Создаем узел `w:num`, который ссылается на `w:abstractNum`
            TreeNode appliedStyle = new TreeNode
            {
                TagName = "w:num",
                Attributes = { { "w:numId", EnsureUniqueNumberingId(root) } },
                Children =
            {
                new TreeNode
                {
                    TagName = "w:abstractNumId",
                    Attributes = { { "w:val", numberingId } }
                }
            },
                CloseTag = true
            };

            return (mainStyleNode, appliedStyle);
        }

        public void InroduceStyleInTree(TreeNode stylesNodeParent, TreeNode styleChilld)
        {
            AddChild(stylesNodeParent, styleChilld);
        }

        public string EnsureUniqueStyleName(TreeNode root, string tag, string startName)
        {
            string baseName = startName; // Базовое имя стиля
            string styleName = baseName; // Начинаем с базового имени
            int counter = 1; // Счетчик для добавления суффикса

            // Поиск всех узлов с указанным тегом
            List<TreeNode> styles = LongBreadthFirstSearch(root, tag);

            for (int i = 0; i < styles.Count; i++)
            {
                styles[i].Attributes.TryGetValue("w:styleId", out string styleId);
                if(styleId == styleName)
                {
                    styleName = baseName + counter++;
                }
            }

            return styleName; // Возвращаем уникальное имя
        }

        public string EnsureUniqueNumberingId(TreeNode root)
        {
            int counter = 0;
            string id = counter.ToString();

            List<TreeNode> styles = LongBreadthFirstSearch(root, "w:abstractNum");

            foreach (var style in styles)
            {
                if (style.Attributes.TryGetValue("w:abstractNumId", out string styleId) && styleId == id)
                {
                    counter++;
                    id = counter.ToString();
                }
            }

            return id;
        }
    }
}
