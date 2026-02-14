using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser.DocumentPipeline.Steps
{
    public class StashStep : IStep
    {
        public void Execute(PiplineContext context)
        {
            context.Stash = new Stash(context.DocumentRoot.LongBreadthFirstSearch("w:body").First());
            context.Stash.StashPages(context.PagesWords, context.IgnorePages);
        }
    }
}
