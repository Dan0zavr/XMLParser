using XMLParser.ApplyStrategies;
using XMLParser.Builders;
using XMLParser.Styles;
using static XMLParser.TreeNode;
using static XMLParser.XMLRead;
using static XMLParser.XMLWrite;
using static XMLParser.Cleaner;
using static XMLParser.Tokenizator;
using static XMLParser.DocumentComposer;
using PDFReader;
using XMLParser.DocumentPipeline.Steps;
using XMLParser.DocumentPipeline;

namespace XMLParser
{
    public class ParseManager
    {
        public string MainScript(string readPath, string savePath, Template template, bool splitDocument = false, int[] pages = null)
        {
            PiplineContext context = new PiplineContext
            {
                InputPath = readPath,
                OutputPath = savePath,
                Template = template,
                IgnorePages = pages
            };

            try
            {
                DocumentPipeline.DocumentPipeline pipeline = new DocumentPipeline.DocumentPipeline(new List<IStep>
                {
                    new UnzipStep(),
                    new ParseXMLStep(),
                    new ReadPDFStep(),
                    new StashStep(),
                    new BuildStylesStep(),
                    new ApplyStyleChangesStep(),
                    new ReconstructParagraphsStep(),
                    new CleanStylesStep(),
                    new ApplyStylesStep(),
                    new UnStashStep(),
                    new ApplyDocumentChangesStep(),
                    new ApplyDocumentChangesStep(),
                    new ZipStep()
                });

                pipeline.Execute(context);
                return context.OutputPath;
            }
            finally
            {
                if (context.TempPath != null)
                {
                    Directory.Delete(context.TempPath, true);
                }
            }

        }

    }
}
