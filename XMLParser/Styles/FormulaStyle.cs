using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser.Styles
{
    public enum AlignmentPreset
    {
        CenterRight,
        CenterLeft,
        LeftRight,
        RightLeft
    }

    public class FormulaStyle : IStyle
    {
        public string StyleType => "FormulaStyle";

        public AlignmentPreset AlignmentPreset { get; set; } = AlignmentPreset.CenterRight;
        public bool Legend { get; set; } = false;
        public bool Numeration { get; set; } = false;
        public string NumerationFormat { get; set; } = "($)";
        public bool EmptyLineAround { get; set; } = true;
    }
}
