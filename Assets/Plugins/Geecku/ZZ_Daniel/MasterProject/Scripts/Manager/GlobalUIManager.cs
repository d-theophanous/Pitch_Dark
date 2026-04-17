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
        public void ToggleUI(UI_Group ui)
        {
            Canvas.gameObject.SetActive(!Canvas.gameObject.activeSelf);

            //- switch action maps
            if (Canvas.gameObject.activeSelf)
            {
                ChangeMainCamera(UICamera);
                InputManager.Instance.PlayerInput.SwitchCurrentActionMap("UI");
            }
            else
            {
                ChangeMainCamera();
                InputManager.Instance.PlayerInput.SwitchCurrentActionMap("Player");
            }
            GameObject tmp = null;
            switch (ui)
            {
                case UI_Group.MAIN_MENU:
                    break;
                case UI_Group.SETTINGS_GENERAL:
                    tmp = Settings;
                    break;
                case UI_Group.LANGUAGE_SELECTION:
                    break;
                case UI_Group.NETWORK_CONNECT:
                    tmp = Networking;
                    break;
                case UI_Group.NETWORK_GATE:
                    tmp = Gate_Net;
                    break;
                default:
                    break;
            }
            tmp.SetActive(!tmp.gameObject.activeSelf);
            StartCoroutine(ToggleReadableElementGroup(ui));
        }
        private IEnumerator ToggleReadableElementGroup(UI_Group ui)
        {
            yield return new WaitForEndOfFrame();
            if (Canvas.gameObject.activeSelf)
            {
                TTSManager.Instance.SwitchReadableElementGroup(ui);
            }
        }
    }
    public enum UI_Group
    {
        LANGUAGE_SELECTION, NETWORK_CONNECT, MAIN_MENU, SETTINGS_GENERAL, NETWORK_GATE
    }
}
