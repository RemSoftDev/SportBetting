using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.OldLineObjects;
using SportRadar.DAL.SportRadarOldLineProvider;

namespace MVVMTest.OldCode
{
    [TestClass]
    public class ParseXmlTest
    {

        [TestMethod]
        [DeploymentItem("XMLFile5.xml")]
        public void ParseMatchCoComeptitorXml()
        {
            string sXmlString = "";

            using (var reader = new StreamReader("XMLFile5.xml"))
            {
                sXmlString = reader.ReadToEnd();
            }
            var value = LineSerializeHelper.StringToObject<SportRadarLineContainer>(sXmlString);
            Assert.IsNotNull(value);
            Assert.IsNotNull(value.MatchToCompetitor);


        }        
        
        [TestMethod]
        [DeploymentItem("dc.xml")]
        [DeploymentItem("bet_domain_map.config")]
        public void ParsTournamentMinusMultistringXml()
        {
            ConfigurationManager.AppSettings["CreateDatabase"] = "0";

            string sXmlString = "";
            DatabaseCache.EnsureDatabaseCache();
            BetDomainMap.EnsureInstance();
            using (var reader = new StreamReader("dc.xml"))
            {
                sXmlString = reader.ReadToEnd();
            }
            var srlc = LineSerializeHelper.StringToObject<SportRadarLineContainer>(sXmlString);
            UpdateStatistics us = new UpdateStatistics();

            var fsr = LineSr.SyncRoutines(eUpdateType.PreMatches, string.Format("SportRadar Pre-Match Update. DataSyncCacheId = {0}", 0), false, us, delegate(object objParam)
            {
                return ProviderHelper.MergeFromSportRadarLineContainer(srlc, 0);
            });

            Assert.AreEqual(fsr,eFileSyncResult.Succeeded);
            var instance = LineSr.Instance.AllObjects.Groups.Where(x=>x.Value.SvrGroupId == 5263).First();
            Assert.AreEqual(8,instance.Value.Strings.Count);
            Assert.AreEqual("Pro A",instance.Value.GetDisplayName("EN"));



        }
        [TestMethod]
        [DeploymentItem("groupsLanguages.xml")]
        [DeploymentItem("content.xml")]
        [DeploymentItem("bet_domain_map.config")]
        public void ParseOutright()
        {
            ConfigurationManager.AppSettings["CreateDatabase"] = "0";

            string sXmlString = "";
            DatabaseCache.EnsureDatabaseCache();
            BetDomainMap.EnsureInstance();
            using (var reader = new StreamReader("groupsLanguages.xml"))
            {
                sXmlString = reader.ReadToEnd();
            }
            var srlc = LineSerializeHelper.StringToObject<SportRadarLineContainer>(sXmlString);
            UpdateStatistics us = new UpdateStatistics();

            var fsr = LineSr.SyncRoutines(eUpdateType.PreMatches, string.Format("SportRadar Pre-Match Update. DataSyncCacheId = {0}", 0), false, us, delegate(object objParam)
            {
                return ProviderHelper.MergeFromSportRadarLineContainer(srlc, 0);
            });
            Assert.AreEqual(fsr, eFileSyncResult.Succeeded);

            //matches
            using (var reader = new StreamReader("content.xml"))
            {
                sXmlString = reader.ReadToEnd();
            }
            srlc = LineSerializeHelper.StringToObject<SportRadarLineContainer>(sXmlString);
            us = new UpdateStatistics();

            fsr = LineSr.SyncRoutines(eUpdateType.PreMatches, string.Format("SportRadar Pre-Match Update. DataSyncCacheId = {0}", 0), false, us, delegate(object objParam)
            {
                return ProviderHelper.MergeFromSportRadarLineContainer(srlc, 0);
            });

            Assert.AreEqual(fsr,eFileSyncResult.Succeeded);
            var instance = LineSr.Instance.AllObjects.Matches.Where(x => x.Value.MatchId == 1004106).First();
            Assert.AreEqual("RHL - Podium1", instance.Value.GetOutrightDisplayName("EN"));
            Assert.AreEqual("RHL - Podium1", instance.Value.MatchView.Name);



        }
        [TestMethod]
        [DeploymentItem("XMLFile1.xml")]
        public void Divide2Xml()
        {
            string sXmlString = "";

            using (var reader = new StreamReader("XMLFile1.xml"))
            {
                sXmlString = reader.ReadToEnd();
            }

            var start = sXmlString.IndexOf("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            var array = new List<string>();
            bool exitFlag = false;
            while (!exitFlag)
            {
                var end = sXmlString.IndexOf("<?xml version=\"1.0\" encoding=\"utf-8\"?>", start + 1);
                if (end < 0)
                {
                    end = sXmlString.Length;
                    exitFlag = true;
                }
                array.Add(sXmlString.Substring(start, end-start));

                start = end;
            }
            Assert.AreEqual(2,array.Count);


        }
        [TestMethod]
        [DeploymentItem("XMLFile3.xml")]
        public void Divide1Xml()
        {
            string sXmlString = "";

            using (var reader = new StreamReader("XMLFile3.xml"))
            {
                sXmlString = reader.ReadToEnd();
            }

            var start = sXmlString.IndexOf("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            var array = new List<string>();
            bool exitFlag = false;
            while (!exitFlag)
            {
                var end = sXmlString.IndexOf("<?xml version=\"1.0\" encoding=\"utf-8\"?>", start + 1);
                if (end < 0)
                {
                    end = sXmlString.Length;
                    exitFlag = true;
                }
                array.Add(sXmlString.Substring(start, end-start));

                start = end;
            }
            Assert.AreEqual(1,array.Count);


        }
        [TestMethod]
        [DeploymentItem("XMLFile6.xml")]
        public void ParseMatchCoComeptitorFailXml()
        {
            string sXmlString = "";

            using (var reader = new StreamReader("XMLFile6.xml"))
            {
                sXmlString = reader.ReadToEnd();
            }
            var value = LineSerializeHelper.StringToObject<SportRadarLineContainer>(sXmlString);
            Assert.IsNotNull(value);
            Assert.IsNotNull(value.MatchToCompetitor);
            Assert.IsNotNull(value.MultiString);
            Assert.IsNotNull(value.LanguageString);

            foreach (var competitorSr in value.Competitor)
            {
                var multistring = value.MultiString.Where(x => x.MultiStringID == competitorSr.MultiStringID).FirstOrDefault();
                Assert.IsNotNull(multistring);
            }
            foreach (var competitorSr in value.Competitor)
            {
                var multistring = value.MatchToCompetitor.Where(x => x.CompetitorID == competitorSr.CompetitorID).FirstOrDefault();
                Assert.IsNotNull(multistring);
            }


        }

        [TestMethod]
        public void ParseMatchesXml()
        {
            //string with one match inside
            const string sXmlString = @"<SportRadarLineContainer>
<MatchInfos><MatchInfo>
<m1>3622544</m1>
<m2>
<f n=""HEAD_TO_HEAD_LAST_MATCHES""/>
<f n=""COMPETITOR_1_TOURNAMENT_LAST_MATCHES_PERFORMANCE_PERCENT"">26</f>
<f n=""COMPETITOR_2_BTR_SUPER_ID"">6023</f>
<f n=""COMPETITOR_1_BTR_SUPER_ID"">6026</f>
<f n=""COMPETITOR_1_TOURNAMENT_LAST_MATCHES_AVERAGE_GOALS_AGAINST"">2.0</f>
<f n=""COMPETITOR_1_TOURNAMENT_LAST_MATCHES_COUNT"">5</f>
<f n=""COMPETITOR_2_TOURNAMENT_LAST_MATCHES_COUNT"">0</f>
<f n=""COMPETITOR_1_TOURNAMENT_LAST_MATCHES_AVERAGE_GOALS_FOR"">0.6</f>
<f n=""COMPETITOR_1_TOURNAMENT_LAST_MATCHES_GOALS_AGAINST"">10</f>
<f n=""COMPETITOR_1_TOURNAMENT_LAST_MATCHES_GOALS_FOR"">3</f>
<f n=""HEAD_TO_HEAD_LAST_5_MATCHES""/>
</m2>
<m3>2013-06-04T17:27:44</m3>
</MatchInfo>
</MatchInfos>
</SportRadarLineContainer>
";
            var value = LineSerializeHelper.StringToObject<SportRadarLineContainer>(sXmlString);
            Assert.IsNotNull(value);
            Assert.IsNotNull(value.MatchInfos);
            Assert.AreNotEqual(0, value.MatchInfos.Length);
            Assert.AreEqual(1, value.MatchInfos.Length);

            Assert.AreEqual("26", value.MatchInfos[0].StatisticValues[1].Value);
            Assert.AreEqual("COMPETITOR_1_TOURNAMENT_LAST_MATCHES_PERFORMANCE_PERCENT", value.MatchInfos[0].StatisticValues[1].Name);

        }

        [TestMethod]
        public void ParseActiveTournamentsXML()
        {
            const string sXmlString = @"<SportRadarLineContainer>
	<ActiveTournaments>
		<T>
			<F1>489</F1><F2>0.01</F2><F3>0,1,1050,5,230,1040,229,12,1030</F3>
		</T><T>
			<F1>488</F1><F2>0.02</F2><F3>0,2,1050,5,230,1040,229,12,1030</F3>
		</T><T>
			<F1>491</F1><F2>0.03</F2><F3>0,3,1050,5,230,1040,229,12,1030</F3>
		</T><T>
			<F1>490</F1><F2>0.04</F2><F3>0,4,1050,5,230,1040,229,12,1030</F3>
		</T><T>
			<F1>8262</F1><F2>0.05</F2><F3>0,5,1050,5,230,1040,229,12,1030</F3>
		</T><T>
			<F1>8263</F1><F2>0.06</F2><F3>0,6,1050,5,230,1040,229,12,1030</F3>
		</T><T>
			<F1>8260</F1><F2>0.07</F2><F3>0,7,1050,5,230,1040,229,12,1030</F3>
		</T><T>
			<F1>8261</F1><F2>0.08</F2><F3>0,8,1050,5,230,1040,229,12,1030</F3>
		</T>
	</ActiveTournaments>
</SportRadarLineContainer>
";
            var value = LineSerializeHelper.StringToObject<SportRadarLineContainer>(sXmlString);
            Assert.IsNotNull(value);
            Assert.IsNotNull(value.ActiveTournaments);
            Assert.AreEqual(8, value.ActiveTournaments.Length);
            Assert.AreEqual("489", value.ActiveTournaments[0].Id);

            Assert.AreEqual("0.02", value.ActiveTournaments[1].OddIncreaseDecrease);
            Assert.AreEqual("3", value.ActiveTournaments[2].VisibleMarkets.ToString().Split(',').ElementAt(1));
        }

        [TestMethod]
        public void ParseBetdomaintypeXml()
        {
            //string with one match inside
            const string sXmlString = @"<SportRadarLineContainer>
  <BetDomainTypeLnList>   
    <BetDomainTypeLn>
      <tag>HTPFT</tag>
      <mappingcode></mappingcode>
      <name></name>
      <bettypetag>HTP</bettypetag>
      <scoretypetag>SCR</scoretypetag>
      <timetypetag>FT</timetypetag>
      <sort>50</sort>
      <active>true</active>
      <externalstate>
        &lt;state xmlns=&quot;sr&quot;&gt;
        &lt;ExternalSort&gt;
        &lt;f n=&quot;SPRT_HANDBALL&quot;&gt;1&lt;/f&gt;&lt;f n=&quot;SPRT_SOCCER&quot;&gt;2&lt;/f&gt;&lt;f n=&quot;SPRT_PESAPALLO&quot;&gt;1&lt;/f&gt;&lt;f n=&quot;SPRT_ICE_HOCKEY&quot;&gt;1&lt;/f&gt;&lt;f n=&quot;SPRT_RUGBY&quot;&gt;1&lt;/f&gt;&lt;f n=&quot;SPRT_FUTSAL&quot;&gt;1&lt;/f&gt;&lt;f n=&quot;SPRT_BANDY&quot;&gt;1&lt;/f&gt;&lt;f n=&quot;SPRT_FLOORBALL&quot;&gt;1&lt;/f&gt;
        &lt;/ExternalSort&gt;
        &lt;/state&gt;
      </externalstate>
    </BetDomainTypeLn>
    <BetDomainTypeLn>
      <tag>SCR1FT</tag>
      <mappingcode></mappingcode>
      <name></name>
      <bettypetag>EXA</bettypetag>
      <scoretypetag>SCR1</scoretypetag>
      <timetypetag>FT</timetypetag>
      <sort>25</sort>
      <active>true</active>
      <externalstate>
        &lt;state xmlns=&quot;sr&quot;&gt;
        &lt;ExternalSort&gt;
        &lt;f n=&quot;SPRT_HANDBALL&quot;&gt;1&lt;/f&gt;&lt;f n=&quot;SPRT_SOCCER&quot;&gt;0&lt;/f&gt;&lt;f n=&quot;SPRT_PESAPALLO&quot;&gt;1&lt;/f&gt;&lt;f n=&quot;SPRT_ICE_HOCKEY&quot;&gt;1&lt;/f&gt;&lt;f n=&quot;SPRT_RUGBY&quot;&gt;1&lt;/f&gt;&lt;f n=&quot;SPRT_FUTSAL&quot;&gt;1&lt;/f&gt;&lt;f n=&quot;SPRT_BANDY&quot;&gt;1&lt;/f&gt;&lt;f n=&quot;SPRT_FLOORBALL&quot;&gt;1&lt;/f&gt;
        &lt;/ExternalSort&gt;
        &lt;/state&gt;
      </externalstate>
    </BetDomainTypeLn>
   
  </BetDomainTypeLnList>
</SportRadarLineContainer>
";



            SportRadarLineContainer srlc = SportRadarLineContainer.FromXmlString(sXmlString);


            Assert.AreEqual(2, srlc.BetDomainTypeLnList.Length);
            Assert.AreEqual("HTPFT", srlc.BetDomainTypeLnList[0].Tag);
            Assert.AreEqual("FT", srlc.BetDomainTypeLnList[0].TimeTypeTag);
            Assert.AreEqual("SCR", srlc.BetDomainTypeLnList[0].ScoreTypeTag);
            Assert.AreEqual("HTP", srlc.BetDomainTypeLnList[0].BetTypeTag);



            var betDomainTypeLn = new BetDomainTypeLn();
            betDomainTypeLn.EnsureExternalObjects();


            betDomainTypeLn.ExternalState = srlc.BetDomainTypeLnList[0].ExternalState;
            betDomainTypeLn.EnsureExternalObjects();

            Assert.AreEqual(2, betDomainTypeLn.GetExternalSort("SPRT_SOCCER"));


        }

        //noy yet working
        [TestMethod]
        public void ParseTournamentXml()
        {
            const string sXmlString = @"<SportRadarLineContainer>
<TournamentInfos>
<TournamentInfo>
<m1>10760</m1>
<m2>
<CompetitorInfos>
<CompetitorInfo>
<m1>3038</m1>
<m2>
<f n=""TOURNAMENT_SORT_POSITION\"">1</f>
<f n=""TOURNAMENT_GOALS_FOR"">4</f>
<f n=""TOURNAMENT_GOALS_AGAINST"">2</f>
<f n=""TOURNAMENT_POSITION_CHANGE"">0</f>
<f n=""BTR_SUPER_ID"">3038</f>
<f n=""TOURNAMENT_MATCHES_LOST"">1</f>
<f n=""TOURNAMENT_MATCHES_WON"">1</f>
<f n=""TOURNAMENT_MATCHES_DRAW"">0</f>
<f n=""TOURNAMENT_POINTS"">3</f>
<f n=""TOURNAMENT_POSITION"">1</f>
<f n=""TOURNAMENT_MATCHES_PLAYED"">2</f>
</m2>
<m5>2013-06-12T17:05:31</m5>
</CompetitorInfo>
</CompetitorInfos>
</m2>
<m3>2013-06-12T17:05:31</m3>
</TournamentInfo>
</TournamentInfos>
</SportRadarLineContainer>";
            var value = LineSerializeHelper.StringToObject<SportRadarLineContainer>(sXmlString);
            Assert.IsNotNull(value);
            Assert.IsNotNull(value.TournamentInfos);
            Assert.AreEqual(1, value.TournamentInfos.Length);
            Assert.AreEqual(10760, value.TournamentInfos[0].TournamentInfoId);
            Assert.AreEqual("2013-06-12T17:05:31", value.TournamentInfos[0].LastModifiedString);

            Assert.IsNotNull(value.TournamentInfos[0].CompetitorInfoCollections.CompetitorInfos);

            Assert.AreEqual(1, value.TournamentInfos[0].CompetitorInfoCollections.CompetitorInfos.Length);
            Assert.IsNotNull(value.TournamentInfos[0].CompetitorInfoCollections.CompetitorInfos[0].StatisticValues);
            Assert.AreEqual(11, value.TournamentInfos[0].CompetitorInfoCollections.CompetitorInfos[0].StatisticValues.Count);
            Assert.AreEqual(3038, value.TournamentInfos[0].CompetitorInfoCollections.CompetitorInfos[0].CompetitorInfoId);
            Assert.AreEqual("2013-06-12T17:05:31", value.TournamentInfos[0].CompetitorInfoCollections.CompetitorInfos[0].LastModifiedString);
            Assert.AreEqual("TOURNAMENT_GOALS_FOR", value.TournamentInfos[0].CompetitorInfoCollections.CompetitorInfos[0].StatisticValues[1].Name);
            Assert.AreEqual("4", value.TournamentInfos[0].CompetitorInfoCollections.CompetitorInfos[0].StatisticValues[1].Value);
        }

        [TestMethod]
        public void ParseCompetitorsXml()
        {
            const string sXmlString = @"<SportRadarLineContainer>
<CompetitorInfos>
<CompetitorInfo>
<m1>1675</m1>
<m2>
<f n=""SHIRT_2_COLOR"">CC0000</f>
<f n=""BTR_SUPER_ID"">1675</f>
<f n=""SHIRT_1_COLOR"">FFFFFF</f>
</m2>
<m3>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</m3>
<m4>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</m4>
<m5>2013-06-13T10:03:52</m5>
</CompetitorInfo>
</CompetitorInfos>
</SportRadarLineContainer>";

            var value = LineSerializeHelper.StringToObject<SportRadarLineContainer>(sXmlString);

            Assert.IsNotNull(value);
            Assert.IsNotNull(value.CompetitorInfos);
            Assert.AreEqual(1, value.CompetitorInfos.Length);
            Assert.AreEqual(1675, value.CompetitorInfos[0].CompetitorInfoId);
            Assert.AreEqual("CC0000", value.CompetitorInfos[0].StatisticValues[0].Value);
            Assert.AreEqual("SHIRT_2_COLOR", value.CompetitorInfos[0].StatisticValues[0].Name);
        }

        [TestMethod]
        public void ParseValues()
        {
            var sXmlString = @"<CompetitorInfo><m2><f n=""COMPETITOR_1_TOURNAMENT_POSITION="" >1</f></m2></CompetitorInfo>";
            var value = LineSerializeHelper.StringToObject<CompetitorInfoSr>(sXmlString);
            Assert.IsNotNull(value);
            Assert.AreEqual(value.StatisticValues.Count, 1);
        }



    }
}