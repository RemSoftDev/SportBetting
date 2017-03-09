using System.Collections.Generic;

namespace SportRadar.DAL.CommonObjects
{
    public class DbNullProps : List<string>
    {
        public new bool Contains(string sColumnName)
        {
            return base.Contains(sColumnName.ToLowerInvariant());
        }

        public new void Add(string sColumnName)
        {
            base.Add(sColumnName.ToLowerInvariant());
        }

        public void Add(bool bIsSpecified, string sColumnName)
        {
            if (!bIsSpecified)
            {
                base.Add(sColumnName.ToLowerInvariant());
            }
        }
    }
}