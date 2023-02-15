using Assets.McCoy.UI;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.McCoy.Brawler
{
  public class McCoyBrawlerDoor : MonoBehaviour
  {
    [SerializeField]
    bool IsShop = false;

    [SerializeField]
    bool IsCouncil = false;

    McCoyBattleGui gui = null;
    McCoyWorldUI worldUi = null;

    bool active = false;

    public void Initialize(McCoyBattleGui gui, McCoyWorldUI worldUI)
    {
      this.gui = gui;
      worldUi = worldUI;
    }

    public void FixedUpdate()
    {
      if(gui == null)
      {
        return;
      }
      var playerPos = UFE.GetPlayer1ControlsScript().gameObject.transform.position;
      float dx = Mathf.Abs(playerPos.x - transform.position.x);
      float dy = Mathf.Abs(playerPos.y - transform.position.y);
      bool activeNow = dy < 1f && dx < 2f;
      if(!active && activeNow)
      {
        active = true;
        gui.ToggleDoor(active, IsShop ? "_Shop" : (IsCouncil ? "_Council" : "Error"));
      }
      else if(active && !activeNow)
      {
        active = false;
        gui.ToggleDoor(false, "");
      }
    }

    public void Interact()
    {
      if(!active)
      {
        return;
      }
      worldUi.ShowShop();
    }
  }
}
