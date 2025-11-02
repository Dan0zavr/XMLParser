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
        private TreeNode _styleRoot;
        private TreeNode _numberingRoot;

        public BuildStyleDirector(TreeNode styleRoot, TreeNode numberingRoot)
        {
            _styleRoot = styleRoot;
            _numberingRoot = numberingRoot;
        }

        public (Dictionary<StyleCategory, TreeNode>, Dictionary<StyleCategory, TreeNode>) BuildAllStyles(Dictionary<IStyle, IStyleBuilder> styles)
        {
            Dictionary<StyleCategory, TreeNode> stylesResult = new Dictionary<StyleCategory, TreeNode>();
            Dictionary<StyleCategory, TreeNode> numberingStylesResult = new Dictionary<StyleCategory, TreeNode>();
            foreach (var (style, builder) in styles) 
            {
                if(style is TextStyle || style is ParagraphStyle || style is PictureStyle)
                {
                    KeyValuePair<StyleCategory, TreeNode> builtStyle = BuildUniqueSimpleStyle(style, builder);
                    stylesResult.Add(builtStyle.Key, builtStyle.Value);
                }
                else if(style is TableStyle)
                {
                    // проблема в том что они добавляются в общий пул и стратегия не может отличить что они относятся к таблице
                    foreach(var tableStyle in BuildUniqueTableStyles((TableStyle)style, builder))
                    {
                        stylesResult.Add(tableStyle.Key, tableStyle.Value);
                    }
                }
                else if(style is NumberingStyle)
                {
                    foreach(var numberingStyle in BuildUniqueNumberingStyles((NumberingStyle)style, (NumberingStyleBuilder)builder))
                    {
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
            StylesUniquelizer uniquelizer = new StylesUniquelizer(_styleRoot);
            StyleCategory category = Enum.Parse<StyleCategory>(styleParams.GetType().ToString());

            TreeNode style = builder.BuildStyle(styleParams);
            TreeNode styleNode = style.QuikBreadthFirstSearch("w:style").First();
            TreeNode nameNode = style.QuikBreadthFirstSearch("w:name").First();

            string styleName = uniquelizer.EnsureUniqueStyleName(_styleRoot, "w:style", "WordRegSimpleStyle");

            styleNode.Attributes["w:styleId"] = styleName;
            nameNode.Attributes["w:val"] = styleName;

            return new KeyValuePair<StyleCategory, TreeNode>(category, style);
        }

        private List<KeyValuePair<StyleCategory, TreeNode>> BuildUniqueTableStyles(TableStyle styleParams, IStyleBuilder builder) 
        { 
            // строим стили
            TreeNode tblStyle = builder.BuildStyle(styleParams);
            KeyValuePair<StyleCategory, TreeNode> tableStyle = new KeyValuePair<StyleCategory, TreeNode>(StyleCategory.TableStyle, tblStyle);
            KeyValuePair<StyleCategory, TreeNode> textTableStyle = BuildUniqueSimpleStyle(styleParams.TextStyle, new TextStyleBuilder());
            KeyValuePair<StyleCategory, TreeNode> paragraphTableStyle = BuildUniqueSimpleStyle(styleParams.ParagraphStyle, new ParagraphStyleBuilder());

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

        private List<KeyValuePair<StyleCategory, TreeNode>> BuildUniqueNumberingStyles(NumberingStyle styleParams, NumberingStyleBuilder builder)
        {
            TreeNode normalStyle = builder.BuildStyle(styleParams);
            TreeNode absStyle = builder.BuildAbstrtactStyle(styleParams);

            KeyValuePair<StyleCategory, TreeNode> style = new KeyValuePair<StyleCategory, TreeNode>(StyleCategory.NumberingStyle, normalStyle);
            KeyValuePair<StyleCategory, TreeNode> abstractStyle = new KeyValuePair<StyleCategory, TreeNode>(StyleCategory.Useless, absStyle);

            (style, abstractStyle) = builder.SyncId(style, abstractStyle, _numberingRoot);
            List<KeyValuePair<StyleCategory, TreeNode>> styles = new List<KeyValuePair<StyleCategory, TreeNode>>() {style, abstractStyle};

            return styles;
        }
    }
}
