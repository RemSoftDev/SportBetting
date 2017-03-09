using System;
using System.Windows.Controls;
using System.Windows.Media;
using JHLib;
using SportBetting.WPF.Prism.Modules.Keyboard.ViewModels;
// FontFamily


namespace SportBetting.WPF.Prism.Modules.Keyboard
{
    /// <summary>
    /// A KeyViewModel is the view-model that underlies each key-button on the keyboard.
    /// </summary>
    public class KeyViewModel : BaseViewModel
    {
        #region Constructors

        public KeyViewModel()
        {
            this.UnshiftedCodePoint = '?';
        }

        public KeyViewModel(char initialCodePoint)
        {
            this.UnshiftedCodePoint = initialCodePoint;
        }

        public KeyViewModel(KeyAssignment ka)
        {
            this.IsLetter = ka.IsLetter;
            if (String.IsNullOrEmpty(ka.FontFamilyName))
            {
                this.FontFamily = null;
            }
            else
            {
                // Those assigned to this specific key-assignment, override those assigned to the key-assignment-set.
                this.FontFamily = new FontFamily(ka.FontFamilyName);
            }
            this.UnshiftedCodePoint = (char)ka.UnshiftedCodePoint;
            this.ShiftedCodePoint = (char)ka.ShiftedCodePoint;
        }
        #endregion



        #region Text
        /// <summary>
        /// Get/set the Text to be shown on the corresponding Button
        /// </summary>
        public string Text
        {
            get
            {
                string sText;
                if (!String.IsNullOrEmpty(_text))
                {
                    sText = _text;
                }
                else
                {
                    sText = this.CodePoint.ToString();
                }
                return sText;
            }
            set
            {
                if (value != _text)
                {
                    _text = value;
                    Notify("Text");
                }
            }
        }
        #endregion

        #region IsLetter
        /// <summary>
        /// Get/set the flag that indicates whether this key-button contains a 'letter',
        /// which for our purposes means the CAPS-LOCK makes it into a capital letter.
        /// </summary>
        public bool IsLetter
        {
            get { return _isLetter; }
            set { _isLetter = value; }
        }
        #endregion

        public FontFamily FontFamily
        {
            // Note: I'm still unsure whether this would ever be merited.
            get { return _fontFamily; }
            set
            {
                if (value != _fontFamily)
                {
                    _fontFamily = value;
                    Notify("FontFamily");
                }
            }
        }

        public char UnshiftedCodePoint
        {
            get { return _unshiftedCodePoint; }
            set
            {
                if (value != _unshiftedCodePoint)
                {
                    _unshiftedCodePoint = value;
                    Notify("Text");
                }
            }
        }

        public char ShiftedCodePoint
        {
            get { return _shiftedCodePoint; }
            set
            {
                if (value != _shiftedCodePoint)
                {
                    _shiftedCodePoint = value;
                    Notify("Text");
                }
            }
        }

        public char CodePoint
        {
            get
            {
                if ((s_domain.IsShiftLock || (_isLetter && s_domain.IsCapsLock)) && _shiftedCodePoint != 0)
                {
                    return ShiftedCodePoint;
                }
                else
                {
                    return UnshiftedCodePoint;
                }
            }
        }

        
        #region fields

        protected string _text;
        protected char _shiftedCodePoint;
        protected char _unshiftedCodePoint;
        public KeyboardViewModel s_domain;
        protected Button _button;
        protected FontFamily _fontFamily;
        protected bool _isLetter;

        #endregion fields
    }
}
