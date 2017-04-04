using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomPropertyDrawer(typeof(ForceField.RaycastOption))]
public class RaycastOptionDrawer : PropertyDrawer
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

            EditorGUILayout.PropertyField(useAnchor);
            if (useAnchor.boolValue)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("anchor"));
                switch (raycastType.enumValueIndex)
                {
                    case (int)ForceField.RaycastOption.ERayCastMode.SphereCast:
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("radius"));
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("numberLimit"));
                        break;
                    case (int)ForceField.RaycastOption.ERayCastMode.RayCast:
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("numberLimit"));
                        break;
                    case (int)ForceField.RaycastOption.ERayCastMode.OverLapSphere:
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("radius"));
                        break;
                }
            }
            else
            {
                switch (raycastType.enumValueIndex)
                {
                    case (int)ForceField.RaycastOption.ERayCastMode.SphereCast:
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("useLocalDirection"));
                        FFEditorToolKit.DrawNormalizedVector(property.FindPropertyRelative("_direction"), "Direction");
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("radius"));
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("numberLimit"));
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("distance"));
                        break;
                    case (int)ForceField.RaycastOption.ERayCastMode.RayCast:
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("useLocalDirection"));
                        FFEditorToolKit.DrawNormalizedVector(property.FindPropertyRelative("_direction"), "Direction");
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("numberLimit"));
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("distance"));
                        break;
                    case (int)ForceField.RaycastOption.ERayCastMode.OverLapSphere:
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("useLocalDirection"));
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("sphereCenter"));
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("radius"));
                        break;
                }

            }
            FFEditorToolKit.EndContents();
        }
        EditorGUI.EndProperty();
    }
}