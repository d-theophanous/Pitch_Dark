using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Geecku.GlobalMangers
{
    [CreateAssetMenu(fileName = "LoggerHolder", menuName = "Scriptable Objects/LoggerHolder")]
    public class LoggerHolder : SerializedScriptableObject
    {
        [SerializeField] public bool LogCounter = true;
        [SerializeField, ReadOnly, HideInInlineEditors] private bool IsLoaded;
        [TableList(AlwaysExpanded = true, IsReadOnly = true, HideToolbar = true)]
        public List<Engine.Logger> Loggers;

        private void Awake()
        {
            if (IsLoaded)
                return;
            IsLoaded = true;
            Loggers = new List<Engine.Logger>();
            Loggers.Clear();
            Loggers.Add(new Engine.Logger() { Name = "Default", On = true, Symbol = "", Color = Color.white });
            Loggers.Add(new Engine.Logger() { Name = "Engine", On = true, Symbol = "Engine", Color = new Color(0.6f, 0.15f, 0.15f) });
        }

        [Button("Add Networks"), HideInInlineEditors]
        private void AddNetwork()
        {
            Loggers.Add(new Engine.Logger() { Name = "Client", On = false, Symbol = "Client", Color = new Color(0.3334018f, 0.5974842f, 0.3237939f) });
            Loggers.Add(new Engine.Logger() { Name = "Server", On = false, Symbol = "Server", Color = new Color(0.610063f, 0.3254948f, 0.3254948f) });
            Loggers.Add(new Engine.Logger() { Name = "Network", On = true, Symbol = "Network", Color = new Color(0.447594f, 0.1824493f, 0.4716981f) });
        }
        [Button("Reset"), HideInInlineEditors]
        private void Reset()
        {
            Awake();
        }
    }
}
