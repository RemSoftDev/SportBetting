using System;

namespace Nbt.Services.Scf.CashIn.Validator.CCTalk
{

    internal class CCTCrypt
    {

        public CCTCrypt()
        {
        }



        public static void Encrypt(byte[] data, int offset, int length, byte[] secArray)
        {
            byte[] tapArray ={ 7, 4, 5, 3, 1, 2, 3, 2, 6, 1 };
            int rotatePlaces = 12;
            int feedMasher = 0x63;

            int i, j;
            int b, c, c1;
            byte xorKey;

            if ((offset < 0) || (offset + length > data.Length))
                throw new ArgumentException();

            if (secArray.Length != 6)
                throw new Exception("security code need 6 bytes");

            // initial XOR
            xorKey = (byte)(255 - (16 * secArray[0] + secArray[4]));
            for (i = 0; i < length; i++)
                data[offset + i] ^= xorKey;

            // conditional reverse bit
            for (i = 0; i < length; i++)
            {
                if ((secArray[3] & (1 << (i % 4))) != 0)
                {
                    b = 0;

                    for (j = 0; j < 8; j++)
                        if ((data[offset + i] & (1 << j)) != 0)
                            b += (128 >> j);

                    data[offset + i] = (byte)(b & 255);
                }
            }

            // rotate clockwise through bit 0 of buffer[n-1]
            for (i = 0; i < rotatePlaces; i++)
            {
                c1 = ((data[offset + length - 1] & 0x01) != 0) ? 128 : 0;

                // xor taps
                for (j = 0; j < length; j++)
                {
                    if ((data[offset + j] & (1 << tapArray[(secArray[1] + j) % 10])) != 0)
                        c1 ^= 128;
                }

                for (j = 0; j < length; j++)
                {
                    c = ((data[offset + j] & 0x01) != 0) ? 128 : 0;

                    //xor feed masher
                    if (((secArray[5] ^ feedMasher) & (1 << ((i + j) % 8))) != 0)
                        c ^= 128;

                    //shift buffer right
                    data[offset + j] = (byte)(((data[offset + j] >> 1) + c1) & 255);
                    c1 = c;
                }
            }

            // final XOR
            xorKey = (byte)((16 * secArray[2] + secArray[2]) & 255);
            for (i = 0; i < length; i++)
                data[offset + i] ^= xorKey;
        }

        public static void Decrypt(byte[] data, int offset, int length, byte[] secArray)
        {
            byte[] tapArray ={ 7, 4, 5, 3, 1, 2, 3, 2, 6, 1 };
            int rotatePlaces = 12;
            int feedMasher = 0x63;

            int i, j;
            int b, c, c1;
            byte xorKey;

            // initial XOR
            xorKey = (byte)((16 * secArray[2] + secArray[2]) & 255);
            for (i = 0; i < length; i++)
                data[offset + i] ^= xorKey;

            // rotate clockwise through bit 0 of buffer[n-1]
            for (i = rotatePlaces - 1; i >= 0; i--)
            {
                c1 = ((data[offset] & 0x80) != 0) ? 1 : 0;

                // xor taps
                for (j = 0; j < length; j++)
                {
                    if ((data[offset + j] & (1 << tapArray[(secArray[1] + j) % 10] - 1)) != 0)
                        c1 ^= 1;
                }

                for (j = length - 1; j >= 0; j--)
                {
                    c = ((data[offset + j] & 0x80) != 0) ? 1 : 0;

                    //xor feed masher
                    if (((secArray[5] ^ feedMasher) & (1 << ((i + j - 1) % 8))) != 0)
                        c ^= 1;

                    //shift buffer right
                    data[offset + j] = (byte)(((data[offset + j] << 1) + c1) & 255);
                    c1 = c;
                }
            }

            // conditional reverse bit
            for (i = 0; i < length; i++)
            {
                if ((secArray[3] & (1 << (i % 4))) != 0)
                {
                    b = 0;

                    for (j = 0; j < 8; j++)
                        if ((data[offset + i] & (1 << j)) != 0)
                            b += (128 >> j);

                    data[offset + i] = (byte)(b & 255);
                }
            }

            // final xor
            xorKey = (byte)(255 - (16 * secArray[0] + secArray[4]));
            for (i = 0; i < length; i++)
                data[offset + i] ^= xorKey;
        }

        //public static void DecryptOld(byte[] data, int offset, int length, byte[] secArray)
        //{
        //    int i3, i4;

        //    byte[] bArr = new byte[] { 7, 4, 5, 3, 1, 2, 3, 2, 6, 1 };
        //    int i1 = 12, i2 = 99;
        //    byte b = (byte)(((16 * secArray[2]) + secArray[2]) & 255);
        //    for (i3 = 0; i3 < length; i3++)
        //    {
        //        data[offset + i3] = (byte)(data[offset + i3] ^ b);
        //    }
        //    for (i3 = i1 - 1; i3 >= 0; i3--)
        //    {
        //        int i7 = (data[offset] & 128) != null ? 1 : 0;
        //        for (i4 = 0; i4 < length; i4++)
        //        {
        //            if ((data[offset + i4] & (1 << ((bArr[(secArray[1] + i4) % 10] - 1) & 31))) != null)
        //                i7 ^= 1;
        //        }
        //        for (i4 = length - 1; i4 >= 0; i4--)
        //        {
        //            int i6 = (data[offset + i4] & 128) != null ? 1 : 0;
        //            if (((secArray[5] ^ i2) & (1 << (((i3 + i4 - 1) % 8) & 31))) != null)
        //                i6 ^= 1;
        //            data[offset + i4] = (byte)(((data[offset + i4] << 1) + i7) & 255);
        //            i7 = i6;
        //        }
        //    }
        //    for (i3 = 0; i3 < length; i3++)
        //    {
        //        if ((secArray[3] & (1 << ((i3 % 4) & 31))) != null)
        //        {
        //            int i5 = 0;
        //            for (i4 = 0; i4 < 8; i4++)
        //            {
        //                if ((data[offset + i3] & (1 << (i4 & 31))) != null)
        //                    i5 += 128 >> (i4 & 31);
        //            }
        //            data[offset + i3] = (byte)(i5 & 255);
        //        }
        //    }
        //    b = (byte)(255 - ((16 * secArray[0]) + secArray[4]));
        //    for (i3 = 0; i3 < length; i3++)
        //    {
        //        data[offset + i3] = (byte)(data[offset + i3] ^ b);
        //    }
        //}

        //public static void EncryptOld(byte[] data, int offset, int length, byte[] secArray)
        //{
        //    int i3, i4;

        //    byte[] bArr = new byte[] { 7, 4, 5, 3, 1, 2, 3, 2, 6, 1 };
        //    int i1 = 12, i2 = 99;
        //    if ((offset < 0) || ((offset + length) > data.Length))
        //        throw new ArgumentException();
        //    if (secArray.Length != 6)
        //        throw new Exception("security code need 6 bytes");
        //    byte b = (byte)(255 - ((16 * secArray[0]) + secArray[4]));
        //    for (i3 = 0; i3 < length; i3++)
        //    {
        //        data[offset + i3] = (byte)(data[offset + i3] ^ b);
        //    }
        //    for (i3 = 0; i3 < length; i3++)
        //    {
        //        if ((secArray[3] & (1 << ((i3 % 4) & 31))) != null)
        //        {
        //            int i5 = 0;
        //            for (i4 = 0; i4 < 8; i4++)
        //            {
        //                if ((data[offset + i3] & (1 << (i4 & 31))) != null)
        //                    i5 += 128 >> (i4 & 31);
        //            }
        //            data[offset + i3] = (byte)(i5 & 255);
        //        }
        //    }
        //    for (i3 = 0; i3 < i1; i3++)
        //    {
        //        int i7 = (data[offset + length - 1] & 1) != null ? 128 : 0;
        //        for (i4 = 0; i4 < length; i4++)
        //        {
        //            if ((data[offset + i4] & (1 << (bArr[(secArray[1] + i4) % 10] & 31))) != null)
        //                i7 ^= 128;
        //        }
        //        for (i4 = 0; i4 < length; i4++)
        //        {
        //            int i6 = (data[offset + i4] & 1) != null ? 128 : 0;
        //            if (((secArray[5] ^ i2) & (1 << (((i3 + i4) % 8) & 31))) != null)
        //                i6 ^= 128;
        //            data[offset + i4] = (byte)(((data[offset + i4] >> 1) + i7) & 255);
        //            i7 = i6;
        //        }
        //    }
        //    b = (byte)(((16 * secArray[2]) + secArray[2]) & 255);
        //    for (i3 = 0; i3 < length; i3++)
        //    {
        //        data[offset + i3] = (byte)(data[offset + i3] ^ b);
        //    }
        //}

    } // class CCTCrypt

}

