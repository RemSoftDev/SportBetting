using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SerialPortManager;

namespace SerialPortManager
{
    class Program
    {
      
        static void Main(string[] args)
        {
            SerialPortManager spm = SerialPortManager.Instance;
            spm.GetIdReaderSerialPort();
           
        }
    }
}
