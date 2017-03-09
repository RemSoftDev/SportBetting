using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Preferences.Services.Crypto;

namespace Preferences.Services.Preference
{
	public class TextFilePref : IPrefSupplier
	{
		private const string _Comment = "#";

		private const string _Separator = "=";

		private const string _DefaultCulture = "EN-US";

		private string name;

		private string fileName;

		private bool autoCommit;

		private Dictionary<string, string> data;

		private object lockObj;

		private SymCrypto crypt;

		public bool AutoCommit
		{
			get
			{
				bool flag = this.autoCommit;
				return flag;
			}
			set
			{
				this.autoCommit = value;
			}
		}

		public string Name
		{
			get
			{
				string str = this.name;
				return str;
			}
		}

		public TextFilePref(string prefName) : this(prefName, prefName, true, true)
		{
		}

		public TextFilePref(string prefName, string fileName) : this(prefName, fileName, true, true)
		{
		}

		public TextFilePref(string prefName, bool autoCommit) : this(prefName, prefName, autoCommit, true)
		{
		}

		public TextFilePref(string prefName, string fileName, bool autoCommit, bool useCrypto)
		{
			this.name = prefName;
			this.fileName = fileName;
			this.autoCommit = autoCommit;
			this.data = new Dictionary<string, string>();
			this.lockObj = new object();
			bool flag = !useCrypto;
			if (flag)
			{
				this.crypt = null;
			}
			else
			{
				this.crypt = new SymCrypto();
			}
			this.Init();
		}

		public void Clear()
		{
			this.data.Clear();
		}

		public void Commit()
		{
			lock (this.lockObj)
			{
				try
				{
					bool flag = this.crypt == null;
					if (flag)
					{
						StreamWriter streamWriter = new StreamWriter(this.fileName);
						try
						{
							Dictionary<string, string>.Enumerator enumerator = this.data.GetEnumerator();
							try
							{
								while (true)
								{
									flag = enumerator.MoveNext();
									if (!flag)
									{
										break;
									}
									KeyValuePair<string, string> current = enumerator.Current;
									streamWriter.WriteLine(string.Concat(current.Key, "=", current.Value));
								}
							}
							finally
							{
								enumerator.Dispose();
							}
						}
						finally
						{
							flag = streamWriter == null;
							if (!flag)
							{
								streamWriter.Dispose();
							}
						}
					}
					else
					{
						SymCrypto.SerializeCryptedObject(this.data, this.fileName, new BinaryFormatter(), this.crypt);
					}
				}
				catch (IOException oException1)
				{
					IOException oException = oException1;
					Console.WriteLine(string.Concat("Cannot read from file (", this.fileName, "): ", oException.Message));
				}
			}
		}

		public bool Contains(string key)
		{
			bool flag = this.data.ContainsKey(key);
			return flag;
		}

		public Dictionary<string, object> Copy()
		{
			Dictionary<string, object> strs;
			lock (this.lockObj)
			{
				strs = new Dictionary<string, object>(this.data.Count);
				Dictionary<string, string>.KeyCollection.Enumerator enumerator = this.data.Keys.GetEnumerator();
				try
				{
					while (true)
					{
						bool flag = enumerator.MoveNext();
						if (!flag)
						{
							break;
						}
						string current = enumerator.Current;
						strs[current] = this.data[current];
					}
				}
				finally
				{
					enumerator.Dispose();
				}
			}
			Dictionary<string, object> strs1 = strs;
			return strs1;
		}

		public int Count()
		{
			int count = this.data.Count;
			return count;
		}

		public void DisableCrypto()
		{
			this.crypt = null;
		}

		public void EnableCrypto()
		{
			this.crypt = new SymCrypto();
		}

		public Hashtable GetAll()
		{
			Hashtable hashtables = new Hashtable(this.data);
			return hashtables;
		}

		public List<PrefEntry> GetAllList()
		{
			List<PrefEntry> prefEntries;
			lock (this.lockObj)
			{
				prefEntries = new List<PrefEntry>(this.data.Count);
				Dictionary<string, string>.Enumerator enumerator = this.data.GetEnumerator();
				try
				{
					while (true)
					{
						bool flag = enumerator.MoveNext();
						if (!flag)
						{
							break;
						}
						KeyValuePair<string, string> current = enumerator.Current;
						prefEntries.Add(new PrefEntry(current.Key, current.Value));
					}
				}
				finally
				{
					enumerator.Dispose();
				}
			}
			List<PrefEntry> prefEntries1 = prefEntries;
			return prefEntries1;
		}

		public bool? GetBooleanEntry(string key)
		{
			string stringEntry = this.GetStringEntry(key);
			bool? nullable = null;
			bool flag = stringEntry == null;
			if (!flag)
			{
				try
				{
					nullable = new bool?(bool.Parse(stringEntry));
				}
				catch (SystemException systemException1)
				{
					SystemException systemException = systemException1;
					Console.WriteLine(string.Concat("Converting to int error:", systemException.Message));
				}
			}
			bool? nullable1 = nullable;
			return nullable1;
		}

		public double? GetDoubleEntry(string key)
		{
			bool flag;
			string stringEntry = this.GetStringEntry(key);
			double? nullable = null;
			CultureInfo cultureInfo = CultureInfo.GetCultureInfo("EN-US");
			bool flag1 = stringEntry == null;
			if (!flag1)
			{
				try
				{
					char[] chrArray = new char[2];
					chrArray[0] = ',';
					chrArray[1] = '.';
					int num = stringEntry.LastIndexOfAny(chrArray);
					if (num < 0)
					{
						flag = true;
					}
					else
					{
						flag = stringEntry[num] != ',';
					}
					flag1 = flag;
					if (!flag1)
					{
						cultureInfo = CultureInfo.GetCultureInfo("de-AT");
					}
					nullable = new double?(Convert.ToDouble(stringEntry, cultureInfo));
				}
				catch (SystemException systemException1)
				{
					SystemException systemException = systemException1;
					Console.WriteLine(string.Concat("Converting to int error:", systemException.Message));
				}
			}
			double? nullable1 = nullable;
			return nullable1;
		}

		public int? GetIntegerEntry(string key)
		{
			string stringEntry = this.GetStringEntry(key);
			int? nullable = null;
			bool flag = stringEntry == null;
			if (!flag)
			{
				try
				{
					nullable = new int?(int.Parse(stringEntry));
				}
				catch (SystemException systemException1)
				{
					SystemException systemException = systemException1;
					Console.WriteLine(string.Concat("Converting to int error:", systemException.Message));
				}
			}
			int? nullable1 = nullable;
			return nullable1;
		}

		public string GetStringEntry(string key)
		{
			string item;
			bool flag = !this.Contains(key);
			if (flag)
			{
				item = null;
			}
			else
			{
				item = this.data[key];
			}
			return item;
		}

		public void Init()
		{
			Dictionary<string, string> strs;
			bool flag;
			string[] strArrays = new string[1];
			strArrays[0] = "=";
			string[] strArrays1 = strArrays;
			lock (this.lockObj)
			{
				try
				{
					bool count = this.crypt == null;
					if (count)
					{
						StreamReader streamReader = new StreamReader(this.fileName);
						try
						{
							strs = new Dictionary<string, string>();
							while (true)
							{
								string str = streamReader.ReadLine();
								string str1 = str;
								count = str != null;
								if (!count)
								{
									break;
								}
								string[] strArrays2 = str1.Trim().Split(strArrays1, StringSplitOptions.RemoveEmptyEntries);
								if ((int)strArrays2.Length < 2)
								{
									flag = true;
								}
								else
								{
									flag = strArrays2[0].StartsWith("#");
								}
								count = flag;
								if (!count)
								{
									string str2 = strArrays2[1];
									count = !str2.Contains("#");
									if (!count)
									{
										str2 = strArrays2[1].Substring(0, strArrays2[1].IndexOf("#") + 1);
									}
									count = strs.ContainsKey(strArrays2[0]);
									if (count)
									{
										this.fileName = "DUPLICATE";
										this.name = "DUPLICATE";
										return;
									}
									strs.Add(strArrays2[0].Trim(), str2.Trim());
								}
							}
							count = strs.Count <= 0;
							if (!count)
							{
								this.data = strs;
							}
						}
						finally
						{
							count = streamReader == null;
							if (!count)
							{
								streamReader.Dispose();
							}
						}
					}
					else
					{
						BinaryFormatter binaryFormatter = new BinaryFormatter();
						strs = SymCrypto.DeserializeCryptedObject(this.fileName, binaryFormatter, this.crypt) as Dictionary<string, string>;
						count = strs == null;
						if (!count)
						{
							Console.WriteLine("success");
							this.data = strs;
						}
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					Console.WriteLine(string.Concat("Exception reading from file (", this.fileName, "): ", exception.Message));
					this.fileName = "ERROR";
					this.name = "ERROR";
				}
			}
		}

		public bool IsCrypto()
		{
			bool flag = this.crypt != null;
			return flag;
		}

		public bool Remove(string key)
		{
			bool flag = this.data.Remove(key);
			return flag;
		}

		public void SetBooleanEntry(string key, bool value)
		{
			this.SetStringEntry(key, value.ToString());
		}

		public void SetDoubleEntry(string key, double value)
		{
			this.SetStringEntry(key, Convert.ToString(value, CultureInfo.GetCultureInfo("EN-US")));
		}

		public void SetIntegerEntry(string key, int value)
		{
			this.SetStringEntry(key, value.ToString());
		}

		public void SetStringEntry(string key, string value)
		{
			bool flag;
			bool flag1;
			lock (this.lockObj)
			{
				bool flag2 = !this.data.ContainsKey(key);
				if (flag2)
				{
					this.data.Add(key, value);
					flag = true;
				}
				else
				{
					bool flag3 = this.data[key].CompareTo(value) != 0;
					flag = flag3;
					flag2 = !flag3;
					if (!flag2)
					{
						this.data[key] = value;
					}
				}
				if (!this.autoCommit)
				{
					flag1 = true;
				}
				else
				{
					flag1 = !flag;
				}
				flag2 = flag1;
				if (!flag2)
				{
					this.Commit();
				}
			}
		}
	}
}