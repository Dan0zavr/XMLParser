using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser.Styles
{
    public class PictureStyle : ParagraphStyle, IStyle
    {
        public string StyleType => "PictureStyle";
        
        public bool AutoGenerateLable { get; set; }
        public string? LabelValue { get; set; }
        public string? LabelNumberingType { get; set; }
        public bool EmptyLineBefore { get; set; }
        public bool EmptyLineAfter { get; set; }

    }
}
