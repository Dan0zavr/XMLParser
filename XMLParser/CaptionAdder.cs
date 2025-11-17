using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;
using static XMLParser.TreeNode;

namespace XMLParser
{
    public static class CaptionAdder
    {
        public static void AddCaption(TreeNode root, PictureStyle style)
        {
            int counter = 1;
            TreeNode captionNode = CreateCaptionNode(style);
            List<TreeNode> paragraphs = root.LongBreadthFirstSearch("w:p");
            List<TreeNode> drawingParagraphs = new List<TreeNode>();

            foreach (TreeNode paragraph in paragraphs) 
            {
                if (paragraph.ContainsChild("w:drawing"))
                {
                    drawingParagraphs.Add(paragraph);
                }
            }

            foreach (TreeNode paragraph in drawingParagraphs)
            {
                TreeNode currentCaption = captionNode.Clone();
                TreeNode labelNode = currentCaption.QuikBreadthFirstSearch("w:t").First();
                string label = style.LabelValue;
                label = label.Replace(PictureStyle.NumMarker, counter.ToString());
                labelNode.Values.Clear();
                labelNode.Values.Add(label);

                paragraph.Children.Add(currentCaption);
                counter++;
            }
        }

        private static TreeNode CreateCaptionNode(PictureStyle style)
        {
            TreeNode label = new TreeNode()
            {
                TagName = "w:t",
                Values = new List<string>(),
                CloseTag = true
            };

            TreeNode rPr = new TreeNode()
            {
                TagName = "w:rPr",
                CloseTag = true
            };

            return new TreeNode() {
                    TagName = "w:r",
                    Children = new List<TreeNode>() { rPr, label },
                    CloseTag = true
                };
        }
    }
}
