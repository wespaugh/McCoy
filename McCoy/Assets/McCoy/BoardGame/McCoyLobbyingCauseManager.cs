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
  }
}
