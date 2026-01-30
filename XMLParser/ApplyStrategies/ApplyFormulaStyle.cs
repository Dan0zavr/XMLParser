using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser.ApplyStrategies
{
    public class ApplyFormulaStyle : ApplyStrategy
    {
        public override void Apply(TreeNode root, TreeNode style)
        {
            List<TreeNode> formulaParagraphs = FindFormulaParagraphs(root);
            TreeNode styleParagraph = style.Children[0];
            int counter = 0;
            for (int i = 0; i < formulaParagraphs.Count; i++)
            {
                counter++;
                TreeNode styleClone = styleParagraph.Clone();
                TreeNode formula = formulaParagraphs[i].QuikBreadthFirstSearch("m:oMathPara").First().Clone();
                for (int j = 0; j < styleClone.Children.Count; j++)
                {
                    if (styleClone.Children[j].TagName == "formula")
                    {
                        styleClone.Children[j] = formula;
                    }
                }

                TreeNode sectPr = FindFirstSectPr(root, formulaParagraphs, i);

                TreeNode tabs = styleClone.QuikBreadthFirstSearch("w:tabs").First();
                string pageWidth = sectPr.QuikBreadthFirstSearch("w:pgSz").First().Attributes["w:w"];
                string leftMargin = sectPr.QuikBreadthFirstSearch("w:pgMar").First().Attributes["w:left"];
                string rightMargin = sectPr.QuikBreadthFirstSearch("w:pgMar").First().Attributes["w:right"];

                (string centerPos, string rightPos) = CalculateTabPositions(pageWidth, leftMargin, rightMargin);
                tabs.Children[0].Attributes["w:pos"] = centerPos;
                tabs.Children[1].Attributes["w:pos"] = rightPos;

                TreeNode number = styleClone.QuikBreadthFirstSearch("number").FirstOrDefault();
                if (number != null)
                {
                    string numberingFormat = number.Values[0].ToString();
                    string numberValue = numberingFormat.Replace('$', Convert.ToChar(counter.ToString()));
                    number.TagName = "w:t";
                    number.Values[0] = numberValue;
                }

                formulaParagraphs[i].Children.Clear();
                formulaParagraphs[i].Children = styleClone.Children;
            }
        }

        private List<TreeNode> FindFormulaParagraphs(TreeNode root)
        {
            List<TreeNode> allParagraphs = root.LongBreadthFirstSearch("w:p");
            List<TreeNode> formulaParagraphs = new List<TreeNode>();

            foreach (TreeNode paragraph in allParagraphs)
            {
                if (paragraph.CheckChild("m:oMathPara"))
                {
                    formulaParagraphs.Add(paragraph);
                }
            }

            return formulaParagraphs;
        }

        private TreeNode FindFirstSectPr(TreeNode root, List<TreeNode> paragraphs, int currentIndex)
        {
            for (int i = currentIndex; i < paragraphs.Count; i++)
            {
                if (paragraphs[i].CheckChild("w:sectPr"))
                {
                    return paragraphs[i].QuikBreadthFirstSearch("w:sectPr").First();
                }
            }
            List<TreeNode> sectPrs = root.LongBreadthFirstSearch("w:sectPr");
            return sectPrs[sectPrs.Count - 1];
        }

        private (string center, string right) CalculateTabPositions(string sPageWidth, string sLeftMargin, string sRightMargin)
        {
            int pageWidth = Convert.ToInt32(sPageWidth);
            int leftMargin = Convert.ToInt32(sLeftMargin);
            int rightMargin = Convert.ToInt32(sRightMargin);

            int textWidth = pageWidth - leftMargin - rightMargin;
            int center = textWidth / 2;
            int right =  textWidth;

            return (center.ToString(), right.ToString());
        }
    }
}
