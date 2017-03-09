using System;

namespace SportBetting.WPF.Prism.Shared.Models
{
    public class BalanceCheckpoint
    {
        public int Index { get; set; }
        public decimal Payin { get; set; }
        public decimal Payout { get; set; }
        public decimal Credit { get; set; }
        public string Operator { get; set; }
        public DateTime CreationTime { get; set; }
        public BalanceCheckpoint()
        {
            CreationTime = DateTime.Now;
        }
    }
}