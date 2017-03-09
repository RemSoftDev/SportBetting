using System;
using System.Collections.ObjectModel;
using Shared;
using WsdlRepository.WsdlServiceReference;

namespace SportBetting.WPF.Prism.Shared.Services
{
    public interface ITicketActions
    {
        TicketWS SaveTicket(out int errorcode, TicketWS ticketWs, User currentUser);
        bool PrintTicket(TicketWS ticketWs, bool isDuplicate);
        TicketWS StoreTicket(User currentUser, string value, out int errorcode, TicketWS ticketWs, Ticket ticket);
        bool PrintStoredTicket(TicketWS ticketWs, string value, string stationNumber, DateTime addHours, long accountId);
        TicketWS CreateNewTicketWS(Ticket newticket);
    }
}