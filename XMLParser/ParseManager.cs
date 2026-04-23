using PDFReader;
using System.Diagnostics;
using XMLParser.ApplyStrategies;
using XMLParser.Builders;
using XMLParser.DocumentPipeline;
using XMLParser.DocumentPipeline.Steps;
using XMLParser.Styles;
using static XMLParser.Cleaner;
using static XMLParser.DocumentComposer;
using static XMLParser.Tokenizator;
using static XMLParser.TreeNode;
using static XMLParser.XMLRead;
using static XMLParser.XMLWrite;

namespace XMLParser
{
    public class ParseManager
    {
        public string MainScript(string readPath, string savePath, Template template, int[] pages = null, string tempPdfPath = null)
        {
            PiplineContext context = new PiplineContext
            {
                InputPath = readPath,
                OutputPath = savePath,
                Template = template,
                IgnorePages = pages,
                TempPdfPath = tempPdfPath
            };

            try
            {
                DocumentPipeline.DocumentPipeline pipeline = new DocumentPipeline.DocumentPipeline(new List<IStep>
                {
                    new UnzipStep(),
                    new ParseXMLStep(),
                    new ConvertDocumentStep(),
                    new ReadPDFStep(),
                    new StashStep(),
                    new BuildStylesStep(),
                    new ApplyStyleChangesStep(),
                    new ReconstructParagraphsStep(),
                    new CleanStylesStep(),
                    new ApplyStylesStep(),
                    new UnStashStep(),
                    new ApplyDocumentChangesStep(),
                    new ZipStep()
                });

                pipeline.Execute(context);
                return context.OutputFile;
            }
            finally
            {
                if (Directory.Exists(context.TempDocumentDirectory))
                {
                    Directory.Delete(context.TempDocumentDirectory, true);
                }

                if (context.TempPdfPath != null)
                {
                    string parent = Directory.GetParent(context.TempPdfPath).FullName;
                    if (Directory.Exists(parent))
                    {
                        Directory.Delete(parent, true);
                    }
                }
                if (Directory.Exists(context.TempPdfDirectory))
                {
                    Directory.Delete(context.TempPdfDirectory, true);
                }
            }

        }

    }
}
