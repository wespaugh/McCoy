using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.Brawler
{
  public interface IBossSpawnListener
  {
    void BossSpawned(ControlsScript boss);
    void BossDied(ControlsScript boss);
  }
}