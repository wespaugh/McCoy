using System;
using UnityEditor;

namespace Assets.McCoy.BoardGame
{
  [Serializable]
  public class MapNodeLinkData
  {
    public string readableID;
    public string BaseNodeGuid;
    public string PortName;
    public string TargetNodeGuid;
  }
}