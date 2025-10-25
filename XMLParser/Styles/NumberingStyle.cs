namespace XMLParser.Styles
{
    public class NumberingStyle : IStyle
    {
        public string StyleType => "NumberingStyle";

        public enum NumberingFormat
        {
            Decimal,
            RomanUpper,
            RomanLower,
            Bullet,
            UpperLetter,
            LowerLetter
        }

        public int Levels { get; set; } = 1;
        public string NumberingType { get; set; }
        public string Marker { get; set; }
    }
}
