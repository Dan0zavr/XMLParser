using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static XMLParser.TreeNode;

namespace XMLParser.ApplyStrategies
{
    public class ApplyNumberingStyleStrategy : ApplyStrategy
    {
        private TreeNode _numberingRoot;

        public ApplyNumberingStyleStrategy(TreeNode numberingRoot)
        {
            _numberingRoot = numberingRoot;
        }
        public override void Apply(TreeNode root, TreeNode style)
        {
            FindNumPrTypes(root, _numberingRoot);
        }

        private Dictionary<int, string> FindNumPrTypes(TreeNode docRoot, TreeNode numRoot)
        {
            Dictionary<int, string> idAndType = new Dictionary<int, string>();
            List<TreeNode> numPr = docRoot.QuikBreadthFirstSearch("w:numPr");
            List<TreeNode> numNodes = numRoot.LongBreadthFirstSearch("w:num");
            List<TreeNode> abstractNumNodes = numRoot.LongBreadthFirstSearch("w:abstractNum");

            for (int i = 0; i < numPr.Count; i++)
            {
                int numId = Convert.ToInt32(numPr[i].Children[1].Attributes["w:val"]);
                int numLvl = Convert.ToInt32(numPr[i].Children[0].Attributes["w:val"]);
                string type = FindType(numNodes, abstractNumNodes, numId, numLvl);
                idAndType.Add(i, type);
            }
            return idAndType;
        }

        private string FindType(List<TreeNode> numNodes, List<TreeNode> abstractNodes, int id, int lvl)
        {
            TreeNode numNode = numNodes.Where(n => Convert.ToInt32(n.Attributes["w:numId"]) == id).FirstOrDefault();

            if (numNode == null) throw new Exception("Id списка не найден");

            int abstractId = Convert.ToInt32(numNode.Children[0].Attributes["w:val"]);
            TreeNode abstractNode = abstractNodes.Where(n => Convert.ToInt32(n.Attributes["w:abstractNumId"]) == abstractId).FirstOrDefault();

            if (abstractNode == null) throw new Exception("Абстрактный Id списка не найден");
            List<TreeNode> levels = abstractNode.LongBreadthFirstSearch("w:lvl");
            TreeNode level = levels.Where(n => Convert.ToInt32(n.Attributes["w:ilvl"]) == lvl).First();
            TreeNode numFormat = level.QuikBreadthFirstSearch("w:numFmt").First();
            return numFormat.Attributes["w:val"];
        }
    }
}
