using NUnit.Framework.Constraints;
using System.Collections;
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
    }
}
