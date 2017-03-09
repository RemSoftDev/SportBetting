using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Media.Imaging;
using Nbt.Services.Spf.Printer;
using System.Drawing.Printing;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace Nbt.Services.Spf
{
    public class DefaultTranslator
    {
        private int codepage;
        public int Codepage
        {
            get
            {
                return codepage;
            }
            set
            {
                if (value > 0)
                    codepage = value;
            }
        }
        private SPFPage page = null;
        private IPrinter printer = null;

        private StringBuilder pageData;

        public DefaultTranslator(IPrinter iPrinter, SPFPage spfPage)
        {
            this.printer = iPrinter;
            this.page = spfPage;
            this.Codepage = 852; //default
            pageData = new StringBuilder();
        }

        public FixedDocument ParseFixDocument(double height)
        {
            if (page == null || page.Items == null || page.Items.Length <= 0)
                return null;
            //return String.Empty;

            if (printer == null)
                return null;

            if (page.CodepageSpecified)
                this.Codepage = (int)page.Codepage;

            FixedDocument document = new FixedDocument();

            int margin = printer.Margin;
            List<TextBlock> tbList = new List<TextBlock>();
            //double width = 180;//178;//270;
            double width = printer.PaperWidth;

            TextBlock tb = new TextBlock() { Width = width, Margin = new Thickness(margin, margin, 0, margin), TextWrapping = System.Windows.TextWrapping.Wrap };
            tbList.Add(tb);
            bool firstLine = true;
            double lineHeight = 0;
            int idx = 0;
            foreach (Object elem in page.Items)
            {
                if (elem.GetType() == typeof(Line))
                {
                    tb.Measure(new Size(width, double.PositiveInfinity));
                    if (firstLine)
                    {
                        lineHeight = tb.DesiredSize.Height;
                        firstLine = false;
                    }
                    if ((tb.DesiredSize.Height + lineHeight) >= height)
                    {
                        tb = new TextBlock() { Width = width, Margin = new Thickness(margin, margin, 0, margin), TextWrapping = System.Windows.TextWrapping.Wrap };
                        tbList.Add(tb);
                        idx++;
                    }
                    Line line = elem as Line;
                    tb.Inlines.Add(ParseLine(line, width, margin));
                }
            }
            foreach (TextBlock textBlock in tbList)
            {
                StackPanel ticket = new StackPanel() { Width = width };
                ticket.Children.Add(textBlock);
                FixedPage fixedPage = new FixedPage();
                fixedPage.Children.Add(ticket);
                PageContent pageContent = new PageContent();
                ((IAddChild)pageContent).AddChild(fixedPage);
                document.Pages.Add(pageContent);

            }

            return document;
        }

        private TextBlock ParseLine(Line line, double width, double margin)
        {
            double newWidth = width - (margin);
            TextBlock tb = new TextBlock() { Width = newWidth };
            tb.Margin = new Thickness(0, 0, 0, 0);
            if (line.Items == null || line.Items.Length <= 0)
                return null;

            int numItems = 0;
            foreach (Object item in line.Items)
            {
                numItems++;
            }
            foreach (Object item in line.Items)
            {
                string text = "";
                numItems--;
                TextBlock tbItem = new TextBlock();
                if (item.GetType() == typeof(ImageItem))
                {
                    try
                    {
                        string fileName = ConfigurationManager.AppSettings["logoFile"]; //"Images/bs-logo.jpg"
                        //if (File.Exists("PrintLogo.jpg")) {
                        if (File.Exists(fileName))
                        {
                            BitmapImage btm;
                            using (MemoryStream ms = new MemoryStream(System.IO.File.ReadAllBytes(fileName)))
                            {
                                btm = new BitmapImage();
                                btm.BeginInit();
                                btm.StreamSource = ms;
                                // Below code for caching is crucial.
                                btm.CacheOption = BitmapCacheOption.OnLoad;
                                btm.EndInit();
                                btm.Freeze();
                            }

                            Image i = new Image();
                            i.Margin = new Thickness(0, 0, 4, 0);
                            //BitmapImage imageSource = new BitmapImage(new Uri(Path.GetFullPath(fileName)));
                            i.Source = btm;
                            i.Stretch = Stretch.Fill;
                            tbItem.Inlines.Add(i);
                        }
                    }
                    catch (Exception ex)
                    {
                        Image i = new Image();
                        i.Stretch = Stretch.Fill;
                        tbItem.Inlines.Add(i);
                    }
                }
                else if (item.GetType() == typeof(BarCodeItem))
                {
                    BarCodeItem bci = item as BarCodeItem;
                    BarCodeCreator bcc = new BarCodeCreator();
                    Canvas c = bcc.BarCode128(bci.Value);

                    tbItem.HorizontalAlignment = HorizontalAlignment.Center;
                    tbItem.TextAlignment = TextAlignment.Center;
                    tbItem.Inlines.Add(c);
                }
                else if (item.GetType() == typeof(TextItem))
                {
                    TextItem txt = item as TextItem;

                    ParseTextFontStyle(txt, ref tbItem);
                    tbItem.Margin = new Thickness(0, 0, 0, 0);
                    Run run = new Run() { Text = txt.Value };
                    if (line.UnderlineSpecified && line.Underline == true)
                    {
                        tbItem.Inlines.Add(new Underline(run));
                    }
                    else tbItem.Inlines.Add(run);
                    text = txt.Value;
                }
                else if (item.GetType() == typeof(FillItem))
                {
                    string filltext = "";
                    for (int i = 0; i < 100; i++)
                        filltext += ((FillItem)item).Value;
                    tbItem.Inlines.Add(new Run()
                    {
                        FontSize = 10,
                        FontFamily = new FontFamily("Arial"),
                        Text = filltext
                    });
                }
                System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular;
                if (tbItem.FontWeight == FontWeights.Bold) style = System.Drawing.FontStyle.Bold;
                IEnumerator<string> enumerator = tbItem.FontFamily.FamilyNames.Values.GetEnumerator();
                enumerator.Reset();
                enumerator.MoveNext();
                int fontsize = (int)tbItem.FontSize - 3;
                string val = enumerator.Current;
                System.Drawing.Size textSize = System.Windows.Forms.TextRenderer.MeasureText(text, new System.Drawing.Font(
                    new System.Drawing.FontFamily(val), fontsize, style));
                if (numItems > 0)
                {
                    newWidth -= textSize.Width;
                    tbItem.Width = textSize.Width;
                }
                else
                {
                    if (newWidth < textSize.Width)
                    {
                        newWidth = width - margin;
                        tb.Inlines.Add(new Run() { Text = "\n" });
                    }
                    tbItem.Width = newWidth;
                }

                tb.Inlines.Add(tbItem);
            }
            return tb;
        }

        /// <summary>
        /// parses "FontStyle" for BlockText and TextItem
        /// </summary>
        /// <param name="txt">BlockText or TextItem</param>
        /// <param name="txtData">printer command previous to text</param>
        /// <param name="txtEndData">printer command after text</param>
        /// <returns>Font of text</returns>	
        private void ParseTextFontStyle(object txt, ref TextBlock textblock)
        {
            if (txt.GetType() == typeof(TextItem))
            {
                if (((TextItem)txt).FontSpecified)
                {
                    //  textblock.FontFamily = ((TextItem)txt).Font;
                }
                textblock.FontFamily = new FontFamily("Arial");
                if (((TextItem)txt).Font == Spf.Font.FontB)
                {
                    textblock.FontFamily = new FontFamily("Times New Roman");
                }
                textblock.FontSize = 12;
                if (((TextItem)txt).SizeSpecified)
                {
                    int fontsize = ((TextItem)txt).Size;
                    if (fontsize == 1) textblock.FontSize = 12;
                    if (fontsize == 2) textblock.FontSize = 15;
                    if (fontsize == 3) textblock.FontSize = 17;
                    if (fontsize == 4) textblock.FontSize = 20;
                }
                if (((TextItem)txt).Font == Font.FontB)
                {
                    textblock.FontSize--;
                }
                //if (((TextItem)txt).CharacterspacingSpecified)
                //charSpacData = printer.CharacterSpacing(((TextItem)txt).Characterspacing);

                if (((TextItem)txt).StyleSpecified)
                    if (((TextItem)txt).Style == Style.Bold || ((TextItem)txt).Style == Style.Emphasized)
                    {
                        textblock.FontWeight = FontWeights.Bold;
                    }
                if (((TextItem)txt).Alignment == Alignment.Center)
                    textblock.TextAlignment = TextAlignment.Center;
                if (((TextItem)txt).Alignment == Alignment.Right)
                    textblock.TextAlignment = TextAlignment.Right;
            }
        }

        /// <summary>
        /// parses BarcodeItem. Indents are ignored(March 2007)
        /// </summary>
        /// <param name="barcode"></param>
        /// <returns>printer command for barcodeItem</returns>
        private StringBuilder ParseBarCodeItem(BarCodeItem barcode)
        {
            StringBuilder bcData = new StringBuilder();

            if (barcode.AlignmentSpecified)
                bcData.Append(printer.Align(barcode.Alignment));
            if (barcode.HeightSpecified)
                bcData.Append(printer.BarCodeHeight(barcode.Height));
            if (barcode.WidthSpecified)
                bcData.Append(printer.BarCodeWidth(barcode.Width));
            if (barcode.HRIPosSpecified)
                bcData.Append(printer.BarCodeHRI(barcode.HRIPos));
            if (barcode.FontSpecified)
                bcData.Append(printer.BarCodeFont(barcode.Font));

            if (barcode.TypeSpecified)
                bcData.Append(printer.BarCode(barcode.Type, barcode.Value));
            else
                return null;
            bcData.Append(printer.Align(Alignment.Left));		//reset Alignment
            return bcData;
        }


    }
}
