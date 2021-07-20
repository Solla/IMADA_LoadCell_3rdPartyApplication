using System;
using System.IO;

namespace IMADA_Force_Measure
{
    class Force_Data_Recorder
    {
        long[] Ticks;
        float[] Forces;
        int DataIndex = 0;
        public Force_Data_Recorder(in int Duration_MS)
        {
            int Data_Length = Duration_MS * 10 + 2000;
            Ticks = new long[Data_Length];
            Forces = new float[Data_Length];
        }
        public void Push(in long Tick, in float Force)
        {
            Ticks[DataIndex] = Tick;
            Forces[DataIndex] = Force;
            DataIndex++;
        }
        public void Save_Raw_Data(string FileName)
        {
            var output = File.CreateText(FileName);
            for (int i = 0; i < DataIndex; ++i)
                output.WriteLine($"{Ticks[i] / (double)TimeSpan.TicksPerMillisecond}\t{Forces[i]}");
            output.Flush();
        }
    }
}
