using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser.Styles
{
    public class ParagraphStyle : TreeNode, IStyle
    {
        private const int twipsToSantimetr = 567;

        public string StyleType => "ParagraphStyle";

        public string Allingnment { get; set; }
        public double? FirstLineIndent { get; set; } = 0;
        public double? LeftIndent { get; set; } = 0;
        public double? RightIndent { get; set; } = 0;
        public int IntervalInText { get; set; }
        public int? BeforeInterval { get; set; } = 0;
        public int? AfterInterval { get; set; } = 0;


        public List<TreeNode> CreateParagraphStyle(ParagraphStyle paragraphStyle)
        {
            List<TreeNode> style = new List<TreeNode>();
            double twips;
            foreach (var prop in typeof(ParagraphStyle).GetProperties())
            {
                TreeNode styleNode = new TreeNode();
                switch (prop.Name)
                {
                    case "Allingnment":

                        styleNode.TagName = "w:jc";
                        styleNode.Attributes.Add("w:val", paragraphStyle.Allingnment);
                        style.Add(styleNode);
                        break;

                    case "FirstLineIndent":
                        styleNode.TagName = "w:ind";
                        string twipsInString;

                        twips = paragraphStyle.FirstLineIndent.Value * twipsToSantimetr;
                        twipsInString = $"{twips}";
                        twipsInString = twipsInString.Replace(',', '.');
                        styleNode.Attributes.Add("w:firstLine", twipsInString);

                        twips = paragraphStyle.RightIndent.Value * twipsToSantimetr;
                        twipsInString = $"{twips}";
                        twipsInString = twipsInString.Replace(',', '.');
                        styleNode.Attributes.Add("w:right", twipsInString);

                        twips = paragraphStyle.LeftIndent.Value * twipsToSantimetr;
                        twipsInString = $"{twips}";
                        twipsInString = twipsInString.Replace(',', '.');
                        styleNode.Attributes.Add("w:left", twipsInString);

                        style.Add(styleNode);
                        break;

                    case "IntervalInText":
                        styleNode.TagName = "w:spacing";
                        styleNode.Attributes.Add("w:line", $"{paragraphStyle.IntervalInText}");
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
                        break;
                }
            }
            return style;
        }

    }
}
