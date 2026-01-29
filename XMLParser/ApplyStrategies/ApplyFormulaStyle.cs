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
            foreach (TreeNode paragraph in formulaParagraphs)
            {
                counter++;
                TreeNode styleClone = styleParagraph.Clone();
                TreeNode formula = paragraph.QuikBreadthFirstSearch("m:oMath").First().Clone();
                TreeNode formulaStub = styleClone.QuikBreadthFirstSearch("formula").First();
                formulaStub = formula;

                TreeNode number = styleClone.QuikBreadthFirstSearch("number").FirstOrDefault();
                if (number != null)
                {
                    string numberingFormat = number.Values[0].ToString();
                    string numberValue = numberingFormat.Replace('$', Convert.ToChar(counter));
                    number.TagName = "w:t";
                    number.Values[0] = numberValue;
                }

                paragraph.Children.Clear();
                paragraph.Children = styleClone.Children;
            }
        }

        private List<TreeNode> FindFormulaParagraphs(TreeNode root)
        {
            List<TreeNode> allParagraphs = root.LongBreadthFirstSearch("w:p");
            List<TreeNode> formulaParagraphs = new List<TreeNode>();

            foreach (TreeNode paragraph in allParagraphs)
            {
                if (paragraph.CheckChild("m:oMath"))
                {
                    formulaParagraphs.Add(paragraph);
                }
            }

            return formulaParagraphs;
        }
    }
}
