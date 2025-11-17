using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser.Styles
{
    public class PictureStyle : IStyle
    {
        public const string NumMarker = "@";
        public string StyleType => "PictureStyle";

        public bool AutoGenerateLable { get; set; } = false;
        public string? LabelValue { get; set; } = string.Empty;
        //public string? LabelNumberingType { get; set; } = string.Empty;
        public bool EmptyLineBefore { get; set; } = true;
        public bool EmptyLineAfter { get; set; } = true;

        public ParagraphStyle ParagraphStyle { get; set; }

    }
}
