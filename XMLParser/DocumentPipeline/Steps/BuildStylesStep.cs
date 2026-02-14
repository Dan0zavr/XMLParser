using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Builders;
using XMLParser.Styles;

namespace XMLParser.DocumentPipeline.Steps
{
    public class BuildStylesStep : IStep
    {
        public void Execute(PiplineContext context)
        {
            BuildStyleDirector buildDirector = new BuildStyleDirector(context.StylesRoot, context.NumberingRoot, new StylesUniquelizer(context.StylesRoot));
            (var inStyles, var inNumbering) = buildDirector.BuildAllStyles(context.Template.GetStyles());
            context.Styles = inStyles.Union(inNumbering).ToDictionary(x => x.Key, y => y.Value);

            XMLParser.StyleIntegrator.IntegrateStylesToTree(context.StylesRoot, inStyles.Values.ToList());
            TreeNode paragraphStyle = inStyles[StyleCategory.ParagraphStyle];
            if (context.NumberingRoot.Children.Count > 0)
            {
                XMLParser.StyleIntegrator.IntegrateNumberingStylesToTree(context.DocumentRoot, context.NumberingRoot, inNumbering.Values.ToList(), paragraphStyle);
            }

            XMLWrite.TreeToXMLDocument(context.StylesRoot, context.StyleSpecialTokens, PiplineContext.STYLES, context.TempPath);
        }
    }
}
