using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.SpecialClasses.Tree;
using XMLParser.Styles;

namespace XMLParser.Builders
{
    public interface IStyleBuilder
    {
        TreeNode BuildStyle(IStyle style);
    }
}
