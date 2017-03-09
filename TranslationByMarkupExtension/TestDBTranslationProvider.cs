using System.ComponentModel;
using System.Runtime.CompilerServices;
using SportRadar.DAL.NewLineObjects;
using TranslationByMarkupExtension.Annotations;

namespace TranslationByMarkupExtension
{
    /// <summary>
    /// 
    /// </summary>
    public class TestDBTranslationProvider : ITranslationProvider, INotifyPropertyChanged
    {
        #region Private Members


        #endregion
        //private static SportRadarMatchCollection _srmc;

        #region Construction

        public TestDBTranslationProvider()
        {

        }


        #endregion

        #region ITranslationProvider Members

        private static TaggedStringDictionary TaggedStrings = LineSr.Instance.AllObjects.TaggedStrings;
        public string Translate(MultistringTag key,params object[] args)
        {
            string result = "";
            string sLang = CurrentLanguage;
            result = TaggedStrings.GetStringSafely(key.Value, sLang.ToLowerInvariant());


            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            return string.Format("{0}", key.Default);
        }

        public object TranslateForPrinter(MultistringTag key)
        {
            string sLang = PrintingLanguage.ToLowerInvariant();

            var result = TaggedStrings.GetStringSafely(key.Value, sLang.ToLowerInvariant());

            if (!string.IsNullOrEmpty(result))
                return result;

            return string.Format("!{0}!", key.Value);
        }

        #endregion

        #region ITranslationProvider Members

        private string _currentLanguage;
        public string CurrentLanguage
        {
            get { return this._currentLanguage ?? DefaultLanguage; }
            set
            {
                this._currentLanguage = value;
                OnPropertyChanged();
            }
        }

        public string PrintingLanguage { get; set; }

        private string _defaultLanguage;
        public string DefaultLanguage
        {
            get { return this._defaultLanguage ?? "EN"; }
            set { this._defaultLanguage = value; }
        }


        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
