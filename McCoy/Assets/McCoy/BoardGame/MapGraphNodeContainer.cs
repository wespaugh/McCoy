using Assets.McCoy.BoardGame;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.BoardGame
{
  public class MapGraphNodeContainer : ScriptableObject
  {
    public List<MapNodeLinkData> NodeLinks = new List<MapNodeLinkData>();
    public List<MapNode> NodeData = new List<MapNode>();

  }
}