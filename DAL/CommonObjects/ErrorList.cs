using System.Collections.Generic;

namespace SportRadar.DAL.CommonObjects
{
    public class ErrorList : List<string>
    {
        public void AddUnique(string sValue)
        {
            if (!this.Contains(sValue))
            {
                this.Add(sValue);
            }
        }

        public void AddFormat(string sFormat, params object[] args)
        {
            this.Add(string.Format(sFormat, args));
        }

        public override string ToString()
        {
            return string.Format("ErrorList ({0})", this.ToFormatString(", "));
        }

        public string ToFormatString(string sSeparator)
        {
            return string.Join(sSeparator, this.ToArray());
        }

        public string ToFormatStringWithLimit(string sSeparator, int iLimit)
        {
            if (iLimit < this.Count)
            {
                string[] arr = new string[iLimit];
                this.CopyTo(0, arr, 0, iLimit);

                return string.Format("(Count: {0}) {1}, More...", this.Count, string.Join(sSeparator, arr));
            }

            return string.Join(sSeparator, this.ToArray());
        }
    }
}