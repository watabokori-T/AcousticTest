using UnityEngine;
using System.Collections;

public class MicTest : MonoBehaviour
{
    private AudioSource audio;
    float[] audioSignal = new float[1024];

    void Start()
    {
        audio = GetComponent<AudioSource>();
        // マイク名、ループするかどうか、AudioClipの秒数、サンプリングレート を指定する
        audio.clip = Microphone.Start(null, true, 10, 44100);
        // マイクが Ready になるまで待機（一瞬）
        while (Microphone.GetPosition(null) <= 0) { }
        audio.Play();

    }
    
    void Update()
	{
        var audioSignal = audio.GetOutputData(1024, 1);
        for (int i = 1; i < audioSignal.Length - 1; ++i)
        {
            Debug.DrawLine(
                    new Vector3(i - 1, audioSignal[i] + 10, 0),
                    new Vector3(i, audioSignal[i + 1] + 10, 0),
                    Color.red);
        }
            /*
            var spectrum = audio.GetSpectrumData(1024, 0, FFTWindow.Hamming);
            for (int i = 1; i < spectrum.Length - 1; ++i)
            {
                Debug.DrawLine(
                        new Vector3(i - 1, spectrum[i] + 10, 0),
                        new Vector3(i, spectrum[i + 1] + 10, 0),
                        Color.red);
                Debug.DrawLine(
                        new Vector3(i - 1, Mathf.Log(spectrum[i - 1]) + 10, 2),
                        new Vector3(i, Mathf.Log(spectrum[i]) + 10, 2),
                        Color.cyan);
                Debug.DrawLine(
                        new Vector3(Mathf.Log(i - 1), spectrum[i - 1] - 10, 1),
                        new Vector3(Mathf.Log(i), spectrum[i] - 10, 1),
                        Color.green);
                Debug.DrawLine(
                        new Vector3(Mathf.Log(i - 1), Mathf.Log(spectrum[i - 1]), 3),
                        new Vector3(Mathf.Log(i), Mathf.Log(spectrum[i]), 3),
                        Color.yellow);
            }
            */
    }

    private void OnAudioFilterRead(float[] data, int channels)
	{

	}
}