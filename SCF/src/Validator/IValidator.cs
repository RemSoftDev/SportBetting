using System;
using System.Collections.Generic;
using System.Text;
using Nbt.Services.Scf.CashIn.Validator.CCTalk;

namespace Nbt.Services.Scf.CashIn.Validator {
	
	public interface IValidator {
		bool Enable(decimal maxCredit);
		bool Disable();
		bool SetEnabledChannels(decimal actCredit);
		decimal GetCredit();
	    void SetCredit(decimal credit);
		bool ResetCredit();
		bool IsEnabled();
	    bool IsDataSetValid();
        List <DeviceInfo> GetDeviceInventory();
        List<DeviceInfo> GetShortDeviceInventory();
        bool CheckBillValidator ();
        bool CheckCoinAcceptor ();

	}

}
