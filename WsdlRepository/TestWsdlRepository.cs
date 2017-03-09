using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using WsdlRepository;
using WsdlRepository.WsdlServiceReference;

namespace WcfService
{
    public class TestWsdlRepository : IWsdlRepository
    {
        private decimal _tempBalance = 0;
        private string defaultTicketNumber = "0000140000000";
        private string _idCardNumber = "";
        private bool _idCardActive = false;
        private int _accountIdWithIdCard = 0;
        ObservableCollection<TicketWS> _ticketsList = new ObservableCollection<TicketWS>();

        public string GetContentManagementData(string stationNr)
        {
            return "";
        }

        public CashOperationtData[] GetStationCashHistory(string stationId, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public decimal GetStationCashInfo(string station, out decimal billsAmount, out int billscount, out decimal coinsamount, out int coinscount)
        {
            throw new NotImplementedException();
        }

        public CashOutItem[] GetCasheOuts(string stationnumber)
        {
            throw new NotImplementedException();
        }

        public string Number { get; set; }


        public string SecureKey { get; set; }

        public UpdateRecord[] UpdateLine(string stationNumber, long lastUpdateFileId, DateTime lastUpdate, out int? total)
        {
            total = 0;
            return null;
        }

        public UpdateRecord[] UpdateLocalization(string stationNumber, long lastUpdateFileId)
        {
            return null;
        }

        public long GetBusinessProps(string stationNumber, out long lastTicketNumber, out long lastTransactionId)
        {
            if (_ticketsList.Count() != 0)
            {
                lastTicketNumber = Convert.ToInt64(_ticketsList.Last().ticketNbr);
            }
            else
            {
                lastTicketNumber = Convert.ToInt64(defaultTicketNumber);
            } 
            lastTransactionId = 0;
            return 0;
        }

        public string SaveTicket(string transactionId, uid uid, TicketWS ticketData, bool isOffLineTicket, string station_id,
                                 out long[] tipLock, out long[] tournamentLock)
        {
            if(ConnectionProblem)
                throw new EndpointNotFoundException();
            _ticketsList.Add(ticketData);
            _tempBalance -= ticketData.stake;
            tipLock = null;
            tournamentLock = null;
            return "1";
        }

        public bool ConnectionProblem { get; set; }

        public TicketWS LoadTicket(string number, string checksum, string stationNumber, string lang, string culture, bool showDetails)
        {
            foreach (var ticket in _ticketsList)
            {
                if (ticket.ticketNbr == number)
                {
                    for (int i = 0; i < ticket.bets.Length; i++)
                    {
                        for (int y = 0; y < ticket.bets[i].bankTips.Length; y++)
                        {
                            TipDetailsWS tDetails = new TipDetailsWS();
                            ticket.bets[i].bankTips[y].tipDetailsWS = tDetails;
                            ticket.bets[i].bankTips[y].tipDetailsWS.tip = "Test Match";
                            ticket.bets[i].bankTips[y].tipDetailsWS.startdate = DateTime.Now;
                            ticket.bets[i].bankTips[y].tipDetailsWS.team1 = "Team 1";
                            ticket.bets[i].bankTips[y].tipDetailsWS.team2 = "Team 2";
                            ticket.bets[i].bankTips[y].tipDetailsWS.event_name = "Team 1 : Team 2";
                            ticket.bets[i].bankTips[y].tipDetailsWS.betDomainName = "Test BetDomain";
                            ticket.bets[i].bankTips[y].tipDetailsWS.specialOddValue = "-1";
                            ticket.bets[i].bankTips[y].tipDetailsWS.specialLiveOddValueFull = "";
                        }
                         
                    }
                   
                    return ticket;
                }
            }
            return null;
        }

        public TicketWS[] LoadStoredTicket(uid userUid, string number, string checksum, string stationNumber, string pin)
        {
            throw new NotImplementedException();
        }

        public bool? CancelTicket(string number, string checksum, string stationNumber)
        {
            throw new NotImplementedException();
        }

        public SessionWS OpenSession(string station_id, bool anonymous, string username, string password, bool isShop)
        {
            SessionWS session = new SessionWS();
            session.cardNumber = "1";
            if (username == "test" && password == "mode")
            {
                session.account_id = 1;
                session.roleName = "user";
                session.balance = new accountBalance { amount = 1000, reserved = 0 };
                session.hasActiveCard = false;
                session.default_language = "EN";
                session.highlight_color = "red";
                session.permissions = null;
                session.role_id = 0;
                _tempBalance = 1000;
                session.cardNumber = "";
                session.session_id = "1003720140211110037882RSUQ6Y38XL";
                return session;
            }
            else if (username == "test2" && password == "mode")
            {
                session.account_id = 2;
                session.roleName = "user";
                session.balance = new accountBalance { amount = 1000, reserved = 0 };
                session.hasActiveCard = false;
                session.default_language = "EN";
                session.highlight_color = "red";
                session.permissions = null;
                session.role_id = 0;
                _tempBalance = 1000;
                session.cardNumber = "";
                session.session_id = "1003720140211110037882RSUQ6Y38XL";
                return session;
            }
            else if (username == "@oper" && password == "check")
            {
                session.account_id = 3;
                session.roleName = "Operator";
                session.hasActiveCard = false;
                session.default_language = "EN";
                session.balance = new accountBalance();
                session.highlight_color = "red";
                session.permissions = new string[2] { "access_usermanagement", "usermanagement_bind_id_card" };
                session.role_id = (int) Role.Operator;
                session.cardNumber = "";
                session.session_id = "1003720140211110037882RSUQ6Y38XL";
                return session;
            }

            if (!String.IsNullOrEmpty(username))
            {
                var exception = new FaultException<HubServiceException>(new HubServiceException());
                exception.Detail.code = 111;
                throw exception;
            }

            session.account_id = 1;
            session.roleName = "user";
            session.balance = new accountBalance();
            session.default_language = "EN";
            session.hasActiveCard = false;
            session.highlight_color = "red";
            session.permissions = null;
            session.role_id = 0;
            session.cardNumber = "";

            session.session_id = "00037201402101641406621IF5UZC044";
            return session;
        }

        public SessionWS LoginWithIdCard(string station_id, string cardNumber, string pin, bool isShopSession = false)
        {
            SessionWS session = new SessionWS();
            if (cardNumber == _idCardNumber && pin == "0000")
            {
                
                switch (_accountIdWithIdCard)
                {
                    case 1:
                        session.account_id = 1;
                        session.session_id = "1003720140211110037882RSUQ6Y38XL";
                        session.username = "test";
                        session.default_language = "EN";
                        session.roleName = "user";
                        session.balance = new accountBalance { amount = 1000, reserved = 0 };
                        break;
                    case 2:
                        session.account_id = 1;
                        session.session_id = "1003720140211110037882RSUQ6Y38XL";
                        session.username = "test1";
                        session.default_language = "EN";
                        session.roleName = "user";
                        session.balance = new accountBalance { amount = 1000, reserved = 0 };
                        break;
                    default: 
                        break;
                }       
            }
            return session;
        }

        public UserTicket[] GetUserTickets(string accountId, ticketCategory ticketCategory, AccountTicketSorting sorting,
                                           int startIndex, int pageSize, out string totalNumber)
        {
            throw new NotImplementedException();
        }

        public UpdateRecord[] GetLatestConfidenceFactorsUpdate(string stationNumber)
        {
            return null;
        }

        public bool CloseSession(string sessionid)
        {
            throw new NotImplementedException();
        }

        public profileForm LoadProfile(uid uid)
        {
            profileForm profForm = new profileForm();
            profForm.fields = new formField[1];

            formField usernameField = new formField();
            usernameField.name = "username";
            usernameField.mandatory = true;
            usernameField.@readonly = true;
            usernameField.searchable = true;
            usernameField.hidden = false;
            usernameField.type = "STRING";
            usernameField.sequence = 1;

            switch (uid.account_id)
            {
                case "1":
                    usernameField.value = "test";
                    break;
                case "2":
                    usernameField.value = "test1";
                    break;
                default:
                    usernameField.value = "";
                    break;
            }

            profForm.fields[0] = usernameField;

            return profForm;
        }

        public string UpdateProfile(long? operatorId, uid uidFromUser, valueForm valueForm)
        {
            throw new NotImplementedException();
        }



        public int RegisterAccount(long? operatorId, valueForm valueForm, string stationNumber, bool isVerified, out string registration_note_number)
        {
            throw new NotImplementedException();
        }

        public string UpdateProfile(uid uidFromUser, valueForm valueForm)
        {
            throw new NotImplementedException();
        }

        public int RegisterAccount(valueForm valueForm, string stationNumber, bool isVerified, out string registration_note_number)
        {
            throw new NotImplementedException();
        }

        public bool ChangePasswordFromTerminal(uid uid, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public string EndUserVerification(uid uid)
        {
            throw new NotImplementedException();
        }

        public bool ToggleAccountState(long accountId, long operatorId)
        {
            throw new NotImplementedException();
        }

        public bool ToggleAccountState(int accountId)
        {
            throw new NotImplementedException();
        }

        public string UpdateIdCard(string cardnumber, string status, bool active, long? accountId)
        {
            _idCardActive = active;
            _idCardNumber = cardnumber;
            if (accountId != null)
            {
                _accountIdWithIdCard = (int) accountId;
            }
            return "true";
        }

        public string RegisterIdCard(string number, bool active, string franchizorId, out string pin)
        {
            pin = "0000";
            return _idCardNumber;
        }

        public void ChangeIDCardPin(uid uid, ref string newPin)
        {
            throw new NotImplementedException();
        }

        public void ChangeOperatorIDCardPin(long operatorId, ref string newPin)
        {
            throw new NotImplementedException();
        }

        public valueField[][] AccountSearch(criteria[] criterias, uid uid)
        {
            foreach (var criteria in criterias)
            {
                if (criteria.value.Contains("test"))
                {
                    valueField username = new valueField();
                    username.name = "username";
                    username.value = "test";

                    valueField active = new valueField();
                    active.name = "active";
                    active.value = "true";

                    valueField account_id = new valueField();
                    account_id.name = "account_id";
                    account_id.value = "1";

                    valueField verified = new valueField();
                    verified.name = "verified";
                    verified.value = "1";

                    valueField cardStatus = new valueField();
                    cardStatus.name = "card_status";
                    cardStatus.value = _accountIdWithIdCard == 1 ? "1" : "0";

                    valueField[] account1 = new valueField[] { username, active, account_id, verified, cardStatus };

                    valueField username2 = new valueField();
                    username2.name = "username";
                    username2.value = "test1";

                    valueField active2 = new valueField();
                    active2.name = "active";
                    active2.value = "true";

                    valueField account_id2 = new valueField();
                    account_id2.name = "account_id";
                    account_id2.value = "2";

                    valueField verified2 = new valueField();
                    verified2.name = "verified";
                    verified2.value = "1";

                    valueField cardStatus2 = new valueField();
                    cardStatus2.name = "card_status";
                    cardStatus2.value = _accountIdWithIdCard == 2 ? "1" : "0";

                    valueField[] account2 = new valueField[] { username2, active2, account_id2, verified2, cardStatus2 };

                    valueField[][] retVal = new valueField[][] { account1, account2 };

                    return retVal;
                }
            }
            return null;
        }

        public Operator[] SearchForOperators(OperatorCriterias criterias, uid uid)
        {
            throw new NotImplementedException();
        }

        public int RegisterOperator(uid uidFromUser, string firstName, string lastName, string username, string password,
                                    string language, int roleId, string email)
        {
            throw new NotImplementedException();
        }

        public bool UpdateOperator(long accountId, OperatorCriterias criterias)
        {
            throw new NotImplementedException();
        }

        public string BindOperatorIdCard(long accountId, string cardNumber, out string pin)
        {
            throw new NotImplementedException();
        }

        public string CreateCheckpoint(string stationNumber, long accountId, out decimal amount)
        {
            throw new NotImplementedException();
        }

        public AccountingRecieptWS GetAccountingRecieptData(DateTime startDate, DateTime endDate, string stationNumber, int? locationId)
        {
            throw new NotImplementedException();
        }

        public string GetAccountByTicket(string ticketNumber)
        {
            throw new NotImplementedException();
        }

        public bool ChangePasswordFromShop(int operatorId, uid getUid, string newPassword)
        {
            throw new NotImplementedException();
        }

        public bool ChangePasswordFromShop(uid getUid, string newPassword)
        {
            throw new NotImplementedException();
        }

        public string History(uid uid, string type, string operationgroup, string startIndex, string limit, DateTime? startDate,
                              DateTime? endDate, out historyData[] history, bool isFilterTrash = true)
        {
            throw new NotImplementedException();
        }

        public ArrayOfProfitAccountingCheckpoints GetProfitAccountingCheckpoint(int objectId, string stationNumber, int currentPosition,
                                                                                int itemsAmountPerPage)
        {
            throw new NotImplementedException();
        }

        public ProfitAccountingCheckpoint CreateProfitAccountingCheckpoint(int locationId, string stationNumber)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StationBalanceCheckpoint> GetStationBalanceCheckpoints(string stationNumber, int startIndex, int pageSize)
        {
            throw new NotImplementedException();
        }

        public decimal GetStationPaymentFlowData(string stationNumber, int currentPosition, int itemsAmountPerPage, out decimal paymentBalance, out decimal locationCashPosition, out decimal totalLocationBalance, out PaymentFlowData[] list, out long itemsTotal)
        {
            throw new NotImplementedException();
        }

        public bool AddPaymentFlow(PaymentFlowData request)
        {
            throw new NotImplementedException();
        }

        public string RegisterPaymentNote(uid uid, ref decimal amountRef, out DateTime expiration, out bool moneyWithdraw,
                                          out string number, out bool withdrawFrombalance)
        {
            throw new NotImplementedException();
        }

        public bool SaveCreditNote(uid iud, string number, string checkSum, decimal amount, string stationNumber,
                                   out PayoutType payoutType)
        {
            throw new NotImplementedException();
        }

        public CreditNoteWS LoadCreditNote(string number, string checksum, string stationNumber)
        {
            throw new NotImplementedException();
        }

        public IdCardInfoItem[] GetIdCardInfo(int accountId, Role user)
        {
            IdCardInfoItem cardItem = new IdCardInfoItem();
            cardItem.number = "1201402251632025088BTTTDN";
            cardItem.registration_status = "1";
            cardItem.active = _idCardActive ? "1" : "0";

            IdCardInfoItem[] items = new IdCardInfoItem[1];
            items[0] = cardItem;
            return items;
        }

        public TicketWS[] LoadStoredTickets(uid uid, string stationNumber, string ticketnumber, string pin, string checkSum)
        {
            throw new NotImplementedException();
        }

        public decimal GetBalance(uid getUid, out decimal reserved, out decimal factor)
        {
            factor = 1;
            reserved = 0;
            return _tempBalance;
        }

        public bool ValidateIdCard(string cardNumber, string stationNumber, bool isShop, out SessionWS sessionId)
        {
            sessionId = null;
            return true;
        }

        public string StoreTicket(string sTransactionId, uid uid, TicketWS ticketData, string pin, bool bIsOffLineTicket,
                                  string sStationNumber)
        {
            throw new NotImplementedException();
        }

        public accountBalance Deposit(string sTransactionId, uid uId, decimal decMoneyAmount, bool realMoney, CashAcceptorType? type, bool depositFromCashpool)
        {
            _tempBalance += decMoneyAmount;
            return new accountBalance {amount = _tempBalance, reserved = 0};
        }

        public decimal CashOut(uid userUID, string stationNumber, out DateTime lastCashout, string comment, out DateTime enddate)
        {
            throw new NotImplementedException();
        }

        public decimal CashOut(uid userUID, string stationNumber, ref decimal money, string getMoneyOutOfCashBox, out DateTime lastCashout, string comment, DateTime starDate)
        {
            throw new NotImplementedException();
        }


        public bool DepositByTicket(string sTransactionId, uid uId, string ticketNumber, string ticketCode, string creditNoteNumber,
                                    string creditNoteCode)
        {
            throw new NotImplementedException();
        }

        public bool? WriteRemoteError2Log(string message, int criticality, string objectId, string msgTerminal)
        {
            throw new NotImplementedException();
        }

        public string GetStationByVerificationNumber(string sVerificationCode)
        {
            throw new NotImplementedException();
        }

        public StationWS GetStationProperties(string stationNumber, string sIpAddress, string teamViewerId, string serverVersion,
                                              PeripheralInfo pi, out valueForm hubResponse,
                                              out BsmHubConfigurationResponse hubConfigurationResponse, DriverInfo[] drivers)
        {
            StationWS testProperties = new StationWS();
            testProperties.isAnonymousBettingEnabled = true;
            testProperties.isStatisticsEnabled = true;
            testProperties.allowMultiBet = false;
            testProperties.allowOffLine = false;
            testProperties.barcodeScannerAlwayActive = false;
            testProperties.stationAutorestartTime = "NONE";
            testProperties.doRestart = 0;
            testProperties.active = 3;
            testProperties.propertyPrematch = 1;
            testProperties.propertyLiveMatch = 0;
            testProperties.printingDefaultLanguage = 7;
            testProperties.printingLanguageSetting = 1;
            testProperties.propertyIdCard = 1;
            testProperties.propertyAutoLogout = 0;
            testProperties.stationName = "Test Terminal";
            testProperties.locationName = "Test Location";
            testProperties.franchisorName = "Test Franchisor";
            testProperties.userCardPinSetting = 3;
            testProperties.operatorCardPinSetting = 1;
            testProperties.authorizedTicketScanning = true;
            testProperties.creditNoteExpireDays = 60;
            testProperties.payoutExpiredPaymentCreditNotes = false;
            testProperties.stationStoreTicketEnabled = false;
            testProperties.allowMixedStakes = true;
            testProperties.multiBetBonusFromOdd = 2;
            testProperties.maxStakeSingleBet = 500;
            testProperties.logRotationFileSize = 20;
            testProperties.logRotationNumber = 50;
            testProperties.maxOdd = 1000;
            testProperties.minStakeCombiBet = 1;
            testProperties.maxStakeCombi = 500;
            testProperties.maxWinSingleBet = 2000;
            testProperties.minStakeSingleBet = 1;
            testProperties.maxStakeSystemBet = 1000;
            testProperties.minStakeSystemBet = 1;
            testProperties.maxWinSystemBet = 10000;
            testProperties.displayTaxField = false;
            testProperties.maxSystemBet = 4;
            testProperties.stationAllowFutureMatches = true;
            testProperties.enableOddsChangeIndication = true;
            testProperties.sngLiveBetTicketAcceptTime = 30;
            testProperties.combiLiveBetTicketAcceptTime = 0;
            testProperties.minCombination = 2;
            testProperties.maxCombination = 20;
            testProperties.printLogo = true;
            testProperties.syncInterval = 1000;
            testProperties.cashAcceptorAlwayActive = true;
            testProperties.franchisorID = 777;
            testProperties.locationID = 888;
            testProperties.vflVideoSourceType = 0;
            testProperties.bonusRanges = new BonusRangeWS[0];
            testProperties.combiLimits = new CombiLimitWS[0];

            hubResponse = new valueForm();
            hubResponse.fields = new valueField[0];
            
            hubConfigurationResponse = new BsmHubConfigurationResponse();

            return testProperties;
        }

        public registrationForm GetRegistrationForm()
        {
            registrationForm regForm = new registrationForm();
            regForm.fields = new formField[1];

            formField usernameField = new formField();
            usernameField.name = "username";
            usernameField.mandatory = true;
            usernameField.@readonly = true;
            usernameField.searchable = true;
            usernameField.hidden = false;
            usernameField.type = "STRING";
            usernameField.sequence = 1;

            regForm.fields[0] = usernameField;


            return regForm;
        }

        public IEnumerable<UserRole> GetAllRoles(string operatorId)
        {
            throw new NotImplementedException();
        }

        public bool DepositByCreditNote(string sTransactionId, uid uId, string noteNumber, string checkSum)
        {
            throw new NotImplementedException();
        }

        public string IssueCreditToStationBalance(uid uid, decimal amount)
        {
            throw new NotImplementedException();
        }


        public SettlementHistory[] GetSettlementHistory(uid accountUid)
        {
            throw new NotImplementedException();
        }

        public OperatorShiftCheckpoint[] GetOperatorShiftCheckpoints(int locationId, short operatorId, out decimal tempBalance, bool isFilterByOperator)
        {
            throw new NotImplementedException();
        }

        public OperatorShiftData CreateOperatorShiftCheckpoint(int locationId, short operatorId)
        {
            throw new NotImplementedException();
        }

        public OperatorWithShift[] GetAllOperatorsWithShifts(uid uid)
        {
            throw new NotImplementedException();
        }

        public SettlementHistoryDetail[] GetSettlementHistoryDetails(int id)
        {
            throw new NotImplementedException();
        }

        public bool CheckForEmptyBoxAndPayments(int locationId, int id)
        {
            throw new NotImplementedException();
        }

        public profileForm LoadPaymentNote(string paymentNoteNumber, uid getUid, out decimal amount)
        {
            throw new NotImplementedException();
        }

        public bool PayCreditNote(uid uid, string number, string code, string accountId, string stationNumber)
        {
            throw new NotImplementedException();
        }

        public string WithdrawByPaymentNote(string paymentNoteNumber, uid getUid, out decimal amount, out bool withFrombalance)
        {
            throw new NotImplementedException();
        }

        public DateTime ProduceOperatorSettlement(ref int operatorId, int accountId, out string opName, out string opLName,
                                                  out string frName, out string locName, out string locOwnerName, out DateTime stDate,
                                                  out DateTime enDate, out CheckpointSlip[] cpArray,
                                                  out TotalSettlementSection totSection)
        {
            throw new NotImplementedException();
        }

        public long[] GetLockedOffer(string sStationNumber, out long[] arrLockedTournamentIds)
        {
            arrLockedTournamentIds = null;
            return null;
        }

        public UserComment[] LoadUserComments(long uid)
        {
            throw new NotImplementedException();
        }

        public bool AddUserComment(long uid, uid operatorId, string comment)
        {
            throw new NotImplementedException();
        }

        public bool DeleteUserComment(int commentId)
        {
            throw new NotImplementedException();
        }

        public bool EditUserComments(long uid, uid operatorId, int commentId, string comment)
        {
            throw new NotImplementedException();
        }

        public UpdateRecord[] UpdateFlags(string stationNumber, long id)
        {
            return null;
        }

        public UpdateRecord[] UpdateStatistics(string stationNumber, long id)
        {
            return new UpdateRecord[0];
        }

        public bool StationEvent(string stationNumber, StationEventType eventType)
        {
            throw new NotImplementedException();
        }

        public bool RegisterCash(uid uid, decimal amount)
        {
            throw new NotImplementedException();
        }

        public UpdateRecord[] GetMetainfo(string stationNumber, long id)
        {
            return null;
        }

        public decimal GetBettingBalance(DateTime startDate, DateTime endDate, string stationNumber, out decimal amountWon)
        {
            throw new NotImplementedException();
        }

        public bool UnbindIdCard(string number)
        {
            throw new NotImplementedException();
        }

        public ProfitAccountingCheckpoint GetProfitAccountingReport(int id, string station, DateTime? startDate, DateTime? endDate)
        {
            throw new NotImplementedException();
        }

        public valueField[] GetAccountByRegistrationNote(string registration_note_number, int franchisorId)
        {
            throw new NotImplementedException();
        }

        public valueField[] GetAccountByRegistrationNote(string registration_note_number)
        {
            throw new NotImplementedException();
        }

        public string Matchsorting(string matches)
        {
            throw new NotImplementedException();
        }

        public string GetLiveStreamFeed()
        {
            throw new NotImplementedException();
        }

        public OperatorShiftData GetOperatorShiftReport(int locationId, short operatorId)
        {
            throw new NotImplementedException();
        }

        public string GetStationResource(int resourceId)
        {
            throw new NotImplementedException();
        }

        public decimal GetCashInfo(string station, out decimal totalStationCash, out decimal locationCashPosition,
                                   out decimal totalLocationCash, out decimal totalLocationPaymentBalance)
        {
            throw new NotImplementedException();
        }
    }
}
