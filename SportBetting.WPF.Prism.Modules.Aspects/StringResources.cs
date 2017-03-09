namespace SportBetting.WPF.Prism.Modules.Aspects
{
    public static class Topic
    {
        public const string PreMatchServiceName         = "PREMATCH";
        public const string StationPropertyServiceName  = "STATIONPROPERTY";
        public const string LockOfferServiceName        = "LOCKOFFER";
        public const string LiveMatchExpiredServiceName     = "LIVEMATCHEXPIRED";

        public const string MockServiceGetData      = "WsdlService.GetData";
        public const string LiveMatchServiceGetData = "LiveMatchService.GetData";
        public const string BuildEntities = "ToEntitiesConverter.BuildEntities";
        public const string BuildEntity = "ToEntitiesConverter.BuildEntity";

        public const string HandleGuiEvent = "DataEventHandlerService.HandleEvent";

        public const string SrDataReceived = "Sr.DataReceived";
        public const string SrRemove = "Sr.Remove";
        public const string SrHandleAgain = "Sr.HandleAgain";
        public const string ServiceDataReceived = "Service.DataReceived";
        public const string ServiceRemoveEntity = "Service.RemoveEntity";

        public const string LiveSort = "Live.Sort";

        public const string EnableLiveMatches = "Service.DisableLiveMatches";
        public const string DeleteLiveMatches = "Service.DeleteLiveMatches";

        public const string BootstrapperLoadViews = "Bootstrapper.LoadViews";
        public const string OpenLiveWindow = "LiveWindow.Open";

        //ID card related messages
        public const string IdCardGetState = "IdCardService.IdCardGetState";
        public const string OnPinNumberEntered = "IdCardService.PinNumberEntered";
        public const string LIVEMATCH = "LIVEMATCH";

        // AutoLogout
        public const string StartAutoLogoutService = "AutoLogoutService.Start";

        public const string CloseCurrentWindow = " ModalWindowBaseViewModel.CloseCurrentWindow";
    }
}
