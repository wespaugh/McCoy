using Assets.McCoy.RPG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.McCoy.Brawler
{
  public class McCoySkillUnlockManager
  {
    public static void PlayerSpawned(ControlsScript controls, List<McCoySkill> mcCoySkills)
    {
      if(mcCoySkills == null)
      {
        return;
      }
      foreach (var skill in mcCoySkills)
      {
        foreach (var moveSet in controls.loadedMoves)
        {
          foreach (var atk in moveSet.attackMoves)
          {
            foreach (var move in skill.EnabledMoves)
            {
              if (move.moveName == atk.moveName)
              {
                atk.locked = skill.Level == 0;
              }
            }
          }
        }
      }
    }
  }
}
