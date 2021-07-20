using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace IMADA_Force_Measure
{
    class Program
    {
		/// <summary>
		/// How many times should a setting be tested.
		/// </summary>
		public const int Repeated_Count = 10;
		/// <summary>
		/// The interval delay between two experiments.
		/// </summary>
		const int Interval_Delay = 1000;
		/// <summary>
		/// Duration per experiment.
		/// </summary>
		const int Duration = 1000;
		/// <summary>
		/// The response time is the time between sending a message and when the measured force reaches this value.
		/// </summary>
		const double ResponseTime_Diff_N = 0.01;
		static IMADA_LoadCell LoadCell;
		static SerialPort arduino;
		static void Init()
		{
			LoadCell = IMADA_LoadCell.FindIMADA();

			if (LoadCell == null)
				throw new Exception("IMADA Not Found.");

			Console.Write("Enter Arduino Device Comm Name: ");
			string Arduino_COM_Port = Console.ReadLine().Trim();
			Console.Write("Enter Arduino Device Comm BaudRate: ");
			int Arduino_COM_Port_Baudrate = int.Parse(Console.ReadLine());
			arduino = new SerialPort(Arduino_COM_Port, Arduino_COM_Port_Baudrate);
			arduino.Open();
		}

		static void Main(string[] args)
		{
			Init();
			Thread.CurrentThread.Priority = ThreadPriority.Highest;
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

			string FolderName = DateTime.Now.ToString("HHmmss");
			Directory.CreateDirectory(FolderName);
			Console.WriteLine($"Create New Directory: {FolderName}");

			Stopwatch stopwatch = new Stopwatch();

			for (int n = 0; n < Repeated_Count; ++n)
			{
				Console.WriteLine($"Iterate{n} is going to start!");
				Thread.Sleep(Interval_Delay);
				Force_Data_Recorder Force_Data = new Force_Data_Recorder(Duration);

				LoadCell.ResetForce();  //Reset Force in Load Cell
				LoadCell.StartRecording();  //Start Recording
				LoadCell.DiscardInBuffer(); //Remove all buffer
				LoadCell.DiscardOutBuffer(); //Remove all buffer

				Thread.Sleep(10);    //Wait for Load Cell
				if (LoadCell.Counter <= 0 || Math.Abs(LoadCell.Force) > 0.01)
				{
					Console.WriteLine("Detect Error in Load Cell!");	//Load Cell is not stable
					Thread.Sleep(10000);
					n--;
					continue;
				}

				stopwatch.Reset();  //Reset stopwatch
				long ArduinoStartTick = stopwatch.Elapsed.Ticks;
				arduino.Write("Go"); //Start Producing Force
				int Counter = 0;
				long ResponseTimeTicks = 0;
				stopwatch.Start();
				while (Duration > stopwatch.ElapsedMilliseconds)
				{
					if (LoadCell.Counter == Counter)    //If new data is not ready
						continue;
					Counter = LoadCell.Counter; //Update the new counter
					float Force = LoadCell.Force;   //Update the new force
					long DataTicks = stopwatch.Elapsed.Ticks;

					if (ResponseTimeTicks == 0 && Math.Abs(Force) > ResponseTime_Diff_N)
						ResponseTimeTicks = DataTicks - ArduinoStartTick;
					Force_Data.Push(DataTicks, Force);
				}
				LoadCell.StopRecording();

				double ResponseTime_MS = ResponseTimeTicks / (double)TimeSpan.TicksPerMillisecond;
				Console.WriteLine($"Iterate:{n} Response Time {ResponseTime_MS}");
				Force_Data.Save_Raw_Data($"{FolderName}\\Iterate_{n}.tsv");
				GC.Collect();
			}
			Console.ReadKey();
		}
	}
}
