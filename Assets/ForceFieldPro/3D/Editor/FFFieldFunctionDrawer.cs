using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomPropertyDrawer(typeof(ForceField.FieldFunction))]
public class FieldFunctionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        EditorGUI.BeginProperty(position, label, property);
        GUILayout.Space(-15f);

        EditorGUILayout.PropertyField(property.FindPropertyRelative("enabled"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("useLocalCoordination"));
        SerializedProperty functionType = property.FindPropertyRelative("fieldFunctionType");
        EditorGUILayout.PropertyField(functionType);
        switch (functionType.enumValueIndex)
        {
            case (int)ForceField.FieldFunction.EFieldFunctionType.Simple:
                SerializedProperty simpleFieldType = property.FindPropertyRelative("simpleFieldType");
                EditorGUILayout.PropertyField(simpleFieldType);
                switch (simpleFieldType.enumValueIndex)
                {
                    case (int)ForceField.FieldFunction.ESimpleFieldType.ConstantForce:
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("constantField"), true);
                        break;
                    case (int)ForceField.FieldFunction.ESimpleFieldType.CentripetalForce:
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("centripetalField"), true);
                        break;
                    case (int)ForceField.FieldFunction.ESimpleFieldType.AxipetalForce:
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("axipetalField"), true);
                        break;
                    case (int)ForceField.FieldFunction.ESimpleFieldType.PerpendicularForce:
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("perpendicularField"), true);
                        break;
                }
                break;
            case (int)ForceField.FieldFunction.EFieldFunctionType.Custom:
                EditorGUILayout.PropertyField(property.FindPropertyRelative("customFieldOption"), true);
                break;
        }

        EditorGUI.EndProperty();
    }
}

