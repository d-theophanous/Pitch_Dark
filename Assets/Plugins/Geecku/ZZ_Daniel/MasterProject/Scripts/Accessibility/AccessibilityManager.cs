using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Accessibility;
using Geecku.GlobalMangers;
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

        [SerializeField] int MagnifierSize;
        [SerializeField] int MagnifierZoom;

        protected override void Update()
        {
            //- Update camera movement when Magnifier is enabled
            if (IsMagnify)
            {
                UpdatePositions();  
            }
        }

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
                Vector3 world_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 new_pos = new Vector3(world_pos.x, world_pos.y, 
                    MagGlasses.gameObject.transform.position.z);
                MagGlasses.gameObject.transform.localPosition = new_pos;
            }  
        }
        private void UpdatePositions()
        {
            //- ToDo scaling problem persists Problem ist dass Magnifier glasses -266 z value
            //- for now only mouse position?
            Vector3 currentWorldPoint;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                Settings.GetComponent<RectTransform>(), Input.mousePosition, UICamera, out currentWorldPoint
            );

            MagCamera.transform.position = new Vector3(currentWorldPoint.x,
                currentWorldPoint.y,
                MagCamera.transform.position.z);
            MagGlasses.transform.position = new Vector3(currentWorldPoint.x,
                currentWorldPoint.y,
                MagGlasses.transform.position.z);

            //Vector3 screen_pos = Input.mousePosition;
            //screen_pos.z = Vector3.Distance(UICamera.transform.position, MagGlasses.transform.position);
            //Vector3 mouse_pos = UICamera.ScreenToWorldPoint(screen_pos);

            //float scale_factor = Settings.scaleFactor;

            //Debug.Log(mouse_pos);
            //MagCamera.transform.position = new Vector3(mouse_pos.x / scale_factor,
            //    mouse_pos.y / scale_factor, 
            //    MagCamera.transform.position.z);
            //MagGlasses.transform.position = new Vector3(mouse_pos.x / scale_factor,
            //    mouse_pos.y / scale_factor,
            //    MagGlasses.transform.position.z);

            //float scale_factor = GetWorldUnitsPerPixel(); //-   maybe only have to do once?
            //float scale_factor = 0.005f; //-   maybe only have to do once?
            //Vector3 tmp_pos = UICamera.WorldToScreenPoint(MagGlasses.transform.position);
            //tmp_pos += new Vector3(InputManager.Instance.MoveMagnifyDelta.x,
            //    InputManager.Instance.MoveMagnifyDelta.y,
            //    0f);
            //MagGlasses.transform.position = UICamera.ScreenToWorldPoint(tmp_pos);

            //Debug.Log(InputManager.Instance.MoveMagnifyDelta);
            ////- change unten to UICamera
            //tmp_pos = UICamera.WorldToScreenPoint(MagCamera.transform.position);
            //tmp_pos += new Vector3(InputManager.Instance.MoveMagnifyDelta.x,
            //    InputManager.Instance.MoveMagnifyDelta.y,
            //    0f);
            //MagCamera.transform.position = UICamera.ScreenToWorldPoint(tmp_pos);

            //Vector3 new_pos_glasses = new Vector3(
            //    MagGlasses.gameObject.transform.position.x + (InputManager.Instance.MoveMagnifyDelta.x * scale_factor),
            //    MagGlasses.gameObject.transform.position.y + (InputManager.Instance.MoveMagnifyDelta.y * scale_factor),
            //    MagGlasses.gameObject.transform.position.z);
            //Vector3 new_pos_camera = new Vector3(
            //    new_pos_glasses.x,
            //    new_pos_glasses.y,
            //    MagCamera.gameObject.transform.position.z);
            //MagGlasses.gameObject.transform.position = new_pos_glasses;
            //MagCamera.gameObject.transform.position = new_pos_camera;
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
}
