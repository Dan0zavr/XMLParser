using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static XMLParser.TreeNode;

namespace XMLParser
{
    public static class DocumentComposer
    {
        private const string document = "document.xml";

        public static (TreeNode titlePage, TreeNode content, TreeNode mainTag) SplitDocument(TreeNode root, bool splitDocument)
        {
            if (!splitDocument)
            {
                TreeNode mTag = DuplicateNodeWithAttributes(root);

                TreeNode tPage = DuplicateNodeWithAttributes(root);

                return (tPage, root, mTag);
            }

            List<TreeNode> titlePageChildren = new List<TreeNode>();
            List<TreeNode> contentChildren = new List<TreeNode>();
            TreeNode? sectionProperties = null;

            bool pageBreakFound = false;

            if (root.Children.Count == 0 || root.Children[0].Children == null)
            {
                // Пустой документ
                return (new TreeNode { TagName = "w:body", CloseTag = true },
                        new TreeNode { TagName = "w:body", CloseTag = true },
                        new TreeNode { TagName = root.TagName, CloseTag = true, Attributes = root.Attributes });
            }

            foreach (TreeNode paragraph in root.Children[0].Children)
            {
                // Проверяем наличие разрыва страницы или секции
                if (!pageBreakFound) pageBreakFound = TryFindPageBreak(paragraph, out sectionProperties);

                if (!pageBreakFound) titlePageChildren.Add(paragraph);
                else contentChildren.Add(paragraph);
            }

            // Если разрыва не было найдено, значит титульной части нет — всё идёт в content
            if (!pageBreakFound)
            {
                contentChildren = titlePageChildren;
                titlePageChildren = new List<TreeNode>();
            }

            // Убедимся, что sectionProperties присутствует в конце
            if (sectionProperties != null && !contentChildren.Contains(sectionProperties))
            {
                contentChildren.Add(sectionProperties);
            }

            TreeNode titlePage = CreateBodyNode(titlePageChildren);
            TreeNode content = CreateBodyNode(contentChildren);

            TreeNode mainTag = DuplicateNodeWithAttributes(root);

            return (titlePage, content, mainTag);
        }

        private static bool TryFindPageBreak(TreeNode paragraph, out TreeNode sectionProperties)
        {
            sectionProperties = paragraph.QuikBreadthFirstSearch("w:sectPr").FirstOrDefault();
            if (sectionProperties != null)
                return true;

            foreach (TreeNode breakNode in paragraph.QuikBreadthFirstSearch("w:br"))
            {
                if (breakNode.Attributes.TryGetValue("w:type", out string value) && value == "page")
                {
                    return true;
                }
            }
            return false;
        }

        private static TreeNode DuplicateNodeWithAttributes(TreeNode node)
        {
            TreeNode duplicatedNode = new TreeNode
            {
                TagName = node.TagName,
                CloseTag = true,
                Attributes = node.Attributes
            };
            return duplicatedNode;
        }

        public static TreeNode MergeDocument(TreeNode titlePage, TreeNode content, TreeNode mainTag)
        {
            TreeNode document = new TreeNode()
            {
                TagName = "w:body",
                CloseTag = true,
            };
            document.Children.AddRange(titlePage.Children);
            document.Children.AddRange(content.Children);
            mainTag.Children.Add(document);

            return mainTag;
        }

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

                        List<TreeNode> allDrawingParagraphs = AddEmptyParagraphsAroundPicture(drawingParagraph);
                        foreach (var paragraph_ in allDrawingParagraphs)
                        {
                            sameNumberParagraphs.Add(paragraph_);
                        }
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

        private static List<TreeNode> AddEmptyParagraphsAroundPicture(TreeNode picture)
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

        private static TreeNode CreateParagraphNode()
        {
            return new TreeNode()
            {
                TagName = "w:p",
                CloseTag = true,
                Children = new List<TreeNode>()
            };
        }

        private static TreeNode CreateParagraphStyleNode()
        {
            return new TreeNode()
            {
                TagName = "w:pPr",
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
