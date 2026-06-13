using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.SpecialClasses.Tree;
using XMLParser.Styles;

namespace XMLParser.Builders
{
    public class TextStyleBuilder : StyleBuilder<TextStyle>
    {
        public override TreeNode BuildStyle(TextStyle textStyle)
        {
            List<TreeNode> textStyleChildren = CreateNastedNodes(textStyle);

            string tagName = "w:rPr";
            string styleType = "character";
            string styleName = "заглушка"; // Сделать гарантию уникального имени вне этого метода (чтобы не передавать root)
            //string styleName = EnsureUniqueStyleName(root, "w:style", "WordRegTextStyle");

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
                Children = textStyleChildren,
                CloseTag = true
            };

            List<TreeNode> textParent = new List<TreeNode>();
            List<TreeNode> name = new List<TreeNode>();
            TreeNode styleNode = new TreeNode();
            textParent.Add(parent);
            name.Add(styleIdAndName);


            //Формирование и заполнение тега для стиля
            styleNode = new TreeNode()
            {
                TagName = "w:style",
                Attributes =
                {
                    {"w:type", styleType },
                    {"w:styleId", styleName }
                },
                Children = name.Union(textParent).ToList(),
                CloseTag = true
            };

            return styleNode;
        }

        private protected override List<TreeNode> CreateNastedNodes(TextStyle styleToTree)
        {
            List<TreeNode> style = new List<TreeNode>();
            foreach (var prop in typeof(TextStyle).GetProperties())
            {
                TreeNode styleNode = new TreeNode();
                switch (prop.Name)
                {
                    case "FontName":
                        styleNode.TagName = "w:rFonts";
                        styleNode.Attributes.Add("w:ascii", styleToTree.FontName);
                        styleNode.Attributes.Add("w:hAnsi", styleToTree.FontName);
                        styleNode.Attributes.Add("w:cs", styleToTree.FontName);
                        style.Add(styleNode);
                        break;

                    case "FontSize":
                        styleNode.TagName = "w:sz";
                        styleToTree.FontSize = styleToTree.FontSize * 2;
                        styleNode.Attributes.Add("w:val", styleToTree.FontSize.ToString());
                        style.Add(styleNode);
                        break;
                }
            }
            return style;
        }
    }
}
