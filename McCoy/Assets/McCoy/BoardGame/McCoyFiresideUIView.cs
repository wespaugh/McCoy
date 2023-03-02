using Assets.McCoy.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.McCoy.BoardGame
{
  public class McCoyFiresideUIView : MonoBehaviour
  {
    [SerializeField]
    McCoyLocalizedText playerName;
    [SerializeField]
    McCoyLocalizedText currentLocation;
    [SerializeField]
    McCoyLocalizedText skillPoints;
    [SerializeField]
    McCoyLocalizedText timeRemaining;
    [SerializeField]
    McCoyLocalizedText funds;
    [SerializeField]
    McCoyLocalizedText controls;
    [SerializeField]
    McCoyLocalizedText lobbyingText;

    public void refresh(string name, string location, string skillPoints, string timeRemaining, string funds, string controls, bool canLobby)
    {
      playerName.SetTextDirectly(name);
      currentLocation.SetTextDirectly(location);
      this.skillPoints.SetTextDirectly(skillPoints);
      this.timeRemaining.SetTextDirectly(timeRemaining);
      this.funds.SetTextDirectly(funds);
      this.controls.SetTextDirectly(controls);
      this.lobbyingText.gameObject.SetActive(canLobby);
    }
  }
}
