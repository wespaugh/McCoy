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
      // unlock everything upfront, and let our skills sort out what's what
      foreach (var moveSet in controls.loadedMoves)
      {
        foreach (var atk in moveSet.attackMoves)
        {
          atk.locked = false;
        }
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
            foreach(var moveSwap in skill.MoveSwaps)
            {
              if(moveSwap.ToDisable.moveName == atk.moveName)
              {
                // if our prerequisite is locked, the to-swap move must also be locked
                if(atk.locked || skill.Level == 0)
                {
                  foreach (var atk2 in moveSet.attackMoves)
                  {
                    if(moveSwap.ToEnable.moveName == atk2.moveName)
                    {
                      Debug.Log($"1. Locking move {atk2.moveName}. {atk.locked}/{skill.Level == 0}");
                      atk2.locked = true;
                    }
                  }
                }
                else
                {
                  // otherwise, lock our prerequisite so that the swapped move replaces it (moves are unlocked by default, we don't actually have to set it unlocked)
                  Debug.Log($"2. Locking move {atk.moveName}. {atk.locked}/{skill.Level == 0}");
                  atk.locked = true;
                }
              }
            }
          }
        }
      }
      // TODO: Support Crit+ and Strength+ here by creating a new lookup. the lookup will take a list of prerequisite moves mapped to an unlock move. if any prereqs are locked, the unlock move will lock. if all are unlocked, all prereqs are locked.
    }
  }
}
