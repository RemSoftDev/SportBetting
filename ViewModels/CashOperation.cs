using System;

namespace ViewModels
{
    public class CashOperation
    {
        public long CashOutId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateModified { get; set; }
        public String OperationType { get; set; }
        public string OperatorID { get; set; }

        public CashOperation(long id, decimal amount, DateTime dateModified, string operationType, string operatorID)
        {
            CashOutId = id;
            Amount = amount;
            DateModified = dateModified;
            OperationType = operationType;
            OperatorID = operatorID;
        }
    }
}
