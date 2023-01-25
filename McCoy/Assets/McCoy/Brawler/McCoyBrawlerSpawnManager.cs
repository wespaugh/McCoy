using Assets.McCoy.BoardGame;
using Assets.McCoy.Brawler.Stages;
using FPLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using UFE3D;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.Brawler
{
  using MobData = Dictionary<Factions, McCoyMobData>;
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

    // Faction -> initialEnemies
    Dictionary<Factions, int> initialSpawnNumbers = new Dictionary<Factions, int>();
    // Faction -> totalEnemiesRemaining
    Dictionary<Factions, int> spawnNumbers = new Dictionary<Factions, int>();
    // Faction -> avgEnemiesAtOnce
    Dictionary<Factions, int> avgSpawnNumbers = new Dictionary<Factions, int>();
    // Faction -> deadEnemies
    Dictionary<Factions, int> monstersKilled = new Dictionary<Factions, int>();

    // living bosses
    List<ControlsScript> livingBosses = new List<ControlsScript>();

    List<GameObject> debugGoalposts = new List<GameObject>();

    bool debugSpawnsOnly => McCoy.GetInstance().DebugSpawnsOnly;

    const float bossXOffset = 3.5f;

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
    // float currentPlayerProgress = 0; // measured in number of enemies they should have encountered
    float levelBoundsStart, levelBoundsEnd;
    ControlsScript player;
    bool allPlayersDead = false;
    bool transitioning = false;
    ControlsScript boss = null;
    // Killing this Ends the Game!
    ControlsScript finalBoss = null;
    IBossSpawnListener bossSpawnListener = null;
    McCoyFactionLookup factionLookup => McCoyFactionLookup.GetInstance();

    public bool lastStage 
    {
      get => UFE.config.currentRound == UFE.config.selectedStage.stageInfo.substages.Count; 
    }

    // spawners within a stage
    List<McCoySpawnData> spawners = null;
    // spawners based on mobs
    MobData mobData = null;
    List<McCoyCombatZoneData> combatZones = null;
    List<IMobChangeDelegate> mobChangeListeners = new List<IMobChangeDelegate>();

    bool allEnemiesDefeated = false;
    string bossName;
    bool waitingForQuestUI = false;
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
      updateRemainingCountToSpawn();
    }

    public void Initialize(Dictionary<Factions, McCoyMobData> mobData, IBossSpawnListener bossSpawnListener)
    {
      allPlayersDead = false;
      spawners = null;
      transitioning = false;
      this.mobData = mobData;
      // currentPlayerProgress = 0f;

      int currentRound = UFE.config.currentRound;
      int maxRounds = UFE.config.selectedStage.stageInfo.substages.Count;
      float percentage = 1.0f / (1 + maxRounds - currentRound);
      foreach(var spawnLookup in mobData)
      {
        int substageMonstersInFaction = (int)(spawnLookup.Value.MonstersInMob * percentage);
        spawnNumbers[spawnLookup.Key] = substageMonstersInFaction;
        initialSpawnNumbers[spawnLookup.Key] = substageMonstersInFaction;
        monstersKilled[spawnLookup.Key] = 0;
        avgSpawnNumbers[spawnLookup.Key] = mobData.ContainsKey(spawnLookup.Key) ? mobData[spawnLookup.Key].CalculateNumberSimultaneousBrawlerEnemies() : 0;
        Debug.Log($"Going to spawn {spawnNumbers[spawnLookup.Key]} {spawnLookup.Key}s into {UFE.config.selectedStage.stageInfo.substages[UFE.config.currentRound-1].substageName}");
      }
      livingBosses.Clear();

      SetTeam(1, Factions.Werewolves);
      SetAllies(1, new List<Factions> { Factions.Werewolves } );

      player = UFE.brawlerEntityManager.GetControlsScript(1);
      playerStartX = ((float)player.worldTransform.position.x);
      this.bossSpawnListener = bossSpawnListener;
      
      UFE.DelaySynchronizedAction(checkSpawns, 3.0f);
    }

    private void FixedUpdate()
    {
      updateSpawners();
      updateCombatZones();
      checkGameEnd();
    }

    private void initGoalposts()
    {

      while (debugGoalposts.Count > 0)
      {
        var first = debugGoalposts[0];
        debugGoalposts.RemoveAt(0);
        Destroy(first);
      }
      int totalSpawns = 0;
      foreach(var v in initialSpawnNumbers)
      {
        totalSpawns += v.Value;
      }

      /*
      for(int i = 0; i < totalSpawns; ++i)
      {
        var goal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        goal.transform.position = getSpawnPoint(i);
        debugGoalposts.Add(goal);
      }
      */

      for(int i = 0; i < 20; ++i)
      {
        var goal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        goal.transform.position = new Vector3(levelBoundsEnd, -4 + i * 1.5f, 0);
        goal.transform.rotation = Quaternion.Euler(0, 0, i * 10);
        goal.transform.localScale = new Vector3(.5f, .5f, .5f);
        debugGoalposts.Add(goal);
      }
    }
    private Vector3 getSpawnPoint(int index)
    {
      int totalSpawns = 0;
      foreach (var v in initialSpawnNumbers)
      {
        totalSpawns += v.Value;
      }
      float period = (levelBoundsEnd - levelBoundsStart) / totalSpawns;
      var retVal = new Vector3(levelBoundsStart + (index * period), 0, 0);
      return retVal;
    }
    private void checkGameEnd()
    {
      if(finalBoss != null)
      {
        McCoyGameState.Instance().FinalBossHealth = Mathf.Max(0, (float) finalBoss.currentLifePoints);
      }
      if(McCoy.GetInstance().debugCheatWin)
      {
        cheatWin();
        return;
      }
      if(inCombatZone)
      {
        return;
      }
      if(livingBosses.Count > 0)
      {
        return;
      }
      foreach(var spawn in spawners)
      {
        return;
      }
      if (player.worldTransform.position.x >= UFE.config.selectedStage.GetLevelExit())
      {
        if(endCondition == SubstageExitCondition.None)
        {
          endCondition = SubstageExitCondition.Escaped;
        }
        if (McCoy.GetInstance().gameState.activeQuest != null && lastStage)
        {
          // active quests must be finished before allowing transition out
          if (!allEnemiesDefeated || transitioning || waitingForQuestUI) return;
          StartCoroutine(waitForQuestEndUI());
        }
        else
        {
          Debug.Log("FIRE WON! " + endCondition);
          fireWon(endCondition);
        }
      }
    }

    private IEnumerator waitForQuestEndUI()
    {
      // on the last stage, show the quest end UI
      if (lastStage)
      {
        UFE.FireAlert(QUEST_COMPLETE, null);
        waitingForQuestUI = true;
        McCoyGameState state = McCoy.GetInstance().gameState;
        while (state.activeQuest != null)
        {
          yield return null;
        }
        waitingForQuestUI = false;
      }
      fireWon(endCondition);
    }

    public void ActorKilled(ControlsScript monster)
    {
      int XP = McCoyFactionLookup.GetInstance().XPForMonster(monster.myInfo.characterName);
      Factions team = (Factions)monster.Team;
      if(team == Factions.Werewolves)
      {
        return;
      }

      if(livingBosses.Contains(monster))
      {
        livingBosses.Remove(monster);
        bossName = monster.myInfo.name;
        boss = null;
        bossSpawnListener.BossDied(boss);
        endCondition = SubstageExitCondition.BossDefeated;
      }

      mobData[(Factions)monster.Team].MonstersKilled(1);
      ++monstersKilled[(Factions)monster.Team];
      foreach(var pc in PlayerCharacters)
      {
        var player = McCoyGameState.GetPlayer(pc);
        if (player.Player == McCoy.GetInstance().gameState.SelectedPlayer || McCoy.GetInstance().levelAllPlayersEvenly)
        {
          player.GainXP(XP);
        }
      }
    }

    private void cheatWin()
    {
      UFE.config.currentRound = UFE.config.selectedStage.stageInfo.substages.Count;
      foreach(var spawnDatum in mobData)
      {
        spawnDatum.Value.MonstersKilled(1000000);
      }
      McCoy.GetInstance().debugCheatWin = false;
      fireWon(SubstageExitCondition.Cheat);
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
      int totalMonstersInSubstage = 0;
      foreach(var spawn in initialSpawnNumbers)
      {
        totalMonstersInSubstage += spawn.Value;
      }
      combatZoneEnemiesRemaining = Math.Min((int)(combatZone.EnemyPercentage * totalMonstersInSubstage), updateRemainingCountToSpawn());
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
            livingBosses.Add(monsterCScript);
            boss = monsterCScript;
            bossSpawnListener.BossSpawned(monsterCScript);
            if(boss.myInfo.characterName == "PentaGran")
            {
              Debug.Log("!!!!!!!!!!!!!!!!!SUMMON THE PENTAGRAN: ");
              finalBoss = boss;
              float serializedHealth = McCoyGameState.Instance().FinalBossHealth;
              if (serializedHealth > 0)
              {
                finalBoss.currentLifePoints = Mathf.Min(serializedHealth, (float) finalBoss.currentLifePoints);
              }
            }
          }
          break;
        }
      }
    }

    private void stageBegan()
    {
      Debug.Log("Stage Began!"); // g'morning, wes. pentagran's spawn down below probably isn't called because this is only called on the first stage
      endCondition = SubstageExitCondition.None;
      allEnemiesDefeated = false;
      transitioning = false;
      levelBoundsStart = UFE.config.selectedStage.LeftBoundary.AsFloat();
      levelBoundsEnd = UFE.config.selectedStage.RightBoundary.AsFloat();
      initGoalposts();
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

      if(lastStage && McCoyGameState.Instance().FinalBattle)
      {
        Debug.Log("NOW! THIS IS IT! NOW'S THE TIME TO CHOOSE!");
        McCoySpawnData bossSpawn = new McCoySpawnData()
        {
          EnemyName = "PentaGran",
          IsBoss = true,
        };
        bossSpawn.Initialize((int)UFE.config.selectedStage.GetLevelExit() - 4);
        spawners.Add(bossSpawn);
      }
    }

    private void recalcAverageSpawns()
    {
      avgEnemiesOnscreenAtOnce = 0;
      foreach (var factionLookup in avgSpawnNumbers)
      {
        avgEnemiesOnscreenAtOnce += factionLookup.Value;
      }
      avgEnemiesOnscreenAtOnce /= avgSpawnNumbers.Count;
    }

    private void recalcPlayerProgress()
    {
      /*
      // arbitrary amount to look ahead by
      float buffer = 4.0f;
      float tempProgress = ((float)player.worldTransform.position.x - playerStartX + buffer)/ (levelBoundsEnd - levelBoundsStart);
      playerX = (float)player.worldTransform.position.x;
      playerY = (float)player.worldTransform.position.y;
      playerZ = (float)player.worldTransform.position.z;
      if(tempProgress > currentPlayerProgress)
      {
        currentPlayerProgress = tempProgress;
      }
      */
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
      // initGoalposts();

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
        int totalMonstersRemaining = updateRemainingCountToSpawn();

        bool exitedCombatZone = combatZoneEnemiesRemaining <= 0 && numLivingEnemies == 0 && inCombatZone;
        if (exitedCombatZone)
        {
          ExitCombatZone();
        }

        if (totalMonstersRemaining <= 0 && numLivingEnemies == 0 && spawners.Count == 0)
        {
          if(inCombatZone)
          {
            Debug.LogError(combatZoneEnemiesRemaining);
          }
          allEnemiesDefeated = true;
          return;
        }

        int currentSpawns = 0;
        foreach(var i in initialSpawnNumbers)
        {
          currentSpawns += initialSpawnNumbers[i.Key] - spawnNumbers[i.Key];
        }
        Vector3 nextSpawnPoint = getSpawnPoint(currentSpawns);
        bool crossedSpawnThreshold = player.transform.position.x >= nextSpawnPoint.x;
        if (!debugSpawnsOnly)
        {
          if (inCombatZone || totalMonstersRemaining > 0 && crossedSpawnThreshold)
          {
            spawnRandomMonster();
          }
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

    private int updateRemainingCountToSpawn()
    {
      Dictionary<McCoyMobData, int> factionHealthLookup = new Dictionary<McCoyMobData, int>();

      foreach(var s in initialSpawnNumbers)
      {
        factionHealthLookup[mobData[s.Key]] = s.Value - monstersKilled[s.Key];
      }

      foreach (var d in mobChangeListeners)
      {
        d.MobsChanged(factionHealthLookup);
      }

      return recalcRemainingMonsters();
    }

    private int recalcRemainingMonsters()
    {
      int totalMonstersRemaining = 0;
      // get total remaining monsters from each mob
      foreach (var m in spawnNumbers)
      {
        totalMonstersRemaining += m.Value;
      }
      return totalMonstersRemaining;
    }

    private void fireWon(SubstageExitCondition endCondition)
    {
      if(transitioning)
      {
        return;
      }
      transitioning = true;

      string message = STINGER_STAGE_CLEARED;
      switch(endCondition)
      {
        case SubstageExitCondition.BossDefeated:
          message = STINGER_BOSS_DEFEATED;
          break;
        case SubstageExitCondition.Escaped:
          message = STINGER_STAGE_ESCAPED;
          break;
        case SubstageExitCondition.Cheat:
          message = $"Levelskip";
          break;
      }

      UFE.FireAlert(message, null);

      float transitionTime = 4.0f; // total time before scene switches
      if (!lastStage)
      {
        float fadeTime = .40f; // fade out time
        float fadeDelay = 1.0f; // time to wait before fading out

        // wait until faded out, then switch substage
        UFE.DelaySynchronizedAction(() =>
        {
          loadNextBrawlerStage();
        }, fadeTime + fadeDelay);

        float buffer = .5f; // time to wait after scene switch
        // wait until faded out and a little more, then fade back in
        UFE.DelaySynchronizedAction(() =>
        {
          WaitAndFadeIn(fadeTime, transitionTime - fadeTime - fadeDelay + buffer);
        }, fadeDelay);
      }
      else
      {
        UFE.DelaySynchronizedAction(() =>
        {
          loadCity();
        }, transitionTime);
      }
    }

    private void loadCity()
    {
      McCoy.GetInstance().BuffManager.ClearAllPlayers();
      foreach (var s in mobData)
      {
        s.Value.StageEnded();
      }
      UFE.FireGameEnds();
      UFE.EndGame();
      McCoy.GetInstance().LoadScene(McCoy.McCoyScenes.CityMap);
    }

    private void loadNextBrawlerStage()
    {
      // also kill any remaining enemies
      foreach (var control in UFE.brawlerEntityManager.GetAllControllers())
      {
        if (control != null && control.isCPU)
        {
          var cScript = UFE.brawlerEntityManager.GetControlsScript(control.player);
          if (!cScript.isDead && !cScript.IsDespawning)
          {
            cScript.ForceKill = true;
          }
        }
      }

      UFE.NextBrawlerStage();
    }

    private void WaitAndFadeIn(float fadeTime, float time)
    {
      CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, false, fadeTime);
      UFE.DelaySynchronizedAction(() =>
      {
        CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, time);
        UFE.DelaySynchronizedAction(() => 
        {
          Debug.Log("New Stage!");
          Initialize(mobData, bossSpawnListener);
        }, time);
      }, time);
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

    private void spawnRandomMonster()
    {
      int totalWeightedMonsters = 0;
      foreach(var s in spawnNumbers)
      {
        totalWeightedMonsters += s.Value;
      }

      int randomMonsterIndex = UnityEngine.Random.Range(0, totalWeightedMonsters);
      int searchIndex = 0;
      foreach (var m in spawnNumbers)
      {
        searchIndex += m.Value;
        if (searchIndex >= randomMonsterIndex && !debugSpawnsOnly)
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
      if (inCombatZone)
      {
        --combatZoneEnemiesRemaining;
      }
      ControlsScript newMonster = UFE.CreateRandomMonster(info, posX, posZ);
      if(McCoy.GetInstance().DebugOneHitKills)
      {
        newMonster.currentLifePoints = 1;
      }
      SetTeam(newMonster, f);
      SetAllies(newMonster, new List<Factions> { f });
      return newMonster;
    }
  }
}