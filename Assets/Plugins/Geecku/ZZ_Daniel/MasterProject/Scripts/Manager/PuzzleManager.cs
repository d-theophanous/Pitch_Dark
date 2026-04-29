using Geecku.GlobalMangers;
using System.Collections.Generic;
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

        [SerializeField] private List<PuzzleData> PuzzleList;

        private PuzzleData CurPuzzle => PuzzleList[PuzzleCount];
        private int PuzzleCount = 0;
        private Camera CurCamera;

        public void StartPuzzle()
        {
            Debug.Log("IN Start Puzzle: " + CurPuzzle);
            Debug.Log("IN Start Puzzle1: " + CurPuzzle.Group);
            Debug.Log("IN Start Puzzle2: " + CurPuzzle.UI);
            CurCamera.gameObject.SetActive(true);

            SetUpPuzzle();
        }
        private void SetUpPuzzle()
        {
            //- assign right group for TTSManager to open
            TTSManager.Instance.SetPuzzleGroup(CurPuzzle.Group);

            GlobalUIManager.Instance.ToggleUI(UI_Group.PUZZLE, false);
            CurPuzzle.UI.SetActive(true);            
        }
        public void SetCameraTransform(Camera camera) { CurCamera = camera; }
    }
}
