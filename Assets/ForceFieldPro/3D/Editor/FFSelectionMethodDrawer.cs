using UnityEngine;
using System.Collections;
using UnityEditor;


[CustomPropertyDrawer(typeof(ForceField.SelectionMethod))]
public class SelectionMethodDrawer : PropertyDrawer
{
    static string noColliderWarning = "To use the collider mode, you need to attach a collider.\nIf you want to use children's collider, you need to attach a rigidbody.";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        GUILayout.Space(-15f);

        EditorGUILayout.PropertyField(property.FindPropertyRelative("enabled"));
        SerializedProperty targetingMode = property.FindPropertyRelative("targetingMode");
        EditorGUILayout.PropertyField(targetingMode);
        switch (targetingMode.enumValueIndex)
        {
            case (int)ForceField.SelectionMethod.ETargetingMode.Collider:
                ForceField ff = property.serializedObject.targetObject as ForceField;
                if (ff.GetComponent<Collider>() == null && ff.GetComponent<Rigidbody>() == null)
                {
                    EditorGUILayout.HelpBox(noColliderWarning, MessageType.Warning);
                }
                break;
            case (int)ForceField.SelectionMethod.ETargetingMode.Raycast:
                EditorGUILayout.PropertyField(property.FindPropertyRelative("rayCastOption"), true);
                break;
            case (int)ForceField.SelectionMethod.ETargetingMode.Manual:
                EditorGUILayout.PropertyField(property.FindPropertyRelative("targetsList"), true);
                break;
        }

        EditorGUI.EndProperty();
    }
}

