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
      foreach (var skill in mcCoySkills)
      {
        foreach (var move in skill.MovesToEnable)
        {
          Debug.Log($"skill name {move.moveName}");
          foreach (var moveSet in controls.loadedMoves)
          {
            foreach (var atk in moveSet.attackMoves)
            {
              if (move.moveName == atk.moveName)
              {
                Debug.Log("Found Move to Enable! " + move.moveName + " skill level " + skill.Level);
                atk.locked = skill.Level == 0;
              }
            }
          }
        }
      }
    }
  }
}
