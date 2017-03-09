using System;
using System.Collections.Generic;
using System.Text;
using Preferences.Services.Preference;

namespace Nbt.Services.Scf.CashIn.Validator.InnovativeTechnology {
	class NV10 : NVNoteValidator {

		public NV10(IPrefSupplier pref, string prefKey) : base(pref, prefKey, NVNoteValidator.VALIDATOR_NV10_NAME, CashInSettings.Default.NV10_ChannelCount) { }
	}
}
