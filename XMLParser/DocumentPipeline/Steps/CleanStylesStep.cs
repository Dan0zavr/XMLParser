using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;

namespace XMLParser.DocumentPipeline.Steps
{
    public class CleanStylesStep : IStep
    {
        public void Execute(PiplineContext context)
        {
            Cleaner.CleanHandStyles(context.DocumentRoot, context.Template, context.DocumentSpecialTokens);
        }
    }
}
