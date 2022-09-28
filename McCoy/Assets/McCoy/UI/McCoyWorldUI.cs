using Assets.McCoy.Brawler;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Assets.McCoy.UI
{
  public class McCoyWorldUI : MonoBehaviour, IBossSpawnListener
  {
    [SerializeField]
    McCoyProgressBar healthBar;

    [SerializeField]
    McCoyProgressBar bossHud;

    [SerializeField]
    McCoyProgressBar enemyHud;

    [SerializeField]
    GameObject stingerPrefab = null;

    [SerializeField]
    Transform stingerTransformRoot = null;

    float enemyHPExpiryTime;
    private Action uiDisappearedCallback;
    const float enemyHealthDuration = 3.0f;

    bool playerHealthInitialized = false;

    ControlsScript boss = null;
    float bossStartingLife = -1;
    private void initializePlayerHealth()
    {
      playerHealthInitialized = true;
      healthBar.transform.localPosition = new Vector3(-4.40f, 4.4f, 0.0f);
      healthBar.Initialize((int)UFE.GetPlayer1ControlsScript().currentLifePoints, 1000);
    }

    public void UpdatePlayerHealth(float percent)
    {
      if(!playerHealthInitialized)
      {
        initializePlayerHealth();
      }

      healthBar.SetFill(percent * 1000);

      if(boss != null)
      {
        bossHud.SetFill((float)boss.currentLifePoints);
      }
    }

    public void updateEnemyHealth(ControlsScript enemy, Action uiDisappearedCallback)
    {
      enemyHud.Initialize(enemy.myInfo.lifePoints, 100);
      enemyHud.SetFill((float)enemy.currentLifePoints);
      enemyHPExpiryTime = Time.time + enemyHealthDuration;
      this.uiDisappearedCallback = uiDisappearedCallback;

      if (! enemyHud.gameObject.activeInHierarchy)
      {
        enemyHud.gameObject.SetActive(true);
        StartCoroutine(runEnemyHPBar());
      }
    }

    private IEnumerator runEnemyHPBar()
    {
      while (Time.time < enemyHPExpiryTime)
      {
        yield return null;
      }
      enemyHud.gameObject.SetActive(false);
      if(uiDisappearedCallback != null)
      {
        uiDisappearedCallback();
      }
    }

    public void BossSpawned(ControlsScript boss)
    {
      bossHud.Initialize((int)boss.currentLifePoints, 333);
      this.boss = boss;
      bossStartingLife = (float)boss.currentLifePoints;
      bossHud.gameObject.SetActive(true);
      bossHud.FadeIn();
    }

    public void BossDied(ControlsScript boss)
    {
      this.boss = null;
      bossHud.FadeOut();
    }

    public void StageBegan()
    {
      var stinger = Instantiate(stingerPrefab, stingerTransformRoot);
      stinger.GetComponent<McCoyStinger>().RunStinger(McCoyStinger.StingerTypes.RoundStart);
    }
    public void StageEnded(McCoyStinger.StingerTypes stingerType)
    {
      stinger.RunStinger(stingerType);
    }
  }
}