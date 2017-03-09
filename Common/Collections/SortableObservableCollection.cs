using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace SportRadar.Common.Collections
{
    public class SortableObservableCollection<T> : ObservableCollection<T>
    {
        public SortableObservableCollection()
            : base()
        {
        }

        public SortableObservableCollection(IList<T> list)
            : base(list)
        {
        }

        public SortableObservableCollection(IEnumerable<T> collection)
            : base(collection)
        {
        }

        protected override void InsertItem(int index, T item)
        {
            {
                base.InsertItem(index, item);
            }
        }

        protected override void RemoveItem(int index)
        {
            {
                base.RemoveItem(index);
            }
        }

        public void Sort<TKey>(Func<T, TKey> keySelector, System.ComponentModel.ListSortDirection direction)
        {
            {
                switch (direction)
                {
                    case System.ComponentModel.ListSortDirection.Ascending:
                        {
                            ApplySort(Items.OrderBy(keySelector));
                            break;
                        }
                    case System.ComponentModel.ListSortDirection.Descending:
                        {
                            ApplySort(Items.OrderByDescending(keySelector));
                            break;
                        }
                }
            }
        }


        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        public SortableObservableCollection(List<T> collection) : base(collection) { }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var eventHandler = CollectionChanged;
            if (eventHandler != null)
            {
                eventHandler(this, e);
            }
        }


        public void Sort<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer)
        {
            {
                ApplySort(Items.OrderBy(keySelector, comparer));
            }
        }

        public void Sort(Comparison<T> comparison)
        {
            {
                List<T> lItems = this.ToList();
                lItems.Sort(comparison);

                ApplySort(lItems);
            }
        }

        public SyncList<T> ToSyncList()
        {
            {
                var list = new SyncList<T>();
                foreach (var item in this)
                {
                    list.Add(item);
                }
                return list;
            }
        }

        private void ApplySort(IEnumerable<T> sortedItems)
        {
            this.ApplySort(sortedItems.ToList());
        }

        public void ApplyChanges(IList<T> lItems)
        {
            {
                for (int i = 0; i < this.Count; )
                {
                    T obj = this[i];

                    if (!lItems.Contains(obj))
                    {
                        this.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }

                this.ApplySort(lItems);
            }
        }

        private void ApplySort(List<T> lItems)
        {
            {
                for (int i = 0; i < lItems.Count; i++)
                {
                    T objItem = lItems[i];
                    int iCurrentIndex = this.IndexOf(objItem);

                    if (iCurrentIndex < 0)
                    {
                        this.Insert(i, objItem);
                    }
                    else if (i != iCurrentIndex)
                    {
                        this.Move(iCurrentIndex, i);
                    }
                }
            }
        }
    }
}