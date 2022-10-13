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

  [Serializable]
  public class McCoyMoveSwap
  {
    [SerializeField]
    public MoveInfo ToDisable;
    [SerializeField]
    public MoveInfo ToEnable;
  }


  /// <summary>
  /// A list of moves to replace when a skill is set as unlocked
  /// The goal here is to enable things like STR+ and CRIT+ to work together properly, with minimal modification to UFE, but the cost is complexity of this list
  /// STR+ Skill will have: 
  /// - BasicPunch becomes StrongPunch
  /// - CritUpPunch becomes StrongCritUpPunch
  /// CRIT+:
  /// - BasicPunch becomes CritUpPunch
  /// - StrongPunch becomes StrongCritUpPunch
  /// </summary>
  [Serializable]
  public class McCoySkillMoveSwapTuple
  {
    public PlayerSkills skill;
    public List<McCoyMoveSwap> MoveSwaps;
  }

  public class McCoySkillMoveInfoLookup : MonoBehaviour
  {
    Dictionary<PlayerSkills, List<MoveInfo>> moveLookupDict = null;
    Dictionary<PlayerSkills, List<McCoyMoveSwap>> moveSwapLookupDict = null;

    [SerializeField]
    List<McCoySkillLookupTuple> moveLookup;

    [SerializeField]
    List<McCoySkillMoveSwapTuple> moveSwapLookup;

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
        // Debug.LogWarning("Unable to find skill " + skill + " in move dictionary");
        return new List<MoveInfo>();
      }
      return moveLookupDict[skill];
    }
    public List<McCoyMoveSwap> GetMoveSwapsForSkill(string skillName)
    {
      PlayerSkills s = SkillForLabel(skillName);
      if(s == PlayerSkills.Invalid)
      {
        Debug.LogWarning("Invalid skill name: " + skillName);
        return new List<McCoyMoveSwap>();
      }
      return GetMoveSwapsForSkill(s);
    }
    public List<McCoyMoveSwap> GetMoveSwapsForSkill(PlayerSkills skill)
    {
      if(moveSwapLookupDict == null)
      {
        moveSwapLookupDict = new Dictionary<PlayerSkills, List<McCoyMoveSwap>>();
        foreach(var swap in moveSwapLookup)
        {
          moveSwapLookupDict[swap.skill] = swap.MoveSwaps;
        }
      }
      if(!moveSwapLookupDict.ContainsKey(skill))
      {
        // Debug.LogWarning("Unable to find move swap " + skill + " in swap dictionary");
        return new List<McCoyMoveSwap>();
      }
      return moveSwapLookupDict[skill];
    }
  }
}
