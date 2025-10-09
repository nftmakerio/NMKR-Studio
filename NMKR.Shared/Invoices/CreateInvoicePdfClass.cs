using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using PdfSharpCore.Pdf;

namespace NMKR.Shared.Invoices
{
    internal class InvoicePositions
    {
        public int Position { get; set; }
        public string Name { get; set; }
        public XParagraphAlignment Alignment { get; set; }
        public int Length { get; set; }
    }

    public class CreateInvoicePdfClass
    {
        private readonly List<InvoicePositions> invoicepositions = new()
        {
            new() {Name = "Description", Alignment = XParagraphAlignment.Left, Position = 50, Length = 150},
            new() {Name = "Count TX", Alignment = XParagraphAlignment.Right, Position = 130, Length = 100},
            new() {Name = "Token Price ADA", Alignment = XParagraphAlignment.Right, Position = 210, Length = 100},
            new() {Name = "ADA Rate", Alignment = XParagraphAlignment.Right, Position = 300, Length = 100},
            new() {Name = "Token Price EUR", Alignment = XParagraphAlignment.Right, Position = 380, Length = 100},
            new() {Name = "Gross EUR", Alignment = XParagraphAlignment.Right, Position = 460, Length = 100},

        };
        public async Task<string> CreateInvoice(EasynftprojectsContext db, int invoiceid, bool printBackground)
        {

            PdfDocument document = new();
            document.Info.Title = "Invoice";
            document.Info.Author = "utxo AG, Switzerland";
            document.Info.Subject = "Invoice";
            XFont font = new("Arial", 11, XFontStyle.Regular);
            XFont font4 = new("Arial", 8, XFontStyle.Regular);
            int side = 1;



            var invoice = await (from a in db.Invoices
                    .Include(a => a.Invoicedetails)
                    .Include(a=>a.Country)
                                 where a.Id==invoiceid
                select a).FirstOrDefaultAsync();

            if (invoice == null)
                return null;

            int pageNo = 1;
            int yPos = 160;
            // Create new page

            PdfPage page = document.AddPage();
            side = 2;

            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Background Image
            if (printBackground)
                AddBackground(gfx);

            AddPageHeader(gfx);
            AddPageFooter(gfx);

            // Address
            DrawStringLeft(invoice.Firstname + " " + invoice.Lastname, gfx, ref yPos, font);
            DrawStringLeft(invoice.Company, gfx, ref yPos, font);
            DrawStringLeft(invoice.Street , gfx, ref yPos, font);
            DrawStringLeft(invoice.Zip + " "+ invoice.City, gfx, ref yPos, font);
            DrawStringLeft(invoice.Country.Nicename, gfx, ref yPos, font);


            // Description
            AddDescription(gfx);
            yPos = 300;

            // Information Box
            AddInformationBox(gfx, yPos, invoice, pageNo);

            // TableHeader 
            yPos = 400;
            AddTableHeader(gfx, yPos);

            // Add Invoicedetails
            yPos = 430;
            yPos = AddInvoiceDetails(ref gfx, yPos, invoice,document, ref pageNo);

            // Table Footer & Summary && Paymentdetails
            CreateFooter(gfx, document, pageNo, printBackground, yPos, invoice,  ref side);



            string filename = GeneralConfigurationClass.TempFilePath+$"invoice_{GlobalFunctions.PadInt(invoiceid,8)}.pdf";
            document.Save(filename);
            return filename;
        }

        private void AddPageFooter(XGraphics gfx)
        {
            XPen pen = new(XColors.Gray, 1f);
            gfx.DrawLine(pen, 50, 790, 570, 790);

            XFont font1 = new("Arial", 7, XFontStyle.Regular);
            gfx.DrawString("utxo AG", font1, XBrushes.Black, new XPoint(50, 800));
            gfx.DrawString("Dammstrasse 16", font1, XBrushes.Black, new XPoint(50, 810));
            gfx.DrawString("6300 Zug", font1, XBrushes.Black, new XPoint(50, 820));
            gfx.DrawString("Switzerland", font1, XBrushes.Black, new XPoint(50, 830));

            gfx.DrawString("UID: CHE-494.509.135", font1, XBrushes.Black, new XPoint(170, 800));
         //   gfx.DrawString("MWST-No: CHE-494.509.135 MWST", font1, XBrushes.Black, new XPoint(170, 810));
            gfx.DrawString("CHF: CH93 0857 3102 5022 0000 1", font1, XBrushes.Black, new XPoint(170, 820));
            gfx.DrawString("EUR: CH30 0857 3102 5022 0181 4", font1, XBrushes.Black, new XPoint(170, 830));

      /*      gfx.DrawString("Members of the Board", font1, XBrushes.Black, new XPoint(350, 800));
            gfx.DrawString("Patrick Tobler", font1, XBrushes.Black, new XPoint(350, 810));
            gfx.DrawString("Ann-Kristin Mackensen", font1, XBrushes.Black, new XPoint(350, 820));
            gfx.DrawString("Kristian Portz", font1, XBrushes.Black, new XPoint(350, 830));*/

    /*        gfx.DrawString("Contact", font1, XBrushes.Black, new XPoint(500, 800));
            gfx.DrawString("info@utxoag.ch", font1, XBrushes.Black, new XPoint(500, 820));
            gfx.DrawString("www.utxoag.ch", font1, XBrushes.Black, new XPoint(500, 830));*/
         //   gfx.DrawString(" ", font1, XBrushes.Black, new XPoint(450, 830));

        }

        private void AddPageHeader(XGraphics gfx)
        {
            XFont font3 = new("Arial", 20, XFontStyle.Regular);
            XFont font4 = new("Arial", 10, XFontStyle.Regular);
            gfx.DrawString("utxo AG", font3, XBrushes.Black, new XPoint(460, 50));
            gfx.DrawString("Dammstrasse 16", font4, XBrushes.Black, new XPoint(460, 70));
            gfx.DrawString("6300 Zug", font4, XBrushes.Black, new XPoint(460, 85));
            gfx.DrawString("Switzerland", font4, XBrushes.Black, new XPoint(460, 100));
        }

        private int AddInvoiceDetails(ref XGraphics gfx, int yPos, Invoice invoice,PdfDocument document, ref int pageNo)
        {
            XFont font4 = new("Arial", 8, XFontStyle.Regular);
            XPen pen = new(XColors.Black, 1f);
            foreach (var invoicedetail in invoice.Invoicedetails)
            {
                foreach (var invoiceposition in invoicepositions)
                {
                    XTextFormatter tf = new(gfx)
                    {
                        Alignment = invoiceposition.Alignment
                    };

                    string content = "";
                    switch (invoiceposition.Name)
                    {
                        case "Count TX":
                            content = invoicedetail.Count==0?"": invoicedetail.Count.ToString();
                            break;
                        case "Description":
                            content = invoicedetail.Description;
                            break;
                        case "Token Price ADA":
                            content = invoicedetail.Count == 0 ? "" : (invoicedetail.Pricesingleada/1000000).ToString("N2");
                            break;
                        case "Token Price EUR":
                            content = invoicedetail.Count == 0 ? "" : invoicedetail.Pricesingleeur.ToString("C");
                            break;
                        case "ADA Rate":
                            content = invoicedetail.Count == 0 ? "" : invoicedetail.Averageadarate.ToString("C3");
                            break;
                        case "Gross EUR":
                            content = invoicedetail.Count == 0 ? "" : invoicedetail.Pricetotaleur.ToString("C2");
                            break;
                        case "Gross ADA":
                            content = invoicedetail.Count == 0 ? "" : (invoicedetail.Pricetotalada/1000000).ToString("N2");
                            break;
                    }

                    tf.DrawString(content, font4, XBrushes.Black,
                        new(invoiceposition.Position, yPos, invoiceposition.Length, 15));
                }

                yPos += 10;


                if (yPos >= 750)
                {
                    gfx = XGraphics.FromPdfPage(document.AddPage());
                    AddPageHeader(gfx);
                    AddPageFooter(gfx);
                    yPos = 430;
                 /*   if (printBackground)
                        AddBackground(gfx);*/
                    pageNo++;
                    AddInformationBox(gfx, 300, invoice, pageNo);
                    AddTableHeader(gfx, 400);
                }

            }

            return yPos;
        }

        private void AddBackground(XGraphics gfx)
        {
            DrawImage(gfx, 0, @"\backgroundimage.png");
        }

        private void DrawStringLeft(string o, XGraphics gfx, ref int yPos, XFont font)
        {
            if (!string.IsNullOrEmpty(o))
                gfx.DrawString(o, font, XBrushes.Black, new XPoint(50, yPos));
            yPos += 15;
        }

        void DrawImage(XGraphics gfx, int number, string imageFilename)
        {
            XImage image = XImage.FromFile(imageFilename);
            gfx.DrawImage(image, 0, 0);
        }

        private void AddTableHeader(XGraphics gfx, int yPos)
        {
            XFont font4 = new("Arial", 8, XFontStyle.Regular);
            XPen pen = new(XColors.Black, 1f);

            foreach (var invoiceposition in invoicepositions)
            {
                XTextFormatter tf = new(gfx)
                {
                    Alignment = invoiceposition.Alignment
                };
                tf.DrawString(invoiceposition.Name, font4, XBrushes.Black,
                    new(invoiceposition.Position, yPos, invoiceposition.Length, 15));

            }
            gfx.DrawLine(pen, 50, yPos + 20, 570, yPos + 20);
        }

        private void AddInformationBox(XGraphics gfx, int yPos, Invoice invoice, int page)
        {
            XFont font3 = new("Arial", 9, XFontStyle.Regular);
            XPen pen = new(XColors.Black, 1f);
            gfx.DrawRectangle(pen, XBrushes.LightGray, 230, yPos - 40, 330, 50);
            gfx.DrawString("Invoice No.", font3, XBrushes.Black, new XPoint(240, yPos - 20));
            gfx.DrawString("Customer No.", font3, XBrushes.Black, new XPoint(320, yPos - 20));
            gfx.DrawString("Accounting period", font3, XBrushes.Black, new XPoint(380, yPos - 20));
            gfx.DrawString("Date", font3, XBrushes.Black, new XPoint(470, yPos - 20));
            gfx.DrawString("Page", font3, XBrushes.Black, new XPoint(530, yPos - 20));

            gfx.DrawLine(pen, 240, yPos - 10, 550, yPos - 10);

            gfx.DrawString(GlobalFunctions.PadInt(invoice.Id,8), font3, XBrushes.Black, new XPoint(240, yPos + 5));
            gfx.DrawString(invoice.CustomerId.ToString(), font3, XBrushes.Black, new XPoint(320, yPos + 5));
            gfx.DrawString(invoice.Billingperiod.ToString(), font3, XBrushes.Black, new XPoint(380, yPos + 5));
            gfx.DrawString(invoice.Invoicedate.ToShortDateString(), font3, XBrushes.Black, new XPoint(470, yPos + 5));
            gfx.DrawString(page.ToString(), font3, XBrushes.Black, new XPoint(540, yPos + 5));
        }

        private void AddDescription(XGraphics gfx)
        {
            XFont font2 = new("Arial", 16, XFontStyle.Bold);
            XFont font3 = new("Arial", 10, XFontStyle.Bold);

            int ypos = 280;
            DrawStringLeft("Invoice", gfx, ref ypos, font2);
            DrawStringLeft("for the usage of NMKR Studio", gfx, ref ypos, font3);
        }
        private void CreateFooter(XGraphics gfx, PdfDocument document, int pageNo, bool printBackground, int yPos, Invoice invoice, ref int side)
        {
            // New Page
            if (yPos >= 700)
            {
                if (side == 1)
                    side = 2;
                else if (side == 2)
                    side = 1;

                gfx = XGraphics.FromPdfPage(document.AddPage());
                yPos = 450;
                if (printBackground)
                    AddBackground(gfx);
                pageNo++;
                AddInformationBox(gfx, 320, invoice, pageNo);
                AddTableHeader(gfx, 420);
            }


            yPos += 15;
            XPen pen = new(XColors.Black, 1f);
            gfx.DrawLine(pen, 50, yPos, 570, yPos);
            yPos += 15;

            XFont font4 = new("Arial", 8, XFontStyle.Regular);
            XFont font5 = new("Arial", 8, XFontStyle.Bold);
            XTextFormatter tf = new(gfx);
            tf.Alignment = XParagraphAlignment.Right;
            tf.DrawString("Net", font4, XBrushes.Black, new(380, yPos, 50, 15));
            tf.DrawString("VAT " + invoice.Taxrate.ToString("N2") + " %", font4, XBrushes.Black, new(450, yPos, 50, 15));
            tf.DrawString("Total", font5, XBrushes.Black, new(520, yPos, 50, 15));
            yPos += 15;
            gfx.DrawLine(pen, 380, yPos, 570, yPos);
            yPos += 10;


                tf.DrawString(invoice.Neteur.ToString("C"), font4, XBrushes.Black, new(380, yPos, 50, 15));
                tf.DrawString(invoice.Usteur.ToString("C"), font4, XBrushes.Black, new(450, yPos, 50, 15));
                tf.DrawString(invoice.Grosseur.ToString("C"), font5, XBrushes.Black, new(520, yPos, 50, 15));

            string st = "";
           

            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString(st, font4, XBrushes.Black, new(50, yPos - 15, 250, 30));
        }

    }
}
