using System;
using System.Collections.Generic;
using System.Text;

namespace SportRadar.DAL.OldLineObjects
{
	public static class OddTypes
    {

		#region BaseBetDomain,ThreeWayBaseBetDomain,SixWayBaseBetDomain
		public const string BD_TAG_0_1 = "BD_TAG_0_1";
		public const string BD_TAG_0_X = "BD_TAG_0_X";
		public const string BD_TAG_0_2 = "BD_TAG_0_2";
		#endregion

		#region HandicapBetDomain
		public const string BD_TAG_1_1 = "BD_TAG_1_1";
		public const string BD_TAG_1_X = "BD_TAG_1_X";
		public const string BD_TAG_1_2 = "BD_TAG_1_2";
		#endregion

		#region UnderOverBetDomain
		public const string BD_TAG_2_0UPTO2 = "BD_TAG_2_0-2";
		public const string BD_TAG_2_3ORMORE = "BD_TAG_2_3+";
		#endregion

		#region HalfTimeFullTimeWinnerBetDomain
		public const string BD_TAG_3_1TO1 = "BD_TAG_3_1+1";
		public const string BD_TAG_3_XTO1 = "BD_TAG_3_X+1";
		public const string BD_TAG_3_2TO1 = "BD_TAG_3_2+1";
		public const string BD_TAG_3_1TOX = "BD_TAG_3_1+X";
		public const string BD_TAG_3_XTOX = "BD_TAG_3_X+X";
		public const string BD_TAG_3_2TOX = "BD_TAG_3_2+X";
		public const string BD_TAG_3_1TO2 = "BD_TAG_3_1+2";
		public const string BD_TAG_3_XTO2 = "BD_TAG_3_X+2";
		public const string BD_TAG_3_2TO2 = "BD_TAG_3_2+2";
		#endregion

		#region HighScoreInHalfBetDomain
		public const string BD_TAG_4_1HZ = "BD_TAG_4_1HZ";
		public const string BD_TAG_4_X = "BD_TAG_4_X";
		public const string BD_TAG_4_2HZ = "BD_TAG_4_2HZ";
		#endregion

		#region ScoresFirstBetDomain
		public const string BD_TAG_5_1 = "BD_TAG_5_1";
		public const string BD_TAG_5_X = "BD_TAG_5_X";
		public const string BD_TAG_5_2 = "BD_TAG_5_2";
		#endregion

		#region ScoreSumBetDomain
		public const string BD_TAG_6_1 = "BD_TAG_6_1";
		public const string BD_TAG_6_X = "BD_TAG_6_X";
		public const string BD_TAG_6_2 = "BD_TAG_6_2";
		#endregion

		#region FirstHalfWinnerBetDomain
		public const string BD_TAG_7_1 = "BD_TAG_7_1";
		public const string BD_TAG_7_X = "BD_TAG_7_X";
		public const string BD_TAG_7_2 = "BD_TAG_7_2";
		#endregion

		#region SecondHalfWinnerBetDomain
		public const string BD_TAG_8_1 = "BD_TAG_8_1";
		public const string BD_TAG_8_X = "BD_TAG_8_X";
		public const string BD_TAG_8_2 = "BD_TAG_8_2";
		#endregion

		#region ScoreHomeTeamBetDomain
		public const string BD_TAG_9_1 = "BD_TAG_9_0-1";
		public const string BD_TAG_9_X = "BD_TAG_9_2-3";
		public const string BD_TAG_9_2 = "BD_TAG_9_4+";
		#endregion

		#region ScoreAwayTeamBetDomain
		public const string BD_TAG_10_0UPTO1 = "BD_TAG_10_0-1";
		public const string BD_TAG_10_2UPTO3 = "BD_TAG_10_2-3";
		public const string BD_TAG_10_4ORMORE = "BD_TAG_10_4+";
		#endregion

		#region FirstScoreMinuteBetDomain
		public const string BD_TAG_11_1TO10 = "BD_TAG_11_1-10";
		public const string BD_TAG_11_11TO20 = "BD_TAG_11_11-20";
		public const string BD_TAG_11_21TO30 = "BD_TAG_11_21-30";
		public const string BD_TAG_11_31TO40 = "BD_TAG_11_31-40";
		public const string BD_TAG_11_41TO50 = "BD_TAG_11_41-50";
		public const string BD_TAG_11_51TO60 = "BD_TAG_11_51-60";
		public const string BD_TAG_11_61TO70 = "BD_TAG_11_61-70";
		public const string BD_TAG_11_71TO80 = "BD_TAG_11_71-80";
		public const string BD_TAG_11_81TOEND = "BD_TAG_11_81-E";
		#endregion

		#region MatchScoreBetDomain
		public const string BD_TAG_12_SCORE_1_0 = "BD_TAG_12_1:0";
		public const string BD_TAG_12_SCORE_0_0 = "BD_TAG_12_0:0";
		public const string BD_TAG_12_SCORE_0_1 = "BD_TAG_12_0:1";
		public const string BD_TAG_12_SCORE_2_0 = "BD_TAG_12_2:0";
		public const string BD_TAG_12_SCORE_1_1 = "BD_TAG_12_1:1";
		public const string BD_TAG_12_SCORE_0_2 = "BD_TAG_12_0:2";
		public const string BD_TAG_12_SCORE_2_1 = "BD_TAG_12_2:1";
		public const string BD_TAG_12_SCORE_2_2 = "BD_TAG_12_2:2";
		public const string BD_TAG_12_SCORE_1_2 = "BD_TAG_12_1:2";
		public const string BD_TAG_12_SCORE_3_0 = "BD_TAG_12_3:0";
		public const string BD_TAG_12_SCORE_3_3 = "BD_TAG_12_3:3";
		public const string BD_TAG_12_SCORE_0_3 = "BD_TAG_12_0:3";
		public const string BD_TAG_12_SCORE_3_1 = "BD_TAG_12_3:1";
		public const string BD_TAG_12_SCORE_4_4 = "BD_TAG_12_4:4";
		public const string BD_TAG_12_SCORE_1_3 = "BD_TAG_12_1:3";
		public const string BD_TAG_12_SCORE_3_2 = "BD_TAG_12_3:2";
		public const string BD_TAG_12_SCORE_2_3 = "BD_TAG_12_2:3";
		public const string BD_TAG_12_SCORE_4_0 = "BD_TAG_12_4:0";
		public const string BD_TAG_12_SCORE_0_4 = "BD_TAG_12_0:4";
		public const string BD_TAG_12_SCORE_4_1 = "BD_TAG_12_4:1";
		public const string BD_TAG_12_SCORE_1_4 = "BD_TAG_12_1:4";
		public const string BD_TAG_12_SCORE_4_2 = "BD_TAG_12_4:2";
		public const string BD_TAG_12_SCORE_2_4 = "BD_TAG_12_2:4";
		public const string BD_TAG_12_SCORE_4_3 = "BD_TAG_12_4:3";
		public const string BD_TAG_12_SCORE_3_4 = "BD_TAG_12_3:4";
		public const string BD_TAG_12_SCORE_5_0 = "BD_TAG_12_5:0";
		public const string BD_TAG_12_SCORE_0_5 = "BD_TAG_12_0:5";
		public const string BD_TAG_12_SCORE_5_1 = "BD_TAG_12_5:1";
		public const string BD_TAG_12_SCORE_1_5 = "BD_TAG_12_1:5";
		public const string BD_TAG_12_SCORE_5_2 = "BD_TAG_12_5:2";
		public const string BD_TAG_12_SCORE_2_5 = "BD_TAG_12_2:5";
		#endregion

		#region DoubleChanceBetDomain
		public const string BD_TAG_13_1X = "BD_TAG_13_1X";
		public const string BD_TAG_13_X2 = "BD_TAG_13_X2";
		public const string BD_TAG_13_12 = "BD_TAG_13_12";
		#endregion

		#region OverTimeWinnerBetDomain
		public const string BD_TAG_26_1 = "BD_TAG_26_1";
		public const string BD_TAG_26_2 = "BD_TAG_26_2";
		#endregion

		#region PenaltyWinnerBetDomain
		public const string BD_TAG_27_1 = "BD_TAG_27_1";
		public const string BD_TAG_27_2 = "BD_TAG_27_2";
		#endregion

        #region BOTH_TEAM_SCORE_BETDOMAIN
        public const string BD_TAG_28_1 = "BD_TAG_28_1";
        public const string BD_TAG_28_2 = "BD_TAG_28_2";
        #endregion


		#region Tennis
		public const string BD_TAG_52_1 = "BD_TAG_52_1";
		public const string BD_TAG_52_2 = "BD_TAG_52_2";

		public const string BD_TAG_53_2_0 = "BD_TAG_53_2:0";
		public const string BD_TAG_53_2_1 = "BD_TAG_53_2:1";
		public const string BD_TAG_53_1_2 = "BD_TAG_53_1:2";
		public const string BD_TAG_53_0_2 = "BD_TAG_53_0:2";

		public const string BD_TAG_54_3_0 = "BD_TAG_54_3:0";
		public const string BD_TAG_54_3_1 = "BD_TAG_54_3:1";
		public const string BD_TAG_54_3_2 = "BD_TAG_54_3:2";
		public const string BD_TAG_54_2_3 = "BD_TAG_54_2:3";
		public const string BD_TAG_54_1_3 = "BD_TAG_54_1:3";
		public const string BD_TAG_54_0_3 = "BD_TAG_54_0:3";

		public const string BD_TAG_51_1 = "BD_TAG_51_1";
		public const string BD_TAG_51_2 = "BD_TAG_51_2";

		public const string BD_TAG_55_1 = "BD_TAG_55_1";
		public const string BD_TAG_55_2 = "BD_TAG_55_2";
		
		#endregion

        #region Basketball
        public const string BD_TAG_61_UNDER = "BD_TAG_61_UNDER";
        public const string BD_TAG_61_OVER = "BD_TAG_61_OVER";
        public const string _SHORTEXT = "_SHORT";
        #endregion

        #region LiveBet
        public const string BD_TAG_70_1 = "BD_TAG_70_1";
        public const string BD_TAG_70_X = "BD_TAG_70_X";
        public const string BD_TAG_70_2 = "BD_TAG_70_2";

        public const string BD_TAG_71_1 = "BD_TAG_71_1";
        public const string BD_TAG_71_X = "BD_TAG_71_X";
        public const string BD_TAG_71_2 = "BD_TAG_71_2";

        public const string BD_TAG_72_1 = "BD_TAG_72_1";
        public const string BD_TAG_72_X = "BD_TAG_72_X";
        public const string BD_TAG_72_2 = "BD_TAG_72_2";

        public const string BD_TAG_73_1 = "BD_TAG_73_1";
        public const string BD_TAG_73_2 = "BD_TAG_73_2";

        public const string BD_TAG_78_UNDER = "BD_TAG_78_UNDER";
        public const string BD_TAG_78_OVER = "BD_TAG_78_OVER";

        public const string BD_TAG_79_UNDER = "BD_TAG_79_UNDER";
        public const string BD_TAG_79_OVER = "BD_TAG_79_OVER";

        public const string BD_TAG_81_1 = "BD_TAG_81_1";
        public const string BD_TAG_81_X = "BD_TAG_81_X";
        public const string BD_TAG_81_2 = "BD_TAG_81_2";

        public const string BD_TAG_83_1 = "BD_TAG_83_1";
        public const string BD_TAG_83_X = "BD_TAG_83_X";
        public const string BD_TAG_83_2 = "BD_TAG_83_2";

        public const string BD_TAG_84_1 = "BD_TAG_84_1";
        public const string BD_TAG_84_X = "BD_TAG_84_X";
        public const string BD_TAG_84_2 = "BD_TAG_84_2";

        public const string BD_TAG_85_1 = "BD_TAG_85_1";
        public const string BD_TAG_85_X = "BD_TAG_85_X";
        public const string BD_TAG_85_2 = "BD_TAG_85_2";

        public const string BD_TAG_86_1 = "BD_TAG_86_1";
        public const string BD_TAG_86_2 = "BD_TAG_86_2";

        // LIVE_SOCCER_UNDER_OVER_OT_BETDOMAIN
        public const string BD_TAG_87_UNDER = "BD_TAG_87_UNDER";
        public const string BD_TAG_87_OVER = "BD_TAG_87_OVER";

        // LIVE_SOCCER_WINNER_REST_OF_MATCH_OT_BETDOMAIN
        public const string BD_TAG_88_1 = "BD_TAG_88_1";
        public const string BD_TAG_88_X = "BD_TAG_88_X";
        public const string BD_TAG_88_2 = "BD_TAG_88_2";

        // LIVE_SOCCER_NEXT_GOAL_OT_BETDOMAIN = 89;
        public const string BD_TAG_89_1 = "BD_TAG_89_1";
        public const string BD_TAG_89_X = "BD_TAG_89_X";
        public const string BD_TAG_89_2 = "BD_TAG_89_2";

        // LIVE_SOCCER_NEXT_GOAL_SHOTOUT_BETDOMAIN = 90;
        public const string BD_TAG_90_1 = "BD_TAG_90_1";
        public const string BD_TAG_90_2 = "BD_TAG_90_2";

        // LIVE Tennis
        //LIVE_TENNIS_MATCH_BETDOMAIN = 100;
        public const string BD_TAG_100_1 = "BD_TAG_100_1";
        public const string BD_TAG_100_2 = "BD_TAG_100_2";
        //LIVE_TENNIS_SET_BETDOMAIN = 105;
        public const string BD_TAG_105_1 = "BD_TAG_105_1";
        public const string BD_TAG_105_2 = "BD_TAG_105_2";
        // LIVE_TENNIS_WINNER_GAME_X_FROM_SET_Y_BETDOMAIN = 110
        public const string BD_TAG_110_1 = "BD_TAG_110_1";
        public const string BD_TAG_110_2 = "BD_TAG_110_2";
        // LIVE_TENNIS_TOTAL_NUMBER_GAMES_IN_SET_Y_BETDOMAIN = 115;
        public const string BD_TAG_115_UNDER = "BD_TAG_115_UNDER";  
        public const string BD_TAG_115_OVER = "BD_TAG_115_OVER";       

        // LIVE Basketball
        //LIVE_BASKETBALL_WINNER_MATCH_INCL_OT_BETDOMAIN = 200;
        public const string BD_TAG_200_1 = "BD_TAG_200_1";  
        public const string BD_TAG_200_2 = "BD_TAG_200_2";
        //LIVE_BASKETBALL_TOTAL_BETDOMAIN = 205;
        public const string BD_TAG_205_UNDER = "BD_TAG_205_UNDER";
        public const string BD_TAG_205_OVER = "BD_TAG_205_OVER";
        // LIVE_BASKETBALL_TOTAL_PERIOD_BETDOMAIN = 210;
        public const string BD_TAG_210_UNDER = "BD_TAG_210_UNDER";
        public const string BD_TAG_210_OVER = "BD_TAG_210_OVER";
        // LIVE_BASKETBALL_DRAW_NO_BET_PERIOD_BETDOMAIN = 215;
        public const string BD_TAG_215_1 = "BD_TAG_215_1";
        // public const string BD_TAG_215_X = "BD_TAG_215_X";  //GMA 12.05.2011 no X, if it's draw, a cancel bet is sent out to clear the bet
        public const string BD_TAG_215_2 = "BD_TAG_215_2";       
        
        #endregion
    }
}
