namespace Nbt.Services.Scf.CashIn.Validator.CCTalk
{

    internal sealed class CCTConstants
    {

        public const byte ACK = 0;
        public const byte AddressChange = 251;
        public const byte AddressClash = 252;
        public const byte AddressPoll = 253;
        public const byte AddressRandom = 250;
        public const byte BeginBillTableUpgrade = 143;
        public const byte BeginFirmwareUpgrade = 139;
        public const string BillValidator = "Bill Validator";
        public const byte BUSYMessage = 6;
        public const byte CalculateROMChecksum = 197;
        public const string CCTalkAddress = "CCTalkAddress";
        public const byte ClearCommsStatusVariables = 3;
        public const string CoinAcceptor = "Coin Acceptor";
        public const byte ConfigurationToEEPROM = 199;
        public const byte CountersToEEPROM = 198;
        public const byte DispenseChange = 223;
        public const byte DispenseCoins = 224;
        public const byte DispenseHopperCoins = 167;
        public const byte DisplayControl = 203;
        public const byte DownloadCalibrationInfo = 182;
        public const byte EmergencyStop = 172;
        public const byte EmptyPayout = 206;
        public const byte EnableHopper = 164;
        public const byte EnterNewPINNumber = 219;
        public const byte EnterPINNumber = 218;
        public const byte ExpansionHeader1 = 100;
        public const byte ExpansionHeader2 = 101;
        public const byte ExpansionHeader3 = 102;
        public const byte ExpansionHeader4 = 103;
        public const byte FactorySetupAndTest = 255;
        public const byte FinishBillTableUpgrade = 142;
        public const byte FinishFirmwareUpgrade = 138;
        public const byte HandheldFunction = 177;
        public const byte IssueGuardCode = 234;
        public const byte KeypadControl = 191;
        public const byte LatchOutputLines = 233;
        public const byte MeterControl = 204;
        public const byte ModifyBankSelect = 179;
        public const byte ModifyBillID = 158;
        public const byte ModifyBillOperatingMode = 153;
        public const byte ModifyCoinID = 185;
        public const byte ModifyDefaultSorterPath = 189;
        public const byte ModifyInhibitAndOverrideRegisters = 162;
        public const byte ModifyInhibitStatus = 231;
        public const byte ModifyMasterInhibitStatus = 228;
        public const byte ModifyPayoutAbsoluteCount = 208;
        public const byte ModifyPayOutCapacity = 187;
        public const byte ModifyPayoutFloat = 175;
        public const byte ModifySecuritySetting = 181;
        public const byte ModifySorterOverrideStatus = 222;
        public const byte ModifySorterPaths = 210;
        public const byte ModifyVariableSet = 165;
        public const byte MyAddress = 1;
        public const byte NAKMessage = 5;
        public const byte OneShotCredit = 220;
        public const byte OperateBidirectionalMotors = 146;
        public const byte OperateMotors = 239;
        public const byte PerformSelfCheck = 232;
        public const byte PerformStackerCycle = 147;
        public const byte PowerManagementControl = 211;
        public const byte PumpRNG = 161;
        public const byte ReadBufferedBillEvents = 159;
        public const byte ReadBufferedCreditOrErrorCodes = 229;
        public const byte ReadDataBlock = 215;
        public const byte ReadInputLines = 237;
        public const byte ReadLastCreditOrErrorCode = 235;
        public const byte ReadOptoVoltages = 148;
        public const byte ReadOtpoStates = 236;
        public const byte RequestAcceptCounter = 225;
        public const byte RequestAddressMode = 169;
        public const byte RequestAlarmCounter = 176;
        public const byte RequestAuditInformationBlock = 205;
        public const byte RequestBankSelect = 178;
        public const byte RequestBaseYear = 170;
        public const byte RequestBillID = 157;
        public const byte RequestBillOperatingMode = 152;
        public const byte RequestBillPosition = 155;
        public const byte RequestBuildCode = 192;
        public const byte requestCipherKey = 160;
        public const byte RequestCoinID = 184;
        public const byte RequestCoinPosition = 212;
        public const byte RequestCommsRevision = 4;
        public const byte RequestCommsStatusVariables = 2;
        public const byte RequestCountryScalingFactor = 156;
        public const byte RequestCreationDate = 196;
        public const byte RequestCurrencyRevision = 145;
        public const byte RequestDatabaseVersion = 243;
        public const byte RequestDataStorageAvailability = 216;
        public const byte RequestDefaultSorterPath = 188;
        public const byte RequestEquipmentCategoryID = 245;
        public const byte RequestFirmwareUpgradeCapability = 141;
        public const byte RequestFraudcounter = 193;
        public const byte RequestHopperCoin = 171;
        public const byte RequestHopperDispeneCount = 168;
        public const byte RequestHopperStatus = 166;
        public const byte RequestIndividualAcceptCounter = 150;
        public const byte RequestIndividualErrorCounter = 149;
        public const byte RequestInhibitStatus = 230;
        public const byte RequestInsertionCounter = 226;
        public const byte RequestLastModificationDate = 195;
        public const byte RequestManufaturerID = 246;
        public const byte RequestMasterInhibitStatus = 227;
        public const byte RequestOptionFlags = 213;
        public const byte RequestPayoutAbsoluteCount = 207;
        public const byte RequestPayoutCapacity = 186;
        public const byte RequestPayoutFloat = 174;
        public const byte RequestPayoutHighLowStatus = 217;
        public const byte RequestPayoutStatus = 190;
        public const byte RequestPollingPriority = 249;
        public const byte RequestProductCode = 244;
        public const byte requestRejectCounter = 194;
        public const byte RequestSecuritySetting = 180;
        public const byte RequestSerialNumber = 242;
        public const byte RequestSoftwareRevision = 241;
        public const byte RequestSorterOverrideStatus = 221;
        public const byte RequestSorterPaths = 209;
        public const byte RequestStatus = 248;
        public const byte RequestTeachStatus = 201;
        public const byte RequestThermistorReading = 173;
        public const byte RequestVariableSet = 247;
        public const byte ResetDevice = 1;
        public const byte ReturnMessageHeader = 0;
        public const byte RouteBill = 154;
        public const byte SetAcceptLimit = 135;
        public const byte SimplePoll = 254;
        public const byte StoreEncryptioncode = 136;
        public const byte SwitchEncryptionCode = 137;
        public const byte TeachModeControl = 202;
        public const byte TestHopper = 163;
        public const byte TestLamps = 151;
        public const byte TestOutputLines = 238;
        public const byte TestSolenoids = 240;
        public const byte UploadBillTables = 144;
        public const byte UploadCoinData = 200;
        public const byte UploadFirmware = 140;
        public const byte UploadWindowData = 183;
        public const byte WriteDataBlock = 214;

        public CCTConstants()
        {
        }

    } // class CCTConstants

}

