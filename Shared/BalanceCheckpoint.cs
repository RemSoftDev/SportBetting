using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class BalanceCheckpoint
    {
        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime CreationTime { get; set; }

        public string Operator { get; set; }

        public decimal Payin { get; set; }

        public decimal Payout { get; set; }

        public decimal Credit { get; set; }
    }
}
