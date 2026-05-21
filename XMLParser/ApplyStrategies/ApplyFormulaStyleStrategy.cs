using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;
using System.Text.RegularExpressions;

namespace XMLParser.ApplyStrategies
{
    public class ApplyFormulaStyleStrategy : ApplyStrategy
    {
        public override void Apply(TreeNode root, TreeNode style)
        {
            Dictionary<string, string> attributes = style.Attributes;
            List<TreeNode> formulaParagraphs = FindFormulaParagraphs(root);
            TreeNode styleParagraph = style.Children[0];
            int counter = 0;
            for (int i = 0; i < formulaParagraphs.Count; i++)
            {
                counter++;
                TreeNode styleClone = styleParagraph.Clone();
                TreeNode formulaBuffer = formulaParagraphs[i].QuikBreadthFirstSearch("m:oMathPara").First().Clone();
                AddParaPr(formulaBuffer);
                for (int j = 0; j < styleClone.Children.Count; j++)
                {
                    if (styleClone.Children[j].TagName == "formula")
                    {
                        styleClone.Children[j] = formulaBuffer;
                    }
                }

                TreeNode sectPr = FindFirstSectPr(root, formulaParagraphs, i);

                TreeNode tabs = CreateTabs(style, sectPr, styleClone);

                TreeNode number = styleClone.QuikBreadthFirstSearch("number").FirstOrDefault();
                if (number != null)
                {
                    string numberingFormat = number.Values[0].ToString();
                    string numberValue = numberingFormat.Replace("$", counter.ToString());
                    number.TagName = "w:t";
                    number.Values[0] = numberValue;
                }

                if (Enum.TryParse<AlignmentPreset>(style.Attributes["alignment"], out var alignment))
                {
                    if (alignment == AlignmentPreset.RightLeft || alignment == AlignmentPreset.CenterLeft)
                    {
                        styleClone = SwitchFormulaAndNumbering(styleClone, Convert.ToBoolean(attributes["numbering"]), attributes["numberingFormat"]);
                    }
                }

                //убираются лишние текстовые запуски с табом
                if (tabs.Children.Count == 1)
                {
                    DeleteExcessTab(styleClone, alignment, Convert.ToBoolean(attributes["numbering"]));
                }

                formulaParagraphs[i].Children = styleClone.Children;
            }

            if (style.Attributes["lineAround"] == "true")
            {
                CreateLineAround(root);
            }
        }

        private void DeleteExcessTab(TreeNode style, AlignmentPreset alignment, bool isNumbered)
        {
            List<TreeNode> r = style.LongBreadthFirstSearch("w:r");
            if ((alignment is AlignmentPreset.CenterLeft || alignment is AlignmentPreset.RightLeft) && !isNumbered) { } // таким образом достигается правильное позиционирование формулы
            else
            {
                for (int j = 0; j < r.Count; j++)
                {
                    if (r[j].CheckChild("w:tab"))
                    {
                        style.Children.Remove(r[j]);
                        break;
                    }
                }
            }
        }

        private TreeNode SwitchFormulaAndNumbering(TreeNode style, bool isNumbered, string numerationFormat)
        {
            TreeNode result = new TreeNode();
            int formulaIndex = 0;
            TreeNode formulaBuffer = new TreeNode();
            int numberingIndex = 0;
            TreeNode numberingBuffer = new TreeNode();

            if (isNumbered)
            {
                for (int i = 0; i < style.Children.Count; i++)
                {
                    if (style.Children[i].TagName == "m:oMathPara")
                    {
                        formulaIndex = i;
                        formulaBuffer = style.Children[i].Clone();
                    }
                    if (style.Children[i].TagName == "w:r")
                    {
                        if (style.Children[i].CheckChild("w:t", out TreeNode text))
                        {
                            string pattern = Regex.Escape(numerationFormat).Replace(@"\$", @"\d");
                            if (Regex.IsMatch(text.Values.FirstOrDefault(), pattern))
                            {
                                numberingIndex = i;
                                numberingBuffer = style.Children[i].Clone();
                            }
                        }
                    }
                }

                result = style.Clone();
                result.Children.Remove(result.Children[formulaIndex]);
                result.Children.Remove(result.Children[numberingIndex - 1]);
                result.Children.Insert(formulaIndex, numberingBuffer);
                result.Children.Insert(numberingIndex, formulaBuffer);
                return result;
            }
            else
            {
                return style.Clone();
            }
        }

        private void CreateLineAround(TreeNode root)
        {
            TreeNode body = root.QuikBreadthFirstSearch("w:body").First();
            List<TreeNode> oldChildren = body.Children;
            List<TreeNode> newChildren = new List<TreeNode>();

            foreach (var child in oldChildren)
            {
                if (child.CheckChild("m:oMathPara"))
                {
                    newChildren.Add(CreateParagraphNode());
                    newChildren.Add(child);
                    newChildren.Add(CreateParagraphNode());
                }
                else
                {
                    newChildren.Add(child);
                }
            }
            body.Children = newChildren;
        }

        private void AddParaPr(TreeNode formula)
        {
            TreeNode jc = new TreeNode
            {
                TagName = "m:jc",
                Attributes = { { "m:val", "left" } }
            };

            TreeNode paraPr = new TreeNode
            {
                TagName = "m:oMathParaPr",
                Children = { jc.Clone() },
                CloseTag = true
            };
            formula.Children.Insert(0, paraPr);
        }

        private List<TreeNode> FindFormulaParagraphs(TreeNode root)
        {
            List<TreeNode> allParagraphs = root.LongBreadthFirstSearch("w:p");
            List<TreeNode> formulaParagraphs = new List<TreeNode>();

            foreach (TreeNode paragraph in allParagraphs)
            {
                if (paragraph.CheckChild("m:oMathPara"))
                {
                    formulaParagraphs.Add(paragraph);
                }
            }

            return formulaParagraphs;
        }

        private TreeNode FindFirstSectPr(TreeNode root, List<TreeNode> paragraphs, int currentIndex)
        {
            for (int i = currentIndex; i < paragraphs.Count; i++)
            {
                if (paragraphs[i].CheckChild("w:sectPr"))
                {
                    return paragraphs[i].QuikBreadthFirstSearch("w:sectPr").First();
                }
            }
            List<TreeNode> sectPrs = root.LongBreadthFirstSearch("w:sectPr");
            return sectPrs[sectPrs.Count - 1];
        }

        private TreeNode CreateTabs(TreeNode style, TreeNode sectPr, TreeNode paragraph)
        {
            TreeNode tabs = paragraph.QuikBreadthFirstSearch("w:tabs").First();
            string pageWidth = sectPr.QuikBreadthFirstSearch("w:pgSz").First().Attributes["w:w"];
            string leftMargin = sectPr.QuikBreadthFirstSearch("w:pgMar").First().Attributes["w:left"];
            string rightMargin = sectPr.QuikBreadthFirstSearch("w:pgMar").First().Attributes["w:right"];

            (string centerPos, string rightPos) = CalculateTabPositions(style, pageWidth, leftMargin, rightMargin);
            if (centerPos == "0")
            {
                tabs.Children.Remove(tabs.Children[0]);
                tabs.Children[0].Attributes["w:pos"] = rightPos;
            }
            else
            {
                tabs.Children[0].Attributes["w:pos"] = centerPos;
                tabs.Children[1].Attributes["w:pos"] = rightPos;
            }
            return tabs;
        }

        private (string center, string right) CalculateTabPositions(TreeNode style, string sPageWidth, string sLeftMargin, string sRightMargin)
        {
            int pageWidth = Convert.ToInt32(sPageWidth);
            int leftMargin = Convert.ToInt32(sLeftMargin);
            int rightMargin = Convert.ToInt32(sRightMargin);

            int textWidth = pageWidth - leftMargin - rightMargin;
            int center = 0;
            int right = 0;

            if (Enum.TryParse<AlignmentPreset>(style.Attributes["alignment"], out var alignment))
            {

                switch (alignment)
                {
                    case AlignmentPreset.CenterRight:
                        center = textWidth / 2;
                        right = textWidth;
                        break;

                    case AlignmentPreset.CenterLeft:
                        center = 0;
                        right = textWidth / 2;
                        break;

                    case AlignmentPreset.LeftRight:
                        center = 0;
                        right = textWidth;
                        break;

                    case AlignmentPreset.RightLeft:
                        center = 0;
                        right = textWidth;
                        break;

                    default:
                        center = textWidth / 2;
                        right = textWidth;
                        break;
                }
            }

            return (center.ToString(), right.ToString());
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
    }
}
