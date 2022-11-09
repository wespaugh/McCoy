using System;
using System.Collections.Generic;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.RPG
{
  [Serializable]
  public class McCoyQuestData
  {
    // unique ID
    public string uuid;

    // quest name
    public string title;

    // brief overview seen from the world map
    public string summary;

    // global turns where the quest might spawn
    public int firstWeekAvailable;
    public int lastWeekAvailable;

    // length of time before the quest disappears
    public int worldDurationInGlobalTurns;

    // quests flags that must have been set before this quest becomes available
    public List<string> prerequisiteQuestFlags = new List<string>();

    // quests that must have been completed before this quest becomes available
    public List<string> possibleLocations = new List<string>();

    /// OBJECTIVE LIST?
    /// 

    // displayed in-level when the quest starts
    public string introText;

    // displayed in level when the final objective is completed
    public string exitText;

    // options given to the player about how to complete the quest
    public List<McCoyQuestChoice> exitChoices = new List<McCoyQuestChoice>();
    public PlayerCharacter characterRestriction { get; set; }
  }
}
