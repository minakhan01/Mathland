using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;


public static class FFEditorToolKit
{

    public static string[] GetLayerNames()
    {
        List<string> layers = new List<string>();
        for (int i = 0; i < 32; i++)
        {
            string layer = LayerMask.LayerToName(i);
            if (layer.Length > 0)
            {
                layers.Add(layer);
            }
        }
        return layers.ToArray();
    }

    public static void DrawPropertyWithChangeCheck(SerializedProperty property, GUIContent label = null, bool includeChildren = true, params GUILayoutOption[] options)
    {
        EditorGUI.BeginChangeCheck();
        if (label == null)
        {
            EditorGUILayout.PropertyField(property, true, options);
        }
        else
        {
            EditorGUILayout.PropertyField(property, label, true, options);
        }
        if (EditorGUI.EndChangeCheck())
        {
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    public static bool DrawHeader(string text, string key = null, bool forceOn = false, bool defaultState = true)
    {
        if (key == null)
        {
            key = "ForceField." + text;
        }
        bool state = EditorPrefs.GetBool(key, defaultState);
        GUILayout.Space(3f);
        if (!forceOn && !state)
        {
            GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
        }
        //draw label
        GUILayout.BeginHorizontal();
        GUILayout.Space(2f);
        bool flag = !GUILayout.Toggle(true, "<b><size=11>" + text + "</size></b>", "DragTab", GUILayout.MinWidth(20f));
        if (forceOn)
        {
            return true;
        }
        if (flag)
        {
            state = !state;
            EditorPrefs.SetBool(key, state);
        }
        GUILayout.Space(2f);
        GUILayout.EndHorizontal();

        GUI.backgroundColor = Color.white;
        if (!forceOn && !state)
        {
            GUILayout.Space(3f);
        }
        return state;
    }

    public static void BeginContents()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(4f);
        EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
        GUILayout.BeginVertical();
        GUILayout.Space(2f);
    }

    public static void EndContents()
    {
        GUILayout.Space(3f);
        GUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(3f);
        GUILayout.EndHorizontal();
        GUILayout.Space(3f);
    }

    public static void DrawListSpecial(SerializedProperty property, string label = null, string itemLabel = null,
        string emptyInfo = null, string emptyWarning = null, string emptyError = null, bool forceOn = false, bool noContainer = false,
        bool drawAddButtons = true, bool drawRemoveButtons = true, bool drawDuplicateButtons = true,
        string addButtonTooltip = null, string removeButtonTooltip = null, string duplicateButtonTooltip = null, Func<System.Object> addFunc = null)
    {
        if (property.isArray)
        {
            if (label == null)
            {
                label = property.name;
            }
            if (itemLabel == null)
            {
                itemLabel = property.name;
            }
            bool header = false;
            if (!noContainer)
            {
                header = DrawHeader(label, forceOn: forceOn);
            }
            if (header || noContainer)
            {
                if (!noContainer)
                {
                    BeginContents();
                }
                int length = property.arraySize;
                if (length == 0)
                {
                    if (emptyInfo != null)
                    {
                        EditorGUILayout.HelpBox(emptyInfo, MessageType.Info);
                    }
                    if (emptyWarning != null)
                    {
                        EditorGUILayout.HelpBox(emptyWarning, MessageType.Warning);
                    }
                    if (emptyError != null)
                    {
                        EditorGUILayout.HelpBox(emptyError, MessageType.Error);
                    }
                }
                EditorGUILayout.BeginVertical();
                for (int i = 0; i < length; i++)
                {
                    if (DrawHeader(itemLabel + " " + i))
                    {
                        BeginContents();

                        DrawPropertyWithChangeCheck(property.GetArrayElementAtIndex(i));
                        if (drawRemoveButtons || drawDuplicateButtons)
                        {
                            GUIContent duplicateContent = duplicateButtonTooltip == null ? new GUIContent("Duplicate") : new GUIContent("Duplicate", duplicateButtonTooltip);
                            GUIContent removeContent = removeButtonTooltip == null ? new GUIContent("Remove") : new GUIContent("Remove", removeButtonTooltip);

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            if (drawDuplicateButtons)
                            {
                                if (GUILayout.Button(duplicateContent, GUILayout.Width(100)))
                                {
                                    property.GetArrayElementAtIndex(i).DuplicateCommand();
                                    break;
                                }
                            }
                            if (drawRemoveButtons)
                            {
                                if (GUILayout.Button(removeContent, GUILayout.Width(100)))
                                {
                                    property.GetArrayElementAtIndex(i).DeleteCommand();
                                    break;
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        EndContents();
                    }
                }
                EditorGUILayout.EndVertical();
                if (drawAddButtons)
                {
                    GUIContent addContent = addButtonTooltip == null ? new GUIContent("Add") : new GUIContent("Add", addButtonTooltip);

                    if (GUILayout.Button(addContent))
                    {
                        if (addFunc == null)
                        {
                            property.InsertArrayElementAtIndex(length);
                            GUI.changed = true;
                        }
                        else
                        {
                            addFunc();
                            GUI.changed = true;
                        }
                    }
                    if (GUI.changed)
                    {
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }

                if (!noContainer)
                {
                    EndContents();
                }
            }
        }
    }

    public static void DrawNormalizedVector(SerializedProperty vector, string label = null, bool cannotBeZero = true)
    {
        if (label == null)
        {
            label = vector.name;
        }
        EditorGUILayout.PropertyField(vector, new GUIContent(label));
        if (vector.propertyType == SerializedPropertyType.Vector2)
        {
            if (cannotBeZero && vector.vector2Value == Vector2.zero)
            {
                EditorGUILayout.HelpBox("This vector cannot be zero!", MessageType.Warning);
            }
            else if (vector.vector2Value != vector.vector2Value.normalized)
            {
                EditorGUILayout.HelpBox("This vector need to be normalized!\nClick the \"Normalize\" button below.", MessageType.Warning);
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Normalize", GUILayout.MaxWidth(190)))
            {
                vector.vector2Value = vector.vector2Value.normalized;
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            if (cannotBeZero && vector.vector3Value == Vector3.zero)
            {
                EditorGUILayout.HelpBox("This vector cannot be zero!", MessageType.Warning);
            }
            else if (vector.vector3Value != vector.vector3Value.normalized)
            {
                EditorGUILayout.HelpBox("This vector need to be normalized!\nClick the \"Normalize\" button below.", MessageType.Warning);
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Normalize", GUILayout.MaxWidth(190)))
            {
                vector.vector3Value = vector.vector3Value.normalized;
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
