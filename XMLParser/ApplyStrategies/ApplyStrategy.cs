using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static XMLParser.TreeNode;
using XMLParser.Styles;

namespace XMLParser.ApplyStrategies
{
    public abstract class ApplyStrategy
    {
        public abstract void Apply(TreeNode root, TreeNode style);

        protected TreeNode CreateStyleNode(string styleTagName, string styleName)
        {
            return new TreeNode()
            {
                TagName = styleTagName,
                Attributes = { { "w:val", styleName } },
                CloseTag = false
            };
        }

        protected void ApplyStylesToNodes(TreeNode root, string tagName, TreeNode styleToApply)
        {
            List<TreeNode> foundedParents = QuikBreadthFirstSearch(root, tagName);

            for (int i = 0; i < foundedParents.Count; i++)
            {
                foundedParents[i].Children.Add(styleToApply);
            }
        }
    }
}
