using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;

namespace SportBetting.WPF.Prism.Shared
{
    public class MatchColorBarConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            if (values == null)
                return "transparent";

            try
            {
                bool isMatchEnabled;
                int srPeriodInfo;
                int OddCount;
                string sFile;

                if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue || values[1].ToString().ToLowerInvariant() == "undefined" || values[0].ToString().ToLowerInvariant() == "undefined")
                {
                    return "transparent";
                }

                isMatchEnabled = values[0] == DependencyProperty.UnsetValue ? ((MatchLn)values[3]).MatchView.IsEnabled : (bool)values[0];
                srPeriodInfo = values[1] == DependencyProperty.UnsetValue ? (int)((MatchLn)values[3]).MatchView.LivePeriodInfo : (int)values[1];
                OddCount = values[2] == DependencyProperty.UnsetValue ? ((MatchLn)values[3]).MatchView.AllEnabledOddCount : (int)values[2];
                sFile = null;

                if (isMatchEnabled && OddCount > 0)
                {

                    switch (srPeriodInfo)
                    {
                        case (int)eLivePeriodInfo.Paused:
                            sFile = "purple";
                            break;
                        case (int)eLivePeriodInfo.Basketball_1st_Quarter:
                        case (int)eLivePeriodInfo.Basketball_2nd_Quarter:
                        case (int)eLivePeriodInfo.Basketball_3rd_Quarter:
                        case (int)eLivePeriodInfo.Basketball_4th_Quarter:
                        case (int)eLivePeriodInfo.IceHockey_1st_Third:
                        case (int)eLivePeriodInfo.IceHockey_2nd_Third:
                        case (int)eLivePeriodInfo.IceHockey_3rd_Third:
                        case (int)eLivePeriodInfo.Soccer_1st_Period:
                        case (int)eLivePeriodInfo.Tennis_1st_Set:
                        case (int)eLivePeriodInfo.Tennis_2nd_Set:
                        case (int)eLivePeriodInfo.Tennis_3rd_Set:
                        case (int)eLivePeriodInfo.Tennis_4th_Set:
                        case (int)eLivePeriodInfo.Tennis_5th_Set:
                            sFile = "green";
                            break;
                        case (int)eLivePeriodInfo.Soccer_2nd_Period:
                            sFile = "orange";
                            break;
                        case (int)eLivePeriodInfo.Basketball_OverTime:
                            sFile = "gray";
                            break;
                        case (int)eLivePeriodInfo.Basketball_Pause1:
                        case (int)eLivePeriodInfo.Basketball_Pause2:
                        case (int)eLivePeriodInfo.Basketball_Pause3:
                            sFile = "purple";
                            break;
                        case (int)eLivePeriodInfo.Soccer_1st_PeriodOverTime:
                        case (int)eLivePeriodInfo.Soccer_2nd_PeriodOverTime:
                        case (int)eLivePeriodInfo.Penalty:
                        case (int)eLivePeriodInfo.OverTime:
                            sFile = "orange_overtime";
                            break;
                        default:
                            return "transparent";
                    }

                }
                else
                {
                    sFile = srPeriodInfo == (int)eLivePeriodInfo.Interrupted ? "interrupted" : "red";
                }
                if (string.IsNullOrEmpty(sFile))
                    return "transparent";



                return sFile;
            }
            catch (Exception e)
            {

            }
            return "transparent";

        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}

