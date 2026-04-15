using Geecku.GlobalMangers;
using System.Collections;
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
        [SerializeField] private GameObject Gate_Net;

        //- for debugging public
        public Camera CurCamera;
 
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
        public void ToggleUI(UI ui)
        {
            Canvas.gameObject.SetActive(!Canvas.gameObject.activeSelf);
            if (Canvas.gameObject.activeSelf)
            {
                ChangeMainCamera(UICamera);
                InputManager.Instance.PlayerInput.SwitchCurrentActionMap("Rest");
            }
            else
            {
                ChangeMainCamera();
                InputManager.Instance.PlayerInput.SwitchCurrentActionMap("Player");
            }
            GameObject tmp = null;
            switch (ui)
            {
                case UI.MAIN_MENU:
                    break;
                case UI.SETTINGS:
                    tmp = Settings;
                    break;
                case UI.LANGUAGE:
                    break;
                case UI.NETWORKING:
                    tmp = Networking;
                    break;
                case UI.GATE_NET:
                    tmp = Gate_Net;
                    break;
                default:
                    break;
            }
            tmp.SetActive(!tmp.gameObject.activeSelf);
        }
    }
    public enum UI
    {
        MAIN_MENU, SETTINGS, LANGUAGE, NETWORKING, GATE_NET
    }
}
