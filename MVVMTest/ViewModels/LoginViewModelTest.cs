using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharedInterfaces;
using SportBetting.WPF.Prism.Shared;
using TranslationByMarkupExtension;
using ViewModels.ViewModels;

namespace MVVMTest.ViewModels
{
    [TestClass]
    public class LoginViewModelTest : BaseTestClass
    {

        [TestMethod]
        public void PinmaskedNotShowing()
        {
            MessageMediator.Setup(x => x.Register(It.IsAny<IActionDetails>())).Returns(true);
            MessageMediator.Setup(x => x.SendMessage<string>(MsgTag.ShowKeyboard, MsgTag.ShowKeyboard)).Returns(true);
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_FORM_REQUIRED)).Returns("required");
            var model = new LoginViewModel();
            model.OnNavigationCompleted();
            Assert.IsTrue(model.IsFocusedLogin);
            Assert.IsFalse(model.IsFocusedPassword);
            MessageMediator.Verify(x => x.Register(It.IsAny<IActionDetails>()), Times.Exactly(3)); ;
            MessageMediator.Verify(x => x.SendMessage<string>(MsgTag.ShowKeyboard, MsgTag.ShowKeyboard), Times.Once);

        }

    }
}