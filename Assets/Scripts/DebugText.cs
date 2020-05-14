using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugText : MonoBehaviour {
    TextMesh textMesh;

    [SerializeField]
    int textLength = 500;
	
    // Use this for initialization
	void Start () {
        textMesh = gameObject.GetComponentInChildren<TextMesh>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnEnable()
    {
        Application.logMessageReceived += LogMessage;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= LogMessage;
    }

    public void LogMessage(string message, string stackTrace, LogType type)
    {
        if(textMesh.text.Length > textLength)
        {
            textMesh.text = message + "\n";
        }
        else
        {
            textMesh.text += message + "\n";
        }
    }
}
