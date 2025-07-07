using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser
{
    public class Cleaner
    {
        public TreeNode CleanHandStyles(TreeNode root, List<string> specialTokens, XMLRead xmlRead, string savePath)
        {
            List<TreeNode> foundedParents = new List<TreeNode>();

            foundedParents = root.QuikBreadthFirstSearch(root, "w:rPr");
            root.TerminateChildren(foundedParents);
            foundedParents = root.QuikBreadthFirstSearch(root, "w:pPr");
            root.TerminateChildren(foundedParents);
            foundedParents = root.QuikBreadthFirstSearch(root, "w:numPr");
            root.TerminateChildren(foundedParents);
            foundedParents = root.QuikBreadthFirstSearch(root, "w:tblPr");
            root.TerminateChildren(foundedParents);

            return root;
        }

        public TreeNode CleanHandTableStyle(TreeNode root)
        {
            List<TreeNode> foundedCells = new List<TreeNode>();

            foundedCells = root.LongBreadthFirstSearch(root, "w:tc");

            foreach (var cell in foundedCells)
            {
                List<TreeNode> foundedParents = new List<TreeNode>();

                foundedParents = cell.QuikBreadthFirstSearch(cell, "w:rPr");
                root.TerminateChildren(foundedParents);
                foundedParents = cell.QuikBreadthFirstSearch(cell, "w:pPr");
                root.TerminateChildren(foundedParents);
            }

            return root;

        }

    }
}
