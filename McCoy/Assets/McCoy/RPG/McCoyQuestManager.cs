using Assets.McCoy.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.RPG
{
  public class McCoyQuestManager : MonoBehaviour
  {
    // all quests, directly loaded from data files. do not modify
    Dictionary<PlayerCharacter, McCoyQuestListData> theQuests = new Dictionary<PlayerCharacter, McCoyQuestListData>();

    // available quests in the current game. each time a save file is loaded, The Quests is copied in, and then all quests that have already been seen by the player are removed
    Dictionary<string, McCoyQuestData> availableQuests = new Dictionary<string, McCoyQuestData>();

    static McCoyQuestManager instance = null;
    public static McCoyQuestManager GetInstance()
    {
      if (instance == null)
      {
        instance = GameObject.Find("UFE Manager").GetComponent<McCoyQuestManager>();
      }
      return instance;
    }

    private void Awake()
    {
      Initialize();
    }

    void Initialize()
    {
      if (theQuests.Count != 0)
      {
        return;
      }
      StartCoroutine(loadQuests());
    }
    IEnumerator loadQuests()
    {
      ResourceRequest worldRequest = Resources.LoadAsync<McCoyQuestListData>("Quests/World Quests");
      yield return worldRequest;
      theQuests[PlayerCharacter.None] = worldRequest.asset as McCoyQuestListData;

      ResourceRequest rexRequest = Resources.LoadAsync<McCoyQuestListData>("Quests/Rex Quests");
      yield return rexRequest;
      theQuests[PlayerCharacter.Rex] = rexRequest.asset as McCoyQuestListData;
    }

    public void ClearQuestData()
    {
      availableQuests.Clear();
    }

    public void GameLoaded()
    {
      availableQuests.Clear();
      foreach (var questList in theQuests)
      {
        foreach (var q in questList.Value.quests)
        {
          q.characterRestriction = questList.Key;
          availableQuests.Add(q.uuid, q);
        }
      }
      foreach(var previousQuest in McCoy.GetInstance().gameState.questsSpawned)
      {
        availableQuests.Remove(previousQuest);
      }
    }

    public void CityLoaded()
    {
      List<string> toRemove = new List<string>();
      List<McCoyQuestData> possibleQuests = new List<McCoyQuestData>();
      foreach(var questKvp in availableQuests)
      {
        var quest = questKvp.Value;
        int currentWeek = McCoy.GetInstance().gameState.Week;
        if (currentWeek > quest.lastWeekAvailable)
        {
          toRemove.Add(quest.uuid);
        }
        else if (currentWeek >= quest.firstWeekAvailable)
        {
          bool canAdd = true;
          foreach(var prereq in quest.prerequisiteQuestFlags)
          {
            // if there is a prereq that hasn't be satisfied, we can't add the quest
            if(!McCoy.GetInstance().gameState.questFlags.Contains(prereq))
            {
              canAdd = false;
              break;
            }
            // if there's already a quest active at this quest's location, we can't add it (max one quest per location)
            foreach(var activeQuest in McCoy.GetInstance().gameState.availableQuests)
            {
              if(activeQuest.possibleLocations[0] == quest.possibleLocations[0])
              {
                canAdd = false;
                break;
              }
            }
          }
          if(canAdd)
          {
            possibleQuests.Add(quest);
          }
        }
      }
      if(possibleQuests.Count > 0)
      {
        int idx = Random.Range(0, possibleQuests.Count);
        McCoyQuestData randomQuest = possibleQuests[idx];
        McCoy.GetInstance().gameState.StartQuest(randomQuest);
        availableQuests.Remove(randomQuest.uuid);
      }
    }
  }
}
