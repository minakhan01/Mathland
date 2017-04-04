using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GUIScript))]
public class GUIScriptEditor : Editor
{

    public override void OnInspectorGUI()
    {
        GUIScript g = target as GUIScript;
        g.helpInfo = EditorGUILayout.TextArea(g.helpInfo);
    }
}
