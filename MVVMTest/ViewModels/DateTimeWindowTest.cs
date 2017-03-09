using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseObjects;
using BaseObjects.ViewModels;
using IocContainer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ninject;
using SportBetting.WPF.Prism.Models;
using TranslationByMarkupExtension;
using ViewModels;

namespace MVVMTest.ViewModels
{

    [TestClass]
    public class DateTimeWindowTest: BaseTestClass
    {

        [TestMethod]
        public void SelectDateTest()
        {
            IoCContainer.Kernel.Unbind<ITranslationProvider>();
            IoCContainer.Kernel.Bind<ITranslationProvider>().To<DBTranslationProvider>().InSingletonScope();
            IoCContainer.Kernel.Get<ITranslationProvider>().CurrentLanguage = "EN";
            var model = new DateTimeViewModel();
            model.YearSelectedCommand.Execute(new Year() { Id = 2001 });
            model.MonthSelectedCommand.Execute(new Month() { Id = 5 });
            model.DaySelectedCommand.Execute(new Day() { Id = 3 });
            Assert.AreEqual(model.Date.Value.Day, 3);
            Assert.AreEqual(model.Date.Value.Month, 5);
            Assert.AreEqual(model.Date.Value.Year, 2001);
            //model.MouseDownCommand.Execute();
            //Assert.IsTrue(!model._timer.IsEnabled);
        }

        [TestMethod]
        public void SelectDateTest2()
        {
            Registration modelmodel = new Registration();
            //modelmodel.SelectDateCommand.Execute();
            SelectDate.Setup(x => x.SelectDate(It.IsAny<DateTime?>(),null,null)).Returns(new DateTime(1, 1, 1));
            modelmodel.SelectDateCommand.Execute("");
            Assert.AreEqual("01.01.0001", modelmodel.Value);

    }
}
}
