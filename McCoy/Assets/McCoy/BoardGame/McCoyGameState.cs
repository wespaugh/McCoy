using Assets.McCoy.Brawler;
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

    public static McCoyGameState Instance()
    {
      return McCoy.GetInstance().gameState;
    }

    // as the player takes each PC turn, these timers tick down
    // once all four are at 0, a Week ends
    float[] playerTurns = { PC_TIME_PER_WEEK, PC_TIME_PER_WEEK, PC_TIME_PER_WEEK, PC_TIME_PER_WEEK };

    [NonSerialized]
    public PlayerCharacter selectedPlayer;

    public Dictionary<PlayerCharacter, McCoyPlayerCharacter> playerCharacters = new Dictionary<PlayerCharacter, McCoyPlayerCharacter>();

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
      Debug.Log("Saving to " + SaveFilename(1));
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
    }

    public void Initialize()
    {
      Initialized = true;
      for(int i = 0; i < PlayerCharacters.Length; ++i)
      {
        PlayerCharacter pc = PlayerCharacters[i];
        /*
        string path = $"{PLAYERCHARACTER_DIRECTORY}/{PlayerName(pc)}";
        Debug.Log(path);
        McCoyPlayerCharacter pcData = Resources.Load<McCoyPlayerCharacter>(path);
        */
        playerCharacters[pc] = new McCoyPlayerCharacter() { Player = pc };// pcData.Clone() as McCoyPlayerCharacter;
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
      if(!mobLocations.ContainsKey(nodeID))
      {
        Debug.LogError("unable to locate node with ID " + nodeID);
        Debug.LogError("there were " + mobLocations.Count + " locations in the dictionary");
        return new List<McCoyMobData>();
      }
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

    public void Spend(int credits)
    {
      credits -= credits;
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

    public bool CanPlayerTakeTurn(PlayerCharacter player)
    {
      return playerTurns[(int)player] > 0;
    }

    public void UpdatePlayerTimeRemaining(PlayerCharacter player, float time)
    {
      playerTurns[(int)player] = Mathf.Max(playerTurns[(int)player]- time,0f);
      Debug.Log("updated player time remaining. " + player + " now has " + playerTurns[(int)player] + " seconds left");
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
  }
}