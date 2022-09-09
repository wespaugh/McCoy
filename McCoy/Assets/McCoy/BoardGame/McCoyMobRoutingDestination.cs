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

    Action callback;

    public void Initialize(MapNode originalNode, MapNode destinationNode, McCoyMobData mob, Action selectCallback)
    {
      zoneLabel.text = destinationNode.ZoneName;
      this.destinationNode = destinationNode;
      this.mob = mob;
      this.originalNode = originalNode;
      this.callback = selectCallback;
    }

    public void ZoneSelected()
    {
      originalNode.Mobs.Remove(mob);
      destinationNode.Mobs.Add(mob);
      callback.Invoke();
    }
  }
}
