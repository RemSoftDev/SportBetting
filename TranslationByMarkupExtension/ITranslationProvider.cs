namespace TranslationByMarkupExtension
{
	public interface ITranslationProvider
	{
		/// <summary>
		/// Translates the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
        /// <param name="bIsPrintingString">Optional. Defines different language for printing tickets, if needed</param>
		/// <returns></returns>
        string Translate(MultistringTag key, params object[] args);

	    object TranslateForPrinter(MultistringTag key);

        string CurrentLanguage { get; set; }

        string DefaultLanguage { get; set; }

        /// <summary>
        /// Gets or Sets Printing language
        /// </summary>
        string PrintingLanguage { get; set; }
	}
}
