using System.Configuration;
using BaseObjects.ViewModels;
using IocContainer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using SharedInterfaces;
using SportBetting.WPF.Prism.Shared;
using SportRadar.DAL.NewLineObjects;
using TranslationByMarkupExtension;

namespace MVVMTest.Services
{
    [TestClass]
    public class TranslationProviderTests
    {
        [TestMethod]
        public void CertificateErrorTest()
        {

            DBTranslationProvider translationProvider = new DBTranslationProvider();

            var translation = translationProvider.Translate(MultistringTags.CERTIFICATE_ERROR);

            Assert.AreEqual("Certificate error.",translation);

        }       
        [TestMethod]
        public void OKTest()
        {

            DBTranslationProvider translationProvider = new DBTranslationProvider();
            var translation = translationProvider.Translate(MultistringTags.TERMINAL_ADMIN_MENU_OK);

            Assert.AreEqual("OK",translation);

        } 
        
        [TestMethod]
        public void stringFormatInvalidTest()
        {

            DBTranslationProvider translationProvider = new DBTranslationProvider();
            var tgString = new TaggedStringLn();
            tgString.Tag = "TERMINAL_CASHOUT";
            tgString.Text = "TERMINAL_CASHOUT {0}{1}";
            tgString.Language = "EN";

            LineSr.Instance.AllObjects.TaggedStrings.MergeLineObject(tgString);

            var translation = translationProvider.Translate(MultistringTags.TERMINAL_CASHOUT,1);

            Assert.AreEqual("TERMINAL_CASHOUT 1{1}", translation);

        }        
        [TestMethod]
        public void stringFormatInvalid3Test()
        {

            DBTranslationProvider translationProvider = new DBTranslationProvider();
            var tgString = new TaggedStringLn();
            tgString.Tag = "TERMINAL_CASHOUT";
            tgString.Text = "TERMINAL_CASHOUT {2}{3}";
            tgString.Language = "EN";

            LineSr.Instance.AllObjects.TaggedStrings.MergeLineObject(tgString);

            var translation = translationProvider.Translate(MultistringTags.TERMINAL_CASHOUT,1);

            Assert.AreEqual("TERMINAL_CASHOUT {2}{3}", translation);

        }       
        [TestMethod]
        public void stringFormatInvalid2Test()
        {

            DBTranslationProvider translationProvider = new DBTranslationProvider();
            var tgString = new TaggedStringLn();
            tgString.Tag = "TERMINAL_CASHOUT";
            tgString.Text = "TERMINAL_CASHOUT {0}{1}";
            tgString.Language = "EN";

            LineSr.Instance.AllObjects.TaggedStrings.MergeLineObject(tgString);

            var translation = translationProvider.Translate(MultistringTags.TERMINAL_CASHOUT,1,2,3);

            Assert.AreEqual("TERMINAL_CASHOUT 12", translation);

        }        
        [TestMethod]
        public void stringFormatTest()
        {

            DBTranslationProvider translationProvider = new DBTranslationProvider();
            var tgString = new TaggedStringLn();
            tgString.Tag = "TERMINAL_CASHOUT";
            tgString.Language = "EN";
            tgString.Text = "TERMINAL_CASHOUT {0}";

            LineSr.Instance.AllObjects.TaggedStrings.MergeLineObject(tgString);

            var translation = translationProvider.Translate(MultistringTags.TERMINAL_CASHOUT,1);

            Assert.AreEqual("TERMINAL_CASHOUT 1", translation);

        }

        [TestMethod]
        public void DisabledDefaultLangOKTest()
        {

            DBTranslationProvider translationProvider = new DBTranslationProvider();
            ConfigurationManager.AppSettings["show_multistring_tags"] = "1";
            var translation = translationProvider.Translate(MultistringTags.TERMINAL_ADMIN_MENU_OK);

            Assert.AreEqual("!TERMINAL_ADMIN_MENU_OK!", translation);
           ConfigurationManager.AppSettings["show_multistring_tags"] = null;



        }


    }


}
