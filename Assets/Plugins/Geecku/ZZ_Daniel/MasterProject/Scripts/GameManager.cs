using Geecku.GlobalMangers;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Daniel.Master
{
    public class GameManager : PersistantDSingleton<GameManager>
    {
        [SerializeField] List<string> SceneList;

        protected override void Awake()
        {
            base.Awake();
            if (WillBeDestroyed) return;

            if (SceneList.Count == 0 || SceneList[0] == "") return;
            SceneManager.LoadScene(SceneList[0], LoadSceneMode.Additive);

            //- Setup languages and everything for debug to be able to skip things
        }

    }
}
