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

        [SerializeField] int MagnifierSize;
        [SerializeField] int MagnifierZoom;

        #region Magnifier

        public void ToggleMagnifier()
        {
            MagGlasses.gameObject.SetActive(
                !MagGlasses.gameObject.activeSelf);
            if (!MagGlasses.gameObject.activeSelf)
                MagGlasses.gameObject.transform.localPosition = 
                    new Vector3(0,0, MagGlasses.gameObject.transform.localPosition.z);
        }
        #endregion

    }
}
