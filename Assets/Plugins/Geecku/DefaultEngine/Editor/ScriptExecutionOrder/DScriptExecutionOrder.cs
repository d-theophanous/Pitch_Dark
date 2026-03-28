using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public static class DScriptExecutionOrder
{
    //[MenuItem("Tools/Find ScriptableObject In Same Folder")]
    //public static void FindSO()
    //{
    //    // Suche nach allen ScriptableObjects im gleichen Ordner
    //}
    static DScriptExecutionOrder()
    {
        var guids = AssetDatabase.FindAssets("t:" + nameof(DScriptExecutionObject));

        DScriptExecutionObject info = null;
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            info = AssetDatabase.LoadAssetAtPath<DScriptExecutionObject>(path);
            break;
        }
        if (info == null)
        {
            Debug.LogError("No default ScriptExecutionOrder Scriptable object was found.");
            return;
        }
        var list = info.Scripts;

        int num_of_changes = 0;
        foreach (var script in MonoImporter.GetAllRuntimeMonoScripts())
        {
            if (script == null || script.GetClass() == null)
                continue;

            var name = script.GetClass().Name;
            bool dont_search_further = false;
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].ScriptName != name)
                    continue;
                var local_info = list[i];
                if (MonoImporter.GetExecutionOrder(script) != local_info.OrderValue)
                {
                    MonoImporter.SetExecutionOrder(script, local_info.OrderValue);
                    dont_search_further = true;
                    num_of_changes++;
                }
                break;
            }
            if (dont_search_further)
                break;
        }
        if (num_of_changes > 0)
            Debug.Log("Script Execution Order changed of #" + num_of_changes + " scripts.");
    }
}
