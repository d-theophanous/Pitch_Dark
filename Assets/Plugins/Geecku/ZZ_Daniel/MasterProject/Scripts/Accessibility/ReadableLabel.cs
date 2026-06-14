using System.Collections.Generic;
using UnityEngine;

namespace Daniel.Master
{
    public class ReadableLabel : ReadableElement
    {
        // St
        // art is called once before the first execution of Update after the MonoBehaviour is created
        public void Start()
        {            
            Element = UI_Element.TEXT;
        }
        public override void Activate()
        {
            ReadText();
        }
    }
}
