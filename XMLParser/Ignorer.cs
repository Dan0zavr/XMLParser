using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser
{
    public static class Ignorer
    {
        public static void IgnoreFormulas(TreeNode root, Action method)
        {
            List<TreeNode> paragraphs = root.LongBreadthFirstSearch("w:p");
            List<TreeNode> formulaParagraphs = new List<TreeNode>();
            foreach (TreeNode node in paragraphs)
            {
                if (node.CheckChild("m:oMathPara"))
                {
                    formulaParagraphs.Add(node);
                }
            }
            List<TreeNode> formulaClones = new List<TreeNode>();
            foreach (TreeNode formula in formulaParagraphs)
            {
                formulaClones.Add(formula.Clone());
            }

            method();

            for (int i = 0; i < formulaParagraphs.Count; i++)
            {
                formulaParagraphs[i].Children = formulaClones[i].Children;
            }
        }
    }
}
