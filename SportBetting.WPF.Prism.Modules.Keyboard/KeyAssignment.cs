using System.Xml.Serialization;

namespace SportBetting.WPF.Prism.Modules.Keyboard
{    
	public enum WhichKeyboardLayout { Unknown, English, Arabic, French, German, Italian, Mandarin, Russian, Turkish, Serbian };

    #region class KeyAssignment
    /// <summary>
    /// A KeyAssignment defines what is assigned to one keyboard key (which we're calling key-buttons).
    /// </summary>
    [XmlRoot("KeyAssignment")]
    public class KeyAssignment
    {
        #region Constructors

        public KeyAssignment()
        {
        }

        public KeyAssignment(int iCodePoint, string sToolTip, bool isLetter)
        {
            UnshiftedCodePoint = iCodePoint;
            ShiftedCodePoint = 0;
            UnshiftedToolTip = sToolTip;
            IsLetter = isLetter;
        }

        /// <summary>
        /// This is used only for letters. It composes the tooltips automatically.
        /// </summary>
        /// <param name="iCodePoint"></param>
        /// <param name="iShiftedCodePoint"></param>
        /// <param name="isToShowBothGlyphs"></param>
        public KeyAssignment(int iCodePoint, int iShiftedCodePoint, bool isToShowBothGlyphs = false)
        {
            UnshiftedCodePoint = iCodePoint;
            ShiftedCodePoint = iShiftedCodePoint;
            string sTheLetter = ((char)iShiftedCodePoint).ToString();
            UnshiftedToolTip = "lowercase letter " + sTheLetter;
            ShiftedToolTip = "capital letter " + sTheLetter;
            IsLetter = true;
            IsToShowBothGlyphs = isToShowBothGlyphs;
        }

        public KeyAssignment(int iCodePoint, int iShiftedCodePoint, string sToolTip, string shiftedToolTip, bool isLetter, bool isToShowBothGlyphs = false)
        {
            UnshiftedCodePoint = iCodePoint;
            ShiftedCodePoint = iShiftedCodePoint;
            UnshiftedToolTip = sToolTip;
            ShiftedToolTip = shiftedToolTip;
            IsLetter = isLetter;
            IsToShowBothGlyphs = isToShowBothGlyphs;
        }

        //public KeyAssignment(int iCodePoint, int iShiftedCodePoint, string sToolTip, string sShiftedToolTip, bool isLetter, string sFontFamilyOverride)
        //{
        //    CodePoint = iCodePoint;
        //    ShiftedCodePoint = iShiftedCodePoint;
        //    ToolTip = sToolTip;
        //    ShiftedToolTip = sShiftedToolTip;
        //    IsLetter = isLetter;
        //    FontFamilyName = sFontFamilyOverride;
        //}

        #endregion Constructors

        #region Properties

        [XmlAttribute("CodePoint")]
        public int UnshiftedCodePoint { get; set; }

        [XmlAttribute("SCodePoint")]
        public int ShiftedCodePoint { get; set; }
 
        [XmlAttribute("ToolTip")]
        public string UnshiftedToolTip { get; set; }

        [XmlAttribute("SToolTip")]
        public string ShiftedToolTip { get; set; }

        [XmlAttribute("isletter")]
        public bool IsLetter { get; set; }

        [XmlAttribute("keyName")]
        public string KeyName { get; set; }

        [XmlAttribute("fontFamilyName")]
        public string FontFamilyName { get; set; }

        [XmlAttribute("htmlEntityName")]
        public string HtmlEntityName { get; set; }

        /// <summary>
        /// Get/set the flag that indicates when both the CodePoint, and the ShiftedCodePoint, glyphs
        /// are to be shown together, one below the other, on the key-cap.
        /// </summary>
        [XmlAttribute("isbothglyphs")]
        public bool IsToShowBothGlyphs { get; set; }

        #endregion Properties
    }
    #endregion
}
