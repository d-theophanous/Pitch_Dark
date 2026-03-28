using Geecku.DefaultEngine.Common.Sound;
using Geecku.GlobalMangers;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Geecku.DefaultEngine.Common.Sound
{
    [System.Serializable]
    public class SoundPlayer : MonoBehaviour
    {
        [LabelText("SFX Type")]
        [LabelWidth(100)]
        [OnValueChanged("SFXChange")]
        [InlineButton("PlaySFX")]
        public SoundManager.SoundType SfxType = SoundManager.SoundType.UI;

        [LabelText("$sfxLabel")]
        [LabelWidth(100)]
        [ValueDropdown("SFXType")]
        [OnValueChanged("SFXChange")]
        [InlineButton("SelectSFX")]
        public SoundClip SfxToPlay;
        private string SfxLabel = "SFX";

        [SerializeField]
        #pragma warning disable CS0414
        private bool ShowSettings = false;
        #pragma warning restore CS0414

        [ShowIf("ShowSettings")]
        [SerializeField]
        #pragma warning disable CS0414
        private bool EditSettings = false;
        #pragma warning restore CS0414

        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ShowIf("ShowSettings")]
        [EnableIf("EditSettings")]
        [SerializeField]
        private SoundClip _SfxBase;

        [Title("Audio Source")]
        [ShowIf("ShowSettings")]
        [EnableIf("EditSettings")]
        [SerializeField]
        private bool WaitToPlay = true;

        [ShowIf("ShowSettings")]
        [EnableIf("EditSettings")]
        [SerializeField]
        private bool UseDefault = true;

        [DisableIf("UseDefault")]
        [ShowIf("ShowSettings")]
        [EnableIf("EditSettings")]
        [SerializeField]
        private AudioSource Audiosource;

        private void SFXChange()
        {
            //keep the label up to date
            SfxLabel = SfxType.ToString() + " SFX";

            //keep the displayed "SFX clip" up to date
            _SfxBase = SfxToPlay;
        }

        //- Commented out since build threw errors. Had 0 references anyway
        //private void SelectSFX()
        //{
        //    UnityEditor.Selection.activeObject = SfxToPlay;
        //}

        //Get's list of SFX from manager, used in the inspector
        private List<SoundClip> SFXType()
        {
            List<SoundClip> SfxList;

            switch (SfxType)
            {
                case SoundManager.SoundType.UI:
                    SfxList = SoundManager.Instance.uiSFX;
                    break;
                case SoundManager.SoundType.Ambient:
                    SfxList = SoundManager.Instance.ambientSFX;
                    break;
                case SoundManager.SoundType.Weapons:
                    SfxList = SoundManager.Instance.weaponSFX;
                    break;
                default:
                    SfxList = SoundManager.Instance.uiSFX;
                    break;
            }

            return SfxList;
        }

        public void PlaySFX()
        {
            if (UseDefault || Audiosource == null)
                SoundManager.PlaySFX(SfxToPlay, WaitToPlay, null);
            else
                SoundManager.PlaySFX(SfxToPlay, WaitToPlay, Audiosource);
        }
    }
}
