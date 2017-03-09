using System;
using System.ComponentModel;


namespace JHLib
{
    /// <summary>
    /// A base class for a something that implements INotifyPropertyChanged.
    /// This is inspired by several online articles, such as Jeremy Likness' article at http://www.codeproject.com/KB/silverlight/mvvm-explained.aspx
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public void Notify(string sPropertyName)
        {
            var handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(sPropertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
