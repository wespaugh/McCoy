using UnityEngine;
using UnityEditor;
using Assets.McCoy.RPG;

[CustomEditor(typeof(McCoyQuestListData))]
public class QuestListEditor : Editor
{
  public override void OnInspectorGUI()
  {
	if (GUILayout.Button("Open Quest List Editor"))
	  CharacterEditorWindow.Init();

  }
}
