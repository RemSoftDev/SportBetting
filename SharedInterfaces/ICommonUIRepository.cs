using System;
using System.Collections.Generic;
using System.Globalization;
using Shared;
using Shared.Statistics;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;

//using Nbt.IDCardReader;

namespace SharedInterfaces
{

    public delegate bool UpdateStation(bool isMandatory);
    public interface ICommonUIRepository
    {
        void Refresh();
        void RecalculateTicket(Ticket ticket, Operator operatorUser);
        Ticket SaveTicket(Ticket ticket, Operator operatorUser);
        Ticket VerifyTicket(Ticket ticket, Operator operatorUser);
        TicketDetails LoadTicket(string number, string checksum, string language);
        Ticket LoadStoreTicket(User user, string number, string checksum, string pin);
        bool CancelTicket(Shared.Operator operatorUser, string number, string checksum);
        bool UnbindIdCard(string number);
        void SetStationNumber(string number);
        User LoginUser(string username, string password);
        User AnonymousUser();
        Operator LoginOperator(string username, string password);
        User CardLoginUser(string number, string pin, Operator operatorUser);
        Operator CardLoginOperator(string number, string pin);

        IList<User> GetActivePlayers();
        IList<Ticket> GetTicketsForPlayer(User user);

        void RemoveTicket(string number, string checksum);


        void SaveLog(string message, LogParams param);

        bool DepositMoney(Shared.Operator oper, User user, decimal amount);
        bool DeposiByCreditNote(string creditNotenumber, string checksum, User user);
        bool DeposiByTicket(string ticketnumber, string checksum, User user);
        decimal GetBalance(User user);

        bool PayPaymentNote(string number, User operatorUser);
        bool PayCreditNote(string number, string checksum, User operatorUser, decimal amount);
        bool PayTicket(string number, string checksum, decimal amount, Operator operatorUser);

        bool WithdrawMoney(User user, decimal amount, Operator oper);
        bool WithdrawFromCashPool(decimal amount, Operator oper);
        PaymentNote CreatePaymentNote(decimal amount, User user);
        CreditNote CreateCreditNote(decimal amount, Operator oper);
        PaymentNote LoadPaymentNote(string number, User operatorUser);
        CreditNote LoadCreditNote(string number, string checksum);

        FormField[] GetFormFields();
        IList<SmallTicketInfo> LoadUserTickets(User user, int startIndex, int pageSize, out int total);
        IList<UserBalanceEntry> LoadUserBalance(User user, int startIndex, int pageSize, DateTime startDate, DateTime endDate, out int total);
        IList<UserCommentEntry> LoadUserComments(User user);
        bool AddUserComment(User user, User operatorUser, UserCommentEntry comment);
        bool EditUserComment(User user, User operatorUser, UserCommentEntry comment);
        bool DeleteUserComment(User user, UserCommentEntry comment);
        bool CloseSession(string sessionid);
        IList<UserProperties> LoadUserProfile(User user);
        IList<UserProperties> UpdateUserProfile(long accountId, User user);
        User RegisterUser(long accountId, IList<UserProperties> userProperties);
        bool ChangeUserPassword(User user, string newPassword, string oldPassword);

        bool VerifyUser(User user);
        bool BlockUser(long accountId, User user);
        bool UnBlockUser(long accountId, User user);

        bool BlockUserCard(User user, string cardnumber);
        string BindUserCard(User user);
        string ChangeUserCardPin(User user, string newPin, string oldpin);

        IList<User> SearchUsers(IList<UserProperties> userProperties, Operator operatorUser);

        Dictionary<string, string> LoadOddSheetPresets();
        void SaveOddSheetPreset(string name, string value);
        void DeleteOddSheetPreset(string name);

        ICollection<GroupVw> GetAllSports();
        ICollection<GroupVw> GetCountriesForSport(long sportId);
        ICollection<GroupVw> GetTournamentsForCountry(long sportId, long countryId);

        /// <summary>
        /// Returns a list of filtered matches that are open for bets (are not finished yet).
        /// </summary>
        /// <param name="language">client's language for localization</param>
        /// <param name="filter">match filter object by which the matches are filtered</param>
        /// <returns></returns>
        IList<IMatchVw> GetOddSheetMatches(string language, MatchesFilter filter);
        IList<TournamentSheet> GetOddSheetTournamentStatistic(long matchLn);
        IList<FormSheet> GetOddSheetFormStatistic(long matchLn);

        bool OperatorLogout(string sessionid);
        Ticket LoadLastTicket(string language);
        IList<Operator> SearchOperators(IList<UserProperties> userProperties, Operator operatorUser);
        IList<UserProperties> LoadOperatorProfile(Operator user, Operator operatorUser);
        IList<UserProperties> UpdateOperatorProfile(Operator user);
        Operator RegisterOperator(IList<UserProperties> userProperties, Operator operatorUser);
        bool ChangeOperatorPassword(Operator user, string newPassword, string oldPassword);

        bool BlockOperator(Operator operatorUser);
        bool UnBlockOperatorUser(Operator operatorUser);

        bool BlockOperatorCard(Operator user, string cardnumber);
        string BindOperatorCard(Operator user);
        string ChangeOperatorCardPin(Operator user, string newPin, string oldpin);

        string VerifyStation(string number);

        #region --- till operations ---

        /// <summary>
        /// Returns amount of cash in till
        /// 
        /// till = A drawer, small chest, or compartment for money, as in a store; similar to cashbox at terminal...
        /// </summary>
        /// <returns></returns>
        decimal MoneyTillBalance();

        /// <summary>
        /// called when operator puts some cash into till
        /// </summary>
        /// <param name="oper"></param>
        /// <param name="amount"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        bool MoneyTillCashIn(Shared.Operator oper, decimal amount, DateTime dateTime, bool withinShop);

        /// <summary>
        /// called when operator collects some cash from till
        /// </summary>
        /// <param name="oper"></param>
        /// <param name="amount"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        bool MoneyTillCashOut(Shared.Operator oper, decimal amount, DateTime dateTime, bool withinShop);

        /// <summary>
        /// returns collection of money till action from start to end date
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        IList<Shared.MoneyTillAction> MoneyTillHistory(DateTime start, DateTime end);

        /// <summary>
        /// return difference between stakes and winnings. That is: all accepted stakes minus all winnings (paid out or not) in period
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        decimal GetBettingBalance(DateTime start, DateTime end);

        #endregion

        #region --- preorder payout ---

        /// <summary>
        /// optional for version 1
        /// </summary>
        /// <param name="oper"></param>
        /// <param name="player"></param>
        /// <param name="desiredDate"></param>
        /// <param name="desiredAmount"></param>
        /// <returns></returns>
        Shared.PreOrderPayOut CreatePreOrderPayOut(Shared.Operator oper, Shared.User user, DateTime desiredDate, decimal desiredAmount);

        /// <summary>
        /// optional for version 1
        /// </summary>
        /// <param name="preorder"></param>
        /// <param name="oper"></param>
        /// <returns></returns>
        Shared.PreOrderPayOut UpdatePreOrderPayOut(Shared.PreOrderPayOut preorder, Shared.Operator oper);

        /// <summary>
        /// optional for version 1. Returns all preorders for given user which are NOT in status Finalized
        /// </summary>
        /// <param name="oper"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        IList<Shared.PreOrderPayOut> GetActivePreOrderPayOuts(Shared.Operator oper, Shared.User user);

        #endregion

        #region --- Secondary (player) screen ---

        /// <summary>
        /// gets top text for player screen
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        string GetTopText(CashierShopContext context);

        /// <summary>
        /// gets main content for player screen. This content is grid. 
        /// <br/>
        /// Expected xml in form (number of columns and rows can vary):
        /// <br/>
        /// &lt;root&gt;
        /// &lt;page&gt;
        /// &lt;header&gt;
        /// &lt;row index="0"&gt;
        /// &lt;column index="0" text="Match" /&gt;
        ///	    &lt;column index="1" text="Tournament" /&gt;
        ///		    &lt;column index="2" text="Selection" /&gt;
        ///		    &lt;column index="3" text="Odds" /&gt;
        ///       &lt;/row&gt;
        ///	&lt;/header&gt;
        ///	&lt;content&gt;
        ///       &lt;row index="0"&gt;
        ///	    &lt;column index="0" text="Real : Barcelona" /&gt;
        ///	    &lt;column index="1" text="Primiera" /&gt;
        ///	    &lt;column index="2" text="Real wins" /&gt;
        ///		    &lt;column index="3" text="2.1" /&gt;
        ///      &lt;/row&gt;
        ///     &lt;row index="1"&gt;
        ///	    &lt;column index="0" text="Milan : Juventus" /&gt;
        ///	    &lt;column index="1" text="Cup Italia" /&gt;
        ///		    &lt;column index="2" text="Milan wins" /&gt;
        ///		    &lt;column index="3" text="1.8" /&gt;
        ///        &lt;/row&gt;
        ///	&lt;/content&gt;
        ///	&lt;footer&gt;
        ///       &lt;row index="0"&gt;
        ///		    &lt;column index="0" text="" /&gt;
        ///		    &lt;column index="0" text="" /&gt;
        ///		    &lt;column index="2" text="Total" /&gt;
        ///		    &lt;column index="3" text="3.78" /&gt;
        ///       &lt;/row&gt;
        ///	&lt;/footer&gt;
        ///	&lt;/page&gt;
        /// &lt;/root&gt;
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        string GetContentGrid(CashierShopContext context);

        /// <summary>
        /// gets bottom text for player screen
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        string GetBottomText(CashierShopContext context);

        #endregion


        bool EmptyBox(Operator operatorUser, decimal amount);
        BalanceCheckpoint CreateBalanceCheckpoint(Operator operatorUser, decimal amount);
        IList<BalanceCheckpoint> LoadCheckpoints(Operator operatorUser, decimal amount, int startIndex, int pagesize);

        event UpdateStation UpdateStationEvent;

        bool AllowAnonymousBetting { get; }
        int AutoLogoutWindowLiveTimeInSec { get; }
        bool BarcodeScannerAlwaysActive { get; }
        bool BarcodeScannerTempActive { get; }
        string Currency { get; }
        Dictionary<string, string> HubSettings { get; }


        int PaymentNoteLength { get; }
        int TicketNumberLength { get; }
        int TicketCheckSumLength { get; }
        int CreditNoteNumberLength { get; }
        int CreditNoteCheckSumLength { get; }

        string DefaultDisplayLanguage { get; }

        bool IsPrematchEnabled { get; }
        bool IsLiveMatchEnabled { get; }

        int Active { get; }
        bool UsePrinter { get; }
        int PrinterStatus { get; }

        string StationNumber { get; }
        string StationName { get; }
        string FranchiserName { get; }
        string LocationName { get; }
        int SyncInterval { get; }
        Dictionary<string, string> CultureInfos { get; }
        IEnumerable<UserRole> GetAllRoles(Operator oper);
        string GetBuildVersion();

        ICollection<GroupVw> GetCategoriesForSport(long id);
        ICollection<GroupVw> GetTournamentsForCategory(long id, long categoryId);
    }
}
