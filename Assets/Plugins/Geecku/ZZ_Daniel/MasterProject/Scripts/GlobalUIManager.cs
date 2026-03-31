using Geecku.GlobalMangers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Daniel.Master
{
    public class GlobalUIManager : PersistantDSingleton<GlobalUIManager>
    {
        [SerializeField] private Camera UICamera;
        [SerializeField] private Camera MagCamera;

        [SerializeField] private Canvas SettingsCanvas;

        [SerializeField] private GameObject WaitingForPlayer;
        [SerializeField] private GameObject HostDisconnected;
        [SerializeField] private GameObject Settings;

        private Camera CurCamera;
 
        public void ChangeMainCamera(Camera new_main)
        {
            if (new_main == null) return;
            if (CurCamera != null)
                CurCamera.depth = -1;

            CurCamera = new_main;
            CurCamera.depth = 0;
        }
        public void ChangeMainCamera()
        {
            ChangeMainCamera(GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>());
        }
        public void ToggleSettings()
        {
            SettingsCanvas.gameObject.SetActive(!SettingsCanvas.gameObject.activeSelf);
            if (SettingsCanvas.gameObject.activeSelf)
                ChangeMainCamera(UICamera);
            else
                ChangeMainCamera();
        }
    }
}
