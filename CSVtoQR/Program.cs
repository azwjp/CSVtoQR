using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVtoQR
{
    class Program
    {
        private static string outputFile = "qr.pdf";
        private static string fontName = "Arial";

        static void Main(string[] args)
        {
            using (var stream = new MemoryStream())
            {
                // upper right is 595, 842 pt
                using (var document = new Document(PageSize.A4))
                {
                    var writer = PdfWriter.GetInstance(document, stream);
                    document.Open();
                    var pdfContentByte = writer.DirectContent;
                    var font = FontFactory.GetFont(fontName, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED).GetCalculatedBaseFont(false);
                    
                    pdfContentByte.SetFontAndSize(font, 32);
                    ShowText(pdfContentByte, 20, 800, "01");
                    pdfContentByte.SetFontAndSize(font, 18);
                    ShowText(pdfContentByte, 20, 750, "QR Code");
                    ShowText(pdfContentByte, 20, 650, "URL");

                    document.Close();
                }

                using (BinaryWriter w = new BinaryWriter(File.OpenWrite(outputFile)))
                {
                    w.Write(stream.ToArray());
                }
            }
        }
        private static void ShowText(PdfContentByte pdfContentByte, float x, float y, string text, int alignment = Element.ALIGN_LEFT, float rotaion = 0)
        {
            pdfContentByte.BeginText();
            pdfContentByte.ShowTextAligned(alignment, text, x, y, rotaion);
            pdfContentByte.EndText();
        }
    }
}
