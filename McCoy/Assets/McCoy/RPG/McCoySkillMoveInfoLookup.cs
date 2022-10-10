using System;
using System.Collections.Generic;
using UFE3D;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.RPG
{
  [Serializable]
  public class McCoySkillLookupTuple
  {
    public PlayerSkills skill;
    public List<MoveInfo> moves;
  }
  public class McCoySkillMoveInfoLookup : MonoBehaviour
  {
    Dictionary<PlayerSkills, List<MoveInfo>> moveLookupDict = null;

    [SerializeField]
    List<McCoySkillLookupTuple> moveLookup;

    public List<MoveInfo> GetMoveUnlocksForSkill(string skill)
    {
      PlayerSkills s = SkillForLabel(skill);
      if(s == PlayerSkills.Invalid)
      {
        Debug.LogWarning("Invalid skill name " + skill);
        return new List<MoveInfo>();
      }
      return GetMoveUnlocksForSkill(SkillForLabel(skill));
    }
    public List<MoveInfo> GetMoveUnlocksForSkill(PlayerSkills skill)
    {
      if(moveLookupDict == null)
      {
        moveLookupDict = new Dictionary<PlayerSkills, List<MoveInfo>>();
        foreach(var m in moveLookup)
        {
          moveLookupDict[m.skill] = m.moves;
        }
      }
      if(! moveLookupDict.ContainsKey(skill))
      {
        Debug.LogWarning("Unable to find skill " + skill + " in move dictionary");
      }
      return moveLookupDict[skill];
    }
  }
}
