using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Preferences.Services.Preference;
using Shared;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.OldCode;
using SportBetting.WPF.Prism.Shared.Models.Repositories;
using SportRadar.DAL.CommonObjects;
using WsdlRepository.WsdlServiceReference;

namespace MVVMTest.OldCode
{
    [TestClass]
    public class StationSettingsTest : BaseTestClass
    {

        [TestMethod]
        public void NoPreffileTest()
        {
            try
            {
                StationSettings StationSettings = new StationSettings();
                StationSettings.Init();
            }
            catch (FileNotFoundException)
            {

            }
        }
    }
}