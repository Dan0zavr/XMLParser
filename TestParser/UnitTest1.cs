using XMLParser;
using XMLParser.Styles;

namespace TestParser
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            string readPath = "C:\\Лабы\\3 курс\\1 семестр\\Сети ответы и вопросы.docx";
            string savePath = "C:\\Users\\Haier\\Desktop\\Тест Форматировщика";
            TextStyle style = new TextStyle()
            {
                FontName = "Times New Roman",
                FontSize = 14
            };

            ParagraphStyle paragraphStyle = new ParagraphStyle()
            {
                Alingnment = "center",
                FirstLineIndent = 1.25,
                LeftIndent = 0,
                RightIndent = 0,
                IntervalInText = 1.5,
                BeforeInterval = 0,
                AfterInterval = 0
            };
            Template template = new Template()
            {
                TextStyle = style,
                ParagraphStyle = paragraphStyle
            };
            ParseManager manager = new ParseManager();
            manager.MainScript(readPath, savePath, template);
        }
    }
}
