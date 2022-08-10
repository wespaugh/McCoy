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
    int[] playerIDs = { 1 };

    // Faction -> <totalEnemiesRemaining, avgEnemiesAtOnce>
    Dictionary<Factions, int> spawnNumbers = new Dictionary<Factions, int>();
    Dictionary<Factions, int> avgSpawnNumbers = new Dictionary<Factions, int>();
    int avgEnemiesOnscreenAtOnce = 0;

    bool debugSpawnsOnly => McCoy.GetInstance().Debug;

    bool allPlayersDead = false;

    ControlsScript boss = null;
    IBossSpawnListener bossSpawnListener = null;

    ControlsScript player;

    McCoyFactionLookup factionLookup => McCoyFactionLookup.GetInstance();

    List<McCoySpawnData> spawners = null;
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

    public void Initialize(SpawnData spawns, IBossSpawnListener bossSpawnListener)
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

      player = UFE.brawlerEntityManager.GetControlsScript(1);

      this.bossSpawnListener = bossSpawnListener;

      UFE.DelaySynchronizedAction(checkSpawns, 3.0f);
    }

    private void FixedUpdate()
    {
      updateSpawners();
      updateBoss();
    }

    private void updateBoss()
    {
      if(boss && boss.currentLifePoints <= 0)
      {
        boss = null;
        bossSpawnListener.BossDied(boss);
      }
    }

    private void updateSpawners()
    {
      // unfortunately we cannot do this during initialize.
      // somehow the prefab is destroyed and by the time we get here the list is full of null refs
      if (spawners == null)
      {
        initSpawners();
      }
      if(debugSpawnsOnly)
      {
        return;
      }
      foreach (var spawner in spawners)
      {
        if (player.worldTransform.position.x >= spawner.xPosition)
        {
          spawners.RemoveAt(0);
          Debug.Log("looking up enemy named " + spawner.EnemyName);
          factionLookup.FindCharacterInfo(spawner.EnemyName, out var charInfo, out var fac);
          var monsterCScript = createMonster(charInfo, fac);
          if (bossSpawnListener != null && spawner.IsBoss)
          {
            boss = monsterCScript;
            bossSpawnListener.BossSpawned(monsterCScript);
          }
          break;
        }
      }
    }

    private void initSpawners()
    {
      spawners = new List<McCoySpawnData>();
      GameObject spawnerRoot = GameObject.FindGameObjectWithTag("Spawner");
      var spawnerList = new List<McCoySpawnerTrigger>(spawnerRoot.GetComponentsInChildren<McCoySpawnerTrigger>());
      spawnerList.Sort((a, b) => { return (int)(a.transform.position.x < b.transform.position.x ? -1 : 1); });

      while(spawnerList.Count > 0)
      {
        var spawner = spawnerList[0];
        spawnerList.Remove(spawner);
        if (spawner != null)
        {
          spawner.spawnData.Initialize(spawner.gameObject.transform.localPosition.x);
          spawners.Add(spawner.spawnData);
          Destroy(spawner.gameObject);
        }
      }
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

      updateAllPlayersDead();

      if(allPlayersDead)
      {
        fireLose();
        return;
      }

      int numPlayers = playerIDs.Length;
      int numLivingEnemies = UFE.brawlerEntityManager.GetNumLivingEntities() - numPlayers;

      recalcAverageSpawns();

      float recheckDelay = 3.0f;

      // if it's time to spawn another monster
      if(avgEnemiesOnscreenAtOnce > numLivingEnemies)
      {
        recalcRemainingMonsters(out var totalMonstersRemaining);

        if (totalMonstersRemaining <= 0 && numLivingEnemies == 0 && spawners.Count == 0)
        {
            fireWon();
            return;
        }
        if (!debugSpawnsOnly)
        {
          spawnRandomMonster(totalMonstersRemaining);
        }
        recheckDelay = (float)UnityEngine.Random.Range(3, 7);
      }

      UFE.DelaySynchronizedAction(checkSpawns, recheckDelay);

    }

    private void recalcRemainingMonsters(out int totalMonstersRemaining)
    {
      totalMonstersRemaining = 0;
      // get total remaining monsters from each mob
      foreach (var m in spawnNumbers)
      {
        totalMonstersRemaining += m.Value;
      }
    }

    private void fireWon()
    {
      UFE.FireAlert("All Dudes BEATEN!", null);

      UFE.DelaySynchronizedAction(() =>
      {
        UFE.FireGameEnds();
        UFE.EndGame();
        McCoy.GetInstance().LoadScene(McCoy.McCoyScenes.CityMap);
      }, 6.0f);
    }

    private void fireLose()
    {
      UFE.FireAlert("Werewolf Down!", null);

      UFE.DelaySynchronizedAction(() =>
      {
        UFE.FireGameEnds();
        UFE.EndGame();
        McCoy.GetInstance().LoadScene(McCoy.McCoyScenes.CityMap);
      }, 6.0f);
    }

    private void updateAllPlayersDead()
    {
      // for one line, assume everyone's dead
      allPlayersDead = true;
      foreach (int i in playerIDs)
      {
        // if any single player is alive, set allPlayersDead back to false and break
        allPlayersDead &= UFE.brawlerEntityManager.GetControlsScript(i).currentLifePoints <= 0;
        if (!allPlayersDead)
        {
          break;
        }
      }
    }

    private void spawnRandomMonster(int totalMonstersRemaining = -1)
    {
      if(totalMonstersRemaining < 0)
      {
        totalMonstersRemaining = 0;
        // get total remaining monsters from each mob
        foreach (var m in spawnNumbers)
        {
          totalMonstersRemaining += m.Value;
        }
      }

      if(totalMonstersRemaining < 0)
      {
        Debug.LogWarning("spawned more actors than we anticipated");
        createMonster(null, Factions.Mages);
      }

      int randomMonsterIndex = UnityEngine.Random.Range(1, totalMonstersRemaining+1);
      foreach (var m in spawnNumbers)
      {
        totalMonstersRemaining -= m.Value;
        if (totalMonstersRemaining < randomMonsterIndex && !debugSpawnsOnly)
        {
          spawnNumbers[m.Key]--;
          UFE3D.CharacterInfo toSpawn = factionLookup.RandomEnemy(m.Key);
          createMonster(toSpawn, m.Key);
          break;
        }
      }
    }

    private ControlsScript createMonster(UFE3D.CharacterInfo info, Factions f)
    {
      ControlsScript newMonster = UFE.CreateRandomMonster(info);
      SetTeam(newMonster, f);
      SetAllies(newMonster, new List<Factions> { f });
      return newMonster;
    }
  }
}