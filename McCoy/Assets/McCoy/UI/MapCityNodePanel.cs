using Assets.McCoy.BoardGame;
using Assets.McCoy.Brawler;
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

    List<McCoyMobData> mobs = new List<McCoyMobData>();

    McCoyCityBoardContents board = null;

    public void Initialize(MapNode node, McCoyCityBoardContents board)
    {
      this.node = node;
      NodeName.text = node.ZoneName;
      this.board = board;

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
        McCoyMobData mobData = new McCoyMobData(fs[i]);
        mob.Initialize(mobData);
        mobs.Add(mobData);
      }
    }

    public void ZoneClicked()
    {
      McCoyStageData stageData = new McCoyStageData();
      stageData.Initialize(node.ZoneName, mobs);

      board.LoadStage(stageData);
    }
  }
}