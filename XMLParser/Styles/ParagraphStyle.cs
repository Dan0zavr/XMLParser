using System.Globalization;

namespace XMLParser.Styles
{
    public class ParagraphStyle : IStyle
    {
        public string StyleType => "ParagraphStyle";

        public string Alingnment { get; set; }
        public double? FirstLineIndent { get; set; } = 0;
        public double? LeftIndent { get; set; } = 0;
        public double? RightIndent { get; set; } = 0;
        public double IntervalInText { get; set; }
        public double? BeforeInterval { get; set; } = 0;
        public double? AfterInterval { get; set; } = 0;
        public bool ContextualSpacing { get; set; } = false;
    }
}
