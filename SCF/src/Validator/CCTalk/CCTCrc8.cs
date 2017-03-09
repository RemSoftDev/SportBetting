namespace Nbt.Services.Scf.CashIn.Validator.CCTalk
{

    internal class CCTCrc8
    {

        private byte crc;

        public byte CRC
        {
            get
            {
                return (byte)((256 - crc) & 255);
            }
        }

        public CCTCrc8()
        {
        }

        public void PushByte(byte data)
        {
            crc += data;
        }

        public void PushData(byte[] data, int offset, int length)
        {
            for (int i = offset; i < (offset + length); i++)
            {
                PushByte(data[i]);
            }
        }

        public void Reset()
        {
            crc = 0;
        }

    } // class CCTCrc8

}

