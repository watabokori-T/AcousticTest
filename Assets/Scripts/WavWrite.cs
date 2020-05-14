using System;
using System.IO;
using System.Text;
using UnityEngine;

public class WavWrite
{
    public static WaveHeaderArgs waveHeader = new WaveHeaderArgs();
    
    public static void createWave(short[] Data, string FileName, short channel, int sampleRate, short bitPerSample)
    {
        using (FileStream filStream = new FileStream(FileName, FileMode.Create, FileAccess.Write))
        using (BinaryWriter binWriter = new BinaryWriter(filStream))
        {
            waveHeader.FormatChunkSize = 16;
            waveHeader.FormatID = 1;
            //waveHeader.Channel = 1;
            waveHeader.Channel = channel;
            //waveHeader.SampleRate = 48000;
            waveHeader.SampleRate = sampleRate;
            //waveHeader.BitPerSample = 16;
            waveHeader.BitPerSample = bitPerSample;

            int NumberOfBytePerSample = ((ushort)(Math.Ceiling((double)waveHeader.BitPerSample / 8)));
            waveHeader.BlockSize = (short)(NumberOfBytePerSample * waveHeader.Channel);
            waveHeader.BytePerSec = waveHeader.SampleRate * waveHeader.Channel * NumberOfBytePerSample;
            int DataLength = Data.Length;
            waveHeader.DataChunkSize = waveHeader.BlockSize * DataLength;
            waveHeader.FileSize = waveHeader.DataChunkSize + 44;

            binWriter.Write(headerBytes());

            for (UInt32 cnt = 0; cnt < DataLength; cnt++)
            {
                double Radian = (double)cnt / waveHeader.SampleRate;
                Radian *= 2 * Math.PI;

                binWriter.Write(BitConverter.GetBytes(Data[cnt]));
            }
        }
    }

    public static byte[] headerBytes()
    {
        byte[] Datas = new byte[44];

        Array.Copy(Encoding.ASCII.GetBytes("RIFF"), 0, Datas, 0, 4);
        Array.Copy(BitConverter.GetBytes((UInt32)(waveHeader.FileSize - 8)), 0, Datas, 4, 4);
        Array.Copy(Encoding.ASCII.GetBytes("WAVE"), 0, Datas, 8, 4);
        Array.Copy(Encoding.ASCII.GetBytes("fmt "), 0, Datas, 12, 4);
        Array.Copy(BitConverter.GetBytes((UInt32)(waveHeader.FormatChunkSize)), 0, Datas, 16, 4);
        Array.Copy(BitConverter.GetBytes((UInt16)(waveHeader.FormatID)), 0, Datas, 20, 2);
        Array.Copy(BitConverter.GetBytes((UInt16)(waveHeader.Channel)), 0, Datas, 22, 2);
        Array.Copy(BitConverter.GetBytes((UInt32)(waveHeader.SampleRate)), 0, Datas, 24, 4);
        Array.Copy(BitConverter.GetBytes((UInt32)(waveHeader.BytePerSec)), 0, Datas, 28, 4);
        Array.Copy(BitConverter.GetBytes((UInt16)(waveHeader.BlockSize)), 0, Datas, 32, 2);
        Array.Copy(BitConverter.GetBytes((UInt16)(waveHeader.BitPerSample)), 0, Datas, 34, 2);
        Array.Copy(Encoding.ASCII.GetBytes("data"), 0, Datas, 36, 4);
        Array.Copy(BitConverter.GetBytes((UInt32)(waveHeader.DataChunkSize)), 0, Datas, 40, 4);

        return (Datas);
    }
}
