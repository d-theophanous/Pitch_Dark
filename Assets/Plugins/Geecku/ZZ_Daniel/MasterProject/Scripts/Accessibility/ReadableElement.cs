using System;
using TMPro;
using UnityEngine;

namespace Daniel.Master
{
    public class ReadableElement : MonoBehaviour, IComparable<ReadableElement>
    {
        [SerializeField] private TMP_Text Text;
        [SerializeField] private UI_Label Label;
        [SerializeField] private UI_Element Element;
        private bool IsHighlighted;

        public int Ordernumber;


        public void ToggleHighlight()
        {
            Debug.Log("ToggleHighlight");
            if (IsHighlighted)
            {
                //- ToDo
            }
            else
            {
                //- ToDo
            }
        }
        private void ReadText()
        {
            //- ToDo
            Debug.Log("Reading text");
        }

        public int CompareTo(ReadableElement other)
        {
            return Ordernumber.CompareTo(other.Ordernumber);
        }
    }
}
