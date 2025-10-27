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
            sectionProperties = QuikBreadthFirstSearch(paragraph, "w:sectPr").FirstOrDefault();
            if (sectionProperties != null)
                return true;

            foreach (TreeNode breakNode in QuikBreadthFirstSearch(paragraph, "w:br"))
            {
                if (breakNode.Attributes.TryGetValue("w:type", out string value) && value == "page")
                {
                    return true;
                }
            }
            return false;
        }

        private static TreeNode CreateBodyNode(List<TreeNode> children)
        {
            TreeNode node = new TreeNode
            {
                TagName = "w:body",
                CloseTag = true,
                Children = children
            };
            return node;
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

        public static void ReconstructParagraphs(TreeNode root, Dictionary<int, List<TreeNode>> extensiveParagrahps)
        {
            List<TreeNode> newRoot = new List<TreeNode>();
            List<TreeNode> oldParagraphs = root.Children;

            for (int i = 0; i < oldParagraphs.Count; i++)
            {
                if (extensiveParagrahps.Any(c => c.Key == i))
                {
                    foreach (var paragraph in extensiveParagrahps[i])
                    {
                        newRoot.Add(paragraph);
                    }
                }
                else
                {
                    newRoot.Add(oldParagraphs[i]);
                }
            }

            root.Children.Clear();
            root.Children = newRoot;
        }

        public static Dictionary<int, TreeNode> ExtractPicturesFromParagraphToDictionary(TreeNode root)
        {
            List<TreeNode> paragraphs = LongBreadthFirstSearch(root, "w:p");
            Dictionary<int, TreeNode> paragraphsWithPic = new Dictionary<int, TreeNode>();

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
                            paragraphsWithPic.Add(i, paragraphs[i]);
                            break;
                        }
                    }
                }
            }
            return paragraphsWithPic;
        }

        private static Dictionary<int, TreeNode> ClearParagraphsStyle(Dictionary<int, TreeNode> paragraphs)
        {
            Dictionary<int, TreeNode> nonStyleParagraphs = new Dictionary<int, TreeNode>();
            foreach (var paragraph in paragraphs)
            {
                paragraph.Value.TerminateSpecialCildren(paragraph.Value, "w:pPr");
                nonStyleParagraphs.Add(paragraph.Key, paragraph.Value);
            }
            return nonStyleParagraphs;
        }

        private static List<TreeNode> SeparateText(List<TreeNode> textParagraphs, List<TreeNode> paragraphsOneNumber, TreeNode paragraphStyle)
        {
            if (textParagraphs.Count == 0) return paragraphsOneNumber;

            var textPackage = new TreeNode()
            {
                TagName = "w:p",
                CloseTag = true,
                Children = new List<TreeNode> { paragraphStyle.Clone() } // Копия списка
            };
            textPackage.Children.AddRange(textParagraphs);
            paragraphsOneNumber.Add(textPackage);
            return paragraphsOneNumber;
        }

        private static (List<TreeNode> textParagraphs, List<TreeNode> paragrahpsOneNumber) SeparateDrawings(KeyValuePair<int, TreeNode> paragraph, TreeNode paragraphStyle)
        {
            List<TreeNode> paragraphsOneNumber = new List<TreeNode>(); // Список абзацев с одним номером
            List<TreeNode> textParagraphs = new List<TreeNode>(); // Список для текстовых абзацев

            for (int i = 0; i < paragraph.Value.Children.Count; i++)
            {
                TreeNode child = paragraph.Value.Children[i];

                if (child.Children.Any(c => c.TagName == "w:drawing"))
                {
                    // Если перед рисунком был текст, создаем новый текстовый абзац
                    paragraphsOneNumber = SeparateText(textParagraphs, paragraphsOneNumber, paragraphStyle);
                    textParagraphs.Clear();

                    // Создаем отдельный абзац для рисунка
                    TreeNode drawingPackage = CreateParagraphForDrawing(paragraphStyle, child);

                    paragraphsOneNumber.Add(drawingPackage);
                }
                else
                {
                    textParagraphs.Add(child);
                }
            }
            return (textParagraphs, paragraphsOneNumber);
        }

        private static TreeNode CreateParagraphForDrawing(TreeNode paragraphStyle, TreeNode child)
        {
            var drawingPackage = new TreeNode()
            {
                TagName = "w:p",
                CloseTag = true,
                Children = new List<TreeNode> { paragraphStyle.Clone(), child.Clone() }
            };
            return drawingPackage;
        }

        public static Dictionary<int, List<TreeNode>> SplitParagraphsWithDrawings(Dictionary<int, TreeNode> paragraphs)
        {
            if (paragraphs.Count == 0) return null;

            Dictionary<int, List<TreeNode>> splittedParagraphs = new Dictionary<int, List<TreeNode>>();
            TreeNode paragraphStyle = ExtractStyle(paragraphs.Values.First(), "w:pPr");
            Dictionary<int, TreeNode> nonStyleParagraphs = ClearParagraphsStyle(paragraphs);

            foreach (var paragraph in nonStyleParagraphs)
            {
                var (textParagraphs, paragrahpsOneNumber) = SeparateDrawings(paragraph, paragraphStyle);

                // Если остался текст без рисунков, добавляем его в отдельный абзац
                paragrahpsOneNumber = SeparateText(textParagraphs, paragrahpsOneNumber, paragraphStyle);

                splittedParagraphs.Add(paragraph.Key, paragrahpsOneNumber);
            }

            return splittedParagraphs;
        }

        private static TreeNode ExtractStyle(TreeNode parent, string styleTagName)
        {
            List<TreeNode> extractedStyles = QuikBreadthFirstSearch(parent, styleTagName);

            if (extractedStyles.Count == 1) return extractedStyles[0];
            else return null;
        }

        public static void CorrectParagraphChildren(string parentName, string tempFolder)
        {
            // Чтение XML-документа
            var (root, specialTokens) = XMLParser.XMLRead.ReadXMLDocument(document, tempFolder);

            // Поиск всех родительских элементов с указанным именем
            List<TreeNode> foundedParents = QuikBreadthFirstSearch(root, parentName);

            // Обработка каждого родительского элемента
            foreach (var parent in foundedParents)
            {
                // Пропускаем, если нет дочерних элементов
                if (parent.Children.Count == 0)
                {
                    continue;
                }

                // Находим первый элемент <w:pStyle> (если он есть)
                var pStyleNode = parent.Children.FirstOrDefault(child => child.TagName == "w:pStyle");

                if (pStyleNode != null)
                {

                    // Удаляем <w:pStyle> из текущей позиции
                    parent.Children.Remove(pStyleNode);

                    // Вставляем <w:pStyle> на первое место
                    parent.Children.Insert(0, pStyleNode);
                }
            }

            // Сериализация и десериализация дерева
            //string serializedTree = xmlRead.SerializeNode(root, specialTokens);
            //xmlRead.StringToXMLDocument(serializedTree, document, tempFolder);
        }
    }
}
