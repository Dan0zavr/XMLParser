using XMLParser;
using XMLParser.Styles;

string readPath = @"C:\Лабы\4 курс\Практика\Преддипломная практика\Преддипломная практика отчет.docx";

string savePath = "C:\\Users\\Temp\\OneDrive\\Рабочий стол\\Тест форматировщика";
ParseManager parser = new ParseManager();
int[] pages = null;

TextStyle textStyle = new TextStyle
{
    FontName = "Times New Roman",
    FontSize = 14,
};

TextStyle tableTextStyle = new TextStyle
{
    FontName = "Times New Roman",
    FontSize = 10,
};

ParagraphStyle paragraphStyle = new ParagraphStyle
{
    Alingnment = "both",
    FirstLineIndent = 1.25,
    IntervalInText = 1.5
};

ParagraphStyle tableParagraphStyle = new ParagraphStyle
{
    Alingnment = "both",
    FirstLineIndent = 0,
    IntervalInText = 1
};

ParagraphStyle pictureParagraphStyle = new ParagraphStyle
{
    Alingnment = "center",
    IntervalInText = 1.5
};

PictureStyle pictureStyle = new PictureStyle
{
    ParagraphStyle = pictureParagraphStyle,
    EmptyLineAround = true,
    AutoGenerateLable = true,
    LabelValue = "Рисунок $ - "
};

TableStyle tableStyle = new TableStyle
{
    VerticalAlignment = "center",
    TextStyle = tableTextStyle,
    ParagraphStyle = tableParagraphStyle,
    LabelValue = "Таблица - $"
};

GlobalStyle globalStyle = new GlobalStyle
{
    LeftMargin = 10,
    SpecialColontitul = "РАБОТАЕТ",
    LastNoNumberingPage = 3,
    Alignment = "center",
};

Template template = new Template 
{
    TextStyle = textStyle,
    ParagraphStyle = paragraphStyle,
    PictureStyle = pictureStyle,
    TableStyle = tableStyle,
    GlobalStyle = globalStyle
};

Console.WriteLine(typeof(XMLParser.ParseManager).Assembly.Location);
Console.WriteLine(typeof(XMLParser.ParseManager).Assembly.FullName);

parser.MainScript(readPath, savePath, template, pages, null);