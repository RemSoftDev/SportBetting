using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class CashierShopContext
    {
        /// <summary>
        /// can be null if no operator is logged in
        /// </summary>
        public Shared.Operator Operator { get; set; }
        /// <summary>
        /// can be null if not logged in or cannot be uniquely determined
        /// </summary>
        public Shared.User User { get; set; }
        /// <summary>
        /// can be null if user is null or user has not created a ticket yet
        /// </summary>
        public Shared.Ticket DraftTicket { get; set; }
    }
}
