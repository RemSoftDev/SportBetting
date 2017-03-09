using System.Windows;
using System.Windows.Controls;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using System.Diagnostics;
using SportRadar.DAL.OldLineObjects;

namespace SportBetting.WPF.Prism.Shared
{
    public class LiveMatchesTemplateSelector : DataTemplateSelector
    {
        private DataTemplate fullTemplate;
        private DataTemplate twoItemsTemplate;
        private DataTemplate basketballTemplate;

        public LiveMatchesTemplateSelector()
        { 
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {

            var currentEntity = (IMatchVw)item;
            if(currentEntity==null)
                return this.fullTemplate;

            string sportDescriptor = currentEntity.SportDescriptor;
            Debug.Assert(sportDescriptor != null);


            if (sportDescriptor == SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY || sportDescriptor == SportSr.SPORT_DESCRIPTOR_SOCCER || sportDescriptor == SportSr.SPORT_DESCRIPTOR_RUGBY)
                return this.fullTemplate;

            if (sportDescriptor == SportSr.SPORT_DESCRIPTOR_TENNIS)
                return this.twoItemsTemplate;

            if (sportDescriptor == SportSr.SPORT_DESCRIPTOR_BASKETBALL || sportDescriptor == SportSr.SPORT_DESCRIPTOR_VOLLEYBALL)
                return this.basketballTemplate;

            return this.fullTemplate;

        }

        public DataTemplate BasketballTemplate
        {
            get
            {
                return basketballTemplate;
            }
            set
            {
                this.basketballTemplate = value;
            }
        }

        public DataTemplate FullTemplate
        {
            get
            {
                return fullTemplate;
            }
            set
            {
                this.fullTemplate = value;
            }
        }

        public DataTemplate TwoItemsTemplate
        {
            get
            {
                return twoItemsTemplate;
            }
            set
            {
                this.twoItemsTemplate = value;
            }
        }
    }
}
