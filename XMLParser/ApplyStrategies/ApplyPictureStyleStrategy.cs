using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static XMLParser.TreeNode;

namespace XMLParser.ApplyStrategies
{
    public class ApplyPictureStyleStrategy : ApplyStrategy
    {
        public override void Apply(TreeNode root, TreeNode style)
        {
            List<TreeNode> paragraphsWithDrawings = ExtractPicturesFromParagraphToList(root);

            foreach (var paragraph in paragraphsWithDrawings)
            {
                List<TreeNode> oldStyles = paragraph.QuikBreadthFirstSearch("w:pPr");

                paragraph.TerminateChildren(oldStyles);

                TreeNode styleToApply = CreateStyleToApply(style);

                foreach (TreeNode styleElement in oldStyles)
                {
                    styleElement.Children.Add(styleToApply);
                }
            }
        }

        private List<TreeNode> ExtractPicturesFromParagraphToList(TreeNode root)
        {
            List<TreeNode> paragraphs = root.LongBreadthFirstSearch("w:p");
            List<TreeNode> paragraphsWithPic = new List<TreeNode>();

            //проход по <w:p>
            for (int i = 0; i < paragraphs.Count; i++)
            {
                //грубо говоря проход по <w:r>
                for (int j = 0; j < paragraphs[i].Children.Count; j++)
                {
                    //поиск <w:drawing>
                    for (int k = 0; k < paragraphs[i].Children[j].Children.Count; k++)
                    {
                        if (paragraphs[i].Children[j].Children[k].TagName == "w:drawing")
                        {
                            paragraphsWithPic.Add(paragraphs[i]);
                            if(i + 1 < paragraphs.Count) // это для подписи к рисункам, позже может сделать выключаемой функцией
                            {
                                paragraphsWithPic.Add(paragraphs[i + 1]);
                            }
                            break;
                        }
                    }
                }
            }
            return paragraphsWithPic;
        }

        private TreeNode CreateStyleToApply(TreeNode style)
        {
            TreeNode styleToApply = new TreeNode()
            {
                TagName = "w:pStyle",
                Attributes = { { "w:val", style.Attributes["w:styleId"] } },
                CloseTag = false
            };

            return styleToApply;
        }
    }
}
