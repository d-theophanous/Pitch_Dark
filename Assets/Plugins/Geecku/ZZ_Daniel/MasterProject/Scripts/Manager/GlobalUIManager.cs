using Geecku.GlobalMangers;
using System.Collections;
using System.Collections.Generic;
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

        [Header("Canvas References")]
        [SerializeField] private Canvas Canvas;
        [SerializeField] private RawImage DialogueBackground;

        //-  mit jedem hinzufügen switch ändern: GetGameObjectFromEnum
        //- ToDo (opt) eigentlich sollte ich pro ui eine classe haben die hat dann:
        //- ui (gameobject), readableelement group und sound zum öffnen und schließen
        [Header("UI References")]
        [SerializeField] private GameObject Settings;
        [SerializeField] private GameObject Networking;
        [SerializeField] private GameObject Gate_Net;
        [SerializeField] private GameObject Dialogue;
        [SerializeField] private GameObject Language_Selection;
        [SerializeField] private GameObject Puzzle;
        [SerializeField] private GameObject Improvisation;
        [SerializeField] private GameObject PlayerSelection;
        [SerializeField] private GameObject AccessibilitySelection;
        [SerializeField] private GameObject GenreSelection;
        [SerializeField] private List<GameObject> PuzzeList;

        [Header("Settings References")]
        [SerializeField] private GameObject Accessibility_Settings;
        [SerializeField] private GameObject Audio_Settings;
        [SerializeField] private GameObject Genre_Settings;
        [SerializeField] private GameObject Language_Settings;
        [SerializeField] private GameObject Controls_Settings;

        //- for debugging public
        public List<UI_Group> CurUIList = new();

        //- for debugging public
        public Camera CurCamera;

        #region Add and Close UI
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
        public void ToggleUI(UI_Group ui, bool is_additive = true)
        {
            GameObject tmp = GetGameObjectFromEnum(ui);

            //- ugly code
            if (ui == UI_Group.DIALOGUE)
            {
                if (!CurUIList.Contains(ui))
                {
                    DialogueBackground.texture = PlayerCam;
                    DialogueBackground.color = Color.white;
                }
                else
                {
                    DialogueBackground.texture = null;
                    DialogueBackground.color = Color.black;
                }
            }
            else if (ui == UI_Group.PUZZLE)
            {
                PuzzeList[PuzzleManager.Instance.PuzzleCount].SetActive(!CurUIList.Contains(ui));                
            }
            else if (ui == UI_Group.SETTINGS_GENERAL)
                AudioManager.Instance.PlaySFX(SFX.OPEN_UI);

            bool is_active = tmp.gameObject.activeSelf;
            if (is_active)
                CloseUI(ui);
            else
                AddUI(ui);

            StartCoroutine(ToggleReadableElementGroup(ui, !is_active));

            if (!is_additive)
                CloseUI(CurUIList[CurUIList.Count - 2]);
        }

        private void AddUI(UI_Group ui)
        {
            if (CurUIList.Count == 0)
            {
                ChangeMainCamera(UICamera);
                InputManager.Instance.PlayerInput.SwitchCurrentActionMap("UI");
            }
            GameObject tmp = GetGameObjectFromEnum(ui);
            tmp.SetActive(true);
            CurUIList.Add(ui);

            //- make sure UI is in front
            tmp.transform.SetAsLastSibling();
            //- opt ToDo set Magnifier as last sibling (i do it in toggle magnifier
            //- however if you switch settings while having the magnifier on its not last anymore
        }
        private void CloseUI(UI_Group ui)
        {
            if (CurUIList.Count == 0)
                return;

            GetGameObjectFromEnum(ui).SetActive(false);
            CurUIList.Remove(ui);

            if (CurUIList.Count == 0)
            {
                ChangeMainCamera();
                InputManager.Instance.PlayerInput.SwitchCurrentActionMap("Player");
            }
            else
                StartCoroutine(
                    ToggleReadableElementGroup(CurUIList[CurUIList.Count - 1], true));
        }
        private GameObject GetGameObjectFromEnum(UI_Group ui)
        {
            GameObject tmp = null;
            switch (ui)
            {
                case UI_Group.LANGUAGE_SELECTION:
                    tmp = Language_Selection;
                    break;
                case UI_Group.NETWORK_CONNECT:
                    tmp = Networking;
                    break;
                case UI_Group.MAIN_MENU:
                    tmp = null;
                    break;
                case UI_Group.SETTINGS_GENERAL:
                    tmp = Settings;
                    break;
                case UI_Group.NETWORK_GATE:
                    tmp = Gate_Net;
                    break;
                case UI_Group.DIALOGUE:
                    tmp = Dialogue;
                    break;
                case UI_Group.NONE:
                    tmp = null;
                    break;
                case UI_Group.PUZZLE:
                    tmp = Puzzle;
                    break;
                case UI_Group.IMPROVISATION:
                    tmp = Improvisation;
                    break;
                case UI_Group.PLAYER_SELECTION:
                    tmp = PlayerSelection;
                    break;
                case UI_Group.ACCESSIBILITY_SELECTION:
                    tmp = AccessibilitySelection;
                    break;
                case UI_Group.GENRE_SELECTION:
                    tmp = GenreSelection;
                    break;
                default:
                    break;
            }
            return tmp;
        }

        #endregion

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

        private IEnumerator ToggleReadableElementGroup(UI_Group ui, bool activate_settings)
        {
            yield return new WaitForEndOfFrame();
            if (activate_settings)
            {
                TTSManager.Instance.SwitchReadableElementGroup(ui);
            }
        }
    }
    //- this is not very efficient but oh well...
    public enum UI_Group
    {
        LANGUAGE_SELECTION, NETWORK_CONNECT, MAIN_MENU, SETTINGS_GENERAL, NETWORK_GATE,
        DIALOGUE, PUZZLE, NONE, IMPROVISATION, PLAYER_SELECTION, ACCESSIBILITY_SELECTION,
        GENRE_SELECTION
    }
    public enum Settings
    {
        LANGUAGE, CONTROLS, AUDIO, ACCESSIBILITY, GENRE, CLOSE
    }
}
