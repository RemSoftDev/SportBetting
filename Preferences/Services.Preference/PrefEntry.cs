namespace Preferences.Services.Preference
{
	public class PrefEntry
	{
		private string key;

		private string val;

		public string Key
		{
			get
			{
				string str = this.key;
				return str;
			}
			set
			{
				bool flag;
				if (value == null)
				{
					flag = true;
				}
				else
				{
					flag = !(value != string.Empty);
				}
				bool flag1 = flag;
				if (!flag1)
				{
					this.key = value;
				}
			}
		}

		public string Value
		{
			get
			{
				string str = this.val;
				return str;
			}
			set
			{
				bool flag;
				if (value == null)
				{
					flag = true;
				}
				else
				{
					flag = !(value != string.Empty);
				}
				bool flag1 = flag;
				if (!flag1)
				{
					this.val = value;
				}
			}
		}

		public PrefEntry(string key, string value)
		{
			this.key = key;
			this.val = value;
		}

		public override string ToString()
		{
			string str = string.Concat("Key=", this.key, " Value=", this.val);
			return str;
		}
	}
}