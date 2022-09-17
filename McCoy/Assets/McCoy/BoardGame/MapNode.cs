using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.BoardGame
{
  [Serializable]
  public class MapNode : SearchableNode
  {
    [NonSerialized]
    public List<McCoyMobData> Mobs = new List<McCoyMobData>();

    private float searchPercent;
    public int SearchPercent => (int)searchPercent;

    public void Search(float strongestMobStrength)
    {
      searchPercent += strongestMobStrength * .6f;
    }

    public string ZoneName;
    public Vector2 Position;

    public int DistanceToMechanism
    {
      get;
      private set;
    }

    public void SetMechanismLocation(MapNode loc)
    {
      DistanceToMechanism = DistanceTo(loc);
    }

  }
}