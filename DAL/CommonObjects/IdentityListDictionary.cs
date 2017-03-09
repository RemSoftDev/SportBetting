using System;
using System.Collections.Generic;

namespace SportRadar.DAL.CommonObjects
{
    public class IdentityListDictionary : Dictionary<Type, IdentityList>
    {
        [Obsolete]
        public new void Clear()
        {

        }

        public void ClearAll()
        {
            foreach (IdentityList il in this.Values)
            {
                il.Clear();
            }

            base.Clear();
        }

        public IdentityList EnsureIdentityList<T>() where T : ObjectBase
        {
            Type type = typeof(T);

            if (!base.ContainsKey(type))
            {
                base.Add(type, new IdentityList());
            }

            return this[type] ?? new IdentityList();
        }

        //        public IdentityList EnsureAdd<T>(long id) where T : SqlBulkCopyObjectBase
        //        {
        //            EnsureIdentityList<T>().AddUnique(id);
        //            return EnsureIdentityList<T>();
        //        }
    }
}