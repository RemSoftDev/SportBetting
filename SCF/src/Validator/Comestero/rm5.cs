using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using NLogger;
using Nbt.Services.Scf.CashIn.Validator;
using Nbt.Services.Serios;
using Preferences.Services.Preference;

namespace Nbt.Services.Scf.CashIn.Validator.Comestero {
	class RM5 : AValidator {
		private readonly int CHANNEL_COUNT = CashInSettings.Default.RM5_ChannelCount; //RM5 has 6 real channels
		private readonly int POLL_DELAY = CashInSettings.Default.RM5_DelayPoll; //milliseconds
		private readonly int DELAY_AFTER_INPUT = CashInSettings.Default.RM5_WaitAfterInput; //milliseconds
		
		private bool noOverpay = false;
		private bool enabled = false;
		private decimal[] channelValues;
		private decimal credit = 0;
		private decimal maxCredit=0;

		private Queue<byte> cashInQueue = new Queue<byte>();
		private Thread pollRM5;
		private Thread cashInWorkThread;

		private object enableLock = new object();
		private object pollLock = new object();
		private object cashInQueueLock = new object();
		private object handleCashLock = new object();
		

		public RM5(IPrefSupplier pref, string prefKey) {
			channelValues = new decimal[CHANNEL_COUNT];
			LoadSettings(pref, prefKey);
			
			Disable();

			pollRM5 = new Thread(new ThreadStart(poll4CoinInput));
			pollRM5.IsBackground = true;
			pollRM5.Start();
			

			cashInWorkThread = new Thread(new ThreadStart(consumeCoinInput));
			cashInWorkThread.IsBackground = true;
			cashInWorkThread.Start();
            ValFound = true;
		}

		public override string ToString() {
			//return base.ToString();
			return "RM5";
		}

		#region AValidator Member
		public override bool Enable(decimal maxEnable) {
			lock (enableLock) {
                base.Enable(maxEnable);
                maxCredit = maxEnable < 0 ? 0 : maxEnable;
                setChannels(0);
                enabled = true;
                setChannels(credit);
            }
			return enabled;
		}
		
		

		public override bool Disable() {
            lock (enableLock) {
                base.Disable();
                disableCoinValidator();
                enabled = false;
            }
			return enabled;
		}

		public override bool SetEnabledChannels(decimal actCredit) {
            lock (enableLock) {
                base.SetEnabledChannels(actCredit);
                setChannels(actCredit);
            }
			return enabled;
		}

		public override decimal GetCredit() {
			return credit;
		}

        public override void SetCredit(decimal credit)
        {
            base.SetCredit(credit);
            this.credit = credit;
        }

		public override bool ResetCredit() {
			credit=0;
			return true;
		}

		public override bool IsEnabled() {
			return enabled;
		}

		#endregion

		
		
		
		private decimal MaximumChannelValue {
			get {
				decimal max = 0;
				for (int i = 0; i < CHANNEL_COUNT; i++) {
					if (channelValues[i] > max) {
						max = channelValues[i];
					}
				}
				return max;
			}
		}

		private decimal MinimumChannelValue {
			get {
				decimal min = channelValues[0];
				for (int i = 1; i < CHANNEL_COUNT; i++) {
					if (channelValues[i] < min && channelValues[i] > 0) {
						min = channelValues[i];
					}
				}
				return min;
			}
		}



		private void setChannels(decimal curCredit) {
			bool enabled;
			if (noOverpay) { //turn off if the maximum coin + actual credit  would exceed MaxLimit
				enabled = (maxCredit == 0) || ((curCredit + MaximumChannelValue) <= maxCredit);
                Log.Debug("noOverpay(true): maxCredit = " + maxCredit + "; curCredit = " + curCredit + "; MaximumChannelValue = " + MaximumChannelValue + "; Enabled = " + enabled);
			} else { //turn off if the minimum coin would go over MaxLimit
				enabled = (maxCredit == 0) || ((curCredit + MinimumChannelValue) <= maxCredit);
                Log.Debug("noOverpay(false): maxCredit = " + maxCredit + "; curCredit = " + curCredit + "; MinimumChannelValue = " + MinimumChannelValue + "; Enabled = " + enabled);

			}
			SeriosMK1Wrapper.Instance.WriteOutput((byte)(enabled ? 0x80 : 0x00)); //0x80 == enabled | 0x00 == disabled
		}

		private void disableCoinValidator() {
			SeriosMK1Wrapper.Instance.WriteOutput((byte) (0x00)); //0x00 == disabled
		}

		/// <summary>
		/// Producer of consumer producer pattern (writes cashInQueue)
		/// </summary>
		private void poll4CoinInput() {
			byte[] input ;
			while (true) {
				if (SeriosMK1Wrapper.Instance.ReadInput(out input)) {
					if (((input[1] ^ 0xFF)& 0x3F) > 0x00) { 
						lock (cashInQueueLock) {
							cashInQueue.Enqueue(input[1]);
						}
					
						Thread.Sleep(DELAY_AFTER_INPUT);
					}
				} else {					
					Console.WriteLine("RM5 read error");
				}
				Thread.Sleep(POLL_DELAY);
			}
		}


		/// <summary>
		/// consumer (reads cashInQueue)
		/// </summary>
		private void consumeCoinInput() {
			byte b;
			byte bitmask = 0x01;
			while (true) {
				if (cashInQueue.Count > 0) {
					lock (cashInQueueLock) {
						b = (byte)cashInQueue.Dequeue();
					}
					for (int i = 0; i < CHANNEL_COUNT; i++) 
						if ((b^0xFF) == (bitmask << i))  
							lock (handleCashLock) {
								credit += channelValues[i];
								OnCashIn(new CashInEventArgs(channelValues[i],true));							
								//Console.WriteLine("Coin [" + channelValues[i] + "] accepted!");
							}												
				}
				Thread.Sleep(10);
			}
		}

		protected override void LoadSettings(IPrefSupplier pref, string prefKey) {
			//from prefs
            //int multiplicator = pref.GetIntegerEntry(prefKey + CashInSettings.Default.PrefValidatorMultiplicator) ?? CashInSettings.Default.ValidatorMultiplicator;

            //if (multiplicator == 0)  //0 would cause error
			int multiplicator = 100;
	
			noOverpay = pref.GetBooleanEntry(prefKey + CashInSettings.Default.PrefValidatorNoOverpay) ?? CashInSettings.Default.ValidatorNoOverpay;

			for (int i = 0; channelValues!=null && i < channelValues.Length; i++) {
				channelValues[i] = (decimal) ((pref.GetDoubleEntry(prefKey+CashInSettings.Default.PrefValidatorChannel+String.Format("{0:##00}", i+1)) ?? 0)  /  multiplicator) ;
                Log.Info("RM5 Channel " + (i + 1) + ": [" + (channelValues[i] == 0 ? "not set" : channelValues[i].ToString()) + "]");
			}

            Log.Info("RM5 No Overpay == " + noOverpay);
            Log.Info("RM5 Multiplicator == " + multiplicator);
		}


	}
}
