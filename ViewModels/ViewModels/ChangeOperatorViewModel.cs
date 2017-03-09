using System.Collections.Generic;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Models.Interfaces;
using SportBetting.WPF.Prism.Modules.Aspects;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;

namespace ViewModels.ViewModels
{
    [ServiceAspect]
    public class ChangeOperatorViewModel : BaseViewModel
    {

        #region Constructors

        public ChangeOperatorViewModel()
        {

        }



        #endregion

        #region Properties

        public global::ViewModels.Registration Username { get; set; }
        public global::ViewModels.Registration FirstName { get; set; }
        public global::ViewModels.Registration LastName { get; set; }
        public global::ViewModels.Registration Email { get; set; }
        public global::ViewModels.Registration Location { get; set; }
        public global::ViewModels.Registration Franchisor { get; set; }
        public global::ViewModels.Registration Role { get; set; }
        public global::ViewModels.Registration CardPin { get; set; }
        //public Registration OperatorType { get; set; }


        public bool IsMouseOver
        {
            get { return _isMouseOver; }
            set
            {
                if (value.Equals(_isMouseOver)) return;
                _isMouseOver = value;
            }
        }



        public List<global::ViewModels.Registration> FormFields
        {
            get { return _formFields; }
            set
            {
                _formFields = value;
                OnPropertyChanged("FormFields");
            }
        }

        #endregion

        #region Commands

        public Command SaveOperatorProfileCommand { get; private set; }
        public Command ChangeOperatorProfileCommand { get; private set; }
        public Command CancelEditingProfileCommand { get; private set; }

        #endregion

        #region Methods

        private List<global::ViewModels.Registration> _formFields;
        private bool _isMouseOver;

        public override void OnNavigationCompleted()
        {
            InitFields();
            base.OnNavigationCompleted();
        }

        private void InitFields()
        {
            var fields = new List<global::ViewModels.Registration>();

            Username = new global::ViewModels.Registration();
            Username.Mandatory = true;
            Username.Label = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_USERNAME) as string;
            Username.Name = "username";
            Username.Type = FieldType.Text;
            Username.Rules = new List<fieldValidationRule> { new fieldValidationRule { name = "MAX", value = "255" }, new fieldValidationRule { name = "MIN", value = "3" } };
            fields.Add(Username);
            Username.Value = ChangeTracker.FoundOperator.Username;

            FirstName = new global::ViewModels.Registration();
            FirstName.Name = "first_name";
            FirstName.Label = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_FIRSTNAME) as string;
            FirstName.Mandatory = true;
            FirstName.Type = FieldType.Text;
            FirstName.Rules = new List<fieldValidationRule> { new fieldValidationRule { name = "MAX", value = "35" }, new fieldValidationRule { name = "MIN", value = "3" } };
            fields.Add(FirstName);
            FirstName.Value = ChangeTracker.FoundOperator.Firstname;


            LastName = new global::ViewModels.Registration();
            LastName.Name = "last_name";
            LastName.Mandatory = true;
            LastName.Label = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_LASTNAME) as string;
            LastName.Type = FieldType.Text;
            LastName.Rules = new List<fieldValidationRule> { new fieldValidationRule { name = "MAX", value = "35" }, new fieldValidationRule { name = "MIN", value = "3" } };
            fields.Add(LastName);
            LastName.Value = ChangeTracker.FoundOperator.Lastname;


            Email = new global::ViewModels.Registration();
            Email.Name = "email";
            Email.Type = FieldType.EMail;
            Email.Label = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_EMAIL) as string;
            fields.Add(Email);
            Email.Value = ChangeTracker.FoundOperator.EMail;

            Location = new global::ViewModels.Registration();
            Location.Name = "location";
            Location.Label = TranslationProvider.Translate(MultistringTags.LOCATION) as string;
            Location.Type = FieldType.Text;
            fields.Add(Location);
            Location.Value = ChangeTracker.FoundOperator.Location;


            Franchisor = new global::ViewModels.Registration();
            Franchisor.Name = "franchisor";
            Franchisor.Label = TranslationProvider.Translate(MultistringTags.FRANCHISOR) as string;
            Franchisor.Type = FieldType.Text;
            fields.Add(Franchisor);
            Franchisor.Value = ChangeTracker.FoundOperator.Franchisor;

            Role = new global::ViewModels.Registration();
            Role.Name = "role";
            Role.Label = TranslationProvider.Translate(MultistringTags.ROLE) as string;
            Role.Type = FieldType.Text;
            fields.Add(Role);
            Role.Value = ChangeTracker.FoundOperator.Role;

            if (ChangeTracker.FoundOperator.ActiveCard == true)
            {
                CardPin = new global::ViewModels.Registration();
                CardPin.Name = "cardpin";
                CardPin.Label = TranslationProvider.Translate(MultistringTags.CARDPIN) as string;
                if (!StationRepository.IsIdCardEnabled)
                    CardPin.Label = TranslationProvider.Translate(MultistringTags.BARCODECARDPIN) as string;
                CardPin.Type = FieldType.Text;
                fields.Add(CardPin);
                CardPin.Value = ChangeTracker.FoundOperator.CardPin;
            }


            FormFields = fields;

        }




        #endregion
    }
}