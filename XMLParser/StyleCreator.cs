using XMLParser.Styles;

namespace XMLParser
{
    public class StyleCreator : TreeNode
    {
        public (TreeNode, TreeNode) CreateParagraphStyleInFile(ParagraphStyle paragraphStyle, TreeNode root)
        {
            TreeNode styleNode = new TreeNode();
            
            styleNode = CreateParagraphStyleNode(paragraphStyle.CreateParagraphStyle(paragraphStyle), root);

            root.Children.Add(styleNode);

            return (styleNode, root);
        }

        public (TreeNode, TreeNode) CreateTextStyleInFile(TextStyle textStyle, TreeNode root)
        {
            TreeNode styleNode = new TreeNode();

            styleNode = CreateTextStyleNode(textStyle.CreateTextStyle(textStyle), root);

            root.Children.Add(styleNode);

            return (styleNode, root);
        }

        public (TreeNode, TreeNode) CreateTableStyleInFile(TableStyle tableStyle, TreeNode root)
        {
            TreeNode styleNode = new TreeNode();

            styleNode = CreateTableStyleNode(tableStyle.CreateTableStyle(tableStyle), root);

            root.Children.Add(styleNode);

            return (styleNode, root);

        }

        public (TreeNode, TreeNode) CreateNumberingStyleInFile(NumberingStyle style, TreeNode root)
        {
            var (numberingStyle, appliedStyle) = CreateNumberingStyleNodes(style.CreateNumberingStyle(), root);

            root.Children.Add(numberingStyle);
            root.Children.Add(appliedStyle);

            return (appliedStyle, root);
        }
    }
}
