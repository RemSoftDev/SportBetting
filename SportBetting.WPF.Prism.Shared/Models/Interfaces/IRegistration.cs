using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using SportRadar.DAL.Annotations;
using WsdlRepository.WsdlServiceReference;

namespace SportBetting.WPF.Prism.Models.Interfaces
{
    public enum FieldType
    {
        Text = 1, Numeric = 2, Date = 3, Password = 4, Password2 = 5, Selector = 6, EMail = 7 , DropDown = 8, TermsConditions = 9
    }

    public class SelectorValue : INotifyPropertyChanged
    {
        private string _name;
        private string _value;
        private Visibility _visibility = Visibility.Visible;

        public SelectorValue(string name, string value)
        {
            this.Name = name;
            this.Value = value;
            if(name == "")
                Visibility = Visibility.Collapsed;
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name)
                    return;
                _name = value;
                OnPropertyChanged();
            }
        }
        public Visibility Visibility
        {
            get { return _visibility; }
            set
            {
                if (value == _visibility)
                    return;
                _visibility = value;
                OnPropertyChanged();
            }
        }

        public string Value
        {
            get { return _value; }
            set
            {
                if (value == _value)
                    return;
                _value = value;
                OnPropertyChanged();
            }
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


    /// <summary>
    /// The registration interface.
    /// </summary>
    public interface IRegistration
    {

        string Name { get; set; }
        string Label { get; set; }
        FieldType Type { get; set; }
        string Value { get; set; }
        string ErrorText { get; set; }
        List<fieldValidationRule> Rules { get; set; }
        bool Focus { get; set; }
        ObservableCollection<SelectorValue> Options { get; set; }
        bool IsEnabled { get; set; }
        bool ReadOnly { get; set; }
        bool Mandatory { get; set; }

    }
}
