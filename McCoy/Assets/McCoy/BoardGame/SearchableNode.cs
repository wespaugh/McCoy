using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.McCoy.BoardGame
{
  [Serializable]
  public class SearchableNode
  {
    [SerializeField]
    public string NodeID;

    [NonSerialized]
    public List<SearchableNode> connectedNodes = new List<SearchableNode>();
  }
}