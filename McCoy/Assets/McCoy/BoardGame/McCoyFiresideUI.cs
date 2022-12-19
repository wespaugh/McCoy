using Assets.McCoy.Localization;
using Assets.McCoy.UI;
using System;
using System.Collections;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.BoardGame
{
  public class McCoyFiresideUI : MonoBehaviour
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
    GameObject lobbyingUI = null;

    public void SetPlayer(PlayerCharacter pc, McCoyCityBoardContents board)
    {
      playerName.SetTextDirectly(PlayerName(pc));
      currentLocation.SetText("com.mccoy.boardgame.currentlocation", (label) =>
      {
        string locationName = board.NameForNode(McCoyGameState.Instance().PlayerLocation(pc));
        Localize(locationName, (location) =>
        {
          currentLocation.SetTextDirectly(label + ": " + location);
        });
      });

      skillPoints.SetText("com.mccoy.rpg.skillpoints", (label) =>
      {
        skillPoints.SetTextDirectly(label + ": " + McCoyGameState.GetPlayer(pc).AvailableSkillPoints);
      });
      timeRemaining.SetText("com.mccoy.boardgame.timeremaining", (label) =>
      {
        float time = McCoyGameState.Instance().TurnTimeRemaining(pc);
        int mins = (int)(time / 60f);
        float secs = time - (mins*60f);
        string timeLabel = label + ": " + mins + "m::" + decimal.Round((decimal)secs, 2) + "s";
        if(time <= 0)
        {
          timeLabel = $"<color=\"red\">{timeLabel}</color>";
        }
        timeRemaining.SetTextDirectly(timeLabel);
      });

      funds.SetText("com.mccoy.boardgame.funds", (label) =>
      {
        funds.SetTextDirectly(label + ": " + McCoyGameState.Instance().Credits);
      });

      Localize("com.mccoy.rpg.skills", (skillsLabel) =>
      {
        Localize("com.mccoy.boardgame.changeplayer", (changePlayerLabel) =>
        {
          Localize("com.mccoy.boardgame.lobbying", (lobbyingLabel) =>
          {
            Localize("com.mccoy.boardgame.map", (mapLabel) =>
            {
              controls.SetTextDirectly($"{mapLabel}:    <sprite name=\"controller_buttons_ps4_1\">    {skillsLabel}:    <sprite name=\"controller_buttons_ps4_0\">    {changePlayerLabel}:    <sprite name=\"controller_buttons_ps4_2\"> /   <sprite name=\"controller_buttons_ps4_3\">");
            });
          });
        });
      });
    }
  }
}
