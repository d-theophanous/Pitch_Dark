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
    }
}
