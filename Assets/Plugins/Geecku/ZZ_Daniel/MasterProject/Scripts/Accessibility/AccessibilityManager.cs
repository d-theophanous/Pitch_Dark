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
        [SerializeField] private float MagnifierMoveSpeed;
        [SerializeField] private Vector3 DefaultMagCamPos;
        [SerializeField] private Vector3 DefaultMagGlassPos;

        public AccessibilityMode AccMode;

        protected override void Start()
        {
            base.Start();
            SetCorners();
        }
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
                MagCamera.gameObject.transform.localPosition = DefaultMagCamPos;
                MagGlasses.gameObject.transform.localPosition = DefaultMagGlassPos;
                MagGlasses.gameObject.transform.SetAsLastSibling();
            }  
        }
        private Vector2 MoveMagnifyDelta;

        public void OnMoveMagnify(InputValue value)
        {
            MoveMagnifyDelta = value.Get<Vector2>();
        }

        private void UpdatePositions()
        {
            Vector3 MoveMagnifyDelta = InputManager.Instance.MoveMagnifyDelta;
            Vector3 offset = new Vector3(MoveMagnifyDelta.x, MoveMagnifyDelta.y, 0f) * MagnifierMoveSpeed * Time.deltaTime;

            // Calculate new position
            Vector3 newPos = MagGlasses.transform.position + offset;

            float halfW = (MagCorners[2].x - MagCorners[0].x) / 2f;
            float halfH = (MagCorners[2].y - MagCorners[0].y) / 2f;
            // Clamp within canvas minus magnifier half-size
            newPos.x = Mathf.Clamp(newPos.x, CanvasCorners[0].x + halfW, CanvasCorners[2].x - halfW);
            newPos.y = Mathf.Clamp(newPos.y, CanvasCorners[0].y + halfH, CanvasCorners[2].y - halfH);
            newPos.z = MagGlasses.transform.position.z; // preserve z

            MagGlasses.transform.position = newPos;
            MagCamera.transform.position = new Vector3(newPos.x, newPos.y, MagCamera.transform.position.z);
        }
        Vector3[] MagCorners = new Vector3[4];
        Vector3[] CanvasCorners = new Vector3[4];
        private void SetCorners()
        {
            // Get the canvas bounds in world space
            RectTransform canvasRect = Settings.GetComponent<RectTransform>();
            canvasRect.GetWorldCorners(CanvasCorners);
            // corners: 0=bottom-left, 1=top-left, 2=top-right, 3=bottom-right

            // Get half the size of the magnifier so it stops at the edge, not the center
            RectTransform magRect = MagGlasses.GetComponent<RectTransform>();
            magRect.GetWorldCorners(MagCorners);
        }

        #endregion

    }
    public enum AccessibilityMode
    {
        BLIND, NONE
    }
}
