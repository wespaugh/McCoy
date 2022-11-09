using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using FPLibrary;
using UFE3D;
using Assets.McCoy.RPG;

public class QuestListEditorWindow : EditorWindow
{
  public static QuestListEditorWindow questEditorWindow;
  public static McCoyQuestListData sentQuestInfo;
  private McCoyQuestListData questInfo;
  // private Dictionary<int, MoveSetData> instantiatedMoveSet = new Dictionary<int, MoveSetData>();

  private Vector2 scrollPos;
  private GameObject quest;

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
    questEditorWindow = EditorWindow.GetWindow<QuestListEditorWindow>(false, "Quest List", true);
    questEditorWindow.Show();
    questEditorWindow.Populate();
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
    if (EditorApplication.isPlayingOrWillChangePlaymode && quest != null)
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

    if (sentQuestInfo != null)
    {
      EditorGUIUtility.PingObject(sentQuestInfo);
      Selection.activeObject = sentQuestInfo;
      sentQuestInfo = null;
    }

    UnityEngine.Object[] selection = Selection.GetFiltered(typeof(McCoyQuestListData), SelectionMode.Assets);
    if (selection.Length > 0)
    {
      if (selection[0] == null) return;
      questInfo = (McCoyQuestListData)selection[0];
    }
  }

  public void OnGUI()
  {
    if (questInfo == null)
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
          questListFoldout = EditorGUILayout.Foldout(questListFoldout, "THE QUESTS (" + questInfo.quests.Length  + ")", foldStyle);
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
            for (int i = 0; i < questInfo.quests.Length; i++)
            {
              EditorGUILayout.Space();
              QuestBlock(questInfo.quests[i]);
            }

            if (StyledButton("New Quest"))
            {
              questInfo.quests = AddElement<McCoyQuestData>(questInfo.quests, new McCoyQuestData());
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
      Undo.RecordObject(questInfo, "QuestList Editor Modify");
      EditorUtility.SetDirty(questInfo);
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


  public void QuestBlock(McCoyQuestData quest, bool resource = false)
  {
    EditorGUILayout.BeginVertical(resource ? subArrayElementStyle : arrayElementStyle);
    {
      EditorGUILayout.Space();
      EditorGUILayout.LabelField("Uuid");
      quest.uuid = EditorGUILayout.TextField(quest.uuid, GUILayout.Width(500));
      EditorGUILayout.LabelField("summary");
      quest.summary = EditorGUILayout.TextField(quest.summary, GUILayout.Width(500));
      quest.firstWeekAvailable = EditorGUILayout.IntSlider("First Available Week:", quest.firstWeekAvailable, 1, 50);
      quest.lastWeekAvailable = EditorGUILayout.IntSlider("Last Available Week:", quest.lastWeekAvailable, 1, 50);
      quest.worldDurationInGlobalTurns = EditorGUILayout.IntSlider("Duration:", quest.worldDurationInGlobalTurns, 1, 10);

      EditorGUILayout.LabelField("Intro Text");
      quest.introText = EditorGUILayout.TextField(quest.introText, GUILayout.Width(500));
      EditorGUILayout.LabelField("Exit Text");
      quest.exitText = EditorGUILayout.TextField(quest.exitText, GUILayout.Width(500));

      var prereqs = quest.prerequisiteQuestFlags;
      int newCount = Mathf.Max(0, EditorGUILayout.IntField("Num Prereqs", prereqs.Count));
      while (newCount < prereqs.Count)
        prereqs.RemoveAt(prereqs.Count - 1);
      while (newCount > prereqs.Count)
        prereqs.Add(null);

      for (int i = 0; i < prereqs.Count; i++)
      {
        prereqs[i] = (string)EditorGUILayout.TextField("ID: ", prereqs[i]);
      }
      quest.prerequisiteQuestFlags = prereqs;

      var locs = quest.possibleLocations;
      int locationCount = Mathf.Max(0, EditorGUILayout.IntField("Locations", locs.Count));
      while (locationCount < locs.Count)
        locs.RemoveAt(locs.Count - 1);
      while (locationCount > locs.Count)
        locs.Add(null);

      for (int i = 0; i < locs.Count; i++)
      {
        locs[i] = (string)EditorGUILayout.TextField("Location Uuid: ", locs[i]);
      }
      quest.possibleLocations = locs;

      var exitChoices = quest.exitChoices;
      int choiceCount = Mathf.Max(0, EditorGUILayout.IntField("Num Exit Options", exitChoices.Count));
      while (choiceCount < exitChoices.Count)
        exitChoices.RemoveAt(exitChoices.Count - 1);
      while (choiceCount > exitChoices.Count)
        exitChoices.Add(new McCoyQuestChoice());

      for (int i = 0; i < exitChoices.Count; i++)
      {
        EditorGUILayout.LabelField("Choice " + (i+1));
        string flag = (string)EditorGUILayout.TextField("Choice Flag: ", exitChoices[i].uuid);
        string displayText = (string)EditorGUILayout.TextField("Display Text: ", exitChoices[i].displayText);
        exitChoices[i] = new McCoyQuestChoice() { displayText = displayText, uuid = flag };
      }
      quest.exitChoices = exitChoices;
    }

    EditorGUILayout.EndVertical();
  }

  public Transform FindTransform(string searchString)
  {
    if (quest == null) return null;
    Transform[] transformChildren = quest.GetComponentsInChildren<Transform>();
    Transform found;
    foreach (Transform transformChild in transformChildren)
    {
      found = transformChild.Find("mixamorig:" + searchString);
      if (found == null) found = transformChild.Find(quest.name + ":" + searchString);
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