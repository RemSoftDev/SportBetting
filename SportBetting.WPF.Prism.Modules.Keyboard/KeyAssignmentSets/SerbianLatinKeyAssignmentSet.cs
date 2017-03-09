using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportBetting.WPF.Prism.Modules.Keyboard.KeyAssignmentSets
{
    public class SerbianLatinKeyAssignmentSet : KeyAssignmentSet
    {
        public SerbianLatinKeyAssignmentSet()
            : base()
        { 
        }

        #region KeyboardLayout
        /// <summary>
        /// Get the WhichKeyboardLayout this KeyAssignmentSet is used for.
        /// </summary>
        public override WhichKeyboardLayout KeyboardLayout
        {
            get { return WhichKeyboardLayout.Serbian; }
        }
        #endregion

        //http://www.microsoft.com/resources/msdn/goglobal/keyboards/kbdycl.html

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

        #region 35: VK_OEM_5
        ///// <summary>
        ///// 23  Z with caron
        ///// </summary>
        public override KeyAssignment VK_OEM_5
        {
            get
            {
                if (_VK_OEM_5 == null)
                {
                    _VK_OEM_5 = new KeyAssignment(0x017E, 0x017D, "small letter Z with caron", "Capital letter Z with caron", true, false);
                }
                return _VK_OEM_5;
            }
        }
        #endregion

        #region 23: VK_OEM_4
        /// <summary>
        /// 23  S with caron
        /// </summary>
        public override KeyAssignment VK_OEM_4
        {
            get
            {
                if (_VK_OEM_4 == null)
                {
                    _VK_OEM_4 = new KeyAssignment(0x0161, 0x0160, "small letter S with caron", "Capital letter S with caron", true, false);
                }
                return _VK_OEM_4;
            }
        }
        #endregion

        #region 36: VK_OEM_6
        /// <summary>
        /// 36  Letter C with acute
        /// </summary>
        public override KeyAssignment VK_OEM_6
        {
            get
            {
                if (_VK_OEM_6 == null)
                {
                    _VK_OEM_6 = new KeyAssignment(0x0111, 0x0110, "Small letter D with stroke", "Capital letter D with stroke", true, false);
                }
                return _VK_OEM_6;
            }
        }
        #endregion

        #region 46: VK_OEM_7
        /// <summary>
        /// 36  Letter C with acute
        /// </summary>
        public override KeyAssignment VK_OEM_7
        {
            get
            {
                if (_VK_OEM_7 == null)
                {
                    _VK_OEM_7 = new KeyAssignment(0x0107, 0x0106, "Small letter C with acute", "Capital letter C with acute", true, false);
                }
                return _VK_OEM_7;
            }
        }
        #endregion

        #region 36: VK_OEM_1
        /// <summary>
        /// 35  letter C with caron
        /// </summary>
        public override KeyAssignment VK_OEM_1
        {
            get
            {
                if (_VK_OEM_1 == null)
                {
                    _VK_OEM_1 = new KeyAssignment(0x010D, 0x010C, "latin small letter C with caron", "latin capital letter C with caron", true, false);
                }
                return _VK_OEM_1;
            }
        }
        #endregion

        #region 11: VK_OEM_PLUS
        /// <summary>
        /// 11  Replace the minus/underscore with Eszett (also called scharfes S)
        ///     German keyboard has Question-mark in the upper location. I'm deciding not to.
        /// </summary>
        public override KeyAssignment VK_OEM_PLUS
        {
            // See http://en.wikipedia.org/wiki/B <- that last char is the eszett.
            get
            {
                if (_VK_OEM_PLUS == null)
                {
                    _VK_OEM_PLUS = new KeyAssignment(0x00DF, 0x1E9E, "Eszett", "capital Eszett", true, false);
                }
                return _VK_OEM_PLUS;
            }
        }
        #endregion

        //shift coma and period to the right and add L with stroke
        #region 46: VK_OEM_COMMA

        public override KeyAssignment VK_OEM_COMMA
        {
            get
            {
                if (_VK_OEM_COMMA == null)
                {
                    _VK_OEM_COMMA = new KeyAssignment(0x0142, 0x0141, "Small letter L with stroke", "Capital letter L with stroke", true, false);
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
                    _VK_OEM_PERIOD = new KeyAssignment(0x002C, 0x003B, ",", ";", false, true);
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
    }
}
