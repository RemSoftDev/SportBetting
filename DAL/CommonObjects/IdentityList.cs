using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using SportRadar.Common.Collections;

namespace SportRadar.DAL.CommonObjects
{
    public class IdentityList : SyncHashSet<long>
    {
        private static readonly SportRadar.Common.Logs.ILog m_logger = SportRadar.Common.Logs.LogFactory.CreateLog(typeof(IdentityList));

        public IdentityList() : base ()
        {
        }

        public IdentityList(IEnumerable<long> collection) : base (collection)
        {
        }

        public IdentityList(DataTable dt, string sColumnName) : base ()
        {
            this.AddFromDataTable(dt, sColumnName.ToLowerInvariant());
        }

        public void AddFromDataTable(DataTable dt, string sColumnName)
        {
            if (dt == null)
            {
                m_logger.WarnFormat("AddFromDataTable() ERROR - DataTable is NULL:\r\n{0}", new StackTrace());
                return;
            }

            foreach (DataRow dr in dt.Rows)
            {
                object obj = dr[sColumnName.ToLowerInvariant()];

                if (obj != DBNull.Value)
                {
                    long lIdentity = Convert.ToInt64(obj);
                    base.AddUnique(lIdentity);
                }
            }
        }

        public void Limit(int iLimit)
        {
            lock (m_oLocker)
            {
                if (this.Count > iLimit)
                {
                    while (this.Count > iLimit)
                    {
                        this.Remove(this.Count);
                    }
                }
            }
        }

        [Obsolete]
        public override void Add(long lIdentity)
        {
            throw new Exception("Method IdentityList.Add() is obsolete. Instead use method AddUnique(long?).");
        }

        public void AddUnique(long? lIdentity)
        {
            if (lIdentity != null && lIdentity != 0)
            {
                base.AddUnique((long)lIdentity);
            }
        }

        public string FormatIds()
        {
            if (this.Count > 0)
            {
                lock (m_oLocker)
                {
                    string[] arr = System.Array.ConvertAll(this.ToArray(), element => element.ToString("G"));
                    return string.Join(", ", arr);
                }
            }

            return "0"; // Default value. Means no records will be returned by query.
        }
    }
}
