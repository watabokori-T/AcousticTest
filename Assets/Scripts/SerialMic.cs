using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SerialMic : MonoBehaviour
{
    [SerializeField]
	public SerialHandler serialHandler;
    [SerializeField]
    public Text text;
    public int baudRate = 115200;  // ボーレート(Arduinoに記述したものに合わせる)

    private List<short> inputList = new List<short>();
    private short input;

    private bool pressed = false;
    private bool rec = false;

    private double time;

    //グラフ関連
    [SerializeField]
    public Transform pointPrefab;
    [SerializeField]
    public GameObject linePrefab;
    private Transform[] points;
    private Vector3 position;
    private GameObject lineGroup;
    private GameObject[] line;
    private Vector2[] my2DPoint;

    void Start()
    {
        serialHandler.OnDataReceived += OnDataReceived;
    }

    void Update()
    {
        if (rec == true)
        {
            time += Time.deltaTime;
            inputList.Add(input);
        }
		else
		{
            
        }
    }

    void OnDataReceived(string message)
    {
        try
        {
            text.text = message;
            input = Int16.Parse(message);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }

    public void OnClick()
	{
        if(pressed == false)
		{
            rec = true;
            Debug.Log("録音中");
            pressed = true;
        }
		else
		{
            rec = false;
            Debug.Log("完了");

            //グラフ表示
            float step = 0.005f;
            Vector3 scale = Vector3.one * step;
            position.z = 0f;
            points = new Transform[inputList.Count - 1];

            lineGroup = new GameObject("LineGroup");
            my2DPoint = new Vector2[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                Transform point = Instantiate(pointPrefab);
                position.x = (i + 0.5f) * step - step * 1000 / 2;
                position.y = (float)inputList[i] / 100;
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

            //wav書出
            string outFilePath = Application.dataPath + "/WavFiles/mic.wav";
            short[] inputArray = inputList.ToArray();
            //bpsとbaudは違うけどarduinoは同じらし？
            short channel = 1;
            //int sampleRate = baudRate / 16;
            Debug.Log(time);
            int sampleRate = (int)(inputArray.Length / time);
            Debug.Log(sampleRate);
            short bitPerSample = 8;
            WavWrite.createWave(inputArray, outFilePath, channel, sampleRate, bitPerSample);

            inputList.Clear();

            pressed = false;
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
}