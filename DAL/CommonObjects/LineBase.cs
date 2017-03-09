using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.DAL.NewLineObjects;

namespace SportRadar.DAL.CommonObjects
{
    public abstract class LineBase
    {
        protected IdentityListDictionary m_diIds = new IdentityListDictionary();

        protected IdentityListDictionary m_diSvrIds = new IdentityListDictionary();

        public abstract string GetString(long? lMultiStringId, string sDefaultValue);

        public virtual  T GetObject<T>(long? lIdentity) where T : ObjectBase
        {
            return default(T);
        }

        public virtual T GetLineObject<T>(long? lIdentity) where T : ILineObjectWithId<T>
        {
            return default(T);
        }

        public virtual T GetLineObject<T>(string sKey) where T : ILineObjectWithKey<T>
        {
            return default(T);
        }

        public IdentityList GetIdentityList<T>() where T : ObjectBase
        {
            return m_diIds.EnsureIdentityList<T>();
        }
    }
}
