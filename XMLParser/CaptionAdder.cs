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
        public static Dictionary<int, List<TreeNode>> AddCaption(TreeNode root, PictureStyle style)
        {
            int counter = 1;
            int paragraphNumber = 0;
            TreeNode captionNode = CreateCaptionNode(style);
            List<TreeNode> paragraphs = root.LongBreadthFirstSearch("w:p");
            List<TreeNode> drawingParagraphs = new List<TreeNode>();
            Dictionary<int, List<TreeNode>> newParagraphs = new Dictionary<int, List<TreeNode>>();

            foreach (TreeNode paragraph in paragraphs) 
            {               
                if (paragraph.ContainsChild("w:drawing"))
                {
                    TreeNode captionParagraph = new TreeNode() 
                    { 
                        TagName = "w:p",
                        Children = new List<TreeNode>{
                            new TreeNode
                            {
                                TagName = "w:pPr",
                                Children = new List<TreeNode>(),
                                CloseTag = true
                            }
                        },
                        CloseTag = true
                    };

                    TreeNode currentCaption = captionNode.Clone();
                    TreeNode labelNode = currentCaption.QuikBreadthFirstSearch("w:t").First();
                    string label = style.LabelValue;
                    label = label.Replace(PictureStyle.NumMarker, counter.ToString());
                    labelNode.Values.Clear();
                    labelNode.Values.Add(label);

                    captionParagraph.Children.Add(currentCaption);
                    counter++;

                    newParagraphs.Add(paragraphNumber, new List<TreeNode> { paragraph, captionParagraph });
                }
                else
                {
                    newParagraphs.Add(paragraphNumber, new List<TreeNode> { paragraph});
                }
                paragraphNumber++;
            }
            return newParagraphs;
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
