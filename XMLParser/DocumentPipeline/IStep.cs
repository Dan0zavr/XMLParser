using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser.DocumentPipeline
{
    public interface IStep
    {
        void Execute(PiplineContext context);
    }
}
