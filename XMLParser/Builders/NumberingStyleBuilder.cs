using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;
using static XMLParser.TreeNode;

namespace XMLParser.Builders
{
    public class NumberingStyleBuilder : StyleBuilder<NumberingStyle>
    {
        public override TreeNode BuildStyle(NumberingStyle style)
        {   
            // Создаем узел `w:num`, который ссылается на `w:abstractNum`
            TreeNode styleNode = new TreeNode
            {
                TagName = "w:num",
                Attributes = { { "w:numId", "-1" } }, // id генерируется позже
                Children = {
                    new TreeNode {
                        TagName = "w:abstractNumId",
                        Attributes = { { "w:val", "-1" } } // Синхронизируется позже
                    }
                },
                CloseTag = true
            };

            return styleNode;
        }

        public TreeNode BuildAbstrtactStyle(NumberingStyle style)
        {
            List<TreeNode> styleChildren = CreateNastedNodes(style);

            TreeNode styleNode = new TreeNode
            {
                TagName = "w:abstractNum",
                Attributes = { { "w:abstractNumId", "-1" } }, //id генерируется позже
                CloseTag = true
            };

            foreach (TreeNode childNode in styleChildren)
            {
                styleNode.Children.Add(childNode);
            }

            return styleNode;
        }

        public (KeyValuePair<StyleCategory, TreeNode>, KeyValuePair<StyleCategory, TreeNode>) SyncId(KeyValuePair<StyleCategory, TreeNode> style, KeyValuePair<StyleCategory, TreeNode> abstractStyle, TreeNode root)
        {
            string abstractStyleId = EnsureUniqueNumberingId(root);
            string styleId = EnsureUniqueNumberingId(root);

            TreeNode abstractNum = abstractStyle.Value.QuikBreadthFirstSearch("w:abstractNum").FirstOrDefault();

            if (abstractNum != null)
            {
                abstractNum.Attributes["w:abstractNumId"] = abstractStyleId;
            }
            else
            {
                throw new Exception("Не получилось найти \"w:abstractNum\"");
            }

            style.Value.Attributes["w:numId"] = styleId;

            TreeNode abstractNumIdInStyle = style.Value.QuikBreadthFirstSearch("w:abstractNumId").FirstOrDefault();

            if (abstractNumIdInStyle != null) 
            {
                abstractNumIdInStyle.Attributes["w:val"] = abstractStyleId;
            }
            else
            {
                throw new Exception("Не получилось найти \"w:abstractNumId\"");
            }

            return (style, abstractStyle);
        }

        private protected override List<TreeNode> CreateNastedNodes(NumberingStyle styleToTree)
        {
            List<TreeNode> styleNodes = new List<TreeNode>();

            for (int level = 1; level <= styleToTree.Levels; level++)
            {
                TreeNode lvlNode = new TreeNode
                {
                    TagName = "w:lvl",
                    Attributes = { { "w:ilvl", $"{level}" } },
                    CloseTag = true
                };

                // Формат нумерации
                lvlNode.Children.Add(new TreeNode
                {
                    TagName = "w:numFmt",
                    Attributes = { { "w:val", styleToTree.NumberingType.ToLower() } }
                });

                // Текст маркера (например, "%1." для 1.)
                lvlNode.Children.Add(new TreeNode
                {
                    TagName = "w:lvlText",
                    Attributes = { { "w:val", styleToTree.Marker.Replace("%1", $"%{level + 1}") } }
                });

                styleNodes.Add(lvlNode);
            }

            return styleNodes;
        }
        private string EnsureUniqueNumberingId(TreeNode root)
        {
            int counter = 0;
            string id = counter.ToString();

            List<TreeNode> styles = root.LongBreadthFirstSearch("w:abstractNum");

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
