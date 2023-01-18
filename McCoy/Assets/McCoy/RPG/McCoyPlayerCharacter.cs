using System;
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
    public int AvailableSkillPoints = 10;
    public List<McCoySkill> Skills;

    private string _skillTreeString;
    private McCoyEquipmentLoadout equipment = new McCoyEquipmentLoadout();
    public McCoyEquipmentLoadout Equipment
    {
      get { return equipment; }
    }
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

    public int[] XpThreshholds = {
      12, 24, 40, 60, 85, 
      115, 150, 200, 275, 380,
      500, 750, 1000, 1500, 2100, 
      2800, 3800, 5000, 6500, 9001
    };


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
      while(idx < XpThreshholds.Length && XP>XpThreshholds[idx])
      {
        ++idx;
      }
      int levelBefore = idx;
      XP += amount;
      while(idx < XpThreshholds.Length && XP > XpThreshholds[idx])
      {
        ++idx;
      }
      int levelsGained = idx - levelBefore;
      AvailableSkillPoints += levelsGained;
    }

  }
}
