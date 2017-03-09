using System;
using System.Collections.Generic;
using System.Text;

namespace Nbt.Services.Scf.CashIn.Validator.ID003// Nbt.Services.Scf.CashIn.src.Validator.ID003
{
    public enum AnswerType
    {
        Default,
        ACK,
        Echo,
        Answer
    };

    public class ID003_Commands
    {
        public const byte GetBillTable = 0x8A;
        public const byte Reset = 0x40;
        public const byte StatusRequest = 0x11;
        public const byte Stack1 = 0x41;
        public const byte Stack2 = 0x42;
        public const byte Return = 0x43;
        public const byte VersionRequest = 0x88;
        public const byte SetEnables = 0xC0;
        public const byte SetSecurities = 0xC1;
        public const byte SetCommMode = 0xC2;
        public const byte SetInhibits = 0xC3;
        public const byte SetDirections = 0xC4;

        public const byte OptionalFunction = 0xC5;
        public const byte BarcodeFunction = 0xC6;
        public const byte BarInhibit = 0xC7;

    }

    public  class ID003_Constants
    {

        public const byte Prefix = 0xFC;
        public const byte ACK = 0x50;

        public const int MinAnswerSize = 5;
        public const int MaxPollCounter = 3;
        public const int DenominationEscrowDif = 0x60;
        public const int ResetTimeout = 3000;
        public const int ReadTimeout = 3 * 1000;
       
    }

    public class ID003_Status_Response
    {

        public const byte Enabled = 0x11;
        public const byte Accepting = 0x12;
        public const byte Escrow = 0x13;
        public const byte Stacking = 0x14;
        public const byte VendValid = 0x15;
        public const byte Staked = 0x16;
        public const byte Rejecting = 0x17;
        public const byte Returning = 0x18;
        public const byte Holding = 0x19;
        public const byte Disabled = 0x1A;
        public const byte Initialized = 0x1B;

        public const byte PowerUp = 0x40;
        public const byte PowerUpBillInAcceptor = 0x41;
        public const byte PowerUpBillInStacker = 0x42;

    }

    
}
