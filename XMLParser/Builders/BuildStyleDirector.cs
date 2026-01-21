using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;
using static XMLParser.TreeNode;

namespace XMLParser.Builders
{
    public class BuildStyleDirector
    {
        private readonly TreeNode _styleRoot;
        private readonly TreeNode _numberingRoot;
        private readonly StylesUniquelizer _uniquelizer;

        public BuildStyleDirector(TreeNode styleRoot, TreeNode numberingRoot, StylesUniquelizer uniquelizer)
        {
            _styleRoot = styleRoot;
            _numberingRoot = numberingRoot;
            _uniquelizer = uniquelizer;
        }

        public (Dictionary<StyleCategory, TreeNode>, Dictionary<StyleCategory, TreeNode>) BuildAllStyles(Dictionary<IStyle, IStyleBuilder> styles)
        {
            Dictionary<StyleCategory, TreeNode> stylesResult = new Dictionary<StyleCategory, TreeNode>();
            Dictionary<StyleCategory, TreeNode> numberingStylesResult = new Dictionary<StyleCategory, TreeNode>();
            foreach (var (style, builder) in styles) 
            {
                if(style is TextStyle || style is ParagraphStyle)
                {
                    KeyValuePair<StyleCategory, TreeNode> builtStyle = BuildUniqueSimpleStyle(style, builder);
                    stylesResult.Add(builtStyle.Key, builtStyle.Value);
                }
                else if(style is PictureStyle pictureStyle)
                {
                    KeyValuePair<StyleCategory, TreeNode> builtStyle = BuildUniqueSimpleStyle(pictureStyle.ParagraphStyle, builder);
                    stylesResult.Add(StyleCategory.PictureStyle, builtStyle.Value);
                }
                else if (style is TableStyle)
                {
                    foreach (var tableStyle in BuildUniqueTableStyles((TableStyle)style, builder))
                    {
                        stylesResult.Add(tableStyle.Key, tableStyle.Value);
                    }
                }
                else if (style is NumberingStyle)
                {
                    NumberingStyle numStyle = (NumberingStyle)style;
                    List<NumberingElementStyle> elements = numStyle.GetElements();
                    foreach (var element in elements)
                    {
                        var numberingStyle = BuildUniqueNumberingStyles(element, (NumberingStyleBuilder)builder); 
                        numberingStylesResult.Add(numberingStyle.Key, numberingStyle.Value);
                    }
                }
                else
                {
                    throw new Exception($"Необработанный стиль {style.GetType()}");
                }
            }

            return (stylesResult, numberingStylesResult);
        }

        private KeyValuePair<StyleCategory, TreeNode> BuildUniqueSimpleStyle(IStyle styleParams, IStyleBuilder builder)
        {
            StyleCategory category = Enum.Parse<StyleCategory>(styleParams.GetType().Name.ToString());

            TreeNode styleNode = builder.BuildStyle(styleParams);
            TreeNode nameNode = styleNode.QuikBreadthFirstSearch("w:name").First();

            string styleName = _uniquelizer.EnsureUniqueStyleName(_styleRoot, "w:style", "WordRegSimpleStyle");

            styleNode.Attributes["w:styleId"] = styleName;
            nameNode.Attributes["w:val"] = styleName;

            return new KeyValuePair<StyleCategory, TreeNode>(category, styleNode);
        }

        private List<KeyValuePair<StyleCategory, TreeNode>> BuildUniqueTableStyles(TableStyle styleParams, IStyleBuilder builder) 
        { 
            // строим стили
            TreeNode tblStyle = builder.BuildStyle(styleParams);
            KeyValuePair<StyleCategory, TreeNode> tableStyle = new KeyValuePair<StyleCategory, TreeNode>(StyleCategory.TableStyle, tblStyle);
            KeyValuePair<StyleCategory, TreeNode> textTableStyle = new KeyValuePair<StyleCategory, TreeNode>(StyleCategory.TableTextStyle, BuildUniqueSimpleStyle(styleParams.TextStyle, new TextStyleBuilder()).Value);
            KeyValuePair<StyleCategory, TreeNode> paragraphTableStyle = new KeyValuePair<StyleCategory, TreeNode>(StyleCategory.TableParagraphStyle, BuildUniqueSimpleStyle(styleParams.ParagraphStyle, new ParagraphStyleBuilder()).Value);

            // ищем ноды в котрых заглушка
            TreeNode textLinkInTable = tblStyle.QuikBreadthFirstSearch("w:rStyle").First();
            TreeNode paragraphLinkInTable = tblStyle.QuikBreadthFirstSearch("w:pStyle").First();

            // ищем имена стилей
            TreeNode textTableStyleNameNode = textTableStyle.Value.QuikBreadthFirstSearch("w:name").First();
            TreeNode paragraphTableStyleNameNode = paragraphTableStyle.Value.QuikBreadthFirstSearch("w:name").First();

            // меняем заглушку на имя стиля
            textLinkInTable.Attributes["w:val"] = textTableStyleNameNode.Attributes["w:val"];
            paragraphLinkInTable.Attributes["w:val"] = paragraphTableStyleNameNode.Attributes["w:val"];

            List<KeyValuePair<StyleCategory, TreeNode>> styles = new List<KeyValuePair<StyleCategory, TreeNode>>() {tableStyle, textTableStyle, paragraphTableStyle };

            return styles;
        }

        private KeyValuePair<StyleCategory, TreeNode> BuildUniqueNumberingStyles(NumberingElementStyle styleParams, NumberingStyleBuilder builder)
        {
            TreeNode absStyle = builder.BuildStyle(styleParams);

            StyleCategory numFmt = DetermineNumberingFormat(absStyle);

            KeyValuePair<StyleCategory, TreeNode> styles = new KeyValuePair<StyleCategory, TreeNode>(numFmt, absStyle);

            return styles;
        }

        private StyleCategory DetermineNumberingFormat(TreeNode abstractStyle)
        { 
            string numberingFormat = abstractStyle.QuikBreadthFirstSearch("w:numFmt").First().Attributes["w:val"];
            switch (numberingFormat)
            {
                case "bullet":
                    return StyleCategory.NumberingStyleMarked;
                case "decimal":
                    return StyleCategory.NumberingStyleNumbered;
                default:
                    throw new NotImplementedException("Такой формат списков не найден");
            }
        }
    }
}
