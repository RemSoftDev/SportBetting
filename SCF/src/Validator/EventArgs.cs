using System;
using System.Collections.Generic;
using System.Text;

namespace Nbt.Services.Scf.CashIn.Validator {
	
	//public delegate void CashInEventHandler (object sender, CashInEventArgs arg);

	public class CashInEventArgs : EventArgs 
    {
		public decimal Credit = 0;
		public decimal MoneyIn = 0;
		public bool IsCoin;
		

		public CashInEventArgs(decimal cashInVal,bool isCoin): this(cashInVal, 0, isCoin) {}					
		
		public CashInEventArgs(decimal cashInVal, decimal crdt,bool isCoin) 
        {
			MoneyIn = cashInVal;
			Credit = crdt;
		    IsCoin = isCoin;

		}

	}

    public class ValidatorEventArgs <T> : EventArgs
    {
        public ValidatorEventArgs (T value)
        {
            Value = value;
        }

        public T Value { get; private set; }
    }
}
