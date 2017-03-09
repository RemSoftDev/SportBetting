using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Shared.Annotations;

namespace ViewModels
{
    public class TCMenuButton : INotifyPropertyChanged
    {
        public string Text { get; set; }

        public string PublicText { get { return Text.Replace("#VFL#", "").Replace("#VHC#", "").Replace("#RG#", ""); } }

        private bool _selected = false;
        public bool Selected {
            get { return _selected; }
            set
            {
                if (value != _selected)
                {
                    _selected = value;
                    OnPropertyChanged("Selected");
                }
            }
        }

        private bool _enabled = true;
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (value != _enabled)
                {
                    _enabled = value;
                    OnPropertyChanged("Enabled");
                }
            }
        }
        public ObservableCollection<TCMenuButton> Children { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            try
            {
                var entity = this;
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs(propertyName));

            }
            catch (Exception e)
            {
                if (Debugger.IsAttached)
                    throw;
            }
        }
    }
}
