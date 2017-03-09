using System.Collections.Generic;

namespace SportRadar.DAL.CommonObjects
{
    public sealed class ItemDictionary : Dictionary<long, ObjectBase>
    {
        public void AddOrUpdate(ObjectBase obj)
        {
            if (this.ContainsKey(obj.ORMID)) this[obj.ORMID] = obj;

            else base.Add(obj.ORMID, obj);
        }

        public void AddOrUpdate(long lKey, ObjectBase obj)
        {
            if (this.ContainsKey(lKey)) this[lKey] = obj;

            else base.Add(lKey, obj);
        }

        public List<T> ToList<T>() where T : ObjectBase
        {
            List<T> list = new List<T>();

            foreach (T obj in this.Values)
            {
                list.Add(obj);
            }

            return list;
        }

        public T[] ToArray<T>() where T : ObjectBase
        {
            return this.ToList<T>().ToArray();
        }
    }
}