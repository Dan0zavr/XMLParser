using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser.DocumentPipeline
{
    public class DocumentPipeline
    {
        private readonly List<IStep> _steps;

        public DocumentPipeline(List<IStep> steps)
        {
            _steps = steps;
        }

        public void Execute(PiplineContext context)
        {
            foreach (var step in _steps)
            {
                step.Execute(context);
            }
        }
    }
}
