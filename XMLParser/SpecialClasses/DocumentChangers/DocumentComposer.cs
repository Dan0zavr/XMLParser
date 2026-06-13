using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using XMLParser.SpecialClasses.Tree;
using XMLParser.Styles;
using static XMLParser.SpecialClasses.Tree.TreeNode;

namespace XMLParser.SpecialClasses.DocumentChangers
{
    public static class DocumentComposer
    {
        private const string document = "document.xml";

        //ключ соответствует номеру абзаца, начиная с 0
        public static Dictionary<int, TreeNode> ExtractPicturesFromParagraphToDictionary(TreeNode body)
        {
            List<TreeNode> paragraphs = body.Children;
            Dictionary<int, TreeNode> paragraphsWithPic = new Dictionary<int, TreeNode>();

            //проход по <w:p>
            for (int i = 0; i < paragraphs.Count; i++)
            {
                List<TreeNode> drawings = paragraphs[i].LongBreadthFirstSearch("w:drawing");
                if(drawings.Count > 0)
                {
                    paragraphsWithPic.Add(i, paragraphs[i]);
                }
            }
            return paragraphsWithPic;
        }

        public static void ReconstructParagraphs(TreeNode body, Dictionary<int, List<TreeNode>> extendedParagraphs)
        {
            List<TreeNode> paragraphs = body.Children;
            Queue<TreeNode> newParagrapsStructure = new Queue<TreeNode>();
            for (int i = 0; i < paragraphs.Count; i++)
            {
                if (extendedParagraphs.ContainsKey(i))
                {
                    foreach (var extendedParagraph in extendedParagraphs[i])
                    {
                        newParagrapsStructure.Enqueue(extendedParagraph);
                    }
                }
                else
                {
                    newParagrapsStructure.Enqueue(paragraphs[i]);
                }
            }

            body.Children.Clear();

            while (newParagrapsStructure.Count > 0)
            {
                body.Children.Add(newParagrapsStructure.Dequeue());
            }
        }

        public static Dictionary<int, List<TreeNode>> SeparateDrawingsAndText(Dictionary<int, TreeNode> paragraphs)
        {
            Dictionary<int, List<TreeNode>> separatedParagraphs = new Dictionary<int, List<TreeNode>>();

            foreach (var paragraph in paragraphs) 
            {
                //сохраняем стиль абзаца
                TreeNode paragraphStyleNode = ExtractStyle(paragraph.Value, "w:pPr");

                //сюда добавляются разделенные текстовые запуки (тег "w:r")
                TreeNode textParagraph = CreateParagraphNode();

                List<TreeNode> sameNumberParagraphs = new List<TreeNode>();

                foreach (var runNode in paragraph.Value.QuikBreadthFirstSearch("w:r"))
                {
                    if (runNode.CheckChild("w:drawing", out TreeNode foundedChild))
                    {
                        CloseTextParagraph(sameNumberParagraphs, textParagraph);
                        TreeNode drawingParagraph = CreateParagraphNode();

                        drawingParagraph.Children.Add(runNode);
                        drawingParagraph.Children.Insert(0, paragraphStyleNode);

                        sameNumberParagraphs.Add(drawingParagraph);
                    }
                    else
                    {
                       textParagraph.Children.Add(runNode);
                    }

                    CloseTextParagraph(sameNumberParagraphs, textParagraph);
                }
                separatedParagraphs.Add(paragraph.Key, sameNumberParagraphs);
            }

            return separatedParagraphs;
        }

        private static List<TreeNode> AddEmptyParagraphsAroundParagraph(TreeNode picture)
        {
            TreeNode firstParagraphNode = CreateParagraphNode();
            TreeNode lastParagraphNode = CreateParagraphNode();
            return new List<TreeNode> { firstParagraphNode, picture, lastParagraphNode};
        }

        //добавляет абзац в список с одинаковым номером и очищает его потомков
        private static void CloseTextParagraph(List<TreeNode> sameNumberParagraphs, TreeNode textParagraph)
        {
            if (textParagraph.Children.Count > 0)
            {
                sameNumberParagraphs.Add(textParagraph.Clone());
                textParagraph.Children.Clear();
            }
        }

        public static TreeNode CreateParagraphNode()
        {
            return new TreeNode()
            {
                TagName = "w:p",
                CloseTag = true,
                Children = new List<TreeNode>()
            };
        }

        public static TreeNode CreateParagraphStyleNode()
        {
            return new TreeNode()
            {
                TagName = "w:pPr",
                CloseTag = true,
                Children = new List<TreeNode>()
            };
        }

        public static TreeNode CreateTextStyleNode()
        {
            return new TreeNode()
            {
                TagName = "w:rPr",
                CloseTag = true,
                Children = new List<TreeNode>()
            };
        }

        private static TreeNode CreateBodyNode(List<TreeNode> children)
        {
            return new TreeNode
            {
                TagName = "w:body",
                CloseTag = true,
                Children = children
            };
        }

        private static TreeNode ExtractStyle(TreeNode parent, string styleTagName)
        {
            List<TreeNode> extractedStyles = parent.QuikBreadthFirstSearch(styleTagName);

            if (extractedStyles.Count == 1)
            {
                return extractedStyles[0];
            }
            else
            {
                return CreateParagraphStyleNode();
            }
        }
    }
}
