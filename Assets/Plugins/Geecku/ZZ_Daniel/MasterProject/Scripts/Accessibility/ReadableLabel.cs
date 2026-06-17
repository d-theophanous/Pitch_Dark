using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Daniel.Master
{
    public class ReadableLabel : ReadableElement
    {
        [Header("Highlight Settings")]
        public Color buttonHighlightColor = Color.red;
        public Color frameColor = Color.red;
        public float frameThickness = 3f;

        private TMP_Text _text;
        private Color _originalButtonColor;
        private GameObject _frameObject;
        private bool _isHighlighted = false;
        // St
        // art is called once before the first execution of Update after the MonoBehaviour is created
        public void Start()
        {            
            Element = UI_Element.TEXT;
            _text = GetComponent<TMP_Text>();
            if (_text != null)
            {
                CreateTextFrame();
            }
        }
        public override void Activate()
        {
            ReadText();
        }

        private void CreateTextFrame()
        {
            _frameObject = new GameObject("HighlightFrame");
            _frameObject.transform.SetParent(transform, false);

            RectTransform frameRect = _frameObject.AddComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = Vector2.zero; // no expansion outside bounds
            frameRect.offsetMax = Vector2.zero;

            CreateBorderLine(_frameObject, "Top", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, frameThickness));
            CreateBorderLine(_frameObject, "Bottom", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, frameThickness));
            CreateBorderLine(_frameObject, "Left", new Vector2(0, 0), new Vector2(0, 1), new Vector2(frameThickness, 0));
            CreateBorderLine(_frameObject, "Right", new Vector2(1, 0), new Vector2(1, 1), new Vector2(frameThickness, 0));

            _frameObject.SetActive(false);
        }

        private void CreateBorderLine(GameObject parent, string lineName, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta)
        {
            GameObject line = new GameObject(lineName);
            line.transform.SetParent(parent.transform, false);

            RectTransform rt = line.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = Vector2.zero;

            Image img = line.AddComponent<Image>();
            img.color = frameColor;
            img.raycastTarget = false;
        }
        protected override void OnSelect()
        {
            base.OnSelect();
            if (_text != null && _frameObject != null)
            {
                _frameObject.SetActive(true);
            }
        }
        protected override void OnDeselect()
        {
            base.OnDeselect();
            if (_text != null && _frameObject != null)
            {
                _frameObject.SetActive(false);
            }
        }
    }
}

