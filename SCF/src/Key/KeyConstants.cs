using System;

namespace Nbt.Services.Key {
	
	/// <summary>
	/// Sammlung von Konstanten für die Implementierung von Schaltern bzw. Schlüsselschaltern.
	/// </summary>
	
	public class KeyConstants {
		public const int KEY_SWITCH_1 = 1;
		public const int KEY_SWITCH_2 = 2;

		public static string getKeyDescription(int keyID) {
			switch(keyID) {
				case KEY_SWITCH_1: return "KEY_SWITCH_1";
				case KEY_SWITCH_2: return "KEY_SWITCH_2";
			}
			return "UNDEFINED_SWITCH";
		}
	}
}
