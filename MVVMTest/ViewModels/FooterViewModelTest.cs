using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Models.Interfaces;
using SportBetting.WPF.Prism.Shared.Models;
using SportBetting.WPF.Prism.Shared.Models.Repositories.Interfaces;
using SportRadar.Common.Collections;
using ViewModels.ViewModels;

namespace MVVMTest
{
    [TestClass]
    public class FooterViewModelTest : BaseTestClass
    {
       


        [TestMethod]
        public void DontShowProgressBar()
        {
            LanguageRepository.Setup(x => x.GetAllLanguages(new SyncObservableCollection<Language>()));
            var model = new FooterViewModel();

            model.UpdateProgress(1);

            Assert.IsFalse(model.ShowProgresBar);
            Assert.AreEqual(model.TotalUpdates, 0);
            Assert.AreEqual(model.ProcessedUpdates, 0);
            model.Close();
        }
        [TestMethod]
        public void ShowProgressBar()
        {
            LanguageRepository.Setup(x => x.GetAllLanguages(new SyncObservableCollection<Language>()));
            //LanguageRepository.Expect(x => x.GetDefaultLanguage()).Return(new Language("EN"));
            var model = new FooterViewModel();

            model.UpdateProgress(100);

            Assert.IsTrue(model.ShowProgresBar);
            Assert.AreEqual(model.TotalUpdates, 100);
            Assert.AreEqual(model.ProcessedUpdates, 0);
            model.Close();
        }

        [TestMethod]
        public void UpdateProgressBar()
        {
            LanguageRepository.Setup(x => x.GetAllLanguages(new SyncObservableCollection<Language>()));
            //LanguageRepository.Expect(x => x.GetDefaultLanguage()).Return(new Language("EN"));
            var model = new FooterViewModel();

            model.UpdateProgress(100);

            Assert.IsTrue(model.ShowProgresBar);
            Assert.AreEqual(model.TotalUpdates, 100);
            Assert.AreEqual(model.ProcessedUpdates, 0);

            model.UpdateProgress(90);

            Assert.IsTrue(model.ShowProgresBar);
            Assert.AreEqual(model.TotalUpdates, 100);
            Assert.AreEqual(model.ProcessedUpdates, 10);

            model.UpdateProgress(80);

            Assert.IsTrue(model.ShowProgresBar);
            Assert.AreEqual(model.TotalUpdates, 100);
            Assert.AreEqual(model.ProcessedUpdates, 20);

            model.Close();
        }

        [TestMethod]
        public void FinishUpdating()
        {
            LanguageRepository.Setup(x => x.GetAllLanguages(new SyncObservableCollection<Language>()));
            //LanguageRepository.Expect(x => x.GetDefaultLanguage()).Return(new Language("EN"));
            var model = new FooterViewModel();

            model.UpdateProgress(100);

            Assert.IsTrue(model.ShowProgresBar);
            Assert.AreEqual(model.TotalUpdates, 100);
            Assert.AreEqual(model.ProcessedUpdates, 0);

            model.UpdateProgress(90);

            Assert.IsTrue(model.ShowProgresBar);
            Assert.AreEqual(model.TotalUpdates, 100);
            Assert.AreEqual(model.ProcessedUpdates, 10);

            model.UpdateProgress(0);

            Assert.IsFalse(model.ShowProgresBar);
            Assert.AreEqual(model.TotalUpdates, 0);
            Assert.AreEqual(model.ProcessedUpdates, 0);

            model.Close();
        }

    }
}