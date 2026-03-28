using Sirenix.OdinInspector;
using UnityEngine;

namespace Geecku.DefaultEngine.Common.Sound
{
    [CreateAssetMenu(fileName = "SoundClip", menuName = "Scriptable Objects/SoundClip")]
    public class SoundClip : ScriptableObject
    {
        [Space, Title("Audio Clip"), Required]
        public AudioClip Clip;

        [Title("Settings"), Range(0f, 1f)]
        public float Volume = 1f;
        [Range(0f, 0.2f)]
        public float VolumeVariation = 0.05f;
        [Range(0f, 2f)]
        public float Pitch = 1f;
        [Range(0f, 0.2f)]
        public float PitchVariation = 0.05f;
    }
}
