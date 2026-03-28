using Geecku.DefaultEngine.Common;
using Geecku.DefaultNetworking;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Geecku.Defines;

namespace Geecku.GlobalMangers
{
    public class Engine : PersistantDSingleton<Engine>
    {
        [PropertySpace(SpaceAfter = 10),
            InfoBox("Version is not set yet.", InfoMessageType.Info, VisibleIf = "@this.Version.IsEmpty"),
            InfoBox("Main is zero. For status Released this should not be the case", InfoMessageType.Warning, VisibleIf = "@this.Version.Main == 0 && this.Version.Status == DVersionStatus.Release")
            ]
        public DVersion Version;

        #region Random
        public static System.Random Random;
        private const int RandomSeed = 245;
        #endregion

        #region Trash
        [FoldoutGroup("Links", GroupName = "Linked Objects", Expanded = true), Space(2)]
        [SerializeField] private Transform _TrashBucket;
        public static Transform TrashBucket => Instance != null ? Instance._TrashBucket : null;
        #endregion

        #region Editor UI
        //[FoldoutGroup("Links"), PropertySpace(SpaceBefore = 2, SpaceAfter = 1), AssetSelector(Paths = PATH_PREFABS)]
        //[SerializeField] protected Borodar.RainbowHierarchy.HierarchyRulesetV2 Ruleset;
        #endregion

        #region Log
        [FoldoutGroup("Links"), PropertySpace(SpaceBefore = 1, SpaceAfter = 6), AssetSelector(Paths = PATH_SCRIPTABLE_OBJECTS + "/Logs/")]
        [SerializeField] private LoggerHolder LogInfo;
        #endregion

        #region Scenes
        #pragma warning disable CS0414
        [FoldoutGroup("Scenes", GroupName = "Scene Names", Expanded = true), PropertySpace(SpaceBefore = 3, SpaceAfter = 1)]
        [SerializeField] private string TemplateScene = "Template Scene";
        [FoldoutGroup("Scenes"), PropertySpace(SpaceBefore = 1, SpaceAfter = 1)]
        [SerializeField] private string NetworkScene = "Network Scene";
        [FoldoutGroup("Scenes"), PropertySpace(SpaceBefore = 6, SpaceAfter = 3)]
        [SerializeField, Tooltip("Must include namespace path")] private string GameMessagesPath;
        [SerializeField] private bool _LoadNetworkScene = true;
        public static bool LoadNetworkScene => Instance._LoadNetworkScene;
        #pragma warning restore CS0414

        public static string GetCurScene() => SceneManager.GetActiveScene().name;
        public static string CurScene;
        public static void LoadScene(string scene_name)
        {
            SceneManager.LoadScene(scene_name);
        }
        public static void UnloadScene(string scene_name)
        {
            SceneManager.UnloadSceneAsync(scene_name);
        }
        public static void LoadSceneAdditive(string scene_name)
        {
            SceneManager.LoadScene(scene_name, LoadSceneMode.Additive);
        }
        public static void SwitchScene(string scene_name)
        {
            LoadSceneAdditive(scene_name);
            if (CurScene == null)
                SceneManager.UnloadSceneAsync(GetCurScene());
            else
                SceneManager.UnloadSceneAsync(CurScene);
            CurScene = scene_name;
        }
        #endregion

        protected override void Awake()
        {
            base.Awake();
            if (WillBeDestroyed)
                return;

            #region Random
            Random = new(RandomSeed);
            #endregion

            #region Log
            LoadLog();
            Application.logMessageReceived += OnLog;
            foreach (var item in StartLog)
                item.Item2?.Invoke(item.Item1);
            StartLog.Clear();
            #endregion

            #region Networking
            //- Here you must change the "DefaultNetworking.Template.GameMessage" type to your GameMessage class
            //NetworkManager.GameMessageClassType = typeof(DefaultNetworking.Template.GameMessageTemplate);
            //NetworkManager.GameMessageClassType = typeof(NetworkTest.Networking.GameMessageTemplate);
            //NetworkManager.GameMessageClassType = typeof(Daniel.Networking.GameMessages);
            //NetworkManager.GameMessageClassType = typeof(Games.SimpleCardBattler.Networking.GameMessages);
            NetworkManager.GameMessageClassType = Type.GetType(GameMessagesPath);
            //NetworkManager.GameMessageClassType = Type.GetType("Games.SimpleCardBattler.Networking.GameMessages");

            if (NetworkManager.GameMessageClassType == null)
                Debug.LogWarning("GameMessagesClass is undefined. Can be defined by GameMessagesPath");
            #endregion

            Engine.LogEngine("Loaded [" + Version + "]");

            if (!LoadNetworkScene || NetworkManager.Instance != null)
                return;
            SceneManager.LoadScene(NetworkScene, UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }
        protected override void Start()
        {
            base.Start();
            if (WillBeDestroyed)
                return;

            //var ruleset = Instantiate(Ruleset, transform);
            //ruleset.name = "Rainbow Ruleset";

            Engine.LogEngine("Started");
            WriteLog();
        }

        #region Logs
        private delegate void Write(object msg);
        private static readonly List<(object, Write)> StartLog = new();
        private static int Counter = 0;
        
        private static bool ErrorLog(int index, object msg)
        {
            if (index < 0 || index >= Instance.Loggers.Count)
            {
                Debug.LogError("Could not log '" + msg + "' because log-index " + index +" was not registered properly in Loggers [" + Instance.Loggers.Count + "].");
                Counter++;
                return true;
            }
            return false;
        }
        private static void SetLog(int index, object message, Write log_methode)
        {
            string name_of_methode = log_methode.Method.Name;
            bool search_for_log = index < 0 || index >= Instance.Loggers.Count;
            if (!search_for_log)
            {
                var v = Instance.Loggers[index];
                if ("Log" + v.Name != name_of_methode)
                    search_for_log = true;
            }
            if (search_for_log)
                for (int i = 0; i < Instance.Loggers.Count; i++)
                {
                    var log = Instance.Loggers[i];
                    if ("Log" + log.Name == name_of_methode)
                    {
                        index = i;
                        if (log.On)
                            Debug.LogWarning("Log-Index was not set correct for " + name_of_methode + ". But I've found it in the list anyway.");
                        break;
                    }
                }
            if (ErrorLog(index, message))
                return;

            var logger = Instance.Loggers[index];
            string msg = (Instance.LogInfo.LogCounter ? Counter + ": " : "") + "<color=" + logger.StringColor + ">" + logger.Symbol + "</color>: " + message;
            if (SafeLog(message, log_methode) && logger.On)
                Debug.Log(msg);
            else
                WriteToLog(msg);
            Counter++;
        }
        public static void Log(object message)
        {
            const int logger_list_index = 0;
            SetLog(logger_list_index, message, Log);
        }
        public static void LogEngine(object message)
        {
            const int logger_list_index = 1;
            SetLog(logger_list_index, message, LogEngine);
        }
        //- Loggers #LOGGER_METHODES
        public static void LogClient(object message) //LOGINDEX = 2
        {
            const int logger_list_index = 2;
            SetLog(logger_list_index, message, LogClient);
        }
        public static void LogServer(object message) //LOGINDEX = 3
        {
            const int logger_list_index = 3;
            SetLog(logger_list_index, message, LogServer);
        }
        public static void LogNetwork(object message) //LOGINDEX = 4
        {
            const int logger_list_index = 4;
            SetLog(logger_list_index, message, LogNetwork);
        }
        
        private static bool SafeLog(object message, Write write)
        {
            if (Instance == null)
                StartLog.Add((message, write));
            return Instance != null;
        }
        private void InjectLogger(string name, int logger_index)
        {
            if (!InitializedLogger)
                return;

            const string logger_insert_string = "#LOGGER_METHODES";
            const int num_of_codelines = 5;
            int num_of_logger = Loggers.Count - 2;
            #if UNITY_EDITOR
            string path = PATH_SCRIPTS + "/GlobalManagers/Engine.cs"; // oder wo deine Datei liegt
            string[] lines = File.ReadAllLines(path);

            int inset_index = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(logger_insert_string))
                {
                    inset_index = i + 1;
                    break;
                }
            }

            if (inset_index == -1)
            {
                Debug.LogError("Insert position '"+ logger_insert_string + "' was not found in DEngine.cs");
                return;
            }
            inset_index += num_of_logger * num_of_codelines;

            string methode_name = $"Log{name}";
            foreach (string line in lines)
            {
                if (line.Contains(methode_name))
                {
                    Debug.LogWarning("Logger with name '" + methode_name + "' is already inside DEngine.cs");
                    return;
                }
            }

            // Code-Snippet vorbereiten
            string code = $@"        public static void {methode_name}(object message) //LOGINDEX = {logger_index}
        {{
            const int logger_list_index = {logger_index};
            SetLog(logger_list_index, message, {methode_name});
        }}";

            // An geeigneter Stelle einf�gen
            var new_lines = new List<string>(lines);
            new_lines.Insert(inset_index, code);

            File.WriteAllLines(path, new_lines);

            UnityEditor.AssetDatabase.Refresh(); // damit Unity neu kompiliert
            UnityEditor.EditorApplication.delayCall += () =>
            {
                Debug.Log($"Logger '{name}' was added successfully.");
            };
            #endif
        }

        [HideInInspector] private bool InitializedLogger => LogHolder != null;

        //[PropertyOrder(1), Title("Active Logs"), Button("<< Init Logger System >>", ButtonSizes.Large), InfoBox("Logger System is not yet initialized.", InfoMessageType.Error), ShowIf("@!this.InitializedLogger")]
        //private void InitLoggers()
        //{
        //    //InitializedLogger = true;
        //    //Loggers.Clear();
        //    //Loggers.Add(new Logger() { Name = "Default", On = true, Symbol = "", Color = Color.white });
        //    //Loggers.Add(new Logger() { Name = "Engine", On = true, Symbol = "Engine", Color = Color.red });
        //}
        //[ContextMenu("Reset Logger")]
        //private void ResetLoggers()
        //{
        //    //InitializedLogger = false;
        //    //Loggers.Clear();
        //}

        //[PropertyOrder(2), SerializeField, InfoBox("LogHolder is not yet set. System is not initialized.", InfoMessageType.Error), Title("Default Asset Path", HorizontalLine =false, Bold = false), ShowIf("@!this.InitializedLogger"), HideLabel, FoldoutGroup("Logger Adder")]
        //private string LogAssetPath = "Assets/Plugins/Geecku/DefaultEngine/Runtime/Prefabs/";
        [PropertyOrder(2), HideLabel, ShowInInspector, InlineProperty, InlineEditor(DrawHeader = false, Expanded = true), FoldoutGroup("Logger Adder", expanded: true, GroupName = "Register Logger"), InfoBox("LogHolder is not yet set. System is not initialized.", InfoMessageType.Error, VisibleIf = "@!this.InitializedLogger")]
        private LoggerHolder LogHolder { get => LogInfo; set => LogInfo = value; }

        private List<Logger> Loggers => LogHolder.Loggers;
        [PropertyOrder(3), SerializeField, ShowIf("@this.InitializedLogger"), FoldoutGroup("Logger Adder"), HorizontalGroup("Logger Adder/logger_g"), HideLabel]
        private string LogName;
        [PropertyOrder(4), SerializeField, ShowIf("@this.InitializedLogger"), FoldoutGroup("Logger Adder"), HorizontalGroup("Logger Adder/logger_g"), LabelText("Symbol"), LabelWidth(60)]
        private string LogSymbol;
        [PropertyOrder(5), SerializeField, ShowIf("@this.InitializedLogger"), FoldoutGroup("Logger Adder"), HorizontalGroup("Logger Adder/logger_g"), LabelText("Color"), LabelWidth(38)]
        private Color LogColor;
        [PropertyOrder(6), ShowIf("@this.InitializedLogger"), Button("Register new Logger", ButtonSizes.Medium), EnableIf("@this.ValideNewLogger && this.InitializedLogger && !string.IsNullOrEmpty(this.LogName) && !string.IsNullOrWhiteSpace(this.LogName)"), FoldoutGroup("Logger Adder")]
        private void AddNewLogger()
        {
            if (!ValideNewLogger || string.IsNullOrEmpty(LogName) || string.IsNullOrWhiteSpace(LogName))
            {
                Debug.LogError("Can not add, an empty named logger.");
                return;
            }
            var logger = new Logger
            {
                Name = LogName,
                Symbol = LogSymbol,
                Color = LogColor,
                Holder = LogHolder
            };
            InjectLogger(logger.Name, Loggers.Count);
            Loggers.Add(logger);

            LogName = "";
            LogSymbol = "";
            LogColor = Color.white;
        }

        private bool ValideNewLogger
        {
            get
            {
                if (!InitializedLogger)
                    return false;
                foreach (var log in Loggers)
                    if (log.Name == LogName || log.Symbol == LogSymbol)
                        return false;
                return true;
            }
        }

        [Serializable]
        public class Logger
        {
            private const string DontDelete1 = "Default";
            private const string DontDelete2 = "Engine";

            [TableColumnWidth(22, false)]
            public bool On;
            [ReadOnly]
            public string Name;
            [TableColumnWidth(70, false), GUIColor("$Color")]
            public string Symbol;
            [TableColumnWidth(65, false)]
            public Color Color;
            [Button, VerticalGroup("Actions"), TableColumnWidth(60, false), EnableIf("@this.Name != \""+ DontDelete1 + "\" && this.Name != \"" + DontDelete2 + "\"")]
            private void Delete()
            {
                if (Holder == null)
                    return;
                const int num_of_codelines = 5;
                #if UNITY_EDITOR
                string path = PATH_SCRIPTS + "/GlobalManagers/Engine.cs"; // oder wo deine Datei liegt
                string[] lines = File.ReadAllLines(path);

                int index = -1;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains($"Log{Name}(object message) //LOGINDEX"))
                    {
                        index = i;
                        break;
                    }
                }

                if (index == -1)
                {
                    Debug.LogError("Methode Name was not found in DEngine.cs (Log" + Name + ")");
                    Holder.Loggers.Remove(this);
                    return;
                }
                var new_lines = new List<string>(lines);
                new_lines.RemoveRange(index, num_of_codelines);

                File.WriteAllLines(path, new_lines);

                Holder.Loggers.Remove(this);
                UnityEditor.AssetDatabase.Refresh();
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    Debug.Log($"Logger Log'{Name}' was successfully removed.");
                };
                #endif
            }
            [HideInInspector]
            public LoggerHolder Holder;

            public string StringColor
            {
                get
                {
                    Color32 color32 = Color;
                    return $"#{color32.r:X2}{color32.g:X2}{color32.b:X2}";
                }
            }
        }
        #endregion

        #region Logger
        private void LoadLog()
        {
            const string log_folder = "default_logs";   //- should be integrated in GitIgnore
            DateTime now = DateTime.Now;
            if (!Directory.Exists(Environment.CurrentDirectory + @"\" + log_folder))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\" + log_folder);

            Lines = new List<string>();
            string date = now.Day + "-" + now.Month + "-" + now.Year + "_" + now.Hour + "-" + now.Minute + "-" + now.Second;
            File.Create(LogFile = Environment.CurrentDirectory + @"\" + log_folder + @"\log_" + date + ".txt");
        }
        private string ConvertString(string msg)
        {
            return msg.Replace("\n", "\n\t");
        }
        private static void WriteLog()
        {
            using StreamWriter writer = new(LogFile);
            foreach (var item in Lines)
                writer.WriteLine(item);
        }
        private static string LogFile;
        private static List<string> Lines = new();
        private static void WriteToLog(string msg)
        {
            Lines.Add("HLog>> " + RemoveHTML(msg));
            Lines.Add("");
        }
        private void OnLog(string condition, string stack_trace, LogType type)
        {
            Lines.Add("MLog>> " + RemoveHTML(condition));
            Lines.Add("\t" + ConvertString(stack_trace));

            //- H-Console
            //HiddenConsoleScript console = GetComponentInChildren<HiddenConsoleScript>();
            //console?.WriteToConsole(condition);
        }

        private static string RemoveHTML(string msg)
        {
            return RemoveHTMLCode(msg, 0);
        }
        private static string RemoveHTMLCode(string msg, int index)
        {
            const char left = '<';
            const char right = '>';

            int left_index = -1;
            for (int i = index; i < msg.Length; i++)
            {
                if (msg[i] == left)
                {
                    left_index = i;
                    break;
                }
            }
            int right_index = -1;
            for (int i = left_index + 1; i < msg.Length; i++)
            {
                if (msg[i] == right)
                {
                    right_index = i;
                    break;
                }
            }
            if (left_index != -1 && right_index != -1)
            {
                return RemoveHTMLCode(msg.Remove(left_index, Mathf.Abs(right_index - left_index) + 1), left_index);
            }
            return msg;
        }
        #endregion

        #region Quit
        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            Quit();
        }
        public static void ForceQuit()
        {
            Debug.Log("Force Quit");
            WriteLog();
            Application.Quit();
        }
        public static void Quit()
        {
            Debug.Log("Quit");
            WriteLog();
        }
        #endregion
    }
}
