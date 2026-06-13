using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.SpecialClasses.Tree;
using XMLParser.Styles;
using static XMLParser.SpecialClasses.Tree.TreeNode;

namespace XMLParser.SpecialClasses.DocumentChangers
{
    public static class CaptionAdder
    {
        public static void AddTableCaption(TreeNode body, TableStyle style)
        {
            TreeNode captionNode = CreateCaptionNode();

            List<int> tablesPositions = new List<int>();
            
            for(int i = 0; i < body.Children.Count; i++)
            {
                if (body.Children[i].TagName == "w:tbl")
                {
                    tablesPositions.Add(i);
                }
            }

            int counter = tablesPositions.Count;

            foreach (var position in tablesPositions.OrderByDescending(x => x))
            {
                TreeNode captionParagraph = CreateParagrphNode();

                TreeNode pPr = captionParagraph.LongBreadthFirstSearch("w:pPr").First();
                pPr.Children.Add(new TreeNode { TagName = "w:keepNext" });

                TreeNode currentCaption = captionNode.Clone();

                TreeNode labelNode = currentCaption.QuikBreadthFirstSearch("w:t").First();

                string label = style.LabelValue.Replace(IStyle.NumMarker, counter.ToString());

                labelNode.Values.Clear();
                labelNode.Values.Add(label);

                captionParagraph.Children.Add(currentCaption);

                body.Children.Insert(position, captionParagraph);
                counter--;
            }
        }

        public static void AddPictureCaption(Dictionary<int, List<TreeNode>> extendedParagaphs, PictureStyle style)
        {
            int counter = 1;
            TreeNode captionNode = CreateCaptionNode();

            foreach (var sameNumberParagraphs in extendedParagaphs.Values) 
            {
                for (int i = sameNumberParagraphs.Count - 1; i >= 0; i--) {
                    if (sameNumberParagraphs[i].QuikBreadthFirstSearch("w:drawing").Count > 0)
                    {
                        TreeNode captionParagraph = CreateParagrphNode();

                        TreeNode currentCaption = captionNode.Clone();

                        //Устанавливаем содержимое подписи
                        TreeNode labelNode = currentCaption.QuikBreadthFirstSearch("w:t").First();
                        string label = style.LabelValue;
                        label = label.Replace(IStyle.NumMarker, counter.ToString());
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

        private static TreeNode CreateParagrphNode()
        {
            return new TreeNode()
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
        }

        private static TreeNode CreateCaptionNode()
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
