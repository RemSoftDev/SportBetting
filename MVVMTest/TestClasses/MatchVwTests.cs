using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.OldLineObjects;
using SportRadar.DAL.ViewObjects;

namespace MVVMTest.TestClasses
{
    [TestClass]
    public class MatchVwTests
    {
        [TestMethod]
        public void MatchVwUpdateTest()
        {


            //var matchLn = TestMatchLn.CreateMatch(1, false, false);

            //DatabaseCache.EnsureDatabaseCache();
            ////DatabaseCache.Instance.AllObjects.Matches.Add(new KeyValuePair<long, IMatchLn>(1, matchLn));

            ////matchLn.BetDomains.Add(TestBetDomain.CreateBetDomain(1, true, 2, true));
            ////matchLn.BetDomains.Add(TestBetDomain.CreateBetDomain(2, false, 1, true));
            ////matchLn.BetDomains.Add(TestBetDomain.CreateBetDomain(3, false, 1, true));
            ////matchLn.BetDomains.Add(TestBetDomain.CreateBetDomain(4, false, 1, false));
            ////matchLn.BetDomains.Add(TestBetDomain.CreateBetDomain(5, false, 1, false));
            ////matchLn.BetDomains.Add(TestBetDomain.CreateBetDomain(6, false, 1, false));

            //MatchVw matchVw = new MatchVw(matchLn);
            //matchVw.RefreshProps();

            //Assert.AreEqual(3, matchVw.VisibleBetDomainCount);
            //Assert.AreEqual(6, matchVw.AllBetDomainCount);
            //Assert.AreEqual(2, matchVw.VisibleOddCount);
            //Assert.AreEqual(4, matchVw.AllVisibleOddCount);
            //Console.WriteLine(matchVw.ChangedPropNames);

            //Assert.IsTrue(matchVw.ChangedPropNames.Contains("VisibleOddCount"));
            //matchVw.RaisePropertiesChanged();
            //matchVw.UnsetChanged();
            //Assert.IsFalse(matchVw.ChangedPropNames.Contains("VisibleOddCount"));

            ////matchLn.BetDomains.Add(TestBetDomain.CreateBetDomain(7, false, 1, true));
            //matchVw.RefreshProps();

            //Assert.AreEqual(3, matchVw.VisibleOddCount);
            //Assert.AreEqual(7, matchVw.AllBetDomainCount);
            //Assert.AreEqual(4, matchVw.VisibleBetDomainCount);
            //Assert.AreEqual(5, matchVw.AllVisibleOddCount);

            //matchVw.RaisePropertiesChanged();

            //Console.WriteLine(matchVw.ChangedPropNames);
            //Assert.IsTrue(matchVw.ChangedPropNames.Contains("VisibleOddCount"));

            //matchVw.UnsetChanged();
            //Console.WriteLine(matchVw.ChangedPropNames);

            //matchVw.RefreshProps();
            //Console.WriteLine(matchVw.ChangedPropNames);
            //Assert.IsFalse(matchVw.ChangedPropNames.Contains("VisibleOddCount"));


        }
    }
}
