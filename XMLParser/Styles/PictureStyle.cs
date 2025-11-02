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

        public string PictureAllignment { get; set; }
        public string LableAllignment { get; set; }
        public bool AutoGenerateLable { get; set; }
        public string? LabelValue { get; set; } 
        public ParagraphStyle PictureParagraphStyle { get; set; }
    }
}
