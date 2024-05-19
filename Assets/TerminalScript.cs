using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class VRDebugConsole : MonoBehaviour
{
    public TextMeshProUGUI debugText;
    private string log;

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        log += logString + "\n";
        debugText.text = log;
    }
}