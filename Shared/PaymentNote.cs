using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class PaymentNote
    {
        public string Number { get; set; }

        public DateTime ExpirationTime { get; set; }

        public decimal Amount { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DocumentType { get; set; }

        public string DocumentNumber { get; set; }
    }
}
