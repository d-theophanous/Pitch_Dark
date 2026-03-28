using System;
using System.Collections;
using UnityEngine;

namespace Geecku.GlobalMangers
{
    public abstract class PersistantDSingleton<T> : Singleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();
            if (WillBeDestroyed)
                return;
            DontDestroyOnLoad(gameObject);
        }
    }
    public abstract class Singleton<T> : Sirenix.OdinInspector.SerializedMonoBehaviour where T : MonoBehaviour
    {
        protected bool WillBeDestroyed;
        public static T Instance { get; private set; }
        protected virtual void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                WillBeDestroyed = true;
                return;
            }
            Instance = this as T;
        }
        protected virtual void Start() { }
        protected virtual void Update() { }

        protected void AfterAwake(Func<bool> predicate, Action action)
        {
            StartCoroutine(CourtineForAfterAwake(predicate, action));
        }
        private IEnumerator CourtineForAfterAwake(Func<bool> predicate, Action action)
        {
            yield return new WaitUntil(predicate);
            action?.Invoke();
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        protected virtual void OnApplicationQuit()
        {
            Instance = null;
        }
    }
}