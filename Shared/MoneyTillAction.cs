using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shared
{
    public enum MoneyTillActionType
    {
        CashIn,
        CashOut
    }

    public class MoneyTillAction
    {
        public Shared.Operator Operator { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateTime { get; set; }
        public MoneyTillActionType Type { get; set; }
    }
}