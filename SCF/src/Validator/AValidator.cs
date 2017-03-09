using System;
using System.Collections.Generic;
using System.Text;
using Preferences.Services.Preference;

namespace Nbt.Services.Scf.CashIn.Validator {

	public abstract class AValidator : IValidator {
		
		//public virtual event CashInEventHandler CashIn;
		public virtual event EventHandler<CashInEventArgs> CashIn;
        public virtual event EventHandler<ValidatorEventArgs<string>> CashLimitExceededEventHandler = null;

        //protected static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(AValidator).Name);
        public string _COMPORTSTR = "CCTALK_ComPort";

        public bool ValFound;
		public AValidator() {			
			//log4net.Config.BasicConfigurator.Configure();
						
			//log4net.Layout.PatternLayout layout = new log4net.Layout.PatternLayout("%date [%thread] %-5level %class - %message%newline");
			/*log4net.Layout.PatternLayout layout = new log4net.Layout.PatternLayout("%date %-5level %line - %message%newline");
			log4net.Appender.ConsoleAppender cca = new log4net.Appender.ConsoleAppender();
			cca.Layout = layout;			
			log4net.Config.BasicConfigurator.Configure(cca);
			*/
			//log.Logger.IsEnabledFor(log4net.Core.Level.Emergency);
			
		}

		#region IValidator Member

		public virtual bool Enable(decimal maxCredit) {
			return false;
		}

		public virtual bool Disable() {
			return false;
		}

		public virtual bool SetEnabledChannels(decimal actCredit) {
			return false;
		}

		public virtual decimal GetCredit() {
			return 0;
		}

		public virtual bool ResetCredit() {
			return false;
		}

		public virtual bool IsEnabled() {
			return false;
		}

	    public virtual bool IsDataSetValid ()
	    {
	        return false;
	    }
        public virtual void SetCredit(decimal credit1)
        {
        }

		protected virtual void LoadSettings(IPrefSupplier pref, string key){			
		}

		protected virtual void OnCashIn(CashInEventArgs e) {
			EventHandler<CashInEventArgs> tmpCI = CashIn;	//for thread safety
			if (tmpCI!=null)
				tmpCI(this, e);
		}

        protected virtual void OnCashLimitExceeded (ValidatorEventArgs<string> e)
        {
            EventHandler<ValidatorEventArgs<string>> tmpCLE = CashLimitExceededEventHandler;	//for thread safety
            if (tmpCLE != null)
            {
                tmpCLE (this, e);
            }
        }

        protected virtual void OnCashInAfterDisable(CashInEventArgs e)
        {
            EventHandler<CashInEventArgs> tmpCI = CashIn;	//for thread safety
            if (tmpCI != null)
                tmpCI(this, e);
        }

        public virtual List <DeviceInfo> GetDeviceInventory()
        {         
            return null;
        }

        public virtual List<DeviceInfo> GetShortDeviceInventory()
        {
            return null;
        }

        public virtual bool CheckBillValidator()
        {
            return false;
        }

        public virtual bool CheckCoinAcceptor ()
        {
            return false;
        }

		#endregion
	}
}
