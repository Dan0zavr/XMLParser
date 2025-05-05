using System.Globalization;

namespace XMLParser.Styles
{
    public class ParagraphStyle : TreeNode, IStyle
    {
        private const int twipsToSantimetr = 567;

        public string StyleType => "ParagraphStyle";

        public string Alingnment { get; set; }
        public double? FirstLineIndent { get; set; } = 0;
        public double? LeftIndent { get; set; } = 0;
        public double? RightIndent { get; set; } = 0;
        public double IntervalInText { get; set; }
        public double? BeforeInterval { get; set; } = 0;
        public double? AfterInterval { get; set; } = 0;
        public bool ContextualSpacing { get; set; } = false;


        public List<TreeNode> CreateParagraphStyle(ParagraphStyle paragraphStyle)
        {
            List<TreeNode> style = new List<TreeNode>();
            double twips;
            foreach (var prop in typeof(ParagraphStyle).GetProperties())
            {
                TreeNode styleNode = new TreeNode();
                switch (prop.Name)
                {
                    case "Alingnment":

                        styleNode.TagName = "w:jc";
                        styleNode.Attributes.Add("w:val", paragraphStyle.Alingnment);
                        style.Add(styleNode);
                        break;

                    case "FirstLineIndent":
                        styleNode.TagName = "w:ind";
                        string twipsInString;

                        twips = paragraphStyle.FirstLineIndent.Value * twipsToSantimetr;

                        twipsInString = twips.ToString(CultureInfo.InvariantCulture);

                        styleNode.Attributes.Add("w:firstLine", twipsInString);

                        twips = paragraphStyle.RightIndent.Value * twipsToSantimetr;
                        twipsInString = twips.ToString(CultureInfo.InvariantCulture);
                        styleNode.Attributes.Add("w:right", twipsInString);

                        twips = paragraphStyle.LeftIndent.Value * twipsToSantimetr;
                        twipsInString = twips.ToString(CultureInfo.InvariantCulture);
                        styleNode.Attributes.Add("w:left", twipsInString);

                        style.Add(styleNode);
                        break;

                    case "IntervalInText":
                        styleNode.TagName = "w:spacing";
                        string lineValue = (Math.Truncate(paragraphStyle.IntervalInText * 240)).ToString(CultureInfo.InvariantCulture);
                        styleNode.Attributes.Add("w:line", lineValue);
                        styleNode.Attributes.Add("w:lineRule", "auto");

                        if (paragraphStyle.BeforeInterval != null)
                        {
                            twips = paragraphStyle.BeforeInterval.Value * twipsToSantimetr;
                            styleNode.Attributes.Add("w:before", $"{twips}");
                        }

                        if (paragraphStyle.AfterInterval != null)
                        {
                            twips = paragraphStyle.AfterInterval.Value * twipsToSantimetr;
                            styleNode.Attributes.Add("w:after", $"{twips}");
                        }
                        style.Add(styleNode);

                        if (paragraphStyle.ContextualSpacing)
                        {
                            TreeNode contextualSpacing = new TreeNode()
                            {
                                TagName = "w:contextualSpacing",
                            };
                            style.Add(contextualSpacing);
                        }
                        break;
                }
            }
            return style;
        }

    }
}
