using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TranslationByMarkupExtension.Annotations;

namespace TranslationByMarkupExtension
{
    public class TranslationData : INotifyPropertyChanged
    {
        private static readonly IDictionary<string, TranslationData> Cache = new Dictionary<string, TranslationData>();
        static object _locker = new object();
        public static TranslationData GetTranslationData(MultistringTag key, string args = "")
        {
            lock (_locker)
            {
                if (key == null || string.IsNullOrWhiteSpace(key.Value))
                {
                    return new TranslationData(key, args);
                }
                else if (!Cache.ContainsKey(key.Value + args))
                {
                    var translationDate = new TranslationData(key, args);
                    Cache.Add(key.Value + args, translationDate);
                    return translationDate;
                }
                return Cache[key.Value + args];
            }
        }

        public static void ChangeLanguage()
        {
            lock (_locker)
            {
                foreach (var translationData in Cache)
                {
                    translationData.Value.OnPropertyChanged("Value");
                }
            }
        }

        #region Private Members

        private MultistringTag _key;
        private string _args = "";

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationData"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        private TranslationData(MultistringTag key, string args = "")
        {
            this._args = args;
            _key = key;
        }


        public object Value
        {
            get
            {
                return TranslationManager.Instance.Translate(_key, _args);
            }
        }


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
