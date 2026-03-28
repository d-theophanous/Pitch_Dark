using Geecku.DefaultEngine.Common.Sound;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using static Geecku.Defines;

namespace Geecku.GlobalMangers
{
    public class SoundManager : Singleton<SoundManager>
    {
        public enum SoundType
        {
            UI,
            Ambient,
            Weapons
        }

        [HorizontalGroup("AudioSource"), SerializeField]
        private AudioSource Audio;

        [TabGroup("UI"), AssetList(Path = PATH_AUDIO + "/UI", AutoPopulate = true)]
        public List<SoundClip> uiSFX;
        [TabGroup("Ambient"), AssetList(Path = PATH_AUDIO + "/Ambient", AutoPopulate = true)]
        public List<SoundClip> ambientSFX;
        [TabGroup("Weapons"), AssetList(Path = PATH_AUDIO + "/Weapons", AutoPopulate = true)]
        public List<SoundClip> weaponSFX;

        protected override void Awake()
        {
            base.Awake();
            if (WillBeDestroyed)
                return;
        }
        protected override void Start()
        {
            base.Start();
            if (WillBeDestroyed)
                return;
        }

        public static void PlaySFX(SoundClip sfx, bool waitToFinish = true, AudioSource audioSource = null)
        {
            if (audioSource == null)
                audioSource = SoundManager.Instance.Audio;

            if (audioSource == null)
            {
                Debug.LogError("You forgot to add a default audio source!");
                return;
            }

            if (!audioSource.isPlaying || !waitToFinish)
            {
                audioSource.clip = sfx.Clip;
                audioSource.volume = sfx.Volume + Random.Range(-sfx.VolumeVariation, sfx.VolumeVariation);
                audioSource.pitch = sfx.Pitch + Random.Range(-sfx.PitchVariation, sfx.PitchVariation);
                audioSource.Play();
            }
        }

        [HorizontalGroup("AudioSource"), ShowIf("@Audio == null"), GUIColor(1f, 0.5f, 0.5f, 1f), Button]
        private void AddAudioSource()
        {
            Audio = this.gameObject.GetComponent<AudioSource>();

            if (Audio == null)
                Audio = this.gameObject.AddComponent<AudioSource>();
        }

    }
}
