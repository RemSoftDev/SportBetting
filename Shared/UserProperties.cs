using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public enum FieldType
    {
        Text = 1, Numeric = 2, Date = 3, Password = 4, Password2 = 5, Selector = 6, EMail = 7
    }

    public class SelectorValue
    {
        private string _name;
        private string _value;

        public SelectorValue(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

    }

    public class UserProperties
    {

        public static readonly Dictionary<string, FieldType> AccountingToFormFieldsMap = new Dictionary<string, FieldType>
		{
			{"EMAIL", FieldType.EMail},
			{"STRING", FieldType.Text},
			{"PHONE", FieldType.Numeric},
			{"NUMBER", FieldType.Numeric},
			{"DATE", FieldType.Date},
			{"PASSWORD", FieldType.Password},
			{"CURRENCY", FieldType.Text},
            {"LANGUAGE", FieldType.Selector}
		};

        private string _name;
        private bool _readOnly;
        private FieldType _type;
        private string _value;
        private ObservableCollection<SelectorValue> _options = new ObservableCollection<SelectorValue>();
        private bool _mandatory;
        private IList<PropertyValidationRule> _rules = new List<PropertyValidationRule>();

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public bool ReadOnly
        {
            get { return _readOnly; }
            set { _readOnly = value; }
        }

        public FieldType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public ObservableCollection<SelectorValue> Options
        {
            get { return _options; }
            set { _options = value; }
        }

        public bool Mandatory
        {
            get { return _mandatory; }
            set { _mandatory = value; }
        }

        public IList<PropertyValidationRule> Rules
        {
            get { return _rules; }
            set { _rules = value; }
        }
    }

    public class PropertyValidationRule
    {
        public PropertyValidationRule(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }
}
