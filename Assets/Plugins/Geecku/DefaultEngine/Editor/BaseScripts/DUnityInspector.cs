using UnityEditor;
using UnityEngine;

namespace Geecku.DefaultEngine.EditorScripts
{
    public abstract class DUnityInspector<T> : Editor where T : MonoBehaviour
    {
        protected T Script => target as T;

        private bool battle_group = true;
        private void Test()
        {


            float width = EditorGUIUtility.currentViewWidth;
            float half_width = width / 2f;

            //Draw(Script.WasLoaded, "Was loaded from Char");

            battle_group = EditorGUILayout.Foldout(battle_group, "Battle Stats");
            if (battle_group)
            {
                float header_width = 120;
                float content_width = 40;

                EditorGUILayout.BeginVertical("box");

                //EditorGUILayout.LabelField("Battle Stats", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("", GUILayout.MinWidth(header_width));
                EditorGUILayout.LabelField("Cur", GUILayout.MinWidth(content_width), GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField("Max", GUILayout.MinWidth(content_width), GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField("Base", GUILayout.MinWidth(content_width), GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();

                //EditorGUILayout.BeginHorizontal();
                //GUI.enabled = false;
                //EditorGUILayout.LabelField("HP", GUILayout.MinWidth(header_width));
                //Draw(script.CurHP, width: content_width);
                //Draw(script.MaxHP, width: content_width);
                //Draw(script.BaseHP, width: content_width);
                //GUI.enabled = true;
                //EditorGUILayout.EndHorizontal();

                //EditorGUILayout.BeginHorizontal();
                //GUI.enabled = false;
                //EditorGUILayout.LabelField("ATK", GUILayout.MinWidth(header_width));
                //Draw(script.CurATK, width: content_width);
                //Draw("-", width: content_width);
                //Draw(script.BaseATK, width: content_width);
                //GUI.enabled = true;
                //EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
        }

        protected void Draw(string var, string name = "", float width = -1)
        {
            if (string.IsNullOrEmpty(name))
                var = EditorGUILayout.DelayedTextField(var, width == -1 ? null : GUILayout.MinWidth(width), GUILayout.ExpandWidth(true));
            else
                var = EditorGUILayout.DelayedTextField(name, var, width == -1 ? null : GUILayout.MinWidth(width), GUILayout.ExpandWidth(true));
        }
        protected void Draw(int var, string name = "", float width = -1)
        {
            if (string.IsNullOrEmpty(name))
                var = EditorGUILayout.DelayedIntField(var, width == -1 ? null : GUILayout.MinWidth(width), GUILayout.ExpandWidth(true));
            else
                var = EditorGUILayout.DelayedIntField(name, var, width == -1 ? null : GUILayout.MinWidth(width), GUILayout.ExpandWidth(true));
        }
        protected void Draw(float var, string name = "", float width = -1)
        {
            if (string.IsNullOrEmpty(name))
                var = EditorGUILayout.DelayedFloatField(var, width == -1 ? null : GUILayout.MinWidth(width), GUILayout.ExpandWidth(true));
            else
                var = EditorGUILayout.DelayedFloatField(name, var, width == -1 ? null : GUILayout.MinWidth(width), GUILayout.ExpandWidth(true));
        }
        protected void Draw(ushort var, string name = "", float width = -1)
        {
            if (string.IsNullOrEmpty(name))
                var = (ushort)EditorGUILayout.DelayedIntField(var, width == -1 ? null : GUILayout.MinWidth(width), GUILayout.ExpandWidth(true));
            else
                var = (ushort)EditorGUILayout.DelayedIntField(name, var, width == -1 ? null : GUILayout.MinWidth(width), GUILayout.ExpandWidth(true));
        }
        protected void Draw(byte var, string name = "")
        {
            if (string.IsNullOrEmpty(name))
                var = (byte)EditorGUILayout.DelayedIntField(var);
            else
                var = (byte)EditorGUILayout.DelayedIntField(name, var);
        }
        protected void Draw(bool var, string name = "", float width = -1)
        {
            GUI.enabled = false;
            if (string.IsNullOrEmpty(name))
                var = EditorGUILayout.Toggle(var);
            else
                var = EditorGUILayout.Toggle(name, var);
            GUI.enabled = true;
        }
    }
}
