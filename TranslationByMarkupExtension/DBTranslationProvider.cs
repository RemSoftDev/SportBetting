using System;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using SportRadar.Common.Logs;
using SportRadar.DAL.NewLineObjects;
using TranslationByMarkupExtension.Annotations;

namespace TranslationByMarkupExtension
{
    /// <summary>
    /// 
    /// </summary>
    public class DBTranslationProvider : ITranslationProvider, INotifyPropertyChanged
    {
        #region Private Members

        private static readonly ILog Log = LogFactory.CreateLog(typeof(DBTranslationProvider));

        #endregion
        //private static SportRadarMatchCollection _srmc;

        #region Construction

        public DBTranslationProvider()
        {

        }


        #endregion

        #region ITranslationProvider Members

        public string Translate(MultistringTag key, params object[] args)
        {

            string result = "";
            string sLang = CurrentLanguage;
            result = LineSr.Instance.AllObjects.TaggedStrings.GetStringSafely(key.Value, sLang.ToLowerInvariant());


            if (!string.IsNullOrEmpty(result))
            {
                if (args.Length > 0)
                {
                    try
                    {
                        for (int i = 0; i < args.Length; i++)
                        {
                            object arg = args[i];
                            result = result.Replace("{" + i + "}", arg.ToString());
                        }
                    }
                    catch (Exception e)
                    {

                        Log.Error(key.Default,e);
                        Log.Error(e.Message, e);
                    }
                }
                return result.Replace("\\r", "\r").Replace("\\n", "\n");
            }

            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["show_multistring_tags"]))
            {
                if (!string.IsNullOrEmpty(key.Default.Trim(' ')))
                    return key.Default;
            }

            return string.Format("!{0}!", key.Value);
        }

        public object TranslateForPrinter(MultistringTag key)
        {
            if (PrintingLanguage != null)
            {
                string sLang = PrintingLanguage.ToLowerInvariant();

                var result = LineSr.Instance.AllObjects.TaggedStrings.GetStringSafely(key.Value, sLang.ToLowerInvariant());

                if (!string.IsNullOrEmpty(result))
                    return result;
            }

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

        public string PrintingLanguage
        {
            get { return _printingLanguage; }
            set { _printingLanguage = value; }
        }

        private string _defaultLanguage;
        private string _printingLanguage;

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
