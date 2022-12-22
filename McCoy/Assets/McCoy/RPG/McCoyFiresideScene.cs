using Assets.McCoy.BoardGame;
using Assets.McCoy.UI;
using com.cygnusprojects.TalentTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFE3D;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.RPG
{
  public class McCoyFiresideScene : MonoBehaviour, IMcCoyInputManager
  {
    [SerializeField]
    SpriteRenderer avalonSprite = null;
    [SerializeField]
    SpriteRenderer penelopeSprite = null;
    [SerializeField]
    SpriteRenderer vickiSprite = null;
    [SerializeField]
    SpriteRenderer rexSprite = null;

    [SerializeField]
    GameObject FiresideStatsPrefab = null;

    [SerializeField]
    GameObject RexSkillTree = null;

    [SerializeField]
    GameObject LobbyingPrefab = null;

    McCoyCityScreen city = null;
    McCoyFiresideUI uiPanel = null;

    McCoyInputManager input = null;
    bool inputInitialized = false;

    McCoySkillTreeMenu talentDelegate = null;
    McCoyLobbyingListUI lobbyingDelegate = null;

    Dictionary<string, List<PlayerCharacter>> pcGroups = null;
    int selectedPCGroupIndex = 0;

    int selectedCharacter = 0;
    private bool canLobby;

    public void UpdateWithPCGroups(McCoyCityScreen root, Dictionary<string, List<PlayerCharacter>> characterGroups)
    {
      this.city = root;
      if (uiPanel == null)
      {
        uiPanel = Instantiate(FiresideStatsPrefab).GetComponent<McCoyFiresideUI>();
      }
      uiPanel.transform.SetParent(root.transform);

      pcGroups = characterGroups;
      selectedPCGroupIndex = 0;

      var skillString = McCoyGameState.Instance().playerCharacters[PlayerCharacter.Rex].SkillTreeString;
      if(string.IsNullOrEmpty(skillString))
      {
        skillString = RexSkillTree.GetComponent<TalentusEngine>().SaveToString();
        skillString = RexSkillTree.GetComponent<TalentusEngine>().ResetSkillTree();
      }
      loadSkills(McCoyGameState.GetPlayer(PlayerCharacter.Rex).AvailableSkillPoints, skillString, PlayerCharacter.Rex);

      // ToggleLobbying(false);
      refresh();
    }

    public void NextPlayer()
    {
      ++selectedCharacter;
      if(selectedCharacter >= PlayerCharacters.Length)
      {
        selectedCharacter = 0;
      }
      city.ChangePlayer(1, false);
      refresh();
    }

    public void PreviousPlayer()
    {
      --selectedCharacter;
      if(selectedCharacter < 0)
      {
        selectedCharacter = PlayerCharacters.Length - 1;
      }
      city.ChangePlayer(-1, false);
      refresh();
    }

    void refresh()
    {
      selectedPCGroupIndex = 0;
      foreach(var pcGroup in pcGroups)
      {
        bool foundPC = false;
        foreach(var pc in pcGroup.Value)
        {
          if(pc == PlayerCharacters[selectedCharacter])
          {
            foundPC = true;
            break;
          }
        }
        if(foundPC)
        {
          break;
        }
        ++selectedPCGroupIndex;
      }

      canLobby = city.Board.NodeWithID(McCoyGameState.Instance().PlayerLocation(PlayerCharacters[selectedCharacter])).LobbyingAvailable;
      uiPanel.SetPlayer(PlayerCharacters[selectedCharacter], city.Board, canLobby);

      foreach (var pc in PlayerCharacters)
      {
        spriteForPC(pc).transform.localScale = pc == PlayerCharacters[selectedCharacter] ? new Vector3(1.2f,1.2f,1.2f) : Vector3.one;
      }

      int i = 0;
      foreach (var pcGroup in pcGroups)
      {
        bool visible = i == selectedPCGroupIndex;
        foreach(PlayerCharacter pc in pcGroup.Value)
        {
          spriteForPC(pc).color = visible ? Color.white : new Color(.5f, .5f, 1, .7f);
        }
        ++i;
      }
    }
    private SpriteRenderer spriteForPC(PlayerCharacter pc)
    {
      switch (pc)
      {
        case PlayerCharacter.Avalon:
          return avalonSprite;
        case PlayerCharacter.Penelope:
          return penelopeSprite;
        case PlayerCharacter.Rex:
          return rexSprite;
        case PlayerCharacter.Vicki:
          return vickiSprite;
      }
      return null;
    }

    public bool CheckInputs(IDictionary<InputReferences, InputEvents> player1PreviousInputs, IDictionary<InputReferences, InputEvents> player1CurrentInputs, IDictionary<InputReferences, InputEvents> player2PreviousInputs, IDictionary<InputReferences, InputEvents> player2CurrentInputs)
    {
      if(!inputInitialized)
      {
        inputInitialized = true;
        input = new McCoyInputManager();
        input.RegisterButtonListener(ButtonPress.Button1, toggleSkills);
        input.RegisterButtonListener(ButtonPress.Button4, toggleLobbying);
        input.RegisterButtonListener(ButtonPress.Button3, closeMenu);
        input.RegisterButtonListener(ButtonPress.Forward, NextPlayer);
        input.RegisterButtonListener(ButtonPress.Back, PreviousPlayer);
        input.RegisterButtonListener(ButtonPress.Button2, ReturnToMap);
      }
      if (talentDelegate != null)
      {
        talentDelegate.CheckInputs(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
        return true;
      };
      if(lobbyingDelegate != null)
      {
        lobbyingDelegate.CheckInputs(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
        return true;
      }
      return input.CheckInputs(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
    }
    
    void toggleLobbying()
    {
      if(!canLobby)
      {
        return;
      }
      lobbyingDelegate = Instantiate(LobbyingPrefab, uiPanel.transform.parent).GetComponent<McCoyLobbyingListUI>();
      lobbyingDelegate.Initialize(city, closeLobbyingUI);
    }

    public void closeLobbyingUI()
    {
      Destroy(lobbyingDelegate.gameObject);
      refresh();
    }

    void toggleSkills()
    {
      OpenSkillTree();
    }

    void closeMenu()
    {
      if(talentDelegate != null)
      {
        Destroy(talentDelegate.gameObject);
        talentDelegate = null;
      }
    }

    private void loadSkills(int availablePoints, string serializedSkills, PlayerCharacter pc)
    {
      if (talentDelegate != null)
      {
        Destroy(talentDelegate.gameObject);
      }
      talentDelegate = null;
      McCoyGameState.GetPlayer(pc).AvailableSkillPoints = availablePoints;
      refresh();
      McCoy.GetInstance().gameState.UpdateSkills(pc, serializedSkills, availablePoints);
    }

    public void OpenSkillTree()
    {
      if (talentDelegate != null)
      {
        return;
      }
      PlayerCharacter selectedPlayer = PlayerCharacters[selectedCharacter];
      string skillTreeString = "";
      int availableSkillPoints = 0;
      McCoyPlayerCharacter playerData = null;
      switch (selectedPlayer)
      {
        case PlayerCharacter.Rex:
        default:
          talentDelegate = Instantiate(RexSkillTree, uiPanel.transform.parent).GetComponent<McCoySkillTreeMenu>();
          playerData = McCoyGameState.GetPlayer(PlayerCharacter.Rex);
          break;
      }
      if (playerData != null)
      {
        availableSkillPoints = playerData.AvailableSkillPoints;
        skillTreeString = playerData.SkillTreeString;
      }
      if (talentDelegate != null)
      {
        talentDelegate.SetAvailableSkillPoints(availableSkillPoints);
        if (!string.IsNullOrEmpty(skillTreeString))
        {
          talentDelegate.LoadSkills(selectedPlayer, skillTreeString, loadSkills);
        }
      }
    }

    public void SelectPlayer(PlayerCharacter selectedPlayer)
    {
      for(int i = 0; i < PlayerCharacters.Length; ++i)
      {
        if(PlayerCharacters[i] == selectedPlayer)
        {
          this.selectedCharacter = i;
          break;
        }
      }
      refresh();
    }

    void ReturnToMap()
    {
      if (uiPanel != null)
      {
        Destroy(uiPanel.gameObject);
      }
      // force a player / map / ui update for selected player
      city.ChangePlayer(0);
      city.CloseFireside();
      uiPanel = null;
    }
  }
}
