using System;
using UnityEditor;

namespace Assets.McCoy.BoardGame
{
  [Serializable]
  public class MapNodeLinkData
  {
    public string BaseNodeGuid;
    public string PortName;
    public string TargetNodeGuid;
  }
}