using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TranslationByMarkupExtension;
using ViewModels.ViewModels;
using WsdlRepository.WsdlServiceReference;

namespace MVVMTest.ViewModels
{
    [TestClass]
    public class CashHistoryViewModelTest : BaseTestClass
    {
        [TestMethod]
        [DeploymentItem("Files\\CashLogFile1.log")]
        public void CashHistoryOrderByDescending()
        {


            var historyFiles = new ObservableCollection<HistoryFile>
                {
                    new HistoryFile {CashValue = "5", DateValue = new DateTime(2014, 5, 20, 9, 27, 12)},
                    new HistoryFile {CashValue = "5", DateValue = new DateTime(2014, 5, 20, 9, 27, 11)},
                    new HistoryFile {CashValue = "5", DateValue = new DateTime(2014, 5, 19, 20, 4, 19)},
                    new HistoryFile {CashValue = "5", DateValue = new DateTime(2014, 5, 19, 12, 4, 13)},
                    new HistoryFile {CashValue = "5", DateValue = new DateTime(2014, 5, 19, 12, 3, 48)},
                    new HistoryFile {CashValue = 0.5m.ToString(CultureInfo.InvariantCulture), DateValue = new DateTime(2014, 5, 19, 0, 3, 55)}
                };
            WsdlRepository.Setup(x => x.GetStationCashHistory(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(new CashOperationtData[]
                {
                    new CashOperationtData() {amount = 0.5M,operation_type = "CASH_IN",created_at = new DateTime(2014, 5, 19, 0, 3, 55)},
                    new CashOperationtData() {amount = 5M,operation_type = "CASH_IN",created_at = new DateTime(2014, 5, 19, 12, 3, 48)},
                    new CashOperationtData() {amount = 5M,operation_type = "CASH_IN",created_at = new DateTime(2014, 5, 19, 12, 4, 13)},
                    new CashOperationtData() {amount = 5M,operation_type = "CASH_IN",created_at = new DateTime(2014, 5, 19, 20, 4, 19)},
                    new CashOperationtData() {amount = 5M,operation_type = "CASH_IN",created_at = new DateTime(2014, 5, 20, 9, 27, 11)},
                    new CashOperationtData() {amount = 5M,operation_type = "CASH_IN", created_at = new DateTime(2014, 5, 20, 9, 27, 12)},
                });

            TranslationProvider.Setup(x => x.Translate(MultistringTags.CASH_HISTORY_NOT_FOUND)).Returns("");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.WEEK)).Returns("");
            var vm = new CashHistoryViewModel();

            CollectionAssert.AreEqual(historyFiles, vm.HistoryFileCollection, new HistoryFileComparer());
        }
    }

    class HistoryFileComparer : IComparer
    {
        public int Compare(HistoryFile x, HistoryFile y)
        {
            if (x.CashDate.Equals(y.CashDate) && x.CashValue.Equals(y.CashValue))
                return 0;
            return 1;
        }

        public int Compare(object x, object y)
        {
            if (x is HistoryFile && y is HistoryFile)
            {
                return Compare(x as HistoryFile, y as HistoryFile);
            }
            return 1;
        }
    }
}
