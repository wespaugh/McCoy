using Assets.McCoy.Brawler;
using Assets.McCoy.RPG;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.BoardGame
{
  [Serializable]
  public class McCoyGameState
  {
    [NonSerialized]
    public bool Initialized;

    private int weekNumber = 1;
    public int Week
    {
      get => weekNumber;
    }

    public Dictionary<PlayerCharacter, McCoyPlayerCharacter> playerCharacters = new Dictionary<PlayerCharacter, McCoyPlayerCharacter>();

    Dictionary<string, List<McCoyMobData>> mobLocations = new Dictionary<string, List<McCoyMobData>>();
    Dictionary<PlayerCharacter, MapNode> playerLocations = new Dictionary<PlayerCharacter, MapNode>();
    BrawlerResult stageesults = null;

    public MapNode AntikytheraMechanismLocation
    {
      get;
      set;
    }

    public void Initialize()
    {
      Initialized = true;
      for(int i = 0; i < PlayerCharacters.Length; ++i)
      {
        PlayerCharacter pc = PlayerCharacters[i];
        string path = $"{PLAYERCHARACTER_DIRECTORY}/{PlayerName(pc)}";
        Debug.Log(path);
        McCoyPlayerCharacter pcData = Resources.Load<McCoyPlayerCharacter>(path);
        playerCharacters[pc] = pcData.Clone() as McCoyPlayerCharacter;
      }
    }

    public MapNode PlayerLocation(PlayerCharacter player)
    {
      if(!playerLocations.ContainsKey(player))
      {
        return null;
      }
      return playerLocations[player];
    }

    public void SetPlayerLocation(PlayerCharacter player, MapNode loc)
    {
      playerLocations[player] = loc;
    }

    public void SetMobs(string nodeID, List<McCoyMobData> mobs)
    {
      mobLocations[nodeID] = mobs;
    }

    public List<McCoyMobData> GetMobs(string nodeID)
    {
      if(!mobLocations.ContainsKey(nodeID))
      {
        Debug.LogError("unable to locate node with ID " + nodeID);
        Debug.LogError("there were " + mobLocations.Count + " locations in the dictionary");
        return new List<McCoyMobData>();
      }
      return mobLocations[nodeID];
    }

    // as the player takes each PC turn, these are flipped false
    // once all four are flipped false, a Week ends
    bool[] playerTurns = { true, true, true, true };

    public bool IsEndOfWeek
    {
      get
      {
        bool retVal = true;
        // it's only the end of the week if all 4 players do NOT have a turn left
        foreach(var p in playerTurns)
        {
          retVal &= !p;
        }
        return retVal;
      }
    }
    public bool CanPlayerTakeTurn(PlayerCharacter player)
    {
      return playerTurns[(int)player];
    }
    public void SetPlayerTookTurn(PlayerCharacter player)
    {
      playerTurns[(int)player] = false;
    }
    public void EndWeek()
    {
      for(int i = 0; i < playerTurns.Length; ++i)
      {
        playerTurns[i] = true;
      }
      ++weekNumber;
    }
  }
}