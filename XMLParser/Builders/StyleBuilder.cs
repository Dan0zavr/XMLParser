using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.SpecialClasses.Tree;
using XMLParser.Styles;
using static XMLParser.SpecialClasses.Tree.TreeNode;

namespace XMLParser.Builders
{
    public abstract class StyleBuilder<T> : IStyleBuilder where T : IStyle
    {
        protected const int twipsToSantimetr = 567;

        public abstract TreeNode BuildStyle(T style);

        TreeNode IStyleBuilder.BuildStyle(IStyle style)
        {
            return BuildStyle((T)style);
        }

        private protected abstract List<TreeNode> CreateNastedNodes(T styleToTree);
    }
}
