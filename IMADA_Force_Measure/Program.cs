using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace IMADA_Force_Measure
{
    class Program
	{
		//This program will send the binary data to Arduino.
		static byte[] DataSendToArduino = { 0x00, 0x01, 0x02 };
		static SerialPort IMADA_SerialPort;
		static SerialPort Arduino_SerialPort;
		static Stopwatch watch = new Stopwatch();
		static bool RecordingData = false;
		static SerialPort OpenSerialPort(string COM_Name, int Baudrate = 115200, int DataBits = 8)
		{
			return new SerialPort(COM_Name)
			{
				BaudRate = Baudrate,
				DataBits = DataBits,
				StopBits = StopBits.One,
				Parity = Parity.None,
				ReadBufferSize = 100 * 1024,
				WriteBufferSize = 100 * 1024,
				ReadTimeout = 2000,
				WriteTimeout = 2000,
				Handshake = Handshake.None,
				RtsEnable = true,
				DtrEnable = true
			};
		}
		static string ReadFileNameFromConsole()
		{
			Console.WriteLine("Press FileName & Press Enter to start.");
			Console.WriteLine("After pressing enter, press Control+C to stop recording");
			string InputText = Console.ReadLine();
			string FileName = InputText?.Trim();
			if (FileName == null || FileName.Length == 0)
				return null;
			return FileName;
		}
		static void Main(string[] args)
		{
			Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e) {
				e.Cancel = true;
				RecordingData = false;
			};

			Console.WriteLine("Press Control+C to stop recording");
			Console.Write("Enter IMADA Device Comm Port: ");
			string IMADA_COM_Port = Console.ReadLine().Trim();
			IMADA_SerialPort = OpenSerialPort(IMADA_COM_Port);
			IMADA_SerialPort.Open();

			Console.Write("Enter Arduino Device Comm Port: ");
			string Arduino_COM_Port = Console.ReadLine().Trim();
			Console.Write("Enter Arduino Device Comm BaudRate: ");
			int Arduino_COM_Port_Baudrate = int.Parse(Console.ReadLine());

			Arduino_SerialPort = OpenSerialPort(Arduino_COM_Port, Arduino_COM_Port_Baudrate, 8);
			Arduino_SerialPort.Open();

			while (true)
			{
				string FileName;
				while ((FileName = ReadFileNameFromConsole()) != null)
					;
                if (File.Exists(FileName))
                {
                    Console.WriteLine($"File \"{FileName}\" Already Exists\n");
                    continue;
                }
                FileStream file = File.Create(FileName);
                StreamWriter swWriter = new StreamWriter(file);

                swWriter.WriteLine("Ticks per ms" + "\t" + TimeSpan.TicksPerMillisecond);
                swWriter.WriteLine("Ticks" + "\t" + "Force");
				char[] array = new char[1024];
				char[] OutputNum = new char[16];
                Console.Clear();
				RecordingData = true;
				watch.Restart();
				Arduino_SerialPort.Write(DataSendToArduino, 0, DataSendToArduino.Length);
				while (RecordingData)
				{
					IMADA_SerialPort.WriteLine("XAR");	//Request new data
                    int DataSize;
                    try
                    {
                        DataSize = IMADA_SerialPort.Read(array, 0, 1024);
                    }catch(TimeoutException e)
                    {
                        continue;
                    }
					//Parse Serial Data
                    for (int i = 0; i < DataSize - 2; ++i)
					{
						if (array[i] != 'r') continue;	//Search the header

						OutputNum[0] = array[i + 1];	//Sign(+/-)

						for (int j = i + 2; j < DataSize; ++j)	//Search the number parts
						{
							if (array[j] == '.' || (array[j] >= '0' && array[j] <= '9'))
								OutputNum[j - i - 1] = array[j];    //Fill all numbers to OutputNum
							else
							{
                                string ForceValue = (new string(OutputNum)).Trim();	//Elimate empty space if exists
                                swWriter.WriteLine(watch.Elapsed.Ticks + "\t" + ForceValue);	//Write into file.
							}
						}
					}
				}
                swWriter.Flush();
                swWriter.Close();
                file.Close();
            }
        }
    }
}
