using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    InputField[] inputField;

    [SerializeField]
    string[] fileName;

    [SerializeField]
    Toggle toggle;

    void Start()
    {
        for (int i = 0; i < inputField.Length; i++)
		{
            inputField[i] = inputField[i].GetComponent<InputField>();
        }

        toggle = toggle.GetComponent<Toggle>();
    }

    public void OnValueChanged()
    {
        if (toggle.isOn == true)
        {
            for (int i = 0; i < inputField.Length; i++)
			{
                inputField[i].text = fileName[i];
			}
        }
        else
        {
            for (int i = 0; i < inputField.Length; i++)
			{
                inputField[i].text = "";
            }
        }
    }
}
