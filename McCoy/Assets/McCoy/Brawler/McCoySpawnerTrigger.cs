using System;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.Brawler
{
  [Serializable]
  public class McCoySpawnData
  {
    public string EnemyName;
    public bool IsBoss;
    public float xPosition { get; private set; }

    public void Initialize(float xPosition)
    {
      this.xPosition = xPosition;
    }
  }
  public class McCoySpawnerTrigger : MonoBehaviour
  {
    [SerializeField]
    public McCoySpawnData spawnData;

    public void OnDestroy()
    {
      // Debug.Log("GETTING DESTROYED HERE SOMEHOW");
    }
  }
}