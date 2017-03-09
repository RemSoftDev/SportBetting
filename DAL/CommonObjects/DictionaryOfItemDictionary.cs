using System;
using System.Collections.Generic;

namespace SportRadar.DAL.CommonObjects
{
    public class DictionaryOfItemDictionary : Dictionary<Type, ItemDictionary>
    {
        [Obsolete]
        public new void Clear()
        {

        }

        public void ClearAll()
        {
            foreach (ItemDictionary di in this.Values)
            {
                di.Clear();
            }

            base.Clear();
        }

        public ItemDictionary EnsureDictionary<T>() where T : ObjectBase
        {
            Type type = typeof(T);

            if (!base.ContainsKey(type))
            {
                base.Add(type, new ItemDictionary());
            }

            return this[type];
        }

        public bool ContainsKey<T>(long lKey) where T : ObjectBase
        {
            ItemDictionary di = EnsureDictionary<T>();

            return di.ContainsKey(lKey);
        }

        public T GetObject<T>(long lKey) where T : ObjectBase
        {
            ItemDictionary di = EnsureDictionary<T>();

            if (di.ContainsKey(lKey))
            {
                return di[lKey] as T;
            }

            return default(T);
        }

        public void Add<T>(T tkObject) where T : ObjectBase
        {
            EnsureDictionary<T>().AddOrUpdate(tkObject);
        }

        public void AddOrUpdate<T>(long lKey, T tkObject) where T : ObjectBase
        {
            EnsureDictionary<T>().AddOrUpdate(lKey, tkObject);
        }

        public void Remove<T>(long lKey)
        {
            Type type = typeof(T);
            this[type].Remove(lKey);
        }

        public int GetCount<T>() where T : ObjectBase
        {
            ItemDictionary di = EnsureDictionary<T>();

            return di.Count;
        }
    }
}