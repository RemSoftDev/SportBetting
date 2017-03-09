using System.Windows;
using System.Windows.Controls;
using SportBetting.WPF.Prism.Models;

namespace ViewModels
{
    public class UserTemplateSelector : DataTemplateSelector
    {

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is AnonymousUser)
            {
#if BETCENTER
                return Application.Current.FindResource("BetcenterAnonymous") as DataTemplate;
#else
                return Application.Current.FindResource("Anonymous") as DataTemplate;
#endif
            }
            if (item is OperatorUser)
            {
#if BETCENTER
                return Application.Current.FindResource("BetcenterOperator") as DataTemplate;
#else
                return Application.Current.FindResource("Operator") as DataTemplate;
#endif
            }
            if (item is LoggedInUser)
            {
#if BETCENTER
                return Application.Current.FindResource("BetcenterLoggedIn") as DataTemplate;
#else
                return Application.Current.FindResource("LoggedIn") as DataTemplate;
#endif
            }            
            if (item is EmptyUser)
            {
#if BETCENTER
                return Application.Current.FindResource("BetcenterNoConnection") as DataTemplate;
#else
                return Application.Current.FindResource("NoConnection") as DataTemplate;
#endif
            }

            return null;

        }
    }
}