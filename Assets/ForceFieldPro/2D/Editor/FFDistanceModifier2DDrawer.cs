using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomPropertyDrawer(typeof(ForceField2D.DistanceModifier))]
public class DistanceModifier2DDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUILayout.Space(-15f);
        EditorGUI.BeginProperty(position, label, property);
        if (FFEditorToolKit.DrawHeader(label.text))
        {
            FFEditorToolKit.BeginContents();

            SerializedProperty modifierType = property.FindPropertyRelative("modifierType");
            EditorGUILayout.PropertyField(modifierType);
            if (modifierType.enumValueIndex == (int)ForceField2D.DistanceModifier.EDistanceModifier.AnimationCurve)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("boundAtZero"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("curveSize"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("curve"));
            }
            else if (modifierType.enumValueIndex != (int)ForceField2D.DistanceModifier.EDistanceModifier.Constant)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("boundAtZero"));
                EditorGUILayout.HelpBox("The modifier will be: (A*distance+B)^N", MessageType.None);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("a"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("b"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("n"));
            }
            FFEditorToolKit.EndContents();
        }
        EditorGUI.EndProperty();
    }
}