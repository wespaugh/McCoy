using Assets.McCoy.Localization;
using Assets.McCoy.UI;
using System;
using TMPro;
using UnityEngine;

namespace Assets.McCoy.BoardGame
{
  public class McCoyMobRoutingDestination : MonoBehaviour
  {
    [SerializeField]
    McCoyLocalizedText zoneLabel = null;

    [SerializeField]
    GameObject highlight = null;

    McCoyMobData mob;
    MapNode originalNode;
    MapNode destinationNode;

    Action<MapNode, MapNode, McCoyMobData> callback;

    public void Initialize(MapNode originalNode, MapNode destinationNode, McCoyMobData mob, Action<MapNode, MapNode, McCoyMobData> selectCallback)
    {
      zoneLabel.SetText( destinationNode.ZoneName );
      this.destinationNode = destinationNode;
      this.mob = mob;
      this.originalNode = originalNode;
      this.callback = selectCallback;
    }

    public void ToggleHighlight(bool on)
    {
      highlight.SetActive(on);
    }

    public void ZoneSelected()
    {
      callback.Invoke(originalNode, destinationNode, mob);
    }
  }
}
