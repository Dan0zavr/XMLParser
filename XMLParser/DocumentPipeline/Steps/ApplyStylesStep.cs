using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using XMLParser.ApplyStrategies;
using XMLParser.Styles;

namespace XMLParser.DocumentPipeline.Steps
{
    public class ApplyStylesStep : IStep
    {
        public void Execute(PiplineContext context)
        {
            ApplyContext applyContext = new ApplyContext(context.NumberingRoot);
            foreach (var strategy in context.Styles)
            {
                applyContext.SetStrategy(strategy.Key);
                applyContext.ApplyStyle(context.DocumentRoot, strategy.Value);
            }
        }
    }
}
