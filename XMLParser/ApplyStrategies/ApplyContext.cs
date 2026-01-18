using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Styles;

namespace XMLParser.ApplyStrategies
{
    public class ApplyContext
    {
        private ApplyStrategy _currentStrategy { get; set; }
        private TreeNode _numberingRoot { get; set; }

        public ApplyContext(TreeNode numbringRoot)
        {
            _numberingRoot = numbringRoot;
        }

        public void SetStrategy(StyleCategory category)
        {
            _currentStrategy = AppointStrategy(category);
        }

        public void ApplyStyle(TreeNode root, TreeNode style)
        {
            _currentStrategy.Apply(root, style);
        }

        private ApplyStrategy AppointStrategy(StyleCategory category)
        {
            switch (category)
            {
                case StyleCategory.TextStyle:
                    return new ApplyTextStyleStrategy();
                case StyleCategory.ParagraphStyle:
                    return new ApplyParagraphStyleStrategy(_numberingRoot);
                case StyleCategory.TableStyle:
                    return new ApplyTableStyleStrategy();
                case StyleCategory.TableTextStyle:
                    return new ApplyTableTextStyleStrategy();
                case StyleCategory.TableParagraphStyle:
                    return new ApplyTableParagraphStrategy();
                case StyleCategory.NumberingStyleMarked:
                    return new ApplyNumberingStyleStrategy(_numberingRoot);
                case StyleCategory.NumberingStyleNumbered:
                    return new ApplyNumberingStyleStrategy(_numberingRoot);
                case StyleCategory.PictureStyle:
                    return new ApplyPictureStyleStrategy();
                case StyleCategory.Useless:
                    return new UselessStrategy();
                default:
                    throw new NotImplementedException($"Необработанный тип при сопоставлении стратегий {category}");
            }
        }
    }
}
