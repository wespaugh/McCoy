using Assets.McCoy.Brawler;
using Assets.McCoy.RPG;
using System;
using TMPro;
using UFE3D;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.McCoy.UI
{
  public class McCoyBattleGui : DefaultBattleGUI, IBossSpawnListener
  {
    [SerializeField]
    public McCoyWorldUI worldSpacePrefab = null;

    GameObject uiRoot = null;

    McCoyWorldUI worldUI = null;

    [SerializeField] TMP_Text alertText = null;

    [SerializeField] TMP_Text debuggerText = null;

    [SerializeField] Button winButton = null;
    [SerializeField] Button spawnButton = null;
    [SerializeField] TMP_InputField xInput = null;
    [SerializeField] TMP_InputField yInput = null;
    [SerializeField] TMP_InputField nameInput = null;

    [SerializeField] TMP_Text tmpNameText = null;
    [SerializeField] TMP_Text enemyName = null;
    [SerializeField] TMP_Text bossName = null;

    [SerializeField] int playerSortOrder;
    [SerializeField] McCoyBrawlerMobStatusLabel mobStatusLabel = null;

    McCoyStageData currentStage;

    bool debug => McCoy.GetInstance().Debug;

    bool spawnerInitialized = false;
    private McCoyBrawlerSpawnManager spawner;
    private int xpCache;

    protected override void OnGameBegin(ControlsScript player1, ControlsScript player2, StageOptions stage)
    {
      base.OnGameBegin(player1, player2, stage);

      tmpNameText.text = player1.myInfo.characterName;

      McCoyPlayerCharacter rex =  McCoy.GetInstance().gameState.playerCharacters[McCoy.GetInstance().gameState.selectedPlayer];
      McCoySkillUnlockManager.PlayerSpawned(player1, rex.Skills);

      updateXP(true);

      if (!spawnerInitialized)
      {
        spawnerInitialized = true;
        initSpawner();
      }
      worldUI.StageBegan();
    }

    protected override void OnGameEnd(ControlsScript winner, ControlsScript loser)
    {
      //worldUI.StageEnded();
    }

    private void Awake()
    {
      Camera.main.orthographic = true;
      toggleDebug();

      if(spawnButton != null)
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
          UFE.CreateRandomMonster(m, x, z);
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

      worldUI = Instantiate(worldSpacePrefab, uiRoot.transform);
      // bonusUI.transform.position = new Vector3(0, 0, 2) + uiRoot.gameObject.transform.position;

      currentStage = McCoy.GetInstance().currentStage;
    }

    private void toggleDebug()
    {
      winButton.gameObject.SetActive(debug);
      spawnButton.gameObject.SetActive(debug);
      xInput.gameObject.SetActive(debug);
      yInput.gameObject.SetActive(debug);
      nameInput.gameObject.SetActive(debug);
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
      if(msg == "stinger_stageover_escaped")
      {
        worldUI.StageEnded(McCoyStinger.StingerTypes.Escaped);
      }
      else if(msg == "stinger_stageover_bossdefeated")
      {
        worldUI.StageEnded(McCoyStinger.StingerTypes.BossDefeated);
      }
      else if(msg == "stinger_stageover_stagecleared")
      {
        worldUI.StageEnded(McCoyStinger.StingerTypes.StageCleared);
      }
      return;
      //alertText.text = msg;
      //UFE.DelaySynchronizedAction(hideAlertText, 2.0f);
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
        var selectedPlayer = gameState.playerCharacters[gameState.selectedPlayer];
        int skillPointsBefore = selectedPlayer.AvailableSkillPoints;
        spawner.ActorKilled(player);
        // if #skillpoints changed, we leveled up. if we leveled up, #skillpoints changed
        updateXP(skillPointsBefore != selectedPlayer.AvailableSkillPoints);
      }
    }

    private void updateXP(bool initialize)
    {
      var gameState = McCoy.GetInstance().gameState;
      int XP = McCoy.GetInstance().gameState.playerCharacters[gameState.selectedPlayer].XP;
      var selectedPlayer = gameState.playerCharacters[gameState.selectedPlayer];
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
      worldUI.UpdatePlayerXP(xpCache, initialize, diff);
    }

    protected override void UpdatePlayerHealthBar(float percent)
    {
      worldUI.UpdatePlayerHealth(percent);
      worldUI.UpdatePlayerXP(xpCache, false);
    }

    protected override void OnLifePointsChange(float newFloat, ControlsScript player)
    {
      if(UFE.GetController(player.playerNum).isCPU)
      {
        enemyName.text = player.myInfo.characterName;
        enemyName.gameObject.SetActive(true);
        worldUI.updateEnemyHealth(player, enemyUIHidden);
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

    public void BossSpawned(ControlsScript boss)
    {
      bossName.gameObject.SetActive(true);
      bossName.text = boss.myInfo.characterName;
      worldUI.BossSpawned(boss);
    }

    public void BossDied(ControlsScript boss)
    {
      bossName.gameObject.SetActive(false);
      worldUI.BossDied(boss);
    }

    public void CheatWin()
    {
      McCoy.GetInstance().debugCheatWin = true;
    }
  }
}