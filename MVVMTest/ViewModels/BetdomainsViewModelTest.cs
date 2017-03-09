using System;
using System.Threading;
using System.Threading.Tasks;
using BaseObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ViewModels.ViewModels;

namespace MVVMTest
{
    [TestClass]
    public class BetdomainsViewModelTest : BaseTestClass
    {



        [TestMethod]
        public void FilterOutrights()
        {
            MyRegionManager.Setup(x => x.NavigatBack(RegionNames.ContentRegion)).Returns(null);
            var match = TestMatchVw.CreateMatch(1, false, false) as TestMatchVw;
            match.ExpiryDate = DateTime.Now.AddMinutes(-5);
            ChangeTracker.Setup(x => x.CurrentMatch).Returns(match);
            var model = new BetDomainsViewModel();

            for (int i = 0; i < 5000; i++)
            {
                Thread.Sleep(1);
            }

            MyRegionManager.Verify(x => x.NavigatBack(RegionNames.ContentRegion),Times.Once);;



        }



    }
}