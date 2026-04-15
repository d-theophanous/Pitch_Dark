using Daniel.Master;
using System.Collections.Generic;
using UnityEngine;

namespace Geecku
{
    public class ReadableElementGroup : MonoBehaviour
    {
        [SerializeField] private ReadableElementGroup Parent;
        [SerializeField] private ReadableElementGroup SubGroup;
        [SerializeField] private List<ReadableElement> ElementList;
        [SerializeField] private UI_Group Group;

        private int CurElementIdx = 0;

        public void ActivateGroup()
        {
            ElementList.Sort();
            if (ElementList.Count > 0 )
            {
                ElementList[CurElementIdx].ToggleHighlight();
            }
        }
        public void DeactivateGroup()
        {
            ElementList[CurElementIdx].ToggleHighlight();
            CurElementIdx = 0;
        }
        public void SwitchToSubGroup()
        {
            DeactivateGroup();
            TTSManager.Instance.CurrentGroup = SubGroup;
        }
        public void SwitchToParentGroup()
        {
            DeactivateGroup();
            TTSManager.Instance.CurrentGroup = Parent;
        }
        public void SwitchToPreviousElement()
        {
            if (CurElementIdx - 1 < 0)
            {
                //- Maybe play sound (ToDo)
                return;
            }
            ElementList[CurElementIdx].ToggleHighlight();
            CurElementIdx--;
            ElementList[CurElementIdx].ToggleHighlight();
        }
        public void SwitchToNextElement()
        {
            if (CurElementIdx + 1 >= ElementList.Count)
            {
                //- Maybe play sound (ToDo)
                return;
            }
            ElementList[CurElementIdx].ToggleHighlight();
            CurElementIdx++;
            ElementList[CurElementIdx].ToggleHighlight();
        }
    }
}
