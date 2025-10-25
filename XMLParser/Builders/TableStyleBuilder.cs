using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;

namespace XMLParser.Builders
{
    public class TableStyleBuilder : StyleBuilder<TableStyle>
    {
        public override TreeNode BuildStyle(TableStyle style)
        {
            List<TreeNode> tableStyleChildren = CreateNastedNodes(style);

            TreeNode tableNameNode = new TreeNode()
            {
                TagName = "w:name",
                Attributes = { { "w:val", "заглушка" } }
            };

            TreeNode endStyle = new TreeNode()
            {
                TagName = "w:style",
                Attributes = { { "w:type", "table" }, { "w:styleId", "заглушка" } },
                Children = new List<TreeNode>(),
                CloseTag = true
            };

            endStyle.Children.Add(tableNameNode);

            endStyle.Children.AddRange(tableStyleChildren);

            return endStyle;
        }

        private protected override List<TreeNode> CreateNastedNodes(TableStyle styleToTree)
        {
            List<TreeNode> style = new List<TreeNode>();
            TreeNode parent = new TreeNode()
            {
                TagName = "w:tblPr",
                Children = new List<TreeNode>(),
                CloseTag = true
            };
            parent.Children.Add(CreateBorderStyle(styleToTree));
            parent.Children.Add(CreateCellPadding(styleToTree));

            style.Add(parent);
            style.Add(CreateMinHeight(styleToTree));

            if (styleToTree.TextStyle != null)
            {
                TreeNode textTableStyle = new TreeNode()
                {
                    TagName = "w:tblStylePr",
                    Attributes = { { "w:type", "cell" } },
                    Children = new List<TreeNode>()
                    {
                        new TreeNode()
                        {
                            TagName = "w:pPr",
                            Children = 
                            {
                                new TreeNode()
                                {
                                    TagName = "w:pStyle",
                                    Attributes = { { "w:val", "заглушка" } }
                                }
                            },
                            CloseTag = true
                        },
                        new TreeNode()
                        {
                            TagName = "w:rPr",
                            Children =
                            {
                                new TreeNode()
                                {
                                    TagName = "w:rStyle",
                                    Attributes = {{ "w:val", "заглушка" } }
                                }
                            },
                            CloseTag = true
                        }
                    },
                    CloseTag = true
                };

                style.Add(textTableStyle);
            }

            return style;
        }

        private TreeNode CreateBorderStyle(TableStyle tableStyle)
        {
            string[] sides = { "w:top", "w:bottom", "w:left", "w:right", "w:insideH", "w:insideV" };

            List<TreeNode> border = new List<TreeNode>();

            foreach (string side in sides)
            {
                TreeNode borderNode = new TreeNode
                {
                    TagName = side,
                    Attributes = { { "w:val", "single" }, { "w:sz", $"{tableStyle.BorderThilness}" }, { "w:color", $"{tableStyle.BorderColor}" } }
                };
                border.Add(borderNode);
            }

            TreeNode style = new TreeNode()
            {
                TagName = "w:tblBorders",
                Children = border,
                CloseTag = true
            };
            return style;
        }

        private TreeNode CreateCellPadding(TableStyle tableStyle)
        {
            string[] sides = { "w:top", "w:bottom", "w:left", "w:right" };
            List<TreeNode> padding = new List<TreeNode>();

            foreach (string side in sides)
            {
                string pad = (Math.Truncate(tableStyle.CellPadding * twipsToSantimetr)).ToString(CultureInfo.InvariantCulture);
                TreeNode paddingNode = new TreeNode
                {
                    TagName = side,
                    Attributes = { { "w:w", pad }, { "w:type", "dxa" } }
                };
                padding.Add(paddingNode);
            }

            TreeNode style = new TreeNode()
            {
                TagName = "w:tblCellMar",
                Children = padding,
                CloseTag = true
            };
            return style;
        }

        private TreeNode CreateMinHeight(TableStyle tableStyle)
        {
            // Минимальная высота ячейки
            TreeNode cellHeightParent = new TreeNode()
            {
                TagName = "w:trPr",
                Children = new List<TreeNode> {
                    new TreeNode {

                        TagName = "w:trHeight",
                        Attributes = {{"w:val",$"{tableStyle.MinCellHeight}"}, {"w:hRule","atLeast"} }
                    }
                },
                CloseTag = true

            };

            return cellHeightParent;
        }
    }
}
