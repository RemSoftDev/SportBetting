using System.Collections;
using System.Collections.Generic;

namespace Preferences.Services.Preference
{
	public interface IPrefSupplier
	{
		bool AutoCommit
		{
			get;
			set;
		}

		string Name
		{
			get;
		}

		void Clear();

		void Commit();

		bool Contains(string key);

		Dictionary<string, object> Copy();

		int Count();

		void DisableCrypto();

		void EnableCrypto();

		Hashtable GetAll();

		List<PrefEntry> GetAllList();

		bool? GetBooleanEntry(string key);

		double? GetDoubleEntry(string key);

		int? GetIntegerEntry(string key);

		string GetStringEntry(string key);

		void Init();

		bool IsCrypto();

		bool Remove(string key);

		void SetBooleanEntry(string key, bool value);

		void SetDoubleEntry(string key, double value);

		void SetIntegerEntry(string key, int value);

		void SetStringEntry(string key, string value);
	}
}