using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLParser.Styles
{
    public class NumberingElementStyle : IStyle
    {
        public string StyleType => "NumberingElementStyle";

        public enum NumberingFormat
        {
            Decimal,
            RomanUpper,
            RomanLower,
            Bullet,
            UpperLetter,
            LowerLetter
        }

        public const int DEFAULT_LEVELS = 0;

        public int Levels { get; set; } = DEFAULT_LEVELS;
        public string NumberingType { get; set; }
        public string Marker { get; set; }
    }
}
