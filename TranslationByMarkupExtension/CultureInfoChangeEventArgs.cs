using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslationByMarkupExtension
{
    public class CultureInfoChangeEventArgs : EventArgs
    {
        private readonly CultureInfo _CultureInfo;

        public CultureInfoChangeEventArgs(CultureInfo ci)
        {
            this._CultureInfo = ci;
        }

        public CultureInfo CultureInfo { get { return this._CultureInfo; } }
    }
}
