using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Daniel.Master
{
    public static class Helper
    {
        public static string GetLanguageString()
        {
            switch (GameManager.Language)
            {
                case Language.ENGLISH:
                    return "en";
                case Language.GERMAN:
                    return "de";
                case Language.DUTCH:
                    return "nl";
                default:
                    return "";
            }
        }
        public static int GetLoopingIndex<T>(List<T> list, int index)
        {
            if (index >= 0 && index < list.Count) 
                return index;
            int new_idx = list.Count;
            if (index < 0)
            {
                new_idx += index;
            }
            else if (index >= list.Count)
            {
                new_idx -= list.Count;
            }
            return new_idx;
        }
    }
}
