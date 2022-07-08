using Assets.McCoy.Brawler;
using System;
using UFE3D;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.UI
{
  public class McCoyBattleGui : DefaultBattleGUI
  {
    [SerializeField]
    public GameObject worldSpacePrefab = null;

    GameObject uiRoot = null;

    McCoyProgressBar healthBar = null;

    [SerializeField] float overrideHealth = -1.0f;

    McCoyStageData currentStage;

    private void Awake()
    {
      var camera2 = GameObject.Find("UI Camera");
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

      var bonusUI = Instantiate(worldSpacePrefab, uiRoot.transform);
      // bonusUI.transform.position = new Vector3(0, 0, 2) + uiRoot.gameObject.transform.position;
      healthBar = bonusUI.GetComponentInChildren<McCoyProgressBar>();
      healthBar.transform.localPosition = new Vector3(-4.40f, 4.4f, 0.0f);

      currentStage = McCoy.GetInstance().currentStage;

      initSpawner();
    }

    private void initSpawner()
    {
      gameObject.AddComponent<McCoyBrawlerSpawnManager>().Initialize(currentStage.GetSpawnData());
      info.text = currentStage.Name;
    }

    private void OnDestroy()
    {
      Destroy(uiRoot);
    }

    protected override void UpdatePlayerHealthBar(float percent)
    {
      healthBar.SetFill(overrideHealth >= 0.0f ? overrideHealth : percent);
    }

    protected override void UpdateWonRounds()
    {
    }
  }
}