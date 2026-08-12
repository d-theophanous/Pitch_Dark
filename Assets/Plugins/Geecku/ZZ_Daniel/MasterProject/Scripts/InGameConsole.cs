using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Attach to a persistent GameObject (e.g. a manager object that survives scene loads).
/// Toggles an in-game overlay showing Unity console output — works in builds.
/// </summary>
public class InGameConsole : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private int maxLines = 100;
    [SerializeField] private int fontSize = 16;
    [SerializeField] private bool pauseTimeWhenOpen = false;

    private bool isVisible = false;
    private readonly List<LogEntry> logEntries = new List<LogEntry>();
    private Vector2 scrollPosition;
    private GUIStyle logStyle;
    private GUIStyle backgroundStyle;

    private struct LogEntry
    {
        public string message;
        public LogType type;
    }

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    public void ToggleConsole()
    {
        isVisible = !isVisible;

        if (pauseTimeWhenOpen)
        {
            Time.timeScale = isVisible ? 0f : 1f;
        }
    }

    private void HandleLog(string message, string stackTrace, LogType type)
    {
        logEntries.Add(new LogEntry { message = message, type = type });

        if (logEntries.Count > maxLines)
        {
            logEntries.RemoveAt(0);
        }

        // Auto-scroll to bottom on new message
        scrollPosition.y = float.MaxValue;
    }

    private void EnsureStyles()
    {
        if (logStyle == null)
        {
            logStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                wordWrap = true
            };
        }

        if (backgroundStyle == null)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.8f));
            tex.Apply();
            backgroundStyle = new GUIStyle { normal = { background = tex } };
        }
    }

    private void OnGUI()
    {
        if (!isVisible) return;

        EnsureStyles();

        float width = Screen.width * 0.9f;
        float height = Screen.height * 0.5f;
        Rect windowRect = new Rect((Screen.width - width) / 2f, 20f, width, height);

        GUI.Box(windowRect, GUIContent.none, backgroundStyle);

        GUILayout.BeginArea(windowRect);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(width), GUILayout.Height(height));

        StringBuilder sb = new StringBuilder();
        foreach (var entry in logEntries)
        {
            Color color = entry.type switch
            {
                LogType.Error or LogType.Exception => Color.red,
                LogType.Warning => Color.yellow,
                _ => Color.white
            };

            logStyle.normal.textColor = color;
            GUILayout.Label(entry.message, logStyle);
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}