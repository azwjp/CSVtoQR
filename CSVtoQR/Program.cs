using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper.Configuration;
using CsvHelper;

namespace CSVtoQR
{
    class Program
    {
        private static string outputFile = "qr.pdf";
        private static string sourceFile = "source.csv";
        private static string fontName = "Arial";
        private static int qrSize = 100;

        private const int columnWidth = 130;
        private const int rowHight = 200;
        private const int leftMargin = 40;
        private const int topMargin = 50;
        private const int pageHight = 842;
        private const int pageWidth = 595;

        // the first line is the header
        static void Main(string[] args)
        {
            IEnumerable<Row> rows;

            using (var reader = new StreamReader(sourceFile))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.RegisterClassMap<RowMap>();
                rows = csv.GetRecords<Row>().ToList();
            }

            using (var stream = new MemoryStream())
            {
                // upper right is 595, 842 pt
                using (var document = new Document(PageSize.A4))
                {
                    var writer = PdfWriter.GetInstance(document, stream);
                    document.Open();
                    var pdf = writer.DirectContent;
                    var font = FontFactory.GetFont(fontName, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED).GetCalculatedBaseFont(false);

                    int x = leftMargin, y = pageHight - topMargin;
                    foreach (var row in rows)
                    {
                        if (x >= pageWidth - columnWidth)
                        {
                            x = leftMargin;
                            y -= rowHight;
                        }
                        if (y < rowHight)
                        {
                            y = pageHight - topMargin;
                            document.NewPage();
                        }

                        pdf.SetFontAndSize(font, 32);
                        pdf.ShowText(x, y, row.header);
                        pdf.SetFontAndSize(font, 18);
                        pdf.AddQRCode(x, y - 120, row.qr, qrSize);
                        pdf.ShowText(x, y - 150, row.qr);

                        x += columnWidth;
                    }
                }

                using (BinaryWriter w = new BinaryWriter(File.OpenWrite(outputFile)))
                {
                    w.Write(stream.ToArray());
                }
            }
        }
    }

    public class Row
    {
        public string header;
        public string qr;
    }

    public sealed class RowMap : ClassMap<Row>
    {
        public RowMap()
        {
            Map(r => r.header).Index(0);
            Map(r => r.qr).Index(1);
        }
    }

    static class Extensions
    {
        public static void ShowText(this PdfContentByte pdfContentByte, float x, float y, string text, int alignment = Element.ALIGN_LEFT)
        {
            pdfContentByte.BeginText();
            pdfContentByte.ShowTextAligned(alignment, text, x, y, 0);
            pdfContentByte.EndText();
        }

        public static void AddQRCode(this PdfContentByte pdfContentByte, float x, float y, string text, int size)
        {
            var image = new BarcodeQRCode(text, size, size, null).GetImage();
            image.SetAbsolutePosition(x, y);
            pdfContentByte.AddImage(image);
        }
    }
}
