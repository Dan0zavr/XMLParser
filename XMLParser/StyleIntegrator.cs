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
    }
}
