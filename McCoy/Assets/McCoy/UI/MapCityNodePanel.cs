using Assets.McCoy.BoardGame;
using Assets.McCoy.Brawler;
using System.Collections.Generic;
using TMPro;
using UFE3D.Brawler;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Assets.McCoy.ProjectConstants;

namespace Assets.McCoy.UI
{
  public class MapCityNodePanel : MonoBehaviour, ISelectHandler, IDeselectHandler
  {
    [SerializeField]
    TMP_Text NodeName = null;

    [SerializeField]
    TMP_Text SearchStatus = null;

    [SerializeField]
    GameObject MobNodeContainer = null;

    [SerializeField]
    GameObject MobPanelPrefab = null;

    [SerializeField]
    Image selectionIcon = null;

    protected ScrollRect scrollRect;
    protected RectTransform contentPanel;

    MapNode node = null;

    List<McCoyMobData> mobs = new List<McCoyMobData>();
    List<GameObject> mobObjects = new List<GameObject>();

    McCoyCityScreen uiRoot = null;

    bool canConnect = false;
    bool isSelected = false;

    bool bound = false;

    private void Awake()
    {
      scrollRect = GetComponentInParent<ScrollRect>();
      contentPanel = transform.parent as RectTransform;
      selectionIcon.gameObject.SetActive(false);
      //bind();
    }

    private void OnDestroy()
    {
      //unbind();
    }

    /*
    private void bind()
    {
      if(bound)
      {
        return;
      }
      bound = true;
    }

    private void unbind()
    {
      if(! bound)
      {
        return;
      }
      Selection.selectionChanged -= selectionChanged;
    }
    */
    public void OnSelect(BaseEventData eventData)
    {
      Canvas.ForceUpdateCanvases();

      isSelected = true;
      var anch = contentPanel.anchoredPosition;
      var newPos = (Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position)
              - (Vector2)scrollRect.transform.InverseTransformPoint(transform.position);
      anch.y = newPos.y;
      contentPanel.anchoredPosition = anch;
      updateIcon();
      ZoneHighlighted();
    }

    private void updateIcon()
    {
      selectionIcon.gameObject.SetActive(isSelected);
      selectionIcon.color = canConnect ? ProjectConstants.GREEN : ProjectConstants.PINK;
    }

    public void OnDeselect(BaseEventData eventData)
    {
      selectionIcon.gameObject.SetActive(false);
      isSelected = false;
    }

    public void Initialize(MapNode node, McCoyCityScreen screen, MapNode mechanismLoc)
    {
      this.node = node;
      NodeName.text = node.ZoneName;
      SearchStatus.text = $"{SearchStateDisplay(node.SearchStatus())}";
      if (screen != null && mechanismLoc != null && mechanismLoc.SearchStatus() == SearchState.CompletelySearched)
      {
        SearchStatus.text = $"Distance to goal: {node.DistanceToMechanism}";
      }
      uiRoot = screen;

      canConnect = uiRoot != null;

      selectionIcon.gameObject.SetActive(canConnect);

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
      if(uiRoot == null || ! canConnect)
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
      uiRoot.Board.SelectMapNode(node, null);
      uiRoot.Board.ToggleZoom(true);
      uiRoot.Board.SetHoverNode(node);
    }

    public void SetInteractable(bool isConnected)
    {
      canConnect = isConnected;
      selectionIcon.gameObject.SetActive(isConnected && isSelected);
    }
  }
}