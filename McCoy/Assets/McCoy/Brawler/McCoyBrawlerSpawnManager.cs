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
    public bool enableSpawning = false;

    // Faction -> <totalEnemiesRemaining, avgEnemiesAtOnce>
    Dictionary<Factions, int> spawnNumbers = new Dictionary<Factions, int>();
    Dictionary<Factions, int> avgSpawnNumbers = new Dictionary<Factions, int>();
    int avgEnemiesOnscreenAtOnce = 0;
    public void Initialize(SpawnData spawns)
    {
      foreach(var s in spawns)
      {
        spawnNumbers[s.Value.Faction] = s.Value.CalculateNumberOfBrawlerEnemies();
        avgSpawnNumbers[s.Value.Faction] = s.Value.CalculateNumberSimultaneousBrawlerEnemies();
      }
      recalcAverageSpawns();

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
      if(UFE.config.lockInputs)
      {
        UFE.DelaySynchronizedAction(checkSpawns, 3.0f);
        return;
      }
      int numPlayers = 1;
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
          if (totalMonstersRemaining < randomMonsterIndex )
          {
            spawnNumbers[m.Key]--;
            UFE.CreateRandomMonster();
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