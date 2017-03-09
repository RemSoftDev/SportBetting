using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using BarcodeLib;
using FontStyle = System.Drawing.FontStyle;
using Image = System.Windows.Controls.Image;

namespace Nbt.Services.Spf
{
	public class BarCodeCreator
	{
		String alphabet39 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-. $/+%*";

		String[] coded39Char = 
		{
            /* 0 */ "000110100", 
            /* 1 */ "100100001", 
            /* 2 */ "001100001", 
            /* 3 */ "101100000",
            /* 4 */ "000110001", 
            /* 5 */ "100110000", 
            /* 6 */ "001110000", 
            /* 7 */ "000100101",
            /* 8 */ "100100100", 
            /* 9 */ "001100100", 
            /* A */ "100001001", 
            /* B */ "001001001",
            /* C */ "101001000", 
            /* D */ "000011001", 
            /* E */ "100011000", 
            /* F */ "001011000",
            /* G */ "000001101", 
            /* H */ "100001100", 
            /* I */ "001001100", 
            /* J */ "000011100",
            /* K */ "100000011", 
            /* L */ "001000011", 
            /* M */ "101000010", 
            /* N */ "000010011",
            /* O */ "100010010", 
            /* P */ "001010010", 
            /* Q */ "000000111", 
            /* R */ "100000110",
            /* S */ "001000110", 
            /* T */ "000010110", 
            /* U */ "110000001", 
            /* V */ "011000001",
            /* W */ "111000000", 
            /* X */ "010010001", 
            /* Y */ "110010000", 
            /* Z */ "011010000",
            /* - */ "010000101", 
            /* . */ "110000100", 
            /*' '*/ "011000100",
            /* $ */ "010101000",
            /* / */ "010100010", 
            /* + */ "010001010", 
            /* % */ "000101010", 
            /* * */ "010010100" 
		};


		public Canvas BarCode128(string code)
		{
			//var b = new BarcodeLib.Barcode(code, BarcodeLib.TYPE.CODE128);
            BarcodeLib.Barcode b = new BarcodeLib.Barcode(code, BarcodeLib.TYPE.CODE128);
			b.Alignment = AlignmentPositions.CENTER;
			b.IncludeLabel = false;
			b.Height = 75;
		    b.Width = 350;
            //b.BarWidth = 1000;
		    while (true)
		    {
                try
                {
                    b.Encode(BarcodeLib.TYPE.CODE128, code);
                    break;
                }
                catch (Exception)
                {
                    b.Width +=5;
                }
		    }
		    
			MemoryStream ms = new MemoryStream();
			b.SaveImage(ms, SaveTypes.BMP); // GIF

			BitmapImage imageSource = new BitmapImage();
			imageSource.BeginInit();
			imageSource.StreamSource = ms;
			imageSource.EndInit();
			Image image = new System.Windows.Controls.Image();
			image.HorizontalAlignment = HorizontalAlignment.Stretch;
			image.Source = imageSource;
            image.Width = 250;
			Canvas canvas = new Canvas() { Height = 60, HorizontalAlignment = HorizontalAlignment.Center, };

			canvas.Margin = new System.Windows.Thickness(0, 0, 0, 0);
			canvas.Children.Add(image);
            canvas.Width = 250;

			return canvas;
		}

		public Canvas BarCode(string code, double width)
		{
			Canvas canvas = new Canvas() { Height = 60 };
			canvas.Margin = new Thickness(20, 10, 20, 5);
			String intercharacterGap = "0";
			String str = '*' + code.ToUpperInvariant() + '*';

			String encodedString = "";
			for (int i = 0; i < str.Length; i++)
			{
				if (i > 0)
					encodedString += intercharacterGap;
				encodedString += coded39Char[alphabet39.IndexOf(str[i])];
			}

			double wideToNarrowRatio = 4;
			int widthOfBarCodeString = 0;

			foreach (char c in encodedString)
			{
				if (c == '1')
					widthOfBarCodeString += (int)(wideToNarrowRatio);
				else
					widthOfBarCodeString++;
			}

			int x = 0;
			int wid = 0;

			canvas.Width = widthOfBarCodeString + 180;

			for (int i = 0; i < encodedString.Length; i++)
			{
				if (encodedString[i] == '1')
					wid = (int)(wideToNarrowRatio * (int)1);
				else
					wid = (int)1;

				SolidColorBrush myBrush;
				if (i % 2 == 0)
					myBrush = new SolidColorBrush(Colors.Black);
				else
					myBrush = new SolidColorBrush(Colors.White);

				Canvas c = new Canvas() { Width = wid, Height = 50, Background = myBrush };
				Canvas.SetLeft(c, x);
				canvas.Children.Add(c);

				x += wid;
			}

			return canvas;


		}
	}
}
