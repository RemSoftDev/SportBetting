

namespace Nbt.Services.Scf.CashIn.Validator.SSP
{
    class CCommands
    {
        public const byte SSP_CMD_RESET = 0x01;
        public const byte SSP_CMD_HOST_PROTOCOL_VERSION = 0x06;
        public const byte SSP_CMD_GET_SERIAL_NUMBER = 0x0C;
        public const byte SSP_CMD_SYNC = 0x11;
        public const byte SSP_CMD_SET_GENERATOR = 0x4A;
        public const byte SSP_CMD_SET_MODULUS = 0x4B;
        public const byte SSP_CMD_KEY_EXCHANGE = 0x4C;
        public const byte SSP_CMD_SET_INHIBITS = 0x02;
        public const byte SSP_CMD_ENABLE = 0xA;
        public const byte SSP_CMD_DISABLE = 0x09;
        public const byte SSP_CMD_POLL = 0x7;
        public const byte SSP_CMD_SETUP_REQUEST = 0x05;
        public const byte SSP_CMD_DISPLAY_ON = 0x03;
        public const byte SSP_CMD_DISPLAY_OFF = 0x04;
        public const byte SSP_CMD_EMPTY = 0x3F;
        public const byte SSP_CMD_LAST_REJECT_CODE = 0x17;
        public const byte SSP_CMD_GET_FW_VERSION = 0x20;
        public const byte SSP_CMD_DATASET_VERSION = 0x21;

        public const byte SSP_POLL_RESET = 0xF1;
        public const byte SSP_POLL_NOTE_READ = 0xEF;
        public const byte SSP_POLL_CREDIT = 0xEE;
        public const byte SSP_POLL_REJECTING = 0xED;
        public const byte SSP_POLL_REJECTED = 0xEC;
        public const byte SSP_POLL_STACKING = 0xCC;
        public const byte SSP_POLL_STACKED = 0xEB;
        public const byte SSP_POLL_SAFE_JAM = 0xEA;
        public const byte SSP_POLL_UNSAFE_JAM = 0xE9;
        public const byte SSP_POLL_DISABLED = 0xE8;
        public const byte SSP_POLL_FRAUD_ATTEMPT = 0xE6;
        public const byte SSP_POLL_STACKER_FULL = 0xE7;
        public const byte SSP_POLL_NOTE_CLEARED_FROM_FRONT = 0xE1;
        public const byte SSP_POLL_NOTE_CLEARED_TO_CASHBOX = 0xE2;
        public const byte SSP_POLL_CASHBOX_REMOVED = 0xE3;
        public const byte SSP_POLL_CASHBOX_REPLACED = 0xE4;
        public const byte SSP_POLL_BAR_CODE_VALIDATED = 0xE5;
        public const byte SSP_POLL_BAR_CODE_ACK = 0xD1;
        public const byte SSP_POLL_NOTE_PATH_OPEN = 0xE0;
        public const byte SSP_POLL_CHANNEL_DISABLE = 0xB5;

        public const byte SSP_RESPONSE_CMD_OK = 0xF0;
        public const byte SSP_RESPONSE_CMD_UNKNOWN = 0xF2;
        public const byte SSP_RESPONSE_CMD_WRONG_PARAMS = 0xF3;
        public const byte SSP_RESPONSE_CMD_PARAM_OUT_OF_RANGE = 0xF4;
        public const byte SSP_RESPONSE_CMD_CANNOT_PROCESS = 0xF5;
        public const byte SSP_RESPONSE_CMD_SOFTWARE_ERROR = 0xF6;
        public const byte SSP_RESPONSE_CMD_FAIL = 0xF8;
        public const byte SSP_RESPONSE_CMD_KEY_NOT_SET = 0xFA;
    }
}
