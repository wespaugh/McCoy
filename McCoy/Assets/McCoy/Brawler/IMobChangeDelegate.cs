using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.Brawler
{
  public interface IMobChangeDelegate
  {
    void MobsChanged(Dictionary<Factions, float> currentMobPopulations);
  }
}