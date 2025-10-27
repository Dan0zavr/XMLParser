using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser.ApplyStrategies
{
    public class UselessStrategy : ApplyStrategy
    {
        public override void Apply(TreeNode root, TreeNode style)
        {
            // эта стратегия нужна для того чтобы ничего не делать для стилей, которые ни к чему не применяются
        }
    }
}
