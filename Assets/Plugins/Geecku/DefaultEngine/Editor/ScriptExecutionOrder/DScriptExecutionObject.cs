using Sirenix.OdinInspector;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DScriptExecutionObject", menuName = "Scriptable Objects/DEditor/DScriptExecutionObject")]
public class DScriptExecutionObject : ScriptableObject
{
    public ExecutionOrderInfo[] Scripts = new ExecutionOrderInfo[0];

    [Serializable]
    public struct ExecutionOrderInfo
    {
        public string ScriptName;
        public int OrderValue;
    }
}
