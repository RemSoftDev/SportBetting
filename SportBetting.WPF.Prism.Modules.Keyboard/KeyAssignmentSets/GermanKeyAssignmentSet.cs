namespace SportBetting.WPF.Prism.Modules.Keyboard.KeyAssignmentSets
{
    /// <summary>
    /// The GermanKeyAssignments subclass of DefaultKeyAssignments overrides those KeyAssignments that need to differ
    /// from those of the English keyboard.
    /// </summary>
    public class GermanKeyAssignmentSet : KeyAssignmentSet
    {
        public GermanKeyAssignmentSet()
            : base()
        {
        }

        #region KeyboardLayout
        /// <summary>
        /// Get the WhichKeyboardLayout this KeyAssignmentSet is used for.
        /// </summary>
        public override WhichKeyboardLayout KeyboardLayout
        {
            get { return WhichKeyboardLayout.German; }
        }
        #endregion

        // See http://en.wikipedia.org/wiki/File:KB_Germany.svg

        #region 11: VK_OEM_MINUS
        /// <summary>
        /// 11  Replace the minus/underscore with Eszett (also called scharfes S)
        ///     German keyboard has Question-mark in the upper location. I'm deciding not to.
        /// </summary>
        public override KeyAssignment VK_OEM_MINUS
        {
            // See http://en.wikipedia.org/wiki/B <- that last char is the eszett.
            get
            {
                if (_VK_OEM_MINUS == null)
                {
                    _VK_OEM_MINUS = new KeyAssignment(0x00DF, 0x1E9E, "Eszett", "capital Eszett", true, false);
                }
                return _VK_OEM_MINUS;
            }
        }
        #endregion

        #region 18: VK_Y
        /// <summary>
        /// 18  Swap Z and Y
        /// </summary>
        public override KeyAssignment VK_Y
        {
            get
            {
                if (_VK_Y == null)
                {
                    _VK_Y = new KeyAssignment(0x007A, 0x005A);
                }
                return _VK_Y;
            }
        }
        #endregion

        #region 23: VK_OEM_4
        /// <summary>
        /// 23  U with umlaut (also called diaeresis)
        /// </summary>
        public override KeyAssignment VK_OEM_4
        {
            get
            {
                if (_VK_OEM_4 == null)
                {
                    _VK_OEM_4 = new KeyAssignment(0x00FC, 0x00DC, "letter u with umlaut", "Capital U with umlaut", true, false);
                }
                return _VK_OEM_4;
            }
        }
        #endregion

        #region 35: VK_OEM_1
        /// <summary>
        /// 35  Letter O with umlaut
        /// </summary>
        public override KeyAssignment VK_OEM_1
        {
            get
            {
                if (_VK_OEM_1 == null)
                {
                    _VK_OEM_1 = new KeyAssignment(0x00F6, 0x00D6, "lowercase letter O with umlaut", "capital letter O with umlaut", true, false);
                }
                return _VK_OEM_1;
            }
        }
        #endregion

        #region 36: VK_OEM_7
        /// <summary>
        /// 36  Letter A with umlaut
        /// </summary>
        public override KeyAssignment VK_OEM_7
        {
            get
            {
                if (_VK_OEM_7 == null)
                {
                    _VK_OEM_7 = new KeyAssignment(0x00E4, 0x00C4, "letter a with umlaut", "Capital A with umlaut", true, false);
                }
                return _VK_OEM_7;
            }
        }
        #endregion

        #region 37: VK_Z
        /// <summary>
        /// 37  Swap Z and Y.
        /// </summary>
        public override KeyAssignment VK_Z
        {
            get
            {
                if (_VK_Z == null)
                {
                    _VK_Z = new KeyAssignment(0x0079, 0x0059);
                }
                return _VK_Z;
            }
        }
        #endregion

    }
}
