using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser
{
    public static class StyleIntegrator
    {
        public static void IntegrateStylesToTree(TreeNode root, List<TreeNode> styles)
        {
            foreach (TreeNode style in styles)
            {
                root.Children.Add(style);
            }
        }

        public static void IntegrateNumberingStylesToTree(TreeNode root, List<TreeNode> containers)
        {
            foreach (var container in containers)
            {
                TreeNode absStyle = container.QuikBreadthFirstSearch("w:abstractNum").First();
                TreeNode normStyle = container.QuikBreadthFirstSearch("w:num").First();
                root.Children.Insert(0, absStyle);
                root.Children.Add(normStyle);
            }
        }
    }
}
