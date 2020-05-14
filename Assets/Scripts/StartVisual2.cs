using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class StartVisual2 : MonoBehaviour
{
    [SerializeField]
    InputField[] inputField;

    [SerializeField]
    public Transform pointPrefab;

    [SerializeField]
    public GameObject linePrefab;

    [SerializeField]
    public Text slideText;

    [SerializeField]
    public Text scrollText;

    [Range(10, 1000)]
    public int resolution = 100;

    [SerializeField]
    int width = 1024;
    //int width = 512;

    [SerializeField]
    int shift = 64;

    //グラフ関連
    private Vector3 position;
    private Transform[][] points;
    private GameObject lineGroup;
    private GameObject[][] line;
    private Vector3[][] my3DPoint;

    int slideWave = 0;
    int scrollWave = 0;
    [SerializeField]
    int slideRange = 2;
    [SerializeField]
    int scrollRange = 2;

    int fs = 48000;

    //音関連
    public class Signals
    {
        public float[][] soundSignal;
        public double[][] fftSignal;
    }
    private Signals signals = new Signals();

    void Awake()
    {
        float step = 5f / resolution;
        Vector3 scale = Vector3.one * step / 10f;
        position.y = 0f;
        position.z = 0f;
        points = new Transform[resolution][];

        lineGroup = new GameObject("LineGroup");
        my3DPoint = new Vector3[resolution][];

        for (int i = 0; i < points.Length; i++)
        {
            points[i] = new Transform[resolution];
            my3DPoint[i] = new Vector3[resolution];
            for (int j = 0; j < points[i].Length; j++)
			{
                Transform point = Instantiate(pointPrefab);
                position.x = (i + 0.5f) * step - step * resolution / 2;
                position.z = (j + 0.5f) * step - step * resolution / 2;
                point.localPosition = position;
                point.localScale = scale;
                point.SetParent(transform, false);
                points[i][j] = point;

                my3DPoint[i][j] = position;
            }
        }

        line = new GameObject[my3DPoint.Length - 1][];
        for (int i = 0; i < my3DPoint.Length - 1; i++)
        {
            line[i] = new GameObject[my3DPoint[i].Length];
            for (int j = 0; j < my3DPoint[i].Length; j++)
			{
                line[i][j] = Instantiate(linePrefab);
                DrawLine(my3DPoint, i, j);
                line[i][j].transform.parent = lineGroup.transform;
            }
        }
    }

    /// <summary>
	/// 線描画
	/// </summary>
	/// <param name="my3DVec"></param>
	/// <param name="startPos"></param>
	/// <param name="beginPos"></param>
    void DrawLine(Vector3[][] my3DVec, int startPos, int beginPos)
    {
        Vector3[] myPoint = new Vector3[2];
        for (int idx = 0; idx < 2; idx++)
        {
            myPoint[idx] = my3DVec[startPos + idx][beginPos];
        }
        line[startPos][beginPos].name = "Line" + startPos;
        LineRenderer lRend = line[startPos][beginPos].GetComponent<LineRenderer>();
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
        for (int i = 0; i < points.Length; i++)
        {
            for (int j = 0; j < points[i].Length; j++)
            {
                Transform point = points[i][j];
                position = point.localPosition;
                position.y = signals.soundSignal[i + slideWave][j + scrollWave];
                point.localPosition = position;

                my3DPoint[i][j] = position;
            }
        }
        //線
        for (int i = 0; i < my3DPoint.Length - 1; i++)
        {
            for (int j = 0; j < my3DPoint[i].Length; j++)
            {
                DrawLine(my3DPoint, i, j);
            }
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

        //double変換、二次元配列化
        //重くなるからとりまwidth長shiftずらしが何個？
        signals.fftSignal = new double[WavRead.valuesR.Length / shift][];
        Parallel.For(0, signals.fftSignal.Length, i =>
        {
            signals.fftSignal[i] = new double[width];
            for (int j = 0; j < signals.fftSignal[i].Length; j++)
            {
                signals.fftSignal[i][j] = (double)WavRead.valuesR[i + j * shift];
            }
        });

        //窓
        Parallel.For(0, signals.fftSignal.Length, i =>
        {
            signals.fftSignal[i] = AcousticMath.Windowing(signals.fftSignal[i], "Hamming");
        });

        //fft
        double length_bit_do = Math.Log(width, 2);
        int length_bit = (int)length_bit_do;

        double[][] fftRe = new double[signals.fftSignal.Length][];
        double[][] fftIm = new double[signals.fftSignal.Length][];
        double[][] outfftIm = new double[signals.fftSignal.Length][];
        double[][] outfft = new double[signals.fftSignal.Length][];
        Parallel.For(0, signals.fftSignal.Length, i =>
        {
            fftRe[i] = new double[width];
            fftIm[i] = new double[width];
            outfftIm[i] = new double[width];
            outfft[i] = new double[width / 2];
            AcousticMath.FFT(length_bit, signals.fftSignal[i], fftIm[i], out fftRe[i], out outfftIm[i]);

            for (int j = 0; j < signals.fftSignal[i].Length / 2; j++)
            {
                outfft[i][j] = Math.Sqrt(fftRe[i][j] * fftRe[i][j] + outfftIm[i][j] * outfftIm[i][j]);
            }
        });

        //float変換
        signals.soundSignal = EditArray.double2float(outfft);
        Debug.Log("signals.soundSignal.Length:" + signals.soundSignal.Length);
        Debug.Log("signals.soundSignal[0].Length:" + signals.soundSignal[0].Length);

        //画面に収まるよに正規化
        EditArray.normalize(signals.soundSignal, 1);

        Display();
    }

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
                slideWave -= slideRange;
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
                slideWave += slideRange;
            }
            Display();
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            if (scrollWave <= 0)
            {
                Debug.Log("これ以上下行けん");
                scrollWave = 0;
            }
            else
            {
                scrollWave -= scrollRange;
            }
            Display();
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (scrollWave >= signals.soundSignal[0].Length - resolution)
            {
                Debug.Log("これ以上上行けん");
                scrollWave = signals.soundSignal[0].Length - resolution;
            }
            else
            {
                scrollWave += scrollRange;
            }
            Display();
        }

        slideText.text = slideWave.ToString();
        //double scrollLavel = (fs / 2) * (scrollWave / signals.soundSignal[0].Length);
        scrollText.text = scrollWave.ToString();
    }

    public void OnEndEdit()
    {
        slideWave = int.Parse(inputField[1].text);
        Display();
    }
}
