using System.Collections.Generic;
using UnityEngine;

namespace Assets.McCoy.RPG
{
  [System.Serializable]
  public class McCoyQuestListData : ScriptableObject
  {
    public McCoyQuestData[] quests = new McCoyQuestData[0];
  }
}
