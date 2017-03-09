using System;
using System.Collections.Generic;
using System.Globalization;
using Nbt.Services.Scf.CashIn;
using Nbt.Services.Scf.CashIn.Validator;
using Nbt.Services.Spf.Printer;

namespace SharedInterfaces
{
    public interface IStationSettings
    {
        bool IsCashinOk { get;  }
        int Active { get; set; }
        string PrefFileName { get; set; }
        bool TurnOffCashInInit { get; set; }
        bool UsePrinter { get; set; }
        IPrinter Printer { get; set; }
        int PrinterStatus { get; set; }
        CultureInfo Culture { get; set; }
        int SyncInterval { get; set; }
        bool IsCashInEnabled { get; set; }
        void SubscribeCashin(EventHandler<CashInEventArgs> asyncAddMoney);
        void SubscribeLimitExceeded(EventHandler<ValidatorEventArgs<string>> limitExceeded);
        void Init();
        void ReadPrefFileData();
        void AddTestMoNeyFromKeyboard(decimal money);
        void EnableCashIn(decimal stake, decimal limit);
        void CashInDisable();
        bool IsCashDatasetValid();
        void UnSubscribeCashin(EventHandler<CashInEventArgs> depositCashInCashIn);
        bool CheckBillValidator();
        bool CheckCoinAcceptor();
        List<DeviceInfo> GetDeviceInventoryList();
        void InitializeCashIn();
    }
}