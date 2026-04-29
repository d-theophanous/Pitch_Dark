using Geecku;
using Geecku.GlobalMangers;
using System.Collections.Generic;
using UnityEngine;

namespace Daniel.Master
{
    public class TTSManager : PersistantDSingleton<TTSManager>
    {
        [SerializeField] private ReadableElementGroup LanguageGroup;
        [SerializeField] private ReadableElementGroup NetworkConnectGroup;
        [SerializeField] private ReadableElementGroup MainMenuGroup;
        [SerializeField] private ReadableElementGroup SettingsGeneralGroup;
        [SerializeField] private ReadableElementGroup DialogueGroup;
        [SerializeField] private ReadableElementGroup NetworkGate;
        [SerializeField] private ReadableElementGroup Puzzle;

        public ReadableElementGroup CurrentGroup;

        public void SwitchReadableElementGroup(UI_Group group)
        {
            ReadableElementGroup new_group;
            switch (group)
            {
                case UI_Group.LANGUAGE_SELECTION:
                    new_group = LanguageGroup;
                    break;
                case UI_Group.NETWORK_CONNECT:
                    new_group = NetworkConnectGroup;
                    break;
                case UI_Group.MAIN_MENU:
                    new_group = MainMenuGroup;
                    break;
                case UI_Group.SETTINGS_GENERAL:
                    new_group = SettingsGeneralGroup;
                    break;
                case UI_Group.DIALOGUE:
                    new_group = DialogueGroup;
                    break;
                case UI_Group.NETWORK_GATE:
                    new_group = NetworkGate;
                    break;
                case UI_Group.PUZZLE:
                    Debug.Log("puzzle:" + Puzzle);
                    new_group = Puzzle;
                    break;
                default:
                    new_group = null;
                    Debug.LogError(group + " doesnt have a switch case yet.");
                    break;
            }
            if (CurrentGroup != null)
            {
                CurrentGroup.DeactivateGroup();
            }
            CurrentGroup = new_group;
            CurrentGroup.ActivateGroup();
            Debug.Log(CurrentGroup);
        }
        public void SwitchReadableElementGroup(ReadableElementGroup group)
        {
            if (CurrentGroup != null)
            {
                CurrentGroup.DeactivateGroup();
            }
            CurrentGroup = group;
            CurrentGroup.ActivateGroup();
            Debug.Log(CurrentGroup);
        }
        public void SetPuzzleGroup(ReadableElementGroup group) { Puzzle = group; }

        #region Element Interaction
        public void ActivateCurElement()
        {
            CurrentGroup.ActivateCurElement();
        }
        public void ReturnCurElement()
        {
            CurrentGroup.ReturnCurElement();
        }
        public void SwitchToPreviousElement()
        {
            CurrentGroup.SwitchToPreviousElement();
        }
        public void SwitchToNextElement()
        {
            CurrentGroup.SwitchToNextElement();
        }
        #endregion
    }
    //- for narration of what the person is hovering over
    public enum UI_Element
    {
        BUTTON, TEXT
    }
}
