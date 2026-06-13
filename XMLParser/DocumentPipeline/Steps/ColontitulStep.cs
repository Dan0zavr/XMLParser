using PDFReader;
using System;
using System.Collections.Generic;
using System.Text;
using XMLParser.SpecialClasses.DocumentChangers;
using XMLParser.SpecialClasses.Tree;
using XMLParser.Styles;

namespace XMLParser.DocumentPipeline.Steps
{
    public class ColontitulStep : IStep
    {
        public void Execute(PiplineContext context)
        {
            if (context.Template.GlobalStyle == null) return;

            int[] targetPages = new int[(int)context.Template.GlobalStyle.LastNoNumberingPage];
            int counter = 1;
            for (int i = 0; i < targetPages.Length; i++)
            {
                targetPages[i] = counter;
                counter++;
            }

            context.ForColontitulPagesWords = PDFReaderEntry.ReadPDF(context.TempPdfPath, targetPages);

            ColontitulService colontitulService = new ColontitulService(context);

            colontitulService.ClearSectPr();

            if (context.Template.GlobalStyle.LastNoNumberingPage != null)
            {
                colontitulService.AddNoNumberingSectPr(targetPages);
            }


            colontitulService.ApplyFooter();
            colontitulService.ApplyFields();

        }
    }
}
