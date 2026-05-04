using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor window that reads a .txt file (one sentence per line) and creates
/// a NarrationScriptData ScriptableObject asset populated with those lines.
///
/// Open via:  Tools > TTS > Create Narration Script Data from .txt
/// </summary>
public class NarrationScriptImporter : EditorWindow
{
    // ── State ──────────────────────────────────────────────────────────────
    private string _sourcePath       = "";
    private string _assetSavePath    = "Assets";
    private string _assetName        = "NarrationScriptData";
    private Vector2 _previewScroll;
    private string[] _previewLines   = System.Array.Empty<string>();
    private bool    _trimWhitespace  = true;
    private bool    _skipEmptyLines  = true;

    // ── Menu item ──────────────────────────────────────────────────────────
    [MenuItem("Tools/TTS/Create Narration Script Data from .txt")]
    public static void ShowWindow()
    {
        var win = GetWindow<NarrationScriptImporter>("TTS Importer");
        win.minSize = new Vector2(420, 400);
    }

    // ── GUI ────────────────────────────────────────────────────────────────
    private void OnGUI()
    {
        GUILayout.Label("TTS Narration Script Importer", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        // ── Source file ──
        EditorGUILayout.LabelField("1. Source .txt file", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        _sourcePath = EditorGUILayout.TextField(_sourcePath);
        if (GUILayout.Button("Browse…", GUILayout.Width(70)))
        {
            string picked = EditorUtility.OpenFilePanel("Select narration text file", "", "txt");
            if (!string.IsNullOrEmpty(picked))
            {
                _sourcePath = picked;
                RefreshPreview();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);

        // ── Options ──
        EditorGUILayout.LabelField("2. Parse options", EditorStyles.boldLabel);
        _trimWhitespace = EditorGUILayout.Toggle("Trim whitespace per line", _trimWhitespace);
        _skipEmptyLines = EditorGUILayout.Toggle("Skip empty lines",         _skipEmptyLines);

        if (GUILayout.Button("Refresh Preview"))
            RefreshPreview();

        EditorGUILayout.Space(8);

        // ── Preview ──
        if (_previewLines.Length > 0)
        {
            EditorGUILayout.LabelField($"Preview  ({_previewLines.Length} lines)", EditorStyles.boldLabel);
            _previewScroll = EditorGUILayout.BeginScrollView(_previewScroll,
                GUILayout.Height(Mathf.Min(_previewLines.Length * 20 + 8, 160)));

            GUI.enabled = false;
            foreach (var line in _previewLines)
                EditorGUILayout.TextField(line);
            GUI.enabled = true;

            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space(8);
        }

        // ── Output asset ──
        EditorGUILayout.LabelField("3. Output asset", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Save folder (inside Assets/):", GUILayout.Width(180));
        _assetSavePath = EditorGUILayout.TextField(_assetSavePath);
        if (GUILayout.Button("Pick…", GUILayout.Width(50)))
        {
            string picked = EditorUtility.SaveFolderPanel(
                "Choose folder for asset", Application.dataPath, "");

            // Convert absolute path → relative Assets/ path
            if (!string.IsNullOrEmpty(picked) && picked.StartsWith(Application.dataPath))
                _assetSavePath = "Assets" + picked.Substring(Application.dataPath.Length);
        }
        EditorGUILayout.EndHorizontal();

        _assetName = EditorGUILayout.TextField("Asset name", _assetName);

        EditorGUILayout.Space(12);

        // ── Create button ──
        bool canCreate = !string.IsNullOrEmpty(_sourcePath)
                      && File.Exists(_sourcePath)
                      && _previewLines.Length > 0
                      && !string.IsNullOrEmpty(_assetName);

        GUI.enabled = canCreate;
        if (GUILayout.Button("Create ScriptableObject Asset", GUILayout.Height(36)))
            CreateAsset();
        GUI.enabled = true;

        if (!canCreate && !string.IsNullOrEmpty(_sourcePath))
            EditorGUILayout.HelpBox(
                File.Exists(_sourcePath)
                    ? "No lines parsed — check parse options or file content."
                    : "File not found. Please browse for a valid .txt file.",
                MessageType.Warning);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private void RefreshPreview()
    {
        if (string.IsNullOrEmpty(_sourcePath) || !File.Exists(_sourcePath))
        {
            _previewLines = System.Array.Empty<string>();
            return;
        }

        _previewLines = ParseFile(_sourcePath);
        Repaint();
    }

    private string[] ParseFile(string path)
    {
        var allLines = File.ReadAllLines(path);

        return allLines
            .Select(l => _trimWhitespace ? l.Trim() : l)
            .Where(l => !_skipEmptyLines || !string.IsNullOrEmpty(l))
            .ToArray();
    }

    private void CreateAsset()
    {
        // Parse lines fresh
        string[] lines = ParseFile(_sourcePath);
        if (lines.Length == 0)
        {
            EditorUtility.DisplayDialog("TTS Importer", "No lines to import.", "OK");
            return;
        }

        // Ensure save folder exists inside the project
        if (!AssetDatabase.IsValidFolder(_assetSavePath))
        {
            // Try to create it
            string parent = Path.GetDirectoryName(_assetSavePath).Replace('\\', '/');
            string leaf   = Path.GetFileName(_assetSavePath);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        // Create the ScriptableObject
        var asset = ScriptableObject.CreateInstance<NarrationScriptData>();
        asset.lines.AddRange(lines);

        string assetPath = AssetDatabase.GenerateUniqueAssetPath(
            $"{_assetSavePath}/{_assetName}.asset");

        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Ping the new asset in the Project window
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);

        EditorUtility.DisplayDialog("TTS Importer",
            $"Created asset with {lines.Length} lines:\n{assetPath}", "OK");
    }
}
