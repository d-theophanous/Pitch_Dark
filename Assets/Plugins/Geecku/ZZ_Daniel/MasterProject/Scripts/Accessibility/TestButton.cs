using NUnit.Framework.Interfaces;
using System.Collections;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.UI;

namespace Daniel.Master
{
    public class TestButton : MonoBehaviour
    {
        //private Button Button;
        //// Start is called once before the first execution of Update after the MonoBehaviour is created
        //void Start()
        //{
        //    StartCoroutine(StartCo());
        //}
        //private IEnumerator StartCo()
        //{
        //    yield return new WaitForEndOfFrame();

        //    var node = AccessibilityManager.Instance.Hierarchy.AddNode();
        //    node.label = "Test";
        //    node.hint = "Test2";
        //    node.role = AccessibilityRole.Button;
        //    node.isActive = true;
        //    node.frameGetter = () => AccessibilityManager.GetFrame(GetComponent<RectTransform>());

        //    AssistiveSupport.notificationDispatcher.SendLayoutChanged(node);
        //    AssistiveSupport.activeHierarchy = AccessibilityManager.Instance.Hierarchy;
        //}

        ////- Debug
        //public void ShowNodes()
        //{
        //    Debug.Log(AccessibilityManager.Instance.Hierarchy.rootNodes.Count);
        //    Debug.Log(AssistiveSupport.isScreenReaderEnabled);
        //}
    }
}
