namespace XMLParser.Styles
{
    public class NumberingStyle : IStyle
    {
        public NumberingElementStyle? MarkedNumbering {  get; set; }
        public NumberingElementStyle? NumberedNumbering { get; set; }

        public string StyleType => "NumberingStyle";

        public List<NumberingElementStyle> GetElements()
        {
            List<NumberingElementStyle> elements = new List<NumberingElementStyle>();
            if (MarkedNumbering != null) elements.Add(MarkedNumbering);
            if (NumberedNumbering != null) elements.Add(NumberedNumbering);
            return elements;
        }
    }
}
