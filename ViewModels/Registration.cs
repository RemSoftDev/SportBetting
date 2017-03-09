using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using BaseObjects;
using IocContainer;
using Ninject;
using SharedInterfaces;
using SportBetting.WPF.Prism.Models.Interfaces;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;
using System.Collections.Generic;
using SportBetting.WPF.Prism.Shared.WpfHelper;

namespace ViewModels
{
    public delegate void isValidatedDelegate(Registration item, bool valid);

    public class Registration : MyModelBase
    {
        #region Variables
        private string EMAIL_REGEX =
            @"^[-a-z0-9!#$%&'*+/=?^_`{|}~]+(?:\.[-a-z0-9!#$%&'*+/=?^_`{|}~]+)*@(?:[a-z0-9]([-a-z0-9]{0,61}[a-z0-9])?\.)*(?:aero|arpa|asia|biz|cat|com|coop|edu|gov|info|int|jobs|mil|mobi|museum|name|net|org|pro|tel|travel|[a-z][a-z])$";
        //old regexp: @"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$";

        public ITranslationProvider TranslationProvider { get { return IoCContainer.Kernel.Get<ITranslationProvider>(); } }

        string ErrorDateLow = "DATELOW";
        string ErrorDateHigh = "DATEHIGH";

        string ErrorTextLow = "TEXTLOW";
        string ErrorTextHigh = "TEXTHIGH";

        string ErrorIntLow = "INTLOW";
        string ErrorIntHigh = "INTHIGH";
        private ObservableCollection<SelectorValue> _options = new ObservableCollection<SelectorValue>() { new SelectorValue("", "") };
        private string _valueInt;
        private bool _valueBool;
        private FieldType _type;
        private string _label;
        private bool _visible = true;
        private bool _focus;
        private List<fieldValidationRule> _rules = new List<fieldValidationRule>();

        public event isValidatedDelegate isValidatedEvent;

        #endregion

        #region Constructor & destructor

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public Registration()
            : this("", "", FieldType.Text, new List<fieldValidationRule>(), new ObservableCollection<SelectorValue>(){new SelectorValue("","")}, false, false)
        {
            this.SelectDateCommand = new Command(OnSelectDateExecute);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Category"/> class.
        /// </summary>
        /// <param name="id">The id. </param>
        /// <param name="name">The name.</param>
        /// <remarks></remarks>
        public Registration(string name, string label, FieldType type, List<fieldValidationRule> rules,
            ObservableCollection<SelectorValue> options, bool readOnly, bool mandatory)
        {
            Label = label;
            Name = name;
            this.Type = type;
            Rules = rules;
            Options = options;
            IsEnabled = readOnly;
            Mandatory = mandatory;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets visiblity of field.
        /// </summary>
        public bool Visible
        {
            get { return _visible; }
            set
            {
                _visible = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        public Registration PasswordConfirmation { get; set; }


        /// <summary>
        /// Gets or sets the Label.
        /// </summary>
        public string Label
        {
            get { return _label; }
            set
            {
                _label = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public FieldType Type
        {
            get { return _type; }
            set { _type = value; }
        }




        public bool ValueBool
        {
            get { return _valueBool; }
            set
            {
                _valueBool = value;
                ValidateFields();
                OnPropertyChanged();
            }
        }



        /// <summary>
        /// Gets or sets ValueInt.
        /// </summary>
        public string ValueInt
        {
            get { return _valueInt; }
            set
            {
                OnValueIntChanged(_valueInt, value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the ErrorText.
        /// </summary>
        public string ErrorText
        {
            get { return _errorText; }
            set
            {
                _errorText = value;
                ValidateFields();
                OnPropertyChanged("Value");
            }
        }


        /// <summary>
        /// Gets or sets the Rules.
        /// </summary>
        public List<fieldValidationRule> Rules
        {
            get { return _rules; }
            set { _rules = value; }
        }


        /// <summary>
        /// Gets or sets the Focus.
        /// </summary>
        public bool Focus
        {
            get { return _focus; }
            set
            {
                _focus = value;
                OnPropertyChanged();
            }
        }

        private static IMediator Mediator
        {
            get { return IoCContainer.Kernel.Get<IMediator>(); }
        }

        private bool _isFocused;
        private bool _isEnabled;
        private bool _readOnly;
        private bool _mandatory;
        private string _errorText;

        public bool IsFocused
        {
            get { return _isFocused; }
            set
            {
                _isFocused = value;
                if (value)
                {
                    Mediator.SendMessage(MsgTag.ShowKeyboard, MsgTag.ShowKeyboard);
                }
                else
                {
                    Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
                }
                OnPropertyChanged();
            }
        }
        public ObservableCollection<SelectorValue> Options
        {
            get { return _options; }
            set { _options = value; }
        }


        /// <summary>
        /// Gets or sets the Focus.
        /// </summary>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// Gets or sets the Focus.
        /// </summary>
        public bool Mandatory
        {
            get { return _mandatory; }
            set
            {
                _mandatory = value;
                OnPropertyChanged();
            }
        }

        public bool ReadOnly
        {
            get { return _readOnly; }
            set { _readOnly = value; OnPropertyChanged(); }
        }

        #endregion

        #region Commands

        public Command SelectDateCommand { get; private set; }

        public string default_value { get; set; }

        public MultistringTag Multistring { get; set; }

        #endregion

        #region Methods

        private void OnSelectDateExecute()
        {
            DateTime? initialDate = null;
            var cultureInfo = TranslationManager.Instance.CurrentLanguage;

            { // parse input string
                DateTime dtTmp;
                //if (DateTime.TryParse(this.Value ?? string.Empty, cultureInfo, DateTimeStyles.None, out dtTmp))
                //{
                //    initialDate = dtTmp;
                //}

                //parsing should be done by data format only, otherwise system uses standard format that does not match. D.P.
                if (DateTime.TryParseExact(this.Value ?? string.Empty, "dd.MM.yyyy", cultureInfo, DateTimeStyles.None, out dtTmp))
                {
                    initialDate = dtTmp;
                }
            }

            ISelectDate selectDate = IoCContainer.Kernel.Get<ISelectDate>();
            var result = selectDate.SelectDate(initialDate, null, null);

            this.Value = (result == null) ? string.Empty : result.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        }




        private void OnValueIntChanged(string oldValue, string newValue)
        {
            if (oldValue == newValue)
            {
                return;
            }

            List<char> numbers = new List<char>();

            foreach (var ch in newValue)
            {
                if (char.IsDigit(ch))
                {
                    numbers.Add(ch);
                }
            }

            string numbersOnly = new string(numbers.ToArray());
            this.Value = numbersOnly;
            _valueInt = numbersOnly;
            OnPropertyChanged("ValueInt");
        }

        public override List<string> ValidateFields()
        {
            List<string> validationResults = new List<string>();
            if (!string.IsNullOrEmpty(ErrorText))
            {
                validationResults.Add(ErrorText);
                ErrorText = null;
            }
            if (string.IsNullOrEmpty(Value) && this.Mandatory)
            {
                validationResults.Add(this.Name + " is required");
            }
            if (Rules != null)
            {
                object minValid = null;
                object maxValid = null;

                List<string> validationRules = new List<string>();
                foreach (fieldValidationRule validationRule in Rules)
                {
                    switch (validationRule.name)
                    {

                        case "MIN":
                            minValid = validationRule.value;
                            break;

                        case "MAX":
                            maxValid = validationRule.value;
                            break;

                        case "REG_EXP":

                            string toTest = "";
                            if (this.Type == FieldType.EMail)
                            {
                                if (this.Value != null) toTest = this.Value.ToLowerInvariant();
                                else toTest = null;
                            }
                            else
                                toTest = this.Value;


                            if (!String.IsNullOrEmpty(this.Value) && !Regex.IsMatch(toTest, validationRule.value))
                            {
                                if (this.Type == FieldType.EMail)
                                    validationResults.Add(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_EMAIL_WRONGFORMAT));
                                else if (String.IsNullOrEmpty(this.Value) && !this.Mandatory)
                                {
                                }
                                else
                                    validationResults.Add(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_INVALIDVALUE));
                            }
                            break;
                    }
                }
                if (validationRules != null && validationRules.Count > 0 && !string.IsNullOrEmpty(Value))
                {
                    foreach (string validationRule in validationRules)
                    {
                        if (!Regex.IsMatch(this.Value, validationRule))
                        {
                            validationResults.Add(TranslationProvider.Translate(MultistringTag.Assign("TERMINAL_FORM_" + Name.ToUpperInvariant() + "_WRONGREGEXP", "")));
                        }

                    }
                }
                if (minValid != null || maxValid != null)
                {
                    string error = "";
                    switch (this.Type)
                    {
                        case FieldType.Text:
                        case FieldType.Password:
                        case FieldType.Password2:
                            error = ValidateText(this.Value, minValid, maxValid);
                            break;
                        case FieldType.Numeric:
                            error = ValidateText(this.Value, minValid, maxValid);
                            break;
                        case FieldType.Date:

                            error = ValidateDate(this.Value, minValid, maxValid);
                            break;
                    }
                    if (!String.IsNullOrEmpty(error))
                    {
                        if (this.Type == FieldType.Password || this.Type == FieldType.Password2)
                            validationResults.Add(error);
                        else
                            validationResults.Add(error);
                    }
                }
                if (PasswordConfirmation != null)
                {
                    string error = "";
                    switch (this.Type)
                    {
                        //case FieldType.Password:
                        //    if (PasswordConfirmation != null && (string.IsNullOrEmpty(error) && !string.IsNullOrEmpty(PasswordConfirmation.Value)))
                        //        error = ValidateSecondPassword(this.Value, PasswordConfirmation.Value);
                        //    break;
                        case FieldType.Password2:
                            if (PasswordConfirmation != null)
                                error = ValidateSecondPassword(this.Value, PasswordConfirmation.Value);
                            break;
                    }
                    if (!String.IsNullOrEmpty(error))
                        validationResults.Add(error);
                }
            }
            if (this.Type == FieldType.EMail && !string.IsNullOrEmpty(Value))
            {
                if (!Regex.IsMatch(this.Value.ToLowerInvariant(), EMAIL_REGEX))
                {
                    validationResults.Add(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_EMAIL_WRONGFORMAT));
                }
            }

            if (validationResults.Count > 0)
            {
                Error = validationResults[0];
            }
            else
            {
                Error = null;
            }

            if (isValidatedEvent != null)
            {
                isValidatedEvent(this, validationResults.Count == 0);
            }
            return validationResults;
        }

        private string ValidateDate(string value, object Min, object Max)
        {
            if (value == null)
                return "";

            DateTime minDate;
            DateTime.TryParse(Min.ToString(), new DateTimeFormatInfo() { DateSeparator = ".", ShortDatePattern = "ddMMyyyy" }, DateTimeStyles.AllowWhiteSpaces, out minDate);

            if (minDate == DateTime.MinValue)
            {
                DateTime valueDate;
                DateTime.TryParse(value, new DateTimeFormatInfo() { DateSeparator = ".", ShortDatePattern = "ddMMyyyy" }, DateTimeStyles.AllowWhiteSpaces, out valueDate);
                DateTime maximumBirthdate = DateTime.Now.AddYears(-Convert.ToInt32(Min));
                DateTime minimumBirthdate = DateTime.Now.AddYears(-Convert.ToInt32(Max));

                if (maximumBirthdate < valueDate)
                {
                    return TranslationProvider.Translate(MultistringTag.Assign("TERMINAL_FORM_" + Name.ToUpperInvariant() + "_" + ErrorDateLow, "")) as string;
                }

                if (minimumBirthdate > valueDate)
                {
                    return TranslationProvider.Translate(MultistringTag.Assign("TERMINAL_FORM_" + Name.ToUpperInvariant() + "_" + ErrorDateHigh, "")) as string;
                }

            }
            else
            {
                DateTime valueDate;
                DateTime.TryParse(value, new DateTimeFormatInfo() { DateSeparator = ".", ShortDatePattern = "ddMMyyyy" }, DateTimeStyles.AllowWhiteSpaces, out valueDate);


                if (Min != null && valueDate < DateTime.Parse((string)Min))
                {
                    return TranslationProvider.Translate(MultistringTag.Assign("TERMINAL_FORM_" + Name.ToUpperInvariant() + "_" + ErrorDateLow, "")) as string;
                }

                if (Max != null && valueDate > DateTime.Parse((string)Max))
                {
                    return TranslationProvider.Translate(MultistringTag.Assign("TERMINAL_FORM_" + Name.ToUpperInvariant() + "_" + ErrorDateHigh, "")) as string;
                }
            }

            return "";
        }

        private string ValidateText(string value, object Min, object Max)
        {
            if (value == null)
                return "";

            if (!this.Mandatory && value.Trim().Length == 0) return "";


            if (Min != null && ((string)value).Trim().Length < Convert.ToInt32(Min))
            {
                var name = Name.ToUpperInvariant();
                var tag = MultistringTag.Assign("TERMINAL_FORM_" + name + "_" + ErrorTextLow, "");
                return TranslationProvider.Translate(tag) as string;
            }

            if (Max != null && ((string)value).Trim().Length > Convert.ToInt32(Max))
            {
                return TranslationProvider.Translate(MultistringTag.Assign("TERMINAL_FORM_" + Name.ToUpperInvariant() + "_" + ErrorTextHigh, "")) as string;
            }

            return "";
        }
        private string ValidateSecondPassword(string value, string secondValue)
        {
            if (value == null)
                return "";

            if (!value.Equals(secondValue))
            {
                return TranslationProvider.Translate(MultistringTags.PASSWORDS_ARE_DIFFERENT) as string;
            }


            return "";
        }

        private string ValidateInteger(string value, object Min, object Max)
        {

            if (Min != null && ((string)value) != "" && Convert.ToDecimal(value) < Convert.ToDecimal(Min))
            {
                return TranslationProvider.Translate(MultistringTag.Assign("TERMINAL_FORM_" + Name.ToUpperInvariant() + "_" + ErrorIntLow, "")) as string;
            }

            if (Max != null && ((string)value) != "" && Convert.ToDecimal(value) > Convert.ToDecimal(Max))
            {
                return TranslationProvider.Translate(MultistringTag.Assign("TERMINAL_FORM_" + Name.ToUpperInvariant() + "_" + ErrorIntHigh, "")) as string;
            }

            return "";
        }

        #endregion

    }
}