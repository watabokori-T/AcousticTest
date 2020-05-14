using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Composition : MonoBehaviour
{
    const int sampleRate = 48000;
    const short channel = 1;
    const short bitPerSample = 16;

    const double vol = 32767;
    const double degBase = 360;
    const double C = 261.626;
    const double D = 293.665;
    const double E = 329.628;
    const double F = 349.228;
    const double G = 391.995;
    const double A = 440.0;
    const double B = 493.883;

    [SerializeField]
    InputField inputField;

    [SerializeField]
    InputField inputFieldPrefab;

    [SerializeField]
    GameObject canvas;

    [SerializeField]
    public AudioSource audioSource;

    InputField[][] inputFields;
    [SerializeField]
    private int toneNum = 16;
    [SerializeField]
    private int chordNum = 2;

    private short[] composition;

    void Start()
	{
        inputFields = new InputField[toneNum][];
        for(int i = 0; i < toneNum; i++)
		{
            inputFields[i] = new InputField[chordNum];
            for(int j = 0; j < chordNum; j++)
			{
                inputFields[i][j] = Instantiate(inputFieldPrefab, new Vector3(-450 + i * 60, j * 30, 0), Quaternion.identity);
                inputFields[i][j].transform.SetParent(canvas.transform, false);
                inputFields[i][j].name = "inputField" + i + j;
			}
		}

        composition = new short[sampleRate * 4];
    }

    /// <summary>
	/// Button押下
	/// </summary>
    public void Rec()
	{
        for (int i = 0; i < toneNum; i++)
        {
            for (int j = 0; j < chordNum; j++)
            {
                if(inputFields[i][j].text == "c")
				{
                    compose(i * sampleRate / 4, (i + 1) * sampleRate / 4, C);
                }
                else if(inputFields[i][j].text == "d")
                {
                    compose(i * sampleRate / 4, (i + 1) * sampleRate / 4, D);
                }
                else if (inputFields[i][j].text == "e")
                {
                    compose(i * sampleRate / 4, (i + 1) * sampleRate / 4, E);
                }
                else if (inputFields[i][j].text == "f")
                {
                    compose(i * sampleRate / 4, (i + 1) * sampleRate / 4, F);
                }
                else if (inputFields[i][j].text == "g")
                {
                    compose(i * sampleRate / 4, (i + 1) * sampleRate / 4, G);
                }
                else if (inputFields[i][j].text == "a")
                {
                    compose(i * sampleRate / 4, (i + 1) * sampleRate / 4, A);
                }
                else if (inputFields[i][j].text == "b")
                {
                    compose(i * sampleRate / 4, (i + 1) * sampleRate / 4, B);
                }
				else
				{
                    compose(i * sampleRate / 4, (i + 1) * sampleRate / 4, 0);
                }
            }
        }
        string outFilePath = Application.dataPath + inputField.text;
        WavWrite.createWave(composition, outFilePath, channel, sampleRate, bitPerSample);
    }

    /// <summary>
	/// PlayButton押下
	/// </summary>
    public void playWav()
    {
        string outFilePath = Application.dataPath + inputField.text;
        WavRead.ReadWave(outFilePath);
        float[] newWavClip = EditArray.int2float(WavRead.valuesR);

        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.clip = AudioClip.Create("composition", newWavClip.Length, 1, 48000, false);
        audioSource.clip.SetData(newWavClip, 0);
        audioSource.volume = 0.001f;
        audioSource.Play();
    }

    public void compose(int start, int end, double tone)
	{
        double sampleWidth;
        double deg = 0;
        if(tone == 0)
		{
            for (int i = start; i < end; i++)
            {
                //composition[i] = 0;
                composition[i] += 0;
            }
        }
		else
		{
            for (int i = start; i < end; i++)
            {
                //composition[i] = (short)(vol * Math.Sin(deg * Math.PI / 180));
                composition[i] += (short)(vol * Math.Sin(deg * Math.PI / 180));
                sampleWidth = degBase / (sampleRate / tone);
                deg += sampleWidth;
            }
        }
    }
}
