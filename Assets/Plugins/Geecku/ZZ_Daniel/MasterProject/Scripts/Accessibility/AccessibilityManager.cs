using Geecku.DefaultNetworking;
using Geecku.GlobalMangers;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Daniel.Master
{
    public class AccessibilityManager : PersistantDSingleton<AccessibilityManager>
    {
        [Header("Magnifier References")]
        [SerializeField] RawImage MagGlasses;
        [SerializeField] Camera MagCamera;
        [SerializeField] Camera MainCamera;
        [SerializeField] Camera UICamera;
        [SerializeField] RenderTexture MagTexture;
        [SerializeField] Canvas Settings;

        [Header("Screen Space Canvas")]
        [SerializeField] GameObject ScreenCanvas;

        [SerializeField] int MagnifierSize;
        [SerializeField] int MagnifierZoom;

        public AccessibilityMode AccMode;

        protected override void Update()
        {
            //- Update camera movement when Magnifier is enabled
            if (IsMagnify)
            {
                UpdatePositions();  
            }
        }
        #region Accessibility Mode Handling
        public void SetAccessibilityMode(int mode)
        {
            AccMode = (AccessibilityMode)mode;
            SubscribeEvents(AccMode);
            ToggleSettings(AccMode);
            GlobalUIManager.Instance.ToggleUI(UI_Group.GENRE_SELECTION, false);
        }
        private void ToggleSettings(AccessibilityMode mode)
        {
            switch (mode)
            {
                case AccessibilityMode.BLIND:
                    ScreenCanvas.SetActive(true);
                    break;
                case AccessibilityMode.NONE:
                    break;
                default:
                    break;
            }
        }
        private void SubscribeEvents(AccessibilityMode mode)
        {
            if (mode == AccMode)
            {
                Debug.Log("Accessibility mode is already active");
                return;
            }
            switch (mode)
            {
                case AccessibilityMode.BLIND:
                    break;
                case AccessibilityMode.NONE:
                    break;
                default:
                    break;
            }

        }

        //- Events
        //public event EventHandler SetupAccessibility;
        #region Functions
        //- event functions
        #endregion
        #endregion

        #region Magnifier

        private bool IsMagnify => MagGlasses.gameObject.activeSelf;
        /// <summary>
        /// Activate or deactivate Magnifier and put magnifying glasses back to middle of screen
        /// when turned off
        /// </summary>
        public void ToggleMagnifier()
        {
            MagGlasses.gameObject.SetActive(!IsMagnify);
            //- snap to mouse once enabled
            if (IsMagnify)
            {
                //Vector3 world_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 world_pos = GlobalUIManager.Instance.CurCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                Vector3 new_pos = new Vector3(world_pos.x, world_pos.y, 
                    MagGlasses.gameObject.transform.position.z);
                MagGlasses.gameObject.transform.localPosition = new_pos;
                MagGlasses.gameObject.transform.SetAsLastSibling();
            }  
        }
        private void UpdatePositions()
        {
            //- ToDo scaling problem persists Problem ist dass Magnifier glasses -266 z value
            //- for now only mouse position?
            Vector3 currentWorldPoint;
            //- ToDo switch to new INput system
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                Settings.GetComponent<RectTransform>(), Mouse.current.position.ReadValue(), UICamera, out currentWorldPoint
            );

            MagCamera.transform.position = new Vector3(currentWorldPoint.x,
                currentWorldPoint.y,
                MagCamera.transform.position.z);
            MagGlasses.transform.position = new Vector3(currentWorldPoint.x,
                currentWorldPoint.y,
                MagGlasses.transform.position.z);
        }
        private float GetWorldUnitsPerPixel()
        {
            // Depth of object along camera's look direction
            float depth = Vector3.Dot(
                transform.position - UICamera.transform.position,
                UICamera.transform.forward
            );

            // World units per pixel at that depth
            float frustumHeight = 2f * depth * Mathf.Tan(UICamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            return frustumHeight / Screen.height;
        }

        #endregion

    }
    public enum AccessibilityMode
    {
        BLIND, NONE
    }
}
