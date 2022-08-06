using Assets.McCoy.Brawler;
using UnityEngine;

namespace Assets.McCoy.UI
{
  public class McCoyWorldUI : MonoBehaviour, IBossSpawnListener
  {
    [SerializeField]
    McCoyProgressBar healthBar;

    [SerializeField]
    McCoyProgressBar bossHud;

    bool playerHealthInitialized = false;

    ControlsScript boss = null;
    float bossStartingLife = -1;
    private void initializePlayerHealth()
    {
      playerHealthInitialized = true;
      healthBar.transform.localPosition = new Vector3(-4.40f, 4.4f, 0.0f);
      healthBar.Initialize(1000, 100);
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

    public void BossSpawned(ControlsScript boss)
    {
      bossHud.Initialize((int)boss.currentLifePoints, 210);
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
  }
}