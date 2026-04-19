using System.Collections.Generic;
using UnityEngine;

namespace Daniel.Master
{
    public class ReadableElementGroup : MonoBehaviour
    {
        [SerializeField] private List<ReadableElement> ElementList;
        [field: SerializeField] public UI_Group Group {  get; private set; }

        //- this is a mess
        public ReadableElementGroup Parent;
        private int CurElementIdx = 0;

        private void Awake()
        {
            foreach (ReadableElement element in ElementList) 
            {
                element.SetParent(this);
            }
        }
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
        public void ActivateCurElement()
        {
            ElementList[CurElementIdx].Activate();
        }
        public void ReturnCurElement()
        {
            ElementList[CurElementIdx].Return();
        }
        public void SwitchToPreviousElement()
        {
            if (CurElementIdx - 1 < 0)
            {
                Debug.Log("no more previous elements");
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
                Debug.Log("no more next elements");
                return;
            }
            ElementList[CurElementIdx].ToggleHighlight();
            CurElementIdx++;
            ElementList[CurElementIdx].ToggleHighlight();
        }
    }
}
