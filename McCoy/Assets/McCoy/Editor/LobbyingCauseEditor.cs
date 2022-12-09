using Assets.McCoy.BoardGame;
using Assets.McCoy.RPG;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(McCoyLobbyingCause))]
public class LobbyingCauseEditor : Editor
{
  public override void OnInspectorGUI()
  {
    if (GUILayout.Button("Open Quest List Editor"))
      LobbyingCauseListEditorWindow.Init();

  }
}
