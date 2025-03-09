using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser
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
        public NumberingFormat NumberingType {  get; set; }
        public string Marker { get; set; }
        public double? FirstLineIndent { get; set; } = 0;
        public double? LeftIndent { get; set; } = 0;
        public double? RightIndent { get; set; } = 0;
        public int IntervalInText { get; set; }
        public int? BeforeInterval { get; set; } = 0;
        public int? AfterInterval { get; set; } = 0;

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
                    Attributes = { { "w:val", Enum.GetName(typeof(NumberingFormat), NumberingType).ToLower() } }
                });

                // Текст маркера (например, "%1." для 1.)
                lvlNode.AddChild(lvlNode, new TreeNode
                {
                    TagName = "w:lvlText",
                    Attributes = { { "w:val", Marker.Replace("%1", $"%{level + 1}") } }
                });

                // Отступы
                TreeNode indNode = new TreeNode
                {
                    TagName = "w:ind"
                };

                indNode.Attributes.Add("w:firstLine", $"{FirstLineIndent * TwipsToCentimeter}");
                indNode.Attributes.Add("w:left", $"{(LeftIndent + level * 1.5) * TwipsToCentimeter}"); // Каждый уровень на 1.5 см дальше
                indNode.Attributes.Add("w:right", $"{RightIndent * TwipsToCentimeter}");

                lvlNode.AddChild(lvlNode, indNode);

                // Интервал
                TreeNode spacingNode = new TreeNode
                {
                    TagName = "w:spacing",
                    Attributes =
                {
                    { "w:line", $"{IntervalInText}" },
                    { "w:lineRule", "auto" }
                }
                };

                if (BeforeInterval.HasValue)
                    spacingNode.Attributes.Add("w:before", $"{BeforeInterval * TwipsToCentimeter}");
                if (AfterInterval.HasValue)
                    spacingNode.Attributes.Add("w:after", $"{AfterInterval * TwipsToCentimeter}");

                lvlNode.AddChild(lvlNode, spacingNode);

                styleNodes.Add(lvlNode);
            }

            return styleNodes;
        }
    }
}
