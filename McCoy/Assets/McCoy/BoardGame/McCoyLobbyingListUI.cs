using Assets.McCoy.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.McCoy.BoardGame
{
  public class McCoyLobbyingListUI : MonoBehaviour
  {
    [SerializeField]
    GameObject lobbyingListItemPrefab = null;

    [SerializeField]
    Transform lobbyingContent = null;

    List<GameObject> listItems = new List<GameObject>();

    McCoyCityScreen city;
    public void Initialize(McCoyCityScreen cityScreen)
    {
      city = cityScreen;
      while(listItems.Count > 0)
      {
        var item1 = listItems[0];
        listItems.RemoveAt(0);
        Destroy(item1);
      }
      var causes = McCoyLobbyingCauseManager.GetInstance().LobbyingCauses;
      causes.Sort((x, y) =>
      {
        return x.cost == y.cost ? 0 : (x.cost < y.cost ? -1 : 1);
      });
      foreach(var cause in causes)
      {
        Instantiate(lobbyingListItemPrefab,lobbyingContent).GetComponent<McCoyLobbyingListItem>().Initialize(cause, city);
      }
    }
  }
}
