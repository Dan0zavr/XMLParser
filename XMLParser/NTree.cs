using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser
{
    public class NTree
    {
        private string TagName { get; set; }
        private string Value { get; set; }
        private List<NTree> Children { get; set; } = new();
        private Dictionary<string, string> Attributes { get; set; }= new();

        public void AddChild(NTree child)
        {
            Children.Add(child);
        }
    }
}
