// Place this file inside any Editor folder in your project, e.g.:
// Assets/Editor/TTSGeneratorWindow.cs
//
// Opens via Unity menu: Tools -> TTS Clip Generator
//
// Requirements:
//   - A Google Cloud API key with Cloud Text-to-Speech API enabled
//   - Three ScriptableObjects, one per language, each with a List<string> field
//     containing the lines in that language
//
// Output:
//   Assets/Audio/TTS/EN/   one .wav per line
//   Assets/Audio/TTS/NL/
//   Assets/Audio/TTS/DE/
//
// IMPORTANT: Never commit your API key to source control.
// Store it only in the Editor Window (it is saved to EditorPrefs on your
// local machine only, never inside any project asset).

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Daniel.Master.Editor
{
    public class TTSGeneratorWindow : EditorWindow
    {
        // ── Menu item ──────────────────────────────────────────────────────
        [MenuItem("Tools/TTS Clip Generator")]
        public static void Open() =>
            GetWindow<TTSGeneratorWindow>("TTS Clip Generator");

        // ── Google Cloud TTS REST endpoint ─────────────────────────────────
        private const string GCP_TTS_URL =
            "https://texttospeech.googleapis.com/v1/text:synthesize?key=";

        // ── Voice names per language
        //    Full list: https://cloud.google.com/text-to-speech/docs/voices
        //    These are high-quality Neural2 voices. Change if you prefer others.
        private static readonly Dictionary<string, (string langCode, string voiceName)> Voices
            = new()
            {
                { "EN", ("en-US", "en-US-Neural2-F") },
                { "NL", ("nl-NL", "nl-NL-Wavenet-A") },
                { "DE", ("de-DE", "de-DE-Neural2-F") },
            };

        // ── Serialised per-language config ────────────────────────────────
        [Serializable]
        private class LangConfig
        {
            public bool            enabled    = true;
            public ScriptableObject asset     = null;
            public string          fieldName  = "";
            public List<string>    fieldNames = new();  // detected on asset change
        }

        private LangConfig configEN = new();
        private LangConfig configNL = new();
        private LangConfig configDE = new();

        // ── Settings ───────────────────────────────────────────────────────
        private string apiKey    = "";
        private bool   showKey   = false;
        private string outputRoot = "Assets/Audio/TTS";

        // Speaking rate and pitch (1.0 = default)
        private float speakingRate = 1.0f;
        private float pitchSemitones = 0f;

        // ── State ──────────────────────────────────────────────────────────
        private bool      isGenerating  = false;
        private string    statusMessage = "";
        private bool      statusIsError = false;
        private Vector2   scroll;

        // ── EditorPrefs key for API key storage ───────────────────────────
        private const string PREFS_API_KEY = "TTSGen_GCP_ApiKey";

        // ── Lifecycle ──────────────────────────────────────────────────────
        private void OnEnable()
        {
            // Load API key from local EditorPrefs (never stored in assets)
            apiKey = EditorPrefs.GetString(PREFS_API_KEY, "");
        }

        private void OnDisable()
        {
            EditorPrefs.SetString(PREFS_API_KEY, apiKey);
        }

        // ── GUI ────────────────────────────────────────────────────────────
        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.Space(8);

            DrawApiKeySection();
            EditorGUILayout.Space(10);
            DrawLanguageSection("English (EN)", configEN, "EN");
            EditorGUILayout.Space(6);
            DrawLanguageSection("Dutch (NL)",   configNL, "NL");
            EditorGUILayout.Space(6);
            DrawLanguageSection("German (DE)",  configDE, "DE");
            EditorGUILayout.Space(10);
            DrawVoiceSettings();
            EditorGUILayout.Space(10);
            DrawOutputSection();
            EditorGUILayout.Space(10);
            DrawGenerateButton();

            if (!string.IsNullOrEmpty(statusMessage))
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.HelpBox(statusMessage,
                    statusIsError ? MessageType.Error : MessageType.Info);
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.EndScrollView();
        }

        // ── API key section ────────────────────────────────────────────────
        private void DrawApiKeySection()
        {
            EditorGUILayout.LabelField("Google Cloud API Key", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Stored in EditorPrefs on this machine only. " +
                "Never saved inside any project file or asset. " +
                "Do NOT paste it into any script or ScriptableObject.",
                MessageType.Warning);

            EditorGUILayout.BeginHorizontal();
            if (showKey)
                apiKey = EditorGUILayout.TextField(apiKey);
            else
                apiKey = EditorGUILayout.PasswordField(apiKey);

            if (GUILayout.Button(showKey ? "Hide" : "Show", GUILayout.Width(46)))
                showKey = !showKey;
            EditorGUILayout.EndHorizontal();
        }

        // ── Per-language section ───────────────────────────────────────────
        private void DrawLanguageSection(string label, LangConfig cfg, string langCode)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            cfg.enabled = EditorGUILayout.Toggle(cfg.enabled, GUILayout.Width(16));
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            // Show chosen voice name as a small hint
            if (Voices.TryGetValue(langCode, out var v))
            {
                GUIStyle hint = new GUIStyle(EditorStyles.miniLabel)
                    { alignment = TextAnchor.MiddleRight };
                EditorGUILayout.LabelField(v.voiceName, hint);
            }
            EditorGUILayout.EndHorizontal();

            if (!cfg.enabled)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            // ScriptableObject slot
            var newAsset = (ScriptableObject)EditorGUILayout.ObjectField(
                "ScriptableObject", cfg.asset, typeof(ScriptableObject), false);

            if (newAsset != cfg.asset)
            {
                cfg.asset = newAsset;
                RefreshFields(cfg);
            }

            // Field picker (only shown when asset is assigned)
            if (cfg.asset != null)
            {
                if (cfg.fieldNames.Count > 0)
                {
                    int idx = Mathf.Max(0, cfg.fieldNames.IndexOf(cfg.fieldName));
                    idx = EditorGUILayout.Popup("List<string> field", idx,
                        cfg.fieldNames.ToArray());
                    cfg.fieldName = cfg.fieldNames[idx];

                    // Preview: show how many lines are detected
                    var lines = GetLines(cfg);
                    EditorGUILayout.LabelField(
                        $"{lines.Count} line{(lines.Count == 1 ? "" : "s")} detected",
                        EditorStyles.miniLabel);
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "No public List<string> fields found on this asset.",
                        MessageType.Warning);
                }
            }

            EditorGUILayout.EndVertical();
        }

        // ── Voice settings ─────────────────────────────────────────────────
        private void DrawVoiceSettings()
        {
            EditorGUILayout.LabelField("Voice Settings", EditorStyles.boldLabel);
            speakingRate  = EditorGUILayout.Slider("Speaking rate",    speakingRate,  0.25f, 4.0f);
            pitchSemitones = EditorGUILayout.Slider("Pitch (semitones)", pitchSemitones, -20f, 20f);
        }

        // ── Output folder ──────────────────────────────────────────────────
        private void DrawOutputSection()
        {
            EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
            outputRoot = EditorGUILayout.TextField("Root folder", outputRoot);
            EditorGUILayout.HelpBox(
                $"Clips saved to:\n" +
                $"  {outputRoot}/EN/\n" +
                $"  {outputRoot}/NL/\n" +
                $"  {outputRoot}/DE/",
                MessageType.None);
        }

        // ── Generate button ────────────────────────────────────────────────
        private void DrawGenerateButton()
        {
            bool anyEnabled = (configEN.enabled && configEN.asset != null) ||
                              (configNL.enabled && configNL.asset != null) ||
                              (configDE.enabled && configDE.asset != null);

            bool canGenerate = !isGenerating
                            && !string.IsNullOrEmpty(apiKey)
                            && anyEnabled;

            GUI.enabled = canGenerate;
            if (GUILayout.Button("Generate Clips", GUILayout.Height(38)))
                Generate();
            GUI.enabled = true;
        }

        // ── Reflection helpers ─────────────────────────────────────────────
        private void RefreshFields(LangConfig cfg)
        {
            cfg.fieldNames.Clear();
            cfg.fieldName = "";
            if (cfg.asset == null) return;

            foreach (FieldInfo fi in cfg.asset.GetType().GetFields(
                         BindingFlags.Public | BindingFlags.Instance))
            {
                if (fi.FieldType == typeof(List<string>))
                    cfg.fieldNames.Add(fi.Name);
            }

            if (cfg.fieldNames.Count > 0)
                cfg.fieldName = cfg.fieldNames[0];
        }

        private List<string> GetLines(LangConfig cfg)
        {
            if (cfg.asset == null || string.IsNullOrEmpty(cfg.fieldName))
                return new List<string>();

            FieldInfo fi = cfg.asset.GetType().GetField(cfg.fieldName,
                BindingFlags.Public | BindingFlags.Instance);

            if (fi == null) return new List<string>();

            return fi.GetValue(cfg.asset) as List<string> ?? new List<string>();
        }

        // ── Generation ─────────────────────────────────────────────────────
        private void Generate()
        {
            isGenerating  = true;
            statusMessage = "Starting...";
            statusIsError = false;
            Repaint();

            // Collect work items
            var jobs = new List<(LangConfig cfg, string langCode)>
            {
                (configEN, "EN"),
                (configNL, "NL"),
                (configDE, "DE"),
            };

            // Run via EditorCoroutine pattern (plain coroutine on EditorApplication.update)
            EditorCoroutineRunner.Start(GenerateCoroutine(jobs));
        }

        private IEnumerator GenerateCoroutine(
            List<(LangConfig cfg, string langCode)> jobs)
        {
            int totalDone  = 0;
            int totalFailed = 0;

            foreach (var (cfg, langCode) in jobs)
            {
                if (!cfg.enabled || cfg.asset == null) continue;

                List<string> lines = GetLines(cfg);
                if (lines.Count == 0) continue;

                // Ensure output folder exists
                string assetFolder = $"{outputRoot}/{langCode}";
                string absFolder   = Path.GetFullPath(
                    Path.Combine(Application.dataPath, "..",
                                 assetFolder));
                Directory.CreateDirectory(absFolder);

                var voice = Voices[langCode];

                for (int i = 0; i < lines.Count; i++)
                {
                    string line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // File name: zero-padded index + first 40 chars of text
                    string safeName = $"{i:D4}_{Sanitise(line, 40)}";
                    string filePath = Path.Combine(absFolder, safeName + ".wav");

                    // Update progress bar
                    float progress = (float)totalDone /
                        (GetTotalLines() + 0.001f);
                    bool cancelled = EditorUtility.DisplayCancelableProgressBar(
                        "Generating TTS clips",
                        $"[{langCode}] {i + 1}/{lines.Count}: {line.Substring(0, Mathf.Min(40, line.Length))}...",
                        progress);

                    if (cancelled)
                    {
                        SetStatus("Generation cancelled.", false);
                        EditorUtility.ClearProgressBar();
                        isGenerating = false;
                        yield break;
                    }

                    // Build JSON request body
                    string json = BuildRequestJson(line, voice.langCode,
                        voice.voiceName, speakingRate, pitchSemitones);

                    // Send request
                    var request = new UnityWebRequest(
                        GCP_TTS_URL + apiKey, "POST");
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                    request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");

                    var op = request.SendWebRequest();
                    while (!op.isDone) yield return null;

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError(
                            $"[TTSGen] Failed [{langCode}] line {i}: " +
                            $"{request.error}\n{request.downloadHandler.text}");
                        totalFailed++;
                    }
                    else
                    {
                        // Response is JSON with a base64 "audioContent" field
                        string responseJson = request.downloadHandler.text;
                        byte[] wavBytes     = ExtractAudioBytes(responseJson);

                        if (wavBytes != null && wavBytes.Length > 0)
                        {
                            File.WriteAllBytes(filePath, wavBytes);
                            totalDone++;
                        }
                        else
                        {
                            Debug.LogError(
                                $"[TTSGen] Empty audio [{langCode}] line {i}.");
                            totalFailed++;
                        }
                    }

                    request.Dispose();

                    // Small delay to stay within API rate limits
                    double waitUntil = EditorApplication.timeSinceStartup + 0.05;
                    while (EditorApplication.timeSinceStartup < waitUntil)
                        yield return null;
                }
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();

            string summary = $"Done. {totalDone} clip(s) generated" +
                             (totalFailed > 0 ? $", {totalFailed} failed (see Console)." : ".");
            SetStatus(summary, totalFailed > 0);
            isGenerating = false;
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private int GetTotalLines()
        {
            int total = 0;
            if (configEN.enabled) total += GetLines(configEN).Count;
            if (configNL.enabled) total += GetLines(configNL).Count;
            if (configDE.enabled) total += GetLines(configDE).Count;
            return Mathf.Max(total, 1);
        }

        private void SetStatus(string msg, bool isError)
        {
            statusMessage = msg;
            statusIsError = isError;
            Repaint();
        }

        // Build the Google Cloud TTS JSON request body
        private static string BuildRequestJson(
            string text, string languageCode, string voiceName,
            float rate, float pitch)
        {
            // Escape special characters for JSON
            string escaped = text
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r");

            return $@"{{
  ""input"": {{ ""text"": ""{escaped}"" }},
  ""voice"": {{
    ""languageCode"": ""{languageCode}"",
    ""name"": ""{voiceName}""
  }},
  ""audioConfig"": {{
    ""audioEncoding"": ""LINEAR16"",
    ""speakingRate"": {rate.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},
    ""pitch"": {pitch.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}
  }}
}}";
        }

        // Extract base64 audio from GCP response and decode to bytes
        private static byte[] ExtractAudioBytes(string json)
        {
            // Response format: { "audioContent": "<base64>" }
            const string key = "\"audioContent\":";
            int keyIdx = json.IndexOf(key, StringComparison.Ordinal);
            if (keyIdx < 0) return null;

            int start = json.IndexOf('"', keyIdx + key.Length) + 1;
            int end   = json.IndexOf('"', start);
            if (start <= 0 || end <= start) return null;

            string base64 = json.Substring(start, end - start);
            return Convert.FromBase64String(base64);
        }

        // Sanitise a string for use as a filename
        private static string Sanitise(string text, int maxLength)
        {
            var sb = new StringBuilder();
            foreach (char c in text)
            {
                if (char.IsLetterOrDigit(c) || c == '_') sb.Append(c);
                else if (c == ' ' && sb.Length > 0 && sb[sb.Length - 1] != '_') sb.Append('_');
                if (sb.Length >= maxLength) break;
            }
            return sb.ToString().TrimEnd('_');
        }
    }

    // ── Minimal Editor coroutine runner ───────────────────────────────────
    // Drives IEnumerator coroutines on EditorApplication.update so we can
    // yield on UnityWebRequest without entering Play Mode.
    internal static class EditorCoroutineRunner
    {
        private static IEnumerator _current;

        public static void Start(IEnumerator coroutine)
        {
            _current = coroutine;
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }

        private static void Tick()
        {
            if (_current == null || !_current.MoveNext())
            {
                EditorApplication.update -= Tick;
                _current = null;
            }
        }
    }
}
