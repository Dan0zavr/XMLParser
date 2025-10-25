using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;
using static XMLParser.TreeNode;

namespace XMLParser
{
    // применяет стили к конкретным элементам
    public class Applyer
    {
        private const string document = "document.xml";

        public void ApplyPictureStyle(TreeNode root, TreeNode style)
        {
            List<TreeNode> paragraphsWithDrawings = ExtractPicturesFromParagraphToList(root);

            foreach (var paragraph in paragraphsWithDrawings)
            {
                List<TreeNode> oldStyle = QuikBreadthFirstSearch(paragraph, "w:pPr");

                paragraph.TerminateChildren(oldStyle);

                TreeNode styleToApply = CreateStyleToApply(style);

                foreach (TreeNode styleElement in oldStyle)
                {
                    styleElement.Children.Add(styleToApply);
                }
            }

        }

        private TreeNode CreateStyleToApply(TreeNode style)
        {
            TreeNode styleToApply = new TreeNode()
            {
                TagName = "w:pStyle",
                Attributes = { { "w:val", style.Attributes["w:styleId"] } },
                CloseTag = false
            };

            return styleToApply;
        }


        public void ApplyNumberingStyle(TreeNode root, TreeNode aplliedStyle, int numLevel)
        {
            string styleTagName = "";
            string numberingStyleId = aplliedStyle.Attributes["w:numId"];
            List<TreeNode> children = new List<TreeNode>();

            TreeNode numberingLevel = new TreeNode()
            {
                TagName = "w:ilvl",
                Attributes = { { "w:val", $"{numLevel}" } }
            };

            TreeNode numberingStyle = new TreeNode()
            {
                TagName = "w:numId",
                Attributes = { { "w:val", numberingStyleId } },
            };

            children.Add(numberingLevel);
            children.Add(numberingStyle);

            List<TreeNode> foundedParents = new List<TreeNode>();
            foundedParents = QuikBreadthFirstSearch(root, "w:numPr");

            root.AddChildren(foundedParents, children);

        }

        public void ApplyTableCellStyle(TreeNode root, TreeNode textStyle, TreeNode paragraphStyle)
        {
            List<TreeNode> cells = LongBreadthFirstSearch(root, "w:tc");

            foreach (TreeNode cell in cells)
            {
                ApplyStyle(cell,  paragraphStyle, "paragraph");
                ApplyStyle(cell, textStyle, "character");
            }
        }

        private string GetTagName(string styleType)
        {
            string tagName;

            switch (styleType)
            {
                case "character":
                    tagName = "w:rPr";
                    break;
                case "paragraph":
                    tagName = "w:pPr";
                    break;
                case "table":
                    tagName = "w:tblPr";
                    break;
                default:
                    throw new ArgumentException($"Unknown style type: {styleType}");
            }
            return tagName;
        }

        private string GetStyleTagName(string styleType)
        {
            string styleTagName;
            switch (styleType)
            {
                case "character":
                    styleTagName = "w:rStyle";
                    break;
                case "paragraph":
                    styleTagName = "w:pStyle";
                    break;
                case "table":
                    styleTagName = "w:tblStyle";
                    break;
                default:
                    throw new ArgumentException($"Unknown style type: {styleType}");

            }
            return styleTagName;
        }

        private TreeNode CreateStyleNode(string styleTagName, string styleName)
        {
            return new TreeNode()
            {
                TagName = styleTagName,
                Attributes = { { "w:val", styleName } },
                CloseTag = false
            };
        }

        private void ApplyStylesToNodes(TreeNode root, string tagName, TreeNode styleToApply)
        {
            List<TreeNode> foundedParents = QuikBreadthFirstSearch(root, tagName);

            for (int i = 0; i < foundedParents.Count; i++)
            {
                foundedParents[i].Children.Add(styleToApply);
            }
        }

        public void ApplyStyle(TreeNode root, TreeNode style, string styleType)
        {
            string styleTagName = GetStyleTagName(styleType);
            string tagName = GetTagName(styleType);
            string styleName = style.Attributes["w:styleId"];
            
            TreeNode applyStyle = CreateStyleNode(styleTagName, styleName);

            ApplyStylesToNodes(root, tagName, applyStyle);
            
        }

        public void SaveApply(XMLWrite xmlWrite, TreeNode root, List<string> specialTokens, string tempFolder)
        {
            string serializedTree = xmlWrite.SerializeNode(root, specialTokens);
            xmlWrite.StringToXMLDocument(serializedTree, document, tempFolder);
        }


        private List<TreeNode> ExtractPicturesFromParagraphToList(TreeNode root)
        {
            List<TreeNode> paragraphs = LongBreadthFirstSearch(root, "w:p");
            List<TreeNode> paragraphsWithPic = new List<TreeNode>();

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
                            paragraphsWithPic.Add(paragraphs[i]);
                            break;
                        }
                    }
                }
            }
            return paragraphsWithPic;
        }

    }
}
