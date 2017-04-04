using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(ForceField))]
public class ForceFieldEditor : Editor
{
    ForceField forceField;
    SerializedProperty alwaysIgnoredList;
    SerializedProperty selectionMethods;
    SerializedProperty fieldFunctions;
    SerializedProperty layerMask;
    SerializedProperty sendMessage;
    SerializedProperty ignoreMass;
    SerializedProperty showTooltips;
    SerializedProperty gizmosMode;
    SerializedProperty drawTargetsConnection;
    SerializedProperty drawFieldPointers;
    SerializedProperty drawRaycastArea;
    SerializedProperty targetsConnectionColor;
    SerializedProperty testObject;
    SerializedProperty pointerLength;
    SerializedProperty pointerSpace;
    SerializedProperty pointerXCount;
    SerializedProperty pointerYCount;
    SerializedProperty pointerZCount;
    SerializedProperty strongPointerColor;
    SerializedProperty weakPointerColor;
    SerializedProperty strongThreshold;
    SerializedProperty weakThreshold;
    SerializedProperty raycastColor;
    SerializedProperty generalMultiplier;
    SerializedProperty useMassCenter;
    SerializedProperty useLocalSpace;

    static string selectionMethodsEmptyWarning =
        "You need at least one SelectionMethod to allow the force field choose its targets. Click the Add button below to add one.";
    static string fieldFunctionsEmptyWarning =
        "You need at least one FieldFunction to apply the force on the targets of the field. Click the Add button below to add one.";

    static string addSelectionMethodButtonTooltip = "Click this button to add a new selection method.";
    static string removeSelectionMethodButtonTooltip = "Click this button to remove this selection method.";
    static string duplicateSelectionMethodButtonTooltip = "Click this button to duplicate this selection method";
    static string addFieldFunctionButtonTooltip = "Click this button to add a new field function.";
    static string removeFieldFunctionButtonTooltip = "Click this button to remove this field function.";
    static string duplicateFieldFunctionButtonTooltip = "Click this button to duplicate this field function";

    void OnEnable()
    {
        forceField = target as ForceField;
        alwaysIgnoredList = serializedObject.FindProperty("alwaysIgnoredList");
        selectionMethods = serializedObject.FindProperty("selectionMethods");
        fieldFunctions = serializedObject.FindProperty("fieldFunctions");
        layerMask = serializedObject.FindProperty("layerMask");
        sendMessage = serializedObject.FindProperty("sendMessage");
        ignoreMass = serializedObject.FindProperty("ignoreMass");
        showTooltips = serializedObject.FindProperty("showTooltips");
        gizmosMode = serializedObject.FindProperty("gizmosMode");
        drawTargetsConnection = serializedObject.FindProperty("drawTargetsConnection");
        drawFieldPointers = serializedObject.FindProperty("drawFieldPointers");
        drawRaycastArea = serializedObject.FindProperty("drawRaycastArea");
        targetsConnectionColor = serializedObject.FindProperty("targetsConnectionColor");
        testObject = serializedObject.FindProperty("testObject");
        pointerLength = serializedObject.FindProperty("pointerLength");
        pointerSpace = serializedObject.FindProperty("pointerSpace");
        pointerXCount = serializedObject.FindProperty("pointerXCount");
        pointerYCount = serializedObject.FindProperty("pointerYCount");
        pointerZCount = serializedObject.FindProperty("pointerZCount");
        strongPointerColor = serializedObject.FindProperty("strongPointerColor");
        weakPointerColor = serializedObject.FindProperty("weakPointerColor");
        strongThreshold = serializedObject.FindProperty("strongThreshold");
        weakThreshold = serializedObject.FindProperty("weakThreshold");
        raycastColor = serializedObject.FindProperty("raycastColor");
        generalMultiplier = serializedObject.FindProperty("generalMultiplier");
        useMassCenter = serializedObject.FindProperty("useMassCenter");
        useLocalSpace = serializedObject.FindProperty("useLocalSpace");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.Space();


        if (FFEditorToolKit.DrawHeader("Selection Method Options"))
        {
            FFEditorToolKit.BeginContents();
            EditorGUI.indentLevel++;

            FFEditorToolKit.DrawPropertyWithChangeCheck(sendMessage);
            FFEditorToolKit.DrawPropertyWithChangeCheck(alwaysIgnoredList);
            FFEditorToolKit.DrawPropertyWithChangeCheck(layerMask);
            FFEditorToolKit.DrawListSpecial(selectionMethods, itemLabel: "SelectionMethod", emptyWarning: selectionMethodsEmptyWarning, noContainer: true,
                addButtonTooltip: addSelectionMethodButtonTooltip, removeButtonTooltip: removeSelectionMethodButtonTooltip, duplicateButtonTooltip: duplicateSelectionMethodButtonTooltip,
                addFunc: forceField.AddSelectionMethod);

            EditorGUI.indentLevel--;
            FFEditorToolKit.EndContents();
        }

        if (FFEditorToolKit.DrawHeader("Field Function Options"))
        {
            FFEditorToolKit.BeginContents();
            EditorGUI.indentLevel++;

            FFEditorToolKit.DrawPropertyWithChangeCheck(ignoreMass);
            FFEditorToolKit.DrawPropertyWithChangeCheck(useMassCenter);
            FFEditorToolKit.DrawPropertyWithChangeCheck(generalMultiplier);
            FFEditorToolKit.DrawListSpecial(fieldFunctions, itemLabel: "FieldFunction", emptyWarning: fieldFunctionsEmptyWarning, noContainer: true,
                addButtonTooltip: addFieldFunctionButtonTooltip, removeButtonTooltip: removeFieldFunctionButtonTooltip, duplicateButtonTooltip: duplicateFieldFunctionButtonTooltip,
                addFunc: forceField.AddFieldFunction);

            EditorGUI.indentLevel--;
            FFEditorToolKit.EndContents();
        }

        if (FFEditorToolKit.DrawHeader("Visualization Options"))
        {
            FFEditorToolKit.BeginContents();
            EditorGUI.indentLevel++;

            FFEditorToolKit.DrawPropertyWithChangeCheck(showTooltips);
            FFEditorToolKit.DrawPropertyWithChangeCheck(gizmosMode);

            FFEditorToolKit.DrawPropertyWithChangeCheck(drawTargetsConnection);
            FFEditorToolKit.DrawPropertyWithChangeCheck(drawFieldPointers);
            FFEditorToolKit.DrawPropertyWithChangeCheck(drawRaycastArea);

            if (FFEditorToolKit.DrawHeader("Targets Connection Options", defaultState: false))
            {
                FFEditorToolKit.BeginContents();
                FFEditorToolKit.DrawPropertyWithChangeCheck(targetsConnectionColor, new GUIContent("Color"));
                FFEditorToolKit.EndContents();
            }

            if (FFEditorToolKit.DrawHeader("Field Pointer Options", defaultState: false))
            {
                FFEditorToolKit.BeginContents();
                FFEditorToolKit.DrawPropertyWithChangeCheck(testObject);
                FFEditorToolKit.DrawPropertyWithChangeCheck(useLocalSpace);
                FFEditorToolKit.DrawPropertyWithChangeCheck(pointerLength);
                FFEditorToolKit.DrawPropertyWithChangeCheck(pointerSpace);
                FFEditorToolKit.DrawPropertyWithChangeCheck(pointerXCount);
                FFEditorToolKit.DrawPropertyWithChangeCheck(pointerYCount);
                FFEditorToolKit.DrawPropertyWithChangeCheck(pointerZCount);
                FFEditorToolKit.DrawPropertyWithChangeCheck(strongThreshold);
                FFEditorToolKit.DrawPropertyWithChangeCheck(strongPointerColor);
                FFEditorToolKit.DrawPropertyWithChangeCheck(weakThreshold);
                FFEditorToolKit.DrawPropertyWithChangeCheck(weakPointerColor);
                FFEditorToolKit.EndContents();
            }

            if (FFEditorToolKit.DrawHeader("Raycast Area Options", defaultState: false))
            {
                FFEditorToolKit.BeginContents();
                FFEditorToolKit.DrawPropertyWithChangeCheck(raycastColor, new GUIContent("Color"));
                FFEditorToolKit.EndContents();
            }
            EditorGUI.indentLevel--;
            FFEditorToolKit.EndContents();
        }
    }

}




