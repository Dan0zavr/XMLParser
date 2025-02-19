using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser
{
    public class NTree
    {
        private string OpenTagName { get; set; }
        private Dictionary<string, string> Attributes { get; set; } = new();
        private string? Value { get; set; }
        private List<NTree> Children { get; set; } = new();
        private string? CloseTagName { get; set; }
        

        public NTree(string openName, string? closeName, string? value = null)
        {
            OpenTagName = openName;
            Value = value;
            CloseTagName = closeName;
        }

        public void AddChild(NTree child)
        {
            Children.Add(child);
        }

        public void AddAttribute(string name, string value)
        {
            Attributes[name] = value;
        }
    }
}
