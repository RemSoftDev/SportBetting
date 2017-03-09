using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;


namespace Nbt.Services.SerialDevice {
	
	/// <summary>
	/// each TTY should only exist once
	/// </summary>
	public sealed class SerialPortManager {

		private static readonly object lockObj = new object();
		private static Dictionary<string, SerialPort> serialPorts; 
	
	
		
		public static SerialPort Instance(string portName) {			
				lock (lockObj) {
					if (serialPorts == null) 
						serialPorts = new Dictionary<string, SerialPort>();
											
					if (!serialPorts.ContainsKey(portName))						
						serialPorts.Add(portName, new SerialPort(portName));
					return serialPorts[portName];

				}
		}

		public static SerialPort Instance(string portName,int baud, int dataBits, StopBits stop, Parity par) {			
				lock (lockObj) {
					if (serialPorts == null)
						serialPorts = new Dictionary<string, SerialPort>();
					if ( !serialPorts.ContainsKey(portName) )
						serialPorts.Add(portName, new SerialPort(portName, baud, par, dataBits, stop));
					return serialPorts[portName];
				}	
		}

					
						

		
		




	}
}
