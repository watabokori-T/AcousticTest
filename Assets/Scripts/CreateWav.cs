using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CreateWav : MonoBehaviour
{
    [SerializeField]
    InputField inputField;

    [SerializeField]
    int wavLength = 1048576;

    [SerializeField]
    public AudioSource audioSource;

    const int sampleRate = 48000;
    const short channel = 1;
    const short bitPerSample = 16;

    public void createWav()
	{
        //short[] newWav = noise(wavLength);
        short[] newWav = yamaha();

        string outFilePath = Application.dataPath + inputField.text;
        WavWrite.createWave(newWav, outFilePath, channel, sampleRate, bitPerSample);
	}

    public void playWav()
	{
        string outFilePath = Application.dataPath + inputField.text;
        WavRead.ReadWave(outFilePath);
        float[] newWavClip = EditArray.int2float(WavRead.valuesR);

        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.clip = AudioClip.Create("newwav", newWavClip.Length, 1, 48000, false);
        audioSource.clip.SetData(newWavClip, 0);
        audioSource.volume = 0.001f;
        audioSource.Play();
    }

    /// <summary>
	/// ノイズ
	/// </summary>
	/// <param name="leng">長さ</param>
	/// <returns></returns>
    public short[] noise(int leng)
	{
        short[] noise = new short[leng];
        System.Random r = new System.Random();
        for (int i = 0; i < leng; i++)
        {
            noise[i] = (short)r.Next(Int16.MinValue, Int16.MaxValue);
        }
        return noise;
    }

    /// <summary>
	/// ヤマハ音楽教室正弦波
	/// </summary>
	/// <returns></returns>
    public short[] yamaha()
	{
        //const int sampleRate = 48000;
        const double vol = 32767;
        const double degBase = 360;
        const double C = 261.626;
        const double D = 293.665;
        const double E = 329.628;
        const double F = 349.228;
        const double G = 391.995;
        const double A = 440.0;
        double sampleWidth;
        double deg = 0;

        short[] yamaha = new short[sampleRate * 4];
        int i = 0;
        for(i = 0; i < sampleRate / 4; i++)
		{
            yamaha[i] = (short)(vol * Math.Sin(deg * Math.PI / 180));
            sampleWidth = degBase / (sampleRate / C);
            deg += sampleWidth;
		}
        for (i = sampleRate / 4; i < sampleRate / 2; i++)
        {
            yamaha[i] = (short)(vol * Math.Sin(deg * Math.PI / 180));
            sampleWidth = degBase / (sampleRate / D);
            deg += sampleWidth;
        }
        for (i = sampleRate / 2; i < sampleRate * 3 / 4; i++)
        {
            yamaha[i] = (short)(vol * Math.Sin(deg * Math.PI / 180));
            sampleWidth = degBase / (sampleRate / E);
            deg += sampleWidth;
        }
        for (i = sampleRate * 3 / 4; i < sampleRate; i++)
        {
            yamaha[i] = (short)(vol * Math.Sin(deg * Math.PI / 180));
            sampleWidth = degBase / (sampleRate / F);
            deg += sampleWidth;
        }
        for (i = sampleRate; i < sampleRate * 3 / 2; i++)
        {
            yamaha[i] = (short)(vol * Math.Sin(deg * Math.PI / 180));
            sampleWidth = degBase / (sampleRate / G);
            deg += sampleWidth;
        }
        for (i = sampleRate * 3 / 2; i < sampleRate * 7 / 4; i++)
        {
            yamaha[i] = (short)(vol * Math.Sin(deg * Math.PI / 180));
            sampleWidth = degBase / (sampleRate / A);
            deg += sampleWidth;
        }
        for (i = sampleRate * 7 / 4; i < sampleRate * 2; i++)
        {
            yamaha[i] = (short)(vol * Math.Sin(deg * Math.PI / 180));
            sampleWidth = degBase / (sampleRate / F);
            deg += sampleWidth;
        }
        for (i = sampleRate * 2; i < sampleRate * 9 / 4; i++)
        {
            yamaha[i] = (short)(vol * Math.Sin(deg * Math.PI / 180));
            sampleWidth = degBase / (sampleRate / E);
            deg += sampleWidth;
        }
        for (i = sampleRate * 9 / 4; i < sampleRate * 5 / 2; i++)
        {
            yamaha[i] = 0;
        }
        for (i = sampleRate * 5 / 2; i < sampleRate * 11 / 4; i++)
        {
            yamaha[i] = (short)(vol * Math.Sin(deg * Math.PI / 180));
            sampleWidth = degBase / (sampleRate / D);
            deg += sampleWidth;
        }
        for (i = sampleRate * 11 / 4; i < sampleRate * 3; i++)
        {
            yamaha[i] = 0;
        }
        for (i = sampleRate * 3; i < sampleRate * 7 / 2; i++)
        {
            yamaha[i] = (short)(vol * Math.Sin(deg * Math.PI / 180));
            sampleWidth = degBase / (sampleRate / C);
            deg += sampleWidth;
        }
        for (i = sampleRate * 7 / 2; i < sampleRate * 4; i++)
        {
            yamaha[i] = 0;
        }

        return yamaha;
	}
}
