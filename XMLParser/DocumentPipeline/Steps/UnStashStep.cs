using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser.DocumentPipeline.Steps
{
    public class UnStashStep : IStep
    {
        public void Execute(PiplineContext context)
        {
            if (context.Stash != null)
            {
                context.Stash.UnStash();
            }
        }
    }
}
