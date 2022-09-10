using Assets.McCoy.UI;
using System;
using TMPro;
using UnityEngine;

namespace Assets.McCoy.BoardGame
{
  public class McCoyMobRoutingDestination : MonoBehaviour
  {
    [SerializeField]
    TMP_Text zoneLabel = null;

    McCoyMobData mob;
    MapNode originalNode;
    MapNode destinationNode;

    Action<MapNode, MapNode, McCoyMobData> callback;

    public void Initialize(MapNode originalNode, MapNode destinationNode, McCoyMobData mob, Action<MapNode, MapNode, McCoyMobData> selectCallback)
    {
      zoneLabel.text = destinationNode.ZoneName;
      this.destinationNode = destinationNode;
      this.mob = mob;
      this.originalNode = originalNode;
      this.callback = selectCallback;
    }

    public void ZoneSelected()
    {
      callback.Invoke(originalNode, destinationNode, mob);
    }
  }
}
