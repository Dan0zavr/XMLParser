using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;

namespace XMLParser.DocumentPipeline.Steps
{
    public class ReconstructParagraphsStep : IStep
    {
        public void Execute(PiplineContext context)
        {
            TreeNode contentBody = context.DocumentRoot.LongBreadthFirstSearch("w:body").First();

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
    }
}
