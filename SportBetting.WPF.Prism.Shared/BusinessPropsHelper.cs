using System;
using System.Collections.Generic;
using System.Data;
using SportRadar.Common;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;
using WsdlRepository.WsdlServiceReference;

namespace SportBetting.WPF.Prism.Shared
{
    public class BusinessPropsHelper : IBusinessPropsHelper
    {
        public const long ERROR_NUMBER = - 1;
        public const int  TICKET_NUMBER_FORMAT_LENGTH = 7;
        public const int  TRANSACTION_INDEX_LENGTH = 16;

        //private static ILog m_logger = LogManager.GetLogger(typeof(BusinessPropsHelper));
        private static ILog Log = LogFactory.CreateLog(typeof(BusinessPropsHelper));

        private static long m_lLastTransactionID = 0;
        private static long m_lLastTicketNumber = 0;
        private static long m_lLastCreditNoteNumber = 0;
        private static TicketWS m_lastTicket = null;

        private static string m_sStationNumber = string.Empty;
        static object locker = new object();

        public void Initialize(string sStationNumber, BusinessProps bp)
        {
            lock (locker)
            {


                Log.InfoFormat("Initializing '{0}' {1}", sStationNumber, BusinessPropsToString(bp));

                m_sStationNumber = sStationNumber;

                ExcpHelper.ThrowIf(string.IsNullOrEmpty(m_sStationNumber), "Station Number is invalid");
                List<TransactionQueueSr> lObjects = TransactionQueueSr.GetByQuery("SELECT * FROM TransactionQueue WHERE Type = {0} OR Type = {1} OR Type = {2} ORDER BY TransactionQueueID DESC", (int) eTransactionQueueType.Ticket, (int) eTransactionQueueType.DepositByCreditNote, (int) eTransactionQueueType.Deposit);

                foreach (TransactionQueueSr tq in lObjects)
                {
                    eTransactionQueueType eType = (eTransactionQueueType) tq.Type;

                    if (eType == eTransactionQueueType.Ticket && m_lLastTicketNumber == 0)
                    {
                        m_lastTicket = SerializeHelper.StringToDataContractObject<TicketWS>(tq.Object1);
                        m_lLastTicketNumber = ParseNumberId(m_lastTicket.ticketNbr);
                    }
                    else if (eType == eTransactionQueueType.DepositByCreditNote && m_lLastCreditNoteNumber == 0)
                    {
                        string sNoteNumber = tq.Object2;

                        m_lLastCreditNoteNumber = ParseNumberId(sNoteNumber);
                    }

                    if (m_lLastTicketNumber > 0 && m_lLastCreditNoteNumber > 0)
                    {
                        break;
                    }
                }


                m_lLastTransactionID = bp.LastTransactionId;
                m_lLastTicketNumber = Math.Max(m_lLastTicketNumber, bp.LastTicketNumber);
                m_lLastCreditNoteNumber = Math.Max(m_lLastCreditNoteNumber, bp.LastCreditNoteNumber);
            }
        }

        public static string BusinessPropsToString(BusinessProps bp)
        {
            lock (locker)
            {
                return string.Format("BusinessProps {{LastTransactionId = {0}, LastTicketNumber = {1}, LastCreditNoteNumber = {2}}}", bp.LastTransactionId, bp.LastTicketNumber, bp.LastCreditNoteNumber);
            }
        }

        /*
        public static long LastTicketNumber()
        {
            return  m_lLastTicketNumber;
        }

        public static long LastCreditNoteNumber()
        {
            return m_lLastCreditNoteNumber;
        }
        */

        public static TicketWS GetLastTicket()
        {
            lock (locker)
            {
                return m_lastTicket;
            }
        }

        public string GenerateNextTicketNumber()
        {
            lock (locker)
            {
                m_lLastTicketNumber++;

                string sNextNumber = FullTicketNumber(m_lLastTicketNumber);

                Log.InfoFormat("GenerateNextTicketNumber('{0}') = '{1}'", m_sStationNumber, sNextNumber);

                return sNextNumber;
            }
        }

        public string GenerateNextCreditNoteNumber()
        {
            lock (locker)
            {
                m_lLastCreditNoteNumber++;

                string sNextNumber = FullTicketNumber(m_lLastCreditNoteNumber);

                Log.InfoFormat("GenerateNextCreditNoteNumber('{0}') = '{1}'", m_sStationNumber, sNextNumber);

                return sNextNumber;
            }
        }

        public static string GetLastTicketNumber()
        {
            lock (locker)
            {
                return FullTicketNumber(m_lLastTicketNumber);
            }
        }

        private static string FullTicketNumber(long lVariablePartOfTicketNumber)
        {
            lock (locker)
            {
                string sFormat = string.Format("D{0}", TICKET_NUMBER_FORMAT_LENGTH);
                string sVariablePartOfTicketNumber = lVariablePartOfTicketNumber.ToString(sFormat);

                if (sVariablePartOfTicketNumber.Length > TICKET_NUMBER_FORMAT_LENGTH)
                {
                    sVariablePartOfTicketNumber = sVariablePartOfTicketNumber.Substring(sVariablePartOfTicketNumber.Length - TICKET_NUMBER_FORMAT_LENGTH);
                }

                return m_sStationNumber + DateTime.Now.ToString("yy") + sVariablePartOfTicketNumber;
            }
        }

        public static long ParseNumberId(string sNumberId)
        {
            lock (locker)
            {
                try
                {
                    //StationSettings settings = StationSettings.GetSettings;

                    //ExcpHelper.ThrowIf(settings == null, "Settings IS NULL");
                    ExcpHelper.ThrowIf(string.IsNullOrEmpty(m_sStationNumber), "Station Number is invalid");
                    ExcpHelper.ThrowIf(string.IsNullOrEmpty(sNumberId) || m_sStationNumber.Length >= sNumberId.Length, "Parsed Number is invalid");

                    int iStationNumberPos = sNumberId.IndexOf(m_sStationNumber, StringComparison.OrdinalIgnoreCase);

                    ExcpHelper.ThrowIf(iStationNumberPos != 0, "Cannot find current Station Number ('{0}') in incoming Number ('{1}')", m_sStationNumber, sNumberId);

                    string sLongToParse = sNumberId.Substring(m_sStationNumber.Length);

                    return Convert.ToInt64(sLongToParse);
                }
                catch (Exception excp)
                {
                    Log.Error(ExcpHelper.FormatException(excp, "CRITICAL ERROR: SetLastTransactionId() - Cannot set last Transaction ID"),excp);
                }

                return ERROR_NUMBER;
            }
        }

        public string GetNextTransactionId()
        {
            lock (locker)
            {
                try
                {
                    //StationSettings settings = StationSettings.GetSettings;

                    //ExcpHelper.ThrowIf(settings == null, "Settings IS NULL");
                    //ExcpHelper.ThrowIf(string.IsNullOrEmpty(settings.StationNumber), "Station Number is invalid");
                    //ExcpHelper.ThrowIf(string.IsNullOrEmpty(sStationNumber), "Station Number is invalid");

                    m_lLastTransactionID = m_lLastTransactionID + 1;

                    string sFormat = string.Format("D{0}", TRANSACTION_INDEX_LENGTH);

                    //string sTransactionId = settings.StationNumber + m_lLastTransactionID.ToString(sFormat);
                    string sTransactionId = m_sStationNumber + m_lLastTransactionID.ToString(sFormat);

                    Log.InfoFormat("GetNextTransactionId('{0}') = '{1}'", m_sStationNumber, sTransactionId);

                    return sTransactionId;
                }
                catch (Exception excp)
                {
                    Log.Error(ExcpHelper.FormatException(excp, "CRITICAL ERROR: GetNextTransactionId()"),excp);
                }

                return string.Empty;
            }
        }

        


    }
}
