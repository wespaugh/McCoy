using Assets.McCoy.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.McCoy.BoardGame
{
  class McCoyMobRoutingUI : MonoBehaviour
  {
    [SerializeField]
    TMP_Text alert = null;

    [SerializeField]
    TMP_Text instructions = null;

    [SerializeField]
    McCoyMobRoutingDestination zoneSelectPrefab = null;

    [SerializeField]
    Button leftButton = null;

    [SerializeField]
    Button rightButton = null;

    [SerializeField]
    Transform zoneSelectRoot = null;

    List<Tuple<MapNode, McCoyMobData>> zoneMobs = new List<Tuple<MapNode, McCoyMobData>>();
    int zoneIndex = 0;

    List<GameObject> zoneSelectObjects = new List<GameObject>();
    public void Initialize(Dictionary<MapNode, List<McCoyMobData>> routedMobsInMapNodes)
    {
      int totalCount = 0;
      foreach (var zone in routedMobsInMapNodes)
      {
        totalCount += zone.Value.Count;
        foreach (McCoyMobData mob in zone.Value)
        {
          zoneMobs.Add(new Tuple<MapNode, McCoyMobData>(zone.Key, mob));
        }
      }

      if (totalCount == 1)
      {
        leftButton.gameObject.SetActive(false);
        rightButton.gameObject.SetActive(false);
      }

      if (totalCount == 0)
      {
        Debug.LogError("why are we here?");
        return;
      }

      zoneIndex = 0;
      updateZoneIndex();
    }

    private void updateZoneIndex()
    {
      while(zoneSelectObjects.Count > 0)
      {
        var obj = zoneSelectObjects[0];
        zoneSelectObjects.RemoveAt(0);
        Destroy(obj);
      }

      alert.text = $"Mobs Routed out of {zoneMobs[zoneIndex].Item1.ZoneName}!";
      instructions.text = $"{zoneMobs[zoneIndex].Item2.Faction} Routed! Pick a zone and send them packing!";

      foreach(var mapNode in zoneMobs[zoneIndex].Item1.connectedNodes)
      {
        Debug.Log($"{zoneMobs[zoneIndex].Item1.ZoneName} had a connected node {(mapNode as MapNode).ZoneName}");
        var zoneSelect = Instantiate(zoneSelectPrefab, zoneSelectRoot);
        zoneSelect.Initialize(zoneMobs[zoneIndex].Item1, mapNode as MapNode, zoneMobs[zoneIndex].Item2, closeMenu); // todo, this
        zoneSelectObjects.Add(zoneSelect.gameObject);
      }
    }

    private void closeMenu()
    {
      zoneMobs.RemoveAt(zoneIndex);
      if (zoneMobs.Count == 0)
      {
        Destroy(gameObject);
      }
      else if(zoneIndex < 0)
      {
        zoneIndex = 0;
      }
      updateZoneIndex();
    }
  }
}
