using Geecku.GlobalMangers;
using UnityEngine;
using FMODUnity;

namespace Daniel.Master
{
    public class FMODEvents : PersistantDSingleton<FMODEvents>
    {
        [field: Header("Notes")]
        [field: SerializeField] public EventReference Note { get; private set; }

        [field: Header("Improvisation Track")]
        [field: SerializeField] public EventReference ImprovisationTrack { get; private set; }
        [field: Header("Dialogue")]
        [field: SerializeField] public EventReference Dialogue { get; private set; }

        [field: SerializeField] public EventReference OneShotEvent { get; private set; }
        [field: SerializeField] public EventReference WallScratchEvent { get; private set; }
        [field: SerializeField] public EventReference WallFaceEvent { get; private set; }
        [field: SerializeField] public EventReference FootstepEvent { get; private set; }
        [field: SerializeField] public EventReference InteractableEvent { get; private set; }
        [field: SerializeField] public EventReference TTSEvent { get; private set; }
    }
}
