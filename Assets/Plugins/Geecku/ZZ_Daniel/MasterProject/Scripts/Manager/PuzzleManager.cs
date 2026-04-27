using Geecku.GlobalMangers;
using UnityEngine;

namespace Daniel.Master
{
    public class PuzzleManager : PersistantDSingleton<PuzzleManager>
    {
        //- Camera aufsetzen die in Render texture outputet, die in Puzzle UI ist
        //- Simple puzzle UI aufsetzen
        //- ReadableGroup aufsetzen
        //- lösungsloop aufsetzen
        //- Tür öffnen logic einbauen
        //- collider nach durchgehen einbauen

        public void StartPuzzle(Transform camera_position)
        {
            Debug.Log("puzzle started");
        }
    }
}
