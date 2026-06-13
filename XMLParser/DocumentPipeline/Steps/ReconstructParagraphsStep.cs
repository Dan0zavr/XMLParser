using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;
using System.Diagnostics;
using UglyToad.PdfPig.Fonts.Encodings;
using XMLParser.SpecialClasses.Tree;
using XMLParser.SpecialClasses.DocumentChangers;

namespace XMLParser.DocumentPipeline.Steps
{
    public class ReconstructParagraphsStep : IStep
    {
        public void Execute(PiplineContext context)
        {
            TreeNode contentBody = context.DocumentRoot.LongBreadthFirstSearch("w:body").First();

            FillMissedStylesParentNode(contentBody);

            if (context.Template.TableStyle != null)
            {
                if (context.Template.TableStyle.LabelValue != null)
                {
                    CaptionAdder.AddTableCaption(contentBody, context.Template.TableStyle);
                }
            }

            if (context.Template.PictureStyle == null) return;

            Dictionary<int, TreeNode> paragraphsWithPictures = DocumentComposer.ExtractPicturesFromParagraphToDictionary(contentBody);

            if (paragraphsWithPictures == null) return;

            Dictionary<int, List<TreeNode>> extendedParagaphs = DocumentComposer.SeparateDrawingsAndText(paragraphsWithPictures);

            if (context.Template.PictureStyle.EmptyLineAround)
            {
                AddEmptyParagraphs(extendedParagaphs);
            }

            if (context.Template.PictureStyle.AutoGenerateLable)
            {
                CaptionAdder.AddPictureCaption(extendedParagaphs, context.Template.PictureStyle);
            }

            DocumentComposer.ReconstructParagraphs(contentBody, extendedParagaphs);

            if (context.Template.PictureStyle.AutoGenerateLable)
            {
                AddKeepCaption(contentBody);
            }
        }

        private void AddKeepCaption(TreeNode body) //Чтобы рисунок и подпись всегда были на одной странице
        {
            List<TreeNode> paragraphs = body.LongBreadthFirstSearch("w:p");
            List<TreeNode> paragraphsWithDrawings = new List<TreeNode>();
            foreach (var paragraph in paragraphs)
            {
                List<TreeNode> drawings = paragraph.LongBreadthFirstSearch("w:drawing"); 

                if(drawings.Count > 0)
                {
                    paragraphsWithDrawings.Add(paragraph);
                }
            }

            foreach(var paragraph in paragraphsWithDrawings)
            {
                TreeNode pPr = paragraph.LongBreadthFirstSearch("w:pPr").FirstOrDefault();

                if(pPr != null)
                {
                    pPr.Children.Insert(0, new TreeNode { TagName = "w:keepNext"});
                }
            }
        }

        private void AddEmptyParagraphs(Dictionary<int, List<TreeNode>> extendedParagaphs)
        {
            foreach (var sameNumbersParagraphs in extendedParagaphs.Values)
            {
                for (int i = sameNumbersParagraphs.Count - 1; i >= 0; i--)
                {
                    if (sameNumbersParagraphs[i].QuikBreadthFirstSearch("w:drawing").Count > 0)
                    {
                        int j = i;
                        sameNumbersParagraphs.Insert(++j, DocumentComposer.CreateParagraphNode());
                        sameNumbersParagraphs.Insert(--j, DocumentComposer.CreateParagraphNode());
                    }
                }
            }
        }

        private void FillMissedStylesParentNode(TreeNode root)
        {
            List<TreeNode> paragraphNodes = root.LongBreadthFirstSearch("w:p");
            foreach (var paragraphNode in paragraphNodes)
            {
                if (!paragraphNode.CloseTag)
                {
                    paragraphNode.CloseTag = true;
                }

                if (paragraphNode.QuikBreadthFirstSearch("w:pPr").Count == 0)
                {
                    TreeNode pPr = DocumentComposer.CreateParagraphStyleNode();
                    TreeNode rPr = DocumentComposer.CreateTextStyleNode();
                    pPr.Children.Add(rPr);
                    paragraphNode.Children.Insert(0, pPr);
                }

                if (paragraphNode.QuikBreadthFirstSearch("w:rPr").Count == 0)
                {
                    TreeNode rPr = DocumentComposer.CreateTextStyleNode();
                    TreeNode pPr = paragraphNode.QuikBreadthFirstSearch("w:pPr").FirstOrDefault();
                    if (pPr != null)
                    {
                        pPr.Children.Add(rPr);
                    }
                }
            }
        }
    }
}
