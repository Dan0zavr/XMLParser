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
            Stash stash = new Stash(context.DocumentRoot);
            ApplyContext applyContext = new ApplyContext(context.NumberingRoot);
            foreach (var strategy in context.Styles)
            {
                applyContext.SetStrategy(strategy.Key);

                if((strategy.Key == StyleCategory.TextStyle || strategy.Key == StyleCategory.ParagraphStyle) && stash.StashedTables.Count == 0)
                {
                    stash.StashTables();
                }

                if ((strategy.Key == StyleCategory.TableStyle || strategy.Key == StyleCategory.TableTextStyle || strategy.Key == StyleCategory.TableParagraphStyle) && stash.StashedTables.Count > 0)
                {
                    stash.UnStashTables();
                }
                
                applyContext.ApplyStyle(context.DocumentRoot, strategy.Value);
            }

            if(stash.StashedTables.Count > 0)
            {
                stash.UnStashTables();
            }
        }
    }
}
