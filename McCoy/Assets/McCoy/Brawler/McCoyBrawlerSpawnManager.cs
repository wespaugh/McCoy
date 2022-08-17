using Assets.McCoy.BoardGame;
using Assets.McCoy.Brawler.Stages;
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

    // Faction -> totalEnemiesRemaining
    Dictionary<Factions, int> spawnNumbers = new Dictionary<Factions, int>();
    // Faction -> avgEnemiesAtOnce
    Dictionary<Factions, int> avgSpawnNumbers = new Dictionary<Factions, int>();

    bool debugSpawnsOnly => McCoy.GetInstance().Debug;

    int monstersSpawned = 0;
    int totalMonstersToSpawn = 0;
    int avgEnemiesOnscreenAtOnce = 0;
    float playerStartX = 0;
    [SerializeField]
    float currentPlayerProgress = 0; // measured in number of enemies they should have encountered
    float levelBoundsStart, levelBoundsEnd;
    ControlsScript player;
    bool allPlayersDead = false;
    bool transitioning = false;
    ControlsScript boss = null;
    IBossSpawnListener bossSpawnListener = null;
    McCoyFactionLookup factionLookup => McCoyFactionLookup.GetInstance();
    List<McCoySpawnData> spawners = null;
    SpawnData spawnData = null;

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
      spawnData = spawns;
      allPlayersDead = false;
      spawners = null;

      foreach(var s in spawns)
      {
        spawnNumbers[s.Value.Faction] = s.Value.CalculateNumberOfBrawlerEnemies();
        totalMonstersToSpawn += spawnNumbers[s.Value.Faction];
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
      playerStartX = ((float)player.worldTransform.position.x);
      if(spawners == null || spawners.Count == 0 || debugSpawnsOnly)
      {
        return;
      }

      foreach (var spawner in spawners)
      {
        if (player.worldTransform.position.x >= spawner.xPosition)
        {
          spawners.RemoveAt(0);
          factionLookup.FindCharacterInfo(spawner.EnemyName, out var charInfo, out var fac);
          var monsterCScript = createMonster(charInfo, fac, spawner.xPosition, ((float)player.worldTransform.position.z));
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
      transitioning = false;
      levelBoundsStart = UFE.config.selectedStage.LeftBoundary.AsFloat();
      levelBoundsEnd = UFE.config.selectedStage.RightBoundary.AsFloat();

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

    private void recalcPlayerProgress()
    {
      // arbitrary amount to look ahead by
      float buffer = 4.0f;
      float tempProgress = ((float)player.worldTransform.position.x + (playerStartX - levelBoundsStart) + buffer) * totalMonstersToSpawn / (levelBoundsEnd - levelBoundsStart);

      if(tempProgress > currentPlayerProgress)
      {
        currentPlayerProgress = tempProgress;
      }
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

      int numLivingEnemies = calcLivingEnemies();

      recalcAverageSpawns();
      recalcPlayerProgress();

      float recheckDelay = 3.0f;

      // if it's time to spawn another monster
      if(avgEnemiesOnscreenAtOnce > numLivingEnemies )
      {
        recalcRemainingMonsters(out int totalMonstersRemaining);

        if (totalMonstersRemaining <= 0 && numLivingEnemies == 0 && spawners.Count == 0)
        {
            fireWon();
            return;
        }
        if (!debugSpawnsOnly && totalMonstersRemaining > 0 && currentPlayerProgress > monstersSpawned)
        {
          ++monstersSpawned;
          spawnRandomMonster(totalMonstersRemaining);
        }
        recheckDelay = (float)UnityEngine.Random.Range(3, 7);
      }

      UFE.DelaySynchronizedAction(checkSpawns, recheckDelay);

    }

    private int calcLivingEnemies()
    {
      int numLivingPlayers = 0;
      foreach(int i in playerIDs)
      {
        if(UFE.brawlerEntityManager.GetControlsScript(i).currentLifePoints >= 0)
        {
          ++numLivingPlayers;
        }
      }
      return UFE.brawlerEntityManager.GetNumLivingEntities() - numLivingPlayers;
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
      if(transitioning)
      {
        return;
      }
      transitioning = true;
      UFE.FireAlert("All Dudes BEATEN!", null);

      UFE.DelaySynchronizedAction(() =>
      {
        if (UFE.config.currentRound == 3)
        {
          UFE.FireGameEnds();
          UFE.EndGame();
          McCoy.GetInstance().LoadScene(McCoy.McCoyScenes.CityMap);
        }
        else
        {
          UFE.NextBrawlerStage();
          Initialize(spawnData, bossSpawnListener);
        }
      }, 6.0f);
    }

    private void fireLose()
    {
      if(transitioning)
      {
        return;
      }
      transitioning = true;

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

    private void spawnRandomMonster(int totalMonstersRemaining)
    {
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

    private ControlsScript createMonster(UFE3D.CharacterInfo info, Factions f, float? posX = null, float? posZ = null)
    {
      ControlsScript newMonster = UFE.CreateRandomMonster(info, posX, posZ);
      SetTeam(newMonster, f);
      SetAllies(newMonster, new List<Factions> { f });
      return newMonster;
    }
  }
}