using UnityEditor;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.BoardGame
{
  public class McCoyMobData
  {
    public int XP { get; private set; }
    public int Health { get; private set; }

    public Factions Faction { get; private set; }

    public McCoyMobData(Factions f, int startingXP = -1, int startingHealth = -1)
    {
      int xp = startingXP < 0 ? Random.Range(1, 11) : startingXP;
      int health = startingHealth < 0 ? Random.Range(1, 7) : startingHealth;
      Initialize(xp, health, f);
    }
    public void Initialize(int startingXP, int startingHealth, Factions f)
    {
      XP = startingXP;
      Health = startingHealth;
      Faction = f;
    }
    public void ChangeHealth(int amount)
    {
      if (amount > 0) amount = Mathf.Min(6, Health + amount);
      else amount = Mathf.Max(1, Health + amount);
    }

    public int CalculateNumberOfBrawlerEnemies()
    {
      return Health * 2;
    }

    public int CalculateNumberSimultaneousBrawlerEnemies()
    {
      return Health * 1;
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
  }
}