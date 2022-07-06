using Assets.McCoy.BoardGame;
using System.Collections.Generic;
using TMPro;
using UFE3D.Brawler;
using UnityEditor;
using UnityEngine;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.UI
{
  public class MapCityNodePanel : MonoBehaviour
  {
    [SerializeField]
    TMP_Text NodeName = null;

    [SerializeField]
    GameObject MobNodeContainer = null;

    [SerializeField]
    GameObject MobPanelPrefab = null;

    MapNode node = null;

    public void Initialize(MapNode node)
    {
      this.node = node;
      NodeName.text = node.ZoneName;

      List<Factions> fs = new List<Factions>();
      do
      {
        if (Random.Range(0, 6) <= 1) fs.Add(Factions.AngelMilitia);
        if (Random.Range(0, 6) <= 1) fs.Add(Factions.CyberMinotaurs);
        if (Random.Range(0, 6) <= 1) fs.Add(Factions.Mages);
      } while (fs.Count == 0);

      for (int i = 0; i < fs.Count; ++i)
      {
        MapCityNodePanelMob mob = Instantiate(MobPanelPrefab, MobNodeContainer.transform).GetComponent<MapCityNodePanelMob>();
        mob.Initialize(fs[i], Random.Range(1,6), Random.Range(1,6));
      }
    }

    public void ZoneClicked()
    {
      Debug.Log($"Clicked node with ID {node.NodeID}");
      UFE.StartBrawlerMode();
    }
  }
}