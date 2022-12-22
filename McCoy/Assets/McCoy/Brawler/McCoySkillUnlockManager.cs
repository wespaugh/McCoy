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
          Debug.Log("unlocking " + atk.moveName + " by default. Look for it to be locked later");
          atk.locked = false;
        }
        moveSet.physics._moveForwardSpeedBonus = 0;
        moveSet.physics._moveBackSpeedBonus = 0;
        moveSet.physics._moveSidewaysSpeedBonus = 0;
      }

      // REALLY REALLY REALLY REALLY REMEMBER THIS
      // DON'T DELETE THIS COMMENT
      // IN FACT
      // MAKE IT OBNOXIOUSLY LONG JUST SO IT'S NOT IGNORED
      // AND DEFINITELY THIS WILL BE A PROBLEM AGAIN FOR ME,
      // OR IF SOMEONE ELSE TAKES OVER THE PROJECT
      // AND MAYBE THERE'S A WAY TO REMOVE THIS EXTRA OBSTACLE
      // BUT NO SOLUTION WAS IMMEDIATELY OBVIOUS TO ME
      // AND THAT'S AS MUCH ATTENTION AS I CAN GIVE ABSOLUTELY ANYTHING
      // OK.
      // HERE WE GO.
      // THE COMMENT IS.
      // SkillForLabel(string label) MUST  have the skill name you need to lock/unlock

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
                if(!atk.locked)
                {
                  Debug.Log("Just unlocked " + atk.moveName);
                }
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
                      Debug.Log("move " + atk2.moveName + " was unavailable, locking");
                      atk2.locked = true;
                    }
                  }
                }
                else
                {
                  Debug.Log("skill " + atk.moveName + " was replaced, locking");
                  // otherwise, lock our prerequisite so that the swapped move replaces it (moves are unlocked by default, we don't actually have to set it unlocked)
                  atk.locked = true;
                }
              }
            }
          }
        }
        if (skill.Level > 0)
        {
          foreach (var buff in skill.BuffsToAdd)
          {
            Debug.Log("UNLOCKING BUFF: " + buff.Buff);
            buff.Init(controls);
            McCoy.GetInstance().BuffManager.AddBuff(buff, controls.playerNum);
          }
        }
      }
      // TODO: Support Crit+ and Strength+ here by creating a new lookup. the lookup will take a list of prerequisite moves mapped to an unlock move. if any prereqs are locked, the unlock move will lock. if all are unlocked, all prereqs are locked.
    }
  }
}
