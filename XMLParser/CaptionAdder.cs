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
        public static void AddCaption(Dictionary<int, List<TreeNode>> extendedParagaphs, PictureStyle style)
        {
            int counter = 1;
            TreeNode captionNode = CreateCaptionNode(style);

            foreach (var sameNumberParagraphs in extendedParagaphs.Values) 
            {
                for (int i = sameNumberParagraphs.Count - 1; i >= 0; i--) {
                    if (sameNumberParagraphs[i].QuikBreadthFirstSearch("w:drawing").Count > 0)
                    {
                        TreeNode captionParagraph = new TreeNode()
                        {
                            TagName = "w:p",
                            Children = new List<TreeNode>
                            {
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

                        //Устанавливаем содержимое подписи
                        TreeNode labelNode = currentCaption.QuikBreadthFirstSearch("w:t").First();
                        string label = style.LabelValue;
                        label = label.Replace(PictureStyle.NumMarker, counter.ToString());
                        labelNode.Values.Clear();
                        labelNode.Values.Add(label);

                        captionParagraph.Children.Add(currentCaption);
                        counter++;
                        int j = i;
                        sameNumberParagraphs.Insert(++j, captionParagraph);
                    }
                }
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
