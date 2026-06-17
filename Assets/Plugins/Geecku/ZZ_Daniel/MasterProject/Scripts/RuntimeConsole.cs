using System.Collections.Generic;
using UnityEngine;

public class RuntimeConsole : MonoBehaviour
{
    [SerializeField] private KeyCode ToggleKey = KeyCode.F1;
    [SerializeField] private int MaxLines = 50;

    private struct LogEntry
    {
        public string message;
        public LogType type;
        public string time;
    }

    private List<LogEntry> _logs = new();
    private Vector2 _scrollPos;
    private bool _visible = true;
    private bool _autoScroll = true;

    private GUIStyle _logStyle;
    private GUIStyle _warnStyle;
    private GUIStyle _errorStyle;
    private GUIStyle _boxStyle;

    private readonly Color LogColor = new Color(0.85f, 0.85f, 0.85f);
    private readonly Color WarnColor = new Color(0.95f, 0.80f, 0.25f);
    private readonly Color ErrorColor = new Color(0.90f, 0.30f, 0.30f);

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }
    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string message, string stackTrace, LogType type)
    {
        _logs.Add(new LogEntry
        {
            message = message,
            type = type,
            time = System.DateTime.Now.ToString("HH:mm:ss")
        });

        if (_logs.Count > MaxLines)
            _logs.RemoveAt(0);

        if (_autoScroll)
            _scrollPos.y = float.MaxValue;
    }

    private void OnGUI()
    {
        if (!_visible) return;

        InitStyles();

        float w = 420f;
        float h = 220f;
        float x = Screen.width - w - 10f;
        float y = 10f;

        GUI.Box(new Rect(x - 4, y - 4, w + 8, h + 34), GUIContent.none, _boxStyle);

        // Header
        GUI.Label(new Rect(x, y, 200, 18),
            $"<color=#888>console</color>  <color=#555>{_logs.Count} lines</color>",
            new GUIStyle(GUI.skin.label) { richText = true, fontSize = 11 });

        // Clear button
        if (GUI.Button(new Rect(x + w - 46, y, 46, 18), "clear",
            new GUIStyle(GUI.skin.button) { fontSize = 10 }))
            _logs.Clear();

        // Auto-scroll toggle
        _autoScroll = GUI.Toggle(new Rect(x + w - 140, y, 90, 18), _autoScroll,
            " auto-scroll", new GUIStyle(GUI.skin.toggle) { fontSize = 10 });

        // Scroll view
        var scrollRect = new Rect(x, y + 22, w, h);
        _scrollPos = GUI.BeginScrollView(scrollRect, _scrollPos,
            new Rect(0, 0, w - 16, Mathf.Max(h, _logs.Count * 18)));

        for (int i = 0; i < _logs.Count; i++)
        {
            var entry = _logs[i];
            GUIStyle style = entry.type switch
            {
                LogType.Warning => _warnStyle,
                LogType.Error => _errorStyle,
                LogType.Exception => _errorStyle,
                _ => _logStyle
            };
            GUI.Label(new Rect(0, i * 18, w - 16, 18),
                $"[{entry.time}] {entry.message}", style);
        }

        GUI.EndScrollView();
    }

    private bool _stylesInitialized;
    private void InitStyles()
    {
        if (_stylesInitialized) return;
        _stylesInitialized = true;

        Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        _boxStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = MakeTex(new Color(0.08f, 0.08f, 0.08f, 0.92f)) }
        };

        GUIStyle Base(Color col) => new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            wordWrap = false,
            normal = { textColor = col }
        };

        _logStyle = Base(LogColor);
        _warnStyle = Base(WarnColor);
        _errorStyle = Base(ErrorColor);
    }
}