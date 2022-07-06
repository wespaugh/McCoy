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
    public List<object> Mobs = new List<object>();

    public string ZoneName;
    public Vector2 Position;
  }
}