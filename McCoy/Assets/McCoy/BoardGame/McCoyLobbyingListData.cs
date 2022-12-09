using UnityEngine;

namespace Assets.McCoy.BoardGame
{
  [System.Serializable]
  public class McCoyLobbyingCauseListData : ScriptableObject
  {
    public McCoyLobbyingCause[] causes = new McCoyLobbyingCause[0];
  }
}
