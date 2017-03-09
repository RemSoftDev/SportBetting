using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using SportBetting.WPF.Prism.Shared.Annotations;

namespace SportBetting.WPF.Prism.Shared.Models
{
    /// <summary>
    /// Sport Data object class which fully supports serialization, property changed notifications,
    /// backwards compatibility and error checking.
    /// </summary>
    public class ComboBoxItem : INotifyPropertyChanged
    {
        private string _name;
        private Visibility _visibility = Visibility.Visible;

        public ComboBoxItem(string name, long id)
        {
            Id = id;
            Name = name;
        }

        public ComboBoxItem(string name, long id, string sportDescription)
        {
            Id = id;
            Name = name;
            SportDescription = sportDescription;
        }

        public long Id { get; private set; }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        
        public string SportDescription { get; private set; }
        
        public Visibility Visibility
        {
            get { return _visibility; }
            set
            {
                _visibility = value;
                OnPropertyChanged();
            }
        }


        public int OrderId { get; set; }

        public override int GetHashCode()
        {
            return (int)Id;
        }

        public override bool Equals(object obj)
        {
            var odd = obj as ComboBoxItem;
            return odd != null && odd.Id == this.Id && odd.Name == this.Name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}