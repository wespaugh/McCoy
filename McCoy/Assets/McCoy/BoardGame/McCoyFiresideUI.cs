using Assets.McCoy.Localization;
using Assets.McCoy.RPG;
using Assets.McCoy.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UFE3D;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.BoardGame
{
  public class McCoyFiresideUI : MonoBehaviour, IMcCoyInputManager
  {
    enum FiresideMenus
    {
      Stats,
      Equipment,
      Skills
    }

    [SerializeField]
    McCoyFiresideUIView leftUiAnchor = null;
    [SerializeField]
    McCoyFiresideUIView rightUiAnchor = null;

    [SerializeField]
    GameObject statsRoot = null;

    [SerializeField]
    McCoyEquipmentMenu equipmentMenu = null;
    private bool inputInitialized;
    private McCoyInputManager input;

    FiresideMenus currentMenu = FiresideMenus.Stats;

    void Awake()
    {
      refreshMenu();
    }

    public bool CheckInputs(IDictionary<InputReferences, InputEvents> player1PreviousInputs, IDictionary<InputReferences, InputEvents> player1CurrentInputs, IDictionary<InputReferences, InputEvents> player2PreviousInputs, IDictionary<InputReferences, InputEvents> player2CurrentInputs)
    {
      if (!inputInitialized)
      {
        inputInitialized = true;
        input = new McCoyInputManager();
        input.RegisterButtonListener(ButtonPress.Button5, previousMenu);
        input.RegisterButtonListener(ButtonPress.Button6, nextMenu);
      }
      bool retVal = false;

      if (currentMenu == FiresideMenus.Equipment)
      {
        retVal |= equipmentMenu.CheckInputs(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
        if(retVal)
        {
          Debug.Log("equipment menu handled input");
        }
      }

      retVal |= input.CheckInputs(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
      if(retVal)
      {
        Debug.Log("fireside UI handled input");
      }
      return retVal;
    }

    private void previousMenu()
    {
      switch (currentMenu)
      {
        case FiresideMenus.Stats:
          currentMenu = FiresideMenus.Equipment;
          break;
        case FiresideMenus.Equipment:
          currentMenu = FiresideMenus.Stats;
          break;
      }
      refreshMenu();
    }

    private void nextMenu()
    {
      switch(currentMenu)
      {
        case FiresideMenus.Stats:
          currentMenu = FiresideMenus.Equipment; 
          break;
        case FiresideMenus.Equipment:
          currentMenu = FiresideMenus.Stats;
          break;
      }
      refreshMenu();
    }

    private void refreshMenu()
    {
      statsRoot.SetActive(currentMenu == FiresideMenus.Stats);
      equipmentMenu.gameObject.SetActive(currentMenu == FiresideMenus.Equipment);
    }

    public void SetPlayer(PlayerCharacter pc, McCoyCityBoardContents board, bool canLobby, Animator playerAnimator, string animName)
    {
      bool left = pc == PlayerCharacter.Avalon || pc == PlayerCharacter.Penelope;

      leftUiAnchor.gameObject.SetActive(left);
      rightUiAnchor.gameObject.SetActive(!left);
      McCoyFiresideUIView dataPanel = left ? leftUiAnchor : rightUiAnchor;

      equipmentMenu.Initialize(playerAnimator, animName);


      string playerName = PlayerName(pc);

      Localize("com.mccoy.boardgame.currentlocation", (currentLoc) =>
      {
        string locationName = board.NameForNode(McCoyGameState.Instance().PlayerLocation(pc));
        Localize(locationName, (location) =>
        {
          string currentLocation = currentLoc + ": " + location;

          Localize("com.mccoy.rpg.skillpoints", (skills) =>
          {
            string skillPoints = skills + ": " + McCoyGameState.GetPlayer(pc).AvailableSkillPoints;

            Localize("com.mccoy.boardgame.timeremaining", (timeLoc) =>
            {
              float time = McCoyGameState.Instance().TurnTimeRemaining(pc);
              int mins = (int)(time / 60f);
              float secs = time - (mins * 60f);
              string timeLabel = time + ": " + mins + "m::" + decimal.Round((decimal)secs, 2) + "s";
              if (time <= 0)
              {
                timeLabel = $"<color=\"red\">{timeLabel}</color>";
              }
              string timeRemaining = timeLabel;

              Localize("com.mccoy.boardgame.funds", (fundsLabel) =>
              {
                string funds = fundsLabel + ": " + McCoyGameState.Instance().Credits;

                Localize("com.mccoy.rpg.skills", (skillsLabel) =>
                {
                  Localize("com.mccoy.boardgame.changeplayer", (changePlayerLabel) =>
                  {
                    Localize("com.mccoy.boardgame.lobbying", (lobbyingLabel) =>
                    {
                      Localize("com.mccoy.boardgame.map", (mapLabel) =>
                      {
                        string controlsLabel = $"{mapLabel}:    <sprite name=\"controller_buttons_ps4_1\">    {skillsLabel}:    <sprite name=\"controller_buttons_ps4_0\">    {changePlayerLabel}:    <sprite name=\"controller_buttons_ps4_2\"> /   <sprite name=\"controller_buttons_ps4_3\">";
                        dataPanel.refresh(playerName, currentLocation, skillPoints, timeRemaining, funds, controlsLabel, canLobby);
                      });
                    });
                  });
                });
              });
            });
          });
        });
      });
    }
  }
}
