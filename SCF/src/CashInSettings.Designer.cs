﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Nbt.Services.Scf.CashIn {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class CashInSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static CashInSettings defaultInstance = ((CashInSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new CashInSettings())));
        
        public static CashInSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("6")]
        public int NumOfValidators {
            get {
                return ((int)(this["NumOfValidators"]));
            }
            set {
                this["NumOfValidators"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Validator")]
        public string PrefValidatorKey {
            get {
                return ((string)(this["PrefValidatorKey"]));
            }
            set {
                this["PrefValidatorKey"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("_Device")]
        public string PrefValidatorDevice {
            get {
                return ((string)(this["PrefValidatorDevice"]));
            }
            set {
                this["PrefValidatorDevice"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("CashIn")]
        public string PrefName {
            get {
                return ((string)(this["PrefName"]));
            }
            set {
                this["PrefName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("_Channel_")]
        public string PrefValidatorChannel {
            get {
                return ((string)(this["PrefValidatorChannel"]));
            }
            set {
                this["PrefValidatorChannel"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("_WriteDelay_ms")]
        public string PrefValidatorWriteDelay {
            get {
                return ((string)(this["PrefValidatorWriteDelay"]));
            }
            set {
                this["PrefValidatorWriteDelay"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("_Baudrate")]
        public string PrefValidatorBaud {
            get {
                return ((string)(this["PrefValidatorBaud"]));
            }
            set {
                this["PrefValidatorBaud"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("_Databits")]
        public string PrefValidatorDatabits {
            get {
                return ((string)(this["PrefValidatorDatabits"]));
            }
            set {
                this["PrefValidatorDatabits"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("_Stopbits")]
        public string PrefValidatorStopbits {
            get {
                return ((string)(this["PrefValidatorStopbits"]));
            }
            set {
                this["PrefValidatorStopbits"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("_Parity")]
        public string PrefValidatorParity {
            get {
                return ((string)(this["PrefValidatorParity"]));
            }
            set {
                this["PrefValidatorParity"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("300")]
        public int ValidatorBaudrate {
            get {
                return ((int)(this["ValidatorBaudrate"]));
            }
            set {
                this["ValidatorBaudrate"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("8")]
        public int ValidatorDatabits {
            get {
                return ((int)(this["ValidatorDatabits"]));
            }
            set {
                this["ValidatorDatabits"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Two")]
        public global::System.IO.Ports.StopBits ValidatorStopbits {
            get {
                return ((global::System.IO.Ports.StopBits)(this["ValidatorStopbits"]));
            }
            set {
                this["ValidatorStopbits"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("None")]
        public global::System.IO.Ports.Parity ValidatorParity {
            get {
                return ((global::System.IO.Ports.Parity)(this["ValidatorParity"]));
            }
            set {
                this["ValidatorParity"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("_NoOverpay")]
        public string PrefValidatorNoOverpay {
            get {
                return ((string)(this["PrefValidatorNoOverpay"]));
            }
            set {
                this["PrefValidatorNoOverpay"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ValidatorNoOverpay {
            get {
                return ((bool)(this["ValidatorNoOverpay"]));
            }
            set {
                this["ValidatorNoOverpay"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int RM5_WaitAfterInput {
            get {
                return ((int)(this["RM5_WaitAfterInput"]));
            }
            set {
                this["RM5_WaitAfterInput"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int RM5_DelayPoll {
            get {
                return ((int)(this["RM5_DelayPoll"]));
            }
            set {
                this["RM5_DelayPoll"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("6")]
        public int RM5_ChannelCount {
            get {
                return ((int)(this["RM5_ChannelCount"]));
            }
            set {
                this["RM5_ChannelCount"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("8")]
        public int NV10_ChannelCount {
            get {
                return ((int)(this["NV10_ChannelCount"]));
            }
            set {
                this["NV10_ChannelCount"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("_Type")]
        public string PrefValidatorType {
            get {
                return ((string)(this["PrefValidatorType"]));
            }
            set {
                this["PrefValidatorType"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("_Address")]
        public string PrefValidatorAddress {
            get {
                return ((string)(this["PrefValidatorAddress"]));
            }
            set {
                this["PrefValidatorAddress"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("_EncryptionKey")]
        public string PrefValidatorEncryptionKey {
            get {
                return ((string)(this["PrefValidatorEncryptionKey"]));
            }
            set {
                this["PrefValidatorEncryptionKey"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("_UseAcceptorValues")]
        public string UseAcceptorValues {
            get {
                return ((string)(this["UseAcceptorValues"]));
            }
            set {
                this["UseAcceptorValues"] = value;
            }
        }
    }
}
