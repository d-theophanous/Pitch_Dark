using System.Collections.Generic;
using UnityEngine;

namespace Daniel.Master
{
    [CreateAssetMenu(
        fileName = "PuzzleData",
        menuName = "Puzzle Data",
        order = 0)]
    public class PuzzleData : ScriptableObject
    {
        public ReadableElementGroup Group;
        public GameObject UI;
    }
}
