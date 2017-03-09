using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SportRadar.DAL.ViewObjects;

namespace SharedInterfaces
{
    public interface IPrintService
    {
        bool PrintCreditNote(CreditNote creditNote);
        bool PrintPaymentNote(PaymentNote paymentNote);
        bool PrintTicket(Ticket ticket, bool isDuplicate);
    }
}
