using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO.Ports;
using NLogger;
using Nbt.Services.Scf.CashIn.Validator.CCTalk;
using Preferences.Services.Preference;
using Nbt.Services.Scf.CashIn.Validator;
using Nbt.Services.SerialDevice;

namespace Nbt.Services.Scf.CashIn.Validator.InnovativeTechnology {


	public partial class NVNoteValidator:AValidator {
		
		public const byte INHIBIT_BASE = 130;
		public const byte UNINHIBIT_BASE = 150;

		//public const byte NOTE_NOT_RECOGNISED = 20;
		//public const byte MECHANISM_RUNNING_SLOW = 30;
		//public const byte STRIMMING_ATTEMPTED = 40;
		//public const byte STACKER_ERROR = 60;

		public const byte VALIDATOR_BUSY = 120;
		public const byte VALIDATOR_NOT_BUSY = 121;
		public const byte COMMAND_ERROR = 255;

		//public const byte STATUS = 182;
		public const byte ENABLE_ALL = 184;
		public const byte DISABLE_ALL = 185;

		public const string VALIDATOR_NV10_NAME = "NV10";


		protected int writeDelay;        
        protected decimal[] channelValues;

		private decimal maxCredit=0;
        private decimal credit;
        private bool enabled;
        private bool busy = false;
		private SerialPort serialConnection;
        private string portName;
        private int baudRate ;
        private int dataBits;
        private StopBits stopBits ;
		Parity parity ;					
		private object enableLock = new object();
		private string validatorName = string.Empty;
        

        # region Konstruktoren

        public NVNoteValidator(IPrefSupplier pref, string prefKey, string name, int channelCount) {            
            channelValues = new decimal[channelCount];
			validatorName = name;


            serialConnection = SerialDevice.SerialPortManager.Instance(CCTCommunication.GetVirtualComPort(), baudRate, dataBits, stopBits, parity);

				try
				{
                    if (serialConnection != null && !serialConnection.IsOpen)
                        serialConnection.Open();

                    serialConnection.ErrorReceived += new SerialErrorReceivedEventHandler(Port_ErrorReceived);
                    serialConnection.DataReceived += new SerialDataReceivedEventHandler(handleSerialDataReceived);
					ValFound = true;
					Disable();

				}
				catch (Exception e)
				{
                    Log.Error("serial connect", e);
					System.Diagnostics.Debug.WriteLine(e.StackTrace);
                    serialConnection = null;
				}		
        	
				           
        }

        # endregion

		public override string ToString() {
			if (validatorName == null || validatorName == string.Empty)
				return base.ToString();
			return validatorName;
		}


		#region AValidator Member

		public override bool Enable(decimal maxEnable) {
            lock (enableLock) {
				base.Enable(maxEnable);
				maxCredit = maxEnable < 0 ? 0 : maxEnable;
				if (writeCommand(NVNoteValidator.ENABLE_ALL)) {
                    setChannels(0);
                    enabled = true;
                    setChannels(credit);
                }
                waitWhileBusy();
            }
			return true;
        }

        public override bool Disable() {
            lock (enableLock) {
                base.Disable();
				if (writeCommand(NVNoteValidator.DISABLE_ALL)) {
                    enabled = false;
                }
                waitWhileBusy();
            }
			return true;
        }

        public override bool SetEnabledChannels(decimal actCredit) {
            lock (enableLock) {
                base.SetEnabledChannels(actCredit);
                setChannels(actCredit);
                waitWhileBusy();
            }
			return true;
        }


        public override decimal GetCredit() {
            base.GetCredit();
            waitWhileBusy();
            return credit;
        }

        public override void SetCredit(decimal credit)
        {
            base.SetCredit(credit);
            waitWhileBusy();
            this.credit = credit;
        }

        public override bool ResetCredit() {
            base.ResetCredit();
            waitWhileBusy();
            credit = 0;
            return true;

        }

        public override bool IsEnabled() {
            lock (enableLock) {
                base.IsEnabled();
                Log.Info("IsEnabled = " + enabled);
                return enabled;
            }
		}
		#endregion 

		protected override void LoadSettings(IPrefSupplier pref, string prefKey) {
			base.LoadSettings(pref, prefKey);			
			
			writeDelay = pref.GetIntegerEntry(prefKey+CashInSettings.Default.PrefValidatorWriteDelay) ?? 120 ;
			baudRate = pref.GetIntegerEntry(prefKey+CashInSettings.Default.PrefValidatorBaud) ?? CashInSettings.Default.ValidatorBaudrate ;
			dataBits = pref.GetIntegerEntry(prefKey+CashInSettings.Default.PrefValidatorDatabits) ?? CashInSettings.Default.ValidatorDatabits ;
			
			decimal? _stopBits = (decimal) pref.GetDoubleEntry(prefKey+CashInSettings.Default.PrefValidatorStopbits);
			if (_stopBits == null)
				stopBits = CashInSettings.Default.ValidatorStopbits;
			else if (_stopBits < 1)
				stopBits = StopBits.None;
			else if (_stopBits  < 1.5m)
				stopBits = StopBits.One;
			else if (_stopBits < 2)
				stopBits = StopBits.OnePointFive;
			else 
				stopBits = StopBits.Two;

			int? _parity = pref.GetIntegerEntry(prefKey+CashInSettings.Default.PrefValidatorParity);
			if (_parity == null)
				parity = CashInSettings.Default.ValidatorParity;
			else if (_parity == 1)
				parity = Parity.Odd;
			else if (_parity == 2)
				parity = Parity.Even;
			else
				parity = Parity.None;			

			for (int i = 0; channelValues!=null && i < channelValues.Length; i++) {
				string mykey =prefKey+CashInSettings.Default.PrefValidatorChannel+String.Format("{0:##00}", i+1);
				channelValues[i] = (decimal) (pref.GetDoubleEntry(mykey) ?? 0);
				//Console.WriteLine("NV10 Channel " + (i + 1) + ": [" + (channelValues[i] == 0 ? "not set" : channelValues[i].ToString()) + "]");
                Log.Debug("NV10 Channel " + (i + 1) + ": [" + (channelValues[i] == 0 ? "not set" : channelValues[i].ToString()) + "]");
			}			
		}	



        # region private Methoden

        private void setChannels(decimal actCredit) {
            bool channelEnable;
			writeCommand(NVNoteValidator.DISABLE_ALL);
            for (int i = 0; i < channelValues.Length; i++) {
				channelEnable = ((maxCredit == 0) || (actCredit + channelValues[i] <= maxCredit)) && (channelValues[i] != 0);
				writeCommand(channelEnable ? ((byte)(NVNoteValidator.UNINHIBIT_BASE + i + 1)) : ((byte)(NVNoteValidator.INHIBIT_BASE + i + 1)));
            }
			writeCommand(NVNoteValidator.ENABLE_ALL);
        }


		/// <summary>
		/// handles events from SerialPort.ErrorReceived of a SerialPort object     
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.IO.Ports.SerialErrorReceivedEventArgs"/> instance containing the event data.</param>
		private static void Port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e) {
            Log.Error("Port Error. TODO: handle Error");
		}

		private void handleSerialDataReceived(object sender, SerialDataReceivedEventArgs data) {
			if (data.EventType == SerialData.Eof)
                Log.Error("serial EOF received" + data.EventType);
			if (sender != null)
				handleReceivedCommand((byte)((SerialPort)sender).ReadByte());
		}

		private void handleReceivedCommand(byte b) {
			switch (b) {
				case NVNoteValidator.VALIDATOR_BUSY:
					this.busy = true;
					break;
				case NVNoteValidator.VALIDATOR_NOT_BUSY:
					this.busy = false;
					break;
				case NVNoteValidator.COMMAND_ERROR:
					break;
				default:					// channel signal	
					if (channelValues!=null && b>=1 && b<= channelValues.Length) {
						credit += channelValues[b-1];
						OnCashIn(new CashInEventArgs(channelValues[b - 1],false));						
					}
					break;
			}
		}
		
		private bool writeCommand(byte command) {
			if (serialConnection == null)
				return false;

				try
				{
					serialConnection.Write((new byte[1] { command }), 0, 1);
					if (writeDelay > 0)
						Thread.Sleep(writeDelay);
				}
				catch (Exception)
				{
				}
			
			return true;
		}



        private void waitWhileBusy() { 
            while (this.busy) {
                Thread.Sleep(100);
            }
        }
		


		#endregion
	}
}
