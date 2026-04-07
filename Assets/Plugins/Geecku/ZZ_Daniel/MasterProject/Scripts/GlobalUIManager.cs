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

        [SerializeField] private Canvas Canvas;

        [SerializeField] private GameObject Settings;
        [SerializeField] private GameObject Networking;

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
            ToggleUI(UI.SETTINGS);
        }
        public void ToggleNetworking()
        {
            ToggleUI(UI.NETWORKING);
        }
        private void ToggleUI(UI ui)
        {
            Canvas.gameObject.SetActive(!Canvas.gameObject.activeSelf);
            if (Canvas.gameObject.activeSelf)
                ChangeMainCamera(UICamera);
            else
                ChangeMainCamera();
            switch (ui)
            {
                case UI.MAIN_MENU:
                    break;
                case UI.SETTINGS:
                    Settings.SetActive(!Settings.gameObject.activeSelf);
                    break;
                case UI.LANGUAGE:
                    break;
                case UI.NETWORKING:
                    Networking.SetActive(!Networking.gameObject.activeSelf);
                    break;
                default:
                    break;
            }
        }
    }
    public enum UI
    {
        MAIN_MENU, SETTINGS, LANGUAGE, NETWORKING
    }
}
