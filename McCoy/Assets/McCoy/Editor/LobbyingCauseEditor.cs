using Assets.McCoy.BoardGame;
using Assets.McCoy.RPG;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(McCoyLobbyingCauseListData))]
public class LobbyingCauseEditor : Editor
{
  public override void OnInspectorGUI()
  {
    if (GUILayout.Button("Open Lobbying Cause List Editor"))
      LobbyingCauseListEditorWindow.Init();

  }
}
