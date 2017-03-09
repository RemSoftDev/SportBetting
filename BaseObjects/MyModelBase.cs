using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BaseObjects
{
    public class MyModelBase : INotifyPropertyChanged, IDataErrorInfo
    {

        private string _error;
        private bool _isEmpty = true;

        public string this[string columnName]
        {
            get
            {
                if (IsEmpty && EmptyValidation)
                    return " ";
                return Error;
            }
        }

        public bool IsEmpty
        {
            get { return _isEmpty; }
            set { _isEmpty = value; }
        }
        private string _value = "";
        private string _valueMasked = "";
        public bool EmptyValidation
        {
            get { return _emptyValidation; }
            set
            {
                _emptyValidation = value;
                ValidateFields();
            }
        }

        public string Value
        {
            get { return _value; }
            set
            {
                Error = null;
                IsEmpty = string.IsNullOrEmpty(value);

                if (value == null)
                    _value = string.Empty;
                _value = value;
                OnPropertyChanged();
                ValidateFields();
            }
        }

        /// <summary>
        /// Gets or sets ValueMasked.
        /// </summary>
        public string ValueMasked
        {
            get { return _valueMasked; }
            set
            {
                Error = null;
                OnValueMaskedChanged(_valueMasked, value);
                OnPropertyChanged();
                ValidateFields();

            }
        }

        private void OnValueMaskedChanged(string oldValue, string newValue)
        {

            if (((oldValue ?? string.Empty).Length + 1) == ((newValue ?? string.Empty).Length))
            {
                var change = newValue.Replace("*", "");
                var position = newValue.IndexOf(change);
                this.Value = (this.Value ?? string.Empty).Insert(position, change);
                _valueMasked = (newValue ?? string.Empty).Replace(change, "*");
            }
            else if ((oldValue ?? string.Empty).Length > (newValue ?? string.Empty).Length)
            {
                this.Value = (this.Value ?? string.Empty).Substring(0, (newValue ?? string.Empty).Length);
                _valueMasked = (newValue ?? string.Empty).Substring(0, (newValue ?? string.Empty).Length);
            }
        }

        public string Error
        {
            get { return _error; }
            set
            {
                _error = value;
                OnPropertyChanged();
                OnPropertyChanged("Value");
                OnPropertyChanged("ValueMasked");
            }
        }

        public virtual List<string> ValidateFields()
        {
            if (Validate != null)
                return Validate(this, null);
            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event DelegateDataSqlUpdateSucceeded Validate;
        public delegate List<string> DelegateDataSqlUpdateSucceeded(object sender, string property);

        private string calledProperty = "";
        private bool _emptyValidation = true;

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            calledProperty = "";
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                calledProperty = propertyName;
                handler(this, new PropertyChangedEventArgs(propertyName));
            }

        }


    }
}