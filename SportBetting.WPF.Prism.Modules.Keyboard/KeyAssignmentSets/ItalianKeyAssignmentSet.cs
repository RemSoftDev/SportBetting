namespace SportBetting.WPF.Prism.Modules.Keyboard.KeyAssignmentSets
{
    /// <summary>
    /// The GermanKeyAssignments subclass of DefaultKeyAssignments overrides those KeyAssignments that need to differ
    /// from those of the English keyboard.
    /// </summary>
    public class ItalianKeyAssignmentSet : KeyAssignmentSet
    {
        public ItalianKeyAssignmentSet()
            : base()
        {
        }

        #region KeyboardLayout
        /// <summary>
        /// Get the WhichKeyboardLayout this KeyAssignmentSet is used for.
        /// </summary>
        public override WhichKeyboardLayout KeyboardLayout
        {
            get { return WhichKeyboardLayout.Italian; }
        }
        #endregion



        public override KeyAssignment VK_OEM_4
        {
            get
            {
                if (_VK_OEM_4 == null)
                {
                    _VK_OEM_4 = new KeyAssignment(0x00E9, 0x00C9, "", "", true, false);
                }
                return _VK_OEM_4;
            }
        }

        public override KeyAssignment VK_OEM_6
        {
            get
            {
                if (_VK_OEM_6 == null)
                {
                    _VK_OEM_6 = new KeyAssignment(0x00F9, 0x00D9, "", "", true, false);
                }
                return _VK_OEM_6;
            }
        }

        public override KeyAssignment VK_OEM_1
        {
            get
            {
                if (_VK_OEM_1 == null)
                {
                    _VK_OEM_1 = new KeyAssignment(0x00F2, 0x00D2, "", "", true, false);
                }
                return _VK_OEM_1;
            }
        }

        public override KeyAssignment VK_OEM_7
        {
            get
            {
                if (_VK_OEM_7 == null)
                {
                    _VK_OEM_7 = new KeyAssignment(0x00E0, 0x00C0, "", "", true, false);
                }
                return _VK_OEM_7;
            }
        }

        public override KeyAssignment VK_OEM_MINUS
        {
            get
            {
                if (_VK_OEM_MINUS == null)
                {
                    _VK_OEM_MINUS = new KeyAssignment(0x00EC, 0x00CC, "", "", true, false);
                }
                return _VK_OEM_MINUS;
            }
        }

        public override KeyAssignment VK_OEM_PLUS
        {
            get
            {
                if (_VK_OEM_PLUS == null)
                {
                    _VK_OEM_PLUS = new KeyAssignment(0x00E7, 0x00C7, "", "", true, false);
                }
                return _VK_OEM_PLUS;
            }
        }



    }
}
