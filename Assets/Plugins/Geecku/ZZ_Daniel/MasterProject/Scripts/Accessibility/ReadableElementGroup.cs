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
        public List<ReadableElement> GetElements() => ElementList;
        public void RemoveElement(ReadableElement element)
        {
            if (ElementList.Contains(element))
                ElementList.Remove(element);
        }
        public void AddElement(ReadableElement element)
        {
            if (!ElementList.Contains(element))
                ElementList.Add(element);
        }
        public void ActivateGroup()
        {
            ElementList.Sort();
            if (ElementList.Count > 0 )
            {
                ElementList[CurElementIdx].ToggleHighlight();
            }
        }
        //- kinda ugly and unneccessary opt ToDo
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
                AudioManager.Instance.PlaySFX(SFX.NO_MORE_ELEMENTS);
                return;
            }
            AudioManager.Instance.PlaySFX(SFX.SWITCH_ELEMENT);
            ElementList[CurElementIdx].ToggleHighlight();
            CurElementIdx--;
            ElementList[CurElementIdx].ToggleHighlight();
        }
        public void SwitchToNextElement()
        {
            if (CurElementIdx + 1 >= ElementList.Count)
            {
                AudioManager.Instance.PlaySFX(SFX.NO_MORE_ELEMENTS);
                Debug.Log("no more next elements");
                return;
            }
            AudioManager.Instance.PlaySFX(SFX.SWITCH_ELEMENT);
            ElementList[CurElementIdx].ToggleHighlight();
            CurElementIdx++;
            ElementList[CurElementIdx].ToggleHighlight();
        }
        public void SetCurElement(int index)
        {
            if (index < 0 || ElementList.Count - 1 >= index)
                return;
            ElementList[CurElementIdx].ToggleHighlight();
            CurElementIdx = index;
            ElementList[CurElementIdx].ToggleHighlight();
        }
        public void RepeatCurElement()
        {
            ElementList[CurElementIdx].Repeat();
        }
        public ReadableElement GetCurElement()
        {
            return ElementList[CurElementIdx];
        }
    }
}
