using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace Nbt.Services.Spf.Printer {
	/// <summary>
	/// for POS Printer supporting ESC/Pos </summary>
	/// <remarks>
	/// implements important features of ESC/POS Standards (for Epson TMT88 an compatbile)</remarks>	
	public class EpsonCompatible : IPrinter {


		private const char _ESC_Sign = (char)27;
		private const char _GS_Sign = (char)29;
		private const char _DLE_Sign = (char)16;	
		private const char _Horizontal_Tab = (char)9;
		private static char[] _Line_Feed = { (char)10 };
		private const Font _DefaulFont = Font.FontA;
		private const int _singleCharWidthFontA = 12;
		private const int _singleCharWidthFontB = 9;
		private const int _DefaultFontWidthFactor = 1;
		private const int _DefaultMaxTab = 42; //also max character in line of FontA  =504/12				
		private const int _DefaultDotsPerLine = 504;
		private const int _DefaultHorizontalMotionUnit = 180; //1 dot is 1/180 inch		

		private static Hashtable _charWidth = new Hashtable(2);
		private SerialPort printerPort = null;
		private BitArray printerResponse;				
		private int characterSpacing;
		private int fontsWidthFactor;
		private Font defaultFont;


		public int MaxTab { get { return _DefaultMaxTab; } }
		public int DotsPerLine { get { return _DefaultDotsPerLine; } }
		public int HorizontalMotionUnit { get { return _DefaultHorizontalMotionUnit; } }
		/// <summary>
		/// set by CharacterSize(n) function
		/// </summary>
		public int FontWidthFactor {
			get { return fontsWidthFactor; }
			//set { if (value>=1 && value <=4) fontsWidthFactor = value; }
		}		
		public Font DefaultFont {
			get { return defaultFont; }
			set { defaultFont = value; }
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="EpsonTMT88Compatible"/> class.
		/// </summary>
		/// portName COM1, BaudRate 19200, Parity.None, Databits 8, StopBits.One
		public EpsonCompatible(): this("COM1", 19200, Parity.None, 8, StopBits.One) {
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="EpsonTMT88Compatible"/> class.
		/// </summary>
		/// portName COM1, BaudRate 19200, Parity.None, Databits 8, StopBits.One
		public EpsonCompatible(string interfaceName, int baudRate, Parity parity, int dataBits, StopBits stopBits) {
			this.characterSpacing = 0;
			this.fontsWidthFactor = _DefaultFontWidthFactor;
			this.DefaultFont = _DefaulFont;
            if (!_charWidth.ContainsKey(Font.FontA))
            {
                _charWidth.Add(Font.FontA, _singleCharWidthFontA);
            }
            else
            {
                _charWidth[Font.FontA] = _singleCharWidthFontA;
            }
            if (!_charWidth.ContainsKey(Font.FontA))
            {
                _charWidth.Add(Font.FontB, _singleCharWidthFontB);
            }
            else
            {
                _charWidth[Font.FontB] = _singleCharWidthFontB;
            }
			printerPort = new SerialPort(interfaceName, baudRate, parity, dataBits, stopBits);
			printerPort.ErrorReceived += new SerialErrorReceivedEventHandler(_Port_ErrorReceived);
		}

		/// <summary>
		/// returns charwidth(including characterspacing if set) for specified Font 
		/// </summary>
		/// <param name="f"></param>
		/// <returns>char width in dots</returns>
		public int GetCharWidth(Font f) {
			if (_charWidth.ContainsKey(f))
				return (int)_charWidth[f] + characterSpacing;
			return 0;
		}
		

		/// <summary>
		/// handles events from SerialPort.ErrorReceived of a SerialPort object     
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.IO.Ports.SerialErrorReceivedEventArgs"/> instance containing the event data.</param>
		private static void _Port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e) {
		}

		/// <summary>ESC R: returns printercommand for selecting an intenational character set </summary>
		public string SelectCharacterSet(int n) {
			char[] esc_code = { _ESC_Sign, (char)'R', (char)n };
			if (n < 0 || n > 10)
				esc_code[2] = (char)0;
			return new String(esc_code);
		}

		/// <summary>ESC t: returns printercommand for selecting character code table page (ASCII >128)</summary>
		public string SelectCharacterCodeTable(int page) {
			char[] esc_code = { _ESC_Sign, (char)'t', (char)page };
			if (page < 0 || page > 5)
				esc_code[2] = (char)0;
			return new String(esc_code);

		}

		/// <summary>GS P: Sets horizontal an vertical motion units to 1/value inches 
		/// use horizontal, vertical=0 for default values (180,360 for TMT88III)</summary>
		public string SetMotionUnits(int horizontal, int vertical) {
			char[] esc_code = { _GS_Sign, (char)'P', (char)(horizontal % 256), (char)(vertical % 256) };
			return new String(esc_code);

		}

		/// <summary>ESC 3: returns printercommand for setting line spacing n*horizontal motion unit</summary>
		public string LineSpacing(int n) {
			char[] esc_code = { _ESC_Sign, (char)'3', (char)n };
			return new String(esc_code);
		}
		/// <summary>ESC 2: returns printercommand for setting line spacing 1/6 inch (30 dots)</summary>
		public string LineSpacingDefault() {
			char[] esc_code = { _ESC_Sign, (char)'2' };
			return new String(esc_code);
		}

		/// <summary>ESC M: returns printercommand for setting font</summary>
		public string CharacterFont(Font f) {
			char[] esc_code = { _ESC_Sign, (char)'M', (char)0 };
			if (f == Font.FontB)
				esc_code[2] = (char)1;
			return new String(esc_code);
		}

		/// <summary>GS !: returns printercommand for setting font size</summary>
		public string CharacterSize(int size) {
			char[] esc_code = { _GS_Sign, (char)'!', (char)0 };
			fontsWidthFactor = 1;
			switch (size) {
				case 2: esc_code[2] = (char)16;	//double width, single height
					fontsWidthFactor = 2;
				break;
				case 3: esc_code[2] = (char)1;	//single width, double height
				break;
				case 4: esc_code[2] = (char)17;	//2w 2h
					fontsWidthFactor = 2;
				break;
				case 5: esc_code[2] = (char)33;	//3w 2h
					fontsWidthFactor = 3;
				break;
				case 6: esc_code[2] = (char)18;	//2w 3h
					fontsWidthFactor = 2;
				break;
				case 7: esc_code[2] = (char)34;	//3w 3h 
					fontsWidthFactor = 3;
				break;
				case 8: esc_code[2] = (char)50;	//4w 3h
					fontsWidthFactor = 4;
				break;
				case 9: esc_code[2] = (char)35;	//3w 4h
					fontsWidthFactor = 3;
				break;
				case 10: esc_code[2] = (char)51; //4w 4h
					fontsWidthFactor = 4;
				break;
				case 11: esc_code[2] = (char)32;//3w 1h
					fontsWidthFactor = 3;
				break;
				case 12: esc_code[2] = (char)2;	//1w 3h
				break;
				case 13: esc_code[2] = (char)49; //4w 2h 
					fontsWidthFactor = 4;
				break;
				case 14: esc_code[2] = (char)19; //2w 4h
					fontsWidthFactor = 2;
				break;
			}
			return new String(esc_code);
		}

		/// <summary>ESC SP: returns printercommand for setting (right side) character spacing</summary>
		public string CharacterSpacing(int n) {
			char[] esc_code = { _ESC_Sign, (char)32, (char)0 };
			characterSpacing = n % 256;
			esc_code[2] = (char) characterSpacing;
			return new String(esc_code);
		}


		/// <summary>ESC !: returns printercommand for setting Font and Style via int value</summary>
		public string PrintMode(int n) {
			char[] esc_code = { _ESC_Sign, (char)'!', (char)0 };
			esc_code[2] = (char)(n % 256);
			return new String(esc_code);
		}

		/// <summary>ESC -: returns printercommand for underline mode on</summary>
		public string UnderlineOn() {
			char[] esc_code = { _ESC_Sign, (char)'-', (char)1 };
			//char[] esc_code = { _ESC_Sign, (char)'-', (char)1 };  //2 dot underline mode
			return new String(esc_code);
		}
		/// <summary>ESC -: returns printercommand for underline mode on</summary>
		public string UnderlineOff() {
			char[] esc_code = { _ESC_Sign, (char)'-', (char)0 };
			return new String(esc_code);
		}

		/// <summary>ESC E: returns printercommand for emphasized mode on</summary>
		public string EmphasizedOn() {
			char[] esc_code = { _ESC_Sign, (char)'E', (char)1 };
			return new String(esc_code);
		}
		/// <summary>ESC E: returns printercommand for emphasized mode off</summary>
		public string EmphasizedOff() {
			char[] esc_code = { _ESC_Sign, (char)'E', (char)0 };
			return new String(esc_code);
		}

		/// <summary>ESC G: returns printercommand for Bold(double-strike) mode on</summary>
		public string BoldOn() {
			char[] esc_code = { _ESC_Sign, (char)'G', (char)1 };
			return new String(esc_code);
		}
		/// <summary>ESC G: returns printercommand for Bold(double-strike) mode off</summary>
		public string BoldOff() {
			char[] esc_code = { _ESC_Sign, (char)'G', (char)0 };
			return new String(esc_code);
		}

		/// <summary>GS B: returns printercommand for inverted(white/black reverse) mode on</summary>
		public string InvertedOn() {
			char[] esc_code = { _GS_Sign, (char)'B', (char)1 };
			return new String(esc_code);
		}
		/// <summary>GS B: returns printercommand for inverted(white/black reverse) mode off</summary>
		public string InvertedOff() {
			char[] esc_code = { _GS_Sign, (char)'B', (char)0 };
			return new String(esc_code);
		}

		/// <summary>LF: returns printercommand for line feed</summary>
		public string LineFeed() {
			return new String(_Line_Feed);
		}
		/// <summary>LF: returns printercommand for line feed</summary>
		public string LineFeed(int n) {
			StringBuilder sb = new StringBuilder(LineFeed(), n);
			//StringBuilder sb = new StringBuilder(null);
			for (int i = 0; i < n; i++)
				sb.Append(_Line_Feed);
			
			return sb.ToString();
		}
		
		/// <summary>HT: returns printercommand for horizontal tabulator</summary>
		public string HorizontalTab() {
			return new String(_Horizontal_Tab,1);
		}

		/// <summary>ESC D: returns printercommand for setting horizontal tab positions</summary>
		public string HorizontalTabPosition(int pos) {
			char[] esc_code = { _ESC_Sign, (char)'D', (char)(pos % MaxTab), (char)0 };
			
			return new String(esc_code);
		}

		/// <summary>ESC $: returns printercommand for setting absolute horizontal print position</summary>
		public string AbsoluteHorizontalPosition(int pos) {
			char[] esc_code = { _ESC_Sign, (char)'$', (char)(pos % 256), (char)(pos / 256) };
			return new String(esc_code);
		}
		/// <summary>ESC \: returns printercommand for setting relative(from actual) horizontal print position</summary>
		public string RelativeHorizontalPosition(int pos) {
			char[] esc_code = { _ESC_Sign, (char)92, (char)(pos % 256), (char)(pos / 256) };
			return new String(esc_code);
		}

		/// <summary>GS L: returns printercommand for setting left margin (start of line)</summary>
		public string LeftMargin(int pos) {
			char[] esc_code = { _GS_Sign, (char)'L', (char)(pos % 256), (char)(pos / 256) };
			return new String(esc_code);
		}
		/// <summary>GS W: returns printercommand for setting printing rea width </summary>
		public string AreaWidth(int pos) {
			char[] esc_code = { _GS_Sign, (char)'W', (char)(pos % 256), (char)(pos / 256) };
			return new String(esc_code);
		}


		/// <summary>ESC a: returns printercommand for setting justification (left, lenter, right)</summary>
		public string Align(Alignment align) {
			char[] esc_code = { _ESC_Sign, (char)'a', (char)0 };
			if (align == Alignment.Center)
				esc_code[2] = (char)1;
			else if (align == Alignment.Right)
				esc_code[2] = (char)2;
			return new String(esc_code);
		}

		/// <summary>GS *: returns printercommand for dinfing a bit image</summary>
		public string DefineImage(string path) {
			char[] esc_code = { _GS_Sign, (char)42, (char)128, (char)12 };
			//char[] esc_code = { (char)28, (char)113, (char)1 };		//FS q, define NV bit-image 
			FileStream imageFile = null;
			try {
				Connect();
				//printerPort.Write(esc_code, 0, 2);
				imageFile = new FileStream(path, FileMode.Open, FileAccess.Read);
				for (int i = 0; i < imageFile.Length; i++)
					printerPort.Write((new byte[1] { (byte)imageFile.ReadByte() }), 0, 1);
				imageFile.Close();
			}
			catch (Exception ex) {
			//	return new String(esc_code);
				return null;
			}
			finally {
				Disconnect();
			}
			return String.Empty;
		}
		/// <summary>GS /: returns printercommand for printing bit image</summary>
		public string PrintImage(){
			char[] esc_code = {_GS_Sign, (char)47, (char)0 };
			//char[] esc_code = { (char)28, (char)112, (char)1, (char)0 };	//FS p, print NV bitimage
			//return new String(_Line_Feed) + new String(esc_code);
			return new String(esc_code);
		}


		/// <summary>GS f: returns printercommand for bar code font </summary>
		public string BarCodeFont(Font f) {
			char[] esc_code = { _GS_Sign, (char)'f', (char)0 };
			if (f == Font.FontB)
				esc_code[2] = (char)1;

			return new String(esc_code);
		}
		/// <summary>GS f, GS H: returns printercommand for position of HRI text</summary>	
		public string BarCodeHRI(BCHRIPos pos) {
			char[] esc_code = { _GS_Sign, (char)'H', (char)2 };
			if (pos == BCHRIPos.Above)
				esc_code[2] = (char)1;
			else if (pos == BCHRIPos.Both)
				esc_code[2] = (char)3;
			else if (pos == BCHRIPos.None)
				esc_code[2] = (char)0;
			return new String(esc_code);
		}
		/// <summary>GS h: returns printercommand for bar code height </summary>
		public string BarCodeHeight(int h) {
			char[] esc_code = { _GS_Sign, (char)'h', (char)162 };
			if (h > 0 && h <= 255)
				esc_code[2] = (char)h;
			return new String(esc_code);
		}
		/// <summary>GS w: returns printercommand for bar code width</summary>
		public string BarCodeWidth(int w) {
			char[] esc_code = { _GS_Sign, (char)'w', (char)3 };
			if (w >= 0 && w <= 6)
				esc_code[2] = (char)w;
			return new String(esc_code);
		}
		/// <summary>GS k: returns printercommand for printing barcode</summary>
		public string BarCode(BCType type, string data) {
			// ESC  'k'  m   n  d1..dn
			char[] esc_code = { _GS_Sign, (char)'k', (char)0, (char)0 };
			int bcType = 0;		//65..73 = m
			int n = data.Length;	// code length  = n
			//char[] bcData;
			string bcData = data;

			switch (type) {
				case BCType.UPCA:	//legth: 11..12; Characters: '0'..'9'
					if (n<11 || n> 12)
						return null;
					bcType = 65;
//					bcData = new char[n];
//					bcData = data;
					break;
				case BCType.UPCE:
					if (n<11 || n> 12)
						return null;
					bcType = 66;
					break;
				case BCType.EAN13:
					if (n<12 || n> 13)
						return null;
					bcType = 67;
					break;
				case BCType.EAN8:
					if (n<7 || n> 8)
						return null;
					bcType = 68;					
					break;
				case BCType.CODE39:	//legth: 1..255; Characters: '0'..'9', 'A".."Z", " $%+-./", "*"=start/stop char, 
					if (n<1 || n> 255)
						return null;
					bcType = 69;
					break;
				case BCType.ITF:
					if (n<1 || n> 255 || n%2!=0)  //even, "0".."9"
						return null;
					bcType = 70;
					break;
				case BCType.CODABAR:	//"0".."9", "A".."Z", "$+-./:"
					if (n<1 || n> 255)
						return null;
					bcType = 71;
					break;
				case BCType.CODE93:		//0x0h..0x7Fh
					if (n<1 || n>255)
						return null;
					bcType = 72;
					break;
				case BCType.CODE128:		//0x0h..0x7Fh
					if (n<2 || n>255)
						return null;
					bcType = 73;
					break;
			}
			if (bcType == 0)
				return null;
			
			esc_code[2] = (char)bcType;
			esc_code[3] = (char)n;

			return new String(esc_code)+bcData;
		}


		/// <summary>GS V, ESC i:returns printercommand for cutting paper </summary>
		public string CutPaper() {
			//char[] esc_code = { _GS_Sign, (char)'V', (char)1 };
			char[] esc_code = { _GS_Sign, (char)'V', (char)0 };		//full cut if supported			
			return new String(esc_code);
		}

		


		/// <summary>
		/// checks online status, for more status transmission commands see manual 'guide1.pdf' page599+
        /// Works only with Epson TM-T88
		/// </summary>
		/// <returns>
		/// 	<c>true</c> for online.
		/// </returns>
		public bool GetOnlineStatus() {
			char[] esc_code = { _DLE_Sign, (char)4, (char)1 };
			Connect();
			BitArray status = GetPrinterStatus(esc_code);
			bool retval  =  status != null  &&  !status[3];
			Disconnect();
			return retval;
			

        }

        private void ConnectAndDisconnect()
        {
            Connect();
            Disconnect();
            printerResponse = new BitArray(0);
        }

        /* Checks, if printer is reachable at all.
         * Works with both Epson TM-T88 and Star TSP100
        * */
        public bool GetConnectionStatus()
        {
            Thread thread = new Thread(new ThreadStart(ConnectAndDisconnect));
            printerResponse = null;
            thread.Start();
            int i = 0;
            while (i < 20 && printerResponse == null)
            {
                Thread.Sleep(50);
                i++;
            }
            thread.Abort();
            bool retval = printerResponse != null;
            Disconnect();
            return retval;
        }

        public bool GetPaperEndStatus()
        {
			char[] esc_code = { _DLE_Sign, (char)4, (char)4 };
			Connect();
			BitArray status = GetPrinterStatus(esc_code);
			bool retval  =  status != null  &&  (status[5] || status[6]);			
			Disconnect();
			return retval;
		}

		/// <summary>
		/// Gets the 1-byte status from printer. (for DLE EOT: Real-time status transmissions)
		/// </summary>
		private BitArray GetPrinterStatus(char[] command) {
			Thread thread = new Thread(new ThreadStart(ReadData));
			printerPort.Write(new String(command));
			thread.Start();
			while (thread.IsAlive) {
				Thread.Sleep(100);
			}
			thread = null;
			
			return printerResponse;			
		}

		/// <summary>
		/// Reads the data.
		/// </summary>
		private void ReadData() {
			try {
				lock (this) {
					printerResponse = null;
					printerPort.ReadTimeout = 2500;
					int i = printerPort.ReadByte();

					if (i != -1) {
						byte[] b = { (byte)i };
						BitArray bitArray = new BitArray(b);
						printerResponse = (BitArray)bitArray.Clone();
					}
					else {
						//_logger.Error("Read Data Failure");
					}
				}
			}
			catch (Exception ex) {
				//_logger.Error("ReadData + " + ex.ToString());
			}
		}
		/// <summary>
		/// Prints the page| sends data to the printer.
		/// </summary>
		/// <param name="page">The page.</param>
		/// <returns></returns>
		public bool PrintData(string page) {
			try {				
				Connect();		
				byte[] data = new byte[page.Length];
                for (int j = 0; j < page.Length; j++)
                {
                    data[j] = (byte)page[j];
                }
				int i = 0;
                int length = 100;
				while (i < data.Length) {
                    if (i > data.Length - 100)
                    {
                        length = data.Length % length;
                    }
					printerPort.Write(data, i, length);

					i+=100;
				}
			}
			catch (Exception ex) {
				//_logger.Error("Error printing page: " + ex.Message);
				
				return false;
			}
			finally {
				Disconnect();
			}
			return true;
		}

		/// <summary>
		/// Connects this instance.
		/// </summary>
		private void Connect() {
			if (!printerPort.IsOpen) {
				printerPort.Open();
			}
		}

		/// <summary>
		/// Disconnects this instance.
		/// </summary>
		private void Disconnect() {
			if (printerPort.IsOpen) {
				printerPort.Close();
			}
		}


        public int Margin
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
        public double PaperWidth
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
    }
}
