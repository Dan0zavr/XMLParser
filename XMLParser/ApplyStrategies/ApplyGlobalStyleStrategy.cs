using System;
using System.Collections.Generic;
using System.Text;

namespace XMLParser.ApplyStrategies
{
    public class ApplyGlobalStyleStrategy : ApplyStrategy
    {
        public override void Apply(TreeNode root, TreeNode style)
        {
            TreeNode sectPr = root.QuikBreadthFirstSearch("w:sectPr").FirstOrDefault();

            if (sectPr == null) throw new Exception("Не найден конец документа");

            for(int i = 0; i < sectPr.Children.Count; i++)
            {
                if (sectPr.Children[i].TagName == "w:pgMar")
                {
                    sectPr.Children[i] = style.LongBreadthFirstSearch("w:pgMar").First();
                }
            }
        }
    }
}
