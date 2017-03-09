using System;
using System.Collections.Generic;
using System.Text;
using Nbt.Services.Key;
using System.Threading;
using System.Diagnostics;

namespace Nbt.Services.Serios {
	public class SeriosMK1Keys : AKey {
		private readonly int KEY_ACTION_DELAY = 1000; // disables firing multiple key events for this delay
		private readonly int POLL_DELAY = 100;

        //private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(SeriosMK1Keys).Name);


		public override event EventHandler<KeyEventArgs> KeyEvent;
		//public override event KeyDelegate KeyEvent;

		private Thread _pollAcceptor;

		public SeriosMK1Keys() {
            //_pollAcceptor = new Thread(new ThreadStart(pollInput));
            //_pollAcceptor.Start();
            //_logger.Debug("Serios KEYs reading enabled");
			//Console.WriteLine("DEBUG SeriosMK1KeySwitch initialised");
		}

		/// <summary>
		/// Polls for KeySwitch Events
		/// </summary>
        //private void pollInput() {
        //    byte[] input = new byte[9];
        //    byte _input = 0x00;
        //    while (true) {
        //        if (SeriosMK1Wrapper.Instance.ReadInput(out input)) {
        //            _input = (byte) (input[0] ^ 0xFF); //invert all bits for easier handling
        //            if ((_input & 0x01) > 0x00) { // 00000001 == keyswitch1 activated
        //                OnKeyEvent(new KeyEventArgs(KeyConstants.KEY_SWITCH_1, true));
        //            } else {
        //                if ((_input & 0x02) > 0x00) { // 00000010 == keyswitch2 activated
        //                    OnKeyEvent(new KeyEventArgs(KeyConstants.KEY_SWITCH_2, true));
        //                }
        //            }
        //            //todo: add code here for other keys

        //        } else {
        //            //_logger.Error("SeriosMK1KeySwitch not working properly!");
        //        }
        //        Thread.Sleep(POLL_DELAY);
        //    }

        //}

		protected virtual void OnKeyEvent(KeyEventArgs e) {
			EventHandler<KeyEventArgs> tmpCI = KeyEvent;	//for thread safety
			//KeyDelegate tmpCI = KeyEvent;	//for thread safety
			if (tmpCI!=null)
				tmpCI(this, e);
			Debug.WriteLine(" KeySwitch " + e.KeyID + " activated");
			Thread.Sleep(KEY_ACTION_DELAY);
		}



	}
}
