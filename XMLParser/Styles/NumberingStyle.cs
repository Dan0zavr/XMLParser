namespace XMLParser.Styles
{
    public class NumberingStyle : TreeNode, IStyle
    {
        private const int TwipsToCentimeter = 567;
        public string StyleType => "NumberingStyle";

        public enum NumberingFormat
        {
            Decimal,
            RomanUpper,
            RomanLower,
            Bullet,
            UpperLetter,
            LowerLetter
        }

        public int Levels { get; set; } = 1;
        public string NumberingType { get; set; }
        public string Marker { get; set; }

        public List<TreeNode> CreateNumberingStyle()
        {
            List<TreeNode> styleNodes = new List<TreeNode>();

            for (int level = 1; level <= Levels; level++)
            {
                TreeNode lvlNode = new TreeNode
                {
                    TagName = "w:lvl",
                    Attributes = { { "w:ilvl", $"{level}" } },
                    CloseTag = true
                };

                // Формат нумерации
                lvlNode.AddChild(lvlNode, new TreeNode
                {
                    TagName = "w:numFmt",
                    Attributes = { { "w:val", NumberingType.ToLower() } }
                });

                // Текст маркера (например, "%1." для 1.)
                lvlNode.AddChild(lvlNode, new TreeNode
                {
                    TagName = "w:lvlText",
                    Attributes = { { "w:val", Marker.Replace("%1", $"%{level + 1}") } }
                });

                styleNodes.Add(lvlNode);
            }

            return styleNodes;
        }
    }
}
