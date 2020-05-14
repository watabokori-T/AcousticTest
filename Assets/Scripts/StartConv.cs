using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class StartConv : MonoBehaviour
{
    [SerializeField]
    InputField[] inputField;

    void Start()
    {
        for (int i = 0; i < inputField.Length; i++)
        {
            inputField[i] = inputField[i].GetComponent<InputField>();
        }
    }

    public void startCalc()
    {
        string waveFilePath1 = Application.dataPath + inputField[0].text;
        Debug.Log(waveFilePath1);
        if (!File.Exists(waveFilePath1))
        {
            Debug.Log("ファイルどこじゃ？");
        }

        string waveFilePath2 = Application.dataPath + inputField[1].text;
        Debug.Log(waveFilePath2);
        if (!File.Exists(waveFilePath2))
        {
            Debug.Log("ファイルどこじゃ？");
        }

        string outFilePath = Application.dataPath + inputField[2].text;
        Debug.Log(outFilePath);

        //畳込
        Convolution.conv(waveFilePath1, waveFilePath2, outFilePath);
    }
}
