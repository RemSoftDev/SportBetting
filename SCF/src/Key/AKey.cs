using System;

namespace Nbt.Services.Key {

	public interface IKey {

	}

	public abstract class AKey : IKey {
		public abstract event EventHandler<KeyEventArgs> KeyEvent;
	}


}
