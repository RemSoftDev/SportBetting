using System;
using IocContainer;
using Ninject;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.Common.Logs;
using WsdlRepository;

namespace SportBetting.WPF.Prism.OldCode
{
    public class BarCodeConverter
    {
        private static IChangeTracker _changeTracker;
        public static IChangeTracker ChangeTracker
        {
            get
            {
                return _changeTracker ?? (_changeTracker = IoCContainer.Kernel.Get<IChangeTracker>());
            }
        }

        #region BarcodeType enum

        public enum BarcodeType
        {
            TICKET = 0,
            CREDIT_NOTE = 1,
            STORED_TICKET = 2,
            PAYMENT_NOTE = 3,
            REGISTRATION_NOTE = 4,
            TAXNUMBER = 5,
            CARDBARCODE = 6
        }

        #endregion
        private static ILog Log = LogFactory.CreateLog(typeof(BarCodeConverter));

        private const String TicketStartStr = "0";
        private const String CreditNoteStartStr = "1";
        private const String StoredTicketStartStr = "2";
        private const String PaymentNoteStartStr = "3";
        private const String RegistrationNoteStartStr = "4";
        private const String BarcodeCardStartStr = "6";
        public const int StationLen = 4;
        public static readonly int TicketNumLen = StationRepository.TicketNumberLength;
        public static readonly int PaymentNoteLen = StationRepository.PaymentNoteLength;
        public static readonly int TicketCodeLen = StationRepository.CheckSumLength;
        public static readonly int TaxNumberLen = StationRepository.TaxNumberLength;
        public static readonly int CardBarcodeLen = StationRepository.CardBarcodeLen;
        private const int PinLen = 4;

        private static string barcodeText = "";
        private static DateTime lastKeyPress = DateTime.Now;
        public static BarcodeType? Type { get; private set; }

        private static IStationRepository StationRepository
        {
            get { return IoCContainer.Kernel.Get<IStationRepository>(); }
        }

        public static string Pin
        {
            get { return BarCodeConverter.BarCodeStr2Pin(barcodeText); }
        }

        public static long AccountId
        {
            get { return BarCodeConverter.BarCodeStr2AccountId(barcodeText); }
        }

        public static string StationNumber
        {
            get { return BarCodeConverter.BarCodeStr2StationNum(barcodeText); }
        }

        public static string TicketNumberShort
        {
            get { return BarCodeConverter.BarCodeStr2ShortTicketNum(barcodeText); }
        }

        public static string TicketNumber
        {
            get { return BarCodeConverter.BarCodeStr2TicketNum(barcodeText); }
        }
        public static string PaymentNoteNumber
        {
            get { return BarCodeConverter.BarCodeStr2PaymentNoteNum(barcodeText); }
        }

        public static string CheckSum
        {
            get { return BarCodeConverter.BarcodeStr2CheckSum(barcodeText); }
        }

        public static string TaxNumber
        {
            get { return BarCodeConverter.BarCodeStr2TaxNum(barcodeText); }
        }
        public static string CardBarcode
        {
            get { return BarCodeConverter.BarCodeStr2TaxNum(barcodeText); }
        }

        public static string FullTicketNumber
        {
            get
            {
                if (Type == BarcodeType.STORED_TICKET)
                    return StationNumber + TicketNumberShort + Pin;
                else
                    return StationNumber + TicketNumberShort + CheckSum;
            }
        }

        public static bool IsBarcodeInput { get; set; }

        public static void Clear()
        {
            barcodeText = string.Empty;
            Type = null;
        }

        public static void ProcessBarcode(char inputChar)
        {
            //Console.WriteLine(inputChar);
            ChangeTracker.MouseClickLastTime = DateTime.Now;
            var now = DateTime.Now;
            if (now - lastKeyPress > new TimeSpan(0, 0, 0, 10))
            {
                Clear();
                //Console.WriteLine("clear barcode input. too much time between inputs");
            }
            lastKeyPress = DateTime.Now;

            if (inputChar == 2)
            {
                Clear();
               // Console.WriteLine("clear barcode input. special char");
                Type = null;
                return;
            }
            if (!Char.IsDigit(inputChar) && inputChar.ToString() != "\r" && !ChangeTracker.CanScanTaxNumber) //to do add check for are we in registration view in taxnumber field
            {
                Log.Debug("invalid char");
               // Console.WriteLine("invalid char");
                return;
            }
            
            if (Type == null)
            {
                try
                {
                    if (inputChar == '0' || inputChar == '1' || inputChar == '2' || inputChar == '3' || inputChar == '4'|| inputChar == '6')
                    {
                        Type = (BarcodeType)Convert.ToInt32(inputChar.ToString());
                        return;
                    }
                    else
                        Type = BarcodeType.TAXNUMBER;

                }
                catch (Exception)
                {
                    return;
                }
            }
            else
            {
                if (Char.IsLetterOrDigit(inputChar) || inputChar.ToString() == "\r")
                {
                    barcodeText += inputChar;
                    Log.Debug(barcodeText);
                    //Console.WriteLine(barcodeText);
                }
            }






        }

        private static string BarCodeStr2StationNum(String barcodeStr)
        {
            try
            {
                return barcodeStr.Substring(0, StationLen);
            }
            catch (Exception)
            {
            }
            return "";
        }

        public static string BarCodeStr2ShortTicketNum(String barcodeStr)
        {
            try
            {
                return barcodeStr.Substring(StationLen, TicketNumLen - TicketCodeLen).Substring(0, TicketNumLen - TicketCodeLen);
            }
            catch (Exception)
            {
            }
            return "";
        }

        public static string BarCodeStr2TicketNum(String barcodeStr)
        {
            try
            {
                if (barcodeStr.Length < TicketNumLen)
                    return "";
                return barcodeStr.Substring(0, TicketNumLen);
            }
            catch (Exception)
            {
            }
            return "";
        }

        public static string BarCodeStr2PaymentNoteNum(String barcodeStr)
        {
            try
            {
                if (barcodeStr.Length < PaymentNoteLen)
                    return "";

                return barcodeStr.Substring(0, PaymentNoteLen);
            }
            catch (Exception)
            {
            }
            return "";
        }

        public static string BarcodeStr2CheckSum(String barcodeStr)
        {
            try
            {
                if (barcodeStr.Length < TicketCodeLen + TicketNumLen)
                    return "";
                return barcodeStr.Substring(StationLen + TicketNumLen - TicketCodeLen).Substring(0, TicketCodeLen);
            }
            catch (Exception)
            {
            }
            return "";
        }

        public static string BarCodeStr2TaxNum(String barcodeStr)
        {
            try
            {
                if (barcodeStr.Length < TaxNumberLen)
                    return "";

                return barcodeStr.Substring(0, barcodeStr.Length - 1);
            }
            catch (Exception)
            {
            }
            return "";
        }
        public static string BarCodeStr2CardBarcode(String barcodeStr)
        {
            try
            {
                if (barcodeStr.Length < CardBarcodeLen)
                    return "";

                return barcodeStr.Substring(0, barcodeStr.Length - 1);
            }
            catch (Exception)
            {
            }
            return "";
        }

        public static String ConvertNumber2TicketBarcode(String number)
        {
            return TicketStartStr + number;
        }

        public static String ConvertNumber2CardBarcode(String number)
        {
            return BarcodeCardStartStr + number;
        }

        public static String ConvertNumber2CreditNoteBarcode(String number)
        {
            return CreditNoteStartStr + number;
        }

        public static string ConvertToStoreTicket(String number, String pin)
        {
            return StoredTicketStartStr + number + pin;
        }

        private static long BarCodeStr2AccountId(string inputbarcodeText)
        {
            try
            {
                var str = inputbarcodeText.Substring(0, inputbarcodeText.Length - PinLen);
                return Convert.ToInt64(str);
            }
            catch (Exception)
            {
            }
            return 0;
        }

        private static string BarCodeStr2Pin(string inputbarcodeText)
        {
            try
            {
                return inputbarcodeText.Substring(StationLen + TicketNumLen - PinLen).Substring(0, PinLen);
            }
            catch (Exception)
            {
            }
            return "";
        }

        public static bool IsComplete()
        {
            if (Type == BarcodeType.TICKET)
                return barcodeText.Length == StationRepository.TicketNumberLength + StationRepository.CheckSumLength + 1 && barcodeText.Contains("\r");
            if (Type == BarcodeType.CREDIT_NOTE)
                return barcodeText.Length == StationRepository.TicketNumberLength + StationRepository.CheckSumLength + 1 && barcodeText.Contains("\r");
            if (Type == BarcodeType.STORED_TICKET)
                return barcodeText.Length == StationRepository.TicketNumberLength + StationRepository.CheckSumLength + 1 + PinLen && barcodeText.Contains("\r");
            if (Type == BarcodeType.PAYMENT_NOTE)
                return barcodeText.Length == StationRepository.PaymentNoteLength + 1 && barcodeText.Contains("\r");
            if (Type == BarcodeType.REGISTRATION_NOTE)
                return barcodeText.Length == StationRepository.TicketNumberLength + StationRepository.CheckSumLength + 1 && barcodeText.Contains("\r");
            if (Type == BarcodeType.TAXNUMBER)
                return barcodeText.Length == TaxNumberLen + 1 && barcodeText.Contains("\r");
            if (Type == BarcodeType.CARDBARCODE)
                return barcodeText.Length == CardBarcodeLen + 1 && barcodeText.Contains("\r");
            return false;
        }

        public static String ConvertNumber2PaymentNoteBarcode(String number)
        {
            return PaymentNoteStartStr + number;
        }

        public static String ConvertNumber2RegistrationNoteBarcode(String number)
        {
            return RegistrationNoteStartStr + number;
        }
    }
}