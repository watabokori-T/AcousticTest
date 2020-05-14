using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StartVisual : MonoBehaviour
{
    [SerializeField]
    InputField[] inputField;

    [SerializeField]
    public Transform pointPrefab;

    [SerializeField]
    public GameObject linePrefab;

    [SerializeField]
    public Text slideText;

    [Range(10, 1000)]
    public int resolution = 1000;

    //グラフ関連
    private Vector3 position;
    private Transform[] points;
    private GameObject lineGroup;
    private GameObject[] line;
    private Vector2[] my2DPoint;

    int slideWave = 0;

    //音関連
    public class Signals
	{
        public float[] soundSignal;
        public double[] fftSignal;
    }
    private Signals signals = new Signals();

    void Awake()
    {
        float step = 5f / resolution;
        Vector3 scale = Vector3.one * step;
        position.y = 0f;
        position.z = 0f;
        points = new Transform[resolution];

        lineGroup = new GameObject("LineGroup");
        my2DPoint = new Vector2[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            Transform point = Instantiate(pointPrefab);
            position.x = (i + 0.5f) * step - step * resolution / 2;
            point.localPosition = position;
            point.localScale = scale;
            point.SetParent(transform, false);
            point.name = "Cube" + i;
            points[i] = point;

            my2DPoint[i] = position;
        }

        line = new GameObject[my2DPoint.Length - 1];
        for (int i = 0; i < my2DPoint.Length - 1; i++)
        {
            line[i] = Instantiate(linePrefab);
            DrawLine(my2DPoint, i);
            line[i].transform.parent = lineGroup.transform;
        }
    }

    /// <summary>
	/// 線描画
	/// </summary>
	/// <param name="my2DVec"></param>
	/// <param name="startPos"></param>
    void DrawLine(Vector2[] my2DVec, int startPos)
    {
        Vector3[] myPoint = new Vector3[2];
        for (int idx = 0; idx < 2; idx++)
        {
            myPoint[idx] = new Vector3(my2DVec[startPos + idx].x, my2DVec[startPos + idx].y, 0.0f);
        }
        line[startPos].name = "Line" + startPos;
        LineRenderer lRend = line[startPos].GetComponent<LineRenderer>();
        lRend.SetVertexCount(2);
        lRend.SetWidth(0.005f, 0.005f);
        Vector3 startVec = myPoint[0];
        Vector3 endVec = myPoint[1];
        lRend.SetPosition(0, startVec);
        lRend.SetPosition(1, endVec);
    }

    /// <summary>
	/// グラフ表示
	/// </summary>
    void Display()
	{
        //プロット
        for (int i = 0; i < points.Length; i++)
        {
            Transform point = points[i];
            position = point.localPosition;
            position.y = signals.soundSignal[i + slideWave];
            point.localPosition = position;

            my2DPoint[i] = position;
        }
        //線
        for (int i = 0; i < my2DPoint.Length - 1; i++)
        {
            DrawLine(my2DPoint, i);
        }
    }

    void Start()
    {
        for (int i = 0; i < inputField.Length; i++)
        {
            inputField[i] = inputField[i].GetComponent<InputField>();
        }

        inputField[1].text = "0";
    }

    /// <summary>
	/// 波形表示
	/// </summary>
    public void startVisual()
    {
        //wav読込
        string waveFilePath1 = Application.dataPath + inputField[0].text;
        Debug.Log(waveFilePath1);
        if (!File.Exists(waveFilePath1))
        {
            Debug.Log("ファイルどこじゃ？");
        }
        WavRead.ReadWave(waveFilePath1);

        //float変換
        signals.soundSignal = new float[WavRead.valuesR.Length];
        signals.soundSignal = EditArray.int2float(WavRead.valuesR);

        //画面に収まるよに正規化
        EditArray.normalize(signals.soundSignal, 1);

        Display();
    }

    /// <summary>
	/// スペクトル表示
	/// </summary>
    public void startFFTVisual()
    {
        //wav読込
        string waveFilePath1 = Application.dataPath + inputField[0].text;
        Debug.Log(waveFilePath1);
        if (!File.Exists(waveFilePath1))
        {
            Debug.Log("ファイルどこじゃ？");
        }
        WavRead.ReadWave(waveFilePath1);

        //double変換
        signals.fftSignal = EditArray.int2double(WavRead.valuesR);

        //窓
        signals.fftSignal = AcousticMath.Windowing(signals.fftSignal, "Hamming");

        //fft
        double length_bit_do = Math.Log(signals.fftSignal.Length, 2);
        int length_bit = (int)length_bit_do;

        double[] fftRe = new double[signals.fftSignal.Length];
        double[] fftIm = new double[signals.fftSignal.Length];
        double[] outfftIm = new double[signals.fftSignal.Length];

        AcousticMath.FFT(length_bit, signals.fftSignal, fftIm, out fftRe, out outfftIm);
        
        double[] outfft = new double[signals.fftSignal.Length / 2];
        for(int i = 0; i < signals.fftSignal.Length / 2; i++)
		{
            outfft[i] = Math.Sqrt(fftRe[i] * fftRe[i] + outfftIm[i] * outfftIm[i]);
		}

        //float変換
        signals.soundSignal = EditArray.double2float(outfft);

        //画面に収まるよに正規化
        EditArray.normalize(signals.soundSignal, 1);

        Display();
    }

    //表示スライド
    void Update()
	{
        if (Input.GetKey(KeyCode.LeftArrow))
        {
			if (slideWave <= 0)
			{
                Debug.Log("これ以上左行けん");
                slideWave = 0;
			}
			else
			{
                slideWave -= 10;
            }
            Display();
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (slideWave >= signals.soundSignal.Length - resolution)
            {
                Debug.Log("これ以上右行けん");
                slideWave = signals.soundSignal.Length - resolution;
            }
			else
			{
                slideWave += 10;
            }
            Display();
        }

        slideText.text = slideWave.ToString();
    }

    public void OnEndEdit()
	{
        slideWave = int.Parse(inputField[1].text);
        Display();
    }
}
