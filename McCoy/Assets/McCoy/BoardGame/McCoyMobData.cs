using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.BoardGame
{
  public class McCoyMobData
  {
    public int XP { get; private set; }
    int maxHealth = 6;
    public int MaxHealth
    {
      get => maxHealth;
      private set => maxHealth = value;
    }

    public bool MarkedForDeath
    {
      get;
      private set;
    }

    private int monsterHealthScaleFactor = 6;

    private float minHealthForXP = 0.5f;
    private float regenPerWeek = 0.5f;

    List<MapNode> routedLocations = new List<MapNode>();
    public bool IsRouted
    {
      get;
      set;
    }

    public void FinishedRouting()
    {
      IsRouted = false;
      routedLocations.Clear();
    }

    public float Health { 
      get; 
      private set; 
    }

    public Factions Faction { get; private set; }

    public int MonstersPerSubstage
    {
      get;
      private set;
    }
    private int monstersKilledInStage = 0;

    public McCoyMobData(Factions f, int startingXP = -1, int startingHealth = -1)
    {
      int xp = startingXP < 0 ? UnityEngine.Random.Range(1, 11) : startingXP;
      int health = startingHealth < 0 ? UnityEngine.Random.Range(1, 7) : startingHealth;
      Initialize(xp, health, f);
    }
    public void Initialize(int startingXP, int startingHealth, Factions f)
    {
      XP = startingXP;
      Health = startingHealth;
      Faction = f;
    }

    public int StageBegan()
    {
      monstersKilledInStage = 0;
      MonstersPerSubstage = (int)(CalculateNumberOfBrawlerEnemies() / (float)UFE.config.selectedStage.stageInfo.substages.Count);
      return MonstersPerSubstage;
    }

    public void OffscreenCombat(int damage)
    {
      changeHealth(-damage);
    }

    public void StageEnded()
    {
      float percentKilled = ((float)MonstersPerSubstage) / ((float)monstersKilledInStage);
      float healthReduced = Health * percentKilled;
      Debug.Log($"Health reduced from {Health} by {healthReduced}");
      changeHealth(-healthReduced);
    }

    private void changeHealth(float amount)
    {
      if (amount > 0) Health = Mathf.Min(MaxHealth, Health + amount);
      else
      {
        float newVal = Health + amount;
        if(newVal < .5f)
        {
          IsRouted = true;
          Lose1Strength();
          Health = MaxHealth / 2;
        }
        else
        {
          Health = newVal;
        }
      }
    }

    public void MonstersKilled(int numKilled)
    {
      monstersKilledInStage += numKilled;
    }

    public float CalculateNumberOfBrawlerEnemies()
    {
      return Health * monsterHealthScaleFactor;
    }

    public int CalculateNumberSimultaneousBrawlerEnemies()
    {
      return (int)(Health * 2);
    }

    public int StrengthForXP(int XP = -1)
    {
      int val = XP < 0 ? this.XP : XP;
      if (val == 1) return 1;
      if (val == 2) return 2;
      if (val == 3) return 3;
      if (val <= 5) return 4;
      if (val <= 7) return 5;
      return 6;
    }

    public void AddXP(int amt)
    {
      if (amt > 0) XP = Mathf.Min(amt + XP, 10);
      else XP = Mathf.Max(amt + XP, 1);
    }

    public void Lose1Strength()
    {
      // if we're already weak enough, we just go down to 1 XP
      int currentStrength = StrengthForXP();
      if(currentStrength == 1)
      {
        // stop routing immediately if this mob is being destroyed
        IsRouted = false;
        MarkedForDeath = true;
      }

      if (currentStrength <= 2)
      {
        XP = 1;
        return;
      }
      // bump down 2 strength, then add 1 xp so we're at the very bottom of the target strength range
      int targetStrength = currentStrength - 2;
      while (StrengthForXP() != targetStrength) --XP;
      ++XP;
    }

    internal void WeekEnded(bool combat)
    {
      float healthPercent = ((float)Health) / ((float)MaxHealth);

      if(!combat)
      {
        if (healthPercent >= minHealthForXP)
        {
          AddXP(2);
        }
        changeHealth(regenPerWeek * (float)maxHealth);
      }
    }

    public void AddRoutingLocation(MapNode m)
    {
      routedLocations.Add(m);
    }

    public bool CanRouteTo(MapNode m)
    {
      foreach(var loc in routedLocations)
      {
        if(m.NodeID == loc.NodeID)
        {
          return false;
        }
      }
      return true;
    }

    public void Absorb(McCoyMobData otherMob)
    {
      int xpGain = (otherMob.XP / 2) + (otherMob.XP % 2 == 1 ? 1 : 0);
      AddXP(xpGain);
      Health = Math.Max(Health, otherMob.Health);
      routedLocations = otherMob.routedLocations;
      IsRouted = otherMob.IsRouted;
    }
  }
}