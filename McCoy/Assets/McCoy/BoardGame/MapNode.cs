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

    public string ZoneName;
    public Vector2 Position;
  }
}