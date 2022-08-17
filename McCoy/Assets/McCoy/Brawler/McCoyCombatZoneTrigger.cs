using System;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.Brawler
{
  [Serializable]
  public class McCoyCombatZoneData
  {
    // percentage of total level population to spawn in this combat zone
    [SerializeField]
    float enemyPercentage;

    public float EnemyPercentage
    {
      get => enemyPercentage;
    }

    public float XPosition { get; private set; }
    public void Initialize(float xPos)
    {
      XPosition = xPos;
    }
  }

  public class McCoyCombatZoneTrigger : MonoBehaviour
  {
    [SerializeField]
    McCoyCombatZoneData zoneData;
    public McCoyCombatZoneData ZoneData
    {
      get => zoneData;
    }
  }
}