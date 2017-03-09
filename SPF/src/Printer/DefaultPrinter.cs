using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading;

namespace Nbt.Services.Spf.Printer
{
    public class DefaultPrinter : IPrinter
    {


        public double PageSize { get; set; }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="EpsonTMT88Compatible"/> class.
		/// </summary>
		/// portName COM1, BaudRate 19200, Parity.None, Databits 8, StopBits.One
		public DefaultPrinter(double pixelWidth) {
            PageSize = pixelWidth;
		}

        private int margin;
        private double width;

        public int Margin
        {
            get
            {
                return margin;
            }
            set
            {
                margin = value;
            }
        }

        public double PaperWidth
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }

        public int MaxTab
        {
            get { throw new NotImplementedException(); }
        }

        public int FontWidthFactor
        {
            get { throw new NotImplementedException(); }
        }

        public int DotsPerLine
        {
            get { throw new NotImplementedException(); }
        }

        public int HorizontalMotionUnit
        {
            get { throw new NotImplementedException(); }
        }

        public Font DefaultFont
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int GetCharWidth(Font f)
        {
            throw new NotImplementedException();
        }

        public string SelectCharacterSet(int n)
        {
            throw new NotImplementedException();
        }

        public string SelectCharacterCodeTable(int page)
        {
            throw new NotImplementedException();
        }

        public string SetMotionUnits(int horizontal, int vertical)
        {
            throw new NotImplementedException();
        }

        public string LineSpacing(int n)
        {
            throw new NotImplementedException();
        }

        public string LineSpacingDefault()
        {
            throw new NotImplementedException();
        }

        public string CharacterFont(Font f)
        {
            throw new NotImplementedException();
        }

        public string CharacterSize(int n)
        {
            throw new NotImplementedException();
        }

        public string CharacterSpacing(int n)
        {
            throw new NotImplementedException();
        }

        public string PrintMode(int n)
        {
            throw new NotImplementedException();
        }

        public string UnderlineOn()
        {
            throw new NotImplementedException();
        }

        public string UnderlineOff()
        {
            throw new NotImplementedException();
        }

        public string EmphasizedOn()
        {
            throw new NotImplementedException();
        }

        public string EmphasizedOff()
        {
            throw new NotImplementedException();
        }

        public string BoldOn()
        {
            throw new NotImplementedException();
        }

        public string BoldOff()
        {
            throw new NotImplementedException();
        }

        public string InvertedOn()
        {
            throw new NotImplementedException();
        }

        public string InvertedOff()
        {
            throw new NotImplementedException();
        }

        public string LineFeed()
        {
            throw new NotImplementedException();
        }

        public string LineFeed(int n)
        {
            throw new NotImplementedException();
        }

        public string HorizontalTab()
        {
            throw new NotImplementedException();
        }

        public string HorizontalTabPosition(int pos)
        {
            throw new NotImplementedException();
        }

        public string AbsoluteHorizontalPosition(int pos)
        {
            throw new NotImplementedException();
        }

        public string RelativeHorizontalPosition(int pos)
        {
            throw new NotImplementedException();
        }

        public string LeftMargin(int pos)
        {
            throw new NotImplementedException();
        }

        public string AreaWidth(int pos)
        {
            throw new NotImplementedException();
        }

        public string Align(Alignment align)
        {
            throw new NotImplementedException();
        }

        public string DefineImage(string path)
        {
            throw new NotImplementedException();
        }

        public string PrintImage()
        {
            throw new NotImplementedException();
        }

        public string BarCodeFont(Font f)
        {
            throw new NotImplementedException();
        }

        public string BarCodeHRI(BCHRIPos pos)
        {
            throw new NotImplementedException();
        }

        public string BarCodeHeight(int h)
        {
            throw new NotImplementedException();
        }

        public string BarCodeWidth(int w)
        {
            throw new NotImplementedException();
        }

        public string BarCode(BCType type, string data)
        {
            throw new NotImplementedException();
        }

        public string CutPaper()
        {
            throw new NotImplementedException();
        }

        public bool GetOnlineStatus()
        {
            throw new NotImplementedException();
        }

        public bool GetConnectionStatus()
        {
            throw new NotImplementedException();
        }

        public bool GetPaperEndStatus()
        {
            throw new NotImplementedException();
        }

        public bool PrintData(string data)
        {
            throw new NotImplementedException();
        }
    }
}
