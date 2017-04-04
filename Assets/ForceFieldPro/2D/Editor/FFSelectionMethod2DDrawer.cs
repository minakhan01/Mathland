using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomPropertyDrawer(typeof(ForceField2D.SelectionMethod))]
public class SelectionMethod2DDrawer : PropertyDrawer
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
            case (int)ForceField2D.SelectionMethod.ETargetingMode.Collider2D:
                ForceField2D ff = property.serializedObject.targetObject as ForceField2D;
                if (ff.GetComponent<Collider2D>() == null && ff.GetComponent<Rigidbody2D>() == null)
                {
                    EditorGUILayout.HelpBox(noColliderWarning, MessageType.Warning);
                }
                break;
            case (int)ForceField2D.SelectionMethod.ETargetingMode.Raycast:
                EditorGUILayout.PropertyField(property.FindPropertyRelative("rayCastOption"), true);
                break;
            case (int)ForceField2D.SelectionMethod.ETargetingMode.Manual:
                EditorGUILayout.PropertyField(property.FindPropertyRelative("targetsList"), true);
                break;
        }
        EditorGUI.EndProperty();
    }
}

