namespace Nbt.Services.Spf.Printer {
	/// <summary>
	/// interface for a Printer using ESC/Pos commands
	/// </summary>
	public interface IPrinter {
		int MaxTab { get ;  }
		int FontWidthFactor {
			get;
			//set;
		}
		int DotsPerLine { get; }
		int HorizontalMotionUnit { get; }		
		Font DefaultFont {
			get;
			set;
		}
        int Margin { get; set; }
        double PaperWidth { get; set; }

		int GetCharWidth(Font f);
		//ESC R: returns printercommand for selecting an intenational character set 
		string SelectCharacterSet(int n);
		//ESC t: returns printercommand for selecting character code table page (ASCII >128)
		string SelectCharacterCodeTable(int page);
		//GS P: Sets horizontal an vertical motion units to 1/value inches 
		string SetMotionUnits(int horizontal, int vertical);

		//ESC 2: returns printercommand for setting line spacing
		string LineSpacing(int n);
		//ESC 3: returns printercommand for setting line spacing
		string LineSpacingDefault();

		//ESC M: returns printercommand for setting Font 
		string CharacterFont(Font f);
		//GS !: returns printercommand for setting Character Size 
		string CharacterSize(int n);
		//ESC SP: returns printercommand for setting right-side character spacing
		string CharacterSpacing(int n);
		//ESC !: returns printercommand for Print Mode(Font/Size/Style...)
		string PrintMode(int n);
		//ESC -: returns printercommand for turning underline mode on/off
		string UnderlineOn();		
		string UnderlineOff();
		//ESC E: returns printercommand for turning emphasized mode on/off
		string EmphasizedOn();		
		string EmphasizedOff();
		//ESC G: returns printercommand for turning double-strike mode on/off
		string BoldOn();
		string BoldOff();
		//GS B: returns printercommand for turning black/white reverse printing mode on/off
		string InvertedOn();
		string InvertedOff();

		//LF :returns printercommand for linefeed
		string LineFeed();
		//LF :returns printercommand for n linefeeds
		string LineFeed(int n);

		//HT: returns printercommand for horizontal tabulator
		string HorizontalTab();
		//ESC D: returns printercommand for horizontal tabulator at pos (ESC D)
		string HorizontalTabPosition(int pos);
		//ESC $: returns printercommand for setting absolute(from start of line) horizontal print position
		string AbsoluteHorizontalPosition(int pos);
		//ESC  \: returns printercommand for setting realtive(from current) horizontal print position
		string RelativeHorizontalPosition(int pos);
		//GS L: returns printercommand for setting left margin
		string LeftMargin(int pos);
		//GS W: returns printercommand for setting printing area width
		string AreaWidth(int pos);
		//ESC a: returns printercommand for setting justification (left, lenter, right)
		string Align(Alignment align);
		
		//GS *: returns printercommand for dinfing a bit image
		string DefineImage(string path);
		//GS /: returns printercommand for printing bit image
		string PrintImage();

		//GS f: returns printercommand for bar code font 
		string BarCodeFont(Font f);
		//GS H: returns printercommand for position of HRI text		
		string BarCodeHRI(BCHRIPos pos);
		//GS h: returns printercommand for bar code height 
		string BarCodeHeight(int h);
		//GS w: returns printercommand for bar code width
		string BarCodeWidth(int w);
		//GS k: returns printercommand for printing barcode
		string BarCode(BCType type, string data);

		//GS V, ESC i:returns printercommand for cutting paper
		string CutPaper();

		//get online status
		bool GetOnlineStatus();
        //GMU 2010-10-08 get connection status
        bool GetConnectionStatus();
        //paper end reached?
		bool GetPaperEndStatus();

		//sends all data to the printer
		bool PrintData(string data);
	}
}
