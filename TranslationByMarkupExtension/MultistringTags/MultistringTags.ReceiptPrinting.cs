using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslationByMarkupExtension
{
    partial class MultistringTags
    {
        // Translations for the receipt printouts (bet ticket, payment note, ...)

        #region BET TICKET
        public static MultistringTag SHOP_RECEIPT_TICKET_DUPLICATE = MultistringTag.Assign("SHOP_RECEIPT_TICKET_DUPLICATE",""); // Duplicate (LJO)
        public static MultistringTag SHOP_RECEIPT_TICKET_HEADER = MultistringTag.Assign("SHOP_RECEIPT_TICKET_HEADER",""); // Bet Ticket (LJO)
        public static MultistringTag SHOP_RECEIPT_TICKET_NUMBER = MultistringTag.Assign("SHOP_RECEIPT_TICKET_NUMBER",""); // Ticket-Nr. (LJO)
        public static MultistringTag SHOP_RECEIPT_TICKET_CODE = MultistringTag.Assign("SHOP_RECEIPT_TICKET_CODE",""); // Code (LJO)
        public static MultistringTag SHOP_RECEIPT_TICKET_BET_TYPE = MultistringTag.Assign("SHOP_RECEIPT_TICKET_BET_TYPE",""); // Bet Type (LJO)
        public static MultistringTag SHOP_RECEIPT_TICKET_BET_TYPE_BANKER = MultistringTag.Assign("SHOP_RECEIPT_TICKET_BET_TYPE_BANKER",""); // Banker (LJO)
        public static MultistringTag SHOP_RECEIPT_TICKET_FEE = MultistringTag.Assign("SHOP_RECEIPT_TICKET_FEE",""); // Fee  (LJO)
        public static MultistringTag SHOP_RECEIPT_TICKET_BONUS = MultistringTag.Assign("SHOP_RECEIPT_TICKET_BONUS",""); // Bonus (LJO)
        public static MultistringTag SHOP_RECEIPT_TICKET_TOTAL_ODDS = MultistringTag.Assign("SHOP_RECEIPT_TICKET_TOTAL_ODDS",""); // Total Odds (LJO)
        public static MultistringTag SHOP_RECEIPT_TICKET_STAKE = MultistringTag.Assign("SHOP_RECEIPT_TICKET_STAKE",""); // Stake (LJO)
        public static MultistringTag SHOP_RECEIPT_TICKET_WINNINGS = MultistringTag.Assign("SHOP_RECEIPT_TICKET_WINNINGS",""); // Possible Winning (LJO)

        public static MultistringTag SHOP_ENUM_SHARED_TICKETSTATES_MULTY = MultistringTag.Assign("SHOP_ENUM_SHARED_TICKETSTATES_MULTY",""); // Multy (LJO)
        public static MultistringTag SHOP_ENUM_SHARED_TICKETSTATES_SYSTEM = MultistringTag.Assign("SHOP_ENUM_SHARED_TICKETSTATES_SYSTEM",""); // System (LJO)
        public static MultistringTag SHOP_ENUM_SHARED_TICKETSTATES_SINGLE = MultistringTag.Assign("SHOP_ENUM_SHARED_TICKETSTATES_SINGLE",""); // Single (LJO)
        public static MultistringTag SHOP_ENUM_SHARED_TICKETSTATES_MULTYSINGLES = MultistringTag.Assign("SHOP_ENUM_SHARED_TICKETSTATES_MULTYSINGLES",""); // Multy / Singles (LJO)

        public static MultistringTag SHOP_RECEIPT_TICKET_TIP_ODDS = MultistringTag.Assign("SHOP_RECEIPT_TICKET_TIP_ODDS",""); // Odds (LJO)
        #endregion

        #region PAYMENT NOTE
        public static MultistringTag SHOP_RECEIPT_PAYMENT_NOTE_HEADER = MultistringTag.Assign("SHOP_RECEIPT_PAYMENT_NOTE_HEADER",""); // Payment note (LJO)
        public static MultistringTag SHOP_RECEIPT_PAYMENT_NOTE_NUMBER = MultistringTag.Assign("SHOP_RECEIPT_PAYMENT_NOTE_NUMBER",""); // Payment-No: (LJO)
        public static MultistringTag SHOP_RECEIPT_PAYMENT_NOTE_AMOUNT = MultistringTag.Assign("SHOP_RECEIPT_PAYMENT_NOTE_AMOUNT",""); // Amount (LJO)
        #endregion

        #region CREDIT NOTE
        public static MultistringTag SHOP_RECEIPT_CREDIT_NOTE_HEADER = MultistringTag.Assign("SHOP_RECEIPT_CREDIT_NOTE_HEADER",""); // Credit note (LJO)
        public static MultistringTag SHOP_RECEIPT_CREDIT_NOTE_NUMBER = MultistringTag.Assign("SHOP_RECEIPT_CREDIT_NOTE_NUMBER",""); // Credit-No: (LJO)
        public static MultistringTag SHOP_RECEIPT_CREDIT_NOTE_AMOUNT = MultistringTag.Assign("SHOP_RECEIPT_CREDIT_NOTE_AMOUNT",""); // Amount (LJO)
        #endregion
    }
}
