using UnityEngine;
using System.Collections.Generic;

namespace Daniel.Master
{

public class ScreenLogger : MonoBehaviour
{
    private List<string> messages = new List<string>();
    private GUIStyle style;

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string message, string stackTrace, LogType type)
    {
        string color = type == LogType.Error || type == LogType.Exception ? "red" :
                       type == LogType.Warning ? "yellow" : "white";

        messages.Add($"<color={color}>{message}</color>");

        if (messages.Count > 20) // keep last 20 messages
            messages.RemoveAt(0);
    }

    void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle();
            style.fontSize = 20;
            style.richText = true;
        }

        for (int i = 0; i < messages.Count; i++)
        {
            GUI.Label(new Rect(10, 10 + i * 25, Screen.width - 10, 30), messages[i], style);
        }
    }
    }
}