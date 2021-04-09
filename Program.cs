using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Filter;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace ConsoleApplication1
{
    internal class Program
    {
        public const string LIPSUM = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
        public static void Main(string[] args)
        {
            Program p = new Program();
            p.CreateFileFromBytes(p.CreateSmallDoc(),"fontTest.pdf");
            //Console.Out.WriteLine(p.ExtractTextFromDoc());
            
            p.manipulatePdf(p.CreateSmallDoc());
            //p.ExtractTextFromDoc();
        }

        public string ExtractTextFromDoc()
        {
            PdfDocument pdf = new PdfDocument(new PdfReader(new MemoryStream(CreateSmallDoc())));
            Rectangle rect = new Rectangle(100, 100, 200, 200);
            TextRegionEventFilter regionFilter = new TextRegionEventFilter(rect);
            StringBuilder builder = new StringBuilder();

            for (int page = 1; page <= pdf.GetNumberOfPages(); page++)
            {
                ITextExtractionStrategy strat = new FilteredTextEventListener(new LocationTextExtractionStrategy(), regionFilter);
                string str = PdfTextExtractor.GetTextFromPage(pdf.GetPage(page), strat) + "\n\n";
                builder.Append(str);
            }
            return builder.ToString();
        }
        
        public void CreateFileFromBytes(byte[] bytes,string filename)
        {
            FileStream stream = new FileStream("../../"+filename, FileMode.Create);
            stream.Write(bytes,0,bytes.Length);
            stream.Close();
        }

        public void manipulatePdf(byte[] bytes)
        {
            PdfDocument pdf = new PdfDocument(new PdfReader(new MemoryStream(bytes)));
            Rectangle rect = new Rectangle(100, 100, 200, 200);
            CustomFontFilter fontFilter = new CustomFontFilter(rect);
            FilteredEventListener listener = new FilteredEventListener();

            LocationTextExtractionStrategy strat = listener.AttachEventListener(new LocationTextExtractionStrategy(),fontFilter);

            PdfCanvasProcessor parser = new PdfCanvasProcessor(listener);
            parser.ProcessPageContent(pdf.GetFirstPage());
            
            pdf.Close();
            String actualText = strat.GetResultantText();
            Console.Out.WriteLine(actualText);
        }

        protected class CustomFontFilter : TextRegionEventFilter
        {
            public CustomFontFilter(Rectangle filterRect):base(filterRect)
            {
            }

            public override bool Accept(IEventData data, EventType type)
            {
                if (type.Equals(EventType.RENDER_TEXT))
                {
                    TextRenderInfo renderInfo = (TextRenderInfo) data;
                    PdfFont font = renderInfo.GetFont();
                    if (null != font)
                    {
                        string fontname = font.GetFontProgram().GetFontNames().GetFontName();
                        return fontname.EndsWith("Bold") || fontname.EndsWith("Oblique");
                    }
                }

                return false;
            }
        }
        
        public byte[] CreateSmallDoc()
        {
            MemoryStream memStream = new MemoryStream();
            PdfDocument pdf = new PdfDocument(new PdfWriter(memStream));
            Document doc = new Document(pdf);
            for (int i = 0; i < 5; i++)
            {
                PdfCanvas pdfC = new PdfCanvas(pdf.AddNewPage());
                Canvas c = new Canvas(pdfC, pdfC.GetDocument().GetDefaultPageSize());
                Paragraph p = new Paragraph(LIPSUM).SetWidth(200);

                if (i == 0)
                    p.SetFont(PdfFontFactory.CreateFont("../../font_Bold.ttf", true));
                
                c.ShowTextAligned(p, 100, 100, TextAlignment.LEFT);
                c.Close();
            }

            doc.Add(new Div().SetFixedPosition(100, 100, 200).SetHeight(200).SetBackgroundColor(ColorConstants.BLACK));
            doc.Close();
            pdf.Close();
            return memStream.ToArray();
        }
    }
}