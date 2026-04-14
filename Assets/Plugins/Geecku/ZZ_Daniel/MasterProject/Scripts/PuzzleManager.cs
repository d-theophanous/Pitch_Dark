using Geecku.GlobalMangers;
using UnityEngine;

namespace Daniel.Master
{
    public class PuzzleManager : PersistantDSingleton<PuzzleManager>
    {

        public void StartPuzzle()
        {
            Debug.Log("puzzle started");
        }
    }
}
