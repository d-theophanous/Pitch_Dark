using System;
using TMPro;
using UnityEngine;

namespace Daniel.Master
{
    public class ReadableElement : MonoBehaviour, IComparable<ReadableElement>
    {
        [SerializeField] protected ReadableElementGroup Child;
        [SerializeField] protected ReadableElementGroup Parent;
        [SerializeField] protected TMP_Text Text;
        //- anstelle von Label vlt Dictonary mit englishem Text als Key
        //- sprach triple oder so als value
        protected UI_Element Element;
        protected bool IsHighlighted = false;

        public int Ordernumber;

        private void Start()
        {
            if (Child != null)
            {
                Child.Parent = Parent;
            }
        }
        public virtual void ToggleHighlight()
        {
            if (IsHighlighted)
            {
                OnDeselect();
                IsHighlighted = false;
            }
            else
            {
                OnSelect();
                IsHighlighted = true;
            }
        }
        protected virtual void OnSelect() 
        {
            
        }
        protected virtual void OnDeselect() { }
        public virtual void Activate() { }
        public virtual void Return()
        {
            if (Parent != null)
            {
                TTSManager.Instance.SwitchReadableElementGroup(Parent.Parent);
                AudioManager.Instance.PlaySFX(SFX.GO_BACK);
            }
            else
            {
                AudioManager.Instance.PlaySFX(SFX.NO_MORE_ELEMENTS);
                Debug.Log("readable element doesnt have a parent");
            }
        }
        protected void ReadText()
        {
            //- ToDo
            Debug.Log("Reading text");
        }
        public void SetParent(ReadableElementGroup parent) { Parent = parent; }

        public int CompareTo(ReadableElement other)
        {
            return Ordernumber.CompareTo(other.Ordernumber);
        }
    }
}
