using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.McCoy.RPG
{
  public class McCoyQuestData : ScriptableObject
  {
    // unique ID
    public string uuid;

    // brief overview seen from the world map
    public string summary;

    // global turns where the quest might spawn
    public Range weekAvailability;

    // length of time before the quest disappears
    public int worldDurationInGlobalTurns;

    // quests that must have been completed before this quest becomes available
    List<string> prerequisiteQuestIds = new List<string>();

    /// OBJECTIVE LIST?
    /// 

    // displayed in-level when the quest starts
    public string introText;

    // displayed in level when the final objective is completed
    string exitText;

    // options given to the player about how to complete the quest
    List<string> exitChoices = new List<string>();
  }
}
