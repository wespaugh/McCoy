using Assets.McCoy.Localization;
using Assets.McCoy.RPG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

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

    [SerializeField]
    GameObject statsRoot = null;
    [SerializeField]
    McCoyEquipmentMenu equipmentMenu = null;

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

    public void SetMenu(FiresideMenus currentMenu)
    {
      statsRoot.SetActive(currentMenu == FiresideMenus.Stats);
      equipmentMenu.gameObject.SetActive(currentMenu == FiresideMenus.Equipment);
    }
  }
}
