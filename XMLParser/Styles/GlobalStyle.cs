using System;
using System.Collections.Generic;
using System.Text;

namespace XMLParser.Styles
{
    public class GlobalStyle : IStyle
    {
        public string StyleType => "GlobalStyle";
        // Поля указаны в сантиметрах
        public double LeftMargin { get; set; } = 3.0;
        public double RightMargin { get; set; } = 1.5;
        public double TopMargin { get; set; } = 2.0;
        public double BottomMargin { get; set; } = 2.0;

        public string? SpecialColontitul = null;
        public int? LastNoNumberingPage = null;
        public TextStyle? NumberingTextStyle { get; set; }
        public string Alignment { get; set; }
    }
}
