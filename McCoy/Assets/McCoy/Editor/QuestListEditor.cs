using Assets.McCoy.RPG;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(McCoyQuestListData))]
public class QuestListEditor : Editor
{
  public override void OnInspectorGUI()
  {
	if (GUILayout.Button("Open Quest List Editor"))
	  QuestListEditorWindow.Init();

  }
}
