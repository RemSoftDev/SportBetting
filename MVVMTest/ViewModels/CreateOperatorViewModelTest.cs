
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SportBetting.WPF.Prism.Models;
using TranslationByMarkupExtension;
using SportRadar.Common.Collections;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using System;
using ViewModels.ViewModels;
using WsdlRepository.WsdlServiceReference;

namespace MVVMTest.ViewModels
{
    [TestClass]
    public class CreateOperatorViewModelTest : BaseTestClass
    {
       
        [TestMethod]
        public void RegisterDoneTest()
        {
            var model = new CreateOperatorViewModel();
            model.OnNavigationCompleted();
            ChangeTracker.Object.CurrentUser = new AnonymousUser("111",111);
            model.FirstName.Value = "name";
            model.LastName.Value = "name";
            model.Username.Value = "name";
            model.Password.Value = "name";
            model.ConfirmPassword.Value = "name";
            model.OperatorType.Value = "1";
            WsdlRepository.Setup(x => x.RegisterOperator(It.IsAny<uid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>())).Returns(1);
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_FORM_DONE)).Returns("done");
            model.RegisterCommand.Execute(null);
            Assert.IsFalse(model.IsEnabledRegister);
            Assert.IsTrue(model.IsEnabledBindCard);
            TranslationProvider.Verify(x => x.Translate(MultistringTags.TERMINAL_FORM_DONE));
            WsdlRepository.Verify(x => x.RegisterOperator(It.IsAny<uid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()));

        }

         [TestMethod]
        public void EmptyResponceTest()
        {
            var model = new CreateOperatorViewModel();
             model.OnNavigationCompleted();
            ChangeTracker.Object.CurrentUser = new AnonymousUser("111", 111);
            model.FirstName.Value = "name";
            model.LastName.Value = "name";
            model.Username.Value = "name";
            model.Password.Value = "name";
            model.ConfirmPassword.Value = "name";
            model.OperatorType.Value = "1";
            WsdlRepository.Setup(x => x.RegisterOperator(It.IsAny<uid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>())).Returns(0);
            //TranslationProvider.Expect(x => x.Translate(MultistringTags.TERMINAL_FORM_DONE)).Return("done");
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_FORM_DONE)).Returns("done");
            model.RegisterCommand.Execute(null);
            Assert.IsTrue(model.IsEnabledRegister);
            Assert.IsFalse(model.IsEnabledBindCard);
            TranslationProvider.Verify(x => x.Translate(MultistringTags.TERMINAL_FORM_DONE),Times.Never);



        }

       
    }
}