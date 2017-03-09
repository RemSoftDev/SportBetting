using System;
using System.Globalization;
using System.Threading;
using IocContainer;
using Ninject;

namespace TranslationByMarkupExtension
{
    public class TranslationManager
    {
        private static TranslationManager _translationManager;

        public event EventHandler BeforeLanguageChanged;
        public event EventHandler LanguageChanged;

        public CultureInfo CurrentLanguage
        {
            get { return Thread.CurrentThread.CurrentUICulture; }
            private set
            {
                OnBeforeLanguageChanged(value);

                Thread.CurrentThread.CurrentCulture = value;
                Thread.CurrentThread.CurrentUICulture = value;

                TranslationData.ChangeLanguage();

                OnLanguageChanged(value);
            }
        }

        public void SetLanguage(string shortLangName, CultureInfo cultureInfo)
        {
            CurrentLanguage = cultureInfo;
            this.TranslationProvider.CurrentLanguage = shortLangName;


        }

        public static TranslationManager Instance
        {
            get { return _translationManager ?? (_translationManager = new TranslationManager()); }
        }

        public ITranslationProvider TranslationProvider
        {
            get
            {
                return IoCContainer.Kernel.Get<ITranslationProvider>();
            }
        }

        private void OnLanguageChanged(CultureInfo cultureInfo)
        {
            var handler = LanguageChanged;
            if (handler != null)
            {
                handler(this, new CultureInfoChangeEventArgs(cultureInfo));
            }
        }

        private void OnBeforeLanguageChanged(CultureInfo cultureInfo)
        {
            var handler = BeforeLanguageChanged;
            if (handler != null)
            {
                handler(this, new CultureInfoChangeEventArgs(cultureInfo));
            }
        }

        public object Translate(MultistringTag key, string args = "")
        {
            if (TranslationProvider != null)
            {
                object translatedValue = null;
                translatedValue = String.IsNullOrEmpty(args) ? TranslationProvider.Translate(key) : TranslationProvider.Translate(key, args.Split(','));
                if (translatedValue != null)
                {
                    return translatedValue;
                }
            }

            string result = key.Value;
            if (!String.IsNullOrEmpty(args))
                result = String.Format(result, args.Split(','));

            return string.Format("!{0}!", result);
        }


    }
}
