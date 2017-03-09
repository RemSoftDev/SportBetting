using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Nbt.Services.Serios {

	/// <summary>
	/// Accessing Hardware (e.g. for RM5)
	/// </summary>
	public class SeriosMK1Wrapper {

			private readonly string DEVICE_STRING = @"//./EZUSB-0"; //device string according to SeriosMK1 manual
			private bool hardwareFound = false;


			//UsbWin32a _objSerUSB;

			private static SeriosMK1Wrapper instance = null; 
			private static readonly object singletonLock = new object();


			#region UsbWin32a.dll

			[DllImport("UsbWin32a.dll")]
			protected static extern int WriteBulkData(byte[] abInBuffer, char bNumberOfBytes, char bPipeNumber, string strDeviceString);

			protected static int Write(byte[] abInBuffer, char bNumberOfBytes, char bPipeNumber, string strDeviceString) {
				return SeriosMK1Wrapper.WriteBulkData(abInBuffer, bNumberOfBytes, bPipeNumber, strDeviceString);
			}

			[DllImport("UsbWin32a.dll")]
			protected static extern int ReadBulkData(byte[] abOutBuffer, char bNumberOfBytes, char bPipeNumber, string strDeviceString);

			protected static int Read(byte[] abOutBuffer, char bNumberOfBytes, char bPipeNumber, string strDeviceString) {
				return SeriosMK1Wrapper.ReadBulkData(abOutBuffer, bNumberOfBytes, bPipeNumber, strDeviceString);
			} 
			#endregion

		
			/// <summary>
			/// Singleton, only one class accessing hardware
			/// </summary>
			public static SeriosMK1Wrapper Instance {
				get {
					lock (singletonLock) {
						if (instance == null) {
							instance = new SeriosMK1Wrapper();
						}
						return instance;
					}
				}
			}

			private SeriosMK1Wrapper() {
			}

			/// <summary>
			/// Hardware available?
			/// </summary>
			public bool HardwareFound {
				get {
					if (!hardwareFound)
						hardwareFound = CheckHardwareAvailable();
					return hardwareFound;
				}
				set { hardwareFound = value; }
			}
	

			/// <summary>
			/// Looks up hardware
			/// </summary>			
			protected bool CheckHardwareAvailable() {
				byte[] outBuffer = new byte[10];

				char numberOfBytes = (char)10;
				char pipeNumber = (char)3;				

				SeriosMK1Wrapper.Read(outBuffer, numberOfBytes, pipeNumber, DEVICE_STRING);
			
				foreach (byte b in outBuffer) 
					if (b == 0) 
						return false;

				return true;
			}

			/// <summary>
			/// Write output
			/// </summary>
			/// <param name="output">new output state</param>
			public void WriteOutput(byte output) {

				const char numberOfBytes = (char) 6;
				const char pipeNumber = (char) 0;
				byte[] outBuffer = new byte[6] { 0x07, output, 0x00, 0x00, 0x00, 0x00 }; 

				if (HardwareFound) 
					SeriosMK1Wrapper.Write(outBuffer, numberOfBytes, pipeNumber, DEVICE_STRING);
				
			}

			/// <summary>
			/// read input
			/// </summary>	
			public bool ReadInput(out byte[] outBuffer) {				
	
				outBuffer = new byte[9];
				const char numberOfBytes = (char) 9;
				const char pipeNumber = (char) 2;

				if (HardwareFound) {
					SeriosMK1Wrapper.Read(outBuffer, numberOfBytes, pipeNumber, DEVICE_STRING);

				}
				return HardwareFound;
			}
        
		}


}
