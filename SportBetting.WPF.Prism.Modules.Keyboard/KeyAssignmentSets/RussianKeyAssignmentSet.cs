namespace SportBetting.WPF.Prism.Modules.Keyboard.KeyAssignmentSets
{
    /// <summary>
    /// The RussianKeyAssignments subclass of KeyAssignmentSet overrides those KeyAssignments that need to differ
    /// from those of the English keyboard.
    /// </summary>
    public class RussianKeyAssignmentSet : KeyAssignmentSet
    {
        public RussianKeyAssignmentSet()
            : base()
        {
        }

        #region KeyboardLayout
        /// <summary>
        /// Get the WhichKeyboardLayout this KeyAssignmentSet is used for.
        /// </summary>
        public override WhichKeyboardLayout KeyboardLayout
        {
            get { return WhichKeyboardLayout.Russian; }
        }
        #endregion

        #region FontFamilyName
        /// <summary>
        /// Get the name of the FontFamily that we want to use to display the keycaps of the keyboard in, when using this Russian keyboard.
        /// </summary>
        public override string FontFamilyName
        {
            get
            {
                return "Bitstream Cyberbase, Roman";
            }
        }
        #endregion

        #region The individual key assignments

        // This one called for a total override from the English defaults.

        #region 0: VK_OEM_3
        /// <summary>
        /// 0  Cyrillic letter IO
        /// </summary>
        public override KeyAssignment VK_OEM_3
        {
            get
            {
                if (_VK_OEM_3 == null)
                {
                    _VK_OEM_3 = new KeyAssignment(0x0451, 0x0401, "Cyrillic lowercase letter IO", "Cyrillic capital letter IO", true);
                }
                return _VK_OEM_3;
            }
        }
        #endregion

        #region 13:  VK_Q
        /// <summary>
        /// 13  Cyrillic letter SHORT I
        /// </summary>
        public override KeyAssignment VK_Q
        {
            get
            {
                if (_VK_Q == null)
                {
                    _VK_Q = new KeyAssignment(0x0439, 0x0419, "Cyrillic small letter SHORT I", "Cyrillic capital letter SHORT I", true);
                }
                return _VK_Q;
            }
        }
        #endregion

        #region 14: VK_W
        /// <summary>
        /// 14  Cyrillic letter TSE
        /// </summary>
        public override KeyAssignment VK_W
        {
            get
            {
                if (_VK_W == null)
                {
                    _VK_W = new KeyAssignment(0x0446, 0x0426, "Cyrillic small letter TSE", "Cyrillic capital letter TSE", true);
                }
                return _VK_W;
            }
        }
        #endregion

        #region 15: VK_E
        /// <summary>
        /// 15  Cyrillic letter U
        /// </summary>
        public override KeyAssignment VK_E
        {
            get
            {
                if (_VK_E == null)
                {
                    _VK_E = new KeyAssignment(0x0443, 0x0423, "Cyrillic small letter U", "Cyrillic capital letter U", true);
                }
                return _VK_E;
            }
        }
        #endregion

        #region 16: VK_R
        /// <summary>
        /// 16  Cyrillic letter KA
        /// </summary>
        public override KeyAssignment VK_R
        {
            get
            {
                if (_VK_R == null)
                {
                    _VK_R = new KeyAssignment(0x043A, 0x041A, "Cyrillic small letter KA", "Cyrillic capital letter KA", true);
                }
                return _VK_R;
            }
        }
        #endregion

        #region 17: VK_T
        /// <summary>
        /// 17  Cyrillic letter IE
        /// </summary>
        public override KeyAssignment VK_T
        {
            get
            {
                if (_VK_T == null)
                {
                    _VK_T = new KeyAssignment(0x0435, 0x0415, "Cyrillic small letter IE", "Cyrillic capital letter IE", true);
                }
                return _VK_T;
            }
        }
        #endregion

        #region 18: VK_Y
        /// <summary>
        /// 18  Cyrillic letter EN
        /// </summary>
        public override KeyAssignment VK_Y
        {
            get
            {
                if (_VK_Y == null)
                {
                    _VK_Y = new KeyAssignment(0x043D, 0x041D, "Cyrillic small letter EN", "Cyrillic capital letter EN", true);
                }
                return _VK_Y;
            }
        }
        #endregion

        #region 19:  VK_U
        /// <summary>
        /// 19  Cyrillic letter GHE
        /// </summary>
        public override KeyAssignment VK_U
        {
            get
            {
                if (_VK_U == null)
                {
                    _VK_U = new KeyAssignment(0x0433, 0x0413, "Cyrillic small letter GHE", "Cyrillic capital letter GHE", true);
                }
                return _VK_U;
            }
        }
        #endregion

        #region 20: VK_I
        /// <summary>
        /// 20  Cyrillic letter SHA
        /// </summary>
        public override KeyAssignment VK_I
        {
            get
            {
                if (_VK_I == null)
                {
                    _VK_I = new KeyAssignment(0x0448, 0x0428, "Cyrillic small letter SHA", "Cyrillic capital letter SHA", true);
                }
                return _VK_I;
            }
        }
        #endregion

        #region 21: VK_O
        /// <summary>
        /// 21  Cyrillic letter SHCHA
        /// </summary>
        public override KeyAssignment VK_O
        {
            get
            {
                if (_VK_O == null)
                {
                    _VK_O = new KeyAssignment(0x0449, 0x0429, "Cyrillic small letter SHCHA", "Cyrillic capital letter SHCHA", true);
                }
                return _VK_O;
            }
        }
        #endregion

        #region 22: VK_P
        /// <summary>
        /// 22  Cyrillic letter ZE
        /// </summary>
        public override KeyAssignment VK_P
        {
            get
            {
                if (_VK_P == null)
                {
                    _VK_P = new KeyAssignment(0x0437, 0x0417, "Cyrillic small letter ZE", "Cyrillic capital letter ZE", true);
                }
                return _VK_P;
            }
        }
        #endregion

        #region 23: VK_OEM_4
        /// <summary>
        /// 23  Cyrillic letter HA
        /// </summary>
        public override KeyAssignment VK_OEM_4
        {
            get
            {
                if (_VK_OEM_4 == null)
                {
                    _VK_OEM_4 = new KeyAssignment(0x0445, 0x0425, "Cyrillic small letter HA", "Cyrillic capital letter HA", true);
                }
                return _VK_OEM_4;
            }
        }
        #endregion

        #region 24: VK_OEM_6
        /// <summary>
        /// 24  Cyrillic HARD SIGN
        /// </summary>
        public override KeyAssignment VK_OEM_6
        {
            get
            {
                if (_VK_OEM_6 == null)
                {
                    _VK_OEM_6 = new KeyAssignment(0x044A, 0x042A, "Cyrillic small HARD SIGN", "Cyrillic uppercase HARD SIGN", true);
                }
                return _VK_OEM_6;
            }
        }
        #endregion

        #region 26: VK_A
        /// <summary>
        /// 26  Cyrillic letter EF
        /// </summary>
        public override KeyAssignment VK_A
        {
            get
            {
                if (_VK_A == null)
                {
                    _VK_A = new KeyAssignment(0x0444, 0x0424, "Cyrillic lowercase letter EF", "Cyrillic capital letter EF", true);
                }
                return _VK_A;
            }
        }
        #endregion

        #region 27: VK_S
        /// <summary>
        /// 27  Cyrillic letter YERU
        /// </summary>
        public override KeyAssignment VK_S
        {
            get
            {
                if (_VK_S == null)
                {
                    _VK_S = new KeyAssignment(0x044B, 0x042B, "Cyrillic lowercase letter YERU", "Cyrillic capital letter YERU", true);
                }
                return _VK_S;
            }
        }
        #endregion

        #region 28: VK_D
        /// <summary>
        /// 28  Cyrillic letter VE
        /// </summary>
        public override KeyAssignment VK_D
        {
            get
            {
                if (_VK_D == null)
                {
                    _VK_D = new KeyAssignment(0x0432, 0x0412, "Cyrillic lowercase letter VE", "Cyrillic capital letter VE", true);
                }
                return _VK_D;
            }
        }
        #endregion

        #region 29: VK_F
        /// <summary>
        /// 29  Cyrillic letter A
        /// </summary>
        public override KeyAssignment VK_F
        {
            get
            {
                if (_VK_F == null)
                {
                    _VK_F = new KeyAssignment(0x0430, 0x0410, "Cyrillic lowercase letter A", "Cyrillic capital letter A", true);
                }
                return _VK_F;
            }
        }
        #endregion

        #region 30: VK_G
        /// <summary>
        /// 30  Cyrillic letter PE
        /// </summary>
        public override KeyAssignment VK_G
        {
            get
            {
                if (_VK_G == null)
                {
                    _VK_G = new KeyAssignment(0x043F, 0x041F, "Cyrillic lowercase letter PE", "Cyrillic capital letter PE", true);
                }
                return _VK_G;
            }
        }
        #endregion

        #region 31: VK_H
        /// <summary>
        /// 31  Cyrillic letter ER
        /// </summary>
        public override KeyAssignment VK_H
        {
            get
            {
                if (_VK_H == null)
                {
                    _VK_H = new KeyAssignment(0x0440, 0x0420, "Cyrillic lowercase letter ER", "Cyrillic capital letter ER", true);
                }
                return _VK_H;
            }
        }
        #endregion

        #region 32: VK_J
        /// <summary>
        /// 32  Cyrillic letter O
        /// </summary>
        public override KeyAssignment VK_J
        {
            get
            {
                if (_VK_J == null)
                {
                    _VK_J = new KeyAssignment(0x043E, 0x041E, "Cyrillic lowercase letter O", "Cyrillic capital letter O", true);
                }
                return _VK_J;
            }
        }
        #endregion

        #region 33: VK_K
        /// <summary>
        /// 33  Cyrillic letter EL
        /// </summary>
        public override KeyAssignment VK_K
        {
            get
            {
                if (_VK_K == null)
                {
                    _VK_K = new KeyAssignment(0x043B, 0x041B, "Cyrillic lowercase letter EL", "Cyrillic capital letter EL", true);
                }
                return _VK_K;
            }
        }
        #endregion

        #region 34: VK_L
        /// <summary>
        /// 34  Cyrillic letter DE
        /// </summary>
        public override KeyAssignment VK_L
        {
            get
            {
                if (_VK_L == null)
                {
                    _VK_L = new KeyAssignment(0x0434, 0x0414, "Cyrillic lowercase letter DE", "Cyrillic capital letter DE", true, false);
                }
                return _VK_L;
            }
        }
        #endregion

        #region 35: VK_OEM_1
        /// <summary>
        /// 35  Cyrillic letter ZHE
        /// </summary>
        public override KeyAssignment VK_OEM_1
        {
            get
            {
                if (_VK_OEM_1 == null)
                {
                    _VK_OEM_1 = new KeyAssignment(0x0436, 0x0416, "Cyrillic lowercase letter ZHE", "Cyrillic capital letter ZHE", true, false);
                }
                return _VK_OEM_1;
            }
        }
        #endregion

        #region 36: VK_OEM_7
        /// <summary>
        /// 36  Cyrillic letter E
        /// </summary>
        public override KeyAssignment VK_OEM_7
        {
            get
            {
                if (_VK_OEM_7 == null)
                {
                    _VK_OEM_7 = new KeyAssignment(0x044D, 0x042D, "Cyrillic lowercase letter E", "Cyrillic capital letter E", true);
                }
                return _VK_OEM_7;
            }
        }
        #endregion

        #region 37: VK_Z
        /// <summary>
        /// 37  Cyrillic letter YA
        /// </summary>
        public override KeyAssignment VK_Z
        {
            get
            {
                if (_VK_Z == null)
                {
                    _VK_Z = new KeyAssignment(0x044F, 0x042F, "Cyrillic small letter YA", "Cyrillic capital letter YA", true);
                }
                return _VK_Z;
            }
        }
        #endregion

        #region 38: VK_X
        /// <summary>
        /// 38  Cyrillic letter CHE
        /// </summary>
        public override KeyAssignment VK_X
        {
            get
            {
                if (_VK_X == null)
                {
                    _VK_X = new KeyAssignment(0x0447, 0x0427, "Cyrillic small letter CHE", "Cyrillic capital letter CHE", true);
                }
                return _VK_X;
            }
        }
        #endregion

        #region 39: VK_C
        /// <summary>
        /// 39  Cyrillic letter ES
        /// </summary>
        public override KeyAssignment VK_C
        {
            get
            {
                if (_VK_C == null)
                {
                    _VK_C = new KeyAssignment(0x0441, 0x0421, "Cyrillic small letter ES", "Cyrillic capital letter ES", true);
                }
                return _VK_C;
            }
        }
        #endregion

        #region 40: VK_V
        /// <summary>
        /// 40  Cyrillic letter EM
        /// </summary>
        public override KeyAssignment VK_V
        {
            get
            {
                if (_VK_V == null)
                {
                    _VK_V = new KeyAssignment(0x043C, 0x041C, "Cyrillic small letter EM", "Cyrillic capital letter EM", true);
                }
                return _VK_V;
            }
        }
        #endregion

        #region 41: VK_B
        /// <summary>
        /// 41  Cyrillic letter I
        /// </summary>
        public override KeyAssignment VK_B
        {
            get
            {
                if (_VK_B == null)
                {
                    _VK_B = new KeyAssignment(0x0438, 0x0418, "Cyrillic small letter I", "Cyrillic capital letter I", true);
                }
                return _VK_B;
            }
        }
        #endregion

        #region 42: VK_N
        /// <summary>
        /// 42  Cyrillic letter TE
        /// </summary>
        public override KeyAssignment VK_N
        {
            get
            {
                if (_VK_N == null)
                {
                    _VK_N = new KeyAssignment(0x0442, 0x0422, "Cyrillic small letter TE", "Cyrillic capital letter TE", true);
                }
                return _VK_N;
            }
        }
        #endregion

        #region 43: VK_M
        /// <summary>
        /// 43  Cyrillic SOFT SIGN
        /// </summary>
        public override KeyAssignment VK_M
        {
            get
            {
                if (_VK_M == null)
                {
                    _VK_M = new KeyAssignment(0x044C, 0x042C, "Cyrillic lowercase SOFT SIGN", "Cyrillic uppercase SOFT SIGN", true);
                }
                return _VK_M;
            }
        }
        #endregion

        #region 44: VK_OEM_COMMA
        /// <summary>
        /// 44  Cyrillic BE
        /// </summary>
        public override KeyAssignment VK_OEM_COMMA
        {
            get
            {
                if (_VK_OEM_COMMA == null)
                {
                    _VK_OEM_COMMA = new KeyAssignment(0x0431, 0x0411, "Cyrillic lowercase BE", "Cyrillic uppercase BE", true);
                }
                return _VK_OEM_COMMA;
            }
        }
        #endregion

        #region 45: VK_OEM_PERIOD
        /// <summary>
        /// 45  Cyrillic YU
        /// </summary>
        public override KeyAssignment VK_OEM_PERIOD
        {
            get
            {
                if (_VK_OEM_PERIOD == null)
                {
                    _VK_OEM_PERIOD = new KeyAssignment(0x044E, 0x042E, "Cyrillic lowercase YU", "Cyrillic uppercase YU", true);
                }
                return _VK_OEM_PERIOD;
            }
        }
        #endregion

        #region 46: VK_OEM_2
        /// <summary>
        /// 46  Period
        /// </summary>
        public override KeyAssignment VK_OEM_2
        {
            get
            {
                if (_VK_OEM_2 == null)
                {
                    _VK_OEM_2 = new KeyAssignment(0x002E, "Period", false);
                }
                return _VK_OEM_2;
            }
        }
        #endregion

        #endregion The individual key assignments
    }
}
