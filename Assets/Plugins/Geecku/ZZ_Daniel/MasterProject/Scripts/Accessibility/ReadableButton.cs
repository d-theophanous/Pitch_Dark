using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Daniel.Master
{
    [RequireComponent (typeof (Button))]
    public class ReadableButton : ReadableElement
    {
        private Button Button;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            Element = UI_Element.BUTTON;
            Button = GetComponent<Button>();
        }
        public override void Activate()
        {
            if (Child != null)
            {
                TTSManager.Instance.SwitchReadableElementGroup(Child);
            }
            else
            {
                Button.onClick.Invoke();
            }
        }
        protected override void OnSelect()
        {
            Button.OnPointerEnter(new PointerEventData(EventSystem.current));
        }
        protected override void OnDeselect()
        {
            Button.OnPointerExit(new PointerEventData(EventSystem.current));
        }
    }
}
