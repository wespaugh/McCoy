using Assets.McCoy.BoardGame;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.Brawler
{
  using SpawnData = Dictionary<Factions, McCoyMobData>;
  public class McCoyStageData
  {
    public string Name { get; private set; }
    // mobs indexed by faction, paired by the number of enemies they can spawn
    SpawnData spawnLimits = new Dictionary<Factions, McCoyMobData>();

    public SpawnData GetSpawnData()
    {
      return new SpawnData(spawnLimits);
    }

    public void Initialize(string name, List<McCoyMobData> mobs)
    {
      Name = name;
      foreach(var m in mobs)
      {
        spawnLimits[m.Faction] = m;
      }
    }
  }
} 