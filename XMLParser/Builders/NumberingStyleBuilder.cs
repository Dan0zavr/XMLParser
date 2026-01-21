using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;
using static XMLParser.TreeNode;

namespace XMLParser.Builders
{
    public class NumberingStyleBuilder : StyleBuilder<NumberingElementStyle>
    {
        private List<int> usedIds = new List<int>();
        private List<int> usedAbstractIds = new List<int>();

        public override TreeNode BuildStyle(NumberingElementStyle style)
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

        public (TreeNode, TreeNode) SyncId(TreeNode style, TreeNode abstractStyle, TreeNode root)
        {
            string abstractStyleId = EnsureUniqueAbstractNumberingId(root);
            string styleId = EnsureUniqueNumberingId(root);

            abstractStyle.Attributes["w:abstractNumId"] = abstractStyleId;

            style.Attributes["w:numId"] = styleId;

            TreeNode abstractNumIdInStyle = style.QuikBreadthFirstSearch("w:abstractNumId").FirstOrDefault();

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

        private protected override List<TreeNode> CreateNastedNodes(NumberingElementStyle styleToTree)
        {
            List<TreeNode> styleNodes = new List<TreeNode>();

            for (int level = NumberingElementStyle.DEFAULT_LEVELS; level <= styleToTree.Levels; level++)
            {
                TreeNode lvlNode = new TreeNode
                {
                    TagName = "w:lvl",
                    Attributes = { { "w:ilvl", $"{level}" } },
                    CloseTag = true
                };

                //С чего начинается нумерация
                lvlNode.Children.Add(new TreeNode
                {
                    TagName = "w:start",
                    Attributes = { { "w:val", "1"} }
                });

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

                lvlNode.Children.Add(new TreeNode
                {
                    TagName = "w:pPr",
                    CloseTag = true
                });

                styleNodes.Add(lvlNode);
            }

            return styleNodes;
        }
        private string EnsureUniqueNumberingId(TreeNode root)
        {
            int counter = 1;
            int id = counter;

            List<TreeNode> styles = root.LongBreadthFirstSearch("w:num");

            foreach (var style in styles)
            {
                if (style.Attributes.TryGetValue("w:numId", out string styleId) && styleId == id.ToString())
                {
                    counter++;
                    id = counter;
                }

                if (usedIds.Contains(id))
                {
                    counter++;
                    id = counter;
                }
            }

            usedIds.Add(id);
            return id.ToString();
        }

        private string EnsureUniqueAbstractNumberingId(TreeNode root)
        {
            int counter = 1;
            int id = counter;

            List<TreeNode> styles = root.LongBreadthFirstSearch("w:abstractNum");

            foreach (var style in styles)
            {
                if (style.Attributes.TryGetValue("w:abstractNumId", out string styleId) && styleId == id.ToString())
                {
                    counter++;
                    id = counter;
                }

                if (usedAbstractIds.Contains(id))
                {
                    counter++;
                    id = counter;
                }
            }

            usedAbstractIds.Add(id);
            return id.ToString();
        }
    }
}
