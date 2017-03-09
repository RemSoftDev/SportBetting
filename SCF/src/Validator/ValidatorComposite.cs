using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using NLogger;
using Preferences.Services.Preference;
//using NBT.Services.Scf.CashIn;

namespace Nbt.Services.Scf.CashIn.Validator {

	public class ValidatorComposite : AValidator {

        private List<AValidator> validators = new List<AValidator>();

        public List<AValidator> Validators
        {
            get { return validators; }
        }

        //static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(ValidatorComposite).Name);

		//public event CashInEventHandler CashInComposite;
		
		//public override event CashInEventHandler CashIn;				
		public override event EventHandler<CashInEventArgs> CashIn;
        public override event EventHandler<ValidatorEventArgs<string>> CashLimitExceededEventHandler = null;

		private decimal credit = 0;
		private object creditLock = new object();
		private decimal maxCredit = 0;

        private bool CheckTabTerminalHardware (IPrefSupplier pref, string mode)
        {
            bool result = false;

            try
            {
                if (TabTerminal.TabTerminal.CheckTabBoard())
                {
                    result = true;
                    Type t = Type.GetType ("Nbt.Services.Scf.CashIn.Validator.TabTerminal.TabTerminal");
                    if (t != null)
                    {
                        AValidator v = (AValidator)Activator.CreateInstance(t, new object[] {pref, "Validator", mode });
                        if (v != null)
                        {
                            ValFound = v.ValFound;
                            validators.Add (v);
                        }
                    }
                }

            }
            catch 
            {
            }

            return result;
        }
       

        public ValidatorComposite(IPrefSupplier pref, string mode)
        {
			
			string validatorKey="Validator";
			string validator;
		    ValFound = false;
		    bool validatorFound = false;

            
            if (pref != null && pref.Count() > 0)
            {
                if (CheckTabTerminalHardware (pref, mode))
                {

                }
                else
                {
                    for (int i = 1; i < CashInSettings.Default.NumOfValidators; i++)
                    {
                        if ((validator = pref.GetStringEntry(validatorKey + i + "_Type")) != null)
                        {
                            validatorFound = true;
                            try
                            {
                                Type t = Type.GetType(this.GetType().Namespace + '.' + validator);
                                if (t != null)
                                {
                                    AValidator v = (AValidator)Activator.CreateInstance(t, new object[] { pref, validatorKey + i, mode });
                                    if (!ValFound && v != null)
                                        ValFound = v.ValFound;
                                    validators.Add(v);
                                }
                                else
                                {
                                    Log.Error("Could not create an instance of class:" + this.GetType().Namespace + '.' + validator);
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Error("In ValidatorComposite.", e);
                            } 
                        }
                        else if (!validatorFound)
                        {
                            ValFound = true;
                        } 
                    }
                }

                foreach (AValidator v in validators)
                {
                    v.CashIn += this.Refresh;
                    v.CashLimitExceededEventHandler += this.OnCashLimitExceeded;
                    Log.Debug("validator:" + v.ToString() + ",credit=" + v.GetCredit() + ",enable=" + v.IsEnabled());
                }
            }
		
		}


		public bool AddValidator(AValidator validator) {
			validators.Add(validator);
			return true;
		}
		
		public bool RemoveValidator(AValidator validator) {
			return validators.Remove(validator);			
		}

		public int Count() {
			return validators.Count;
		}


		#region IValidator Member

		public override bool Enable(decimal maxCredit) {
			this.maxCredit = (maxCredit>=0) ? maxCredit: 0;
			if (validators == null)
				return false;

            foreach (AValidator v in validators)
            {
                if (v.ValFound)
                {
                    v.SetCredit(this.credit);
                    v.Enable(this.maxCredit);
                }
            }

		    return true;
		}

		public override bool Disable() {
			if (validators == null)
				return false;

			foreach (AValidator v in validators)
                if (v.ValFound)
                {
                    v.Disable();
                }
			
			return true;			
		}

		public override bool SetEnabledChannels(decimal actCredit) {
			if (validators == null)
				return false;

			foreach (AValidator v in validators)
                if (v.ValFound)
                {
                    v.SetEnabledChannels(actCredit);
                }

			return true;
			
		}

		public override decimal GetCredit() {
			/*decimal act_credit = 0;
			if (validators == null)
				return 0.0;

			foreach (AValidator v in validators)
				act_credit += v.GetCredit();

			return act_credit;		*/
			return credit;
		}

        public override void SetCredit(decimal credit)
        {
            this.credit = credit;
        }

		public override bool ResetCredit() {
			if (validators == null)
				return false;

			foreach (AValidator v in validators)
				v.ResetCredit();

			maxCredit = 0;
			credit = 0;

			return true;
		}

		public override bool IsEnabled() {			
			if (validators == null)
				return false;

			foreach (AValidator v in validators)
				if (v.IsEnabled())
					return true;
			return false;
		}

        public override bool IsDataSetValid ()
        {
            if (validators == null)
                return false;

            foreach (AValidator v in validators)
            {
                string validator_type = v.ToString();

                if (v.IsDataSetValid ())
                {
                    return true;
                }
            }
            return false;
        }

        public override List<DeviceInfo> GetDeviceInventory()
        {
            if (validators == null)
            {
                return null;
            }

            List<DeviceInfo> list = new List<DeviceInfo>();

            foreach (AValidator v in validators)
            {
                if (v.ValFound)
                {
                    List<DeviceInfo> dev_info = v.GetDeviceInventory();
                    if (dev_info != null)
                    {
                        list.AddRange (dev_info);
                    }
                }
            }
            return list;
        }
        public override List<DeviceInfo> GetShortDeviceInventory()
        {
            if (validators == null)
            {
                return null;
            }

            List<DeviceInfo> list = new List<DeviceInfo>();

            foreach (AValidator v in validators)
            {
                if (v.ValFound)
                {
                    List<DeviceInfo> dev_info = v.GetShortDeviceInventory();
                    if (dev_info != null)
                    {
                        list.AddRange(dev_info);
                    }
                }
            }
            return list;
        } 

        public override bool CheckBillValidator ()
        {
            bool result = false;

            if (validators != null)
            {
                foreach (AValidator v in validators)
                {
                    if (v.ValFound)
                    {
                        result |= v.CheckBillValidator();
                    }
                }
            }

            return result;
        }

        public override bool CheckCoinAcceptor ()
        {
            bool result = false;

            if (validators != null)
            {
                foreach (AValidator v in validators)
                {
                    if (v.ValFound)
                    {
                        result |= v.CheckCoinAcceptor();
                    }
                }
            }
            return result;
        }
        
		#endregion

		//[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
		private void Refresh(object sender, CashInEventArgs e) {						
			EventHandler<CashInEventArgs> tmpCI = CashIn;	//for thread safety
				if (sender is InnovativeTechnology.NVNoteValidator)
					Log.Debug("typ is NV");
				if (sender is Comestero.RM5)
                    Log.Debug("typ is RM5");
				/*if (sender.GetType() == typeof(InnovativeTechnology.NVNoteValidator))
					Console.WriteLine("typ is NV");*/
				if (e.MoneyIn>0) {
					lock (creditLock) {
						this.credit += e.MoneyIn;
											
						if (tmpCI != null)		//fire event (to CashInManager)
							tmpCI(sender, new CashInEventArgs(e.MoneyIn, this.credit,e.IsCoin));

						if (maxCredit>0)
							SetEnabledChannels(this.credit);

					}
				}
			
		}


        private void OnCashLimitExceeded (object sender, ValidatorEventArgs<string>e)
        {
            EventHandler<ValidatorEventArgs<string>> handler = CashLimitExceededEventHandler;
            if (handler != null)
            {
                handler (sender, e);
            }
        }
		/*public static void ValidatorTest() {
			//Comestero.RM5CoinValidator rm5=new Comestero.RM5CoinValidator();
			InnovativeTechnology.NVNoteValidator nv10 = new InnovativeTechnology.NVNoteValidator(InnovativeTechnology.NVDefines.VALIDATOR_NV10_NAME, 8);
			nv10.CashIn += Handler;
			nv10.Enable(0);
		//	nv10.Disable();
			while (true) {
			}
		

		}*/

	}
}
