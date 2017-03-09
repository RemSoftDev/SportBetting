using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace SmartCardReader
{
    public class ComPort : IDisposable
    {
        private SerialPort serialPort;
        private static string com_port;
        private int com_baud_rate;
        private Parity parity = Parity.None;
        private StopBits stop_bits = StopBits.One;


        public ComPort (string port, int baud_rate, Parity _parity, StopBits _stop_bits)
        {
            if (String.IsNullOrEmpty(port))
            {

            }
            else
            {
                com_port = port;
                com_baud_rate = baud_rate;
                parity = _parity;
                stop_bits = _stop_bits;
            }
        }

        public byte[] SendCmd (byte[] cmd)
        {
            byte[] data = null;
            serialPort.DiscardInBuffer ();
            serialPort.Write(cmd, 0, cmd.Length);
            System.Threading.Thread.Sleep(150);

            if (serialPort.BytesToRead > 0)
            {
                data = new byte[serialPort.BytesToRead];
                serialPort.Read(data, 0, data.Length);
            }

            return data;
        }

        public byte[] SendCmd(byte[] cmd, int delay_ms)
        {
            byte[] data = null;
            serialPort.DiscardInBuffer();
            serialPort.Write(cmd, 0, cmd.Length);
            System.Threading.Thread.Sleep (delay_ms);

            if (serialPort.BytesToRead > 0)
            {
                data = new byte[serialPort.BytesToRead];
                serialPort.Read(data, 0, data.Length);
            }

            return data;
        }


        public byte[] SendCmd (string cmd)
        {
            byte[] data = null;
            serialPort.Write(cmd);
            System.Threading.Thread.Sleep(300);

            if (serialPort.BytesToRead > 0)
            {
                data = new byte[serialPort.BytesToRead];
                serialPort.Read(data, 0, data.Length);
            }

            return data;

        }

        public byte[] SendCmd (string cmd, int delay_ms)
        {
            byte[] data = null;
            serialPort.Write(cmd);
            System.Threading.Thread.Sleep (delay_ms);

            if (serialPort.BytesToRead > 0)
            {
                data = new byte[serialPort.BytesToRead];
                serialPort.Read(data, 0, data.Length);
            }

            return data;

        }


        public string GetComPortNane()
        {
            return com_port;
        }



        public bool TryOpen()
        {
            bool result = false;

            if (!String.IsNullOrEmpty (com_port))
            {
                try
                {
                    TryClose();
                    serialPort = new SerialPort(com_port, com_baud_rate, parity, 8, stop_bits);
                    serialPort.ReadTimeout = 2000;
                    serialPort.WriteTimeout = 2000;
                    serialPort.Open();
                    serialPort.DiscardInBuffer();
                    serialPort.DiscardOutBuffer();
                    //serialPort.DataReceived += OnDataReceived;
                    result = serialPort.IsOpen;
                }
                catch
                {
                    result = false;
                }
            }

            return result;
        }

        public bool TryClose()
        {
            bool result = false;

            if (serialPort != null)
            {
                if (serialPort.IsOpen)
                {
                    // serialPort.DataReceived -= OnDataReceived;
                    serialPort.Close();
                    serialPort.Dispose();
                    serialPort = null;
                    result = true;
                }
            }

            return result;
        }

        public bool IsOpen()
        {
            return serialPort.IsOpen;
        }

        public void Dispose()
        {
            if (serialPort != null)
            {
                TryClose();
            }
        }

    }
}

