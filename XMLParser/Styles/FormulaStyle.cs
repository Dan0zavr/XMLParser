using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser.Styles
{
    public class FormulaStyle : IStyle
    {
        public string StyleType => "FormulaStyle";

        public string FormulaAlingnment { get; set; } = "center";
        public bool Legend { get; set; } = false;
        public bool Numeration { get; set; } = false;
        public string NumerationFormat { get; set; } = "($)";
        public string NumerationAlingnment { get; set; } = "right";
    }
}
