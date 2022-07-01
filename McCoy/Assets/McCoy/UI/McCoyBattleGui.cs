using UFE3D;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.UI
{
  public class McCoyBattleGui : DefaultBattleGUI
  {
    [SerializeField]
    public GameObject healthBarPrefab = null;
    [SerializeField]
    public GameObject healthBarAnchor = null;

    McCoyProgressBar healthBar = null;

    [SerializeField] float overrideHealth = -1.0f;

    private void Awake()
    {
      if(healthBarPrefab != null && healthBar == null)
      {
        healthBar = Instantiate(healthBarPrefab, healthBarAnchor.transform).GetComponent<McCoyProgressBar>();
      }
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