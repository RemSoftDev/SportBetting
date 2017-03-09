using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shared
{
    public enum PreOrderPayOutStatus
    {
        Pending,
        Approved,
        Denied,
        Expired,
        Paid,
        Finalized
    }

    public class PreOrderPayOut
    {
        public Shared.User User { get; set; }
        public DateTime DesiredDate { get; set; }
        public decimal DesiredAmount { get; set; }
        public DateTime ApprovedDate { get; set; }
        public decimal ApprovedAmount { get; set; }
        public DateTime ExpireDate { get; set; }
        public string Comments { get; set; }
        public PreOrderPayOutStatus Status { get; set; }

        public DateTime CreatedOn { get; set; }
        public Shared.Operator CreatedBy { get; set; }
        public DateTime ChangedOn { get; set; }
        public Shared.Operator ChangedBy { get; set; }
    }
}
