using Assets.McCoy.BoardGame;
using Assets.McCoy.Brawler;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UFE3D;
using UnityEngine;

namespace Assets.McCoy.UI
{
  public class McCoyWorldUI : MonoBehaviour, IMcCoyInputManager
  {
    [SerializeField]
    Transform menuAnchor = null;

    [SerializeField]
    GameObject shopMenuPrefab = null;

    [SerializeField]
    GameObject lobbyingMenuPrefab = null;

    IMcCoyInputManager currentMenuInputManager = null;
    GameObject currentMenu = null;

    McCoyInputManager _inputManager = null;
    private McCoyBattleGui battleGui;

    McCoyInputManager inputManager
    {
      get
      {
        if(_inputManager == null)
        {
          _inputManager = new McCoyInputManager();
          _inputManager.RegisterButtonListener(UFE3D.ButtonPress.Button3, closeMenu);
        }
        return _inputManager;
      }
    }

    public void Initialize(McCoyBattleGui mcCoyBattleGui)
    {
      this.battleGui = mcCoyBattleGui;
    }

    private void closeMenu()
    {
      if(currentMenu != null)
      {
        UFE.timeScale = 1;
        Destroy(currentMenu);
        currentMenu = null;
        currentMenuInputManager = null;
        battleGui.ToggleCanvasUI(true);
      }
    }

    public void ShowShop()
    {
      currentMenu = Instantiate(shopMenuPrefab, menuAnchor);
      var lobbyUI = currentMenu.GetComponent<McCoyShopListUI>();
      lobbyUI.Initialize(null, closeMenu);
      currentMenuInputManager = currentMenu.GetComponent<McCoyShopListUI>();
      battleGui.ToggleCanvasUI(false);
      UFE.timeScale = 0;
    }

    public void ShowCouncil()
    {
      currentMenu = Instantiate(lobbyingMenuPrefab, menuAnchor);
      var lobbyUI = currentMenu.GetComponent<McCoyLobbyingListUI>();
      lobbyUI.Initialize(null, closeMenu);
      currentMenuInputManager = currentMenu.GetComponent<McCoyLobbyingListUI>();
      battleGui.ToggleCanvasUI(false);
      UFE.timeScale = 0;
    }

    public bool CheckInputs(IDictionary<InputReferences, InputEvents> player1PreviousInputs, IDictionary<InputReferences, InputEvents> player1CurrentInputs, IDictionary<InputReferences, InputEvents> player2PreviousInputs, IDictionary<InputReferences, InputEvents> player2CurrentInputs)
    {
      if(currentMenu == null)
      {
        return false;
      }
      return currentMenuInputManager.CheckInputs(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
    }
  }
}