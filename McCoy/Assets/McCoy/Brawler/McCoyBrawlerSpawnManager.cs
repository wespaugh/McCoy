using Assets.McCoy.BoardGame;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.Brawler
{
  using SpawnData = Dictionary<Factions, McCoyMobData>;
  public class McCoyBrawlerSpawnManager : MonoBehaviour
  {
    // Faction -> <totalEnemiesRemaining, avgEnemiesAtOnce>
    Dictionary<Factions, int> spawnNumbers = new Dictionary<Factions, int>();
    Dictionary<Factions, int> avgSpawnNumbers = new Dictionary<Factions, int>();
    int avgEnemiesOnscreenAtOnce = 0;

    bool debugSpawnsOnly = false;

    bool allPlayersDead = false;

    public static void SetTeam(int id, Factions f)
    {
      UFE.brawlerEntityManager.GetControlsScript(id).Team = (int)f;
    }
    public static void SetTeam(ControlsScript s, Factions f)
    {
      SetTeam(s.playerNum, f);
    }

    public static void SetAllies(int i, List<Factions> allies)
    {
      SetAllies(UFE.brawlerEntityManager.GetControlsScript(i), allies);
    }

    public static void SetAllies(ControlsScript s, List<Factions> allies)
    {
      List<int> iAllies = new List<int>();
      allies.ForEach((x) => iAllies.Add((int)x));
      s.SetAllies(iAllies);
    }

    public void Initialize(SpawnData spawns)
    {
      allPlayersDead = false;

      foreach(var s in spawns)
      {
        spawnNumbers[s.Value.Faction] = s.Value.CalculateNumberOfBrawlerEnemies();
        avgSpawnNumbers[s.Value.Faction] = s.Value.CalculateNumberSimultaneousBrawlerEnemies();
      }
      recalcAverageSpawns();

      SetTeam(1, Factions.Werewolves);
      SetAllies(1, new List<Factions> { Factions.Werewolves } );

      UFE.DelaySynchronizedAction(checkSpawns, 3.0f);
    }

    private void recalcAverageSpawns()
    {
      avgEnemiesOnscreenAtOnce = 0;
      foreach (var factionLookup in avgSpawnNumbers)
      {
        avgEnemiesOnscreenAtOnce += factionLookup.Value;
      }
      avgEnemiesOnscreenAtOnce /= spawnNumbers.Count;
    }

    private void checkSpawns()
    {
      if(allPlayersDead)
      {
        return;
      }
      if(UFE.config.lockInputs)
      {
        UFE.DelaySynchronizedAction(checkSpawns, 3.0f);
        return;
      }
      int[] playerIDs = { 1 };

      // for one line, assume everyone's dead
      allPlayersDead = true;
      foreach (int i in playerIDs)
      {
        // if any single player is alive, set allPlayersDead back to false and break
        allPlayersDead &= UFE.brawlerEntityManager.GetControlsScript(i).currentLifePoints <= 0;
        if(!allPlayersDead)
        {
          break;
        }
      }

      if(allPlayersDead)
      {
        Debug.Log("YOU lose!");
        UFE.FireAlert("Werewolf Down!", null);

        UFE.DelaySynchronizedAction(() =>
        {
          UFE.FireGameEnds();
          UFE.EndGame();
          McCoy.GetInstance().LoadScene(McCoy.McCoyScenes.CityMap);
        }, 6.0f);
        return;
      }

      int numPlayers = playerIDs.Length;
      int numLivingEnemies = UFE.brawlerEntityManager.GetNumLivingEntities() - numPlayers;

      recalcAverageSpawns();

      // if it's time to spawn another monster
      if(avgEnemiesOnscreenAtOnce > numLivingEnemies)
      {
        int totalMonstersRemaining = 0;
        // get total remaining monsters from each mob
        foreach(var m in spawnNumbers)
        {
          totalMonstersRemaining += m.Value;
        }

        if (totalMonstersRemaining <= 0)
        {
          if (numLivingEnemies == 0)
          {
            Debug.Log("YOU WON!");
            UFE.FireAlert("All Dudes BEATEN!", null);

            UFE.DelaySynchronizedAction(() =>
            {
              UFE.FireGameEnds();
              UFE.EndGame();
              McCoy.GetInstance().LoadScene(McCoy.McCoyScenes.CityMap);
            }, 6.0f);
          }
          else
          {
            UFE.DelaySynchronizedAction(checkSpawns, 3.0f);
          }
          return;
        }

        int randomMonsterIndex = UnityEngine.Random.Range(1, totalMonstersRemaining+1);

        foreach(var m in spawnNumbers)
        {
          totalMonstersRemaining -= m.Value;
          if (totalMonstersRemaining < randomMonsterIndex && !debugSpawnsOnly)
          {
            spawnNumbers[m.Key]--;
            ControlsScript newMonster = UFE.CreateRandomMonster();
            SetTeam(newMonster, m.Key);
            SetAllies(newMonster, new List<Factions> { m.Key });
            break;
          }
        }
        UFE.DelaySynchronizedAction(checkSpawns, (float)UnityEngine.Random.Range(3, 7));
      }
      // check again in a bit
      else
      {
        UFE.DelaySynchronizedAction(checkSpawns, 3.0f);
      }

    }
  }
}