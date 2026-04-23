using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;
using System.Diagnostics;

namespace XMLParser.DocumentPipeline.Steps
{
    public class ReconstructParagraphsStep : IStep
    {
        public void Execute(PiplineContext context)
        {
            TreeNode contentBody = context.DocumentRoot.LongBreadthFirstSearch("w:body").First();

            FillMissedStylesParentNode(contentBody);

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
                CaptionAdder.AddCaption(extendedParagaphs, context.Template.PictureStyle);
            }

            DocumentComposer.ReconstructParagraphs(contentBody, extendedParagaphs);
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
