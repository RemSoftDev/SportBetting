using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SportBetting.WPF.Prism.ModalWindows;

namespace ViewModels
{
    public class TermsAndConditionsCategory
    {
        public String Category { get; set; }

        private Dictionary<string, List<string>> _dTexts;
        public Dictionary<string, List<string>> Texts
        {
            get { return _dTexts ?? (_dTexts = new Dictionary<string, List<string>>()); }
            set { _dTexts = value; }
        }

        private Dictionary<string, Dictionary<string, List<string>>> _dHeadings = new Dictionary<string, Dictionary<string, List<string>>>();
        public Dictionary<string, Dictionary<string, List<string>>> Headings
        {
            get { return _dHeadings ?? (_dHeadings = new Dictionary<string, Dictionary<string, List<string>>>()); }
            set { _dHeadings = value; }
        }

        private ObservableCollection<TCMenuButton> _rbList;
        public ObservableCollection<TCMenuButton> Buttons
        {
            get { return _rbList ?? (_rbList = new ObservableCollection<TCMenuButton>()); }
            set { _rbList = value; }
        }

        public TermsAndConditionsCategory(string category)
        {
            Category = category;
        }
    }
}
