using System;
using System.Windows;
using System.Windows.Controls;
using IocContainer;
using Ninject;
using SportBetting.WPF.Prism.Models.Interfaces;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;

namespace ViewModels
{
    public class RegistrationDataTemplateSelector : DataTemplateSelector
    {
        private IChangeTracker _changeTracker;
        public IChangeTracker ChangeTracker
        {
            get
            {
                return _changeTracker ?? (_changeTracker = IoCContainer.Kernel.Get<IChangeTracker>());
            }
        }

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            Registration registration = item as Registration;

            string DisabledTemplateName = "RegistrationDateDisabledTemplate";
            if (ChangeTracker.NeedVerticalRegistrationFields)
                DisabledTemplateName += "Vertical";

            DataTemplate disabledTemplate = Application.Current.FindResource(DisabledTemplateName) as DataTemplate;

            if ((disabledTemplate != null) && (!registration.IsEnabled))
                return disabledTemplate;

            if ((disabledTemplate != null) && (registration.ReadOnly))
                return disabledTemplate;

            if (registration != null)
            {
                DataTemplate dataTemplate = null;
                string templateName = "";

                switch (registration.Type)
                {
                    case FieldType.Date:
                        {
                            templateName = "RegistrationDataTemplateDate";
                            break;
                        }
                    case FieldType.TermsConditions:
                        {
                            templateName = "RegistrationDataTemplateCheckbox";
                            break;
                        }
                    case FieldType.Numeric:
                        {
                            templateName = "RegistrationDataTemplateNumeric";
                            break;
                        }
                    case FieldType.Password:
                    case FieldType.Password2:
                        {
                            templateName = "RegistrationDataTemplatePassword";
                            break;
                        }
                    case FieldType.DropDown:
                    case FieldType.Selector:
                        {
                            templateName = "RegistrationDataTemplate_Selector";
                            break;
                        }
                    case FieldType.EMail:
                    case FieldType.Text:
                        {
                            if (registration.Name == "tax_number")
                                templateName = "RegistrationDataTaxNumber";
                            else
                                templateName = "RegistrationDataTemplateText";

                            break;
                        }
                }

                if (!String.IsNullOrEmpty(templateName))
                {

                    if (ChangeTracker.NeedVerticalRegistrationFields)
                        templateName += "Vertical";

                    dataTemplate = Application.Current.FindResource(templateName) as DataTemplate;
                }

                if (dataTemplate != null)
                    return dataTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}