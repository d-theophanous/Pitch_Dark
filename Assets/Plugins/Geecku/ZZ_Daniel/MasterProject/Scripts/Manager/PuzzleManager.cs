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

        private int PuzzleCount;
        private Camera CurCamera;
        [SerializeField] private RenderTexture Texture;

        public void StartPuzzle()
        {
            Debug.Log("puzzle started");
            CurCamera.gameObject.SetActive(true);
            
            //- weitermachen
            //- global ui manager puzzle geben 
            //- readable groups aufsetzen
            //- 
        }
        public void SetCameraTransform(Camera camera) { CurCamera = camera; }
    }
}
