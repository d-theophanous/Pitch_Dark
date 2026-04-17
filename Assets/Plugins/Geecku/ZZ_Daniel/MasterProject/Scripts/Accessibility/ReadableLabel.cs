using UnityEngine;

namespace Daniel.Master
{
    public class ReadableLabel : ReadableElement
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Element = UI_Element.TEXT;
        }
        public override void Activate()
        {
            ReadText();
        }
    }
}
