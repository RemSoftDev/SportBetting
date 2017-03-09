namespace TranslationByMarkupExtension
{
	public class MultistringTag
	{
        private MultistringTag()
        {
        }
		public string Value { get; private set; }
		public static MultistringTag Assign(string tag, string defaultvalue)
		{
			return new MultistringTag(){Value = tag, Default = defaultvalue};
		}

	    public string Default { get; set; }
	}
}
