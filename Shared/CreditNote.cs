using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class CreditNote
    {
        public string Number { get; set; }

        public string CheckSum { get; set; }

        public decimal Amount { get; set; }

        public DateTime? ExpireDate { get; set; }
    }
}
