using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace TranslationByMarkupExtension
{
	/// <summary>
	/// The Translate Markup extension returns a binding to a TranslationData
	/// that provides a translated resource of the specified key
	/// </summary>
	public class TranslateExtension : MarkupExtension
	{
		#region Private Members

		private MultistringTag _key;
        private string _args = null;

		#endregion

		#region Construction

		/// <summary>
		/// Initializes a new instance of the <see cref="TranslateExtension"/> class.
		/// </summary>
		/// <param name="key">The key.</param>
		public TranslateExtension(MultistringTag key)
		{
			_key = key;
		}		
		public TranslateExtension()
		{
		}

		#endregion

		[ConstructorArgument("key")]
		public MultistringTag Key
		{
			get { return _key; }
			set { _key = value; }
		}

        public string Args
        {
            get { return _args; }
            set { _args = value; }
        }

		/// <summary>
		/// See <see cref="MarkupExtension.ProvideValue" />
		/// </summary>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
		    var binding = new Binding("Value")
				  {
					  Source = TranslationData.GetTranslationData(_key, _args)
				  };

            return binding.ProvideValue(serviceProvider);
		}
	}
}
