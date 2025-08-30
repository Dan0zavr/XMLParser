using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser.Styles
{
    public class Template : IStyle
    {
        public string StyleType => "Template";
        public TextStyle TextStyle { get; set; }
        public ParagraphStyle ParagraphStyle { get; set; }
        public NumberingStyle? NumberingStyle { get; set; }
        public TableStyle? TableStyle { get; set; }
        public ParagraphStyle? PictureStyle { get; set; }
    }
}

