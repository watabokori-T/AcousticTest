using System;
using System.IO;
using System.Linq;

public class Convolution
{
    const int sampleRate = 48000;
    const short channel = 1;
    const short bitPerSample = 16;

    public static void conv(string tspFilePath, string soundFilePath, string outFilePath)
    {
        double[] tspSignal, soundSignal, impulseSignal;
        short[] shortSignal;

        //TSP読込
        WavRead.ReadWave(tspFilePath);
        tspSignal = EditArray.int2double(WavRead.valuesR);
        Array.Reverse(tspSignal);

        //音源読込
        WavRead.ReadWave(soundFilePath);
        soundSignal = EditArray.int2double(WavRead.valuesR);

        //畳込
        double length_bit_do = Math.Log(WavRead.valuesR.Length, 2);
        int length_bit = (int)length_bit_do;
        impulseSignal = new double[WavRead.valuesR.Length];
        impulseSignal = AcousticMath.Convolution(soundSignal, tspSignal, length_bit);

        //short変換
        shortSignal = EditArray.double2short(impulseSignal);

        //インパルス応答書出
        WavWrite.createWave(shortSignal, outFilePath, channel, sampleRate, bitPerSample);
    }
}
