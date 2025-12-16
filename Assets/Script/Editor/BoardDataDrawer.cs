 using UnityEngine;
 using UnityEditor;
 using UnityEditorInternal;
 using System;
 using System.Collections;
 using System.Collections.Generic;

//aggiungo attributi per disegnare l'editor personalizzato
//dentro tipe of inserisco la classe di cui voglio fare l'editor personalizzato
[CustomEditor(typeof(BoardData))]
public class BoardDataDrawer : Editor
{

    private BoardData GameDataInstance => target as BoardData;
    private ReorderableList _dataList;

    private void OnEnable()
    {
        InitializeReorderableList(ref _dataList, "SearchWords", "Words to Search");

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawColumnsInputFields();
        EditorGUILayout.Space();
        ConvertToUpperButton();
        ClearBoardButton();
        FillUpWithRandomLettersButton();

        if (GameDataInstance.Board != null && GameDataInstance.Columns > 0 && GameDataInstance.Rows > 0)
        {
            DrawBoardTable();
        }

        //disegno la lista riordinabile delle parole da cercare
        EditorGUILayout.Space();
        _dataList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
        if (GUI.changed)
        {
            EditorUtility.SetDirty(GameDataInstance); //update editor per il gamedataistance
        }
    }

    private void DrawColumnsInputFields()
    {
        var columnsTemp = GameDataInstance.Columns;
        var rowsTemps = GameDataInstance.Rows;

        GameDataInstance.Columns = EditorGUILayout.IntField("Columns", GameDataInstance.Columns);
        GameDataInstance.Rows = EditorGUILayout.IntField("Rows", GameDataInstance.Rows);

        if ((GameDataInstance.Columns != columnsTemp || GameDataInstance.Rows != rowsTemps)
        && GameDataInstance.Columns > 0 && GameDataInstance.Rows > 0)
        {
            GameDataInstance.CreateNewBoard();

        }



    }

    private void DrawBoardTable()
    {
        // Paracadute: riallinea se Board è incoerente o nulla
        if (GameDataInstance.Board == null
            || GameDataInstance.Board.Length != GameDataInstance.Columns)
        {
            GameDataInstance.CreateNewBoard();
        }
        else
        {
            for (int x = 0; x < GameDataInstance.Columns; x++)
            {
                if (GameDataInstance.Board[x] == null
                    || GameDataInstance.Board[x].Row == null
                    || GameDataInstance.Board[x].Row.Length != GameDataInstance.Rows)
                {
                    GameDataInstance.CreateNewBoard();
                    break;
                }
            }
        }

        var tableStyle = new GUIStyle("box");
        tableStyle.padding = new RectOffset(10, 10, 10, 10);
        tableStyle.margin.left = 32;

        var columnStyle = new GUIStyle { fixedWidth = 50 };

        var rowStyle = new GUIStyle
        {
            fixedHeight = 25,
            fixedWidth = 40,
            alignment = TextAnchor.MiddleCenter
        };

        var textFieldStyle = new GUIStyle(EditorStyles.textField)
        {
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        textFieldStyle.normal.background = Texture2D.grayTexture;
        textFieldStyle.normal.textColor = Color.white;

        EditorGUILayout.BeginHorizontal(tableStyle);

        for (int x = 0; x < GameDataInstance.Columns; x++)
        {
            EditorGUILayout.BeginVertical(columnStyle);

            for (int y = 0; y < GameDataInstance.Rows; y++)
            {
                EditorGUILayout.BeginHorizontal(rowStyle);

                var cellText = GameDataInstance.Board[x].Row[y] ?? string.Empty;

                var edited = EditorGUILayout.TextArea(cellText, textFieldStyle);

                if (!string.IsNullOrEmpty(edited) && edited.Length > 1)
                    edited = edited.Substring(0, 1);

                GameDataInstance.Board[x].Row[y] = edited ?? string.Empty;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndHorizontal();
    }


    // questa funzione crea e configurare una lista riordinabile personalizzata
    private void InitializeReorderableList(ref ReorderableList list, string propertyName, string listLabel)
    {

        //creazione della lista
        list = new ReorderableList(serializedObject,
                serializedObject.FindProperty(propertyName),
                true, true, true, true);
        //draggable, displayHeader, displayAddButton, displayRemoveButton

        //lamda function    chiamata ogni volta che bisogna disegnare l'header della lista
        list.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, listLabel);
        };

        var l = list;

        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            //
            var element = l.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("word"), GUIContent.none);
        };
    }

    // converto tutte le lettere in maiuscole

    private void ConvertToUpperButton()
    {
        if (GUILayout.Button("Convert All to Uppercase"))
        {
            for (var i = 0; i < GameDataInstance.Columns; i++)
            {
                for (var j = 0; j < GameDataInstance.Rows; j++)
                {
                    if (!string.IsNullOrEmpty(GameDataInstance.Board[i].Row[j]))
                    {
                        GameDataInstance.Board[i].Row[j] = GameDataInstance.Board[i].Row[j].ToUpper();
                    }
                }
            }

            foreach (var searchWord in GameDataInstance.SearchWords)
            {
                if (!string.IsNullOrEmpty(searchWord.word))
                {
                    searchWord.word = searchWord.word.ToUpper();
                }
            }
        }
    }

    private void ClearBoardButton()
    {
        if (GUILayout.Button("Clear Board"))
        {
            for (int i = 0; i < GameDataInstance.Columns; i++)
            {
                for (int j = 0; j < GameDataInstance.Rows; j++)
                {
                    GameDataInstance.Board[i].Row[j] = "";
                }
            }
        }
    }

    private void FillUpWithRandomLettersButton()
    {
        if (GUILayout.Button("Fill Up With Random Letters"))
        {

            Debug.Log("Button pressed: starting fill...");

            //estraggo direzioni, posizioni di partenza e lettere casuali
            var random = new System.Random();

            //Inserisco le parole nella griglia accedendo la lista delle parole da cercare
            foreach (var searchWord in GameDataInstance.SearchWords)
            {
                string upperWord = searchWord.word.ToUpper();
                bool placed = false; //bool per controllare se la parola è stata posizionata
                int attempts = 0; //tentativi fatti per inserire la parola

                //provo a posizionare la parola finché non riesco o supero i tentativi

                while (!placed && attempts < 100)
                {
                    attempts++;

                    // direzioni possibili: dx, giù, diagonale dx-giù
                    (int dx, int dy)[] dirs = new (int, int)[]
                    {
                        ( 1,  0), // →
                        ( 0,  1), // ↓
                        ( 1,  1), // ↘︎
                        (-1,  0), // ←
                        ( 0, -1), // ↑
                        (-1, -1)  // ↖︎
                    };
                    var (dirX, dirY) = dirs[random.Next(dirs.Length)];


                    //scelgo casualmente la cella di partenza
                    // range valido per X in base a dx
                    int minStartX = (dirX == 1) ? 0 : (dirX == -1) ? (upperWord.Length - 1) : 0;
                    int maxStartX = (dirX == 1) ? (GameDataInstance.Columns - upperWord.Length)
                                                : (dirX == -1) ? (GameDataInstance.Columns - 1)
                                                                : (GameDataInstance.Columns - 1);

                    // range valido per Y in base a dy
                    int minStartY = (dirY == 1) ? 0 : (dirY == -1) ? (upperWord.Length - 1) : 0;
                    int maxStartY = (dirY == 1) ? (GameDataInstance.Rows - upperWord.Length)
                                                : (dirY == -1) ? (GameDataInstance.Rows - 1)
                                                                : (GameDataInstance.Rows - 1);

                    // se non c'è spazio in quella direzione, riprova
                    if (minStartX > maxStartX || minStartY > maxStartY)
                        continue;

                    int startX = random.Next(minStartX, maxStartX + 1);
                    int startY = random.Next(minStartY, maxStartY + 1);


                    int endX = startX + dirX * (upperWord.Length - 1);
                    int endY = startY + dirY * (upperWord.Length - 1);

                    // controlla se la parola entra nei limiti
                    if (endX < 0 || endX >= GameDataInstance.Columns || endY < 0 || endY >= GameDataInstance.Rows)
                        continue;

                    bool canPlace = true;
                    for (int k = 0; k < upperWord.Length; k++)
                    {
                        string currentCell = GameDataInstance.Board[startX + dirX * k].Row[startY + dirY * k];

                        //se la cella corrente non è vuota o non corrisponde alla lettera della parola, non posso posizionarla
                        if (!string.IsNullOrEmpty(currentCell) && currentCell != upperWord[k].ToString())
                        {
                            canPlace = false;
                            break;
                            //Debug.Log("Conflict at " + (startX + dirX * k) + "," + (startY + dirY * k) + " for letter " + upperWord[k]);
                        }
                    }

                    //inserimento della parola
                    if (canPlace)
                    {
                        for (int k = 0; k < upperWord.Length; k++)
                        {
                            GameDataInstance.Board[startX + dirX * k].Row[startY + dirY * k] = upperWord[k].ToString();
                        }
                        placed = true;
                    }


                }
                 Debug.Log(placed
                    ? $"Placed word '{upperWord}' after {attempts} attempts."
                    : $"Failed to place word '{upperWord}' after {attempts} attempts.");
                
              
            }

            // Riempi le celle restanti con lettere casuali
            for (int i = 0; i < GameDataInstance.Columns; i++)
            {
                for (int j = 0; j < GameDataInstance.Rows; j++)
                {
                    if (string.IsNullOrEmpty(GameDataInstance.Board[i].Row[j]))
                    {
                        char randomChar = (char)random.Next('A', 'Z' + 1);
                        GameDataInstance.Board[i].Row[j] = randomChar.ToString();
                    }
                }
            }

            // aggiorna la vista
            EditorUtility.SetDirty(GameDataInstance);
        }
    }


}
 