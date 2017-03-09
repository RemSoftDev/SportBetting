using System;
using System.Collections.Generic;
using System.Text;

namespace Nbt.Services.Spf {
	/// <summary>
	/// implementation of SPF object representation
	/// </summary>
	partial class SPFPage {


		/// <summary>
		/// Gets the SPF schema.
		/// </summary>
		/// <value>The SPF schema.</value>
		public static string SPFSchema {
			get {
				return Nbt.Services.Spf.src.Resource1.SPFSchema;				
			}
		}
	}

	partial class TextItem {

		private int leftPos;
		private int textWidth;
		private string textPrinterData;
		private string posPrinterData;

		public TextItem() {
			this.LeftPos = 0;
			this.TextWidth = 0;
			textPrinterData = "";
			posPrinterData = "";
			//this.Font = Font.FontA;
		}

		[System.Xml.Serialization.XmlIgnore()]
		public int LeftPos {
			get { return leftPos; }
			set { leftPos = value; }
		}

		[System.Xml.Serialization.XmlIgnore()]		
		public int TextWidth {
			get { return textWidth; }
			set { textWidth = value; }
		}

		/// <summary>
		/// TextPrinterdata consists of ESC/commands created by Translator class, except printing position
		/// </summary>
		[System.Xml.Serialization.XmlIgnore()]		
		public string TextPrinterData {
			get { return textPrinterData; }
			set { textPrinterData = value; }
		}

		[System.Xml.Serialization.XmlIgnore()]		
		public string PositionPrinterData {
			get { return posPrinterData; }
			set { posPrinterData = value; }
		}
	}

}
