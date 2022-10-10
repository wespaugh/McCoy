using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.BoardGame
{
  [Serializable]
  public class McCoyMobData
  {
    int xp;
    public int XP 
    {
      get => xp;
      private set
      {
        xp = value;
      } 
    }
    int maxHealth = 6;
    public int MaxHealth
    {
      get => maxHealth;
      private set => maxHealth = value;
    }

    bool markedForDeath;
    public bool MarkedForDeath
    {
      get => markedForDeath;
      private set
      {
        markedForDeath = value;
      }
    }

    private int monsterHealthScaleFactor = 6;

    private float minHealthForXP = 0.5f;
    private float regenPerWeek = 0.5f;

    [NonSerialized]
    List<MapNode> routedLocations = new List<MapNode>();
    public List<MapNode> RoutedLocations
    {
      get
      {
        if(routedLocations == null)
        {
          routedLocations = new List<MapNode>();
        }
        return routedLocations;
      }
      private set
      {
        routedLocations = value;
      }
    }

    bool isRouted;
    public bool IsRouted
    {
      get => isRouted;
      set
      {
        isRouted = value;
      }
    }

    float health;
    public float Health {
      get => health;
      private set
      {
        health = value;
      }
    }

    Factions faction;
    public Factions Faction 
    { 
      get => faction;
      private set
      {
        faction = value;
      }
    }

    int monstersPerSubstage;
    public int MonstersPerSubstage
    {
      get => monstersPerSubstage;
      private set
      {
        monstersPerSubstage = value;
      }
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
      float healthReduced = calculateHealthDelta(monstersKilledInStage, true);
      changeHealth(healthReduced);
    }

    public void FinishedRouting()
    {
      IsRouted = false;
      RoutedLocations.Clear();
    }

    public float HealthPreview(int monstersKilled)
    {
      return Health + calculateHealthDelta(monstersKilled);
    }

    private float calculateHealthDelta(int monstersKilled, bool log = false)
    {
      float healthReduced = monstersKilled / monsterHealthScaleFactor;
      if (log)
      {
        Debug.Log($"{monstersKilled} {Faction} killed. Health reduced from {Health} by {healthReduced}");
      }
      return -healthReduced;
    }

    private void changeHealth(float amount)
    {
      if (amount > 0) Health = Mathf.Min(MaxHealth, Health + amount);
      else
      {
        float newVal = Health + amount;
        if(newVal < MOB_ROUTING_HEALTH_THRESHOLD)
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
      RoutedLocations.Add(m);
    }

    public bool CanRouteTo(MapNode m)
    {
      foreach(var loc in RoutedLocations)
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
      RoutedLocations = otherMob.RoutedLocations;
    }

    public McCoyMobData Split()
    {
      int newXP = (int) Math.Round((float)XP / 2f);
      XP = newXP;
      McCoyMobData retVal = new McCoyMobData(Faction, newXP, (int) Health);
      return retVal;
    }
  }
}