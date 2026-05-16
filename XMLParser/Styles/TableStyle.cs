using System.Globalization;

namespace XMLParser.Styles
{
    public class TableStyle : IStyle
    {
        private const int twipsToSantimetr = 567;

        public string StyleType => "TableStyle";

        public double CellPadding { get; set; } = 50;
        public int MinCellHeight { get; set; } = 0;
        public string VerticalAlignment { get; set; }
        public int BorderThilness { get; set; } = 4;
        public string BorderColor { get; set; } = "000000";
        public string? LabelValue { get; set; }

        public TextStyle TextStyle { get; set; }
        public ParagraphStyle ParagraphStyle { get; set; }
    }
}
