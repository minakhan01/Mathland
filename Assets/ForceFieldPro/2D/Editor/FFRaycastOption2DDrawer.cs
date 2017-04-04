using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomPropertyDrawer(typeof(ForceField2D.RaycastOption))]
public class RaycastOption2DDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        GUILayout.Space(-15f);
        if (FFEditorToolKit.DrawHeader(label.text))
        {
            FFEditorToolKit.BeginContents();
            SerializedProperty useAnchor = property.FindPropertyRelative("useAnchor");
            SerializedProperty raycastType = property.FindPropertyRelative("raycastType");
            EditorGUILayout.PropertyField(raycastType);

            switch (raycastType.enumValueIndex)
            {
                case (int)ForceField2D.RaycastOption.ERayCastMode.RayCast:
                    EditorGUILayout.PropertyField(useAnchor);
                    if (useAnchor.boolValue)
                    {
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("anchor"));
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("numberLimit"));
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("useLocalDirection"));
                        FFEditorToolKit.DrawNormalizedVector(property.FindPropertyRelative("_direction"), "Direction");
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("numberLimit"));
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("distance"));
                    }
                    break;
                case (int)ForceField2D.RaycastOption.ERayCastMode.OverLapArea:
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("useLocalDirection"));
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("point1"));
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("point2"));
                    break;
                case (int)ForceField2D.RaycastOption.ERayCastMode.OverLapCircle:
                    EditorGUILayout.PropertyField(useAnchor);
                    if (useAnchor.boolValue)
                    {
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("anchor"));
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("radius"));
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("useLocalDirection"));
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("center"));
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("radius"));
                    }
                    break;
                case (int)ForceField2D.RaycastOption.ERayCastMode.OverLapPoint:
                    EditorGUILayout.PropertyField(useAnchor);
                    if (useAnchor.boolValue)
                    {
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("anchor"));
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("useLocalDirection"));
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("center"));
                    }
                    break;
            }
            FFEditorToolKit.EndContents();
        }
        EditorGUI.EndProperty();
    }
}