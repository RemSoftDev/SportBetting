using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SportRadar.Common.Collections;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;

namespace SportRadar.DAL.ViewObjects
{
    public class ViewObjectBase : INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected virtual SyncDictionary<string, List<string>> LinePropsToViewProps
        {
            get;
            set;
        }

        public bool Changed { get; protected set; }
        protected SyncList<string> m_lChangedPropNames = new SyncList<string>();


        public virtual void RaisePropertiesChanged()
        {
            throw new NotImplementedException("Do not call ViewObjectBase.RaisePropertiesChanged() directly.");
        }

        public virtual void UnsetChanged()
        {
            throw new NotImplementedException("Do not call ViewObjectBase.UnsetChanged() directly.");
        }

        public static System.Windows.Threading.Dispatcher Dispatcher { get; set; }

        private delegate void DelegateSafelyApplyChanges<T>(SyncList<T> lSource, SyncObservableCollection<T> collTarget);

        public static void SafelyApplyChanges<T>(SyncList<T> lSource, SyncObservableCollection<T> collTarget)
        {
            ExcpHelper.ThrowIf<InvalidOperationException>(ViewObjectBase.Dispatcher == null, "SafelyApplyChanges<{0}> ERROR", typeof(T).Name);

            if (!ViewObjectBase.Dispatcher.CheckAccess())
            {
                ViewObjectBase.Dispatcher.Invoke(new DelegateSafelyApplyChanges<T>(SafelyApplyChanges), lSource, collTarget);
            }
            else
            {
                collTarget.ApplyChanges(lSource);
            }
        }

        public void DoPropertyChanged(string sPropertyName)
        {
            try
            {
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(sPropertyName));
                }
            }
            catch
            {

            }
        }

        protected virtual void OnRaisePropertiesChanged<T>(T objLine) where T : ILineObject<T>
        {
            // To override in derived classes if necessary
        }

        protected virtual void OnPropertyChanged(ObservablePropertyBase opb)
        {
            AddToChangedPropNames(opb.PropertyName);
            DoPropertyChanged(opb.PropertyName);

            Debug.Assert(this.LinePropsToViewProps != null);
            List<string> lPropertyNames = this.LinePropsToViewProps.ContainsKey(opb.PropertyName) ? this.LinePropsToViewProps[opb.PropertyName] : new List<string>() { opb.PropertyName };

            foreach (string sPropertyName in lPropertyNames)
            {
                AddToChangedPropNames(sPropertyName);

                DoPropertyChanged(sPropertyName);
            }
        }

        protected virtual void AddToChangedPropNames(string sPropertyName)
        {
            if (!m_lChangedPropNames.Contains(sPropertyName))
            {
                m_lChangedPropNames.Add(sPropertyName);
            }
        }

        public virtual void RaisePropertiesChanged<T>(T objLine) where T : ILineObject<T>
        {
            Debug.Assert(objLine != null);
            Debug.Assert(objLine.ChangedProps != null);

            if (objLine.ChangedProps.Count > 0)
            {
                foreach (ObservablePropertyBase opb in objLine.ChangedProps)
                {
                    OnPropertyChanged(opb);
                }

                OnRaisePropertiesChanged<T>(objLine);

                if (!this.Changed)
                {
                    this.Changed = true;
                    DoPropertyChanged("Changed");
                    DoPropertyChanged("ChangedPropNames");
                }
            }
        }
    }

    public abstract class ViewObjectBase<T> : ViewObjectBase where T : ILineObject<T>
    {
        protected T m_objLine = default(T);

        public T LineObject { get { return m_objLine; } }

        public virtual string ChangedPropNames
        {
            get { return string.Join(", ", m_lChangedPropNames.ToArray()); }
        }

        protected ViewObjectBase(T objLine)
        {
            this.Changed = false;
            Debug.Assert(objLine != null);
            m_objLine = objLine;
        }

        public override void RaisePropertiesChanged()
        {
            this.RaisePropertiesChanged<T>(m_objLine);
        }

        public override void UnsetChanged()
        {
            if (this.Changed)
            {
                this.Changed = false;
                DoPropertyChanged("Changed");

                m_lChangedPropNames.Clear();
            }
        }

        public abstract Visibility Visibility { get; }
        public abstract bool IsEnabled { get; }

        public override string ToString()
        {
            return string.Format("View of {0}", m_objLine);
        }
    }
}
