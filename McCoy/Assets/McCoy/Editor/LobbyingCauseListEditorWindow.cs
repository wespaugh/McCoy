using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using FPLibrary;
using UFE3D;
using Assets.McCoy.RPG;
using Assets.McCoy.BoardGame;

public class LobbyingCauseListEditorWindow : EditorWindow
{
  public static LobbyingCauseListEditorWindow lobbyingCauseEditorWindow;
  public static McCoyLobbyingCauseListData sentLobbyingCauseListData;
  private McCoyLobbyingCauseListData lobbyingCauseList;
  // private Dictionary<int, MoveSetData> instantiatedMoveSet = new Dictionary<int, MoveSetData>();

  private Vector2 scrollPos;
  private GameObject lobbyingCauseObject;

  private string titleStyle;
  private string addButtonStyle;
  private string rootGroupStyle;
  private string subGroupStyle;
  private string arrayElementStyle;
  private string subArrayElementStyle;
  private string toggleStyle;
  private string foldStyle;
  private string enumStyle;

  private bool questListFoldout;

  [MenuItem("Window/McCoy/Quest List Editor")]
  public static void Init()
  {
    lobbyingCauseEditorWindow = EditorWindow.GetWindow<LobbyingCauseListEditorWindow>(false, "Lobbying Cause List", true);
    lobbyingCauseEditorWindow.Show();
    lobbyingCauseEditorWindow.Populate();
  }

  void OnSelectionChange()
  {
    Populate();
    Repaint();
  }

  void OnEnable()
  {
    Populate();
  }

  void OnFocus()
  {
    Populate();
  }

  void OnDisable()
  {
    ClosePreview();
  }

  void OnDestroy()
  {
    ClosePreview();
  }

  void OnLostFocus()
  {
    //ClosePreview();
  }

  public void PreviewCharacter(bool hasAnimator = false)
  {
  }

  public void ClosePreview()
  {
  }

  void helpButton(string page)
  {
  }

  void Update()
  {
    if (EditorApplication.isPlayingOrWillChangePlaymode && lobbyingCauseObject != null)
    {
      ClosePreview();
    }
  }

  void Populate()
  {
    this.titleContent = new GUIContent("Quest", (Texture)Resources.Load("Icons/Character"));

    // Style Definitions
    titleStyle = "MeTransOffRight";
    addButtonStyle = "CN CountBadge";
    rootGroupStyle = "GroupBox";
    subGroupStyle = "ObjectFieldThumb";
    arrayElementStyle = "FrameBox";
    subArrayElementStyle = "HelpBox";
    foldStyle = "Foldout";
    enumStyle = "MiniPopup";
    toggleStyle = "BoldToggle";

    if (sentLobbyingCauseListData != null)
    {
      EditorGUIUtility.PingObject(sentLobbyingCauseListData);
      Selection.activeObject = sentLobbyingCauseListData;
      sentLobbyingCauseListData = null;
    }

    UnityEngine.Object[] selection = Selection.GetFiltered(typeof(McCoyLobbyingCauseListData), SelectionMode.Assets);
    if (selection.Length > 0)
    {
      if (selection[0] == null) return;
      lobbyingCauseList = (McCoyLobbyingCauseListData)selection[0];
    }
  }

  public void OnGUI()
  {
    if (lobbyingCauseList == null)
    {
      GUILayout.BeginHorizontal("GroupBox");
      GUILayout.Label("Select a quest list\nor create a new quest list.", "CN EntryInfo");
      GUILayout.EndHorizontal();
      EditorGUILayout.Space();
      if (GUILayout.Button("Create new quest list"))
        ScriptableObjectUtility.CreateAsset<McCoyQuestListData>();
      return;
    }

    GUIStyle fontStyle = new GUIStyle();
    fontStyle.font = (Font)Resources.Load("EditorFont");
    fontStyle.fontSize = 30;
    fontStyle.alignment = TextAnchor.UpperCenter;
    fontStyle.normal.textColor = Color.white;
    fontStyle.hover.textColor = Color.white;


    scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
    {
      // The Quests
      EditorGUILayout.BeginVertical(rootGroupStyle);
      {
        EditorGUILayout.BeginHorizontal();
        {
          questListFoldout = EditorGUILayout.Foldout(questListFoldout, "Lobbying Causes (" + lobbyingCauseList.causes.Length + ")", foldStyle);
        }
        EditorGUILayout.EndHorizontal();

        if (questListFoldout)
        {
          EditorGUILayout.BeginVertical(subGroupStyle);
          {
            EditorGUILayout.Space();
            EditorGUI.indentLevel += 1;

            EditorGUIUtility.labelWidth = 150;

            EditorGUILayout.Space();
            for (int i = 0; i < lobbyingCauseList.causes.Length; i++)
            {
              EditorGUILayout.Space();
              LobbyingCauseBlock(lobbyingCauseList.causes[i]);
            }

            if (StyledButton("New Cause"))
            {
              lobbyingCauseList.causes = AddElement<McCoyLobbyingCause>(lobbyingCauseList.causes, new McCoyLobbyingCause());
            }

            EditorGUI.indentLevel -= 1;
          }
          EditorGUILayout.EndVertical();
        }

      }
      EditorGUILayout.EndVertical();

    }
    EditorGUILayout.EndScrollView();


    if (GUI.changed)
    {
      Undo.RecordObject(lobbyingCauseList, "CauseList Editor Modify");
      EditorUtility.SetDirty(lobbyingCauseList);
      if (UFE.autoSaveAssets) AssetDatabase.SaveAssets();
    }
  }


  private void SubGroupTitle(string _name)
  {
    Texture2D originalBackground = GUI.skin.box.normal.background;
    GUI.skin.box.normal.background = Texture2D.grayTexture;

    GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
    EditorGUILayout.BeginHorizontal();
    GUILayout.FlexibleSpace();
    GUILayout.Label(_name);
    GUILayout.FlexibleSpace();
    EditorGUILayout.EndHorizontal();
    GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

    GUI.skin.box.normal.background = originalBackground;
  }

  public bool StyledButton(string label)
  {
    EditorGUILayout.Space();
    GUILayoutUtility.GetRect(1, 20);
    EditorGUILayout.BeginHorizontal();
    GUILayout.FlexibleSpace();
    bool clickResult = GUILayout.Button(label, addButtonStyle);
    GUILayout.FlexibleSpace();
    EditorGUILayout.EndHorizontal();
    EditorGUILayout.Space();
    return clickResult;
  }


  public void LobbyingCauseBlock(McCoyLobbyingCause cause, bool resource = false)
  {
    EditorGUILayout.BeginVertical(resource ? subArrayElementStyle : arrayElementStyle);
    {
      EditorGUILayout.Space();
      EditorGUILayout.LabelField("Title");
      cause.title = EditorGUILayout.TextField(cause.title, GUILayout.Width(500));
      EditorGUILayout.LabelField("summary");
      cause.summary = EditorGUILayout.TextField(cause.summary, GUILayout.Width(500));
      EditorGUILayout.LabelField("Lobbying Cause");
      cause.lobbyingCause = (McCoyLobbyingCause.LobbyingCause)EditorGUILayout.EnumPopup(cause.lobbyingCause, GUILayout.Width(500));
      
    }

    EditorGUILayout.EndVertical();
  }

  public Transform FindTransform(string searchString)
  {
    if (lobbyingCauseObject == null) return null;
    Transform[] transformChildren = lobbyingCauseObject.GetComponentsInChildren<Transform>();
    Transform found;
    foreach (Transform transformChild in transformChildren)
    {
      found = transformChild.Find("mixamorig:" + searchString);
      if (found == null) found = transformChild.Find(lobbyingCauseObject.name + ":" + searchString);
      if (found == null) found = transformChild.Find(searchString);
      if (found != null) return found;
    }
    return null;
  }


  public void PaneOptions<T>(T[] elements, T element, System.Action<T[]> callback)
  {
    if (elements == null || elements.Length == 0) return;
    GenericMenu toolsMenu = new GenericMenu();

    if ((elements[0] != null && elements[0].Equals(element)) || (elements[0] == null && element == null) || elements.Length == 1)
    {
      toolsMenu.AddDisabledItem(new GUIContent("Move Up"));
      toolsMenu.AddDisabledItem(new GUIContent("Move To Top"));
    }
    else
    {
      toolsMenu.AddItem(new GUIContent("Move Up"), false, delegate () { callback(MoveElement<T>(elements, element, -1)); });
      toolsMenu.AddItem(new GUIContent("Move To Top"), false, delegate () { callback(MoveElement<T>(elements, element, -elements.Length)); });
    }
    if ((elements[elements.Length - 1] != null && elements[elements.Length - 1].Equals(element)) || elements.Length == 1)
    {
      toolsMenu.AddDisabledItem(new GUIContent("Move Down"));
      toolsMenu.AddDisabledItem(new GUIContent("Move To Bottom"));
    }
    else
    {
      toolsMenu.AddItem(new GUIContent("Move Down"), false, delegate () { callback(MoveElement<T>(elements, element, 1)); });
      toolsMenu.AddItem(new GUIContent("Move To Bottom"), false, delegate () { callback(MoveElement<T>(elements, element, elements.Length)); });
    }

    toolsMenu.AddSeparator("");

    if (element != null && element is System.ICloneable)
    {
      toolsMenu.AddItem(new GUIContent("Copy"), false, delegate () { callback(CopyElement<T>(elements, element)); });
    }
    else
    {
      toolsMenu.AddDisabledItem(new GUIContent("Copy"));
    }

    if (element != null && CloneObject.objCopy != null && CloneObject.objCopy.GetType() == typeof(T))
    {
      toolsMenu.AddItem(new GUIContent("Paste"), false, delegate () { callback(PasteElement<T>(elements, element)); });
    }
    else
    {
      toolsMenu.AddDisabledItem(new GUIContent("Paste"));
    }

    toolsMenu.AddSeparator("");

    if (!(element is System.ICloneable))
    {
      toolsMenu.AddDisabledItem(new GUIContent("Duplicate"));
    }
    else
    {
      toolsMenu.AddItem(new GUIContent("Duplicate"), false, delegate () { callback(DuplicateElement<T>(elements, element)); });
    }
    toolsMenu.AddItem(new GUIContent("Remove"), false, delegate () { callback(RemoveElement<T>(elements, element)); });

    toolsMenu.ShowAsContext();
    EditorGUIUtility.ExitGUI();
  }

  public T[] RemoveElement<T>(T[] elements, T element)
  {
    List<T> elementsList = new List<T>(elements);
    elementsList.Remove(element);
    return elementsList.ToArray();
  }

  public T[] AddElement<T>(T[] elements, T element)
  {
    List<T> elementsList = new List<T>(elements);
    elementsList.Add(element);
    return elementsList.ToArray();
  }

  public T[] CopyElement<T>(T[] elements, T element)
  {
    CloneObject.objCopy = (object)(element as ICloneable).Clone();
    return elements;
  }

  public T[] PasteElement<T>(T[] elements, T element)
  {
    if (CloneObject.objCopy == null) return elements;
    List<T> elementsList = new List<T>(elements);
    elementsList.Insert(elementsList.IndexOf(element) + 1, (T)CloneObject.objCopy);
    //CloneObject.objCopy = null;
    return elementsList.ToArray();
  }

  public T[] DuplicateElement<T>(T[] elements, T element)
  {
    List<T> elementsList = new List<T>(elements);
    elementsList.Insert(elementsList.IndexOf(element) + 1, (T)(element as ICloneable).Clone());
    return elementsList.ToArray();
  }

  public T[] MoveElement<T>(T[] elements, T element, int steps)
  {
    List<T> elementsList = new List<T>(elements);
    int newIndex = Mathf.Clamp(elementsList.IndexOf(element) + steps, 0, elements.Length - 1);
    elementsList.Remove(element);
    elementsList.Insert(newIndex, element);
    return elementsList.ToArray();
  }
}