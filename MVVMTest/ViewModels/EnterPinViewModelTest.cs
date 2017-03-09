using System.Collections.Generic;
using BaseObjects.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TranslationByMarkupExtension;

namespace MVVMTest
{
    [TestClass]
    public class EnterPinViewModelTest : BaseTestClass
    {

        [TestMethod]
        public void PinmaskedNotShowing()
        {

            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_PIN_4_SYMBOLS)).Returns("{0}");

            var model = new EnterPinViewModel();
            var firedEvents = new List<string>();

            model.PropertyChanged += ((sender, e) => firedEvents.Add(e.PropertyName));


            model.Pin.ValueMasked += "1";
            model.Pin.ValueMasked += "1";

            Assert.AreEqual(model.Pin.ValueMasked, "**");
        }

    }
}