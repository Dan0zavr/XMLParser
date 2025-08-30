using System.Globalization;

namespace XMLParser.Styles
{
    public class TableStyle : TreeNode, IStyle
    {
        private const int twipsToSantimetr = 567;

        public string StyleType => "TableStyle";

        public double CellPadding { get; set; } = 50;
        public int MinCellHeight { get; set; } = 0;
        public string VerticalAlignment { get; set; }
        public int BorderThilness { get; set; } = 4;
        public string BorderColor { get; set; } = "000000";
        public bool RepeatHeader { get; set; }

        public TextStyle TextStyle { get; set; }
        public ParagraphStyle ParagraphStyle { get; set; }

        public List<TreeNode> CreateTableStyle(TableStyle tableStyle)
        {
            List<TreeNode> style = new List<TreeNode>();
            TreeNode parent = new TreeNode()
            {
                TagName = "w:tblPr",
                Children = new List<TreeNode>(),
                CloseTag = true
            };
            parent.Children.Add(CreateBorderStyle(tableStyle));
            parent.Children.Add(CreateCellPadding(tableStyle));

            style.Add(parent);
            style.Add(CreateMinHeight(tableStyle));

            if (tableStyle.TextStyle != null)
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
                            Children = tableStyle.ParagraphStyle.CreateParagraphStyle(tableStyle.ParagraphStyle),
                            CloseTag = true
                        },
                        new TreeNode()
                        {
                            TagName = "w:rPr",
                            Children = tableStyle.TextStyle.CreateTextStyle(tableStyle.TextStyle),
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
