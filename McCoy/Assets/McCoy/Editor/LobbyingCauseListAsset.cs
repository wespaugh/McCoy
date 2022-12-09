using Assets.McCoy.BoardGame;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.Editor
{
  public class LobbyingCauseListAsset : ScriptableObject
  {
    [MenuItem("Assets/Create/McCoy/Lobbying Cause List")]
    public static void CreateAsset()
    {
      ScriptableObjectUtility.CreateAsset<McCoyLobbyingCauseListData>();
    }
  }
}