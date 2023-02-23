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

    [SerializeField]
    Animator selectedCharacterAnimator = null;

    McCoyCityScreen city = null;
    McCoyFiresideUI uiPanel = null;

    McCoyInputManager input = null;
    bool inputInitialized = false;

    McCoySkillTreeMenu talentDelegate = null;
    McCoyLobbyingListUI lobbyingDelegate = null;

    Dictionary<string, List<PlayerCharacter>> pcGroups = null;
    int selectedPCGroupIndex = 0;

    int selectedCharacterIdx = 0;
    private bool canLobby;

    public void Refresh(McCoyCityScreen root)
    {
      this.city = root;
      if (uiPanel == null)
      {
        uiPanel = Instantiate(FiresideStatsPrefab).GetComponent<McCoyFiresideUI>();
      }

      pcGroups = new Dictionary<string, List<PlayerCharacter>>();
      foreach (var pc in PlayerCharacters)
      {
        string zoneId = McCoy.GetInstance().gameState.PlayerLocation(pc);
        if (!pcGroups.ContainsKey(zoneId))
        {
          pcGroups[zoneId] = new List<PlayerCharacter>();
        }
        pcGroups[zoneId].Add(pc);
      }

      selectedPCGroupIndex = 0;

      var skillString = McCoyGameState.Instance().playerCharacters[PlayerCharacter.Rex].SkillTreeString;
      if(string.IsNullOrEmpty(skillString))
      {
        RexSkillTree.GetComponent<TalentusEngine>().SaveToString();
        skillString = RexSkillTree.GetComponent<TalentusEngine>().ResetSkillTree();
      }
      loadSkills(McCoyGameState.GetPlayer(PlayerCharacter.Rex).AvailableSkillPoints, skillString, PlayerCharacter.Rex);

      refresh();
    }

    public void NextPlayer()
    {
      ++selectedCharacterIdx;
      if(selectedCharacterIdx >= PlayerCharacters.Length)
      {
        selectedCharacterIdx = 0;
      }
      city.ChangePlayer(1, false);
      refresh();
    }

    public void PreviousPlayer()
    {
      --selectedCharacterIdx;
      if(selectedCharacterIdx < 0)
      {
        selectedCharacterIdx = PlayerCharacters.Length - 1;
      }
      Debug.Log("Fireside selected character is now" + PlayerCharacters[selectedCharacterIdx]);
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
          if(pc == PlayerCharacters[selectedCharacterIdx])
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

      canLobby = city.Board.NodeWithID(McCoyGameState.Instance().PlayerLocation(PlayerCharacters[selectedCharacterIdx])).LobbyingAvailable;
      uiPanel.SetPlayer(PlayerCharacters[selectedCharacterIdx], city.Board, canLobby, selectedCharacterAnimator);

      foreach (var pc in PlayerCharacters)
      {
        spriteForPC(pc).transform.localScale = pc == PlayerCharacters[selectedCharacterIdx] ? new Vector3(1.2f,1.2f,1.2f) : Vector3.one;
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
        input.RegisterButtonListener(ButtonPress.Button7, PreviousPlayer);
        input.RegisterButtonListener(ButtonPress.Button8, NextPlayer);
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

      if(uiPanel != null && uiPanel.CheckInputs(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs))
      {
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
      lobbyingDelegate = Instantiate(LobbyingPrefab).GetComponent<McCoyLobbyingListUI>();
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
        uiPanel.gameObject.SetActive(true);
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
      PlayerCharacter selectedPlayer = PlayerCharacters[selectedCharacterIdx];
      string skillTreeString = "";
      int availableSkillPoints = 0;
      McCoyPlayerCharacter playerData = null;
      switch (selectedPlayer)
      {
        case PlayerCharacter.Rex:
        default:
          talentDelegate = Instantiate(RexSkillTree, city.transform.parent).GetComponent<McCoySkillTreeMenu>();
          uiPanel.gameObject.SetActive(false);
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
          this.selectedCharacterIdx = i;
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
