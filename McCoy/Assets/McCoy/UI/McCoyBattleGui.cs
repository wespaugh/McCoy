using Assets.McCoy.Brawler;
using Assets.McCoy.RPG;
using Assets.McCoy;
using TMPro;
using UFE3D;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using static Assets.McCoy.ProjectConstants;
using Assets.McCoy.BoardGame;

namespace Assets.McCoy.UI
{
  public class McCoyBattleGui : DefaultBattleGUI, IBossSpawnListener
  {
    [SerializeField]
    public McCoyWorldUI worldSpacePrefab = null;

    GameObject uiRoot = null;

    McCoyWorldUI worldUI = null;
    [SerializeField]
    LiquidBar healthBar = null;
    [SerializeField]
    LiquidBar xpBar = null;

    [SerializeField] TMP_Text alertText = null;

    [SerializeField] TMP_Text debuggerText = null;

    [SerializeField] Button winButton = null;
    [SerializeField] Button spawnButton = null;
    [SerializeField] TMP_InputField xInput = null;
    [SerializeField] TMP_InputField yInput = null;
    [SerializeField] TMP_InputField nameInput = null;
    [SerializeField] Toggle endPlayerTimer = null;

    [SerializeField] TMP_Text tmpNameText = null;
    [SerializeField] TMP_Text enemyName = null;
    [SerializeField] TMP_Text bossName = null;

    [SerializeField] int playerSortOrder;
    [SerializeField] McCoyBrawlerMobStatusLabel mobStatusLabel = null;

    [SerializeField] McCoyQuestTextUI questUI = null;
    [SerializeField] TMP_Text playerTimer = null;

    McCoyStageData currentStage;

    bool debug => McCoy.GetInstance().Debug;

    bool spawnerInitialized = false;
    private McCoyBrawlerSpawnManager spawner;
    private int xpCache;
    private bool questCompleted;
    private bool levelBegan = false;
    private float elapsedTime;
    private PlayerCharacter selectedPlayer;
    string timeRemainingString = null;

    protected override void OnGameBegin(ControlsScript player1, ControlsScript player2, StageOptions stage)
    {
      base.OnGameBegin(player1, player2, stage);

      UFE.canvas.renderMode = RenderMode.ScreenSpaceCamera;
      UFE.canvas.worldCamera = Camera.main;

      levelBegan = true;

      tmpNameText.text = player1.myInfo.characterName;

      var pc = McCoyGameState.GetPlayer(McCoyGameState.Instance().SelectedPlayer);
      selectedPlayer =  pc.Player;
      McCoySkillUnlockManager.PlayerSpawned(player1, pc.Skills);

      updateXP(true);

      if (!spawnerInitialized)
      {
        spawnerInitialized = true;
        initSpawner();
      }
      // worldUI.StageBegan();

      McCoyQuestData activeQuest = McCoy.GetInstance().gameState.activeQuest;
      if ( activeQuest != null)
      {
        startCutscene(activeQuest.introCutscene);
        /*
        questUI.gameObject.SetActive(true);
        questUI.BeginQuest(activeQuest);
        */
      }
    }

    protected override void OnGameEnd(ControlsScript winner, ControlsScript loser)
    {
      McCoy.GetInstance().gameState.UpdatePlayerTimeRemaining(selectedPlayer, elapsedTime);
      //worldUI.StageEnded();
    }

    private void Awake()
    {
      Camera.main.orthographic = true;
      // fixes flickering issue
      transform.localPosition = new Vector3(0f, 0f, -10f);
      toggleDebug();

      Localize("com.mccoy.boardgame.timeRemaining", (s) => timeRemainingString = s);

      if (spawnButton != null)
      {
        spawnButton.onClick.AddListener(() =>
        {
          float x = string.IsNullOrEmpty(xInput.text) ? 0.0f : float.Parse(xInput.text);
          float z = string.IsNullOrEmpty(yInput.text) ? 0.0f : float.Parse(yInput.text);
          UFE3D.CharacterInfo m = null;
          foreach(var name in UFE.config.characters)
          {
            if(name.characterName.ToLower() == nameInput.text.ToLower())
            {
              m = name;
              break;
            }
          }
          UFE.CreateRandomMonster(m, x, z, debug: true);
        });
      }
      var camera2 = GameObject.Find("BattleUI Camera");
      if(camera2.transform.childCount > 0)
      {
        uiRoot = camera2.transform.GetChild(0).gameObject;
      }
      else
      {
        uiRoot = new GameObject("UI Root");
        uiRoot.transform.localPosition = new Vector3(18.0f, 33.0f, 4.0f);
        // uiRoot.transform.position = camera2.transform.position;
      }

      /*
      worldUI = Instantiate(worldSpacePrefab, uiRoot.transform);
      worldUI.transform.localPosition = new Vector3(-.65f, .26f, -8.75f);// + uiRoot.gameObject.transform.position;
      */

      currentStage = McCoy.GetInstance().currentStage;
    }

    private void toggleDebug()
    {
      winButton.gameObject.SetActive(debug);
      spawnButton.gameObject.SetActive(debug);
      xInput.gameObject.SetActive(debug);
      yInput.gameObject.SetActive(debug);
      nameInput.gameObject.SetActive(debug);
      endPlayerTimer.gameObject.SetActive(debug);
    }

    private void initSpawner()
    {
      spawner = gameObject.AddComponent<McCoyBrawlerSpawnManager>();
      spawner.Initialize(currentStage.GetSpawnData(), this);
      spawner.AddMobSpawnListener(mobStatusLabel);
      info.text = currentStage.Name;
    }

    private void OnDestroy()
    {
      Destroy(uiRoot);
    }

    protected override void SetAlertMessage(string msg)
    {
      if(msg == ProjectConstants.STINGER_STAGE_ESCAPED)
      {
        //worldUI.StageEnded(McCoyStinger.StingerTypes.Escaped);
      }
      else if(msg == ProjectConstants.STINGER_BOSS_DEFEATED)
      {
        //worldUI.StageEnded(McCoyStinger.StingerTypes.BossDefeated);
      }
      else if(msg == ProjectConstants.STINGER_STAGE_CLEARED)
      {
        //worldUI.StageEnded(McCoyStinger.StingerTypes.StageCleared);
      }
      else if(msg == ProjectConstants.QUEST_COMPLETE)
      {
        startCutscene(McCoy.GetInstance().gameState.activeQuest.exitText, true);
        // questUI.QuestEnded();
      }
      else if(msg == ProjectConstants.CUTSCENE_FINISHED)
      {
        UFE.PauseGame(false);
        if (!questCompleted)
        {
          //worldUI.gameObject.SetActive(true);
          uiRoot.gameObject.SetActive(true);
        }
        else
        {
          McCoy.GetInstance().gameState.CompleteQuest();
        }
      }
      return;
      //alertText.text = msg;
      //UFE.DelaySynchronizedAction(hideAlertText, 2.0f);
    }

    private void startCutscene(string name, bool questComplete = false)
    {
      StartCoroutine(yieldThenPause());
      var _ = McCoy.GetInstance().ShowCutsceneAsync(name);
      //worldUI.gameObject.SetActive(false);
      uiRoot.gameObject.SetActive(false);
      questCompleted = questComplete;
    }
    IEnumerator yieldThenPause()
    {
      float start = Time.time;
      while (Time.time < start + 3.0f)
      {
        yield return null;
      }
      UFE.timeScale = 0f;
    }

    private void hideAlertText()
    {
      SetAlertMessage("");
    }

    protected override void OnNewAlert(string msg, ControlsScript player)
    {
      if (player == null)
      {
        SetAlertMessage(msg);
      }
      else if(msg == "Actor Killed")
      {
        McCoy.GetInstance().BuffManager.ClearPlayer(player.playerNum);
        var gameState = McCoy.GetInstance().gameState;
        var selectedPlayer = gameState.playerCharacters[gameState.SelectedPlayer];
        int skillPointsBefore = selectedPlayer.AvailableSkillPoints;
        spawner.ActorKilled(player);
        // if #skillpoints changed, we leveled up. if we leveled up, #skillpoints changed
        updateXP(skillPointsBefore != selectedPlayer.AvailableSkillPoints);
      }
    }

    private void updateXP(bool initialize)
    {
      var gameState = McCoy.GetInstance().gameState;
      int XP = gameState.GetPlayerCharacter(gameState.SelectedPlayer).XP;
      var selectedPlayer = gameState.GetPlayerCharacter(gameState.SelectedPlayer);
      int xpFloor = 0;
      int xpCeiling = 0;
      for (int i = 0; i < selectedPlayer.XpThreshholds.Length; ++i)
      {
        if (selectedPlayer.XpThreshholds[i] > XP)
        {
          if (i > 0)
          {
            xpFloor = selectedPlayer.XpThreshholds[i - 1];
          }
          xpCeiling = selectedPlayer.XpThreshholds[i];
          break;
        }
      }
      int diff = xpCeiling - xpFloor;
      xpCache = XP - xpFloor;
      xpBar.targetFillAmount =(float) xpCache / (float)diff;
      //worldUI.UpdatePlayerXP(xpCache, initialize, diff);
    }

    protected override void UpdatePlayerHealthBar(float percent)
    {
      healthBar.targetFillAmount = percent;
      //worldUI.UpdatePlayerHealth(percent);
      //worldUI.UpdatePlayerXP(xpCache, false);
    }

    protected override void OnLifePointsChange(float newFloat, ControlsScript player)
    {
      if(UFE.GetController(player.playerNum).isCPU)
      {
        enemyName.text = player.myInfo.characterName;
        enemyName.gameObject.SetActive(true);
        //worldUI.updateEnemyHealth(player, enemyUIHidden);
      }
    }

    private void enemyUIHidden()
    {
      enemyName.gameObject.SetActive(false);
    }

    protected override void UpdateWonRounds()
    {
    }

    private void FixedUpdate()
    {
      if (UFE.GetPlayer1ControlsScript() != null && UFE.GetPlayer1ControlsScript().mySpriteRenderer != null)
      {
        playerSortOrder = UFE.GetPlayer1ControlsScript().mySpriteRenderer.sortingOrder;
      }

      if(levelBegan)
      {
        elapsedTime += (float)(UFE.timeScale * Time.deltaTime);
        int timeRemaining = (int)(McCoyGameState.Instance().TurnTimeRemaining(selectedPlayer) - elapsedTime);
        timeRemaining = Mathf.Max(0,timeRemaining);
        playerTimer.text = timeRemainingString + timeRemaining;
      }

      if (UFE.config.debugOptions.debugMode)
      {
        string sb = "";
        foreach(var controlScript in UFE.brawlerEntityManager.GetAllControlsScripts())
        {
          if(controlScript.Value != null && controlScript.Value.debugOn)
          {
            sb += $"{controlScript.Value.debuggerText}\n\n";
          }
        }
        debuggerText.text = sb;
      }
    }

    protected override void OnButtonPress(ButtonPress buttonPress, ControlsScript player)
    {
      base.OnButtonPress(buttonPress, player);
      if (UFE.isPaused())
      {
        Debug.Log($"OnButtonPress: {buttonPress}, {player}");
      }
    }

    public void BossSpawned(ControlsScript boss)
    {
      bossName.gameObject.SetActive(true);
      bossName.text = boss.myInfo.characterName;
      //worldUI.BossSpawned(boss);
    }

    public void BossDied(ControlsScript boss)
    {
      bossName.gameObject.SetActive(false);
      //worldUI.BossDied(boss);
    }

    public void CheatWin()
    {
      McCoy.GetInstance().debugCheatWin = true;
      if(endPlayerTimer.isOn)
      {
        McCoyGameState.Instance().UpdatePlayerTimeRemaining(selectedPlayer, 1000000);
      }
      if (McCoy.GetInstance().gameState.activeQuest != null)
      {
        McCoy.GetInstance().gameState.CompleteQuest();
      }
    }
  }
}