using System;
using System.Collections.Generic;
using System.Text;

namespace Nbt.Services.Key {
	//public delegate void CashInEventHandler (object sender, CashInEventArgs arg);
	public class KeyEventArgs : EventArgs {
	
		public int KeyID = -1;
		public bool Pressed = false;

			public KeyEventArgs(int keyId, bool pressed) {
				KeyID = keyId;		
				Pressed = pressed;
			}

			

		

	}
}
