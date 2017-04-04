using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomPropertyDrawer(typeof(ForceField.FFCentripetalField))]
public class FFCentripetalFieldDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUILayout.Space(-15f);
        EditorGUI.BeginProperty(position, label, property);
        if (FFEditorToolKit.DrawHeader(label.text))
        {
            FFEditorToolKit.BeginContents();

            EditorGUILayout.PropertyField(property.FindPropertyRelative("referencePoint"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("force"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("distanceModifier"));

            FFEditorToolKit.EndContents();
        }
        EditorGUI.EndProperty();
    }
}