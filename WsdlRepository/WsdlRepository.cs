using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web.Services.Protocols;
using WsdlRepository;
using WsdlRepository.WsdlServiceReference;

namespace WcfService
{
    public class WsdlRepository : IWsdlRepository
    {
        public BSMHubServiceClient Client
        {
            get
            {
                return _client;
            }
            private set { _client = value; }
        }

        public bool ConnectionProblem { get; set; }

        public WsdlRepository()
        {
            Client = new BSMHubServiceClient();
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(
             delegate
             {
                 return true;
             });
        }

        public string SecureKey
        {
            get { return _secureKey; }
            set { _secureKey = value; }
        }


        public static void BypassCertificateError()
        {
            ServicePointManager.ServerCertificateValidationCallback +=

                delegate(
                    Object sender1,
                    X509Certificate certificate,
                    X509Chain chain,
                    SslPolicyErrors sslPolicyErrors)
                {
                    return true;
                };
        }

        private string Stationnumber;
        private string _secureKey = "";
        private BSMHubServiceClient _client;

        public StationWS GetStationProperties(string StationNumber, string ip, string teamViewerID, string version, PeripheralInfo pi, out valueForm BasConfiguration, out BsmHubConfigurationResponse BsmHubConfiguration, DriverInfo[] drivers)
        {

            lock (SecureKey)
            {
                Stationnumber = StationNumber;
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetStationProperties(StationNumber, ip, teamViewerID, version, pi, drivers, out BasConfiguration, out BsmHubConfiguration);
                }
            }

        }

        public CashOperationtData[] GetStationCashHistory(string stationId, DateTime startDate, DateTime endDate)
        {
            lock (SecureKey)
                return Client.GetStationCashHistory(stationId, startDate, endDate);
        }

        public decimal GetStationCashInfo(string station, out decimal billsAmount, out int billscount, out decimal coinsamount, out int coinscount)
        {
            lock (SecureKey)
            {
                return Client.GetStationCashInfo(station, out billsAmount, out billscount, out coinsamount, out coinscount);
            }

        }


        public registrationForm GetRegistrationForm()
        {

            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;

                    return Client.GetRegistrationForm();
                }
            }
        }

        public IEnumerable<UserRole> GetAllRoles(string operatorId)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetAllRoles(operatorId);
                }
            }
        }

        public bool DepositByCreditNote(string sTransactionId, uid uId, string noteNumber, string checkSum)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.DepositByCreditNote(sTransactionId, uId, noteNumber, checkSum);
                }
            }
        }

        public string IssueCreditToStationBalance(uid uid, decimal amount)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.IssueCreditToStationBalance(uid, amount);
                }
            }
        }

        public SettlementHistory[] GetSettlementHistory(uid accountUid)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetSettlementHistory(accountUid);
                }
            }
        }

        public OperatorShiftCheckpoint[] GetOperatorShiftCheckpoints(int locationId, short operatorId, out decimal tempBalance, bool isFilterByOperator)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetOperatorShiftCheckpoints(locationId, operatorId, isFilterByOperator, out tempBalance);
                }
            }
        }

        public OperatorShiftData CreateOperatorShiftCheckpoint(int locationId, short operatorId)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.CreateOperatorShiftCheckpoint(locationId, operatorId);
                }
            }
        }

        public OperatorWithShift[] GetAllOperatorsWithShifts(uid uid)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetAllOperatorsWithShifts(uid);
                }
            }
        }

        public SettlementHistoryDetail[] GetSettlementHistoryDetails(int id)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetSettlementHistoryDetails(id);
                }
            }
        }

        public bool CheckForEmptyBoxAndPayments(int locationId, int id)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.CheckForEmptyBoxAndPayments(locationId, id);
                }
            }
        }

        public profileForm LoadPaymentNote(string paymentNoteNumber, uid getUid, out decimal amount)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.LoadPaymentNote(paymentNoteNumber, getUid, out amount);
                }
            }
        }

        public bool PayCreditNote(uid getUid, string number, string code, string stationUsername, string stationNumber)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    decimal amount;
                    return Client.PayCreditNote(getUid, number, code, stationUsername, out amount);
                }
            }
        }

        public string WithdrawByPaymentNote(string paymentNoteNumber, uid getUid, out decimal amount, out bool withFrombalance)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.WithdrawByPaymentNote(paymentNoteNumber, getUid, out amount, out withFrombalance);
                }
            }
        }
        public CashOutItem[] GetCasheOuts(string stationnumber)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetCasheOuts(stationnumber);
                }
            }
        }

        public DateTime ProduceOperatorSettlement(ref int operatorId, int accountId, out string opName, out string opLName, out string frName, out string locName, out string locOwnerName, out DateTime stDate, out DateTime enDate, out CheckpointSlip[] cpArray, out TotalSettlementSection totSection)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.ProduceOperatorSettlement(ref operatorId, accountId, out opName, out opLName, out frName, out locName, out locOwnerName, out stDate, out enDate, out cpArray, out totSection);
                }
            }
        }

        public UpdateRecord[] UpdateLine(string stationNumber, long lastUpdateFileId, DateTime lastUpdate, out int? total)
        {
            lock (SecureKey)
            {
                Stationnumber = stationNumber;

                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.UpdateLine(stationNumber, lastUpdateFileId, lastUpdate, out total);
                }
            }
        }

        public UpdateRecord[] UpdateStatistics(string stationNumber, long lastUpdateFileId)
        {

            lock (SecureKey)
            {
                Stationnumber = stationNumber;

                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.UpdateStatistics(stationNumber, lastUpdateFileId);
                }
            }
        }
        public UpdateRecord[] GetLatestConfidenceFactorsUpdate(string stationNumber)
        {

            lock (SecureKey)
            {
                Stationnumber = stationNumber;

                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetLatestConfidenceFactorsUpdate(stationNumber);
                }
            }
        }

        public UpdateRecord[] UpdateLocalization(string stationNumber, long lastUpdateFileId)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.UpdateLocalization(stationNumber, lastUpdateFileId);
                }
            }
        }

        public UpdateRecord[] UpdateFlags(string stationNumber, long lastUpdateFileId)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.UpdateFlags(stationNumber, lastUpdateFileId);
                }
            }
        }
        public bool RegisterCash(uid uid, decimal amount)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.RegisterCash(uid, amount, CashAcceptorType.BillValidator);
                }
            }
        }

        public UpdateRecord[] GetMetainfo(string stationNumber, long id)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.UpdateLineConfig(stationNumber, id);
                }
            }
        }
        public decimal GetBettingBalance(DateTime startDate, DateTime endDate, string stationNumber, out decimal amountWon)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetBettingBalance(startDate, endDate, stationNumber, out amountWon);
                }
            }
        }


        public long GetBusinessProps(string stationNumber, out long lastTicketNumber, out long lastTransactionId)
        {
            lock (SecureKey)
            {
                Stationnumber = stationNumber;

                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    string secureKey;
                    var result = Client.GetBusinessProps(stationNumber, out lastTicketNumber, out lastTransactionId, out secureKey);
                    if (secureKey != null)
                        SecureKey = secureKey;
                    return result;
                }
            }
        }

        public TicketWS LoadTicket(string number, string checksum, string stationNumber, string lang, string culture, bool showDetails)
        {

            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.LoadTicket(number, checksum, stationNumber, lang, culture, showDetails);
                }
            }
        }

        public TicketWS[] LoadStoredTicket(uid userUid, string number, string checksum, string stationNumber, string pin)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.LoadStoredTickets(userUid, stationNumber, number, pin, checksum);
                }
            }
        }

        public SessionWS OpenSession(string station_id, bool anonymous, string username, string password, bool isShop)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.OpenSession(station_id, anonymous, username, password, isShop);
                }
            }
        }

        public SessionWS LoginWithIdCard(string station_id, string cardNumber, string pin, bool isShopSession = false)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.LoginWithIdCard(station_id, cardNumber, isShopSession, pin);
                }
            }
        }

        public UserTicket[] GetUserTickets(string accountId, ticketCategory ticketCategory, AccountTicketSorting sorting, int startIndex, int pageSize, out string totalNumber)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetUserTickets(accountId, ticketCategory, sorting, startIndex, pageSize, null, null, out totalNumber);
                }
            }
        }

        public bool CloseSession(string sessionid)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.CloseSession(sessionid);
                }
            }
        }
        public profileForm LoadProfile(uid uid)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.LoadProfile(uid);
                }
            }
        }

        public string UpdateProfile(long? operatorId, uid uidFromUser, valueForm valueForm)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.UpdateProfile((int?)operatorId, uidFromUser, valueForm);
                }
            }
        }

        public int RegisterAccount(long? operatorId, valueForm valueForm, string stationNumber, bool isVerified, out string registration_note_number)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.RegisterAccount((int?)operatorId, valueForm, stationNumber, isVerified, out registration_note_number);
                }
            }
        }

        public string Matchsorting(string matches)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetMatchSorting(matches);
                }
            }
        }

        public bool ChangePasswordFromTerminal(uid uid, string oldPassword, string newPassword)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.ChangePasswordFromTerminal(uid, oldPassword, newPassword);
                }
            }
        }

        public string EndUserVerification(uid uid)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.EndUserVerification(uid);
                }
            }
        }

        public bool ToggleAccountState(long accountId, long operatorId)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.ToggleAccountState((int)accountId, (int?)operatorId);
                }
            }
        }

        public string UpdateIdCard(string cardnumber, string status, bool active, long? accountId)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.UpdateIdCard(cardnumber, status, active, (int?)accountId);
                }
            }
        }

        public string RegisterIdCard(string number, bool active, string franchizorId, out string pin)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.RegisterIdCard(number, active, franchizorId, out pin);
                }
            }
        }
        public bool UnbindIdCard(string number)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.UnbindIdCard(number);
                }
            }
        }

        public void ChangeIDCardPin(uid uid, ref string newPin)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    Client.ChangeIDCardPin(uid, ref newPin);
                }
            }
        }

        public void ChangeOperatorIDCardPin(long operatorId, ref string newPin)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    Client.ChangeOperatorIDCardPin((int)operatorId, ref newPin);
                }
            }
        }

        public valueField[][] AccountSearch(criteria[] criterias, uid uid)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.AccountSearch(null, null, null, null, null, criterias, uid, false);
                }
            }
        }

        public Operator[] SearchForOperators(OperatorCriterias criterias, uid uid)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.SearchForOperators(criterias, uid);
                }
            }
        }

        public int RegisterOperator(uid uidFromUser, string firstName, string lastName, string username, string password, string language, int roleId, string email)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.RegisterOperator(uidFromUser, firstName, lastName, username, password, language, roleId, email);
                }
            }
        }

        public bool UpdateOperator(long accountId, OperatorCriterias criterias)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.UpdateOperator((int)accountId, criterias);
                }
            }
        }

        public string BindOperatorIdCard(long accountId, string cardNumber, out string pin)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.BindOperatorIdCard((int)accountId, cardNumber, out pin);
                }
            }
        }

        public string CreateCheckpoint(string stationNumber, long accountId, out decimal amount)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.CreateCheckpoint(stationNumber, (int)accountId, out amount);
                }
            }
        }


        public AccountingRecieptWS GetAccountingRecieptData(DateTime startDate, DateTime endDate, string stationNumber, int? locationId)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetAccountingRecieptData(startDate, endDate, stationNumber, locationId);
                }
            }
        }

        public string GetAccountByTicket(string ticketNumber)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetAccountByTicket(ticketNumber);
                }
            }
        }

        public bool ChangePasswordFromShop(int operatorId, uid getUid, string newPassword)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.ChangePasswordFromShop(operatorId, getUid, newPassword);
                }
            }
        }

        public string History(uid uid, string type, string operationgroup, string startIndex, string limit, DateTime? startDate, DateTime? endDate, out historyData[] history, bool hideSystemRecalculation = true)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.History(uid, type, operationgroup, startIndex, limit, startDate, endDate, hideSystemRecalculation, out history);
                }
            }
        }

        public ArrayOfProfitAccountingCheckpoints GetProfitAccountingCheckpoint(int objectId, string stationNumber, int currentPosition, int itemsAmountPerPage)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetProfitAccountingCheckpoint(objectId, stationNumber, currentPosition, itemsAmountPerPage);
                }
            }
        }

        public ProfitAccountingCheckpoint CreateProfitAccountingCheckpoint(int locationId, string stationNumber)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.CreateProfitAccountingCheckpoint(locationId, stationNumber);
                }
            }
        }

        public IEnumerable<StationBalanceCheckpoint> GetStationBalanceCheckpoints(string stationNumber, int startIndex, int pageSize)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetStationBalanceCheckpoints(stationNumber, startIndex, pageSize);
                }
            }
        }

        public decimal GetStationPaymentFlowData(string stationNumber, int currentPosition, int itemsAmountPerPage, out decimal paymentBalance, out decimal locationCashPosition, out decimal totalLocationBalance, out PaymentFlowData[] list, out long itemsTotal)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetStationPaymentFlowData(stationNumber, currentPosition, itemsAmountPerPage, out paymentBalance, out locationCashPosition, out totalLocationBalance, out list, out itemsTotal);
                }
            }
        }

        public bool AddPaymentFlow(PaymentFlowData request)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.AddPaymentFlow(request);
                }
            }
        }

        public string RegisterPaymentNote(uid uid, ref decimal amountRef, out DateTime expiration, out bool moneyWithdraw, out string number, out bool withdrawFrombalance)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.RegisterPaymentNote(uid, ref amountRef, out expiration, out moneyWithdraw, out number, out withdrawFrombalance);
                }
            }
        }

        public bool SaveCreditNote(uid uid, string number, string checkSum, decimal amount, string stationNumber, out PayoutType payoutType)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.SaveCreditNote(uid, number, checkSum, amount, stationNumber, out payoutType);
                }
            }
        }

        public CreditNoteWS LoadCreditNote(string number, string checksum, string stationNumber)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.LoadCreditNote(number, checksum, stationNumber);
                }
            }
        }

        public IdCardInfoItem[] GetIdCardInfo(int accountId, Role user)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetIdCardInfo(accountId, user);
                }
            }
        }

        public TicketWS[] LoadStoredTickets(uid uid, string stationNumber, string ticketnumber, string pin, string checkSum)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.LoadStoredTickets(uid, stationNumber, ticketnumber, pin, checkSum);
                }
            }
        }

        public decimal GetBalance(uid getUid, out decimal reserved, out decimal factor)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetBalance(getUid, out reserved, out factor);
                }
            }
        }

        public bool ValidateIdCard(string cardNumber, string stationNumber, bool isShop, out SessionWS sessionId)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.ValidateIdCard(cardNumber, stationNumber, isShop, out sessionId);
                }
            }
        }

        public string StoreTicket(string sTransactionId, uid uid, TicketWS ticketData, string pin, bool bIsOffLineTicket, string sStationNumber)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.StoreTicket(sTransactionId, uid, ticketData, pin, bIsOffLineTicket, sStationNumber);
                }
            }
        }

        public accountBalance Deposit(string sTransactionId, uid uId, decimal decMoneyAmount, bool realMoney, CashAcceptorType? type, bool depositFromCashpool)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.Deposit(sTransactionId, uId, decMoneyAmount, realMoney, depositFromCashpool, type);
                }
            }
        }

        public decimal CashOut(uid userUID, string stationNumber, out DateTime lastCashout, string comment, out DateTime enddate)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.CashOut(userUID, stationNumber, comment, out lastCashout, out enddate);
                }
            }
        }

        public bool DepositByTicket(string sTransactionId, uid uId, string ticketNumber, string ticketCode, string creditNoteNumber, string creditNoteCode)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.DepositByTicket(sTransactionId, uId, ticketNumber, ticketCode, creditNoteNumber, creditNoteCode);
                }
            }
        }

        public bool? WriteRemoteError2Log(string message, int criticality, string objectId, string msgTerminal)
        {
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.WriteRemoteError2Log(message, criticality, objectId, msgTerminal);
                }
            }
        }

        public string GetStationByVerificationNumber(string sVerificationCode)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetStationByVerificationNumber(sVerificationCode);
                }
            }
        }

        public bool? CancelTicket(string number, string checksum, string stationNumber)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.CancelTicket(number + checksum, stationNumber);
                }
            }
        }

        public string SaveTicket(string transactionId, uid uid, TicketWS ticketData, bool isOffLineTicket, string station_id, out long[] tipLock, out long[] tournamentLock)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.SaveTicket(transactionId, uid, ticketData, isOffLineTicket, station_id, out tipLock, out tournamentLock);
                }
            }
        }

        public long[] GetLockedOffer(string sStationNumber, out long[] arrLockedTournamentIds)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetLockedOffer(sStationNumber, out arrLockedTournamentIds);
                }
            }
        }

        public UserComment[] LoadUserComments(long uid)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.LoadUserComment(uid);
                }
            }
        }

        public bool EditUserComments(long uid, uid operatorId, int commentId, string comment)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.EditUserComment(uid, operatorId, commentId, comment);
                }
            }
        }

        public bool AddUserComment(long uid, uid operatorId, string comment)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.AddUserComment(uid, operatorId, comment);
                }
            }
        }

        public bool DeleteUserComment(int commentId)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.DeleteUserComment(commentId);
                }
            }
        }
        public bool StationEvent(string stationNumber, StationEventType eventType)
        {
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.StationEvent(stationNumber, eventType);
                }
            }
        }

        public ProfitAccountingCheckpoint GetProfitAccountingReport(int id, string station, DateTime? startDate, DateTime? endDate)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetProfitAccountingReport(id, station, startDate, endDate);
                }
            }
        }

        public valueField[] GetAccountByRegistrationNote(string registration_note_number, int franchisorId)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetAccountByRegistrationNote(registration_note_number, franchisorId);
                }
            }
        }

        public string GetLiveStreamFeed()
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetLiveStreamFeed();
                }
            }
        }

        public OperatorShiftData GetOperatorShiftReport(int locationId, short operatorId)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetOperatorShiftReport(locationId, operatorId);
                }
            }
        }

        public string GetStationResource(int resourceId)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetStationResource(resourceId);
                }
            }
        }

        public decimal GetCashInfo(string station, out decimal totalStationCash, out decimal locationCashPosition, out decimal totalLocationCash, out decimal totalLocationPaymentBalance)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetCashInfo(station, out totalStationCash, out locationCashPosition, out totalLocationCash, out totalLocationPaymentBalance);
                }
            }
        }

        public string GetContentManagementData(string stationNr)
        {
            lock (SecureKey)
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("StationNumber", Stationnumber);
                httpRequestProperty.Headers.Add("terminalSessionKey", SecureKey);

                using (var contextScope = new OperationContextScope(Client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                    return Client.GetContentManagementData(stationNr);
                }
            }
        }
    }

}
