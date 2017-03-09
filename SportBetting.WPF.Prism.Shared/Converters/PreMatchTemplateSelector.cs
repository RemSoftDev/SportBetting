using System.Text;
using System.Windows;
using System.Windows.Controls;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Models;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using SportRadar.Common.Collections;
using System.Diagnostics;
using SportRadar.DAL.OldLineObjects;

namespace SportBetting.WPF.Prism.Shared
{
    public class PreMatchTemplateSelector : DataTemplateSelector
    {
        private DataTemplate fullTemplate;
        private DataTemplate twoItemsTemplate;
        private DataTemplate matchBetTemplate;
        private DataTemplate _twoItemsUoTemplate;
        private DataTemplate _outrightTemplate;

        public PreMatchTemplateSelector()
        {
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {

            var currentEntity = (IMatchVw)item;
            if (currentEntity == null)
                return this.fullTemplate;

            var m_sportGroup = currentEntity.SportView.LineObject;
            Debug.Assert(m_sportGroup != null && m_sportGroup.GroupSport != null && !string.IsNullOrEmpty(m_sportGroup.GroupSport.SportDescriptor));

            string sSportDescroptor = m_sportGroup.GroupSport.SportDescriptor;

            if (currentEntity.IsOutright) 
                return this._outrightTemplate;

            if (sSportDescroptor == SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY || sSportDescroptor == SportSr.SPORT_DESCRIPTOR_SOCCER)
                return this.fullTemplate;

            if (sSportDescroptor == SportSr.SPORT_DESCRIPTOR_BASKETBALL)
                return this._twoItemsUoTemplate;
            if (sSportDescroptor == SportSr.SPORT_DESCRIPTOR_TENNIS)
                return this.twoItemsTemplate;

            if (sSportDescroptor == SportSr.SPORT_DESCRIPTOR_RUGBY || sSportDescroptor == SportSr.SPORT_DESCRIPTOR_HANDBALL)
                return this.matchBetTemplate;

            return this.twoItemsTemplate;

        }

        public DataTemplate MatchBetTemplate
        {
            get
            {
                return this.matchBetTemplate;
            }
            set
            {
                this.matchBetTemplate = value;
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
        public DataTemplate TwoItemsUoTemplate
        {
            get { return _twoItemsUoTemplate; }
            set { _twoItemsUoTemplate = value; }
        }

        public DataTemplate OutrightTemplate
        {
            get { return _outrightTemplate; }
            set { _outrightTemplate = value; }
        }
    }
}