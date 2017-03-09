using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared;
using SportRadar.DAL.NewLineObjects;
using WsdlRepository.oldcode;

namespace MVVMTest.Services
{
    [TestClass]
    public class ConfidenceFactorTest2 : BaseTestClass
    {
        [TestMethod]
        public void NoMatchFactorTest()
        {

            MyLineSr.Setup(x => x.GetAllLiabilities("0*" + LineSr.TOURN_CONF_RATING)).Returns(new LiabilityLn() { factor =  1  });
            MyLineSr.Setup(x => x.GetAllLiabilities("1*" + LineSr.CONF_RATING_VALUES)).Returns(new LiabilityLn() { factor =  250  });
            MyLineSr.Setup(x => x.GetAllLiabilities("2*" + LineSr.MATCH_FACTOR)).Returns(new LiabilityLn() { factor =  1.5m  });
            ConfidenceFactor confidenceFactor = new ConfidenceFactor();
            var ticket = new Ticket();
            ticket.TipItems.Add(TestTipItem.CreateTipItem());

            ticket.TipItems.Add(TestTipItem.CreateTipItem());
            ticket.TipItems[0].Match.MatchId = 2;
            ticket.TotalOddDisplay = 7.5m;
            var maxbet = confidenceFactor.CalculateFactor(ticket);

            Assert.AreEqual(38.461538461538461538461538462m, maxbet);
        }

        [TestMethod]
        public void NoSportFactorTest()
        {

            MyLineSr.Setup(x => x.GetAllLiabilities("0*" + LineSr.TOURN_CONF_RATING)).Returns(new LiabilityLn() { factor =  1  });
            MyLineSr.Setup(x => x.GetAllLiabilities("1*" + LineSr.CONF_RATING_VALUES)).Returns(new LiabilityLn() { factor =  250  });
            MyLineSr.Setup(x => x.GetAllLiabilities("SPORT|1|DEFAULT*" + LineSr.LIMIT_FACTORS)).Returns(new LiabilityLn() { factor =  1.5m  });
            ConfidenceFactor confidenceFactor = new ConfidenceFactor();
            var ticket = new Ticket();
            ticket.TipItems.Add(TestTipItem.CreateTipItem());

            ticket.TipItems.Add(TestTipItem.CreateTipItem());
            ticket.TipItems[0].Match.MatchId = 2;
            ticket.TipItems[0].Match.MatchView.SportView.LineObject.SvrGroupId = 1;
            ticket.TotalOddDisplay = 7.5m;
            var maxbet = confidenceFactor.CalculateFactor(ticket);

            Assert.AreEqual(38.461538461538461538461538462m, maxbet);
        }

        [TestMethod]
        public void AssignedatchFactorTest()
        {
            MyLineSr.Setup(x => x.GetAllLiabilities("0*" + LineSr.TOURN_CONF_RATING)).Returns(new LiabilityLn() { factor = 1  });
            MyLineSr.Setup(x => x.GetAllLiabilities("1*" + LineSr.CONF_RATING_VALUES)).Returns(new LiabilityLn() { factor = 250  });
            MyLineSr.Setup(x => x.GetAllLiabilities("2*" + LineSr.MATCH_FACTOR)).Returns(new LiabilityLn() { factor =  1.5m  });
            ConfidenceFactor confidenceFactor = new ConfidenceFactor();
            var ticket = new Ticket();
            ticket.TipItems.Add(TestTipItem.CreateTipItem());

            ticket.TipItems.Add(TestTipItem.CreateTipItem());
            ticket.TipItems[0].Match.MatchId = 2;
            ticket.TipItems[1].Match.MatchId = 2;
            ticket.TotalOddDisplay = 7.5m;
            var maxbet = confidenceFactor.CalculateFactor(ticket);

            Assert.AreEqual(57.692307692307692307692307692m, maxbet);
        }

        [TestMethod]
        public void AssignedatchFactorLessThanOneTest()
        {
            MyLineSr.Setup(x => x.GetAllLiabilities("0*" + LineSr.TOURN_CONF_RATING)).Returns(new LiabilityLn() { factor =  1  });
            MyLineSr.Setup(x => x.GetAllLiabilities("1*" + LineSr.CONF_RATING_VALUES)).Returns(new LiabilityLn() { factor =  250  });
            MyLineSr.Setup(x => x.GetAllLiabilities("2*" + LineSr.MATCH_FACTOR)).Returns(new LiabilityLn() { factor = 0.5m  });
            ConfidenceFactor confidenceFactor = new ConfidenceFactor();
            var ticket = new Ticket();
            ticket.TipItems.Add(TestTipItem.CreateTipItem());

            ticket.TipItems.Add(TestTipItem.CreateTipItem());
            ticket.TipItems.Add(TestTipItem.CreateTipItem());
            ticket.TipItems[0].Match.MatchId = 2;
            ticket.TipItems[1].Match.MatchId = 2;
            ticket.TipItems[1].Match.MatchId = 3;
            ticket.TotalOddDisplay = 7.5m;
            var maxbet = confidenceFactor.CalculateFactor(ticket);

            Assert.AreEqual(19.230769230769230769230769231M, maxbet);
        }
    }
}
