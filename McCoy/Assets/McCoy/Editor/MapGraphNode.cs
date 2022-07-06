using Assets.McCoy.BoardGame;
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Assets.McCoy.Editor
{
  [Serializable]
  public class MapGraphNode : Node
  {
    public GUID guid;
    public bool entryPoint = false;
    public string zoneName;
    public Vector2 position;
  }
}