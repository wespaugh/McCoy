using Assets.McCoy.BoardGame;
using Assets.McCoy.Brawler;
using System.Collections.Generic;
using TMPro;
using UFE3D.Brawler;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
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

    [SerializeField]
    Button enterZoneButton;

    MapNode node = null;

    List<McCoyMobData> mobs = new List<McCoyMobData>();
    List<GameObject> mobObjects = new List<GameObject>();

    McCoyCityScreen uiRoot = null;

    public void Initialize(MapNode node, McCoyCityScreen screen)
    {
      this.node = node;
      NodeName.text = node.ZoneName;
      uiRoot = screen;

      enterZoneButton.gameObject.SetActive(screen != null);

      while(this.mobObjects.Count > 0)
      {
        var next = this.mobObjects[0];
        mobObjects.RemoveAt(0);
        Destroy(next);
      }
      mobObjects.Clear();
      this.mobs.Clear();

      foreach(var mobData in node.Mobs)
      {
        MapCityNodePanelMob mob = Instantiate(MobPanelPrefab, MobNodeContainer.transform).GetComponent<MapCityNodePanelMob>();
        mob.Initialize(mobData);
        this.mobs.Add(mobData);
        mobObjects.Add(mob.gameObject);
      }
    }

    public void ZoneClicked()
    {
      if(uiRoot == null)
      {
        return;
      }
      McCoyStageData stageData = new McCoyStageData();
      if(mobs.Count == 0)
      {
        Factions f;
        switch(Random.Range(1,4))
        {
          case 1:
            f = Factions.Mages;
            break;
          case 2:
            f = Factions.AngelMilitia;
            break;
          case 3:
            f = Factions.CyberMinotaurs;
            break;
          default:
            f = Factions.CyberMinotaurs;
            break;
        }
        mobs.Add(new McCoyMobData(f, 1, 1));
      }
      stageData.Initialize(node.ZoneName, mobs);

      uiRoot.LoadStage(node, stageData);
    }

    public void ZoneHighlighted()
    {
      uiRoot.Board.SetHoverNode(node);
    }
  }
}