using XMLParser.Styles;

namespace XMLParser
{
    public class StyleCreator : TreeNode
    {
        public TreeNode CreateParagraphStyleNode(List<TreeNode> paragraphStyleChildren, TreeNode root)
        {
            string tagName = "w:pPr";
            string styleType = "paragraph";
            string styleName = EnsureUniqueStyleName(root, "w:style", "WordRegParagraphStyle");

            //Формирование тега для имени стиля
            TreeNode styleIdAndName = new TreeNode()
            {
                TagName = $"w:name",
                Attributes = { { "w:val", styleName } },
                CloseTag = false
            };

            //Формирование тега, содержащего параметры стиля
            TreeNode parent = new TreeNode()
            {
                TagName = tagName,
                Children = paragraphStyleChildren,
                CloseTag = true
            };

            List<TreeNode> paragraphParent = new List<TreeNode>();
            List<TreeNode> name = new List<TreeNode>();
            paragraphParent.Add(parent);
            name.Add(styleIdAndName);

            //Формирование и заполнение тега для стиля
            TreeNode styleNode = new TreeNode()
            {
                TagName = "w:style",
                Attributes =
                {
                    {"w:type", styleType },
                    {"w:styleId", styleName }
                },
                Children = name.Union(paragraphParent).ToList(),
                CloseTag = true
            };

            return styleNode;

        }

        public TreeNode CreateTextStyleNode(List<TreeNode> textStyleChildren, TreeNode root)
        {
            string tagName = "w:rPr";
            string styleType = "character";
            string styleName = EnsureUniqueStyleName(root, "w:style", "WordRegTextStyle");

            //Формирование тега для имени стиля
            TreeNode styleIdAndName = new TreeNode()
            {
                TagName = $"w:name",
                Attributes = { { "w:val", styleName } },
                CloseTag = false
            };

            //Формирование тега, содержащего параметры стиля
            TreeNode parent = new TreeNode()
            {
                TagName = tagName,
                Children = textStyleChildren,
                CloseTag = true
            };

            List<TreeNode> textParent = new List<TreeNode>();
            List<TreeNode> name = new List<TreeNode>();
            TreeNode styleNode = new TreeNode();
            textParent.Add(parent);
            name.Add(styleIdAndName);


            //Формирование и заполнение тега для стиля
            styleNode = new TreeNode()
            {
                TagName = "w:style",
                Attributes =
                {
                    {"w:type", styleType },
                    {"w:styleId", styleName }
                },
                Children = name.Union(textParent).ToList(),
                CloseTag = true
            };

            return styleNode;
        }

        public (TreeNode, TreeNode) CreateNumberingStyleNodes(List<TreeNode> styleChildren, TreeNode root)
        {
            string numberingId = EnsureUniqueNumberingId(root);

            // Создаем узел `w:abstractNum`, содержащий все уровни `w:lvl`
            TreeNode mainStyleNode = new TreeNode
            {
                TagName = "w:abstractNum",
                Attributes = { { "w:abstractNumId", numberingId } },
                CloseTag = true
            };

            foreach (TreeNode childNode in styleChildren)
            {
                mainStyleNode.AddChild(mainStyleNode, childNode);
            }

            // Создаем узел `w:num`, который ссылается на `w:abstractNum`
            TreeNode appliedStyle = new TreeNode
            {
                TagName = "w:num",
                Attributes = { { "w:numId", EnsureUniqueNumberingId(root) } },
                Children =
            {
                new TreeNode
                {
                    TagName = "w:abstractNumId",
                    Attributes = { { "w:val", numberingId } }
                }
            },
                CloseTag = true
            };

            return (mainStyleNode, appliedStyle);
        }

        public TreeNode CreateTableStyleNode(List<TreeNode> tableStyle, TreeNode root)
        {
            string tableName = EnsureUniqueStyleName(root, "w:style", "WordRegTableStyle");
            TreeNode tableNameNode = new TreeNode()
            {
                TagName = "w:name",
                Attributes = { { "w:val", tableName } }
            };

            TreeNode style = new TreeNode()
            {
                TagName = "w:style",
                Attributes = { { "w:type", "table" }, { "w:styleId", tableName } },
                Children = new List<TreeNode>(),
                CloseTag = true
            };

            style.Children.Add(tableNameNode);

            foreach (TreeNode childNode in tableStyle)
            {
                style.Children.Add(childNode);
            }

            return style;
        }

        public string EnsureUniqueStyleName(TreeNode root, string tag, string startName)
        {
            string baseName = startName; // Базовое имя стиля
            string styleName = baseName; // Начинаем с базового имени
            int counter = 1; // Счетчик для добавления суффикса

            // Поиск всех узлов с указанным тегом
            List<TreeNode> styles = LongBreadthFirstSearch(root, tag);

            for (int i = 0; i < styles.Count; i++)
            {
                styles[i].Attributes.TryGetValue("w:styleId", out string styleId);
                if (styleId == styleName)
                {
                    styleName = baseName + counter++;
                }
            }

            return styleName; // Возвращаем уникальное имя
        }

        public string EnsureUniqueNumberingId(TreeNode root)
        {
            int counter = 0;
            string id = counter.ToString();

            List<TreeNode> styles = LongBreadthFirstSearch(root, "w:abstractNum");

            foreach (var style in styles)
            {
                if (style.Attributes.TryGetValue("w:abstractNumId", out string styleId) && styleId == id)
                {
                    counter++;
                    id = counter.ToString();
                }
            }

            return id;
        }

        public (TreeNode, TreeNode) CreateParagraphStyleInFile(ParagraphStyle paragraphStyle, TreeNode root)
        {
            TreeNode styleNode = new TreeNode();
            
            styleNode = CreateParagraphStyleNode(paragraphStyle.CreateParagraphStyle(paragraphStyle), root);

            AddChild(root, styleNode);

            return (styleNode, root);
        }

        public (TreeNode, TreeNode) CreateTextStyleInFile(TextStyle textStyle, TreeNode root)
        {
            TreeNode styleNode = new TreeNode();

            styleNode = CreateTextStyleNode(textStyle.CreateTextStyle(textStyle), root);

            AddChild (root, styleNode);

            return (styleNode, root);
        }

        public (TreeNode, TreeNode) CreateTableStyleInFile(TableStyle tableStyle, TreeNode root)
        {
            TreeNode styleNode = new TreeNode();

            styleNode = CreateTableStyleNode(tableStyle.CreateTableStyle(tableStyle), root);

            AddChild(root, styleNode);

            return (styleNode, root);

        }

        public (TreeNode, TreeNode) CreateNumberingStyleInFile(NumberingStyle style, TreeNode root)
        {
            var (numberingStyle, appliedStyle) = CreateNumberingStyleNodes(style.CreateNumberingStyle(), root);

            AddChild(root, numberingStyle);
            AddChild(root, appliedStyle);

            return (appliedStyle, root);
        }
    }
}
