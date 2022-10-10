﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFE3D;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.RPG
{
  [Serializable]
  public class McCoyPlayerCharacter
  {
    public PlayerCharacter Player;
    public int AvailableSkillPoints = 1;
    public List<McCoySkill> Skills;

    private string _skillTreeString;
    public string SkillTreeString
    {
      get
      {
        return _skillTreeString;
      }
      set
      {
        _skillTreeString = value;
        Skills = LoadSkillsFromTalentus(_skillTreeString);
      }
    }
    public int XP = 1;

    public int[] XpThreshholds = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};

    /*
    public object Clone()
    {
      McCoyPlayerCharacter retVal = CreateInstance<McCoyPlayerCharacter>();
      retVal.Player = this.Player;
      retVal.AvailableSkillPoints = this.AvailableSkillPoints;
      retVal.Skills = new List<McCoySkill>(this.Skills);
      retVal.XP = this.XP;
      return retVal;
    }
    */
    public void GainXP(int amount)
    {
      int idx = 0;
      while(XP<XpThreshholds[idx])
      {
        ++idx;
      }
      int levelBefore = idx;
      XP += amount;
      while(XP < XpThreshholds[idx])
      {
        ++idx;
      }
      int levelsGained = idx - levelBefore;

      AvailableSkillPoints += levelsGained;
    }
  }
}
