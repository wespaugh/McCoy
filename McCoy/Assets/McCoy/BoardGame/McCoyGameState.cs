using Assets.McCoy.Brawler;
using System;
using System.Collections.Generic;
using UnityEngine;

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

    Dictionary<string, List<McCoyMobData>> mobLocations = new Dictionary<string, List<McCoyMobData>>();

    Dictionary<int, MapNode> playerLocations = new Dictionary<int, MapNode>();
    BrawlerResult stageesults = null;

    public MapNode PlayerLocation(int playerNumber)
    {
      if(!playerLocations.ContainsKey(playerNumber))
      {
        return null;
      }
      return playerLocations[playerNumber];
    }

    public void SetPlayerLocation(int playerNumber, MapNode loc)
    {
      playerLocations[playerNumber] = loc;
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
    public bool CanPlayerTakeTurn(int pNumber)
    {
      return playerTurns[pNumber - 1];
    }
    public void SetPlayerTookTurn(int pNumber)
    {
      playerTurns[pNumber - 1] = false;
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