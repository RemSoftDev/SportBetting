namespace SportBetting.WPF.Prism.Modules.Keyboard.KeyAssignmentSets
{
    /// <summary>
    /// The SpanishKeyAssignments subclass of KeyAssignmentSet overrides those KeyAssignments that need to differ
    /// from those of the English keyboard.
    /// </summary>
    public class TurkishKeyAssignmentSet : KeyAssignmentSet
    {
        public TurkishKeyAssignmentSet()
            : base()
        {
        }

        #region KeyboardLayout

        public override WhichKeyboardLayout KeyboardLayout
        {
            get { return WhichKeyboardLayout.Turkish; }
        }
        #endregion

        #region The individual key assignments



        #region 35: VK_OEM_5
        public override KeyAssignment VK_OEM_5
        {
            get
            {
                if (_VK_OEM_5 == null)
                {
                    _VK_OEM_5 = new KeyAssignment(0x002C, 0x003B, "comma", "Semicolon", true, true);
                }
                return _VK_OEM_5;
            }
        }
        #endregion

        #region 35: VK_OEM_4
        public override KeyAssignment VK_OEM_4
        {
            get
            {
                if (_VK_OEM_4 == null)
                {
                    _VK_OEM_4 = new KeyAssignment(0x011F, 0x011E, "g tilde", "G Tilde", true, false);
                }
                return _VK_OEM_4;
            }
        }
        #endregion

        #region 36: VK_OEM_6
        public override KeyAssignment VK_OEM_6
        {
            get
            {
                if (_VK_OEM_6 == null)
                {
                    _VK_OEM_6 = new KeyAssignment(0x00FC, 0x00DC, "u tılde", "U tılde", true, false);
                }
                return _VK_OEM_6;
            }
        }
        #endregion

        #region 36: VK_OEM_1
        public override KeyAssignment VK_OEM_1
        {
            get
            {
                if (_VK_OEM_1 == null)
                {
                    _VK_OEM_1 = new KeyAssignment(0x015F, 0x015E, "s", "S", true, false);
                }
                return _VK_OEM_1;
            }
        }
        #endregion


        #region 46: VK_OEM_7

        public override KeyAssignment VK_OEM_7
        {
            get
            {
                if (_VK_OEM_7 == null)
                {
                    _VK_OEM_7 = new KeyAssignment(0x0069, 0x0130, "i", "I", false, false);
                }
                return _VK_OEM_7;
            }
        }
        #endregion

        #region 46: VK_OEM_COMMA

        public override KeyAssignment VK_OEM_COMMA
        {
            get
            {
                if (_VK_OEM_COMMA == null)
                {
                    _VK_OEM_COMMA = new KeyAssignment(0x00F6, 0x00D6, "ö", "Ö", false, false);
                }
                return _VK_OEM_COMMA;
            }
        }
        #endregion
        #region 46: VK_OEM_PERIOD

        public override KeyAssignment VK_OEM_PERIOD
        {
            get
            {
                if (_VK_OEM_PERIOD == null)
                {
                    _VK_OEM_PERIOD = new KeyAssignment(0x00E7, 0x00C7, "c", "C", false, false);
                }
                return _VK_OEM_PERIOD;
            }
        }
        #endregion
        #region 46: VK_OEM_2
        public override KeyAssignment VK_OEM_2
        {
            get
            {
                if (_VK_OEM_2 == null)
                {
                    _VK_OEM_2 = new KeyAssignment(0x002E, 0x003A, ".", ":", false, true);
                }
                return _VK_OEM_2;
            }
        }
        #endregion

        #endregion The individual key assignments
    }
}
