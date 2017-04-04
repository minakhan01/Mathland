using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomPropertyDrawer(typeof(ForceField2D.FFConstantField))]
public class ConstantField2DDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUILayout.Space(-15f);
        EditorGUI.BeginProperty(position, label, property);
        if (FFEditorToolKit.DrawHeader(label.text))
        {
            FFEditorToolKit.BeginContents();

            EditorGUILayout.PropertyField(property.FindPropertyRelative("force"));

            SerializedProperty direction = property.FindPropertyRelative("_direction");
            FFEditorToolKit.DrawNormalizedVector(direction, "Direction");

            FFEditorToolKit.EndContents();
        }
        EditorGUI.EndProperty();
    }
}