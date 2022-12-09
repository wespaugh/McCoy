using Assets.McCoy.BoardGame;
using Assets.McCoy.UI;
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
    GameObject LobbyingUIPrefab = null;

    McCoyCityScreen root = null;
    GameObject lobbyingUI = null;

    McCoyInputManager input = null;
    bool inputInitialized = false;

    Dictionary<string, List<PlayerCharacter>> pcGroups = null;
    int selectedPCGroupIndex = 0;

    int selectedCharacter = 0;

    public void UpdateWithPCGroups(McCoyCityScreen root, Dictionary<string, List<PlayerCharacter>> characterGroups)
    {
      this.root = root;
      if (lobbyingUI == null)
      {
        lobbyingUI = Instantiate(LobbyingUIPrefab);
        lobbyingUI.GetComponent<McCoyLobbyingListUI>().Initialize(root);
      }
      lobbyingUI.transform.SetParent(root.transform);
      lobbyingUI.gameObject.SetActive(false);

      pcGroups = characterGroups;
      selectedPCGroupIndex = 0;
      refresh();
    }

    public void NextPlayer()
    {
      ++selectedCharacter;
      if(selectedCharacter >= PlayerCharacters.Length)
      {
        selectedCharacter = 0;
      }
      refresh();
    }

    public void PreviousPlayer()
    {
      --selectedCharacter;
      if(selectedCharacter < 0)
      {
        selectedCharacter = PlayerCharacters.Length - 1;
      }
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

      bool canLobby = root.Board.NodeWithID(McCoyGameState.Instance().PlayerLocation(PlayerCharacters[selectedCharacter])).LobbyingAvailable;
      lobbyingUI.gameObject.SetActive(canLobby);

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
        input.RegisterButtonListener(ButtonPress.Button6, NextPlayer);
        input.RegisterButtonListener(ButtonPress.Button5, PreviousPlayer);
        input.RegisterButtonListener(ButtonPress.Button3, ReturnToMap);
      }

      return input.CheckInputs(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
    }
    
    void ReturnToMap()
    {
      root.CloseFireside();
      Destroy(lobbyingUI);
      lobbyingUI = null;
    }
  }
}
