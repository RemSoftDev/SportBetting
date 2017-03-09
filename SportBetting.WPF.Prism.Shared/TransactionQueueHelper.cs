using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Timers;
using System.Xml;
using System.Xml.Serialization;
using IocContainer;
using Ninject;
using Shared;
using SharedInterfaces;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.Common.Logs;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.Connection;
using TranslationByMarkupExtension;
using WsdlRepository;
using WsdlRepository.WsdlServiceReference;

namespace SportBetting.WPF.Prism.Shared
{
    public class TransactionQueueHelper : ITransactionQueueHelper
    {

        public const int ERROR_ORMID = -1;

        //private static ILog Logger = LogManager.GetLogger(typeof(TransactionQueueHelper));
        private static object m_oLocker = new object();
        private static ILog Logger = LogFactory.CreateLog(typeof(TransactionQueueHelper));


        private static IWsdlRepository WsdlRepository
        {
            get { return IoCContainer.Kernel.Get<IWsdlRepository>(); }
        }


        private IMediator Mediator
        {
            get { return IoCContainer.Kernel.Get<IMediator>(); }
        }
        private IPrinterHandler PrinterHandler
        {
            get { return IoCContainer.Kernel.Get<IPrinterHandler>(); }
        }

        private IStationRepository StationRepository
        {
            get { return IoCContainer.Kernel.Get<IStationRepository>(); }
        }

        private static IChangeTracker _changeTracker;

        private static IChangeTracker ChangeTracker
        {
            get { return _changeTracker ?? (_changeTracker = IoCContainer.Kernel.Get<IChangeTracker>()); }
        }



        private string Limit(string sData)
        {
            const int MAX = 32;

            if (string.IsNullOrEmpty(sData))
            {
                return "null";
            }

            sData = sData.Replace("\r\n", string.Empty);

            return sData.Length > MAX ? sData.Substring(0, MAX) + "..." : sData;
        }

        private string TraceTransactionQueue(TransactionQueueSr tq)
        {
            if (tq == null)
            {
                return "TransactionQueue (null)";
            }

            return string.Format("TransactionQueue (ORMID: {0};  Type: {1};  Description: '{2}';  TransactionId: {3};  Uid: {4};  Object1: '{5}';  Object2: '{6}';  Tag1: '{7}';  Tag2: '{8}';)", tq.ORMID, tq.Type, Limit(tq.Description), tq.TransactionId, Limit(tq.UidState), Limit(tq.Object1), Limit(tq.Object2), Limit(tq.Tag1), Limit(tq.Tag2));
        }

        private eTransactionQueueType ShortToType(short iValue)
        {
            try
            {
                return (eTransactionQueueType)iValue;
            }
            catch (Exception excp)
            {
                Logger.ErrorFormat("Cannot convert Transaction Type {0}", excp, iValue);
            }

            return eTransactionQueueType.None;
        }

        private void CreateAndSaveTransactionQueue(eTransactionQueueType eType, string sDescription, string sTransactionId, uid User, string sObject1, string sObject2, string sObject3, string sTag1, string sTag2)
        {
            Debug.Assert(sDescription != null);

            TransactionQueueSr tq = null;


            tq = new TransactionQueueSr();

            tq.Type = (short)eType;
            tq.Description = sDescription;
            tq.TransactionId = sTransactionId;
            tq.UidState = SerializeHelper.SerializableObjectToString<uid>(User);
            tq.Object1 = sObject1;
            tq.Object2 = sObject2;
            tq.Object3 = sObject3;
            tq.Tag1 = sTag1;
            tq.Tag2 = sTag2;

            tq.Created = DateTime.Now;

            tq.Save();
        }


        public List<TransactionQueueSr> GetAllFromQueue()
        {
            try
            {
                return TransactionQueueSr.GetTransactionQueueList();
            }
            catch (Exception excp)
            {
                Logger.ErrorFormat("GetAllFromQueue ERROR:\r\n{0}\r\n{1}", excp, excp.Message, excp.StackTrace);
            }

            return null;
        }

        public int GetCountTransactionQueue()
        {
            try
            {
                return TransactionQueueSr.GetCountTransactionQueue();
            }
            catch (Exception excp)
            {
                Logger.ErrorFormat("GetCountTransactionQueue ERROR:\r\n{0}\r\n{1}", excp, excp.Message, excp.StackTrace);
            }

            return 0;
        }

        public void Log()
        {
            IList<TransactionQueueSr> lTransactionObjects = GetAllFromQueue();

            if (lTransactionObjects != null && lTransactionObjects.Count > 0)
            {
                string sWarning = string.Format("ATTENTION!!! There are {0} item(s) in Transaction Queue:\r\n", lTransactionObjects.Count);

                foreach (TransactionQueueSr tq in lTransactionObjects)
                {
                    sWarning += TraceTransactionQueue(tq) + "\r\n";
                }

                Logger.Warn(sWarning);
            }
        }

        public void DeleteTransactionObjects(List<TransactionQueueSr> lObjectsToDelete)
        {
            Debug.Assert(lObjectsToDelete != null);

            try
            {
                string sInfo = string.Format("DeleteTransactionObjects() count: {0}", lObjectsToDelete.Count);

                using (IDbConnection conn = ConnectionManager.GetConnection())
                {
                    using (IDbTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            foreach (TransactionQueueSr tqToDelete in lObjectsToDelete)
                            {
                                tqToDelete.Delete(conn, transaction);

                                sInfo += string.Format("{0} deleted from queue.\r\n", tqToDelete);
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            if (transaction != null)
                            {
                                transaction.Rollback();
                            }

                            throw;
                        }
                    }
                }

                Logger.Info(sInfo);
            }
            catch (Exception excp)
            {
                Logger.ErrorFormat("DeleteTransactionObjects({0} objects) ERROR:\r\n{1}\r\n{2}", excp, lObjectsToDelete != null ? lObjectsToDelete.Count : -1, excp.Message, excp.StackTrace);
            }
        }

        public void TryToSaveTransactionObjectsOnHub()
        {
            lock (m_oLocker)
            {
                try
                {
                    List<TransactionQueueSr> lTransactionObjects = GetAllFromQueue();

                    if (lTransactionObjects != null && lTransactionObjects.Count > 0)
                    {
                        Logger.InfoFormat("Trying to save {0} Transaction Objects on HUB ...", lTransactionObjects.Count);

                        var lObjectsToDelete = new List<TransactionQueueSr>();


                        foreach (TransactionQueueSr tq in lTransactionObjects)
                        {
                            try
                            {
                                Logger.InfoFormat("Trying to save {0}", TraceTransactionQueue(tq));

                                eTransactionQueueType eType = ShortToType(tq.Type);
                                var uId = SerializeHelper.StringToSerializableObject<uid>(tq.UidState);

                                if (uId == null)
                                {
                                    Logger.ErrorFormat("uid is null, there is something wrong!!! tq.id = {0}", new Exception(), tq.ORMID);
                                }


                                string sErrors = string.Empty;

                                switch (eType)
                                {
                                    case eTransactionQueueType.None:

                                        Logger.Error("Unknown Type. Skipping...", new Exception());
                                        break;

                                    case eTransactionQueueType.Ticket:

                                        var ticket = SerializeHelper.StringToDataContractObject<TicketWS>(tq.Object1);
                                        var td = SerializeHelper.StringToSerializableObject<TicketData>(tq.Tag1);
                                        uId.session_id = "";

                                        try
                                        {
                                            int iResult = SaveTicketOnHub(tq.TransactionId, uId, ticket, td.StationNumber, td.IsOffLineTicket, ref sErrors);

                                            if (iResult == Ticket.TICKET_SAVE_SUCCESSFUL)
                                            {
                                                PrinterHandler.CreateXmlAndSaveFile(ticket, false);
                                                Logger.InfoFormat("{0} successfully saved", ticket);
                                                lObjectsToDelete.Add(tq);
                                            }
                                            else if (iResult == Ticket.TICKET_ALREADY_SAVED)
                                            {
                                                PrinterHandler.CreateXmlAndSaveFile(ticket, false);
                                                Logger.InfoFormat("{0} already saved", ticket);
                                                lObjectsToDelete.Add(tq);
                                            }
                                            else if (iResult != 0)
                                            {
                                                Logger.InfoFormat("{0} have invalid data", ticket);
                                                lObjectsToDelete.Add(tq);
                                            }
                                            else
                                            {
                                                Logger.ErrorFormat("{0} was not saved:\r\n{1}\r\nResult {2}. Continue trying to save next time...", new Exception(), ticket, sErrors, iResult);
                                            }
                                        }
                                        catch (FaultException<HubServiceException> ex)
                                        {
                                            Logger.InfoFormat("{0} have invalid data", ticket);
                                            Logger.Error(ex.Message, ex);
                                            lObjectsToDelete.Add(tq);
                                        }


                                        break;

                                    case eTransactionQueueType.Deposit:

                                        decimal amount = decimal.Parse(tq.Object1);
                                        bool realMoney = bool.Parse(tq.Object3);
                                        CashAcceptorType? type = null;
                                        CashAcceptorType typeTmp;
                                        Enum.TryParse(tq.Object2, out typeTmp);
                                        if (!String.IsNullOrEmpty(tq.Object2))
                                        {
                                            type = typeTmp;
                                        }
                                        uId.session_id = "";

                                        Exception error = null;
                                        try
                                        {
                                            accountBalance bResult = SaveDepositOnHub(tq.TransactionId, uId, amount, realMoney, ref error, type);

                                            if (bResult != null)
                                            {
                                                Logger.InfoFormat("Deposit {0} for {1} successfully saved", amount, uId);
                                                lObjectsToDelete.Add(tq);
                                            }
                                            else
                                            {
                                                if (error == null)
                                                {
                                                    Logger.ErrorFormat("Deposit {0} for {1} was not saved. Errors:\r\n{2}\r\nContinue trying to save next time...", new Exception(), amount, uId, "empty responce");
                                                    break;
                                                }
                                                Logger.ErrorFormat("Deposit {0} for {1} was not saved. Errors:\r\n{2}\r\nContinue trying to save next time...", new Exception(), amount, uId, error.Message);
                                            }
                                        }
                                        catch (FaultException<HubServiceException> ex)
                                        {
                                            Logger.ErrorFormat("Deposit {0} for {1} was not saved. Errors:\r\n{2}\r\nContinue trying to save next time...", new Exception(), amount, uId, error.Message);
                                            lObjectsToDelete.Add(tq);
                                        }

                                        break;
                                    case eTransactionQueueType.DepositByTicket:

                                        string ticketNumber = tq.Object1;
                                        string creditNumber = tq.Object2;
                                        uId.session_id = "";
                                        try
                                        {
                                            var tnumber = "";
                                            var tcode = "";
                                            var crnumber = "";
                                            var crcode = "";
                                            if (ticketNumber.Length > 4)
                                                tnumber = ticketNumber.Substring(0, ticketNumber.Length - 4);
                                            if (ticketNumber.Length > 4)
                                                tcode = ticketNumber.Substring(ticketNumber.Length - 4);
                                            if (creditNumber.Length > 4)
                                                crnumber = creditNumber.Substring(0, creditNumber.Length - 4);
                                            if (creditNumber.Length > 4)
                                                crcode = creditNumber.Substring(creditNumber.Length - 4);

                                            bool bResultDeposit = DepositByTicketOnHub(tq.TransactionId, uId, tnumber, tcode, crnumber, crcode, ref sErrors);

                                            if (bResultDeposit)
                                            {
                                                Logger.InfoFormat("DepositByTicket {0} for {1} successfully saved", ticketNumber, uId);
                                                lObjectsToDelete.Add(tq);
                                            }
                                            else
                                            {
                                                Logger.ErrorFormat("DepositByTicket {0} for {1} was not saved. TicketNumber {2}  Errors:\r\n{2}\r\nContinue trying to save next time...", new Exception(), ticketNumber, uId, sErrors);
                                            }
                                        }
                                        catch (FaultException<HubServiceException> ex)
                                        {
                                            Logger.ErrorFormat("DepositByTicket {0} for {1} was not saved. TicketNumber {2}  Errors:\r\n{2}\r\nContinue trying to save next time...", ex, ticketNumber, uId, sErrors);
                                            lObjectsToDelete.Add(tq);
                                        }
                                        break;

                                    case eTransactionQueueType.DepositByCreditNote:

                                        string checksum = tq.Object1;
                                        string noteNumber = tq.Object2;
                                        uId.session_id = "";

                                        try
                                        {


                                            bool bResultCredit = DepositByCreditNoteOnHub(tq.TransactionId, uId, noteNumber, checksum, ref sErrors);

                                            if (bResultCredit)
                                            {
                                                Logger.InfoFormat("DepositByCreditNote {0} for {1} successfully saved", checksum, uId);
                                                lObjectsToDelete.Add(tq);
                                            }
                                            else
                                            {
                                                Logger.ErrorFormat("DepositByCreditNote {0} for {1} was not saved. NoteNumber {2}  Errors:\r\n{2}\r\nContinue trying to save next time...", new Exception(), checksum, uId, sErrors, noteNumber);
                                            }
                                        }
                                        catch (FaultException<HubServiceException> ex)
                                        {
                                            Logger.ErrorFormat("DepositByCreditNote {0} for {1} was not saved. NoteNumber {2}  Errors:\r\n{2}\r\nContinue trying to save next time...", new Exception(), checksum, uId, sErrors, noteNumber);
                                            lObjectsToDelete.Add(tq);
                                        }
                                        break;

                                    case eTransactionQueueType.Cash:
                                        bool moneyIn = bool.Parse(tq.Object3);
                                        uid userUID = SerializeHelper.StringToSerializableObject<uid>(tq.UidState);

                                        try
                                        {
                                            DateTime startdate;
                                            DateTime enddate;
                                            var bResultCash = RegisterMoney(userUID, moneyIn, ref sErrors, out startdate, out enddate);
                                            Logger.InfoFormat("RegisterMoney {0} successfully saved", moneyIn, uId);
                                            lObjectsToDelete.Add(tq);
                                        }
                                        catch (FaultException<HubServiceException> ex)
                                        {
                                            Logger.InfoFormat("{0}{1} have invalid data", moneyIn, uId);
                                            Logger.Error(ex.Message, ex);
                                            lObjectsToDelete.Add(tq);

                                        }
                                        break;
                                    default:

                                        Debug.Assert(false);
                                        break;
                                }
                            }
                            catch (Exception excp)
                            {
                                Logger.InfoFormat("Cannot save on HUB {0}", TraceTransactionQueue(tq));
                            }
                        }

                        DeleteTransactionObjects(lObjectsToDelete);
                    }
                }
                catch (Exception excp)
                {
                    Logger.Error("General Error", excp);
                }


            }

        }

        public void PutTicketIntoTransactionQueue(string sTransactionId, uid User, TicketWS ticketData, string sStationNumber, bool bIsOffLineTicket, string sDescription)
        {
            CreateAndSaveTransactionQueue(eTransactionQueueType.Ticket, sDescription, sTransactionId, User, //SerializeHelper.SerializableObjectToString<uid>(uId), 
                                          SerializeHelper.DataContractObjectToString<TicketWS>(ticketData), // Object 1
                                          null, // Object 2
                                          null, SerializeHelper.SerializableObjectToString<TicketData>(new TicketData { StationNumber = sStationNumber, IsOffLineTicket = bIsOffLineTicket }), // Tag1
                                          null); // Tag 2
        }

        public void PutRegisterMoneyIntoTransactionQueue(uid userUID, string sTransactionId, decimal money, bool cashIn, string sDescription)
        {
            CreateAndSaveTransactionQueue(eTransactionQueueType.Cash, sDescription, sTransactionId, userUID, null, money.ToString(), // Object 1
                                          cashIn.ToString(), // Object 2
                                          null, // Tag1
                                          null); // Tag 2
        }

        public void PutDepositByTicketIntoTransactionQueue(string sTransactionId, uid User, string number, string code, string creditnumber, string creditCode, string desc)
        {
            CreateAndSaveTransactionQueue(eTransactionQueueType.DepositByTicket, desc, sTransactionId, User, number + code, // Object 1
                                          creditnumber + creditCode, // Object 2
                                          null, null, // Tag1
                                          null); // Tag 2
        }

        public void PutDepositByCreditNoteIntoTransactionQueue(string sTransactionId, uid User, string number, string checksum, string desc)
        {
            CreateAndSaveTransactionQueue(eTransactionQueueType.DepositByCreditNote, desc, sTransactionId, User, checksum, // Object 1
                                          number, // Object 2
                                          null, null, // Tag1
                                          null); // Tag 2
        }

        public void PutDepositIntoTransactionQueue(string sTransactionId, uid User, decimal decAmount, bool realMoney, string sDescription, CashAcceptorType? type)
        {
            string typeValue = "";
            if (type != null)
                typeValue = type.Value.ToString();
            CreateAndSaveTransactionQueue(eTransactionQueueType.Deposit, sDescription, sTransactionId, User, decAmount.ToString("G"), // Object 1
                                          typeValue, // Object 2
                                          realMoney.ToString(), null, // Tag1
                                          null); // Tag 2
        }

        private int SaveTicketOnHub(string sTransactionId, uid UserSession, TicketWS ticketData, string sStationNumber, bool bIsOffLineTicket, ref string sError)
        {
            uid anonUID = UserSession;
            if (StationRepository.AllowAnonymousBetting)
            {
                if (anonUID.account_id == "0")
                    anonUID = null;
            }

            try
            {
                long[] tipItems;
                long[] tournamentLock;
                string sResult = WsdlRepository.SaveTicket(sTransactionId, anonUID, ticketData, bIsOffLineTicket, sStationNumber, out tipItems, out tournamentLock);
                return Convert.ToInt32(sResult);
            }
            catch (FaultException<HubServiceException> ex)
            {
                Logger.ErrorFormat("SaveTicket ERROR: {0}\r\n{1}", ex, ex.Message, ex.StackTrace);
                throw;
            }
            catch (Exception ex)
            {
                sError = ex.Message;
                Logger.ErrorFormat("SaveTicket ERROR: {0}\r\n{1}", ex, ex.Message, ex.StackTrace);
            }


            return Ticket.TICKET_SAVE_FAILED;
        }

        private int StoreTicket(string sTransactionId, uid uid, TicketWS ticketDataWs, string sStationNumber, bool bIsOffLineTicket, string pin, ref string sErrors)
        {
            try
            {
                string sResult = WsdlRepository.StoreTicket(sTransactionId, uid, ticketDataWs, pin, bIsOffLineTicket, sStationNumber);

                return Convert.ToInt32(sResult);
            }
            catch (FaultException<HubServiceException> ex)
            {
                Logger.ErrorFormat("CashOut ERROR: {0}\r\n{1}", ex, ex.Message, ex.StackTrace);
                throw;
            }
            catch (Exception ex)
            {
                sErrors = ex.Message;
                Logger.ErrorFormat("CashOut ERROR: {0}\r\n{1}", ex, ex.Message, ex.StackTrace);
            }

            return Ticket.TICKET_SAVE_FAILED; //this line actually never executed
        }

        private decimal RegisterMoney(uid userUID, bool cashIn, ref string sError, out DateTime startdate, out DateTime enddate)
        {
            startdate = DateTime.MinValue;
            enddate = DateTime.MinValue;
            if (!cashIn)
            {
                try
                {
                    var amount = WsdlRepository.CashOut(userUID, StationRepository.StationNumber, out startdate, "Get Money out of cash box", out  enddate);
                    ChangeTracker.LastCashoutDate = startdate;
                    return amount;
                }
                catch (FaultException<HubServiceException> ex)
                {
                    Logger.ErrorFormat("CashOut ERROR: {0}\r\n{1}", ex, ex.Message, ex.StackTrace);
                    throw;
                }
                catch (Exception ex)
                {
                    sError = ex.Message;
                    Logger.ErrorFormat("CashOut ERROR: {0}\r\n{1}", ex, ex.Message, ex.StackTrace);
                }
            }

            return 0M;
        }

        private accountBalance SaveDepositOnHub(string sTransactionId, uid uId, decimal decMoneyAmount, bool realMoney, ref Exception sError, CashAcceptorType? type)
        {
            Debug.Assert(!string.IsNullOrEmpty(sTransactionId));
            Debug.Assert(uId != null);

            try
            {
                return WsdlRepository.Deposit(sTransactionId, uId, decMoneyAmount, realMoney, type, !realMoney);
            }
            catch (FaultException<HubServiceException> ex)
            {
                Logger.ErrorFormat("CashOut ERROR: {0}\r\n{1}", ex, ex.Message, ex.StackTrace);
                sError = ex;
                throw;
            }
            catch (Exception ex)
            {
                sError = ex;

                Logger.ErrorFormat("SaveDeposit ERROR: {0}\r\n{1}", ex, ex.Message, ex.StackTrace);
            }

            return null;
        }

        private bool DepositByTicketOnHub(string sTransactionId, uid uId, String ticketNumber, String ticketCode, string creditNoteNumber, string creditNoteCode, ref string sError)
        {
            Debug.Assert(!string.IsNullOrEmpty(sTransactionId));
            Debug.Assert(uId != null);
            try
            {
                return WsdlRepository.DepositByTicket(sTransactionId, uId, ticketNumber, ticketCode, creditNoteNumber, creditNoteCode);
            }
            catch (FaultException<HubServiceException> ex)
            {
                Logger.ErrorFormat("CashOut ERROR: {0}\r\n{1}", ex, ex.Message, ex.StackTrace);
                throw;
            }
            catch (Exception ex)
            {
                sError = ex.Message;
                Logger.ErrorFormat("SaveDepositByTicket ERROR: {0}\r\n{1}", ex, ex.Message, ex.StackTrace);
            }


            return false;
        }

        private bool DepositByCreditNoteOnHub(string sTransactionId, uid uId, String noteNumber, string checkSum, ref string sError)
        {
            Debug.Assert(!string.IsNullOrEmpty(sTransactionId));
            Debug.Assert(uId != null);

            try
            {
                return WsdlRepository.DepositByCreditNote(sTransactionId, uId, noteNumber, checkSum);
            }
            catch (FaultException<HubServiceException> ex)
            {
                Logger.ErrorFormat("CashOut ERROR: {0}\r\n{1}", ex, ex.Message, ex.StackTrace);
                throw ex;
            }
            catch (Exception ex)
            {
                sError = ex.Message;
                Logger.ErrorFormat("DepositByCreditNote ERROR: {0}\r\n{1}", ex, ex.Message, ex.StackTrace);
            }

            return false;
        }

        public int TrySaveTicketOnHub(string sTransactionId, uid uid, TicketWS ticketDataWs, string sStationNumber, bool bIsOffLineTicket)
        {
            string error = string.Empty;
            int iResult = SaveTicketOnHub(sTransactionId, uid, ticketDataWs, sStationNumber, bIsOffLineTicket, ref error);


            if (iResult == Ticket.TICKET_SAVE_SUCCESSFUL)
            {
                Logger.InfoFormat("{0} successfully saved", ticketDataWs);
            }
            else if (uid.account_id == "1" && error != null && ticketDataWs.ticketTyp == Ticket.TICKET_TYP_SPORTBET)
            {
                iResult = Ticket.TICKET_SAVE_SUCCESSFUL;
                Logger.ErrorFormat("{0} was not saved. Errors:\r\n{1}", new Exception(), ticketDataWs, error);

                PutTicketIntoTransactionQueue(sTransactionId, uid, ticketDataWs, sStationNumber, bIsOffLineTicket, "Cannot save ticket:\r\n" + error);
            }

            return iResult;
        }

        public int TryStoreTicketOnHub(string sTransactionId, uid uidSession, TicketWS ticketDataWs, string sStationNumber, bool bIsOffLineTicket, string pin)
        {
            string sErrors = string.Empty;
            int iResult = StoreTicket(sTransactionId, uidSession, ticketDataWs, sStationNumber, bIsOffLineTicket, pin, ref sErrors);

            // Uncomment line bellow to force testing TransactionQueueHelper
            // TransactionQueueHelper.PutTicketIntoTransactionQueue(sTransactionId, uidSession, ticketDataWs, sStationNumber, bIsOffLineTicket, "Test save ticket:\r\n" + sErrors);

            if (iResult == Ticket.TICKET_SAVE_SUCCESSFUL)
            {
                Logger.InfoFormat("{0} successfully saved", ticketDataWs);
            }

            return iResult;
        }

        public decimal TryRegisterMoneyOnHub(uid userUID, string sTransactionId, bool cashIn, string operationType, int operatorId, bool checkpoint, out DateTime startdate, out DateTime enddate)
        {
            string error = string.Empty;
            var money = RegisterMoney(userUID, cashIn, ref error, out startdate, out enddate);

            var cash = new StationCashSr { Cash = money, MoneyIn = cashIn, OperationType = operationType, OperatorID = operatorId.ToString(), CashCheckPoint = checkpoint, DateModified = DateTime.Now };
            using (IDbConnection con = ConnectionManager.GetConnection())
            {
                cash.Save(con, null);
            }

            if (money > 0)
            {
                Logger.InfoFormat("{0} successfully saved", money);
            }
            else if (error != null)
            {
                Logger.ErrorFormat("{0} was not saved. Errors:\r\n{1}", new Exception(), money, error);
                var lostConnection = new Tuple<string, string, bool, int>("LostInternetConnection", "", false, 0);
                Mediator.SendMessage(lostConnection, "Error");

                PutRegisterMoneyIntoTransactionQueue(userUID, sTransactionId, money, cashIn, "Cannot register money:\r\n" + error);
            }

            return 0;
        }

        public accountBalance TryDepositMoneyOnHub(string sTransactionId, uid User, decimal money, bool realMoney, ref Exception error, CashAcceptorType? type)
        {
            accountBalance iResult = SaveDepositOnHub(sTransactionId, User, money, realMoney, ref error, type);

            if (iResult != null)
            {
                Logger.InfoFormat("{0} successfully deposited", money);
            }
            else if (error != null)
            {
                var lostConnection = new Tuple<string, string, bool, int>("LostInternetConnection", "", false, 0);
                Mediator.SendMessage(lostConnection, "Error");
                Logger.ErrorFormat("{0} was not deposited. Errors:\r\n{1}", new Exception(), money, error.Message);

                PutDepositIntoTransactionQueue(sTransactionId, User, money, realMoney, "Cannot store Deposit:\r\n" + error.Message, type);
            }
            return iResult;
        }

        public bool TryDepositByTicketMoneyOnHub(string sTransactionId, uid User, string number, string code, string creditNumber, string creditCode, ref string error)
        {
            bool iResult = DepositByTicketOnHub(sTransactionId, User, number, code, creditNumber, creditCode, ref error);


            if (iResult)
            {
                Logger.InfoFormat("{0} successfully deposited", number);
            }
            else if (error != null)
            {
                Logger.ErrorFormat("{0} was not deposited. Errors:\r\n{1}", new Exception(), number, error);

                var lostConnection = new Tuple<string, string, bool, int>("LostInternetConnection", "", false, 0);
                Mediator.SendMessage(lostConnection, "Error");
                PutDepositByTicketIntoTransactionQueue(sTransactionId, User, number, code, creditNumber, creditCode, "Cannot Deposit by ticket:\r\n" + error);
            }

            return iResult;
        }

        public bool TryDepositByCreditNoteMoneyOnHub(string sTransactionId, uid User, string number, string checkSum, ref string error)
        {
            bool iResult = DepositByCreditNoteOnHub(sTransactionId, User, number, checkSum, ref error);


            if (iResult)
            {
                Logger.InfoFormat("{0} successfully deposited", number);
            }
            else if (error != null)
            {
                Logger.ErrorFormat("{0} was not deposited. Errors:\r\n{1}", new Exception(), number, error);

                var lostConnection = new Tuple<string, string, bool, int>("LostInternetConnection", "", false, 0);
                Mediator.SendMessage(lostConnection, "Error");
                PutDepositByCreditNoteIntoTransactionQueue(sTransactionId, User, number, checkSum, "Cannot Deposit by credit note:\r\n" + error);
            }

            return iResult;
        }

        public static class SerializeHelper
        {
            //private static ILog Logger = LogManager.GetLogger(typeof(TransactionQueueHelper));

            #region DataContract

            public static MemoryStream DataContractObjectToStream<T>(object objToSerialize)
            {
                Type tObject = typeof(T);

                Debug.Assert(objToSerialize != null && objToSerialize.GetType() == tObject);

                try
                {
                    var ms = new MemoryStream();
                    var dcs = new DataContractSerializer(tObject);

                    dcs.WriteObject(ms, objToSerialize);

                    ms.Position = 0;

                    return ms;
                }
                catch (Exception excp)
                {
                    Logger.ErrorFormat("DataContractObjectToStream<{0}> ({1}) ERROR:{2}\r\n{3}", excp, tObject, objToSerialize != null ? objToSerialize.ToString() : "NULL", excp.Message, excp.StackTrace);
                }

                return null;
            }

            public static byte[] DataContractObjectToByteArray<T>(object objToSerialize)
            {
                Type tObject = typeof(T);

                try
                {
                    using (MemoryStream ms = DataContractObjectToStream<T>(objToSerialize))
                    {
                        return ms.ToArray();
                    }
                }
                catch (Exception excp)
                {
                    Logger.ErrorFormat("DataContractObjectToByteArray<{0}> ({1}) ERROR:{2}\r\n{3}", excp, tObject, objToSerialize != null ? objToSerialize.ToString() : "NULL", excp.Message, excp.StackTrace);
                }

                return null;
            }

            public static string DataContractObjectToString<T>(object objToSerialize)
            {
                Type tObject = typeof(T);

                try
                {
                    using (MemoryStream ms = DataContractObjectToStream<T>(objToSerialize))
                    {
                        using (var sr = new StreamReader(ms))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
                catch (Exception excp)
                {
                    Logger.ErrorFormat("DataContractObjectToString<{0}> ({1}) ERROR:{2}\r\n{3}", excp, tObject, objToSerialize != null ? objToSerialize.ToString() : "NULL", excp.Message, excp.StackTrace);
                }

                return null;
            }

            public static T StreamToDataContractObject<T>(Stream sm)
            {
                Debug.Assert(sm != null);
                Type tObject = typeof(T);

                try
                {
                    XmlDictionaryReader xdr = XmlDictionaryReader.CreateTextReader(sm, new XmlDictionaryReaderQuotas());
                    var dcs = new DataContractSerializer(tObject);

                    return (T)dcs.ReadObject(xdr, true);
                }
                catch (Exception excp)
                {
                    Logger.ErrorFormat("StreamToDataContractObject<{0}> () ERROR:{1}\r\n{2}", excp, tObject, excp.Message, excp.StackTrace);
                }

                return default(T);
            }

            public static T BytesToDataContractObject<T>(byte[] arrBytes)
            {
                Debug.Assert(arrBytes != null);
                Type tObject = typeof(T);

                try
                {
                    using (var ms = new MemoryStream(arrBytes))
                    {
                        return StreamToDataContractObject<T>(ms);
                    }
                }
                catch (Exception excp)
                {
                    Logger.ErrorFormat("BytesToDataContractObject<{0}> () ERROR:{1}\r\n{2}", excp, tObject, excp.Message, excp.StackTrace);
                }

                return default(T);
            }

            public static T StringToDataContractObject<T>(string sSerializedObject)
            {
                Debug.Assert(!string.IsNullOrEmpty(sSerializedObject));
                Type tObject = typeof(T);

                try
                {
                    byte[] arrBytes = Encoding.ASCII.GetBytes(sSerializedObject);
                    return BytesToDataContractObject<T>(arrBytes);
                }
                catch (Exception excp)
                {
                    Logger.ErrorFormat("StringToDataContractObject<{0}> (string {1} characters) ERROR:{2}\r\n{3}", excp, tObject, !string.IsNullOrEmpty(sSerializedObject) ? sSerializedObject.Length : -1, excp.Message, excp.StackTrace);
                }

                return default(T);
            }

            #endregion // DataContract

            #region Serializable

            public static MemoryStream SerializableObjectToStream<T>(object objToSerialize)
            {
                Type tObject = typeof(T);

                Debug.Assert(objToSerialize != null && objToSerialize.GetType() == tObject);

                try
                {
                    var ms = new MemoryStream();
                    var xs = new XmlSerializer(tObject);

                    xs.Serialize(ms, objToSerialize);
                    ms.Position = 0;

                    return ms;
                }
                catch (Exception excp)
                {
                    Logger.ErrorFormat(" SerializableObjectToStream<{0}> ({1}) ERROR:{2}\r\n{3}", excp, tObject, objToSerialize != null ? objToSerialize.ToString() : "NULL", excp.Message, excp.StackTrace);
                }

                return null;
            }

            public static byte[] SerializableObjectToByteArray<T>(object objToSerialize)
            {
                Type tObject = typeof(T);

                try
                {
                    using (MemoryStream ms = SerializableObjectToStream<T>(objToSerialize))
                    {
                        return ms.ToArray();
                    }
                }
                catch (Exception excp)
                {
                    Logger.ErrorFormat("SerializableObjectToByteArray<{0}> ({1}) ERROR:{2}\r\n{3}", excp, tObject, objToSerialize != null ? objToSerialize.ToString() : "NULL", excp.Message, excp.StackTrace);
                }

                return null;
            }


            public static string SerializableObjectToString<T>(object objToSerialize)
            {
                Type tObject = typeof(T);

                try
                {
                    using (MemoryStream ms = SerializableObjectToStream<T>(objToSerialize))
                    {
                        using (var sr = new StreamReader(ms))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
                catch (Exception excp)
                {
                    Logger.ErrorFormat("SerializableObjectToString<{0}> ({1}) ERROR:{2}\r\n{3}", excp, tObject, objToSerialize != null ? objToSerialize.ToString() : "NULL", excp.Message, excp.StackTrace);
                }

                return null;
            }

            public static T StreamToSerializableObject<T>(Stream sm)
            {
                Debug.Assert(sm != null);
                Type tObject = typeof(T);

                try
                {
                    var xs = new XmlSerializer(tObject);

                    return (T)xs.Deserialize(sm);
                    ;
                }
                catch (Exception excp)
                {
                    Logger.ErrorFormat("StreamToSerializableObject<{0}> () ERROR:{1}\r\n{2}", excp, tObject, excp.Message, excp.StackTrace);
                }

                return default(T);
            }

            public static T BytesToSerializableObject<T>(byte[] arrBytes)
            {
                Debug.Assert(arrBytes != null);
                Type tObject = typeof(T);

                try
                {
                    using (var ms = new MemoryStream(arrBytes))
                    {
                        return StreamToSerializableObject<T>(ms);
                    }
                }
                catch (Exception excp)
                {
                    Logger.ErrorFormat("BytesToSerializableObject<{0}> () ERROR:{1}\r\n{2}", excp, tObject, excp.Message, excp.StackTrace);
                }

                return default(T);
            }

            public static T StringToSerializableObject<T>(string sSerializedObject)
            {
                Debug.Assert(!string.IsNullOrEmpty(sSerializedObject));
                Type tObject = typeof(T);

                try
                {
                    byte[] arrBytes = Encoding.ASCII.GetBytes(sSerializedObject);
                    return BytesToSerializableObject<T>(arrBytes);
                }
                catch (Exception excp)
                {
                    Logger.ErrorFormat("StringToSerializableObject<{0}> (string {1} characters) ERROR:{2}\r\n{3}", excp, tObject, !string.IsNullOrEmpty(sSerializedObject) ? sSerializedObject.Length : -1, excp.Message, excp.StackTrace);
                }

                return default(T);
            }

            #endregion // Serializable
        }

        [Serializable]
        public class TicketData
        {
            [DataMember(IsRequired = true)]
            public string StationNumber { get; set; }

            [DataMember(IsRequired = true)]
            public bool IsOffLineTicket { get; set; }

            [DataMember(IsRequired = false)]
            public string PinCode { get; set; }
        }
    }
}