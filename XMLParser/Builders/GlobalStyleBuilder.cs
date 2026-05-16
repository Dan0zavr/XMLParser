using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using XMLParser.Styles;

namespace XMLParser.Builders
{
    public class GlobalStyleBuilder : StyleBuilder<GlobalStyle>
    {
        public override TreeNode BuildStyle(GlobalStyle style)
        {
            return new TreeNode
            {
                TagName = "globalContainer",
                CloseTag = true,
                Children = CreateNastedNodes(style)
            };
        }

        private protected override List<TreeNode> CreateNastedNodes(GlobalStyle styleToTree)
        {
            List<TreeNode> nastedNodes = new List<TreeNode>();

            TreeNode margin = new TreeNode
            {
                TagName = "w:pgMar",
                Attributes = { {"w:top", $"{CmToTwips(styleToTree.TopMargin)}" },
                               {"w:bottom", $"{CmToTwips(styleToTree.BottomMargin)}" },
                               {"w:left", $"{CmToTwips(styleToTree.LeftMargin)}" },
                               {"w:right", $"{CmToTwips(styleToTree.RightMargin)}" },
                               {"w:header", "708" },
                               {"w:footer", "708" },
                               {"w:gutter", "0" } }
            };

            nastedNodes.Add(margin);

            return nastedNodes;
        }

        private int CmToTwips(double cm)
        {
            return (int)Math.Round(cm * 1440 / 2.54);
        }
    }
}
