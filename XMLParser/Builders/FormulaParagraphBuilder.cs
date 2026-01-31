using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;

namespace XMLParser.Builders
{
    public class FormulaParagraphBuilder : StyleBuilder<FormulaStyle>
    {
        public override TreeNode BuildStyle(FormulaStyle style)
        {
            return new TreeNode
            {
                TagName = "formulaContainer",
                Children = CreateNastedNodes(style),
                Attributes = CreateContainerAttributes(style),
                CloseTag = true
            };
        }

        private Dictionary<string, string> CreateContainerAttributes(FormulaStyle style)
        {
            const string LINE_AROUND = "lineAround";

            Dictionary<string, string> attributes = new Dictionary<string, string>();

            attributes.Add(LINE_AROUND, $"{style.EmptyLineAround.ToString().ToLower()}");

            return attributes;
        }

        private protected override List<TreeNode> CreateNastedNodes(FormulaStyle styleToTree)
        {
            List<TreeNode> nastedNodes = new List<TreeNode>();

            TreeNode jc = new TreeNode
            {
                TagName = "w:jc",
                Attributes = { { "w:val", "left" } }
            };

            TreeNode tabs = new TreeNode
            {
                TagName = "w:tabs",
                Children = new List<TreeNode>
                {
                    new TreeNode // tab'ы для формулы
                    {
                        TagName = "w:tab",
                        Attributes = {{"w:val", "center"}, {"w:pos", "value"} }
                    },
                    new TreeNode // tab'ы для номера
                    {
                        TagName = "w:tab",
                        Attributes = {{"w:val", "right"}, {"w:pos", "value"} }
                    }
                },
                CloseTag = true
            };

            TreeNode tab = new TreeNode
            {
                TagName = "w:r",
                Children = new List<TreeNode>
                {
                    new TreeNode
                    {
                        TagName = "w:tab"
                    }
                },
                CloseTag = true
            };

            TreeNode formulaParagraph = new TreeNode()
            {
                TagName = "w:p",
                Children = new List<TreeNode>() 
                { 
                    new TreeNode
                    {
                        TagName = "w:pPr",
                        Children = new List<TreeNode>
                        {
                            jc,
                            tabs
                        },
                        CloseTag = true
                    },

                    tab.Clone(),

                    new TreeNode
                    {
                        TagName = "formula",
                        Children = new List<TreeNode>(),
                        CloseTag = true
                    }
                },
                CloseTag = true
            };

            if (styleToTree.Numeration)
            {
                TreeNode number = new TreeNode
                {
                    TagName = "w:r",
                    Children = new List<TreeNode>
                    {
                        new TreeNode
                        {
                            TagName = "number", // заменится на w:t в ApplyStrategy
                            Values = new List<string> {styleToTree.NumerationFormat},
                            CloseTag = true
                        }
                    },
                    CloseTag = true
                };

                formulaParagraph.Children.Add(tab.Clone());
                formulaParagraph.Children.Add(number);
            }

            nastedNodes.Add(formulaParagraph);

            if (styleToTree.Legend) 
            {
                TreeNode legendParagraph = new TreeNode()
                {

                };
            }

            return nastedNodes;
        }
    }
}
