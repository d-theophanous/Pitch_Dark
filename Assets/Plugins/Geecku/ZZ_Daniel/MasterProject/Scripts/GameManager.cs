using Geecku.GlobalMangers;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Daniel.Master
{
    public class GameManager : PersistantDSingleton<GameManager>
    {
        [SerializeField] List<string> SceneList;

        public int PlayerNumber;

        protected override void Awake()
        {
            base.Awake();
            if (WillBeDestroyed) return;


            //- Setup languages and everything for debug to be able to skip things
        }
        protected override void Start()
        {
            base.Start();
            GlobalUIManager.Instance.ToggleNetworking();
        }
        private void SwitchSceneToFirstInList()
        {
            if (SceneList.Count == 0 || SceneList[0] == "") return;
            StartCoroutine(SwitchScene(SceneList[0]));
        }
        private IEnumerator SwitchScene(string scene_name)
        {
            SceneManager.LoadScene(scene_name, LoadSceneMode.Additive);
            yield return new WaitForEndOfFrame();
            GlobalUIManager.Instance.ChangeMainCamera();
        }

    }
}
