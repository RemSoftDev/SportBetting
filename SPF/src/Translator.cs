using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;

//using System.Xml.XPath;
//using System.Xml.Serialization;
using Nbt.Services.Spf.Printer;

namespace Nbt.Services.Spf
{

    public class Translator
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

        /// <summary>
        /// Translator instance is used to translate from SPFPage object to a printer command 
        /// </summary>
        /// <param name="iPrinter">specifies the printer to create correct syntax of printer command</param>
        /// <param name="spfPage">object to be printed</param>
        public Translator(IPrinter iPrinter, SPFPage spfPage)
        {
            this.printer = iPrinter;
            this.page = spfPage;
            this.Codepage = 852; //default
            pageData = new StringBuilder();
        }

        /// <summary>
        /// Parses the SPFPage object (set by constructor) and returns a printer command.
        /// syntax of returned printer command is assembled using IPrinter compatible printer (set in constructor)
        /// </summary>
        /// <returns>the printer command for printing SPFPage (with IPinter)</returns>
        public string Parse()
        {
            if (page == null || page.Items == null || page.Items.Length <= 0)
                return null;
            //return String.Empty;

            if (printer == null)
                return null;

            if (page.CodepageSpecified)
                this.Codepage = (int)page.Codepage;

            foreach (Object elem in page.Items)
            {
                if (elem.GetType() == typeof(Line))
                {
                    Line line = elem as Line;
                    pageData.Append(ParseLine(line));
                }
                if (elem.GetType() == typeof(Block))
                {
                    Block blk = elem as Block;
                    pageData.Append(ParseBlock(blk));
                }
            }
            pageData.Append(printer.LineFeed(5));
            pageData.Append(printer.CutPaper());

            return pageData.ToString();
        }

        /// <summary>
        /// parses Block which consist of one or more BlockTexts using method ParsteBlockText(BlockText).
        /// </summary>
        /// <param name="block">block of page</param>
        /// <returns>printer command for block</returns>
        private StringBuilder ParseBlock(Block block)
        {
            StringBuilder blockData = new StringBuilder();

            if (block.LineSpacingSpecified)
                blockData.Append(printer.LineSpacing(block.LineSpacing));
            else
                blockData.Append(printer.LineSpacingDefault());

            if (block.BlockText != null)
                foreach (BlockText bt in block.BlockText)
                {
                    blockData.Append(printer.LineFeed());
                    pageData.Append(ParseBlockText(bt));
                }

            return blockData;
        }

        /// <summary>
        /// parses BlockText (part of Block). called from ParseBlock(...).
        /// </summary>
        /// <param name="txt">blockText of Block</param>
        /// <returns>printer command for blocktext</returns>
        private StringBuilder ParseBlockText(BlockText txt)
        {
            StringBuilder btData = new StringBuilder();
            StringBuilder btEndData = new StringBuilder();

            if (txt.Value == null)
                return null;

            Font btFont = ParseTextFontStyle(txt, ref btData, ref btEndData);

            StringBuilder blockData2 = new StringBuilder();
            char[] seperator2 = new char[] { ' ' };
            char[] seperator3 = new char[] { '\n', '\r', '\t' };
            char[] seperatorAll = new char[seperator2.Length + seperator3.Length];
            Array.Copy(seperator2, seperatorAll, seperator2.Length);
            Array.Copy(seperator3, 0, seperatorAll, seperator2.Length, seperator3.Length);

            int foundPos = 0;
            int startPos = -1;
            int dotsUsed = 0;
            string singleWord;
            string[] splittedWord;
            int width = printer.GetCharWidth(btFont) * printer.FontWidthFactor;
            txt.Value = Convert(txt.Value);
            do
            {
                startPos = foundPos + 1;
                foundPos = txt.Value.IndexOfAny(seperator2, startPos);

                if (foundPos > -1)		// whitespace found
                    singleWord = txt.Value.Substring(startPos - 1, foundPos - startPos + 1);
                else
                    singleWord = txt.Value.Substring(startPos - 1);  //last word of block

                splittedWord = singleWord.Split(seperator3, StringSplitOptions.RemoveEmptyEntries);	//split lineFeed
                for (int i = 0; i < splittedWord.Length; i++)
                {
                    if (i > 0)
                    {								//replace linfe feed
                        blockData2.Append(printer.LineFeed());
                        dotsUsed = 0;
                    }
                    if (dotsUsed > 0 && splittedWord[i].Length * width + dotsUsed > printer.DotsPerLine)
                    {
                        blockData2.Append(printer.LineFeed());		//print word in new line
                        dotsUsed = 0;
                        splittedWord[i] = splittedWord[i].TrimStart(seperatorAll); 	//without leadingwhitspac
                    }

                    blockData2.Append(splittedWord[i]);
                    dotsUsed += splittedWord[i].Length * width;
                }

            } while (foundPos > -1);


            btData.Append(blockData2);
            btData.Append(btEndData);

            return btData;


        }


        /// <summary>
        /// predicate function for generic list (findAll( )). true for all TextItem obejcts
        /// </summary>
        /// <param name="item">List type object</param>
        /// <returns>true for TextItem obejcts</returns>
        private static bool IsTextItem(object item)
        {
            return item.GetType() == typeof(TextItem);
        }
        /// <summary>
        /// to find matching objects in a generic list. true for all TexItem objects where LeftPos is set.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true for all TexItem objects where LeftPos is set</returns>
        private static bool IsSpecialPosTextItem(object item)
        {
            if (item.GetType() == typeof(TextItem))
                return ((TextItem)item).LeftPos > 0;
            return false;
        }

        /// <summary>
        /// parses a Line, Lien can consist of (one or more) TextItem, FillItem, BarcodeItem or ImageItem
        /// </summary>
        /// <param name="line"></param>
        /// <returns>printer command for Line-part of SPF-object</returns>
        private StringBuilder ParseLine(Line line)
        {
            if (line.Items == null || line.Items.Length <= 0)
                return null;

            StringBuilder lineData = new StringBuilder();
            List<object> textAndFillItem = new List<object>();
            List<int> textWidth = new List<int>();
            int txtWidthSum = 0;
            int numFillItems = 0;

            int recurrence = 0;
            bool underline = false;

            if (line.RecurrenceSpecified)
                recurrence = line.Recurrence;
            if (line.UnderlineSpecified)
                underline = line.Underline;

            lineData.Append(printer.LineFeed());

            if (line.LineSpacingSpecified)
                lineData.Append(printer.LineSpacing(line.LineSpacing));
            else
                lineData.Append(printer.LineSpacingDefault());
            if (underline)
                lineData.Append(printer.UnderlineOn());


            foreach (Object item in line.Items)
            {
                if (item.GetType() == typeof(ImageItem))
                {
                    ImageItem image = item as ImageItem;
                    lineData.Append(ParseImageItem(image));
                }
                if (item.GetType() == typeof(BarCodeItem))
                {
                    BarCodeItem bc = item as BarCodeItem;
                    lineData.Append(ParseBarCodeItem(bc));
                    lineData.Append(printer.AbsoluteHorizontalPosition(0));
                }
                if (item.GetType() == typeof(TextItem))
                {
                    TextItem txt = item as TextItem;
                    ParseTextItem(txt);			//set some properties oft txt TextItem
                    textAndFillItem.Add(txt);
                    //textWidth.Add(txt.TextWidth);
                    txtWidthSum += txt.TextWidth;
                    //lineData.Append(ParseTextItem(txt));
                }
                if (item.GetType() == typeof(FillItem))
                {
                    FillItem fill = item as FillItem;
                    textAndFillItem.Add(fill);
                    numFillItems++;
                    //lineData.Append(ParseFillItem(fill));
                }

            }

            #region Append printer command for text and fill items (stored in textAndFillItem List).
            // quite simple when there are no fill items (else condition). Mixed lists will be parsed until the first
            // textItem with special position (indent, align center|right), appended and this is repeated with the remaining list items.			
            if (numFillItems > 0)
            {
                Font fillFont = printer.DefaultFont;	//fill font will be font of previous text item or defaultfont
                int i = 0, old_i = 0, remain = textAndFillItem.Count;
                int leftPosAfter = 0;		//!?
                double fullSpace = 0.0;
                int specialTextPosInd;
                int fillWidth = 0;
                int unusedWidth = 0;		//should always be 0 except when mixing fonts or fontB with Indent

                do
                {			//try to arragne mixture fo text and fill items
                    specialTextPosInd = textAndFillItem.GetRange(old_i, remain).FindIndex(IsSpecialPosTextItem) + old_i;
                    if (specialTextPosInd < 0)
                    {  //no special positioned text in line
                        specialTextPosInd = textAndFillItem.Count;		//parse all
                        fullSpace = printer.DotsPerLine - leftPosAfter;
                    }
                    else
                    {
                        int leftPosTmp = ((TextItem)textAndFillItem[specialTextPosInd]).LeftPos;
                        fullSpace = leftPosTmp - leftPosAfter;
                        leftPosAfter = leftPosTmp + ((TextItem)textAndFillItem[specialTextPosInd]).TextWidth;
                        txtWidthSum = 0;
                        foreach (object item in textAndFillItem.GetRange(old_i, specialTextPosInd - old_i).FindAll(IsTextItem))
                            txtWidthSum += ((TextItem)item).TextWidth;
                        numFillItems = textAndFillItem.GetRange(old_i, specialTextPosInd - old_i).FindAll(delegate(object o) { return o.GetType() == typeof(FillItem); }).Count;
                    }

                    float fillSpace = ((float)fullSpace - txtWidthSum) / numFillItems + unusedWidth;//standard fill
                    //foreach (object item in textAndFillItem) {
                    for (i = old_i; i <= specialTextPosInd && i < textAndFillItem.Count; i++)
                    {
                        if (textAndFillItem[i].GetType() == typeof(TextItem))
                        {
                            TextItem txtItem = textAndFillItem[i] as TextItem;
                            lineData.Append(txtItem.TextPrinterData);
                            if (txtItem.FontSpecified)
                                fillFont = txtItem.Font;
                        }
                        else if (textAndFillItem[i].GetType() == typeof(FillItem))
                        {
                            lineData.Append(ParseFillItem((FillItem)textAndFillItem[i], (int)fillSpace, fillFont, out fillWidth));
                            unusedWidth = (int)fillSpace - fillWidth;
                        }
                    }
                    //if fillWidth < fillSpace (due to FontB in special cases) filling is not 100% excat

                    old_i = specialTextPosInd + 1;		//first not printed item
                    remain = textAndFillItem.Count - i;
                } while (specialTextPosInd < textAndFillItem.Count && remain > 0);

            }
            else
            {				//no fillitems
                if (textAndFillItem != null)
                {
                    int leftPos = 0;
                    foreach (Object item in textAndFillItem)
                        if (item.GetType() == typeof(TextItem))
                        {  //must be
                            TextItem txtItem = item as TextItem;
                            if (txtItem.LeftPos > 0)
                            {
                                txtItem.LeftPos = LeftPosText(leftPos, txtItem);
                                lineData.Append(printer.AbsoluteHorizontalPosition(txtItem.LeftPos));
                            }
                            lineData.Append(txtItem.TextPrinterData);
                            leftPos += txtItem.LeftPos + txtItem.TextWidth;
                        }
                }

                if (underline)
                {		//only fillitem free lines have to be underlined 'manually'
                    lineData.Append(printer.AbsoluteHorizontalPosition(0));
                    lineData.Append(printer.CharacterFont(printer.DefaultFont));
                    for (int i = 0; i < printer.MaxTab - 1; i++)
                        lineData.Append(" ");
                }
            }
            #endregion

            if (underline)
                lineData.Append(printer.UnderlineOff());

            if (recurrence > 0)
            {
                StringBuilder manyLines = new StringBuilder(lineData.Length * recurrence);
                for (int j = 0; j < recurrence; j++)
                    manyLines.Append(lineData);
                lineData.Append(manyLines);
            }

            return lineData;
        }

        /// <summary>
        /// parses "FontStyle" for BlockText and TextItem
        /// </summary>
        /// <param name="txt">BlockText or TextItem</param>
        /// <param name="txtData">printer command previous to text</param>
        /// <param name="txtEndData">printer command after text</param>
        /// <returns>Font of text</returns>	
        private Font ParseTextFontStyle(object txt, ref StringBuilder txtData, ref StringBuilder txtEndData)
        {
            Font txtFont = printer.DefaultFont;
            if (txt.GetType() != typeof(BlockText) && txt.GetType() != typeof(TextItem))
            {
                return txtFont;
            }

            string fontData, fontSizeData, charSpacData, fontStyleData, underlinedData, invertedData;
            fontData = fontSizeData = charSpacData = fontStyleData = underlinedData = invertedData = String.Empty;

            //Codeverdopplung wegen "Fehlers" im XML Schema (da Attribut von komplexen Typ nicht möglich)
            //allerdings kann so der FontStyle von TextItem und BlockText unterschiedlich behandelt werden
            if (txt.GetType() == typeof(TextItem))
            {
                if (((TextItem)txt).FontSpecified)
                {
                    txtFont = ((TextItem)txt).Font;
                    fontData = printer.CharacterFont(((TextItem)txt).Font);//print mode 			
                }
                if (((TextItem)txt).SizeSpecified)
                    fontSizeData = printer.CharacterSize(((TextItem)txt).Size); //print mode									
                if (((TextItem)txt).CharacterspacingSpecified)
                    charSpacData = printer.CharacterSpacing(((TextItem)txt).Characterspacing);

                if (((TextItem)txt).StyleSpecified)
                    if (((TextItem)txt).Style == Style.Bold)
                    {
                        fontStyleData = printer.BoldOn();
                        txtEndData.Append(printer.BoldOff());
                    }
                    else if (((TextItem)txt).Style == Style.Emphasized)
                    {
                        fontStyleData = printer.EmphasizedOn();
                        txtEndData.Append(printer.EmphasizedOff());  //print mode
                    }
                if (((TextItem)txt).UnderlineSpecified)
                {
                    underlinedData = printer.UnderlineOn();
                    txtEndData.Append(printer.UnderlineOff());		//print mode
                }
                if (((TextItem)txt).InvertSpecified)
                {
                    invertedData = printer.InvertedOn();
                    txtEndData.Append(printer.InvertedOff());
                    //((TextItem)txt).Value = 
                }
                //calculate text Width
                ((TextItem)txt).TextWidth = ((TextItem)txt).Value != null ? ((TextItem)txt).Value.Length * printer.GetCharWidth(txtFont) * printer.FontWidthFactor : 0;
                //reset to normal:
                txtEndData.Append(printer.CharacterFont(printer.DefaultFont));
                txtEndData.Append(printer.CharacterSize(1));
                txtEndData.Append(printer.CharacterSpacing(0));

            }
            else
            {
                if (((BlockText)txt).FontSpecified)
                {
                    txtFont = ((BlockText)txt).Font;
                    fontData = printer.CharacterFont(((BlockText)txt).Font);//print mode 
                }
                if (((BlockText)txt).SizeSpecified)
                    fontSizeData = printer.CharacterSize(((BlockText)txt).Size); //print mode
                if (((BlockText)txt).CharacterspacingSpecified)
                {
                    charSpacData = printer.CharacterSpacing(((BlockText)txt).Characterspacing);
                    txtEndData.Append(printer.CharacterSpacing(0));
                }
                if (((BlockText)txt).StyleSpecified)
                    if (((BlockText)txt).Style == Style.Bold)
                    {
                        fontStyleData = printer.BoldOn();
                        txtEndData.Append(printer.BoldOff());
                    }
                    else if (((BlockText)txt).Style == Style.Emphasized)
                    {
                        fontStyleData = printer.EmphasizedOn();
                        txtEndData.Append(printer.EmphasizedOff());  //print mode
                    }
                if (((BlockText)txt).UnderlineSpecified)
                {
                    underlinedData = printer.UnderlineOn();
                    txtEndData.Append(printer.UnderlineOff());		//print mode
                }
                if (((BlockText)txt).InvertSpecified)
                {
                    invertedData = printer.InvertedOn();
                    txtEndData.Append(printer.InvertedOff());
                }
            }

            //txtEndData.Append(printer.PrintMode(0));

            txtData.Append(fontData);
            txtData.Append(fontSizeData);
            txtData.Append(charSpacData);
            txtData.Append(fontStyleData);
            txtData.Append(underlinedData);
            txtData.Append(invertedData);

            return txtFont;
        }

        /// <summary>
        /// helping function for parsing text(and fill)Items.
        /// calculates left position of text (set with indent or Alignment)
        /// </summary>
        /// <param name="leftStart">offset from left margin</param>
        /// <param name="txt">containing text</param>
        /// <returns>absolute position (in dots) from left margin</returns>
        private int LeftPosText(int leftStart, TextItem txt)
        {
            int leftPos = 0;
            Alignment align = Alignment.Left;
            int indent = 0;

            if (txt.IndentSpecified)
                indent = txt.Indent;
            if (txt.AlignmentSpecified)
                align = txt.Alignment;


            if (indent > 0)
            {
                switch (align)
                {
                    case Alignment.Right:
                        leftPos = (int)((double)indent / printer.MaxTab * printer.DotsPerLine - txt.TextWidth);
                        break;
                    case Alignment.Center:
                        leftPos = (int)((double)indent / printer.MaxTab * printer.DotsPerLine - txt.TextWidth / 2.0);
                        break;
                    default:
                        leftPos = (int)((double)indent / printer.MaxTab * printer.DotsPerLine);
                        break;
                }
            }
            else
            {		//no indent
                switch (align)
                {
                    case Alignment.Right:
                        leftPos = (int)printer.DotsPerLine - txt.TextWidth;
                        break;
                    case Alignment.Center:
                        leftPos = (int)((double)leftStart + (printer.DotsPerLine - leftStart) / 2.0 - txt.TextWidth / 2.0);
                        break;
                    default:
                        leftPos = (int)leftStart;
                        break;
                }
            }
            return (leftPos >= 0) ? leftPos : 0;
        }

        /// <summary>
        /// parses TextItem and sets fields(added to xml-schema fields) of TextItem object:
        /// 
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        private StringBuilder ParseTextItem(TextItem txt)
        {
            Font txtFont = printer.DefaultFont;
            int leftPos = 0;
            StringBuilder txtData = new StringBuilder();
            StringBuilder txtEndData = new StringBuilder();

            txtFont = ParseTextFontStyle(txt, ref txtData, ref txtEndData);

            txt.LeftPos = LeftPosText(0, txt);
            txt.PositionPrinterData = printer.AbsoluteHorizontalPosition(leftPos);  //default

            //Console.WriteLine("txt_value="+txt.Value);
            txtData.Append(Convert(txt.Value));

            txtData.Append(txtEndData);

            txt.TextPrinterData = txtData.ToString();

            return txtData;
        }

        /// <summary>
        /// Parses FillItem
        /// </summary>
        /// <param name="padding">FillItem</param>
        /// <param name="space">space for FllItem (in dots)</param>
        /// <param name="font"></param>
        /// <param name="fillWidth">returns width (in dots) of in fact connected fillitems</param>
        /// <returns>printer command for FillItem</returns>
        private StringBuilder ParseFillItem(FillItem padding, int space, Font font, out int fillWidth)
        {
            StringBuilder fillText = new StringBuilder();
            int i = 0;
            if (padding.Value == null)
                padding.Value = " ";
            int patternWidth = padding.Value.Length * printer.GetCharWidth(font);
            fillText.Append(printer.CharacterFont(font));
            for (i = 0; i < space / patternWidth; i++)
                fillText.Append(padding.Value);

            fillWidth = i * patternWidth;
            //space - i*patternWidth  == remaining space
            return fillText;
        }

        /// <summary>
        /// parses ImageItem. Indents are ignored, image is uploaded to printer immediately (March 2007)
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private StringBuilder ParseImageItem(ImageItem image)
        {
            StringBuilder imageData = new StringBuilder();

            if (image.AlignmentSpecified)
                imageData.Append(printer.Align(image.Alignment));
            if (image.Path != null && image.Path != "")
                printer.DefineImage(image.Path);
            imageData.Append(printer.PrintImage());
            return imageData;
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



        /// <summary>
        /// converts text with Encoder defined by Codepage.
        /// used for converting Text.
        /// </summary>
        /// <param name="text">text (encoded with Encoding.Default)</param>
        /// <returns>with Codepage Encoder encoded text</returns>
        public string Convert(string text)
        {
            if (text == null)
                return String.Empty;

            Encoding enc_dest = Encoding.GetEncoding(this.Codepage);
            Encoding enc_src = Encoding.Default;

            byte[] data = Encoding.Convert(enc_src, enc_dest, Encoding.Default.GetBytes(text));
            char[] ret_data = new char[data.Length];

            for (int i = 0; i < data.Length; i++)
                ret_data[i] = (char)data[i];

            return new String(ret_data);
        }



        //string xmlLine1 = "<Line Recurrence=2 LineSpacing=50 UnderLine=true><TextItem>text bla bla<TextItem></Line>";
        /// <summary>
        /// unused test function. parsing xml manually. use XmlPreprocess.Marshaling instead.
        /// </summary>
        /// <param name="xmlLine"></param>
        /// <returns></returns>
        public static Line ParseTextLine(string xmlLine)
        {
            Line item = new Line();

            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(xmlLine);
            /*if (xDoc.HasChildNodes) {
                foreach (XmlNode xn in xDoc.ChildNodes) {
                    Console.WriteLine(" xn =" + xn.Name);
                    Console.WriteLine(" xn attribs:" + xn.Attributes.Count);


                    foreach (XmlAttribute xna in xn.Attributes) {
                        Console.WriteLine("xn attr count:" + xna.GetType().ToString() );						
                        Console.WriteLine("axnode Name:" + xna.Name);
                        Console.WriteLine("axn Value    :" + xna.Value); 
                    }
                }
            }*/

            Console.WriteLine();
            foreach (XmlNode xNode in xDoc)
            {
                if (xNode.NodeType == XmlNodeType.Attribute)
                {
                    Console.WriteLine("xnode Attrib Name:" + xNode.Name);
                    Console.WriteLine("xnode Attrib Val :" + xNode.Value);
                }
                else if (xNode.NodeType == XmlNodeType.Document)
                {
                    Console.WriteLine("Document type follows..");
                }
                else if (xNode.NodeType == XmlNodeType.Element)
                {
                    Console.WriteLine("Element type follows..");
                    Console.WriteLine("xnode  ELEM Name:" + xNode.Name);
                }

                /*Console.WriteLine("xnode attr count:" + xNode.Attributes.Count);
                Console.WriteLine("xnode inner text:" + xNode.InnerText );
                Console.WriteLine("xnode child count:" + xNode.ChildNodes.Count );
                Console.WriteLine("xnode inner xml:" + xNode.InnerXml );
                Console.WriteLine("xnode outerXML :" + xNode.OuterXml );				
                Console.WriteLine("xnode type:" + xNode.GetType().ToString());
                */

                Console.WriteLine("attribs:" + xNode.Attributes.Count);
                foreach (XmlNode xn in xNode.Attributes)
                {
                    Console.WriteLine("xnode attribs Name:" + xn.Name);
                    Console.WriteLine("xnode attribs Value    :" + xn.Value);

                }

                foreach (XmlElement xe in xNode.ChildNodes)
                {
                    Console.WriteLine("xnode elem type:" + xe.GetType().ToString());
                    Console.WriteLine("xnode elem Name:" + xe.Name);
                    //Console.WriteLine("xnode elem Val :" + xe.Value);
                    Console.WriteLine("xnode elem childs" + xe.FirstChild.Value);
                }
            }
            Console.WriteLine(" name:" + xDoc.Name);
            Console.WriteLine(" type:" + xDoc.NodeType);
            //			StreamReader reader = new System.IO.StreamReader(
            //System.IO.Stream stream = System.IO.Stream;
            return item;
        }
    }
}