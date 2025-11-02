using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static XMLParser.TreeNode;

namespace XMLParser
{
    public class StylesUniquelizer
    {
        public List<string> Names = new List<string>();

        public StylesUniquelizer(TreeNode root) 
        {
            List<TreeNode> nodes = root.LongBreadthFirstSearch("w:style");

            foreach (TreeNode node in nodes) 
            {
                Names.Add(node.Attributes.GetValueOrDefault("w:styleId"));
            }
        }

        public string EnsureUniqueStyleName(TreeNode root, string tag, string startName)
        {
            string baseName = startName; // Базовое имя стиля
            string styleName = baseName; // Начинаем с базового имени
            int counter = 1; // Счетчик для добавления суффикса 

            while (Names.Contains(styleName))
            {
                styleName = baseName + counter++;
            }

            Names.Add(styleName);
            return styleName;
        }
    }
}
