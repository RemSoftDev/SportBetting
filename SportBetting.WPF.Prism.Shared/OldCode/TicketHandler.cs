using IocContainer;
using Ninject;
using Shared;
using SportBetting.WPF.Prism.Shared;
using WsdlRepository;
using WsdlRepository.WsdlServiceReference;

namespace SportBetting.WPF.Prism.OldCode
{
    /// <summary>
    /// Class checks if the Station WEB-Service is avialable and saves the ticket over the Service. If not, the ticket is saved in the local database.
    /// </summary>
    public class TicketSaveHandler : ITicketSaveHandler
    {




        private IStationRepository StationRepository
        {
            get { return IoCContainer.Kernel.Get<IStationRepository>(); }
        }
        private IBusinessPropsHelper BusinessPropsHelper
        {
            get { return IoCContainer.Kernel.Get<IBusinessPropsHelper>(); }
        }
        private ITransactionQueueHelper TransactionQueueHelper
        {
            get { return IoCContainer.Kernel.Get<ITransactionQueueHelper>(); }
        }



        public int SaveTicket(TicketWS TicketDataWS, string StationNumber, bool IsOffLineTicket, User user)
        {
            StationRepository.SetStationAppConfigValue("LastTicketNbr", TicketDataWS.ticketNbr + " " + TicketDataWS.checkSum);

            int iResult = TransactionQueueHelper.TrySaveTicketOnHub(BusinessPropsHelper.GetNextTransactionId(), StationRepository.GetUid(user), TicketDataWS, StationNumber, IsOffLineTicket);

            return iResult;
        }

        public int StoreTicket(User uid, TicketWS TicketDataWS, string StationNumber, bool IsOffLineTicket, string pin, bool livebet)
        {

            StationRepository.SetStationAppConfigValue("LastTicketNbr", TicketDataWS.ticketNbr + " " + TicketDataWS.checkSum);

            int iResult = TransactionQueueHelper.TryStoreTicketOnHub(BusinessPropsHelper.GetNextTransactionId(), StationRepository.GetUid(uid), TicketDataWS, StationNumber, IsOffLineTicket, pin);

            return iResult;

        }
    }

    public interface ITicketSaveHandler
    {
        int SaveTicket(TicketWS ticket, string stationNumber, bool b, User user);
    }
}