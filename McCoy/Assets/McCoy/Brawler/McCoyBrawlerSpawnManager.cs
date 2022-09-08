using Assets.McCoy.BoardGame;
using Assets.McCoy.Brawler.Stages;
using FPLibrary;
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
    enum SubstageExitCondition
    {
      None,
      Escaped,
      BossDefeated,
      AllEnemiesDefeated,
      Cheat
    }
    int[] playerIDs = { 1 };

    // Faction -> enemies required total
    Dictionary<Factions, int> initialSpawnNumbers = new Dictionary<Factions, int>();
    // Faction -> totalEnemiesRemaining
    Dictionary<Factions, int> spawnNumbers = new Dictionary<Factions, int>();
    // Faction -> avgEnemiesAtOnce
    Dictionary<Factions, int> avgSpawnNumbers = new Dictionary<Factions, int>();

    bool debugSpawnsOnly => McCoy.GetInstance().Debug;

    const float bossXOffset = 3.5f;

    int monstersSpawned = 0;
    int totalMonstersToSpawn = 0;
    int avgEnemiesOnscreenAtOnce = 0;
    bool inCombatZone = false;
    int combatZoneEnemiesRemaining = 0;
    float playerStartX = 0;
    [SerializeField]
    float playerX = 0.0f;
    [SerializeField]
    float playerY = 0.0f;
    [SerializeField]
    float playerZ = 0.0f;
    float currentPlayerProgress = 0; // measured in number of enemies they should have encountered
    float levelBoundsStart, levelBoundsEnd;
    ControlsScript player;
    bool allPlayersDead = false;
    bool transitioning = false;
    ControlsScript boss = null;
    IBossSpawnListener bossSpawnListener = null;
    McCoyFactionLookup factionLookup => McCoyFactionLookup.GetInstance();
    // spawners within a stage
    List<McCoySpawnData> spawners = null;
    // spawners based on mobs
    SpawnData spawnData = null;
    List<McCoyCombatZoneData> combatZones = null;
    List<IMobChangeDelegate> mobChangeListeners = new List<IMobChangeDelegate>();

    bool allEnemiesDefeated = false;
    bool bossRemains = false;
    string bossName;
    SubstageExitCondition endCondition;

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

    public void AddMobSpawnListener(IMobChangeDelegate d)
    {
      mobChangeListeners.Add(d);
      updateRemainingMonsters();
    }

    public void Initialize(SpawnData spawns, IBossSpawnListener bossSpawnListener)
    {
      spawnData = spawns;
      allPlayersDead = false;
      spawners = null;

      foreach(var s in spawns)
      {
        int enemiesInSubstage = s.Value.StageBegan();
        spawnNumbers[s.Value.Faction] = enemiesInSubstage;
        Debug.Log($"Going to spawn {spawnNumbers[s.Value.Faction]} {s.Value.Faction}s into {UFE.config.selectedStage.stageInfo.substages[UFE.config.currentRound-1].substageName}");
        initialSpawnNumbers[s.Value.Faction] = spawnNumbers[s.Value.Faction];
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
      updateCombatZones();
      updateBoss();
      checkGameEnd();
    }

    private void checkGameEnd()
    {
      if(McCoy.GetInstance().debugCheatWin)
      {
        cheatWin();
        return;
      }
      if (! bossRemains && (allEnemiesDefeated || player.worldTransform.position.x >= UFE.config.selectedStage.GetLevelExit()))
      {
        if(endCondition == SubstageExitCondition.None)
        {
          endCondition = allEnemiesDefeated ? SubstageExitCondition.AllEnemiesDefeated : SubstageExitCondition.Escaped;
        }
        fireWon(endCondition);
      }
    }

    private void cheatWin()
    {
      // call all the things killed
      List<Factions> allKeys = new List<Factions>(spawnNumbers.Keys);
      foreach(var spawnNumber in allKeys)
      {
        spawnNumbers[spawnNumber] = 0;
      }
      McCoy.GetInstance().debugCheatWin = false;
      fireWon(SubstageExitCondition.Cheat);
    }

    private void updateBoss()
    {
      if(boss && boss.currentLifePoints <= 0)
      {
        bossRemains = false;
        bossName = boss.myInfo.name;
        boss = null;
        bossSpawnListener.BossDied(boss);
        endCondition = SubstageExitCondition.BossDefeated;
      }
    }

    private void updateCombatZones()
    {
      if(combatZones == null)
      {
        return;
      }
      foreach(var combatZone in combatZones)
      {
        if(player.worldTransform.position.x >= combatZone.XPosition && ! debugSpawnsOnly)
        {
          EnterCombatZone(combatZone);
          combatZones.RemoveAt(0);
          break;
        }
      }
    }

    private void EnterCombatZone(McCoyCombatZoneData combatZone)
    {
      inCombatZone = true;
      UFE.config.selectedStage.SetTemporaryBoundaries(combatZone.XPosition - 10.0f, combatZone.XPosition + 6.0f);
      combatZoneEnemiesRemaining = Math.Min((int)(combatZone.EnemyPercentage * totalMonstersToSpawn), updateRemainingMonsters());
      currentPlayerProgress += combatZoneEnemiesRemaining;
      UFE.cameraScript.FreezeForCombatZone();
    }

    private void ExitCombatZone()
    {
      Debug.Log("GO! GO! GO!");
      inCombatZone = false;
      UFE.config.selectedStage.UnsetTemporaryBoundaries();
      UFE.cameraScript.ReleaseFromCombatZone();
    }

    private void updateSpawners()
    {
      // unfortunately we cannot do this during initialize.
      // somehow the prefab is destroyed and by the time we get here the list is full of null refs
      if (spawners == null)
      {
        spawners = new List<McCoySpawnData>();
        UFE.DelaySynchronizedAction(stageBegan, 0.5f);
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
          float x = spawner.xPosition + bossXOffset;
          UFE.config.selectedStage.stageInfo.GetYBounds(UFE.config.currentRound, x, out var min, out var max);
          var monsterCScript = createMonster(charInfo, fac, x, ((float) (min + max) / 2.0f));
          if (bossSpawnListener != null && spawner.IsBoss)
          {
            boss = monsterCScript;
            bossSpawnListener.BossSpawned(monsterCScript);
          }
          break;
        }
      }
    }

    private void stageBegan()
    {
      endCondition = SubstageExitCondition.None;
      allEnemiesDefeated = false;
      currentPlayerProgress = 0;
      monstersSpawned = 0;
      transitioning = false;
      levelBoundsStart = UFE.config.selectedStage.LeftBoundary.AsFloat();
      levelBoundsEnd = UFE.config.selectedStage.RightBoundary.AsFloat();

      spawners.Clear();

      GameObject spawnerRoot = GameObject.FindGameObjectWithTag("Spawner");
      if(spawnerRoot == null)
      {
        return;
      }
      var spawnerList = new List<McCoySpawnerTrigger>(spawnerRoot.GetComponentsInChildren<McCoySpawnerTrigger>());
      spawnerList.Sort((a, b) => { return (int)(a.transform.position.x < b.transform.position.x ? -1 : 1); });

      while(spawnerList.Count > 0)
      {
        var spawner = spawnerList[0];
        spawnerList.Remove(spawner);
        if (spawner != null)
        {
          if (spawner.spawnData.IsBoss)
          {
            bossRemains = true;
          }
          spawner.spawnData.Initialize(spawner.gameObject.transform.localPosition.x);
          spawners.Add(spawner.spawnData);
          Destroy(spawner.gameObject);
        }
      }

      combatZones = new List<McCoyCombatZoneData>();
      var combatZoneList = new List<McCoyCombatZoneTrigger>(spawnerRoot.GetComponentsInChildren<McCoyCombatZoneTrigger>());
      combatZoneList.Sort((a, b) => { return (int)(a.transform.position.x < b.transform.position.x ? -1 : 1); });
      while(combatZoneList.Count > 0)
      {
        var combatZone = combatZoneList[0];
        combatZoneList.Remove(combatZone);
        combatZone.ZoneData.Initialize(combatZone.gameObject.transform.localPosition.x);
        combatZones.Add(combatZone.ZoneData);
        Destroy(combatZone.gameObject);
      }
      Destroy(spawnerRoot);
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
      playerX = (float)player.worldTransform.position.x;
      playerY = (float)player.worldTransform.position.y;
      playerZ = (float)player.worldTransform.position.z;
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

      float recheckDelay = 1.0f;

      // if it's time to spawn another monster
      if(avgEnemiesOnscreenAtOnce > numLivingEnemies )
      {
        int totalMonstersRemaining = updateRemainingMonsters(true);

        bool exitedCombatZone = combatZoneEnemiesRemaining <= 0 && numLivingEnemies == 0 && inCombatZone;
        if (exitedCombatZone)
        {
          ExitCombatZone();
        }

        if (totalMonstersRemaining <= 0 && numLivingEnemies == 0 && spawners.Count == 0)
        {
          if(inCombatZone)
          {
            Debug.LogError("(but we're in a combat zone)");
            Debug.LogError(combatZoneEnemiesRemaining);
          }
          allEnemiesDefeated = true;
          return;
        }
        if (!debugSpawnsOnly && totalMonstersRemaining > 0 && currentPlayerProgress > monstersSpawned)
        {
          ++monstersSpawned;
          if(inCombatZone)
          {
            --combatZoneEnemiesRemaining;
          }
          spawnRandomMonster(totalMonstersRemaining);
        }
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

    private int updateRemainingMonsters(bool updateDelegates = false)
    {
      Dictionary<Factions, float> factionHealthLookup = updateDelegates ? new Dictionary<Factions, float>() : null;

      if (factionHealthLookup != null)
      {
        foreach (var d in this.mobChangeListeners)
        {
          d.MobsChanged(factionHealthLookup);
        }
      }

      return recalcRemainingMonsters(factionHealthLookup);
    }

    private int recalcRemainingMonsters(Dictionary<Factions, float> factionHealthLookup)
    {
      int totalMonstersRemaining = 0;
      // get total remaining monsters from each mob
      foreach (var m in spawnNumbers)
      {
        totalMonstersRemaining += m.Value;
        if(factionHealthLookup != null)
        {
          factionHealthLookup[m.Key] = ((float)m.Value) / ((float)initialSpawnNumbers[m.Key]);
        }
      }
      return totalMonstersRemaining;
    }

    private void commitStageResultsToMobs()
    {
      var monstersKilled = new Dictionary<Factions, int>();
      // get total remaining monsters from each mob
      foreach (var m in spawnNumbers)
      {
        monstersKilled[m.Key] = initialSpawnNumbers[m.Key] - m.Value;
      }

      foreach (var spawn in spawnData)
      {
        // divide by substages again here because the percentage of enemies killed on this substage is only 1/3 of the total enemies killed for the stage
        spawn.Value.MonstersKilled(monstersKilled[spawn.Key]);
      }
    }

    private void fireWon(SubstageExitCondition endCondition)
    {
      if(transitioning)
      {
        return;
      }
      transitioning = true;

      commitStageResultsToMobs();

      string message = "All Dudes Beaten!";
      switch(endCondition)
      {
        case SubstageExitCondition.BossDefeated:
          message = $"Defeated {bossName}!";
          break;
        case SubstageExitCondition.Escaped:
          message = $"Howl Another Day!";
          break;
        case SubstageExitCondition.Cheat:
          message = $"Levelskip";
          break;
      }

      UFE.FireAlert(message, null);

      float transitionTime = 4.0f; // total time before scene switches
      if (UFE.config.currentRound != 3)
      {
        float fadeTime = .40f; // fade out time
        float fadeDelay = 2.0f; // time to wait before fading out

        // wait until faded out, then switch substage
        UFE.DelaySynchronizedAction(() =>
        {
          // also kill any remaining enemies
          foreach (var control in UFE.brawlerEntityManager.GetAllControllers())
          {
            if(control != null && control.isCPU)
            {
              var cScript = UFE.brawlerEntityManager.GetControlsScript(control.player);
              if(!cScript.isDead)
              {
                cScript.Physics.isFatallyFalling = true;
              }
            }
          }

          UFE.NextBrawlerStage();
          Initialize(spawnData, bossSpawnListener);
          player.worldTransform.position = FPVector.zero;
          UFE.cameraScript.MoveCameraToLocation(player.worldTransform.position.ToVector(), Vector3.zero, UFE.cameraScript.targetFieldOfView, 1000, player.gameObject);
          UFE.DelaySynchronizedAction(() => UFE.cameraScript.ReleaseCam(), .5f);
        }, fadeTime + fadeDelay);

        float buffer = .5f; // time to wait after scene switch
        // wait until faded out and a little more, then fade back in
        UFE.DelaySynchronizedAction(() =>
        {
          CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, false, fadeTime);
          WaitAndFadeIn(transitionTime - fadeTime - fadeDelay + buffer);
        }, fadeDelay);
      }
      else
      {
        foreach (var s in spawnData)
        {
          s.Value.StageEnded();
        }
        UFE.DelaySynchronizedAction(() =>
        {
          UFE.FireGameEnds();
          UFE.EndGame();
          McCoy.GetInstance().LoadScene(McCoy.McCoyScenes.CityMap);
        }, transitionTime);
      }
    }

    private void WaitAndFadeIn(float time)
    {
      UFE.DelaySynchronizedAction(() =>
      {
        CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, time);
      }, time);
    }

    private void fireLose()
    {
      if(transitioning)
      {
        return;
      }
      transitioning = true;

      commitStageResultsToMobs();

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