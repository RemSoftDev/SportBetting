using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Management;
using System.Printing;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using IocContainer;
using Nbt.Services.Spf;
using Nbt.Services.Spf.Printer;
using Nbt.Station.Design;
using Ninject;
using Shared;
using SharedInterfaces;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.OldCode;
using SportBetting.WPF.Prism.Shared.Services;
using SportRadar.Common;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportBetting.WPF.Prism.Shared.Resources;
using TranslationByMarkupExtension;
using System.Collections.ObjectModel;
using WsdlRepository;
using WsdlRepository.WsdlServiceReference;
using Bet = Nbt.Station.Design.Bet;
using AnonymousUser = SportBetting.WPF.Prism.Models.AnonymousUser;
using System.Globalization;

//using CreditNoteWS = WcfService.WsdlServiceReference.CreditNoteWS;

namespace SportBetting.WPF.Prism.OldCode
{
    /// <summary>
    /// Prints tickets, creditNotes, Cashdrawer-Closings and checks printer-status
    /// </summary>
    public class PrinterHandler : IPrinterHandler
    {
        private static ILog Log = LogFactory.CreateLog(typeof(PrinterHandler));

        public int currentStatus { get; set; }

        public event EventHandler RefreshNotPrintedCount;
        private IStationSettings _stationSettings;
        public IStationSettings StationSettings
        {
            get
            {
                return _stationSettings ?? (_stationSettings = IoCContainer.Kernel.Get<IStationSettings>());
            }
        }

        public int NotPrintedItemsCount
        {
            get
            {
                var files = Directory.GetFiles(foldername, "*.xml");
                return files.Length;
            }
            set { throw new NotImplementedException(); }
        }

        public int PrintedTicketsCount
        {
            get
            {
                var files = Directory.GetFiles(printedTicketsfoldername, "ticket*.xml");
                return files.Length;
            }
            set { throw new NotImplementedException(); }
        }

        public static string specifier = "F";

        public const int ERROR_ID = -1;
        public const int PRINTER_NOT_FOUND = 0;
        public const int PRINTER_READY = 1;
        public const int PRINTER_NO_PAPER = 2;

        //private static log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(PrinterHandler));


        private static int betDomainOffset = 0; //Offset needed for displaying MultiWayTips
        private static PrinterHandler _printerHandler = null;
        private static bool alreadyWarnedPrinterFound = false; //send NBTLOG-Message only once on every system startup
        private static bool alreadyWarnedPrinterError = false; //send NBTLOG-Message only once on every system startup
        private bool paper_width_2_inch = false;
        private static Nbt.Services.Spf.XmlPreprocess xmlPreDuplicate = null;

        /// <summary>
        /// Default Constructor adds Bindings to Printer and PrintingLanguage (maybe currently not used 2011-01-28)
        /// </summary>
        private string foldername = "printer objects";
        private string printedTicketsfoldername = "printed tickets";
        public PrinterHandler()
        {
            Binding printerB = new Binding("Printer");
            Binding printLangB = new Binding("PrintingLanguage");
            Directory.CreateDirectory(foldername);
            Directory.CreateDirectory(printedTicketsfoldername);

            printerB.Source = StationRepository;
            printLangB.Source = StationRepository;
            //sysLangB.NotifyOnTargetUpdated = true;
        }

        private static SportBetting.WPF.Prism.Shared.Services.Interfaces.IChangeTracker ChangeTracker
        {
            get { return IoCContainer.Kernel.Get<SportBetting.WPF.Prism.Shared.Services.Interfaces.IChangeTracker>(); }
        }

        private static IDispatcher _dispatcher;
        public static IDispatcher Dispatcher
        {
            get
            {
                return _dispatcher ?? (_dispatcher = IoCContainer.Kernel.Get<IDispatcher>());
            }
        }

        private static IStationRepository StationRepository
        {
            get { return IoCContainer.Kernel.Get<IStationRepository>(); }
        }

        private static IWsdlRepository WsdlRepository
        {
            get { return IoCContainer.Kernel.Get<IWsdlRepository>(); }
        }

        public ITranslationProvider TranslationProvider
        {
            get { return IoCContainer.Kernel.Get<ITranslationProvider>(); }
        }



        private string SelectedLanguage
        {
            get { return TranslationProvider.PrintingLanguage/*.CurrentLanguage*/; }
        }

        /// Singleton as Property
        public static PrinterHandler GetInstance
        {
            get
            {
                if (_printerHandler == null)
                    _printerHandler = new PrinterHandler();
                return _printerHandler;
            }
        }
        public static IRepository Repository
        {
            get { return IoCContainer.Kernel.Get<IRepository>(); }
        }

        public void DeleteAllPrinterObjects()
        {
            try
            {
                foreach (var file in Directory.GetFiles(foldername))
                {
                    File.Delete(file);
                }
                foreach (var file in Directory.GetFiles(printedTicketsfoldername))
                {
                    File.Delete(file);
                }


            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
            }
        }

        public void WriteBarcodeCardNumber(string s)
        {
            PrintBarcode(s);
        }

        private String PrintMatch(TipWS tip, decimal pOdd, bool underline, bool livebet)
        {
            StringBuilder sb = new StringBuilder();

            var name = "";

            string tournamentName;
            String betDomainName;
            string date;
            string sOddName = "";
            if (tip.tipDetailsWS != null)
            {
                //var sTeamHome = tip.tipDetailsWS.team1;
                //var sTeamAway = tip.tipDetailsWS.team2;
                //name = FormatOpponents(sTeamHome, sTeamAway, true, livebet, false);
                name = tip.tipDetailsWS.event_name;
                try
                {
                    string[] strX = tip.tipDetailsWS.event_name.Split(':');
                    if (strX.Length < 2) strX[1] = "";
                    name = FormatOpponents(strX[0].Trim(), strX[1].Trim(), true, livebet, false);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                }
                sOddName = tip.tipDetailsWS.tip;

                tournamentName = tip.tipDetailsWS.ligaName;
                if (tip.tipDetailsWS.specialLiveOddValueFull == "0" || tip.tipDetailsWS.specialLiveOddValueFull == "-1")
                {
                    betDomainName = String.Format(tip.tipDetailsWS.betDomainName, tip.tipDetailsWS.specialOddValue) ?? "";
                }
                else if (tip.tipDetailsWS.specialOddValue == "-1")
                {
                    betDomainName = String.Format(tip.tipDetailsWS.betDomainName, tip.tipDetailsWS.specialLiveOddValueFull) ?? "";
                }
                else
                {
                    betDomainName = String.Format(tip.tipDetailsWS.betDomainName, tip.tipDetailsWS.specialOddValue, tip.tipDetailsWS.specialLiveOddValueFull) ?? "";
                }

                DateTimeSr matchTime = new DateTimeSr(tip.tipDetailsWS.startdate.Value);
                date = matchTime.LocalDateTime.ToString("dd.MM.yyyy HH:mm");
            }
            else
            {
                var odd = Repository.GetOddBySvrId(tip.svrOddID);

                if (odd == null)
                {
                    Log.Error("Odd not found for (svrOddId=" + tip.svrOddID + ")", new Exception());
                    return string.Empty;
                }

                IMatchLn match = odd.LineObject.BetDomain != null ? odd.LineObject.BetDomain.Match : null;

                if (match == null)
                {
                    Log.ErrorFormat("Match not found for odd {0}", new Exception(), odd);
                    return string.Empty;
                }
                if (match.outright_type == eOutrightType.None)
                {
                    var sTeamHome = match.HomeCompetitor.GetDisplayName(SelectedLanguage);
                    var sTeamAway = match.AwayCompetitor.GetDisplayName(SelectedLanguage);
                    name = FormatOpponents(sTeamHome, sTeamAway, true, livebet, false);

                }
                else
                {
                    name = match.MatchView.Name;
                }

                tournamentName = match.MatchView.TournamentNameToShow;
                if (odd.BetDomainView.IsToInverse)
                    betDomainName = String.Format(LineSr.Instance.AllObjects.TaggedStrings.GetStringSafely(odd.BetDomainView.LineObject.NameTag, SelectedLanguage), odd.SpecialBetdomainValue);
                else
                    betDomainName = odd.LineObject.BetDomain.GetDisplayName(SelectedLanguage) ?? "";

                date = match.StartDate.Value.LocalDateTime.ToString("dd.MM.yyyy HH:mm");
            }

            name = name.Replace("\n   ", String.Empty);


            sb.Append("<Line><TextItem Font=\"FontB\">" + tip.matchCode.ToString("G") + "</TextItem>" + "<TextItem Font=\"FontB\" Style=\"Bold\">" + tournamentName + "</TextItem>"); // We show Match Code to User here

            sb.Append("<TextItem Alignment=\"Right\" Font=\"FontB\">" + date + "</TextItem></Line>");

            sb.Append("<Line> <TextItem Alignment=\"Center\" Font=\"FontA\" Style=\"Bold\">" + name +
                      "</TextItem></Line>");

            /*if (betDomainName.Length > 40)
            {
                sb.Append("<Line Underline=\"false\">");
                sb.Append("<TextItem Alignment=\"Left\" Font=\"FontB\">" + betDomainName + ": </TextItem>");
                sb.Append("</Line>");
                sb.Append("<Line Underline=\"" + underline.ToString().ToLowerInvariant() + "\">");
                sb.Append("<TextItem Font=\"FontA\">");
                betDomainOffset = 0;
            }
            else if (betDomainName.Length > 25)
            {
                sb.Append("<Line Underline=\"false\">");
                sb.Append("<TextItem Alignment=\"Left\">" + betDomainName + ": </TextItem>");
                sb.Append("</Line>");
                sb.Append("<Line Underline=\"" + underline.ToString().ToLowerInvariant() + "\">");
                sb.Append("<TextItem Font=\"FontA\">");

                betDomainOffset = 0;
            }
            else
            {
                sb.Append("<Line Underline=\"" + underline.ToString().ToLowerInvariant() + "\">");
                sb.Append("<TextItem Alignment=\"Left\">" + betDomainName + ": ");
                betDomainOffset = betDomainName.Length + 2;
            }*/
            sb.Append("<Line Underline=\"false\">");
            sb.Append("<TextItem Alignment=\"Left\">" + betDomainName + "</TextItem></Line>");
            betDomainOffset = 0;
            sb.Append("<Line Underline=\"true\"> <TextItem Alignment=\"Left\">");
            if (sOddName == "")
            {
                sOddName = Repository.GetOddBySvrId(tip.svrOddID).LineObject.OddView.DisplayNameForPrinting(SelectedLanguage);
            }
            sb.Append(sOddName + "</TextItem>");

            sb.Append("<TextItem Alignment=\"Right\" Font=\"FontA\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TICKET_ODD) + Decimal2String(pOdd, 2) +
                      "</TextItem></Line>");
            return sb.ToString();
        }

        public static string FormatOpponents(string teamHome, string teamAway, bool minimize, bool isLiveBet, bool makeLong)
        {
            String comp1 = "";
            String comp2 = "";

            if (teamAway == null)
            {
                teamAway = "";
            }
            if (teamHome == null)
            {
                teamHome = "";
            }
            if (isLiveBet)
            {
                teamAway = teamAway.ToUpperInvariant();
                teamHome = teamHome.ToUpperInvariant();
            }
            try
            {
                if (minimize)
                {
                    comp1 = TextUtil.Shorten(teamHome, 15, "...");
                    comp2 = TextUtil.Shorten(teamAway, 15, "...").Trim();
                }
                else if (!makeLong)
                {
                    if (isLiveBet)
                    {
                        comp1 = TextUtil.Shorten(teamHome, 16, "...") + "\n"; //add NewLine to save Space
                        comp2 = TextUtil.Shorten(teamAway, 16, "...").Trim();
                    }
                    else
                    {
                        comp1 = TextUtil.Shorten(teamHome, 20, "...") + "\n"; //add NewLine to save Space
                        comp2 = TextUtil.Shorten(teamAway, 20, "...").Trim();
                    }
                }
                else
                {
                    comp1 = TextUtil.Shorten(teamHome, 18, "..."); //no NewLine to save Space
                    comp2 = TextUtil.Shorten(teamAway, 18, "...").Trim();
                }



                if (comp2 != "")
                {
                    return comp1 + " - " + comp2;
                }
                else
                {
                    return comp1;
                }
            }
            catch (Exception Ex)
            {
                Log.Error("In FormatOpponents:\n", Ex);
                if (teamAway != null)
                {
                    return teamHome + " - \n" + teamAway;
                }
                else
                {
                    return teamHome;
                }
            }
        }


        /// <summary>
        /// Initializes the printer, writes NbtLog-Entries, if printer is not available and it has not warned before
        /// </summary>
        /// <param name="checkConnection">also establishes a connection to the printer if needed</param>
        /// <returns>the IPrinter object to print data</returns>
        public IPrinter InitPrinter(bool checkConnection)
        {
            // set printing language
            if (StationRepository.PrintingLanguageSetting == 2) // user default
            {
                TranslationProvider.PrintingLanguage = TranslationProvider.CurrentLanguage;
            }
            else // station default
            {
                TranslationProvider.PrintingLanguage = StationRepository.DefaultDisplayLanguage ?? TranslationProvider.CurrentLanguage;
            }


            IPrinter printer = StationSettings.Printer;
            try
            {
                int status = checkPrinter(printer);
                if (status != PRINTER_READY)
                {
                    if (!alreadyWarnedPrinterError)
                    {
                        WsdlRepository.StationEvent(StationRepository.StationNumber, StationEventType.PRINTER_ERROR);
                        alreadyWarnedPrinterError = true;
                    }
                }
                else
                {
                    alreadyWarnedPrinterError = false;
                }
                bool statuschanged = StationRepository.PrinterStatus != status ? true : false;
                StationRepository.PrinterStatus = status;

                if (checkConnection && printer != null && (status != PrinterHandler.PRINTER_READY))
                {
                    //this is ESC/POS-Printer which is currently not available
                    switch (status)
                    {
                        case PrinterHandler.PRINTER_NOT_FOUND:
                            Log.Debug("ESC/POS-Printer not found ");
                            if (!alreadyWarnedPrinterFound)
                            {
                                NbtLogSr.WriteNbtLogEntry(String.Format(LogMessages.PrinterHandler1, "default printer"),
                                                          NbtLogSr.PRIORITY_MEDIUM, StationRepository.StationNumber,
                                                          NbtLogSr.MSG_TERMINAL);
                                alreadyWarnedPrinterFound = true;
                            }
                            break;
                        case PrinterHandler.PRINTER_NO_PAPER:
                            Log.Debug("ESC/POS-Printer has no paper in initPrinter: ");
                            if (!alreadyWarnedPrinterFound)
                            {
                                NbtLogSr.WriteNbtLogEntry(String.Format(LogMessages.PrinterHandler2, "default printer"),
                                                          NbtLogSr.PRIORITY_MEDIUM, StationRepository.StationNumber,
                                                          NbtLogSr.MSG_TERMINAL);
                                alreadyWarnedPrinterFound = true;
                            }
                            break;
                    }
                }
                else if (status != PrinterHandler.PRINTER_READY)
                {
                    //This is a windows Default printer which is currently not available
                    Log.Debug("Windows-Printer offline in initPrinter:" +
                                  "default printer");
                    if (!alreadyWarnedPrinterFound)
                    {
                        NbtLogSr.WriteNbtLogEntry(String.Format(LogMessages.PrinterHandler3, "default printer"), NbtLogSr.PRIORITY_MEDIUM,
                                                  StationRepository.StationNumber, NbtLogSr.MSG_TERMINAL);
                        alreadyWarnedPrinterFound = true;
                    }
                    return printer;
                }
                else
                {
                    //Printer is Okay
                    return printer;
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Error checking Printer: " + ex);
                if (!alreadyWarnedPrinterFound)
                {
                    NbtLogSr.WriteNbtLogEntry(String.Format(LogMessages.PrinterHandler4, "default printer", ex), NbtLogSr.PRIORITY_LOW,
                                              StationRepository.StationNumber, NbtLogSr.MSG_TERMINAL);
                    alreadyWarnedPrinterFound = true;
                }
                return null;
            }
            return null;
        }

        /// <summary>
        /// checks the printer if possible (Check for Connection, Online-Status, Peper, etc.
        /// </summary>
        /// <param name="printer">the IPrinter to be checked</param>
        /// <returns>PrinterStatus StationSettings.</returns>
        public int checkPrinter(Nbt.Services.Spf.Printer.IPrinter printer)
        {
            currentStatus = 0;
            if (printer != null)
            {
                ManagementObjectSearcher searcher;


                searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer WHERE Default = 'true'");
                foreach (ManagementObject defPrinter in searcher.Get())
                {
                    if (defPrinter["Name"].ToString().ToUpperInvariant().IndexOf("NII") != -1)
                    {
                        paper_width_2_inch = true;
                    }
                    else
                    {
                        paper_width_2_inch = false;
                    }
                    int errorcode = Convert.ToInt16(defPrinter["DetectedErrorState"]);
                    if (defPrinter["WorkOffline"].ToString().ToLowerInvariant().Equals("true") ||
                        (errorcode == 4 || errorcode == 6 || errorcode == 7 || errorcode == 8 || errorcode == 9))
                    // no paper, no toner, door open, jammed, offline
                    {
                        currentStatus = errorcode;
                        System.Diagnostics.Debug.WriteLine("status in printer handler: " + currentStatus);
                        return PrinterHandler.PRINTER_NOT_FOUND;
                    }
                    else return PrinterHandler.PRINTER_READY;
                }
                return PrinterHandler.PRINTER_NOT_FOUND;
            }
            return PrinterHandler.PRINTER_NOT_FOUND;
        }

        public bool PrintBarcode(string number)
        {
            Nbt.Services.Spf.Printer.IPrinter printer = InitPrinter(true);
            if (printer == null)
            {
                return false;
            }

            StringBuilder sb = new StringBuilder();


            String barcodeStr = BarCodeConverter.ConvertNumber2CardBarcode(number);
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?><Page xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"SPFSchema.xsd\" Codepage=\"850\">");
            sb.Append("<Line><FillItem>-</FillItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line>");
            sb.Append("<BarCodeItem Alignment=\"Center\" HRIPos=\"None\" Height=\"100\"  Type=\"CODE93\">" + barcodeStr +
                      "</BarCodeItem>");
            sb.Append("</Line>");
            var text = TranslationProvider.TranslateForPrinter(MultistringTags.BARCODE_NOTIFICATION_TEXT) as string;
            sb.Append(SplitStringBySpaces(text, 30, 2));

            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><FillItem>-</FillItem></Line>");
            sb.Append("</Page>");
            Nbt.Services.Spf.XmlPreprocess xmlPre = new Nbt.Services.Spf.XmlPreprocess(sb.ToString(), false);

            bool found = PrintDefaultTicket(printer, xmlPre, false);
            if (!found)
            {
                WsdlRepository.StationEvent(StationRepository.StationNumber, StationEventType.PRINTER_ERROR);

                StationRepository.PrinterStatus = PrinterHandler.PRINTER_NOT_FOUND;
            }
            return found;
        }


        public XmlPreprocess CreateXmlAndSaveFile(TicketWS rTicketWs, bool isDuplicate)
        {
            string tax_number = null;


            if (StationRepository.DisplayTaxNumber & !(ChangeTracker.CurrentUser is AnonymousUser))
            {

                tax_number = ChangeTracker.CurrentUser.TaxNumber;

            }

            var sb = CreateTicketXmlForPrinting(rTicketWs, isDuplicate, tax_number);

            Nbt.Services.Spf.XmlPreprocess xmlPre = new Nbt.Services.Spf.XmlPreprocess(sb.ToString(), false);


            var filename = printedTicketsfoldername + "\\ticket" + rTicketWs.ticketNbr + ".xml";
            StreamWriter sw = new StreamWriter(filename);
            sw.Write(xmlPre.xmlData);
            sw.Flush();
            sw.Close();
            return xmlPre;

        }

        public bool PrintTicket(TicketWS rTicketWs, bool isDuplicate)
        {
            try
            {
                rTicketWs.bets[0].tips2BetMulti = rTicketWs.bets[0].tips2BetMulti.OrderBy(x => x.svrOddID).OrderBy(x => x.matchCode).ToArray();
                rTicketWs.bets[0].bankTips = rTicketWs.bets[0].bankTips.OrderBy(x => x.svrOddID).OrderBy(x => x.matchCode).ToArray();
                if (!StationRepository.UsePrinter)
                {
                    return true;
                }
                Nbt.Services.Spf.Printer.IPrinter printer = InitPrinter(true);

                var xmlPre = CreateXmlAndSaveFile(rTicketWs, isDuplicate);

                bool found = PrintDefaultTicket(printer, xmlPre, true);
                Log.Debug("printing done");

                if (!found)
                {
                    StationRepository.PrinterStatus = PrinterHandler.PRINTER_NOT_FOUND;
                }

                return found;
            }
            catch (Exception Ex)
            {

                string msg = "Error printing ticket" + rTicketWs.ticketNbr + ": " + Ex;
                Log.Error(Ex.Message, Ex);
                Log.Error(msg, Ex);
                WriteRemoteError2Log(msg, NbtLogSr.PRIORITY_MEDIUM, rTicketWs.ticketNbr, NbtLogSr.MSG_TICKET);
                return false;
            }


        }



        private void WriteRemoteError2Log(string msg, int priorityMedium, string ticketNbr, string msgTicket)
        {
            try
            {
                WsdlRepository.WriteRemoteError2Log(msg, priorityMedium, ticketNbr, msgTicket);

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }

        }


        public StringBuilder CreateTicketXmlForPrinting(TicketWS rTicketWs, bool isDuplicate, string tax_number)
        {
            bool livebet = rTicketWs.ticketTyp == Ticket.TICKET_TYP_LIVEBET || rTicketWs.ticketTyp == Ticket.TICKET_TYP_BOTH;
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?><Page xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"SPFSchema.xsd\" Codepage=\"850\">");
            decimal bonusfactor;

            foreach (BetWS bet in rTicketWs.bets)
            {
                String barcodeStr = BarCodeConverter.ConvertNumber2TicketBarcode(rTicketWs.ticketNbr + rTicketWs.checkSum);

                sb.Append("<Line>");
                sb.Append("<BarCodeItem Alignment=\"Center\" HRIPos=\"None\" Height=\"100\"  Type=\"CODE93\">" + barcodeStr + "</BarCodeItem>");
                sb.Append("</Line>");

                sb.Append(printHeaderForTicket());

                sb.Append("<Line Underline=\"true\"><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TICKET_NAME) + "</TextItem></Line>");

                if (isDuplicate)
                {
                    sb.Append("<Line Underline=\"true\"><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"2\" >" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_DUPLICATE) + "</TextItem></Line>");
                }

                //sb.Append("<Line><TextItem Alignment=\"Center\" Indent=\"1\" Characterspacing=\"3\" Font=\"FontA\" Size=\"1\">"+num.ToString()+"</TextItem> </Line>");//test code TODO : remove Test Code
                //sb.Append("<Line><FillItem>-</FillItem></Line>");

                if (isDuplicate)
                {
                    sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"1\">" +
                              rTicketWs.acceptedTime.ToString("dd.MM.yyyy") + "</TextItem>");
                    //DateTimeUtils.DisplayNormalDate(DateTime.Now, StationRepository.SystemCulture) + "</TextItem>");
                    //sb.Append("<FillItem>*</FillItem>");
                    sb.Append("<TextItem  Alignment=\"Right\" Font=\"FontB\">" + rTicketWs.acceptedTime.ToString("HH:mm") +
                        //Nbt.Common.Utils.Date.DateTimeUtils.DisplayTimeShort(DateTime.Now, StationRepository.SystemCulture) +
                              "</TextItem> </Line>");
                }
                else
                {
                    sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"1\">" +
                              DateTime.Now.ToString("dd.MM.yyyy") + "</TextItem>");
                    //DateTimeUtils.DisplayNormalDate(DateTime.Now, StationRepository.SystemCulture) + "</TextItem>");
                    //sb.Append("<FillItem>*</FillItem>");
                    sb.Append("<TextItem  Alignment=\"Right\" Font=\"FontB\">" + DateTime.Now.ToString("HH:mm") +
                        //Nbt.Common.Utils.Date.DateTimeUtils.DisplayTimeShort(DateTime.Now, StationRepository.SystemCulture) +
                              "</TextItem> </Line>");
                }

                sb.Append("<Line Underline=\"false\">");
                sb.Append("<TextItem  Alignment=\"Left\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TICKET_NUM_SHORT) + "</TextItem>");
                sb.Append("<TextItem  Alignment=\"Right\" Size=\"2\" Style=\"None\">" + rTicketWs.ticketNbr + "</TextItem></Line>");
                sb.Append("<Line Underline=\"false\">");
                sb.Append("<TextItem  Alignment=\"Left\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_TICKETCODE) + "</TextItem>");
                sb.Append("<TextItem  Alignment=\"Right\" Size=\"2\" Style=\"None\">" + rTicketWs.checkSum + "</TextItem></Line>");



                if (!String.IsNullOrEmpty(tax_number))
                {
                    sb.Append("<Line Underline=\"false\">");
                    sb.Append("<TextItem  Alignment=\"Left\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_TAXNUMBER) + "</TextItem>");
                    sb.Append("<TextItem  Alignment=\"Right\" Size=\"2\" Style=\"None\">" + tax_number + "</TextItem></Line>");
                }

                sb.Append("<Line><FillItem>=</FillItem></Line>");
                //sb.Append(" <Line Underline=\"1\"><FillItem>  </FillItem></Line>");
                sb.Append("<Line Underline=\"false\">");

                object ticket_type;
                if (isDuplicate)
                {
                    ticket_type = rTicketWs.isAnonymous ? TranslationProvider.TranslateForPrinter(MultistringTags.TICKET_TYPE_ANONYMOUS) : TranslationProvider.TranslateForPrinter(MultistringTags.TICKET_TYPE_ACCOUNT);
                }
                else
                {
                    ticket_type = ChangeTracker.CurrentUser is AnonymousUser ? TranslationProvider.TranslateForPrinter(MultistringTags.TICKET_TYPE_ANONYMOUS) : TranslationProvider.TranslateForPrinter(MultistringTags.TICKET_TYPE_ACCOUNT);
                }

                sb.Append("<TextItem Alignment=\"Left\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TICKET_TYPE) + "</TextItem>");
                sb.Append("<TextItem Alignment=\"Right\" Size=\"2\">" + ticket_type + "</TextItem> </Line>");

                sb.Append("<Line> <TextItem Alignment=\"Left\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_BET_TYPE) + "</TextItem>");


                long prevBankGroupID = -1;
                int bankCount = 0;
                int multiWayCount = 0;
                foreach (TipWS tip in bet.tips2BetMulti)
                {
                    if (tip.bankGroupID != prevBankGroupID && !tip.bank)
                    {
                        multiWayCount++;
                        prevBankGroupID = tip.bankGroupID;
                    }
                    if (tip.bank)
                    {
                        bankCount++;
                    }
                }

                if (bet.betType == Bet.BET_TYPE_SYSTEM || bet.betType == Bet.BET_TYPE_SYSTEMPATH)
                {
                    sb.Append("<TextItem Alignment=\"Right\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_SYSTEM) + " " + bet.systemX + "/" + bet.systemY);
                    if (bankCount > 0)
                    {
                        sb.Append("+" + bankCount + "B");
                    }
                    if (multiWayCount > 0)
                    {
                        sb.Append("+" + multiWayCount + "W");
                    }
                    sb.Append("</TextItem> </Line>");
                    sb.Append("<Line><FillItem>=</FillItem></Line>");
                    for (int i = 0; i < bet.bankTips.Length; i++)
                    {
                        sb.Append(PrintMatch(bet.bankTips[i], bet.bankTips[i].odd, true /*i < bet.bankTips.Length - 1*/, livebet));
                    }
                    sb.Append(PrintMultiWayTips(bet.tips2BetMulti, true, bankCount, multiWayCount, livebet));
                    //It is impossible to have a system without system tipps
                }
                else if (bet.betType == Bet.BET_TYPE_COMBI || bet.betType == Bet.BET_TYPE_COMBIPATH)
                {
                    sb.Append("<TextItem Alignment=\"Right\" Style=\"Bold\">");
                    if (bet.bankTips.Length == 0)
                    {
                        sb.Append(TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_PATH));
                    }
                    else
                    {
                        sb.Append(TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_COMBI));
                        if (multiWayCount > 0)
                            sb.Append("+" + multiWayCount + "W");
                    }
                    sb.Append("</TextItem> </Line>");
                    sb.Append("<Line><FillItem>=</FillItem></Line>");
                    for (int i = 0; i < bet.bankTips.Length; i++)
                    {
                        sb.Append(PrintMatch(bet.bankTips[i], bet.bankTips[i].odd, true /*i < bet.bankTips.Length - 1*/, livebet));
                    }
                    sb.Append(PrintMultiWayTips(bet.tips2BetMulti, bet.bankTips.Length != 0, bankCount, multiWayCount, livebet));
                }
                else
                {
                    //SINGLE:
                    sb.Append("<TextItem Alignment=\"Right\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_SINGLE) + "</TextItem> </Line>");
                    sb.Append("<Line><FillItem>=</FillItem></Line>");

                    foreach (TipWS tip in bet.bankTips)
                    {
                        sb.Append(PrintMatch(tip, tip.odd, false, livebet));
                    }
                    foreach (TipWS tip in bet.tips2BetMulti)
                    {
                        sb.Append(PrintMatch(tip, tip.odd, false, livebet));
                    }
                }

                sb.Append("<Line Underline=\"false\"><FillItem>=</FillItem></Line>");
                if (bet.rows == 1)
                {
                    sb.Append("<Line Underline=\"false\">");
                    sb.Append("<TextItem  Alignment=\"Left\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TOTAL_ODD) + "</TextItem>");
                    sb.Append("<TextItem  Alignment=\"Right\" Size=\"1\" Style=\"Bold\">" + String.Format("{0:F2}", bet.maxOdd) + "</TextItem></Line>");
                    sb.Append("<Line Underline=\"false\">");
                    sb.Append("<TextItem  Alignment=\"Left\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_STAKE_PER_BET) + "</TextItem>");
                    sb.Append("<TextItem  Alignment=\"Right\" Size=\"4\" Style=\"Bold\">" + String.Format("{0:F2}", bet.stake) + " " + StationRepository.Currency + "</TextItem>  </Line>");
                }
                else
                {
                    sb.Append("<Line Underline=\"false\">");
                    sb.Append("<TextItem  Alignment=\"Left\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_STAKE_PER_ROW) + "</TextItem>");
                    sb.Append("<TextItem  Alignment=\"Right\" Size=\"1\" Style=\"Bold\">" + String.Format("{0:F2}", bet.stake / bet.rows) + " " + StationRepository.Currency + "</TextItem></Line>");
                    sb.Append("<Line Underline=\"false\">");
                    sb.Append("<TextItem  Alignment=\"Left\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_STAKE_TOTAL) + " (" + bet.rows + " " + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_ROWS) + ") " + "</TextItem>");
                    sb.Append("<TextItem  Alignment=\"Right\" Size=\"4\" Style=\"Bold\">" + String.Format("{0:F2}", bet.stake) + " " + StationRepository.Currency + "</TextItem>  </Line>");
                }

                if (rTicketWs.manipulationFee > 0)
                {
                    sb.Append("<Line Underline=\"false\"><TextItem  Alignment=\"Left\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_MANIPULATION_FEE) + "</TextItem><TextItem  Alignment=\"Right\" Size=\"1\" Style=\"Bold\">" + String.Format("{0:F2}", rTicketWs.manipulationFeeValue) + " " + StationRepository.Currency + "</TextItem></Line>");
                }
                bonusfactor = rTicketWs.superBonus;
                var bonus = TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_BONUS_DESC);
                if (bonusfactor > 1.001m)
                {
                    //var bonusvalue = rTicketWs.superBonusValue;

                    //string a = Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;
                    //var _bonusValueRounded = bonusvalue.ToString(CultureInfo.InvariantCulture);
                    //if (_bonusValueRounded.IndexOf(a) != -1)
                    //{
                    //    _bonusValueRounded = _bonusValueRounded.Substring(0, _bonusValueRounded.IndexOf(a) + 3);
                    //    bonusvalue = Decimal.Parse(_bonusValueRounded, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
                    //}

                    decimal bonusvalue = ChangeTracker.TruncateDecimal(rTicketWs.superBonusValue);

                    sb.Append("<Line><TextItem Font=\"FontA\" Alignment=\"Left\" Style=\"Bold\">" + bonus + " +" + String.Format("{0:F2}", (bonusfactor - 1) * 100) + "%" + "</TextItem>");
                    sb.Append("<TextItem Alignment=\"Right\" Font=\"FontA\" Style=\"Bold\">" + String.Format("{0:F2}", bonusvalue) + " " + StationRepository.Currency + "</TextItem></Line>");
                }
                sb.Append("<Line Underline=\"false\">");
                sb.Append("<TextItem  Alignment=\"Left\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_POSS_WIN) + "</TextItem>");
                sb.Append("<TextItem  Alignment=\"Right\" Size=\"4\" Style=\"Bold\">" + String.Format("{0:F2}", bet.maxWin) + " " + StationRepository.Currency + "</TextItem></Line>");

                if (bonusfactor > 1.001m)
                {
                    sb.Append("<Line><FillItem>=</FillItem></Line>");
                    sb.Append("<Line> <TextItem Alignment=\"Center\" Indent=\"1\" Font=\"FontA\" Style=\"Bold\">" + bonus + "</TextItem></Line>");
                    sb.Append("<Line> <TextItem Alignment=\"Center\" Indent=\"1\" Font=\"FontB\" >" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_BONUS_TEXT) + "</TextItem></Line>");
                }
            }
            sb.Append(printFooter());
            sb.Append("</Page>");
            sb.Replace('&', '+'); //Printer is not able to print '&' character
            return sb;
        }

        /// <summary>
        /// Prints ticket using Windows Default Printer
        /// </summary>
        /// <param name="printer">the selected printer</param>
        /// <param name="xmlPre">XMLPreprocess of the ticket</param>
        /// <param name="b"></param>
        /// <returns>true if ok</returns>
        public bool PrintDefaultTicket(IPrinter printer, XmlPreprocess xmlPre, bool saveIfError)
        {
            bool prolonged_wait = false;

            try
            {
                xmlPreDuplicate = xmlPre;
                var filename = foldername + "\\" + DateTime.Now.ToFileTime() + ".xml";
                if (saveIfError)
                {
                    StreamWriter sw = new StreamWriter(filename);
                    sw.Write(xmlPre.xmlData);
                    sw.Flush();
                    sw.Close();
                }
                ManagementObjectSearcher searcher;
                ManagementScope scope = new ManagementScope(@"\root\cimv2");
                string printerName = "";
                scope.Connect();
                searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer WHERE Default = 'true'");
                ManagementObject myprinter = null;
                foreach (ManagementObject foundPrinter in searcher.Get())
                {
                    printerName = foundPrinter["Name"].ToString();
                    myprinter = foundPrinter;
                    break;
                }

                printer.PaperWidth = 270;
                if (printerName != null)
                {
                    if (printerName.ToUpperInvariant().IndexOf("EPSON TM") != -1)
                    {
                        printer.Margin = 30;
                        prolonged_wait = true;
                    }
                    else if (printerName.ToUpperInvariant().IndexOf("NII") != -1)
                    {
                        printer.Margin = 0;
                        printer.PaperWidth = 180;

                    }
                }

                Log.Debug("parse xml for printing");
                Nbt.Services.Spf.DefaultTranslator transNew = new Nbt.Services.Spf.DefaultTranslator(printer,
                                                                                                     xmlPre.Marshaling());
                System.Windows.Controls.PrintDialog pd = new System.Windows.Controls.PrintDialog();
                FixedDocument doc = null;
                Dispatcher.Invoke((Action)(() =>
                    {
                        doc = transNew.ParseFixDocument(pd.PrintableAreaHeight);
                        //var window = new Window();
                        //window.Content = doc;
                        //window.ShowDialog();
                    }));

                int printQueueCount = 0;
                DateTime now = DateTime.Now;
                // printing

                var printQueue = LocalPrintServer.GetDefaultPrintQueue();
                printQueueCount = printQueue.NumberOfJobs;
                Log.Debug("start printing");

                try
                {
                    Dispatcher.Invoke((Action)(() =>
                        { pd.PrintDocument(doc.DocumentPaginator, ""); }));

                }
                catch (Exception exception)
                {
                    Log.Error("", exception);
                    if (RefreshNotPrintedCount != null)
                        RefreshNotPrintedCount(null, null);
                    return false;
                }
                // check, if ticket is printed

                int waitTime = GetWaitTime(LocalPrintServer.GetDefaultPrintQueue().GetPrintJobInfoCollection());

                if (prolonged_wait)
                {
                    waitTime *= 2;
                }

                while (true)
                {
                    Thread.Sleep(100);
                    printQueue = LocalPrintServer.GetDefaultPrintQueue();
                    if (printQueue.NumberOfJobs == printQueueCount || printQueue.NumberOfJobs == 0)
                    {

                        if (saveIfError)
                        {
                            File.Delete(filename);
                        }
                        if (RefreshNotPrintedCount != null)
                            RefreshNotPrintedCount(null, null);
                        return true;
                    }
                    TimeSpan span = DateTime.Now - now;

                    if (span.Seconds > waitTime)
                    {
                        var services = ServiceController.GetServices();
                        foreach (var serviceController in services)
                        {
                            if (serviceController.ServiceName.Contains("Spooler"))
                            {
                                try
                                {
                                    CancelAllJobs(myprinter);
                                }
                                catch (Exception ex)
                                {

                                    Log.Error(ex.Message, ex);
                                }
                                serviceController.Stop();
                                serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                                serviceController.Start();
                                serviceController.WaitForStatus(ServiceControllerStatus.Running);


                                try
                                {
                                    Dispatcher.Invoke((Action)(() =>
                                        { pd.PrintDocument(doc.DocumentPaginator, ""); }));

                                }
                                catch (Exception exception)
                                {
                                    Log.Error("", exception);
                                    if (RefreshNotPrintedCount != null)
                                        RefreshNotPrintedCount(null, null);
                                    return false;
                                }
                                while (true)
                                {
                                    Thread.Sleep(100);
                                    printQueue = LocalPrintServer.GetDefaultPrintQueue();
                                    if (printQueue.NumberOfJobs == printQueueCount || printQueue.NumberOfJobs == 0)
                                    {
                                        if (saveIfError)
                                        {
                                            File.Delete(filename);
                                        }
                                        if (RefreshNotPrintedCount != null)
                                            RefreshNotPrintedCount(null, null);

                                        return true;
                                    }
                                    span = DateTime.Now - now;
                                    if (span.Seconds > waitTime)
                                    {
                                        try
                                        {
                                            CancelAllJobs(myprinter);
                                        }
                                        catch (Exception ex)
                                        {

                                            Log.Error(ex.Message, ex);
                                        }
                                        serviceController.Stop();
                                        serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                                        serviceController.Start();
                                        serviceController.WaitForStatus(ServiceControllerStatus.Running);
                                        try
                                        {
                                            CancelAllJobs(myprinter);
                                        }
                                        catch (Exception ex)
                                        {

                                            Log.Error(ex.Message, ex);
                                        }
                                        if (RefreshNotPrintedCount != null)
                                            RefreshNotPrintedCount(null, null);
                                        return false;
                                    }

                                }
                            }
                        }
                        if (RefreshNotPrintedCount != null)
                            RefreshNotPrintedCount(null, null);
                        return false;
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                if (RefreshNotPrintedCount != null)
                    RefreshNotPrintedCount(null, null);
                return false;
            }
        }
        private static void CancelAllJobs(ManagementObject printer)
        {
            printer.InvokeMethod("CancelAllJobs", null);
        }

        private static int GetWaitTime(PrintJobInfoCollection printQueue)
        {
            if (printQueue == null || !(printQueue.Any()))
                return 10;

            return printQueue.Sum(j => j.NumberOfPages) * 5 > 10
                               ? printQueue.Sum(j => j.NumberOfPages) * 5
                               : 10;
        }

        /// <summary>
        /// prints all multiWays of a ticket
        /// </summary>
        /// <param name="pTips">All MultiWayTops to print</param>
        /// <param name="HasNormalBankTips">specifies if the ticket also has bankTips and therefore should display "Bank-Tips" and "MultiWay-Tips headlines</param>
        /// <param name="pBankCount">Number of banks, so last bank is not underlined</param>
        /// <param name="pMultiWayCount">Nunber of MultiWays, currently not used (2011-01-28)</param>
        /// <returns></returns>
        private String PrintMultiWayTips(IList<TipWS> pTips, bool HasNormalBankTips, int pBankCount, int pMultiWayCount, bool livebet)
        {
            pTips = pTips.OrderBy(x => x.bankGroupID).ToList();
            StringBuilder allSB = new StringBuilder();
            StringBuilder bankSB = new StringBuilder();
            StringBuilder multiWaySB = new StringBuilder();
            int bankCount = 0;
            int multiWayCount = 0;

            bankSB.Append(" <Line Underline=\"false\"><FillItem>=</FillItem></Line>");
            bankSB.Append("<Line Underline=\"false\">");
            bankSB.Append("<TextItem Alignment=\"Right\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_BANK) + "</TextItem> </Line>");
            bankSB.Append(" <Line Underline=\"false\"><FillItem>=</FillItem></Line>");

            if (HasNormalBankTips)
            {
                multiWaySB.Append(" <Line Underline=\"false\"><FillItem>=</FillItem></Line>");
                multiWaySB.Append("<Line Underline=\"false\">");
                multiWaySB.Append("<TextItem Alignment=\"Right\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_PATH) + "</TextItem> </Line>");
                multiWaySB.Append(" <Line Underline=\"false\"><FillItem>=</FillItem></Line>");
            }
            long curBankGroupID = -1;
            for (int i = 0; i < pTips.Count; i++)
            {
                bool underline = false;
                if (pTips[i].bank)
                {
                    //if (bankCount < pBankCount - 1)
                    {
                        underline = true;
                    }
                    //if (printBankSeparator)
                    //{
                    //    bankSB.Append(" <Line Underline=\"false\"><FillItem>-</FillItem></Line>");
                    //}
                    //else
                    //{
                    //    printBankSeparator = true;
                    //}

                    bankSB.Append(PrintMatch(pTips[i], pTips[i].odd, underline, livebet));
                    curBankGroupID = pTips[i].bankGroupID;
                    bankCount++;
                }
                else
                {
                    if (curBankGroupID == pTips[i].bankGroupID)
                    {
                        //if (i < pTips.Count - 1 && pTips[i + 1].bankGroupID != pTips[i].bankGroupID)
                        {
                            underline = true;
                        }

                        var odd = Repository.GetOddBySvrId(pTips[i].svrOddID);

                        if (odd == null)
                        {
                            Log.Error("Odd not found for (svrOddId=" + pTips[i].svrOddID + ")", new Exception());
                            return string.Empty;
                        }

                        if (odd != null && odd.BetDomainView != null && odd.BetDomainView.IsToInverse)
                        {
                            string betDomainName = String.Format(LineSr.Instance.AllObjects.TaggedStrings.GetStringSafely(odd.BetDomainView.LineObject.NameTag, SelectedLanguage), odd.SpecialBetdomainValue);

                            multiWaySB.Append("<Line Underline=\"false\">");
                            multiWaySB.Append("<TextItem Alignment=\"Left\">" + betDomainName + ": </TextItem></Line>");
                            betDomainOffset = 0;

                        }

                        multiWaySB.Append("<Line Underline=\"" + underline.ToString().ToLowerInvariant() + "\"><TextItem Font=\"FontA\">");

                        multiWaySB.Append(' ', betDomainOffset); //insert as many spaces as needed
                        //string oddname = Repository.GetOddBySvrId(pTips[i].svrOddID).LineObject.GetDisplayName(SelectedLanguage);

                        IMatchLn match = odd.LineObject.BetDomain != null ? odd.LineObject.BetDomain.Match : null;

                        if (match == null)
                        {
                            Log.ErrorFormat("Match not found for odd {0}", new Exception(), odd);
                            return string.Empty;
                        }

                        string oddname = Repository.GetOddBySvrId(pTips[i].svrOddID).LineObject.OddView.DisplayNameForPrinting(SelectedLanguage);

                        multiWaySB.Append(oddname + "</TextItem>");
                        multiWaySB.Append("<TextItem Alignment=\"Right\" Font=\"FontA\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TICKET_ODD) +
                                          Decimal2String(pTips[i].odd, 2) + "</TextItem></Line>");
                    }
                    else
                    {
                        multiWaySB.Append(PrintMatch(pTips[i], pTips[i].odd, true/*false*/, livebet));
                        curBankGroupID = pTips[i].bankGroupID;
                        multiWayCount++;
                    }
                }
            }
            if (bankCount > 0)
            {
                allSB.Append(bankSB);
            }
            if (multiWayCount > 0)
            {
                allSB.Append(multiWaySB);
            }
            return allSB.ToString();
        }


        public static string Decimal2String(decimal value, decimal factor)
        {
            //if (num < 2)
            //    return String.Format("{0:F3}", value);
            //if (value < 10 * factor)
            //    return String.Format("{0:F2}", value);
            //if (value < 100 * factor)
            //    return String.Format("{0:F1}", value);
            if (factor == 2)
                return String.Format("{0:F2}", value, CultureInfo.InvariantCulture);
            if (factor == 1)
                return String.Format("{0:F1}", value, CultureInfo.InvariantCulture);
            return String.Format("{0:F0}", value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Prints a credit note
        /// </summary>
        /// <param name="credit">Value of Credit</param>
        /// <param name="ticketNbr">Number of Credit note similar to TicketNumber</param>
        /// <param name="ticketChksum">random checkSum of TicketNumber</param>
        /// <param name="cancel">prints "Storno" if true, owherwise "CreditNote" </param>
        /// <param name="acceptedTime">time when the ticket has been accepted</param>
        /// <param name="paidTime">time when ticket is paid</param>
        /// <returns>true if ok</returns>
        public bool PrintCreditNote(decimal credit, String ticketNbr, String ticketChksum, bool cancel,
                                           DateTime acceptedTime, DateTime paidTime)
        {
            if (!StationRepository.UsePrinter)
            {
                MessageBox.Show("Creditnote " + credit);
                return true;
            }
            Nbt.Services.Spf.Printer.IPrinter printer = InitPrinter(true);
            if (printer == null)
            {
                return false;
            }
            int creditNoteExpiryDays = StationRepository.CreditNoteExpirationDays;
            String barcodeStr = BarCodeConverter.ConvertNumber2CreditNoteBarcode(ticketNbr + ticketChksum);
            StringBuilder sb = new StringBuilder();
            sb.Append(printHeader());
            if (cancel)
            {
                sb.Append(
                    "<Line Underline=\"true\"><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"2\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_CREDIT_STORNO) + "</TextItem></Line>");
            }
            else if (acceptedTime != DateTime.MinValue && paidTime != DateTime.MinValue)
            {
                sb.Append(
                    "<Line Underline=\"true\"><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"2\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_CREDIT_WINNER) + "</TextItem></Line>");
            }
            else
            {
                sb.Append(
                    "<Line Underline=\"true\"><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"2\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_CREDIT_HEADER) + "</TextItem></Line>");
            }
            sb.Append("<Line><FillItem> </FillItem></Line>");


            sb.Append("<Line>");
            sb.Append("<BarCodeItem Alignment=\"Center\" HRIPos=\"None\" Height=\"100\"  Type=\"CODE93\">" + barcodeStr +
                      "</BarCodeItem>");
            sb.Append("</Line>");

            if (!cancel && acceptedTime != DateTime.MinValue && paidTime != DateTime.MinValue)
            {
                sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"1\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_ACCEPTED_AT) + "</TextItem>");
                sb.Append("<TextItem  Alignment=\"Right\" Font=\"FontB\">" + acceptedTime.ToString("dd.MM.yyyy HH:mm") + "</TextItem> </Line>");
                sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"1\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_PAID_AT) + "</TextItem>");
                sb.Append("<TextItem  Alignment=\"Right\" Font=\"FontB\">" + paidTime.ToString("dd.MM.yyyy HH:mm") + "</TextItem> </Line>");
                /*sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"1\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_ACCEPTED_AT) + "</TextItem>");
                sb.Append("<TextItem  Alignment=\"Right\" Font=\"FontB\">" + acceptedTime.ToShortDateString() + " " +
                          Nbt.Common.Utils.Date.DateTimeUtils.DisplayTimeShort(acceptedTime, StationRepository.SystemCulture) +
                          "</TextItem> </Line>");
                sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"1\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_PAID_AT) + "</TextItem>");
                sb.Append("<TextItem  Alignment=\"Right\" Font=\"FontB\">" + paidTime.ToShortDateString() + " " +
                          Nbt.Common.Utils.Date.DateTimeUtils.DisplayTimeShort(paidTime, StationRepository.SystemCulture) +
                          "</TextItem> </Line>");*/
            }
            else
            {
                sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"1\">" + DateTime.Today.ToString("dd.MM.yyyy") + "</TextItem>");
                sb.Append("<TextItem  Alignment=\"Right\" Font=\"FontB\">" + DateTime.Now.ToString("HH:mm") + "</TextItem></Line>");
                /*sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"1\">" + DateTime.Today.ToShortDateString() +
                          "</TextItem>");
                //sb.Append("<FillItem>*</FillItem>");
                sb.Append("<TextItem  Alignment=\"Right\" Font=\"FontB\">" +
                          Nbt.Common.Utils.Date.DateTimeUtils.DisplayTimeShort(DateTime.Now, StationRepository.SystemCulture) +
                          "</TextItem> </Line>");*/
                sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"1\">" +
                          TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_CREDITNOTE_EXPIRY_DATE) + "</TextItem>");
                sb.Append("<TextItem  Alignment=\"Right\" Font=\"FontB\">" + DateTime.Today.AddDays(creditNoteExpiryDays).ToString("dd.MM.yyyy") + "</TextItem></Line>");
            }
            sb.Append("<Line Underline=\"false\">");
            sb.Append("<TextItem  Alignment=\"Left\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TICKET_NUM_SHORT) + "</TextItem>");
            sb.Append("<TextItem  Alignment=\"Right\" Size=\"2\" Style=\"None\">" + ticketNbr + "</TextItem></Line>");
            sb.Append("<Line Underline=\"false\">");
            sb.Append("<TextItem  Alignment=\"Left\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_TICKETCODE) + "</TextItem>");
            sb.Append("<TextItem  Alignment=\"Right\" Size=\"2\" Style=\"None\">" + ticketChksum + "</TextItem></Line>");
            sb.Append("<Line><FillItem>-</FillItem></Line>");

            sb.Append("<Line Underline=\"false\">");
            sb.Append("<TextItem  Alignment=\"Left\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_CREDIT_DESC) + "</TextItem>");
            sb.Append("<TextItem  Alignment=\"Right\" Size=\"4\" Style=\"Bold\">" + String.Format("{0:F2}", credit) + " " + StationRepository.Currency +
                      "</TextItem></Line>");
            sb.Append(printFooter());
            //sb.Append("<Line><FillItem>=</FillItem></Line>");
            //sb.Append(" <Line Underline=\"1\"><FillItem>  </FillItem></Line>");
            sb.Append("<Line><FillItem>-</FillItem></Line>");
            sb.Append("</Page>");
            sb.Replace('&', '+'); //Printer is not able to print '&' character
            Nbt.Services.Spf.XmlPreprocess xmlPre = new Nbt.Services.Spf.XmlPreprocess(sb.ToString(), false);

            bool found = PrintDefaultTicket(printer, xmlPre, true);
            if (!found)
            {
                WsdlRepository.StationEvent(StationRepository.StationNumber, StationEventType.PRINTER_ERROR);
                StationRepository.PrinterStatus = PrinterHandler.PRINTER_NOT_FOUND;
            }
            return found;
        }

        /// <summary>
        /// Prints a credit note
        /// </summary>
        /// <param name="credit">Value of Credit</param>
        /// <param name="ticketNbr">Number of Credit note similar to TicketNumber</param>
        /// <param name="ticketChksum">random checkSum of TicketNumber</param>
        /// <param name="cancel">prints "Storno" if true, owherwise "CreditNote" </param>
        /// <param name="acceptedTime">time when the ticket has been accepted</param>
        /// <param name="paidTime">time when ticket is paid</param>
        /// <returns>true if ok</returns>
        public bool PrintStoredTicket(TicketWS ticket, String pin, string stationNumber, DateTime expireDate, string userid)
        {
            bool livebet = ticket.ticketTyp == Ticket.TICKET_TYP_LIVEBET || ticket.ticketTyp == Ticket.TICKET_TYP_BOTH;

            StringBuilder sb = new StringBuilder();
            decimal bonusfactor = 1;
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?><Page xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"SPFSchema.xsd\" Codepage=\"850\">");


            if (!StationRepository.UsePrinter)
            {
                MessageBox.Show("Stored ticket " + ticket.ticketNbr + " " + ticket.checkSum + " (pin: " + pin + ")");
                return true;
            }
            Nbt.Services.Spf.Printer.IPrinter printer = InitPrinter(false);
            if (printer == null)
            {
                return false;
            }
            foreach (BetWS bet in ticket.bets)
            {
                String barcodeStr = BarCodeConverter.ConvertToStoreTicket(ticket.ticketNbr + ticket.checkSum, pin);

                sb.Append("<Line>");
                sb.Append("<BarCodeItem Alignment=\"Center\" HRIPos=\"None\" Height=\"100\"  Type=\"CODE93\">" + barcodeStr +
                          "</BarCodeItem>");
                sb.Append("</Line>");

                sb.Append(printHeaderForTicket());

                sb.Append("<Line Underline=\"true\"><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_STORE_TICKET_HEADER) + "</TextItem></Line>");

                //sb.Append("<Line><TextItem Alignment=\"Center\" Indent=\"1\" Characterspacing=\"3\" Font=\"FontA\" Size=\"1\">"+num+"</TextItem> </Line>");//test code TODO : remove Test Code
                //sb.Append("<Line><FillItem>-</FillItem></Line>");

                sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"1\">" +
                          DateTimeUtils.DisplayNormalDate(DateTime.Now, StationRepository.Culture) + "</TextItem>");
                //sb.Append("<FillItem>*</FillItem>");
                sb.Append("<TextItem  Alignment=\"Right\" Font=\"FontB\">" +
                          DateTimeUtils.DisplayTimeShort(DateTime.Now, StationRepository.Culture) +
                          "</TextItem> </Line>");
                sb.Append("<Line Underline=\"false\">");
                sb.Append("<TextItem  Alignment=\"Left\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TICKET_NUM_SHORT) + "</TextItem>");
                sb.Append("<TextItem  Alignment=\"Right\" Size=\"2\" Style=\"None\">" + ticket.ticketNbr + "</TextItem></Line>");
                sb.Append("<Line Underline=\"false\">");
                sb.Append("<TextItem  Alignment=\"Left\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_TICKETCODE) + "</TextItem>");
                sb.Append("<TextItem  Alignment=\"Right\" Size=\"2\" Style=\"None\">" + ticket.checkSum + "</TextItem></Line>");

                sb.Append("<Line Underline=\"false\">");
                sb.Append("<TextItem  Alignment=\"Left\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TICKET_PIN) + "</TextItem>");
                sb.Append("<TextItem  Alignment=\"Right\" Size=\"2\" Style=\"None\">" + pin + "</TextItem></Line>");
                sb.Append("<Line><FillItem>=</FillItem></Line>");
                //sb.Append(" <Line Underline=\"1\"><FillItem>  </FillItem></Line>");


                sb.Append("<Line Underline=\"false\">");
                sb.Append("<TextItem Alignment=\"Left\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_BET_TYPE) + "</TextItem>");


                long prevBankGroupID = -1;
                int bankCount = 0;
                int multiWayCount = 0;
                foreach (TipWS tip in bet.tips2BetMulti)
                {
                    if (tip.bankGroupID != prevBankGroupID && !tip.bank)
                    {
                        multiWayCount++;
                        prevBankGroupID = tip.bankGroupID;
                    }
                    if (tip.bank)
                    {
                        bankCount++;
                    }
                }

                if (bet.betType == Bet.BET_TYPE_SYSTEM || bet.betType == Bet.BET_TYPE_SYSTEMPATH)
                {
                    sb.Append("<TextItem Alignment=\"Right\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_SYSTEM) + " " + bet.systemX + "/" + bet.systemY);
                    if (bankCount > 0)
                    {
                        sb.Append("+" + bankCount + "B");
                    }
                    if (multiWayCount > 0)
                    {
                        sb.Append("+" + multiWayCount + "W");
                    }
                    sb.Append("</TextItem> </Line>");
                    sb.Append("<Line><FillItem>=</FillItem></Line>");
                    for (int i = 0; i < bet.bankTips.Length; i++)
                    {
                        sb.Append(PrintMatch(bet.bankTips[i], bet.bankTips[i].odd, true/*i < bet.bankTips.Length - 1*/, livebet));
                    }
                    sb.Append(PrintMultiWayTips(bet.tips2BetMulti, true, bankCount, multiWayCount, livebet));
                    //It is impossible to have a system without system tipps
                }
                else if (bet.betType == Bet.BET_TYPE_COMBI || bet.betType == Bet.BET_TYPE_COMBIPATH)
                {
                    sb.Append("<TextItem Alignment=\"Right\" Style=\"Bold\">");
                    if (bet.bankTips.Length == 0)
                    {
                        sb.Append(TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_PATH));
                    }
                    else
                    {
                        sb.Append(TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_COMBI));
                        if (bankCount > 0)
                            sb.Append("+" + bankCount + "W");
                    }
                    sb.Append("</TextItem> </Line>");
                    sb.Append("<Line><FillItem>=</FillItem></Line>");
                    for (int i = 0; i < bet.bankTips.Length; i++)
                    {
                        sb.Append(PrintMatch(bet.bankTips[i], bet.bankTips[i].odd, /*i < bet.bankTips.Length - 1*/ true, livebet));
                    }
                    sb.Append(PrintMultiWayTips(bet.tips2BetMulti, bet.bankTips.Length != 0, bankCount, multiWayCount, livebet));
                }
                else
                {
                    //SINGLE:
                    sb.Append("<TextItem Alignment=\"Right\" Size=\"2\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_SINGLE) + "</TextItem> </Line>");
                    sb.Append("<Line><FillItem>=</FillItem></Line>");

                    foreach (TipWS tip in bet.bankTips)
                    {
                        sb.Append(PrintMatch(tip, tip.odd, false, livebet));
                    }
                }

                bonusfactor = ticket.superBonus;
                if (bonusfactor > 1.001m)
                {
                    var bonus = TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_BONUS_DESC);
                    sb.Append("<Line> <TextItem Alignment=\"Center\" Indent=\"1\" Font=\"FontA\" Style=\"Bold\">" + bonus +
                              "</TextItem></Line>");
                    sb.Append("<Line> <TextItem Alignment=\"Center\" Indent=\"1\" Font=\"FontB\" >" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_BONUS_TEXT) +
                              "</TextItem></Line>");
                    sb.Append("<Line Underline=\"false\">");
                    sb.Append("<TextItem Font=\"FontA\" Alignment=\"Left\" Style=\"Bold\">" + bonus + " +" +
                              String.Format("{0:F2}", (bonusfactor - 1) * 100) + "%" + "</TextItem>");
                    sb.Append("<TextItem Alignment=\"Right\" Font=\"FontA\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TICKET_ODD) +
                              String.Format("{0:F2}", bonusfactor) + "</TextItem></Line>");
                }
            }
            sb.Append("<Line><FillItem>=</FillItem></Line>");
            sb.Append("<Line Underline=\"false\">");
            sb.Append("<TextItem  Alignment=\"Left\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_EXPIREDATE) + "</TextItem>");
            sb.Append("<TextItem  Alignment=\"Right\" Size=\"2\" Style=\"None\">" +
                      DateTimeUtils.DisplayNormalDate(expireDate, StationRepository.Culture) +
                      "</TextItem></Line>");

            sb.Append(printFooter());
            sb.Append("</Page>");
            sb.Replace('&', '+'); //Printer is not able to print '&' character
            Nbt.Services.Spf.XmlPreprocess xmlPre = new Nbt.Services.Spf.XmlPreprocess(sb.ToString(), false);

            bool found = PrintDefaultTicket(printer, xmlPre, true);
            if (!found)
            {
                WsdlRepository.StationEvent(StationRepository.StationNumber, StationEventType.PRINTER_ERROR);
                StationRepository.PrinterStatus = PrinterHandler.PRINTER_NOT_FOUND;
            }
            return found;
        }


        /// <summary>
        /// Prints a message informing that deposit isn't lost
        /// </summary>
        public bool PrintDepositLostMessage(decimal credit, string sUsername)
        {
            if (!StationRepository.UsePrinter)
            {
                MessageBox.Show("The connection to server is lost. Your deposit is not lost");
                return true;
            }
            Nbt.Services.Spf.Printer.IPrinter printer = InitPrinter(false);
            if (printer == null)
            {
                return false;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(printHeader());
            sb.Append(
                "<Line Underline=\"true\"><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"2\" Style=\"None\">" +
                TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_CONNECTION_LOST_HEADER) + "</TextItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");


            sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"1\">" + DateTime.Today.ToShortDateString() +
                      "</TextItem>");
            sb.Append("<TextItem  Alignment=\"Right\" Font=\"FontB\">" +
                      DateTimeUtils.DisplayTimeShort(DateTime.Now,
                                                                           StationRepository.Culture) +
                      "</TextItem> </Line>");
            //sb.Append("<Line Underline=\"false\">");

            //IoCContainer.Kernel.Get<IChangeTracker>().
            sb.Append(SplitStringBySpaces(sUsername + ", " +
                TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_CONNECTION_LOST_DEPOSIT_BODY) + " " +
                TranslationProvider.TranslateForPrinter(MultistringTags.DEPOSIT) + " " + credit + " " + StationRepository.Currency + " " +
                StationRepository.Currency, 30, 2));
            sb.Append(printFooter());
            sb.Append("<Line><FillItem>-</FillItem></Line>");
            sb.Append("</Page>");
            sb.Replace('&', '+'); //Printer is not able to print '&' character
            Nbt.Services.Spf.XmlPreprocess xmlPre = new Nbt.Services.Spf.XmlPreprocess(sb.ToString(), false);

            bool found = PrintDefaultTicket(printer, xmlPre, false);
            if (!found)
            {
                WsdlRepository.StationEvent(StationRepository.StationNumber, StationEventType.PRINTER_ERROR);
                StationRepository.PrinterStatus = PrinterHandler.PRINTER_NOT_FOUND;
            }
            return found;
        }

        private static string SplitStringBySpaces(string sIn, int iLength, int font_size)
        {
            string[] words = sIn.Split(' ');
            string sOut = "";
            List<string> arrOut = new List<string>();

            int iCurLen = 0;
            string sCurLine = "";
            foreach (string word in words)
            {
                if (iCurLen + word.Length > iLength)
                {
                    // add ne line
                    arrOut.Add(sCurLine);
                    sCurLine = word + " ";
                    iCurLen = word.Length;
                }
                else
                {
                    sCurLine += word + " ";
                    iCurLen = sCurLine.Length;
                }
            }
            arrOut.Add(sCurLine);

            foreach (string s in arrOut)
            {
                sOut += "<Line><TextItem Alignment=\"Left\" Characterspacing=\"2\" Size=\"" + font_size.ToString() + "\" Style=\"None\">" + s + "</TextItem></Line>";
            }

            return sOut;
        }

        /// <summary>
        /// Prints a cash balance receipt
        /// </summary>
        /// <returns>true if ok</returns>
        public bool PrintCashBalance(Dictionary<Decimal, int> cashinNotes, DateTime startDate, DateTime endDate, decimal cashin, decimal cashout, decimal collected, bool isDuplicate, bool isBallanceInfo, string operatorName, int id)
        {
            if (!StationRepository.UsePrinter)
            {
                return true;
            }
            Nbt.Services.Spf.Printer.IPrinter printer = InitPrinter(false);
            if (printer == null)
            {
                return false;
            }


            StringBuilder sb = new StringBuilder();
            //CashDrawerTyp SportBetTyp = CashDrawerTypFactory.LoadCashDrawerTypByQuery("Typ = '0'", "CashDrawerTypID");
            sb.Append(printHeader());
            if (isBallanceInfo)
            {
                sb.Append(
                    "<Line Underline=\"true\"><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_BALLANCE_INFORMATION) + " </TextItem></Line>");
            }
            else
            {
                sb.Append(
                    "<Line Underline=\"true\"><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_CASH_DRAWER_TITLE) + " </TextItem></Line>");
            }
            //GMU 2011-06-01 now printing StationNumber in separate line for better visibility (on TSP100-COM-Emulation Station-Number was divided and written on two lines.... 99 \n 99
            if (isBallanceInfo)
            {
                sb.Append(
                    "<Line Underline=\"false\"><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    StationRepository.StationNumber + "</TextItem></Line>");
            }
            else
            {
                //update: 4.4.2013, now printing also # Date, ID and Name Station, Location Franchisor
                /////////////// sb.Append("<Line><FillItem> </FillItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "</TextItem></Line>");
                sb.Append("<Line><FillItem> </FillItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_STATION_NAME) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + ": " + StationRepository.StationName + "</TextItem><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + "NR: " + StationRepository.StationNumber + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FRANCHISOR) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + ": " + StationRepository.FranchiserName + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_LOCATION) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + ": " + StationRepository.LocationName + "</TextItem></Line>");
                sb.Append("<Line><FillItem>-</FillItem></Line>");
                //update block end
            }

            #region showing count of notes and coints

            Dictionary<decimal, int> cashNotes =
                 new Dictionary<decimal, int>();
            cashNotes.Add(200, 0);
            cashNotes.Add(100, 0);
            cashNotes.Add(50, 0);
            cashNotes.Add(20, 0);
            cashNotes.Add(10, 0);
            cashNotes.Add(5, 0);

            Dictionary<decimal, int> coints =
                new Dictionary<decimal, int>();
            coints.Add(2, 0);
            coints.Add(1, 0);
            coints.Add(0.50m, 0);
            coints.Add(0.20m, 0);
            coints.Add(0.10m, 0);
            coints.Add(0, 0);
            int i = 0;

            List<decimal> list = new List<decimal>(coints.Keys);

            if (!paper_width_2_inch)
            {

                sb.Append("<Line><TextItem Alignment=\"Center\" Font=\"FontB\" Size=\"2\">" + String.Format("{0,8}", TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_BANKNOTES) + "(" + " " + StationRepository.Currency + "):") +
                          "</TextItem><TextItem  Alignment=\"Center\" Font=\"FontB\" Size=\"2\">" + String.Format("{0,8}", TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_COINTS) + "(" + " " + StationRepository.Currency + "):") +
                              "</TextItem> </Line>");

                sb.Append("<Line><TextItem Alignment=\"Center\" Font=\"FontB\" Size=\"2\">" + String.Format("{0,8}", TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT)) +
                          "</TextItem><TextItem  Alignment=\"Center\" Font=\"FontB\" Size=\"2\">" + String.Format("{0,8}", TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_QUANTITY)) +
                              "</TextItem>" +
                          "<TextItem Alignment=\"Center\" Font=\"FontB\" Size=\"2\">" + String.Format("{0,8}", TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT)) +
                          "</TextItem><TextItem  Alignment=\"Center\" Font=\"FontB\" Size=\"2\">" + String.Format("{0,8}", TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_QUANTITY)) +
                              "</TextItem> </Line>");



                foreach (var cashNote in cashNotes)
                {
                    int countNotes = 0;
                    int countCoins = 0;

                    //notes
                    if (cashinNotes.ContainsKey(cashNote.Key))
                    {
                        countNotes = cashinNotes[cashNote.Key];
                    }
                    if (list[i] != 0)
                    {
                        if (cashinNotes.ContainsKey(list[i]))
                        {
                            countCoins = cashinNotes[list[i]];
                        }
                        else if (cashinNotes.ContainsKey(list[i]))
                        {
                            countCoins = cashinNotes[list[i]];
                        }
                    }
                    string cointCountString = list[i] != 0 ? countCoins.ToString() : "";

                    if (list[i] != 0)
                    {

                        sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"2\">" + string.Format("{0,10}", string.Format("{0:N2}", cashNote.Key) + "\t") + string.Format("{0,10}", (countNotes + "\t")) + string.Format("{0,10}", string.Format("{0:N2}", list[i]) + "\t" + "\t") + cointCountString + "</TextItem> </Line>");
                    }
                    else
                    {
                        sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"2\">" + string.Format("{0,10}", string.Format("{0:N2}", cashNote.Key) + "\t") + string.Format("{0,10}", (countNotes + "\t")) + string.Format("{0,10}", "" + "\t" + "\t") + cointCountString + "</TextItem> </Line>");

                    }
                    i++;
                }
            }
            else
            {
                sb.Append("<Line><TextItem Alignment=\"Center\" Font=\"FontB\" Size=\"2\">" + String.Format("{0,8}", TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_BANKNOTES) + "(" + " " + StationRepository.Currency + "):") + "</TextItem> </Line>");
                sb.Append("<Line><TextItem Alignment=\"Center\" Font=\"FontB\" Size=\"2\">" + String.Format("{0,8}", TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT)) +
                          "</TextItem><TextItem  Alignment=\"Center\" Font=\"FontB\" Size=\"2\">" + String.Format("{0,14}", TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_QUANTITY)) +
                           "</TextItem> </Line>");

                foreach (var cashNote in cashNotes)
                {
                    int countNotes = 0;


                    //notes
                    if (cashinNotes.ContainsKey(cashNote.Key))
                    {
                        countNotes = cashinNotes[cashNote.Key];
                    }


                    sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"2\">" + string.Format("{0,10}", string.Format("{0:N2}", cashNote.Key)) +
                        "</TextItem><TextItem  Alignment=\"Center\" Font=\"FontB\" Size=\"2\">" +
                        string.Format("{0,10}", countNotes) + "</TextItem> </Line>");

                }

                sb.Append("<Line><TextItem Alignment=\"Center\" Font=\"FontB\" Size=\"2\">" + String.Format("{0,8}", TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_COINTS) + "(" + " " + StationRepository.Currency + "):") + "</TextItem> </Line>");
                sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"2\">" + String.Format("{0,8}", TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT)) +
                          "</TextItem><TextItem  Alignment=\"Center\" Font=\"FontB\" Size=\"2\">" + String.Format("{0,14}", TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_QUANTITY)) +
                           "</TextItem> </Line>");

                foreach (var cashNote in coints)
                {
                    int countNotes = 0;


                    //notes
                    if (cashinNotes.ContainsKey(cashNote.Key))
                    {
                        countNotes = cashinNotes[cashNote.Key];
                    }


                    sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"2\">" + string.Format("{0,10}", string.Format("{0:N2}", cashNote.Key)) +
                        "</TextItem><TextItem  Alignment=\"Center\" Font=\"FontB\" Size=\"2\">" +
                        string.Format("{0,10}", countNotes) + "</TextItem> </Line>");

                }

            }
            sb.Append("<Line><FillItem>-</FillItem></Line>");
            #endregion showing count of notes and coints

            if (isDuplicate)
            {
                sb.Append(
                    "<Line Underline=\"true\"><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_DUPLICATE) + "</TextItem></Line>");
            }

            sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"2\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_CASH_DRAWER_FROM) +
                      "</TextItem>");
            sb.Append("<TextItem  Alignment=\"Right\" Font=\"FontB\" Size=\"2\">" + startDate.ToString("dd.MM.yyyy HH:mm") + "</TextItem> </Line>");

            sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"2\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_CASH_DRAWER_TO) +
                      "</TextItem>");
            sb.Append("<TextItem  Alignment=\"Right\" Font=\"FontB\" Size=\"2\">" + endDate.ToString("dd.MM.yyyy HH:mm") + "</TextItem> </Line>");
            string cashoutString = "<Line>";
            if (cashin > 0)
            {
                sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"2\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_CASH_DRAWER_CASH_IN) +
                          "</TextItem>");
                sb.Append("<TextItem  Alignment=\"Right\" Font=\"FontB\" Size=\"2\">" + cashin.ToString("N2") + " " + StationRepository.Currency +
                          "</TextItem> </Line>");
                cashoutString = "<Line Underline=\"true\">";
            }
            if (cashout > 0)
            {
                sb.Append("<Line Underline=\"true\"><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"2\">" +
                          TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_CASH_DRAWER_CASH_OUT) + "</TextItem>");
                sb.Append("<TextItem  Alignment=\"Right\" Font=\"FontB\" Size=\"2\">" + cashout.ToString("N2") + " " + StationRepository.Currency +
                          "</TextItem> </Line>");
            }

            if (!isBallanceInfo && collected > 0)
            {
                sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"2\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_CASH_COLLECTED) +
                          "</TextItem>");
                sb.Append("<TextItem  Alignment=\"Right\" Font=\"FontB\" Size=\"2\">" +
                          collected.ToString("N2") + " " + StationRepository.Currency +
                          "</TextItem> </Line>");
            }

            if (cashin > 0 && cashout > 0)
            {
                sb.Append(cashoutString + "<TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"2\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_CASH_DRAWER_DIFFERENCE) +
                          "</TextItem>");
                sb.Append("<TextItem  Alignment=\"Right\" Font=\"FontB\" Size=\"2\">" +
                          (cashin - cashout).ToString("N2") + " " + StationRepository.Currency +
                          "</TextItem> </Line>");
            }
            if (!isBallanceInfo)
            {

                sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"2\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_CASH_DRAWER_OPERATOR) +
                          "</TextItem>");
                sb.Append("<TextItem  Alignment=\"Right\" Font=\"FontB\" Size=\"2\">" + operatorName +
                          "</TextItem> </Line>");

                sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"2\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_CASH_DRAWER_CASH_OUT_ID).ToString
                              () +
                          "</TextItem>");
                sb.Append("<TextItem  Alignment=\"Right\" Font=\"FontB\" Size=\"2\">" + id + "</TextItem> </Line>");
            }
            ///////////sb.Append("<Line><FillItem>-</FillItem></Line>");
            sb.Append("</Page>");

            Nbt.Services.Spf.XmlPreprocess xmlPre = new Nbt.Services.Spf.XmlPreprocess(sb.ToString(), false);

            bool found = PrintDefaultTicket(printer, xmlPre, true);
            if (!found)
            {
                WsdlRepository.StationEvent(StationRepository.StationNumber, StationEventType.PRINTER_ERROR);
                StationRepository.PrinterStatus = PrinterHandler.PRINTER_NOT_FOUND;
            }
            return found;
        }

        /// <summary>
        /// creates a string containing the footer of any ticket
        /// </summary>
        /// <returns>a XML-String containing format of footer</returns>
        private static String printFooter()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<Line><FillItem>=</FillItem></Line>");
            if (StationRepository.FooterLine1 != null && StationRepository.FooterLine1 != "")
            {
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"1\" Style=\"None\" Font=\"FontB\">" +
                          StationRepository.FooterLine1 + "</TextItem></Line>");
            }
            if (StationRepository.FooterLine2 != null && StationRepository.FooterLine2 != "")
            {
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"1\" Style=\"None\" Font=\"FontB\">" +
                          StationRepository.FooterLine2 + "</TextItem></Line>");
            }
            if (StationRepository.BetTerms != null && StationRepository.BetTerms != "")
            {
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"1\" Style=\"None\" Font=\"FontB\">" +
                          StationRepository.BetTerms + "</TextItem></Line>");
            }
            return sb.ToString();
        }

        /// <summary>
        /// creates a string containing the footer of any ticket
        /// </summary>
        /// <returns>a XML-String containing format of header</returns>
        public static String printHeader()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?><Page xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"SPFSchema.xsd\" Codepage=\"850\">");
            if (StationRepository.PrintLogo)
            {
                sb.Append("<Line><ImageItem Alignment=\"Center\" Path=\"\" /></Line>");
            }
            if (!string.IsNullOrEmpty(StationRepository.HeaderLine1))
            {
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" +
                          StationRepository.HeaderLine1 + "</TextItem></Line>");
            }
            if (!string.IsNullOrEmpty(StationRepository.HeaderLine2))
            {
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" +
                          StationRepository.HeaderLine2 + "</TextItem></Line>");
            }
            if (!string.IsNullOrEmpty(StationRepository.HeaderLine3))
            {
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" +
                          StationRepository.HeaderLine3 + "</TextItem></Line>");
            }
            if (!string.IsNullOrEmpty(StationRepository.HeaderLine4))
            {
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" +
                          StationRepository.HeaderLine4 + "</TextItem></Line>");
            }

            return sb.ToString();
        }

        public static String printHeaderForTicket()
        {
            StringBuilder sb = new StringBuilder();

            if (StationRepository.PrintLogo)
            {
                sb.Append("<Line><ImageItem Alignment=\"Center\" Path=\"\" /></Line>");
            }
            if (!string.IsNullOrEmpty(StationRepository.HeaderLine1))
            {
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" +
                          StationRepository.HeaderLine1 + "</TextItem></Line>");
            }
            if (!string.IsNullOrEmpty(StationRepository.HeaderLine2))
            {
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" +
                          StationRepository.HeaderLine2 + "</TextItem></Line>");
            }
            if (!string.IsNullOrEmpty(StationRepository.HeaderLine3))
            {
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" +
                          StationRepository.HeaderLine3 + "</TextItem></Line>");
            }
            if (!string.IsNullOrEmpty(StationRepository.HeaderLine4))
            {
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" +
                          StationRepository.HeaderLine4 + "</TextItem></Line>");
            }

            return sb.ToString();
        }

        /// <summary>
        /// prints a short test string to test printer output
        /// </summary>
        /// <returns>true if ok (also if UsePrinter == false)</returns>
        public bool PrintTestString()
        {
            if (!StationRepository.UsePrinter)
            {
                StationRepository.PrinterStatus = PrinterHandler.PRINTER_READY;
                return true;
            }
            Log.Debug("Start printing Test-String");
            Nbt.Services.Spf.Printer.IPrinter printer = InitPrinter(true);
            if (printer == null)
            {
                return false;
            }
            SendLogoToPrinter();
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?><Page xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"SPFSchema.xsd\" Codepage=\"850\"><Line Underline=\"false\"><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TEST_PAGE) + " Station" + StationRepository.StationNumber + " " + DateTime.Now + "</TextItem></Line>");

            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem>-</FillItem></Line>");
            sb.Append("</Page>");
            Nbt.Services.Spf.XmlPreprocess xmlPre = new Nbt.Services.Spf.XmlPreprocess(sb.ToString(), false);

            bool found = PrintDefaultTicket(printer, xmlPre, false);
            if (!found)
            {
                WsdlRepository.StationEvent(StationRepository.StationNumber, StationEventType.PRINTER_ERROR);
                StationRepository.PrinterStatus = PrinterHandler.PRINTER_NOT_FOUND;
            }
            Log.Debug("Finished printing Test-String on Windows-Printer");
            return found;
        }



        /// <summary>
        /// sends logo.tmb image to ESC/POS compatible printer if file is found
        /// </summary>
        /// <returns>true if ok</returns>
        internal bool SendLogoToPrinter()
        {
            Nbt.Services.Spf.Printer.IPrinter printer = InitPrinter(true);
            String fileName = "Resources\\logo.tmb";
            if (printer == null)
            {
                Log.Debug("Printer not found");
                return false;
            }
            else if (!StationRepository.PrintLogo)
            {
                return false;
            }
            else if (!File.Exists(System.Windows.Forms.Application.StartupPath + "\\" + fileName))
            {
                Log.Debug("Logo-File not found: " + fileName);
                return false;
            }
            printer.DefineImage(fileName);
            return true;
        }

        public bool PrintPaymentNote(decimal credit, String nbr, DateTime acceptedTime)
        {
            if (!StationRepository.UsePrinter)
            {
                MessageBox.Show("Payment note printed " + credit);
                return true;
            }
            Nbt.Services.Spf.Printer.IPrinter printer = InitPrinter(false);
            if (printer == null)
            {
                return false;
            }
            String barcodeStr = BarCodeConverter.ConvertNumber2PaymentNoteBarcode(nbr);
            StringBuilder sb = new StringBuilder();
            sb.Append(printHeader());

            sb.Append("<Line Underline=\"true\"><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_PAYMENT_HEADER) + "</TextItem></Line>");

            sb.Append("<Line><FillItem> </FillItem></Line>");


            sb.Append("<Line>");
            sb.Append("<BarCodeItem Alignment=\"Center\" HRIPos=\"None\" Height=\"100\"  Type=\"CODE93\">" + barcodeStr + "</BarCodeItem>");
            sb.Append("</Line>");

            sb.Append("<Line><TextItem Alignment=\"Left\" Font=\"FontB\" Size=\"1\">" + acceptedTime.ToString("dd.MM.yyyy") + "</TextItem>");
            //sb.Append("<FillItem>*</FillItem>");
            sb.Append("<TextItem  Alignment=\"Right\" Font=\"FontB\">" + acceptedTime.ToString("HH:mm") + "</TextItem> </Line>");
            sb.Append("<Line Underline=\"false\">");

            sb.Append("<TextItem  Alignment=\"Left\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PAYMENT_NR) + "</TextItem>");
            sb.Append("<TextItem  Alignment=\"Right\" Size=\"2\" Style=\"None\">" + nbr + "</TextItem></Line>");
            sb.Append("<Line><FillItem>-</FillItem></Line>");

            sb.Append("<Line Underline=\"false\">");
            sb.Append("<TextItem  Alignment=\"Left\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_CREDIT_DESC) + "</TextItem>");
            sb.Append("<TextItem  Alignment=\"Right\" Size=\"4\" Style=\"Bold\">" + String.Format("{0:F2}", credit) + " " + StationRepository.Currency + "</TextItem></Line>");
            sb.Append(printFooter());
            //sb.Append("<Line><FillItem>=</FillItem></Line>");
            //sb.Append(" <Line Underline=\"1\"><FillItem>  </FillItem></Line>");
            sb.Append("<Line><FillItem>-</FillItem></Line>");
            sb.Append("</Page>");
            sb.Replace('&', '+'); //Printer is not able to print '&' character
            Nbt.Services.Spf.XmlPreprocess xmlPre = new Nbt.Services.Spf.XmlPreprocess(sb.ToString(), false);
            bool found = PrintDefaultTicket(printer, xmlPre, true);
            if (!found)
            {
                WsdlRepository.StationEvent(StationRepository.StationNumber, StationEventType.PRINTER_ERROR);

            }
            return found;
        }

        public void PrintChangeOrientationBarcode()
        {

            Nbt.Services.Spf.Printer.IPrinter printer = InitPrinter(false);
            StringBuilder sb = new StringBuilder();

            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?><Page xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"SPFSchema.xsd\" Codepage=\"850\"><Line Underline=\"false\"><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">Display Orientation Change Barcode </TextItem></Line>");
            sb.Append("<Line><FillItem>-</FillItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line>");
            sb.Append("<BarCodeItem Alignment=\"Center\" HRIPos=\"None\" Height=\"100\"  Type=\"CODE93\">" + "000000000000000000" + "</BarCodeItem>");
            sb.Append("</Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><FillItem>-</FillItem></Line>");
            sb.Append("</Page>");

            Nbt.Services.Spf.XmlPreprocess xmlPre = new Nbt.Services.Spf.XmlPreprocess(sb.ToString(), false);
            Print(printer, xmlPre);
        }

        public void PrintPinNote(string pin)
        {
            if (!StationRepository.UsePrinter)
            {
                MessageBox.Show("New pin " + pin);
                return;
            }

            Nbt.Services.Spf.Printer.IPrinter printer = InitPrinter(true);
            if (printer == null)
            {
                return;
            }

            SendLogoToPrinter();

            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?><Page xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"SPFSchema.xsd\" Codepage=\"850\"><Line Underline=\"false\"><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">PIN: </TextItem></Line>");
            sb.Append("<Line><FillItem>-</FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"3\" Style=\"None\">" + pin + "</TextItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem></FillItem></Line>");
            sb.Append("<Line><FillItem>-</FillItem></Line>");
            sb.Append("</Page>");


            Nbt.Services.Spf.XmlPreprocess xmlPre = new Nbt.Services.Spf.XmlPreprocess(sb.ToString(), false);

            Log.Debug("Printing PIN...");
            Print(printer, xmlPre);
        }

        private bool Print(Nbt.Services.Spf.Printer.IPrinter printer, Nbt.Services.Spf.XmlPreprocess xmlPre)
        {
            bool found = PrintDefaultTicket(printer, xmlPre, false);
            if (!found)
            {
                WsdlRepository.StationEvent(StationRepository.StationNumber, StationEventType.PRINTER_ERROR);
            }

            Log.Debug("Finished printing Test-String on Windows-Printer");
            return found;
        }


        /// <summary>
        /// prints the Account Receipt
        /// </summary>
        /// <returns>true if ok (also if UsePrinter == false)</returns>
        public bool PrintAccountReceipt(string type, AccountingRecieptWS reciept, DateTime start, DateTime end)
        {
            if (!StationRepository.UsePrinter)
            {
                return true;
            }

            Nbt.Services.Spf.Printer.IPrinter printer = InitPrinter(false);
            if (printer == null)
            {
                return false;
            }
            string TimeFormat = "dd.MM.yyyy HH:mm";

            StringBuilder sb = new StringBuilder();
            sb.Append(printHeader());

            if (type == "location")
            {
                sb.Append(
                    "<Line><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"3\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_LOCATION_ACCOUNT_RECIEPT) +
                    "</TextItem></Line>");
            }
            else
            {
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"3\" Font=\"FontB\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_ACCOUNT_RECIEPT) + "</TextItem></Line>");
            }

            sb.Append("<Line><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "</TextItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FRANCHISOR) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + ": " + StationRepository.FranchiserName + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_LOCATION) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + ": " + StationRepository.LocationName + "</TextItem></Line>");

            if (type == "terminal")
            {
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_STATION_NAME) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + ": " + StationRepository.StationName + "</TextItem><TextItem Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">ID: " + StationRepository.StationNumber + "</TextItem></Line>");
            }

            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_ACCOUNTING_PERIOD) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + start.ToString(TimeFormat) + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + end.ToString(TimeFormat) + "</TextItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");

            if ((reciept.pmTotalStakeCount != 0 || reciept.pmTotalPayoutCount != 0 ||
                 reciept.pmTotalWinStakeCount != 0) || StationRepository.IsPrematchEnabled ||
                (reciept.LocationAccountingRecieptProperties.enablePrematch != 0 && type == "location"))
            {
                // PREMATCH
                sb.Append("<Line><FillItem>-</FillItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_PREMATCH) + "</TextItem></Line>");
                sb.Append("<Line><FillItem>-</FillItem></Line>");
                // Single/Double
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_SINGLE) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Nr. Bets: " + reciept.pmSingleDoubleStakeCount + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.pmSingleDoubleStakeSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

                // Multi (3-10)
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_MULTI) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Nr: " + reciept.pmMultiStakeCount + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.pmMultiStakeSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

                // Multi (11+)
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\"  Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_MULTI_PLUS) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Nr: " + reciept.pmSuperMultiStakeCount + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.pmSuperMultiStakeSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

                sb.Append("<Line><FillItem>-</FillItem></Line>");


                // Total Stacke Prematch
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL_STAKE_PM) + "</TextItem></Line>");

                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Nr. " + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TICKETS) + ": " + reciept.pmTotalStakeCount.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Stake " + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.pmTotalSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

                // Total payout Prematch
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                  TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL_PAYOUT_PM) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                  TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TICKETS) + ": " + reciept.pmTotalPayoutCount.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                  TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.pmTotalPayoutSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");


                // Win
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_WIN) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Nr :" + reciept.pmTotalWinStakeCount.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.pmTotalWinSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

                sb.Append("<Line><FillItem>=</FillItem></Line>");
                // Total Prematch Tax
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL_PM_TAX) + ": " +
                    "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + reciept.pmTotalTaxSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

            }

            if ((reciept.mixedTotalStakeCount != 0 || reciept.mixedTotalPayoutCount != 0 ||
                 reciept.mixedTotalWinStakeCount != 0) || (StationRepository.AllowMixedStakes && StationRepository.AllowLive && StationRepository.IsPrematchEnabled) ||
                (reciept.LocationAccountingRecieptProperties.enablePrematch != 0 && reciept.LocationAccountingRecieptProperties.enableLive != 0 && reciept.LocationAccountingRecieptProperties.enableMixed != 0 && type == "location"))
            {
                // MIXED 
                sb.Append("<Line><FillItem>-</FillItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_MIXED) + "</TextItem></Line>");
                sb.Append("<Line><FillItem>-</FillItem></Line>");

                // Single/Double
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_SINGLE) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Nr. Bets: " + reciept.mixedSingleDoubleStakeCount.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.mixedSingleDoubleStakeSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

                // Multi (3-10)
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_MULTI) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Nr: " + reciept.mixedMultiStakeCount.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.mixedMultiStakeSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

                // Multi (11+)
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_MULTI_PLUS) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Nr: " + reciept.mixedSuperMultiStakeCount.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.mixedSuperMultiStakeSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");
                sb.Append("<Line><FillItem>-</FillItem></Line>");
                // Total Stake Mixed
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL_STAKE_MIXED) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Nr. " + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TICKETS) + ": " + reciept.mixedTotalStakeCount.ToString("d") + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Stake " + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.mixedTotalSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");
                // Total Payout Mixed
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL_PAYOUT_MIXED) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TICKETS) + ": " + reciept.mixedTotalPayoutCount.ToString("d") + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.mixedTotalPayoutSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

                // Win
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_WIN) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Nr :" + reciept.mixedTotalWinStakeCount.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.mixedTotalWinSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");
                sb.Append("<Line><FillItem>=</FillItem></Line>");

                // Total Mixed Tax
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL_MIX_TAX) + ": " +
                     "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + reciept.mixedTotalTaxSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

            }

            // LIVE
            if ((reciept.liveTotalStakeCount != 0 || reciept.liveTotalPayoutCount != 0 ||
                 reciept.liveTotalWinStakeCount != 0) || StationRepository.AllowLive ||
                (reciept.LocationAccountingRecieptProperties.enableLive != 0 && type == "location"))
            {
                sb.Append("<Line><FillItem>-</FillItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_LIVE) + "</TextItem></Line>");
                sb.Append("<Line><FillItem>-</FillItem></Line>");

                // Single/Double
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_SINGLE) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Nr. Bets: " + reciept.liveSingleDoubleStakeCount.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.liveSingleDoubleStakeSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

                // Multi (3-10)
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_MULTI) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Nr :" + reciept.liveMultiStakeCount.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.liveMultiStakeSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

                // Multi (11+)
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_MULTI_PLUS) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Nr :" + reciept.liveSuperMultiStakeCount.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.liveSuperMultiStakeSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

                sb.Append("<Line><FillItem>-</FillItem></Line>");
                // Total Stacke Live
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL_STAKE_LIVE) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Nr. " + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TICKETS) + ": " + reciept.liveTotalStakeCount.ToString("d") + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Stake " + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.liveTotalSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

                // Total Payout Live
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL_PAYOUT_LIVE) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TICKETS) + ": " + reciept.liveTotalPayoutCount.ToString("d") + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.liveTotalPayoutSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");


                // Win
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_WIN) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Nr :" + reciept.liveTotalWinStakeCount.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.liveTotalWinSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");
                sb.Append("<Line><FillItem>=</FillItem></Line>");

                // Total Live Tax
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL_LIVE_TAX) + ": " +
                     "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + reciept.liveTotalTaxSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

            }


            //new parts
            //VHC
            if ((reciept.vhcTotalPayoutCount != 0 || reciept.vhcTotalStakeCount != 0 || reciept.vhcTotalWinStakeCount != 0) || StationRepository.AllowVhc || (reciept.LocationAccountingRecieptProperties.enableVHC != 0 && type == "location"))
            {
                sb.Append("<Line><FillItem>-</FillItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_VHC) + "</TextItem></Line>");
                sb.Append("<Line><FillItem>-</FillItem></Line>");

                //// Single/Double
                //sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                //    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_SINGLE) + "</TextItem></Line>");
                //sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                //    "Nr. Bets: " + reciept.vhcSingleDoubleStakeCount.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                //    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.vhcSingleDoubleStakeSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

                //// Multi (3-10)
                //sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                //    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_MULTI) + "</TextItem></Line>");
                //sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                //    "Nr :" + reciept.vhcMultiStakeCount.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                //    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.vhcMultiStakeSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

                //// Multi (11+)
                //sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                //    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_MULTI_PLUS) + "</TextItem></Line>");
                //sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                //    "Nr :" + reciept.vhcSuperMultiStakeCount.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                //    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.vhcSuperMultiStakeSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

                sb.Append("<Line><FillItem>-</FillItem></Line>");
                // Total Stacke VHC
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL_STAKE_VHC) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Nr. " + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TICKETS) + ": " + reciept.vhcTotalStakeCount.ToString("d") + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Stake " + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.vhcTotalSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

                // Total Payout VHC
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL_PAYOUT_VHC) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TICKETS) + ": " + reciept.vhcTotalPayoutCount.ToString("d") + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.vhcTotalPayoutSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

                // Win
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_WIN) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Nr :" + reciept.vhcTotalWinStakeCount.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.vhcTotalWinSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");
                sb.Append("<Line><FillItem>=</FillItem></Line>");

                // Total VHC Tax
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL_VHC_TAX) + ": " +
                     "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + reciept.vhcTotalTaxSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

            }


            if ((reciept.vflTotalPayoutCount != 0 || reciept.vflTotalStakeCount != 0 ||
                 reciept.vflTotalWinStakeCount != 0) || StationRepository.AllowVfl || (reciept.LocationAccountingRecieptProperties.enableVFL != 0 && type == "location"))
            {
                //VFL
                sb.Append("<Line><FillItem>-</FillItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_VFL) + "</TextItem></Line>");
                sb.Append("<Line><FillItem>-</FillItem></Line>");

                //// Single/Double
                //sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                //    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_SINGLE) + "</TextItem></Line>");
                //sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                //    "Nr. Bets: " + reciept.vflSingleDoubleStakeCount.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                //    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.vflSingleDoubleStakeSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

                //// Multi (3-10)
                //sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                //    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_MULTI) + "</TextItem></Line>");
                //sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                //    "Nr :" + reciept.vflMultiStakeCount.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                //    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.vflMultiStakeSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

                //// Multi (11+)
                //sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                //    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_MULTI_PLUS) + "</TextItem></Line>");
                //sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                //    "Nr :" + reciept.vflSuperMultiStakeCount.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                //    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.vflSuperMultiStakeSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

                sb.Append("<Line><FillItem>-</FillItem></Line>");
                // Total Stacke VHC
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL_STAKE_VFL) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Nr. " + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TICKETS) + ": " + reciept.vflTotalStakeCount.ToString("d") + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Stake " + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.vflTotalSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

                // Total Payout VHC
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL_PAYOUT_VFL) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TICKETS) + ": " + reciept.vflTotalPayoutCount.ToString("d") + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.vflTotalPayoutSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

                // Win
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_WIN) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Nr :" + reciept.vflTotalWinStakeCount.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.vflTotalWinSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");
                sb.Append("<Line><FillItem>=</FillItem></Line>");

                // Total VFL Tax
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL_VFL_TAX) + ": " +
                     "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + reciept.vflTotalTaxSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

            }

            // TOTAL
            sb.Append("<Line><FillItem>-</FillItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL) + "</TextItem></Line>");
            sb.Append("<Line><FillItem>-</FillItem></Line>");

            // Total Stakes
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL_STAKES) + "</TextItem></Line>");

            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                "Nr. " + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TICKETS) + ": " + reciept.totalStakeCount.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                "Stake " + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.totalStakeSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

            // Total Wins
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL_WINS) + "</TextItem></Line>");

            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                "Nr :" + reciept.totalWinCount.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.totalWinSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

            // Paid out winnings
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"None\">" +
                TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL_PAYOUT_WIN) + "</TextItem></Line>");

            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                "Nr :" + reciept.totalPayoutCount.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + reciept.totalPayoutSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

            sb.Append("<Line><FillItem>=</FillItem></Line>");

            // Total Bet Balance
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL_BET_BALANCE) + ": " +
                 "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + (reciept.totalStakeSum - reciept.totalWinSum).ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

            // Unpaid winnings
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL_UNPAID_WINNINGS) + ": " +
                 "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + (reciept.totalWinSum - reciept.totalPayoutSum).ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

            // Total Tax
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL_TAX) + ": " +
                 "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + reciept.totalTaxSum.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");


            sb.Append("</Page>");

            Nbt.Services.Spf.XmlPreprocess xmlPre = new Nbt.Services.Spf.XmlPreprocess(sb.ToString(), false);

            bool found = PrintDefaultTicket(printer, xmlPre, false);
            if (!found)
            {
                WsdlRepository.StationEvent(StationRepository.StationNumber, StationEventType.PRINTER_ERROR);
                StationRepository.PrinterStatus = PrinterHandler.PRINTER_NOT_FOUND;
            }
            return found;
        }

        public bool PrintShopPaymentReciept(string type, decimal amount, long id, string username)
        {

            if (!StationRepository.UsePrinter)
            {
                return true;
            }

            Nbt.Services.Spf.Printer.IPrinter printer = InitPrinter(false);
            if (printer == null)
            {
                return false;
            }
            string TimeFormat = "dd.MM.yyyy HH:mm";

            StringBuilder sb = new StringBuilder();
            sb.Append(printHeader());

            if (type == "credit")
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"3\" Font=\"FontB\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_CREDIT_RECIEPT) + "</TextItem></Line>");
            else
                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"3\" Font=\"FontB\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PAYMENT_RECIEPT) + "</TextItem></Line>");


            sb.Append("<Line><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + DateTime.Now.ToString(TimeFormat) + "</TextItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FRANCHISOR) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + " " + StationRepository.FranchiserName + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_LOCATION) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + " " + StationRepository.LocationName + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_STATION_NAME) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + " " + StationRepository.StationName + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + "NR: " + StationRepository.StationNumber + "</TextItem></Line>");

            sb.Append("<Line><FillItem> </FillItem></Line>");

            if (type == "credit")
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_CREDIT) + " " + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_REGISTERED_BY) + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + "" + "</TextItem></Line>");
            else
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_PAYMENT) + " " + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_REGISTERED_BY) + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + "" + "</TextItem></Line>");

            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + " " + username + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + "ID: " + id.ToString() + "</TextItem></Line>");

            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><FillItem>-</FillItem></Line>");

            //amount
            if (type == "credit")
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_CREDIT) + " " + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + amount.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");
            else
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_PAYMENT) + " " + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + amount.ToString(specifier) + " " + StationRepository.Currency + "</TextItem></Line>");

            sb.Append("</Page>");

            Nbt.Services.Spf.XmlPreprocess xmlPre = new Nbt.Services.Spf.XmlPreprocess(sb.ToString(), false);

            bool found = PrintDefaultTicket(printer, xmlPre, false);
            if (!found)
            {
                WsdlRepository.StationEvent(StationRepository.StationNumber, StationEventType.PRINTER_ERROR);
                StationRepository.PrinterStatus = PrinterHandler.PRINTER_NOT_FOUND;
            }
            return found;
        }

        public bool PrintChechpointForTerminal(ProfitAccountingCheckpoint checkpoint, bool shopPaymentsReadLocationOwner)
        {
            Nbt.Services.Spf.Printer.IPrinter printer = InitPrinter(true);
            if (printer == null)
            {
                return false;
            }

            StringBuilder sb = new StringBuilder();
            string TimeFormat = "dd.MM.yyyy HH:mm";
            DateTime endDate = checkpoint.general.endDate.Value;
            DateTime startDate = checkpoint.general.startDate.Value;

            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?><Page xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"SPFSchema.xsd\" Codepage=\"850\">");

            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"3\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PROFIT_RECIEPT) + "</TextItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "</TextItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FRANCHISOR) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + " " + checkpoint.general.franchisor + "</TextItem><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + "ID: " + StationRepository.FranchisorID.ToString() + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_LOCATION) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + " " + checkpoint.general.location + "</TextItem><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + "ID: " + StationRepository.LocationID.ToString() + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_STATION_NAME) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + " " + checkpoint.station.stationName + "</TextItem><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + "ID: " + StationRepository.StationNumber + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_ACCOUNTING_PERIOD) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + startDate.ToString(TimeFormat) + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + endDate.ToString(TimeFormat) + "</TextItem></Line>");
            sb.Append("<Line><FillItem>-</FillItem></Line>");

            //cash part
            sb.Append("<Line><FillItem> </FillItem></Line>");
            if (checkpoint.station.totalCashIn.HasValue)
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TOTAL_CASH_IN) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + String.Format("{0:0.00}", checkpoint.station.totalCashIn.Value, CultureInfo.CurrentCulture) + " " + StationRepository.Currency + "</TextItem></Line>");
            else
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TOTAL_CASH_IN) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\"> </TextItem></Line>");

            if (checkpoint.station.totalCashOut.HasValue)
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TOTAL_CASH_OUT) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + String.Format("{0:0.00}", checkpoint.station.totalCashOut.Value, CultureInfo.CurrentCulture) + " " + StationRepository.Currency + "</TextItem></Line>");
            else
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TOTAL_CASH_OUT) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\"> </TextItem></Line>");

            if (checkpoint.station.tax.HasValue)
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TAX) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + String.Format("{0:0.00}", checkpoint.station.tax.Value, CultureInfo.CurrentCulture) + " " + StationRepository.Currency + "</TextItem></Line>");
            else
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TAX) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\"> </TextItem></Line>");

            if (checkpoint.station.model == "B")
            {
                //add net hold
            }
            else if (checkpoint.station.model == "A")
            {
                //add net cash
            }

            sb.Append("<Line><FillItem>-</FillItem></Line>");

            //share
            sb.Append("<Line><FillItem> </FillItem></Line>");

            if (checkpoint.station.basis.HasValue)
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_BASIS_FOR_PROFIT_SHARING) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + String.Format("{0:0.00}", checkpoint.station.basis.Value, CultureInfo.CurrentCulture) + " " + StationRepository.Currency + "</TextItem></Line>");
            else
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_BASIS_FOR_PROFIT_SHARING) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\"> </TextItem></Line>");

            if (checkpoint.station.model == "B")
            {
                if (checkpoint.station.fixStakeCommission.HasValue)
                    sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FIX_STAKE_COMMISSION) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + String.Format("{0:0.00}", checkpoint.station.fixStakeCommission.Value, CultureInfo.CurrentCulture) + " " + StationRepository.Currency + "</TextItem></Line>");
                else
                    sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FIX_STAKE_COMMISSION) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\"> </TextItem></Line>");

                if (checkpoint.station.flexCommission.HasValue)
                    sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FLEX_COMMISSION) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + String.Format("{0:0.00}", checkpoint.station.flexCommission.Value, CultureInfo.CurrentCulture) + " " + StationRepository.Currency + "</TextItem></Line>");
                else
                    sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FLEX_COMMISSION) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\"> </TextItem></Line>");

                sb.Append("<Line><FillItem>-</FillItem></Line>");
                sb.Append("<Line><FillItem> </FillItem></Line>");
            }

            if (checkpoint.station.shopOwnerShare.HasValue)
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_SHOP_OWNER_SHARE) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + String.Format("{0:0.00}", checkpoint.station.shopOwnerShare.Value, CultureInfo.CurrentCulture) + " " + StationRepository.Currency + "</TextItem></Line>");
            else
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_SHOP_OWNER_SHARE) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\"> </TextItem></Line>");

            if (!shopPaymentsReadLocationOwner)
            {
                if (checkpoint.station.franchisorShare.HasValue)
                    sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FRANCHISOR_SHARE) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + String.Format("{0:0.00}", checkpoint.station.franchisorShare.Value, CultureInfo.CurrentCulture) + " " + StationRepository.Currency + "</TextItem></Line>");
                else
                    sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FRANCHISOR_SHARE) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\"> </TextItem></Line>");
            }

            sb.Append("<Line><FillItem>-</FillItem></Line>");

            //cash transfer
            if (checkpoint.station.cashTransfer.HasValue)
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_CASH_TRANSFER) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + String.Format("{0:0.00}", checkpoint.station.cashTransfer.Value, CultureInfo.CurrentCulture) + " " + StationRepository.Currency + "</TextItem></Line>");
            else
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_CASH_TRANSFER) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\"> </TextItem></Line>");

            sb.Append("</Page>");

            Nbt.Services.Spf.XmlPreprocess xmlPre = new Nbt.Services.Spf.XmlPreprocess(sb.ToString(), false);

            bool found = PrintDefaultTicket(printer, xmlPre, false);
            if (!found)
            {
                WsdlRepository.StationEvent(StationRepository.StationNumber, StationEventType.PRINTER_ERROR);
                StationRepository.PrinterStatus = PrinterHandler.PRINTER_NOT_FOUND;
            }
            return found;
        }

        public bool PrintChechpointForLocation(ProfitAccountingCheckpoint checkpoint, bool shopPaymentsReadLocationOwner)
        {
            Nbt.Services.Spf.Printer.IPrinter printer = InitPrinter(true);
            if (printer == null || checkpoint == null)
            {
                return false;
            }

            StringBuilder sb = new StringBuilder();
            string TimeFormat = "dd.MM.yyyy HH:mm";
            DateTime endDate = checkpoint.general.endDate.Value;
            DateTime startDate = checkpoint.general.startDate.Value;

            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?><Page xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"SPFSchema.xsd\" Codepage=\"850\">");

            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"3\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_LOC_PROFIT_RECIEPT) + "</TextItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "</TextItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FRANCHISOR) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + " " + checkpoint.general.franchisor + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_LOCATION) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + " " + checkpoint.general.location + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_ACCOUNTING_PERIOD) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + startDate.ToString(TimeFormat) + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + endDate.ToString(TimeFormat) + "</TextItem></Line>");
            sb.Append("<Line><FillItem>-</FillItem></Line>");

            //cash part
            sb.Append("<Line><FillItem> </FillItem></Line>");
            if (checkpoint.location.totalCashIn.HasValue)
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TOTAL_CASH_IN) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + String.Format("{0:0.00}", checkpoint.location.totalCashIn.Value, CultureInfo.CurrentCulture) + " " + StationRepository.Currency + "</TextItem></Line>");
            else
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TOTAL_CASH_IN) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\"> </TextItem></Line>");

            if (checkpoint.location.totalCashOut.HasValue)
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TOTAL_CASH_OUT) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + String.Format("{0:0.00}", checkpoint.location.totalCashOut.Value, CultureInfo.CurrentCulture) + " " + StationRepository.Currency + "</TextItem></Line>");
            else
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TOTAL_CASH_OUT) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\"> </TextItem></Line>");

            if (checkpoint.location.tax.HasValue)
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TAX) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + String.Format("{0:0.000}", checkpoint.location.tax.Value, CultureInfo.CurrentCulture) + " " + StationRepository.Currency + "</TextItem></Line>");
            else
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_TAX) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\"> </TextItem></Line>");

            //add net cash and net hold

            sb.Append("<Line><FillItem>-</FillItem></Line>");

            //commisions
            sb.Append("<Line><FillItem> </FillItem></Line>");

            if (checkpoint.location.basis.HasValue)
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_BASIS_FOR_PROFIT_SHARING) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + String.Format("{0:0.00}", checkpoint.location.basis.Value, CultureInfo.CurrentCulture) + " " + StationRepository.Currency + "</TextItem></Line>");
            else
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_BASIS_FOR_PROFIT_SHARING) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\"> </TextItem></Line>");

            if (checkpoint.location.fixStakeCommission.HasValue)
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FIX_STAKE_COMMISSION) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + String.Format("{0:0.00}", checkpoint.location.fixStakeCommission.Value, CultureInfo.CurrentCulture) + " " + StationRepository.Currency + "</TextItem></Line>");
            else
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FIX_STAKE_COMMISSION) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\"> </TextItem></Line>");

            if (checkpoint.location.flexCommission.HasValue)
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FLEX_COMMISSION) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + String.Format("{0:0.00}", checkpoint.location.flexCommission.Value, CultureInfo.CurrentCulture) + " " + StationRepository.Currency + "</TextItem></Line>");
            else
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FLEX_COMMISSION) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\"> </TextItem></Line>");

            //share
            sb.Append("<Line><FillItem>-</FillItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");

            if (checkpoint.location.shopOwnerShare.HasValue)
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_SHOP_OWNER_SHARE) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + String.Format("{0:0.00}", checkpoint.location.shopOwnerShare.Value, CultureInfo.CurrentCulture) + " " + StationRepository.Currency + "</TextItem></Line>");
            else
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_SHOP_OWNER_SHARE) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\"> </TextItem></Line>");

            if (!shopPaymentsReadLocationOwner)
            {
                if (checkpoint.location.franchisorShare.HasValue)
                    sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FRANCHISOR_SHARE) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + String.Format("{0:0.00}", checkpoint.location.franchisorShare.Value, CultureInfo.CurrentCulture) + " " + StationRepository.Currency + "</TextItem></Line>");
                else
                    sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FRANCHISOR_SHARE) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\"> </TextItem></Line>");
            }

            sb.Append("<Line><FillItem>-</FillItem></Line>");

            //cash transfer
            if (checkpoint.location.cashTransfer.HasValue)
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_CASH_TRANSFER) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + String.Format("{0:0.00}", checkpoint.location.cashTransfer.Value, CultureInfo.CurrentCulture) + " " + StationRepository.Currency + "</TextItem></Line>");
            else
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_CASH_TRANSFER) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\"> </TextItem></Line>");

            sb.Append("</Page>");

            Nbt.Services.Spf.XmlPreprocess xmlPre = new Nbt.Services.Spf.XmlPreprocess(sb.ToString(), false);

            bool found = PrintDefaultTicket(printer, xmlPre, false);
            if (!found)
            {
                WsdlRepository.StationEvent(StationRepository.StationNumber, StationEventType.PRINTER_ERROR);
                StationRepository.PrinterStatus = PrinterHandler.PRINTER_NOT_FOUND;
            }
            return found;
        }

        public bool PrintOperatorSettlementResponce(ProduceOperatorSettlementResponse responce)
        {
            if (!StationRepository.UsePrinter)
            {
                return true;
            }

            Nbt.Services.Spf.Printer.IPrinter printer = InitPrinter(false);
            if (printer == null)
            {
                return false;
            }
            string TimeFormat = "dd.MM.yyyy HH:mm";

            StringBuilder sb = new StringBuilder();
            sb.Append(printHeader());

            //enter content
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"3\" Font=\"FontB\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_SETTLEMENT_RECIEPT) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "</TextItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");

            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_OPERATOR_NAME) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + responce.operatorFirstName + " " + responce.operatorLastName + "</TextItem></Line>");

            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FRANCHISOR) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + ": " + responce.franchisorName + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_LOCATION) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + ": " + responce.locationName + "</TextItem></Line>");

            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_SETTLEMENT_PERIOD) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + responce.settlementPeriodStartDate.ToString(TimeFormat) + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + responce.settlementPeriodEndDate.ToString(TimeFormat) + "</TextItem></Line>");

            sb.Append("<Line><FillItem>-</FillItem></Line>");

            //print checkpoints
            for (int i = 0; i < responce.checkpoints.Length; i++)
            {
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_CHECKPOINT) + " " + (i + 1) + "</TextItem>");
                sb.Append("<TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + responce.checkpoints[i].startDate.ToString(TimeFormat) + " / " + responce.checkpoints[i].endDate.ToString(TimeFormat) + "</TextItem></Line>");

                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"3\" Font=\"FontB\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_EMPTY_BOX) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Nr. :" + responce.checkpoints[i].emptyBoxTotalNumber.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + responce.checkpoints[i].emptyBoxTotalAmount.ToString() + " " + StationRepository.Currency + "</TextItem></Line>");

                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"3\" Font=\"FontB\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PAYED_PM_NOTES) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Nr. :" + responce.checkpoints[i].paymentTotalNumber.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + responce.checkpoints[i].paymentTotalAmount.ToString() + " " + StationRepository.Currency + "</TextItem></Line>");

                sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"3\" Font=\"FontB\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PAYED_CR_NOTES) + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    "Nr. :" + responce.checkpoints[i].creditTotalNumber.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                    TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + responce.checkpoints[i].creditTotalAmount.ToString() + " " + StationRepository.Currency + "</TextItem></Line>");

                sb.Append("<Line><FillItem>-</FillItem></Line>");
            }

            ////total
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"4\" Size=\"2\" Font=\"FontB\" Style=\"Bold\">" +
                TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINTER_TOTAL) + "</TextItem></Line>");

            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"3\" Font=\"FontB\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_EMPTY_BOX) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                "Nr. :" + responce.total.emptyBoxTotalNumber.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + responce.total.emptyBoxTotalAmount.ToString() + " " + StationRepository.Currency + "</TextItem></Line>");

            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"3\" Font=\"FontB\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PAYED_PM_NOTES) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                "Nr. :" + responce.total.paymentTotalNumber.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + responce.total.paymentTotalAmount.ToString() + " " + StationRepository.Currency + "</TextItem></Line>");

            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"3\" Font=\"FontB\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PAYED_CR_NOTES) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                "Nr. :" + responce.total.creditTotalNumber.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + responce.total.creditTotalAmount.ToString() + " " + StationRepository.Currency + "</TextItem></Line>");

            sb.Append("<Line><FillItem>=</FillItem></Line>");

            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"8\" Size=\"3\" Font=\"FontB\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_OPERATOR_BALANCE) + ": " + responce.total.balance.ToString() + " " + StationRepository.Currency + "</TextItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");

            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"8\" Size=\"3\" Font=\"FontB\" Style=\"None\">" + responce.LocationOwnerName + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"8\" Size=\"3\" Font=\"FontB\" Style=\"None\">_____________</TextItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"8\" Size=\"3\" Font=\"FontB\" Style=\"None\">" + responce.operatorFirstName + " " + responce.operatorLastName + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"8\" Size=\"3\" Font=\"FontB\" Style=\"None\">_____________</TextItem></Line>");
            sb.Append("</Page>");

            Nbt.Services.Spf.XmlPreprocess xmlPre = new Nbt.Services.Spf.XmlPreprocess(sb.ToString(), false);

            bool found = PrintDefaultTicket(printer, xmlPre, true);
            if (!found)
            {
                WsdlRepository.StationEvent(StationRepository.StationNumber, StationEventType.PRINTER_ERROR);
                StationRepository.PrinterStatus = PrinterHandler.PRINTER_NOT_FOUND;
            }
            return found;
        }

        public bool PrintPaymentRecept(string number, string code, decimal amount, bool isCreditNote)
        {
            if (!StationRepository.UsePrinter)
            {
                return true;
            }

            Nbt.Services.Spf.Printer.IPrinter printer = InitPrinter(false);
            if (printer == null)
            {
                return false;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(printHeader());
            if (isCreditNote)
            {
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_CREDITNOTE_RECEPT) + "</TextItem></Line>");
            }
            else
            {
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PAYMENTNOTE_RECEPT) + "</TextItem></Line>");
            }
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "</TextItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");

            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"1\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FRANCHISOR) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + " " + StationRepository.FranchiserName + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"1\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_LOCATION) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + " " + StationRepository.LocationName + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"1\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_STATION_NAME) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + " " + StationRepository.StationName + "</TextItem><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + "NR: " + StationRepository.StationNumber + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"1\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_OPERATOR_NAME) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + " " + ChangeTracker.CurrentUser.Username + "</TextItem></Line>");
            sb.Append("<Line><FillItem>=</FillItem></Line>");

            if (isCreditNote)
            {
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_CREDITNOTE_NR) + ": " + number + " " + code + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_CREDITNOTE_AMOUNT) + ": " + amount + " " + StationRepository.Currency + "</TextItem></Line>");
            }
            else
            {
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PAYMENTNOTE_NR) + ": " + number + " " + code + "</TextItem></Line>");
                sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PAYMENTNOTE_AMOUNT) + ": " + amount + " " + StationRepository.Currency + "</TextItem></Line>");
            }

            sb.Append("</Page>");

            Nbt.Services.Spf.XmlPreprocess xmlPre = new Nbt.Services.Spf.XmlPreprocess(sb.ToString(), false);

            bool found = PrintDefaultTicket(printer, xmlPre, true);
            if (!found)
            {
                WsdlRepository.StationEvent(StationRepository.StationNumber, StationEventType.PRINTER_ERROR);
                StationRepository.PrinterStatus = PrinterHandler.PRINTER_NOT_FOUND;
            }
            return found;


        }

        public bool PrintOperatorShiftReport(OperatorShiftCheckpoint checkpoint, decimal balance)
        {
            if (!StationRepository.UsePrinter)
            {
                return true;
            }

            Nbt.Services.Spf.Printer.IPrinter printer = InitPrinter(false);
            if (printer == null)
            {
                return false;
            }

            string TimeFormat = "dd.MM.yyyy HH:mm";

            StringBuilder sb = new StringBuilder();
            sb.Append(printHeader());

            //appent content
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_OPER_SHIFT_REPORT) + "</TextItem></Line>");

            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + DateTime.Now.ToString(TimeFormat) + "</TextItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");

            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"1\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FRANCHISOR) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + ": " + StationRepository.FranchiserName + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"1\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_LOCATION) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + ": " + StationRepository.LocationName + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"1\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PRINT_OPERATOR) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + ": " + checkpoint.operatorName + "</TextItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><FillItem>-</FillItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_SHIFT_DATES) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + checkpoint.startDate.ToString(TimeFormat) + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + checkpoint.endDate.ToString(TimeFormat) + "</TextItem></Line>");

            sb.Append("<Line><FillItem> </FillItem></Line>");
            //empty boxes/payments
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"3\" Font=\"FontB\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_EMPTY_BOX) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                "Nr. :" + checkpoint.emptyBoxTotalNumber.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + checkpoint.emptyBoxTotalAmount.ToString() + "</TextItem></Line>");

            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"3\" Font=\"FontB\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_PAYMENT) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                "Nr. :" + checkpoint.payoutTotalNumber.ToString() + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" +
                TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_AMOUNT) + ": " + checkpoint.payoutTotalAmount.ToString() + "</TextItem></Line>");

            sb.Append("<Line><FillItem>=</FillItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_SHIFT_BALANCE) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + (checkpoint.emptyBoxTotalAmount - checkpoint.payoutTotalAmount).ToString() + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_ACCOUNT_TRANSACTIONS) + ":</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + balance.ToString() + "</TextItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");

            sb.Append("</Page>");

            Nbt.Services.Spf.XmlPreprocess xmlPre = new Nbt.Services.Spf.XmlPreprocess(sb.ToString(), false);

            bool found = PrintDefaultTicket(printer, xmlPre, true);
            if (!found)
            {
                WsdlRepository.StationEvent(StationRepository.StationNumber, StationEventType.PRINTER_ERROR);
                StationRepository.PrinterStatus = PrinterHandler.PRINTER_NOT_FOUND;
            }
            return found;
        }

        public bool PrintRegistrationNote(string user, string registration_note_number)
        {

            if (!StationRepository.UsePrinter)
            {
                return true;
            }

            Nbt.Services.Spf.Printer.IPrinter printer = InitPrinter(false);
            if (printer == null)
            {
                return false;
            }
            String barcodeStr = BarCodeConverter.ConvertNumber2RegistrationNoteBarcode(registration_note_number);

            StringBuilder sb = new StringBuilder();
            sb.Append(printHeader());

            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_REGISTRATION_NOTE) + "</TextItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "</TextItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");

            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"1\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_LOCATION) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + " " + StationRepository.LocationName + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Center\" Characterspacing=\"2\" Size=\"1\" Style=\"Bold\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_STATION_NAME) + "</TextItem></Line>");
            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_NAME) + " " + StationRepository.StationName + "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" + "NR: " + StationRepository.StationNumber + "</TextItem></Line>");

            sb.Append("<Line><FillItem> </FillItem></Line>");
            sb.Append("<Line>");
            sb.Append("<BarCodeItem Alignment=\"Center\" HRIPos=\"None\" Height=\"100\"  Type=\"CODE93\">" + barcodeStr + "</BarCodeItem>");
            sb.Append("</Line>");

            sb.Append("<Line><TextItem  Alignment=\"Left\" Characterspacing=\"2\" Size=\"1\" Style=\"None\">" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_FORM_USERNAME) + ": " +/* "</TextItem><TextItem  Alignment=\"Right\" Characterspacing=\"2\" Size=\"2\" Style=\"None\">" +*/ user + "</TextItem></Line>");
            sb.Append("<Line><FillItem> </FillItem></Line>");
            //
            string congratulation = TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_REGISTRATION_CONGRATULATION).ToString();
            //congratulation = congratulation.Replace("*username*", user);
            congratulation = String.Format(congratulation, user);

            int len = 48;
            if (paper_width_2_inch)
            {
                len = 32;
            }
            sb.Append(SplitStringBySpaces(congratulation, len, 1));

            sb.Append("</Page>");



            Nbt.Services.Spf.XmlPreprocess xmlPre = new Nbt.Services.Spf.XmlPreprocess(sb.ToString(), false);

            bool found = PrintDefaultTicket(printer, xmlPre, false);
            if (!found)
            {
                WsdlRepository.StationEvent(StationRepository.StationNumber, StationEventType.PRINTER_ERROR);
                StationRepository.PrinterStatus = PrinterHandler.PRINTER_NOT_FOUND;
            }
            return found;
        }
        public bool PrintLastObject()
        {
            IPrinter printer;
            if (xmlPreDuplicate == null || (printer = InitPrinter(false)) == null)
                return false;

            xmlPreDuplicate.DuplicateText =
                "<Line Underline=\"true\"><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"2\" >"
                + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_DUPLICATE) + "</TextItem></Line>";

            bool found = PrintDefaultTicket(printer, xmlPreDuplicate, false);
            if (!found)
            {
                WsdlRepository.StationEvent(StationRepository.StationNumber, StationEventType.PRINTER_ERROR);
                StationRepository.PrinterStatus = PrinterHandler.PRINTER_NOT_FOUND;
            }
            return found;
        }

        public bool PrintLastObject(int amount)
        {
            IPrinter printer;
            var files = Directory.GetFiles(foldername, "*.xml");
            if (files.Length == 0 || (printer = InitPrinter(false)) == null)
                return false;

            foreach (var file in files)
            {
                StreamReader sr = new StreamReader(file);
                var DuplicateText = "<Line Underline=\"true\"><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"2\" >" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_DUPLICATE) + "</TextItem></Line>";
                var xmlPreprocess = new XmlPreprocess(sr.ReadToEnd(), false, DuplicateText);
                sr.Close();


                bool found = PrintDefaultTicket(printer, xmlPreprocess, false);
                if (!found)
                {
                    WsdlRepository.StationEvent(StationRepository.StationNumber, StationEventType.PRINTER_ERROR);
                    StationRepository.PrinterStatus = PrinterHandler.PRINTER_NOT_FOUND;
                    return false;

                }
                else
                {
                    File.Delete(file);
                }
            }

            if (RefreshNotPrintedCount != null)
                RefreshNotPrintedCount(null, null);

            return true;


        }

        public bool PrintLastTickets(int amount)
        {
            IPrinter printer;

            var files = new DirectoryInfo(printedTicketsfoldername).GetFiles().Where(x => x.Name.Contains("ticket"))
                                                  .OrderByDescending(f => f.LastWriteTime)
                                                  .ToList();

            if (files.Count == 0 || (printer = InitPrinter(false)) == null)
                return false;

            for (int i = 0; i < amount; i++)
            {
                var file = files[i];
                StreamReader sr = new StreamReader(file.Open(FileMode.Open));
                var DuplicateText = "<Line Underline=\"true\"><TextItem  Alignment=\"Center\" Characterspacing=\"8\" Size=\"2\" >" + TranslationProvider.TranslateForPrinter(MultistringTags.TERMINAL_DUPLICATE) + "</TextItem></Line>";
                var xmlPreprocess = new XmlPreprocess(sr.ReadToEnd(), false, DuplicateText);
                sr.Close();


                bool found = PrintDefaultTicket(printer, xmlPreprocess, false);
                if (!found)
                {
                    WsdlRepository.StationEvent(StationRepository.StationNumber, StationEventType.PRINTER_ERROR);
                    StationRepository.PrinterStatus = PrinterHandler.PRINTER_NOT_FOUND;
                    return false;
                }
            }

            if (RefreshNotPrintedCount != null)
                RefreshNotPrintedCount(null, null);

            return true;


        }

    }
}