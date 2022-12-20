using Assets.McCoy.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Assets.McCoy.BoardGame.McCoyLobbyingCause;

namespace Assets.McCoy.BoardGame
{
  class McCoyLobbyingCauseManager : MonoBehaviour
  {
    private static McCoyLobbyingCauseManager instance;
    private List<McCoyLobbyingCause> lobbyingCauses;

    public List<McCoyLobbyingCause> LobbyingCauses
    {
      get => lobbyingCauses;
    }

    public static McCoyLobbyingCauseManager GetInstance()
    {
      if (instance == null)
      {
        instance = GameObject.Find("UFE Manager").GetComponent<McCoyLobbyingCauseManager>();
      }
      return instance;
    }

    public void Awake()
    {
      StartCoroutine(loadCauses());
    }
    IEnumerator loadCauses()
    {
      ResourceRequest req = Resources.LoadAsync<McCoyLobbyingCauseListData>($"LobbyingCauses");
      yield return req;
      var lobbyingCauses = req.asset as McCoyLobbyingCauseListData;
      this.lobbyingCauses = new List<McCoyLobbyingCause>();
      foreach(var lobbyCause in lobbyingCauses.causes)
      {
        this.lobbyingCauses.Add(lobbyCause.Clone() as McCoyLobbyingCause);
      }
    }
    public void ApplyCause(LobbyingCause cause, McCoyCityScreen city)
    {
      McCoyGameState.Instance().causesLobbiedFor.Add(cause);
      string zoneUnlock = "";
      switch (cause)
      {
        case McCoyLobbyingCause.LobbyingCause.SkyBridge:
          zoneUnlock = ProjectConstants.SKYBRIDGE_ID;
          break;
        case McCoyLobbyingCause.LobbyingCause.Subway:
          zoneUnlock = ProjectConstants.SUBWAY_ID;
          break;
      }
      if(!string.IsNullOrEmpty(zoneUnlock))
      {
        city.UnlockZone(zoneUnlock);
      }
    }
  }
}
