using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomPropertyDrawer(typeof(ForceField2D.FieldFunction))]
public class FieldFunction2DDrawer : PropertyDrawer
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
            case (int)ForceField2D.FieldFunction.EFieldFunctionType.Simple:
                SerializedProperty simpleFieldType = property.FindPropertyRelative("simpleFieldType");
                EditorGUILayout.PropertyField(simpleFieldType);
                switch (simpleFieldType.enumValueIndex)
                {
                    case (int)ForceField2D.FieldFunction.ESimpleFieldType.ConstantForce:
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("constantField"), true);
                        break;
                    case (int)ForceField2D.FieldFunction.ESimpleFieldType.CentripetalForce:
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("centripetalField"), true);
                        break;
                    case (int)ForceField2D.FieldFunction.ESimpleFieldType.AxipetalForce:
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("axipetalField"), true);
                        break;
                }
                break;
            case (int)ForceField2D.FieldFunction.EFieldFunctionType.Custom:
                EditorGUILayout.PropertyField(property.FindPropertyRelative("customFieldOption"), true);
                break;
        }
        EditorGUI.EndProperty();
    }
}

