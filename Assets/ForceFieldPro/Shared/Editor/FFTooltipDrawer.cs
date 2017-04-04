using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(FFToolTip))]
public class FFToolTipDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent oldLabel)
    {
        FFToolTip labelAttribute = attribute as FFToolTip;
        GUIContent label;
        SerializedProperty showFlag = property.serializedObject.FindProperty("showTooltips");
        if (showFlag != null && showFlag.propertyType == SerializedPropertyType.Boolean && !showFlag.boolValue)
        {
            label = new GUIContent(oldLabel.text);
        }
        else
        {
            label = new GUIContent(oldLabel.text, labelAttribute.tooltip);
        }
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.PropertyField(position, property, label, true);
        EditorGUI.EndProperty();
    }
}