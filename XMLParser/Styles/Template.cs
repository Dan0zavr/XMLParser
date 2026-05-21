using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.Builders;
using XMLParser.ApplyStrategies;

namespace XMLParser.Styles
{
    public class Template : IStyle
    {
        public string StyleType => "Template";
        public required TextStyle TextStyle { get; set; }
        public required ParagraphStyle ParagraphStyle { get; set; }
        public NumberingStyle? NumberingStyle { get; set; }
        public TableStyle? TableStyle { get; set; }
        public PictureStyle? PictureStyle { get; set; }
        public FormulaStyle? FormulaStyle { get; set; }
        public GlobalStyle GlobalStyle { get; set; }

        public Dictionary<IStyle, IStyleBuilder> GetStyles()
        {
            Dictionary<IStyle, IStyleBuilder> styles = new Dictionary<IStyle, IStyleBuilder>();

            styles.Add(TextStyle, new TextStyleBuilder());
            styles.Add(ParagraphStyle, new ParagraphStyleBuilder());
            //styles.Add(GlobalStyle, new GlobalStyleBuilder());
            if (NumberingStyle != null) styles.Add(NumberingStyle, new  NumberingStyleBuilder());
            if (TableStyle != null) styles.Add(TableStyle, new TableStyleBuilder());
            if (PictureStyle != null) styles.Add(PictureStyle, new PictureStyleBuilder());
            if (FormulaStyle != null) styles.Add(FormulaStyle, new FormulaParagraphBuilder());
            return styles;
        }
    }
}

