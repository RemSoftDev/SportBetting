using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslationByMarkupExtension
{
    partial class MultistringTags
    {
        #region PRINTOUT CONTROL UI
        public static MultistringTag SHOP_PRINTOUT_NO_PRESET_NAME = MultistringTag.Assign("SHOP_PRINTOUT_NO_PRESET_NAME",""); // "No preset name is specified. Please specify a name." (LJO)
        public static MultistringTag SHOP_PRINTOUT_EXC_NO_TOURNAMENTS = MultistringTag.Assign("SHOP_PRINTOUT_EXC_NO_TOURNAMENTS",""); // no selected tournaments to print oddsheets (LJO)
        public static MultistringTag SHOP_PRINTOUT_ODDSHEETS_TYPE = MultistringTag.Assign("SHOP_PRINTOUT_ODDSHEETS_TYPE",""); // "Oddsheet type" Oddsheet Type (LJO)
        public static MultistringTag SHOP_PRINTOUT_TEMPLATES = MultistringTag.Assign("SHOP_PRINTOUT_TEMPLATES",""); // "Templates" (LJO)
        public static MultistringTag SHOP_PRINTOUT_TITLE_PAGE = MultistringTag.Assign("SHOP_PRINTOUT_TITLE_PAGE",""); // "Title page" (LJO)
        public static MultistringTag SHOP_PRINTOUT_WATERMARK = MultistringTag.Assign("SHOP_PRINTOUT_WATERMARK",""); // "Watermark" (LJO)
        public static MultistringTag SHOP_PRINTOUT_CB_NO_SELECTION = MultistringTag.Assign("SHOP_PRINTOUT_CB_NO_SELECTION",""); // "None" Default value in the watermark and title page combobox - no image selected (LJO)
        public static MultistringTag SHOP_PRINTOUT_GENERATION_ERROR = MultistringTag.Assign("SHOP_PRINTOUT_GENERATION_ERROR",""); //Failed to create oddsheet report. Please check the Error tab for more info. (LJO)
        #endregion

        #region SIMPLE ODDSHEET
        public static MultistringTag SHOP_PRINTOUT_SIMPLE_ODSHEET = MultistringTag.Assign("SHOP_PRINTOUT_SIMPLE_ODSHEET",""); // "Basic oddsheet" simple oddsheet template name (LJO)
        #endregion

        #region EXTENDED ODDSHEET
        public static MultistringTag SHOP_PRINTOUT_EXTENDED_ODDSHEET = MultistringTag.Assign("SHOP_PRINTOUT_EXTENDED_ODDSHEET",""); // "Extended oddsheet" extended oddsheet template name (LJO)
        public static MultistringTag SHOP_PRINTOUT_EXTENDED_ODDSHEET_RESULT_BETS = MultistringTag.Assign("SHOP_PRINTOUT_EXTENDED_ODDSHEET_RESULT_BETS",""); // "Result Bets" ExtendedOddsheetParameters.ResultBets (LJO)
        public static MultistringTag SHOP_PRINTOUT_EXTENDED_ODDSHEET_TABLES_LAYOUT_TYPE = MultistringTag.Assign("SHOP_PRINTOUT_EXTENDED_ODDSHEET_TABLES_LAYOUT_TYPE",""); // "Tables position" ExtendedOddsheetParameters.TablesLayoutType (LJO)
        // enums
        public static MultistringTag SHOP_ENUM_TABLESLAYOUTTYPE_NONE = MultistringTag.Assign("SHOP_ENUM_TABLESLAYOUTTYPE_NONE",""); // "Hidden" enum TablesLayoutType.None (LJO)
        public static MultistringTag SHOP_ENUM_TABLESLAYOUTTYPE_APPENDONEND = MultistringTag.Assign("SHOP_ENUM_TABLESLAYOUTTYPE_APPENDONEND",""); // "On end" enum TablesLayoutType.AppendOnEnd (LJO)
        public static MultistringTag SHOP_ENUM_TABLESLAYOUTTYPE_APPENTTOTOURNAMENT = MultistringTag.Assign("SHOP_ENUM_TABLESLAYOUTTYPE_APPENTTOTOURNAMENT",""); // "Append to tournament" enum TablesLayoutType.AppentToTournament (LJO)
        public static MultistringTag SHOP_ENUM_ODDSHEETTYPE_TIME = MultistringTag.Assign("SHOP_ENUM_ODDSHEETTYPE_TIME",""); // "Time" Printout oddsheet type (LJO)
        public static MultistringTag SHOP_ENUM_ODDSHEETTYPE_TOURNAMENT = MultistringTag.Assign("SHOP_ENUM_ODDSHEETTYPE_TOURNAMENT",""); // "Tournament" Printout oddsheet type (LJO)

        public static MultistringTag SHOP_ENUM_REPORTGENERATOR_ODDSHEETPRINTING_PARAMETERS_ODDSHEETTYPE_TIME = MultistringTag.Assign("SHOP_ENUM_REPORTGENERATOR_ODDSHEETPRINTING_PARAMETERS_ODDSHEETTYPE_TIME",""); // Time (LJO)
        public static MultistringTag SHOP_ENUM_REPORTGENERATOR_ODDSHEETPRINTING_PARAMETERS_ODDSHEETTYPE_TOURNAMENT = MultistringTag.Assign("SHOP_ENUM_REPORTGENERATOR_ODDSHEETPRINTING_PARAMETERS_ODDSHEETTYPE_TOURNAMENT",""); // Tournament (LJO)

        public static MultistringTag SHOP_ENUM_REPORTGENERATOR_ODDSHEETPRINTING_PARAMETERS_TABLESLAYOUTTYPE_NONE = MultistringTag.Assign("SHOP_ENUM_REPORTGENERATOR_ODDSHEETPRINTING_PARAMETERS_TABLESLAYOUTTYPE_NONE",""); // Don't show (LJO)
        public static MultistringTag SHOP_ENUM_REPORTGENERATOR_ODDSHEETPRINTING_PARAMETERS_TABLESLAYOUTTYPE_APPENDONEND = MultistringTag.Assign("SHOP_ENUM_REPORTGENERATOR_ODDSHEETPRINTING_PARAMETERS_TABLESLAYOUTTYPE_APPENDONEND",""); // On end (LJO)
        public static MultistringTag SHOP_ENUM_REPORTGENERATOR_ODDSHEETPRINTING_PARAMETERS_TABLESLAYOUTTYPE_APPENTTOTOURNAMENT = MultistringTag.Assign("SHOP_ENUM_REPORTGENERATOR_ODDSHEETPRINTING_PARAMETERS_TABLESLAYOUTTYPE_APPENTTOTOURNAMENT",""); // Append to tournament (LJO)

        public static MultistringTag SHOP_PRINTOUT_MATCHRESULT_DRAW = MultistringTag.Assign("SHOP_PRINTOUT_MATCHRESULT_DRAW",""); // Printout form match result (LJO)
        public static MultistringTag SHOP_PRINTOUT_MATCHRESULT_LOSS = MultistringTag.Assign("SHOP_PRINTOUT_MATCHRESULT_LOSS",""); // Printout form match result (LJO)
        public static MultistringTag SHOP_PRINTOUT_MATCHRESULT_WIN = MultistringTag.Assign("SHOP_PRINTOUT_MATCHRESULT_WIN",""); // Printout form match result (LJO)
        #endregion

        #region GENERIC ODDSHEET
        public static MultistringTag SHOP_PRINTOUT_GENERIC_ODDSHEET = MultistringTag.Assign("SHOP_PRINTOUT_GENERIC_ODDSHEET",""); // "Generic oddsheet" extended oddsheet template name (LJO)
        #endregion
    }
}
