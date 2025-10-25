namespace XMLParser.Styles
{
    public class TextStyle : IStyle
    {
        public string StyleType => "TextStyle";
        public string FontName { get; set; }
        public int FontSize { get; set; }
    }
}
