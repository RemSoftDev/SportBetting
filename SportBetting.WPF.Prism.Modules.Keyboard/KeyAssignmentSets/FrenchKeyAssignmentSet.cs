namespace SportBetting.WPF.Prism.Modules.Keyboard.KeyAssignmentSets
{
    /// <summary>
    /// The FrenchKeyAssignments subclass of KeyAssignmentSet overrides those KeyAssignments that need to differ
    /// from those of the English keyboard.
    /// </summary>
    public class FrenchKeyAssignmentSet : KeyAssignmentSet
    {
        public FrenchKeyAssignmentSet()
            : base()
        {
        }

        #region KeyboardLayout
        /// <summary>
        /// Get the WhichKeyboardLayout this KeyAssignmentSet is used for.
        /// </summary>
        public override WhichKeyboardLayout KeyboardLayout
        {
            get { return WhichKeyboardLayout.French; }
        }
        #endregion

        //TODO: This is unfinished. See image at
        // en.wikipedia.org/wiki/File:KB_France.svg

        #region 13:  VK_Q
        /// <summary>
        /// 13  Replace Q with A
        /// </summary>
        public override KeyAssignment VK_Q
        {
            get
            {
                if (_VK_Q == null)
                {
                    _VK_Q = new KeyAssignment(0x0061, 0x0041);
                }
                return _VK_Q;
            }
        }
        #endregion

        #region 14: VK_W
        /// <summary>
        /// 14  Swap the Z and W
        /// </summary>
        public override KeyAssignment VK_W
        {
            get
            {
                if (_VK_W == null)
                {
                    _VK_W = new KeyAssignment(0x007A, 0x005A);
                }
                return _VK_W;
            }
        }
        #endregion

        #region 24: VK_OEM_6
        /// <summary>
        /// 24  Replace the Right brackets with dollar sign/franc sign
        /// TODO: This should actually get a 3rd, blue symbol - the currency sign.
        /// </summary>
        public override KeyAssignment VK_OEM_6
        {
            get
            {
                if (_VK_OEM_6 == null)
                {
                    _VK_OEM_6 = new KeyAssignment(0x0024, 0x00A3, "Dollar sign", "Pound sign", false, true);
                }
                return _VK_OEM_6;
            }
        }
        #endregion

        #region 26: VK_A
        /// <summary>
        /// 26  Replace A with Q
        /// </summary>
        public override KeyAssignment VK_A
        {
            get
            {
                if (_VK_A == null)
                {
                    _VK_A = new KeyAssignment(0x0071, 0x0051);
                }
                return _VK_A;
            }
        }
        #endregion

        #region 35: VK_OEM_1
        /// <summary>
        /// 35  Replace the semicolon/colon with M
        /// </summary>
        public override KeyAssignment VK_OEM_1
        {
            get
            {
                if (_VK_OEM_1 == null)
                {
                    _VK_OEM_1 = new KeyAssignment(0x006D, 0x004D);
                }
                return _VK_OEM_1;
            }
        }
        #endregion

        #region 36: VK_OEM_7
        /// <summary>
        /// 36  Replace Apostrophe/Quotation with Letter U with grave/Micro
        /// </summary>
        public override KeyAssignment VK_OEM_7
        {
            get
            {
                if (_VK_OEM_7 == null)
                {
                    //TODO: Actually, we may need to allow the U-with-grave to have a CAPS state.
                    _VK_OEM_7 = new KeyAssignment(0x00F9, 0x00B5, "letter U with grave", "Micro sign", false, true);
                }
                return _VK_OEM_7;
            }
        }
        #endregion

        #region 37: VK_Z
        /// <summary>
        /// 37  Swap the Z and W
        /// </summary>
        public override KeyAssignment VK_Z
        {
            get
            {
                if (_VK_Z == null)
                {
                    _VK_Z = new KeyAssignment(0x0077, 0x0057);
                }
                return _VK_Z;
            }
        }
        #endregion

    }
}
