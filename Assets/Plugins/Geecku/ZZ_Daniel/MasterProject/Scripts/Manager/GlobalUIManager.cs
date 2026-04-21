using Geecku.GlobalMangers;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Daniel.Master
{
    public class GlobalUIManager : PersistantDSingleton<GlobalUIManager>
    {
        [Header ("Camera Stuff")]
        [SerializeField] private Camera UICamera;
        [SerializeField] private Camera MagCamera;
        [SerializeField] private RenderTexture PlayerCam;
        public GameObject SecondPlayerCam;

        [Header("Canvas References")]
        [SerializeField] private Canvas Canvas;
        [SerializeField] private RawImage Background;

        [Header("UI References")]
        [SerializeField] private GameObject Settings;
        [SerializeField] private GameObject Networking;
        [SerializeField] private GameObject Gate_Net;
        [SerializeField] private GameObject Dialogue;
        [SerializeField] private GameObject Language_Selection;

        [Header("Settings References")]
        [SerializeField] private GameObject Accessibility_Settings;
        [SerializeField] private GameObject Audio_Settings;
        [SerializeField] private GameObject Genre_Settings;
        [SerializeField] private GameObject Language_Settings;
        [SerializeField] private GameObject Controls_Settings;

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
        public void ToggleUI(UI_Group ui, bool toggle_whole_canvas = true)
        {
            //- Either settings are newly opened or should completely be closed
            if (toggle_whole_canvas)
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
                    tmp = Language_Selection;
                    break;
                case UI_Group.NETWORK_CONNECT:
                    tmp = Networking;
                    break;
                case UI_Group.NETWORK_GATE:
                    tmp = Gate_Net;
                    break;
                case UI_Group.DIALOGUE:
                    tmp = Dialogue;
                    if (Canvas.gameObject.activeSelf)
                    {
                        SecondPlayerCam.gameObject.SetActive(true);
                        Background.texture = PlayerCam;
                        Background.color = Color.white;
                    }
                    else
                    {
                        SecondPlayerCam.gameObject.SetActive(false);
                        Background.texture = null;
                        Background.color = Color.black;
                    }
                    break;
                default:
                    break;
            }
            tmp.SetActive(!tmp.gameObject.activeSelf);
            StartCoroutine(ToggleReadableElementGroup(ui));
        }

        #region Settings
        private GameObject CurSettings;
        public void SetSettings(Settings settings)
        {
            if (CurSettings != null)
            {
                CurSettings.SetActive(false);
            }
            switch (settings)
            {
                case Master.Settings.LANGUAGE:
                    CurSettings = Language_Settings;
                    break;
                case Master.Settings.CONTROLS:
                    CurSettings = Controls_Settings;
                    break;
                case Master.Settings.AUDIO:
                    CurSettings = Audio_Settings;
                    break;
                case Master.Settings.ACCESSIBILITY:
                    CurSettings = Accessibility_Settings;
                    break;
                case Master.Settings.GENRE:
                    CurSettings = Genre_Settings;
                    break;
                case Master.Settings.CLOSE:
                    CurSettings.SetActive(false);
                    CurSettings = null;
                    return;
                default:
                    break;
            }
            CurSettings.SetActive(true);
        }
        public void SetAccessibility() { SetSettings(Master.Settings.ACCESSIBILITY); }
        public void SetAudio() { SetSettings(Master.Settings.AUDIO); }
        public void SetGenre() { SetSettings(Master.Settings.GENRE); }
        public void SetControls() { SetSettings(Master.Settings.CONTROLS); }
        public void SetLanguage() { SetSettings(Master.Settings.LANGUAGE); }
        #endregion

        #region Dialogue
        public ReadableDialogue GetReadableDialogue()
        {
            if (Dialogue == null)
            {
                Debug.LogWarning("Dialogue is null");
                return null;
            }
            return Dialogue.GetComponentInChildren<ReadableDialogue>();
        }
        #endregion

        private IEnumerator ToggleReadableElementGroup(UI_Group ui)
        {
            yield return new WaitForEndOfFrame();
            if (Canvas.gameObject.activeSelf)
            {
                TTSManager.Instance.SwitchReadableElementGroup(ui);
            }
        }
    }
    //- this is not very efficient but oh well...
    public enum UI_Group
    {
        LANGUAGE_SELECTION, NETWORK_CONNECT, MAIN_MENU, SETTINGS_GENERAL, NETWORK_GATE,
        DIALOGUE
    }
    public enum Settings
    {
        LANGUAGE, CONTROLS, AUDIO, ACCESSIBILITY, GENRE, CLOSE
    }
}
