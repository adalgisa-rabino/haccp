using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;


[CustomEditor(typeof(AlphabetData))] //name of the class which we wanna create editor
[CanEditMultipleObjects]


public class AlphabetDataDrawer : Editor
{
    private ReorderableList AlphabetPlainList;
    private ReorderableList AlphabetNormalList;
    private ReorderableList AlphatbetHighlightedList;
    private ReorderableList AlphabetWrongList;

    private void OnEnable()
    {
        InitializeReorderableLists(ref AlphabetPlainList, "AlphabatPlain", "Alphabet Plain");
        InitializeReorderableLists(ref AlphabetNormalList, "AlphabatNormal", "Alphabet Normal");
        InitializeReorderableLists(ref AlphatbetHighlightedList, "AlphabatHighlighted", "Alphabet Highlighted");
        InitializeReorderableLists(ref AlphabetWrongList, "AlphabatWrong", "Alphabet Wrong");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        AlphabetPlainList.DoLayoutList();
        AlphabetNormalList.DoLayoutList();
        AlphatbetHighlightedList.DoLayoutList();
        AlphabetWrongList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();

    }

    private void InitializeReorderableLists(ref ReorderableList list, string propertyName, string listLabel)
    {
        //creazione della lista
        list = new ReorderableList(serializedObject,
                serializedObject.FindProperty(propertyName),
                true, true, true, true);
        //draggable, displayHeader, displayAddButton, displayRemoveButton

        list.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, listLabel);
        };

        var l = list;

        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = l.serializedProperty.GetArrayElementAtIndex(index);

            rect.y += 2;
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("letter"), GUIContent.none);

            EditorGUI.PropertyField(
                new Rect(rect.x + 70, rect.y, rect.width - 60 - 30, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("image"), GUIContent.none);

        };


    }
    
      
} 
