using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SportBetting.WPF.Prism.Converters;
using SportBetting.WPF.Prism.OldCode;
using SportBetting.WPF.Prism.Shared;

namespace MVVMTest.OldCode
{
    [TestClass]
    public class ConverterTest : BaseTestClass
    {

        [TestMethod]
        public void TrueTest()
        {
            MultiBooleanANDToVisibilityConverter converter = new MultiBooleanANDToVisibilityConverter();
            var result = converter.Convert(new object[]{true, true, true}, null, null, null);
            Assert.AreEqual(result, Visibility.Visible);
        }
        [TestMethod]
        public void FalseTest()
        {
            MultiBooleanANDToVisibilityConverter converter = new MultiBooleanANDToVisibilityConverter();
            var result = converter.Convert(new object[]{false, false, false}, null, null, null);
            Assert.AreEqual(result, Visibility.Collapsed);
        }
        [TestMethod]
        public void HalfTrueTest()
        {
            MultiBooleanANDToVisibilityConverter converter = new MultiBooleanANDToVisibilityConverter();
            var result = converter.Convert(new object[]{true, false, true}, null, null, null);
            Assert.AreEqual(result, Visibility.Collapsed);
        }
        [TestMethod]
        public void Decilam0Test()
        {
            BooleanToVisibilityCollapsedConverter converter = new BooleanToVisibilityCollapsedConverter();
            var result = converter.Convert(0, typeof(Visibility), "decimal", null);
            Assert.AreEqual(result, Visibility.Collapsed);
        }
        [TestMethod]
        public void Decilam01Test()
        {
            BooleanToVisibilityCollapsedConverter converter = new BooleanToVisibilityCollapsedConverter();
            var result = converter.Convert(0.1m, typeof(Visibility), "decimal", null);
            Assert.AreEqual(result, Visibility.Visible);
        }

    }
}