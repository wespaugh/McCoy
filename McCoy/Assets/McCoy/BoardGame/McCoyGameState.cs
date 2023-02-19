using Assets.McCoy.RPG;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

    private int credits = 500;
    public int Credits
    {
      get => credits;
    }

    public float FinalBossHealth = 100000f;
    public bool FinalBattle
    {
      get;set;
    }

    public static McCoyGameState Instance()
    {
      return McCoy.GetInstance().gameState;
    }

    // as the player takes each PC turn, these timers tick down
    // once all four are at 0, a Week ends
    float[] playerTurns = { PC_TIME_PER_WEEK, PC_TIME_PER_WEEK, PC_TIME_PER_WEEK, PC_TIME_PER_WEEK };

    public PlayerCharacter SelectedPlayer;

    public Dictionary<PlayerCharacter, McCoyPlayerCharacter> playerCharacters = new Dictionary<PlayerCharacter, McCoyPlayerCharacter>();
    public static McCoyPlayerCharacter GetPlayer(PlayerCharacter pc)
    {
      return Instance().GetPlayerCharacter(pc);
    }
    public McCoyPlayerCharacter GetPlayerCharacter(PlayerCharacter pc)
    {
      return playerCharacters[pc];
    }

    Dictionary<string, List<McCoyMobData>> mobLocations = new Dictionary<string, List<McCoyMobData>>();
    Dictionary<PlayerCharacter, string> playerLocations = new Dictionary<PlayerCharacter, string>();

    // uuids of all quests that have spawned at some point
    public List<string> questsSpawned = new List<string>();
    public List<McCoyQuestData> availableQuests = new List<McCoyQuestData>();
    public McCoyQuestData activeQuest { get; set; }

    // flags set as quests are completed and choices are made
    public List<string> questFlags = new List<string>();
    public List<McCoyLobbyingCause.LobbyingCause> causesLobbiedFor = new List<McCoyLobbyingCause.LobbyingCause>();

    string antikytheraMechanismLocation;
    public string AntikytheraMechanismLocation
    {
      get
      {
        if(antikytheraMechanismLocation == null || string.IsNullOrEmpty(antikytheraMechanismLocation))
        {
          return null;
        }
        return antikytheraMechanismLocation;
      }
      set
      {
        antikytheraMechanismLocation = value;
      }
    }

    Dictionary<string, MapNodeSearchData> nodeSearchData = new Dictionary<string, MapNodeSearchData>();

    public void Save()
    {
      if(! Initialized)
      {
        return;
      }
      BinaryFormatter bf = new BinaryFormatter();
      FileStream file = File.Create(SaveFilename(1));
      bf.Serialize(file, this);
      file.Close();
    }

    public void Load()
    {
      BinaryFormatter bf = new BinaryFormatter();
      FileStream file = File.Open(SaveFilename(1), FileMode.Open);
      McCoyGameState save = (McCoyGameState)bf.Deserialize(file);
      file.Close();
      Initialized = true;
      weekNumber = save.weekNumber;
      playerTurns = save.playerTurns;
      AntikytheraMechanismLocation = save.AntikytheraMechanismLocation;
      mobLocations = save.mobLocations;
      playerLocations = save.playerLocations;
      playerCharacters = save.playerCharacters;
      nodeSearchData = save.nodeSearchData;
      credits = save.credits;
      causesLobbiedFor = save.causesLobbiedFor;
      SelectedPlayer = save.SelectedPlayer;
      FinalBossHealth = save.FinalBossHealth;
    }

    public void Initialize(List<MapNode> mapNodes)
    {
      Initialized = true;

      foreach (var mapNode in mapNodes)
      {
        List<Factions> fs = new List<Factions>();
        do
        {
          if (UnityEngine.Random.Range(0, 6) <= 1) fs.Add(Factions.AngelMilitia);
          if (UnityEngine.Random.Range(0, 6) <= 1) fs.Add(Factions.CyberMinotaurs);
          if (UnityEngine.Random.Range(0, 6) <= 1) fs.Add(Factions.Mages);
        } while (fs.Count == 0);
        List<McCoyMobData> mobData = new List<McCoyMobData>();
        foreach (var f in fs)
        {
          mobData.Add(new McCoyMobData(f));
        }
        SetMobs(mapNode.NodeID, mobData);
      }

      for (int i = 0; i < PlayerCharacters.Length; ++i)
      {
        PlayerCharacter pc = PlayerCharacters[i];
        playerCharacters[pc] = new McCoyPlayerCharacter() { Player = pc };
      }
    }

    public void InitPlayerStartingLocations(List<MapNode> mapNodes)
    {
      List<int> indices = new List<int>();
      // add a list of map point indexes to randomly pull from
      for (int i = 0; i < mapNodes.Count; ++i)
      {
        indices.Add(i);
      }
      PlayerCharacter[] players = PlayerCharacters;
      for (int i = 0; i < players.Length; ++i)
      {
        while (indices.Count > 0)
        {
          int index = UnityEngine.Random.Range(0, indices.Count); // the index in a list of numbers to randomly pick
          int randomMapPoint = indices[index]; // the randomly picked number
          indices.RemoveAt(index); // remove the index so the same map point isn't picked again
          MapNode startLoc = mapNodes[randomMapPoint];
          // don't start players on disabled nodes
          if (startLoc.Disabled)
          {
            continue;
          }
          McCoy.GetInstance().gameState.SetPlayerLocation(players[i], mapNodes[randomMapPoint]); // add the map node at the randomly picked number
          break;
        }
      }
    }

    public void StartQuest(McCoyQuestData quest)
    {
      questsSpawned.Add(quest.uuid);
      availableQuests.Add(quest);
    }

    public void UpdateSearchData(List<MapNodeSearchData> searchData)
    {
      nodeSearchData.Clear();
      foreach(var s in searchData)
      {
        nodeSearchData[s.NodeID] = s;
      }
    }

    public void CompleteQuest()
    {
      activeQuest.Complete = true;
      McCoy.GetInstance().gameState.questFlags.Add(activeQuest.uuid);
      availableQuests.Remove(activeQuest);
      activeQuest = null;
    }

    public MapNodeSearchData GetSearchData(string nodeID)
    {
      if(string.IsNullOrEmpty(nodeID) || !nodeSearchData.ContainsKey(nodeID))
      {
        return null;
      }
      return nodeSearchData[nodeID];
    }

    public string PlayerLocation(PlayerCharacter player)
    {
      if(!playerLocations.ContainsKey(player) || string.IsNullOrEmpty(playerLocations[player]))
      {
        return null;
      }
      return playerLocations[player];
    }

    public void SetPlayerLocation(PlayerCharacter player, MapNode loc)
    {
      playerLocations[player] = loc.NodeID;
    }

    public void SetMobs(string nodeID, List<McCoyMobData> mobs)
    {
      mobLocations[nodeID] = mobs;
    }

    public List<McCoyMobData> GetMobs(string nodeID)
    {
      return mobLocations[nodeID];
    }

    public bool IsEndOfWeek
    {
      get
      {
        bool retVal = true;
        // it's only the end of the week if all 4 players do NOT have a turn left
        foreach(var p in playerTurns)
        {
          retVal &= p == 0;
        }
        return retVal;
      }
    }

    public void ReceiveCredits(int rewardCredits)
    {
      credits += rewardCredits;
    }

    public void Spend(int credits)
    {
      this.credits -= credits;
      if(credits < 0)
      {
        Debug.LogWarning("You have spent too much!");
      }
    }

    public void UpdateSkills(PlayerCharacter player, string v, int availableSkillPoints)
    {
      playerCharacters[player].SkillTreeString = v;
      playerCharacters[player].AvailableSkillPoints = availableSkillPoints;
    }

    public McCoyEquipmentLoadout PlayerEquipment(PlayerCharacter player)
    {
      return playerCharacters[player].Equipment;
    }

    public bool CanPlayerTakeTurn(PlayerCharacter player)
    {
      return playerTurns[(int)player] > 0;
    }

    public void UpdatePlayerTimeRemaining(PlayerCharacter player, float timeElapsed)
    {
      playerTurns[(int)player] = Mathf.Max(playerTurns[(int)player]- timeElapsed,0f);
    }
    public void EndWeek()
    {
      for(int i = 0; i < playerTurns.Length; ++i)
      {
        playerTurns[i] = PC_TIME_PER_WEEK;
      }
      ++weekNumber;
    }

    public float TurnTimeRemaining(PlayerCharacter selectedPlayer)
    {
      return playerTurns[(int)selectedPlayer];
    }

    public void BuyItem(McCoyEquipmentItem item)
    {
      playerCharacters[SelectedPlayer].Equipment.AddEquipment(item);
    }

    public void PickUpCredits(int amount)
    {
      credits += amount;
    }
  }
}