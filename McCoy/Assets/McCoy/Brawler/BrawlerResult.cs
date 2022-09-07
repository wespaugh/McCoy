using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.Brawler
{
  public class BrawlerResult : ScriptableObject
  {
    List<Factions> factionsWeakened = new List<Factions>();
    List<Factions> factionsRouted = new List<Factions>();
  }
}