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
        private StylesUniquelizer _uniquelizer;
        private TreeNode _styleRoot;
        private TreeNode _numberingRoot;

        public BuildStyleDirector(StylesUniquelizer uniquelizer, TreeNode styleRoot, TreeNode numberingRoot)
        {
            _uniquelizer = uniquelizer;
            _styleRoot = styleRoot;
            _numberingRoot = numberingRoot;
        }

        public (List<TreeNode>, List<TreeNode>) BuildAllStyles(Dictionary<IStyle, IStyleBuilder> styles)
        {
            List<TreeNode> stylesResult = new List<TreeNode>();
            List<TreeNode> numberingStylesResult = new List<TreeNode>();
            foreach (var (style, builder) in styles) 
            {
                if(style is TextStyle || style is ParagraphStyle || style is PictureStyle)
                {
                    stylesResult.Add(BuildUniqueSimpleStyle(style, builder));
                }
                else if(style is TableStyle)
                {
                    foreach(var tableStyle in BuildUniqueTableStyles((TableStyle)style, builder))
                    {
                        stylesResult.Add(tableStyle);
                    }
                }
                else if(style is NumberingStyle)
                {
                    foreach(var numberingStyle in BuildUniqueNumberingStyles((NumberingStyle)style, (NumberingStyleBuilder)builder))
                    {
                        numberingStylesResult.Add(numberingStyle);
                    }
                }
                else
                {
                    throw new Exception($"Необработанный стиль {style.GetType()}");
                }
            }

            return (stylesResult, numberingStylesResult);
        }

        private TreeNode BuildUniqueSimpleStyle(IStyle styleParams, IStyleBuilder builder)
        {
            TreeNode style = builder.BuildStyle(styleParams);
            TreeNode styleNode = QuikBreadthFirstSearch(style, "w:style").First();
            TreeNode nameNode = QuikBreadthFirstSearch(style, "w:name").First();

            string styleName = _uniquelizer.EnsureUniqueStyleName(_styleRoot, "w:style", "WordRegSimpleStyle");

            styleNode.Attributes["w:styleId"] = styleName;
            nameNode.Attributes["w:val"] = styleName;

            return style;
        }

        private List<TreeNode> BuildUniqueTableStyles(TableStyle styleParams, IStyleBuilder builder) 
        { 
            // строим стили
            TreeNode tableStyle = builder.BuildStyle(styleParams);
            TreeNode textTableStyle = BuildUniqueSimpleStyle(styleParams.TextStyle, new TextStyleBuilder());
            TreeNode paragraphTableStyle = BuildUniqueSimpleStyle(styleParams.ParagraphStyle, new ParagraphStyleBuilder());

            // ищем ноды в котрых заглушка
            TreeNode textLinkInTable = QuikBreadthFirstSearch(tableStyle, "w:rStyle").First();
            TreeNode paragraphLinkInTable = QuikBreadthFirstSearch(tableStyle, "w:pStyle").First();

            // ищем имена стилей
            TreeNode textTableStyleNameNode = QuikBreadthFirstSearch(textTableStyle, "w:name").First();
            TreeNode paragraphTableStyleNameNode = QuikBreadthFirstSearch(paragraphTableStyle, "w:name").First();

            // меняем заглушку на имя стиля
            textLinkInTable.Attributes["w:val"] = textTableStyleNameNode.Attributes["w:val"];
            paragraphLinkInTable.Attributes["w:val"] = paragraphTableStyleNameNode.Attributes["w:val"];

            List<TreeNode> styles = new List<TreeNode>() {tableStyle, textTableStyle, paragraphTableStyle };

            return styles;
        }

        private List<TreeNode> BuildUniqueNumberingStyles(NumberingStyle styleParams, NumberingStyleBuilder builder)
        {
            TreeNode style = builder.BuildStyle(styleParams);
            TreeNode abstractStyle = builder.BuildAbstrtactStyle(styleParams);

            (style, abstractStyle) = builder.SyncId(style, abstractStyle, _numberingRoot);
            List<TreeNode> styles = new List<TreeNode>() {style, abstractStyle };

            return styles;
        }
    }
}
