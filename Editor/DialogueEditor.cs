// using UnityEngine;
// using UnityEditor;
// // using UnityEditorInternal;
// using System.Collections;
// using System.Collections.Generic;
// using System;
// using Malee.List;

// [CanEditMultipleObjects]
// [CustomEditor(typeof(DialogueSO))]
// public class ExampleEditor : Editor {
//     SerializedProperty dialogueId;
//     SerializedProperty dialogues;
//     ReorderableList dialoguesRO;
//     // Dictionary<string, ReorderableList> innerListDict = new Dictionary<string, ReorderableList>();

// 	void OnEnable() {
//         dialogueId = serializedObject.FindProperty("dialogueId");

//         dialogues = serializedObject.FindProperty("dialogues");
//         dialoguesRO = new ReorderableList(dialogues);
//         // dialoguesRO = new ReorderableList(serializedObject, dialogues, true, true, true, true);
//         // dialoguesRO.drawHeaderCallback = (rect) => 
//         // {
//         //     EditorGUI.LabelField(rect, "Convos");
//         // };
//         // dialoguesRO.drawElementCallback = DrawElement;
//         // dialoguesRO.elementHeightCallback = index => {
//         //     var element = dialogues.GetArrayElementAtIndex(index);

//         //     var innerList = element.FindPropertyRelative("lines");

//         //     return (innerList.arraySize + 4) * EditorGUIUtility.singleLineHeight;
//         // };
// 	}

//     // void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
//     // {
//     //     SerializedProperty element = dialoguesRO.serializedProperty.GetArrayElementAtIndex(index);

//     //     EditorGUI.PropertyField(
//     //         new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight),
//     //         element.FindPropertyRelative("id"),
//     //         GUIContent.none,
//     //         false
//     //     );

//     //     var linesRO = element.FindPropertyRelative("lines");
//     //     string listKey = element.propertyPath;
//     //     ReorderableList innerReorderableList;

//     //     if (innerListDict.ContainsKey(listKey)) {
//     //         innerReorderableList = innerListDict[listKey];
//     //     }
//     //     else
//     //     {
//     //         innerReorderableList = new ReorderableList(element.serializedObject, linesRO)
//     //         {
//     //             displayAdd = true,
//     //             displayRemove = true,
//     //             draggable = true,

//     //             drawHeaderCallback = innerRect =>
//     //             {
//     //                 EditorGUI.LabelField(innerRect, "Lines");
//     //             },

//     //             drawElementCallback = (innerRect, innerIndex, innerA, innerH) =>
//     //             {
//     //                 var innerElement = linesRO.GetArrayElementAtIndex(innerIndex);

//     //                 var text = innerElement.FindPropertyRelative("text");
//     //                 // EditorGUI.PropertyField(innerRect, text, GUIContent.none, false);
//     //                 text.stringValue = EditorGUI.TextArea(new Rect(innerRect.x, innerRect.y, innerRect.width, innerRect.height), text.stringValue);

//     //                 EditorGUI.PropertyField(innerRect, innerElement.FindPropertyRelative("nextConvoId"));
//     //             },
//     //         };
//     //         innerListDict[listKey] = innerReorderableList;
//     //     }

//     //     var height = (linesRO.arraySize + 3) * EditorGUIUtility.singleLineHeight;
//     //     innerReorderableList.DoList(new Rect(rect.x, rect.y, rect.width, height));
//     //     // innerReorderableList.DoLayoutList();
//     // }

// 	public override void OnInspectorGUI() {

// 		serializedObject.Update();

//         // EditorGUILayout.PropertyField(dialogueName);
//         EditorGUILayout.PropertyField(dialogueId);
// 		dialoguesRO.DoLayoutList();

// 		serializedObject.ApplyModifiedProperties();
// 	}
// }