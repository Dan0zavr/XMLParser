using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static XMLParser.TreeNode;

namespace XMLParser.ApplyStrategies
{
    public class ApplyNumberingStyleStrategy : ApplyStrategy
    {
        private TreeNode _numberingRoot;
        private List<TreeNode> _numPr;
        private List<TreeNode> _numNodes;
        private List<TreeNode> _abstractNodes;

        public ApplyNumberingStyleStrategy(TreeNode numberingRoot)
        {
            _numberingRoot = numberingRoot;
        }
        public override void Apply(TreeNode root, TreeNode style)
        {
            TreeNode normStyle = style.QuikBreadthFirstSearch("w:num").First();

            _numNodes = _numberingRoot.LongBreadthFirstSearch("w:num");
            _abstractNodes = _numberingRoot.LongBreadthFirstSearch("w:abstractNum");
            _numPr = root.QuikBreadthFirstSearch("w:numPr");

            string styleId = normStyle.Attributes["w:numId"];
            string styleType = FindTypeByNum(normStyle, _abstractNodes);

            Dictionary<int, string> allNumsAndTypes = FindNumPrTypes(root, _numberingRoot);
            Dictionary<int, string> styleNumsAndTypes = allNumsAndTypes.Where(n => n.Value == styleType).ToDictionary();

            foreach (var element in styleNumsAndTypes)
            {
                _numPr[element.Key].Children[1].Attributes["w:val"] = styleId;
            }
        }

        private Dictionary<int, string> FindNumPrTypes(TreeNode docRoot, TreeNode numRoot)
        {
            Dictionary<int, string> idAndType = new Dictionary<int, string>();

            for (int i = 0; i < _numPr.Count; i++)
            {
                int numId = Convert.ToInt32(_numPr[i].Children[1].Attributes["w:val"]);
                int numLvl = Convert.ToInt32(_numPr[i].Children[0].Attributes["w:val"]);
                string type = FindTypeByNum(_numNodes, _abstractNodes, numId, numLvl);
                idAndType.Add(i, type);
            }
            return idAndType;
        }

        private string FindTypeByNum(List<TreeNode> numNodes, List<TreeNode> abstractNodes, int id, int lvl)
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

        private string FindTypeByNum(TreeNode numNode, List<TreeNode> abstractNodes)
        {
            int abstractId = Convert.ToInt32(numNode.Children[0].Attributes["w:val"]);
            TreeNode abstractNode = abstractNodes.Where(n => Convert.ToInt32(n.Attributes["w:abstractNumId"]) == abstractId).FirstOrDefault();
            TreeNode numFormat = abstractNode.QuikBreadthFirstSearch("w:numFmt").First();
            return numFormat.Attributes["w:val"];
        }
    }
}
