using System;
using System.Collections.Generic;
using System.Linq;
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

            if (context.Template.PictureStyle != null)
            {
                Dictionary<int, TreeNode> paragraphsWithPictures = DocumentComposer.ExtractPicturesFromParagraphToDictionary(contentBody);

                if (paragraphsWithPictures != null)
                {
                    DocumentComposer.ReconstructParagraphs(contentBody, DocumentComposer.SeparateDrawingsAndText(paragraphsWithPictures));
                    Dictionary<int, List<TreeNode>> paragraphsWithCaptions = CaptionAdder.AddCaption(contentBody, context.Template.PictureStyle); //Оптимизировать так, чтобы реконструировать абзацы только 1 раз
                    DocumentComposer.ReconstructParagraphs(contentBody, paragraphsWithCaptions);
                }
            }
        }
    }
}
