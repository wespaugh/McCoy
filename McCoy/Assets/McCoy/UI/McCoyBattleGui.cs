using Assets.McCoy.Brawler;
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

    [SerializeField] Button spawnButton = null;
    [SerializeField] TMP_InputField xInput = null;
    [SerializeField] TMP_InputField yInput = null;
    [SerializeField] TMP_InputField nameInput = null;

    [SerializeField] TMP_Text tmpNameText = null;

    [SerializeField] TMP_Text bossName = null;

    McCoyStageData currentStage;

    bool debug => McCoy.GetInstance().Debug;

    bool spawnerInitialized = false;


    protected override void OnGameBegin(ControlsScript player1, ControlsScript player2, StageOptions stage)
    {
      base.OnGameBegin(player1, player2, stage);

      tmpNameText.text = player1.myInfo.characterName;

      if (!spawnerInitialized)
      {
        spawnerInitialized = true;
        initSpawner();
      }
    }

    protected override void OnGameEnd(ControlsScript winner, ControlsScript loser)
    {
    }

    private void Awake()
    {
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
      spawnButton.gameObject.SetActive(debug);
      xInput.gameObject.SetActive(debug);
      yInput.gameObject.SetActive(debug);
      nameInput.gameObject.SetActive(debug);
    }

    private void initSpawner()
    {
      gameObject.AddComponent<McCoyBrawlerSpawnManager>().Initialize(currentStage.GetSpawnData(), this);
      info.text = currentStage.Name;
    }

    private void OnDestroy()
    {
      Destroy(uiRoot);
    }

    protected override void SetAlertMessage(string msg)
    {
      alertText.text = msg;

      UFE.DelaySynchronizedAction(hideAlertText, 2.0f);
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
    }
    protected override void UpdatePlayerHealthBar(float percent)
    {
      worldUI.UpdatePlayerHealth(percent);
    }

    protected override void UpdateWonRounds()
    {
    }

    private void FixedUpdate()
    {
      if(UFE.config.debugOptions.debugMode)
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
  }
}