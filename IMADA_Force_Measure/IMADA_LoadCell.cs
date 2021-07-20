using System.Diagnostics;
using System.IO.Ports;
using System.Management;
using System.Threading;

namespace IMADA_Force_Measure
{
    public class IMADA_LoadCell
	{
		SerialPort IMADA_SerialPort;
		private Thread thread;
		public volatile int Counter;
		public volatile float Force;
		protected IMADA_LoadCell(in SerialPort serialPort)
		{
			IMADA_SerialPort = serialPort;
		}
		public static IMADA_LoadCell FindIMADA()
		{
			ManagementObjectCollection ManObjReturn;
			ManagementObjectSearcher ManObjSearch;
			ManObjSearch = new ManagementObjectSearcher("Select * from Win32_SerialPort");
			ManObjReturn = ManObjSearch.Get();
			foreach (ManagementObject ManObj in ManObjReturn)
			{
				if (!ManObj["Name"].ToString().Contains("IMADA"))
					continue;
				string IMADA_COM_Port = ManObj["DeviceID"].ToString();
				SerialPort IMADA_SerialPort = new SerialPort(IMADA_COM_Port);
				IMADA_SerialPort.BaudRate = 115200;
				IMADA_SerialPort.DataBits = 8;
				IMADA_SerialPort.StopBits = StopBits.One;
				IMADA_SerialPort.Parity = Parity.None;
				IMADA_SerialPort.NewLine = "\r";
				IMADA_SerialPort.ReadBufferSize = 102400;
				IMADA_SerialPort.WriteBufferSize = 102400;
				IMADA_SerialPort.ReadTimeout = 2000;
				IMADA_SerialPort.WriteTimeout = 2000;
				IMADA_SerialPort.Handshake = Handshake.None;
				IMADA_SerialPort.RtsEnable = true;
				IMADA_SerialPort.DtrEnable = true;
				IMADA_SerialPort.Open();
				return new IMADA_LoadCell(IMADA_SerialPort);
			}
			return null;
		}
		private void RequireOneData()
		{
			IMADA_SerialPort.WriteLine("XAR");
		}
		public virtual void ResetForce()
		{
			IMADA_SerialPort.WriteLine("XFZ");
		}
		public virtual void DiscardInBuffer()
		{
			IMADA_SerialPort.DiscardInBuffer();
		}
		public virtual void DiscardOutBuffer()
		{
			IMADA_SerialPort.DiscardOutBuffer();
		}
		public virtual void StopRecording()
		{
			thread?.Abort();
			thread = null;
		}
		public virtual void StartRecording()
		{
			thread?.Abort();
			thread = new Thread(WorkerThread)
			{
				Priority = ThreadPriority.Highest,
				IsBackground = true
			};
			Counter = -1;
			Force = 0;
			thread.Start();
		}
		private void WorkerThread()
		{
			try
			{
				Stopwatch TimeoutStopwatch = new Stopwatch();
				TimeoutStopwatch.Start();
				while (true)
				{
					string values = null;
					TimeoutStopwatch.Restart();
					RequireOneData();
					while (TimeoutStopwatch.ElapsedMilliseconds < 2)
					{
						values = IMADA_SerialPort.ReadExisting();
						if (values.Contains("r"))
						{
							string ForceValueString = values.Split('r')[1].Substring(0, 6);
							Force = float.Parse(ForceValueString);
							break;
						}
					}
					if (values == null)
						continue;
					Counter++;
				}
			}catch
			{

			}
		}
	}

}
