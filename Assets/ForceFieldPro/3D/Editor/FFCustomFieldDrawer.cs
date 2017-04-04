using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomPropertyDrawer(typeof(ForceField.FFCustomOption))]
public class FFCustomFieldDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUILayout.Space(-15f);
        EditorGUI.BeginProperty(position, label, property);
        if (FFEditorToolKit.DrawHeader(label.text))
        {
            FFEditorToolKit.BeginContents();
            SerializedProperty fieldFunction = property.FindPropertyRelative("fieldFunction");
            if (fieldFunction.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Attach a component that extends ForceField.CustomFieldFunction to use this option.", MessageType.Warning);
            }
            EditorGUILayout.PropertyField(fieldFunction);

            FFEditorToolKit.EndContents();
        }
        EditorGUI.EndProperty();
    }
}
