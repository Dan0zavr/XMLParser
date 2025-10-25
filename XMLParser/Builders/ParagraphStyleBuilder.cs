using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;

namespace XMLParser.Builders
{
    public class ParagraphStyleBuilder : StyleBuilder<ParagraphStyle>
    {
        public override TreeNode BuildStyle(ParagraphStyle style)
        {
            List<TreeNode> paragraphStyleChildren = CreateNastedNodes(style);

            string tagName = "w:pPr";
            string styleType = "paragraph";
            string styleName = "заглушка"; // Сделать гарантию уникального имени вне этого метода (чтобы не передавать root)
            //string styleName = EnsureUniqueStyleName(root, "w:style", "WordRegParagraphStyle");

            //Формирование тега для имени стиля
            TreeNode styleIdAndName = new TreeNode()
            {
                TagName = $"w:name",
                Attributes = { { "w:val", styleName } },
                CloseTag = false
            };

            //Формирование тега, содержащего параметры стиля
            TreeNode parent = new TreeNode()
            {
                TagName = tagName,
                Children = paragraphStyleChildren,
                CloseTag = true
            };

            List<TreeNode> paragraphParent = new List<TreeNode>();
            List<TreeNode> name = new List<TreeNode>();
            paragraphParent.Add(parent);
            name.Add(styleIdAndName);

            //Формирование и заполнение тега для стиля
            TreeNode styleNode = new TreeNode()
            {
                TagName = "w:style",
                Attributes =
                {
                    {"w:type", styleType },
                    {"w:styleId", styleName }
                },
                Children = name.Union(paragraphParent).ToList(),
                CloseTag = true
            };

            return styleNode;

        }

        private protected override List<TreeNode> CreateNastedNodes(ParagraphStyle styleToTree)
        {
            List<TreeNode> style = new List<TreeNode>();
            double twips;
            foreach (var prop in typeof(ParagraphStyle).GetProperties())
            {
                TreeNode styleNode = new TreeNode();
                switch(prop.Name)
                {
                    case "Alingnment":

                        styleNode.TagName = "w:jc";
                        styleNode.Attributes.Add("w:val", styleToTree.Alingnment);
                        style.Add(styleNode);
                        break;

                    case "FirstLineIndent":
                        styleNode.TagName = "w:ind";
                        string twipsInString;

                        twips = styleToTree.FirstLineIndent.Value * twipsToSantimetr;

                        twipsInString = twips.ToString(CultureInfo.InvariantCulture);

                        styleNode.Attributes.Add("w:firstLine", twipsInString);

                        twips = styleToTree.RightIndent.Value * twipsToSantimetr;
                        twipsInString = twips.ToString(CultureInfo.InvariantCulture);
                        styleNode.Attributes.Add("w:right", twipsInString);

                        twips = styleToTree.LeftIndent.Value * twipsToSantimetr;
                        twipsInString = twips.ToString(CultureInfo.InvariantCulture);
                        styleNode.Attributes.Add("w:left", twipsInString);

                        style.Add(styleNode);
                        break;

                    case "IntervalInText":
                        styleNode.TagName = "w:spacing";
                        string lineValue = (Math.Truncate(styleToTree.IntervalInText * 240)).ToString(CultureInfo.InvariantCulture);
                        styleNode.Attributes.Add("w:line", lineValue);
                        styleNode.Attributes.Add("w:lineRule", "auto");

                        if (styleToTree.BeforeInterval != null)
                        {
                            twips = styleToTree.BeforeInterval.Value * twipsToSantimetr;
                            styleNode.Attributes.Add("w:before", $"{twips}");
                        }

                        if (styleToTree.AfterInterval != null)
                        {
                            twips = styleToTree.AfterInterval.Value * twipsToSantimetr;
                            styleNode.Attributes.Add("w:after", $"{twips}");
                        }
                        style.Add(styleNode);

                        if (styleToTree.ContextualSpacing)
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
